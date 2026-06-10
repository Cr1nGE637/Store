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
  mainImage?: ProductImage | null;
};

export type ProductImage = {
  productImageId: string;
  productId: string;
  url: string;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  altText: string;
  isMain: boolean;
  displayOrder: number;
  createdAtUtc: string;
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
  mainImage?: CartItemImage | null;
  price: number;
  quantity: number;
};

export type CartItemImage = {
  url: string;
  altText: string;
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

export type ProductImportPreview = {
  totalRows: number;
  createCount: number;
  updateCount: number;
  errorCount: number;
  rows: ProductImportPreviewRow[];
  errors: string[];
};

export type ProductImportPreviewRow = {
  rowNumber: number;
  sku: string;
  name?: string | null;
  categoryCode?: string | null;
  price?: number | null;
  stockQuantity?: number | null;
  action: "Create" | "Update" | "Error" | string;
  errors: string[];
};

export type ProductImportResult = {
  totalRows: number;
  createdCount: number;
  updatedCount: number;
  stockUpdatedCount: number;
};

export type ConsultationItem = {
  productId: string;
  categoryCode: string;
  specifications: Record<string, string>;
};

export type CompatibilityFinding = {
  severity: "Ok" | "Warning" | "Error" | "Unknown" | string;
  ruleCode: string;
  message: string;
};

export type ProductRecommendation = {
  productId: string;
  type: "Alternative" | "Accessory" | "RequiredPart" | string;
  reason: string;
};

export type ConsultationResult = {
  consultationId: string;
  status: "Ok" | "Warning" | "Error" | "Unknown" | string;
  items: ConsultationItem[];
  findings: CompatibilityFinding[];
  recommendations: ProductRecommendation[];
  checkedAtUtc: string;
};

export type CompatibilityRulePayload = {
  code: string;
  name: string;
  sourceCategoryCode: string;
  targetCategoryCode: string;
  sourceSpecificationKey: string;
  targetSpecificationKey: string;
  operator: "Equals" | "NotEquals" | "In" | "GreaterThanOrEqual" | "LessThanOrEqual" | string;
  expectedValue?: string | null;
  severity: "Ok" | "Warning" | "Error" | "Unknown" | string;
  messageTemplate: string;
  recommendationType?: "Alternative" | "Accessory" | "RequiredPart" | string | null;
  isActive: boolean;
};

export type CompatibilityRule = CompatibilityRulePayload & {
  id: string;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
};
