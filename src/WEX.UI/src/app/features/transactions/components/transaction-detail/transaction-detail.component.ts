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
import { MatIconModule } from '@angular/material/icon';
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
    MatIconModule,
  ],
  template: `
    <mat-card class="detail-card">
      <mat-card-header>
        <mat-card-title>Transaction Detail</mat-card-title>
        @if (!lookupMode) {
          <mat-card-subtitle>ID: {{ transactionId }}</mat-card-subtitle>
        }
      </mat-card-header>

      <mat-card-content>
        <!-- Lookup form: ID + currency when navigated from nav menu -->
        @if (lookupMode) {
          <form [formGroup]="lookupForm" (ngSubmit)="onLookup()" novalidate class="lookup-form">
            <mat-form-field appearance="outline" class="id-field">
              <mat-label>Transaction ID</mat-label>
              <input matInput formControlName="transactionId" placeholder="Paste transaction UUID" />
              @if (lookupForm.get('transactionId')?.hasError('required') && lookupForm.get('transactionId')?.touched) {
                <mat-error>Transaction ID is required.</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline" class="currency-field">
              <mat-label>Target Currency (ISO 4217)</mat-label>
              <input matInput formControlName="currency"
                     placeholder="e.g. EUR, GBP, JPY"
                     maxlength="3" style="text-transform:uppercase" />
              @if (lookupForm.get('currency')?.hasError('required') && lookupForm.get('currency')?.touched) {
                <mat-error>Currency code is required.</mat-error>
              }
              @if (lookupForm.get('currency')?.hasError('pattern')) {
                <mat-error>Must be a 3-letter ISO 4217 code (e.g. EUR).</mat-error>
              }
            </mat-form-field>

            <button mat-flat-button color="primary" type="submit"
                    [disabled]="lookupForm.invalid || loading()">
              <mat-icon>search</mat-icon> Look Up
            </button>
          </form>
        } @else {
          <!-- Currency-only form when ID comes from route -->
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
        }

        @if (loading()) {
          <div class="spinner-wrap"><mat-spinner diameter="40" /></div>
        }

        @if (errorMessage()) {
          <div class="error-banner">
            <mat-icon color="warn">error_outline</mat-icon>
            <span>{{ errorMessage() }}</span>
          </div>
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
    .detail-card { max-width: 680px; margin: 2rem auto; }
    .currency-form { display: flex; gap: 1rem; align-items: flex-start; margin-bottom: 1rem; }
    .lookup-form { display: flex; gap: 1rem; align-items: flex-start; margin-bottom: 1rem; flex-wrap: wrap; }
    .id-field { flex: 2 1 260px; }
    .currency-field { flex: 1 1 140px; }
    .spinner-wrap { display: flex; justify-content: center; padding: 1rem; }
    .divider { margin: 1rem 0; }
    .result-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.5rem 1rem; }
    .label { font-weight: 500; color: #555; }
    .converted { font-weight: 700; font-size: 1.1rem; color: #1565c0; }
    .error-banner {
      display: flex; align-items: center; gap: 8px;
      color: #c62828; background: #ffebee;
      border-radius: 4px; padding: 10px 16px; margin: 8px 0;
      font-size: 0.9rem;
    }
  `],
})
export class TransactionDetailComponent implements OnInit {
  private readonly service = inject(TransactionService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly result = signal<ConvertedTransaction | null>(null);
  readonly errorMessage = signal<string | null>(null);

  transactionId = '';
  lookupMode = false;

  readonly currencyForm = this.fb.group({
    currency: ['', [Validators.required, Validators.pattern(/^[A-Za-z]{3}$/)]],
  });

  readonly lookupForm = this.fb.group({
    transactionId: ['', Validators.required],
    currency: ['', [Validators.required, Validators.pattern(/^[A-Za-z]{3}$/)]],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.transactionId = id;
      this.lookupMode = false;
    } else {
      this.lookupMode = true;
    }
  }

  onConvert(): void {
    if (this.currencyForm.invalid) { this.currencyForm.markAllAsTouched(); return; }
    this.fetch(this.transactionId, this.currencyForm.value.currency!);
  }

  onLookup(): void {
    if (this.lookupForm.invalid) { this.lookupForm.markAllAsTouched(); return; }
    const { transactionId, currency } = this.lookupForm.value;
    this.fetch(transactionId!, currency!);
  }

  private fetch(id: string, currency: string): void {
    this.loading.set(true);
    this.result.set(null);
    this.errorMessage.set(null);

    this.service.getWithCurrency(id, currency).subscribe({
      next: (res) => { this.result.set(res); this.loading.set(false); },
      error: (err) => {
        this.loading.set(false);
        if (err.status === 404) {
          this.errorMessage.set(`Transaction "${id}" was not found. Please check the ID and try again.`);
        } else if (err.status === 422) {
          this.errorMessage.set('Currency conversion unavailable: no exchange rate found for the given currency near the transaction date.');
        } else {
          this.errorMessage.set('An unexpected error occurred. Please try again.');
        }
      },
    });
  }
}

