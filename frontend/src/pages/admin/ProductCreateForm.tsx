import { ImagePlus, Plus, PackagePlus, Trash2 } from "lucide-react";
import { FormEvent, useEffect, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "../../api/client";
import { queryKeys } from "../../api/queryKeys";
import type { Category, ProductPayload } from "../../api/types";
import { GhostButton, PrimaryButton } from "../../components/ui/buttons";
import { FormError, SoftBadge } from "../../components/ui/common";
import { specificationOptions } from "../../utils/specifications";
import { AdminForm, AdminInlineForm, FormGrid } from "./admin.styles";
import styled from "styled-components";

const emptyProduct: ProductPayload = {
  sku: "",
  productName: "",
  productDescription: "",
  productPrice: 0,
  brand: "",
  model: "",
  warrantyMonths: 12,
  categoryId: "",
  specifications: {}
};

type SpecRow = {
  id: string;
  key: string;
  value: string;
};

const initialSpecRows: SpecRow[] = [
  { id: "memory", key: "memory", value: "16GB" },
  { id: "processor", key: "processor", value: "Apple M3" }
];
const allowedImageTypes = new Set(["image/jpeg", "image/png", "image/webp"]);
const maxImageSizeBytes = 5 * 1024 * 1024;

type ProductCreateFormProps = {
  categories: Category[];
  onChanged: () => Promise<unknown>;
  embedded?: boolean;
};

export function ProductCreateForm({ categories, onChanged, embedded = false }: ProductCreateFormProps) {
  const [payload, setPayload] = useState<ProductPayload>({ ...emptyProduct, categoryId: categories[0]?.categoryId ?? "" });
  const [specRows, setSpecRows] = useState<SpecRow[]>(initialSpecRows);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreviewUrl, setImagePreviewUrl] = useState<string | null>(null);
  const [imageAltText, setImageAltText] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const FormShell = embedded ? AdminInlineForm : AdminForm;
  const queryClient = useQueryClient();
  const createProductMutation = useMutation({
    mutationFn: api.createProduct,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.productsRoot }),
        queryClient.invalidateQueries({ queryKey: queryKeys.adminProductsRoot }),
        queryClient.invalidateQueries({ queryKey: queryKeys.categories })
      ]);
    }
  });

  useEffect(() => {
    if (!imageFile) {
      setImagePreviewUrl(null);
      return;
    }

    const nextPreviewUrl = URL.createObjectURL(imageFile);
    setImagePreviewUrl(nextPreviewUrl);
    return () => URL.revokeObjectURL(nextPreviewUrl);
  }, [imageFile]);

  function parseSpecs() {
    return Object.fromEntries(
      specRows
        .map((row) => [row.key.trim(), row.value.trim()])
        .filter(([key, value]) => key && value)
    );
  }

  function updateSpecRow(id: string, next: Partial<SpecRow>) {
    setSpecRows((rows) => rows.map((row) => (row.id === id ? { ...row, ...next } : row)));
  }

  function addSpecRow() {
    setSpecRows((rows) => [
      ...rows,
      {
        id: crypto.randomUUID(),
        key: specificationOptions[0].key,
        value: ""
      }
    ]);
  }

  function removeSpecRow(id: string) {
    setSpecRows((rows) => rows.filter((row) => row.id !== id));
  }

  function selectImage(file: File | null) {
    setError(null);
    if (file && !allowedImageTypes.has(file.type)) {
      setImageFile(null);
      setError("Поддерживаются только JPEG, PNG и WebP");
      return;
    }

    if (file && file.size > maxImageSizeBytes) {
      setImageFile(null);
      setError("Изображение не должно превышать 5 MB");
      return;
    }

    setImageFile(file);
    if (file && !imageAltText.trim()) {
      setImageAltText(`${payload.productName} ${payload.brand} ${payload.model}`.trim());
    }
  }

  function resetForm() {
    setPayload({ ...emptyProduct, categoryId: categories[0]?.categoryId ?? "" });
    setSpecRows(initialSpecRows);
    setImageFile(null);
    setImageAltText("");
  }

  function applyPreset(preset: "smartphone" | "laptop" | "monitor") {
    const rowsByPreset: Record<typeof preset, SpecRow[]> = {
      smartphone: [
        { id: "deviceModel", key: "deviceModel", value: "Galaxy S24" },
        { id: "storage", key: "storage", value: "256GB" },
        { id: "screenSize", key: "screenSize", value: "6.2" },
        { id: "batteryCapacityMah", key: "batteryCapacityMah", value: "4000" },
        { id: "connectorType", key: "connectorType", value: "USB-C" },
        { id: "requiredChargerWatts", key: "requiredChargerWatts", value: "25" },
        { id: "operatingSystem", key: "operatingSystem", value: "Android" }
      ],
      laptop: [
        { id: "deviceModel", key: "deviceModel", value: "Zenbook 14" },
        { id: "processor", key: "processor", value: "Intel Core i7" },
        { id: "memory", key: "memory", value: "16GB" },
        { id: "storage", key: "storage", value: "512GB SSD" },
        { id: "screenSize", key: "screenSize", value: "14" },
        { id: "connectorType", key: "connectorType", value: "USB-C" },
        { id: "requiredChargerWatts", key: "requiredChargerWatts", value: "65" }
      ],
      monitor: [
        { id: "screenSize", key: "screenSize", value: "27" },
        { id: "resolution", key: "resolution", value: "2560x1440" },
        { id: "refreshRate", key: "refreshRate", value: "144Hz" },
        { id: "connectorType", key: "connectorType", value: "HDMI" }
      ]
    };

    setSpecRows(rowsByPreset[preset]);
  }

  async function createProduct(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const createdProduct = await createProductMutation.mutateAsync({ ...payload, specifications: parseSpecs() });

      if (imageFile) {
        try {
          await api.uploadProductImage(
            createdProduct.productId,
            imageFile,
            imageAltText.trim() || `${createdProduct.productName} ${createdProduct.brand} ${createdProduct.model}`,
            true);
          await Promise.all([
            queryClient.invalidateQueries({ queryKey: queryKeys.productsRoot }),
            queryClient.invalidateQueries({ queryKey: queryKeys.adminProductsRoot }),
            queryClient.invalidateQueries({ queryKey: queryKeys.productRoot }),
            queryClient.invalidateQueries({ queryKey: queryKeys.productImages(createdProduct.productId) })
          ]);
        } catch (imageUploadError) {
          resetForm();
          await onChanged();
          setError(`Товар создан, но изображение не загружено: ${imageUploadError instanceof Error ? imageUploadError.message : "ошибка загрузки"}`);
          return;
        }
      }

      resetForm();
      await onChanged();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Create failed");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <FormShell onSubmit={createProduct}>
      {!embedded && <h2>Новый товар</h2>}
      <FieldLabel>
        <span>Название товара</span>
        <small>Показывается покупателю в каталоге, карточке и корзине.</small>
        <input value={payload.productName} onChange={(event) => setPayload({ ...payload, productName: event.target.value })} placeholder="Название" />
      </FieldLabel>
      <FieldLabel>
        <span>SKU</span>
        <small>Уникальный артикул для импорта, склада и поиска товара.</small>
        <input value={payload.sku} onChange={(event) => setPayload({ ...payload, sku: event.target.value })} placeholder="SKU" />
      </FieldLabel>
      <FieldLabel>
        <span>Бренд</span>
        <small>Производитель или торговая марка устройства.</small>
        <input value={payload.brand} onChange={(event) => setPayload({ ...payload, brand: event.target.value })} placeholder="Бренд" />
      </FieldLabel>
      <FieldLabel>
        <span>Модель</span>
        <small>Конкретная модель внутри линейки бренда.</small>
        <input value={payload.model} onChange={(event) => setPayload({ ...payload, model: event.target.value })} placeholder="Модель" />
      </FieldLabel>
      <FieldLabel>
        <span>Описание</span>
        <small>Коротко объясняет назначение и преимущества товара.</small>
        <textarea value={payload.productDescription} onChange={(event) => setPayload({ ...payload, productDescription: event.target.value })} placeholder="Описание" />
      </FieldLabel>
      <FormGrid>
        <FieldLabel>
          <span>Цена</span>
          <small>Текущая цена продажи в рублях.</small>
          <input value={payload.productPrice || ""} onChange={(event) => setPayload({ ...payload, productPrice: Number(event.target.value) })} placeholder="Цена" type="number" />
        </FieldLabel>
        <FieldLabel>
          <span>Гарантия</span>
          <small>Срок гарантийного обслуживания в месяцах.</small>
          <input value={payload.warrantyMonths || ""} onChange={(event) => setPayload({ ...payload, warrantyMonths: Number(event.target.value) })} placeholder="Гарантия, мес." type="number" />
        </FieldLabel>
      </FormGrid>
      <FieldLabel>
        <span>Категория</span>
        <small>Влияет на фильтры каталога и правила цифрового консультанта.</small>
        <select value={payload.categoryId} onChange={(event) => setPayload({ ...payload, categoryId: event.target.value })}>
          <option value="">Категория</option>
          {categories.map((category) => (
            <option key={category.categoryId} value={category.categoryId}>
              {category.categoryName} ({category.categoryCode})
            </option>
          ))}
        </select>
      </FieldLabel>
      <ImageSection>
        <ImagePreview>
          {imagePreviewUrl ? (
            <img src={imagePreviewUrl} alt="Предпросмотр изображения товара" />
          ) : (
            <ImagePlaceholder>
              <ImagePlus size={22} />
              <span>Изображение не выбрано</span>
            </ImagePlaceholder>
          )}
        </ImagePreview>
        <ImageControls>
          <FieldLabel>
            <span>Файл изображения</span>
            <small>JPEG, PNG или WebP до 5 MB.</small>
            <input
              type="file"
              accept="image/jpeg,image/png,image/webp"
              disabled={isSubmitting}
              onChange={(event) => selectImage(event.target.files?.[0] ?? null)}
            />
          </FieldLabel>
          <FieldLabel>
            <span>Описание изображения</span>
            <small>Текст для доступности и подписи, если картинка не загрузится.</small>
            <input
              value={imageAltText}
              onChange={(event) => setImageAltText(event.target.value)}
              placeholder="Описание изображения"
              disabled={isSubmitting}
            />
          </FieldLabel>
        </ImageControls>
      </ImageSection>
      <SpecHeader>
        <strong>Характеристики</strong>
        <SoftBadge>{specRows.length} строк</SoftBadge>
      </SpecHeader>
      <PresetRow>
        <GhostButton type="button" onClick={() => applyPreset("smartphone")}>Смартфон</GhostButton>
        <GhostButton type="button" onClick={() => applyPreset("laptop")}>Ноутбук</GhostButton>
        <GhostButton type="button" onClick={() => applyPreset("monitor")}>Монитор</GhostButton>
      </PresetRow>
      <SpecRows>
        {specRows.map((row) => (
          <SpecRowGrid key={row.id}>
            <FieldLabel>
              <span>Характеристика</span>
              <small>Параметр товара для фильтров и консультанта.</small>
              <select value={row.key} onChange={(event) => updateSpecRow(row.id, { key: event.target.value })}>
                {specificationOptions.map((option) => (
                  <option key={option.key} value={option.key}>
                    {option.label}
                  </option>
                ))}
              </select>
            </FieldLabel>
            <FieldLabel>
              <span>Значение</span>
              <small>Фактическое значение выбранного параметра.</small>
              <input value={row.value} onChange={(event) => updateSpecRow(row.id, { value: event.target.value })} placeholder="Значение" />
            </FieldLabel>
            <GhostButton type="button" onClick={() => removeSpecRow(row.id)} disabled={specRows.length === 1}>
              <Trash2 size={16} />
            </GhostButton>
          </SpecRowGrid>
        ))}
      </SpecRows>
      <GhostButton type="button" onClick={addSpecRow}>
        <Plus size={16} /> Добавить характеристику
      </GhostButton>
      {error && <FormError>{error}</FormError>}
      <PrimaryButton type="submit" disabled={isSubmitting}>
        <PackagePlus size={18} /> {isSubmitting ? "Создаю..." : "Создать товар"}
      </PrimaryButton>
    </FormShell>
  );
}

