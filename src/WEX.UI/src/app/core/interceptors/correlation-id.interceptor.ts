import { Injectable, inject } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { v4 as uuidv4 } from 'uuid';

/**
 * Adds a X-Correlation-ID header to every outgoing API request
 * so server logs can be correlated with client-side actions.
 */
@Injectable()
export class CorrelationIdInterceptor implements HttpInterceptor {
  intercept(
    req: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    const correlationId = uuidv4();
    const cloned = req.clone({
      setHeaders: { 'X-Correlation-ID': correlationId },
    });
    return next.handle(cloned);
  }
}
