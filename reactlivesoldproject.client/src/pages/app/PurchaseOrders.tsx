import { useState, useEffect } from 'react';
import { usePurchaseOrders } from '../../hooks/usePurchaseOrders';
import { PurchaseOrderDto, PurchaseOrderStatus } from '../../types/purchases.types';
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

const PurchaseOrdersPage = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const vendorIdFromUrl = searchParams.get('vendorId');

  const [status, setStatus] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearchTerm = useDebounce(searchTerm, 500);

  const { purchaseOrders, loading, fetchPurchaseOrders, deletePurchaseOrder } =
    usePurchaseOrders();

  const [alertDialog, setAlertDialog] = useState<AlertDialogState>({
    open: false,
    title: '',
    description: '',
  });

  useEffect(() => {
    fetchPurchaseOrders(
      vendorIdFromUrl || undefined,
      status === 'all' ? undefined : status
    );
  }, [status, vendorIdFromUrl, fetchPurchaseOrders]);

  const filteredOrders = purchaseOrders.filter((po) => {
    if (!debouncedSearchTerm) return true;
    const searchLower = debouncedSearchTerm.toLowerCase();
    return (
      po.poNumber.toLowerCase().includes(searchLower) ||
      po.vendorName?.toLowerCase().includes(searchLower)
    );
  });

  const handleDelete = async (id: string) => {
    try {
      await deletePurchaseOrder(id);
      toast.success('Orden de compra eliminada correctamente');
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: 'Error',
        description: error.message || 'Error al eliminar la orden de compra',
      });
    }
  };

  const getStatusBadge = (status: PurchaseOrderStatus) => {
    const variants: Record<
      PurchaseOrderStatus,
      { variant: 'default' | 'secondary' | 'destructive' | 'outline'; label: string }
    > = {
      Draft: { variant: 'outline', label: 'Borrador' },
      Submitted: { variant: 'secondary', label: 'Enviada' },
      Approved: { variant: 'default', label: 'Aprobada' },
      PartiallyReceived: { variant: 'secondary', label: 'Parcial' },
      Received: { variant: 'default', label: 'Recibida' },
      Cancelled: { variant: 'destructive', label: 'Cancelada' },
    };
    const config = variants[status];
    return <Badge variant={config.variant}>{config.label}</Badge>;
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
          <h1 className="text-3xl font-bold tracking-tight">Órdenes de Compra</h1>
          <p className="text-muted-foreground mt-1">
            Gestiona las órdenes de compra a proveedores
          </p>
        </div>
        <Button
          onClick={() => navigate('/app/purchase-orders/new')}
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
          Nueva Orden de Compra
        </Button>
      </div>

      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <div className="flex items-center gap-4 w-full">
              <Input
                placeholder="Buscar por número de orden o proveedor..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="max-w-sm"
              />
              <Tabs value={status} onValueChange={setStatus}>
                <TabsList>
                  <TabsTrigger value="all">Todas</TabsTrigger>
                  <TabsTrigger value="Draft">Borrador</TabsTrigger>
                  <TabsTrigger value="Submitted">Enviadas</TabsTrigger>
                  <TabsTrigger value="Approved">Aprobadas</TabsTrigger>
                  <TabsTrigger value="Received">Recibidas</TabsTrigger>
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
          ) : filteredOrders.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              No se encontraron órdenes de compra
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Número</TableHead>
                  <TableHead>Proveedor</TableHead>
                  <TableHead>Fecha</TableHead>
                  <TableHead>Estado</TableHead>
                  <TableHead className="text-right">Total</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredOrders.map((po) => (
                  <TableRow key={po.id}>
                    <TableCell className="font-medium">{po.poNumber}</TableCell>
                    <TableCell>{po.vendorName}</TableCell>
                    <TableCell>
                      {format(new Date(po.orderDate), 'dd MMM yyyy', { locale: es })}
                    </TableCell>
                    <TableCell>{getStatusBadge(po.status)}</TableCell>
                    <TableCell className="text-right">
                      {formatCurrency(po.totalAmount, po.currency)}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => navigate(`/app/purchase-orders/${po.id}`)}
                        >
                          Ver
                        </Button>
                        {po.status === PurchaseOrderStatus.Draft && (
                          <>
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => navigate(`/app/purchase-orders/${po.id}/edit`)}
                            >
                              Editar
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => handleDelete(po.id)}
                            >
                              Eliminar
                            </Button>
                          </>
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

export default PurchaseOrdersPage;
