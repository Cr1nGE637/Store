import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useLocation, useNavigate } from "react-router-dom";
import { api, session } from "../api/client";
import { queryKeys } from "../api/queryKeys";
import type { CheckoutRequest } from "../api/types";
import { useAdminCatalogData } from "../hooks/useAdminCatalogData";
import { useCartData } from "../hooks/useCartData";
import { useCatalogData } from "../hooks/useCatalogData";
import { useOrdersData } from "../hooks/useOrdersData";
import { viewFromPath, viewPaths } from "../types/navigation";
import type { View } from "../types/navigation";

export function useAppController() {
  const [auth, setAuth] = useState(session.get());
  const [postLoginView, setPostLoginView] = useState<View | null>(null);
  const [notice, setNotice] = useState<string | null>(null);
  const location = useLocation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const view = viewFromPath(location.pathname);

  const catalog = useCatalogData(view);
  const adminCatalog = useAdminCatalogData(auth, view);
  const cart = useCartData(auth, view, setNotice);
  const orders = useOrdersData(auth, view);

  const checkoutMutation = useMutation({
    mutationFn: api.checkout,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.cartRoot }),
        queryClient.invalidateQueries({ queryKey: queryKeys.ordersRoot })
      ]);
    }
  });

  function goTo(nextView: View) {
    navigate(viewPaths[nextView]);
  }

  async function handleLogout() {
    await api.logout();
    setAuth(null);
    orders.clearPendingCheckout();
    navigate("/");
    setNotice("Вы вышли из аккаунта");
  }

  async function handleLogin(value: ReturnType<typeof session.get>) {
    await cart.syncGuestCartToAccount();
    setAuth(value);
    const nextView = postLoginView;
    setPostLoginView(null);
    navigate(nextView ? viewPaths[nextView] : "/");
    setNotice("Вы вошли в аккаунт");
  }

  async function checkout(payload: CheckoutRequest) {
    if (!auth) {
      setPostLoginView("cart");
      navigate("/auth");
      setNotice("Войдите в аккаунт, чтобы оформить заказ");
      return;
    }

    await checkoutMutation.mutateAsync(payload);
    orders.markCheckoutPending();
    cart.clearAfterCheckout();
    navigate("/orders");
    setNotice("Заказ отправлен в обработку");
    void orders.reloadOrders();
  }

  async function handleAdminChanged() {
    await Promise.all([
      catalog.reloadProducts(),
      adminCatalog.reloadAdminProducts(),
      catalog.reloadCategories()
    ]);
    setNotice("Данные обновлены");
  }

  return {
    adminCatalog,
    auth,
    cart,
    catalog,
    checkout,
    goTo,
    handleAdminChanged,
    handleLogin,
    handleLogout,
    notice,
    orders,
    setNotice,
    view
  };
}

export type AppController = ReturnType<typeof useAppController>;
