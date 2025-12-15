import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useVendorInvoices } from '../../hooks/useVendorInvoices';
import { usePurchaseReceipts } from '../../hooks/usePurchaseReceipts';
import { useGetVendors } from '../../hooks/useVendors';
import { CreateVendorInvoiceDto } from '../../types/purchases.types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { toast } from 'sonner';
import { ArrowLeft } from 'lucide-react';
import { Textarea } from '@/components/ui/textarea';
import { AutoNumberInput } from '@/components/common/AutoNumberInput';

const VendorInvoiceForm = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = !!id;

  const { createVendorInvoice, getVendorInvoiceById } = useVendorInvoices();
  const { purchaseReceipts, fetchPurchaseReceipts } = usePurchaseReceipts();
  const { data: vendors } = useGetVendors();
  const [loading, setLoading] = useState(false);
  const [invoiceNumber, setInvoiceNumber] = useState<string>('');

  const [formData, setFormData] = useState<CreateVendorInvoiceDto>({
    vendorId: '',
    purchaseReceiptId: undefined,
    vendorInvoiceReference: '',
    invoiceDate: new Date().toISOString().split('T')[0],
    dueDate: '',
    subtotal: 0,
    taxAmount: 0,
    totalAmount: 0,
    notes: '',
  });

  useEffect(() => {
    // Fetch posted purchase receipts
    fetchPurchaseReceipts(undefined, 'Posted');
  }, [fetchPurchaseReceipts]);

  // Load invoice data when editing
  useEffect(() => {
    if (isEditing && id) {
      setLoading(true);
      getVendorInvoiceById(id)
        .then((invoice) => {
          if (invoice) {
            setInvoiceNumber(invoice.invoiceNumber || '');
            setFormData({
              vendorId: invoice.vendorId || '',
              purchaseReceiptId: invoice.purchaseReceiptId || undefined,
              vendorInvoiceReference: invoice.vendorInvoiceReference || '',
              invoiceDate: invoice.invoiceDate.split('T')[0],
              dueDate: invoice.dueDate.split('T')[0],
              subtotal: invoice.subtotal,
              taxAmount: invoice.taxAmount,
              totalAmount: invoice.totalAmount,
              notes: invoice.notes || '',
            });
          }
        })
        .finally(() => setLoading(false));
    }
  }, [isEditing, id, getVendorInvoiceById]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.vendorId) {
      toast.error('Seleccione un proveedor');
      return;
    }

    if (!formData.vendorInvoiceReference) {
      toast.error('Ingrese la referencia de la factura del proveedor');
      return;
    }

    if (formData.totalAmount <= 0) {
      toast.error('El monto total debe ser mayor a 0');
      return;
    }

    try {
      await createVendorInvoice(formData);
      toast.success('Factura de proveedor creada exitosamente');
      navigate('/app/vendor-invoices');
    } catch (error: any) {
      toast.error(error.message || 'Error al crear la factura');
    }
  };

  // Calcular total automáticamente
  useEffect(() => {
    const total = formData.subtotal + formData.taxAmount;
    if (total !== formData.totalAmount) {
      setFormData((prev) => ({ ...prev, totalAmount: total }));
    }
  }, [formData.subtotal, formData.taxAmount]);

  if (loading) {
    return (
      <div className="flex justify-center items-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/app/vendor-invoices')}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            {isEditing ? 'Editar Factura' : 'Nueva Factura de Proveedor'}
          </h1>
          <p className="text-muted-foreground mt-1">
            Registre la factura recibida del proveedor
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
              {/* Número de Factura */}
              <AutoNumberInput
                label="No. Factura"
                value={invoiceNumber}
                onChange={setInvoiceNumber}
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
                  <Label htmlFor="purchaseReceiptId">Recepción de Compra (Opcional)</Label>
                  <Select
                    value={formData.purchaseReceiptId || 'none'}
                    onValueChange={(value) =>
                      setFormData({ ...formData, purchaseReceiptId: value === 'none' ? undefined : value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione una recepción" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Sin recepción</SelectItem>
                      {purchaseReceipts?.map((receipt) => (
                        <SelectItem key={receipt.id} value={receipt.id}>
                          {receipt.receiptNumber} - {receipt.vendorName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="vendorInvoiceReference">Referencia Factura Proveedor *</Label>
                  <Input
                    id="vendorInvoiceReference"
                    type="text"
                    value={formData.vendorInvoiceReference}
                    onChange={(e) =>
                      setFormData({ ...formData, vendorInvoiceReference: e.target.value })
                    }
                    placeholder="Número de factura del proveedor"
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="invoiceDate">Fecha de Factura *</Label>
                  <Input
                    id="invoiceDate"
                    type="date"
                    value={formData.invoiceDate}
                    onChange={(e) => setFormData({ ...formData, invoiceDate: e.target.value })}
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="dueDate">Fecha de Vencimiento *</Label>
                  <Input
                    id="dueDate"
                    type="date"
                    value={formData.dueDate}
                    onChange={(e) => setFormData({ ...formData, dueDate: e.target.value })}
                    required
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Montos */}
          <Card>
            <CardHeader>
              <CardTitle>Montos</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="subtotal">Subtotal *</Label>
                  <Input
                    id="subtotal"
                    type="number"
                    step="0.01"
                    min="0"
                    value={formData.subtotal}
                    onChange={(e) =>
                      setFormData({ ...formData, subtotal: parseFloat(e.target.value) || 0 })
                    }
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="taxAmount">Impuestos *</Label>
                  <Input
                    id="taxAmount"
                    type="number"
                    step="0.01"
                    min="0"
                    value={formData.taxAmount}
                    onChange={(e) =>
                      setFormData({ ...formData, taxAmount: parseFloat(e.target.value) || 0 })
                    }
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="totalAmount">Total</Label>
                  <Input
                    id="totalAmount"
                    type="number"
                    step="0.01"
                    value={formData.totalAmount}
                    disabled
                    className="bg-muted"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="notes">Notas</Label>
                <Textarea
                  id="notes"
                  value={formData.notes || ''}
                  onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                  placeholder="Notas adicionales..."
                  rows={3}
                />
              </div>
            </CardContent>
          </Card>

          {/* Botones de acción */}
          <div className="flex justify-end gap-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => navigate('/app/vendor-invoices')}
            >
              Cancelar
            </Button>
            <Button type="submit">
              {isEditing ? 'Actualizar Factura' : 'Crear Factura'}
            </Button>
          </div>
        </div>
      </form>
    </div>
  );
};

export default VendorInvoiceForm;
