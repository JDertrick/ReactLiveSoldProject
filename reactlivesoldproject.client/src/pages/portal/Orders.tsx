const CustomerOrders = () => {
  // TODO: Fetch customer orders from API
  const orders: any[] = []; // Placeholder

  return (
    <div className="px-4 sm:px-6 lg:px-8">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-2xl font-semibold text-gray-900">My Orders</h1>
          <p className="mt-2 text-sm text-gray-700">
            View your complete order history and track your purchases
          </p>
        </div>
      </div>

      <div className="mt-8">
        {orders.length > 0 ? (
          <div className="space-y-6">
            {orders.map((order: any) => (
              <div key={order.id} className="bg-white shadow rounded-lg overflow-hidden">
                <div className="px-4 py-5 sm:p-6">
                  <div className="flex items-center justify-between mb-4">
                    <div>
                      <h3 className="text-lg font-medium text-gray-900">
                        Order #{order.orderNumber || order.id.slice(0, 8)}
                      </h3>
                      <p className="text-sm text-gray-500">
                        {new Date(order.createdAt).toLocaleDateString()} at{' '}
                        {new Date(order.createdAt).toLocaleTimeString()}
                      </p>
                    </div>
                    <span
                      className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${
                        order.status === 'Finalized'
                          ? 'bg-green-100 text-green-800'
                          : 'bg-yellow-100 text-yellow-800'
                      }`}
                    >
                      {order.status}
                    </span>
                  </div>

                  <div className="border-t border-gray-200 pt-4">
                    <h4 className="text-sm font-medium text-gray-900 mb-3">Order Items</h4>
                    <ul className="space-y-2">
                      {order.items?.map((item: any, index: number) => (
                        <li key={index} className="flex items-center justify-between">
                          <div className="flex items-center space-x-3">
                            {item.product?.imageUrl && (
                              <img
                                src={item.product.imageUrl}
                                alt={item.product.name}
                                className="h-12 w-12 rounded object-cover"
                              />
                            )}
                            <div>
                              <p className="text-sm font-medium text-gray-900">
                                {item.product?.name || 'Product'}
                              </p>
                              <p className="text-xs text-gray-500">
                                Quantity: {item.quantity}
                              </p>
                            </div>
                          </div>
                          <span className="text-sm font-medium text-gray-900">
                            ${item.totalPrice.toFixed(2)}
                          </span>
                        </li>
                      ))}
                    </ul>
                  </div>

                  <div className="border-t border-gray-200 mt-4 pt-4 flex items-center justify-between">
                    <span className="text-base font-semibold text-gray-900">Total</span>
                    <span className="text-xl font-bold text-gray-900">
                      ${order.totalAmount.toFixed(2)}
                    </span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="bg-white shadow rounded-lg">
            <div className="px-4 py-12 text-center">
              <svg
                className="mx-auto h-16 w-16 text-gray-400"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z"
                />
              </svg>
              <h3 className="mt-4 text-lg font-medium text-gray-900">No orders yet</h3>
              <p className="mt-2 text-sm text-gray-500">
                You haven't made any purchases yet. Your order history will appear here.
              </p>
            </div>
          </div>
        )}
      </div>

      {/* Order History Stats */}
      {orders.length > 0 && (
        <div className="mt-8 bg-white shadow rounded-lg">
          <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
            <h3 className="text-lg leading-6 font-medium text-gray-900">
              Order Statistics
            </h3>
          </div>
          <div className="px-4 py-5 sm:p-6">
            <div className="grid grid-cols-1 gap-5 sm:grid-cols-3">
              <div>
                <dt className="text-sm font-medium text-gray-500">Total Orders</dt>
                <dd className="mt-1 text-3xl font-semibold text-gray-900">{orders.length}</dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Total Spent</dt>
                <dd className="mt-1 text-3xl font-semibold text-gray-900">
                  ${orders.reduce((sum, order) => sum + order.totalAmount, 0).toFixed(2)}
                </dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Items Purchased</dt>
                <dd className="mt-1 text-3xl font-semibold text-gray-900">
                  {orders.reduce((sum, order) => sum + (order.items?.length || 0), 0)}
                </dd>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default CustomerOrders;
