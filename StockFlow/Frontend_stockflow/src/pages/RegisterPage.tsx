import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { saveTokens } from "../api/tokenStore";
import { FormError } from "../components/FormError";
import { TextInput } from "../components/TextInput";
import { register } from "../services/authService";

export function RegisterPage() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const navigate = useNavigate();

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setError(null);
    const form = new FormData(event.currentTarget);

    try {
      const response = await register({
        firstName: String(form.get("firstName") ?? ""),
        lastName: String(form.get("lastName") ?? ""),
        email: String(form.get("email") ?? ""),
        password: String(form.get("password") ?? ""),
      });

      saveTokens(response.accessToken, response.refreshToken);
      navigate("/orders");
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-card">
      <h1>Create account</h1>
      <p className="auth-subtitle">New accounts receive the Customer role by default.</p>
      <FormError error={error} />
      <form className="stacked-form" onSubmit={onSubmit}>
        <TextInput label="First name" name="firstName" required />
        <TextInput label="Last name" name="lastName" required />
        <TextInput label="Email" name="email" type="email" required />
        <TextInput label="Password" name="password" type="password" required />
        <button type="submit" className="btn btn-primary" disabled={loading}>
          {loading ? "Creating account…" : "Register"}
        </button>
      </form>
      <p className="auth-footer">
        Already have an account? <Link to="/login">Login</Link>
      </p>
    </div>
  );
}
