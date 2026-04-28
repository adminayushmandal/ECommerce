import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AuthenticatedUser } from '../models/auth.models';
import { LookupResponse } from '../models/session.models';
import { AccountSessionService } from './account-session.service';
import { CartApiService } from './cart-api.service';
import { GuestCartService } from './guest-cart.service';
import { PaymentApiService } from './payment-api.service';
import { ShopperLocationService } from './shopper-location.service';
import { ToastService } from './toast.service';

interface StartCheckoutOptions {
  redirectTo?: string;
  beforePaymentRedirect?: () => void;
}

@Injectable({ providedIn: 'root' })
export class CheckoutFlowService {
  private readonly router = inject(Router);
  private readonly http = inject(HttpClient);
  private readonly accountSession = inject(AccountSessionService);
  private readonly cartApi = inject(CartApiService);
  private readonly guestCart = inject(GuestCartService);
  private readonly paymentApi = inject(PaymentApiService);
  private readonly shopperLocation = inject(ShopperLocationService);
  private readonly toast = inject(ToastService);

  private readonly inProgress = signal(false);

  readonly checkoutInProgress = computed(() => this.inProgress());

  secureCheckoutQueryParams(redirectTo = this.router.url): Record<string, string> {
    return {
      intent: 'checkout',
      redirectTo,
    };
  }

  async startPayPalCheckout(options: StartCheckoutOptions = {}): Promise<void> {
    const currentUser = await this.getCurrentUser();
    const redirectTo = options.redirectTo ?? this.router.url;

    if (!currentUser) {
      this.toast.info('Checkout ready', 'Sign in to continue with secure checkout.');
      await this.router.navigate(['/identity/login'], {
        queryParams: this.secureCheckoutQueryParams(redirectTo),
      });
      return;
    }

    if (this.guestCart.items().length === 0) {
      this.toast.warn('Cart is empty', 'Add at least one product before checkout.');
      return;
    }

    let location = this.shopperLocation.location();
    if (!location) {
      this.shopperLocation.useLudhianaDemoLocation();
      location = this.shopperLocation.location();
      this.toast.info('Demo location active', 'Using the Ludhiana store area for omnichannel checkout allocation.');
    }

    if (!location) {
      this.toast.warn('Location required', 'Choose a fulfillment location before checkout.');
      return;
    }

    this.inProgress.set(true);

    try {
      await this.replaceServerCart(currentUser.email, location.latitude, location.longitude);

      const origin = window.location.origin;
      const payPalOrder = await firstValueFrom(
        this.paymentApi.createPayPalOrder({
          storeId: null,
          customerEmail: currentUser.email,
          customerLatitude: location.latitude,
          customerLongitude: location.longitude,
          returnUrl: `${origin}/checkout/paypal/return`,
          cancelUrl: `${origin}/checkout/paypal/cancel`,
        })
      );

      options.beforePaymentRedirect?.();
      window.location.assign(payPalOrder.approvalUrl);
    } catch (error) {
      this.toast.error(
        'Checkout failed',
        error instanceof Error ? error.message : 'Unable to start PayPal checkout.'
      );
    } finally {
      this.inProgress.set(false);
    }
  }

  private async getCurrentUser(): Promise<AuthenticatedUser | null> {
    const currentUser = this.accountSession.currentUser();
    if (currentUser) {
      return currentUser;
    }

    try {
      const lookup = await firstValueFrom(
        this.http.get<LookupResponse>('/api/Lookup', { withCredentials: true })
      );
      this.accountSession.markAuthenticated(lookup.currentUser);
      return lookup.currentUser;
    } catch {
      this.accountSession.clear();
      return null;
    }
  }

  private async replaceServerCart(customerEmail: string, customerLatitude: number, customerLongitude: number): Promise<void> {
    const serverCart = await firstValueFrom(this.cartApi.getCart());

    for (const item of serverCart.items) {
      await firstValueFrom(this.cartApi.removeItem(item.id));
    }

    for (const item of this.guestCart.items()) {
      await firstValueFrom(
        this.cartApi.addItem({
          productId: item.productId,
          productVariantId: item.variantId,
          quantity: item.quantity,
          customerEmail,
          customerLatitude,
          customerLongitude,
        })
      );
    }
  }
}
