import { Filter, Info, Search, ShoppingCart } from "lucide-react";
import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import styled from "styled-components";
import type { Cart, Category, Product, ProductFilters } from "../../api/types";
import { GhostButton, PrimaryButton } from "../../components/ui/buttons";
import { CardHeading, EmptyState, FormError, MetaRow, Panel, PanelHeading, SoftBadge } from "../../components/ui/common";
import { money } from "../../utils/format";
import { specificationLabel, specificationOptions, specificationPlaceholder, stockText, stockTone } from "../../utils/specifications";

type ShopViewProps = {
  brands: string[];
  cart: Cart | null;
  categories: Category[];
  error: string | null;
  filters: ProductFilters;
  isLoading: boolean;
  isRefreshing: boolean;
  pendingProductId: string | null;
  products: Product[];
  onAdd: (product: Product) => Promise<void>;
  onFiltersChange: (filters: ProductFilters) => void;
};

export function ShopView({ brands, cart, categories, error, filters, isLoading, isRefreshing, pendingProductId, products, onAdd, onFiltersChange }: ShopViewProps) {
  const [expandedProductId, setExpandedProductId] = useState<string | null>(null);
  const navigate = useNavigate();
  const cartProductIds = new Set(cart?.items.map((item) => item.productId) ?? []);
  const categoryById = useMemo(
    () => new Map(categories.map((category) => [category.categoryId, category.categoryName])),
    [categories]
  );

  return (
    <>
      <FiltersPanel>
        <PanelHeading>
          <div>
            <h2>Каталог</h2>
            <p>{isRefreshing ? "Обновляю данные..." : "Подобрано товаров"}</p>
          </div>
          <SoftBadge>{products.length} поз.</SoftBadge>
        </PanelHeading>

        <FilterBar>
          <SearchField>
            <Search size={18} />
            <input
              value={filters.search ?? ""}
              onChange={(event) => onFiltersChange({ ...filters, search: event.target.value })}
              placeholder="Название, SKU, бренд, модель"
            />
          </SearchField>
          <select value={filters.categoryId ?? ""} onChange={(event) => onFiltersChange({ ...filters, categoryId: event.target.value })}>
            <option value="">Все категории</option>
            {categories.map((category) => (
              <option key={category.categoryId} value={category.categoryId}>
                {category.categoryName}
              </option>
            ))}
          </select>
          <select value={filters.brand ?? ""} onChange={(event) => onFiltersChange({ ...filters, brand: event.target.value })}>
            <option value="">Все бренды</option>
            {brands.map((brand) => (
              <option key={brand} value={brand}>
                {brand}
              </option>
            ))}
          </select>
          <input value={filters.minPrice ?? ""} onChange={(event) => onFiltersChange({ ...filters, minPrice: event.target.value })} placeholder="Цена от" />
          <input value={filters.maxPrice ?? ""} onChange={(event) => onFiltersChange({ ...filters, maxPrice: event.target.value })} placeholder="Цена до" />
          <ToggleField>
            <input checked={Boolean(filters.inStockOnly)} onChange={(event) => onFiltersChange({ ...filters, inStockOnly: event.target.checked })} type="checkbox" />
            В наличии
          </ToggleField>
        </FilterBar>

        <SpecFilter>
          <Filter size={18} />
          <select
            value={filters.specificationKey ?? ""}
            onChange={(event) => onFiltersChange({ ...filters, specificationKey: event.target.value })}
          >
            <option value="">Любая характеристика</option>
            {specificationOptions.map((option) => (
              <option key={option.key} value={option.key}>
                {option.label}
              </option>
            ))}
          </select>
          <input
            value={filters.specificationValue ?? ""}
            onChange={(event) => onFiltersChange({ ...filters, specificationValue: event.target.value })}
            placeholder={specificationPlaceholder(filters.specificationKey)}
          />
        </SpecFilter>
      </FiltersPanel>

      {error ? <FormError>{error}</FormError> : null}
      {isLoading ? <EmptyState>Загружаю каталог...</EmptyState> : null}
      {!isLoading && products.length === 0 ? <EmptyState>Товары не найдены</EmptyState> : null}

      <ProductGrid>
        {products.map((product) => {
          const isExpanded = expandedProductId === product.productId;
          const specs = Object.entries(product.specifications);
          const tone = stockTone(product.isInStock, product.availableQuantity);

          return (
            <ProductCard
              key={product.productId}
              role="link"
              tabIndex={0}
              onClick={() => navigate(`/products/${product.productId}`)}
              onKeyDown={(event) => {
                if (event.key === "Enter" || event.key === " ") {
                  event.preventDefault();
                  navigate(`/products/${product.productId}`);
                }
              }}
            >
              <ProductVisual>
                <DeviceScreen>
                  <span>{product.brand.slice(0, 2).toUpperCase()}</span>
                </DeviceScreen>
              </ProductVisual>
              <ProductInfo>
                <CardHeading>
                  <div>
                    <strong>{product.productName}</strong>
                    <span>
                      {product.brand} {product.model}
                    </span>
                  </div>
                  <PriceBlock>
                    <b>{money.format(product.productPrice)}</b>
                    <small>{categoryById.get(product.categoryId) ?? "Электроника"}</small>
                  </PriceBlock>
                </CardHeading>
                <Description>{product.productDescription}</Description>
                <ProductMeta>
                  <span>SKU {product.sku}</span>
                  <span>Гарантия {product.warrantyMonths} мес.</span>
                  <StockLabel $stock={tone}>{stockText(product.isInStock, product.availableQuantity)}</StockLabel>
                </ProductMeta>
                <SpecList>
                  {specs.slice(0, 4).map(([key, value]) => (
                    <span key={key}>
                      <b>{specificationLabel(key)}</b> {value}
                    </span>
                  ))}
                </SpecList>
                {isExpanded && (
                  <ProductDetails>
                    <p>{product.productDescription}</p>
                    <DetailsGrid>
                      {specs.map(([key, value]) => (
                        <div key={key}>
                          <span>{specificationLabel(key)}</span>
                          <strong>{value}</strong>
                        </div>
                      ))}
                    </DetailsGrid>
                  </ProductDetails>
                )}
                <ActionRow>
                  <PrimaryButton
                    disabled={!product.isInStock || pendingProductId !== null}
                    onClick={(event) => {
                      event.stopPropagation();
                      void onAdd(product);
                    }}
                  >
                    <ShoppingCart size={18} /> {pendingProductId === product.productId ? "Добавляю..." : cartProductIds.has(product.productId) ? "Добавить еще" : "В корзину"}
                  </PrimaryButton>
                  <GhostButton
                    type="button"
                    onClick={(event) => {
                      event.stopPropagation();
                      setExpandedProductId(isExpanded ? null : product.productId);
                    }}
                  >
                    <Info size={18} /> {isExpanded ? "Скрыть" : "Подробнее"}
                  </GhostButton>
                </ActionRow>
              </ProductInfo>
            </ProductCard>
          );
        })}
      </ProductGrid>
    </>
  );
}

