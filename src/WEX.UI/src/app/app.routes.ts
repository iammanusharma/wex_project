import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'transactions', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component')
        .then(m => m.LoginComponent),
  },
  {
    path: 'transactions',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/transactions/components/create-transaction/create-transaction.component')
        .then(m => m.CreateTransactionComponent),
  },
  {
    path: 'transactions/lookup',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/transactions/components/transaction-detail/transaction-detail.component')
        .then(m => m.TransactionDetailComponent),
  },
  {
    path: 'transactions/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/transactions/components/transaction-detail/transaction-detail.component')
        .then(m => m.TransactionDetailComponent),
  },
  { path: '**', redirectTo: 'transactions' },
];
