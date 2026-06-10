import type {
  Cart,
  Category,
  CheckoutRequest,
  CheckoutResult,
  CompatibilityRule,
  CompatibilityRulePayload,
  ConsultationResult,
  LoginResponse,
  Order,
  Product,
  ProductFilters,
  ProductImage,
  ProductImportPreview,
  ProductImportResult,
  ProductPayload,
  ProductRecommendation,
  RegisterResponse
} from "./types";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "/api";
const TOKEN_KEY = "store.auth.token";
const ROLE_KEY = "store.auth.role";
const EMAIL_KEY = "store.auth.email";

export function resolveMediaUrl(url: string) {
  if (!url || /^[a-z][a-z0-9+.-]*:/i.test(url) || url.startsWith("//")) {
    return url;
  }

  const normalizedBase = API_BASE_URL.replace(/\/+$/, "");
  const normalizedUrl = url.startsWith("/") ? url : `/${url}`;
  return `${normalizedBase}${normalizedUrl}`;
}

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
  const isFormData = options.body instanceof FormData;

  if (options.body !== undefined && !isFormData) {
    headers.set("Content-Type", "application/json");
  }

  if (options.auth !== false && token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const body: BodyInit | undefined = options.body === undefined
    ? undefined
    : isFormData
      ? options.body as FormData
      : JSON.stringify(options.body);

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers,
    credentials: "include",
    body
  });

  if (!response.ok) {
    if (response.status === 401) {
      clearSession();
      throw new Error("Сессия истекла или токен не подошел. Войдите заново под менеджером.");
    }

    if (response.status === 403) {
      throw new Error("Для этого действия нужна роль менеджера.");
    }

    const message = await response.text();
    throw new Error(extractErrorMessage(message, response.status));
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

async function requestBlob(path: string, options: RequestOptions = {}) {
  const headers = new Headers(options.headers);
  const token = getToken();
  const { auth: _auth, body: _body, ...fetchOptions } = options;

  if (options.auth !== false && token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...fetchOptions,
    headers,
    credentials: "include"
  });

  if (!response.ok) {
    if (response.status === 401) {
      clearSession();
      throw new Error("Сессия истекла или токен не подошел. Войдите заново под менеджером.");
    }

    if (response.status === 403) {
      throw new Error("Для этого действия нужна роль менеджера.");
    }

    const message = await response.text();
    throw new Error(extractErrorMessage(message, response.status));
  }

  return {
    blob: await response.blob(),
    fileName: fileNameFromContentDisposition(response.headers.get("content-disposition"))
  };
}

function extractErrorMessage(responseText: string, status: number) {
  if (!responseText) {
    return `Request failed: ${status}`;
  }

  try {
    const parsed = JSON.parse(responseText) as unknown;

    if (typeof parsed === "string") {
      return parsed;
    }

    if (!parsed || typeof parsed !== "object") {
      return responseText;
    }

    const problem = parsed as {
      title?: string;
      detail?: string;
      message?: string;
      errors?: Record<string, string[] | string>;
    };

    if (problem.detail) {
      return problem.detail;
    }

    if (problem.message) {
      return problem.message;
    }

    if (problem.errors) {
      const firstError = Object.values(problem.errors)
        .flatMap((value) => Array.isArray(value) ? value : [value])
        .find(Boolean);
      if (firstError) {
        return firstError;
      }
    }

    if (problem.title) {
      return problem.title;
    }
  } catch {
    return responseText;
  }

  return responseText;
}

function fileNameFromContentDisposition(contentDisposition: string | null) {
  if (!contentDisposition) {
    return undefined;
  }

  const utf8Match = /filename\*=UTF-8''([^;]+)/i.exec(contentDisposition);
  if (utf8Match?.[1]) {
    return decodeURIComponent(utf8Match[1]);
  }

  const asciiMatch = /filename="?([^";]+)"?/i.exec(contentDisposition);
  return asciiMatch?.[1];
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

  uploadProductImage(productId: string, file: File, altText?: string, isMain = true) {
    const form = new FormData();
    form.append("file", file);
    form.append("isMain", String(isMain));
    if (altText) {
      form.append("altText", altText);
    }

    return request<ProductImage>(`/Products/${productId}/images`, {
      method: "POST",
      body: form
    });
  },

  productImages(productId: string) {
    return request<ProductImage[]>(`/Products/${productId}/images`);
  },

  setMainProductImage(productId: string, imageId: string) {
    return request<ProductImage>(`/Products/${productId}/images/${imageId}/main`, {
      method: "PUT"
    });
  },

  deleteProductImage(productId: string, imageId: string) {
    return request<void>(`/Products/${productId}/images/${imageId}`, {
      method: "DELETE"
    });
  },

  downloadProductsExport() {
    return requestBlob("/Products/export");
  },

  previewProductsImport(file: File) {
    const form = new FormData();
    form.append("file", file);

    return request<ProductImportPreview>("/Products/import/preview", {
      method: "POST",
      body: form
    });
  },

  importProducts(file: File) {
    const form = new FormData();
    form.append("file", file);

    return request<ProductImportResult>("/Products/import", {
      method: "POST",
      body: form
    });
  },

  replenish(productId: string, amount: number) {
    return request<void>("/Inventory/replenish", {
      method: "POST",
      body: { productId, amount }
    });
  },

  checkCartCompatibility(productIds: string[]) {
    return request<ConsultationResult>("/Consulting/cart/check", {
      method: "POST",
      body: { productIds }
    });
  },

  productRecommendations(productId: string, limit = 4) {
    return request<ProductRecommendation[]>(`/Consulting/products/${productId}/recommendations${toQuery({ limit })}`, {
      auth: false
    });
  },

  compatibilityRules() {
    return request<CompatibilityRule[]>("/Consulting/rules");
  },

  createCompatibilityRule(payload: CompatibilityRulePayload) {
    return request<CompatibilityRule>("/Consulting/rules", {
      method: "POST",
      body: payload
    });
  },

  updateCompatibilityRule(ruleId: string, payload: CompatibilityRulePayload) {
    return request<CompatibilityRule>(`/Consulting/rules/${ruleId}`, {
      method: "PUT",
      body: payload
    });
  },

  deleteCompatibilityRule(ruleId: string) {
    return request<void>(`/Consulting/rules/${ruleId}`, { method: "DELETE" });
  },

  testCompatibilityRule(sourceProductId: string, targetProductId: string, rule: CompatibilityRulePayload) {
    return request<ConsultationResult>("/Consulting/rules/test", {
      method: "POST",
      body: { sourceProductId, targetProductId, rule }
    });
  }
};
