import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { filter, map, startWith } from 'rxjs';

import { CatalogApiService } from '../../../../core/services/catalog-api.service';
import { GuestCartService } from '../../../../core/services/guest-cart.service';
import { formatCurrency } from '../../../../core/models/store.models';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-store-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  templateUrl: './store-shell.component.html',
  styleUrl: './store-shell.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StoreShellComponent {
  private readonly router = inject(Router);
  private readonly catalogApi = inject(CatalogApiService);
  private readonly toast = inject(ToastService);
  protected readonly guestCart = inject(GuestCartService);

  protected readonly cartOpen = signal(false);
  protected readonly formatCurrency = formatCurrency;
  protected readonly featuredCounts = computed(() => {
    const products = this.catalogApi.products();
    const categories = new Set(products.map((product) => product.categoryName));

    return {
      products: products.length,
      categories: categories.size,
      cartItems: this.guestCart.itemCount(),
    };
  });

  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects),
      startWith(this.router.url)
    ),
    { initialValue: this.router.url }
  );

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

  protected proceedToCheckout(): void {
    this.closeCart();
    this.toast.info('Checkout ready', 'Sign in to continue with secure checkout.');
  }

  protected secureCheckoutQueryParams(): Record<string, string> {
    return {
      intent: 'checkout',
      redirectTo: this.currentUrl(),
    };
  }
}
