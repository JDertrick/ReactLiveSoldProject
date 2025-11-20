import { useState, useEffect, useMemo } from "react";
import {
  useGetProducts,
  useCreateProduct,
  useGetTags,
} from "../../hooks/useProducts";
import { useGetCustomers } from "../../hooks/useCustomers";
import { useCategories } from "../../hooks/useCategories";
import {
  useCreateSalesOrder,
  useAddItemToOrder,
  useUpdateItemInOrder,
  useRemoveItemFromOrder,
  useFinalizeOrder,
  useCancelSalesOrder,
  useGetSalesOrders,
} from "../../hooks/useSalesOrders";
import {
  Product,
  ProductVariant,
  CreateProductDto,
  UpdateProductDto,
  VariantProductDto,
} from "../../types/product.types";
import { Customer } from "../../types/customer.types";
import { SalesOrder, SaleType } from "../../types/salesorder.types";
import { Card, CardContent } from "../../components/ui/card";
import { Button } from "../../components/ui/button";
import { Input } from "../../components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "../../components/ui/dialog";
import { Badge } from "../../components/ui/badge";
import {
  ShoppingCart,
  Edit2,
  Trash2,
  X,
  Users,
  Search,
  Plus,
  Minus,
  MoreVertical,
  Save,
  QrCode,
} from "lucide-react";
import ProductFormModal from "../../components/products/ProductFormModal";
import VariantModal from "../../components/products/VariantModal";
import { CustomerCombobox } from "../../components/common/CustomerCombobox";
import { CustomAlertDialog } from "../../components/common/AlertDialog";
import { ConfirmDialog } from "../../components/common/ConfirmDialog";

