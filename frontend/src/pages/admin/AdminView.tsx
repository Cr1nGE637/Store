import type { Category, Product } from "../../api/types";
import { TwoColumn } from "../../components/ui/common";
import styled from "styled-components";
import { ProductCatalogTabs } from "./ProductCatalogTabs";
import { StockReplenishForm } from "./StockReplenishForm";

type AdminViewProps = {
  categories: Category[];
  products: Product[];
  onChanged: () => Promise<unknown>;
};

export function AdminView({ categories, products, onChanged }: AdminViewProps) {
  return (
    <TwoColumn>
      <ProductCatalogTabs categories={categories} products={products} onChanged={onChanged} />
      <AdminAside>
        <StockReplenishForm products={products} onChanged={onChanged} />
      </AdminAside>
    </TwoColumn>
  );
}

const AdminAside = styled.div`
  display: grid;
  gap: 14px;
  align-content: start;
`;
