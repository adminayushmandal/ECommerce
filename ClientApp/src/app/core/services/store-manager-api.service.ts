import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, throwError } from 'rxjs';

import { StoreManagerPayment } from '../models/payment.models';
import { CatalogProduct, StoreDto } from '../models/store.models';

@Injectable({ providedIn: 'root' })
export class StoreManagerApiService {
  private readonly http = inject(HttpClient);
  private readonly managerApiUrl = '/api/StoreManager';

  getProducts(storeId: string | null = null): Observable<CatalogProduct[]> {
    return this.http
      .get<CatalogProduct[]>(`${this.managerApiUrl}/products`, {
        params: this.createStoreParams(storeId),
        withCredentials: true,
      })
      .pipe(map((products) => products.slice().sort((left, right) => left.name.localeCompare(right.name))), catchError((error) => this.handleError(error)));
  }

  getStores(): Observable<StoreDto[]> {
    return this.http
      .get<StoreDto[]>(`${this.managerApiUrl}/stores`, { withCredentials: true })
      .pipe(catchError((error) => this.handleError(error)));
  }

  getPayments(storeId: string | null = null): Observable<StoreManagerPayment[]> {
    return this.http
      .get<StoreManagerPayment[]>(`${this.managerApiUrl}/payments`, {
        params: this.createStoreParams(storeId),
        withCredentials: true,
      })
      .pipe(catchError((error) => this.handleError(error)));
  }

  private createStoreParams(storeId: string | null): HttpParams {
    return storeId ? new HttpParams().set('storeId', storeId) : new HttpParams();
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    const fallbackMessage = 'Unable to load store manager data.';
    const message =
      (typeof error.error === 'string' && error.error) ||
      error.error?.detail ||
      error.error?.title ||
      error.message ||
      fallbackMessage;

    return throwError(() => new Error(message));
  }
}
