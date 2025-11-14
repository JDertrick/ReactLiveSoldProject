import { useState } from "react";
import {
  useGetMovementsByOrganization,
  useCreateStockMovement,
} from "../../hooks/useStockMovements";
import { useGetProducts } from "../../hooks/useProducts";
import {
  CreateStockMovementDto,
  StockMovementType,
} from "../../types/stockmovement.types";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { CustomAlertDialog } from "@/components/common/AlertDialog";
import { AlertDialogState } from "@/types/alertdialogstate.type";

const StockMovementsPage = () => {
  const [fromDate, setFromDate] = useState<string>("");
  const [toDate, setToDate] = useState<string>("");
  const { data: movements, isLoading } = useGetMovementsByOrganization(
    fromDate,
    toDate
  );
  const { data: products } = useGetProducts(true);
  const createMovement = useCreateStockMovement();

  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [alertDialog, setAlertDialog] = useState<AlertDialogState>({
    open: false,
    title: "",
    description: "",
  });

  const [formData, setFormData] = useState<CreateStockMovementDto>({
    productVariantId: "",
    movementType: StockMovementType.Adjustment,
    quantity: 0,
    notes: "",
    reference: "",
    unitCost: undefined,
  });

  const getMovementTypeBadge = (movementType: string) => {
    const badgeConfig = {
      InitialStock: { variant: "default", label: "Inicial" },
      Purchase: { variant: "default", label: "Compra" },
      Sale: { variant: "destructive", label: "Venta" },
      Return: { variant: "default", label: "Devolución" },
      Adjustment: { variant: "secondary", label: "Ajuste" },
      Loss: { variant: "destructive", label: "Pérdida" },
      Transfer: { variant: "secondary", label: "Transferencia" },
      SaleCancellation: { variant: "default", label: "Cancelación" },
    } as const;

    const config = badgeConfig[movementType as keyof typeof badgeConfig] || {
      variant: "default",
      label: movementType,
    };
    return <Badge variant={config.variant as any}>{config.label}</Badge>;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.productVariantId) {
      setAlertDialog({
        open: true,
        title: "Error",
        description: "Debe seleccionar una variante de producto",
      });
      return;
    }

    if (formData.quantity === 0) {
      setAlertDialog({
        open: true,
        title: "Error",
        description: "La cantidad no puede ser 0",
      });
      return;
    }

    try {
      await createMovement.mutateAsync(formData);
      setAlertDialog({
        open: true,
        title: "Éxito",
        description: "Movimiento de inventario registrado correctamente",
      });
      setIsAddModalOpen(false);
      setFormData({
        productVariantId: "",
        movementType: StockMovementType.Adjustment,
        quantity: 0,
        notes: "",
        reference: "",
        unitCost: undefined,
      });
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: "Error",
        description:
          error.response?.data?.message || "Error al registrar el movimiento",
      });
    }
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <p>Cargando movimientos de inventario...</p>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      <CustomAlertDialog
        open={alertDialog.open}
        title={alertDialog.title}
        description={alertDialog.description}
        onClose={() => setAlertDialog({ ...alertDialog, open: false })}
      />

      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <div>
              <CardTitle>Movimientos de Inventario</CardTitle>
              <CardDescription>
                Historial completo de movimientos de stock (ledger)
              </CardDescription>
            </div>
            <Button onClick={() => setIsAddModalOpen(true)}>
              Registrar Movimiento Manual
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <div className="flex gap-4 mb-6">
            <div className="flex-1">
              <Label htmlFor="fromDate">Desde</Label>
              <Input
                id="fromDate"
                type="date"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
              />
            </div>
            <div className="flex-1">
              <Label htmlFor="toDate">Hasta</Label>
              <Input
                id="toDate"
                type="date"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
              />
            </div>
            <div className="flex items-end">
              <Button
                variant="secondary"
                onClick={() => {
                  setFromDate("");
                  setToDate("");
                }}
              >
                Limpiar Filtros
              </Button>
            </div>
          </div>

          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Fecha</TableHead>
                <TableHead>Producto</TableHead>
                <TableHead>SKU</TableHead>
                <TableHead>Tipo</TableHead>
                <TableHead className="text-right">Cantidad</TableHead>
                <TableHead className="text-right">Stock Anterior</TableHead>
                <TableHead className="text-right">Stock Posterior</TableHead>
                <TableHead>Usuario</TableHead>
                <TableHead>Notas</TableHead>
                <TableHead>Referencia</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {movements && movements.length > 0 ? (
                movements.map((movement) => (
                  <TableRow key={movement.id}>
                    <TableCell>
                      {new Date(movement.createdAt).toLocaleDateString(
                        "es-ES",
                        {
                          year: "numeric",
                          month: "short",
                          day: "numeric",
                          hour: "2-digit",
                          minute: "2-digit",
                        }
                      )}
                    </TableCell>
                    <TableCell>{movement.productName}</TableCell>
                    <TableCell>{movement.variantSku}</TableCell>
                    <TableCell>
                      {getMovementTypeBadge(movement.movementType)}
                    </TableCell>
                    <TableCell
                      className={`text-right font-semibold ${
                        movement.quantity >= 0
                          ? "text-green-600"
                          : "text-red-600"
                      }`}
                    >
                      {movement.quantity > 0
                        ? `+${movement.quantity}`
                        : movement.quantity}
                    </TableCell>
                    <TableCell className="text-right">
                      {movement.stockBefore}
                    </TableCell>
                    <TableCell className="text-right font-semibold">
                      {movement.stockAfter}
                    </TableCell>
                    <TableCell className="text-sm text-gray-600">
                      {movement.createdByUserName || "Sistema"}
                    </TableCell>
                    <TableCell className="text-sm text-gray-600 max-w-xs truncate">
                      {movement.notes || "-"}
                    </TableCell>
                    <TableCell className="text-sm text-gray-600">
                      {movement.reference || "-"}
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell
                    colSpan={10}
                    className="text-center py-8 text-gray-500"
                  >
                    No hay movimientos de inventario registrados
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Add Stock Movement Modal */}
      <Dialog open={isAddModalOpen} onOpenChange={setIsAddModalOpen}>
        <DialogContent className="max-w-md">
          <form onSubmit={handleSubmit}>
            <DialogHeader>
              <DialogTitle>Registrar Movimiento de Inventario</DialogTitle>
              <DialogDescription>
                Registre ajustes manuales, compras, pérdidas u otros movimientos
              </DialogDescription>
            </DialogHeader>

            <div className="space-y-4 py-4">
              <div>
                <Label htmlFor="productVariant">Producto / Variante</Label>
                <select
                  id="productVariant"
                  className="w-full border border-gray-300 rounded-md p-2"
                  value={formData.productVariantId}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      productVariantId: e.target.value,
                    })
                  }
                  required
                >
                  <option value="">Seleccionar variante...</option>
                  {products?.map((product) =>
                    product.variants?.map((variant) => (
                      <option key={variant.id} value={variant.id}>
                        {product.name} - {variant.sku} (Stock:{" "}
                        {variant.stockQuantity})
                      </option>
                    ))
                  )}
                </select>
              </div>

              <div>
                <Label htmlFor="movementType">Tipo de Movimiento</Label>
                <select
                  id="movementType"
                  className="w-full border border-gray-300 rounded-md p-2"
                  value={formData.movementType}
                  onChange={(e) =>
                    setFormData({ ...formData, movementType: e.target.value })
                  }
                  required
                >
                  {/* <option value={StockMovementType.Adjustment}>Ajuste</option> */}
                  <option value={StockMovementType.Purchase}>Compra</option>
                  <option value={StockMovementType.Loss}>Pérdida</option>
                  <option value={StockMovementType.Return}>Devolución</option>
                  {/* <option value={StockMovementType.InitialStock}>Stock Inicial</option> */}
                  {/* <option value={StockMovementType.Transfer}>Transferencia</option> */}
                </select>
              </div>

              <div>
                <Label htmlFor="quantity">
                  Cantidad (positivo: entrada, negativo: salida)
                </Label>
                <Input
                  id="quantity"
                  type="number"
                  value={formData.quantity}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      quantity: parseInt(e.target.value) || 0,
                    })
                  }
                  placeholder="Ej: 10 o -5"
                  required
                />
              </div>

              <div>
                <Label htmlFor="unitCost">Costo Unitario (opcional)</Label>
                <Input
                  id="unitCost"
                  type="number"
                  step="0.01"
                  value={formData.unitCost || ""}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      unitCost: e.target.value
                        ? parseFloat(e.target.value)
                        : undefined,
                    })
                  }
                  placeholder="$0.00"
                />
              </div>

              <div>
                <Label htmlFor="reference">Referencia (opcional)</Label>
                <Input
                  id="reference"
                  value={formData.reference || ""}
                  onChange={(e) =>
                    setFormData({ ...formData, reference: e.target.value })
                  }
                  placeholder="Ej: Factura #123, Orden de compra #456"
                />
              </div>

              <div>
                <Label htmlFor="notes">Notas (opcional)</Label>
                <textarea
                  id="notes"
                  className="w-full border border-gray-300 rounded-md p-2 min-h-[80px]"
                  value={formData.notes || ""}
                  onChange={(e) =>
                    setFormData({ ...formData, notes: e.target.value })
                  }
                  placeholder="Descripción del movimiento..."
                />
              </div>
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="secondary"
                onClick={() => setIsAddModalOpen(false)}
              >
                Cancelar
              </Button>
              <Button type="submit" disabled={createMovement.isPending}>
                {createMovement.isPending ? "Guardando..." : "Registrar"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default StockMovementsPage;
