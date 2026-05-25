import type { Category, Product } from "../../api/types";
import { TwoColumn } from "../../components/ui/common";
import { ProductCreateForm } from "./ProductCreateForm";
import { StockReplenishForm } from "./StockReplenishForm";

type AdminViewProps = {
  categories: Category[];
  products: Product[];
  onChanged: () => Promise<unknown>;
};

export function AdminView({ categories, products, onChanged }: AdminViewProps) {
  return (
    <TwoColumn>
      <ProductCreateForm categories={categories} onChanged={onChanged} />
      <StockReplenishForm products={products} onChanged={onChanged} />
    </TwoColumn>
  );
}
