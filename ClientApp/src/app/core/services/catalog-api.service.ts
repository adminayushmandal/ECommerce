import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';

import { CatalogProduct } from '../models/store.models';

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
      .map((product) => ({
        ...product,
        variants: product.variants
          .filter((variant) => variant.isActive)
          .sort((left, right) => left.name.localeCompare(right.name)),
      }))
      .sort((left, right) => left.name.localeCompare(right.name));
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
