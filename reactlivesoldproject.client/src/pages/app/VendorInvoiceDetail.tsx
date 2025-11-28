import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useVendorInvoices } from '../../hooks/useVendorInvoices';
import { VendorInvoiceDto, InvoicePaymentStatus } from '../../types/purchases.types';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { ArrowLeft } from 'lucide-react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

const VendorInvoiceDetail = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const { getVendorInvoiceById } = useVendorInvoices();
  const [invoice, setInvoice] = useState<VendorInvoiceDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (id) {
      setLoading(true);
      getVendorInvoiceById(id)
        .then((data) => {
          if (data) setInvoice(data);
        })
        .finally(() => setLoading(false));
    }
  }, [id, getVendorInvoiceById]);

  const getPaymentStatusBadge = (status: InvoicePaymentStatus) => {
    const variants: Record<
      InvoicePaymentStatus,
      { variant: 'default' | 'secondary' | 'destructive' | 'outline'; label: string }
    > = {
      Unpaid: { variant: 'destructive', label: 'Pendiente' },
      PartiallyPaid: { variant: 'secondary', label: 'Parcial' },
      Paid: { variant: 'default', label: 'Pagada' },
      Overdue: { variant: 'destructive', label: 'Vencida' },
    };
    const config = variants[status];
    return <Badge variant={config.variant}>{config.label}</Badge>;
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN',
    }).format(amount);
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (!invoice) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate('/app/vendor-invoices')}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <h1 className="text-3xl font-bold tracking-tight">Factura no encontrada</h1>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate('/app/vendor-invoices')}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">
              Factura {invoice.invoiceNumber}
            </h1>
            <p className="text-muted-foreground mt-1">Detalles de la factura del proveedor</p>
          </div>
        </div>
        <div className="flex gap-2">
          {invoice.paymentStatus === InvoicePaymentStatus.Unpaid && (
            <Button
              onClick={() => navigate(`/app/payments/new?invoiceId=${invoice.id}`)}
              className="gap-2"
            >
              Registrar Pago
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
              <p className="text-sm text-muted-foreground">Número de Factura</p>
              <p className="font-medium">{invoice.invoiceNumber}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Referencia del Proveedor</p>
              <p className="font-medium">{invoice.vendorInvoiceReference || '-'}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Proveedor</p>
              <p className="font-medium">{invoice.vendorName}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Fecha de Factura</p>
              <p className="font-medium">
                {format(new Date(invoice.invoiceDate), 'dd MMM yyyy', { locale: es })}
              </p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Fecha de Vencimiento</p>
              <p className="font-medium">
                {format(new Date(invoice.dueDate), 'dd MMM yyyy', { locale: es })}
              </p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Estado de Pago</p>
              <div className="mt-1">{getPaymentStatusBadge(invoice.paymentStatus)}</div>
            </div>
            {invoice.purchaseReceiptNumber && (
              <div>
                <p className="text-sm text-muted-foreground">Recepción de Compra</p>
                <p className="font-medium">{invoice.purchaseReceiptNumber}</p>
              </div>
            )}
            {invoice.notes && (
              <div className="md:col-span-2 lg:col-span-3">
                <p className="text-sm text-muted-foreground">Notas</p>
                <p className="font-medium">{invoice.notes}</p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Payment Details */}
      <Card>
        <CardHeader>
          <CardTitle>Detalles de Pago</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            <div className="flex justify-between">
              <span className="text-muted-foreground">Subtotal</span>
              <span className="font-medium">{formatCurrency(invoice.subtotal)}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Impuestos</span>
              <span className="font-medium">{formatCurrency(invoice.taxAmount)}</span>
            </div>
            <div className="flex justify-between text-lg font-bold pt-2 border-t">
              <span>Total de Factura</span>
              <span>{formatCurrency(invoice.totalAmount)}</span>
            </div>
            <div className="flex justify-between pt-2 border-t">
              <span className="text-muted-foreground">Monto Pagado</span>
              <span className="font-medium text-green-600">
                {formatCurrency(invoice.amountPaid)}
              </span>
            </div>
            <div className="flex justify-between text-lg font-bold pt-2 border-t">
              <span className="text-destructive">Monto Pendiente</span>
              <span className="text-destructive">{formatCurrency(invoice.amountDue)}</span>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Payment Applications (if any) */}
      {invoice.paymentApplications && invoice.paymentApplications.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Historial de Pagos</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {invoice.paymentApplications.map((application, index) => (
                <div
                  key={index}
                  className="flex justify-between items-center p-3 bg-muted rounded-lg"
                >
                  <div>
                    <p className="font-medium">
                      {application.paymentNumber || `Pago #${index + 1}`}
                    </p>
                    {application.paymentDate && (
                      <p className="text-sm text-muted-foreground">
                        {format(new Date(application.paymentDate), 'dd MMM yyyy', { locale: es })}
                      </p>
                    )}
                  </div>
                  <span className="font-medium text-green-600">
                    {formatCurrency(application.amountApplied)}
                  </span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
};

export default VendorInvoiceDetail;
