import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { firstValueFrom } from 'rxjs';

import { OrderDto, OrderStatus } from '../../../../core/models/order.models';
import { AccountPreferenceState } from '../../../../core/models/session.models';
import { formatCurrency } from '../../../../core/models/store.models';
import { AccountSessionService } from '../../../../core/services/account-session.service';
import { AuthApiService } from '../../../../core/services/auth-api.service';
import { GuestCartService } from '../../../../core/services/guest-cart.service';
import { OrderApiService } from '../../../../core/services/order-api.service';
import { ToastService } from '../../../../core/services/toast.service';

const preferenceStorageKey = 'ecommerce.account-preferences';

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [FormsModule, RouterLink, ButtonModule, SkeletonModule, TagModule, ToggleSwitchModule],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfilePageComponent {
  private readonly router = inject(Router);
  private readonly authApi = inject(AuthApiService);
  private readonly accountSession = inject(AccountSessionService);
  private readonly guestCart = inject(GuestCartService);
  private readonly orderApi = inject(OrderApiService);
  private readonly toast = inject(ToastService);
  private ordersLoadedForUserId: string | null = null;

  protected readonly currentUser = this.accountSession.currentUser;
  protected readonly isAuthenticated = this.accountSession.isAuthenticated;
  protected readonly sessionLoading = this.accountSession.loading;
  protected readonly preferences = signal<AccountPreferenceState>(this.restorePreferences());
  protected readonly orders = signal<OrderDto[]>([]);
  protected readonly ordersLoading = signal(false);
  protected readonly ordersError = signal<string | null>(null);
  protected readonly formatCurrency = formatCurrency;
  protected readonly orderSummary = computed(() => {
    const orders = this.orders();

    return {
      count: orders.length,
      totalSpend: orders.reduce((total, order) => total + order.totalAmount, 0),
    };
  });
  protected readonly cartSnapshot = computed(() => ({
    itemCount: this.guestCart.itemCount(),
    subtotal: this.guestCart.subtotal(),
  }));

  constructor() {
    this.accountSession.refresh(true);

    effect(() => {
      const user = this.currentUser();

      if (!user) {
        this.ordersLoadedForUserId = null;
        this.orders.set([]);
        this.ordersError.set(null);
        return;
      }

      if (this.ordersLoadedForUserId === user.id) {
        return;
      }

      this.ordersLoadedForUserId = user.id;
      void this.loadOrders();
    });
  }

  protected getUserInitials(): string {
    const displayName = this.currentUser()?.displayName?.trim();
    if (!displayName) {
      return 'EC';
    }

    return displayName
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part.charAt(0).toUpperCase())
      .join('');
  }

  protected updatePreference(preference: keyof AccountPreferenceState, event: Event): void {
    const target = event.target as HTMLInputElement;
    this.updatePreferenceValue(preference, target.checked);
  }

  protected updatePreferenceValue(preference: keyof AccountPreferenceState, checked: boolean): void {
    this.preferences.update((current) => ({
      ...current,
      [preference]: checked,
    }));

    this.persistPreferences();
    this.toast.success('Settings updated', 'Your storefront preferences were saved on this device.');
  }

  protected formatOrderDate(value: string): string {
    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return 'Date unavailable';
    }

    return new Intl.DateTimeFormat('en-IN', {
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(date);
  }

  protected formatOrderStatus(status: OrderStatus): string {
    if (typeof status === 'string') {
      return this.humanizeStatus(status);
    }

    const statusNames: Record<number, string> = {
      0: 'Draft',
      1: 'Pending payment',
      2: 'Allocated',
      3: 'Packed',
      4: 'Shipped',
      5: 'Delivered',
      6: 'Cancelled',
    };

    return statusNames[status] ?? 'Unknown';
  }

  protected getOrderItemSummary(order: OrderDto): string {
    const itemCount = order.items.reduce((total, item) => total + item.quantity, 0);

    if (itemCount === 0) {
      return 'No items';
    }

    return itemCount === 1 ? '1 item' : `${itemCount} items`;
  }

  protected async refreshOrders(): Promise<void> {
    const user = this.currentUser();
    if (!user) {
      return;
    }

    this.ordersLoadedForUserId = user.id;
    await this.loadOrders();
  }

  protected async signOut(): Promise<void> {
    try {
      await firstValueFrom(this.authApi.logout());
    } catch {
      // Ignore network/session expiry errors and still clear client state.
    } finally {
      this.accountSession.clear();
      this.toast.info('Signed out', 'Your account session has been closed.');
      await this.router.navigateByUrl('/');
    }
  }

  private async loadOrders(): Promise<void> {
    this.ordersLoading.set(true);
    this.ordersError.set(null);

    try {
      const orders = await firstValueFrom(this.orderApi.getMyOrders());
      this.orders.set(orders);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unable to load orders.';
      this.ordersError.set(message);
      this.toast.error('Orders unavailable', message);
    } finally {
      this.ordersLoading.set(false);
    }
  }

  private humanizeStatus(value: string): string {
    return value
      .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
      .replace(/[_-]+/g, ' ')
      .trim()
      .replace(/\w\S*/g, (word) => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase());
  }

  private restorePreferences(): AccountPreferenceState {
    if (typeof window === 'undefined' || !window.localStorage) {
      return this.getDefaultPreferences();
    }

    try {
      const rawValue = window.localStorage.getItem(preferenceStorageKey);
      if (!rawValue) {
        return this.getDefaultPreferences();
      }

      const parsed = JSON.parse(rawValue) as Partial<AccountPreferenceState>;
      return {
        orderUpdates: parsed.orderUpdates ?? true,
        wishListAlerts: parsed.wishListAlerts ?? true,
        restockNotices: parsed.restockNotices ?? false,
      };
    } catch {
      return this.getDefaultPreferences();
    }
  }

  private persistPreferences(): void {
    if (typeof window === 'undefined' || !window.localStorage) {
      return;
    }

    window.localStorage.setItem(preferenceStorageKey, JSON.stringify(this.preferences()));
  }

  private getDefaultPreferences(): AccountPreferenceState {
    return {
      orderUpdates: true,
      wishListAlerts: true,
      restockNotices: false,
    };
  }
}
