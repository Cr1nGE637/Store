import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { api } from "../api/client";
import { queryKeys } from "../api/queryKeys";
import type { ProductFilters } from "../api/types";
import type { View } from "../types/navigation";

export function useCatalogData(view: View) {
  const [filters, setFilters] = useState<ProductFilters>({ inStockOnly: true });

  const categories = useQuery({
    queryKey: queryKeys.categories,
    queryFn: api.categories
  });

  const products = useQuery({
    queryKey: queryKeys.products(filters),
    queryFn: () => api.products(filters),
    refetchInterval: view === "shop" ? 15000 : false
  });

  const brands = useMemo(
    () => Array.from(new Set((products.data ?? []).map((product) => product.brand))).sort(),
    [products.data]
  );

  return {
    brands,
    categories: categories.data ?? [],
    error: products.error instanceof Error ? products.error.message : categories.error instanceof Error ? categories.error.message : null,
    filters,
    isProductsLoading: products.isLoading,
    isProductsRefreshing: products.isFetching && !products.isLoading,
    products: products.data ?? [],
    reloadCategories: categories.refetch,
    reloadProducts: products.refetch,
    setFilters
  };
}
