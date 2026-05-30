import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'transactions', pathMatch: 'full' },
  {
    path: 'transactions',
    loadComponent: () =>
      import('./features/transactions/components/create-transaction/create-transaction.component')
        .then(m => m.CreateTransactionComponent),
  },
  {
    path: 'transactions/:id',
    loadComponent: () =>
      import('./features/transactions/components/transaction-detail/transaction-detail.component')
        .then(m => m.TransactionDetailComponent),
  },
  { path: '**', redirectTo: 'transactions' },
];
