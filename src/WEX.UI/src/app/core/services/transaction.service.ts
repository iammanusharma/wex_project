import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateTransactionRequest,
  ConvertedTransaction,
} from '../models/transaction.models';

@Injectable({ providedIn: 'root' })
export class TransactionService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/transactions`;

  /** POST /api/v1/transactions — store a new purchase transaction */
  create(request: CreateTransactionRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.baseUrl, request);
  }

  /** GET /api/v1/transactions/{id}?currency={code} — retrieve with conversion */
  getWithCurrency(
    transactionId: string,
    currency: string
  ): Observable<ConvertedTransaction> {
    const params = new HttpParams().set('currency', currency.toUpperCase());
    return this.http.get<ConvertedTransaction>(
      `${this.baseUrl}/${transactionId}`,
      { params }
    );
  }
}
