import { HubConnectionState } from "@microsoft/signalr";
import { useEffect, useState } from "react";
import { getAccessToken } from "../api/tokenStore";
import { createInventoryHubConnection } from "../services/signalRService";
import type { InventoryUpdatedEvent, LowStockAlertEvent } from "../types/api";

function dispatchStatus(status: string) {
  window.dispatchEvent(new CustomEvent("stockflow:signalr-status", { detail: status }));
}

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
      dispatchStatus("Disconnected");
      return;
    }

    const connection = createInventoryHubConnection(
      (event: InventoryUpdatedEvent) => {
        window.dispatchEvent(new CustomEvent("stockflow:inventory-event", { detail: event }));
        window.dispatchEvent(new Event("stockflow:inventory-updated"));
      },
      (event: LowStockAlertEvent) =>
        window.dispatchEvent(new CustomEvent("stockflow:low-stock-alert", { detail: event })),
    );

    connection.onreconnecting(() => dispatchStatus("Reconnecting"));
    connection.onreconnected(() => dispatchStatus("Connected"));
    connection.onclose(() => dispatchStatus("Disconnected"));

    dispatchStatus("Connecting");
    void connection
      .start()
      .then(() => dispatchStatus("Connected"))
      .catch(() => dispatchStatus("Disconnected"));

    return () => {
      void connection.stop();
      dispatchStatus("Disconnected");
    };
  }, [token]);

  return null;
}

export function mapHubState(state: HubConnectionState): string {
  switch (state) {
    case HubConnectionState.Connected:
      return "Connected";
    case HubConnectionState.Connecting:
      return "Connecting";
    case HubConnectionState.Reconnecting:
      return "Reconnecting";
    default:
      return "Disconnected";
  }
}
