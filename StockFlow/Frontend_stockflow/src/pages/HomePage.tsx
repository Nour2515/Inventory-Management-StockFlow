import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getErrorMessage } from "../api/client";
import { useAuth } from "../hooks/useAuth";
import { getInventory } from "../services/inventoryService";
import { getMyOrders, getAllOrders } from "../services/orderService";
import { getProducts } from "../services/productService";
import { getTransactions } from "../services/transactionService";
import {
  canAccessInventory,
  canAccessTransactions,
  canAdminOrders,
  canCreateOrder,
  ROLES,
} from "../utils/auth";

type DashboardStats = {
  products?: number;
  inventory?: number;
  myOrders?: number;
  allOrders?: number;
  transactions?: number;
};

export function HomePage() {
  const auth = useAuth();
  const [stats, setStats] = useState<DashboardStats>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setLoading(true);
      setError("");
      const next: DashboardStats = {};

      try {
        const products = await getProducts();
        if (!cancelled) {
          next.products = products.length;
        }
      } catch (err) {
        if (!cancelled) {
          setError(getErrorMessage(err));
        }
      }

      if (auth.isAuthenticated && canAccessInventory(auth.roles)) {
        try {
          const inventory = await getInventory();
          if (!cancelled) {
            next.inventory = inventory.length;
          }
        } catch {
          /* omit metric if forbidden */
        }
      }

      if (auth.isAuthenticated && canCreateOrder(auth.roles)) {
        try {
          const orders = await getMyOrders();
          if (!cancelled) {
            next.myOrders = orders.length;
          }
        } catch {
          /* omit */
        }
      }

      if (auth.isAuthenticated && canAdminOrders(auth.roles)) {
        try {
          const orders = await getAllOrders();
          if (!cancelled) {
            next.allOrders = orders.length;
          }
        } catch {
          /* omit */
        }
      }

      if (auth.isAuthenticated && canAccessTransactions(auth.roles)) {
        try {
          const transactions = await getTransactions();
          if (!cancelled) {
            next.transactions = transactions.length;
          }
        } catch {
          /* omit */
        }
      }

      if (!cancelled) {
        setStats(next);
        setLoading(false);
      }
    }

    void load();
    return () => {
      cancelled = true;
    };
  }, [auth.isAuthenticated, auth.roles]);

  const roleHint = auth.isAuthenticated
    ? auth.roles.includes(ROLES.Customer)
      ? "Browse products and place orders from the Orders page."
      : auth.roles.includes(ROLES.InventoryManager)
        ? "Monitor inventory, reservations, and transactions."
        : auth.roles.includes(ROLES.Admin)
          ? "Full access to catalog, warehouses, orders, and inventory."
          : "Use the sidebar to navigate allowed sections."
    : "Sign in to access warehouses, orders, and real-time updates.";

  return (
    <main className="page">
      <header className="page-header">
        <h1>Dashboard</h1>
        <p>{roleHint}</p>
      </header>

      {error && <div className="alert alert-error">{error}</div>}

      {loading ? (
        <p className="loading-state">Loading…</p>
      ) : (
        <div className="card-grid">
          {stats.products !== undefined && (
            <div className="stat-card">
              <div className="stat-label">Products</div>
              <div className="stat-value">{stats.products}</div>
              <Link to="/products">View products</Link>
            </div>
          )}
          {stats.inventory !== undefined && (
            <div className="stat-card">
              <div className="stat-label">Inventory rows</div>
              <div className="stat-value">{stats.inventory}</div>
              <Link to="/inventory">View inventory</Link>
            </div>
          )}
          {stats.myOrders !== undefined && (
            <div className="stat-card">
              <div className="stat-label">My orders</div>
              <div className="stat-value">{stats.myOrders}</div>
              <Link to="/orders">View orders</Link>
            </div>
          )}
          {stats.allOrders !== undefined && (
            <div className="stat-card">
              <div className="stat-label">All orders</div>
              <div className="stat-value">{stats.allOrders}</div>
              <Link to="/orders">Manage orders</Link>
            </div>
          )}
          {stats.transactions !== undefined && (
            <div className="stat-card">
              <div className="stat-label">Transactions</div>
              <div className="stat-value">{stats.transactions}</div>
              <Link to="/transactions">View transactions</Link>
            </div>
          )}
        </div>
      )}

      {!auth.isAuthenticated && (
        <section className="card">
          <h2>Get started</h2>
          <p>
            <Link to="/login" className="btn btn-primary">
              Login
            </Link>{" "}
            or{" "}
            <Link to="/register" className="btn btn-ghost">
              Register
            </Link>{" "}
            to create orders and access warehouse data.
          </p>
        </section>
      )}
    </main>
  );
}
