import { useState, useEffect } from 'react';
import { useGetProducts, useCreateProduct, useGetTags } from '../../hooks/useProducts';
import { useGetCustomers } from '../../hooks/useCustomers';
import {
  useCreateSalesOrder,
  useAddItemToOrder,
  useUpdateItemInOrder,
  useRemoveItemFromOrder,
  useFinalizeOrder,
  useCancelSalesOrder,
  useGetSalesOrders
} from '../../hooks/useSalesOrders';
import { Product, ProductVariant, ProductVariantDto, CreateProductDto } from '../../types/product.types';
import { Customer } from '../../types/customer.types';
import { SalesOrder } from '../../types/salesorder.types';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '../../components/ui/card';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../components/ui/dialog';
import { Badge } from '../../components/ui/badge';
import { PlusCircle, ShoppingCart, Edit2, Trash2, Check, X, Package, ClipboardList, Users } from 'lucide-react';
import ProductFormModal from '../../components/products/ProductFormModal';
import VariantModal from '../../components/products/VariantModal';
import { CustomerCombobox } from '../../components/common/CustomerCombobox';
import { CustomAlertDialog } from '../../components/common/AlertDialog';
import { ConfirmDialog } from '../../components/common/ConfirmDialog';

const LiveSalesPage = () => {
  const { data: products } = useGetProducts(false); // Only published products
  const { data: customers } = useGetCustomers();
  const { data: tags } = useGetTags();
  const { data: draftOrders, refetch: refetchDraftOrders } = useGetSalesOrders('Draft');
  const createSalesOrder = useCreateSalesOrder();
  const addItemToOrder = useAddItemToOrder();
  const updateItemInOrder = useUpdateItemInOrder();
  const removeItemFromOrder = useRemoveItemFromOrder();
  const finalizeOrder = useFinalizeOrder();
  const cancelOrder = useCancelSalesOrder();
  const createProduct = useCreateProduct();

  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);
  const [currentDraftOrder, setCurrentDraftOrder] = useState<SalesOrder | null>(null);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [showVariantDialog, setShowVariantDialog] = useState(false);
  const [editingItem, setEditingItem] = useState<{ itemId: string; quantity: number; price: number } | null>(null);
  const [orderSuccess, setOrderSuccess] = useState(false);

  // Product creation modal states
  const [showProductModal, setShowProductModal] = useState(false);
  const [showVariantManagerModal, setShowVariantManagerModal] = useState(false);
  const [productVariants, setProductVariants] = useState<ProductVariantDto[]>([]);

  // Dialog states
  const [alertDialog, setAlertDialog] = useState<{ open: boolean; title: string; description: string }>({
    open: false,
    title: '',
    description: '',
  });
  const [confirmDialog, setConfirmDialog] = useState<{
    open: boolean;
    title: string;
    description: string;
    onConfirm: () => void;
    variant?: 'default' | 'destructive';
    confirmText?: string;
  }>({
    open: false,
    title: '',
    description: '',
    onConfirm: () => {},
  });

  const filteredProducts = searchTerm
    ? products?.filter(p =>
        p.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.description?.toLowerCase().includes(searchTerm.toLowerCase())
      )
    : products;

  // Refetch draft orders when mutations complete
  useEffect(() => {
    refetchDraftOrders();
  }, [createSalesOrder.isSuccess, addItemToOrder.isSuccess, updateItemInOrder.isSuccess, removeItemFromOrder.isSuccess, cancelOrder.isSuccess, finalizeOrder.isSuccess]);

  // Create initial draft order or add to existing
  const handleCreateDraftOrder = async (customerId: string, firstItem: { productVariantId: string; quantity: number; price: number }) => {
    try {
      const orderData = {
        customerId,
        items: [{
          productVariantId: firstItem.productVariantId,
          quantity: firstItem.quantity,
          customUnitPrice: firstItem.price,
        }],
      };

      const newOrder = await createSalesOrder.mutateAsync(orderData);
      setCurrentDraftOrder(newOrder);
    } catch (error) {
      console.error('Error creating draft order:', error);
      setAlertDialog({
        open: true,
        title: 'Error al crear orden',
        description: 'No se pudo crear la orden. Por favor intente nuevamente.',
      });
    }
  };

  // Add variant to order
  const handleAddToOrder = async (product: Product, variant: ProductVariant) => {
    if (!variant) {
      setAlertDialog({
        open: true,
        title: 'Variante requerida',
        description: 'Por favor seleccione una variante antes de agregar el producto.',
      });
      return;
    }

    if (!selectedCustomer) {
      setAlertDialog({
        open: true,
        title: 'Cliente requerido',
        description: 'Por favor seleccione un cliente primero antes de agregar productos.',
      });
      return;
    }

    const price = variant.price && variant.price > 0 ? variant.price : product.basePrice;

    try {
      if (currentDraftOrder) {
        // Add to existing draft order
        const updatedOrder = await addItemToOrder.mutateAsync({
          orderId: currentDraftOrder.id,
          item: {
            productVariantId: variant.id,
            quantity: 1,
            customUnitPrice: price,
          },
        });
        setCurrentDraftOrder(updatedOrder);
      } else {
        // Create new draft order with first item
        await handleCreateDraftOrder(selectedCustomer.id, {
          productVariantId: variant.id,
          quantity: 1,
          price,
        });
      }

      setSelectedProduct(null);
      setShowVariantDialog(false);
    } catch (error) {
      console.error('Error adding item to order:', error);
      setAlertDialog({
        open: true,
        title: 'Error al agregar producto',
        description: 'No se pudo agregar el producto a la orden. Por favor intente nuevamente.',
      });
    }
  };

  // Update item in order
  const handleUpdateItem = async (itemId: string, quantity: number, price: number) => {
    if (!currentDraftOrder) return;

    try {
      const updatedOrder = await updateItemInOrder.mutateAsync({
        orderId: currentDraftOrder.id,
        itemId,
        data: {
          quantity,
          customUnitPrice: price,
        },
      });
      setCurrentDraftOrder(updatedOrder);
      setEditingItem(null);
    } catch (error) {
      console.error('Error updating item:', error);
      setAlertDialog({
        open: true,
        title: 'Error al actualizar item',
        description: 'No se pudo actualizar el item. Por favor intente nuevamente.',
      });
    }
  };

  // Remove item from order
  const handleRemoveItem = (itemId: string) => {
    if (!currentDraftOrder) return;

    setConfirmDialog({
      open: true,
      title: 'Eliminar item',
      description: '¿Está seguro de que desea eliminar este item de la orden?',
      variant: 'destructive',
      confirmText: 'Eliminar',
      onConfirm: async () => {
        try {
          const updatedOrder = await removeItemFromOrder.mutateAsync({
            orderId: currentDraftOrder.id,
            itemId,
          });
          setCurrentDraftOrder(updatedOrder);
        } catch (error: any) {
          console.error('Error removing item:', error);
          if (error.response?.data?.message?.includes('último item')) {
            setConfirmDialog({
              open: true,
              title: 'Último item',
              description: 'Esta es la última item de la orden. ¿Desea cancelar toda la orden?',
              variant: 'destructive',
              confirmText: 'Cancelar orden',
              onConfirm: () => handleCancelOrder(),
            });
          } else {
            setAlertDialog({
              open: true,
              title: 'Error al eliminar item',
              description: 'No se pudo eliminar el item. Por favor intente nuevamente.',
            });
          }
        }
      },
    });
  };

  // Finalize order
  const handleFinalizeOrder = () => {
    if (!currentDraftOrder || !selectedCustomer) return;

    const total = currentDraftOrder.totalAmount;
    const balance = selectedCustomer.wallet?.balance || 0;

    if (total > balance) {
      setAlertDialog({
        open: true,
        title: 'Fondos insuficientes',
        description: 'El cliente no tiene fondos suficientes en su wallet. Por favor agregue fondos primero.',
      });
      return;
    }

    setConfirmDialog({
      open: true,
      title: 'Finalizar orden',
      description: `¿Confirmar la orden por $${total.toFixed(2)}? Se descontará del wallet del cliente y se actualizará el inventario.`,
      confirmText: 'Finalizar orden',
      onConfirm: async () => {
        try {
          await finalizeOrder.mutateAsync(currentDraftOrder.id);

          setOrderSuccess(true);
          setTimeout(() => setOrderSuccess(false), 3000);

          // Reset state
          setCurrentDraftOrder(null);
          setSelectedCustomer(null);
        } catch (error) {
          console.error('Error finalizing order:', error);
          setAlertDialog({
            open: true,
            title: 'Error al finalizar orden',
            description: 'No se pudo finalizar la orden. Por favor intente nuevamente.',
          });
        }
      },
    });
  };

  // Cancel order
  const handleCancelOrder = () => {
    if (!currentDraftOrder) return;

    setConfirmDialog({
      open: true,
      title: 'Cancelar orden',
      description: '¿Está seguro de que desea cancelar esta orden? Esta acción no se puede deshacer.',
      variant: 'destructive',
      confirmText: 'Cancelar orden',
      onConfirm: async () => {
        try {
          await cancelOrder.mutateAsync(currentDraftOrder.id);
          setCurrentDraftOrder(null);
          setSelectedCustomer(null);
        } catch (error) {
          console.error('Error canceling order:', error);
          setAlertDialog({
            open: true,
            title: 'Error al cancelar orden',
            description: 'No se pudo cancelar la orden. Por favor intente nuevamente.',
          });
        }
      },
    });
  };

  // Switch to existing draft order
  const handleSelectDraftOrder = (order: SalesOrder) => {
    const customer = customers?.find(c => c.id === order.customerId);
    setSelectedCustomer(customer || null);
    setCurrentDraftOrder(order);
  };

  // Start new order (save current if exists)
  const handleNewOrder = () => {
    if (currentDraftOrder) {
      // Current draft is already saved, just clear state
      setCurrentDraftOrder(null);
      setSelectedCustomer(null);
    }
  };

  const openVariantDialog = (product: Product) => {
    setSelectedProduct(product);
    setShowVariantDialog(true);
  };

  // Handle product creation
  const handleCreateProduct = async (data: CreateProductDto, isEditing: boolean) => {
    if (isEditing) return; // Solo crear, no editar

    try {
      await createProduct.mutateAsync(data);
      setShowProductModal(false);
      setProductVariants([]);
    } catch (error) {
      console.error('Error creating product:', error);
      setAlertDialog({
        open: true,
        title: 'Error al crear producto',
        description: 'No se pudo crear el producto. Por favor intente nuevamente.',
      });
    }
  };

  return (
    <div className="space-y-4">
      {/* Success Banner */}
      {orderSuccess && (
        <div className="bg-green-50 border-l-4 border-green-400 p-4 rounded">
          <div className="flex">
            <div className="flex-shrink-0">
              <Check className="h-5 w-5 text-green-400" />
            </div>
            <div className="ml-3">
              <p className="text-sm text-green-700">
                Orden completada exitosamente. El wallet ha sido debitado y el inventario actualizado.
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Draft Orders Bar */}
      {draftOrders && draftOrders.length > 0 && (
        <Card>
          <CardHeader className="pb-3">
            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <ClipboardList className="h-5 w-5 mr-2 text-indigo-600" />
                <CardTitle className="text-base">Órdenes en Progreso ({draftOrders.length})</CardTitle>
              </div>
              <Button size="sm" variant="outline" onClick={handleNewOrder}>
                <PlusCircle className="h-4 w-4 mr-1" />
                Nueva Orden
              </Button>
            </div>
          </CardHeader>
          <CardContent className="pt-0">
            <div className="flex gap-2 overflow-x-auto pb-2">
              {draftOrders.map((order) => (
                <button
                  key={order.id}
                  onClick={() => handleSelectDraftOrder(order)}
                  className={`flex-shrink-0 p-3 rounded-lg border-2 transition-all ${
                    currentDraftOrder?.id === order.id
                      ? 'border-indigo-600 bg-indigo-50'
                      : 'border-gray-200 hover:border-gray-300 bg-white'
                  }`}
                >
                  <div className="text-left min-w-[200px]">
                    <p className="text-sm font-medium text-gray-900">{order.customerName}</p>
                    <p className="text-xs text-gray-500">{order.items.length} items</p>
                    <p className="text-sm font-bold text-indigo-600 mt-1">${order.totalAmount.toFixed(2)}</p>
                  </div>
                </button>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      <div className="h-[calc(100vh-16rem)] flex gap-4">
        {/* Left Column - Product Search */}
        <Card className="w-1/3 flex flex-col">
          <CardHeader>
            <CardTitle className="flex items-center justify-between">
              <span>Productos</span>
              <Button size="sm" variant="outline" onClick={() => setShowProductModal(true)}>
                <PlusCircle className="h-4 w-4 mr-1" />
                Nuevo
              </Button>
            </CardTitle>
            <CardDescription>
              <Input
                type="text"
                placeholder="Buscar productos..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="mt-2"
              />
            </CardDescription>
          </CardHeader>
          <CardContent className="flex-1 overflow-y-auto space-y-3 p-4">
            {filteredProducts && filteredProducts.length > 0 ? (
              filteredProducts.map((product) => (
                <div
                  key={product.id}
                  onClick={() => openVariantDialog(product)}
                  className="bg-gray-50 rounded-lg p-3 cursor-pointer hover:bg-gray-100 border border-gray-200 transition-colors"
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
                        {product.variants?.reduce((sum, v) => sum + v.stock, 0) || 0} unidades disponibles
                      </p>
                    </div>
                  </div>
                </div>
              ))
            ) : (
              <div className="text-center py-8">
                <Package className="mx-auto h-12 w-12 text-gray-400" />
                <p className="mt-2 text-sm text-gray-500">No se encontraron productos</p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Middle Column - Current Order */}
        <Card className="w-1/3 flex flex-col">
          <CardHeader>
            <CardTitle className="flex items-center">
              <ShoppingCart className="h-5 w-5 mr-2" />
              Orden Actual
            </CardTitle>
            <CardDescription>
              <div className="mt-2">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Cliente
                </label>
                <CustomerCombobox
                  customers={customers}
                  selectedCustomer={selectedCustomer}
                  onSelectCustomer={(customer) => {
                    // Check if this customer already has a draft order
                    const existingOrder = draftOrders?.find(o => o.customerId === customer?.id);
                    if (existingOrder && customer) {
                      setConfirmDialog({
                        open: true,
                        title: 'Orden existente',
                        description: `${customer.firstName} ${customer.lastName} ya tiene una orden en progreso. ¿Desea continuar con esa orden?`,
                        confirmText: 'Continuar con orden',
                        onConfirm: () => handleSelectDraftOrder(existingOrder),
                      });
                    } else {
                      setSelectedCustomer(customer);
                      if (!customer && currentDraftOrder) {
                        // If deselecting customer and there's a current order, keep the order
                        // User can manually cancel if needed
                      } else if (customer && !currentDraftOrder) {
                        // New customer selected, no current order - ready to add items
                        setCurrentDraftOrder(null);
                      }
                    }
                  }}
                  disabled={!!currentDraftOrder}
                />
              </div>
            </CardDescription>
          </CardHeader>

          <CardContent className="flex-1 overflow-y-auto space-y-3 p-4">
            {currentDraftOrder && currentDraftOrder.items && currentDraftOrder.items.length > 0 ? (
              currentDraftOrder.items.map((item) => (
                <div key={item.id} className="bg-white rounded-lg p-3 border border-gray-200">
                  {editingItem?.itemId === item.id ? (
                    <div className="space-y-2">
                      <div className="flex justify-between items-start mb-2">
                        <div className="flex-1">
                          <h4 className="text-sm font-medium text-gray-900">{item.productName}</h4>
                          <p className="text-xs text-gray-500">{item.variantSku}</p>
                        </div>
                      </div>
                      <div className="grid grid-cols-2 gap-2">
                        <div>
                          <label className="text-xs text-gray-600">Cantidad</label>
                          <Input
                            type="number"
                            min="1"
                            value={editingItem.quantity}
                            onChange={(e) => setEditingItem({ ...editingItem, quantity: parseInt(e.target.value) || 1 })}
                            className="mt-1"
                          />
                        </div>
                        <div>
                          <label className="text-xs text-gray-600">Precio Unit.</label>
                          <Input
                            type="number"
                            min="0"
                            step="0.01"
                            value={editingItem.price}
                            onChange={(e) => setEditingItem({ ...editingItem, price: parseFloat(e.target.value) || 0 })}
                            className="mt-1"
                          />
                        </div>
                      </div>
                      <div className="flex gap-2">
                        <Button
                          size="sm"
                          onClick={() => handleUpdateItem(item.id, editingItem.quantity, editingItem.price)}
                          className="flex-1"
                        >
                          <Check className="h-4 w-4 mr-1" />
                          Guardar
                        </Button>
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() => setEditingItem(null)}
                          className="flex-1"
                        >
                          <X className="h-4 w-4 mr-1" />
                          Cancelar
                        </Button>
                      </div>
                    </div>
                  ) : (
                    <>
                      <div className="flex justify-between items-start mb-2">
                        <div className="flex-1">
                          <h4 className="text-sm font-medium text-gray-900">{item.productName}</h4>
                          <p className="text-xs text-gray-500">{item.variantSku}</p>
                          {item.unitPrice !== item.originalPrice && (
                            <Badge variant="secondary" className="mt-1 text-xs">
                              Precio modificado
                            </Badge>
                          )}
                        </div>
                        <div className="flex gap-1">
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={() => setEditingItem({ itemId: item.id, quantity: item.quantity, price: item.unitPrice })}
                          >
                            <Edit2 className="h-4 w-4" />
                          </Button>
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={() => handleRemoveItem(item.id)}
                          >
                            <Trash2 className="h-4 w-4 text-red-600" />
                          </Button>
                        </div>
                      </div>
                      <div className="flex justify-between items-center text-sm">
                        <div>
                          <span className="text-gray-600">Cantidad:</span> <span className="font-medium">{item.quantity}</span>
                          <span className="text-gray-600 ml-3">x</span> <span className="font-medium">${item.unitPrice.toFixed(2)}</span>
                        </div>
                        <span className="font-bold text-gray-900">${item.subtotal.toFixed(2)}</span>
                      </div>
                    </>
                  )}
                </div>
              ))
            ) : (
              <div className="text-center py-12">
                <ShoppingCart className="mx-auto h-12 w-12 text-gray-400" />
                <p className="mt-2 text-sm text-gray-500">Sin items</p>
                <p className="text-xs text-gray-400 mt-1">
                  {selectedCustomer
                    ? 'Agregue productos para crear una orden'
                    : 'Seleccione un cliente para iniciar'}
                </p>
              </div>
            )}
          </CardContent>

          {currentDraftOrder && (
            <CardFooter className="flex-col space-y-3 border-t bg-gray-50 p-4">
              <div className="w-full flex justify-between items-center">
                <span className="text-base font-semibold text-gray-900">Total</span>
                <span className="text-xl font-bold text-gray-900">${currentDraftOrder.totalAmount.toFixed(2)}</span>
              </div>
              <div className="w-full grid grid-cols-2 gap-2">
                <Button
                  variant="outline"
                  onClick={handleCancelOrder}
                  disabled={!currentDraftOrder}
                >
                  Cancelar
                </Button>
                <Button
                  onClick={handleFinalizeOrder}
                  disabled={!currentDraftOrder || !selectedCustomer}
                >
                  Finalizar Orden
                </Button>
              </div>
            </CardFooter>
          )}
        </Card>

        {/* Right Column - Customer & Summary */}
        <Card className="w-1/3 flex flex-col">
          <CardHeader>
            <CardTitle className="flex items-center">
              <Users className="h-5 w-5 mr-2" />
              Resumen
            </CardTitle>
          </CardHeader>
          <CardContent className="flex-1 overflow-y-auto p-4 space-y-4">
            {selectedCustomer ? (
              <>
                <div className="bg-gradient-to-r from-indigo-500 to-purple-600 rounded-lg p-4 text-white">
                  <p className="text-sm opacity-90">Cliente Seleccionado</p>
                  <p className="text-xl font-bold mt-1">
                    {selectedCustomer.firstName} {selectedCustomer.lastName}
                  </p>
                  <p className="text-xs opacity-75 mt-1">{selectedCustomer.email}</p>
                </div>

                <div className="bg-white rounded-lg p-4 border border-gray-200">
                  <h4 className="text-sm font-medium text-gray-900 mb-3">Información de Wallet</h4>
                  <div className="space-y-2">
                    <div className="flex justify-between">
                      <span className="text-sm text-gray-600">Balance Actual:</span>
                      <span className="text-sm font-bold text-green-600">
                        ${selectedCustomer.wallet?.balance.toFixed(2) || '0.00'}
                      </span>
                    </div>
                    {currentDraftOrder && (
                      <>
                        <div className="flex justify-between">
                          <span className="text-sm text-gray-600">Total Orden:</span>
                          <span className="text-sm font-bold text-gray-900">
                            ${currentDraftOrder.totalAmount.toFixed(2)}
                          </span>
                        </div>
                        <div className="pt-2 border-t border-gray-200">
                          <div className="flex justify-between">
                            <span className="text-sm text-gray-600">Balance Después:</span>
                            <span className={`text-sm font-bold ${
                              (selectedCustomer.wallet?.balance || 0) - currentDraftOrder.totalAmount >= 0
                                ? 'text-green-600'
                                : 'text-red-600'
                            }`}>
                              ${((selectedCustomer.wallet?.balance || 0) - currentDraftOrder.totalAmount).toFixed(2)}
                            </span>
                          </div>
                        </div>
                        {currentDraftOrder.totalAmount > (selectedCustomer.wallet?.balance || 0) && (
                          <div className="mt-3 p-2 bg-red-50 border border-red-200 rounded">
                            <p className="text-xs text-red-700">
                              Fondos insuficientes. Faltan: $
                              {(currentDraftOrder.totalAmount - (selectedCustomer.wallet?.balance || 0)).toFixed(2)}
                            </p>
                          </div>
                        )}
                      </>
                    )}
                  </div>
                </div>

                {currentDraftOrder && currentDraftOrder.items && currentDraftOrder.items.length > 0 && (
                  <div className="bg-white rounded-lg p-4 border border-gray-200">
                    <h4 className="text-sm font-medium text-gray-900 mb-3">Items de la Orden</h4>
                    <ul className="space-y-2">
                      {currentDraftOrder.items.map((item) => (
                        <li key={item.id} className="text-sm flex justify-between">
                          <span className="text-gray-700">
                            {item.quantity}x {item.productName}
                          </span>
                          <span className="font-medium text-gray-900">
                            ${item.subtotal.toFixed(2)}
                          </span>
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </>
            ) : (
              <div className="text-center py-12">
                <div className="mx-auto h-12 w-12 rounded-full bg-gray-100 flex items-center justify-center">
                  <Users className="h-6 w-6 text-gray-400" />
                </div>
                <p className="mt-2 text-sm text-gray-500">Sin cliente seleccionado</p>
                <p className="text-xs text-gray-400 mt-1">Seleccione un cliente para iniciar</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Variant Selection Dialog */}
      <Dialog open={showVariantDialog} onOpenChange={setShowVariantDialog}>
        <DialogContent className="sm:max-w-[500px]">
          <DialogHeader>
            <DialogTitle>{selectedProduct?.name}</DialogTitle>
            <DialogDescription>
              Seleccione una variante para agregar a la orden
            </DialogDescription>
          </DialogHeader>

          {selectedProduct && (
            <div className="space-y-4">
              {selectedProduct.imageUrl && (
                <img
                  src={selectedProduct.imageUrl}
                  alt={selectedProduct.name}
                  className="w-full h-48 object-cover rounded-lg"
                />
              )}

              <div>
                <p className="text-sm text-gray-600">{selectedProduct.description}</p>
                <p className="text-xl font-bold text-gray-900 mt-2">${selectedProduct.basePrice.toFixed(2)}</p>
              </div>

              {selectedProduct.variants && selectedProduct.variants.length > 0 ? (
                <div className="space-y-2 max-h-64 overflow-y-auto">
                  <h4 className="text-sm font-medium text-gray-700">Variantes disponibles:</h4>
                  {selectedProduct.variants.map((variant) => (
                    <button
                      key={variant.id}
                      onClick={() => handleAddToOrder(selectedProduct, variant)}
                      disabled={variant.stock === 0 || !selectedCustomer}
                      className="w-full p-3 text-left border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                      <div className="flex justify-between items-center">
                        <div>
                          <p className="text-sm font-medium text-gray-900">{variant.sku}</p>
                          <p className="text-xs text-gray-500">
                            {variant.size && `Talla: ${variant.size}`}
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
                  <p className="text-sm text-gray-500">No hay variantes disponibles</p>
                </div>
              )}
            </div>
          )}

          <DialogFooter>
            <Button variant="outline" onClick={() => setShowVariantDialog(false)}>
              Cerrar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Product Creation Modal */}
      <ProductFormModal
        isOpen={showProductModal}
        editingProduct={null}
        tags={tags}
        isLoading={createProduct.isPending}
        variants={productVariants}
        onClose={() => {
          setShowProductModal(false);
          setProductVariants([]);
        }}
        onSubmit={handleCreateProduct}
        onOpenVariantModal={() => setShowVariantManagerModal(true)}
      />

      {/* Variant Manager Modal */}
      <VariantModal
        isOpen={showVariantManagerModal}
        variants={productVariants}
        onClose={() => setShowVariantManagerModal(false)}
        onSaveVariants={(variants) => {
          setProductVariants(variants);
          setShowVariantManagerModal(false);
        }}
      />

      {/* Alert Dialog */}
      <CustomAlertDialog
        open={alertDialog.open}
        onClose={() => setAlertDialog({ ...alertDialog, open: false })}
        title={alertDialog.title}
        description={alertDialog.description}
      />

      {/* Confirm Dialog */}
      <ConfirmDialog
        open={confirmDialog.open}
        onClose={() => setConfirmDialog({ ...confirmDialog, open: false })}
        onConfirm={confirmDialog.onConfirm}
        title={confirmDialog.title}
        description={confirmDialog.description}
        variant={confirmDialog.variant}
        confirmText={confirmDialog.confirmText}
      />
    </div>
  );
};

export default LiveSalesPage;
