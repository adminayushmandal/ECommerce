import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { PaymentApiService } from '../../../../core/services/payment-api.service';

@Component({
  selector: 'app-paypal-cancel-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './paypal-cancel-page.component.html',
  styleUrl: './paypal-cancel-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PayPalCancelPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly paymentApi = inject(PaymentApiService);

  protected readonly status = signal<'cancelling' | 'cancelled' | 'error'>('cancelling');
  protected readonly errorMessage = signal<string | null>(null);

  async ngOnInit(): Promise<void> {
    const payPalOrderId = this.route.snapshot.queryParamMap.get('token');

    if (!payPalOrderId) {
      this.status.set('cancelled');
      return;
    }

    try {
      await firstValueFrom(this.paymentApi.cancelPayPalOrder(payPalOrderId));
      this.status.set('cancelled');
    } catch (error) {
      this.status.set('error');
      this.errorMessage.set(error instanceof Error ? error.message : 'Unable to cancel the PayPal payment.');
    }
  }
}
