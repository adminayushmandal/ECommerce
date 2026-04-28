export interface OrderItemDto {
  id: string;
  productId: string;
  productVariantId: string | null;
  productName: string;
  variantName: string | null;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface CartDto {
  orderId: string | null;
  orderNumber: string | null;
  userId: string;
  customerEmail: string;
  customerLatitude: number;
  customerLongitude: number;
  totalAmount: number;
  items: OrderItemDto[];
  isEmpty: boolean;
}

export interface AddCartItemRequest {
  productId: string;
  productVariantId: string | null;
  quantity: number;
  customerEmail: string;
  customerLatitude: number;
  customerLongitude: number;
}

export interface UpdateCartItemQuantityRequest {
  quantity: number;
}

export interface CheckoutCartRequest {
  storeId: string | null;
  customerEmail: string;
  customerLatitude: number;
  customerLongitude: number;
}
