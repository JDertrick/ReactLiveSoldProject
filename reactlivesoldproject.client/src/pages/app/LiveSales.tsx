import { useState } from 'react';
import { useGetProducts } from '../../hooks/useProducts';
import { useGetCustomers } from '../../hooks/useCustomers';
import { useCreateSalesOrder } from '../../hooks/useSalesOrders';
import { Product, ProductVariant } from '../../types/product.types';
import { Customer } from '../../types/customer.types';

interface CartItem {
  productId: string;
  productName: string;
  variantId: string;
  variant: ProductVariant;
  quantity: number;
  price: number;
}

const LiveSalesPage = () => {
  const { data: products } = useGetProducts(false); // Only published products
  const { data: customers } = useGetCustomers();
  const createSalesOrder = useCreateSalesOrder();

  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [orderSuccess, setOrderSuccess] = useState(false);

  const filteredProducts = searchTerm
    ? products?.filter(p =>
        p.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.description?.toLowerCase().includes(searchTerm.toLowerCase())
      )
    : products;

  const handleAddToCart = (product: Product, variant: ProductVariant) => {
    if (!variant) {
      alert('Please select a variant');
      return;
    }

    const price = variant.price && variant.price > 0 ? variant.price : product.basePrice;
    const existingItem = cart.find(
      item => item.productId === product.id && item.variantId === variant.id
    );

    if (existingItem) {
      setCart(cart.map(item =>
        item.productId === product.id && item.variantId === variant.id
          ? { ...item, quantity: item.quantity + 1 }
          : item
      ));
    } else {
      setCart([...cart, {
        productId: product.id,
        productName: product.name,
        variantId: variant.id,
        variant,
        quantity: 1,
        price,
      }]);
    }
    setSelectedProduct(null);
  };

  const handleUpdateQuantity = (index: number, quantity: number) => {
    if (quantity <= 0) {
      handleRemoveFromCart(index);
      return;
    }
    setCart(cart.map((item, i) => i === index ? { ...item, quantity } : item));
  };

  const handleRemoveFromCart = (index: number) => {
    setCart(cart.filter((_, i) => i !== index));
  };

  const handleClearCart = () => {
    setCart([]);
    setSelectedCustomer(null);
  };

  const calculateTotal = () => {
    return cart.reduce((total, item) => total + (item.price * item.quantity), 0);
  };

  const handlePlaceOrder = async () => {
    if (!selectedCustomer || cart.length === 0) {
      alert('Please select a customer and add items to the cart');
      return;
    }

    // Check wallet balance
    const total = calculateTotal();
    if (total > (selectedCustomer.wallet?.balance || 0)) {
      alert('Insufficient wallet balance. Please add funds to the customer wallet first.');
      return;
    }

    try {
      const orderData = {
        customerId: selectedCustomer.id,
        items: cart.map(item => ({
          productVariantId: item.variantId,
          quantity: item.quantity,
          customUnitPrice: item.price !== item.variant.price ? item.price : undefined,
        })),
      };

      await createSalesOrder.mutateAsync(orderData);

      setOrderSuccess(true);
      setTimeout(() => setOrderSuccess(false), 3000);

      handleClearCart();
    } catch (error) {
      console.error('Error creating order:', error);
      alert('Failed to create order. Please try again.');
    }
  };

  return (
    <div className="space-y-4">
      {/* Success Banner */}
      {orderSuccess && (
        <div className="bg-green-50 border-l-4 border-green-400 p-4 rounded">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg className="h-5 w-5 text-green-400" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
              </svg>
            </div>
            <div className="ml-3">
              <p className="text-sm text-green-700">
                Order placed successfully! The wallet has been debited and inventory updated.
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="h-[calc(100vh-12rem)] flex gap-4">
      {/* Left Column - Product Search */}
      <div className="w-1/3 bg-white shadow rounded-lg flex flex-col">
        <div className="p-4 border-b">
          <h2 className="text-lg font-semibold text-gray-900 mb-3">Products</h2>
          <input
            type="text"
            placeholder="Search products..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
          />
        </div>
        <div className="flex-1 overflow-y-auto p-4 space-y-3">
          {filteredProducts && filteredProducts.length > 0 ? (
            filteredProducts.map((product) => (
              <div
                key={product.id}
                onClick={() => setSelectedProduct(product)}
                className="bg-gray-50 rounded-lg p-3 cursor-pointer hover:bg-gray-100 border border-gray-200"
              >
                <div className="flex gap-3">
                  {product.imageUrl && (
                    <img
                      src={product.imageUrl}
                      alt={product.name}
                      className="w-16 h-16 rounded object-cover flex-shrink-0"
                    />
                  )}
                  <div className="flex-1 min-w-0">
                    <h3 className="text-sm font-medium text-gray-900 truncate">{product.name}</h3>
                    <p className="text-sm text-gray-500">${product.basePrice.toFixed(2)}</p>
                    <p className="text-xs text-gray-400 mt-1">
                      {product.variants?.reduce((sum, v) => sum + v.stock, 0) || 0} units available
                    </p>
                  </div>
                </div>
              </div>
            ))
          ) : (
            <p className="text-sm text-gray-500 text-center py-8">No products found</p>
          )}
        </div>
      </div>

      {/* Middle Column - Cart Builder */}
      <div className="w-1/3 bg-white shadow rounded-lg flex flex-col">
        <div className="p-4 border-b">
          <h2 className="text-lg font-semibold text-gray-900 mb-3">Cart</h2>

          {/* Customer Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Select Customer
            </label>
            <select
              value={selectedCustomer?.id || ''}
              onChange={(e) => {
                const customer = customers?.find(c => c.id === e.target.value);
                setSelectedCustomer(customer || null);
              }}
              className="w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
            >
              <option value="">-- Select Customer --</option>
              {customers?.map(customer => (
                <option key={customer.id} value={customer.id}>
                  {customer.firstName} {customer.lastName} (${customer.wallet?.balance.toFixed(2) || '0.00'})
                </option>
              ))}
            </select>
          </div>

          {selectedCustomer && (
            <div className="mt-3 p-3 bg-indigo-50 rounded-lg">
              <div className="flex justify-between items-center">
                <div>
                  <p className="text-sm font-medium text-gray-900">
                    {selectedCustomer.firstName} {selectedCustomer.lastName}
                  </p>
                  <p className="text-xs text-gray-500">{selectedCustomer.email}</p>
                </div>
                <div className="text-right">
                  <p className="text-sm font-bold text-indigo-600">
                    ${selectedCustomer.wallet?.balance.toFixed(2) || '0.00'}
                  </p>
                  <p className="text-xs text-gray-500">Wallet</p>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Cart Items */}
        <div className="flex-1 overflow-y-auto p-4 space-y-3">
          {cart.length > 0 ? (
            cart.map((item, index) => (
              <div key={index} className="bg-gray-50 rounded-lg p-3 border border-gray-200">
                <div className="flex justify-between items-start mb-2">
                  <div className="flex-1">
                    <h4 className="text-sm font-medium text-gray-900">{item.productName}</h4>
                    {item.variant && (
                      <p className="text-xs text-gray-500">
                        {item.variant.size && `Size: ${item.variant.size}`}
                        {item.variant.size && item.variant.color && ' • '}
                        {item.variant.color && `Color: ${item.variant.color}`}
                      </p>
                    )}
                  </div>
                  <button
                    onClick={() => handleRemoveFromCart(index)}
                    className="text-red-600 hover:text-red-800"
                  >
                    <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => handleUpdateQuantity(index, item.quantity - 1)}
                      className="w-8 h-8 rounded-md border border-gray-300 flex items-center justify-center hover:bg-gray-100"
                    >
                      -
                    </button>
                    <span className="w-12 text-center text-sm font-medium">{item.quantity}</span>
                    <button
                      onClick={() => handleUpdateQuantity(index, item.quantity + 1)}
                      className="w-8 h-8 rounded-md border border-gray-300 flex items-center justify-center hover:bg-gray-100"
                    >
                      +
                    </button>
                  </div>
                  <span className="text-sm font-bold text-gray-900">
                    ${(item.price * item.quantity).toFixed(2)}
                  </span>
                </div>
              </div>
            ))
          ) : (
            <div className="text-center py-8">
              <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
              </svg>
              <p className="mt-2 text-sm text-gray-500">Cart is empty</p>
            </div>
          )}
        </div>

        {/* Cart Footer */}
        <div className="p-4 border-t bg-gray-50">
          <div className="flex justify-between items-center mb-3">
            <span className="text-base font-semibold text-gray-900">Total</span>
            <span className="text-xl font-bold text-gray-900">${calculateTotal().toFixed(2)}</span>
          </div>
          {selectedCustomer && cart.length > 0 && (
            <div className="mb-3">
              {calculateTotal() > (selectedCustomer.wallet?.balance || 0) ? (
                <p className="text-xs text-red-600">⚠️ Insufficient wallet balance</p>
              ) : (
                <p className="text-xs text-green-600">✓ Sufficient wallet balance</p>
              )}
            </div>
          )}
          <div className="grid grid-cols-2 gap-2">
            <button
              onClick={handleClearCart}
              disabled={cart.length === 0}
              className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Clear
            </button>
            <button
              onClick={handlePlaceOrder}
              disabled={!selectedCustomer || cart.length === 0}
              className="px-4 py-2 bg-indigo-600 text-white rounded-md text-sm font-medium hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Place Order
            </button>
          </div>
        </div>
      </div>

      {/* Right Column - Variant Selector or Summary */}
      <div className="w-1/3 bg-white shadow rounded-lg flex flex-col">
        <div className="p-4 border-b">
          <h2 className="text-lg font-semibold text-gray-900">
            {selectedProduct ? 'Select Variant' : 'Order Summary'}
          </h2>
        </div>
        <div className="flex-1 overflow-y-auto p-4">
          {selectedProduct ? (
            <div>
              {selectedProduct.imageUrl && (
                <img
                  src={selectedProduct.imageUrl}
                  alt={selectedProduct.name}
                  className="w-full h-48 object-cover rounded-lg mb-4"
                />
              )}
              <h3 className="text-lg font-semibold text-gray-900 mb-2">{selectedProduct.name}</h3>
              {selectedProduct.description && (
                <p className="text-sm text-gray-600 mb-4">{selectedProduct.description}</p>
              )}
              <p className="text-xl font-bold text-gray-900 mb-4">${selectedProduct.basePrice.toFixed(2)}</p>

              {selectedProduct.variants && selectedProduct.variants.length > 0 ? (
                <div className="space-y-2">
                  <h4 className="text-sm font-medium text-gray-700 mb-2">Select Variant:</h4>
                  {selectedProduct.variants.map((variant) => (
                    <button
                      key={variant.id}
                      onClick={() => handleAddToCart(selectedProduct, variant)}
                      disabled={variant.stock === 0}
                      className="w-full p-3 text-left border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      <div className="flex justify-between items-center">
                        <div>
                          <p className="text-sm font-medium text-gray-900">{variant.sku}</p>
                          <p className="text-xs text-gray-500">
                            {variant.size && `Size: ${variant.size}`}
                            {variant.size && variant.color && ' • '}
                            {variant.color && `Color: ${variant.color}`}
                          </p>
                          <p className="text-xs text-gray-500 mt-1">
                            Stock: {variant.stock}
                          </p>
                        </div>
                        <div className="text-right">
                          <p className="text-sm font-bold text-gray-900">
                            ${(variant.price || selectedProduct.basePrice).toFixed(2)}
                          </p>
                        </div>
                      </div>
                    </button>
                  ))}
                </div>
              ) : (
                <div className="text-center py-8">
                  <p className="text-sm text-gray-500">No variants available for this product</p>
                  <p className="text-xs text-gray-400 mt-1">Products must have at least one variant to be sold</p>
                </div>
              )}

              <button
                onClick={() => setSelectedProduct(null)}
                className="w-full mt-3 px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>
            </div>
          ) : (
            <div>
              {cart.length > 0 ? (
                <div className="space-y-4">
                  <div className="bg-gradient-to-r from-indigo-500 to-purple-600 rounded-lg p-4 text-white">
                    <p className="text-sm opacity-90">Current Order</p>
                    <p className="text-2xl font-bold mt-1">${calculateTotal().toFixed(2)}</p>
                    <p className="text-xs opacity-75 mt-1">{cart.length} items</p>
                  </div>

                  {selectedCustomer && (
                    <div className="bg-gray-50 rounded-lg p-4">
                      <h4 className="text-sm font-medium text-gray-900 mb-2">Customer Info</h4>
                      <p className="text-sm text-gray-700">{selectedCustomer.firstName} {selectedCustomer.lastName}</p>
                      <p className="text-xs text-gray-500">{selectedCustomer.email}</p>
                      <p className="text-xs text-gray-500">{selectedCustomer.phoneNumber}</p>
                      <div className="mt-3 pt-3 border-t border-gray-200">
                        <div className="flex justify-between">
                          <span className="text-sm text-gray-600">Wallet Balance:</span>
                          <span className="text-sm font-bold text-gray-900">
                            ${selectedCustomer.wallet?.balance.toFixed(2) || '0.00'}
                          </span>
                        </div>
                        <div className="flex justify-between mt-1">
                          <span className="text-sm text-gray-600">After Purchase:</span>
                          <span className="text-sm font-bold text-gray-900">
                            ${((selectedCustomer.wallet?.balance || 0) - calculateTotal()).toFixed(2)}
                          </span>
                        </div>
                      </div>
                    </div>
                  )}

                  <div className="bg-gray-50 rounded-lg p-4">
                    <h4 className="text-sm font-medium text-gray-900 mb-2">Order Items</h4>
                    <ul className="space-y-2">
                      {cart.map((item, index) => (
                        <li key={index} className="text-sm flex justify-between">
                          <span className="text-gray-700">
                            {item.quantity}x {item.productName}
                            {item.variant && ` (${item.variant.sku})`}
                          </span>
                          <span className="font-medium text-gray-900">
                            ${(item.price * item.quantity).toFixed(2)}
                          </span>
                        </li>
                      ))}
                    </ul>
                  </div>
                </div>
              ) : (
                <div className="text-center py-12">
                  <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                  </svg>
                  <p className="mt-2 text-sm text-gray-500">No active order</p>
                  <p className="text-xs text-gray-400 mt-1">Add items to start a new order</p>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
      </div>
    </div>
  );
};

export default LiveSalesPage;
