import { OrderItemDto } from './cart.models';

export type OrderStatus = number | string;

export interface OrderDto {
  id: string;
  orderNumber: string;
  userId: string;
  customerEmail: string;
  customerLatitude: number;
  customerLongitude: number;
  allocatedStoreId: string | null;
  allocatedStoreName: string | null;
  createdAt: string;
  status: OrderStatus;
  totalAmount: number;
  items: OrderItemDto[];
}
