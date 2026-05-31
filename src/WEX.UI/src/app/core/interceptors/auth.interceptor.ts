import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { AuthService } from '../services/auth.service';

/**
 * Attaches the JWT Bearer token to every outgoing HTTP request.
 * Skips the login endpoint itself to avoid circular dependency.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  if (token && !req.url.includes('/auth/login')) {
    const authReq = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    });
    return next(authReq);
  }

  return next(req);
};
