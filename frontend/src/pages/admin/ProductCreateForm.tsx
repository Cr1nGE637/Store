import { Plus, PackagePlus, Trash2 } from "lucide-react";
import { FormEvent, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "../../api/client";
import { queryKeys } from "../../api/queryKeys";
import type { Category, ProductPayload } from "../../api/types";
import { GhostButton, PrimaryButton } from "../../components/ui/buttons";
import { FormError, SoftBadge } from "../../components/ui/common";
import { specificationOptions } from "../../utils/specifications";
import { AdminForm, FormGrid } from "./admin.styles";
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

type ProductCreateFormProps = {
  categories: Category[];
  onChanged: () => Promise<unknown>;
};

export function ProductCreateForm({ categories, onChanged }: ProductCreateFormProps) {
  const [payload, setPayload] = useState<ProductPayload>({ ...emptyProduct, categoryId: categories[0]?.categoryId ?? "" });
  const [specRows, setSpecRows] = useState<SpecRow[]>(initialSpecRows);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
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

  function applyPreset(preset: "smartphone" | "laptop" | "monitor") {
    const rowsByPreset: Record<typeof preset, SpecRow[]> = {
      smartphone: [
        { id: "deviceType", key: "deviceType", value: "смартфон" },
        { id: "memory", key: "memory", value: "8GB" },
        { id: "storage", key: "storage", value: "256GB" },
        { id: "battery", key: "battery", value: "5000 mAh" }
      ],
      laptop: [
        { id: "deviceType", key: "deviceType", value: "ноутбук" },
        { id: "processor", key: "processor", value: "Intel Core i7" },
        { id: "memory", key: "memory", value: "16GB" },
        { id: "storage", key: "storage", value: "512GB SSD" }
      ],
      monitor: [
        { id: "deviceType", key: "deviceType", value: "монитор" },
        { id: "diagonal", key: "diagonal", value: "27" },
        { id: "refreshRate", key: "refreshRate", value: "144Hz" },
        { id: "interface", key: "interface", value: "HDMI, DisplayPort" }
      ]
    };

    setSpecRows(rowsByPreset[preset]);
  }

  async function createProduct(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await createProductMutation.mutateAsync({ ...payload, specifications: parseSpecs() });
      setPayload({ ...emptyProduct, categoryId: categories[0]?.categoryId ?? "" });
      setSpecRows(initialSpecRows);
      await onChanged();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Create failed");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <AdminForm onSubmit={createProduct}>
      <h2>Новый товар</h2>
      <input value={payload.productName} onChange={(event) => setPayload({ ...payload, productName: event.target.value })} placeholder="Название" />
      <input value={payload.sku} onChange={(event) => setPayload({ ...payload, sku: event.target.value })} placeholder="SKU" />
      <input value={payload.brand} onChange={(event) => setPayload({ ...payload, brand: event.target.value })} placeholder="Бренд" />
      <input value={payload.model} onChange={(event) => setPayload({ ...payload, model: event.target.value })} placeholder="Модель" />
      <textarea value={payload.productDescription} onChange={(event) => setPayload({ ...payload, productDescription: event.target.value })} placeholder="Описание" />
      <FormGrid>
        <input value={payload.productPrice || ""} onChange={(event) => setPayload({ ...payload, productPrice: Number(event.target.value) })} placeholder="Цена" type="number" />
        <input value={payload.warrantyMonths || ""} onChange={(event) => setPayload({ ...payload, warrantyMonths: Number(event.target.value) })} placeholder="Гарантия, мес." type="number" />
      </FormGrid>
      <select value={payload.categoryId} onChange={(event) => setPayload({ ...payload, categoryId: event.target.value })}>
        <option value="">Категория</option>
        {categories.map((category) => (
          <option key={category.categoryId} value={category.categoryId}>
            {category.categoryName} ({category.categoryCode})
          </option>
        ))}
      </select>
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
            <select value={row.key} onChange={(event) => updateSpecRow(row.id, { key: event.target.value })}>
              {specificationOptions.map((option) => (
                <option key={option.key} value={option.key}>
                  {option.label}
                </option>
              ))}
            </select>
            <input value={row.value} onChange={(event) => updateSpecRow(row.id, { value: event.target.value })} placeholder="Значение" />
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
    </AdminForm>
  );
}

const SpecHeader = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding-top: 4px;
`;

const PresetRow = styled.div`
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 8px;

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
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
