export const ROLES = {
  Admin: "Admin",
  Customer: "Customer",
  InventoryManager: "InventoryManager",
  WarehouseManager: "WarehouseManager",
} as const;

export type AppRole = (typeof ROLES)[keyof typeof ROLES];

export function hasAnyRole(userRoles: string[], allowed: string[]) {
  return allowed.some((role) => userRoles.includes(role));
}

export function canManageProducts(roles: string[]) {
  return hasAnyRole(roles, [ROLES.Admin]);
}

export function canManageCategories(roles: string[]) {
  return hasAnyRole(roles, [ROLES.Admin]);
}

export function canViewWarehouses(roles: string[]) {
  return roles.length > 0;
}

export function canCreateWarehouse(roles: string[]) {
  return hasAnyRole(roles, [ROLES.Admin]);
}

export function canEditWarehouse(roles: string[]) {
  return hasAnyRole(roles, [ROLES.Admin, ROLES.WarehouseManager]);
}

export function canAccessInventory(roles: string[]) {
  return hasAnyRole(roles, [ROLES.Admin, ROLES.InventoryManager]);
}

export function canAccessReservations(roles: string[]) {
  return hasAnyRole(roles, [ROLES.Admin, ROLES.InventoryManager]);
}

export function canAccessTransactions(roles: string[]) {
  return hasAnyRole(roles, [ROLES.Admin, ROLES.InventoryManager]);
}

export function canCreateOrder(roles: string[]) {
  return hasAnyRole(roles, [ROLES.Customer]);
}

export function canViewMyOrders(roles: string[]) {
  return hasAnyRole(roles, [ROLES.Customer]);
}

export function canAdminOrders(roles: string[]) {
  return hasAnyRole(roles, [ROLES.Admin]);
}

export function getDefaultHomePath(roles: string[]) {
  if (hasAnyRole(roles, [ROLES.InventoryManager])) {
    return "/inventory";
  }
  if (hasAnyRole(roles, [ROLES.Customer])) {
    return "/orders";
  }
  return "/";
}
