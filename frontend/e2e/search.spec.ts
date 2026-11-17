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

test.afterEach(async ({ page }) => {
  await page.unrouteAll({ behavior: 'wait' });
});

test("Công thức dùng ảnh đúng nguồn và không hiện bảng dinh dưỡng khi chưa có số liệu", async ({ page }) => {
  await page.goto("/recipes/bo-sot-vang");
  await expect(page.getByRole("heading", { level: 1, name: "Bò sốt vang", exact: true })).toBeVisible();
  await expect(page.getByRole("heading", { name: "Giá trị dinh dưỡng (mỗi khẩu phần)", exact: true })).toHaveCount(0);
  await expect(page.getByRole("img", { name: "Bò sốt vang", exact: true }).first())
    .toHaveAttribute("src", "https://www.savourydays.com/wp-content/uploads/2011/09/bo-sot-vang1.jpg");

  const backend = process.env.PLAYWRIGHT_API_URL || "http://localhost:5000";
  const response = await page.request.get(`${backend}/api/v1/recipes/bun-cha-ha-noi`);
  expect(response.status()).toBe(200);
  const { data } = await response.json();
  expect(data.ingredients).toEqual(expect.arrayContaining([
    expect.objectContaining({ name: "Đường cát (ngâm dưa góp)", quantity: 25, unit: "g" }),
    expect.objectContaining({ name: "Nước cốt chanh (ngâm dưa góp)", quantity: 15, unit: "ml" }),
    expect.objectContaining({ name: "Muối tinh (ngâm dưa góp)", quantity: 0.75, unit: "tsp" }),
  ]));
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
    await page.goto("/search?q=" + encodeURIComponent("Phở"));
    await page.getByRole("button", { name: "Bộ lọc", exact: true }).click();
    const [filtered] = await Promise.all([
      page.waitForResponse(response => response.url().includes('/recipes/search') &&
        new URL(response.url()).searchParams.get('difficulty') === '1'),
      page.locator("select").filter({ has: page.locator('option[value="1"]') }).selectOption("1"),
    ]);
    await expect.poll(() => new URL(page.url()).searchParams.get("difficulty")).toBe("1");
    expect(filtered.status()).toBe(200);
  });

  test("Giữ focus khi điều hướng bằng bàn phím", async ({ page }) => {
    await page.goto("/search");
    await page.keyboard.press("Tab");
    // Kiểm tra focus element không bị mất
    const focused = await page.evaluate(() => document.activeElement?.tagName);
    expect(focused).not.toBe("BODY");
  });
});

test("Đăng ký tài khoản qua giao diện và nhận thông báo xác nhận email", async ({ page }) => {
  await page.goto("/auth/register");
  await page.getByLabel("Tên hiển thị", { exact: true }).fill("Tác giả kiểm thử");
  await page.getByLabel("Email", { exact: true }).fill(`e2e-${Date.now()}@example.test`);
  await page.getByLabel("Mật khẩu", { exact: true }).fill("E2ePass!2026_Only");
  await page.getByLabel("Nhập lại mật khẩu", { exact: true }).fill("E2ePass!2026_Only");
  const registered = page.waitForResponse(response => response.url().endsWith("/auth/register") && response.request().method() === "POST");
  await page.getByRole("button", { name: "Đăng ký", exact: true }).click();
  expect((await registered).status()).toBe(201);
  await expect(page.getByText(/Kiểm tra.*email|kiểm tra.*hộp thư|đăng ký thành công/i).first()).toBeVisible();
});

