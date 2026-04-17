import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { map } from 'rxjs';

import { CatalogProductVariant, formatCurrency, getDefaultVariant, getSelectedPrice } from '../../../../core/models/store.models';
import { CatalogApiService } from '../../../../core/services/catalog-api.service';
import { GuestCartService } from '../../../../core/services/guest-cart.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-product-detail-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './product-detail-page.component.html',
  styleUrl: './product-detail-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductDetailPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly catalogApi = inject(CatalogApiService);
  private readonly guestCart = inject(GuestCartService);
  private readonly toast = inject(ToastService);

  private currentProductId: string | null = null;
  private readonly slug = toSignal(this.route.paramMap.pipe(map((params) => params.get('slug'))), {
    initialValue: this.route.snapshot.paramMap.get('slug'),
  });

  protected readonly formatCurrency = formatCurrency;
  protected readonly loading = this.catalogApi.loading;
  protected readonly selectedVariantId = signal<string | null>(null);
  protected readonly quantity = signal(1);
  protected readonly product = computed(() => this.catalogApi.findBySlug(this.slug()));
  protected readonly notFound = computed(() => this.catalogApi.loaded() && !this.product());
  protected readonly selectedVariant = computed<CatalogProductVariant | null>(() => {
    const product = this.product();
    const selectedVariantId = this.selectedVariantId();

    if (!product) {
      return null;
    }

    return product.variants.find((variant) => variant.id === selectedVariantId) ?? getDefaultVariant(product);
  });
  protected readonly displayPrice = computed(() => {
    const product = this.product();
    if (!product) {
      return 0;
    }

    return getSelectedPrice(product, this.selectedVariant());
  });
  protected readonly relatedProducts = computed(() => {
    const product = this.product();
    if (!product) {
      return [];
    }

    return this.catalogApi.products()
      .filter((candidate) => candidate.categoryId === product.categoryId && candidate.id !== product.id)
      .slice(0, 3);
  });

  constructor() {
    effect(() => {
      const product = this.product();
      if (!product || product.id === this.currentProductId) {
        return;
      }

      this.currentProductId = product.id;
      this.selectedVariantId.set(getDefaultVariant(product)?.id ?? null);
      this.quantity.set(1);
    });
  }

  protected selectVariant(variantId: string): void {
    this.selectedVariantId.set(variantId);
  }

  protected increaseQuantity(): void {
    this.quantity.update((quantity) => quantity + 1);
  }

  protected decreaseQuantity(): void {
    this.quantity.update((quantity) => Math.max(1, quantity - 1));
  }

  protected addToCart(): void {
    const product = this.product();
    if (!product) {
      return;
    }

    this.guestCart.addItem(product, this.selectedVariant(), this.quantity());
    this.toast.success(
      'Added to cart',
      `${this.quantity()} x ${product.name}${this.selectedVariant()?.name ? ` (${this.selectedVariant()?.name})` : ''} added successfully.`
    );
  }

  protected async buyNow(): Promise<void> {
    const product = this.product();
    if (!product) {
      return;
    }

    this.guestCart.addItem(product, this.selectedVariant(), this.quantity());
    this.toast.info(
      'Checkout started',
      `${product.name}${this.selectedVariant()?.name ? ` (${this.selectedVariant()?.name})` : ''} was added to your cart. Sign in to continue checkout.`
    );
    await this.router.navigate(['/identity/login'], {
      queryParams: {
        intent: 'checkout',
        redirectTo: `/products/${product.slug}`,
      },
    });
  }
}
