import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { api } from "../api/client";
import { queryKeys } from "../api/queryKeys";
import type { View } from "../types/navigation";

type AuthSession = {
  email: string;
  role: string;
  token: string;
} | null;

export function useOrdersData(auth: AuthSession, view: View, onCheckoutOrderDetected?: () => void) {
  const [pendingCheckoutStartedAt, setPendingCheckoutStartedAt] = useState<number | null>(null);

  const orders = useQuery({
    queryKey: queryKeys.orders(auth?.token),
    queryFn: api.orders,
    enabled: Boolean(auth),
    refetchInterval: auth && view === "orders" ? (pendingCheckoutStartedAt ? 1000 : 3000) : false
  });

  useEffect(() => {
    if (!pendingCheckoutStartedAt || !orders.data) return;

    const hasNewOrder = orders.data.some((order) => {
      const createdAt = new Date(order.createdAt).getTime();
      return Number.isFinite(createdAt) && createdAt >= pendingCheckoutStartedAt - 5000;
    });

    if (hasNewOrder) {
      setPendingCheckoutStartedAt(null);
      onCheckoutOrderDetected?.();
    }
  }, [onCheckoutOrderDetected, orders.data, pendingCheckoutStartedAt]);

  return {
    clearPendingCheckout: () => setPendingCheckoutStartedAt(null),
    hasPendingCheckout: Boolean(pendingCheckoutStartedAt),
    isRefreshing: orders.isFetching,
    markCheckoutPending: () => setPendingCheckoutStartedAt(Date.now()),
    orders: orders.data ?? [],
    reloadOrders: orders.refetch
  };
}
