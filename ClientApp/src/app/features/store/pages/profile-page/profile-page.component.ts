import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AccountPreferenceState } from '../../../../core/models/session.models';
import { formatCurrency } from '../../../../core/models/store.models';
import { AccountSessionService } from '../../../../core/services/account-session.service';
import { AuthApiService } from '../../../../core/services/auth-api.service';
import { GuestCartService } from '../../../../core/services/guest-cart.service';
import { ToastService } from '../../../../core/services/toast.service';

const preferenceStorageKey = 'ecommerce.account-preferences';

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfilePageComponent {
  private readonly router = inject(Router);
  private readonly authApi = inject(AuthApiService);
  private readonly accountSession = inject(AccountSessionService);
  private readonly guestCart = inject(GuestCartService);
  private readonly toast = inject(ToastService);

  protected readonly currentUser = this.accountSession.currentUser;
  protected readonly isAuthenticated = this.accountSession.isAuthenticated;
  protected readonly sessionLoading = this.accountSession.loading;
  protected readonly preferences = signal<AccountPreferenceState>(this.restorePreferences());
  protected readonly formatCurrency = formatCurrency;
  protected readonly cartSnapshot = computed(() => ({
    itemCount: this.guestCart.itemCount(),
    subtotal: this.guestCart.subtotal(),
  }));

  constructor() {
    this.accountSession.refresh(true);
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
    this.preferences.update((current) => ({
      ...current,
      [preference]: target.checked,
    }));

    this.persistPreferences();
    this.toast.success('Settings updated', 'Your storefront preferences were saved on this device.');
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
