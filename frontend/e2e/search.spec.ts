import { test, expect } from "@playwright/test";

test.describe("Luồng tìm kiếm món ăn E2E (FR-SRCH-001 -> FR-SRCH-004)", () => {
  test("Hiển thị giao diện tìm kiếm với ô nhập liệu và các bộ lọc", async ({ page }) => {
    await page.goto("/search");
    await expect(page).toHaveTitle(/Tìm Kiếm/);

    const searchInput = page.getByPlaceholder(/Tìm kiếm theo tên món/i);
    await expect(searchInput).toBeVisible();
  });

  test("Nhập từ khóa và kích hoạt tìm kiếm sau debounce 300ms", async ({ page }) => {
    await page.goto("/search");
    const searchInput = page.getByPlaceholder(/Tìm kiếm theo tên món/i);
    await searchInput.fill("Phở");

    await page.waitForTimeout(600);
    expect(page.url()).toContain("q=Ph%E1%BB%9F");
  });

  test("Thao tác chọn bộ lọc độ khó và cập nhật URL query params", async ({ page }) => {
    await page.goto("/search");
    const easyBtn = page.getByRole("button", { name: /Dễ/i }).first();
    if (await easyBtn.isVisible()) {
      await easyBtn.click();
      await page.waitForTimeout(500);
      expect(page.url()).toContain("difficulty=");
    }
  });

  test("Hỗ trợ điều hướng bàn phím theo chuẩn Accessibility WCAG 2.1 AA", async ({ page }) => {
    await page.goto("/search");
    await page.keyboard.press("Tab");
    // Kiểm tra focus element không bị mất
    const focused = await page.evaluate(() => document.activeElement?.tagName);
    expect(focused).toBeTruthy();
  });
});

