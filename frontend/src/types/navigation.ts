export type View = "shop" | "cart" | "orders" | "admin" | "auth";

export const viewPaths: Record<View, string> = {
  shop: "/",
  auth: "/auth",
  cart: "/cart",
  orders: "/orders",
  admin: "/admin"
};

export function viewFromPath(pathname: string): View {
  if (pathname.startsWith("/auth")) return "auth";
  if (pathname.startsWith("/cart")) return "cart";
  if (pathname.startsWith("/orders")) return "orders";
  if (pathname.startsWith("/admin")) return "admin";
  return "shop";
}

export function viewTitle(view: View) {
  if (view === "auth") return "Аккаунт";
  if (view === "cart") return "Корзина";
  if (view === "orders") return "История заказов";
  if (view === "admin") return "Управление магазином";
  return "Каталог электроники";
}

export function viewSubtitle(view: View) {
  if (view === "auth") return "Вход и регистрация покупателя";
  if (view === "cart") return "Проверьте позиции и оформите онлайн-заказ";
  if (view === "orders") return "Статусы, оплата и отмена заказов покупателя";
  if (view === "admin") return "Базовые менеджерские действия для демо";
  return "Поиск по бренду, категории, цене, наличию и характеристикам";
}
