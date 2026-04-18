export interface CatalogProductVariant {
  id: string;
  productId: string;
  sku: string;
  name: string;
  attributeSummary: string | null;
  imageUrl: string;
  priceOverride: number | null;
  effectivePrice: number;
  isActive: boolean;
}

export interface CatalogProduct {
  id: string;
  categoryId: string;
  categoryName: string;
  sku: string;
  name: string;
  slug: string;
  description: string;
  imageUrl: string;
  basePrice: number;
  isActive: boolean;
  variants: CatalogProductVariant[];
  availableStoreId?: string | null;
  availableStoreName?: string | null;
  availableQuantity?: number | null;
}

export interface ShopperLocation {
  latitude: number;
  longitude: number;
  label: string;
  source: 'browser' | 'demo';
}

export interface ProductStoreAvailability {
  storeId: string;
  storeCode: string;
  storeName: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  latitude: number;
  longitude: number;
  availableQuantity: number;
  canFulfill: boolean;
  distanceKilometers: number | null;
}

export interface NearestStore {
  id: string;
  code: string;
  name: string;
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  latitude: number;
  longitude: number;
  isActive: boolean;
  distanceKilometers: number;
}

export interface GuestCartItem {
  lineId: string;
  productId: string;
  productSlug: string;
  productName: string;
  categoryName: string;
  imageUrl: string;
  variantId: string | null;
  variantName: string | null;
  variantSummary: string | null;
  unitPrice: number;
  quantity: number;
}

export function getDefaultVariant(product: CatalogProduct): CatalogProductVariant | null {
  return product.variants[0] ?? null;
}

export function getSelectedPrice(product: CatalogProduct, variant: CatalogProductVariant | null): number {
  return variant?.effectivePrice ?? product.basePrice;
}

export function getStartingPrice(product: CatalogProduct): number {
  if (product.variants.length === 0) {
    return product.basePrice;
  }

  return Math.min(...product.variants.map((variant) => variant.effectivePrice));
}

export function formatCurrency(value: number): string {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    maximumFractionDigits: 0,
  }).format(value);
}

export function formatDistance(distanceKilometers: number | null): string {
  if (distanceKilometers == null) {
    return 'Distance unavailable';
  }

  return `${distanceKilometers.toFixed(distanceKilometers >= 10 ? 0 : 1)} km away`;
}
