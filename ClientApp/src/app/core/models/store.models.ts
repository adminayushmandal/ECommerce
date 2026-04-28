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

export interface CatalogProductVariantRequest {
  sku: string;
  name: string;
  attributeSummary: string | null;
  imageUrl: string;
  priceOverride: number | null;
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

export interface CatalogProductRequest {
  categoryId: string;
  sku: string;
  name: string;
  slug: string;
  description: string;
  basePrice: number;
  imageUrl: string;
  isActive: boolean;
}

export interface StoreDto {
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
}

export interface ShopperLocation {
  latitude: number;
  longitude: number;
  label: string;
  source: 'browser' | 'demo';
}

export type DemoLocationKey = 'ludhiana' | 'dibrugarh' | 'shillong';

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
    currency: 'USD',
    maximumFractionDigits: 2,
  }).format(value);
}

export function formatDistance(distanceKilometers: number | null): string {
  if (distanceKilometers == null) {
    return 'Distance unavailable';
  }

  return `${distanceKilometers.toFixed(distanceKilometers >= 10 ? 0 : 1)} km away`;
}

export function canRenderImageUrl(value: string | null | undefined): boolean {
  if (!value) {
    return false;
  }

  return /^(https?:\/\/|\/|\.\/|\.\.\/|assets\/|data:image\/)/i.test(value.trim());
}

export function normalizeHexColor(value: string | null | undefined, fallback = '111827'): string {
  const rawValue = value?.trim() ?? '';
  const dummyImageColor = rawValue.match(/\/\d+x\d+\/([0-9a-f]{6})(?:\/|\?)/i)?.[1];
  const normalized = dummyImageColor ?? rawValue.replace(/^#/, '');
  return /^[0-9a-f]{6}$/i.test(normalized) ? normalized.toUpperCase() : fallback.toUpperCase();
}

export function getSwatchColor(value: string | null | undefined, fallback = '111827'): string {
  return `#${normalizeHexColor(value, fallback)}`;
}

export function getProductVisualBackground(value: string | null | undefined, fallback = '111827'): string {
  const color = getSwatchColor(value, fallback);

  return [
    'radial-gradient(circle at 24% 18%, rgba(255,255,255,0.82) 0 8%, transparent 9% 100%)',
    'radial-gradient(circle at 78% 26%, rgba(255,255,255,0.34) 0 14%, transparent 15% 100%)',
    `linear-gradient(135deg, ${color} 0%, #111827 118%)`,
  ].join(', ');
}
