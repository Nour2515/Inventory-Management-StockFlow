import { useEffect, useState, type FormEvent } from "react";
import { FormError } from "../components/FormError";
import { TextInput } from "../components/TextInput";
import { useAuth } from "../hooks/useAuth";
import {
  createCategory,
  deleteCategory,
  getCategories,
  updateCategory,
} from "../services/categoryService";
import type { CategoryResponse } from "../types/api";
import { canManageCategories } from "../utils/auth";

export function CategoriesPage() {
  const auth = useAuth();
  const isAdmin = canManageCategories(auth.roles);
  const [categories, setCategories] = useState<CategoryResponse[]>([]);
  const [editing, setEditing] = useState<CategoryResponse | null>(null);
  const [showCreate, setShowCreate] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      setCategories(await getCategories());
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  async function submitCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    try {
      await createCategory({
        name: String(form.get("name") ?? ""),
        description: String(form.get("description") ?? ""),
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
      await updateCategory(editing.id, {
        name: String(form.get("name") ?? ""),
        description: String(form.get("description") ?? ""),
      });
      setEditing(null);
      await load();
    } catch (err) {
      setError(err);
    }
  }

  async function remove(id: number) {
    if (!window.confirm("Delete this category?")) {
      return;
    }
    try {
      await deleteCategory(id);
      await load();
    } catch (err) {
      setError(err);
    }
  }

  return (
    <main className="page">
      <header className="page-header">
        <h1>Categories</h1>
        <p>Product groupings used across the catalog.</p>
      </header>

      <FormError error={error} />

      {isAdmin && (
        <div className="row-actions" style={{ marginBottom: 16 }}>
          <button type="button" className="btn btn-primary" onClick={() => setShowCreate((v) => !v)}>
            {showCreate ? "Cancel" : "Add category"}
          </button>
        </div>
      )}

      {isAdmin && showCreate && (
        <section className="card">
          <h2>New category</h2>
          <form className="stacked-form" onSubmit={submitCreate}>
            <TextInput label="Name" name="name" required />
            <TextInput label="Description" name="description" />
            <button type="submit" className="btn btn-primary">
              Create
            </button>
          </form>
        </section>
      )}

      {editing && isAdmin && (
        <section className="card">
          <h2>Edit category</h2>
          <form className="stacked-form" onSubmit={submitEdit}>
            <TextInput label="Name" name="name" required defaultValue={editing.name} />
            <TextInput
              label="Description"
              name="description"
              defaultValue={editing.description ?? ""}
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
      ) : categories.length === 0 ? (
        <p className="empty-state">No categories found.</p>
      ) : (
        <div className="table-wrap">
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Description</th>
                {isAdmin && <th>Actions</th>}
              </tr>
            </thead>
            <tbody>
              {categories.map((category) => (
                <tr key={category.id}>
                  <td>{category.name}</td>
                  <td>{category.description ?? "—"}</td>
                  {isAdmin && (
                    <td>
                      <div className="row-actions">
                        <button
                          type="button"
                          className="btn btn-ghost btn-sm"
                          onClick={() => setEditing(category)}
                        >
                          Edit
                        </button>
                        <button
                          type="button"
                          className="btn btn-danger btn-sm"
                          onClick={() => void remove(category.id)}
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </main>
  );
}
