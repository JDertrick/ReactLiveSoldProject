import { useState, useEffect } from 'react';
import { usePurchaseReceipts } from '../../hooks/usePurchaseReceipts';
import { PurchaseReceiptDto, PurchaseReceiptStatus } from '../../types/purchases.types';
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

const PurchaseReceiptsPage = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const vendorIdFromUrl = searchParams.get('vendorId');

  const [status, setStatus] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearchTerm = useDebounce(searchTerm, 500);

  const { purchaseReceipts, loading, fetchPurchaseReceipts, deletePurchaseReceipt } =
    usePurchaseReceipts();

  const [alertDialog, setAlertDialog] = useState<AlertDialogState>({
    open: false,
    title: '',
    description: '',
  });

  useEffect(() => {
    fetchPurchaseReceipts(
      vendorIdFromUrl || undefined,
      status === 'all' ? undefined : status
    );
  }, [status, vendorIdFromUrl, fetchPurchaseReceipts]);

  const filteredReceipts = purchaseReceipts.filter((receipt) => {
    if (!debouncedSearchTerm) return true;
    const searchLower = debouncedSearchTerm.toLowerCase();
    return (
      receipt.receiptNumber.toLowerCase().includes(searchLower) ||
      receipt.vendorName?.toLowerCase().includes(searchLower) ||
      receipt.purchaseOrderNumber?.toLowerCase().includes(searchLower)
    );
  });

  const handleDelete = async (id: string) => {
    try {
      await deletePurchaseReceipt(id);
      toast.success('Recepción eliminada correctamente');
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: 'Error',
        description: error.message || 'Error al eliminar la recepción',
      });
    }
  };

  const getStatusBadge = (status: PurchaseReceiptStatus) => {
    const variants: Record<
      PurchaseReceiptStatus,
      { variant: 'default' | 'secondary' | 'destructive' | 'outline'; label: string }
    > = {
      Draft: { variant: 'outline', label: 'Borrador' },
      Received: { variant: 'secondary', label: 'Recibida' },
      Posted: { variant: 'default', label: 'Contabilizada' },
      Cancelled: { variant: 'destructive', label: 'Cancelada' },
    };
    const config = variants[status];
    return <Badge variant={config.variant}>{config.label}</Badge>;
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Recepciones de Compra</h1>
          <p className="text-muted-foreground mt-1">
            Gestiona la recepción de mercancía de proveedores
          </p>
        </div>
        <Button
          onClick={() => navigate('/app/purchase-receipts/new')}
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
          Nueva Recepción
        </Button>
      </div>

      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <div className="flex items-center gap-4 w-full">
              <Input
                placeholder="Buscar por número de recepción, OC o proveedor..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="max-w-sm"
              />
              <Tabs value={status} onValueChange={setStatus}>
                <TabsList>
                  <TabsTrigger value="all">Todas</TabsTrigger>
                  <TabsTrigger value="Draft">Borrador</TabsTrigger>
                  <TabsTrigger value="Received">Recibidas</TabsTrigger>
                  <TabsTrigger value="Posted">Contabilizadas</TabsTrigger>
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
          ) : filteredReceipts.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              No se encontraron recepciones
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Número</TableHead>
                  <TableHead>Orden de Compra</TableHead>
                  <TableHead>Proveedor</TableHead>
                  <TableHead>Fecha Recepción</TableHead>
                  <TableHead>Almacén</TableHead>
                  <TableHead>Estado</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredReceipts.map((receipt) => (
                  <TableRow key={receipt.id}>
                    <TableCell className="font-medium">{receipt.receiptNumber}</TableCell>
                    <TableCell>{receipt.purchaseOrderNumber || '-'}</TableCell>
                    <TableCell>{receipt.vendorName}</TableCell>
                    <TableCell>
                      {format(new Date(receipt.receiptDate), 'dd MMM yyyy', { locale: es })}
                    </TableCell>
                    <TableCell>{receipt.warehouseLocationName || '-'}</TableCell>
                    <TableCell>{getStatusBadge(receipt.status)}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => navigate(`/app/purchase-receipts/${receipt.id}`)}
                        >
                          Ver
                        </Button>
                        {receipt.status === PurchaseReceiptStatus.Draft && (
                          <>
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() =>
                                navigate(`/app/purchase-receipts/${receipt.id}/edit`)
                              }
                            >
                              Editar
                            </Button>
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => handleDelete(receipt.id)}
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

export default PurchaseReceiptsPage;
