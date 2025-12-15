import { useState, useEffect } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { usePayments } from '../../hooks/usePayments';
import { useVendorInvoices } from '../../hooks/useVendorInvoices';
import { useGetVendors } from '../../hooks/useVendors';
import { useCompanyBankAccounts } from '../../hooks/useCompanyBankAccounts';
import { useVendorBankAccounts } from '../../hooks/useVendorBankAccounts';
import { CreatePaymentDto, PaymentMethod } from '../../types/purchases.types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { toast } from 'sonner';
import { ArrowLeft } from 'lucide-react';
import { Textarea } from '@/components/ui/textarea';
import { AutoNumberInput } from '@/components/common/AutoNumberInput';

const PaymentForm = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const invoiceIdFromUrl = searchParams.get('invoiceId');
  const isEditing = !!id;

  const { createPayment } = usePayments();
  const { vendorInvoices, fetchVendorInvoices } = useVendorInvoices();
  const { data: vendors } = useGetVendors();
  const { companyBankAccounts, fetchCompanyBankAccounts } = useCompanyBankAccounts();
  const { vendorBankAccounts, fetchVendorBankAccounts } = useVendorBankAccounts();

  const [paymentNumber, setPaymentNumber] = useState<string>('');
  const [selectedInvoiceId, setSelectedInvoiceId] = useState<string | undefined>(invoiceIdFromUrl || undefined);

  const [formData, setFormData] = useState<CreatePaymentDto>({
    vendorId: '',
    paymentDate: new Date().toISOString().split('T')[0],
    amountPaid: 0,
    paymentMethod: PaymentMethod.BankTransfer,
    companyBankAccountId: '',
    vendorBankAccountId: '',
    referenceNumber: '',
    notes: '',
    invoiceApplications: [],
  });

  useEffect(() => {
    // Fetch unpaid vendor invoices and company bank accounts
    fetchVendorInvoices(undefined, 'Unpaid');
    fetchCompanyBankAccounts();
  }, [fetchVendorInvoices, fetchCompanyBankAccounts]);

  // If invoice is preselected, get vendor and amount
  useEffect(() => {
    if (selectedInvoiceId && vendorInvoices.length > 0) {
      const invoice = vendorInvoices.find((inv) => inv.id === selectedInvoiceId);
      if (invoice) {
        setFormData((prev) => ({
          ...prev,
          vendorId: invoice.vendorId || '',
          amountPaid: invoice.amountDue,
        }));
      }
    }
  }, [selectedInvoiceId, vendorInvoices]);

  // Fetch vendor bank accounts when vendor is selected
  useEffect(() => {
    if (formData.vendorId) {
      fetchVendorBankAccounts(formData.vendorId);
    }
  }, [formData.vendorId, fetchVendorBankAccounts]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.vendorId) {
      toast.error('Seleccione un proveedor');
      return;
    }

    if (!formData.companyBankAccountId) {
      toast.error('Seleccione la cuenta bancaria de su empresa');
      return;
    }

    if (formData.amountPaid <= 0) {
      toast.error('El monto debe ser mayor a 0');
      return;
    }

    // Prepare invoice applications
    const invoiceApplications = selectedInvoiceId
      ? [{ vendorInvoiceId: selectedInvoiceId, amountApplied: formData.amountPaid }]
      : [];

    // Prepare payment data, removing empty strings for optional GUIDs
    const paymentData: CreatePaymentDto = {
      vendorId: formData.vendorId,
      paymentDate: formData.paymentDate,
      paymentMethod: formData.paymentMethod,
      companyBankAccountId: formData.companyBankAccountId,
      vendorBankAccountId: formData.vendorBankAccountId && formData.vendorBankAccountId !== ''
        ? formData.vendorBankAccountId
        : undefined,
      amountPaid: formData.amountPaid,
      currency: formData.currency || 'MXN',
      exchangeRate: formData.exchangeRate || 1.0,
      referenceNumber: formData.referenceNumber || undefined,
      notes: formData.notes || undefined,
      invoiceApplications,
    };

    try {
      await createPayment(paymentData);
      toast.success('Pago creado exitosamente');
      navigate('/app/payments');
    } catch (error: any) {
      toast.error(error.response?.data?.message || error.message || 'Error al crear el pago');
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
              {/* Número de Pago */}
              <AutoNumberInput
                label="No. Pago"
                value={paymentNumber}
                onChange={setPaymentNumber}
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
                  <Label htmlFor="amountPaid">Monto *</Label>
                  <Input
                    id="amountPaid"
                    type="number"
                    step="0.01"
                    min="0"
                    value={formData.amountPaid}
                    onChange={(e) =>
                      setFormData({ ...formData, amountPaid: parseFloat(e.target.value) || 0 })
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
                  <Label htmlFor="companyBankAccountId">Cuenta Bancaria de la Empresa *</Label>
                  <Select
                    value={formData.companyBankAccountId}
                    onValueChange={(value) =>
                      setFormData({ ...formData, companyBankAccountId: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione cuenta bancaria" />
                    </SelectTrigger>
                    <SelectContent>
                      {companyBankAccounts.map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.bankName} - {account.accountNumber} ({account.currency})
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">
                    Cuenta desde la cual se realizará el pago
                  </p>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="vendorBankAccountId">Cuenta Bancaria del Proveedor (Opcional)</Label>
                  <Select
                    value={formData.vendorBankAccountId || 'none'}
                    onValueChange={(value) =>
                      setFormData({ ...formData, vendorBankAccountId: value === 'none' ? '' : value })
                    }
                    disabled={!formData.vendorId}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione cuenta del proveedor" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Sin especificar</SelectItem>
                      {vendorBankAccounts.map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.bankName} - {account.accountNumber}
                          {account.isPrimary && ' (Principal)'}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">
                    Cuenta destino donde se depositará el pago
                  </p>
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
                          ${formData.amountPaid.toFixed(2)}
                        </p>
                      </div>
                      {formData.amountPaid > invoice.amountDue && (
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
