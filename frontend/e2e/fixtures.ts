import { test as base } from "@playwright/test";

export { expect } from "@playwright/test";

export const test = base.extend({
  page: async ({ page }, provide) => {
    await page.route("**/*", route => {
      const headers = route.request().headers();
      // Production Next.js prefetches visible links in every fresh test context.
      // Skip speculative requests so they do not share the API budget of tested navigations.
      if (headers["next-router-prefetch"] || headers["next-router-segment-prefetch"]) {
        return route.abort();
      }
      return route.fallback();
    });
    await provide(page);
  },
});
