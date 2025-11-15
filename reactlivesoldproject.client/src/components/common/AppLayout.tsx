import { useState, useEffect } from 'react';
import { Outlet, Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import { useOrganizations } from '../../hooks/useOrganizations';
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
import { Settings, LogOut, User, Building2 } from "lucide-react";
import { OrganizationSettingsModal } from './OrganizationSettingsModal';
import { CustomizationSettings } from '../../types/organization.types';

const AppLayout = () => {
  const { user, logout, isSuperAdmin, organizationId } = useAuthStore();
  const navigate = useNavigate();
  const location = useLocation();
  // Sidebar open by default on desktop, closed on mobile
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [settingsOpen, setSettingsOpen] = useState(false);

  // Load organization settings (only for non-super-admin users)
  const { data: organization } = useOrganizations(
    !isSuperAdmin && organizationId ? organizationId : undefined
  );

  // Apply custom colors when organization settings change
  useEffect(() => {
    if (organization?.customizationSettings) {
      try {
        const settings: CustomizationSettings = JSON.parse(organization.customizationSettings);
        applyCustomColors(settings);
      } catch (e) {
        console.error('Error parsing customization settings:', e);
      }
    }
  }, [organization]);

  const applyCustomColors = (colors: CustomizationSettings) => {
    const root = document.documentElement;
    if (colors.primaryColor) root.style.setProperty('--primary-color', colors.primaryColor);
    if (colors.sidebarBg) root.style.setProperty('--sidebar-bg', colors.sidebarBg);
    if (colors.sidebarText) root.style.setProperty('--sidebar-text', colors.sidebarText);
    if (colors.sidebarActiveBg) root.style.setProperty('--sidebar-active-bg', colors.sidebarActiveBg);
    if (colors.sidebarActiveText) root.style.setProperty('--sidebar-active-text', colors.sidebarActiveText);
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const isActive = (path: string) => {
    return location.pathname === path || location.pathname.startsWith(path + '/');
  };

  const superAdminNavigation = [
    {
      name: 'Dashboard',
      path: '/superadmin',
      icon: (
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
        </svg>
      ),
    },
    {
      name: 'Organizations',
      path: '/superadmin/organizations',
      icon: (
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
        </svg>
      ),
    },
  ];

  const appNavigation = [
    {
      name: 'Dashboard',
      path: '/app',
      icon: (
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
        </svg>
      ),
    },
    {
      name: 'Customers',
      path: '/app/customers',
      icon: (
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
        </svg>
      ),
    },
    {
      name: 'Products',
      path: '/app/products',
      icon: (
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
        </svg>
      ),
    },
    {
      name: 'Stock Movements',
      path: '/app/stock-movements',
      icon: (
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
        </svg>
      ),
    },
    {
      name: 'Live Sales',
      path: '/app/live-sales',
      icon: (
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
        </svg>
      ),
    },
    {
      name: 'Wallet',
      path: '/app/wallet',
      icon: (
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
        </svg>
      ),
    },
  ];

  const navigation = isSuperAdmin ? superAdminNavigation : appNavigation;

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Mobile Overlay - only show when sidebar is open on mobile */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 bg-black bg-opacity-50 z-40 md:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <div
        className={`fixed inset-y-0 left-0 z-50 transition-all duration-300
          ${sidebarOpen ? 'w-64' : 'w-64 -translate-x-full'}
          md:translate-x-0 md:${sidebarOpen ? 'w-64' : 'w-20'}`}
        style={{
          backgroundColor: 'var(--sidebar-bg, #111827)',
          color: 'var(--sidebar-text, #D1D5DB)'
        }}
      >
        <div className="flex flex-col h-full">
          {/* Logo */}
          <div
            className="flex items-center justify-between h-16 px-4"
            style={{ backgroundColor: 'var(--sidebar-active-bg, #1F2937)' }}
          >
            {sidebarOpen ? (
              <h1 className="text-lg font-bold" style={{ color: 'var(--sidebar-active-text, #FFFFFF)' }}>
                LiveSold
              </h1>
            ) : (
              <span className="text-lg font-bold hidden md:block" style={{ color: 'var(--sidebar-active-text, #FFFFFF)' }}>
                LS
              </span>
            )}
            {/* Desktop toggle button */}
            <button
              onClick={() => setSidebarOpen(!sidebarOpen)}
              className="p-1 rounded-md opacity-60 hover:opacity-100 transition-opacity hidden md:block"
              style={{ color: 'var(--sidebar-text, #D1D5DB)' }}
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={sidebarOpen ? "M11 19l-7-7 7-7m8 14l-7-7 7-7" : "M13 5l7 7-7 7M5 5l7 7-7 7"} />
              </svg>
            </button>
            {/* Mobile close button */}
            <button
              onClick={() => setSidebarOpen(false)}
              className="p-1 rounded-md opacity-60 hover:opacity-100 transition-opacity md:hidden"
              style={{ color: 'var(--sidebar-text, #D1D5DB)' }}
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Navigation */}
          <nav className="flex-1 px-2 py-4 space-y-1 overflow-y-auto">
            {navigation.map((item) => {
              const active = isActive(item.path);
              return (
                <Link
                  key={item.path}
                  to={item.path}
                  onClick={() => {
                    // Close sidebar on mobile when clicking a link
                    if (window.innerWidth < 768) {
                      setSidebarOpen(false);
                    }
                  }}
                  className="flex items-center px-3 py-2 text-sm font-medium rounded-md transition-all"
                  style={
                    active
                      ? {
                          backgroundColor: 'var(--sidebar-active-bg, #1F2937)',
                          color: 'var(--sidebar-active-text, #FFFFFF)',
                        }
                      : {
                          color: 'var(--sidebar-text, #D1D5DB)',
                        }
                  }
                  onMouseEnter={(e) => {
                    if (!active) {
                      e.currentTarget.style.backgroundColor = 'var(--sidebar-active-bg, #1F2937)';
                      e.currentTarget.style.opacity = '0.7';
                    }
                  }}
                  onMouseLeave={(e) => {
                    if (!active) {
                      e.currentTarget.style.backgroundColor = 'transparent';
                      e.currentTarget.style.opacity = '1';
                    }
                  }}
                  title={!sidebarOpen ? item.name : undefined}
                >
                  <span className="flex-shrink-0">{item.icon}</span>
                  {sidebarOpen && <span className="ml-3">{item.name}</span>}
                </Link>
              );
            })}
          </nav>

          {/* User Section */}
          <div className="border-t border-opacity-20" style={{ borderColor: 'var(--sidebar-text, #D1D5DB)' }}>
            <div className="px-3 py-4">
              {sidebarOpen ? (
                <div className="space-y-2">
                  <div className="flex items-center">
                    <div className="flex-shrink-0">
                      <div
                        className="w-8 h-8 rounded-full flex items-center justify-center"
                        style={{ backgroundColor: 'var(--primary-color, #4F46E5)' }}
                      >
                        <span className="text-xs font-medium text-white">
                          {user?.firstName?.charAt(0) || user?.email?.charAt(0)}
                        </span>
                      </div>
                    </div>
                    <div className="ml-3 flex-1 min-w-0">
                      <p className="text-sm font-medium truncate" style={{ color: 'var(--sidebar-active-text, #FFFFFF)' }}>
                        {user?.firstName || user?.email}
                      </p>
                      <p className="text-xs truncate opacity-70" style={{ color: 'var(--sidebar-text, #D1D5DB)' }}>
                        {user?.role}
                      </p>
                    </div>
                  </div>
                  <button
                    onClick={handleLogout}
                    className="w-full flex items-center px-3 py-2 text-sm font-medium rounded-md transition-all hover:opacity-80"
                    style={{
                      color: 'var(--sidebar-text, #D1D5DB)',
                      backgroundColor: 'transparent',
                    }}
                    onMouseEnter={(e) => {
                      e.currentTarget.style.backgroundColor = 'var(--sidebar-active-bg, #1F2937)';
                      e.currentTarget.style.opacity = '0.7';
                    }}
                    onMouseLeave={(e) => {
                      e.currentTarget.style.backgroundColor = 'transparent';
                      e.currentTarget.style.opacity = '1';
                    }}
                  >
                    <svg className="w-5 h-5 mr-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                    </svg>
                    Logout
                  </button>
                </div>
              ) : (
                <button
                  onClick={handleLogout}
                  className="w-full flex justify-center px-3 py-2 text-sm font-medium rounded-md transition-all"
                  style={{ color: 'var(--sidebar-text, #D1D5DB)' }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.backgroundColor = 'var(--sidebar-active-bg, #1F2937)';
                    e.currentTarget.style.opacity = '0.7';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.backgroundColor = 'transparent';
                    e.currentTarget.style.opacity = '1';
                  }}
                  title="Logout"
                >
                  <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                  </svg>
                </button>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className={`transition-all duration-300 ${sidebarOpen ? 'md:pl-64' : 'md:pl-20'}`}>
        <div className="sticky top-0 z-40 bg-white border-b border-gray-200 px-6 py-3 shadow-sm">
          <div className="flex items-center justify-between">
            {/* Mobile Menu Button */}
            <button
              className="md:hidden p-2 -ml-2 rounded-md text-gray-600 hover:text-gray-900 hover:bg-gray-100"
              onClick={() => setSidebarOpen(true)}
            >
              <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            </button>

            <h2 className="text-xl font-bold text-gray-900 md:block hidden">
              {navigation.find(item => isActive(item.path))?.name || 'LiveSold Platform'}
            </h2>
            <h2 className="text-lg font-bold text-gray-900 md:hidden">
              {navigation.find(item => isActive(item.path))?.name || 'LiveSold'}
            </h2>

            {/* User Menu with Avatar */}
            <div className="flex items-center gap-4">
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" className="flex items-center gap-3 h-12">
                    <Avatar className="h-9 w-9">
                      <AvatarImage src={user?.avatarUrl} alt={user?.firstName || user?.email} />
                      <AvatarFallback className="bg-indigo-600 text-white font-semibold">
                        {user?.firstName?.charAt(0) || user?.email?.charAt(0)}
                      </AvatarFallback>
                    </Avatar>
                    <div className="text-left hidden md:block">
                      <p className="text-sm font-medium text-gray-900">
                        {user?.firstName || user?.email}
                      </p>
                      <p className="text-xs text-gray-500">{user?.role}</p>
                    </div>
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-56">
                  <DropdownMenuLabel>
                    <div className="flex flex-col">
                      <span className="text-sm font-semibold">{user?.firstName} {user?.lastName}</span>
                      <span className="text-xs text-gray-500">{user?.email}</span>
                    </div>
                  </DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem className="cursor-pointer">
                    <User className="mr-2 h-4 w-4" />
                    <span>Mi Perfil</span>
                  </DropdownMenuItem>
                  {!isSuperAdmin && (
                    <DropdownMenuItem
                      className="cursor-pointer"
                      onSelect={() => setSettingsOpen(true)}
                    >
                      <Building2 className="mr-2 h-4 w-4" />
                      <span>Configuración de Organización</span>
                    </DropdownMenuItem>
                  )}
                  <DropdownMenuItem className="cursor-pointer">
                    <Settings className="mr-2 h-4 w-4" />
                    <span>Configuración</span>
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem
                    className="cursor-pointer text-red-600 focus:text-red-600"
                    onSelect={handleLogout}
                  >
                    <LogOut className="mr-2 h-4 w-4" />
                    <span>Cerrar Sesión</span>
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>

              {/* Settings Button - Hidden on mobile, available in dropdown */}
              {!isSuperAdmin && (
                <Button
                  variant="outline"
                  size="icon"
                  onClick={() => setSettingsOpen(true)}
                  title="Configuración de Organización"
                  className="h-10 w-10 hidden lg:flex"
                >
                  <Settings className="h-5 w-5" />
                </Button>
              )}
            </div>
          </div>
        </div>

        <main className="p-6">
          <Outlet />
        </main>
      </div>

      {/* Settings Modal */}
      <OrganizationSettingsModal
        open={settingsOpen}
        onClose={() => setSettingsOpen(false)}
      />
    </div>
  );
};

export default AppLayout;
