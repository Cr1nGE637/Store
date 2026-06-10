import { PackagePlus, Pencil } from "lucide-react";
import { useState } from "react";
import styled from "styled-components";
import type { Category, Product } from "../../api/types";
import { Panel } from "../../components/ui/common";
import { ProductCreateForm } from "./ProductCreateForm";
import { ProductEditForm } from "./ProductEditForm";
import { ProductImportExportPanel } from "./ProductImportExportPanel";

type ProductCatalogTabsProps = {
  categories: Category[];
  products: Product[];
  onChanged: () => Promise<unknown>;
  embedded?: boolean;
};

type ProductTab = "create" | "edit";

export function ProductCatalogTabs({ categories, products, onChanged, embedded = false }: ProductCatalogTabsProps) {
  const [activeTab, setActiveTab] = useState<ProductTab>("create");
  const Shell = embedded ? ProductContent : ProductPanel;

  return (
    <Shell>
      <TabsHeader>
        <h2>Товары</h2>
        <HeaderActions>
          <Tabs role="tablist" aria-label="Управление товарами">
            <TabButton type="button" role="tab" aria-selected={activeTab === "create"} $active={activeTab === "create"} onClick={() => setActiveTab("create")}>
              <PackagePlus size={16} /> Создание
            </TabButton>
            <TabButton type="button" role="tab" aria-selected={activeTab === "edit"} $active={activeTab === "edit"} onClick={() => setActiveTab("edit")}>
              <Pencil size={16} /> Редактирование
            </TabButton>
          </Tabs>
        </HeaderActions>
      </TabsHeader>
      <ProductImportExportPanel onChanged={onChanged} />

      {activeTab === "create" ? (
        <ProductCreateForm categories={categories} onChanged={onChanged} embedded />
      ) : (
        <ProductEditForm categories={categories} products={products} onChanged={onChanged} embedded />
      )}
    </Shell>
  );
}

const ProductPanel = styled(Panel)`
  display: grid;
  gap: 14px;
  align-content: start;
  padding: 16px;
`;

const ProductContent = styled.div`
  display: grid;
  gap: 14px;
  align-content: start;
`;

const TabsHeader = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding-bottom: 12px;
  border-bottom: 1px solid #e4ebe8;

  h2 {
    margin: 0;
    font-size: 20px;
    line-height: 1.2;
  }

  @media (max-width: 760px) {
    align-items: stretch;
    flex-direction: column;
  }
`;

const Tabs = styled.div`
  display: inline-grid;
  grid-template-columns: 1fr 1fr;
  gap: 4px;
  border-radius: 8px;
  padding: 4px;
  background: #eef2f3;
`;

const HeaderActions = styled.div`
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 8px;

  @media (max-width: 760px) {
    justify-content: stretch;

    > button {
      width: 100%;
    }
  }
`;

const TabButton = styled.button<{ $active: boolean }>`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 7px;
  min-height: 36px;
  border: 0;
  border-radius: 6px;
  padding: 8px 11px;
  color: ${({ $active }) => ($active ? "#ffffff" : "#3f4d49")};
  background: ${({ $active }) => ($active ? "#238466" : "transparent")};
  font-weight: 700;
  white-space: nowrap;
  transition:
    background 0.16s ease,
    color 0.16s ease;

  &:hover {
    background: ${({ $active }) => ($active ? "#1c6d55" : "#dfe8e5")};
  }

  &:focus-visible {
    outline: 3px solid rgba(35, 132, 102, 0.18);
    outline-offset: 2px;
  }
`;
