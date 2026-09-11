export type ApiResult = unknown;

export type AuthResponse = {
  accessToken: string;
  refreshToken: string;
};

export type LoginRequest = {
  email: string;
  password: string;
};

export type RegisterRequest = LoginRequest & {
  firstName: string;
  lastName: string;
};

export type CreateProductRequest = {
  name: string;
  sku: string;
  description?: string;
  price: number;
  categoryId: number;
};

export type UpdateProductRequest = {
  name: string;
  sku: string;
  description?: string;
  price: number;
  categoryId: number;
  isActive: boolean;
};

export type CategoryResponse = {
  id: number;
  name: string;
  description?: string | null;
};

export type CreateCategoryRequest = {
  name: string;
  description?: string;
};

export type UpdateCategoryRequest = {
  name: string;
  description?: string;
};

export type WarehouseResponse = {
  id: number;
  name: string;
  location: string;
  isActive: boolean;
  createdAt: string;
};

export type CreateWarehouseRequest = {
  name: string;
  location: string;
};

export type UpdateWarehouseRequest = {
  name: string;
  location: string;
};

export type ProductResponse = {
  id: number;
  name: string;
  sku: string;
  description?: string | null;
  price: number;
  isActive: boolean;
  categoryId: number;
  categoryName: string;
  createdAt: string;
  updatedAt?: string | null;
};

export type CreateInventoryRequest = {
  productId: number;
  warehouseId: number;
  onhandQuantity: number;
  reorderLevel: number;
};

export type UpdateInventoryRequest = {
  reorderLevel: number;
};

export type InventoryResponse = {
  id: number;
  productId: number;
  productName: string;
  warehouseId: number;
  warehouseName: string;
  onHandQuantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  reorderLevel: number;
  updatedAt: string;
};

export type StockAvailabilityResponse = {
  productId: number;
  warehouseId: number;
  requstedQuantity: number;
  availableQuantity: number;
  isAvailable: boolean;
};

export type CreateOrderRequest = {
  items: Array<{
    productId: number;
    quantity: number;
  }>;
};

export type UpdateOrderStatusRequest = {
  status: "Pending" | "Confirmed" | "Cancelled" | "Completed";
};

export type OrderItemResponse = {
  id: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
};

export type OrderResponse = {
  id: number;
  userId: number;
  status: UpdateOrderStatusRequest["status"];
  totalAmount: number;
  createdAt: string;
  updatedAt?: string | null;
  items: OrderItemResponse[];
};

export type CreateReservationRequest = {
  orderId: number;
  productId: number;
  warehouseId: number;
  quantity: number;
};

export type ReservationResponse = {
  id: number;
  orderId: number;
  productId: number;
  productName: string;
  warehouseId: number;
  warehouseName: string;
  quantity: number;
  expiresAt: string;
  status: "Active" | "Released" | "Expired" | "Cancelled";
  createdAt: string;
};

export type CreateInventoryTransactionRequest = {
  productId: number;
  warehouseId: number;
  quantity: number;
  type:
    | "StockIn"
    | "Sale"
    | "Return"
    | "TransferIn"
    | "TransferOut"
    | "Reservation"
    | "ReservationReleased";
  reference?: string;
};

export type InventoryTransactionResponse = {
  id: number;
  productId: number;
  productName: string;
  warehouseId: number;
  warehouseName: string;
  quantity: number;
  type: CreateInventoryTransactionRequest["type"];
  reference?: string | null;
  createdAt: string;
};

export type InventoryUpdatedEvent = {
  inventoryId: number;
  productId: number;
  warehouseId: number;
  onHandQuantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  reason: string;
};

export type LowStockAlertEvent = {
  inventoryId: number;
  productId: number;
  productName: string;
  warehouseId: number;
  warehouseName?: string;
  availableQuantity: number;
  reorderLevel: number;
};
