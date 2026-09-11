import { useEffect, useState } from "react";
import { getAccessToken } from "../api/tokenStore";
import { createInventoryHubConnection } from "../services/signalRService";

export function SignalRListener() {
  const [token, setToken] = useState(() => getAccessToken());

  useEffect(() => {
    function onAuthChanged() {
      setToken(getAccessToken());
    }

    window.addEventListener("stockflow:auth-changed", onAuthChanged);
    return () => window.removeEventListener("stockflow:auth-changed", onAuthChanged);
  }, []);

  useEffect(() => {
    if (!token) {
      return;
    }

    const connection = createInventoryHubConnection(
      () => window.dispatchEvent(new Event("stockflow:inventory-updated")),
      (event) =>
        window.dispatchEvent(
          new CustomEvent("stockflow:low-stock-alert", { detail: event }),
        ),
    );

    void connection.start().catch(() => undefined);

    return () => {
      void connection.stop();
    };
  }, [token]);

  return null;
}
