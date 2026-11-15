import { test, expect, type Page } from "@playwright/test";

// Chuyển tiếp tới API thật của môi trường kiểm thử riêng, không dựng response giả.
test.beforeEach(async ({ page }) => {
  const apiUrl = process.env.PLAYWRIGHT_API_URL;
  if (apiUrl) {
    await page.route("**/api/v1/**", async route => {
      const url = new URL(route.request().url());
      const response = await route.fetch({ url: `${apiUrl}${url.pathname}${url.search}` });
      await route.fulfill({ response });
    });
  }
});

test.describe("Luồng tìm kiếm món ăn E2E (FR-SRCH-001 -> FR-SRCH-004)", () => {
  test("Hiển thị giao diện tìm kiếm với ô nhập liệu và các bộ lọc", async ({ page }) => {
    await page.goto("/search");
    await expect(page).toHaveTitle(/Tìm.*kiếm/i);

    const searchInput = page.getByPlaceholder(/Tìm theo tên món/i);
    await expect(searchInput).toBeVisible();
  });

  test("Nhập từ khóa và kích hoạt tìm kiếm sau debounce 300ms", async ({ page }) => {
    await page.goto("/search");
    const searchInput = page.getByPlaceholder(/Tìm theo tên món/i);
    const result = page.waitForResponse(response => {
      const url = new URL(response.url());
      return url.pathname.endsWith("/recipes/search") && url.searchParams.get("q") === "Phở";
    });
    await searchInput.fill("Phở");

    await expect.poll(() => new URL(page.url()).searchParams.get("q")).toBe("Phở");
    expect((await result).status()).toBe(200);
  });

  test("Thao tác chọn bộ lọc độ khó và cập nhật URL query params", async ({ page }) => {
    await page.goto("/search");
    await page.getByRole("button", { name: "Bộ lọc", exact: true }).click();
    await page.locator("select").filter({ has: page.locator('option[value="Easy"]') }).selectOption("Easy");
    await expect.poll(() => new URL(page.url()).searchParams.get("difficulty")).toBe("Easy");
  });

  test("Giữ focus khi điều hướng bằng bàn phím", async ({ page }) => {
    await page.goto("/search");
    await page.keyboard.press("Tab");
    // Kiểm tra focus element không bị mất
    const focused = await page.evaluate(() => document.activeElement?.tagName);
    expect(focused).not.toBe("BODY");
  });
});

