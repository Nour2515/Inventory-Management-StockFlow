import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { saveTokens } from "../api/tokenStore";
import { FormError } from "../components/FormError";
import { TextInput } from "../components/TextInput";
import { getDefaultHomePath } from "../utils/auth";
import { login } from "../services/authService";

export function LoginPage() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const navigate = useNavigate();

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setError(null);
    const form = new FormData(event.currentTarget);

    try {
      const response = await login({
        email: String(form.get("email") ?? ""),
        password: String(form.get("password") ?? ""),
      });

      saveTokens(response.accessToken, response.refreshToken);
      const payload = response.accessToken.split(".")[1];
      const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
      const claims = JSON.parse(window.atob(normalized)) as {
        role?: string | string[];
        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?: string | string[];
      };
      const roleClaim =
        claims.role ??
        claims["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
      const roles = Array.isArray(roleClaim) ? roleClaim : roleClaim ? [roleClaim] : [];
      navigate(getDefaultHomePath(roles));
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-card">
      <h1>Sign in to StockFlow</h1>
      <p className="auth-subtitle">Manage inventory, orders, and stock in one place.</p>
      <FormError error={error} />
      <form className="stacked-form" onSubmit={onSubmit}>
        <TextInput label="Email" name="email" type="email" required />
        <TextInput label="Password" name="password" type="password" required />
        <button type="submit" className="btn btn-primary" disabled={loading}>
          {loading ? "Signing in…" : "Login"}
        </button>
      </form>
      <p className="auth-footer">
        No account? <Link to="/register">Register</Link>
      </p>
    </div>
  );
}
