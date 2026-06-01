import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, MatToolbarModule, MatButtonModule, MatIconModule],
  template: `
    <mat-toolbar color="primary">
      <span>WEX Corporate Payments</span>
      <span class="spacer"></span>
      @if (auth.isAuthenticated()) {
        <button mat-button routerLink="/transactions">New Transaction</button>
        <button mat-button routerLink="/transactions/lookup">Transaction Detail</button>
        <button mat-icon-button (click)="auth.logout()" title="Sign out">
          <mat-icon>logout</mat-icon>
        </button>
      }
    </mat-toolbar>
    <main class="main-content">
      <router-outlet />
    </main>
  `,
  styles: [`
    .spacer { flex: 1 1 auto; }
    .main-content { padding: 1rem 2rem; }
  `],
})
export class AppComponent {
  readonly auth = inject(AuthService);
  title = 'WEX Corporate Payments';
}


