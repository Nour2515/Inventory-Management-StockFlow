import { apiClient } from "../api/client";
import type { CategoryResponse, CreateCategoryRequest, UpdateCategoryRequest } from "../types/api";

export async function getCategories() {
  const response = await apiClient.get<CategoryResponse[]>("/api/Categories");
  return response.data;
}

export async function createCategory(request: CreateCategoryRequest) {
  const response = await apiClient.post<CategoryResponse>("/api/Categories/Add", request);
  return response.data;
}

export async function updateCategory(id: number, request: UpdateCategoryRequest) {
  const response = await apiClient.put<CategoryResponse>(`/api/Categories/${id}`, request);
  return response.data;
}

export async function deleteCategory(id: number) {
  await apiClient.delete(`/api/Categories/${id}`);
}
