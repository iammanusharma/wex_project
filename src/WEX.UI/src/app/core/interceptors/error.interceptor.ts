import { Injectable, inject } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiError } from '../models/transaction.models';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  private readonly snackBar = inject(MatSnackBar);

  intercept(
    req: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        const apiError = error.error as ApiError;
        const message = this.buildMessage(error.status, apiError);
        this.snackBar.open(message, 'Dismiss', {
          duration: 6000,
          panelClass: ['error-snackbar'],
        });
        return throwError(() => error);
      })
    );
  }

  private buildMessage(status: number, error: ApiError): string {
    if (status === 400 && error?.errors) {
      const fieldErrors = Object.values(error.errors).flat().join(' ');
      return `Validation error: ${fieldErrors}`;
    }
    if (status === 404) {
      return error?.detail ?? 'Transaction not found.';
    }
    if (status === 422) {
      return error?.detail ?? 'The purchase cannot be converted to the target currency.';
    }
    return error?.title ?? 'An unexpected error occurred. Please try again.';
  }
}
