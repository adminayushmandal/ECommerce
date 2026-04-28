import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, map, tap, throwError } from 'rxjs';

import {
  CatalogProduct,
  CatalogProductRequest,
  CatalogProductVariant,
  CatalogProductVariantRequest,
} from '../models/store.models';

interface CatalogState {
  products: CatalogProduct[];
  loading: boolean;
  loaded: boolean;
  error: string | null;
}

@Injectable({ providedIn: 'root' })
export class CatalogApiService {
  private readonly http = inject(HttpClient);
  private readonly productsApiUrl = '/api/Products';

  private readonly state = signal<CatalogState>({
    products: [],
    loading: false,
    loaded: false,
    error: null,
  });

  readonly products = computed(() => this.state().products);
  readonly loading = computed(() => this.state().loading);
  readonly loaded = computed(() => this.state().loaded);
  readonly error = computed(() => this.state().error);

  constructor() {
    this.loadProducts();
  }

  fetchProducts(storeId: string | null = null): Observable<CatalogProduct[]> {
    const url = storeId
      ? `${this.productsApiUrl}?storeId=${encodeURIComponent(storeId)}`
      : this.productsApiUrl;

    return this.http
      .get<CatalogProduct[]>(url)
      .pipe(map((products) => this.normalizeProducts(products)));
  }

  getProduct(productId: string): Observable<CatalogProduct> {
    return this.http
      .get<CatalogProduct>(`${this.productsApiUrl}/${encodeURIComponent(productId)}`)
      .pipe(
        map((product) => this.normalizeProduct(product)),
        catchError((error) => this.handleError(error))
      );
  }

  createProduct(request: CatalogProductRequest): Observable<CatalogProduct> {
    return this.http
      .post<CatalogProduct>(this.productsApiUrl, request, { withCredentials: true })
      .pipe(
        map((product) => this.normalizeProduct(product)),
        tap(() => this.loadProducts(true)),
        catchError((error) => this.handleError(error))
      );
  }

  updateProduct(productId: string, request: CatalogProductRequest): Observable<CatalogProduct> {
    return this.http
      .put<CatalogProduct>(`${this.productsApiUrl}/${encodeURIComponent(productId)}`, request, { withCredentials: true })
      .pipe(
        map((product) => this.normalizeProduct(product)),
        tap((product) => this.upsertCachedProduct(product)),
        catchError((error) => this.handleError(error))
      );
  }

  deleteProduct(productId: string): Observable<void> {
    return this.http
      .delete<void>(`${this.productsApiUrl}/${encodeURIComponent(productId)}`, { withCredentials: true })
      .pipe(
        tap(() => this.removeCachedProduct(productId)),
        catchError((error) => this.handleError(error))
      );
  }

  getProductVariants(productId: string): Observable<CatalogProductVariant[]> {
    return this.http
      .get<CatalogProductVariant[]>(`${this.productsApiUrl}/${encodeURIComponent(productId)}/variants`)
      .pipe(
        map((variants) => this.normalizeVariants(variants)),
        catchError((error) => this.handleError(error))
      );
  }

  getProductVariant(productId: string, variantId: string): Observable<CatalogProductVariant> {
    return this.http
      .get<CatalogProductVariant>(
        `${this.productsApiUrl}/${encodeURIComponent(productId)}/variants/${encodeURIComponent(variantId)}`
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  createProductVariant(productId: string, request: CatalogProductVariantRequest): Observable<CatalogProductVariant> {
    return this.http
      .post<CatalogProductVariant>(`${this.productsApiUrl}/${encodeURIComponent(productId)}/variants`, request, {
        withCredentials: true,
      })
      .pipe(
        tap(() => this.loadProducts(true)),
        catchError((error) => this.handleError(error))
      );
  }

  updateProductVariant(
    productId: string,
    variantId: string,
    request: CatalogProductVariantRequest
  ): Observable<CatalogProductVariant> {
    return this.http
      .put<CatalogProductVariant>(
        `${this.productsApiUrl}/${encodeURIComponent(productId)}/variants/${encodeURIComponent(variantId)}`,
        request,
        { withCredentials: true }
      )
      .pipe(
        tap(() => this.loadProducts(true)),
        catchError((error) => this.handleError(error))
      );
  }

  deleteProductVariant(productId: string, variantId: string): Observable<void> {
    return this.http
      .delete<void>(
        `${this.productsApiUrl}/${encodeURIComponent(productId)}/variants/${encodeURIComponent(variantId)}`,
        { withCredentials: true }
      )
      .pipe(
        tap(() => this.loadProducts(true)),
        catchError((error) => this.handleError(error))
      );
  }

  loadProducts(force = false): void {
    const currentState = this.state();
    if (currentState.loading || (currentState.loaded && !force)) {
      return;
    }

    this.state.update((state) => ({
      ...state,
      loading: true,
      error: null,
    }));

    this.http.get<CatalogProduct[]>(this.productsApiUrl).subscribe({
      next: (products) => {
        this.state.set({
          products: this.normalizeProducts(products),
          loading: false,
          loaded: true,
          error: null,
        });
      },
      error: (error: unknown) => {
        this.state.set({
          products: currentState.products,
          loading: false,
          loaded: currentState.loaded,
          error: this.getErrorMessage(error),
        });
      },
    });
  }

  findBySlug(slug: string | null): CatalogProduct | undefined {
    if (!slug) {
      return undefined;
    }

    return this.products().find((product) => product.slug === slug);
  }

  private normalizeProducts(products: CatalogProduct[]): CatalogProduct[] {
    return products
      .filter((product) => product.isActive)
      .map((product) => this.normalizeProduct(product))
      .sort((left, right) => left.name.localeCompare(right.name));
  }

  private normalizeProduct(product: CatalogProduct): CatalogProduct {
    return {
      ...product,
      variants: this.normalizeVariants(product.variants),
    };
  }

  private normalizeVariants(variants: CatalogProductVariant[]): CatalogProductVariant[] {
    return variants
      .filter((variant) => variant.isActive)
      .sort((left, right) => left.name.localeCompare(right.name));
  }

  private upsertCachedProduct(product: CatalogProduct): void {
    this.state.update((state) => ({
      ...state,
      products: this.normalizeProducts([
        ...state.products.filter((candidate) => candidate.id !== product.id),
        product,
      ]),
    }));
  }

  private removeCachedProduct(productId: string): void {
    this.state.update((state) => ({
      ...state,
      products: state.products.filter((product) => product.id !== productId),
    }));
  }

  private handleError(error: unknown): Observable<never> {
    return throwError(() => new Error(this.getErrorMessage(error)));
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      return (
        (typeof error.error === 'string' && error.error) ||
        error.error?.detail ||
        error.error?.title ||
        error.message ||
        'Unable to load the product collection right now.'
      );
    }

    return 'Unable to load the product collection right now.';
  }
}
