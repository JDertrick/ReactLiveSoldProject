import { useState, useEffect } from "react";
import { Outlet, Link, useNavigate, useLocation } from "react-router-dom";
import { useAuthStore } from "../../store/authStore";
import { useOrganizations } from "../../hooks/useOrganizations";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import {
  Settings,
  LogOut,
  User,
  Building2,
  LayoutDashboard,
  Users,
  Box,
  ArrowRightLeft,
  BookCopy,
  Warehouse,
  ClipboardList,
  ShoppingCart,
  Wallet,
  Bell,
  ChevronRight,
  Menu,
  X,
  Plus,
  FileText,
  Search,
  Receipt,
  ClipboardCheck,
  Contact,
  Package,
  Truck,
  DollarSign,
} from "lucide-react";
import { OrganizationSettingsModal } from "./OrganizationSettingsModal";
import { CustomizationSettings } from "../../types/organization.types";
import { NotificationDropdown } from "../notifications/NotificationDropdown";

const LiveSoldLogo = () => (
  <div className="flex items-center gap-2">
    <div className="bg-primary p-1.5 rounded-lg">
      <svg
        xmlns="http://www.w3.org/2000/svg"
        width="20"
        height="20"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
        className="text-primary-foreground"
      >
        <path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z" />
      </svg>
    </div>
    <span className="font-bold text-xl text-foreground">LiveSold</span>
  </div>
);

