import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { TransactionService } from '../../../../core/services/transaction.service';
import { ConvertedTransaction } from '../../../../core/models/transaction.models';

@Component({
  selector: 'app-transaction-detail',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatDividerModule,
  ],
  template: `
    <mat-card class="detail-card">
      <mat-card-header>
        <mat-card-title>Transaction Detail</mat-card-title>
        <mat-card-subtitle>ID: {{ transactionId }}</mat-card-subtitle>
      </mat-card-header>

      <mat-card-content>
        <!-- Currency lookup form -->
        <form [formGroup]="currencyForm" (ngSubmit)="onConvert()" novalidate class="currency-form">
          <mat-form-field appearance="outline">
            <mat-label>Target Currency (ISO 4217)</mat-label>
            <input matInput formControlName="currency"
                   placeholder="e.g. EUR, GBP, JPY"
                   maxlength="3" style="text-transform:uppercase" />
            @if (currencyForm.get('currency')?.hasError('required') && currencyForm.get('currency')?.touched) {
              <mat-error>Currency code is required.</mat-error>
            }
            @if (currencyForm.get('currency')?.hasError('pattern')) {
              <mat-error>Must be a 3-letter ISO 4217 code (e.g. EUR).</mat-error>
            }
          </mat-form-field>
          <button mat-flat-button color="primary" type="submit"
                  [disabled]="currencyForm.invalid || loading()">
            Convert
          </button>
        </form>

        @if (loading()) {
          <div class="spinner-wrap"><mat-spinner diameter="40" /></div>
        }

        @if (result()) {
          <mat-divider class="divider" />
          <div class="result-grid">
            <div class="label">Description</div>
            <div class="value">{{ result()!.description }}</div>

            <div class="label">Transaction Date</div>
            <div class="value">{{ result()!.transactionDate }}</div>

            <div class="label">Original Amount (USD)</div>
            <div class="value">{{ result()!.originalAmountUsd | currency:'USD' }}</div>

            <div class="label">Exchange Rate ({{ result()!.targetCurrency }})</div>
            <div class="value">{{ result()!.exchangeRate }}</div>

            <div class="label">Converted Amount ({{ result()!.targetCurrency }})</div>
            <div class="value converted">{{ result()!.convertedAmount | number:'1.2-2' }} {{ result()!.targetCurrency }}</div>
          </div>
        }
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .detail-card { max-width: 600px; margin: 2rem auto; }
    .currency-form { display: flex; gap: 1rem; align-items: flex-start; margin-bottom: 1rem; }
    .spinner-wrap { display: flex; justify-content: center; padding: 1rem; }
    .divider { margin: 1rem 0; }
    .result-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.5rem 1rem; }
    .label { font-weight: 500; color: #555; }
    .converted { font-weight: 700; font-size: 1.1rem; color: #1565c0; }
  `],
})
export class TransactionDetailComponent implements OnInit {
  private readonly service = inject(TransactionService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly result = signal<ConvertedTransaction | null>(null);
  transactionId = '';

  readonly currencyForm = this.fb.group({
    currency: ['', [Validators.required, Validators.pattern(/^[A-Za-z]{3}$/)]],
  });

  ngOnInit(): void {
    this.transactionId = this.route.snapshot.paramMap.get('id') ?? '';
  }

  onConvert(): void {
    if (this.currencyForm.invalid) {
      this.currencyForm.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.result.set(null);

    this.service.getWithCurrency(
      this.transactionId,
      this.currencyForm.value.currency!
    ).subscribe({
      next: (res) => { this.result.set(res); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
