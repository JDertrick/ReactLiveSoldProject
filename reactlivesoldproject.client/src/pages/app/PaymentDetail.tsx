import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { usePayments } from '../../hooks/usePayments';
import { PaymentDto, PaymentStatus, PaymentMethod } from '../../types/purchases.types';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { ArrowLeft } from 'lucide-react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import { toast } from 'sonner';

const PaymentDetail = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const { getPaymentById, approvePayment, rejectPayment, postPayment } = usePayments();
  const [payment, setPayment] = useState<PaymentDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (id) {
      loadPayment();
    }
  }, [id]);

  const loadPayment = () => {
    if (!id) return;
    setLoading(true);
    getPaymentById(id)
      .then((data) => {
        if (data) setPayment(data);
      })
      .finally(() => setLoading(false));
  };

  const handleApprove = async () => {
    if (!id) return;
    try {
      await approvePayment(id);
      toast.success('Pago aprobado correctamente');
      loadPayment();
    } catch (error: any) {
      toast.error(error.message || 'Error al aprobar el pago');
    }
  };

  const handlePost = async () => {
    if (!id) return;
    try {
      await postPayment(id);
      toast.success('Pago contabilizado correctamente');
      loadPayment();
    } catch (error: any) {
      toast.error(error.message || 'Error al contabilizar el pago');
    }
  };

  const getStatusBadge = (status: PaymentStatus) => {
    const variants: Record<
      PaymentStatus,
      { variant: 'default' | 'secondary' | 'destructive' | 'outline'; label: string }
    > = {
      Pending: { variant: 'secondary', label: 'Pendiente' },
      Approved: { variant: 'outline', label: 'Aprobado' },
      Rejected: { variant: 'destructive', label: 'Rechazado' },
      Posted: { variant: 'default', label: 'Contabilizado' },
    };
    const config = variants[status];
    return <Badge variant={config.variant}>{config.label}</Badge>;
  };

  const getPaymentMethodLabel = (method: PaymentMethod) => {
    const labels: Record<PaymentMethod, string> = {
      Cash: 'Efectivo',
      Check: 'Cheque',
      BankTransfer: 'Transferencia Bancaria',
      CreditCard: 'Tarjeta de Crédito',
      DebitCard: 'Tarjeta de Débito',
      Other: 'Otro',
    };
    return labels[method];
  };

  const formatCurrency = (amount: number, currency: string = 'MXN') => {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: currency,
    }).format(amount);
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (!payment) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate('/app/payments')}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <h1 className="text-3xl font-bold tracking-tight">Pago no encontrado</h1>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate('/app/payments')}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Pago {payment.paymentNumber}</h1>
            <p className="text-muted-foreground mt-1">Detalles del pago a proveedor</p>
          </div>
        </div>
        <div className="flex gap-2">
          {payment.status === PaymentStatus.Pending && (
            <Button onClick={handleApprove} variant="default">
              Aprobar Pago
            </Button>
          )}
          {payment.status === PaymentStatus.Approved && (
            <Button onClick={handlePost} variant="default">
              Contabilizar
            </Button>
          )}
        </div>
      </div>

      {/* General Information */}
      <Card>
        <CardHeader>
          <CardTitle>Información General</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">Número de Pago</p>
              <p className="font-medium">{payment.paymentNumber}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Proveedor</p>
              <p className="font-medium">{payment.vendorName}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Estado</p>
              <div className="mt-1">{getStatusBadge(payment.status)}</div>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Fecha de Pago</p>
              <p className="font-medium">
                {payment.paymentDate
                  ? format(new Date(payment.paymentDate), 'dd MMM yyyy', { locale: es })
                  : '-'}
              </p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Método de Pago</p>
              <p className="font-medium">{getPaymentMethodLabel(payment.paymentMethod)}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Número de Referencia</p>
              <p className="font-medium">{payment.referenceNumber || '-'}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Monto</p>
              <p className="font-bold text-lg">{formatCurrency(payment.amountPaid, payment.currency)}</p>
            </div>
            {payment.currency !== 'MXN' && (
              <div>
                <p className="text-sm text-muted-foreground">Tipo de Cambio</p>
                <p className="font-medium">{payment.exchangeRate?.toFixed(4) || '1.0000'}</p>
              </div>
            )}
            {payment.companyBankAccountName && (
              <div>
                <p className="text-sm text-muted-foreground">Cuenta Bancaria (Empresa)</p>
                <p className="font-medium">{payment.companyBankAccountName}</p>
              </div>
            )}
            {payment.vendorBankAccountName && (
              <div>
                <p className="text-sm text-muted-foreground">Cuenta Bancaria (Proveedor)</p>
                <p className="font-medium">{payment.vendorBankAccountName}</p>
              </div>
            )}
            {payment.createdByUserName && (
              <div>
                <p className="text-sm text-muted-foreground">Creado Por</p>
                <p className="font-medium">{payment.createdByUserName}</p>
              </div>
            )}
            {payment.notes && (
              <div className="md:col-span-2 lg:col-span-3">
                <p className="text-sm text-muted-foreground">Notas</p>
                <p className="font-medium">{payment.notes}</p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Payment Applications */}
      {payment.applications && payment.applications.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Aplicación del Pago a Facturas</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Número de Factura</TableHead>
                  <TableHead>Referencia</TableHead>
                  <TableHead className="text-right">Total Factura</TableHead>
                  <TableHead className="text-right">Monto Aplicado</TableHead>
                  <TableHead className="text-right">Pendiente</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {payment.applications.map((app) => (
                  <TableRow key={app.vendorInvoiceId}>
                    <TableCell className="font-medium">{app.invoiceNumber || app.vendorInvoiceNumber || '-'}</TableCell>
                    <TableCell>{app.vendorInvoiceReference || '-'}</TableCell>
                    <TableCell className="text-right">
                      {formatCurrency(app.invoiceTotalAmount || 0, payment.currency)}
                    </TableCell>
                    <TableCell className="text-right font-medium text-green-600">
                      {formatCurrency(app.amountApplied || 0, payment.currency)}
                    </TableCell>
                    <TableCell className="text-right">
                      {formatCurrency(
                        (app.invoiceTotalAmount || 0) - (app.invoiceAmountPaid || 0),
                        payment.currency
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      {/* Accounting Information */}
      {payment.status === PaymentStatus.Posted && payment.journalEntryNumber && (
        <Card>
          <CardHeader>
            <CardTitle>Información Contable</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-muted-foreground">Asiento Contable</p>
                <p className="font-medium">{payment.journalEntryNumber}</p>
              </div>
              {payment.postedAt && (
                <div>
                  <p className="text-sm text-muted-foreground">Fecha de Contabilización</p>
                  <p className="font-medium">
                    {format(new Date(payment.postedAt), 'dd MMM yyyy HH:mm', { locale: es })}
                  </p>
                </div>
              )}
              {payment.postedByUserName && (
                <div>
                  <p className="text-sm text-muted-foreground">Contabilizado Por</p>
                  <p className="font-medium">{payment.postedByUserName}</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
};

export default PaymentDetail;
