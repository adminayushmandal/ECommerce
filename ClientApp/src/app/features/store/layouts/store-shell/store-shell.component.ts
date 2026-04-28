import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { filter, map, startWith } from 'rxjs';

import { AccountSessionService } from '../../../../core/services/account-session.service';
import { CheckoutFlowService } from '../../../../core/services/checkout-flow.service';
import { GuestCartService } from '../../../../core/services/guest-cart.service';
import { DemoLocationKey, canRenderImageUrl, formatCurrency, getProductVisualBackground } from '../../../../core/models/store.models';
import { ShopperLocationService } from '../../../../core/services/shopper-location.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-store-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, BadgeModule, ButtonModule, DrawerModule, TagModule, TooltipModule],
  templateUrl: './store-shell.component.html',
  styleUrl: './store-shell.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StoreShellComponent {
  private readonly router = inject(Router);
  private readonly accountSession = inject(AccountSessionService);
  private readonly checkoutFlow = inject(CheckoutFlowService);
  private readonly shopperLocation = inject(ShopperLocationService);
  private readonly toast = inject(ToastService);
  protected readonly guestCart = inject(GuestCartService);

  protected readonly cartOpen = signal(false);
  protected readonly checkoutInProgress = this.checkoutFlow.checkoutInProgress;
  protected readonly formatCurrency = formatCurrency;
  protected readonly canRenderImageUrl = canRenderImageUrl;
  protected readonly currentUser = this.accountSession.currentUser;
  protected readonly isAuthenticated = this.accountSession.isAuthenticated;
  protected readonly sessionLoading = this.accountSession.loading;
  protected readonly shopperLocationState = this.shopperLocation.location;
  protected readonly canAccessManagerDashboard = computed(() => {
    const roles = this.currentUser()?.roles ?? [];
    return roles.some((role) => role === 'Administrator' || role === 'StoreManager');
  });

  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects),
      startWith(this.router.url)
    ),
    { initialValue: this.router.url }
  );

  constructor() {
    this.accountSession.refresh();
  }

  protected openCart(): void {
    this.cartOpen.set(true);
  }

  protected closeCart(): void {
    this.cartOpen.set(false);
  }

  protected increaseQuantity(lineId: string, quantity: number): void {
    this.guestCart.updateQuantity(lineId, quantity + 1);
    this.toast.info('Cart updated', 'Item quantity increased.');
  }

  protected decreaseQuantity(lineId: string, quantity: number): void {
    this.guestCart.updateQuantity(lineId, quantity - 1);
    this.toast.info(
      quantity - 1 <= 0 ? 'Item removed' : 'Cart updated',
      quantity - 1 <= 0 ? 'The item was removed from your cart.' : 'Item quantity decreased.'
    );
  }

  protected removeItem(lineId: string): void {
    this.guestCart.removeItem(lineId);
    this.toast.warn('Item removed', 'The item was removed from your cart.');
  }

  protected continueShopping(): void {
    this.closeCart();
    this.toast.info('Browsing continued', 'Your cart stays saved on this device.');
  }

  protected async proceedToCheckout(): Promise<void> {
    await this.checkoutFlow.startPayPalCheckout({
      redirectTo: this.currentUrl(),
      beforePaymentRedirect: () => this.closeCart(),
    });
  }

  protected secureCheckoutQueryParams(): Record<string, string> {
    return this.checkoutFlow.secureCheckoutQueryParams(this.currentUrl());
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

  protected getFirstName(): string {
    const displayName = this.currentUser()?.displayName?.trim();
    return displayName?.split(/\s+/)[0] ?? 'Account';
  }

  protected async useBrowserLocation(): Promise<void> {
    const success = await this.shopperLocation.useBrowserLocation();
    if (!success) {
      this.toast.warn('Location unavailable', 'Allow browser location access, or use one of the store demo locations.');
      return;
    }

    this.toast.success('Location updated', 'The storefront will now sort store availability by your location.');
  }

  protected useLudhianaDemoLocation(): void {
    this.useDemoLocation('ludhiana');
  }

  protected useDemoLocation(location: DemoLocationKey): void {
    this.shopperLocation.useDemoLocation(location);
    this.toast.info('Demo location active', `Using the ${this.shopperLocation.location()?.label ?? 'selected store'} for omnichannel availability previews.`);
  }

  protected clearLocation(): void {
    this.shopperLocation.clear();
    this.toast.info('Location cleared', 'Store availability will be shown without distance-based ordering.');
  }

  protected getCartItemVisualBackground(imageUrl: string): string {
    return getProductVisualBackground(imageUrl);
  }
}
