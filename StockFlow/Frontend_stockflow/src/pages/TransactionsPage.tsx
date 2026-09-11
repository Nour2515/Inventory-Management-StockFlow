import { useEffect, useState, type FormEvent } from "react";
import { getErrorMessage } from "../api/client";
import { TextInput } from "../components/TextInput";
import { getInventory } from "../services/inventoryService";
import { createTransaction, getTransactions } from "../services/transactionService";
import type {
  CreateInventoryTransactionRequest,
  InventoryResponse,
  InventoryTransactionResponse,
} from "../types/api";

const transactionTypes: CreateInventoryTransactionRequest["type"][] = [
  "StockIn",
  "Sale",
  "Return",
  "TransferIn",
  "TransferOut",
];

export function TransactionsPage() {
  const [transactions, setTransactions] = useState<InventoryTransactionResponse[]>([]);
  const [inventory, setInventory] = useState<InventoryResponse[]>([]);
  const [selectedInventoryId, setSelectedInventoryId] = useState(0);
  const [error, setError] = useState("");

  async function loadPageData() {
    setError("");

    try {
      const [transactionRows, inventoryRows] = await Promise.all([
        getTransactions(),
        getInventory(),
      ]);
      setTransactions(transactionRows);
      setInventory(inventoryRows);
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  useEffect(() => {
    const timeoutId = window.setTimeout(() => void loadPageData(), 0);
    return () => window.clearTimeout(timeoutId);
  }, []);

  async function submitTransaction(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const inventoryRow = inventory.find((row) => row.id === selectedInventoryId);

    if (!inventoryRow) {
      setError("Choose an inventory row.");
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
      setError(getErrorMessage(err));
    }
  }

  return (
    <main>
      <h1>Transactions</h1>
      {error && <p className="error-text">{error}</p>}

      <form onSubmit={submitTransaction}>
        <h2>Create transaction</h2>
        <label>
          Inventory row
          <select
            value={selectedInventoryId || ""}
            onChange={(event) => setSelectedInventoryId(Number(event.target.value))}
            required
          >
            <option value="">Choose inventory row</option>
            {inventory.map((row) => (
              <option key={row.id} value={row.id}>
                {row.productName} at {row.warehouseName}
              </option>
            ))}
          </select>
        </label>
        <label>
          Type
          <select name="type">
            {transactionTypes.map((type) => (
              <option key={type} value={type}>
                {type}
              </option>
            ))}
          </select>
        </label>
        <TextInput label="Quantity" name="quantity" type="number" required />
        <TextInput label="Reference" name="reference" />
        <button type="submit">Create transaction</button>
      </form>

      <table>
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
              <td>{transaction.reference}</td>
              <td>{new Date(transaction.createdAt).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </main>
  );
}
