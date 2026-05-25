export const specificationOptions = [
  { key: "deviceType", label: "Тип устройства", placeholder: "смартфон, ноутбук, монитор" },
  { key: "memory", label: "Память", placeholder: "16GB, 32GB" },
  { key: "storage", label: "Накопитель", placeholder: "512GB SSD, 1TB SSD" },
  { key: "processor", label: "Процессор", placeholder: "Apple M3, Intel Core i7" },
  { key: "diagonal", label: "Диагональ", placeholder: "14, 27" },
  { key: "refreshRate", label: "Частота обновления", placeholder: "60Hz, 144Hz" },
  { key: "gpu", label: "Видеокарта", placeholder: "RTX 4070, integrated" },
  { key: "battery", label: "Аккумулятор", placeholder: "5000 mAh" },
  { key: "color", label: "Цвет", placeholder: "черный, серебристый" },
  { key: "interface", label: "Интерфейс", placeholder: "USB-C, PCIe 4.0" }
] as const;

const specificationLabels = Object.fromEntries(
  specificationOptions.map((option) => [option.key, option.label])
);

export function specificationLabel(key: string) {
  return specificationLabels[key] ?? key;
}

export function specificationPlaceholder(key?: string) {
  return specificationOptions.find((option) => option.key === key)?.placeholder ?? "Значение";
}

export function stockTone(isInStock: boolean, quantity: number) {
  if (!isInStock || quantity <= 0) return "empty";
  if (quantity <= 3) return "low";
  return "ok";
}

export function stockText(isInStock: boolean, quantity: number) {
  if (!isInStock || quantity <= 0) return "Нет в наличии";
  if (quantity <= 3) return `Мало: ${quantity} шт.`;
  return `В наличии: ${quantity} шт.`;
}
