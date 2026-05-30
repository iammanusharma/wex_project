import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TransactionService } from '../../../../core/services/transaction.service';

@Component({
  selector: 'app-create-transaction',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <mat-card class="form-card">
      <mat-card-header>
        <mat-card-title>Store Purchase Transaction</mat-card-title>
        <mat-card-subtitle>All amounts in US Dollars (USD)</mat-card-subtitle>
      </mat-card-header>

      <mat-card-content>
        <form [formGroup]="form" (ngSubmit)="onSubmit()" novalidate>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Description</mat-label>
            <input matInput formControlName="description"
                   placeholder="e.g. Office supplies"
                   maxlength="50" />
            <mat-hint align="end">{{ form.get('description')?.value?.length ?? 0 }}/50</mat-hint>
            @if (form.get('description')?.hasError('required') && form.get('description')?.touched) {
              <mat-error>Description is required.</mat-error>
            }
            @if (form.get('description')?.hasError('maxlength')) {
              <mat-error>Description must not exceed 50 characters.</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Transaction Date</mat-label>
            <input matInput [matDatepicker]="picker" formControlName="transactionDate" />
            <mat-datepicker-toggle matIconSuffix [for]="picker" />
            <mat-datepicker #picker />
            @if (form.get('transactionDate')?.hasError('required') && form.get('transactionDate')?.touched) {
              <mat-error>Transaction date is required.</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Amount (USD)</mat-label>
            <span matTextPrefix>$&nbsp;</span>
            <input matInput type="number" formControlName="amountUsd"
                   placeholder="0.00" step="0.01" min="0.01" />
            @if (form.get('amountUsd')?.hasError('required') && form.get('amountUsd')?.touched) {
              <mat-error>Amount is required.</mat-error>
            }
            @if (form.get('amountUsd')?.hasError('min')) {
              <mat-error>Amount must be greater than zero.</mat-error>
            }
          </mat-form-field>

          <div class="actions">
            <button mat-stroked-button type="button" routerLink="/transactions">Cancel</button>
            <button mat-flat-button color="primary" type="submit"
                    [disabled]="form.invalid || submitting()">
              @if (submitting()) {
                <mat-spinner diameter="20" />
              } @else {
                Store Transaction
              }
            </button>
          </div>

        </form>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .form-card { max-width: 560px; margin: 2rem auto; }
    .full-width { width: 100%; margin-bottom: 1rem; }
    .actions { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 1rem; }
  `],
})
export class CreateTransactionComponent {
  private readonly service = inject(TransactionService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly submitting = signal(false);

  readonly form = this.fb.group({
    description: ['', [Validators.required, Validators.maxLength(50)]],
    transactionDate: [null as Date | null, Validators.required],
    amountUsd: [null as number | null, [Validators.required, Validators.min(0.01)]],
  });

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    const date = this.form.value.transactionDate as Date;
    const isoDate = date.toISOString().split('T')[0]; // yyyy-MM-dd

    this.service.create({
      description: this.form.value.description!,
      transactionDate: isoDate,
      amountUsd: this.form.value.amountUsd!,
    }).subscribe({
      next: (res) => this.router.navigate(['/transactions', res.id]),
      error: () => this.submitting.set(false),
    });
  }
}
