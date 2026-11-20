import type { Page } from "@playwright/test";
import { test, expect } from "./fixtures";
import { toStatusNumber, toStatusLabel } from "../src/lib/enums";

test("Trạng thái công thức thống nhất giữa enum số và chuỗi", () => {
  for (const [value, expected, label] of [
    [0, 0, 'Bản nháp'], ['0', 0, 'Bản nháp'], ['Draft', 0, 'Bản nháp'],
    [1, 1, 'Đã xuất bản'], ['1', 1, 'Đã xuất bản'], ['Published', 1, 'Đã xuất bản'],
    [2, 2, 'Đã lưu trữ'], ['2', 2, 'Đã lưu trữ'], ['Archived', 2, 'Đã lưu trữ'],
  ] as const) {
    expect(toStatusNumber(value)).toBe(expected);
    expect(toStatusLabel(value)).toBe(label);
  }
});

test("Khách xem công thức đã xuất bản không bị gắn nhãn lưu trữ", async ({ page }) => {
  const backend = process.env.PLAYWRIGHT_API_URL || 'http://localhost:5000';
  const response = await page.request.get(`${backend}/api/v1/recipes/thit-xien-nuong`);
  expect(response.status()).toBe(200);
  expect((await response.json()).data.status).toBe(1);
  await page.goto('/recipes');
  await expect(page.getByRole('article').first()).toBeVisible();
  await expect(page.getByText('Lưu trữ', { exact: true })).toHaveCount(0);
  await page.goto('/recipes/thit-xien-nuong');
  await expect(page.getByRole('heading', { level: 1, name: 'Thịt xiên nướng', exact: true })).toBeVisible();
  await expect(page.getByText(/Công thức đang ở trạng thái/)).toHaveCount(0);
  await expect(page.getByText('Lưu trữ', { exact: true })).toHaveCount(0);
});

test("Khách xem ảnh gà nướng sả và đủ trường dinh dưỡng đã nhập", async ({ page }) => {
  await page.goto('/recipes/ga-nuong-sa');
  await expect(page.getByRole('heading', { level: 1, name: 'Gà nướng sả', exact: true })).toBeVisible();
  await expect(page.getByText(/Công thức đang ở trạng thái/)).toHaveCount(0);
  const image = page.getByRole('img', { name: 'Gà nướng sả', exact: true }).first();
  await expect.poll(() => image.evaluate(element => (element as HTMLImageElement).naturalWidth), { timeout: 15000 }).toBeGreaterThan(0);
  await expect(page.getByRole('heading', { name: /Giá trị dinh dưỡng \(mỗi khẩu phần\)/ })).toBeVisible();
  await expect(page.getByText('Natri', { exact: true })).toBeVisible();
  await expect(page.getByText('339 mg', { exact: true })).toBeVisible();
});

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

