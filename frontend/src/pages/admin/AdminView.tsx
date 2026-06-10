import { Boxes, Bot, Package } from "lucide-react";
import { useState } from "react";
import styled from "styled-components";
import type { Category, Product } from "../../api/types";
import { Panel, SoftBadge } from "../../components/ui/common";
import { ConsultingRulesPanel } from "./ConsultingRulesPanel";
import { ProductCatalogTabs } from "./ProductCatalogTabs";
import { StockReplenishForm } from "./StockReplenishForm";

type AdminViewProps = {
  categories: Category[];
  products: Product[];
  onChanged: () => Promise<unknown>;
};

type AdminTab = "products" | "stock" | "consulting";

export function AdminView({ categories, products, onChanged }: AdminViewProps) {
  const [activeTab, setActiveTab] = useState<AdminTab>("products");
  const lowStockCount = products.filter((product) => product.availableQuantity <= 3).length;

  return (
    <AdminPanel>
      <AdminHeader>
        <div>
          <h2>Управление магазином</h2>
          <p>Каталог, склад и правила цифрового консультанта в одном рабочем пространстве</p>
        </div>
        <SoftBadge>{products.length} товаров</SoftBadge>
      </AdminHeader>

      <AdminTabs role="tablist" aria-label="Разделы админки">
        <AdminTabButton type="button" role="tab" aria-selected={activeTab === "products"} $active={activeTab === "products"} onClick={() => setActiveTab("products")}>
          <Package size={16} /> Товары
        </AdminTabButton>
        <AdminTabButton type="button" role="tab" aria-selected={activeTab === "stock"} $active={activeTab === "stock"} onClick={() => setActiveTab("stock")}>
          <Boxes size={16} /> Остатки
          {lowStockCount > 0 && <Counter>{lowStockCount}</Counter>}
        </AdminTabButton>
        <AdminTabButton type="button" role="tab" aria-selected={activeTab === "consulting"} $active={activeTab === "consulting"} onClick={() => setActiveTab("consulting")}>
          <Bot size={16} /> Консультант
        </AdminTabButton>
      </AdminTabs>

      <AdminContent>
        {activeTab === "products" && (
          <ProductCatalogTabs categories={categories} products={products} onChanged={onChanged} embedded />
        )}
        {activeTab === "stock" && (
          <StockReplenishForm products={products} onChanged={onChanged} embedded />
        )}
        {activeTab === "consulting" && (
          <ConsultingRulesPanel categories={categories} products={products} embedded />
        )}
      </AdminContent>
    </AdminPanel>
  );
}

const AdminPanel = styled(Panel)`
  display: grid;
  gap: 14px;
  padding: 16px;
`;

const AdminHeader = styled.div`
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  padding-bottom: 12px;
  border-bottom: 1px solid #e4ebe8;

  h2 {
    margin: 0;
    font-size: 20px;
    line-height: 1.2;
  }

  p {
    margin: 5px 0 0;
    color: #64736e;
    font-size: 14px;
    line-height: 1.35;
  }

  @media (max-width: 760px) {
    flex-direction: column;
  }
`;

const AdminTabs = styled.div`
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 4px;
  border-radius: 8px;
  padding: 4px;
  background: #eef2f3;

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;

const AdminTabButton = styled.button<{ $active: boolean }>`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 7px;
  min-height: 38px;
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

const Counter = styled.span`
  display: inline-grid;
  min-width: 20px;
  height: 20px;
  place-items: center;
  border-radius: 999px;
  padding: 0 6px;
  color: #7a4b05;
  background: #fff5df;
  font-size: 12px;
  font-weight: 800;
`;

const AdminContent = styled.div`
  display: grid;
  min-height: 520px;
  align-content: start;
`;
