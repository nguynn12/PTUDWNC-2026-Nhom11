import { expect, test, type Page } from "@playwright/test";

const adminMenu = [
  ["Tổng quan", "/dashboard"],
  ["Công thức của tôi", "/dashboard/recipes"],
  ["Tạo công thức mới", "/dashboard/recipes/new"],
  ["Danh mục món ăn", "/dashboard/categories"],
  ["Thùng rác công thức", "/admin/recipes/trash"],
  ["Người dùng", "/admin/users"],
  ["Nhật ký hệ thống", "/admin/system-logs"],
  ["Về trang chủ", "/"],
];

const sidebar = (page: Page) => page.getByRole("navigation", { name: "Điều hướng quản trị" });

async function restoreTestSession(page: Page, baseURL: string, role: "Admin" | "Author") {
  // Exercise the real client role guards and session restoration without writing to a live database.
  await page.context().addCookies([{ name: "cb_logged_in", value: "1", url: baseURL }]);
  await page.addInitScript(() => localStorage.setItem("cb.refreshToken", "sidebar-test-refresh"));
  await page.route("**/api/v1/**", async route => {
    const request = route.request();
    const path = new URL(request.url()).pathname;

    if (request.method() === "POST" && path === "/api/v1/auth/refresh") {
      await route.fulfill({ json: { data: {
        accessToken: "sidebar-test-access",
        refreshToken: "sidebar-test-refresh",
        expiresIn: 900,
        user: { id: "sidebar-test", displayName: role, roles: [role] },
      } } });
    } else if (request.method() === "GET") {
      await route.fulfill({ json: { data: [], meta: { total: 0, totalPages: 0, page: 1, pageSize: 10 } } });
    } else {
      await route.abort();
    }
  });
}

async function expectMenu(page: Page, links: string[][], active?: string) {
  const nav = sidebar(page);
  await expect(nav).toHaveCount(1);
  await expect(nav.getByRole("link")).toHaveText(links.map(([label]) => label));
  for (const [label, href] of links) {
    await expect(nav.getByRole("link", { name: label, exact: true })).toHaveAttribute("href", href);
  }
  await expect(nav.locator("[aria-current]")).toHaveCount(active ? 1 : 0);
  if (active) await expect(nav.locator('[aria-current="page"]')).toHaveAttribute("href", active);
}

test("Admin giữ nguyên một sidebar khi chuyển giữa Dashboard và Admin", async ({ page, baseURL }) => {
  await page.setViewportSize({ width: 1366, height: 768 });
  await restoreTestSession(page, baseURL!, "Admin");
  await page.goto("/dashboard");
  await expectMenu(page, adminMenu, "/dashboard");
  await expect(page.getByRole("button", { name: /Ẩn menu|Mở menu quản trị|Đóng menu quản trị/ })).toHaveCount(0);
  await expect.poll(() => sidebar(page).evaluate(el => el.scrollHeight <= el.clientHeight)).toBe(true);
  const persistentSidebar = await sidebar(page).elementHandle();

  for (const [label, href] of [adminMenu[1], adminMenu[4], adminMenu[5], adminMenu[6], adminMenu[0]]) {
    await sidebar(page).getByRole("link", { name: label, exact: true }).click();
    await expect(page).toHaveURL(url => url.pathname === href);
    await expectMenu(page, adminMenu, href);
    expect(await persistentSidebar?.evaluate(el => el.isConnected)).toBe(true);
  }

  await sidebar(page).getByRole("link", { name: "Tạo công thức mới" }).click();
  await expectMenu(page, adminMenu, "/dashboard/recipes/new");

  await page.setViewportSize({ width: 1366, height: 360 });
  await expect.poll(() => sidebar(page).evaluate(el => el.scrollHeight > el.clientHeight)).toBe(true);
  const lastLink = sidebar(page).getByRole("link", { name: "Về trang chủ" });
  await lastLink.focus();
  await expect(lastLink).toBeInViewport();
});

test("Author chỉ thấy menu được phép và bị chặn khi mở trang Admin", async ({ page, baseURL }) => {
  await restoreTestSession(page, baseURL!, "Author");
  await page.goto("/dashboard");
  const authorMenu = [adminMenu[0], adminMenu[1], adminMenu[2], adminMenu[7]];
  await expectMenu(page, authorMenu, "/dashboard");

  for (const [, href] of adminMenu.slice(3, 7)) {
    await page.goto(href);
    await expect(page.getByRole("heading", { name: "Không có quyền truy cập" })).toBeVisible();
    await expectMenu(page, authorMenu);
  }
});