test.describe("Hồi quy kho công thức trước demo", () => {
  test("giữ tổng kết quả API khi chuyển sang trang tiếp theo", async ({ page }) => {
    const backend = process.env.PLAYWRIGHT_API_URL || "http://localhost:5000";
    const response = await page.request.get(`${backend}/api/v1/recipes?pageSize=12`);
    expect(response.ok()).toBeTruthy();
    const { meta } = await response.json();
    expect(meta.total).toBeGreaterThan(12);

    await page.goto("/recipes?sortBy=title&sortOrder=asc");
    const total = page.getByText(`Tìm thấy ${meta.total} công thức`, { exact: true });
    await expect(total).toBeVisible();
    const pagination = page.getByRole("navigation", { name: "Điều hướng phân trang" });
    await pagination.getByRole("link", { name: "2", exact: true }).click();
    await expect(page).toHaveURL(/\/recipes\?.*\bpage=2\b/);
    const params = new URL(page.url()).searchParams;
    expect(params.get("sortBy")).toBe("title");
    expect(params.get("sortOrder")).toBe("asc");
    await expect(pagination.locator('[aria-current="page"]')).toHaveText("2");
    await expect(total).toBeVisible();
  });

  test("hiển thị tổng 0 và trạng thái rỗng khi API trả thành công", async ({ page }) => {
    await page.goto("/recipes?categoryId=00000000-0000-0000-0000-ffffffffffff");
    await expect(page.getByText("Tìm thấy 0 công thức", { exact: true })).toBeVisible();
    await expect(page.getByRole("heading", { name: "Không tìm thấy công thức phù hợp" })).toBeVisible();
    await expect(page.getByRole("navigation", { name: "Điều hướng phân trang" })).toHaveCount(0);
  });

  test("báo bộ lọc không hợp lệ và cho đặt lại", async ({ page }) => {
    await page.goto("/recipes?categoryId=khong-hop-le");
    await expect(page.getByRole("alert").getByRole("heading", { name: "Bộ lọc không hợp lệ" })).toBeVisible();
    await expect(page.getByRole("heading", { name: "Không tìm thấy công thức phù hợp" })).toHaveCount(0);
    await page.getByRole("link", { name: "Đặt lại bộ lọc", exact: true }).click();
    await expect(page).toHaveURL(/\/recipes$/);
    await expect(page.getByRole("article").first()).toBeVisible();
  });

  test("báo mất kết nối backend và giữ tham số khi thử lại", async ({ page }) => {
    const outageUrl = process.env.PLAYWRIGHT_OUTAGE_BASE_URL;
    test.skip(!outageUrl, "Cần bản frontend kiểm thử trỏ vào backend không khả dụng.");
    const url = new URL("/recipes?page=2&maxTotalTime=60", outageUrl).href;
    await page.goto(url);
    const loadError = page.getByRole("alert").filter({ has: page.getByRole("heading", { name: "Không thể tải danh sách công thức" }) });
    await expect(loadError).toBeVisible();
    await expect(page.getByRole("heading", { name: "Không tìm thấy công thức phù hợp" })).toHaveCount(0);
    const retry = page.getByRole("link", { name: "Thử lại", exact: true });
    const navigation = page.waitForRequest(request => request.isNavigationRequest() && request.url() === url);
    await retry.click();
    await navigation;
    await expect(page).toHaveURL(url);
    await expect(loadError).toBeVisible();
  });
});

