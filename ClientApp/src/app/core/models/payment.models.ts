export interface PayPalCreateOrderRequest {
  storeId: string | null;
  customerEmail: string;
  customerLatitude: number;
  customerLongitude: number;
  returnUrl: string;
  cancelUrl: string;
}

export interface PayPalCreateOrderResponse {
  paymentId: string;
  orderId: string;
  orderNumber: string;
  payPalOrderId: string;
  approvalUrl: string;
  amount: number;
  currencyCode: string;
}

export interface PayPalCaptureResponse {
  paymentId: string;
  orderId: string;
  orderNumber: string;
  payPalOrderId: string;
  payPalCaptureId: string | null;
  paymentStatus: number | string;
  orderStatus: number | string;
  amount: number;
  currencyCode: string;
}

export interface PayPalClientConfig {
  clientId: string;
  currencyCode: string;
  intent: string;
}

export interface StoreManagerPayment {
  paymentId: string;
  orderId: string;
  orderNumber: string;
  customerEmail: string;
  allocatedStoreId: string | null;
  allocatedStoreName: string | null;
  provider: number | string;
  providerOrderId: string | null;
  providerCaptureId: string | null;
  paymentStatus: number | string;
  orderStatus: number | string;
  amount: number;
  currency: string;
  refundedAmount: number;
  failureReason: string | null;
  createdAt: string;
  capturedAt: string | null;
}
