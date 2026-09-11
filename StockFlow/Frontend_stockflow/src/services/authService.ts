import { apiClient } from "../api/client";
import type { AuthResponse, LoginRequest, RegisterRequest } from "../types/api";

export async function register(request: RegisterRequest) {
  const response = await apiClient.post<AuthResponse>("/api/Auth/register", request);
  return response.data;
}

export async function login(request: LoginRequest) {
  const response = await apiClient.post<AuthResponse>("/api/Auth/login", request);
  return response.data;
}
