import axios from "axios";
import { getAccessToken } from "./tokenStore";

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "https://localhost:44326",
  headers: {
    "Content-Type": "application/json",
  },
});

apiClient.interceptors.request.use((config) => {
  const token = getAccessToken();

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

export function getErrorMessage(error: unknown) {
  if (!axios.isAxiosError(error)) {
    return "Unexpected error";
  }

  const data = error.response?.data;

  if (typeof data === "string") {
    return data;
  }

  if (data?.errors) {
    return Object.values(data.errors).flat().join(" ");
  }

  return data?.message ?? data?.title ?? data?.detail ?? error.message;
}
