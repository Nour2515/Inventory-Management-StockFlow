import { useEffect, useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { FormError } from "../components/FormError";
import { TextInput } from "../components/TextInput";
import { useAuth } from "../hooks/useAuth";
import { getCategories } from "../services/categoryService";
import {
  createProduct,
  deactivateProduct,
  getProducts,
  updateProduct,
} from "../services/productService";
import type { CategoryResponse, ProductResponse } from "../types/api";
import { canCreateOrder, canManageProducts } from "../utils/auth";

export function ProductsPage() {
  const auth = useAuth();
  const isAdmin = canManageProducts(auth.roles);
  const showOrderLink = auth.isAuthenticated && canCreateOrder(auth.roles);

  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [categories, setCategories] = useState<CategoryResponse[]>([]);
  const [editing, setEditing] = useState<ProductResponse | null>(null);
  const [showCreate, setShowCreate] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [loading, setLoading] = useState(true);

  async function loadProducts() {
    setLoading(true);
    setError(null);

    try {
      const [productRows, categoryRows] = await Promise.all([getProducts(), getCategories()]);
      setProducts(productRows);
      setCategories(categoryRows);
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadProducts();
  }, []);

  async function submitCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);

    try {
      await createProduct({
        name: String(form.get("name") ?? ""),
        sku: String(form.get("sku") ?? ""),
        description: String(form.get("description") ?? ""),
        price: Number(form.get("price") ?? 0),
        categoryId: Number(form.get("categoryId") ?? 0),
      });
      setShowCreate(false);
      event.currentTarget.reset();
      await loadProducts();
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
      await updateProduct(editing.id, {
        name: String(form.get("name") ?? ""),
        sku: String(form.get("sku") ?? ""),
        description: String(form.get("description") ?? ""),
        price: Number(form.get("price") ?? 0),
        categoryId: Number(form.get("categoryId") ?? 0),
        isActive: form.get("isActive") === "on",
      });
      setEditing(null);
      await loadProducts();
    } catch (err) {
      setError(err);
    }
  }

  async function deactivate(id: number) {
    if (!window.confirm("Deactivate this product?")) {
      return;
    }

    try {
      await deactivateProduct(id);
      await loadProducts();
    } catch (err) {
      setError(err);
    }
  }

  return (
    <main className="page">
      <header className="page-header">
        <h1>Products</h1>
        <p>Catalog of items available in StockFlow.</p>
      </header>

      <FormError error={error} />

      {isAdmin && (
        <div className="row-actions" style={{ marginBottom: 16 }}>
          <button type="button" className="btn btn-primary" onClick={() => setShowCreate((v) => !v)}>
            {showCreate ? "Cancel" : "Add product"}
          </button>
        </div>
      )}

      {isAdmin && showCreate && (
        <section className="card">
          <h2>New product</h2>
          <form className="stacked-form" onSubmit={submitCreate}>
            <TextInput label="Name" name="name" required />
            <TextInput label="SKU" name="sku" required />
            <TextInput label="Description" name="description" />
            <TextInput label="Price" name="price" type="number" required />
            <label>
              Category
              <select name="categoryId" required defaultValue="">
                <option value="" disabled>
                  Select category
                </option>
                {categories.map((cat) => (
                  <option key={cat.id} value={cat.id}>
                    {cat.name}
                  </option>
                ))}
              </select>
            </label>
            <button type="submit" className="btn btn-primary">
              Create product
            </button>
          </form>
        </section>
      )}

      {editing && isAdmin && (
        <section className="card">
          <h2>Edit {editing.name}</h2>
          <form className="stacked-form" onSubmit={submitEdit}>
            <TextInput label="Name" name="name" required defaultValue={editing.name} />
            <TextInput label="SKU" name="sku" required defaultValue={editing.sku} />
            <TextInput
              label="Description"
              name="description"
              defaultValue={editing.description ?? ""}
            />
            <TextInput
              label="Price"
              name="price"
              type="number"
              required
              defaultValue={String(editing.price)}
            />
            <label>
              Category
              <select name="categoryId" required defaultValue={editing.categoryId}>
                {categories.map((cat) => (
                  <option key={cat.id} value={cat.id}>
                    {cat.name}
                  </option>
                ))}
              </select>
            </label>
            <label style={{ flexDirection: "row", alignItems: "center", gap: 8 }}>
              <input type="checkbox" name="isActive" defaultChecked={editing.isActive} />
              Active
            </label>
            <div className="row-actions">
              <button type="submit" className="btn btn-primary">
                Save changes
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
      ) : products.length === 0 ? (
        <p className="empty-state">No products found.</p>
      ) : (
        <div className="table-wrap">
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>SKU</th>
                <th>Price</th>
                <th>Category</th>
                <th>Status</th>
                {(isAdmin || showOrderLink) && <th>Actions</th>}
              </tr>
            </thead>
            <tbody>
              {products.map((product) => (
                <tr key={product.id}>
                  <td>{product.name}</td>
                  <td>{product.sku}</td>
                  <td>{product.price.toFixed(2)}</td>
                  <td>{product.categoryName}</td>
                  <td>
                    <span className={product.isActive ? "badge badge-active" : "badge badge-inactive"}>
                      {product.isActive ? "Active" : "Inactive"}
                    </span>
                  </td>
                  {(isAdmin || showOrderLink) && (
                    <td>
                      <div className="row-actions">
                        {showOrderLink && product.isActive && (
                          <Link to="/orders" state={{ productId: product.id }} className="btn btn-ghost btn-sm">
                            Order
                          </Link>
                        )}
                        {isAdmin && (
                          <>
                            <button
                              type="button"
                              className="btn btn-ghost btn-sm"
                              onClick={() => setEditing(product)}
                            >
                              Edit
                            </button>
                            {product.isActive && (
                              <button
                                type="button"
                                className="btn btn-danger btn-sm"
                                onClick={() => void deactivate(product.id)}
                              >
                                Deactivate
                              </button>
                            )}
                          </>
                        )}
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
