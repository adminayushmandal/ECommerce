import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { CatalogProduct, formatCurrency, getDefaultVariant, getStartingPrice } from '../../../../core/models/store.models';
import { CatalogApiService } from '../../../../core/services/catalog-api.service';
import { GuestCartService } from '../../../../core/services/guest-cart.service';
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
  private readonly guestCart = inject(GuestCartService);
  private readonly toast = inject(ToastService);

  protected readonly formatCurrency = formatCurrency;
  protected readonly searchTerm = signal('');
  protected readonly selectedCategory = signal('All');
  protected readonly products = this.catalogApi.products;
  protected readonly loading = this.catalogApi.loading;
  protected readonly error = this.catalogApi.error;
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
      title: 'New-season favorites',
      description: 'Products selected for quick discovery, strong visuals, and easy add-to-cart flow.',
      value: `${this.products().slice(0, 4).length} spotlight items`,
    },
    {
      title: 'Guest-ready shopping',
      description: 'Anonymous visitors can browse, search, and keep a cart before moving into secure checkout.',
      value: `${this.guestCart.itemCount()} item${this.guestCart.itemCount() === 1 ? '' : 's'} saved`,
    },
    {
      title: 'Fast product comparison',
      description: 'Category filters and keyword search keep the catalog easy to scan even on smaller screens.',
      value: `${this.categories().length - 1} live categories`,
    },
  ]);

  protected onSearchInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchTerm.set(target.value);
  }

  protected setCategory(category: string): void {
    this.selectedCategory.set(category);
  }

  protected refreshProducts(): void {
    this.catalogApi.loadProducts(true);
    this.toast.info('Refreshing catalog', 'Fetching the latest product collection.');
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
}
