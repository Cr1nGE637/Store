import { ImagePlus, Pencil, Plus, RotateCcw, Save, Star, Trash2 } from "lucide-react";
import { FormEvent, useEffect, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import styled from "styled-components";
import { api, resolveMediaUrl } from "../../api/client";
import { queryKeys } from "../../api/queryKeys";
import type { Category, Product, ProductPayload } from "../../api/types";
import { GhostButton, PrimaryButton } from "../../components/ui/buttons";
import { FormError, SoftBadge } from "../../components/ui/common";
import { specificationOptions } from "../../utils/specifications";
import { AdminForm, AdminInlineForm, FormGrid } from "./admin.styles";

type SpecRow = {
  id: string;
  key: string;
  value: string;
};

type ProductEditFormProps = {
  categories: Category[];
  products: Product[];
  onChanged: () => Promise<unknown>;
  embedded?: boolean;
};

const emptyPayload: ProductPayload = {
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

const allowedImageTypes = new Set(["image/jpeg", "image/png", "image/webp"]);
const maxImageSizeBytes = 5 * 1024 * 1024;

function productToPayload(product: Product): ProductPayload {
  return {
    sku: product.sku,
    productName: product.productName,
    productDescription: product.productDescription,
    productPrice: product.productPrice,
    brand: product.brand,
    model: product.model,
    warrantyMonths: product.warrantyMonths,
    categoryId: product.categoryId,
    specifications: product.specifications
  };
}

function specsToRows(specifications: Record<string, string>): SpecRow[] {
  const rows = Object.entries(specifications).map(([key, value], index) => ({
    id: `${key}-${index}`,
    key,
    value
  }));

  return rows.length > 0
    ? rows
    : [{ id: "empty-spec", key: specificationOptions[0].key, value: "" }];
}

export function ProductEditForm({ categories, products, onChanged, embedded = false }: ProductEditFormProps) {
  const [selectedProductId, setSelectedProductId] = useState("");
  const [payload, setPayload] = useState<ProductPayload>(emptyPayload);
  const [specRows, setSpecRows] = useState<SpecRow[]>(specsToRows({}));
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreviewUrl, setImagePreviewUrl] = useState<string | null>(null);
  const [imageAltText, setImageAltText] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [imageError, setImageError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isUploadingImage, setIsUploadingImage] = useState(false);
  const FormShell = embedded ? AdminInlineForm : AdminForm;
  const queryClient = useQueryClient();

  const selectedProduct = useMemo(
    () => products.find((product) => product.productId === selectedProductId),
    [products, selectedProductId]
  );

  const updateProductMutation = useMutation({
    mutationFn: ({ productId, nextPayload }: { productId: string; nextPayload: ProductPayload }) =>
      api.updateProduct(productId, nextPayload),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.productsRoot }),
        queryClient.invalidateQueries({ queryKey: queryKeys.adminProductsRoot }),
        queryClient.invalidateQueries({ queryKey: queryKeys.productRoot })
      ]);
    }
  });

  const productImages = useQuery({
    queryKey: queryKeys.productImages(selectedProductId),
    queryFn: () => api.productImages(selectedProductId),
    enabled: Boolean(selectedProductId)
  });

  const uploadImageMutation = useMutation({
    mutationFn: ({ productId, file, altText }: { productId: string; file: File; altText?: string }) =>
      api.uploadProductImage(productId, file, altText, true),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.productsRoot }),
        queryClient.invalidateQueries({ queryKey: queryKeys.adminProductsRoot }),
        queryClient.invalidateQueries({ queryKey: queryKeys.productRoot }),
        queryClient.invalidateQueries({ queryKey: queryKeys.productImages(selectedProductId) })
      ]);
    }
  });

  const setMainImageMutation = useMutation({
    mutationFn: ({ productId, imageId }: { productId: string; imageId: string }) =>
      api.setMainProductImage(productId, imageId),
    onSuccess: async () => {
      await invalidateProductImages();
    }
  });

  const deleteImageMutation = useMutation({
    mutationFn: ({ productId, imageId }: { productId: string; imageId: string }) =>
      api.deleteProductImage(productId, imageId),
    onSuccess: async () => {
      await invalidateProductImages();
    }
  });

  async function invalidateProductImages() {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: queryKeys.productsRoot }),
      queryClient.invalidateQueries({ queryKey: queryKeys.adminProductsRoot }),
      queryClient.invalidateQueries({ queryKey: queryKeys.productRoot }),
      queryClient.invalidateQueries({ queryKey: queryKeys.productImages(selectedProductId) }),
      queryClient.invalidateQueries({ queryKey: queryKeys.cartRoot })
    ]);
  }

  useEffect(() => {
    if (products.length === 0) {
      setSelectedProductId("");
      setPayload({ ...emptyPayload, categoryId: categories[0]?.categoryId ?? "" });
      setSpecRows(specsToRows({}));
      return;
    }

    if (!selectedProductId || !products.some((product) => product.productId === selectedProductId)) {
      setSelectedProductId(products[0].productId);
    }
  }, [categories, products, selectedProductId]);

  useEffect(() => {
    if (!selectedProduct) return;
    setError(null);
    setImageError(null);
    setPayload(productToPayload(selectedProduct));
    setSpecRows(specsToRows(selectedProduct.specifications));
    setImageFile(null);
    setImageAltText(selectedProduct.mainImage?.altText ?? "");
  }, [selectedProduct]);

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

  function resetForm() {
    if (!selectedProduct) return;
    setError(null);
    setImageError(null);
    setPayload(productToPayload(selectedProduct));
    setSpecRows(specsToRows(selectedProduct.specifications));
    setImageFile(null);
    setImageAltText(selectedProduct.mainImage?.altText ?? "");
  }

  function selectImage(file: File | null) {
    setImageError(null);
    if (file && !allowedImageTypes.has(file.type)) {
      setImageFile(null);
      setImageError("Поддерживаются только JPEG, PNG и WebP");
      return;
    }

    if (file && file.size > maxImageSizeBytes) {
      setImageFile(null);
      setImageError("Изображение не должно превышать 5 MB");
      return;
    }

    setImageFile(file);
    if (file && !imageAltText.trim() && selectedProduct) {
      setImageAltText(`${selectedProduct.productName} ${selectedProduct.brand} ${selectedProduct.model}`);
    }
  }

  async function saveProduct(event: FormEvent) {
    event.preventDefault();
    if (!selectedProductId) return;

    setError(null);
    setIsSubmitting(true);

    try {
      await updateProductMutation.mutateAsync({
        productId: selectedProductId,
        nextPayload: { ...payload, specifications: parseSpecs() }
      });
      await onChanged();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Update failed");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function uploadImage() {
    if (!selectedProduct || !imageFile) return;

    setImageError(null);
    setIsUploadingImage(true);

    try {
      await uploadImageMutation.mutateAsync({
        productId: selectedProduct.productId,
        file: imageFile,
        altText: imageAltText
      });
      setImageFile(null);
      await onChanged();
    } catch (err) {
      setImageError(err instanceof Error ? err.message : "Image upload failed");
    } finally {
      setIsUploadingImage(false);
    }
  }

  async function setMainImage(imageId: string) {
    if (!selectedProduct) return;

    setImageError(null);
    try {
      await setMainImageMutation.mutateAsync({
        productId: selectedProduct.productId,
        imageId
      });
      await onChanged();
    } catch (err) {
      setImageError(err instanceof Error ? err.message : "Set main image failed");
    }
  }

  async function deleteImage(imageId: string) {
    if (!selectedProduct) return;

    setImageError(null);
    try {
      await deleteImageMutation.mutateAsync({
        productId: selectedProduct.productId,
        imageId
      });
      await onChanged();
    } catch (err) {
      setImageError(err instanceof Error ? err.message : "Delete image failed");
    }
  }

  return (
    <FormShell onSubmit={saveProduct}>
      {!embedded && <h2>Редактирование товара</h2>}
      <FieldLabel>
        <span>Товар для редактирования</span>
        <small>Выберите существующий товар, чтобы изменить его данные и изображения.</small>
        <select value={selectedProductId} onChange={(event) => setSelectedProductId(event.target.value)} disabled={products.length === 0}>
          <option value="">Выберите товар</option>
          {products.map((product) => (
            <option key={product.productId} value={product.productId}>
              {product.productName} · {product.sku}
            </option>
          ))}
        </select>
      </FieldLabel>

      {selectedProduct && (
        <ProductSummary>
          <span>
            {selectedProduct.brand} {selectedProduct.model}
          </span>
          <SoftBadge>{selectedProduct.availableQuantity} шт.</SoftBadge>
        </ProductSummary>
      )}

      {selectedProduct && (
        <ImageSection>
          <ImagePreview>
            {imagePreviewUrl ? (
              <img src={imagePreviewUrl} alt="Предпросмотр изображения товара" />
            ) : selectedProduct.mainImage ? (
              <img src={resolveMediaUrl(selectedProduct.mainImage.url)} alt={selectedProduct.mainImage.altText || selectedProduct.productName} />
            ) : (
              <ImagePlaceholder>
                <ImagePlus size={22} />
                <span>Нет изображения</span>
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
                disabled={!selectedProduct || isUploadingImage}
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
                disabled={!selectedProduct || isUploadingImage}
              />
            </FieldLabel>
            <GhostButton type="button" disabled={!imageFile || isUploadingImage} onClick={() => void uploadImage()}>
              <ImagePlus size={16} /> {isUploadingImage ? "Загружаю..." : "Загрузить изображение"}
            </GhostButton>
          </ImageControls>
          {productImages.isLoading && <ImageHint>Загружаю изображения...</ImageHint>}
          {!productImages.isLoading && (productImages.data?.length ?? 0) > 0 && (
            <ImageGallery>
              {productImages.data?.map((image) => (
                <ImageTile key={image.productImageId}>
                  <img src={resolveMediaUrl(image.url)} alt={image.altText || selectedProduct.productName} />
                  <div>
                    <strong>{image.isMain ? "Основное" : image.originalFileName}</strong>
                    <span>{Math.ceil(image.sizeBytes / 1024)} KB</span>
                  </div>
                  <GhostButton
                    type="button"
                    disabled={image.isMain || setMainImageMutation.isPending || deleteImageMutation.isPending}
                    onClick={() => void setMainImage(image.productImageId)}
                  >
                    <Star size={16} />
                  </GhostButton>
                  <GhostButton
                    type="button"
                    disabled={setMainImageMutation.isPending || deleteImageMutation.isPending}
                    onClick={() => void deleteImage(image.productImageId)}
                  >
                    <Trash2 size={16} />
                  </GhostButton>
                </ImageTile>
              ))}
            </ImageGallery>
          )}
          {imageError && <FormError>{imageError}</FormError>}
        </ImageSection>
      )}

      <FieldLabel>
        <span>Название товара</span>
        <small>Показывается покупателю в каталоге, карточке и корзине.</small>
        <input value={payload.productName} onChange={(event) => setPayload({ ...payload, productName: event.target.value })} placeholder="Название" disabled={!selectedProduct} />
      </FieldLabel>
      <FieldLabel>
        <span>SKU</span>
        <small>Уникальный артикул для импорта, склада и поиска товара.</small>
        <input value={payload.sku} onChange={(event) => setPayload({ ...payload, sku: event.target.value })} placeholder="SKU" disabled={!selectedProduct} />
      </FieldLabel>
      <FieldLabel>
        <span>Бренд</span>
        <small>Производитель или торговая марка устройства.</small>
        <input value={payload.brand} onChange={(event) => setPayload({ ...payload, brand: event.target.value })} placeholder="Бренд" disabled={!selectedProduct} />
      </FieldLabel>
      <FieldLabel>
        <span>Модель</span>
        <small>Конкретная модель внутри линейки бренда.</small>
        <input value={payload.model} onChange={(event) => setPayload({ ...payload, model: event.target.value })} placeholder="Модель" disabled={!selectedProduct} />
      </FieldLabel>
      <FieldLabel>
        <span>Описание</span>
        <small>Коротко объясняет назначение и преимущества товара.</small>
        <textarea value={payload.productDescription} onChange={(event) => setPayload({ ...payload, productDescription: event.target.value })} placeholder="Описание" disabled={!selectedProduct} />
      </FieldLabel>
      <FormGrid>
        <FieldLabel>
          <span>Цена</span>
          <small>Текущая цена продажи в рублях.</small>
          <input value={payload.productPrice || ""} onChange={(event) => setPayload({ ...payload, productPrice: Number(event.target.value) })} placeholder="Цена" type="number" disabled={!selectedProduct} />
        </FieldLabel>
        <FieldLabel>
          <span>Гарантия</span>
          <small>Срок гарантийного обслуживания в месяцах.</small>
          <input value={payload.warrantyMonths || ""} onChange={(event) => setPayload({ ...payload, warrantyMonths: Number(event.target.value) })} placeholder="Гарантия, мес." type="number" disabled={!selectedProduct} />
        </FieldLabel>
      </FormGrid>
      <FieldLabel>
        <span>Категория</span>
        <small>Влияет на фильтры каталога и правила цифрового консультанта.</small>
        <select value={payload.categoryId} onChange={(event) => setPayload({ ...payload, categoryId: event.target.value })} disabled={!selectedProduct}>
          <option value="">Категория</option>
          {categories.map((category) => (
            <option key={category.categoryId} value={category.categoryId}>
              {category.categoryName} ({category.categoryCode})
            </option>
          ))}
        </select>
      </FieldLabel>

      <SpecHeader>
        <strong>Характеристики</strong>
        <SoftBadge>{specRows.length} строк</SoftBadge>
      </SpecHeader>
      <SpecRows>
        {specRows.map((row) => (
          <SpecRowGrid key={row.id}>
            <FieldLabel>
              <span>Характеристика</span>
              <small>Параметр товара для фильтров и консультанта.</small>
              <select value={row.key} onChange={(event) => updateSpecRow(row.id, { key: event.target.value })} disabled={!selectedProduct}>
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
              <input value={row.value} onChange={(event) => updateSpecRow(row.id, { value: event.target.value })} placeholder="Значение" disabled={!selectedProduct} />
            </FieldLabel>
            <GhostButton type="button" onClick={() => removeSpecRow(row.id)} disabled={!selectedProduct || specRows.length === 1}>
              <Trash2 size={16} />
            </GhostButton>
          </SpecRowGrid>
        ))}
      </SpecRows>
      <ActionRow>
        <GhostButton type="button" onClick={addSpecRow} disabled={!selectedProduct}>
          <Plus size={16} /> Добавить характеристику
        </GhostButton>
        <GhostButton type="button" onClick={resetForm} disabled={!selectedProduct || isSubmitting}>
          <RotateCcw size={16} /> Сбросить
        </GhostButton>
      </ActionRow>
      {error && <FormError>{error}</FormError>}
      <PrimaryButton type="submit" disabled={!selectedProduct || isSubmitting}>
        {isSubmitting ? <Pencil size={18} /> : <Save size={18} />}
        {isSubmitting ? "Сохраняю..." : "Сохранить изменения"}
      </PrimaryButton>
    </FormShell>
  );
}

const ProductSummary = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  color: #64736e;
  font-size: 14px;
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

const ImageHint = styled.div`
  color: #64736e;
  font-size: 13px;
`;

const ImageGallery = styled.div`
  display: grid;
  grid-column: 1 / -1;
  gap: 8px;
`;

const ImageTile = styled.div`
  display: grid;
  grid-template-columns: 56px minmax(0, 1fr) auto auto;
  gap: 8px;
  align-items: center;
  border: 1px solid #e4ebe8;
  border-radius: 8px;
  padding: 8px;
  background: #ffffff;

  img {
    width: 56px;
    aspect-ratio: 1;
    object-fit: contain;
  }

  strong,
  span {
    display: block;
  }

  span {
    color: #64736e;
    font-size: 12px;
  }

  button {
    min-width: 40px;
    padding-inline: 10px;
  }

  @media (max-width: 760px) {
    grid-template-columns: 56px minmax(0, 1fr);

    button {
      width: 100%;
    }
  }
`;

const SpecHeader = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding-top: 4px;
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

const ActionRow = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
`;
