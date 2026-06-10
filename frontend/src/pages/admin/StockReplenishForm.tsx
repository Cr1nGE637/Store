import { Boxes } from "lucide-react";
import { FormEvent, useEffect, useMemo, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "../../api/client";
import { queryKeys } from "../../api/queryKeys";
import type { Product } from "../../api/types";
import { PrimaryButton } from "../../components/ui/buttons";
import { FormError } from "../../components/ui/common";
import { AdminForm, AdminInlineForm } from "./admin.styles";

type StockReplenishFormProps = {
  products: Product[];
  onChanged: () => Promise<unknown>;
  embedded?: boolean;
};

export function StockReplenishForm({ products, onChanged, embedded = false }: StockReplenishFormProps) {
  const [productId, setProductId] = useState("");
  const [amount, setAmount] = useState(10);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const FormShell = embedded ? AdminInlineForm : AdminForm;
  const queryClient = useQueryClient();
  const replenishStock = useMutation({
    mutationFn: ({ nextProductId, nextAmount }: { nextProductId: string; nextAmount: number }) => api.replenish(nextProductId, nextAmount),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.productsRoot }),
        queryClient.invalidateQueries({ queryKey: queryKeys.adminProductsRoot })
      ]);
    }
  });

  const selectedProduct = useMemo(() => products.find((product) => product.productId === productId), [products, productId]);

  useEffect(() => {
    if (products.length === 0) {
      setProductId("");
      return;
    }

    if (!productId && products.length > 0) {
      setProductId(products[0].productId);
      return;
    }

    if (productId && !products.some((product) => product.productId === productId)) {
      setProductId(products[0].productId);
    }
  }, [productId, products]);

  async function replenish(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    if (!productId) {
      setError("Выберите товар для пополнения");
      setIsSubmitting(false);
      return;
    }

    if (amount < 1) {
      setError("Количество должно быть больше нуля");
      setIsSubmitting(false);
      return;
    }

    try {
      await replenishStock.mutateAsync({ nextProductId: productId, nextAmount: amount });
      await onChanged();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Stock update failed");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <FormShell onSubmit={replenish}>
      <h2>Остатки</h2>
      <select value={productId} onChange={(event) => setProductId(event.target.value)} disabled={products.length === 0}>
        <option value="">Выберите товар</option>
        {products.map((product) => (
          <option key={product.productId} value={product.productId}>
            {product.productName} · {product.sku} · остаток: {product.availableQuantity} шт.
          </option>
        ))}
      </select>
      {selectedProduct && (
        <small>
          {selectedProduct.brand} {selectedProduct.model}, сейчас на складе: {selectedProduct.availableQuantity} шт.
        </small>
      )}
      <input value={amount} onChange={(event) => setAmount(Number(event.target.value))} min={1} type="number" />
      {error && <FormError>{error}</FormError>}
      <PrimaryButton type="submit" disabled={isSubmitting || products.length === 0 || !productId || amount < 1}>
        <Boxes size={18} /> {isSubmitting ? "Обновляю..." : "Пополнить склад"}
      </PrimaryButton>
    </FormShell>
  );
}
