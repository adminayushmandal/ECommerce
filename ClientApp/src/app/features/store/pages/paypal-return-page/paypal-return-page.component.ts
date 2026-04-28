import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { formatCurrency } from '../../../../core/models/store.models';
import { GuestCartService } from '../../../../core/services/guest-cart.service';
import { PaymentApiService } from '../../../../core/services/payment-api.service';

@Component({
  selector: 'app-paypal-return-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './paypal-return-page.component.html',
  styleUrl: './paypal-return-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PayPalReturnPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly paymentApi = inject(PaymentApiService);
  private readonly guestCart = inject(GuestCartService);

  protected readonly formatCurrency = formatCurrency;
  protected readonly status = signal<'capturing' | 'success' | 'error'>('capturing');
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly orderNumber = signal<string | null>(null);
  protected readonly amount = signal<number | null>(null);
  protected readonly currencyCode = signal<string | null>(null);

  async ngOnInit(): Promise<void> {
    const payPalOrderId = this.route.snapshot.queryParamMap.get('token');

    if (!payPalOrderId) {
      this.status.set('error');
      this.errorMessage.set('PayPal did not return an order token.');
      return;
    }

    try {
      const capture = await firstValueFrom(this.paymentApi.capturePayPalOrder(payPalOrderId));
      this.guestCart.clear();
      this.orderNumber.set(capture.orderNumber);
      this.amount.set(capture.amount);
      this.currencyCode.set(capture.currencyCode);
      this.status.set('success');
    } catch (error) {
      this.status.set('error');
      this.errorMessage.set(error instanceof Error ? error.message : 'Unable to capture the PayPal payment.');
    }
  }
}
