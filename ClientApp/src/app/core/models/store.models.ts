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
