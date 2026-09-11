import { useRef, useState } from "react";
import type { HubConnection } from "@microsoft/signalr";
import { createInventoryHubConnection } from "../services/signalRService";
import type { InventoryUpdatedEvent, LowStockAlertEvent } from "../types/api";

export function SignalRPage() {
  const connectionRef = useRef<HubConnection | null>(null);
  const [status, setStatus] = useState("Disconnected");
  const [inventoryEvents, setInventoryEvents] = useState<InventoryUpdatedEvent[]>([]);
  const [lowStockAlerts, setLowStockAlerts] = useState<LowStockAlertEvent[]>([]);

  async function connect() {
    if (connectionRef.current) {
      return;
    }

    const connection = createInventoryHubConnection(
      (event) => {
        setInventoryEvents((current) => [event, ...current].slice(0, 20));
        window.dispatchEvent(new Event("stockflow:inventory-updated"));
      },
      (event) => setLowStockAlerts((current) => [event, ...current].slice(0, 20)),
    );

    connectionRef.current = connection;
    setStatus("Connecting");

    try {
      await connection.start();
      setStatus("Connected");
    } catch (error) {
      connectionRef.current = null;
      setStatus(error instanceof Error ? error.message : "Connection failed");
    }
  }

  async function disconnect() {
    await connectionRef.current?.stop();
    connectionRef.current = null;
    setStatus("Disconnected");
  }

  return (
    <main>
      <h1>SignalR</h1>
      <p>Connects to /hubs/inventory.</p>
      <section className="actions">
        <button type="button" onClick={connect}>
          Connect
        </button>
        <button type="button" onClick={disconnect}>
          Disconnect
        </button>
      </section>
      <p>Status: {status}</p>

      <section className="panel alert-panel">
        <h2>Low stock alerts</h2>
        {lowStockAlerts.length === 0 ? (
          <p>No low stock alerts received.</p>
        ) : (
          lowStockAlerts.map((alert, index) => (
            <div key={`${alert.inventoryId}-${index}`} className="alert">
              {alert.productName} at {alert.warehouseName ?? `warehouse ${alert.warehouseId}`}:
              available {alert.availableQuantity}, reorder level {alert.reorderLevel}
            </div>
          ))
        )}
      </section>

      <section className="panel">
        <h2>Inventory updates</h2>
        {inventoryEvents.length === 0 ? (
          <p>No inventory updates received.</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Inventory</th>
                <th>Product</th>
                <th>Warehouse</th>
                <th>On hand</th>
                <th>Reserved</th>
                <th>Available</th>
                <th>Reason</th>
              </tr>
            </thead>
            <tbody>
              {inventoryEvents.map((event, index) => (
                <tr key={`${event.inventoryId}-${index}`}>
                  <td>{event.inventoryId}</td>
                  <td>{event.productId}</td>
                  <td>{event.warehouseId}</td>
                  <td>{event.onHandQuantity}</td>
                  <td>{event.reservedQuantity}</td>
                  <td>{event.availableQuantity}</td>
                  <td>{event.reason}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>
    </main>
  );
}