test("Menu quản trị mobile có overlay, Escape và tự đóng khi chuyển trang", async ({ page, baseURL }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await restoreTestSession(page, baseURL!, "Admin");
  await page.goto("/dashboard");
  const trigger = page.getByRole("button", { name: "Mở menu quản trị" });
  const drawer = page.getByRole("dialog", { name: "Culinary Blog" });

  await expect(trigger).toBeVisible();
  await expect(page.getByRole("button", { name: "Mở menu di động" })).toHaveCount(0);
  await expect(sidebar(page)).toHaveCount(0);
  await trigger.click();
  await expect(drawer).toBeVisible();
  await expect(page.locator('[data-slot="sheet-overlay"]')).toBeVisible();
  await expectMenu(page, adminMenu, "/dashboard");

  await page.keyboard.press("Escape");
  await expect(drawer).toBeHidden();
  await expect(trigger).toBeFocused();

  await trigger.click();
  await page.mouse.click(380, 40);
  await expect(drawer).toBeHidden();

  await trigger.click();
  await sidebar(page).getByRole("link", { name: "Người dùng", exact: true }).click();
  await expect(page).toHaveURL(url => url.pathname === "/admin/users");
  await expect(drawer).toBeHidden();
  await expect(page.getByRole("heading", { name: "Người dùng", level: 1 })).toBeVisible();
  await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth))
    .toBe(390);

  await trigger.click();
  await expectMenu(page, adminMenu, "/admin/users");
  await page.setViewportSize({ width: 1366, height: 768 });
  await expect(drawer).toBeHidden();
  await expect(page.locator('[data-slot="sheet-overlay"]')).toHaveCount(0);
  await expect(trigger).toHaveCount(0);
  await expectMenu(page, adminMenu, "/admin/users");

  await page.setViewportSize({ width: 390, height: 844 });
  await expect(trigger).toBeVisible();
  await expect(drawer).toBeHidden();
});

test("Thùng rác tách tải, lỗi, rỗng và dữ liệu; bảng cuộn trong khung", async ({ page, baseURL }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await restoreTestSession(page, baseURL!, "Admin");
  let mode: "error" | "empty" | "data" = "error";
  let completeFirstRequest!: () => void;
  const firstRequest = new Promise<void>(resolve => { completeFirstRequest = resolve; });
  const requests: string[] = [];

  await page.route("**/api/v1/admin/recipes/trash?*", async route => {
    const currentPage = new URL(route.request().url()).searchParams.get("page") ?? "";
    requests.push(currentPage);
    if (requests.length === 1) await firstRequest;
    if (mode === "error") {
      await route.fulfill({ status: 503, json: { title: "Tạm thời không thể tải dữ liệu" } });
      return;
    }
    const data = mode === "empty" ? [] : [{
      id: "deleted-recipe",
      title: `Công thức thử trang ${currentPage}`,
      slug: "cong-thuc-thu",
      categoryName: "Món thử",
      authorName: "Tác giả thử",
      deletedAt: "2026-11-20T00:00:00Z",
    }];
    await route.fulfill({
      json: { data, meta: { page: Number(currentPage), pageSize: 10, total: data.length ? 11 : 0, totalPages: data.length ? 2 : 0 } },
    });
  });

  await page.goto("/admin/recipes/trash", { waitUntil: "domcontentloaded" });
  await expect(page.getByRole("heading", { level: 1, name: "Thùng rác công thức" })).toBeVisible();
  await expect(page.getByText("Đang tải thùng rác...", { exact: true })).toBeVisible();
  await expect(page.getByRole("table")).toHaveCount(0);
  await expect(page.getByRole("heading", { name: "Thùng rác đang trống" })).toHaveCount(0);
  completeFirstRequest();

  await expect(page.getByRole("alert").getByRole("heading", { name: "Không thể tải thùng rác" })).toBeVisible();
  await expect(page.getByRole("table")).toHaveCount(0);
  await expect(page.getByRole("heading", { name: "Thùng rác đang trống" })).toHaveCount(0);
  await expect(page.getByText("Không thể tải danh sách thùng rác", { exact: true })).toHaveCount(0);
  await page.getByRole("button", { name: "Mở menu quản trị" }).click();
  await page.getByRole("button", { name: "Đóng menu quản trị" }).click();
  await expect(page.getByRole("dialog", { name: "Culinary Blog" })).toBeHidden();

  mode = "empty";
  await page.getByRole("button", { name: "Thử lại" }).click();
  await expect(page.getByRole("heading", { name: "Thùng rác đang trống" })).toBeVisible();
  await expect(page.getByRole("table")).toHaveCount(0);
  await expect(page.getByRole("link", { name: "Quản lý công thức" })).toHaveAttribute("href", "/dashboard/recipes");

  mode = "data";
  await page.getByRole("button", { name: "Làm mới" }).click();
  const table = page.getByRole("table", { name: "Công thức đã xóa" });
  await expect(table.getByRole("cell", { name: /Công thức thử trang 1/ })).toBeVisible();
  await expect(table.getByRole("button", { name: "Khôi phục" })).toBeVisible();
  await table.getByRole("button", { name: "Xóa vĩnh viễn" }).click();
  await expect(page.getByText("Xác nhận Xóa Vĩnh Viễn Công Thức?")).toBeVisible();
  await page.getByRole("button", { name: "Hủy bỏ" }).click();
  await page.getByRole("button", { name: "Trang sau" }).click();
  await expect(table.getByRole("cell", { name: /Công thức thử trang 2/ })).toBeVisible();
  expect(requests).toEqual(["1", "1", "1", "2"]);
  expect(await table.evaluate(el => el.parentElement!.scrollWidth > el.parentElement!.clientWidth)).toBe(true);
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBe(390);
});
