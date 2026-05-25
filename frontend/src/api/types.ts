export type Role = "Customer" | "Manager" | string;

export type LoginResponse = {
  email: string;
  token: string;
  role: Role;
};

export type RegisterResponse = {
  userId: string;
  name: string;
  email: string;
  role: Role;
};

export type Category = {
  categoryId: string;
  categoryName: string;
  categoryCode: string;
};

export type Product = {
  productId: string;
  sku: string;
  productName: string;
  productDescription: string;
  productPrice: number;
  brand: string;
  model: string;
  warrantyMonths: number;
  categoryId: string;
  specifications: Record<string, string>;
  availableQuantity: number;
  isInStock: boolean;
};

export type ProductFilters = {
  search?: string;
  categoryId?: string;
  brand?: string;
  minPrice?: string;
  maxPrice?: string;
  inStockOnly?: boolean;
  specificationKey?: string;
  specificationValue?: string;
};

export type CartItem = {
  cartItemId: string;
  productId: string;
  productName: string;
  price: number;
  quantity: number;
};

export type Cart = {
  cartId: string;
  customerId: string;
  isCheckoutPending: boolean;
  items: CartItem[];
};

export type CheckoutRequest = {
  recipientName: string;
  phone: string;
  deliveryAddress: string;
  deliveryMethod: string;
  paymentMethod: string;
};

export type CheckoutResult = {
  cartId: string;
  customerId: string;
  items: CartItem[];
};

export type OrderProduct = {
  productId: string;
  productName: string;
  price: number;
  quantity: number;
};

export type Order = {
  orderId: string;
  customerId: string;
  recipientName: string;
  phone: string;
  paymentMethod: string;
  totalAmount: number;
  status: string;
  createdAt: string;
  paidAt?: string | null;
  paidAmount?: number | null;
  paymentTransactionId?: string | null;
  cancelledAt?: string | null;
  products: OrderProduct[];
};

export type ProductPayload = {
  sku: string;
  productName: string;
  productDescription: string;
  productPrice: number;
  brand: string;
  model: string;
  warrantyMonths: number;
  categoryId: string;
  specifications: Record<string, string>;
};
