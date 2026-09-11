import { useEffect, useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { getErrorMessage } from "../api/client";
import { TextInput } from "../components/TextInput";
import { createProduct, getProductById, getProducts } from "../services/productService";
import type { ProductResponse } from "../types/api";

export function ProductsPage() {
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [selectedProduct, setSelectedProduct] = useState<ProductResponse | null>(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function loadProducts() {
    setLoading(true);
    setError("");

    try {
      setProducts(await getProducts());
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    const timeoutId = window.setTimeout(() => void loadProducts(), 0);
    return () => window.clearTimeout(timeoutId);
  }, []);

  async function selectProduct(id: number) {
    setError("");

    try {
      setSelectedProduct(await getProductById(id));
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  async function create(event: FormEvent<HTMLFormElement>) {
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
      event.currentTarget.reset();
      await loadProducts();
    } catch (err) {
      setError(getErrorMessage(err));
    }
  }

  return (
    <main>
      <h1>Products</h1>
      {error && <p className="error-text">{error}</p>}
      {loading && <p>Loading products...</p>}

      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Price</th>
            <th>Active</th>
            <th>Category</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {products.map((product) => (
            <tr key={product.id}>
              <td>{product.name}</td>
              <td>{product.price}</td>
              <td>{product.isActive ? "Yes" : "No"}</td>
              <td>{product.categoryName}</td>
              <td>
                <button type="button" onClick={() => void selectProduct(product.id)}>
                  Select
                </button>
                <Link to="/orders" state={{ productId: product.id }}>
                  Order
                </Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {selectedProduct && (
        <section className="panel">
          <h2>Selected product</h2>
          <p>{selectedProduct.name}</p>
          <p>SKU: {selectedProduct.sku}</p>
          <p>Price: {selectedProduct.price}</p>
        </section>
      )}

      <form onSubmit={create}>
        <h2>Create product Admin</h2>
        <TextInput label="Name" name="name" required />
        <TextInput label="SKU" name="sku" required />
        <TextInput label="Description" name="description" />
        <TextInput label="Price" name="price" type="number" required />
        <TextInput label="Category id" name="categoryId" type="number" required />
        <button type="submit">Create product</button>
      </form>
    </main>
  );
}
