import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { usePurchaseOrders } from '../../hooks/usePurchaseOrders';
import { PurchaseOrderDto, PurchaseOrderStatus } from '../../types/purchases.types';
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
import { ArrowLeft, Edit } from 'lucide-react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

const PurchaseOrderDetail = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const { getPurchaseOrderById } = usePurchaseOrders();
  const [order, setOrder] = useState<PurchaseOrderDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (id) {
      setLoading(true);
      getPurchaseOrderById(id)
        .then((data) => {
          if (data) setOrder(data);
        })
        .finally(() => setLoading(false));
    }
  }, [id, getPurchaseOrderById]);

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

  if (loading) {
    return (
      <div className="flex justify-center items-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (!order) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate('/app/purchase-orders')}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <h1 className="text-3xl font-bold tracking-tight">Orden de Compra no encontrada</h1>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate('/app/purchase-orders')}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">
              Orden de Compra {order.poNumber}
            </h1>
            <p className="text-muted-foreground mt-1">
              Detalles de la orden de compra
            </p>
          </div>
        </div>
        <div className="flex gap-2">
          {order.status === PurchaseOrderStatus.Draft && (
            <Button
              onClick={() => navigate(`/app/purchase-orders/${order.id}/edit`)}
              className="gap-2"
            >
              <Edit className="h-4 w-4" />
              Editar
            </Button>
          )}
          {(order.status === PurchaseOrderStatus.Approved ||
            order.status === PurchaseOrderStatus.PartiallyReceived) && (
            <Button
              onClick={() => navigate(`/app/purchase-receipts/new?poId=${order.id}`)}
              className="gap-2"
            >
              Crear Recepción
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
              <p className="text-sm text-muted-foreground">Número de Orden</p>
              <p className="font-medium">{order.poNumber}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Proveedor</p>
              <p className="font-medium">{order.vendorName}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Estado</p>
              <div className="mt-1">{getStatusBadge(order.status)}</div>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Fecha de Orden</p>
              <p className="font-medium">
                {format(new Date(order.orderDate), 'dd MMM yyyy', { locale: es })}
              </p>
            </div>
            {order.expectedDeliveryDate && (
              <div>
                <p className="text-sm text-muted-foreground">Fecha Estimada de Entrega</p>
                <p className="font-medium">
                  {format(new Date(order.expectedDeliveryDate), 'dd MMM yyyy', { locale: es })}
                </p>
              </div>
            )}
            <div>
              <p className="text-sm text-muted-foreground">Moneda</p>
              <p className="font-medium">{order.currency}</p>
            </div>
            {order.exchangeRate !== 1 && (
              <div>
                <p className="text-sm text-muted-foreground">Tipo de Cambio</p>
                <p className="font-medium">{order.exchangeRate.toFixed(4)}</p>
              </div>
            )}
            <div>
              <p className="text-sm text-muted-foreground">Creado Por</p>
              <p className="font-medium">{order.createdByUserName || 'N/A'}</p>
            </div>
            {order.notes && (
              <div className="md:col-span-2 lg:col-span-3">
                <p className="text-sm text-muted-foreground">Notas</p>
                <p className="font-medium">{order.notes}</p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Items */}
      <Card>
        <CardHeader>
          <CardTitle>Items de la Orden</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Línea</TableHead>
                <TableHead>Producto</TableHead>
                <TableHead>Descripción</TableHead>
                <TableHead className="text-right">Cantidad</TableHead>
                <TableHead className="text-right">Costo Unitario</TableHead>
                <TableHead className="text-right">Descuento</TableHead>
                <TableHead className="text-right">Impuesto</TableHead>
                <TableHead className="text-right">Total</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {order.items.map((item) => (
                <TableRow key={item.id}>
                  <TableCell>{item.lineNumber}</TableCell>
                  <TableCell>
                    <div>
                      <div className="font-medium">{item.productName}</div>
                      {item.variantName && (
                        <div className="text-sm text-muted-foreground">{item.variantName}</div>
                      )}
                    </div>
                  </TableCell>
                  <TableCell className="max-w-xs">
                    <div className="text-sm text-muted-foreground">{item.description}</div>
                  </TableCell>
                  <TableCell className="text-right">{item.quantity}</TableCell>
                  <TableCell className="text-right">
                    {formatCurrency(item.unitCost, order.currency)}
                  </TableCell>
                  <TableCell className="text-right">
                    {item.discountPercentage > 0 ? (
                      <div>
                        <div>{item.discountPercentage}%</div>
                        <div className="text-sm text-muted-foreground">
                          -{formatCurrency(item.discountAmount, order.currency)}
                        </div>
                      </div>
                    ) : (
                      '-'
                    )}
                  </TableCell>
                  <TableCell className="text-right">
                    {item.taxRate > 0 ? (
                      <div>
                        <div>{item.taxRate}%</div>
                        <div className="text-sm text-muted-foreground">
                          {formatCurrency(item.taxAmount, order.currency)}
                        </div>
                      </div>
                    ) : (
                      '-'
                    )}
                  </TableCell>
                  <TableCell className="text-right font-medium">
                    {formatCurrency(item.lineTotal, order.currency)}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Totals */}
      <Card>
        <CardHeader>
          <CardTitle>Totales</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-2">
            <div className="flex justify-between">
              <span className="text-muted-foreground">Subtotal</span>
              <span className="font-medium">{formatCurrency(order.subtotal, order.currency)}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Impuestos</span>
              <span className="font-medium">{formatCurrency(order.taxAmount, order.currency)}</span>
            </div>
            <div className="flex justify-between text-lg font-bold pt-2 border-t">
              <span>Total</span>
              <span>{formatCurrency(order.totalAmount, order.currency)}</span>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default PurchaseOrderDetail;
