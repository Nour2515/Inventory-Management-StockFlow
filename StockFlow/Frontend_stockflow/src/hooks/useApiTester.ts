import { useState } from "react";
import { getErrorMessage } from "../api/client";

export function useApiTester() {
  const [result, setResult] = useState<unknown>(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function run(action: () => Promise<unknown>) {
    setLoading(true);
    setError("");

    try {
      const data = await action();
      setResult(data ?? { ok: true });
    } catch (err) {
      setResult(null);
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }

  return { result, error, loading, run };
}