const FiltersPanel = styled(Panel)`
  display: grid;
  gap: 12px;
  margin-bottom: 14px;
  padding: 16px;
`;

const FilterBar = styled.section`
  display: grid;
  grid-template-columns: minmax(240px, 2fr) repeat(4, minmax(120px, 1fr)) auto;
  gap: 10px;

  @media (max-width: 1080px) {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;

const SearchField = styled.label`
  display: flex;
  align-items: center;
  gap: 8px;
  border: 1px solid #cad5d2;
  border-radius: 8px;
  padding: 0 10px;
  background: #ffffff;

  input {
    border: 0;
    box-shadow: none;
    padding-inline: 0;
  }

  @media (max-width: 1080px) {
    grid-column: 1 / -1;
  }
`;

const ToggleField = styled.label`
  display: flex;
  align-items: center;
  gap: 8px;
  min-height: 42px;
  border: 1px solid #cad5d2;
  border-radius: 8px;
  padding: 0 10px;
  background: #ffffff;
  white-space: nowrap;

  input {
    width: 18px;
  }
`;

const SpecFilter = styled.section`
  display: grid;
  grid-template-columns: auto minmax(190px, 1fr) minmax(190px, 1fr);
  align-items: center;
  gap: 10px;

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;

const ProductGrid = styled.section`
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(420px, 1fr));
  gap: 16px;

  @media (max-width: 1080px) {
    grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
  }

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;

const ProductCard = styled.article`
  display: grid;
  grid-template-columns: 150px 1fr;
  min-height: 280px;
  overflow: hidden;
  border: 1px solid #d8e0de;
  border-radius: 8px;
  background: #ffffff;
  box-shadow: 0 1px 2px rgba(23, 32, 38, 0.04);
  cursor: pointer;
  transition:
    border-color 0.16s ease,
    box-shadow 0.16s ease;

  &:hover {
    border-color: #b7c6c2;
    box-shadow: 0 12px 28px rgba(23, 32, 38, 0.08);
  }

  &:focus-visible {
    outline: 3px solid rgba(35, 132, 102, 0.16);
    outline-offset: 3px;
  }

  @media (max-width: 760px) {
    grid-template-columns: 110px 1fr;
  }

  @media (max-width: 520px) {
    grid-template-columns: 1fr;
  }
