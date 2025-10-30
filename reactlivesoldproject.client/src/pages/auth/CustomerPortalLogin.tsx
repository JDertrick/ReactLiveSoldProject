import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useCustomerPortalLogin } from '../../hooks/useAuth';
import { usePortalBrandStore } from '../../store/portalBrandStore';
import apiClient from '../../services/api';
import { CustomerPortalLoginRequest } from '../../types/auth.types';

const CustomerPortalLogin = () => {
  const navigate = useNavigate();
  const { orgSlug } = useParams<{ orgSlug: string }>();
  const customerLogin = useCustomerPortalLogin();
  const { organizationName, logoUrl, setOrganizationBrand } = usePortalBrandStore();

  const [formData, setFormData] = useState<CustomerPortalLoginRequest>({
    organizationSlug: orgSlug || '',
    email: '',
    password: '',
  });

  const [loading, setLoading] = useState(true);
  const [orgError, setOrgError] = useState<string | null>(null);

  // Load organization branding
  useEffect(() => {
    const loadOrganizationBrand = async () => {
      if (!orgSlug) {
        setOrgError('Organization not found');
        setLoading(false);
        return;
      }

      try {
        const response = await apiClient.get(`/public/organization/${orgSlug}`);
        const org = response.data;

        setOrganizationBrand(orgSlug, org.name, org.logoUrl || '');
        setLoading(false);
      } catch (error) {
        setOrgError('Organization not found or inactive');
        setLoading(false);
      }
    };

    loadOrganizationBrand();
  }, [orgSlug, setOrganizationBrand]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      await customerLogin.mutateAsync(formData);
      navigate(`/portal/${orgSlug}/dashboard`);
    } catch (error) {
      console.error('Login failed:', error);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading organization...</p>
        </div>
      </div>
    );
  }

  if (orgError) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
        <div className="max-w-md w-full">
          <div className="rounded-md bg-red-50 p-4">
            <div className="flex">
              <div className="ml-3">
                <h3 className="text-sm font-medium text-red-800">
                  Organization Not Found
                </h3>
                <div className="mt-2 text-sm text-red-700">
                  {orgError}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          {logoUrl && (
            <div className="flex justify-center mb-4">
              <img src={logoUrl} alt={organizationName} className="h-16 w-auto" />
            </div>
          )}
          <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
            {organizationName}
          </h2>
          <p className="mt-2 text-center text-sm text-gray-600">
            Customer Portal - Sign in to your account
          </p>
        </div>

        <form className="mt-8 space-y-6" onSubmit={handleSubmit}>
          <div className="rounded-md shadow-sm -space-y-px">
            <div>
              <label htmlFor="email" className="sr-only">
                Email address
              </label>
              <input
                id="email"
                name="email"
                type="email"
                autoComplete="email"
                required
                value={formData.email}
                onChange={handleChange}
                className="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-t-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm"
                placeholder="Email address"
              />
            </div>
            <div>
              <label htmlFor="password" className="sr-only">
                Password
              </label>
              <input
                id="password"
                name="password"
                type="password"
                autoComplete="current-password"
                required
                value={formData.password}
                onChange={handleChange}
                className="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-b-md focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm"
                placeholder="Password"
              />
            </div>
          </div>

          {customerLogin.isError && (
            <div className="rounded-md bg-red-50 p-4">
              <div className="flex">
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-red-800">
                    Login failed
                  </h3>
                  <div className="mt-2 text-sm text-red-700">
                    {customerLogin.error instanceof Error
                      ? customerLogin.error.message
                      : 'Invalid email or password. Please try again.'}
                  </div>
                </div>
              </div>
            </div>
          )}

          <div>
            <button
              type="submit"
              disabled={customerLogin.isPending}
              className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {customerLogin.isPending ? (
                <span className="flex items-center">
                  <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Signing in...
                </span>
              ) : (
                'Sign in'
              )}
            </button>
          </div>

          <div className="text-center">
            <p className="text-xs text-gray-500">
              Test customer: maria@example.com / Customer123!
            </p>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CustomerPortalLogin;
