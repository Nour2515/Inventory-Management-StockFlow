import { Route, Routes } from "react-router-dom";
import { AppLayout } from "./components/AppLayout";
import { SignalRListener } from "./components/SignalRListener";
import { CategoriesPage } from "./pages/CategoriesPage";
import { HomePage } from "./pages/HomePage";
import { InventoryPage } from "./pages/InventoryPage";
import { LoginPage } from "./pages/LoginPage";
import { OrdersPage } from "./pages/OrdersPage";
import { ProductsPage } from "./pages/ProductsPage";
import { RegisterPage } from "./pages/RegisterPage";
import { ReservationsPage } from "./pages/ReservationsPage";
import { SignalRPage } from "./pages/SignalRPage";
import { TransactionsPage } from "./pages/TransactionsPage";
import { WarehousesPage } from "./pages/WarehousesPage";

function App() {
  return (
    <>
      <SignalRListener />
      <Routes>
        <Route element={<AppLayout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/products" element={<ProductsPage />} />
          <Route path="/categories" element={<CategoriesPage />} />
          <Route path="/warehouses" element={<WarehousesPage />} />
          <Route path="/inventory" element={<InventoryPage />} />
          <Route path="/orders" element={<OrdersPage />} />
          <Route path="/reservations" element={<ReservationsPage />} />
          <Route path="/transactions" element={<TransactionsPage />} />
          <Route path="/signalr" element={<SignalRPage />} />
        </Route>
      </Routes>
    </>
  );
}

export default App;
