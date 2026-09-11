import { useEffect, useMemo, useState, type FormEvent } from "react";
import { getErrorMessage } from "../api/client";
import { TextInput } from "../components/TextInput";
import { useAuth } from "../hooks/useAuth";
import { getInventory } from "../services/inventoryService";
import { getMyOrders } from "../services/orderService";
import {
  cancelReservation,
  createReservation,
  getReservationsByOrder,
  releaseReservation,
} from "../services/reservationService";
import type { InventoryResponse, OrderResponse, ReservationResponse } from "../types/api";

export function ReservationsPage() {
  const auth = useAuth();
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [inventory, setInventory] = useState<InventoryResponse[]>([]);
  const [reservations, setReservations] = useState<ReservationResponse[]>([]);
  const [selectedOrderId, setSelectedOrderId] = useState(0);
  const [selectedProductId, setSelectedProductId] = useState(0);
  const [selectedInventoryId, setSelectedInventoryId] = useState(0);
  const [error, setError] = useState("");

  const selectedOrder = orders.find((order) => order.id === selectedOrderId);
  const orderProducts = selectedOrder?.items ?? [];
  const matchingInventory = useMemo(
    () => inventory.filter((row) => row.productId === selectedProductId),
    [inventory, selectedProductId],
  );

  async function loadBaseData() {
    setError("");

    try {
      const [orderRows, inventoryRows] = await Promise.all([getMyOrders(), getInventory()]);
      setOrders(orderRows);
      setInventory(inventoryRows);
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  async function loadReservations(orderId: number) {
    if (!orderId) {
      setReservations([]);
      return;
    }

    setError("");

    try {
      setReservations(await getReservationsByOrder(orderId));
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  useEffect(() => {
    const timeoutId = window.setTimeout(() => void loadBaseData(), 0);
    return () => window.clearTimeout(timeoutId);
  }, []);

  async function submitReservation(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const inventoryRow = inventory.find((row) => row.id === selectedInventoryId);

    if (!selectedOrderId || !selectedProductId || !inventoryRow) {
      setError("Choose an order, product, and warehouse.");
      return;
    }

    const form = new FormData(event.currentTarget);

    try {
      await createReservation({
        orderId: selectedOrderId,
        productId: selectedProductId,
        warehouseId: inventoryRow.warehouseId,
        quantity: Number(form.get("quantity") ?? 0),
        createdByUserId: auth.userId ?? 0,
      });
      event.currentTarget.reset();
      await loadReservations(selectedOrderId);
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  async function release(id: number) {
    try {
      await releaseReservation(id);
      await loadReservations(selectedOrderId);
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  async function cancel(id: number) {
    try {
      await cancelReservation(id);
      await loadReservations(selectedOrderId);
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  return (
    <main>
      <h1>Reservations</h1>
      {error && <p className="error-text">{error}</p>}

      <section className="panel">
        <h2>Choose order</h2>
        <select
          value={selectedOrderId || ""}
          onChange={(event) => {
            const orderId = Number(event.target.value);
            setSelectedOrderId(orderId);
            setSelectedProductId(0);
            setSelectedInventoryId(0);
            void loadReservations(orderId);
          }}
        >
          <option value="">Choose order</option>
          {orders.map((order) => (
            <option key={order.id} value={order.id}>
              Order #{order.id} - {order.status} - {order.totalAmount}
            </option>
          ))}
        </select>
      </section>

      <form onSubmit={submitReservation}>
        <h2>Create reservation</h2>
        <label>
          Product from order
          <select
            value={selectedProductId || ""}
            onChange={(event) => {
              setSelectedProductId(Number(event.target.value));
              setSelectedInventoryId(0);
            }}
            required
          >
            <option value="">Choose product</option>
            {orderProducts.map((item) => (
              <option key={item.productId} value={item.productId}>
                {item.productName} ordered: {item.quantity}
              </option>
            ))}
          </select>
        </label>
        <label>
          Warehouse inventory
          <select
            value={selectedInventoryId || ""}
            onChange={(event) => setSelectedInventoryId(Number(event.target.value))}
            required
          >
            <option value="">Choose warehouse</option>
            {matchingInventory.map((row) => (
              <option key={row.id} value={row.id}>
                {row.warehouseName} available: {row.availableQuantity}
              </option>
            ))}
          </select>
        </label>
        <TextInput label="Quantity" name="quantity" type="number" required />
        <button type="submit">Create reservation</button>
      </form>

      <section>
        <h2>Reservations for selected order</h2>
        <table>
          <thead>
            <tr>
              <th>Product</th>
              <th>Warehouse</th>
              <th>Quantity</th>
              <th>Status</th>
              <th>Created</th>
              <th>Expires</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {reservations.map((reservation) => (
              <tr key={reservation.id}>
                <td>{reservation.productName}</td>
                <td>{reservation.warehouseName}</td>
                <td>{reservation.quantity}</td>
                <td>{reservation.status}</td>
                <td>{new Date(reservation.createdAt).toLocaleString()}</td>
                <td>{new Date(reservation.expiresAt).toLocaleString()}</td>
                <td>
                  <button type="button" onClick={() => void release(reservation.id)}>
                    Release
                  </button>
                  <button type="button" onClick={() => void cancel(reservation.id)}>
                    Cancel
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </main>
  );
}
