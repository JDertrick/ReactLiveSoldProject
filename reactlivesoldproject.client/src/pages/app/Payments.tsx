import { useState, useEffect } from 'react';
import { usePayments } from '../../hooks/usePayments';
import { PaymentDto, PaymentStatus, PaymentMethod } from '../../types/purchases.types';
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

const PaymentsPage = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const vendorIdFromUrl = searchParams.get('vendorId');

  const [status, setStatus] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearchTerm = useDebounce(searchTerm, 500);

  const { payments, loading, fetchPayments, approvePayment, rejectPayment, postPayment } =
    usePayments();

  const [alertDialog, setAlertDialog] = useState<AlertDialogState>({
    open: false,
    title: '',
    description: '',
  });

  useEffect(() => {
    fetchPayments(
      vendorIdFromUrl || undefined,
      status === 'all' ? undefined : status
    );
  }, [status, vendorIdFromUrl, fetchPayments]);

  const filteredPayments = payments.filter((payment) => {
    if (!debouncedSearchTerm) return true;
    const searchLower = debouncedSearchTerm.toLowerCase();
    return (
      payment.paymentNumber.toLowerCase().includes(searchLower) ||
      payment.vendorName?.toLowerCase().includes(searchLower) ||
      payment.referenceNumber?.toLowerCase().includes(searchLower)
    );
  });

  const handleApprove = async (id: string) => {
    try {
      await approvePayment(id);
      toast.success('Pago aprobado correctamente');
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: 'Error',
        description: error.message || 'Error al aprobar el pago',
      });
    }
  };

  const handlePost = async (id: string) => {
    try {
      await postPayment(id);
      toast.success('Pago contabilizado correctamente');
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: 'Error',
        description: error.message || 'Error al contabilizar el pago',
      });
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
      BankTransfer: 'Transferencia',
      CreditCard: 'Tarjeta Crédito',
      DebitCard: 'Tarjeta Débito',
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

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Pagos a Proveedores</h1>
          <p className="text-muted-foreground mt-1">
            Gestiona los pagos realizados a proveedores
          </p>
        </div>
        <Button onClick={() => navigate('/app/payments/new')} size="lg" className="gap-2">
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
          Nuevo Pago
        </Button>
      </div>

      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <div className="flex items-center gap-4 w-full">
              <Input
                placeholder="Buscar por número de pago o proveedor..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="max-w-sm"
              />
              <Tabs value={status} onValueChange={setStatus}>
                <TabsList>
                  <TabsTrigger value="all">Todos</TabsTrigger>
                  <TabsTrigger value="Pending">Pendientes</TabsTrigger>
                  <TabsTrigger value="Approved">Aprobados</TabsTrigger>
                  <TabsTrigger value="Posted">Contabilizados</TabsTrigger>
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
          ) : filteredPayments.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              No se encontraron pagos
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Número</TableHead>
                  <TableHead>Proveedor</TableHead>
                  <TableHead>Fecha</TableHead>
                  <TableHead>Método</TableHead>
                  <TableHead>Referencia</TableHead>
                  <TableHead>Estado</TableHead>
                  <TableHead className="text-right">Monto</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredPayments.map((payment) => (
                  <TableRow key={payment.id}>
                    <TableCell className="font-medium">{payment.paymentNumber}</TableCell>
                    <TableCell>{payment.vendorName}</TableCell>
                    <TableCell>
                      {format(new Date(payment.paymentDate), 'dd MMM yyyy', { locale: es })}
                    </TableCell>
                    <TableCell>{getPaymentMethodLabel(payment.paymentMethod)}</TableCell>
                    <TableCell>{payment.referenceNumber || '-'}</TableCell>
                    <TableCell>{getStatusBadge(payment.status)}</TableCell>
                    <TableCell className="text-right">
                      {formatCurrency(payment.amount, payment.currency)}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => navigate(`/app/payments/${payment.id}`)}
                        >
                          Ver
                        </Button>
                        {payment.status === PaymentStatus.Pending && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleApprove(payment.id)}
                          >
                            Aprobar
                          </Button>
                        )}
                        {payment.status === PaymentStatus.Approved && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handlePost(payment.id)}
                          >
                            Contabilizar
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

export default PaymentsPage;
