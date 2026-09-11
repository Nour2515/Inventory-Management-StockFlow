import { Route, Routes } from "react-router-dom";
import { NavMenu } from "./components/NavMenu";
import { SignalRListener } from "./components/SignalRListener";
import { HomePage } from "./pages/HomePage";
import { InventoryPage } from "./pages/InventoryPage";
import { LoginPage } from "./pages/LoginPage";
import { OrdersPage } from "./pages/OrdersPage";
import { ProductsPage } from "./pages/ProductsPage";
import { RegisterPage } from "./pages/RegisterPage";
import { ReservationsPage } from "./pages/ReservationsPage";
import { SignalRPage } from "./pages/SignalRPage";
import { TransactionsPage } from "./pages/TransactionsPage";

function App() {
  return (
    <div>
      <SignalRListener />
      <NavMenu />
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/products" element={<ProductsPage />} />
        <Route path="/inventory" element={<InventoryPage />} />
        <Route path="/orders" element={<OrdersPage />} />
        <Route path="/reservations" element={<ReservationsPage />} />
        <Route path="/transactions" element={<TransactionsPage />} />
        <Route path="/signalr" element={<SignalRPage />} />
      </Routes>
    </div>
  );
}

export default App;
