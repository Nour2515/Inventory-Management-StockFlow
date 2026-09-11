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

type ApiErrorBody = {
  message?: string;
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
};

export function getFieldErrors(error: unknown): Record<string, string[]> | null {
  if (!axios.isAxiosError(error)) {
    return null;
  }

  const data = error.response?.data as ApiErrorBody | undefined;
  if (data?.errors && typeof data.errors === "object") {
    return data.errors;
  }

  return null;
}

export function getErrorMessage(error: unknown) {
  if (!axios.isAxiosError(error)) {
    return "Unexpected error";
  }

  const data = error.response?.data as ApiErrorBody | string | undefined;

  if (typeof data === "string") {
    return data;
  }

  if (data?.message) {
    return data.message;
  }

  if (data?.errors) {
    return Object.values(data.errors).flat().join(" ");
  }

  return data?.title ?? data?.detail ?? error.message;
}
