import { apiClient } from "../api/client";
import type {
  CreateWarehouseRequest,
  UpdateWarehouseRequest,
  WarehouseResponse,
} from "../types/api";

export async function getWarehouses() {
  const response = await apiClient.get<WarehouseResponse[]>("/api/Warehouses");
  return response.data;
}

export async function createWarehouse(request: CreateWarehouseRequest) {
  const response = await apiClient.post<WarehouseResponse>("/api/Warehouses", request);
  return response.data;
}

export async function updateWarehouse(id: number, request: UpdateWarehouseRequest) {
  const response = await apiClient.put<WarehouseResponse>(`/api/Warehouses/${id}`, request);
  return response.data;
}
