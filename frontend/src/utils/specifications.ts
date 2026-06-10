export const specificationOptions = [
  { key: "socket", label: "Socket", placeholder: "AM4, LGA1700" },
  { key: "memoryType", label: "Memory type", placeholder: "DDR4, DDR5" },
  { key: "formFactor", label: "Form factor", placeholder: "ATX, Micro-ATX, M.2" },
  { key: "chipset", label: "Chipset", placeholder: "B550, Z790" },
  { key: "capacity", label: "Capacity", placeholder: "16GB, 1TB, 750W" },
  { key: "powerConsumptionWatts", label: "Power, W", placeholder: "750" },
  { key: "recommendedPsuWatts", label: "Recommended PSU, W", placeholder: "650" },
  { key: "powerWatts", label: "Мощность, Вт", placeholder: "20, 65, 120" },
  { key: "requiredChargerWatts", label: "Требуемая зарядка, Вт", placeholder: "20, 45, 65" },
  { key: "connectorType", label: "Connector", placeholder: "PCIe 8-pin, USB-C" },
  { key: "deviceModel", label: "Device model", placeholder: "iPhone 15, ThinkPad T14" },
  { key: "compatibleDeviceModel", label: "Compatible device", placeholder: "iPhone 15" },
  { key: "compatibilityGroup", label: "Compatibility group", placeholder: "AM4-build, mobile-usb-c" },
  { key: "deviceType", label: "Тип устройства", placeholder: "смартфон, ноутбук, монитор" },
  { key: "memory", label: "Память", placeholder: "16GB, 32GB" },
  { key: "storage", label: "Накопитель", placeholder: "512GB SSD, 1TB SSD" },
  { key: "processor", label: "Процессор", placeholder: "Apple M3, Intel Core i7" },
  { key: "screenSize", label: "Диагональ", placeholder: "6.1, 14, 27, 55" },
  { key: "resolution", label: "Разрешение", placeholder: "Full HD, 4K, 2560x1440" },
  { key: "refreshRate", label: "Частота обновления", placeholder: "60Hz, 144Hz" },
  { key: "smartTv", label: "Smart TV", placeholder: "yes, no, webOS" },
  { key: "gpu", label: "Видеокарта", placeholder: "RTX 4070, integrated" },
  { key: "batteryCapacityMah", label: "Аккумулятор, mAh", placeholder: "5000" },
  { key: "batteryLifeHours", label: "Автономность, ч", placeholder: "8, 24, 40" },
  { key: "operatingSystem", label: "ОС", placeholder: "Android, iOS, Windows" },
  { key: "connectionType", label: "Тип подключения", placeholder: "Bluetooth, USB-C, 3.5mm" },
  { key: "fastChargingStandard", label: "Быстрая зарядка", placeholder: "USB PD, Qi, MagSafe" },
  { key: "cableInputType", label: "Вход кабеля", placeholder: "USB-C, HDMI" },
  { key: "cableOutputType", label: "Выход кабеля", placeholder: "USB-C, Lightning, HDMI" },
  { key: "networkStandard", label: "Сетевой стандарт", placeholder: "Wi-Fi 6, Wi-Fi 7, 5G" },
  { key: "frequencyBand", label: "Диапазон", placeholder: "2.4GHz, 5GHz, 6GHz" },
  { key: "ports", label: "Порты", placeholder: "HDMI x2, USB-C x1" },
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