`;

const ProductVisual = styled.div`
  display: grid;
  place-items: center;
  background:
    linear-gradient(135deg, rgba(116, 211, 174, 0.24), rgba(226, 104, 81, 0.18)),
    #f5f8f7;

  @media (max-width: 760px) {
    min-height: 100%;
  }
`;

const DeviceScreen = styled.div`
  position: relative;
  display: grid;
  width: 70px;
  height: 110px;
  place-items: center;
  border: 8px solid #27373c;
  border-radius: 14px;
  background: linear-gradient(160deg, #ffffff, #c6eee0);
  box-shadow: 0 10px 24px rgba(23, 32, 38, 0.18);

  &::after {
    position: absolute;
    bottom: -5px;
    width: 20px;
    height: 3px;
    border-radius: 999px;
    background: #71817d;
    content: "";
  }

  span {
    color: #172026;
    font-size: 18px;
    font-weight: 800;
  }
`;

const ProductInfo = styled.div`
  display: grid;
  gap: 12px;
  align-content: start;
  padding: 16px;
`;

const PriceBlock = styled.div`
  display: grid;
  justify-items: end;
  gap: 4px;
  text-align: right;

  small {
    color: #64736e;
    font-size: 12px;
  }

  @media (max-width: 760px) {
    justify-items: start;
    text-align: left;
  }
`;

const Description = styled.p`
  display: -webkit-box;
  margin: 0;
  overflow: hidden;
  color: #64736e;
  font-size: 14px;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 3;
`;

const StockLabel = styled.span<{ $stock: "ok" | "low" | "empty" }>`
  color: ${({ $stock }) => {
    if ($stock === "ok") return "#16684f";
    if ($stock === "low") return "#7a4b05";
    return "#9a342b";
  }};
  background: ${({ $stock }) => {
    if ($stock === "ok") return "#e6f6ef";
    if ($stock === "low") return "#fff5df";
    return "#fff1ef";
  }} !important;
  font-weight: 700;
`;

const ProductMeta = styled(MetaRow)`
  align-self: start;
`;

const SpecList = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 6px;

  span {
    border-radius: 8px;
    padding: 4px 8px;
    color: #3f4d49;
    background: #eef2f3;
    font-size: 12px;

    b {
      margin-right: 4px;
      color: #172026;
    }
  }
`;

const ProductDetails = styled.div`
  display: grid;
  gap: 10px;
  border-top: 1px solid #e4ebe8;
  padding-top: 12px;

  p {
    margin: 0;
    color: #64736e;
    font-size: 14px;
    line-height: 1.45;
  }
`;

const DetailsGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 8px;

  div {
    display: grid;
    gap: 3px;
    border-radius: 8px;
    padding: 8px 10px;
    background: #f4f7f6;
  }

  span {
    color: #64736e;
    font-size: 12px;
  }

  strong {
    font-size: 13px;
    overflow-wrap: anywhere;
  }

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;

const ActionRow = styled.div`
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 8px;
  align-self: end;

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;
