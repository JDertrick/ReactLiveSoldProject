import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { usePurchaseOrders } from '../../hooks/usePurchaseOrders';
import { useGetProducts } from '../../hooks/useProducts';
import { useGetVendors } from '../../hooks/useVendors';
import { CreatePurchaseOrderDto, CreatePurchaseOrderItemDto } from '../../types/purchases.types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { toast } from 'sonner';
import { Trash2, Plus, ArrowLeft } from 'lucide-react';
import { Textarea } from '@/components/ui/textarea';
import { AutoNumberInput } from '@/components/common/AutoNumberInput';

const PurchaseOrderForm = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = !!id;

  const { createPurchaseOrder, getPurchaseOrderById, updatePurchaseOrderStatus } = usePurchaseOrders();
  const { data: vendors } = useGetVendors();
  const { data: productsData } = useGetProducts(1, 1000, 'all', '');
  const products = productsData?.items || [];

  const [loading, setLoading] = useState(false);
  const [poNumber, setPoNumber] = useState<string>('');
  const [formData, setFormData] = useState<CreatePurchaseOrderDto>({
    vendorId: '',
    orderDate: new Date().toISOString().split('T')[0],
    expectedDeliveryDate: '',
    notes: '',
    items: [],
  });

  const [currentItem, setCurrentItem] = useState<CreatePurchaseOrderItemDto>({
    productId: '',
    productVariantId: undefined,
    quantity: 1,
    unitCost: 0,
    discountPercentage: 0,
    taxRate: 0,
  });

  // Cargar datos de la orden si estamos editando
  useEffect(() => {
    if (isEditing && id) {
      setLoading(true);
      getPurchaseOrderById(id)
        .then((order) => {
          if (order) {
            setPoNumber(order.poNumber || '');
            setFormData({
              vendorId: order.vendorId,
              orderDate: order.orderDate.split('T')[0],
              expectedDeliveryDate: order.expectedDeliveryDate ? order.expectedDeliveryDate.split('T')[0] : '',
              notes: order.notes || '',
              items: order.items?.map((item) => ({
                productId: item.productId,
                productVariantId: item.productVariantId,
                quantity: item.quantity,
                unitCost: item.unitCost,
                discountPercentage: item.discountPercentage,
                taxRate: item.taxRate,
              })) || [],
            });
          }
        })
        .catch((error) => {
          toast.error('Error al cargar la orden de compra');
          console.error(error);
        })
        .finally(() => {
          setLoading(false);
        });
    }
  }, [isEditing, id, getPurchaseOrderById]);

  const handleAddItem = () => {
    if (!currentItem.productId || currentItem.quantity <= 0 || currentItem.unitCost <= 0) {
      toast.error('Complete todos los campos del producto');
      return;
    }

    setFormData({
      ...formData,
      items: [...formData.items, { ...currentItem }],
    });

    setCurrentItem({
      productId: '',
      productVariantId: undefined,
      quantity: 1,
      unitCost: 0,
      discountPercentage: 0,
      taxRate: 0,
    });
  };

  const handleRemoveItem = (index: number) => {
    setFormData({
      ...formData,
      items: formData.items.filter((_, i) => i !== index),
    });
  };

  const calculateItemTotal = (item: CreatePurchaseOrderItemDto) => {
    const subtotal = item.quantity * item.unitCost;
    const discount = subtotal * (item.discountPercentage / 100);
    const afterDiscount = subtotal - discount;
    const tax = afterDiscount * (item.taxRate / 100);
    return afterDiscount + tax;
  };

  const calculateTotal = () => {
    return formData.items.reduce((sum, item) => sum + calculateItemTotal(item), 0);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.vendorId) {
      toast.error('Seleccione un proveedor');
      return;
    }

    if (formData.items.length === 0) {
      toast.error('Agregue al menos un producto');
      return;
    }

    try {
      await createPurchaseOrder(formData);
      toast.success('Orden de compra creada exitosamente');
      navigate('/app/purchase-orders');
    } catch (error: any) {
      toast.error(error.message || 'Error al crear la orden de compra');
    }
  };

  const getProductName = (productId: string) => {
    const product = products.find(p => p.productId === productId);
    return product?.productName || '';
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/app/purchase-orders')}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            {isEditing ? 'Editar Orden de Compra' : 'Nueva Orden de Compra'}
          </h1>
          <p className="text-muted-foreground mt-1">
            Complete los datos de la orden de compra
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="grid gap-6">
          {/* Información General */}
          <Card>
            <CardHeader>
              <CardTitle>Información General</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Número de Orden de Compra */}
              <AutoNumberInput
                label="No. Orden de Compra"
                value={poNumber}
                onChange={setPoNumber}
                allowManualEntry={false}
                isEditing={isEditing}
                placeholder="Se generará automáticamente"
              />

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="vendorId">Proveedor *</Label>
                  <Select
                    value={formData.vendorId}
                    onValueChange={(value) => setFormData({ ...formData, vendorId: value })}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione un proveedor" />
                    </SelectTrigger>
                    <SelectContent>
                      {vendors?.map((vendor) => (
                        <SelectItem key={vendor.id} value={vendor.id}>
                          {vendor.contact?.company || `${vendor.contact?.firstName} ${vendor.contact?.lastName}`}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="orderDate">Fecha de Orden *</Label>
                  <Input
                    id="orderDate"
                    type="date"
                    value={formData.orderDate}
                    onChange={(e) => setFormData({ ...formData, orderDate: e.target.value })}
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="expectedDeliveryDate">Fecha Esperada de Entrega</Label>
                  <Input
                    id="expectedDeliveryDate"
                    type="date"
                    value={formData.expectedDeliveryDate || ''}
                    onChange={(e) =>
                      setFormData({ ...formData, expectedDeliveryDate: e.target.value })
                    }
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="notes">Notas</Label>
                  <Textarea
                    id="notes"
                    value={formData.notes || ''}
                    onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                    placeholder="Notas adicionales..."
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Productos */}
          <Card>
            <CardHeader>
              <CardTitle>Productos</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Formulario para agregar productos */}
              <div className="grid grid-cols-1 md:grid-cols-6 gap-4 p-4 bg-muted/50 rounded-lg">
                <div className="md:col-span-2">
                  <Label>Producto</Label>
                  <Select
                    value={currentItem.productId}
                    onValueChange={(value) => setCurrentItem({ ...currentItem, productId: value })}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione producto" />
                    </SelectTrigger>
                    <SelectContent>
                      {products?.map((product) => (
                        <SelectItem key={product.productId} value={product.productId}>
                          {product.productName} {product.variantName ? `- ${product.variantName}` : ''}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div>
                  <Label>Cantidad</Label>
                  <Input
                    type="number"
                    min="1"
                    value={currentItem.quantity}
                    onChange={(e) =>
                      setCurrentItem({ ...currentItem, quantity: parseInt(e.target.value) || 1 })
                    }
                  />
                </div>

                <div>
                  <Label>Costo Unit.</Label>
                  <Input
                    type="number"
                    step="0.01"
                    min="0"
                    value={currentItem.unitCost}
                    onChange={(e) =>
                      setCurrentItem({ ...currentItem, unitCost: parseFloat(e.target.value) || 0 })
                    }
                  />
                </div>

                <div>
                  <Label>Desc. %</Label>
                  <Input
                    type="number"
                    step="0.01"
                    min="0"
                    max="100"
                    value={currentItem.discountPercentage}
                    onChange={(e) =>
                      setCurrentItem({
                        ...currentItem,
                        discountPercentage: parseFloat(e.target.value) || 0,
                      })
                    }
                  />
                </div>

                <div>
                  <Label>IVA %</Label>
                  <Input
                    type="number"
                    step="0.01"
                    min="0"
                    max="100"
                    value={currentItem.taxRate}
                    onChange={(e) =>
                      setCurrentItem({ ...currentItem, taxRate: parseFloat(e.target.value) || 0 })
                    }
                  />
                </div>

                <div className="flex items-end md:col-span-6">
                  <Button type="button" onClick={handleAddItem} className="w-full gap-2">
                    <Plus className="h-4 w-4" />
                    Agregar Producto
                  </Button>
                </div>
              </div>

              {/* Lista de productos agregados */}
              {formData.items.length > 0 && (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Producto</TableHead>
                      <TableHead>Cantidad</TableHead>
                      <TableHead>Costo Unit.</TableHead>
                      <TableHead>Desc. %</TableHead>
                      <TableHead>IVA %</TableHead>
                      <TableHead className="text-right">Total</TableHead>
                      <TableHead></TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {formData.items.map((item, index) => (
                      <TableRow key={index}>
                        <TableCell>{getProductName(item.productId)}</TableCell>
                        <TableCell>{item.quantity}</TableCell>
                        <TableCell>${item.unitCost.toFixed(2)}</TableCell>
                        <TableCell>{item.discountPercentage}%</TableCell>
                        <TableCell>{item.taxRate}%</TableCell>
                        <TableCell className="text-right font-medium">
                          ${calculateItemTotal(item).toFixed(2)}
                        </TableCell>
                        <TableCell>
                          <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            onClick={() => handleRemoveItem(index)}
                          >
                            <Trash2 className="h-4 w-4 text-destructive" />
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                    <TableRow>
                      <TableCell colSpan={5} className="text-right font-semibold">
                        Total:
                      </TableCell>
                      <TableCell className="text-right font-bold text-lg">
                        ${calculateTotal().toFixed(2)}
                      </TableCell>
                      <TableCell></TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              )}
            </CardContent>
          </Card>

          {/* Botones de acción */}
          <div className="flex justify-end gap-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => navigate('/app/purchase-orders')}
            >
              Cancelar
            </Button>
            <Button type="submit">
              {isEditing ? 'Actualizar Orden' : 'Crear Orden de Compra'}
            </Button>
          </div>
        </div>
      </form>
    </div>
  );
};

export default PurchaseOrderForm;
