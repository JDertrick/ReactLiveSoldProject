import { useState, useEffect } from 'react';
import { useVendorInvoices } from '../../hooks/useVendorInvoices';
import {
  VendorInvoiceDto,
  InvoiceStatus,
  InvoicePaymentStatus,
} from '../../types/purchases.types';
import { CustomAlertDialog } from '@/components/common/AlertDialog';
import { AlertDialogState } from '@/types/alertdialogstate.type';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useDebounce } from '@/hooks/useDebounce';
import { toast } from 'sonner';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

const VendorInvoicesPage = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const vendorIdFromUrl = searchParams.get('vendorId');

  const [status, setStatus] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearchTerm = useDebounce(searchTerm, 500);

  const { vendorInvoices, loading, fetchVendorInvoices, deleteVendorInvoice } =
    useVendorInvoices();

  const [alertDialog, setAlertDialog] = useState<AlertDialogState>({
    open: false,
    title: '',
    description: '',
  });

  useEffect(() => {
    fetchVendorInvoices(
      vendorIdFromUrl || undefined,
      status === 'all' ? undefined : status
    );
  }, [status, vendorIdFromUrl, fetchVendorInvoices]);

  const filteredInvoices = vendorInvoices.filter((inv) => {
    if (!debouncedSearchTerm) return true;
    const searchLower = debouncedSearchTerm.toLowerCase();
    return (
      inv.invoiceNumber.toLowerCase().includes(searchLower) ||
      inv.vendorInvoiceReference?.toLowerCase().includes(searchLower) ||
      inv.vendorName?.toLowerCase().includes(searchLower)
    );
  });

  const handleDelete = async (id: string) => {
    try {
      await deleteVendorInvoice(id);
      toast.success('Factura eliminada correctamente');
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: 'Error',
        description: error.message || 'Error al eliminar la factura',
      });
    }
  };

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

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Facturas de Proveedores</h1>
          <p className="text-muted-foreground mt-1">
            Gestiona las facturas recibidas de proveedores
          </p>
        </div>
        <Button
          onClick={() => navigate('/app/vendor-invoices/new')}
          size="lg"
          className="gap-2"
        >
          <svg
            className="w-5 h-5"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth={2}
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M12 6v6m0 0v6m0-6h6m-6 0H6"
            />
          </svg>
          Nueva Factura
        </Button>
      </div>

      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <div className="flex items-center gap-4 w-full">
              <Input
                placeholder="Buscar por número de factura o proveedor..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="max-w-sm"
              />
              <Tabs value={status} onValueChange={setStatus}>
                <TabsList>
                  <TabsTrigger value="all">Todas</TabsTrigger>
                  <TabsTrigger value="Unpaid">Pendientes</TabsTrigger>
                  <TabsTrigger value="PartiallyPaid">Parciales</TabsTrigger>
                  <TabsTrigger value="Paid">Pagadas</TabsTrigger>
                </TabsList>
              </Tabs>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="flex justify-center items-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
            </div>
          ) : filteredInvoices.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              No se encontraron facturas
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Número</TableHead>
                  <TableHead>Referencia</TableHead>
                  <TableHead>Proveedor</TableHead>
                  <TableHead>Fecha</TableHead>
                  <TableHead>Vencimiento</TableHead>
                  <TableHead>Estado Pago</TableHead>
                  <TableHead className="text-right">Total</TableHead>
                  <TableHead className="text-right">Pendiente</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredInvoices.map((inv) => (
                  <TableRow key={inv.id}>
                    <TableCell className="font-medium">{inv.invoiceNumber}</TableCell>
                    <TableCell>{inv.vendorInvoiceReference || '-'}</TableCell>
                    <TableCell>{inv.vendorName}</TableCell>
                    <TableCell>
                      {format(new Date(inv.invoiceDate), 'dd MMM yyyy', { locale: es })}
                    </TableCell>
                    <TableCell>
                      {format(new Date(inv.dueDate), 'dd MMM yyyy', { locale: es })}
                    </TableCell>
                    <TableCell>{getPaymentStatusBadge(inv.paymentStatus)}</TableCell>
                    <TableCell className="text-right">
                      {formatCurrency(inv.totalAmount)}
                    </TableCell>
                    <TableCell className="text-right">
                      {formatCurrency(inv.amountDue)}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => navigate(`/app/vendor-invoices/${inv.id}`)}
                        >
                          Ver
                        </Button>
                        {inv.paymentStatus === InvoicePaymentStatus.Unpaid && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => navigate(`/app/payments/new?invoiceId=${inv.id}`)}
                          >
                            Pagar
                          </Button>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <CustomAlertDialog
        open={alertDialog.open}
        onClose={() => setAlertDialog({ ...alertDialog, open: false })}
        title={alertDialog.title}
        description={alertDialog.description}
      />
    </div>
  );
};

export default VendorInvoicesPage;
