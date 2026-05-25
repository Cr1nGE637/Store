import { Navigate, Route, Routes } from "react-router-dom";
import { AdminView } from "../pages/admin/AdminView";
import { AuthView } from "../pages/auth/AuthView";
import { CartView } from "../pages/cart/CartView";
import { OrdersView } from "../pages/orders/OrdersView";
import { ProductDetailsView } from "../pages/product/ProductDetailsView";
import { ShopView } from "../pages/shop/ShopView";
import type { AppController } from "./useAppController";

type AppRoutesProps = {
  app: AppController;
};

export function AppRoutes({ app }: AppRoutesProps) {
  return (
    <Routes>
      <Route
        path="/"
        element={
          <ShopView
            brands={app.catalog.brands}
            cart={app.cart.activeCart}
            categories={app.catalog.categories}
            filters={app.catalog.filters}
            error={app.catalog.error}
            isLoading={app.catalog.isProductsLoading}
            isRefreshing={app.catalog.isProductsRefreshing}
            pendingProductId={app.cart.pendingProductId}
            products={app.catalog.products}
            onAdd={app.cart.addProduct}
            onFiltersChange={app.catalog.setFilters}
          />
        }
      />

      <Route path="/auth" element={<AuthView onLogin={app.handleLogin} />} />

      <Route
        path="/products/:productId"
        element={
          <ProductDetailsView
            cart={app.cart.activeCart}
            categories={app.catalog.categories}
            pendingProductId={app.cart.pendingProductId}
            onAdd={app.cart.addProduct}
          />
        }
      />

      <Route
        path="/cart"
        element={
          <CartView
            cart={app.cart.activeCart}
            isAuthed={Boolean(app.auth)}
            onCheckout={app.checkout}
            onQuantityChange={app.cart.changeCartItemQuantity}
            onRemove={app.cart.removeCartItem}
          />
        }
      />

      <Route
        path="/orders"
        element={
          <OrdersView
            hasPendingCheckout={app.orders.hasPendingCheckout}
            isAuthed={Boolean(app.auth)}
            isRefreshing={app.orders.isRefreshing}
            orders={app.orders.orders}
          />
        }
      />

      <Route
        path="/admin"
        element={
          app.auth?.role === "Manager" ? (
            <AdminView
              categories={app.catalog.categories}
              products={app.adminCatalog.adminProducts}
              onChanged={app.handleAdminChanged}
            />
          ) : (
            <Navigate to="/" replace />
          )
        }
      />

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
