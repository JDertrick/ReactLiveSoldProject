import { useState, useMemo } from "react";
import {
  useGetMovementsByOrganization,
  useCreateStockMovement,
  usePostStockMovement,
  useUnpostStockMovement,
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
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Check, ChevronsUpDown, Package, DollarSign, Hash, TrendingUp, TrendingDown } from "lucide-react";
import { cn } from "@/lib/utils";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

const StockMovementsPage = () => {
  const [fromDate, setFromDate] = useState<string>("");
  const [toDate, setToDate] = useState<string>("");
  const { data: movements, isLoading } = useGetMovementsByOrganization(
    fromDate,
    toDate
  );
  const { data: products } = useGetProducts(true);
  const createMovement = useCreateStockMovement();
  const postMovement = usePostStockMovement();
  const unpostMovement = useUnpostStockMovement();

  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [alertDialog, setAlertDialog] = useState<AlertDialogState>({
    open: false,
    title: "",
    description: "",
  });
  const [openCombobox, setOpenCombobox] = useState(false);

  const [formData, setFormData] = useState<CreateStockMovementDto>({
    productVariantId: "",
    movementType: StockMovementType.Purchase,
    quantity: 0,
    notes: "",
    reference: "",
    unitCost: undefined,
  });

  // Flatten products and variants for the combobox
  const productVariantOptions = useMemo(() => {
    if (!products) return [];

    return products.flatMap((product) =>
      product.variants?.map((variant) => ({
        value: variant.id,
        label: `${product.name} - ${variant.sku || "Sin SKU"}`,
        productName: product.name,
        sku: variant.sku || "Sin SKU",
        stock: variant.stockQuantity,
        price: variant.price,
        averageCost: variant.averageCost,
      })) || []
    );
  }, [products]);

  const selectedVariant = productVariantOptions.find(
    (v) => v.value === formData.productVariantId
  );

  // Calculate statistics - MUST be before any conditional returns
  const stats = useMemo(() => {
    if (!movements) return { total: 0, posted: 0, draft: 0, purchases: 0 };

    return {
      total: movements.length,
      posted: movements.filter(m => m.isPosted).length,
      draft: movements.filter(m => !m.isPosted).length,
      purchases: movements.filter(m => m.movementType === 'Purchase').length,
    };
  }, [movements]);

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

    // Validar que las compras tengan costo unitario
    if (formData.movementType === StockMovementType.Purchase && !formData.unitCost) {
      setAlertDialog({
        open: true,
        title: "Error",
        description: "Las compras deben incluir un costo unitario",
      });
      return;
    }

    try {
      await createMovement.mutateAsync(formData);
      setAlertDialog({
        open: true,
        title: "Éxito",
        description: "Movimiento creado como borrador. Debe postearlo para que afecte el inventario.",
      });
      setIsAddModalOpen(false);
      setFormData({
        productVariantId: "",
        movementType: StockMovementType.Purchase,
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

  const handlePostMovement = async (movementId: string) => {
    try {
      await postMovement.mutateAsync(movementId);
      setAlertDialog({
        open: true,
        title: "Éxito",
        description: "Movimiento posteado correctamente. El inventario ha sido actualizado.",
      });
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: "Error",
        description:
          error.response?.data?.message || "Error al postear el movimiento",
      });
    }
  };

  const handleUnpostMovement = async (movementId: string) => {
    try {
      await unpostMovement.mutateAsync(movementId);
      setAlertDialog({
        open: true,
        title: "Éxito",
        description: "Movimiento desposteado correctamente. El inventario ha sido revertido.",
      });
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: "Error",
        description:
          error.response?.data?.message || "Error al despostear el movimiento",
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

      {/* Page Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Movimientos de Inventario</h1>
          <p className="text-gray-500 mt-1">
            Sistema de ledger con costo promedio ponderado y posteo
          </p>
        </div>
        <Button onClick={() => setIsAddModalOpen(true)} size="lg" className="gap-2">
          <Package className="w-4 h-4" />
          Nuevo Movimiento
        </Button>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Total Movimientos</CardDescription>
            <CardTitle className="text-3xl">{stats.total}</CardTitle>
          </CardHeader>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Posteados</CardDescription>
            <CardTitle className="text-3xl text-green-600">{stats.posted}</CardTitle>
          </CardHeader>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Borradores</CardDescription>
            <CardTitle className="text-3xl text-orange-600">{stats.draft}</CardTitle>
          </CardHeader>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Compras</CardDescription>
            <CardTitle className="text-3xl text-blue-600">{stats.purchases}</CardTitle>
          </CardHeader>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <div>
              <CardTitle className="text-xl">Historial de Movimientos</CardTitle>
              <CardDescription>
                Ledger completo de entradas y salidas de inventario
              </CardDescription>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {/* Info Banner */}
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-4">
            <div className="flex items-start gap-3">
              <div className="bg-blue-100 rounded-full p-2">
                <Package className="w-5 h-5 text-blue-600" />
              </div>
              <div className="flex-1">
                <h3 className="font-semibold text-blue-900 mb-1">
                  Sistema de Posteo y Costo Promedio Ponderado
                </h3>
                <p className="text-sm text-blue-700">
                  Los movimientos se crean como <strong>borradores</strong> y deben ser <strong>posteados</strong> para afectar el inventario.
                  Al postear compras con costo, el sistema calcula automáticamente el <strong>costo promedio ponderado</strong> del producto.
                  Solo puede despostear el último movimiento posteado de cada producto.
                </p>
              </div>
            </div>
          </div>

          {/* Filters */}
          <div className="bg-gray-50 rounded-lg p-4 mb-6">
            <div className="flex gap-4 items-end">
              <div className="flex-1">
                <Label htmlFor="fromDate" className="font-semibold">Desde</Label>
                <Input
                  id="fromDate"
                  type="date"
                  value={fromDate}
                  onChange={(e) => setFromDate(e.target.value)}
                  className="mt-1"
                />
              </div>
              <div className="flex-1">
                <Label htmlFor="toDate" className="font-semibold">Hasta</Label>
                <Input
                  id="toDate"
                  type="date"
                  value={toDate}
                  onChange={(e) => setToDate(e.target.value)}
                  className="mt-1"
                />
              </div>
              <Button
                variant="outline"
                onClick={() => {
                  setFromDate("");
                  setToDate("");
                }}
              >
                Limpiar
              </Button>
            </div>
          </div>

          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Fecha</TableHead>
                <TableHead>Estado</TableHead>
                <TableHead>Producto</TableHead>
                <TableHead>SKU</TableHead>
                <TableHead>Tipo</TableHead>
                <TableHead className="text-right">Cantidad</TableHead>
                <TableHead className="text-right">Costo Unit.</TableHead>
                <TableHead className="text-right">Stock Ant.</TableHead>
                <TableHead className="text-right">Stock Post.</TableHead>
                <TableHead>Usuario</TableHead>
                <TableHead>Notas</TableHead>
                <TableHead>Referencia</TableHead>
                <TableHead className="text-right">Acciones</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {movements && movements.length > 0 ? (
                movements.map((movement, index) => (
                  <TableRow
                    key={movement.id}
                    className={index % 2 === 0 ? "bg-white" : "bg-gray-50"}
                  >
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
                    <TableCell>
                      {movement.isPosted ? (
                        <Badge variant="default" className="bg-green-600">
                          Posteado
                        </Badge>
                      ) : (
                        <Badge variant="secondary">Borrador</Badge>
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
                    <TableCell className="text-right text-sm">
                      {movement.unitCost
                        ? `$${movement.unitCost.toFixed(2)}`
                        : "-"}
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
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        {!movement.isPosted && (
                          <Button
                            size="sm"
                            variant="default"
                            onClick={() => handlePostMovement(movement.id)}
                            disabled={postMovement.isPending}
                            className="gap-1"
                          >
                            <Check className="w-3 h-3" />
                            Postear
                          </Button>
                        )}
                        {movement.isPosted &&
                          movement.movementType !== "Sale" &&
                          movement.movementType !== "SaleCancellation" && (
                            <Button
                              size="sm"
                              variant="outline"
                              onClick={() => handleUnpostMovement(movement.id)}
                              disabled={unpostMovement.isPending}
                              className="gap-1"
                            >
                              Despostear
                            </Button>
                          )}
                        {movement.isPosted &&
                          (movement.movementType === "Sale" ||
                            movement.movementType === "SaleCancellation") && (
                            <span className="text-xs text-gray-400 italic px-2">
                              Automático
                            </span>
                          )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell
                    colSpan={13}
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

      {/* Add Stock Movement Modal - Redesigned */}
      <Dialog open={isAddModalOpen} onOpenChange={setIsAddModalOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <form onSubmit={handleSubmit}>
            <DialogHeader>
              <DialogTitle className="text-2xl font-bold flex items-center gap-2">
                <Package className="w-6 h-6" />
                Registrar Movimiento de Inventario
              </DialogTitle>
              <DialogDescription className="text-base">
                Los movimientos se crean como <strong>borrador</strong>. Deberá postearlos para que afecten el inventario.
              </DialogDescription>
            </DialogHeader>

            <div className="space-y-6 py-6">
              {/* Product Variant Search */}
              <div className="space-y-2">
                <Label className="text-base font-semibold flex items-center gap-2">
                  <Package className="w-4 h-4" />
                  Producto / Variante
                </Label>
                <Popover open={openCombobox} onOpenChange={setOpenCombobox}>
                  <PopoverTrigger asChild>
                    <Button
                      variant="outline"
                      role="combobox"
                      aria-expanded={openCombobox}
                      className="w-full justify-between h-auto min-h-[50px]"
                    >
                      {selectedVariant ? (
                        <div className="flex flex-col items-start gap-1 text-left">
                          <span className="font-semibold">{selectedVariant.productName}</span>
                          <span className="text-xs text-gray-500">
                            SKU: {selectedVariant.sku} | Stock: {selectedVariant.stock} |
                            Costo Prom: ${selectedVariant.averageCost.toFixed(2)}
                          </span>
                        </div>
                      ) : (
                        "Buscar producto..."
                      )}
                      <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-[600px] p-0">
                    <Command>
                      <CommandInput placeholder="Buscar producto o SKU..." />
                      <CommandList>
                        <CommandEmpty>No se encontraron productos.</CommandEmpty>
                        <CommandGroup>
                          {productVariantOptions.map((variant) => (
                            <CommandItem
                              key={variant.value}
                              value={variant.label}
                              onSelect={() => {
                                setFormData({
                                  ...formData,
                                  productVariantId: variant.value,
                                });
                                setOpenCombobox(false);
                              }}
                              className="py-3"
                            >
                              <Check
                                className={cn(
                                  "mr-2 h-4 w-4",
                                  formData.productVariantId === variant.value
                                    ? "opacity-100"
                                    : "opacity-0"
                                )}
                              />
                              <div className="flex flex-col gap-1">
                                <span className="font-semibold">{variant.productName}</span>
                                <span className="text-xs text-gray-500">
                                  SKU: {variant.sku} | Stock: {variant.stock} |
                                  Precio: ${variant.price.toFixed(2)} |
                                  Costo Prom: ${variant.averageCost.toFixed(2)}
                                </span>
                              </div>
                            </CommandItem>
                          ))}
                        </CommandGroup>
                      </CommandList>
                    </Command>
                  </PopoverContent>
                </Popover>
                {selectedVariant && (
                  <div className="bg-blue-50 border border-blue-200 rounded-md p-3 mt-2">
                    <div className="grid grid-cols-3 gap-2 text-sm">
                      <div>
                        <span className="text-gray-600">Stock Actual:</span>
                        <p className="font-bold text-lg">{selectedVariant.stock}</p>
                      </div>
                      <div>
                        <span className="text-gray-600">Precio Venta:</span>
                        <p className="font-bold text-lg">${selectedVariant.price.toFixed(2)}</p>
                      </div>
                      <div>
                        <span className="text-gray-600">Costo Promedio:</span>
                        <p className="font-bold text-lg">${selectedVariant.averageCost.toFixed(2)}</p>
                      </div>
                    </div>
                  </div>
                )}
              </div>

              <div className="grid grid-cols-2 gap-4">
                {/* Movement Type */}
                <div className="space-y-2">
                  <Label className="text-base font-semibold">Tipo de Movimiento</Label>
                  <Select
                    value={formData.movementType}
                    onValueChange={(value: string) =>
                      setFormData({ ...formData, movementType: value })
                    }
                  >
                    <SelectTrigger className="h-12">
                      <SelectValue placeholder="Seleccionar tipo..." />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value={StockMovementType.Purchase}>
                        <div className="flex items-center gap-2">
                          <TrendingUp className="w-4 h-4 text-green-600" />
                          Compra
                        </div>
                      </SelectItem>
                      <SelectItem value={StockMovementType.Loss}>
                        <div className="flex items-center gap-2">
                          <TrendingDown className="w-4 h-4 text-red-600" />
                          Pérdida
                        </div>
                      </SelectItem>
                      <SelectItem value={StockMovementType.Return}>
                        <div className="flex items-center gap-2">
                          <TrendingUp className="w-4 h-4 text-blue-600" />
                          Devolución
                        </div>
                      </SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                {/* Quantity */}
                <div className="space-y-2">
                  <Label className="text-base font-semibold flex items-center gap-2">
                    <Hash className="w-4 h-4" />
                    Cantidad
                  </Label>
                  <Input
                    type="number"
                    value={formData.quantity}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        quantity: parseInt(e.target.value) || 0,
                      })
                    }
                    placeholder="Ej: 10"
                    className="h-12 text-lg"
                    required
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                {/* Unit Cost */}
                <div className="space-y-2">
                  <Label className="text-base font-semibold flex items-center gap-2">
                    <DollarSign className="w-4 h-4" />
                    Costo Unitario
                    {formData.movementType === StockMovementType.Purchase && (
                      <Badge variant="destructive" className="ml-2">Requerido</Badge>
                    )}
                  </Label>
                  <Input
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
                    className="h-12 text-lg"
                    required={formData.movementType === StockMovementType.Purchase}
                  />
                  <p className="text-xs text-gray-500">
                    💡 Se usa para calcular el costo promedio ponderado
                  </p>
                </div>

                {/* Reference */}
                <div className="space-y-2">
                  <Label className="text-base font-semibold">Referencia</Label>
                  <Input
                    value={formData.reference || ""}
                    onChange={(e) =>
                      setFormData({ ...formData, reference: e.target.value })
                    }
                    placeholder="Ej: Factura #123"
                    className="h-12"
                  />
                </div>
              </div>

              {/* Notes */}
              <div className="space-y-2">
                <Label className="text-base font-semibold">Notas</Label>
                <Textarea
                  value={formData.notes || ""}
                  onChange={(e) =>
                    setFormData({ ...formData, notes: e.target.value })
                  }
                  placeholder="Descripción del movimiento (opcional)..."
                  className="min-h-[100px] resize-none"
                />
              </div>
            </div>

            <DialogFooter className="gap-2">
              <Button
                type="button"
                variant="outline"
                onClick={() => setIsAddModalOpen(false)}
                className="px-6"
              >
                Cancelar
              </Button>
              <Button
                type="submit"
                disabled={createMovement.isPending}
                className="px-6"
              >
                {createMovement.isPending ? "Guardando..." : "Crear Movimiento"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default StockMovementsPage;
