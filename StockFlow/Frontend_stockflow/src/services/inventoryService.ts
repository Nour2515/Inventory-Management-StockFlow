import { apiClient } from "../api/client";
import type {
  InventoryResponse,
  StockAvailabilityResponse,
  UpdateInventoryRequest,
} from "../types/api";

export async function getInventory() {
  const response = await apiClient.get<InventoryResponse[]>("/api/Inventory");
  return response.data;
}

export async function getAvailability(productId: number, warehouseId: number, quantity: number) {
  const response = await apiClient.get<StockAvailabilityResponse>("/api/Inventory/availability", {
    params: { productId, warehouseId, quantity },
  });
  return response.data;
}

export async function updateInventory(id: number, request: UpdateInventoryRequest) {
  const response = await apiClient.put<InventoryResponse>(`/api/Inventory/${id}`, request);
  return response.data;
}