const LiveSalesPage = () => {
  const { data: productsData } = useGetProducts(1, 9999, "published", ""); // Fetch all published products
  const { data: customers } = useGetCustomers();
  const { data: tags } = useGetTags();
  const { categories } = useCategories();
  const { data: draftOrders, refetch: refetchDraftOrders } =
    useGetSalesOrders("Draft");
  const createSalesOrder = useCreateSalesOrder();
  const addItemToOrder = useAddItemToOrder();
  const updateItemInOrder = useUpdateItemInOrder();
  const removeItemFromOrder = useRemoveItemFromOrder();
  const finalizeOrder = useFinalizeOrder();
  const cancelOrder = useCancelSalesOrder();
  const createProduct = useCreateProduct();

  const [activeCategory, setActiveCategory] = useState({
    id: "all",
    name: "Todo",
  });
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(
    null
  );
  const [currentDraftOrder, setCurrentDraftOrder] = useState<SalesOrder | null>(
    null
  );
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [showVariantDialog, setShowVariantDialog] = useState(false);
  const [saleType, setSaleType] = useState<SaleType>(SaleType.Retail);
  const [editingItem, setEditingItem] = useState<{
    itemId: string;
    quantity: number;
    price: number;
  } | null>(null);

  const [alertDialog, setAlertDialog] = useState<{
    open: boolean;
    title: string;
    description: string;
  }>({
    open: false,
    title: "",
    description: "",
  });
  const [confirmDialog, setConfirmDialog] = useState<{
    open: boolean;
    title: string;
    description: string;
    onConfirm: () => void;
    variant?: "default" | "destructive";
    confirmText?: string;
  }>({
    open: false,
    title: "",
    description: "",
    onConfirm: () => {},
  });

  // Product creation modal states
  const [showProductModal, setShowProductModal] = useState(false);
  const [showVariantManagerModal, setShowVariantManagerModal] = useState(false);

  const products = useMemo(() => productsData?.items ?? [], [productsData]);

  const filteredVariantsProducts = useMemo(() => {
    let filteredProducts = products;

    if (activeCategory.id !== "all") {
      filteredProducts = filteredProducts.filter(
        (p) => p.product.categoryId === activeCategory.id
      );
    }

    if (searchTerm) {
      filteredProducts = filteredProducts.filter(
        (p) =>
          p.productName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
          p.productDescription
            ?.toLowerCase()
            .includes(searchTerm.toLowerCase()) ||
          p.sku?.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    return filteredProducts;
  }, [products, activeCategory, searchTerm]);

  // Refetch draft orders when mutations complete
  useEffect(() => {
    refetchDraftOrders();
  }, [
    createSalesOrder.isSuccess,
    addItemToOrder.isSuccess,
    updateItemInOrder.isSuccess,
    removeItemFromOrder.isSuccess,
    cancelOrder.isSuccess,
    finalizeOrder.isSuccess,
  ]);

  // Create initial draft order or add to existing
  const handleCreateDraftOrder = async (
    customerId: string,
    firstItem: { productVariantId: string; quantity: number; price: number; saleType: SaleType }
  ) => {
    try {
      const orderData = {
        customerId,
        items: [
          {
            productVariantId: firstItem.productVariantId,
            quantity: firstItem.quantity,
            saleType: firstItem.saleType,
            customUnitPrice: firstItem.price,
          },
        ],
      };

      const newOrder = await createSalesOrder.mutateAsync(orderData);
      setCurrentDraftOrder(newOrder);
    } catch (error) {
      console.error("Error creating draft order:", error);
      setAlertDialog({
        open: true,
        title: "Error al crear orden",
        description: "No se pudo crear la orden. Por favor intente nuevamente.",
      });
    }
  };

  // Add variant to order
  const handleAddToOrder = async (
    product: Product,
    variantProduct: VariantProductDto
  ) => {
    if (!variantProduct) {
      setAlertDialog({
        open: true,
        title: "Variante requerida",
        description:
          "Por favor seleccione una variante antes de agregar el producto.",
      });
      return;
    }

    if (!selectedCustomer) {
      setAlertDialog({
        open: true,
        title: "Cliente requerido",
        description:
          "Por favor seleccione un cliente primero antes de agregar productos.",
      });
      return;
    }

    // Determinar precio según el tipo de venta seleccionado
    let price: number;
    if (saleType === SaleType.Wholesale) {
      // Usar precio al por mayor si está disponible, sino usar precio detal
      price = variantProduct.wholesalePrice && variantProduct.wholesalePrice > 0
        ? variantProduct.wholesalePrice
        : variantProduct.price && variantProduct.price > 0
        ? variantProduct.price
        : product.basePrice;
    } else {
      // Usar precio detal
      price = variantProduct.price && variantProduct.price > 0
        ? variantProduct.price
        : product.basePrice;
    }

    try {
      if (currentDraftOrder) {
        // Add to existing draft order
        const updatedOrder = await addItemToOrder.mutateAsync({
          orderId: currentDraftOrder.id,
          item: {
            productVariantId: variantProduct.id ?? "",
            quantity: 1,
            saleType: saleType,
            customUnitPrice: price,
          },
        });
        setCurrentDraftOrder(updatedOrder);
      } else {
        // Create new draft order with first item
        await handleCreateDraftOrder(selectedCustomer.id, {
          productVariantId: variantProduct.id ?? "",
          quantity: 1,
          price,
          saleType: saleType,
        });
      }

      setSelectedProduct(null);
      setShowVariantDialog(false);
    } catch (error) {
      console.error("Error adding item to order:", error);
      setAlertDialog({
        open: true,
        title: "Error al agregar producto",
        description:
          "No se pudo agregar el producto a la orden. Por favor intente nuevamente.",
      });
    }
  };

  const handleUpdateQuantity = (itemId: string, newQuantity: number) => {
    if (!currentDraftOrder) return;

    const itemToUpdate = currentDraftOrder.items.find(
      (item) => item.id === itemId
    );
    if (!itemToUpdate) return;

    if (newQuantity <= 0) {
      handleRemoveItem(itemId);
    } else {
      handleUpdateItem(itemId, newQuantity, itemToUpdate.unitPrice);
    }
  };
  // Update item in order
  const handleUpdateItem = async (
    itemId: string,
    quantity: number,
    price: number
  ) => {
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
      console.error("Error updating item:", error);
      setAlertDialog({
        open: true,
        title: "Error al actualizar item",
        description:
          "No se pudo actualizar el item. Por favor intente nuevamente.",
      });
    }
  };

  // Remove item from order
  const handleRemoveItem = (itemId: string) => {
    if (!currentDraftOrder) return;

    setConfirmDialog({
      open: true,
      title: "Eliminar item",
      description: "¿Está seguro de que desea eliminar este item de la orden?",
      variant: "destructive",
      confirmText: "Eliminar",
      onConfirm: async () => {
        try {
          const updatedOrder = await removeItemFromOrder.mutateAsync({
            orderId: currentDraftOrder.id,
            itemId,
          });
          setCurrentDraftOrder(updatedOrder);
        } catch (error: any) {
          console.error("Error removing item:", error);
          if (error.response?.data?.message?.includes("último item")) {
            setConfirmDialog({
              open: true,
              title: "Último item",
              description:
                "Esta es la última item de la orden. ¿Desea cancelar toda la orden?",
              variant: "destructive",
              confirmText: "Cancelar orden",
              onConfirm: () => handleCancelOrder(),
            });
          } else {
            setAlertDialog({
              open: true,
              title: "Error al eliminar item",
              description:
                "No se pudo eliminar el item. Por favor intente nuevamente.",
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
        title: "Fondos insuficientes",
        description:
          "El cliente no tiene fondos suficientes en su wallet. Por favor agregue fondos primero.",
      });
      return;
    }

    setConfirmDialog({
      open: true,
      title: "Finalizar orden",
      description: `¿Confirmar la orden por $${(total ?? 0).toFixed(
        2
      )}? Se descontará del wallet del cliente y se actualizará el inventario.`,
      confirmText: "Finalizar orden",
      onConfirm: async () => {
        try {
          await finalizeOrder.mutateAsync(currentDraftOrder.id);

          // Reset state
          setCurrentDraftOrder(null);
          setSelectedCustomer(null);
        } catch (error) {
          console.error("Error finalizing order:", error);
          setAlertDialog({
            open: true,
            title: "Error al finalizar orden",
            description:
              "No se pudo finalizar la orden. Por favor intente nuevamente.",
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
      title: "Cancelar orden",
      description:
        "¿Está seguro de que desea cancelar esta orden? Esta acción no se puede deshacer.",
      variant: "destructive",
      confirmText: "Cancelar orden",
      onConfirm: async () => {
        try {
          await cancelOrder.mutateAsync(currentDraftOrder.id);
          setCurrentDraftOrder(null);
          setSelectedCustomer(null);
        } catch (error) {
          console.error("Error canceling order:", error);
          setAlertDialog({
            open: true,
            title: "Error al cancelar orden",
            description:
              "No se pudo cancelar la orden. Por favor intente nuevamente.",
          });
        }
      },
    });
  };

  // Switch to existing draft order
  const handleSelectDraftOrder = (order: SalesOrder) => {
    const customer = customers?.find((c) => c.id === order.customerId);
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
    // Logic to select a customer for the new order
  };

  const openVariantDialog = (product: Product) => {
    setSelectedProduct(product);
    setShowVariantDialog(true);
  };

  // Handle product creation
  const handleCreateProduct = async (
    data: CreateProductDto | UpdateProductDto,
    isEditing: boolean
  ) => {
    if (isEditing) return; // Solo crear, no editar

    try {
      await createProduct.mutateAsync(data as CreateProductDto);
      setShowProductModal(false);
    } catch (error) {
      console.error("Error creating product:", error);
      setAlertDialog({
        open: true,
        title: "Error al crear producto",
        description:
          "No se pudo crear el producto. Por favor intente nuevamente.",
      });
    }
  };

  return (
    <div className="bg-gray-50/50">
      {/* Draft Orders Bar */}
      <div className="bg-white border-b border-gray-200 px-4 py-2">
        <div className="flex items-center gap-2">
          {draftOrders?.map((order) => (
            <button
              key={order.id}
              onClick={() => handleSelectDraftOrder(order)}
              className={`flex items-center gap-2 p-2 rounded-md transition-colors ${
                currentDraftOrder?.id === order.id
                  ? "bg-indigo-100 text-indigo-700"
                  : "hover:bg-gray-100"
              }`}
            >
              <div className="bg-gray-200 rounded-full w-8 h-8 flex items-center justify-center text-sm font-semibold text-gray-600">
                {order.customerName?.substring(0, 2).toUpperCase()}
              </div>
              <div>
                <p className="text-sm font-semibold">{order.customerName}</p>
                <p className="text-xs text-gray-500">
                  Ord #{order.id.substring(0, 4)}... • $
                  {order.totalAmount.toFixed(2)}
                </p>
              </div>
              {currentDraftOrder?.id === order.id && (
                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    handleCancelOrder(); // This should be a close/remove tab action instead
                  }}
                  className="ml-2 text-gray-500 hover:text-gray-800"
                >
                  <X size={16} />
                </button>
              )}
            </button>
          ))}
          <button
            onClick={handleNewOrder}
            className="flex items-center justify-center w-10 h-10 bg-gray-100 rounded-md hover:bg-gray-200"
          >
            <Plus size={20} className="text-gray-600" />
          </button>
        </div>
      </div>

      <div className="flex h-[calc(100vh-120px)] p-4 gap-4">
        {/* Main Content (Left) */}
        <div className="w-2/3 flex flex-col gap-4">
          <div className="bg-white p-4 rounded-lg border border-gray-200">
            <div className="flex gap-4">
              <div className="relative flex-grow">
                <Search
                  className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"
                  size={20}
                />
                <Input
                  placeholder="Buscar productos..."
                  className="pl-10"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </div>
              <Button variant="outline" size="icon">
                <QrCode size={20} />
              </Button>
              <div className="flex items-center gap-2">
                <Button
                  variant={activeCategory.id === "all" ? "default" : "outline"}
                  onClick={() => setActiveCategory({ id: "all", name: "Todo" })}
                >
                  Todo
                </Button>
                {categories?.map((category) => (
                  <Button
                    key={category.id}
                    variant={
                      activeCategory.id === category.id ? "default" : "outline"
                    }
                    onClick={() => setActiveCategory(category)}
                  >
                    {category.name}
                  </Button>
                ))}
              </div>
            </div>
          </div>
          <div className="flex-1 overflow-y-auto grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {filteredVariantsProducts?.map((variantProduct) => (
              <Card
                key={variantProduct.id}
                className="overflow-hidden flex flex-col h-80"
              >
                <div className="relative aspect-square w-full bg-gray-100 flex items-center justify-center">
                  {variantProduct.imageUrl ? (
                    <img
                      src={variantProduct.imageUrl}
                      alt={variantProduct.productName}
                      className="w-full h-full object-cover"
                    />
                  ) : (
                    <span className="text-gray-400 text-lg p-2 text-center break-words">
                      {variantProduct.productName}
                    </span>
                  )}
                  <Badge className="absolute top-2 right-2">
                    {variantProduct.stockQuantity}
                  </Badge>
                  {variantProduct.stockQuantity < 5 && (
                    <Badge
                      className={`absolute top-2 left-2 ${
                        variantProduct.stockQuantity === 0
                          ? "bg-orange-100 text-orange-800"
                          : "bg-yellow-100 text-yellow-800"
                      }`}
                    >
                      {variantProduct.stockQuantity === 0 ? "Agotado" : "Bajo"}
                    </Badge>
                  )}
                </div>
                <CardContent className="p-3 flex-1 flex flex-col justify-between">
                  <div>
                    <h3 className="font-semibold text-sm truncate">
                      {variantProduct.productName}
                    </h3>
                    <p className="text-xs text-gray-500">
                      {variantProduct.sku}
                    </p>
                  </div>
                  <div className="flex justify-between items-center mt-2">
                    <div>
                      <p className="font-bold text-indigo-600">
                        ${saleType === SaleType.Wholesale && variantProduct.wholesalePrice
                          ? variantProduct.wholesalePrice.toFixed(2)
                          : variantProduct.price?.toFixed(2)}
                      </p>
                      <p className="text-xs text-gray-500">
                        {saleType === SaleType.Wholesale ? "Mayor" : "Detal"}
                      </p>
                    </div>
                    {/* <Button
                      size="icon"
                      className="rounded-full w-8 h-8"
                      onClick={() => openVariantDialog(variantProduct.product)}
                      disabled={
                        variantProduct.stockQuantity === 0 || !selectedCustomer
                      }
                    >
                      <Plus size={16} />
                    </Button> */}
                    <Button
                      size="icon"
                      className="rounded-full w-8 h-8"
                      onClick={() =>
                        handleAddToOrder(variantProduct.product, variantProduct)
                      }
                      disabled={
                        variantProduct.stockQuantity === 0 || !selectedCustomer
                      }
                    >
                      <Plus size={16} />
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>

        {/* Right Sidebar (Cart) */}
        <div className="w-1/3 bg-white rounded-lg border border-gray-200 flex flex-col">
          {selectedCustomer && currentDraftOrder ? (
            <>
              <div className="p-4 border-b border-gray-200">
                <div className="flex justify-between items-center">
                  <h3 className="font-semibold">CLIENTE</h3>
                  <div className="flex items-center gap-2">
                    <Button variant="ghost" size="icon">
                      <Trash2 size={16} className="text-gray-500" />
                    </Button>
                    <Button variant="ghost" size="icon">
                      <MoreVertical size={16} className="text-gray-500" />
                    </Button>
                  </div>
                </div>
                <div className="bg-indigo-600 text-white p-4 rounded-lg mt-2">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 bg-indigo-400 rounded-full flex items-center justify-center font-bold">
                      {selectedCustomer?.firstName?.charAt(0)}
                      {selectedCustomer?.lastName?.charAt(0)}
                    </div>
                    <div>
                      <p className="font-bold">
                        {selectedCustomer.firstName} {selectedCustomer.lastName}
                      </p>
                      <p className="text-sm opacity-80">Cliente Frecuente</p>
                    </div>
                    <Button variant="ghost" size="icon" className="ml-auto">
                      <Edit2 size={16} />
                    </Button>
                  </div>
                  <div className="mt-4 flex justify-between items-end">
                    <div>
                      <p className="text-sm opacity-80">WALLET</p>
                      <p className="font-bold text-2xl">
                        ${selectedCustomer.wallet?.balance.toFixed(2)}
                      </p>
                    </div>
                    <Button variant="ghost" size="icon">
                      <ShoppingCart size={16} />
                    </Button>
                  </div>
                </div>
                <div className="flex justify-between items-center mt-2 text-sm">
                  <Badge variant="outline">Borrador</Badge>
                  <p className="text-gray-500">
                    Orden #{currentDraftOrder.id.substring(0, 8)}
                  </p>
                </div>
              </div>
              <div className="flex-1 overflow-y-auto p-4 space-y-3">
                {currentDraftOrder.items.map((item) => (
                  <div key={item.id} className="flex items-center gap-3">
                    <div className="w-12 h-12 bg-gray-100 rounded-md">
                      {/* Placeholder for image */}
                    </div>
                    <div className="flex-grow">
                      <p className="font-semibold text-sm">
                        {item.productName}
                      </p>
                      <p className="text-xs text-gray-500">
                        {item.itemDescription || "Default"}
                      </p>
                    </div>
                    <div className="flex items-center gap-2">
                      <Button
                        size="icon"
                        variant="outline"
                        className="w-7 h-7"
                        onClick={() =>
                          handleUpdateQuantity(item.id, item.quantity - 1)
                        }
                      >
                        <Minus size={14} />
                      </Button>
                      <span className="font-bold w-4 text-center">
                        {item.quantity}
                      </span>
                      <Button
                        size="icon"
                        variant="outline"
                        className="w-7 h-7"
                        onClick={() =>
                          handleUpdateQuantity(item.id, item.quantity + 1)
                        }
                      >
                        <Plus size={14} />
                      </Button>
                    </div>
                    <p className="font-semibold text-sm w-16 text-right">
                      ${item.subtotal.toFixed(2)}
                    </p>
                  </div>
                ))}
              </div>

              <div className="p-4 border-t border-gray-200 space-y-3">
                <div className="flex justify-between text-sm">
                  <p className="text-gray-500">
                    Subtotal ({currentDraftOrder.items.length} items)
                  </p>
                  <p className="font-semibold">
                    ${currentDraftOrder.totalAmount.toFixed(2)}
                  </p>
                </div>
                <div className="flex justify-between items-center">
                  <p className="text-gray-500">Total a Pagar</p>
                  <p className="font-bold text-2xl">
                    ${currentDraftOrder.totalAmount.toFixed(2)}
                  </p>
                </div>
                <div className="grid grid-cols-2 gap-3">
                  <Button variant="outline" onClick={handleCancelOrder}>
                    <Save size={16} className="mr-2" />
                    Guardar
                  </Button>
                  <Button onClick={handleFinalizeOrder}>Cobrar &rarr;</Button>
                </div>
              </div>
            </>
          ) : (
            <div className="flex-1 flex flex-col items-center justify-center text-center p-4">
              <Users size={48} className="text-gray-300" />
              <h3 className="mt-4 font-semibold">Seleccionar cliente</h3>
              <p className="mt-1 text-sm text-gray-500">
                Elija un cliente existente o cree uno nuevo para comenzar a
                vender.
              </p>
              <div className="mt-4 w-full space-y-4">
                <CustomerCombobox
                  customers={customers}
                  selectedCustomer={selectedCustomer}
                  onSelectCustomer={(customer) => {
                    const existingOrder = draftOrders?.find(
                      (o) => o.customerId === customer?.id
                    );
                    if (existingOrder && customer) {
                      setConfirmDialog({
                        open: true,
                        title: "Orden existente",
                        description: `${customer.firstName} ${customer.lastName} ya tiene una orden en progreso. ¿Desea continuar con esa orden?`,
                        confirmText: "Continuar con orden",
                        onConfirm: () => handleSelectDraftOrder(existingOrder),
                      });
                    } else {
                      setSelectedCustomer(customer);
                      setCurrentDraftOrder(null);
                    }
                  }}
                  disabled={!!currentDraftOrder}
                />

                {/* Selector de tipo de venta */}
                <div className="space-y-2">
                  <label className="block text-sm font-medium text-gray-700">
                    Tipo de Venta
                  </label>
                  <div className="grid grid-cols-2 gap-2">
                    <button
                      type="button"
                      onClick={() => setSaleType(SaleType.Retail)}
                      className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${
                        saleType === SaleType.Retail
                          ? "bg-indigo-600 text-white"
                          : "bg-gray-100 text-gray-700 hover:bg-gray-200"
                      }`}
                    >
                      Al Detal
                    </button>
                    <button
                      type="button"
                      onClick={() => setSaleType(SaleType.Wholesale)}
                      className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${
                        saleType === SaleType.Wholesale
                          ? "bg-indigo-600 text-white"
                          : "bg-gray-100 text-gray-700 hover:bg-gray-200"
                      }`}
                    >
                      Al Por Mayor
                    </button>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
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
                <p className="text-sm text-gray-600">
                  {selectedProduct.description}
                </p>
                <p className="text-xl font-bold text-gray-900 mt-2">
                  ${(selectedProduct.basePrice ?? 0).toFixed(2)}
                </p>
              </div>

              {selectedProduct.variants &&
              selectedProduct.variants.length > 0 ? (
                <div className="space-y-2 max-h-64 overflow-y-auto">
                  <h4 className="text-sm font-medium text-gray-700">
                    Variantes disponibles:
                  </h4>
                  {selectedProduct.variants.map((variant) => (
                    <button
                      key={variant.id}
                      onClick={() => handleAddToOrder(selectedProduct, variant)}
                      disabled={
                        variant.stockQuantity === 0 || !selectedCustomer
                      }
                      className="w-full p-3 text-left border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                      <div className="flex justify-between items-center">
                        <div>
                          <p className="text-sm font-medium text-gray-900">
                            {variant.sku}
                          </p>
                          <p className="text-xs text-gray-500">
                            {variant.size && `Talla: ${variant.size}`}
                            {variant.size && variant.color && " • "}
                            {variant.color && `Color: ${variant.color}`}
                          </p>
                          <p className="text-xs text-gray-500 mt-1">
                            Stock: {variant.stockQuantity}
                          </p>
                        </div>
                        <div className="text-right">
                          <p className="text-sm font-bold text-gray-900">
                            $
                            {(
                              (variant.price || selectedProduct.basePrice) ??
                              0
                            ).toFixed(2)}
                          </p>
                        </div>
                      </div>
                    </button>
                  ))}
                </div>
              ) : (
                <div className="text-center py-8">
                  <p className="text-sm text-gray-500">
                    No hay variantes disponibles
                  </p>
                  <button
                    onClick={() =>
                      handleAddToOrder(selectedProduct, {
                        id: selectedProduct.variants[0].id,
                        price: selectedProduct.basePrice,
                        stockQuantity: 1, // Assume 1 if no variants
                        sku: "default",
                      } as VariantProductDto)
                    }
                  >
                    Add default
                  </button>
                </div>
              )}
            </div>
          )}

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setShowVariantDialog(false)}
            >
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
        onClose={() => {
          setShowProductModal(false);
        }}
        onSubmit={handleCreateProduct}
      />

      {/* Variant Manager Modal */}
      <VariantModal
        isOpen={showVariantManagerModal}
        product={selectedProduct} // Pass selectedProduct to VariantModal
        onClose={() => setShowVariantManagerModal(false)}
        customAlertDialog={setAlertDialog} // Pass setAlertDialog
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