test.describe("Vòng đời công thức TV3 — API và PostgreSQL thật", () => {
  test.skip(!process.env.E2E_AUTHOR_EMAIL || !process.env.E2E_AUTHOR_PASSWORD,
    "Cần tài khoản Author đã xác nhận email trên môi trường kiểm thử riêng.");

  test.beforeEach(async ({ page }) => {
    await page.goto("/auth/login?callbackUrl=/dashboard/recipes/new");
    await page.getByLabel("Email", { exact: true }).fill(process.env.E2E_AUTHOR_EMAIL!);
    await page.getByLabel("Mật khẩu", { exact: true }).fill(process.env.E2E_AUTHOR_PASSWORD!);
    await page.getByRole("button", { name: "Đăng nhập", exact: true }).click();
    await expect.poll(() => new URL(page.url()).pathname).toBe("/dashboard/recipes/new");
  });

  async function createDraft(page: Page) {
    const title = `Cơm rau kiểm thử ${Date.now()}`;
    await page.getByPlaceholder("Ví dụ: Phở Bò Truyền Thống Hà Nội Nước Trong...").fill(title);
    await page.getByPlaceholder("Giới thiệu hương vị đặc trưng, nguồn gốc hoặc bí quyết giúp món ăn tròn vị...")
      .fill("Công thức kiểm thử trên database riêng, không sử dụng làm dữ liệu nộp bài.");
    await expect(page.locator("select").first().locator("option").first()).toHaveAttribute("value", /.+/);
    const saved = page.waitForResponse(response => /\/api\/v1\/recipes\/?$/.test(response.url())
      && response.request().method() === "POST");
    await page.getByRole("button", { name: "Khởi tạo Công thức & Sang Bước 2", exact: true }).click();
    const response = await saved;
    expect(response.status()).toBe(201);
    const { data } = await response.json();
    expect(data.title).toBe(title);
    expect(data.status).toBe("Draft");
    expect(data.concurrencyToken).toBeGreaterThan(0);
    return { title, id: data.id as string };
  }

  test("Tạo bản nháp, mở lại và lưu đúng danh mục cùng If-Match", async ({ page }) => {
    const { id, title } = await createDraft(page);
    await page.goto(`/dashboard/recipes/${id}/edit`);
    await expect(page.getByPlaceholder("Ví dụ: Phở Bò Truyền Thống Hà Nội Nước Trong...")).toHaveValue(title);
    await expect(page.locator("select").first()).toHaveValue(/[0-9a-f-]{36}/);
    const saved = page.waitForResponse(response => response.url().endsWith(`/recipes/${id}`)
      && response.request().method() === "PUT");
    await page.getByRole("button", { name: "Lưu thông tin", exact: true }).click();
    const response = await saved;
    expect(response.status()).toBe(200);
    expect(response.request().headers()["if-match"]).toMatch(/^"[1-9]\d*"$/);
  });

  test("Danh sách rỗng và mất kết nối hiển thị trạng thái phù hợp", async ({ page }) => {
    await page.goto("/dashboard/recipes");
    await page.getByPlaceholder("Tìm theo tên công thức...").fill(`khong-co-mon-${Date.now()}`);
    await expect(page.getByRole("heading", { name: "Không tìm thấy công thức nào", exact: true })).toBeVisible();
    await page.route("**/api/v1/recipes?**", route => route.abort("failed"));
    await page.getByPlaceholder("Tìm theo tên công thức...").fill("kiem-tra-mat-ket-noi");
    await expect(page.getByRole("heading", { name: "Không thể tải danh sách công thức", exact: true })).toBeVisible();
    await expect(page.getByRole("button", { name: "Thử nạp lại", exact: true })).toBeVisible();
  });

  test("Thêm nguyên liệu và bước nấu rồi xuất bản công thức", async ({ page }) => {
    const { id } = await createDraft(page);
    await page.getByPlaceholder("VD: Thịt thăn bò, Xương ống...").fill("Gạo");
    await page.getByPlaceholder("VD: 1.5 hoặc 500").fill("200");
    await page.getByPlaceholder("g, kg, ml, muỗng...").fill("g");
    const ingredient = page.waitForResponse(response => response.url().includes(`/recipes/${id}/ingredients`)
      && response.request().method() === "POST");
    await page.getByRole("button", { name: "Thêm vào danh sách (POST)", exact: true }).click();
    expect((await ingredient).status()).toBe(201);
    await page.getByRole("button", { name: "Tiếp tục Bước 3: Các bước thực hiện", exact: true }).click();
    await page.getByPlaceholder("VD: Sơ chế & chần xương bò khử mùi...").fill("Nấu cơm");
    await page.getByPlaceholder("Mô tả chi tiết nhiệt độ lửa, thời gian canh chỉnh và mẹo nhận biết khi đạt yêu cầu...")
      .fill("Vo sạch gạo, thêm nước phù hợp và nấu đến khi hạt cơm chín mềm.");
    const step = page.waitForResponse(response => response.url().includes(`/recipes/${id}/steps`)
      && response.request().method() === "POST");
    await page.getByRole("button", { name: "Thêm Bước 1 (POST)", exact: true }).click();
    expect((await step).status()).toBe(201);
    const published = page.waitForResponse(response => response.url().endsWith(`/recipes/${id}/publish`)
      && response.request().method() === "PATCH");
    await page.getByRole("button", { name: "Hoàn tất & Xuất bản", exact: true }).click();
    const response = await published;
    expect(response.status()).toBe(200);
    expect((await response.json()).data.status).toBe("Published");
    expect(response.request().headers()["if-match"]).toMatch(/^"[1-9]\d*"$/);
  });
});

