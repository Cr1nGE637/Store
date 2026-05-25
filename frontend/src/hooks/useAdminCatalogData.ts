import { useQuery } from "@tanstack/react-query";
import { api } from "../api/client";
import { queryKeys } from "../api/queryKeys";
import type { View } from "../types/navigation";

type AuthSession = {
  email: string;
  role: string;
  token: string;
} | null;

export function useAdminCatalogData(auth: AuthSession, view: View) {
  const isManager = auth?.role === "Manager";

  const adminProducts = useQuery({
    queryKey: queryKeys.adminProducts(auth?.token),
    queryFn: () => api.products({}),
    enabled: isManager,
    refetchInterval: isManager && view === "admin" ? 10000 : false
  });

  return {
    adminProducts: adminProducts.data ?? [],
    reloadAdminProducts: adminProducts.refetch
  };
}
