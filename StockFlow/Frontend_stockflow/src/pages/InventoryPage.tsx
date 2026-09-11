import { useEffect, useState, type FormEvent } from "react";
import { getErrorMessage } from "../api/client";
import { TextInput } from "../components/TextInput";
import { getAvailability, getInventory, updateInventory } from "../services/inventoryService";
import type { InventoryResponse, StockAvailabilityResponse } from "../types/api";

export function InventoryPage() {
  const [inventory, setInventory] = useState<InventoryResponse[]>([]);
  const [selected, setSelected] = useState<InventoryResponse | null>(null);
  const [availabilityRow, setAvailabilityRow] = useState<InventoryResponse | null>(null);
  const [availability, setAvailability] = useState<StockAvailabilityResponse | null>(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function loadInventory() {
    setLoading(true);
    setError("");

    try {
      setInventory(await getInventory());
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    const timeoutId = window.setTimeout(() => void loadInventory(), 0);

    function onInventoryUpdated() {
      void loadInventory();
    }

    window.addEventListener("stockflow:inventory-updated", onInventoryUpdated);
    return () => {
      window.clearTimeout(timeoutId);
      window.removeEventListener("stockflow:inventory-updated", onInventoryUpdated);
    };
  }, []);

  async function submitUpdate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selected) {
      return;
    }

    const form = new FormData(event.currentTarget);

    try {
      await updateInventory(selected.id, {
        reorderLevel: Number(form.get("reorderLevel") ?? selected.reorderLevel),
      });
      setSelected(null);
      await loadInventory();
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  async function checkAvailability(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);

    try {
      setAvailability(
        await getAvailability(
          availabilityRow?.productId ?? 0,
          availabilityRow?.warehouseId ?? 0,
          Number(form.get("quantity") ?? 0),
        ),
      );
    } catch (err) {
      setAvailability(null);
      setError(getErrorMessage(err));
    }
  }

  return (
    <main>
      <h1>Inventory</h1>
      {error && <p className="error-text">{error}</p>}
      {loading && <p>Loading inventory...</p>}

      <table>
        <thead>
          <tr>
            <th>Product</th>
            <th>Warehouse</th>
            <th>On hand</th>
            <th>Reserved</th>
            <th>Available</th>
            <th>Reorder level</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {inventory.map((item) => (
            <tr key={item.id}>
              <td>{item.productName}</td>
              <td>{item.warehouseName}</td>
              <td>{item.onHandQuantity}</td>
              <td>{item.reservedQuantity}</td>
              <td>{item.availableQuantity}</td>
              <td>{item.reorderLevel}</td>
              <td>
                <button type="button" onClick={() => setSelected(item)}>
                  Edit reorder
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {selected && (
        <form onSubmit={submitUpdate}>
          <h2>Update reorder level for {selected.productName}</h2>
          <TextInput
            label="Reorder level"
            name="reorderLevel"
            type="number"
            required
          />
          <button type="submit">Save</button>
        </form>
      )}

      <form onSubmit={checkAvailability}>
        <h2>Check availability</h2>
        <label>
          Inventory item
          <select
            name="inventoryId"
            onChange={(event) =>
              setAvailabilityRow(
                inventory.find((item) => item.id === Number(event.target.value)) ?? null,
              )
            }
          >
            <option value="">Choose inventory row</option>
            {inventory.map((item) => (
              <option key={item.id} value={item.id}>
                {item.productName} at {item.warehouseName}
              </option>
            ))}
          </select>
        </label>
        <TextInput label="Quantity" name="quantity" type="number" required />
        <button type="submit">Check</button>
      </form>

      {availability && (
        <section className="panel">
          <h2>Availability</h2>
          <p>Available quantity: {availability.availableQuantity}</p>
          <p>{availability.isAvailable ? "Enough stock" : "Not enough stock"}</p>
        </section>
      )}
    </main>
  );
}
