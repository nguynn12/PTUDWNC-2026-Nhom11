import type { Page } from "@playwright/test";
import { expect, test } from "./fixtures";

const apiUrl = process.env.PLAYWRIGHT_API_URL;
const authorEmail = process.env.E2E_AUTHOR_EMAIL;
const authorPassword = process.env.E2E_AUTHOR_PASSWORD;
const titlePlaceholder = /Ví dụ: Phở Bò Truyền Thống/;
const descriptionPlaceholder = /Giới thiệu hương vị đặc trưng/;

function nutritionToggle(page: Page) {
  return page.getByRole("checkbox", { name: /Bổ sung thông số Dinh dưỡng/ });
}

async function fillRecipe(page: Page) {
  await page.getByPlaceholder(titlePlaceholder).fill(`Cơm kiểm thử ${crypto.randomUUID()}`);
  await page.getByPlaceholder(descriptionPlaceholder).fill("Công thức kiểm thử độc lập cho phần dinh dưỡng.");
  await expect(page.locator("select").first()).toHaveValue(/[0-9a-f-]{36}/);
}

async function createDraft(page: Page) {
  const saved = page.waitForResponse(response =>
    new URL(response.url()).pathname.replace(/\/$/, "") === "/api/v1/recipes" &&
    response.request().method() === "POST");
  await page.getByRole("button", { name: /Khởi tạo Công thức & Sang Bước 2/ }).click();
  const response = await saved;
  expect(response.status()).toBe(201);
  return { data: (await response.json()).data, payload: response.request().postDataJSON() };
}

async function findInDashboard(page: Page, title: string) {
  if (!new URL(page.url()).pathname.endsWith("/dashboard/recipes")) {
    await page.getByRole("link", { name: "Công thức của tôi", exact: true }).click();
  }
  await page.getByPlaceholder("Tìm theo tên công thức...").fill(title);
  return page.getByRole("link", { name: title, exact: true });
}

async function openEditor(page: Page, id: string, title: string) {
  await (await findInDashboard(page, title)).click();
  await page.waitForURL(`**/dashboard/recipes/${id}/edit`);
  await expect(page.getByRole("button", { name: "Lưu thông tin", exact: true })).toBeEnabled();
}

async function saveEdit(page: Page, id: string) {
  const saved = page.waitForResponse(response =>
    response.url().endsWith(`/recipes/${id}`) && response.request().method() === "PUT");
  await page.getByRole("button", { name: "Lưu thông tin", exact: true }).click();
  const response = await saved;
  expect(response.status()).toBe(200);
  return (await response.json()).data;
}

