import * as signalR from "@microsoft/signalr";
import { getAccessToken } from "../api/tokenStore";
import type { InventoryUpdatedEvent, LowStockAlertEvent } from "../types/api";

const baseUrl = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:44326";

export function createInventoryHubConnection(
  onInventoryUpdated: (event: InventoryUpdatedEvent) => void,
  onLowStockAlert: (event: LowStockAlertEvent) => void,
) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/inventory`, {
      accessTokenFactory: () => getAccessToken() ?? "",
    })
    .withAutomaticReconnect()
    .build();

  connection.on("InventoryUpdated", onInventoryUpdated);
  connection.on("LowStockAlert", onLowStockAlert);

  return connection;
}
