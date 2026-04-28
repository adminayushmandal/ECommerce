import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';

import { AddCartItemRequest, CartDto, CheckoutCartRequest, UpdateCartItemQuantityRequest } from '../models/cart.models';
import { OrderDto } from '../models/order.models';

@Injectable({ providedIn: 'root' })
export class CartApiService {
  private readonly http = inject(HttpClient);
  private readonly cartApiUrl = '/api/Cart';

  getCart(): Observable<CartDto> {
    return this.http
      .get<CartDto>(this.cartApiUrl, { withCredentials: true })
      .pipe(catchError((error) => this.handleError(error)));
  }

  addItem(request: AddCartItemRequest): Observable<CartDto> {
    return this.http
      .post<CartDto>(`${this.cartApiUrl}/items`, request, { withCredentials: true })
      .pipe(catchError((error) => this.handleError(error)));
  }

  updateItemQuantity(orderItemId: string, request: UpdateCartItemQuantityRequest): Observable<CartDto> {
    return this.http
      .put<CartDto>(`${this.cartApiUrl}/items/${encodeURIComponent(orderItemId)}`, request, { withCredentials: true })
      .pipe(catchError((error) => this.handleError(error)));
  }

  removeItem(orderItemId: string): Observable<CartDto> {
    return this.http
      .delete<CartDto>(`${this.cartApiUrl}/items/${encodeURIComponent(orderItemId)}`, { withCredentials: true })
      .pipe(catchError((error) => this.handleError(error)));
  }

  checkout(request: CheckoutCartRequest): Observable<OrderDto> {
    return this.http
      .post<OrderDto>(`${this.cartApiUrl}/checkout`, request, { withCredentials: true })
      .pipe(catchError((error) => this.handleError(error)));
  }

  private handleError(error: HttpErrorResponse) {
    const fallbackMessage = 'Unable to update the cart. Please try again.';
    const message =
      (typeof error.error === 'string' && error.error) ||
      error.error?.detail ||
      error.error?.title ||
      error.message ||
      fallbackMessage;

    return throwError(() => new Error(message));
  }
}
