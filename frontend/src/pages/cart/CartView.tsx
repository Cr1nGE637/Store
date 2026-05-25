import { CreditCard, Trash2 } from "lucide-react";
import { FormEvent, useState } from "react";
import styled from "styled-components";
import type { Cart, CartItem, CheckoutRequest } from "../../api/types";
import { GhostButton, PrimaryButton } from "../../components/ui/buttons";
import { EmptyState, FormError, Panel, PanelHeading, SoftBadge, TwoColumn } from "../../components/ui/common";
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
  grid-template-columns: minmax(0, 1fr) minmax(110px, auto) 90px auto;
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
