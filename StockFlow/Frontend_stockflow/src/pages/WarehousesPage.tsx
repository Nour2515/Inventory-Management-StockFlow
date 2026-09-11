import { useEffect, useState, type FormEvent } from "react";
import { FormError } from "../components/FormError";
import { RequireRole } from "../components/RequireRole";
import { TextInput } from "../components/TextInput";
import { useAuth } from "../hooks/useAuth";
import { createWarehouse, getWarehouses, updateWarehouse } from "../services/warehouseService";
import type { WarehouseResponse } from "../types/api";
import { canCreateWarehouse, canEditWarehouse, ROLES } from "../utils/auth";

export function WarehousesPage() {
  const auth = useAuth();
  const canCreate = canCreateWarehouse(auth.roles);
  const canEdit = canEditWarehouse(auth.roles);

  const [warehouses, setWarehouses] = useState<WarehouseResponse[]>([]);
  const [editing, setEditing] = useState<WarehouseResponse | null>(null);
  const [showCreate, setShowCreate] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      setWarehouses(await getWarehouses());
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (auth.isAuthenticated) {
      void load();
    }
  }, [auth.isAuthenticated]);

  async function submitCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    try {
      await createWarehouse({
        name: String(form.get("name") ?? ""),
        location: String(form.get("location") ?? ""),
      });
      setShowCreate(false);
      event.currentTarget.reset();
      await load();
    } catch (err) {
      setError(err);
    }
  }

  async function submitEdit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!editing) {
      return;
    }
    const form = new FormData(event.currentTarget);
    try {
      await updateWarehouse(editing.id, {
        name: String(form.get("name") ?? ""),
        location: String(form.get("location") ?? ""),
      });
      setEditing(null);
      await load();
    } catch (err) {
      setError(err);
    }
  }

  return (
    <RequireRole
      allowed={[ROLES.Admin, ROLES.WarehouseManager, ROLES.InventoryManager, ROLES.Customer]}
      title="Warehouses unavailable"
    >
      <main className="page">
        <header className="page-header">
          <h1>Warehouses</h1>
          <p>Storage locations for inventory.</p>
        </header>

        <FormError error={error} />

        {canCreate && (
          <div className="row-actions" style={{ marginBottom: 16 }}>
            <button type="button" className="btn btn-primary" onClick={() => setShowCreate((v) => !v)}>
              {showCreate ? "Cancel" : "Add warehouse"}
            </button>
          </div>
        )}

        {canCreate && showCreate && (
          <section className="card">
            <h2>New warehouse</h2>
            <form className="stacked-form" onSubmit={submitCreate}>
              <TextInput label="Name" name="name" required />
              <TextInput label="Location" name="location" required />
              <button type="submit" className="btn btn-primary">
                Create warehouse
              </button>
            </form>
          </section>
        )}

        {editing && canEdit && (
          <section className="card">
            <h2>Edit warehouse</h2>
            <form className="stacked-form" onSubmit={submitEdit}>
              <TextInput label="Name" name="name" required defaultValue={editing.name} />
              <TextInput
                label="Location"
                name="location"
                required
                defaultValue={editing.location}
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
        ) : warehouses.length === 0 ? (
          <p className="empty-state">No warehouses found.</p>
        ) : (
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Location</th>
                  <th>Status</th>
                  {canEdit && <th>Actions</th>}
                </tr>
              </thead>
              <tbody>
                {warehouses.map((warehouse) => (
                  <tr key={warehouse.id}>
                    <td>{warehouse.name}</td>
                    <td>{warehouse.location}</td>
                    <td>
                      <span
                        className={
                          warehouse.isActive ? "badge badge-active" : "badge badge-inactive"
                        }
                      >
                        {warehouse.isActive ? "Active" : "Inactive"}
                      </span>
                    </td>
                    {canEdit && (
                      <td>
                        <button
                          type="button"
                          className="btn btn-ghost btn-sm"
                          onClick={() => setEditing(warehouse)}
                        >
                          Edit
                        </button>
                      </td>
                    )}
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
