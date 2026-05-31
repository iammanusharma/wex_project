import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresIn: number;
}

const TOKEN_KEY = 'wex_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly loginUrl = `${environment.apiBaseUrl}/auth/login`;

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(this.loginUrl, credentials).pipe(
      tap(response => localStorage.setItem(TOKEN_KEY, response.accessToken))
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;
    try {
      // Parse expiry from JWT payload (no extra library needed)
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }
}
