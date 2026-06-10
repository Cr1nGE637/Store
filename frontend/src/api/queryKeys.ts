import type { ProductFilters } from "./types";

export const queryKeys = {
  adminProductsRoot: ["admin-products"] as const,
  adminProducts: (token?: string) => ["admin-products", token ?? "anonymous"] as const,
  cartRoot: ["cart"] as const,
  cart: (token?: string) => ["cart", token ?? "anonymous"] as const,
  cartConsultationRoot: ["cart-consultation"] as const,
  cartConsultation: (productIds: string[], token?: string) => ["cart-consultation", token ?? "anonymous", productIds] as const,
  categories: ["categories"] as const,
  compatibilityRulesRoot: ["compatibility-rules"] as const,
  compatibilityRules: (token?: string) => ["compatibility-rules", token ?? "anonymous"] as const,
  ordersRoot: ["orders"] as const,
  orders: (token?: string) => ["orders", token ?? "anonymous"] as const,
  productRoot: ["product"] as const,
  product: (productId?: string) => ["product", productId ?? "unknown"] as const,
  productImages: (productId?: string) => ["product-images", productId ?? "unknown"] as const,
  productRecommendations: (productId?: string) => ["product-recommendations", productId ?? "unknown"] as const,
  productsRoot: ["products"] as const,
  products: (filters: ProductFilters) => ["products", filters] as const
};