test.describe("Vòng đời và dinh dưỡng Đợt 2", () => {
  test.skip(!apiUrl || !authorEmail || !authorPassword, "Cần API và Author trên database kiểm thử riêng.");
  let accessToken: string;

  test.beforeEach(async ({ page }) => {
    await page.goto("/auth/login?callbackUrl=/dashboard/recipes/new");
    await page.getByLabel("Email", { exact: true }).fill(authorEmail!);
    await page.getByLabel("Mật khẩu", { exact: true }).fill(authorPassword!);
    const response = page.waitForResponse(r => r.url().endsWith("/auth/login") && r.request().method() === "POST");
    await page.getByRole("button", { name: "Đăng nhập", exact: true }).click();
    const signedIn = await response;
    expect(signedIn.status()).toBe(200);
    accessToken = (await signedIn.json()).data.accessToken;
    await page.waitForURL("**/dashboard/recipes/new");
  });

  for (const enabled of [false, true]) {
    test(`${enabled ? "Bật và để trống" : "Không bật"} dinh dưỡng khi tạo nháp lưu null`, async ({ page }) => {
      await fillRecipe(page);
      await expect(nutritionToggle(page)).not.toBeChecked();
      if (enabled) await nutritionToggle(page).check();
      const { data, payload } = await createDraft(page);
      expect(payload.nutrition).toBeNull();
      expect(data.nutrition).toBeNull();
      await openEditor(page, data.id, data.title);
      await expect(nutritionToggle(page)).not.toBeChecked();
      await nutritionToggle(page).check();
      await expect(page.getByLabel("Calo (kcal)", { exact: true })).toHaveValue("");
      await expect(page.getByLabel("Natri (mg)", { exact: true })).toHaveValue("");
    });
  }

  test("Lưu số 0 và chỉ số nhập riêng, sửa và bỏ chọn có chủ đích", async ({ page }) => {
    await fillRecipe(page);
    await nutritionToggle(page).check();
    await page.getByLabel("Calo (kcal)", { exact: true }).fill("0");
    await page.getByLabel("Chất đạm (g)", { exact: true }).fill("7.25");
    const { data, payload } = await createDraft(page);
    expect(payload.nutrition).toEqual({ calories: 0, protein: 7.25 });
    const nutrition = { calories: 0, protein: 7.25, fat: null, carbohydrates: null, fiber: null, sodium: null };
    expect(data.nutrition).toMatchObject(nutrition);

    await openEditor(page, data.id, data.title);
    await expect(nutritionToggle(page)).toBeChecked();
    await expect(page.getByLabel("Calo (kcal)", { exact: true })).toHaveValue("0");
    await expect(page.getByLabel("Chất béo (g)", { exact: true })).toHaveValue("");
    await nutritionToggle(page).uncheck();
    await nutritionToggle(page).check();
    await expect(page.getByLabel("Chất đạm (g)", { exact: true })).toHaveValue("7.25");
    await page.getByPlaceholder(descriptionPlaceholder).fill("Chỉ chỉnh sửa mô tả và giữ nguyên thông tin dinh dưỡng.");
    expect((await saveEdit(page, data.id)).nutrition).toMatchObject(nutrition);

    await openEditor(page, data.id, data.title);
    await expect(page.getByLabel("Chất đạm (g)", { exact: true })).toHaveValue("7.25");
    await page.getByLabel("Chất đạm (g)", { exact: true }).clear();
    expect((await saveEdit(page, data.id)).nutrition).toMatchObject({ ...nutrition, protein: null });
    await openEditor(page, data.id, data.title);
    await expect(page.getByLabel("Calo (kcal)", { exact: true })).toHaveValue("0");
    await expect(page.getByLabel("Chất đạm (g)", { exact: true })).toHaveValue("");
    await nutritionToggle(page).uncheck();
    await expect(page.getByText(/sẽ được xóa khi lưu/)).toBeVisible();
    expect((await saveEdit(page, data.id)).nutrition).toBeNull();
    await openEditor(page, data.id, data.title);
    await expect(nutritionToggle(page)).not.toBeChecked();
  });

  test("Ẩn nút Publish của bài Archived và hiển thị 0 kcal sau khi xuất bản lại", async ({ page }) => {
    const authorization = { Authorization: `Bearer ${accessToken}` };

    await fillRecipe(page);
    await nutritionToggle(page).check();
    await page.getByLabel("Calo (kcal)", { exact: true }).fill("0");
    const { data } = await createDraft(page);
    const recipeUrl = `${apiUrl}/api/v1/recipes/${data.id}`;

    async function mutate(method: string, endpoint: string, body?: object) {
      const current = await page.request.get(recipeUrl, { headers: authorization });
      expect(current.status()).toBe(200);
      const etag = current.headers().etag;
      expect(etag).toMatch(/^"[1-9]\d*"$/);
      return page.request.fetch(recipeUrl + endpoint, {
        method, data: body, headers: { ...authorization, "If-Match": etag },
      });
    }

    expect((await mutate("POST", "/ingredients", { name: "Gạo", quantity: 200, unit: "g", orderIndex: 1 })).status()).toBe(201);
    expect((await mutate("POST", "/steps", { title: "Nấu cơm", description: "Nấu gạo với nước đến khi chín.", stepNumber: 1 })).status()).toBe(201);
    const published = await mutate("PATCH", "/publish");
    expect(published.status()).toBe(200);
    const publishEtag = published.headers().etag;
    expect(publishEtag).toMatch(/^"[1-9]\d*"$/);
    const retry = await mutate("PATCH", "/publish");
    expect(retry.status()).toBe(200);
    expect(retry.headers().etag).toBe(publishEtag);
    expect((await mutate("PATCH", "/archive")).status()).toBe(200);
    const stalePublish = await page.request.patch(recipeUrl + "/publish", {
      headers: { ...authorization, "If-Match": publishEtag },
    });
    expect(stalePublish.status()).toBe(409);
    expect((await stalePublish.json()).type).toBe("RECIPE_CONCURRENCY_CONFLICT");

    await openEditor(page, data.id, data.title);
    await expect(page.getByText("Trạng thái: Đã lưu trữ", { exact: true })).toBeVisible();
    await expect(page.getByRole("button", { name: /Hoàn tất & Xuất bản/ })).toHaveCount(0);
    await page.getByRole("button", { name: /Bước 4: Ảnh MinIO/ }).click();
    await expect(page.getByRole("button", { name: /Hoàn tất & Xuất bản/ })).toHaveCount(0);
    const invalid = await mutate("PATCH", "/publish");
    expect(invalid.status()).toBe(422);
    expect((await invalid.json()).type).toBe("RECIPE_INVALID_STATUS_TRANSITION");

    expect((await mutate("PATCH", "/unarchive")).status()).toBe(200);
    await openEditor(page, data.id, data.title);
    await expect(page.getByRole("button", { name: "Hoàn tất & Xuất bản", exact: true })).toBeVisible();
    const publish = page.waitForResponse(response =>
      response.url().endsWith(`/recipes/${data.id}/publish`) && response.request().method() === "PATCH");
    await page.getByRole("button", { name: "Hoàn tất & Xuất bản", exact: true }).click();
    expect((await publish).status()).toBe(200);
    await page.waitForURL("**/dashboard/recipes");
    await (await findInDashboard(page, data.title)).click();
    await page.waitForURL(`**/recipes/${data.slug}`);
    await expect(page.getByRole("heading", { level: 1, name: data.title })).toBeVisible();
    await expect(page.getByText("Năng lượng", { exact: true })).toBeVisible();
    await expect(page.getByText("0 kcal", { exact: true })).toHaveCount(2);
    await expect(page.getByText("Chất đạm", { exact: true })).toHaveCount(0);
  });
});
