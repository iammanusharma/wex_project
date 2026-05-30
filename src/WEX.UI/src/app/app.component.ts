import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, MatToolbarModule, MatButtonModule],
  template: `
    <mat-toolbar color="primary">
      <span>WEX Corporate Payments</span>
      <span class="spacer"></span>
      <button mat-button routerLink="/transactions">New Transaction</button>
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
  title = 'WEX Corporate Payments';
}

