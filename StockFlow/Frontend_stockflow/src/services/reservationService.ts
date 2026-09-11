import { apiClient } from "../api/client";
import type { ApiResult, CreateReservationRequest, ReservationResponse } from "../types/api";

export async function createReservation(request: CreateReservationRequest) {
  const response = await apiClient.post<ReservationResponse>("/api/StockReservations", request);
  return response.data;
}

export async function getReservationsByOrder(orderId: number) {
  const response = await apiClient.get<ReservationResponse[]>(
    `/api/StockReservations/order/${orderId}`,
  );
  return response.data;
}

export async function releaseReservation(id: number) {
  const response = await apiClient.put<ApiResult>(`/api/StockReservations/${id}/release`);
  return response.data;
}

export async function cancelReservation(id: number) {
  const response = await apiClient.put<ApiResult>(`/api/StockReservations/cancel/${id}`);
  return response.data;
}