const AppLayout = () => {
  const { user, logout, isSuperAdmin, organizationId } = useAuthStore();
  const navigate = useNavigate();
  const location = useLocation();
  const [sidebarOpen, setSidebarOpen] = useState(window.innerWidth >= 768);
  const [settingsOpen, setSettingsOpen] = useState(false);

  const { data: organization } = useOrganizations(
    !isSuperAdmin && organizationId ? organizationId : undefined
  );

  useEffect(() => {
    const handleResize = () => {
      if (window.innerWidth < 768) {
        setSidebarOpen(false);
      } else {
        setSidebarOpen(true);
      }
    };
    window.addEventListener("resize", handleResize);
    return () => window.removeEventListener("resize", handleResize);
  }, []);

  useEffect(() => {
    if (organization?.customizationSettings) {
      try {
        const settings: CustomizationSettings = JSON.parse(
          organization.customizationSettings
        );
        applyCustomColors(settings);
      } catch (e) {
        console.error("Error parsing customization settings:", e);
      }
    }
  }, [organization]);

  const applyCustomColors = (colors: CustomizationSettings) => {
    const root = document.documentElement;
    // This function will now also control the main shadcn primary color
    if (colors.primaryColor) {
      root.style.setProperty("--primary-color", colors.primaryColor);
      // To keep consistency, we can convert it to oklch or just use it directly
      // For simplicity, we'll assume the color is good enough.
      // A more robust solution would convert HEX/RGB to OKLCH.
      root.style.setProperty(
        "--primary",
        `oklch(from ${colors.primaryColor} l c h)`
      );
    }

    // We leave the rest of the customization as is, but we will favor tailwind classes over it.
    if (colors.sidebarBg)
      root.style.setProperty("--sidebar-bg", colors.sidebarBg);
    if (colors.sidebarText)
      root.style.setProperty("--sidebar-text", colors.sidebarText);
    if (colors.sidebarActiveBg)
      root.style.setProperty("--sidebar-active-bg", colors.sidebarActiveBg);
    if (colors.sidebarActiveText)
      root.style.setProperty("--sidebar-active-text", colors.sidebarActiveText);
  };

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  const isActive = (path: string) => {
    if (path === "/app" && location.pathname !== "/app") {
      return false;
    }
    return location.pathname.startsWith(path);
  };

  const getBreadcrumbs = () => {
    const paths = location.pathname.split("/").filter((x) => x);
    let currentPath = "";
    const breadcrumbs = paths.map((path, index) => {
      currentPath += `/${path}`;
      const isLast = index === paths.length - 1;
      const name =
        path.charAt(0).toUpperCase() + path.slice(1).replace("-", " ");
      return { name, path: currentPath, isLast };
    });
    return [{ name: "App", path: "/app", isLast: false }, ...breadcrumbs];
  };

  const superAdminNavigation = [
    {
      group: "SUPER ADMIN",
      items: [
        {
          name: "Dashboard",
          path: "/superadmin",
          icon: <LayoutDashboard size={20} />,
        },
        {
          name: "Organizaciones",
          path: "/superadmin/organizations",
          icon: <Building2 size={20} />,
        },
      ],
    },
  ];

  const appNavigation = [
    {
      group: "PRINCIPAL",
      items: [
        {
          name: "Dashboard",
          path: "/app",
          icon: <LayoutDashboard size={20} />,
        },
      ],
    },
    {
      group: "OPERACIONES",
      items: [
        { name: "Inventario", path: "/app/products", icon: <Box size={20} /> },
        {
          name: "Movimientos",
          path: "/app/stock-movements",
          icon: <ArrowRightLeft size={20} />,
        },
        {
          name: "Auditoría",
          path: "/app/inventory-audit",
          icon: <ClipboardCheck size={20} />,
        },
      ],
    },
    {
      group: "FINANZAS",
      items: [
        {
          name: "Facturación",
          path: "/app/live-sales",
          icon: <ShoppingCart size={20} />,
        },
        { name: "Billetera", path: "/app/wallet", icon: <Wallet size={20} /> },
        {
          name: "Recibos",
          path: "/app/receipts",
          icon: <ClipboardList size={20} />,
        },
        {
          name: "Contabilidad",
          path: "/app/accounting",
          icon: <FileText size={20} />,
        },
      ],
    },
    {
      group: "COMPRAS",
      items: [
        {
          name: "Órdenes de Compra",
          path: "/app/purchase-orders",
          icon: <ShoppingCart size={20} />,
        },
        {
          name: "Recepciones",
          path: "/app/purchase-receipts",
          icon: <Truck size={20} />,
        },
        {
          name: "Facturas Proveedor",
          path: "/app/vendor-invoices",
          icon: <FileText size={20} />,
        },
        {
          name: "Pagos",
          path: "/app/payments",
          icon: <DollarSign size={20} />,
        },
      ],
    },
    {
      group: "CONFIGURACIÓN",
      items: [
        {
          name: "Clientes",
          path: "/app/customers",
          icon: <Users size={20} />,
        },
        {
          name: "Proveedores",
          path: "/app/vendors",
          icon: <Package size={20} />,
        },
        {
          name: "Contact",
          path: "/app/contacts",
          icon: <Contact size={20} />,
        },
        {
          name: "Categorias",
          path: "/app/categories",
          icon: <BookCopy size={20} />,
        },
        {
          name: "Bodegas",
          path: "/app/locations",
          icon: <Warehouse size={20} />,
        },
        {
          name: "Impuestos",
          path: "/app/tax-settings",
          icon: <Receipt size={20} />,
        },
      ],
    },
  ];

  const navigation = isSuperAdmin ? superAdminNavigation : appNavigation;
  const breadcrumbs = getBreadcrumbs();

  const SidebarContent = () => (
    <div className="flex flex-col h-full bg-card">
      <div className="flex items-center justify-between h-20 px-6 border-b">
        <LiveSoldLogo />
      </div>

      <nav className="flex-1 px-4 py-6 space-y-4 overflow-y-auto">
        {navigation.map((navGroup) => (
          <div key={navGroup.group}>
            <h3 className="px-3 mb-2 text-xs font-semibold tracking-wider text-muted-foreground uppercase">
              {navGroup.group}
            </h3>
            <div className="space-y-1">
              {navGroup.items.map((item) => {
                const active = isActive(item.path);
                return (
                  <Link
                    key={item.path}
                    to={item.path}
                    onClick={() => {
                      if (window.innerWidth < 768) {
                        setSidebarOpen(false);
                      }
                    }}
                    className={`flex items-center px-3 py-2 text-sm font-medium rounded-lg transition-all ${
                      active
                        ? "bg-primary/10 text-primary"
                        : "text-muted-foreground hover:bg-muted/50 hover:text-foreground"
                    }`}
                  >
                    <span className="flex-shrink-0">{item.icon}</span>
                    <span className="ml-3">{item.name}</span>
                  </Link>
                );
              })}
            </div>
          </div>
        ))}
      </nav>

      {/* User Section */}
      <div className="p-4 border-t">
        <Link to="/app/profile">
          <div className="flex items-center w-full p-2 rounded-lg hover:bg-muted/50">
            <Avatar className="h-9 w-9">
              <AvatarImage
                src={user?.avatarUrl}
                alt={user?.firstName || user?.email}
              />
              <AvatarFallback className="bg-primary text-primary-foreground font-semibold">
                {user?.firstName?.charAt(0) || user?.email?.charAt(0)}
              </AvatarFallback>
            </Avatar>
            <div className="ml-3 flex-1 min-w-0">
              <p className="text-sm font-semibold text-foreground truncate">
                {user?.firstName || user?.email}
              </p>
              <p className="text-xs text-muted-foreground truncate">Owner</p>
            </div>
            <ChevronRight className="h-5 w-5 text-muted-foreground ml-2" />
          </div>
        </Link>
      </div>
    </div>
  );

  return (
    <div className="min-h-screen bg-background">
      {/* Mobile Overlay */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 bg-black bg-opacity-30 z-30 md:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <div
        className={`fixed inset-y-0 left-0 z-40 transition-transform duration-300 transform
          ${sidebarOpen ? "translate-x-0" : "-translate-x-full"}
          md:translate-x-0 w-64 border-r bg-card`}
      >
        <SidebarContent />
      </div>

      {/* Main Content */}
      <div className="md:pl-64 transition-all duration-300">
        <header className="sticky top-0 z-20 bg-background/80 backdrop-blur-lg border-b">
          <div className="flex items-center justify-between h-20 px-6">
            {/* Left side: Breadcrumbs and Mobile Menu */}
            <div className="flex items-center gap-4">
              <button
                className="md:hidden p-2 -ml-2 rounded-md text-muted-foreground"
                onClick={() => setSidebarOpen(true)}
              >
                <Menu className="w-6 h-6" />
              </button>
              <div className="hidden md:flex items-center gap-2 text-sm text-muted-foreground">
                {breadcrumbs.map((crumb, index) => (
                  <div key={index} className="flex items-center gap-2">
                    {!crumb.isLast ? (
                      <Link to={crumb.path} className="hover:text-foreground">
                        {crumb.name}
                      </Link>
                    ) : (
                      <span className="font-semibold text-foreground">
                        {crumb.name}
                      </span>
                    )}
                    {!crumb.isLast && <ChevronRight className="h-4 w-4" />}
                  </div>
                ))}
              </div>
            </div>

            {/* Center: Title for Mobile */}
            <h1 className="text-lg font-bold text-foreground md:hidden">
              {breadcrumbs[breadcrumbs.length - 1]?.name || "LiveSold"}
            </h1>

            {/* Right side: Search, Actions, User Menu */}
            <div className="flex items-center gap-4">
              <NotificationDropdown />

              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button
                    variant="ghost"
                    className="relative h-10 w-10 rounded-full"
                  >
                    <Avatar className="h-10 w-10">
                      <AvatarImage
                        src={user?.avatarUrl}
                        alt={user?.firstName || user?.email}
                      />
                      <AvatarFallback className="bg-primary text-primary-foreground font-semibold">
                        {user?.firstName?.charAt(0) || user?.email?.charAt(0)}
                      </AvatarFallback>
                    </Avatar>
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-64">
                  <DropdownMenuLabel>
                    <div className="flex flex-col space-y-1">
                      <p className="text-sm font-medium leading-none">
                        {user?.firstName} {user?.lastName}
                      </p>
                      <p className="text-xs leading-none text-muted-foreground">
                        {user?.email}
                      </p>
                    </div>
                  </DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem asChild>
                    <Link to="/app/profile">
                      <User className="mr-2 h-4 w-4" />
                      <span>Mi Perfil</span>
                    </Link>
                  </DropdownMenuItem>
                  {!isSuperAdmin && (
                    <DropdownMenuItem onSelect={() => setSettingsOpen(true)}>
                      <Building2 className="mr-2 h-4 w-4" />
                      <span>Config. de Organización</span>
                    </DropdownMenuItem>
                  )}
                  <DropdownMenuItem>
                    <Settings className="mr-2 h-4 w-4" />
                    <span>Configuración</span>
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem
                    onSelect={handleLogout}
                    className="text-red-500 focus:text-red-500 focus:bg-red-500/10"
                  >
                    <LogOut className="mr-2 h-4 w-4" />
                    <span>Cerrar Sesión</span>
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </div>
          </div>
        </header>

        <main className="p-6">
          <div className="flex justify-between items-center mb-6 md:hidden">
            <h1 className="text-2xl font-bold text-foreground">
              {breadcrumbs[breadcrumbs.length - 1]?.name || "LiveSold"}
            </h1>
          </div>
          <Outlet />
        </main>
      </div>

      <OrganizationSettingsModal
        open={settingsOpen}
        onClose={() => setSettingsOpen(false)}
      />
    </div>
  );
};

export default AppLayout;
