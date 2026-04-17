import { Injectable, computed, signal } from '@angular/core';

import {
  CatalogProduct,
  CatalogProductVariant,
  GuestCartItem,
  getDefaultVariant,
  getSelectedPrice,
} from '../models/store.models';

const storageKey = 'ecommerce.guest-cart';

@Injectable({ providedIn: 'root' })
export class GuestCartService {
  private readonly itemsState = signal<GuestCartItem[]>(this.restoreItems());

  readonly items = computed(() => this.itemsState());
  readonly itemCount = computed(() => this.items().reduce((count, item) => count + item.quantity, 0));
  readonly subtotal = computed(() => this.items().reduce((total, item) => total + (item.unitPrice * item.quantity), 0));

  addItem(product: CatalogProduct, variant: CatalogProductVariant | null = getDefaultVariant(product), quantity = 1): void {
    const sanitizedQuantity = Math.max(1, Math.trunc(quantity));
    const lineId = this.getLineId(product.id, variant?.id ?? null);

    this.itemsState.update((items) => {
      const existingItem = items.find((item) => item.lineId === lineId);
      if (existingItem) {
        return items.map((item) =>
          item.lineId === lineId
            ? { ...item, quantity: item.quantity + sanitizedQuantity }
            : item
        );
      }

      return [
        ...items,
        {
          lineId,
          productId: product.id,
          productSlug: product.slug,
          productName: product.name,
          categoryName: product.categoryName,
          imageUrl: variant?.imageUrl || product.imageUrl,
          variantId: variant?.id ?? null,
          variantName: variant?.name ?? null,
          variantSummary: variant?.attributeSummary ?? null,
          unitPrice: getSelectedPrice(product, variant),
          quantity: sanitizedQuantity,
        },
      ];
    });

    this.persist();
  }

  updateQuantity(lineId: string, quantity: number): void {
    const sanitizedQuantity = Math.trunc(quantity);

    if (sanitizedQuantity <= 0) {
      this.removeItem(lineId);
      return;
    }

    this.itemsState.update((items) =>
      items.map((item) =>
        item.lineId === lineId
          ? { ...item, quantity: sanitizedQuantity }
          : item
      )
    );

    this.persist();
  }

  removeItem(lineId: string): void {
    this.itemsState.update((items) => items.filter((item) => item.lineId !== lineId));
    this.persist();
  }

  clear(): void {
    this.itemsState.set([]);
    this.persist();
  }

  private getLineId(productId: string, variantId: string | null): string {
    return `${productId}::${variantId ?? 'base'}`;
  }

  private restoreItems(): GuestCartItem[] {
    if (typeof window === 'undefined' || !window.localStorage) {
      return [];
    }

    try {
      const rawValue = window.localStorage.getItem(storageKey);
      if (!rawValue) {
        return [];
      }

      const parsed = JSON.parse(rawValue);
      if (!Array.isArray(parsed)) {
        return [];
      }

      return parsed.filter((value): value is GuestCartItem => this.isGuestCartItem(value));
    } catch {
      return [];
    }
  }

  private persist(): void {
    if (typeof window === 'undefined' || !window.localStorage) {
      return;
    }

    window.localStorage.setItem(storageKey, JSON.stringify(this.itemsState()));
  }

  private isGuestCartItem(value: unknown): value is GuestCartItem {
    if (!value || typeof value !== 'object') {
      return false;
    }

    const candidate = value as Partial<GuestCartItem>;

    return typeof candidate.lineId === 'string' &&
      typeof candidate.productId === 'string' &&
      typeof candidate.productSlug === 'string' &&
      typeof candidate.productName === 'string' &&
      typeof candidate.categoryName === 'string' &&
      typeof candidate.imageUrl === 'string' &&
      typeof candidate.unitPrice === 'number' &&
      typeof candidate.quantity === 'number';
  }
}