test("Bò sốt vang hiển thị ảnh gốc, dinh dưỡng và xuất hiện trong danh mục mới", async ({ page }) => {
  await page.goto("/recipes/bo-sot-vang");
  await expect(page.getByRole("heading", { level: 1, name: "Bò sốt vang", exact: true })).toBeVisible();
  await expect(page.getByRole("heading", { name: /Giá trị dinh dưỡng \(mỗi khẩu phần\)/ })).toBeVisible();
  await expect(page.getByText("620 mg", { exact: true })).toBeVisible();
  const image = page.getByRole("img", { name: "Bò sốt vang", exact: true }).first();
  await expect(image)
    .toHaveAttribute("src", "https://www.savourydays.com/wp-content/uploads/2011/09/bo-sot-vang1.jpg");
  await expect(image).toHaveAttribute("referrerpolicy", "no-referrer");
  await expect.poll(() => image.evaluate(element => (element as HTMLImageElement).naturalWidth), { timeout: 15000 }).toBeGreaterThan(0);

  const backend = process.env.PLAYWRIGHT_API_URL || "http://localhost:5000";
  const response = await page.request.get(`${backend}/api/v1/recipes/bun-cha-ha-noi`);
  expect(response.status()).toBe(200);
  const { data } = await response.json();
  expect(data.ingredients).toEqual(expect.arrayContaining([
    expect.objectContaining({ name: "Đường cát (ngâm dưa góp)", quantity: 25, unit: "g" }),
    expect.objectContaining({ name: "Nước cốt chanh (ngâm dưa góp)", quantity: 15, unit: "ml" }),
    expect.objectContaining({ name: "Muối tinh (ngâm dưa góp)", quantity: 0.75, unit: "tsp" }),
  ]));

  await page.getByRole("contentinfo").getByRole("link", { name: "Món hầm & Cà ri", exact: true }).click();
  await expect(page).toHaveURL(/\/categories\/mon-ham-ca-ri$/);
  await expect(page.getByRole("heading", { level: 1, name: "Món hầm & Cà ri", exact: true })).toBeVisible();
  await expect(page.locator('a[href="/recipes/bo-sot-vang"]').first()).toBeVisible();
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

  test("Hồi quy dashboard: tên bài Published mở trang công khai, bài riêng mở editor", async ({ page }) => {
    // Cố định ba trạng thái trong phản hồi danh sách để kiểm tra liên kết mà không sửa dữ liệu demo.
    const recipes = [
      { id: "b0000000-0000-4000-8000-000000000001", title: "Món đã xuất bản", slug: "mon-da-xuat-ban", status: 1 },
      { id: "b0000000-0000-4000-8000-000000000002", title: "Món đang nháp", slug: "mon-dang-nhap", status: 0 },
      { id: "b0000000-0000-4000-8000-000000000003", title: "Món đã lưu trữ", slug: "mon-da-luu-tru", status: 2 },
    ];
    await page.route("**/api/v1/recipes?**", route => {
      if (new URL(route.request().url()).searchParams.get("mine") !== "true") return route.fallback();
      return route.fulfill({
        json: { data: recipes, meta: { page: 1, pageSize: 10, total: recipes.length, totalPages: 1 } },
      });
    });
    await page.getByRole("navigation", { name: "Điều hướng quản trị" })
      .getByRole("link", { name: "Công thức của tôi", exact: true }).click();
    await expect(page.getByRole("link", { name: recipes[0].title, exact: true })).toHaveAttribute("href", "/recipes/mon-da-xuat-ban");
    await expect(page.getByRole("link", { name: recipes[1].title, exact: true })).toHaveAttribute("href", `/dashboard/recipes/${recipes[1].id}/edit`);
    await expect(page.getByRole("link", { name: recipes[2].title, exact: true })).toHaveAttribute("href", `/dashboard/recipes/${recipes[2].id}/edit`);
  });

  test("Tạo bản nháp, mở lại và lưu đúng danh mục cùng If-Match", async ({ page }) => {
    const { id, title } = await createDraft(page);
    await page.getByRole("navigation", { name: "Điều hướng quản trị" })
      .getByRole("link", { name: "Công thức của tôi", exact: true }).click();
    const loaded = page.waitForResponse(response => response.url().endsWith(`/recipes/${id}`)
      && response.request().method() === "GET");
    await page.getByRole("link", { name: title, exact: true }).click();
    await expect(page).toHaveURL(new RegExp(`/dashboard/recipes/${id}/edit$`));
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
      await page.getByRole("navigation", { name: "Điều hướng quản trị" })
      .getByRole("link", { name: "Công thức của tôi", exact: true }).click();
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
    const completedUpload = await uploaded;
    expect(completedUpload.status()).toBe(201);
    const { authorization } = await completedUpload.request().allHeaders();
    expect(authorization).toBeDefined();
    const backend = process.env.PLAYWRIGHT_API_URL || 'http://localhost:5000';
    const images = await page.request.get(`${backend}/api/v1/recipes/${id}/images`, { headers: { authorization } });
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
    const search = page.getByRole("searchbox", { name: "Tìm món ăn, nguyên liệu...", exact: true });
    await search.fill(title);
    await search.press("Enter");
    await expect(page).toHaveURL(url => url.pathname === "/search" && url.searchParams.get("q") === title);
    const result = page.getByRole('link', { name: new RegExp(title) }).first();
    await expect(result).toBeVisible();
    await result.click();
    await expect(page.getByRole('heading', { level: 1, name: title, exact: true })).toBeVisible();
    await expect(page.getByText('Công thức này chưa được bổ sung số liệu dinh dưỡng.', { exact: true })).toBeVisible();
  });
});

