import { Outlet, Link, useNavigate, useParams } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import { usePortalBrandStore } from '../../store/portalBrandStore';

const PortalLayout = () => {
  const { user, logout } = useAuthStore();
  const { organizationName, logoUrl } = usePortalBrandStore();
  const { orgSlug } = useParams();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate(`/portal/${orgSlug}/login`);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Navbar */}
      <nav className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              {logoUrl && (
                <img src={logoUrl} alt={organizationName || ''} className="h-8 w-auto mr-3" />
              )}
              <h1 className="text-xl font-bold text-gray-900">{organizationName}</h1>
            </div>
            <div className="hidden sm:ml-6 sm:flex sm:space-x-8">
              <Link
                to={`/portal/${orgSlug}/dashboard`}
                className="border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700 inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium"
              >
                Dashboard
              </Link>
              <Link
                to={`/portal/${orgSlug}/orders`}
                className="border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700 inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium"
              >
                My Orders
              </Link>
            </div>
            <div className="flex items-center">
              <span className="text-sm text-gray-700 mr-4">
                {user?.firstName} {user?.lastName}
              </span>
              <button
                onClick={handleLogout}
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
              >
                Logout
              </button>
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
