import { expect, test } from "@playwright/test";

// Use the native fixture so Next.js keeps its production prefetch behavior.
test("Khách tìm kiếm, sắp xếp, phân trang và lọc với prefetch production", async ({ page }, testInfo) => {
  const prefetchedPaths = new Set<string>();
  const failedRequests: string[] = [];
  page.on("request", request => {
    const headers = request.headers();
    if (headers["next-router-prefetch"] || headers["next-router-segment-prefetch"]) {
      prefetchedPaths.add(new URL(request.url()).pathname);
    }
  });
  page.on("response", response => {
    if (new URL(response.url()).origin === new URL(page.url()).origin &&
        [429, 500].includes(response.status())) {
      failedRequests.push(`${response.status()} ${new URL(response.url()).pathname}`);
    }
  });

  try {
    await page.goto("/");
    await expect(page.getByRole("article").first()).toBeVisible();
    await expect.poll(() => prefetchedPaths.size).toBeGreaterThan(0);
    await page.getByRole("link", { name: "Tìm kiếm", exact: true }).first().click();
    const search = page.getByPlaceholder(/Tìm theo tên món/i);
    await search.fill("Bò sốt vang");
    const recipeLink = 'a[href="/recipes/bo-sot-vang"]';
    await expect(page.locator(recipeLink).first()).toBeVisible();

    await page.getByRole("link", { name: "Khám phá công thức", exact: true }).first().click();
    const total = page.getByText(/Tìm thấy \d+ công thức/).first();
    await expect(total).toBeVisible();
    const totalBefore = await total.innerText();
    const pagination = page.getByRole("navigation", { name: "Điều hướng phân trang" });
    await page.locator("#sort-select").selectOption("title");
    await expect(page).toHaveURL(/sortBy=title.*sortOrder=asc/);
    await expect(pagination.getByRole("link", { name: "2", exact: true })).toHaveAttribute("href", /sortBy=title/);
    await pagination.getByRole("link", { name: "2", exact: true }).click();
    await expect(pagination.locator('[aria-current="page"]')).toHaveText("2");
    await expect(total).toHaveText(totalBefore);

    const category = page.locator("#filter-category").first();
    const stew = category.getByRole("option", { name: /Món hầm & Cà ri/ });
    const categoryId = await stew.getAttribute("value");
    expect(categoryId).toBeTruthy();
    await category.selectOption(categoryId!);
    await page.getByRole("button", { name: "Áp dụng bộ lọc", exact: true }).filter({ visible: true }).click();
    await expect(page).toHaveURL(url => url.searchParams.get("categoryId") === categoryId && url.searchParams.get("page") === "1");
    await expect(page.locator(recipeLink).first()).toBeVisible();
    await page.locator(recipeLink).first().click();
    await expect(page.getByRole("heading", { name: "Bò sốt vang", level: 1 })).toBeVisible();
    expect(failedRequests).toEqual([]);
    expect([...prefetchedPaths].filter(path => path.startsWith("/recipes/"))).toEqual([]);
  } finally {
    await testInfo.attach("navigation-requests", {
      body: JSON.stringify({ prefetchedPaths: [...prefetchedPaths], failedRequests }),
      contentType: "application/json",
    });
  }
});
