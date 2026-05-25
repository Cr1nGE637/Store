import styled from "styled-components";
import type { Order } from "../../api/types";
import { GhostButton, PrimaryButton } from "../../components/ui/buttons";
import { CardHeading, EmptyState, FormError, MetaRow, SoftBadge } from "../../components/ui/common";
import { useOrderActions } from "../../hooks/useOrderActions";
import { money } from "../../utils/format";

type OrdersViewProps = {
  hasPendingCheckout: boolean;
  isAuthed: boolean;
  isRefreshing: boolean;
  orders: Order[];
};

export function OrdersView({ hasPendingCheckout, isAuthed, isRefreshing, orders }: OrdersViewProps) {
  const { cancel, errorByOrderId, pay, pendingOrderId } = useOrderActions();

  const checkoutNotice = hasPendingCheckout ? (
    <ProcessingNotice>
      {isRefreshing ? "Заказ принят. Обновляем историю заказов..." : "Заказ принят. Ожидаем подтверждение обработки..."}
    </ProcessingNotice>
  ) : null;

  if (!isAuthed) return <EmptyState>Войдите, чтобы увидеть историю заказов</EmptyState>;
  if (hasPendingCheckout && orders.length === 0) {
    return (
      <>
        {checkoutNotice}
        <EmptyState>Заказ создается. Список обновится автоматически.</EmptyState>
      </>
    );
  }
  if (orders.length === 0) return <EmptyState>Заказов пока нет</EmptyState>;

  return (
    <>
      {checkoutNotice}
      <OrdersList>
      {orders.map((order) => (
        <OrderCard key={order.orderId}>
          <CardHeading>
            <div>
              <strong>Заказ {order.orderId.slice(0, 8)}</strong>
              <span>{new Date(order.createdAt).toLocaleString("ru-RU")}</span>
            </div>
            <OrderSummary>
              <SoftBadge>{money.format(order.totalAmount)}</SoftBadge>
              <StatusPill $status={statusTone(order.status)}>{statusLabel(order.status)}</StatusPill>
            </OrderSummary>
          </CardHeading>
          <OrderProducts>
            {order.products.map((product) => (
              <span key={product.productId}>
                {product.productName} x {product.quantity}
              </span>
            ))}
          </OrderProducts>
          <MetaRow>
            <span>{order.recipientName}</span>
            <span>{order.phone}</span>
          </MetaRow>
          {errorByOrderId[order.orderId] && <FormError>{errorByOrderId[order.orderId]}</FormError>}
          <ActionRow>
            <PrimaryButton type="button" disabled={pendingOrderId === order.orderId} onClick={() => void pay(order)}>
              {pendingOrderId === order.orderId ? "Обновляю..." : "Оплатить"}
            </PrimaryButton>
            <GhostButton type="button" disabled={pendingOrderId === order.orderId} onClick={() => void cancel(order)}>
              Отменить
            </GhostButton>
          </ActionRow>
        </OrderCard>
      ))}
      </OrdersList>
    </>
  );
}

function statusTone(status: string) {
  const normalized = status.toLowerCase();
  if (normalized.includes("paid")) return "paid";
  if (normalized.includes("cancel")) return "cancelled";
  if (normalized.includes("reject")) return "cancelled";
  if (normalized.includes("reserve")) return "reserved";
  return "pending";
}

function statusLabel(status: string) {
  const normalized = status.toLowerCase();
  if (normalized.includes("paid")) return "Оплачен";
  if (normalized.includes("cancel")) return "Отменен";
  if (normalized.includes("reject")) return "Отклонен";
  if (normalized.includes("reserve")) return "Зарезервирован";
  if (normalized.includes("created")) return "Создан";
  return status;
}

const ProcessingNotice = styled.div`
  margin-bottom: 12px;
  border: 1px solid #b9d6cd;
  border-radius: 8px;
  padding: 12px 14px;
  color: #172026;
  background: #eef8f4;
  font-weight: 700;
`;

const OrdersList = styled.section`
  display: grid;
  gap: 12px;
`;

const OrderCard = styled.article`
  display: grid;
  gap: 12px;
  padding: 16px;
  border: 1px solid #d8e0de;
  border-radius: 8px;
  background: #ffffff;
  box-shadow: 0 1px 2px rgba(23, 32, 38, 0.04);
`;

const OrderSummary = styled.div`
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 8px;
`;

const StatusPill = styled.span<{ $status: string }>`
  align-self: flex-start;
  border-radius: 999px;
  padding: 4px 8px;
  color: ${({ $status }) => {
    if ($status === "paid") return "#16684f";
    if ($status === "cancelled") return "#9a342b";
    if ($status === "reserved") return "#5740a3";
    return "#7a4b05";
  }};
  background: ${({ $status }) => {
    if ($status === "paid") return "#e6f6ef";
    if ($status === "cancelled") return "#fff1ef";
    if ($status === "reserved") return "#f0edff";
    return "#fff5df";
  }};
  font-weight: 700;
`;

const OrderProducts = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  color: #64736e;
  font-size: 14px;

  span {
    border-radius: 8px;
    padding: 6px 8px;
    background: #f4f7f6;
    color: #3f4d49;
  }
`;

const ActionRow = styled.div`
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
`;
