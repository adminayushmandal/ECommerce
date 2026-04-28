import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';

import { OrderDto } from '../models/order.models';

@Injectable({ providedIn: 'root' })
export class OrderApiService {
  private readonly http = inject(HttpClient);
  private readonly ordersApiUrl = '/api/Orders';

  getMyOrders(): Observable<OrderDto[]> {
    return this.http
      .get<OrderDto[]>(this.ordersApiUrl, { withCredentials: true })
      .pipe(catchError((error) => this.handleError(error)));
  }

  getMyOrderById(orderId: string): Observable<OrderDto> {
    return this.http
      .get<OrderDto>(`${this.ordersApiUrl}/${encodeURIComponent(orderId)}`, { withCredentials: true })
      .pipe(catchError((error) => this.handleError(error)));
  }

  cancelMyOrder(orderId: string): Observable<OrderDto> {
    return this.http
      .post<OrderDto>(`${this.ordersApiUrl}/${encodeURIComponent(orderId)}/cancel`, {}, { withCredentials: true })
      .pipe(catchError((error) => this.handleError(error)));
  }

  private handleError(error: HttpErrorResponse) {
    const fallbackMessage = 'Unable to load orders. Please try again.';
    const message =
      (typeof error.error === 'string' && error.error) ||
      error.error?.detail ||
      error.error?.title ||
      error.message ||
      fallbackMessage;

    return throwError(() => new Error(message));
  }
}
