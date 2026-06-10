import { AlertTriangle, CheckCircle2, CreditCard, Info, Trash2 } from "lucide-react";
import { FormEvent, useState } from "react";
import styled from "styled-components";
import { resolveMediaUrl } from "../../api/client";
import type { Cart, CartItem, CheckoutRequest, ConsultationResult } from "../../api/types";
import { GhostButton, PrimaryButton } from "../../components/ui/buttons";
import { EmptyState, FormError, Panel, PanelHeading, SoftBadge, TwoColumn } from "../../components/ui/common";
import { useCartConsultation } from "../../hooks/useCartConsultation";
import { money } from "../../utils/format";

const initialCheckout: CheckoutRequest = {
  recipientName: "Иван Петров",
  phone: "+7 900 111-22-33",
  deliveryAddress: "г. Москва, ул. Тверская, 12",
  deliveryMethod: "Courier",
  paymentMethod: "Card"
};

type CartViewProps = {
  cart: Cart | null;
  isAuthed: boolean;
  onCheckout: (payload: CheckoutRequest) => Promise<void>;
  onQuantityChange: (item: CartItem, quantity: number) => Promise<void>;
  onRemove: (item: CartItem) => Promise<void>;
};

export function CartView({ cart, isAuthed, onCheckout, onQuantityChange, onRemove }: CartViewProps) {
  const [checkout, setCheckout] = useState(initialCheckout);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [pendingItemId, setPendingItemId] = useState<string | null>(null);
  const total = cart?.items.reduce((sum, item) => sum + item.price * item.quantity, 0) ?? 0;
  const consultation = useCartConsultation(cart, isAuthed);

  async function submitCheckout(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await onCheckout(checkout);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Checkout failed");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function changeQuantity(item: CartItem, quantity: number) {
    setError(null);
    setPendingItemId(item.cartItemId);

    try {
      await onQuantityChange(item, quantity);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Quantity update failed");
    } finally {
      setPendingItemId(null);
    }
  }

  async function removeItem(item: CartItem) {
    setError(null);
    setPendingItemId(item.cartItemId);

    try {
      await onRemove(item);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Remove failed");
    } finally {
      setPendingItemId(null);
    }
  }

  if (!cart || cart.items.length === 0) return <EmptyState>Корзина пока пустая</EmptyState>;

  return (
    <TwoColumn>
      <ListPanel>
        <PanelHeading>
          <div>
            <h2>Состав заказа</h2>
            <p>{cart.items.length} позиций в корзине</p>
          </div>
          <SoftBadge>{money.format(total)}</SoftBadge>
        </PanelHeading>
        {cart.items.map((item) => (
          <LineItem key={item.cartItemId}>
            <CartItemVisual>
              {item.mainImage ? (
                <img src={resolveMediaUrl(item.mainImage.url)} alt={item.mainImage.altText || item.productName} />
              ) : (
                <span>{item.productName.slice(0, 2).toUpperCase()}</span>
              )}
            </CartItemVisual>
            <div>
              <strong>{item.productName}</strong>
              <span>{money.format(item.price)}</span>
            </div>
            <LineTotal>{money.format(item.price * item.quantity)}</LineTotal>
            <input
              min={1}
              type="number"
              value={item.quantity}
              disabled={pendingItemId !== null || isSubmitting}
              onChange={async (event) => {
                await changeQuantity(item, Number(event.target.value));
              }}
            />
            <GhostButton
              type="button"
              disabled={pendingItemId !== null || isSubmitting}
              onClick={async () => {
                await removeItem(item);
              }}
            >
              <Trash2 size={16} /> Удалить
            </GhostButton>
          </LineItem>
        ))}
        <TotalRow>
          <span>Итого</span>
          <strong>{money.format(total)}</strong>
        </TotalRow>
      </ListPanel>

      <CheckoutPanel onSubmit={submitCheckout}>
        <PanelHeading>
          <div>
            <h2>Оформление</h2>
            <p>{isAuthed ? "Доставка и оплата" : "Вход потребуется перед созданием заказа"}</p>
          </div>
        </PanelHeading>
        <CartConsultationStatus
          isAuthed={isAuthed}
          isLoading={consultation.isLoading || consultation.isFetching}
          error={consultation.error}
          result={consultation.data}
        />
        <CheckoutGroup>
          <strong>Получатель</strong>
          <input value={checkout.recipientName} onChange={(event) => setCheckout({ ...checkout, recipientName: event.target.value })} placeholder="Получатель" />
          <input value={checkout.phone} onChange={(event) => setCheckout({ ...checkout, phone: event.target.value })} placeholder="Телефон" />
        </CheckoutGroup>
        <CheckoutGroup>
          <strong>Доставка</strong>
          <input value={checkout.deliveryAddress} onChange={(event) => setCheckout({ ...checkout, deliveryAddress: event.target.value })} placeholder="Адрес доставки" />
          <select value={checkout.deliveryMethod} onChange={(event) => setCheckout({ ...checkout, deliveryMethod: event.target.value })}>
            <option value="Courier">Курьер</option>
            <option value="Pickup">Самовывоз</option>
          </select>
        </CheckoutGroup>
        <CheckoutGroup>
          <strong>Оплата</strong>
          <select value={checkout.paymentMethod} onChange={(event) => setCheckout({ ...checkout, paymentMethod: event.target.value })}>
            <option value="Card">Карта онлайн</option>
            <option value="CashOnDelivery">При получении</option>
          </select>
        </CheckoutGroup>
        {error && <FormError>{error}</FormError>}
        <PrimaryButton type="submit" disabled={isSubmitting || pendingItemId !== null}>
          <CreditCard size={18} /> {isSubmitting ? "Оформляю..." : isAuthed ? "Оформить заказ" : "Войти и оформить"}
        </PrimaryButton>
      </CheckoutPanel>
    </TwoColumn>
  );
}

