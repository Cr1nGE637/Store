import { useEffect, useMemo, useRef, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "../api/client";
import { queryKeys } from "../api/queryKeys";
import type { Cart, CartItem, Product } from "../api/types";
import type { View } from "../types/navigation";

const GUEST_CART_KEY = "store.guest.cart";

type AuthSession = {
  email: string;
  role: string;
  token: string;
} | null;

function loadGuestCartItems(): CartItem[] {
  try {
    const value = localStorage.getItem(GUEST_CART_KEY);
    if (!value) return [];

    const parsed = JSON.parse(value) as CartItem[];
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

function toGuestCart(items: CartItem[]): Cart {
  return {
    cartId: "guest",
    customerId: "guest",
    isCheckoutPending: false,
    items
  };
}

export function useCartData(
  auth: AuthSession,
  view: View,
  onNotice: (message: string) => void,
  syncAfterCheckout = false,
  onCheckoutCartSynced?: () => void
) {
  const [guestCartItems, setGuestCartItems] = useState<CartItem[]>(loadGuestCartItems);
  const [pendingProductId, setPendingProductId] = useState<string | null>(null);
  const checkoutSyncStartedAt = useRef<number | null>(null);
  const queryClient = useQueryClient();
  const cartKey = queryKeys.cart(auth?.token);

  const cart = useQuery({
    queryKey: cartKey,
    queryFn: api.cart,
    enabled: Boolean(auth),
    refetchInterval: auth && (view === "cart" || syncAfterCheckout)
      ? (syncAfterCheckout ? 500 : 5000)
      : false
  });

  const addToCart = useMutation({
    mutationFn: ({ productId, quantity }: { productId: string; quantity: number }) => api.addToCart(productId, quantity),
    onSuccess: (nextCart) => {
      queryClient.setQueryData(cartKey, nextCart);
      void queryClient.invalidateQueries({ queryKey: queryKeys.cartRoot });
    }
  });

  const updateCartItem = useMutation({
    mutationFn: ({ cartItemId, quantity }: { cartItemId: string; quantity: number }) => api.updateCartItem(cartItemId, quantity),
    onSuccess: (nextCart) => {
      queryClient.setQueryData(cartKey, nextCart);
      void queryClient.invalidateQueries({ queryKey: queryKeys.cartRoot });
    }
  });

  const removeCartItemMutation = useMutation({
    mutationFn: (cartItemId: string) => api.removeCartItem(cartItemId),
    onSuccess: (nextCart) => {
      queryClient.setQueryData(cartKey, nextCart);
      void queryClient.invalidateQueries({ queryKey: queryKeys.cartRoot });
    }
  });

  const guestCart = useMemo(() => toGuestCart(guestCartItems), [guestCartItems]);
  const activeCart = auth ? cart.data ?? null : guestCart;

  useEffect(() => {
    localStorage.setItem(GUEST_CART_KEY, JSON.stringify(guestCartItems));
  }, [guestCartItems]);

  useEffect(() => {
    if (!syncAfterCheckout) {
      checkoutSyncStartedAt.current = null;
      return;
    }

    if (checkoutSyncStartedAt.current === null) {
      checkoutSyncStartedAt.current = Date.now();
    }

    if (auth) {
      void cart.refetch();
    } else {
      onCheckoutCartSynced?.();
    }
  }, [auth, cart.refetch, onCheckoutCartSynced, syncAfterCheckout]);

  useEffect(() => {
    if (!syncAfterCheckout || !auth || !cart.data || cart.isFetching) return;

    const startedAt = checkoutSyncStartedAt.current;
    if (!startedAt || cart.dataUpdatedAt < startedAt) return;

    if (!cart.data.isCheckoutPending) {
      onCheckoutCartSynced?.();
    }
  }, [auth, cart.data, cart.dataUpdatedAt, cart.isFetching, onCheckoutCartSynced, syncAfterCheckout]);

  async function syncGuestCartToAccount() {
    if (guestCartItems.length === 0) return;

    for (const item of guestCartItems) {
      await addToCart.mutateAsync({ productId: item.productId, quantity: item.quantity });
    }

    setGuestCartItems([]);
  }

  async function addProduct(product: Product) {
    if (pendingProductId) return;

    if (!auth) {
      setGuestCartItems((items) => {
        const existing = items.find((item) => item.productId === product.productId);
        if (existing) {
          return items.map((item) =>
            item.productId === product.productId
              ? { ...item, quantity: item.quantity + 1 }
              : item);
        }

        return [
          ...items,
          {
            cartItemId: product.productId,
            productId: product.productId,
            productName: product.productName,
            mainImage: product.mainImage ?? null,
            price: product.productPrice,
            quantity: 1
          }
        ];
      });
      onNotice(`${product.productName} добавлен в корзину`);
      return;
    }

    setPendingProductId(product.productId);

    try {
      await addToCart.mutateAsync({ productId: product.productId, quantity: 1 });
      onNotice(`${product.productName} добавлен в корзину`);
    } catch (err) {
      onNotice(err instanceof Error ? err.message : "Add to cart failed");
    } finally {
      setPendingProductId(null);
    }
  }

  async function changeCartItemQuantity(item: CartItem, quantity: number) {
    const normalizedQuantity = Math.max(1, quantity);

    if (!auth) {
      setGuestCartItems((items) =>
        items.map((current) =>
          current.productId === item.productId
            ? { ...current, quantity: normalizedQuantity }
            : current));
      return;
    }

    await updateCartItem.mutateAsync({ cartItemId: item.cartItemId, quantity: normalizedQuantity });
  }

  async function removeCartItem(item: CartItem) {
    if (!auth) {
      setGuestCartItems((items) => items.filter((current) => current.productId !== item.productId));
      return;
    }

    await removeCartItemMutation.mutateAsync(item.cartItemId);
  }

  return {
    activeCart,
    addProduct,
    changeCartItemQuantity,
    pendingProductId,
    removeCartItem,
    reloadCart: cart.refetch,
    syncGuestCartToAccount
  };
}
