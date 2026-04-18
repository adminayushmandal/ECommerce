import { ChangeDetectionStrategy, Component, DestroyRef, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TooltipModule } from 'primeng/tooltip';
import { map } from 'rxjs';

import {
  CatalogProductVariant,
  ProductStoreAvailability,
  formatCurrency,
  formatDistance,
  getDefaultVariant,
  getSelectedPrice,
} from '../../../../core/models/store.models';
import { CatalogApiService } from '../../../../core/services/catalog-api.service';
import { GuestCartService } from '../../../../core/services/guest-cart.service';
import { ShopperLocationService } from '../../../../core/services/shopper-location.service';
import { StoreApiService } from '../../../../core/services/store-api.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-product-detail-page',
  standalone: true,
  imports: [RouterLink, TooltipModule],
  templateUrl: './product-detail-page.component.html',
  styleUrl: './product-detail-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductDetailPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly title = inject(Title);
  private readonly catalogApi = inject(CatalogApiService);
  private readonly guestCart = inject(GuestCartService);
  private readonly shopperLocation = inject(ShopperLocationService);
  private readonly storeApi = inject(StoreApiService);
  private readonly toast = inject(ToastService);

  private currentProductId: string | null = null;
  private readonly slug = toSignal(this.route.paramMap.pipe(map((params) => params.get('slug'))), {
    initialValue: this.route.snapshot.paramMap.get('slug'),
  });

  protected readonly formatCurrency = formatCurrency;
  protected readonly formatDistance = formatDistance;
  protected readonly loading = this.catalogApi.loading;
  protected readonly selectedVariantId = signal<string | null>(null);
  protected readonly quantity = signal(1);
  protected readonly availability = signal<ProductStoreAvailability[]>([]);
  protected readonly availabilityLoading = signal(false);
  protected readonly availabilityError = signal<string | null>(null);
  protected readonly shopperLocationLabel = computed(() => this.shopperLocation.location()?.label ?? 'No location selected');
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
  protected readonly nearestStore = computed(() => this.availability().find((store) => store.canFulfill) ?? this.availability()[0] ?? null);
  protected readonly hasShopperLocation = this.shopperLocation.hasLocation;

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

    effect(() => {
      if (this.loading() && !this.product()) {
        this.title.setTitle('Loading Product | ECommerce');
        return;
      }

      if (this.notFound()) {
        this.title.setTitle('Product Not Found | ECommerce');
        return;
      }

      const product = this.product();
      if (product) {
        this.title.setTitle(`${product.name} | ECommerce`);
      }
    });

    effect((onCleanup) => {
      const product = this.product();
      const variant = this.selectedVariant();
      const shopperLocation = this.shopperLocation.location();

      if (!product) {
        this.availability.set([]);
        this.availabilityError.set(null);
        this.availabilityLoading.set(false);
        return;
      }

      this.availabilityLoading.set(true);
      this.availabilityError.set(null);

      const subscription = this.storeApi
        .getProductAvailability(product.id, variant?.id ?? null, shopperLocation)
        .subscribe({
          next: (availability) => {
            this.availability.set(availability);
            this.availabilityLoading.set(false);
          },
          error: (error: Error) => {
            this.availability.set([]);
            this.availabilityError.set(error.message);
            this.availabilityLoading.set(false);
          },
        });

      onCleanup(() => subscription.unsubscribe());
    });

    this.destroyRef.onDestroy(() => {
      this.title.setTitle('ECommerce');
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

  protected async useBrowserLocation(): Promise<void> {
    const success = await this.shopperLocation.useBrowserLocation();
    if (!success) {
      this.toast.warn('Location unavailable', 'Allow location access in the browser to sort nearby stores.');
      return;
    }

    this.toast.success('Location updated', 'Store availability is now ordered by your current location.');
  }

  protected clearLocation(): void {
    this.shopperLocation.clear();
    this.toast.info('Location cleared', 'Store availability will be shown without distance-based ordering.');
  }
}
