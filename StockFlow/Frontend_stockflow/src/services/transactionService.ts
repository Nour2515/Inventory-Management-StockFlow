import { apiClient } from "../api/client";
import type {
  CreateInventoryTransactionRequest,
  InventoryTransactionResponse,
} from "../types/api";

export async function getTransactions() {
  const response = await apiClient.get<InventoryTransactionResponse[]>("/api/InventoryTransactions");
  return response.data;
}

export async function createTransaction(request: CreateInventoryTransactionRequest) {
  const response = await apiClient.post<InventoryTransactionResponse>("/api/InventoryTransactions", request);
  return response.data;
}
