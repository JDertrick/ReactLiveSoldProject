import { createBrowserRouter, Navigate } from "react-router-dom";
import { useAuthStore } from "../store/authStore";

// Layouts
import AppLayout from "../components/common/AppLayout";
import PortalLayout from "../components/common/PortalLayout";

// Auth Pages
import EmployeeLogin from "../pages/auth/EmployeeLogin";
import CustomerPortalLogin from "../pages/auth/CustomerPortalLogin";
import CustomerPortalRegister from "../pages/auth/CustomerPortalRegister";

// SuperAdmin Pages
import SuperAdminDashboard from "../pages/superadmin/Dashboard";
import OrganizationsPage from "../pages/superadmin/Organizations";

// App Pages (Seller/Owner)
import AppDashboard from "../pages/app/Dashboard";
import ContactsPage from "../pages/app/Contacts";
import CustomersPage from "../pages/app/Customers";
import VendorsPage from "../pages/app/Vendors";
import ProductsPage from "../pages/app/Products";
import LiveSalesPage from "../pages/app/LiveSales";
import WalletPage from "../pages/app/Wallet";
import StockMovementsPage from "../pages/app/StockMovements";
import CustomerWalletPage from "../pages/app/CustomerWalletPage";
import CategoryListPage from "../pages/app/CategoryListPage";
import LocationListPage from "../pages/app/LocationListPage";
import TaxSettings from "../pages/app/TaxSettings";
import InventoryAuditPage from "../pages/app/InventoryAudit";
import InventoryAuditDetailPage from "../pages/app/InventoryAuditDetail";
import InventoryAuditCountPage from "../pages/app/InventoryAuditCount";

// Purchase Pages
import PurchaseOrdersPage from "../pages/app/PurchaseOrders";
import PurchaseReceiptsPage from "../pages/app/PurchaseReceipts";
import VendorInvoicesPage from "../pages/app/VendorInvoices";
import PaymentsPage from "../pages/app/Payments";

// Portal Pages (Customer)
import CustomerDashboard from "../pages/portal/Dashboard";
import CustomerOrders from "../pages/portal/Orders";
import OrganizationUsersPage from "../pages/superadmin/OrganizationUsers";
import AllReceiptsPage from "../pages/app/AllReceiptsPage";
import AccountingPage from "@/pages/app/Accounting";

// Protected Route Component
const ProtectedRoute = ({
  children,
  requiredRole,
}: {
  children: React.ReactNode;
  requiredRole?: string;
}) => {
  const { isAuthenticated, user } = useAuthStore();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (requiredRole && user?.role !== requiredRole) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
};

export const router = createBrowserRouter([
  {
    path: "/",
    element: <Navigate to="/login" replace />,
  },
  {
    path: "/login",
    element: <EmployeeLogin />,
  },
  {
    path: "/portal/:orgSlug/login",
    element: <CustomerPortalLogin />,
  },
  {
    path: "/portal/:orgSlug/register",
    element: <CustomerPortalRegister />,
  },
  {
    path: "/superadmin",
    element: (
      <ProtectedRoute requiredRole="SuperAdmin">
        <AppLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        index: true,
        element: <SuperAdminDashboard />,
      },
      {
        path: "organizations",
        element: <OrganizationsPage />,
      },
      {
        path: "organizations/:organizationId/users",
        element: <OrganizationUsersPage />,
      },
    ],
  },
  {
    path: "/app",
    element: (
      <ProtectedRoute>
        <AppLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        index: true,
        element: <AppDashboard />,
      },
      {
        path: "contacts",
        element: <ContactsPage />,
      },
      {
        path: "customers",
        element: <CustomersPage />,
      },
      {
        path: "customers/:customerId/wallet",
        element: <CustomerWalletPage />,
      },
      {
        path: "vendors",
        element: <VendorsPage />,
      },
      {
        path: "products",
        element: <ProductsPage />,
      },
      {
        path: "categories",
        element: <CategoryListPage />,
      },
      {
        path: "locations",
        element: <LocationListPage />,
      },
      {
        path: "live-sales",
        element: <LiveSalesPage />,
      },
      {
        path: "wallet",
        element: <WalletPage />,
      },
      {
        path: "stock-movements",
        element: <StockMovementsPage />,
      },
      {
        path: "receipts",
        element: <AllReceiptsPage />,
      },
      {
        path: "tax-settings",
        element: <TaxSettings />,
      },
      {
        path: "inventory-audit",
        element: <InventoryAuditPage />,
      },
      {
        path: "inventory-audit/:auditId",
        element: <InventoryAuditDetailPage />,
      },
      {
        path: "inventory-audit/:auditId/count",
        element: <InventoryAuditCountPage />,
      },
      {
        path: "accounting",
        element: <AccountingPage />,
      },
      // Purchase Module Routes
      {
        path: "purchase-orders",
        element: <PurchaseOrdersPage />,
      },
      {
        path: "purchase-receipts",
        element: <PurchaseReceiptsPage />,
      },
      {
        path: "vendor-invoices",
        element: <VendorInvoicesPage />,
      },
      {
        path: "payments",
        element: <PaymentsPage />,
      },
    ],
  },
  // ... (resto de la configuración)
  {
    path: "/portal/:orgSlug",
    element: (
      <ProtectedRoute requiredRole="Customer">
        <PortalLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        path: "dashboard",
        element: <CustomerDashboard />,
      },
      {
        path: "orders",
        element: <CustomerOrders />,
      },
    ],
  },
]);
