import { Customer } from "../../types/customer.types";

interface CustomerProfileTabProps {
  customer: Customer;
}

export const CustomerProfileTab = ({ customer }: CustomerProfileTabProps) => {
  return (
    <div className="space-y-6 animate-fadeIn">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Personal Information */}
        <div className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow duration-200">
          <h3 className="text-sm font-semibold text-gray-900 mb-4 flex items-center">
            <svg className="w-4 h-4 mr-2 text-indigo-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
            Personal Information
          </h3>
          <div className="space-y-3">
            <div>
              <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">Full Name</p>
              <p className="mt-1 text-sm text-gray-900 font-medium">
                {customer.firstName} {customer.lastName}
              </p>
            </div>
            <div>
              <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">Customer ID</p>
              <p className="mt-1 text-sm text-gray-600 font-mono">
                {customer.id}
              </p>
            </div>
          </div>
        </div>

        {/* Contact Information */}
        <div className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow duration-200">
          <h3 className="text-sm font-semibold text-gray-900 mb-4 flex items-center">
            <svg className="w-4 h-4 mr-2 text-indigo-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
            </svg>
            Contact Information
          </h3>
          <div className="space-y-3">
            <div>
              <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">Email</p>
              <a
                href={`mailto:${customer.email}`}
                className="mt-1 text-sm text-indigo-600 hover:text-indigo-700 block"
              >
                {customer.email}
              </a>
            </div>
            <div>
              <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">Phone</p>
              <a
                href={`tel:${customer.phone}`}
                className="mt-1 text-sm text-indigo-600 hover:text-indigo-700 block"
              >
                {customer.phone}
              </a>
            </div>
          </div>
        </div>

        {/* Account Status */}
        <div className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow duration-200">
          <h3 className="text-sm font-semibold text-gray-900 mb-4 flex items-center">
            <svg className="w-4 h-4 mr-2 text-indigo-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            Account Status
          </h3>
          <div className="space-y-3">
            <div>
              <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">Status</p>
              <div className="mt-2">
                <span
                  className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold ${
                    customer.isActive
                      ? "bg-green-100 text-green-800 ring-1 ring-green-600/20"
                      : "bg-red-100 text-red-800 ring-1 ring-red-600/20"
                  }`}
                >
                  <span className={`mr-1.5 h-1.5 w-1.5 rounded-full ${customer.isActive ? 'bg-green-600' : 'bg-red-600'}`}></span>
                  {customer.isActive ? "Active" : "Inactive"}
                </span>
              </div>
            </div>
          </div>
        </div>

        {/* Wallet Summary */}
        <div className="bg-gradient-to-br from-indigo-500 to-purple-600 rounded-lg p-4 text-white shadow-lg hover:shadow-xl transition-shadow duration-200">
          <h3 className="text-sm font-semibold mb-4 flex items-center opacity-90">
            <svg className="w-4 h-4 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
            </svg>
            Wallet Balance
          </h3>
          <div>
            <p className="text-3xl font-bold">
              ${(customer.wallet?.balance ?? 0).toFixed(2)}
            </p>
            <p className="text-xs opacity-75 mt-1">Available Balance</p>
          </div>
        </div>
      </div>

      {/* Additional Stats */}
      <div className="bg-gray-50 border border-gray-200 rounded-lg p-4">
        <h3 className="text-sm font-semibold text-gray-900 mb-3">Quick Stats</h3>
        <div className="grid grid-cols-3 gap-4">
          <div className="text-center">
            <p className="text-2xl font-bold text-indigo-600">-</p>
            <p className="text-xs text-gray-500 mt-1">Total Orders</p>
          </div>
          <div className="text-center">
            <p className="text-2xl font-bold text-indigo-600">-</p>
            <p className="text-xs text-gray-500 mt-1">Total Spent</p>
          </div>
          <div className="text-center">
            <p className="text-2xl font-bold text-indigo-600">-</p>
            <p className="text-xs text-gray-500 mt-1">Transactions</p>
          </div>
        </div>
      </div>
    </div>
  );
};
