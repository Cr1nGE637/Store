import { Boxes, CreditCard, LayoutDashboard, LogIn, LogOut, ShoppingCart, UserRound } from "lucide-react";
import { ReactNode } from "react";
import styled from "styled-components";
import { session } from "../../api/client";
import type { View } from "../../types/navigation";
import { viewSubtitle, viewTitle } from "../../types/navigation";
import { SidebarGhostButton } from "../ui/buttons";

type AuthState = ReturnType<typeof session.get>;

type ShellProps = {
  auth: AuthState;
  cartCount: number;
  children: ReactNode;
  notice: string | null;
  view: View;
  onDismissNotice: () => void;
  onLogout: () => void;
  onViewChange: (view: View) => void;
};

export function Shell({ auth, cartCount, children, notice, view, onDismissNotice, onLogout, onViewChange }: ShellProps) {
  const isManager = auth?.role === "Manager";

  return (
    <AppShell>
      <Sidebar>
        <BrandBlock>
          <BrandMark>SE</BrandMark>
          <div>
            <strong>Store Electronics</strong>
            <span>онлайн-магазин</span>
          </div>
        </BrandBlock>

        <NavList>
          <NavButton type="button" data-active={view === "shop"} onClick={() => onViewChange("shop")}>
            <LayoutDashboard size={18} /> Каталог
          </NavButton>
          <NavButton type="button" data-active={view === "cart"} onClick={() => onViewChange("cart")}>
            <ShoppingCart size={18} /> Корзина
            <Count>{cartCount}</Count>
          </NavButton>
          <NavButton type="button" data-active={view === "orders"} onClick={() => onViewChange("orders")}>
            <CreditCard size={18} /> Заказы
          </NavButton>
          {!auth && (
            <NavButton type="button" data-active={view === "auth"} onClick={() => onViewChange("auth")}>
              <LogIn size={18} /> Войти
            </NavButton>
          )}
          {isManager && (
            <NavButton type="button" data-active={view === "admin"} onClick={() => onViewChange("admin")}>
              <Boxes size={18} /> Админка
            </NavButton>
          )}
        </NavList>

        <SidebarFooter>
          {auth ? (
            <>
              <SessionCard>
                <UserRound size={18} />
                <div>
                  <strong>{auth.email}</strong>
                  <span>{auth.role}</span>
                </div>
              </SessionCard>
              <SidebarGhostButton type="button" onClick={onLogout}>
                <LogOut size={18} /> Выйти
              </SidebarGhostButton>
            </>
          ) : (
            <GuestBox>
              <strong>Гость</strong>
              <span>Войдите, чтобы оформить заказ и видеть историю покупок.</span>
              <SidebarGhostButton type="button" onClick={() => onViewChange("auth")}>
                <LogIn size={18} /> Войти
              </SidebarGhostButton>
            </GuestBox>
          )}
        </SidebarFooter>
      </Sidebar>

      <Workspace>
        <Topbar>
          <div>
            <h1>{viewTitle(view)}</h1>
            <p>{viewSubtitle(view)}</p>
          </div>
          {notice && <Notice onClick={onDismissNotice}>{notice}</Notice>}
        </Topbar>
        {children}
      </Workspace>
    </AppShell>
  );
}

const AppShell = styled.div`
  display: grid;
  grid-template-columns: 280px 1fr;
  min-height: 100vh;

  @media (max-width: 1080px) {
    grid-template-columns: 1fr;
  }
`;

const Sidebar = styled.aside`
  position: sticky;
  top: 0;
  display: flex;
  flex-direction: column;
  gap: 22px;
  height: 100vh;
  padding: 22px;
  color: #f8faf9;
  background: #172026;

  @media (max-width: 1080px) {
    position: static;
    height: auto;
  }

  @media (max-width: 760px) {
    padding: 16px;
  }
`;

const BrandBlock = styled.div`
  display: flex;
  align-items: center;
  gap: 12px;

  span {
    display: block;
    margin-top: 3px;
    color: #b7c6c2;
    font-size: 13px;
  }
`;

const BrandMark = styled.div`
  display: grid;
  width: 44px;
  height: 44px;
  place-items: center;
  border-radius: 8px;
  color: #172026;
  background: #74d3ae;
  font-weight: 800;
`;

const NavList = styled.nav`
  display: grid;
  gap: 8px;

  @media (max-width: 1080px) {
    grid-template-columns: repeat(4, minmax(0, 1fr));
  }

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;

const NavButton = styled.button`
  display: inline-flex;
  align-items: center;
  justify-content: flex-start;
  gap: 8px;
  width: 100%;
  min-height: 40px;
  border: 0;
  border-radius: 8px;
  padding: 10px 12px;
  color: #dce6e2;
  background: transparent;
  text-decoration: none;

  &:hover,
  &[data-active="true"] {
    color: #ffffff;
    background: #25343a;
  }
`;

const Count = styled.span`
  min-width: 24px;
  margin-left: auto;
  padding: 2px 7px;
  border-radius: 999px;
  color: #172026;
  background: #74d3ae;
  text-align: center;
  font-size: 12px;
  font-weight: 700;
`;

const SidebarFooter = styled.div`
  margin-top: auto;
`;

const SessionCard = styled.div`
  display: flex;
  align-items: center;
  gap: 12px;

  span {
    display: block;
    margin-top: 3px;
    color: #b7c6c2;
    font-size: 13px;
  }
`;

const GuestBox = styled.div`
  display: grid;
  gap: 8px;
  padding-top: 14px;
  border-top: 1px solid #314348;

  span {
    color: #b7c6c2;
    font-size: 13px;
    line-height: 1.35;
  }
`;

const Workspace = styled.main`
  min-width: 0;
  padding: 24px;

  @media (max-width: 760px) {
    padding: 16px;
  }
`;

const Topbar = styled.div`
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
  margin-bottom: 18px;

  h1 {
    margin: 0;
    font-size: 34px;
    line-height: 1.08;
  }

  p {
    margin: 8px 0 0;
    color: #64736e;
  }

  @media (max-width: 760px) {
    flex-direction: column;

    h1 {
      font-size: 26px;
    }
  }
`;

const Notice = styled.button`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  max-width: 420px;
  min-height: 40px;
  border: 0;
  border-radius: 8px;
  padding: 10px 14px;
  color: #172026;
  background: #c9f0dc;
  overflow-wrap: anywhere;
  box-shadow: 0 8px 22px rgba(35, 132, 102, 0.14);
`;
