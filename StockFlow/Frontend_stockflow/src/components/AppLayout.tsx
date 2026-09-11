import { NavLink, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";
import {
  canAccessInventory,
  canAccessReservations,
  canAccessTransactions,
  canAdminOrders,
  canCreateOrder,
  canViewMyOrders,
  canViewWarehouses,
} from "../utils/auth";

type NavItem = {
  to: string;
  label: string;
  show: boolean;
};

export function AppLayout() {
  const auth = useAuth();
  const location = useLocation();
  const isAuthPage = location.pathname === "/login" || location.pathname === "/register";

  const navItems: NavItem[] = [
    { to: "/", label: "Dashboard", show: true },
    { to: "/products", label: "Products", show: true },
    { to: "/categories", label: "Categories", show: true },
    { to: "/warehouses", label: "Warehouses", show: auth.isAuthenticated && canViewWarehouses(auth.roles) },
    { to: "/inventory", label: "Inventory", show: auth.isAuthenticated && canAccessInventory(auth.roles) },
    {
      to: "/orders",
      label: auth.isAuthenticated && canAdminOrders(auth.roles) ? "Orders (Admin)" : "Orders",
      show:
        !auth.isAuthenticated ||
        canCreateOrder(auth.roles) ||
        canViewMyOrders(auth.roles) ||
        canAdminOrders(auth.roles),
    },
    {
      to: "/reservations",
      label: "Reservations",
      show: auth.isAuthenticated && canAccessReservations(auth.roles),
    },
    {
      to: "/transactions",
      label: "Transactions",
      show: auth.isAuthenticated && canAccessTransactions(auth.roles),
    },
    { to: "/signalr", label: "SignalR", show: auth.isAuthenticated },
  ];

  if (isAuthPage) {
    return (
      <div className="auth-shell">
        <Outlet />
      </div>
    );
  }

  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="app-header-brand">
          <NavLink to="/" className="brand-link">
            StockFlow
          </NavLink>
        </div>
        <div className="app-header-user">
          {auth.isAuthenticated ? (
            <>
              <span className="user-meta">
                {auth.displayName || auth.email || "User"}
                {auth.roles.length > 0 && (
                  <span className="user-role">{auth.roles.join(", ")}</span>
                )}
              </span>
              <button type="button" className="btn btn-secondary btn-sm" onClick={auth.logout}>
                Logout
              </button>
            </>
          ) : (
            <>
              <NavLink to="/login" className="btn btn-secondary btn-sm">
                Login
              </NavLink>
              <NavLink to="/register" className="btn btn-primary btn-sm">
                Register
              </NavLink>
            </>
          )}
        </div>
      </header>
      <div className="app-body">
        <aside className="app-sidebar">
          <nav className="sidebar-nav">
            {navItems
              .filter((item) => item.show)
              .map((item) => (
                <NavLink key={item.to} to={item.to} className="sidebar-link">
                  {item.label}
                </NavLink>
              ))}
          </nav>
        </aside>
        <div className="app-content">
          <Outlet />
        </div>
      </div>
    </div>
  );
}
