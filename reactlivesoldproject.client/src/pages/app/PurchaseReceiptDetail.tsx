import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { usePurchaseReceipts } from '../../hooks/usePurchaseReceipts';
import { PurchaseReceiptDto, PurchaseReceiptStatus } from '../../types/purchases.types';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { ArrowLeft, Package, FileText, MapPin, Calendar, User } from 'lucide-react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

const PurchaseReceiptDetail = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const { getPurchaseReceiptById } = usePurchaseReceipts();

  const [receipt, setReceipt] = useState<PurchaseReceiptDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (id) {
      setLoading(true);
      getPurchaseReceiptById(id)
        .then((data) => {
          setReceipt(data);
        })
        .catch((error) => {
          console.error('Error al cargar recepción:', error);
        })
        .finally(() => {
          setLoading(false);
        });
    }
  }, [id, getPurchaseReceiptById]);

  const getStatusBadge = (status: PurchaseReceiptStatus) => {
    const variants: Record<
      PurchaseReceiptStatus,
      { variant: 'default' | 'secondary' | 'destructive' | 'outline'; label: string }
    > = {
      Draft: { variant: 'outline', label: 'Borrador' },
      Pending: { variant: 'outline', label: 'Pendiente' },
      Received: { variant: 'secondary', label: 'Recibida' },
      Posted: { variant: 'default', label: 'Contabilizada' },
      Cancelled: { variant: 'destructive', label: 'Cancelada' },
    };
    const config = variants[status];
    return <Badge variant={config?.variant || 'outline'}>{config?.label || status}</Badge>;
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (!receipt) {
    return (
      <div className="flex flex-col items-center justify-center h-screen gap-4">
        <p className="text-muted-foreground">Recepción no encontrada</p>
        <Button onClick={() => navigate('/app/purchase-receipts')}>Volver a Recepciones</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate('/app/purchase-receipts')}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Recepción {receipt.receiptNumber}</h1>
            <p className="text-muted-foreground mt-1">
              Detalles de la recepción de compra
            </p>
          </div>
        </div>
        <div className="flex gap-2">
          {receipt.status === PurchaseReceiptStatus.Pending && (
            <Button variant="outline" onClick={() => navigate(`/app/purchase-receipts/${id}/edit`)}>
              Editar
            </Button>
          )}
          <Button onClick={() => navigate('/app/purchase-receipts')}>Volver</Button>
        </div>
      </div>

      {/* Información General */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Información General</CardTitle>
            {getStatusBadge(receipt.status)}
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div className="flex items-start gap-3">
              <FileText className="h-5 w-5 text-muted-foreground mt-0.5" />
              <div>
                <p className="text-sm text-muted-foreground">Número de Recepción</p>
                <p className="font-medium">{receipt.receiptNumber}</p>
              </div>
            </div>

            {receipt.purchaseOrderNumber && (
              <div className="flex items-start gap-3">
                <Package className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div>
                  <p className="text-sm text-muted-foreground">Orden de Compra</p>
                  <p className="font-medium">{receipt.purchaseOrderNumber}</p>
                </div>
              </div>
            )}

            <div className="flex items-start gap-3">
              <User className="h-5 w-5 text-muted-foreground mt-0.5" />
              <div>
                <p className="text-sm text-muted-foreground">Proveedor</p>
                <p className="font-medium">{receipt.vendorName}</p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <Calendar className="h-5 w-5 text-muted-foreground mt-0.5" />
              <div>
                <p className="text-sm text-muted-foreground">Fecha de Recepción</p>
                <p className="font-medium">
                  {format(new Date(receipt.receiptDate), 'dd MMMM yyyy', { locale: es })}
                </p>
              </div>
            </div>

            {receipt.warehouseLocationName && (
              <div className="flex items-start gap-3">
                <MapPin className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div>
                  <p className="text-sm text-muted-foreground">Almacén</p>
                  <p className="font-medium">{receipt.warehouseLocationName}</p>
                </div>
              </div>
            )}

            <div className="flex items-start gap-3">
              <User className="h-5 w-5 text-muted-foreground mt-0.5" />
              <div>
                <p className="text-sm text-muted-foreground">Recibido Por</p>
                <p className="font-medium">{receipt.receivedByName || 'N/A'}</p>
              </div>
            </div>
          </div>

          {receipt.notes && (
            <div className="mt-6 pt-6 border-t">
              <p className="text-sm text-muted-foreground mb-2">Notas</p>
              <p className="text-sm">{receipt.notes}</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Items Recibidos */}
      <Card>
        <CardHeader>
          <CardTitle>Productos Recibidos</CardTitle>
        </CardHeader>
        <CardContent>
          {receipt.items && receipt.items.length > 0 ? (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Producto</TableHead>
                    <TableHead className="text-center">Cant. Solicitada</TableHead>
                    <TableHead className="text-center">Cant. Recibida</TableHead>
                    <TableHead className="text-right">Costo Unit.</TableHead>
                    <TableHead className="text-right">Descuento</TableHead>
                    <TableHead className="text-right">IVA</TableHead>
                    <TableHead className="text-right">Total</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {receipt.items.map((item) => (
                    <TableRow key={item.id}>
                      <TableCell>
                        <div>
                          <p className="font-medium">{item.productName}</p>
                          {item.variantName && (
                            <p className="text-sm text-muted-foreground">{item.variantName}</p>
                          )}
                          {item.description && (
                            <p className="text-xs text-muted-foreground">{item.description}</p>
                          )}
                        </div>
                      </TableCell>
                      <TableCell className="text-center">{item.quantityOrdered}</TableCell>
                      <TableCell className="text-center">
                        <span className="font-medium">{item.quantityReceived}</span>
                        {item.quantityReceived !== item.quantityOrdered && (
                          <span className="text-xs text-amber-600 ml-2">
                            ({item.quantityReceived > item.quantityOrdered ? '+' : ''}
                            {item.quantityReceived - item.quantityOrdered})
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="text-right">${item.unitCost.toFixed(2)}</TableCell>
                      <TableCell className="text-right">{item.discountPercentage}%</TableCell>
                      <TableCell className="text-right">{item.taxRate}%</TableCell>
                      <TableCell className="text-right font-medium">
                        ${item.lineTotal.toFixed(2)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {/* Total */}
              <div className="flex justify-end mt-6 pt-4 border-t">
                <div className="space-y-2 min-w-[300px]">
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Subtotal:</span>
                    <span className="font-medium">
                      $
                      {receipt.items
                        .reduce((sum, item) => {
                          const subtotal = item.unitCost * item.quantityReceived;
                          const discount = subtotal * (item.discountPercentage / 100);
                          return sum + (subtotal - discount);
                        }, 0)
                        .toFixed(2)}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">IVA:</span>
                    <span className="font-medium">
                      $
                      {receipt.items
                        .reduce((sum, item) => sum + item.taxAmount, 0)
                        .toFixed(2)}
                    </span>
                  </div>
                  <div className="flex justify-between pt-2 border-t">
                    <span className="font-bold">Total:</span>
                    <span className="font-bold text-lg">
                      $
                      {receipt.items
                        .reduce((sum, item) => sum + item.lineTotal, 0)
                        .toFixed(2)}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          ) : (
            <p className="text-center text-muted-foreground py-8">
              No hay productos en esta recepción
            </p>
          )}
        </CardContent>
      </Card>
    </div>
  );
};

export default PurchaseReceiptDetail;
