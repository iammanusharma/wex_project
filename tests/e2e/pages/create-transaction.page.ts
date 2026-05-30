import { type Page, type Locator } from '@playwright/test';

/**
 * Page Object Model for the Create Transaction form.
 * Encapsulates all selectors and actions for the transaction creation UI,
 * making tests resilient to markup changes.
 */
export class CreateTransactionPage {
  readonly page: Page;

  // Form fields
  readonly descriptionInput: Locator;
  readonly transactionDateInput: Locator;
  readonly amountInput: Locator;
  readonly submitButton: Locator;

  // Feedback elements
  readonly successMessage: Locator;
  readonly descriptionError: Locator;
  readonly amountError: Locator;

  constructor(page: Page) {
    this.page = page;
    this.descriptionInput = page.getByLabel(/description/i);
    this.transactionDateInput = page.getByLabel(/transaction date/i);
    this.amountInput = page.getByLabel(/amount/i);
    this.submitButton = page.getByRole('button', { name: /save|submit|store/i });
    this.successMessage = page.getByText(/transaction saved|success/i);
    this.descriptionError = page.getByText(/description is required|must not exceed/i);
    this.amountError = page.getByText(/positive value|amount/i);
  }

  async goto() {
    await this.page.goto('/transactions/new');
    await this.page.waitForLoadState('networkidle');
  }

  async fillForm(description: string, date: string, amount: string) {
    await this.descriptionInput.fill(description);
    await this.transactionDateInput.fill(date);
    await this.amountInput.fill(amount);
  }

  async submit() {
    await this.submitButton.click();
  }

  async fillAndSubmit(description: string, date: string, amount: string) {
    await this.fillForm(description, date, amount);
    await this.submit();
  }
}