type CartConsultationStatusProps = {
  isAuthed: boolean;
  isLoading: boolean;
  error: unknown;
  result?: ConsultationResult;
};

function CartConsultationStatus({ isAuthed, isLoading, error, result }: CartConsultationStatusProps) {
  if (!isAuthed) {
    return (
      <ConsultationBox $tone="neutral">
        <Info size={18} />
        <div>
          <strong>Консультант проверит совместимость после входа</strong>
          <span>Для гостевой корзины заказ можно собрать сейчас, а проверка запустится перед оформлением.</span>
        </div>
      </ConsultationBox>
    );
  }

  if (isLoading) {
    return (
      <ConsultationBox $tone="neutral">
        <Info size={18} />
        <div>
          <strong>Проверяю совместимость корзины</strong>
          <span>Сверяю характеристики товаров с правилами цифрового консультанта.</span>
        </div>
      </ConsultationBox>
    );
  }

  if (error) {
    return <FormError>{error instanceof Error ? error.message : "Не удалось проверить совместимость корзины"}</FormError>;
  }

  if (!result) return null;

  const tone = consultationTone(result.status);
  const hasFindings = result.findings.length > 0;

  return (
    <ConsultationBox $tone={tone}>
      {tone === "ok" ? <CheckCircle2 size={18} /> : tone === "neutral" ? <Info size={18} /> : <AlertTriangle size={18} />}
      <div>
        <strong>{consultationTitle(result.status)}</strong>
        <span>{consultationSummary(result)}</span>
        {hasFindings && (
          <FindingList>
            {result.findings.map((finding) => (
              <li key={`${finding.ruleCode}-${finding.message}`}>
                <b>{severityLabel(finding.severity)}</b>
                {finding.message}
              </li>
            ))}
          </FindingList>
        )}
        {result.recommendations.length > 0 && (
          <RecommendationList>
            {result.recommendations.map((recommendation) => (
              <li key={`${recommendation.productId}-${recommendation.type}`}>{recommendation.reason}</li>
            ))}
          </RecommendationList>
        )}
      </div>
    </ConsultationBox>
  );
}

function consultationTone(status: string): "ok" | "warning" | "error" | "neutral" {
  const normalized = status.toLowerCase();
  if (normalized === "ok") return "ok";
  if (normalized === "warning") return "warning";
  if (normalized === "error") return "error";
  return "neutral";
}

