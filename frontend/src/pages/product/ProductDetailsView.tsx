import { ArrowLeft, ShoppingCart } from "lucide-react";
import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
import styled from "styled-components";
import { api } from "../../api/client";
import { queryKeys } from "../../api/queryKeys";
import type { Cart, Category, Product } from "../../api/types";
import { GhostButton, PrimaryButton } from "../../components/ui/buttons";
import { EmptyState, FormError, Panel, PanelHeading, SoftBadge } from "../../components/ui/common";
import { money } from "../../utils/format";
import { specificationLabel, stockText, stockTone } from "../../utils/specifications";

type ProductDetailsViewProps = {
  cart: Cart | null;
  categories: Category[];
  pendingProductId: string | null;
  onAdd: (product: Product) => Promise<void>;
};

export function ProductDetailsView({ cart, categories, pendingProductId, onAdd }: ProductDetailsViewProps) {
  const { productId } = useParams();
  const navigate = useNavigate();
  const product = useQuery({
    queryKey: queryKeys.product(productId),
    queryFn: () => {
      if (!productId) throw new Error("Product id is missing");
      return api.product(productId);
    },
    enabled: Boolean(productId)
  });
  const categoryById = useMemo(
    () => new Map(categories.map((category) => [category.categoryId, category.categoryName])),
    [categories]
  );

  if (product.isLoading) return <EmptyState>Загружаю товар...</EmptyState>;
  if (product.error) return <FormError>{product.error instanceof Error ? product.error.message : "Не удалось загрузить товар"}</FormError>;
  if (!product.data) return <EmptyState>Товар не найден</EmptyState>;

  const item = product.data;
  const specs = Object.entries(item.specifications);
  const tone = stockTone(item.isInStock, item.availableQuantity);
  const isInCart = Boolean(cart?.items.some((cartItem) => cartItem.productId === item.productId));

  return (
    <ProductPage>
      <GhostButton type="button" onClick={() => navigate(-1)}>
        <ArrowLeft size={18} /> Назад
      </GhostButton>

      <DetailsPanel>
        <ProductVisual>
          <DeviceScreen>
            <span>{item.brand.slice(0, 2).toUpperCase()}</span>
          </DeviceScreen>
        </ProductVisual>

        <ProductInfo>
          <PanelHeading>
            <div>
              <h2>{item.productName}</h2>
              <p>
                {item.brand} {item.model}
              </p>
            </div>
            <SoftBadge>{categoryById.get(item.categoryId) ?? "Электроника"}</SoftBadge>
          </PanelHeading>

          <PriceRow>
            <strong>{money.format(item.productPrice)}</strong>
            <StockLabel $stock={tone}>{stockText(item.isInStock, item.availableQuantity)}</StockLabel>
          </PriceRow>

          <Description>{item.productDescription}</Description>

          <MetaGrid>
            <div>
              <span>SKU</span>
              <strong>{item.sku}</strong>
            </div>
            <div>
              <span>Гарантия</span>
              <strong>{item.warrantyMonths} мес.</strong>
            </div>
            <div>
              <span>Бренд</span>
              <strong>{item.brand}</strong>
            </div>
            <div>
              <span>Модель</span>
              <strong>{item.model}</strong>
            </div>
          </MetaGrid>

          <PrimaryButton disabled={!item.isInStock || pendingProductId !== null} onClick={() => void onAdd(item)}>
            <ShoppingCart size={18} /> {pendingProductId === item.productId ? "Добавляю..." : isInCart ? "Добавить еще" : "В корзину"}
          </PrimaryButton>
        </ProductInfo>
      </DetailsPanel>

      <SpecsPanel>
        <PanelHeading>
          <div>
            <h2>Характеристики</h2>
            <p>Полное описание параметров товара</p>
          </div>
          <SoftBadge>{specs.length} параметров</SoftBadge>
        </PanelHeading>
        <SpecsGrid>
          {specs.map(([key, value]) => (
            <SpecLine key={key}>
              <span>{specificationLabel(key)}</span>
              <strong>{value}</strong>
            </SpecLine>
          ))}
        </SpecsGrid>
      </SpecsPanel>
    </ProductPage>
  );
}

const ProductPage = styled.section`
  display: grid;
  gap: 16px;
`;

const DetailsPanel = styled(Panel)`
  display: grid;
  grid-template-columns: minmax(280px, 0.9fr) minmax(0, 1.1fr);
  overflow: hidden;

  @media (max-width: 860px) {
    grid-template-columns: 1fr;
  }
`;

const ProductVisual = styled.div`
  display: grid;
  min-height: 430px;
  place-items: center;
  background:
    linear-gradient(135deg, rgba(116, 211, 174, 0.24), rgba(226, 104, 81, 0.18)),
    #f5f8f7;
`;

const DeviceScreen = styled.div`
  position: relative;
  display: grid;
  width: 180px;
  height: 280px;
  place-items: center;
  border: 16px solid #27373c;
  border-radius: 28px;
  background: linear-gradient(160deg, #ffffff, #c6eee0);
  box-shadow: 0 22px 42px rgba(23, 32, 38, 0.18);

  &::after {
    position: absolute;
    bottom: -9px;
    width: 48px;
    height: 5px;
    border-radius: 999px;
    background: #71817d;
    content: "";
  }

  span {
    color: #172026;
    font-size: 42px;
    font-weight: 800;
  }
`;

const ProductInfo = styled.div`
  display: grid;
  align-content: start;
  gap: 18px;
  padding: 22px;
`;

const PriceRow = styled.div`
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 12px;

  strong {
    font-size: 30px;
  }
`;

const StockLabel = styled.span<{ $stock: "ok" | "low" | "empty" }>`
  border-radius: 999px;
  padding: 6px 10px;
  color: ${({ $stock }) => {
    if ($stock === "ok") return "#16684f";
    if ($stock === "low") return "#7a4b05";
    return "#9a342b";
  }};
  background: ${({ $stock }) => {
    if ($stock === "ok") return "#e6f6ef";
    if ($stock === "low") return "#fff5df";
    return "#fff1ef";
  }};
  font-size: 14px;
  font-weight: 700;
`;

const Description = styled.p`
  margin: 0;
  color: #64736e;
  line-height: 1.55;
`;

const MetaGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;

  div {
    display: grid;
    gap: 4px;
    border-radius: 8px;
    padding: 10px;
    background: #f4f7f6;
  }

  span {
    color: #64736e;
    font-size: 12px;
  }

  strong {
    overflow-wrap: anywhere;
  }

  @media (max-width: 560px) {
    grid-template-columns: 1fr;
  }
`;

const SpecsPanel = styled(Panel)`
  display: grid;
  gap: 14px;
  padding: 18px;
`;

const SpecsGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;

const SpecLine = styled.div`
  display: flex;
  justify-content: space-between;
  gap: 16px;
  border-bottom: 1px solid #e4ebe8;
  padding: 10px 0;

  span {
    color: #64736e;
  }

  strong {
    text-align: right;
    overflow-wrap: anywhere;
  }
`;
