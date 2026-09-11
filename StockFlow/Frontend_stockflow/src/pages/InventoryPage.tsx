import { useEffect, useState, type FormEvent } from "react";
import { FormError } from "../components/FormError";
import { RequireRole } from "../components/RequireRole";
import { TextInput } from "../components/TextInput";
import {
  createInventory,
  getAvailability,
  getInventory,
  updateInventory,
} from "../services/inventoryService";
import { getProducts } from "../services/productService";
import { getWarehouses } from "../services/warehouseService";
import type { InventoryResponse, ProductResponse, StockAvailabilityResponse } from "../types/api";
import { ROLES } from "../utils/auth";

export function InventoryPage() {
  const [inventory, setInventory] = useState<InventoryResponse[]>([]);
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [warehouseOptions, setWarehouseOptions] = useState<{ id: number; name: string }[]>([]);
  const [editing, setEditing] = useState<InventoryResponse | null>(null);
  const [showCreate, setShowCreate] = useState(false);
  const [availabilityRow, setAvailabilityRow] = useState<InventoryResponse | null>(null);
  const [availability, setAvailability] = useState<StockAvailabilityResponse | null>(null);
  const [error, setError] = useState<unknown>(null);
  const [loading, setLoading] = useState(true);

  async function loadInventory() {
    setLoading(true);
    setError(null);

    try {
      const rows = await getInventory();
      setInventory(rows);
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadInventory();

    function onInventoryUpdated() {
      void loadInventory();
    }

    window.addEventListener("stockflow:inventory-updated", onInventoryUpdated);
    return () => window.removeEventListener("stockflow:inventory-updated", onInventoryUpdated);
  }, []);

  useEffect(() => {
    if (!showCreate) {
      return;
    }

    void (async () => {
      try {
        const [productRows, warehouseRows] = await Promise.all([getProducts(), getWarehouses()]);
        setProducts(productRows);
        setWarehouseOptions(warehouseRows.map((w) => ({ id: w.id, name: w.name })));
      } catch (err) {
        setError(err);
      }
    })();
  }, [showCreate]);

  async function submitUpdate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!editing) {
      return;
    }

    const form = new FormData(event.currentTarget);

    try {
      await updateInventory(editing.id, {
        reorderLevel: Number(form.get("reorderLevel") ?? editing.reorderLevel),
      });
      setEditing(null);
      await loadInventory();
    } catch (err) {
      setError(err);
    }
  }

  async function submitCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);

    try {
      await createInventory({
        productId: Number(form.get("productId") ?? 0),
        warehouseId: Number(form.get("warehouseId") ?? 0),
        onhandQuantity: Number(form.get("onhandQuantity") ?? 0),
        reorderLevel: Number(form.get("reorderLevel") ?? 0),
      });
      setShowCreate(false);
      event.currentTarget.reset();
      await loadInventory();
    } catch (err) {
      setError(err);
    }
  }

  async function checkAvailability(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);

    if (!availabilityRow) {
      return;
    }

    try {
      setAvailability(
        await getAvailability(
          availabilityRow.productId,
          availabilityRow.warehouseId,
          Number(form.get("quantity") ?? 0),
        ),
      );
    } catch (err) {
      setAvailability(null);
      setError(err);
    }
  }

  return (
    <RequireRole allowed={[ROLES.Admin, ROLES.InventoryManager]}>
      <main className="page">
        <header className="page-header">
          <h1>Inventory</h1>
          <p>On-hand, reserved, and available stock by warehouse. Reserved quantity is read-only.</p>
        </header>

        <FormError error={error} />

        <div className="row-actions" style={{ marginBottom: 16 }}>
          <button type="button" className="btn btn-primary" onClick={() => setShowCreate((v) => !v)}>
            {showCreate ? "Cancel" : "Add inventory row"}
          </button>
        </div>

        {showCreate && (
          <section className="card">
            <h2>New inventory record</h2>
            <form className="stacked-form" onSubmit={submitCreate}>
              <label>
                Product
                <select name="productId" required defaultValue="">
                  <option value="" disabled>
                    Select product
                  </option>
                  {products.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.name}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Warehouse
                <select name="warehouseId" required defaultValue="">
                  <option value="" disabled>
                    Select warehouse
                  </option>
                  {warehouseOptions.map((w) => (
                    <option key={w.id} value={w.id}>
                      {w.name}
                    </option>
                  ))}
                </select>
              </label>
              <TextInput label="On-hand quantity" name="onhandQuantity" type="number" required />
              <TextInput label="Reorder level" name="reorderLevel" type="number" required />
              <button type="submit" className="btn btn-primary">
                Create
              </button>
            </form>
          </section>
        )}

        {editing && (
          <section className="card">
            <h2>Update reorder level — {editing.productName}</h2>
            <form className="stacked-form" onSubmit={submitUpdate}>
              <TextInput
                label="Reorder level"
                name="reorderLevel"
                type="number"
                required
                defaultValue={String(editing.reorderLevel)}
              />
              <div className="row-actions">
                <button type="submit" className="btn btn-primary">
                  Save
                </button>
                <button type="button" className="btn btn-ghost" onClick={() => setEditing(null)}>
                  Cancel
                </button>
              </div>
            </form>
          </section>
        )}

        {loading ? (
          <p className="loading-state">Loading…</p>
        ) : inventory.length === 0 ? (
          <p className="empty-state">No inventory records found.</p>
        ) : (
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Warehouse</th>
                  <th>On hand</th>
                  <th>Reserved</th>
                  <th>Available</th>
                  <th>Reorder</th>
                  <th>Updated</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {inventory.map((item) => (
                  <tr key={item.id}>
                    <td>{item.productName}</td>
                    <td>{item.warehouseName}</td>
                    <td className="qty-on-hand">{item.onHandQuantity}</td>
                    <td className="qty-reserved">{item.reservedQuantity}</td>
                    <td className="qty-available">{item.availableQuantity}</td>
                    <td>{item.reorderLevel}</td>
                    <td>{new Date(item.updatedAt).toLocaleString()}</td>
                    <td>
                      <button
                        type="button"
                        className="btn btn-ghost btn-sm"
                        onClick={() => setEditing(item)}
                      >
                        Edit reorder
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <section className="card">
          <h2>Check availability</h2>
          <form className="inline-form" onSubmit={checkAvailability}>
            <label>
              Inventory row
              <select
                name="inventoryId"
                onChange={(event) =>
                  setAvailabilityRow(
                    inventory.find((item) => item.id === Number(event.target.value)) ?? null,
                  )
                }
                required
                defaultValue=""
              >
                <option value="" disabled>
                  Choose row
                </option>
                {inventory.map((item) => (
                  <option key={item.id} value={item.id}>
                    {item.productName} @ {item.warehouseName}
                  </option>
                ))}
              </select>
            </label>
            <TextInput label="Quantity needed" name="quantity" type="number" required />
            <button type="submit" className="btn btn-primary">
              Check
            </button>
          </form>
          {availability && (
            <div className={`alert ${availability.isAvailable ? "alert-success" : "alert-warning"}`}>
              Available: {availability.availableQuantity}.{" "}
              {availability.isAvailable ? "Enough stock for this quantity." : "Not enough stock."}
            </div>
          )}
        </section>
      </main>
    </RequireRole>
  );
}
