import { type Page, type Locator } from '@playwright/test';

/**
 * Page Object Model for the Transaction Detail / Currency Conversion page.
 * Route: /transactions/:id
 *
 * Encapsulates selectors matching the Angular Material template in
 * transaction-detail.component.ts so tests don't hard-code CSS selectors.
 */
export class TransactionDetailPage {
  readonly page: Page;

  // Currency conversion form
  readonly currencyInput: Locator;
  readonly convertButton: Locator;

  // Result grid fields (shown after successful conversion)
  readonly descriptionValue: Locator;
  readonly transactionDateValue: Locator;
  readonly originalAmountValue: Locator;
  readonly exchangeRateLabel: Locator;
  readonly convertedAmountValue: Locator;

  // Loading + error states
  readonly loadingSpinner: Locator;
  readonly currencyError: Locator;

  constructor(page: Page) {
    this.page = page;

    this.currencyInput = page.getByLabel(/target currency/i);
    this.convertButton = page.getByRole('button', { name: /convert/i });

    // Result grid — match by label text in the result-grid div
    this.descriptionValue = page.locator('.result-grid .value').nth(0);
    this.transactionDateValue = page.locator('.result-grid .value').nth(1);
    this.originalAmountValue = page.locator('.result-grid .value').nth(2);
    this.exchangeRateLabel = page.locator('.result-grid .label').nth(3);
    this.convertedAmountValue = page.locator('.result-grid .value.converted');

    this.loadingSpinner = page.locator('mat-spinner');
    this.currencyError = page.getByText(/3-letter iso 4217|currency code is required/i);
  }

  async goto(transactionId: string) {
    await this.page.goto(`/transactions/${transactionId}`);
    await this.page.waitForLoadState('networkidle');
  }

  async convertCurrency(currencyCode: string) {
    await this.currencyInput.fill(currencyCode);
    await this.convertButton.click();
  }

  async waitForResult() {
    // Wait for the converted amount element to appear (API call completes)
    await this.convertedAmountValue.waitFor({ state: 'visible', timeout: 15_000 });
  }
}
