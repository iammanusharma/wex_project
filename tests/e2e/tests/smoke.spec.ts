import { test, expect } from '@playwright/test';

/**
 * Smoke tests — quick sanity checks that run before the full suite.
 * These verify the app is up and basic navigation works.
 */
test.describe('Smoke Tests', () => {
  test('home page loads without errors', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // No error page
    await expect(page).not.toHaveTitle(/error|not found/i);
  });

  test('navigating to /transactions/new shows create form', async ({ page }) => {
    await page.goto('/transactions/new');
    await page.waitForLoadState('networkidle');

    await expect(page.getByRole('button', { name: /save|submit|store/i })).toBeVisible();
  });

  test('page has correct title', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveTitle(/WEX/i);
  });
});
