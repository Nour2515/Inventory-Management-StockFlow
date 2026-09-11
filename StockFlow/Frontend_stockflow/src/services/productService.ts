import { apiClient } from "../api/client";
import type {
  ApiResult,
  CreateProductRequest,
  ProductResponse,
  UpdateProductRequest,
} from "../types/api";

export async function getProducts() {
  const response = await apiClient.get<ProductResponse[]>("/api/Products");
  return response.data;
}

export async function getProductById(id: number) {
  const response = await apiClient.get<ProductResponse>(`/api/Products/${id}`);
  return response.data;
}

export async function createProduct(request: CreateProductRequest) {
  const response = await apiClient.post<ApiResult>("/api/Products/Add", request);
  return response.data;
}

export async function updateProduct(id: number, request: UpdateProductRequest) {
  const response = await apiClient.put<ProductResponse>(`/api/Products/${id}`, request);
  return response.data;
}

export async function deactivateProduct(id: number) {
  await apiClient.patch(`/api/Products/Deactivate/${id}`);
}
