import { useEffect } from 'react';
import { Outlet, Link, useNavigate, useParams, useLocation } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import { usePortalBrandStore } from '../../store/portalBrandStore';

const PortalLayout = () => {
  const { user, logout } = useAuthStore();
  const { organizationName, organizationSlug, logoUrl, setBrand, clearBrand } = usePortalBrandStore();
  const { orgSlug } = useParams();
  const location = useLocation();
  const navigate = useNavigate();

  // Sincronizar portalBrandStore con authStore si no hay datos
  useEffect(() => {
    if (!organizationSlug && user?.organizationSlug) {
      setBrand(
        user.organizationSlug,
        user.organizationName || '',
        user.organizationLogoUrl || null
      );
    }
  }, [organizationSlug, user, setBrand]);

  const handleLogout = () => {
    logout();
    clearBrand();
    navigate(`/portal/${orgSlug}/login`);
  };

  // Usar datos del authStore como fallback
  const displayName = organizationName || user?.organizationName || 'Customer Portal';
  const displayLogo = logoUrl || user?.organizationLogoUrl;

  const isActive = (path: string) => {
    return location.pathname === `/portal/${orgSlug}${path}`;
  };

  const navigation = [
    {
      name: 'Dashboard',
      path: '/dashboard',
      icon: (
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
        </svg>
      ),
    },
    {
      name: 'My Orders',
      path: '/orders',
      icon: (
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
        </svg>
      ),
    },
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Navbar */}
      <nav className="bg-white shadow-sm border-b sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            {/* Logo and Brand */}
            <div className="flex items-center">
              {displayLogo && (
                <img src={displayLogo} alt={displayName} className="h-10 w-auto mr-3" />
              )}
              <h1 className="text-xl font-bold text-gray-900">{displayName}</h1>
            </div>

            {/* Navigation Links */}
            <div className="hidden sm:flex sm:items-center sm:space-x-4">
              {navigation.map((item) => {
                const active = isActive(item.path);
                return (
                  <Link
                    key={item.path}
                    to={`/portal/${orgSlug}${item.path}`}
                    className={`flex items-center px-3 py-2 text-sm font-medium rounded-md transition-colors ${
                      active
                        ? 'bg-indigo-50 text-indigo-700'
                        : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                    }`}
                  >
                    <span className="mr-2">{item.icon}</span>
                    {item.name}
                  </Link>
                );
              })}
            </div>

            {/* User Menu */}
            <div className="flex items-center space-x-4">
              {/* User Info */}
              <div className="hidden md:flex items-center">
                <div className="w-8 h-8 rounded-full bg-gradient-to-r from-indigo-500 to-purple-600 flex items-center justify-center mr-2">
                  <span className="text-sm font-medium text-white">
                    {user?.firstName?.charAt(0) || user?.email?.charAt(0)}
                  </span>
                </div>
                <div className="text-sm">
                  <p className="font-medium text-gray-900">
                    {user?.firstName} {user?.lastName}
                  </p>
                  <p className="text-xs text-gray-500">{user?.email}</p>
                </div>
              </div>

              {/* Logout Button */}
              <button
                onClick={handleLogout}
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-colors"
              >
                <svg className="w-4 h-4 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
                Logout
              </button>
            </div>
          </div>

          {/* Mobile Navigation */}
          <div className="sm:hidden pb-3">
            <div className="space-y-1">
              {navigation.map((item) => {
                const active = isActive(item.path);
                return (
                  <Link
                    key={item.path}
                    to={`/portal/${orgSlug}${item.path}`}
                    className={`flex items-center px-3 py-2 text-sm font-medium rounded-md ${
                      active
                        ? 'bg-indigo-50 text-indigo-700'
                        : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                    }`}
                  >
                    <span className="mr-2">{item.icon}</span>
                    {item.name}
                  </Link>
                );
              })}
            </div>
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <Outlet />
      </main>
    </div>
  );
};

export default PortalLayout;
