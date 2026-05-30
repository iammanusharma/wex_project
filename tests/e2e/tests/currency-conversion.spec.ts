import { test, expect } from '@playwright/test';
import { CreateTransactionPage } from '../pages/create-transaction.page';
import { TransactionDetailPage } from '../pages/transaction-detail.page';

/**
 * E2E tests for the currency conversion flow (Requirement 2).
 *
 * User journey tested:
 *   1. Create a new purchase transaction via the form
 *   2. Navigate to the transaction detail page using the returned ID
 *   3. Enter a target currency code
 *   4. Click Convert
 *   5. Verify the converted amount is displayed with all required fields
 *
 * Prerequisites:
 *   - Angular UI running: npm run start:local (port 4200)
 *   - .NET API running: dotnet run (port 5000)
 *   - PostgreSQL running: docker-compose up db
 *   - Treasury API reachable: https://fiscaldata.treasury.gov
 */
test.describe('Currency Conversion Flow', () => {

  // ---------------------------------------------------------------------------
  // Form visibility & validation
  // ---------------------------------------------------------------------------

  test('should show currency input and convert button on detail page', async ({ page }) => {
    const detailPage = new TransactionDetailPage(page);
    // Navigate with a dummy GUID — we just want to check the form renders
    await detailPage.goto('00000000-0000-0000-0000-000000000001');

    await expect(detailPage.currencyInput).toBeVisible();
    await expect(detailPage.convertButton).toBeVisible();
  });

  test('should disable convert button when currency field is empty', async ({ page }) => {
    const detailPage = new TransactionDetailPage(page);
    await detailPage.goto('00000000-0000-0000-0000-000000000001');

    // Field is empty by default — button should be disabled
    await expect(detailPage.convertButton).toBeDisabled();
  });

  test('should show validation error for invalid currency code', async ({ page }) => {
    const detailPage = new TransactionDetailPage(page);
    await detailPage.goto('00000000-0000-0000-0000-000000000001');

    // Type an invalid code then blur
    await detailPage.currencyInput.fill('XX');
    await detailPage.currencyInput.blur();

    await expect(detailPage.currencyError).toBeVisible();
  });

  test('should show validation error for numeric currency code', async ({ page }) => {
    const detailPage = new TransactionDetailPage(page);
    await detailPage.goto('00000000-0000-0000-0000-000000000001');

    await detailPage.currencyInput.fill('123');
    await detailPage.currencyInput.blur();

    await expect(detailPage.currencyError).toBeVisible();
  });

  // ---------------------------------------------------------------------------
  // Full end-to-end journey (requires running API + Treasury API)
  // ---------------------------------------------------------------------------

  test('full journey: create transaction then convert to EUR', async ({ page }) => {
    // Step 1: Create a transaction via the form
    const createPage = new CreateTransactionPage(page);
    await createPage.goto();
    await createPage.fillAndSubmit(
      'E2E currency test - hotel',
      '2024-06-15',
      '150.00'
    );

    // Step 2: Wait for success and capture the transaction ID from the URL
    // After submit the app should navigate to /transactions/:id or show the ID
    await page.waitForURL(/\/transactions\/[0-9a-f-]{36}/i, { timeout: 10_000 });
    const url = page.url();
    const idMatch = url.match(/transactions\/([0-9a-f-]{36})/i);
    const transactionId = idMatch ? idMatch[1] : '';
    expect(transactionId).not.toBe('');

    // Step 3: Navigate to the detail page
    const detailPage = new TransactionDetailPage(page);
    await detailPage.goto(transactionId);

    // Step 4: Convert to EUR
    await detailPage.convertCurrency('EUR');

    // Step 5: Wait for result and assert all required fields are shown
    await detailPage.waitForResult();

    await expect(detailPage.descriptionValue).toContainText('E2E currency test - hotel');
    await expect(detailPage.originalAmountValue).toContainText('150');
    await expect(detailPage.convertedAmountValue).toBeVisible();
    await expect(detailPage.convertedAmountValue).toContainText('EUR');
  });

  test('full journey: create transaction then convert to GBP', async ({ page }) => {
    // Step 1: Create transaction
    const createPage = new CreateTransactionPage(page);
    await createPage.goto();
    await createPage.fillAndSubmit(
      'E2E GBP conversion test',
      '2024-03-10',
      '200.00'
    );

    await page.waitForURL(/\/transactions\/[0-9a-f-]{36}/i, { timeout: 10_000 });
    const url = page.url();
    const idMatch = url.match(/transactions\/([0-9a-f-]{36})/i);
    const transactionId = idMatch ? idMatch[1] : '';
    expect(transactionId).not.toBe('');

    // Step 2: Convert to GBP
    const detailPage = new TransactionDetailPage(page);
    await detailPage.goto(transactionId);
    await detailPage.convertCurrency('GBP');

    await detailPage.waitForResult();

    await expect(detailPage.convertedAmountValue).toBeVisible();
    await expect(detailPage.convertedAmountValue).toContainText('GBP');
  });

  test('convert with unknown transaction ID shows error state', async ({ page }) => {
    const detailPage = new TransactionDetailPage(page);
    await detailPage.goto('00000000-0000-0000-0000-000000000099');

    await detailPage.convertCurrency('EUR');

    // The error interceptor should show a snackbar or error message
    // The result grid should NOT appear
    await expect(detailPage.convertedAmountValue).not.toBeVisible({ timeout: 8_000 });
  });

  // ---------------------------------------------------------------------------
  // Exchange rate label includes currency code
  // ---------------------------------------------------------------------------

  test('exchange rate label shows target currency code', async ({ page }) => {
    // Create a transaction
    const createPage = new CreateTransactionPage(page);
    await createPage.goto();
    await createPage.fillAndSubmit('Rate label test', '2024-06-15', '50.00');

    await page.waitForURL(/\/transactions\/[0-9a-f-]{36}/i, { timeout: 10_000 });
    const url = page.url();
    const idMatch = url.match(/transactions\/([0-9a-f-]{36})/i);
    const transactionId = idMatch ? idMatch[1] : '';

    const detailPage = new TransactionDetailPage(page);
    await detailPage.goto(transactionId);
    await detailPage.convertCurrency('CAD');
    await detailPage.waitForResult();

    // The label should say "Exchange Rate (CAD)"
    await expect(detailPage.exchangeRateLabel).toContainText('CAD');
  });
});
