import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "../api/client";
import { queryKeys } from "../api/queryKeys";
import type { Order } from "../api/types";

export function useOrderActions() {
  const [errorByOrderId, setErrorByOrderId] = useState<Record<string, string>>({});
  const [pendingOrderId, setPendingOrderId] = useState<string | null>(null);
  const queryClient = useQueryClient();

  const payOrder = useMutation({
    mutationFn: api.payOrder,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.ordersRoot });
    }
  });

  const cancelOrder = useMutation({
    mutationFn: api.cancelOrder,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.ordersRoot });
    }
  });

  async function pay(order: Order) {
    setErrorByOrderId((errors) => ({ ...errors, [order.orderId]: "" }));
    setPendingOrderId(order.orderId);

    try {
      await payOrder.mutateAsync(order.orderId);
    } catch (err) {
      setErrorByOrderId((errors) => ({
        ...errors,
        [order.orderId]: err instanceof Error ? err.message : "Не удалось оплатить заказ"
      }));
    } finally {
      setPendingOrderId(null);
    }
  }

  async function cancel(order: Order) {
    setErrorByOrderId((errors) => ({ ...errors, [order.orderId]: "" }));
    setPendingOrderId(order.orderId);

    if (order.status.toLowerCase() === "paid") {
      setErrorByOrderId((errors) => ({
        ...errors,
        [order.orderId]: "Оплаченный заказ нельзя отменить"
      }));
      setPendingOrderId(null);
      return;
    }

    try {
      await cancelOrder.mutateAsync(order.orderId);
    } catch (err) {
      setErrorByOrderId((errors) => ({
        ...errors,
        [order.orderId]: err instanceof Error ? err.message : "Не удалось отменить заказ"
      }));
    } finally {
      setPendingOrderId(null);
    }
  }

  return {
    cancel,
    errorByOrderId,
    pay,
    pendingOrderId
  };
}
