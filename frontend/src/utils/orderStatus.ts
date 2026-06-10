export type OrderStatusKind = "awaitingStock" | "unpaid" | "paid" | "cancelled" | "rejected" | "unknown";

export function normalizedOrderStatus(status: string) {
  return status.trim().toLowerCase();
}

export function resolveOrderStatus(status: string): OrderStatusKind {
  const normalized = normalizedOrderStatus(status);
  const compact = normalized.replace(/[\s_-]+/g, "");

  if (compact === "awaitingstock" || normalized === "ожидает резерва") return "awaitingStock";
  if (compact === "unpaid" || normalized === "не оплачен" || normalized === "ожидает оплаты") return "unpaid";
  if (compact === "paid" || normalized === "оплачен") return "paid";
  if (compact === "cancelled" || compact === "canceled" || normalized === "отменен" || normalized === "отменён") {
    return "cancelled";
  }
  if (compact === "rejected" || normalized === "отклонен" || normalized === "отклонён") return "rejected";

  return "unknown";
}

export function isPaidOrderStatus(status: string) {
  return resolveOrderStatus(status) === "paid";
}

export function isCancelledOrderStatus(status: string) {
  const resolved = resolveOrderStatus(status);
  return resolved === "cancelled" || resolved === "rejected";
}

export function canPayOrder(status: string) {
  return resolveOrderStatus(status) === "unpaid";
}

export function canCancelOrder(status: string) {
  const resolved = resolveOrderStatus(status);
  return resolved === "awaitingStock" || resolved === "unpaid";
}

export function orderStatusTone(status: string) {
  const resolved = resolveOrderStatus(status);
  if (resolved === "paid") return "paid";
  if (resolved === "cancelled" || resolved === "rejected") return "cancelled";
  if (resolved === "awaitingStock") return "reserved";
  return "pending";
}

export function orderStatusLabel(status: string) {
  const resolved = resolveOrderStatus(status);
  if (resolved === "awaitingStock") return "Проверяем наличие";
  if (resolved === "unpaid") return "Ожидает оплаты";
  if (resolved === "paid") return "Оплачен";
  if (resolved === "cancelled") return "Отменен";
  if (resolved === "rejected") return "Отклонен";
  return status;
}