function consultationTitle(status: string) {
  const normalized = status.toLowerCase();
  if (normalized === "ok") return "Товары совместимы";
  if (normalized === "warning") return "Есть предупреждения консультанта";
  if (normalized === "error") return "Консультант нашел проблему";
  return "Для проверки не хватает данных";
}

function consultationSummary(result: ConsultationResult) {
  if (result.findings.length > 0) {
    return `${result.findings.length} замечаний по ${result.items.length} товарам. Checkout доступен, но лучше проверить подбор.`;
  }

  if (result.status.toLowerCase() === "ok") {
    return "Критичных конфликтов по характеристикам не найдено.";
  }

  return "Часть характеристик отсутствует, поэтому консультант не может дать полный вывод.";
}

function severityLabel(severity: string) {
  const normalized = severity.toLowerCase();
  if (normalized === "error") return "Проблема";
  if (normalized === "warning") return "Предупреждение";
  if (normalized === "unknown") return "Нет данных";
  return "Информация";
}

const ListPanel = styled(Panel)`
  display: grid;
  gap: 12px;
  padding: 16px;
`;

const CheckoutPanel = styled(Panel).attrs({ as: "form" })`
  display: grid;
  gap: 12px;
  align-content: start;
  padding: 16px;
`;

const CheckoutGroup = styled.div`
  display: grid;
  gap: 8px;

  strong {
    font-size: 14px;
  }
`;

const LineItem = styled.div`
  display: grid;
  grid-template-columns: 74px minmax(0, 1fr) minmax(110px, auto) 90px auto;
  gap: 10px;
  align-items: center;
  padding: 12px;
  border-bottom: 1px solid #e4ebe8;
  border-radius: 8px;
  background: #f8faf9;

  span {
    display: block;
    margin-top: 4px;
    color: #64736e;
  }

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;

const CartItemVisual = styled.div`
  display: grid;
  width: 74px;
  aspect-ratio: 1;
  place-items: center;
  overflow: hidden;
  border: 1px solid #d8e0de;
  border-radius: 8px;
  background: #ffffff;
  color: #3f4d49;
  font-weight: 800;

  img {
    width: 100%;
    height: 100%;
    object-fit: contain;
    padding: 6px;
  }

  @media (max-width: 760px) {
    width: 92px;
  }
`;

const LineTotal = styled.strong`
  justify-self: end;
  white-space: nowrap;

  @media (max-width: 760px) {
    justify-self: start;
  }
`;

const TotalRow = styled.div`
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding: 16px 4px 0;
  font-size: 20px;
`;

const ConsultationBox = styled.div<{ $tone: "ok" | "warning" | "error" | "neutral" }>`
  display: grid;
  grid-template-columns: auto minmax(0, 1fr);
  gap: 10px;
  border: 1px solid ${({ $tone }) => {
    if ($tone === "ok") return "#b9dfce";
    if ($tone === "warning") return "#efd59b";
    if ($tone === "error") return "#f0b8ae";
    return "#cad5d2";
  }};
  border-radius: 8px;
  padding: 11px 12px;
  color: ${({ $tone }) => {
    if ($tone === "ok") return "#16684f";
    if ($tone === "warning") return "#7a4b05";
    if ($tone === "error") return "#9a342b";
    return "#3f4d49";
  }};
  background: ${({ $tone }) => {
    if ($tone === "ok") return "#e6f6ef";
    if ($tone === "warning") return "#fff5df";
    if ($tone === "error") return "#fff1ef";
    return "#f4f7f6";
  }};

  strong,
  span {
    display: block;
  }

  span {
    margin-top: 3px;
    line-height: 1.4;
  }
`;

const FindingList = styled.ul`
  display: grid;
  gap: 6px;
  margin: 10px 0 0;
  padding: 0;
  list-style: none;

  li {
    display: grid;
    gap: 3px;
    line-height: 1.35;
  }
`;

const RecommendationList = styled.ul`
  display: grid;
  gap: 6px;
  margin: 10px 0 0;
  padding-left: 18px;
  line-height: 1.35;
`;
