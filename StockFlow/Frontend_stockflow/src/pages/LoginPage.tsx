import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { saveTokens } from "../api/tokenStore";
import { ResultPanel } from "../components/ResultPanel";
import { TextInput } from "../components/TextInput";
import { useApiTester } from "../hooks/useApiTester";
import { login } from "../services/authService";

export function LoginPage() {
  const tester = useApiTester();
  const [saved, setSaved] = useState(false);
  const navigate = useNavigate();

  function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);

    tester.run(async () => {
      const response = await login({
        email: String(form.get("email") ?? ""),
        password: String(form.get("password") ?? ""),
      });

      saveTokens(response.accessToken, response.refreshToken);
      setSaved(true);
      navigate("/");
      return response;
    });
  }

  return (
    <main>
      <h1>Login</h1>
      <form onSubmit={onSubmit}>
        <TextInput label="Email" name="email" type="email" required />
        <TextInput label="Password" name="password" type="password" required />
        <button type="submit">Login</button>
      </form>
      {saved && <p className="success">Access token saved for API requests.</p>}
      <ResultPanel {...tester} />
    </main>
  );
}
