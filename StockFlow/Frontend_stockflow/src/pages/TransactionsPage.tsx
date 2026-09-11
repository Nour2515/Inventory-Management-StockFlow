import { useEffect, useState, type FormEvent } from "react";
import { FormError } from "../components/FormError";
import { RequireRole } from "../components/RequireRole";
import { TextInput } from "../components/TextInput";
import { getInventory } from "../services/inventoryService";
import { createTransaction, getTransactions } from "../services/transactionService";
import type {
  CreateInventoryTransactionRequest,
  InventoryResponse,
  InventoryTransactionResponse,
} from "../types/api";
import { ROLES } from "../utils/auth";

const transactionTypes: CreateInventoryTransactionRequest["type"][] = [
  "StockIn",
  "Sale",
  "Return",
  "TransferIn",
  "TransferOut",
  "Reservation",
  "ReservationReleased",
];

export function TransactionsPage() {
  const [transactions, setTransactions] = useState<InventoryTransactionResponse[]>([]);
  const [inventory, setInventory] = useState<InventoryResponse[]>([]);
  const [selectedInventoryId, setSelectedInventoryId] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  async function loadPageData() {
    setLoading(true);
    setError(null);

    try {
      const [transactionRows, inventoryRows] = await Promise.all([
        getTransactions(),
        getInventory(),
      ]);
      setTransactions(transactionRows);
      setInventory(inventoryRows);
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadPageData();
  }, []);

  async function submitTransaction(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const inventoryRow = inventory.find((row) => row.id === selectedInventoryId);

    if (!inventoryRow) {
      setError(new Error("Choose a product and warehouse."));
      return;
    }

    const form = new FormData(event.currentTarget);

    try {
      await createTransaction({
        productId: inventoryRow.productId,
        warehouseId: inventoryRow.warehouseId,
        quantity: Number(form.get("quantity") ?? 0),
        type: String(form.get("type")) as CreateInventoryTransactionRequest["type"],
        reference: String(form.get("reference") ?? ""),
      });
      event.currentTarget.reset();
      setSelectedInventoryId(0);
      await loadPageData();
    } catch (err) {
      setError(err);
    }
  }

  return (
    <RequireRole allowed={[ROLES.Admin, ROLES.InventoryManager]}>
      <main className="page">
        <header className="page-header">
          <h1>Inventory transactions</h1>
          <p>History of stock movements and adjustments.</p>
        </header>

        <FormError error={error} />

        <section className="card">
          <h2>Process transaction</h2>
          <form className="stacked-form" onSubmit={submitTransaction}>
            <label>
              Product @ warehouse
              <select
                value={selectedInventoryId || ""}
                onChange={(event) => setSelectedInventoryId(Number(event.target.value))}
                required
              >
                <option value="">Choose inventory row</option>
                {inventory.map((row) => (
                  <option key={row.id} value={row.id}>
                    {row.productName} @ {row.warehouseName}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Type
              <select name="type" defaultValue="StockIn">
                {transactionTypes.map((type) => (
                  <option key={type} value={type}>
                    {type}
                  </option>
                ))}
              </select>
            </label>
            <TextInput label="Quantity" name="quantity" type="number" required />
            <TextInput label="Reference" name="reference" />
            <button type="submit" className="btn btn-primary">
              Create transaction
            </button>
          </form>
        </section>

        {loading ? (
          <p className="loading-state">Loading…</p>
        ) : transactions.length === 0 ? (
          <p className="empty-state">No transactions found.</p>
        ) : (
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Warehouse</th>
                  <th>Type</th>
                  <th>Quantity</th>
                  <th>Reference</th>
                  <th>Created</th>
                </tr>
              </thead>
              <tbody>
                {transactions.map((transaction) => (
                  <tr key={transaction.id}>
                    <td>{transaction.productName}</td>
                    <td>{transaction.warehouseName}</td>
                    <td>{transaction.type}</td>
                    <td>{transaction.quantity}</td>
                    <td>{transaction.reference ?? "—"}</td>
                    <td>{new Date(transaction.createdAt).toLocaleString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </main>
    </RequireRole>
  );
}
