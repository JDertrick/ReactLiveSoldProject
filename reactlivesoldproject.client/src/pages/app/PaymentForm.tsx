import { useState, useEffect } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { usePayments } from '../../hooks/usePayments';
import { useVendorInvoices } from '../../hooks/useVendorInvoices';
import { useGetVendors } from '../../hooks/useVendors';
import { CreatePaymentDto, PaymentMethod } from '../../types/purchases.types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { toast } from 'sonner';
import { ArrowLeft } from 'lucide-react';
import { Textarea } from '@/components/ui/textarea';

const PaymentForm = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const invoiceIdFromUrl = searchParams.get('invoiceId');
  const isEditing = !!id;

  const { createPayment } = usePayments();
  const { vendorInvoices, fetchVendorInvoices } = useVendorInvoices();
  const { data: vendors } = useGetVendors();

  const [selectedInvoiceId, setSelectedInvoiceId] = useState<string | undefined>(invoiceIdFromUrl || undefined);

  const [formData, setFormData] = useState<CreatePaymentDto>({
    vendorId: '',
    paymentDate: new Date().toISOString().split('T')[0],
    amount: 0,
    paymentMethod: PaymentMethod.BankTransfer,
    referenceNumber: '',
    notes: '',
    invoiceApplications: [],
  });

  useEffect(() => {
    // Fetch unpaid vendor invoices
    fetchVendorInvoices(undefined, 'Unpaid');
  }, [fetchVendorInvoices]);

  // If invoice is preselected, get vendor and amount
  useEffect(() => {
    if (selectedInvoiceId && vendorInvoices.length > 0) {
      const invoice = vendorInvoices.find((inv) => inv.id === selectedInvoiceId);
      if (invoice) {
        setFormData((prev) => ({
          ...prev,
          vendorId: invoice.vendorId || '',
          amount: invoice.amountDue,
        }));
      }
    }
  }, [selectedInvoiceId, vendorInvoices]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.vendorId) {
      toast.error('Seleccione un proveedor');
      return;
    }

    if (formData.amount <= 0) {
      toast.error('El monto debe ser mayor a 0');
      return;
    }

    // Prepare invoice applications
    const invoiceApplications = selectedInvoiceId
      ? [{ vendorInvoiceId: selectedInvoiceId, amountApplied: formData.amount }]
      : [];

    try {
      await createPayment({
        ...formData,
        invoiceApplications,
      });
      toast.success('Pago creado exitosamente');
      navigate('/app/payments');
    } catch (error: any) {
      toast.error(error.message || 'Error al crear el pago');
    }
  };

  const paymentMethods = [
    { value: PaymentMethod.Cash, label: 'Efectivo' },
    { value: PaymentMethod.Check, label: 'Cheque' },
    { value: PaymentMethod.BankTransfer, label: 'Transferencia Bancaria' },
    { value: PaymentMethod.CreditCard, label: 'Tarjeta de Crédito' },
    { value: PaymentMethod.DebitCard, label: 'Tarjeta de Débito' },
    { value: PaymentMethod.Other, label: 'Otro' },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/app/payments')}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            {isEditing ? 'Editar Pago' : 'Nuevo Pago a Proveedor'}
          </h1>
          <p className="text-muted-foreground mt-1">
            Registre el pago realizado al proveedor
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
                  <Label htmlFor="vendorInvoiceId">Factura (Opcional)</Label>
                  <Select
                    value={selectedInvoiceId || 'none'}
                    onValueChange={(value) => setSelectedInvoiceId(value === 'none' ? undefined : value)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione una factura" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Sin factura</SelectItem>
                      {vendorInvoices
                        ?.filter((inv) => !formData.vendorId || inv.vendorId === formData.vendorId)
                        .map((invoice) => (
                          <SelectItem key={invoice.id} value={invoice.id}>
                            {invoice.invoiceNumber} - {invoice.vendorName} - ${invoice.amountDue}
                          </SelectItem>
                        ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="paymentDate">Fecha de Pago *</Label>
                  <Input
                    id="paymentDate"
                    type="date"
                    value={formData.paymentDate}
                    onChange={(e) => setFormData({ ...formData, paymentDate: e.target.value })}
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="amount">Monto *</Label>
                  <Input
                    id="amount"
                    type="number"
                    step="0.01"
                    min="0"
                    value={formData.amount}
                    onChange={(e) =>
                      setFormData({ ...formData, amount: parseFloat(e.target.value) || 0 })
                    }
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="paymentMethod">Método de Pago *</Label>
                  <Select
                    value={formData.paymentMethod}
                    onValueChange={(value: PaymentMethod) =>
                      setFormData({ ...formData, paymentMethod: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {paymentMethods.map((method) => (
                        <SelectItem key={method.value} value={method.value}>
                          {method.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="referenceNumber">Número de Referencia</Label>
                  <Input
                    id="referenceNumber"
                    type="text"
                    value={formData.referenceNumber || ''}
                    onChange={(e) =>
                      setFormData({ ...formData, referenceNumber: e.target.value })
                    }
                    placeholder="Número de cheque, transferencia, etc."
                  />
                </div>

                <div className="space-y-2 md:col-span-2">
                  <Label htmlFor="notes">Notas</Label>
                  <Textarea
                    id="notes"
                    value={formData.notes || ''}
                    onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                    placeholder="Notas adicionales..."
                    rows={3}
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Resumen */}
          {selectedInvoiceId && vendorInvoices.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Resumen de Factura</CardTitle>
              </CardHeader>
              <CardContent>
                {(() => {
                  const invoice = vendorInvoices.find((inv) => inv.id === selectedInvoiceId);
                  if (!invoice) return null;
                  return (
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <p className="text-sm text-muted-foreground">Número de Factura</p>
                        <p className="font-medium">{invoice.invoiceNumber}</p>
                      </div>
                      <div>
                        <p className="text-sm text-muted-foreground">Total Factura</p>
                        <p className="font-medium">${invoice.totalAmount.toFixed(2)}</p>
                      </div>
                      <div>
                        <p className="text-sm text-muted-foreground">Monto Pendiente</p>
                        <p className="font-medium text-destructive">
                          ${invoice.amountDue.toFixed(2)}
                        </p>
                      </div>
                      <div>
                        <p className="text-sm text-muted-foreground">Monto a Pagar</p>
                        <p className="font-bold text-lg text-primary">
                          ${formData.amount.toFixed(2)}
                        </p>
                      </div>
                      {formData.amount > invoice.amountDue && (
                        <div className="col-span-2">
                          <p className="text-sm text-amber-600">
                            ⚠️ El monto a pagar excede el monto pendiente de la factura
                          </p>
                        </div>
                      )}
                    </div>
                  );
                })()}
              </CardContent>
            </Card>
          )}

          {/* Botones de acción */}
          <div className="flex justify-end gap-4">
            <Button type="button" variant="outline" onClick={() => navigate('/app/payments')}>
              Cancelar
            </Button>
            <Button type="submit">{isEditing ? 'Actualizar Pago' : 'Crear Pago'}</Button>
          </div>
        </div>
      </form>
    </div>
  );
};

export default PaymentForm;
