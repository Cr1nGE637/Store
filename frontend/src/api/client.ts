import type {
  Cart,
  Category,
  CheckoutRequest,
  CheckoutResult,
  LoginResponse,
  Order,
  Product,
  ProductFilters,
  ProductPayload,
  RegisterResponse
} from "./types";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "/api";
const TOKEN_KEY = "store.auth.token";
const ROLE_KEY = "store.auth.role";
const EMAIL_KEY = "store.auth.email";

type RequestOptions = Omit<RequestInit, "body"> & {
  body?: unknown;
  auth?: boolean;
};

function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

function setSession(session: LoginResponse) {
  localStorage.setItem(TOKEN_KEY, session.token);
  localStorage.setItem(ROLE_KEY, session.role);
  localStorage.setItem(EMAIL_KEY, session.email);
}

function clearSession() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(ROLE_KEY);
  localStorage.removeItem(EMAIL_KEY);
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers = new Headers(options.headers);
  const token = getToken();

  if (options.body !== undefined) {
    headers.set("Content-Type", "application/json");
  }

  if (options.auth !== false && token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers,
    credentials: "include",
    body: options.body === undefined ? undefined : JSON.stringify(options.body)
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed: ${response.status}`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const text = await response.text();
  if (!text) {
    return undefined as T;
  }

  return JSON.parse(text) as T;
}

function toQuery(params: Record<string, string | number | boolean | undefined>) {
  const query = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== "") {
      query.set(key, String(value));
    }
  });
  const queryString = query.toString();
  return queryString ? `?${queryString}` : "";
}

function productsQuery(filters: ProductFilters) {
  const baseQuery = toQuery({
    page: 1,
    pageSize: 60,
    Search: filters.search,
    CategoryId: filters.categoryId,
    Brand: filters.brand,
    MinPrice: filters.minPrice,
    MaxPrice: filters.maxPrice,
    InStockOnly: filters.inStockOnly || undefined
  });

  if (!filters.specificationKey || !filters.specificationValue) {
    return baseQuery;
  }

  const separator = baseQuery ? "&" : "?";
  return `${baseQuery}${separator}SpecificationFilters[${encodeURIComponent(filters.specificationKey)}]=${encodeURIComponent(filters.specificationValue)}`;
}

export const session = {
  get() {
    const token = getToken();
    if (!token) return null;
    return {
      token,
      role: localStorage.getItem(ROLE_KEY) ?? "Customer",
      email: localStorage.getItem(EMAIL_KEY) ?? ""
    };
  },
  clear: clearSession
};

export const api = {
  async login(email: string, password: string) {
    const result = await request<LoginResponse>("/Users/login", {
      method: "POST",
      auth: false,
      body: { email, password }
    });
    setSession(result);
    return result;
  },

  async register(name: string, email: string, password: string) {
    return request<RegisterResponse>("/Users/register", {
      method: "POST",
      auth: false,
      body: { name, email, password }
    });
  },

  async logout() {
    try {
      await request<void>("/Users/logout", { method: "POST" });
    } finally {
      clearSession();
    }
  },

  categories() {
    return request<Category[]>("/Categories?page=1&pageSize=100", { auth: false });
  },

  products(filters: ProductFilters) {
    return request<Product[]>(`/Products${productsQuery(filters)}`, { auth: false });
  },

  product(productId: string) {
    return request<Product>(`/Products/${productId}`, { auth: false });
  },

  cart() {
    return request<Cart>("/Cart");
  },

  addToCart(productId: string, quantity: number) {
    return request<Cart>("/Cart/items", {
      method: "POST",
      body: { productId, quantity }
    });
  },

  updateCartItem(cartItemId: string, quantity: number) {
    return request<Cart>(`/Cart/items/${cartItemId}/quantity`, {
      method: "PUT",
      body: { quantity }
    });
  },

  removeCartItem(cartItemId: string) {
    return request<Cart>(`/Cart/items/${cartItemId}`, { method: "DELETE" });
  },

  checkout(payload: CheckoutRequest) {
    return request<CheckoutResult>("/Cart/checkout", {
      method: "POST",
      body: payload
    });
  },

  orders() {
    return request<Order[]>("/Orders/my?page=1&pageSize=50");
  },

  payOrder(orderId: string) {
    return request<void>(`/Orders/${orderId}/payment`, { method: "POST" });
  },

  cancelOrder(orderId: string) {
    return request<void>(`/Orders/${orderId}/cancel`, { method: "POST" });
  },

  createProduct(payload: ProductPayload) {
    return request<Product>("/Products", {
      method: "POST",
      body: payload
    });
  },

  updateProduct(productId: string, payload: ProductPayload) {
    return request<Product>(`/Products/${productId}`, {
      method: "PUT",
      body: payload
    });
  },

  replenish(productId: string, amount: number) {
    return request<void>("/Inventory/replenish", {
      method: "POST",
      body: { productId, amount }
    });
  }
};
