import { expect, test, type Page } from "@playwright/test";

async function stubSessionApi(page: Page) {
  const user = {
    id: "auth-navigation-author",
    displayName: "Tác giả kiểm thử",
    email: "author@example.test",
    avatarUrl: null,
    bio: null,
    roles: ["Author"],
    emailConfirmed: true,
    createdAt: "2026-11-20T00:00:00Z",
  };
  await page.route("**/api/v1/**", async route => {
    const request = route.request();
    const path = new URL(request.url()).pathname.replace(/\/$/, "");

    if (request.method() === "POST" && ["/api/v1/auth/login", "/api/v1/auth/refresh", "/api/v1/auth/register"].includes(path)) {
      await route.fulfill({
        status: path.endsWith("/register") ? 201 : 200,
        json: { data: { accessToken: "e2e-access", refreshToken: "e2e-refresh", expiresIn: 900, user } },
      });
    } else if (request.method() === "POST" && path === "/api/v1/auth/logout") {
      await route.fulfill({ status: 204 });
    } else if (request.method() === "GET" && path === "/api/v1/auth/me") {
      await route.fulfill({ json: { data: user } });
    } else if (request.method() === "GET") {
      await route.fulfill({ json: { data: [], meta: { total: 0, page: 1, pageSize: 10, totalPages: 0 } } });
    } else {
      await route.abort();
    }
  });
}

async function login(page: Page) {
  await page.getByLabel("Email", { exact: true }).fill("author@example.test");
  await page.getByLabel("Mật khẩu", { exact: true }).fill("TestLogIn!23");
  await page.getByRole("button", { name: "Đăng nhập", exact: true }).click();
}

test("Dashboard đã prefetch lúc là khách vẫn mở qua Router sau khi đăng nhập", async ({ page }) => {
  // The native Playwright fixture keeps Next.js prefetch enabled.
  await stubSessionApi(page);
  const documentPaths: string[] = [];
  page.on("request", request => {
    if (request.isNavigationRequest() && request.frame() === page.mainFrame()) {
      documentPaths.push(new URL(request.url()).pathname);
    }
  });
  const prefetched = page.waitForResponse(response => {
    const headers = response.request().headers();
    return new URL(response.url()).pathname === "/dashboard" &&
      Boolean(headers["next-router-prefetch"] || headers["next-router-segment-prefetch"]);
  });

  const response = await page.goto("/dashboard");
  expect(response?.request().redirectedFrom()).not.toBeNull();
  await expect(page).toHaveURL(url =>
    url.pathname === "/auth/login" && url.searchParams.get("callbackUrl") === "/dashboard");
  await expect(page.getByRole("link", { name: "Đăng nhập", exact: true })).toBeVisible();
  await page.evaluate(() => {
    // Use Next's exposed router in the test so the prefetch does not depend on Footer links.
    const app = window as Window & { next: { router: { prefetch(href: string): void } } };
    app.next.router.prefetch("/dashboard");
  });
  expect((await prefetched).status()).toBe(200);

  await login(page);
  await expect(page).toHaveURL(/\/dashboard$/);
  await expect(page.getByRole("heading", { name: "Tổng quan", level: 1 })).toBeVisible();
  expect(documentPaths).toEqual(["/dashboard", "/auth/login"]);
});

test("Đăng nhập giữ callback, khôi phục phiên khi refresh và đăng nhập lại về Dashboard", async ({ page }) => {
  await stubSessionApi(page);
  await page.goto("/dashboard/recipes?view=mine");
  await expect(page).toHaveURL(url =>
    url.pathname === "/auth/login" && url.searchParams.get("callbackUrl") === "/dashboard/recipes?view=mine");
  await login(page);
  await expect(page).toHaveURL(/\/dashboard\/recipes\?view=mine$/);
  await expect(page.getByRole("heading", { name: "Công thức của tôi", level: 1 })).toBeVisible();

  await page.reload();
  await expect(page.getByRole("heading", { name: "Công thức của tôi", level: 1 })).toBeVisible();

  await page.getByRole("button", { name: "Mở menu tài khoản" }).click();
  await page.getByRole("menuitem", { name: "Đăng xuất" }).click();
  await expect(page).toHaveURL(/\/auth\/login(?:\?.*)?$/);
  await page.goto("/auth/login");
  await login(page);
  await expect(page).toHaveURL(/\/dashboard$/);
  const menu = page.getByRole("navigation", { name: "Điều hướng quản trị" });
  await expect(menu.getByRole("link")).toHaveText(["Tổng quan", "Công thức của tôi", "Tạo công thức mới", "Về trang chủ"]);
});

test("Đăng ký vẫn chuyển đến hồ sơ khi API trả phiên đăng nhập", async ({ page }) => {
  await stubSessionApi(page);
  await page.goto("/auth/register");
  await page.getByLabel("Tên hiển thị", { exact: true }).fill("Tác giả kiểm thử");
  await page.getByLabel("Email", { exact: true }).fill("author@example.test");
  await page.getByLabel("Mật khẩu", { exact: true }).fill("TestSignUp!23");
  await page.getByLabel("Nhập lại mật khẩu", { exact: true }).fill("TestSignUp!23");
  await page.getByRole("button", { name: "Đăng ký", exact: true }).click();
  await expect(page).toHaveURL(/\/profile$/);
  await expect(page.getByText(/Đăng ký thành công/)).toBeVisible();
});
