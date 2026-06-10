import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { api, session } from "../api/client";
import { queryKeys } from "../api/queryKeys";
import type { Cart } from "../api/types";

export function useCartConsultation(cart: Cart | null, isAuthed: boolean) {
  const token = session.get()?.token;
  const productIds = useMemo(() => {
    const ids = cart?.items.map((item) => item.productId) ?? [];
    return Array.from(new Set(ids)).sort();
  }, [cart?.items]);

  return useQuery({
    queryKey: queryKeys.cartConsultation(productIds, token),
    queryFn: () => api.checkCartCompatibility(productIds),
    enabled: isAuthed && productIds.length > 0,
    staleTime: 30_000
  });
}