const SpecHeader = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding-top: 4px;
`;

const FieldLabel = styled.label`
  display: grid;
  gap: 5px;

  span {
    color: #172026;
    font-size: 13px;
    font-weight: 800;
  }

  small {
    color: #64736e;
    font-size: 12px;
    line-height: 1.35;
  }
`;

const PresetRow = styled.div`
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 8px;

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;

const ImageSection = styled.section`
  display: grid;
  grid-template-columns: 150px minmax(0, 1fr);
  gap: 12px;
  align-items: start;
  border: 1px solid #e4ebe8;
  border-radius: 8px;
  padding: 12px;
  background: #f8faf9;

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;

const ImagePreview = styled.div`
  display: grid;
  width: 100%;
  aspect-ratio: 4 / 3;
  place-items: center;
  overflow: hidden;
  border: 1px solid #d8e0de;
  border-radius: 8px;
  background: #ffffff;

  img {
    width: 100%;
    height: 100%;
    object-fit: contain;
  }
`;

const ImagePlaceholder = styled.div`
  display: grid;
  gap: 6px;
  justify-items: center;
  color: #64736e;
  font-size: 13px;
`;

const ImageControls = styled.div`
  display: grid;
  gap: 8px;

  input[type="file"] {
    min-height: 42px;
    padding: 9px 10px;
  }
`;

const SpecRows = styled.div`
  display: grid;
  gap: 8px;
`;

const SpecRowGrid = styled.div`
  display: grid;
  grid-template-columns: minmax(140px, 0.8fr) minmax(140px, 1fr) auto;
  gap: 8px;

  button {
    min-width: 44px;
    padding-inline: 10px;
  }

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;
