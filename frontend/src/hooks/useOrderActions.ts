import { useEffect, useRef, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "../api/client";
import { queryKeys } from "../api/queryKeys";
import type { Order } from "../api/types";
import { canCancelOrder, canPayOrder, orderStatusLabel } from "../utils/orderStatus";

export function useOrderActions() {
  const [errorByOrderId, setErrorByOrderId] = useState<Record<string, string>>({});
  const [pendingOrderId, setPendingOrderId] = useState<string | null>(null);
  const [processingPaymentIds, setProcessingPaymentIds] = useState<Set<string>>(() => new Set());
  const [statusOverrides, setStatusOverrides] = useState<Record<string, string>>({});
  const paymentTimers = useRef<number[]>([]);
  const queryClient = useQueryClient();

  const payOrder = useMutation({
    mutationFn: api.payOrder
  });

  const cancelOrder = useMutation({
    mutationFn: api.cancelOrder,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.ordersRoot });
    }
  });

  useEffect(() => {
    return () => {
      paymentTimers.current.forEach((timerId) => window.clearTimeout(timerId));
    };
  }, []);

  function markPaymentProcessing(orderId: string) {
    setProcessingPaymentIds((current) => new Set(current).add(orderId));
    void queryClient.invalidateQueries({ queryKey: queryKeys.ordersRoot });

    const timerId = window.setTimeout(() => {
      setProcessingPaymentIds((current) => {
        const next = new Set(current);
        next.delete(orderId);
        return next;
      });
      void queryClient.invalidateQueries({ queryKey: queryKeys.ordersRoot });
    }, 3500);

    paymentTimers.current.push(timerId);
  }

  function getEffectiveStatus(order: Order) {
    return statusOverrides[order.orderId] ?? order.status;
  }

  async function pay(order: Order) {
    setErrorByOrderId((errors) => ({ ...errors, [order.orderId]: "" }));
    const status = getEffectiveStatus(order);

    if (!canPayOrder(status)) {
      setErrorByOrderId((errors) => ({
        ...errors,
        [order.orderId]: `Нельзя оплатить заказ в статусе "${orderStatusLabel(status)}"`
      }));
      return;
    }

    setPendingOrderId(order.orderId);

    try {
      await payOrder.mutateAsync(order.orderId);
      setStatusOverrides((statuses) => ({ ...statuses, [order.orderId]: "Paid" }));
      markPaymentProcessing(order.orderId);
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
    const status = getEffectiveStatus(order);

    if (!canCancelOrder(status)) {
      setErrorByOrderId((errors) => ({
        ...errors,
        [order.orderId]: `Нельзя отменить заказ в статусе "${orderStatusLabel(status)}"`
      }));
      return;
    }

    setPendingOrderId(order.orderId);

    try {
      await cancelOrder.mutateAsync(order.orderId);
      setStatusOverrides((statuses) => ({ ...statuses, [order.orderId]: "Cancelled" }));
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
    getEffectiveStatus,
    isPaymentProcessing: (orderId: string) => processingPaymentIds.has(orderId),
    pay,
    pendingOrderId
  };
}
