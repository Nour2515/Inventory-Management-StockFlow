import { NavLink } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";

const links = [
  ["/login", "Login"],
  ["/register", "Register"],
  ["/products", "Products"],
  ["/inventory", "Inventory"],
  ["/orders", "Orders"],
  ["/reservations", "Reservations"],
  ["/transactions", "Transactions"],
  ["/signalr", "SignalR"],
] as const;

export function NavMenu() {
  const auth = useAuth();

  return (
    <nav className="nav-menu">
      <strong>StockFlow</strong>
      {links.map(([to, label]) => (
        <NavLink key={to} to={to}>
          {label}
        </NavLink>
      ))}
      <span className="nav-status">
        {auth.isAuthenticated ? `Logged in ${auth.roles.join(", ")}` : "Not logged in"}
      </span>
      {auth.isAuthenticated && (
        <button type="button" onClick={auth.logout}>
          Logout
        </button>
      )}
    </nav>
  );
}
