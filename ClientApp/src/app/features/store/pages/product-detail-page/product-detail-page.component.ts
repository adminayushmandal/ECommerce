import { ChangeDetectionStrategy, ChangeDetectorRef, Component, DestroyRef, ElementRef, NgZone, ViewChild, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DialogModule } from 'primeng/dialog';
import { TooltipModule } from 'primeng/tooltip';
import { map } from 'rxjs';

import {
  CatalogProductVariant,
  ProductStoreAvailability,
  formatCurrency,
  formatDistance,
  getDefaultVariant,
  getSelectedPrice,
} from '../../../../core/models/store.models';
import { CatalogApiService } from '../../../../core/services/catalog-api.service';
import { EnmaApiService } from '../../../../core/services/enma-api.service';
import { GuestCartService } from '../../../../core/services/guest-cart.service';
import { ShopperLocationService } from '../../../../core/services/shopper-location.service';
import { StoreApiService } from '../../../../core/services/store-api.service';
import { ToastService } from '../../../../core/services/toast.service';

interface EnmaChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  streaming?: boolean;
  error?: boolean;
}

@Component({
  selector: 'app-product-detail-page',
  standalone: true,
  imports: [RouterLink, TooltipModule, DialogModule],
  templateUrl: './product-detail-page.component.html',
  styleUrl: './product-detail-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductDetailPageComponent {
  @ViewChild('enmaTimeline')
  private enmaTimeline?: ElementRef<HTMLDivElement>;

  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly title = inject(Title);
  private readonly ngZone = inject(NgZone);
  private readonly catalogApi = inject(CatalogApiService);
  private readonly enmaApi = inject(EnmaApiService);
  private readonly guestCart = inject(GuestCartService);
  private readonly shopperLocation = inject(ShopperLocationService);
  private readonly storeApi = inject(StoreApiService);
  private readonly toast = inject(ToastService);

  private currentProductId: string | null = null;
  private enmaAbortController: AbortController | null = null;
  private readonly slug = toSignal(this.route.paramMap.pipe(map((params) => params.get('slug'))), {
    initialValue: this.route.snapshot.paramMap.get('slug'),
  });

  protected readonly formatCurrency = formatCurrency;
  protected readonly formatDistance = formatDistance;
  protected readonly loading = this.catalogApi.loading;
  protected readonly selectedVariantId = signal<string | null>(null);
  protected readonly quantity = signal(1);
  protected readonly availability = signal<ProductStoreAvailability[]>([]);
  protected readonly availabilityLoading = signal(false);
  protected readonly availabilityError = signal<string | null>(null);
  protected readonly enmaPrompt = signal('');
  protected readonly enmaStreaming = signal(false);
  protected readonly enmaError = signal<string | null>(null);
  protected readonly enmaDialogVisible = signal(false);
  protected readonly enmaMessages = signal<EnmaChatMessage[]>([]);
  protected readonly shopperLocationLabel = computed(() => this.shopperLocation.location()?.label ?? 'No location selected');
  protected readonly product = computed(() => this.catalogApi.findBySlug(this.slug()));
  protected readonly notFound = computed(() => this.catalogApi.loaded() && !this.product());
  protected readonly selectedVariant = computed<CatalogProductVariant | null>(() => {
    const product = this.product();
    const selectedVariantId = this.selectedVariantId();

    if (!product) {
      return null;
    }

    return product.variants.find((variant) => variant.id === selectedVariantId) ?? getDefaultVariant(product);
  });
  protected readonly displayPrice = computed(() => {
    const product = this.product();
    if (!product) {
      return 0;
    }

    return getSelectedPrice(product, this.selectedVariant());
  });
  protected readonly relatedProducts = computed(() => {
    const product = this.product();
    if (!product) {
      return [];
    }

    return this.catalogApi.products()
      .filter((candidate) => candidate.categoryId === product.categoryId && candidate.id !== product.id)
      .slice(0, 3);
  });
  protected readonly nearestStore = computed(() => this.availability().find((store) => store.canFulfill) ?? this.availability()[0] ?? null);
  protected readonly hasShopperLocation = this.shopperLocation.hasLocation;
  protected readonly canAskEnma = computed(() => !!this.product() && this.enmaPrompt().trim().length > 0 && !this.enmaStreaming());
  protected readonly hasEnmaMessages = computed(() => this.enmaMessages().length > 0);
  protected readonly enmaSuggestions = computed(() => {
    const product = this.product();

    if (!product) {
      return [];
    }

    return [
      `Give me a quick summary of ${product.name}.`,
      `What are the key attributes and price options for ${product.name}?`,
      `Which ${product.name} variant looks best for everyday use?`,
    ];
  });

  constructor() {
    effect(() => {
      const product = this.product();
      if (!product || product.id === this.currentProductId) {
        return;
      }

      this.currentProductId = product.id;
      this.selectedVariantId.set(getDefaultVariant(product)?.id ?? null);
      this.quantity.set(1);
      this.resetEnmaState();
    });

    effect(() => {
      if (this.loading() && !this.product()) {
        this.title.setTitle('Loading Product | ECommerce');
        return;
      }

      if (this.notFound()) {
        this.title.setTitle('Product Not Found | ECommerce');
        return;
      }

      const product = this.product();
      if (product) {
        this.title.setTitle(`${product.name} | ECommerce`);
      }
    });

    effect((onCleanup) => {
      const product = this.product();
      const variant = this.selectedVariant();
      const shopperLocation = this.shopperLocation.location();

      if (!product) {
        this.availability.set([]);
        this.availabilityError.set(null);
        this.availabilityLoading.set(false);
        return;
      }

      this.availabilityLoading.set(true);
      this.availabilityError.set(null);

      const subscription = this.storeApi
        .getProductAvailability(product.id, variant?.id ?? null, shopperLocation)
        .subscribe({
          next: (availability) => {
            this.availability.set(availability);
            this.availabilityLoading.set(false);
          },
          error: (error: Error) => {
            this.availability.set([]);
            this.availabilityError.set(error.message);
            this.availabilityLoading.set(false);
          },
        });

      onCleanup(() => subscription.unsubscribe());
    });

    this.destroyRef.onDestroy(() => {
      this.cancelEnmaRequest();
      this.title.setTitle('ECommerce');
    });
  }

  protected updateEnmaPrompt(value: string): void {
    this.enmaPrompt.set(value);
  }

  protected openEnmaDialog(): void {
    if (!this.product()) {
      return;
    }

    this.enmaDialogVisible.set(true);
  }

  protected handleEnmaDialogVisibleChange(visible: boolean): void {
    this.enmaDialogVisible.set(visible);

    if (!visible && this.enmaStreaming()) {
      this.stopEnma();
    }
  }

  protected applyEnmaSuggestion(prompt: string): void {
    this.enmaPrompt.set(prompt);
  }

  protected formatEnmaMessageContent(content: string): string {
    const escapedContent = content
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');

    const blocks = escapedContent
      .replace(/\r\n/g, '\n')
      .split(/\n{2,}/)
      .map((block) => block.trim())
      .filter((block) => block.length > 0);

    return blocks
      .map((block) => {
        const lines = block.split('\n').filter((line) => line.trim().length > 0);
        const isList = lines.every((line) => /^\s*[*-]\s+/.test(line));

        if (isList) {
          return `<ul>${lines
            .map((line) => `<li>${this.formatEnmaInlineMarkdown(line.replace(/^\s*[*-]\s+/, ''))}</li>`)
            .join('')}</ul>`;
        }

        return `<p>${lines.map((line) => this.formatEnmaInlineMarkdown(line)).join('<br>')}</p>`;
      })
      .join('');
  }

  protected handleEnmaPromptKeydown(event: KeyboardEvent): void {
    if (event.key !== 'Enter' || event.shiftKey) {
      return;
    }

    event.preventDefault();
    void this.askEnma();
  }

  protected async askEnma(): Promise<void> {
    const product = this.product();
    const userQuery = this.enmaPrompt().trim();

    if (!product || userQuery.length === 0 || this.enmaStreaming()) {
      return;
    }

    this.cancelEnmaRequest();

    const abortController = new AbortController();
    const userMessageId = this.createEnmaMessageId('user');
    const assistantMessageId = this.createEnmaMessageId('assistant');

    this.enmaMessages.update((messages) => [
      ...messages,
      { id: userMessageId, role: 'user', content: userQuery },
      { id: assistantMessageId, role: 'assistant', content: '', streaming: true },
    ]);
    this.cdr.markForCheck();
    this.scheduleEnmaScroll();

    this.enmaAbortController = abortController;
    this.enmaStreaming.set(true);
    this.enmaError.set(null);
    this.enmaPrompt.set('');

    try {
      await this.enmaApi.streamProductDetail(
        {
          userQuery,
          productId: product.id,
        },
        {
          signal: abortController.signal,
          onChunk: (content) => {
            this.ngZone.run(() => {
              this.appendAssistantChunk(assistantMessageId, content);
              this.cdr.markForCheck();
              this.scheduleEnmaScroll();
            });
          },
        },
      );
    } catch (error) {
      if (abortController.signal.aborted) {
        this.ngZone.run(() => {
          this.finishAssistantMessage(assistantMessageId);
          this.cdr.markForCheck();
        });
        return;
      }

      const message = error instanceof Error ? error.message : 'Unable to get a response from Enma right now.';
      this.ngZone.run(() => {
        this.enmaError.set(message);
        this.setAssistantError(assistantMessageId, message);
        this.cdr.markForCheck();
        this.scheduleEnmaScroll();
        this.toast.error('Enma unavailable', message);
      });
    } finally {
      this.ngZone.run(() => {
        if (this.enmaAbortController === abortController) {
          this.enmaAbortController = null;
        }

        this.finishAssistantMessage(assistantMessageId);
        this.enmaStreaming.set(false);
        this.cdr.markForCheck();
        this.scheduleEnmaScroll();
      });
    }
  }

  protected stopEnma(): void {
    this.cancelEnmaRequest();
    this.enmaStreaming.set(false);
  }

  protected selectVariant(variantId: string): void {
    this.selectedVariantId.set(variantId);
  }

  protected increaseQuantity(): void {
    this.quantity.update((quantity) => quantity + 1);
  }

  protected decreaseQuantity(): void {
    this.quantity.update((quantity) => Math.max(1, quantity - 1));
  }

  protected addToCart(): void {
    const product = this.product();
    if (!product) {
      return;
    }

    this.guestCart.addItem(product, this.selectedVariant(), this.quantity());
    this.toast.success(
      'Added to cart',
      `${this.quantity()} x ${product.name}${this.selectedVariant()?.name ? ` (${this.selectedVariant()?.name})` : ''} added successfully.`
    );
  }

  protected async buyNow(): Promise<void> {
    const product = this.product();
    if (!product) {
      return;
    }

    this.guestCart.addItem(product, this.selectedVariant(), this.quantity());
    this.toast.info(
      'Checkout started',
      `${product.name}${this.selectedVariant()?.name ? ` (${this.selectedVariant()?.name})` : ''} was added to your cart. Sign in to continue checkout.`
    );
    await this.router.navigate(['/identity/login'], {
      queryParams: {
        intent: 'checkout',
        redirectTo: `/products/${product.slug}`,
      },
    });
  }

  protected async useBrowserLocation(): Promise<void> {
    const success = await this.shopperLocation.useBrowserLocation();
    if (!success) {
      this.toast.warn('Location unavailable', 'Allow location access in the browser to sort nearby stores.');
      return;
    }

    this.toast.success('Location updated', 'Store availability is now ordered by your current location.');
  }

  protected clearLocation(): void {
    this.shopperLocation.clear();
    this.toast.info('Location cleared', 'Store availability will be shown without distance-based ordering.');
  }

  private cancelEnmaRequest(): void {
    this.enmaAbortController?.abort();
    this.enmaAbortController = null;
  }

  private resetEnmaState(): void {
    this.cancelEnmaRequest();
    this.enmaPrompt.set('');
    this.enmaError.set(null);
    this.enmaStreaming.set(false);
    this.enmaMessages.set([]);
    this.cdr.markForCheck();
  }

  private appendAssistantChunk(messageId: string, chunk: string): void {
    this.enmaMessages.update((messages) =>
      messages.map((message) =>
        message.id === messageId
          ? {
              ...message,
              content: message.content + chunk,
            }
          : message,
      ),
    );
  }

  private finishAssistantMessage(messageId: string): void {
    this.enmaMessages.update((messages) =>
      messages.map((message) =>
        message.id === messageId
          ? {
              ...message,
              streaming: false,
            }
          : message,
      ),
    );
  }

  private setAssistantError(messageId: string, errorMessage: string): void {
    this.enmaMessages.update((messages) =>
      messages.map((message) =>
        message.id === messageId
          ? {
              ...message,
              content: message.content || errorMessage,
              error: true,
              streaming: false,
            }
          : message,
      ),
    );
  }

  private createEnmaMessageId(role: EnmaChatMessage['role']): string {
    return `${role}-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
  }

  private formatEnmaInlineMarkdown(content: string): string {
    return content.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');
  }

  private scheduleEnmaScroll(): void {
    queueMicrotask(() => {
      const timeline = this.enmaTimeline?.nativeElement;

      if (!timeline) {
        return;
      }

      timeline.scrollTop = timeline.scrollHeight;
    });
  }
}
