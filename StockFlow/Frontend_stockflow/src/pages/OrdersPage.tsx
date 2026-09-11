import { useEffect, useMemo, useState, type FormEvent } from "react";
import { useLocation } from "react-router-dom";
import { FormError } from "../components/FormError";
import { RequireRole } from "../components/RequireRole";
import { TextInput } from "../components/TextInput";
import { useAuth } from "../hooks/useAuth";
import {
  createOrder,
  getAllOrders,
  getMyOrders,
  updateOrderStatus,
} from "../services/orderService";
import { getProducts } from "../services/productService";
import type { OrderResponse, ProductResponse, UpdateOrderStatusRequest } from "../types/api";
import { canAdminOrders, canCreateOrder, canViewMyOrders, ROLES } from "../utils/auth";

type CartItem = {
  productId: number;
  quantity: number;
};

const statuses: UpdateOrderStatusRequest["status"][] = [
  "Pending",
  "Confirmed",
  "Cancelled",
  "Completed",
];

export function OrdersPage() {
  const auth = useAuth();
  const location = useLocation();
  const isCustomer = canCreateOrder(auth.roles);
  const isAdmin = canAdminOrders(auth.roles);
  const showMyOrders = canViewMyOrders(auth.roles);

  const initialProductId = Number((location.state as { productId?: number } | null)?.productId ?? 0);
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [myOrders, setMyOrders] = useState<OrderResponse[]>([]);
  const [allOrders, setAllOrders] = useState<OrderResponse[]>([]);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [selectedProductId, setSelectedProductId] = useState(initialProductId);
  const [submitting, setSubmitting] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const productById = useMemo(
    () => new Map(products.map((product) => [product.id, product])),
    [products],
  );

  const cartTotal = useMemo(
    () =>
      cart.reduce((sum, item) => {
        const price = productById.get(item.productId)?.price ?? 0;
        return sum + price * item.quantity;
      }, 0),
    [cart, productById],
  );

  async function loadPageData() {
    setLoading(true);
    setError(null);

    try {
      const productRows = await getProducts();
      setProducts(productRows.filter((product) => product.isActive));

      if (showMyOrders) {
        setMyOrders(await getMyOrders());
      }
      if (isAdmin) {
        setAllOrders(await getAllOrders());
      }
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadPageData();
  }, [showMyOrders, isAdmin]);

  function addItem(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const productId = Number(form.get("productId") ?? 0);
    const quantity = Number(form.get("quantity") ?? 0);

    if (!productId || quantity <= 0) {
      setError(new Error("Choose a product and enter a quantity greater than zero."));
      return;
    }

    setError(null);
    setCart((current) => [...current, { productId, quantity }]);
    event.currentTarget.reset();
    setSelectedProductId(0);
  }

  async function submitOrder() {
    if (cart.length === 0) {
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      await createOrder({ items: cart });
      setCart([]);
      await loadPageData();
    } catch (err) {
      setError(err);
    } finally {
      setSubmitting(false);
    }
  }

  async function changeStatus(orderId: number, status: UpdateOrderStatusRequest["status"]) {
    setError(null);

    try {
      await updateOrderStatus(orderId, { status });
      await loadPageData();
    } catch (err) {
      setError(err);
    }
  }

  if (!auth.isAuthenticated) {
    return (
      <main className="page">
        <div className="card">
          <h1>Orders</h1>
          <p>Please sign in to view or create orders.</p>
        </div>
      </main>
    );
  }

  if (!isCustomer && !isAdmin && !showMyOrders) {
    return (
      <RequireRole allowed={[ROLES.Customer, ROLES.Admin]}>
        <span />
      </RequireRole>
    );
  }

  return (
    <main className="page">
      <header className="page-header">
        <h1>{isAdmin && !isCustomer ? "Order administration" : "Orders"}</h1>
        <p>
          {isCustomer
            ? "Build a cart from products and submit your order."
            : "Review and update order status."}
        </p>
      </header>

      <FormError error={error} />

      {isCustomer && (
        <>
          <section className="card">
            <h2>Create order</h2>
            <form className="inline-form" onSubmit={addItem}>
              <label>
                Product
                <select
                  name="productId"
                  value={selectedProductId || ""}
                  onChange={(event) => setSelectedProductId(Number(event.target.value))}
                  required
                >
                  <option value="">Choose product</option>
                  {products.map((product) => (
                    <option key={product.id} value={product.id}>
                      {product.name}
                    </option>
                  ))}
                </select>
              </label>
              <TextInput label="Quantity" name="quantity" type="number" required />
              <button type="submit" className="btn btn-primary">
                Add item
              </button>
            </form>
          </section>

          <section className="card">
            <h2>Order summary</h2>
            {cart.length === 0 ? (
              <p className="empty-state" style={{ padding: 16 }}>
                No items in your cart yet.
              </p>
            ) : (
              <>
                <div className="table-wrap">
                  <table className="data-table">
                    <thead>
                      <tr>
                        <th>Product</th>
                        <th>Qty</th>
                        <th>Unit price</th>
                        <th>Subtotal</th>
                        <th />
                      </tr>
                    </thead>
                    <tbody>
                      {cart.map((item, index) => {
                        const product = productById.get(item.productId);
                        const unit = product?.price ?? 0;
                        return (
                          <tr key={`${item.productId}-${index}`}>
                            <td>{product?.name ?? item.productId}</td>
                            <td>{item.quantity}</td>
                            <td>{unit.toFixed(2)}</td>
                            <td>{(unit * item.quantity).toFixed(2)}</td>
                            <td>
                              <button
                                type="button"
                                className="btn btn-ghost btn-sm"
                                onClick={() =>
                                  setCart((current) => current.filter((_, i) => i !== index))
                                }
                              >
                                Remove
                              </button>
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>
                <p>
                  <strong>Estimated total: {cartTotal.toFixed(2)}</strong>
                </p>
                <button
                  type="button"
                  className="btn btn-primary"
                  disabled={submitting}
                  onClick={() => void submitOrder()}
                >
                  {submitting ? "Creating…" : "Create order"}
                </button>
              </>
            )}
          </section>
        </>
      )}

      {showMyOrders && (
        <section className="card">
          <h2>My orders</h2>
          {loading ? (
            <p className="loading-state">Loading…</p>
          ) : myOrders.length === 0 ? (
            <p className="empty-state">No orders found.</p>
          ) : (
            <div className="table-wrap">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Order #</th>
                    <th>Status</th>
                    <th>Total</th>
                    <th>Created</th>
                    <th>Items</th>
                  </tr>
                </thead>
                <tbody>
                  {myOrders.map((order) => (
                    <tr key={order.id}>
                      <td>{order.id}</td>
                      <td>
                        <span className="status-pill">{order.status}</span>
                      </td>
                      <td>{order.totalAmount.toFixed(2)}</td>
                      <td>{new Date(order.createdAt).toLocaleString()}</td>
                      <td>
                        {order.items?.map((item) => (
                          <div key={item.id}>
                            {item.productName} × {item.quantity}
                          </div>
                        ))}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
      )}

      {isAdmin && (
        <section className="card">
          <h2>All orders</h2>
          {loading ? (
            <p className="loading-state">Loading…</p>
          ) : allOrders.length === 0 ? (
            <p className="empty-state">No orders found.</p>
          ) : (
            <div className="table-wrap">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Order #</th>
                    <th>User</th>
                    <th>Status</th>
                    <th>Total</th>
                    <th>Created</th>
                    <th>Items</th>
                    <th>Update status</th>
                  </tr>
                </thead>
                <tbody>
                  {allOrders.map((order) => (
                    <tr key={order.id}>
                      <td>{order.id}</td>
                      <td>{order.userId}</td>
                      <td>
                        <span className="status-pill">{order.status}</span>
                      </td>
                      <td>{order.totalAmount.toFixed(2)}</td>
                      <td>{new Date(order.createdAt).toLocaleString()}</td>
                      <td>
                        {order.items?.map((item) => (
                          <div key={item.id}>
                            {item.productName} × {item.quantity}
                          </div>
                        ))}
                      </td>
                      <td>
                        <select
                          defaultValue={order.status}
                          onChange={(event) =>
                            void changeStatus(
                              order.id,
                              event.target.value as UpdateOrderStatusRequest["status"],
                            )
                          }
                        >
                          {statuses.map((status) => (
                            <option key={status} value={status}>
                              {status}
                            </option>
                          ))}
                        </select>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
      )}
    </main>
  );
}
