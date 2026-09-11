import { useEffect, useMemo, useState, type FormEvent } from "react";
import { useLocation } from "react-router-dom";
import { getErrorMessage } from "../api/client";
import { TextInput } from "../components/TextInput";
import { createOrder, getMyOrders, updateOrderStatus } from "../services/orderService";
import { getProducts } from "../services/productService";
import type { OrderResponse, ProductResponse, UpdateOrderStatusRequest } from "../types/api";

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
  const location = useLocation();
  const initialProductId = Number((location.state as { productId?: number } | null)?.productId ?? 0);
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [selectedProductId, setSelectedProductId] = useState(initialProductId);
  const [error, setError] = useState("");

  const productById = useMemo(
    () => new Map(products.map((product) => [product.id, product])),
    [products],
  );

  async function loadPageData() {
    setError("");

    try {
      const [productRows, orderRows] = await Promise.all([getProducts(), getMyOrders()]);
      setProducts(productRows.filter((product) => product.isActive));
      setOrders(orderRows);
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  useEffect(() => {
    const timeoutId = window.setTimeout(() => void loadPageData(), 0);
    return () => window.clearTimeout(timeoutId);
  }, []);

  function addItem(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const productId = Number(form.get("productId") ?? 0);
    const quantity = Number(form.get("quantity") ?? 0);

    if (!productId || quantity <= 0) {
      setError("Choose a product and enter a quantity greater than zero.");
      return;
    }

    setCart((current) => [...current, { productId, quantity }]);
    event.currentTarget.reset();
  }

  async function submitOrder() {
    setError("");

    try {
      await createOrder({ items: cart });
      setCart([]);
      await loadPageData();
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  async function changeStatus(orderId: number, status: UpdateOrderStatusRequest["status"]) {
    setError("");

    try {
      await updateOrderStatus(orderId, { status });
      await loadPageData();
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  return (
    <main>
      <h1>Orders</h1>
      {error && <p className="error-text">{error}</p>}

      <form onSubmit={addItem}>
        <h2>Create order</h2>
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
                {product.name} - {product.price}
              </option>
            ))}
          </select>
        </label>
        <TextInput label="Quantity" name="quantity" type="number" required />
        <button type="submit">Add item</button>
      </form>

      <section className="panel">
        <h2>Cart</h2>
        {cart.length === 0 ? (
          <p>No items yet.</p>
        ) : (
          <>
            <ul>
              {cart.map((item, index) => (
                <li key={`${item.productId}-${index}`}>
                  {productById.get(item.productId)?.name ?? item.productId} x {item.quantity}
                  <button
                    type="button"
                    onClick={() => setCart((current) => current.filter((_, i) => i !== index))}
                  >
                    Remove
                  </button>
                </li>
              ))}
            </ul>
            <button type="button" onClick={() => void submitOrder()}>
              Submit order
            </button>
          </>
        )}
      </section>

      <section>
        <h2>My orders</h2>
        <table>
          <thead>
            <tr>
              <th>Status</th>
              <th>Total</th>
              <th>Items</th>
              <th>Admin status update</th>
            </tr>
          </thead>
          <tbody>
            {orders.map((order) => (
              <tr key={order.id}>
                <td>{order.status}</td>
                <td>{order.totalAmount}</td>
                <td>
                  {order.items?.map((item) => (
                    <div key={item.id}>
                      {item.productName} x {item.quantity}
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
      </section>
    </main>
  );
}
