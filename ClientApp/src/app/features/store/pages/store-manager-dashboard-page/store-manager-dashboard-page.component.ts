import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { firstValueFrom } from 'rxjs';

import { StoreManagerPayment } from '../../../../core/models/payment.models';
import {
  CatalogProduct,
  CatalogProductRequest,
  StoreDto,
  formatCurrency,
  getProductVisualBackground,
} from '../../../../core/models/store.models';
import { AccountSessionService } from '../../../../core/services/account-session.service';
import { CatalogApiService } from '../../../../core/services/catalog-api.service';
import { StoreManagerApiService } from '../../../../core/services/store-manager-api.service';
import { ToastService } from '../../../../core/services/toast.service';

interface CategoryOption {
  id: string;
  name: string;
}

@Component({
  selector: 'app-store-manager-dashboard-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, ButtonModule, SkeletonModule, TagModule],
  templateUrl: './store-manager-dashboard-page.component.html',
  styleUrl: './store-manager-dashboard-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StoreManagerDashboardPageComponent {
  private readonly accountSession = inject(AccountSessionService);
  private readonly catalogApi = inject(CatalogApiService);
  private readonly managerApi = inject(StoreManagerApiService);
  private readonly toast = inject(ToastService);
  private readonly fb = inject(FormBuilder);
  private loadedForUserId: string | null = null;

  protected readonly currentUser = this.accountSession.currentUser;
  protected readonly isAuthenticated = this.accountSession.isAuthenticated;
  protected readonly sessionLoading = this.accountSession.loading;
  protected readonly formatCurrency = formatCurrency;
  protected readonly stores = signal<StoreDto[]>([]);
  protected readonly managerProducts = signal<CatalogProduct[]>([]);
  protected readonly managerPayments = signal<StoreManagerPayment[]>([]);
  protected readonly selectedStoreId = signal('');
  protected readonly selectedProductId = signal<string | null>(null);
  protected readonly productSearch = signal('');
  protected readonly loading = signal(false);
  protected readonly productsLoading = signal(false);
  protected readonly paymentsLoading = signal(false);
  protected readonly savingProduct = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly productForm = this.fb.nonNullable.group({
    id: [''],
    categoryId: ['', Validators.required],
    sku: ['', [Validators.required, Validators.maxLength(80)]],
    name: ['', [Validators.required, Validators.maxLength(180)]],
    slug: ['', [Validators.required, Validators.maxLength(180)]],
    description: ['', [Validators.required, Validators.maxLength(500)]],
    imageUrl: ['171717', Validators.required],
    basePrice: [0, [Validators.required, Validators.min(0)]],
    isActive: [true],
  });

  protected readonly canAccessManagerDashboard = computed(() => {
    const roles = this.currentUser()?.roles ?? [];
    return roles.some((role) => role === 'Administrator' || role === 'StoreManager');
  });

  protected readonly categoryOptions = computed<CategoryOption[]>(() => {
    const categories = new Map<string, string>();

    for (const product of this.managerProducts()) {
      if (!categories.has(product.categoryId)) {
        categories.set(product.categoryId, product.categoryName);
      }
    }

    return Array.from(categories, ([id, name]) => ({ id, name })).sort((left, right) =>
      left.name.localeCompare(right.name)
    );
  });

  protected readonly selectedStoreName = computed(() => {
    const selectedStoreId = this.selectedStoreId();
    return this.stores().find((store) => store.id === selectedStoreId)?.name ?? 'All stores';
  });

  protected readonly selectedProduct = computed(() => {
    const selectedProductId = this.selectedProductId();
    return this.managerProducts().find((product) => product.id === selectedProductId) ?? null;
  });

  protected readonly filteredProducts = computed(() => {
    const normalizedSearch = this.productSearch().trim().toLowerCase();

    if (!normalizedSearch) {
      return this.managerProducts();
    }

    return this.managerProducts().filter(
      (product) =>
        product.name.toLowerCase().includes(normalizedSearch) ||
        product.sku.toLowerCase().includes(normalizedSearch) ||
        product.categoryName.toLowerCase().includes(normalizedSearch) ||
        product.variants.some((variant) => variant.sku.toLowerCase().includes(normalizedSearch))
    );
  });

  protected readonly managerMetrics = computed(() => {
    const products = this.managerProducts();
    const payments = this.managerPayments();
    const capturedPayments = payments.filter((payment) => this.isStatus(payment.paymentStatus, 'Captured', 2));

    return {
      activeProducts: products.filter((product) => product.isActive).length,
      variantCount: products.reduce((total, product) => total + product.variants.length, 0),
      availableUnits: products.reduce((total, product) => total + (product.availableQuantity ?? 0), 0),
      capturedRevenue: capturedPayments.reduce((total, payment) => total + payment.amount, 0),
    };
  });

  constructor() {
    this.accountSession.refresh(true);

    effect(() => {
      const user = this.currentUser();

      if (!user) {
        this.loadedForUserId = null;
        this.managerProducts.set([]);
        this.managerPayments.set([]);
        return;
      }

      if (!this.canAccessManagerDashboard() || this.loadedForUserId === user.id) {
        return;
      }

      this.loadedForUserId = user.id;
      void this.loadDashboard();
    });
  }

  protected async refreshDashboard(): Promise<void> {
    await Promise.all([this.refreshProducts(), this.refreshPayments()]);
    this.toast.success('Dashboard refreshed', 'Store manager data is up to date.');
  }

  protected async onStoreChange(event: Event): Promise<void> {
    this.selectedStoreId.set((event.target as HTMLSelectElement).value);
    await Promise.all([this.refreshProducts(), this.refreshPayments()]);
  }

  protected onProductSearch(event: Event): void {
    this.productSearch.set((event.target as HTMLInputElement).value);
  }

  protected selectProduct(product: CatalogProduct): void {
    this.selectedProductId.set(product.id);
    this.productForm.setValue({
      id: product.id,
      categoryId: product.categoryId,
      sku: product.sku,
      name: product.name,
      slug: product.slug,
      description: product.description,
      imageUrl: product.imageUrl,
      basePrice: product.basePrice,
      isActive: product.isActive,
    });
  }

  protected startNewProduct(): void {
    const firstCategory = this.categoryOptions()[0];
    this.selectedProductId.set(null);
    this.productForm.setValue({
      id: '',
      categoryId: firstCategory?.id ?? '',
      sku: '',
      name: '',
      slug: '',
      description: '',
      imageUrl: '171717',
      basePrice: 0,
      isActive: true,
    });
  }

  protected syncSlugFromName(): void {
    if (this.productForm.controls.slug.dirty) {
      return;
    }

    this.productForm.controls.slug.setValue(this.toSlug(this.productForm.controls.name.value));
  }

  protected async saveProduct(): Promise<void> {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    const formValue = this.productForm.getRawValue();
    const request: CatalogProductRequest = {
      categoryId: formValue.categoryId,
      sku: formValue.sku.trim(),
      name: formValue.name.trim(),
      slug: formValue.slug.trim(),
      description: formValue.description.trim(),
      imageUrl: formValue.imageUrl.trim(),
      basePrice: Number(formValue.basePrice),
      isActive: formValue.isActive,
    };

    this.savingProduct.set(true);

    try {
      const product = formValue.id
        ? await firstValueFrom(this.catalogApi.updateProduct(formValue.id, request))
        : await firstValueFrom(this.catalogApi.createProduct(request));

      await this.refreshProducts();
      this.selectProduct(product);
      this.toast.success(formValue.id ? 'Product updated' : 'Product created', `${product.name} is ready for the catalog.`);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unable to save this product.';
      this.toast.error('Product save failed', message);
    } finally {
      this.savingProduct.set(false);
    }
  }

  protected async archiveSelectedProduct(): Promise<void> {
    const product = this.selectedProduct();
    if (!product) {
      return;
    }

    this.productForm.controls.isActive.setValue(false);
    await this.saveProduct();
  }

  protected getProductBackground(product: CatalogProduct): string {
    return getProductVisualBackground(product.imageUrl);
  }

  protected formatDate(value: string | null): string {
    if (!value) {
      return 'Not captured';
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return 'Date unavailable';
    }

    return new Intl.DateTimeFormat('en-IN', {
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(date);
  }

  protected formatStatus(value: number | string): string {
    if (typeof value === 'string') {
      return this.humanizeStatus(value);
    }

    const names: Record<number, string> = {
      0: 'Pending',
      1: 'Requires action',
      2: 'Captured',
      3: 'Failed',
      4: 'Cancelled',
      5: 'Partially refunded',
      6: 'Refunded',
    };

    return names[value] ?? 'Unknown';
  }

  protected getPaymentSeverity(payment: StoreManagerPayment): 'success' | 'secondary' | 'danger' | 'warn' {
    if (this.isStatus(payment.paymentStatus, 'Captured', 2)) {
      return 'success';
    }

    if (this.isStatus(payment.paymentStatus, 'Failed', 3) || this.isStatus(payment.paymentStatus, 'Cancelled', 4)) {
      return 'danger';
    }

    if (this.isStatus(payment.paymentStatus, 'RequiresAction', 1)) {
      return 'warn';
    }

    return 'secondary';
  }

  private async loadDashboard(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const stores = await firstValueFrom(this.managerApi.getStores());
      this.stores.set(stores);
      await Promise.all([this.refreshProducts(), this.refreshPayments()]);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unable to load the manager dashboard.';
      this.error.set(message);
      this.toast.error('Dashboard unavailable', message);
    } finally {
      this.loading.set(false);
    }
  }

  private async refreshProducts(): Promise<void> {
    this.productsLoading.set(true);

    try {
      const products = await firstValueFrom(this.managerApi.getProducts(this.selectedStoreId() || null));
      this.managerProducts.set(products);

      const selectedProduct = this.selectedProduct();
      if (selectedProduct) {
        this.selectProduct(selectedProduct);
      } else if (products.length > 0) {
        this.selectProduct(products[0]);
      } else {
        this.startNewProduct();
      }
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unable to load manager products.';
      this.error.set(message);
      this.toast.error('Products unavailable', message);
    } finally {
      this.productsLoading.set(false);
    }
  }

  private async refreshPayments(): Promise<void> {
    this.paymentsLoading.set(true);

    try {
      const payments = await firstValueFrom(this.managerApi.getPayments(this.selectedStoreId() || null));
      this.managerPayments.set(payments);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unable to load manager payments.';
      this.error.set(message);
      this.toast.error('Payments unavailable', message);
    } finally {
      this.paymentsLoading.set(false);
    }
  }

  private isStatus(value: number | string, name: string, numericValue: number): boolean {
    return typeof value === 'number'
      ? value === numericValue
      : value.replace(/\s+/g, '').toLowerCase() === name.toLowerCase();
  }

  private humanizeStatus(value: string): string {
    return value
      .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
      .replace(/[_-]+/g, ' ')
      .trim()
      .replace(/\w\S*/g, (word) => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase());
  }

  private toSlug(value: string): string {
    return value
      .trim()
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }
}
