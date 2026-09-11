import { apiClient } from "../api/client";
import type { ApiResult, CreateOrderRequest, OrderResponse, UpdateOrderStatusRequest } from "../types/api";

export async function createOrder(request: CreateOrderRequest) {
  const response = await apiClient.post<OrderResponse>("/api/Orders", request);
  return response.data;
}

export async function getMyOrders() {
  const response = await apiClient.get<OrderResponse[]>("/api/Orders/My_Orders");
  return response.data;
}

export async function updateOrderStatus(id: number, request: UpdateOrderStatusRequest) {
  const response = await apiClient.put<ApiResult>(`/api/Orders/${id}/status`, request);
  return response.data;
}
