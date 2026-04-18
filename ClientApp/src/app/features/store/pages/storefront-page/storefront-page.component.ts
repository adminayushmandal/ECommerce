import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { switchMap } from 'rxjs';

import {
  CatalogProduct,
  NearestStore,
  formatCurrency,
  formatDistance,
  getDefaultVariant,
  getStartingPrice,
} from '../../../../core/models/store.models';
import { CatalogApiService } from '../../../../core/services/catalog-api.service';
import { GuestCartService } from '../../../../core/services/guest-cart.service';
import { ShopperLocationService } from '../../../../core/services/shopper-location.service';
import { StoreApiService } from '../../../../core/services/store-api.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-storefront-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './storefront-page.component.html',
  styleUrl: './storefront-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontPageComponent {
  private readonly router = inject(Router);
  private readonly catalogApi = inject(CatalogApiService);
  private readonly storeApi = inject(StoreApiService);
  private readonly shopperLocation = inject(ShopperLocationService);
  private readonly guestCart = inject(GuestCartService);
  private readonly toast = inject(ToastService);

  private readonly refreshNonce = signal(0);

  protected readonly formatCurrency = formatCurrency;
  protected readonly formatDistance = formatDistance;
  protected readonly searchTerm = signal('');
  protected readonly selectedCategory = signal('All');
  protected readonly locationPromptOpen = signal(!this.shopperLocation.hasLocation());
  protected readonly browseAllMode = signal(!this.shopperLocation.hasLocation());
  protected readonly catalogProducts = signal<CatalogProduct[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly resolvedStore = signal<NearestStore | null>(null);
  protected readonly shopperLocationState = this.shopperLocation.location;
  protected readonly hasShopperLocation = this.shopperLocation.hasLocation;
  protected readonly products = computed(() => this.catalogProducts());
  protected readonly categories = computed(() => {
    const allCategories = Array.from(new Set(this.products().map((product) => product.categoryName))).sort((left, right) =>
      left.localeCompare(right)
    );

    return ['All', ...allCategories];
  });
  protected readonly filteredProducts = computed(() => {
    const selectedCategory = this.selectedCategory();
    const normalizedSearch = this.searchTerm().trim().toLowerCase();

    return this.products().filter((product) => {
      const matchesCategory = selectedCategory === 'All' || product.categoryName === selectedCategory;
      const matchesSearch =
        normalizedSearch.length === 0 ||
        product.name.toLowerCase().includes(normalizedSearch) ||
        product.description.toLowerCase().includes(normalizedSearch) ||
        product.categoryName.toLowerCase().includes(normalizedSearch) ||
        product.variants.some(
          (variant) =>
            variant.name.toLowerCase().includes(normalizedSearch) ||
            variant.attributeSummary?.toLowerCase().includes(normalizedSearch)
        );

      return matchesCategory && matchesSearch;
    });
  });
  protected readonly curatedCollections = computed(() => [
    {
      title: 'Nearby store catalog',
      description: this.resolvedStore()
        ? `${this.resolvedStore()!.name} is your active fulfillment store.`
        : 'Set a location to resolve the nearest store and see its available catalog.',
      value: this.resolvedStore()
        ? `${formatDistance(this.resolvedStore()!.distanceKilometers)}`
        : 'Location needed',
    },
    {
      title: 'Guest-ready shopping',
      description: 'Anonymous visitors can browse, search, and keep a cart before moving into secure checkout.',
      value: `${this.guestCart.itemCount()} item${this.guestCart.itemCount() === 1 ? '' : 's'} saved`,
    },
    {
      title: 'Store-scoped inventory',
      description: 'Product cards can now reflect the selected store instead of a generic all-products catalog.',
      value: `${this.products().length} local items`,
    },
  ]);

  constructor() {
    effect((onCleanup) => {
      this.refreshNonce();

      const shopperLocation = this.shopperLocationState();
      const browseAllMode = this.browseAllMode();

      if (!shopperLocation && !browseAllMode) {
        this.catalogProducts.set([]);
        this.resolvedStore.set(null);
        this.error.set(null);
        this.loading.set(false);
        this.locationPromptOpen.set(true);
        return;
      }

      this.loading.set(true);
      this.error.set(null);

      const catalogRequest = shopperLocation && !browseAllMode
        ? this.storeApi.getNearestStore(shopperLocation).pipe(
            switchMap((store) => {
              this.resolvedStore.set(store);
              return this.catalogApi.fetchProducts(store.id);
            })
          )
        : this.catalogApi.fetchProducts();

      if (!shopperLocation || browseAllMode) {
        this.resolvedStore.set(null);
      }

      const subscription = catalogRequest.subscribe({
        next: (products) => {
          this.catalogProducts.set(products);
          this.loading.set(false);
        },
        error: (error: Error) => {
          this.catalogProducts.set([]);
          this.error.set(error.message);
          this.loading.set(false);
        },
      });

      onCleanup(() => subscription.unsubscribe());
    });
  }

  protected onSearchInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchTerm.set(target.value);
  }

  protected setCategory(category: string): void {
    this.selectedCategory.set(category);
  }

  protected refreshProducts(): void {
    this.refreshNonce.update((value) => value + 1);
    this.toast.info(
      'Refreshing catalog',
      this.resolvedStore()
        ? `Fetching the latest products for ${this.resolvedStore()!.name}.`
        : 'Fetching the latest product collection.'
    );
  }

  protected addToCart(product: CatalogProduct): void {
    this.guestCart.addItem(product, getDefaultVariant(product));
    this.toast.success('Added to cart', `${product.name} is now in your cart.`);
  }

  protected async buyNow(product: CatalogProduct): Promise<void> {
    this.guestCart.addItem(product, getDefaultVariant(product));
    this.toast.info('Checkout started', `${product.name} was added to your cart. Sign in to continue checkout.`);
    await this.router.navigate(['/identity/login'], {
      queryParams: {
        intent: 'checkout',
        redirectTo: `/products/${product.slug}`,
      },
    });
  }

  protected getStartingPrice(product: CatalogProduct): string {
    return formatCurrency(getStartingPrice(product));
  }

  protected getPrimaryVariantLabel(product: CatalogProduct): string {
    const defaultVariant = getDefaultVariant(product);
    return defaultVariant?.name ?? 'Standard option';
  }

  protected getLocalAvailability(product: CatalogProduct): string {
    if (product.availableQuantity == null) {
      return 'Availability not scoped';
    }

    return `${product.availableQuantity} unit${product.availableQuantity === 1 ? '' : 's'} nearby`;
  }

  protected async useBrowserLocation(): Promise<void> {
    const success = await this.shopperLocation.useBrowserLocation();
    if (!success) {
      this.toast.warn('Location unavailable', 'Allow location access in the browser, or continue without location.');
      return;
    }

    this.browseAllMode.set(false);
    this.locationPromptOpen.set(false);
    this.toast.success('Location updated', 'Loading the nearest store catalog for your current location.');
  }

  protected browseAllProducts(): void {
    this.browseAllMode.set(true);
    this.locationPromptOpen.set(false);
    this.toast.info('Browsing all products', 'Showing the full catalog without store-specific filtering.');
  }

  protected askForLocationAgain(): void {
    this.browseAllMode.set(false);
    this.locationPromptOpen.set(true);
  }
}
