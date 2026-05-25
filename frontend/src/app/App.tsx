import { Shell } from "../components/layout/Shell";
import { AppRoutes } from "./AppRoutes";
import { useAppController } from "./useAppController";

export function App() {
  const app = useAppController();

  return (
    <Shell
      auth={app.auth}
      cartCount={app.cart.activeCart?.items.length ?? 0}
      notice={app.notice}
      view={app.view}
      onDismissNotice={() => app.setNotice(null)}
      onLogout={app.handleLogout}
      onViewChange={app.goTo}
    >
      <AppRoutes app={app} />
    </Shell>
  );
}
