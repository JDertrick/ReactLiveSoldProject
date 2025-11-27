import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { usePurchaseReceipts } from '../../hooks/usePurchaseReceipts';
import { usePurchaseOrders } from '../../hooks/usePurchaseOrders';
import { useLocations } from '../../hooks/useLocations';
import { useGetVendors } from '../../hooks/useVendors';
import { CreatePurchaseReceiptDto, CreatePurchaseItemDto, PurchaseOrderDto } from '../../types/purchases.types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { toast } from 'sonner';
import { ArrowLeft, AlertTriangle, CheckCircle2 } from 'lucide-react';
import { Textarea } from '@/components/ui/textarea';
import { Alert, AlertDescription } from '@/components/ui/alert';

interface ReceiptLineItem extends CreatePurchaseItemDto {
  productName?: string;
  variantName?: string;
  quantityOrdered: number;
}

const PurchaseReceiptForm = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = !!id;

  const { createPurchaseReceipt, getPurchaseReceiptById } = usePurchaseReceipts();
  const { purchaseOrders, fetchPurchaseOrders, getPurchaseOrderById } = usePurchaseOrders();
  const { locations } = useLocations();
  const { data: vendors } = useGetVendors();

  const [formData, setFormData] = useState<CreatePurchaseReceiptDto>({
    purchaseOrderId: undefined,
    vendorId: '',
    receiptDate: new Date().toISOString().split('T')[0],
    warehouseLocationId: undefined,
    notes: '',
    items: [],
  });

  const [receiptLines, setReceiptLines] = useState<ReceiptLineItem[]>([]);
  const [selectedPO, setSelectedPO] = useState<PurchaseOrderDto | null>(null);
  const [showDiscrepancies, setShowDiscrepancies] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    // Fetch approved purchase orders
    fetchPurchaseOrders(undefined, 'Approved');
  }, [fetchPurchaseOrders]);

  // Load existing receipt data when editing
  useEffect(() => {
    if (isEditing && id) {
      setLoading(true);
      getPurchaseReceiptById(id)
        .then((receipt) => {
          if (receipt) {
            // Set form data
            setFormData({
              purchaseOrderId: receipt.purchaseOrderId,
              vendorId: receipt.vendorId,
              receiptDate: receipt.receiptDate.split('T')[0],
              warehouseLocationId: receipt.warehouseLocationId,
              notes: receipt.notes || '',
              items: [],
            });

            // Set receipt lines from existing items
            if (receipt.items) {
              const lines: ReceiptLineItem[] = receipt.items.map((item) => ({
                lineNumber: item.lineNumber,
                productId: item.productId,
                productVariantId: item.productVariantId,
                productName: item.productName,
                variantName: item.variantName,
                description: item.description,
                quantityOrdered: item.quantityOrdered,
                quantityReceived: item.quantityReceived,
                unitCost: item.unitCost,
                discountPercentage: item.discountPercentage,
                taxRate: item.taxRate,
                glInventoryAccountId: item.glInventoryAccountId,
              }));
              setReceiptLines(lines);
            }
          }
        })
        .catch((error) => {
          toast.error('Error al cargar la recepción');
          console.error(error);
        })
        .finally(() => {
          setLoading(false);
        });
    }
  }, [isEditing, id, getPurchaseReceiptById]);

  // Auto-populate vendor and load PO items when purchase order is selected (only in create mode)
  useEffect(() => {
    if (!isEditing && formData.purchaseOrderId && purchaseOrders.length > 0) {
      const po = purchaseOrders.find((p) => p.id === formData.purchaseOrderId);
      if (po) {
        setFormData((prev) => ({
          ...prev,
          vendorId: po.vendorId,
        }));

        // Load full PO details with items
        getPurchaseOrderById(po.id).then((fullPO) => {
          if (fullPO && fullPO.items) {
            setSelectedPO(fullPO);
            // Initialize receipt lines with PO items (recepción ciega - no muestra cantidades)
            const lines: ReceiptLineItem[] = fullPO.items.map((item, index) => ({
              lineNumber: index + 1,
              productId: item.productId,
              productVariantId: item.productVariantId,
              productName: item.productName,
              variantName: item.variantName,
              description: item.description,
              quantityOrdered: item.quantity,
              quantityReceived: 0, // Usuario debe ingresar
              unitCost: item.unitCost,
              discountPercentage: item.discountPercentage,
              taxRate: item.taxRate,
            }));
            setReceiptLines(lines);
          }
        });
      }
    } else if (!isEditing && !formData.purchaseOrderId) {
      // Clear lines if no PO selected (only in create mode)
      setReceiptLines([]);
      setSelectedPO(null);
    }
  }, [isEditing, formData.purchaseOrderId, purchaseOrders, getPurchaseOrderById]);

  const handleQuantityChange = (index: number, quantity: number) => {
    const updatedLines = [...receiptLines];
    updatedLines[index].quantityReceived = quantity;
    setReceiptLines(updatedLines);
  };

  const handleUnitCostChange = (index: number, cost: number) => {
    const updatedLines = [...receiptLines];
    updatedLines[index].unitCost = cost;
    setReceiptLines(updatedLines);
  };

  const getDiscrepancies = () => {
    return receiptLines.filter(
      (line) => line.quantityReceived > 0 && line.quantityReceived !== line.quantityOrdered
    );
  };

  const hasDiscrepancies = () => {
    return getDiscrepancies().length > 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.vendorId) {
      toast.error('Seleccione un proveedor');
      return;
    }

    // Filter only items with received quantity > 0
    const itemsToReceive = receiptLines.filter((line) => line.quantityReceived > 0);

    if (itemsToReceive.length === 0) {
      toast.error('Ingrese al menos una cantidad recibida');
      return;
    }

    // Check for discrepancies
    if (hasDiscrepancies() && !showDiscrepancies) {
      setShowDiscrepancies(true);
      toast.warning('Hay discrepancias en las cantidades. Revise y confirme.');
      return;
    }

    // Prepare items for submission
    const items: CreatePurchaseItemDto[] = itemsToReceive.map((line) => ({
      lineNumber: line.lineNumber,
      productId: line.productId,
      productVariantId: line.productVariantId,
      description: line.description,
      quantityReceived: line.quantityReceived,
      unitCost: line.unitCost,
      discountPercentage: line.discountPercentage,
      taxRate: line.taxRate,
      glInventoryAccountId: line.glInventoryAccountId,
    }));

    try {
      await createPurchaseReceipt({
        ...formData,
        items,
      });
      toast.success('Recepción de compra creada exitosamente');
      navigate('/app/purchase-receipts');
    } catch (error: any) {
      toast.error(error.message || 'Error al crear la recepción de compra');
    }
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
        <Button variant="ghost" size="icon" onClick={() => navigate('/app/purchase-receipts')}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            {isEditing ? 'Editar Recepción' : 'Nueva Recepción de Compra'}
          </h1>
          <p className="text-muted-foreground mt-1">
            Recepción ciega parcial - Ingrese las cantidades recibidas
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
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="vendorId">Proveedor *</Label>
                  <Select
                    value={formData.vendorId}
                    onValueChange={(value) => setFormData({ ...formData, vendorId: value })}
                    disabled={!!formData.purchaseOrderId}
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
                  <Label htmlFor="purchaseOrderId">Orden de Compra *</Label>
                  <Select
                    value={formData.purchaseOrderId || 'none'}
                    onValueChange={(value) =>
                      setFormData({ ...formData, purchaseOrderId: value === 'none' ? undefined : value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione una orden de compra" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Sin orden de compra</SelectItem>
                      {purchaseOrders
                        ?.filter((po) => !formData.vendorId || po.vendorId === formData.vendorId)
                        .map((po) => (
                          <SelectItem key={po.id} value={po.id}>
                            {po.poNumber} - {po.vendorName}
                          </SelectItem>
                        ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="receiptDate">Fecha de Recepción *</Label>
                  <Input
                    id="receiptDate"
                    type="date"
                    value={formData.receiptDate}
                    onChange={(e) => setFormData({ ...formData, receiptDate: e.target.value })}
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="warehouseLocationId">Almacén de Destino</Label>
                  <Select
                    value={formData.warehouseLocationId || 'none'}
                    onValueChange={(value) =>
                      setFormData({ ...formData, warehouseLocationId: value === 'none' ? undefined : value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione un almacén" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Sin almacén específico</SelectItem>
                      {locations?.map((location) => (
                        <SelectItem key={location.id} value={location.id}>
                          {location.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2 md:col-span-2">
                  <Label htmlFor="notes">Notas</Label>
                  <Textarea
                    id="notes"
                    value={formData.notes || ''}
                    onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                    placeholder="Notas adicionales..."
                    rows={2}
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Productos a Recibir */}
          {receiptLines.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Productos a Recibir (Recepción Ciega)</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                {showDiscrepancies && hasDiscrepancies() && (
                  <Alert variant="destructive">
                    <AlertTriangle className="h-4 w-4" />
                    <AlertDescription>
                      <strong>Discrepancias detectadas:</strong> Las cantidades recibidas no coinciden con las
                      solicitadas. Revise la tabla y confirme si desea continuar.
                    </AlertDescription>
                  </Alert>
                )}

                <div className="overflow-x-auto">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Producto</TableHead>
                        {/* <TableHead className="text-center">Cant. Solicitada</TableHead> */}
                        <TableHead className="text-center">Cant. Recibida</TableHead>
                        <TableHead className="text-right">Costo Unit.</TableHead>
                        <TableHead className="text-right">Total</TableHead>
                        <TableHead className="text-center">Estado</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {receiptLines.map((line, index) => {
                        const hasDiscrepancy =
                          line.quantityReceived > 0 && line.quantityReceived !== line.quantityOrdered;
                        const isComplete = line.quantityReceived === line.quantityOrdered;
                        const total = line.quantityReceived * line.unitCost;

                        return (
                          <TableRow key={index} className={hasDiscrepancy ? 'bg-amber-50' : ''}>
                            <TableCell>
                              <div>
                                <p className="font-medium">{line.productName}</p>
                                {line.variantName && (
                                  <p className="text-sm text-muted-foreground">{line.variantName}</p>
                                )}
                                {line.description && (
                                  <p className="text-xs text-muted-foreground">{line.description}2</p>
                                )}
                              </div>
                            </TableCell>
                            {/* <TableCell className="text-center">
                              <span className="font-medium">{line.quantityOrdered}</span>
                            </TableCell> */}
                            <TableCell>
                              <div className='flex justify-center items-center'>
                                <Input
                                  type="number"
                                  min="0"
                                  value={line.quantityReceived}
                                  onChange={(e) => handleQuantityChange(index, parseInt(e.target.value) || 0)}
                                  className={`w-24 text-center ${
                                    hasDiscrepancy ? 'border-amber-500 bg-amber-50' : ''
                                }`}
                              />
                              </div>
                            </TableCell>
                            <TableCell>
                              <div className='flex justify-end'>
                                <Input
                                  type="number"
                                  step="0.01"
                                  min="0"
                                  value={line.unitCost}
                                  onChange={(e) => handleUnitCostChange(index, parseFloat(e.target.value) || 0)}
                                  className="w-28 text-right"
                                />
                              </div>
                            </TableCell>
                            <TableCell className="text-right font-medium">
                              ${total.toFixed(2)}
                            </TableCell>
                            <TableCell className="text-center">
                              {line.quantityReceived === 0 ? (
                                <span className="text-muted-foreground text-sm">Pendiente</span>
                              ) : isComplete ? (
                                <CheckCircle2 className="h-5 w-5 text-green-600 inline" />
                              ) : (
                                <div className="flex items-center gap-1 justify-center">
                                  <AlertTriangle className="h-5 w-5 text-amber-600" />
                                  <span className="text-xs text-amber-600">
                                    {line.quantityReceived > line.quantityOrdered
                                      ? `+${line.quantityReceived - line.quantityOrdered}`
                                      : `${line.quantityReceived - line.quantityOrdered}`}
                                  </span>
                                </div>
                              )}
                            </TableCell>
                          </TableRow>
                        );
                      })}
                    </TableBody>
                  </Table>
                </div>

                {/* Summary */}
                <div className="flex justify-end pt-4 border-t">
                  <div className="space-y-2">
                    <div className="flex justify-between gap-8">
                      <span className="text-muted-foreground">Items a recibir:</span>
                      <span className="font-medium">
                        {receiptLines.filter((l) => l.quantityReceived > 0).length} de {receiptLines.length}
                      </span>
                    </div>
                    <div className="flex justify-between gap-8">
                      <span className="text-muted-foreground">Total:</span>
                      <span className="font-bold text-lg">
                        $
                        {receiptLines
                          .reduce((sum, line) => sum + line.quantityReceived * line.unitCost, 0)
                          .toFixed(2)}
                      </span>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Message when no PO selected */}
          {receiptLines.length === 0 && formData.purchaseOrderId === undefined && (
            <Card>
              <CardContent className="py-12 text-center text-muted-foreground">
                <p className="text-lg">Seleccione una orden de compra para ver los productos</p>
              </CardContent>
            </Card>
          )}

          {/* Botones de acción */}
          <div className="flex justify-end gap-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => navigate('/app/purchase-receipts')}
            >
              Cancelar
            </Button>
            <Button type="submit" disabled={receiptLines.filter((l) => l.quantityReceived > 0).length === 0}>
              {isEditing ? 'Actualizar Recepción' : 'Crear Recepción'}
            </Button>
          </div>
        </div>
      </form>
    </div>
  );
};

export default PurchaseReceiptForm;
