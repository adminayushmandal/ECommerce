import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';

import { NearestStore, ProductStoreAvailability, ShopperLocation } from '../models/store.models';

@Injectable({ providedIn: 'root' })
export class StoreApiService {
  private readonly http = inject(HttpClient);
  private readonly storesApiUrl = '/api/Stores';

  getNearestStore(shopperLocation: ShopperLocation): Observable<NearestStore> {
    const params = new HttpParams()
      .set('customerLatitude', shopperLocation.latitude)
      .set('customerLongitude', shopperLocation.longitude);

    return this.http
      .get<NearestStore>(`${this.storesApiUrl}/nearest`, { params })
      .pipe(catchError((error) => this.handleError(error)));
  }

  getProductAvailability(
    productId: string,
    productVariantId: string | null,
    shopperLocation: ShopperLocation | null
  ): Observable<ProductStoreAvailability[]> {
    let params = new HttpParams();

    if (productVariantId) {
      params = params.set('productVariantId', productVariantId);
    }

    if (shopperLocation) {
      params = params
        .set('customerLatitude', shopperLocation.latitude)
        .set('customerLongitude', shopperLocation.longitude);
    }

    return this.http
      .get<ProductStoreAvailability[]>(
        `${this.storesApiUrl}/availability/products/${productId}`,
        { params }
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  private handleError(error: HttpErrorResponse) {
    const fallbackMessage = 'Unable to load store availability right now.';
    const message =
      (typeof error.error === 'string' && error.error) ||
      error.error?.detail ||
      error.error?.title ||
      error.message ||
      fallbackMessage;

    return throwError(() => new Error(message));
  }
}
