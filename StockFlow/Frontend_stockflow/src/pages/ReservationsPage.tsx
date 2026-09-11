import { useEffect, useMemo, useState, type FormEvent } from "react";
import { FormError } from "../components/FormError";
import { RequireRole } from "../components/RequireRole";
import { TextInput } from "../components/TextInput";
import { useAuth } from "../hooks/useAuth";
import { getInventory } from "../services/inventoryService";
import { getAllOrders } from "../services/orderService";
import {
  cancelReservation,
  createReservation,
  getReservationsByOrder,
  releaseReservation,
} from "../services/reservationService";
import type { InventoryResponse, OrderResponse, ReservationResponse } from "../types/api";
import { canAdminOrders, ROLES } from "../utils/auth";

export function ReservationsPage() {
  const auth = useAuth();
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [inventory, setInventory] = useState<InventoryResponse[]>([]);
  const [reservations, setReservations] = useState<ReservationResponse[]>([]);
  const [selectedOrderId, setSelectedOrderId] = useState(0);
  const [selectedProductId, setSelectedProductId] = useState(0);
  const [selectedInventoryId, setSelectedInventoryId] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const selectedOrder = orders.find((order) => order.id === selectedOrderId);
  const orderProducts = selectedOrder?.items ?? [];
  const matchingInventory = useMemo(
    () => inventory.filter((row) => row.productId === selectedProductId),
    [inventory, selectedProductId],
  );

  async function loadBaseData() {
    setLoading(true);
    setError(null);

    try {
      const [orderRows, inventoryRows] = await Promise.all([
        getAllOrders(),
        getInventory(),
      ]);
      setOrders(orderRows);
      setInventory(inventoryRows);
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }

  async function loadReservations(orderId: number) {
    if (!orderId) {
      setReservations([]);
      return;
    }

    setError(null);

    try {
      setReservations(await getReservationsByOrder(orderId));
    } catch (err) {
      setError(err);
    }
  }

  useEffect(() => {
    if (canAdminOrders(auth.roles) || auth.roles.includes(ROLES.InventoryManager)) {
      void loadBaseData();
    }
  }, [auth.roles]);

  async function submitReservation(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const inventoryRow = inventory.find((row) => row.id === selectedInventoryId);

    if (!selectedOrderId || !selectedProductId || !inventoryRow) {
      setError(new Error("Choose an order, product, and warehouse."));
      return;
    }

    const form = new FormData(event.currentTarget);

    try {
      await createReservation({
        orderId: selectedOrderId,
        productId: selectedProductId,
        warehouseId: inventoryRow.warehouseId,
        quantity: Number(form.get("quantity") ?? 0),
      });
      event.currentTarget.reset();
      await loadReservations(selectedOrderId);
    } catch (err) {
      setError(err);
    }
  }

  async function release(id: number) {
    try {
      await releaseReservation(id);
      await loadReservations(selectedOrderId);
    } catch (err) {
      setError(err);
    }
  }

  async function cancel(id: number) {
    try {
      await cancelReservation(id);
      await loadReservations(selectedOrderId);
    } catch (err) {
      setError(err);
    }
  }

  return (
    <RequireRole allowed={[ROLES.Admin, ROLES.InventoryManager]}>
      <main className="page">
        <header className="page-header">
          <h1>Reservations</h1>
          <p>
            Reserve stock against orders. There is no list-all endpoint — choose an order to load
            its reservations.
          </p>
        </header>

        <FormError error={error} />

        {loading ? (
          <p className="loading-state">Loading…</p>
        ) : (
          <>
            <section className="card">
              <h2>Select order</h2>
              <label>
                Order
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
                      #{order.id} — {order.status} — {order.totalAmount.toFixed(2)}
                    </option>
                  ))}
                </select>
              </label>
            </section>

            {selectedOrderId > 0 && (
              <section className="card">
                <h2>Create reservation</h2>
                <form className="stacked-form" onSubmit={submitReservation}>
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
                          {item.productName} (ordered: {item.quantity})
                        </option>
                      ))}
                    </select>
                  </label>
                  <label>
                    Warehouse
                    <select
                      value={selectedInventoryId || ""}
                      onChange={(event) => setSelectedInventoryId(Number(event.target.value))}
                      required
                    >
                      <option value="">Choose warehouse</option>
                      {matchingInventory.map((row) => (
                        <option key={row.id} value={row.id}>
                          {row.warehouseName} — available: {row.availableQuantity}
                        </option>
                      ))}
                    </select>
                  </label>
                  <TextInput label="Quantity" name="quantity" type="number" required />
                  <button type="submit" className="btn btn-primary">
                    Create reservation
                  </button>
                </form>
              </section>
            )}

            <section className="card">
              <h2>Reservations {selectedOrderId ? `for order #${selectedOrderId}` : ""}</h2>
              {!selectedOrderId ? (
                <p className="empty-state">Select an order to view reservations.</p>
              ) : reservations.length === 0 ? (
                <p className="empty-state">No reservations for this order.</p>
              ) : (
                <div className="table-wrap">
                  <table className="data-table">
                    <thead>
                      <tr>
                        <th>Order</th>
                        <th>Product</th>
                        <th>Warehouse</th>
                        <th>Qty</th>
                        <th>Status</th>
                        <th>Created</th>
                        <th>Expires</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {reservations.map((reservation) => (
                        <tr key={reservation.id}>
                          <td>{reservation.orderId}</td>
                          <td>{reservation.productName}</td>
                          <td>{reservation.warehouseName}</td>
                          <td>{reservation.quantity}</td>
                          <td>
                            <span className="status-pill">{reservation.status}</span>
                          </td>
                          <td>{new Date(reservation.createdAt).toLocaleString()}</td>
                          <td>{new Date(reservation.expiresAt).toLocaleString()}</td>
                          <td>
                            {reservation.status === "Active" && (
                              <div className="row-actions">
                                <button
                                  type="button"
                                  className="btn btn-ghost btn-sm"
                                  onClick={() => void release(reservation.id)}
                                >
                                  Release
                                </button>
                                <button
                                  type="button"
                                  className="btn btn-danger btn-sm"
                                  onClick={() => void cancel(reservation.id)}
                                >
                                  Cancel
                                </button>
                              </div>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </section>
          </>
        )}
      </main>
    </RequireRole>
  );
}