test.describe("Vòng đời công thức TV3 — API và PostgreSQL thật", () => {
  test.skip(!process.env.E2E_AUTHOR_EMAIL || !process.env.E2E_AUTHOR_PASSWORD,
    "Cần tài khoản Author đã xác nhận email trên môi trường kiểm thử riêng.");

  test.beforeEach(async ({ page }) => {
    await page.goto("/auth/login?callbackUrl=/dashboard/recipes/new");
    await page.getByLabel("Email", { exact: true }).fill(process.env.E2E_AUTHOR_EMAIL!);
    await page.getByLabel("Mật khẩu", { exact: true }).fill(process.env.E2E_AUTHOR_PASSWORD!);
    const signedIn = page.waitForResponse(response => response.url().endsWith("/auth/login")
      && response.request().method() === "POST");
    await page.getByRole("button", { name: "Đăng nhập", exact: true }).click();
    expect((await signedIn).status()).toBe(200);
    await page.waitForURL("**/dashboard/recipes/new");
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
    expect(data.status).toBe(0);
    expect(data.concurrencyToken).toBeGreaterThan(0);
    return { title, id: data.id as string };
  }

  test("Tạo bản nháp, mở lại và lưu đúng danh mục cùng If-Match", async ({ page }) => {
    const { id, title } = await createDraft(page);
    const loaded = page.waitForResponse(response => response.url().endsWith(`/recipes/${id}`)
      && response.request().method() === "GET");
    await page.goto(`/dashboard/recipes/${id}/edit`);
    expect((await loaded).status()).toBe(200);
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
    let releaseInitial!: () => void;
    let initialCompleted!: () => void;
    const initialHeld = new Promise<void>(resolve => { releaseInitial = resolve; });
    const initialDone = new Promise<void>(resolve => { initialCompleted = resolve; });
    const initialRequest = page.waitForRequest(request => request.url().includes('/api/v1/recipes?') &&
      !new URL(request.url()).searchParams.has('q'));
    await page.route("**/api/v1/recipes?**", async route => {
      if (new URL(route.request().url()).searchParams.has('q')) return route.fallback();
      const response = await route.fetch();
      await initialHeld;
      await route.fulfill({ response });
      initialCompleted();
    });
    try {
      await page.goto("/dashboard/recipes");
      await initialRequest;
      await page.getByPlaceholder("Tìm theo tên công thức...").fill(`khong-co-mon-${Date.now()}`);
      await expect(page.getByRole("heading", { name: "Không tìm thấy công thức nào", exact: true })).toBeVisible();
    } finally { releaseInitial(); }
    await initialDone;
    await page.waitForLoadState('networkidle');
    await expect(page.getByRole("heading", { name: "Không tìm thấy công thức nào", exact: true })).toBeVisible();
    await page.route("**/api/v1/recipes?**", route => route.abort("failed"));
    await page.getByPlaceholder("Tìm theo tên công thức...").fill("kiem-tra-mat-ket-noi");
    await expect(page.getByRole("heading", { name: "Không thể tải danh sách công thức", exact: true })).toBeVisible();
    await expect(page.getByRole("button", { name: "Thử nạp lại", exact: true })).toBeVisible();
  });

  test("Upload lỗi giữa chừng giữ ảnh đã lưu và retry không tạo ảnh trùng", async ({ page }) => {
    const { id } = await createDraft(page);
    await page.getByRole('button', { name: /Bước 4: Ảnh MinIO/ }).click();
    const png = await page.evaluate(() => {
      const canvas = document.createElement('canvas');
      canvas.width = canvas.height = 64;
      const context = canvas.getContext('2d')!;
      context.fillStyle = '#dc6414';
      context.fillRect(0, 0, 64, 64);
      return canvas.toDataURL('image/png').split(',')[1];
    });
    await page.locator('input[type=file]').setInputFiles(['first.png', 'second.png'].map(name => ({
      name, mimeType: 'image/png', buffer: Buffer.from(png, 'base64'),
    })));
    let uploads = 0;
    await page.route(`**/api/v1/recipes/${id}/images*`, route => {
      if (route.request().method() === 'POST' && ++uploads === 2) return route.abort('failed');
      return route.fallback();
    });
    await page.getByRole('button', { name: 'Tải 2 ảnh lên MinIO', exact: true }).click();
    await expect(page.getByText('Upload ảnh thất bại', { exact: true })).toBeVisible();
    const retry = page.getByRole('button', { name: 'Tải ảnh lên MinIO (POST)', exact: true });
    await expect(retry).toBeEnabled();
    const uploaded = page.waitForResponse(response => response.url().includes(`/recipes/${id}/images`) && response.request().method() === 'POST');
    await retry.click();
    expect((await uploaded).status()).toBe(201);
    const backend = process.env.PLAYWRIGHT_API_URL || 'http://localhost:5000';
    const login = await page.request.post(`${backend}/api/v1/auth/login`, {
      data: { email: process.env.E2E_AUTHOR_EMAIL, password: process.env.E2E_AUTHOR_PASSWORD },
    });
    expect(login.status()).toBe(200);
    const token = (await login.json()).data.accessToken;
    const images = await page.request.get(`${backend}/api/v1/recipes/${id}/images`, { headers: { Authorization: `Bearer ${token}` } });
    expect(images.status()).toBe(200);
    expect((await images.json()).data).toHaveLength(2);
    expect(uploads).toBe(3);
  });

  test("Thêm nguyên liệu và bước nấu rồi xuất bản công thức", async ({ page }) => {
    const { id, title } = await createDraft(page);
    await page.getByPlaceholder("VD: Thịt thăn bò, Xương ống...").fill("Gạo");
    await page.getByPlaceholder("VD: 1.5 hoặc 500").fill("200");
    await page.getByPlaceholder("g, kg, ml, muỗng...").fill("g");
    const ingredient = page.waitForResponse(response => response.url().includes(`/recipes/${id}/ingredients`)
      && response.request().method() === "POST");
    await page.getByRole("button", { name: "Thêm vào danh sách (POST)", exact: true }).click();
    expect((await ingredient).status()).toBe(201);
    await expect(page.getByText("Gạo", { exact: true })).toBeVisible();
    await page.getByRole("button", { name: "Tiếp tục Bước 3: Các bước thực hiện", exact: true }).click();
    await page.getByPlaceholder("VD: Sơ chế & chần xương bò khử mùi...").fill("Nấu cơm");
    await page.getByPlaceholder("Mô tả chi tiết nhiệt độ lửa, thời gian canh chỉnh và mẹo nhận biết khi đạt yêu cầu...")
      .fill("Vo sạch gạo, thêm nước phù hợp và nấu đến khi hạt cơm chín mềm.");
    const step = page.waitForResponse(response => response.url().includes(`/recipes/${id}/steps`)
      && response.request().method() === "POST");
    await page.getByRole("button", { name: "Thêm Bước 1 (POST)", exact: true }).click();
    expect((await step).status()).toBe(201);
    // Mutation còn tải lại phiên bản Recipe sau 201; đợi bước đã hiện rồi mới xuất bản.
    await expect(page.locator("span").filter({ hasText: /^Bước 1: Nấu cơm$/ })).toBeVisible();
    const published = page.waitForResponse(response => response.url().endsWith(`/recipes/${id}/publish`)
      && response.request().method() === "PATCH");
    await page.getByRole("button", { name: "Hoàn tất & Xuất bản", exact: true }).click();
    const response = await published;
    expect(response.status()).toBe(200);
    expect((await response.json()).data.status).toBe(1);
    expect(response.request().headers()["if-match"]).toMatch(/^"[1-9]\d*"$/);
    await page.goto('/search?q=' + encodeURIComponent(title));
    await expect(page.getByRole('link', { name: new RegExp(title) }).first()).toBeVisible();
  });
});

