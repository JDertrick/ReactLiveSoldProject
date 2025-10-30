import { createBrowserRouter, Navigate } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';

// Layouts
import AppLayout from '../components/common/AppLayout';
import PortalLayout from '../components/common/PortalLayout';

// Auth Pages
import EmployeeLogin from '../pages/auth/EmployeeLogin';
import CustomerPortalLogin from '../pages/auth/CustomerPortalLogin';

// SuperAdmin Pages
import SuperAdminDashboard from '../pages/superadmin/Dashboard';
import OrganizationsPage from '../pages/superadmin/Organizations';

// App Pages (Seller/Owner)
import AppDashboard from '../pages/app/Dashboard';
import CustomersPage from '../pages/app/Customers';
import ProductsPage from '../pages/app/Products';
import LiveSalesPage from '../pages/app/LiveSales';

// Portal Pages (Customer)
import CustomerDashboard from '../pages/portal/Dashboard';
import CustomerOrders from '../pages/portal/Orders';

// Protected Route Component
const ProtectedRoute = ({ children, requiredRole }: { children: React.ReactNode; requiredRole?: string }) => {
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
    path: '/',
    element: <Navigate to="/login" replace />,
  },
  {
    path: '/login',
    element: <EmployeeLogin />,
  },
  {
    path: '/portal/:orgSlug/login',
    element: <CustomerPortalLogin />,
  },
  {
    path: '/superadmin',
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
        path: 'organizations',
        element: <OrganizationsPage />,
      },
    ],
  },
  {
    path: '/app',
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
        path: 'customers',
        element: <CustomersPage />,
      },
      {
        path: 'products',
        element: <ProductsPage />,
      },
      {
        path: 'live-sales',
        element: <LiveSalesPage />,
      },
    ],
  },
  {
    path: '/portal/:orgSlug',
    element: (
      <ProtectedRoute requiredRole="Customer">
        <PortalLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        path: 'dashboard',
        element: <CustomerDashboard />,
      },
      {
        path: 'orders',
        element: <CustomerOrders />,
      },
    ],
  },
]);
