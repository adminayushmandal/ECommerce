import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';

import {
  PayPalCaptureResponse,
  PayPalClientConfig,
  PayPalCreateOrderRequest,
  PayPalCreateOrderResponse,
} from '../models/payment.models';

@Injectable({ providedIn: 'root' })
export class PaymentApiService {
  private readonly http = inject(HttpClient);
  private readonly payPalApiUrl = '/api/Payments';

  getPayPalConfig(): Observable<PayPalClientConfig> {
    return this.http
      .get<PayPalClientConfig>(`${this.payPalApiUrl}/config`, { withCredentials: true })
      .pipe(catchError((error) => this.handleError(error)));
  }

  createPayPalOrder(request: PayPalCreateOrderRequest): Observable<PayPalCreateOrderResponse> {
    return this.http
      .post<PayPalCreateOrderResponse>(`${this.payPalApiUrl}/orders`, request, { withCredentials: true })
      .pipe(catchError((error) => this.handleError(error)));
  }

  capturePayPalOrder(payPalOrderId: string): Observable<PayPalCaptureResponse> {
    return this.http
      .post<PayPalCaptureResponse>(
        `${this.payPalApiUrl}/orders/${encodeURIComponent(payPalOrderId)}/capture`,
        {},
        { withCredentials: true }
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  cancelPayPalOrder(payPalOrderId: string): Observable<PayPalCaptureResponse> {
    return this.http
      .post<PayPalCaptureResponse>(
        `${this.payPalApiUrl}/orders/${encodeURIComponent(payPalOrderId)}/cancel`,
        {},
        { withCredentials: true }
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  private handleError(error: HttpErrorResponse) {
    const fallbackMessage = 'Unable to process the payment. Please try again.';
    const message =
      (typeof error.error === 'string' && error.error) ||
      error.error?.detail ||
      error.error?.title ||
      error.message ||
      fallbackMessage;

    return throwError(() => new Error(message));
  }
}
