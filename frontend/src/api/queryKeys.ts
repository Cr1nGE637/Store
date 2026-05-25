import type { ProductFilters } from "./types";

export const queryKeys = {
  adminProductsRoot: ["admin-products"] as const,
  adminProducts: (token?: string) => ["admin-products", token ?? "anonymous"] as const,
  cartRoot: ["cart"] as const,
  cart: (token?: string) => ["cart", token ?? "anonymous"] as const,
  categories: ["categories"] as const,
  ordersRoot: ["orders"] as const,
  orders: (token?: string) => ["orders", token ?? "anonymous"] as const,
  productRoot: ["product"] as const,
  product: (productId?: string) => ["product", productId ?? "unknown"] as const,
  productsRoot: ["products"] as const,
  products: (filters: ProductFilters) => ["products", filters] as const
};
