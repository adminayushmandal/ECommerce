import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, signal, type WritableSignal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { switchMap } from 'rxjs';

import {
  canRenderImageUrl,
  CatalogProduct,
  CatalogProductVariant,
  DemoLocationKey,
  getProductVisualBackground,
  getSwatchColor,
  NearestStore,
  StoreDto,
  formatCurrency,
  formatDistance,
  getDefaultVariant,
  getStartingPrice,
} from '../../../../core/models/store.models';
import { CatalogApiService } from '../../../../core/services/catalog-api.service';
import { CheckoutFlowService } from '../../../../core/services/checkout-flow.service';
import { GuestCartService } from '../../../../core/services/guest-cart.service';
import { ShopperLocationService } from '../../../../core/services/shopper-location.service';
import { StoreApiService } from '../../../../core/services/store-api.service';
import { ToastService } from '../../../../core/services/toast.service';

type AvailabilityMode = 'all' | 'in-stock' | 'out-of-stock';

interface PriceRangeFilter {
  label: string;
  minInclusive: number | null;
  maxExclusive: number | null;
}

@Component({
  selector: 'app-storefront-page',
  standalone: true,
  imports: [
    NgTemplateOutlet,
    RouterLink,
    BadgeModule,
    ButtonModule,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
    SkeletonModule,
    TagModule,
    TooltipModule,
  ],
  templateUrl: './storefront-page.component.html',
  styleUrl: './storefront-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontPageComponent {
  private readonly catalogApi = inject(CatalogApiService);
  private readonly checkoutFlow = inject(CheckoutFlowService);
  private readonly storeApi = inject(StoreApiService);
  private readonly shopperLocation = inject(ShopperLocationService);
  private readonly guestCart = inject(GuestCartService);
  private readonly toast = inject(ToastService);

  private readonly refreshNonce = signal(0);
  private readonly sizeSortOrder = new Map(
    ['XS', 'S', 'M', 'L', 'XL', '2X', '3X', '4Y', '5Y', '6Y', '7Y', '8Y', '9Y', '26', '28', '30', '32', '34', '36'].map(
      (value, index) => [value, index]
    )
  );
  private readonly compactPriceRangeFilters: PriceRangeFilter[] = [
    { label: 'All', minInclusive: null, maxExclusive: null },
    { label: 'Under $5', minInclusive: null, maxExclusive: 5 },
    { label: '$5 - $8', minInclusive: 5, maxExclusive: 8 },
    { label: '$8+', minInclusive: 8, maxExclusive: null },
  ];
  private readonly standardPriceRangeFilters: PriceRangeFilter[] = [
    { label: 'All', minInclusive: null, maxExclusive: null },
    { label: 'Under $50', minInclusive: null, maxExclusive: 50 },
    { label: '$50 - $90', minInclusive: 50, maxExclusive: 90 },
    { label: '$90+', minInclusive: 90, maxExclusive: null },
  ];

  protected readonly formatCurrency = formatCurrency;
  protected readonly formatDistance = formatDistance;
  protected readonly canRenderImageUrl = canRenderImageUrl;
  protected readonly searchTerm = signal('');
  protected readonly selectedCategory = signal('All');
  protected readonly selectedSizes = signal<ReadonlySet<string>>(new Set<string>());
  protected readonly selectedColors = signal<ReadonlySet<string>>(new Set<string>());
  protected readonly selectedFits = signal<ReadonlySet<string>>(new Set<string>());
  protected readonly selectedFabrics = signal<ReadonlySet<string>>(new Set<string>());
  protected readonly selectedPriceRange = signal('All');
  protected readonly availabilityMode = signal<AvailabilityMode>('all');
  protected readonly mobileFiltersOpen = signal(false);
  protected readonly locationPromptOpen = signal(!this.shopperLocation.hasLocation());
  protected readonly browseAllMode = signal(!this.shopperLocation.hasLocation());
  protected readonly catalogProducts = signal<CatalogProduct[]>([]);
  protected readonly storeLocations = signal<StoreDto[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly resolvedStore = signal<NearestStore | null>(null);
  protected readonly shopperLocationState = this.shopperLocation.location;
  protected readonly hasShopperLocation = this.shopperLocation.hasLocation;
  protected readonly products = computed(() => this.catalogProducts());
  protected readonly storeNetworkLabel = computed(() => {
    const stores = this.storeLocations();
    if (stores.length === 0) {
      return 'Ludhiana, Dibrugarh, Shillong';
    }

    return stores.map((store) => store.city).join(', ');
  });
  protected readonly categories = computed(() => {
    const allCategories = Array.from(new Set(this.products().map((product) => product.categoryName))).sort((left, right) =>
      left.localeCompare(right)
    );

    return ['All', ...allCategories];
  });
  protected readonly sizeOptions = computed(() =>
    this.getUniqueVariantValues((variant) => this.getVariantSizeValue(variant)).sort((left, right) =>
      this.compareSizes(left, right)
    )
  );
  protected readonly colorOptions = computed(() => this.getUniqueVariantValues((variant) => this.getVariantAttribute(variant, 'Color')));
  protected readonly fitOptions = computed(() => this.getUniqueVariantValues((variant) => this.getVariantAttribute(variant, 'Fit')));
  protected readonly fabricOptions = computed(() =>
    this.getUniqueVariantValues((variant) => this.getVariantAttribute(variant, 'Fabric'))
  );
  protected readonly priceRanges = computed(() => {
    const maxPrice = Math.max(0, ...this.products().map((product) => getStartingPrice(product)));
    return maxPrice <= 20 ? this.compactPriceRangeFilters : this.standardPriceRangeFilters;
  });
  protected readonly availableProductCount = computed(() => this.products().filter((product) => this.isProductInStock(product)).length);
  protected readonly outOfStockProductCount = computed(() => this.products().filter((product) => this.isProductOutOfStock(product)).length);
  protected readonly activeFilterCount = computed(() => {
    let count = 0;

    if (this.selectedCategory() !== 'All') {
      count += 1;
    }

    if (this.availabilityMode() !== 'all') {
      count += 1;
    }

    if (this.selectedPriceRange() !== 'All') {
      count += 1;
    }

    return count + this.selectedSizes().size + this.selectedColors().size + this.selectedFits().size + this.selectedFabrics().size;
  });
  protected readonly hasActiveFilters = computed(() => this.activeFilterCount() > 0);
  protected readonly filteredProducts = computed(() => {
    const selectedCategory = this.selectedCategory();
    const normalizedSearch = this.searchTerm().trim().toLowerCase();
    const selectedSizes = this.selectedSizes();
    const selectedColors = this.selectedColors();
    const selectedFits = this.selectedFits();
    const selectedFabrics = this.selectedFabrics();
    const selectedPriceRange = this.selectedPriceRange();
    const availabilityMode = this.availabilityMode();

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
      const matchesAvailability = this.matchesAvailability(product, availabilityMode);
      const matchesPrice = this.matchesPriceRange(product, selectedPriceRange);
      const matchesVariantFilters = this.matchesVariantFilters(
        product,
        selectedSizes,
        selectedColors,
        selectedFits,
        selectedFabrics
      );

      return matchesCategory && matchesSearch && matchesAvailability && matchesPrice && matchesVariantFilters;
    });
  });
  protected readonly featuredProducts = computed(() => this.products().slice(0, 3));
  protected readonly curatedCollections = computed(() => [
    {
      title: 'Local edit',
      description: this.resolvedStore()
        ? `${this.resolvedStore()!.name} is curating this drop for your checkout.`
        : 'Set a location to resolve the nearest store and unlock a sharper local edit.',
      value: this.resolvedStore()
        ? `${formatDistance(this.resolvedStore()!.distanceKilometers)}`
        : 'Location needed',
    },
    {
      title: 'Saved bag',
      description: 'Keep pieces in a guest bag, then sign in only when checkout is close.',
      value: `${this.guestCart.itemCount()} item${this.guestCart.itemCount() === 1 ? '' : 's'} saved`,
    },
    {
      title: 'Capsule catalog',
      description: 'Browse by collection, color story, size note, and product detail without losing speed.',
      value: `${this.products().length} pieces`,
    },
  ]);

  constructor() {
    this.storeApi.getStores().subscribe({
      next: (stores) => this.storeLocations.set(stores),
      error: () => this.storeLocations.set([]),
    });

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

  protected toggleSize(size: string): void {
    this.toggleSetValue(this.selectedSizes, size);
  }

  protected toggleColor(color: string): void {
    this.toggleSetValue(this.selectedColors, color);
  }

  protected toggleFit(fit: string): void {
    this.toggleSetValue(this.selectedFits, fit);
  }

  protected toggleFabric(fabric: string): void {
    this.toggleSetValue(this.selectedFabrics, fabric);
  }

  protected setPriceRange(range: string): void {
    this.selectedPriceRange.set(range);
  }

  protected setAvailabilityMode(mode: Exclude<AvailabilityMode, 'all'>, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.availabilityMode.set(checked ? mode : 'all');
  }

  protected clearFilters(): void {
    this.selectedCategory.set('All');
    this.selectedSizes.set(new Set<string>());
    this.selectedColors.set(new Set<string>());
    this.selectedFits.set(new Set<string>());
    this.selectedFabrics.set(new Set<string>());
    this.selectedPriceRange.set('All');
    this.availabilityMode.set('all');
  }

  protected toggleMobileFilters(): void {
    this.mobileFiltersOpen.update((isOpen) => !isOpen);
  }

  protected closeMobileFilters(): void {
    this.mobileFiltersOpen.set(false);
  }

  protected isSizeSelected(size: string): boolean {
    return this.selectedSizes().has(size);
  }

  protected isColorSelected(color: string): boolean {
    return this.selectedColors().has(color);
  }

  protected isFitSelected(fit: string): boolean {
    return this.selectedFits().has(fit);
  }

  protected isFabricSelected(fabric: string): boolean {
    return this.selectedFabrics().has(fabric);
  }

  protected getFilterColor(color: string): string {
    const matchingVariant = this.products()
      .flatMap((product) => product.variants)
      .find((variant) => this.getVariantAttribute(variant, 'Color') === color);

    return getSwatchColor(matchingVariant?.imageUrl, 'd9d8d2');
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

    this.toast.info('Checkout started', `${product.name} was added to your bag.`);
    await this.checkoutFlow.startPayPalCheckout({
      redirectTo: `/products/${product.slug}`,
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

  protected getProductVisualBackground(product: CatalogProduct): string {
    return getProductVisualBackground(product.imageUrl);
  }

  protected getVariantSwatchColor(product: CatalogProduct): string {
    return getSwatchColor(getDefaultVariant(product)?.imageUrl ?? product.imageUrl);
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

  protected useDemoLocation(location: DemoLocationKey): void {
    this.shopperLocation.useDemoLocation(location);
    this.browseAllMode.set(false);
    this.locationPromptOpen.set(false);
    this.toast.info('Demo location active', `Loading stock near ${this.shopperLocation.location()?.label ?? 'the selected store'}.`);
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

  private toggleSetValue(filter: WritableSignal<ReadonlySet<string>>, value: string): void {
    filter.update((currentValues) => {
      const nextValues = new Set(currentValues);

      if (nextValues.has(value)) {
        nextValues.delete(value);
      } else {
        nextValues.add(value);
      }

      return nextValues;
    });
  }

  private getUniqueVariantValues(resolveValue: (variant: CatalogProductVariant) => string | null): string[] {
    const values = this.products().flatMap((product) =>
      product.variants
        .map((variant) => resolveValue(variant))
        .filter((value): value is string => value != null && value.trim().length > 0)
    );

    return Array.from(new Set(values)).sort((left, right) => left.localeCompare(right, undefined, { numeric: true }));
  }

  private getVariantSizeValue(variant: CatalogProductVariant): string | null {
    return this.getVariantAttribute(variant, 'Size') ?? this.getVariantAttribute(variant, 'Waist');
  }

  private getVariantAttribute(variant: CatalogProductVariant, attributeName: string): string | null {
    const normalizedAttributeName = attributeName.toLowerCase();
    const summary = variant.attributeSummary ?? '';
    const matchingPart = summary
      .split(',')
      .map((part) => part.trim())
      .find((part) => part.toLowerCase().startsWith(`${normalizedAttributeName}:`));

    if (!matchingPart) {
      return null;
    }

    const separatorIndex = matchingPart.indexOf(':');
    const value = matchingPart.slice(separatorIndex + 1).trim();
    return value.length > 0 ? value : null;
  }

  private compareSizes(left: string, right: string): number {
    const leftOrder = this.sizeSortOrder.get(left) ?? Number.MAX_SAFE_INTEGER;
    const rightOrder = this.sizeSortOrder.get(right) ?? Number.MAX_SAFE_INTEGER;

    return leftOrder - rightOrder || left.localeCompare(right, undefined, { numeric: true });
  }

  private matchesAvailability(product: CatalogProduct, availabilityMode: AvailabilityMode): boolean {
    if (availabilityMode === 'all') {
      return true;
    }

    return availabilityMode === 'in-stock' ? this.isProductInStock(product) : this.isProductOutOfStock(product);
  }

  private isProductInStock(product: CatalogProduct): boolean {
    return product.availableQuantity == null || product.availableQuantity > 0;
  }

  private isProductOutOfStock(product: CatalogProduct): boolean {
    return product.availableQuantity != null && product.availableQuantity <= 0;
  }

  private matchesPriceRange(product: CatalogProduct, selectedPriceRange: string): boolean {
    const priceRange = this.priceRanges().find((range) => range.label === selectedPriceRange);

    if (!priceRange || priceRange.label === 'All') {
      return true;
    }

    const price = getStartingPrice(product);
    const aboveMinimum = priceRange.minInclusive == null || price >= priceRange.minInclusive;
    const belowMaximum = priceRange.maxExclusive == null || price < priceRange.maxExclusive;

    return aboveMinimum && belowMaximum;
  }

  private matchesVariantFilters(
    product: CatalogProduct,
    selectedSizes: ReadonlySet<string>,
    selectedColors: ReadonlySet<string>,
    selectedFits: ReadonlySet<string>,
    selectedFabrics: ReadonlySet<string>
  ): boolean {
    const hasVariantFilters =
      selectedSizes.size > 0 || selectedColors.size > 0 || selectedFits.size > 0 || selectedFabrics.size > 0;

    if (!hasVariantFilters) {
      return true;
    }

    return product.variants.some(
      (variant) =>
        this.matchesSelectedValue(selectedSizes, this.getVariantSizeValue(variant)) &&
        this.matchesSelectedValue(selectedColors, this.getVariantAttribute(variant, 'Color')) &&
        this.matchesSelectedValue(selectedFits, this.getVariantAttribute(variant, 'Fit')) &&
        this.matchesSelectedValue(selectedFabrics, this.getVariantAttribute(variant, 'Fabric'))
    );
  }

  private matchesSelectedValue(selectedValues: ReadonlySet<string>, value: string | null): boolean {
    return selectedValues.size === 0 || (value != null && selectedValues.has(value));
  }
}
