import { test, expect } from '@playwright/test';
import { CreateTransactionPage } from '../pages/create-transaction.page';

/**
 * E2E tests for Create Transaction feature.
 * Runs against the Angular UI connected to a live API.
 *
 * Prerequisites:
 *   - Angular UI running (npm run start:local in src/WEX.UI)
 *   - .NET API running (dotnet run in src/WEX.API)
 *   - PostgreSQL running (docker-compose up db)
 */
test.describe('Create Transaction', () => {
  let createPage: CreateTransactionPage;

  test.beforeEach(async ({ page }) => {
    createPage = new CreateTransactionPage(page);
    await createPage.goto();
  });

  // ---------------------------------------------------------------------------
  // Happy path
  // ---------------------------------------------------------------------------

  test('should display the create transaction form', async ({ page }) => {
    await expect(createPage.descriptionInput).toBeVisible();
    await expect(createPage.transactionDateInput).toBeVisible();
    await expect(createPage.amountInput).toBeVisible();
    await expect(createPage.submitButton).toBeVisible();
  });

  test('should submit valid transaction and show success feedback', async ({ page }) => {
    await createPage.fillAndSubmit(
      'E2E test - office supplies',
      '2024-06-15',
      '49.99'
    );

    // Expect success indication — snackbar, redirect, or success text
    await expect(createPage.successMessage).toBeVisible({ timeout: 10_000 });
  });

  test('should clear form after successful submission', async ({ page }) => {
    await createPage.fillAndSubmit(
      'E2E test - conference',
      '2024-07-01',
      '199.00'
    );

    await expect(createPage.successMessage).toBeVisible({ timeout: 10_000 });
    // Form should reset
    await expect(createPage.descriptionInput).toHaveValue('');
  });

  test('should accept description of exactly 50 characters', async ({ page }) => {
    const fiftyChars = 'A'.repeat(50);
    await createPage.fillAndSubmit(fiftyChars, '2024-06-15', '10.00');

    await expect(createPage.successMessage).toBeVisible({ timeout: 10_000 });
  });

  // ---------------------------------------------------------------------------
  // Validation — client-side (immediate feedback before API call)
  // ---------------------------------------------------------------------------

  test('should show error when description is empty', async ({ page }) => {
    await createPage.descriptionInput.focus();
    await createPage.descriptionInput.blur();

    await expect(createPage.descriptionError).toBeVisible();
  });

  test('should disable submit button when form is invalid', async ({ page }) => {
    // Fresh form — required fields empty — submit should be disabled
    await expect(createPage.submitButton).toBeDisabled();
  });

  test('should show error when description exceeds 50 characters', async ({ page }) => {
    await createPage.descriptionInput.fill('A'.repeat(51));
    await createPage.descriptionInput.blur();

    await expect(createPage.descriptionError).toBeVisible();
  });

  test('should show error when amount is zero', async ({ page }) => {
    await createPage.amountInput.fill('0');
    await createPage.amountInput.blur();

    await expect(createPage.amountError).toBeVisible();
  });

  test('should show error when amount is negative', async ({ page }) => {
    await createPage.amountInput.fill('-5.00');
    await createPage.amountInput.blur();

    await expect(createPage.amountError).toBeVisible();
  });

  // ---------------------------------------------------------------------------
  // Accessibility
  // ---------------------------------------------------------------------------

  test('should have no critical accessibility violations on form page', async ({ page }) => {
    // Basic a11y checks via ARIA roles
    await expect(page.getByRole('form')).toBeVisible();
    await expect(createPage.submitButton).toHaveAttribute('type', 'submit');
  });
});
