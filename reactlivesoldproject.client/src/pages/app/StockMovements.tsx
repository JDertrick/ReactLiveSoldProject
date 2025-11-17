import { useState, useMemo } from "react";
import {
  useGetMovementsByOrganization,
  useCreateStockMovement,
  usePostStockMovement,
  useUnpostStockMovement,
  useRejectStockMovement,
} from "../../hooks/useStockMovements";
import { useGetProducts } from "../../hooks/useProducts";
import { useLocations } from "../../hooks/useLocations";
import {
  CreateStockMovementDto,
  StockMovementDto,
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
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
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
import {
  Check,
  ChevronsUpDown,
  Package,
  DollarSign,
  Hash,
  TrendingUp,
  TrendingDown,
  Send,
  XCircle,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { toast } from "sonner";

const StockMovementsPage = () => {
  const [fromDate, setFromDate] = useState<string>("");
  const [toDate, setToDate] = useState<string>("");
  const { data: movements, isLoading } = useGetMovementsByOrganization(
    fromDate,
    toDate
  );
  const { data: products } = useGetProducts(true);
  const { locations, isLoading: isLoadingLocations } = useLocations();
  const createMovement = useCreateStockMovement();
  const postMovement = usePostStockMovement();
  const unpostMovement = useUnpostStockMovement();
  const rejectMovement = useRejectStockMovement();

  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [selectedMovement, setSelectedMovement] =
    useState<StockMovementDto | null>(null);
  const [isConfirmPostOpen, setIsConfirmPostOpen] = useState(false);
  const [isConfirmRejectOpen, setIsConfirmRejectOpen] = useState(false);
  const [openCombobox, setOpenCombobox] = useState(false);

  const [formData, setFormData] = useState<CreateStockMovementDto>({
    productVariantId: "",
    movementType: StockMovementType.Purchase,
    quantity: 0,
    notes: "",
    reference: "",
    unitCost: undefined,
    sourceLocationId: undefined,
    destinationLocationId: undefined,
  });

  // Flatten products and variants for the combobox
  const productVariantOptions = useMemo(() => {
    if (!products) return [];

    return products.flatMap(
      (product) =>
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
    if (!movements) return { total: 0, posted: 0, draft: 0, rejected: 0 };

    return {
      total: movements.length,
      posted: movements.filter((m) => m.isPosted).length,
      draft: movements.filter((m) => !m.isPosted && !m.isRejected).length,
      rejected: movements.filter((m) => m.isRejected).length,
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
      toast.error("Debe seleccionar una variante de producto");
      return;
    }

    if (formData.quantity === 0) {
      toast.error("La cantidad no puede ser 0");
      return;
    }

    // Validar que las compras tengan costo unitario
    if (
      formData.movementType === StockMovementType.Purchase &&
      !formData.unitCost
    ) {
      toast.error("Las compras deben incluir un costo unitario");
      return;
    }

    // Validar transferencias
    if (formData.movementType === StockMovementType.Transfer) {
      if (!formData.sourceLocationId || !formData.destinationLocationId) {
        toast.error(
          "Las transferencias requieren ubicación de origen y destino"
        );
        return;
      }
      if (formData.sourceLocationId === formData.destinationLocationId) {
        toast.error("La ubicación de origen y destino no pueden ser la misma");
        return;
      }
    }

    try {
      await createMovement.mutateAsync(formData);
      toast.success(
        "Movimiento creado como borrador. Debe postearlo para que afecte el inventario."
      );
      setIsAddModalOpen(false);
      setFormData({
        productVariantId: "",
        movementType: StockMovementType.Purchase,
        quantity: 0,
        notes: "",
        reference: "",
        unitCost: undefined,
        sourceLocationId: undefined,
        destinationLocationId: undefined,
      });
    } catch (error: any) {
      toast.error(
        error.response?.data?.message || "Error al registrar el movimiento"
      );
    }
  };

  const handlePostClick = (movement: StockMovementDto) => {
    setSelectedMovement(movement);
    setIsConfirmPostOpen(true);
  };

  const handleRejectClick = (movement: StockMovementDto) => {
    setSelectedMovement(movement);
    setIsConfirmRejectOpen(true);
  };

  const handleConfirmPost = async () => {
    if (!selectedMovement) return;
    try {
      await postMovement.mutateAsync(selectedMovement.id);
      toast.success("Movimiento posteado correctamente.");
      setIsConfirmPostOpen(false);
    } catch (error: any) {
      toast.error(
        error.response?.data?.message || "Error al postear el movimiento"
      );
    }
  };

  const handleConfirmReject = async () => {
    if (!selectedMovement) return;
    try {
      await rejectMovement.mutateAsync(selectedMovement.id);
      toast.success("Movimiento rechazado correctamente.");
      setIsConfirmRejectOpen(false);
    } catch (error: any) {
      toast.error(
        error.response?.data?.message || "Error al rechazar el movimiento"
      );
    }
  };

  const handleUnpostMovement = async (movementId: string) => {
    try {
      await unpostMovement.mutateAsync(movementId);
      toast.success("Movimiento desposteado correctamente.");
    } catch (error: any) {
      toast.error(
        error.response?.data?.message || "Error al despostear el movimiento"
      );
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
      {/* Page Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">
            Movimientos de Inventario
          </h1>
          <p className="text-gray-500 mt-2 text-sm">
            Sistema de ledger con costo promedio ponderado y posteo
          </p>
        </div>
        <Button
          onClick={() => setIsAddModalOpen(true)}
          size="lg"
          className="gap-2"
        >
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
            <CardTitle className="text-3xl text-green-600">
              {stats.posted}
            </CardTitle>
          </CardHeader>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Borradores</CardDescription>
            <CardTitle className="text-3xl text-orange-600">
              {stats.draft}
            </CardTitle>
          </CardHeader>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Rechazados</CardDescription>
            <CardTitle className="text-3xl text-red-600">
              {stats.rejected}
            </CardTitle>
          </CardHeader>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <div>
              <CardTitle className="text-xl">
                Historial de Movimientos
              </CardTitle>
              <CardDescription className="text-sm text-gray-500">
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
                <h3 className="font-semibold text-blue-900 mb-2">
                  Sistema de Posteo y Costo Promedio Ponderado
                </h3>
                <p className="text-sm text-blue-700">
                  Los movimientos se crean como <strong>borradores</strong> y
                  deben ser <strong>posteados</strong> para afectar el
                  inventario. Al postear compras con costo, el sistema calcula
                  automáticamente el <strong>costo promedio ponderado</strong>{" "}
                  del producto. Solo puede despostear el último movimiento
                  posteado de cada producto.
                </p>
              </div>
            </div>
          </div>

          {/* Filters */}
          <div className="bg-gray-50 rounded-lg p-4 mb-6">
            <div className="flex gap-4 items-end">
              <div className="flex-1">
                <Label htmlFor="fromDate" className="font-semibold">
                  Desde
                </Label>
                <Input
                  id="fromDate"
                  type="date"
                  value={fromDate}
                  onChange={(e) => setFromDate(e.target.value)}
                  className="mt-1"
                />
              </div>
              <div className="flex-1">
                <Label htmlFor="toDate" className="font-semibold">
                  Hasta
                </Label>
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

          {/* Desktop Table View */}
          <div className="hidden lg:block overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Fecha</TableHead>
                  <TableHead>Estado</TableHead>
                  <TableHead>Producto</TableHead>
                  <TableHead>SKU</TableHead>
                  <TableHead>Tipo</TableHead>
                  <TableHead>Ubicación</TableHead>
                  <TableHead className="text-right">Cantidad</TableHead>
                  <TableHead className="text-right">Costo Unit.</TableHead>
                  <TableHead>Usuario</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {movements && movements.length > 0 ? (
                  movements.map((movement, index) => (
                    <TableRow
                      key={movement.id}
                      className={cn(
                        index % 2 === 0 ? "bg-white" : "bg-gray-50",
                        movement.isPosted && "bg-green-50",
                        movement.isRejected && "bg-red-50"
                      )}
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
                        ) : movement.isRejected ? (
                          <Badge variant="destructive">Rechazado</Badge>
                        ) : (
                          <Badge variant="secondary">Borrador</Badge>
                        )}
                      </TableCell>
                      <TableCell>{movement.productName}</TableCell>
                      <TableCell>{movement.variantSku}</TableCell>
                      <TableCell>
                        {getMovementTypeBadge(movement.movementType)}
                      </TableCell>
                      <TableCell className="text-sm">
                        {movement.movementType === "Transfer" ? (
                          <div className="flex items-center gap-1">
                            <Badge variant="outline" className="text-xs">
                              {movement.sourceLocation?.name || "N/A"}
                            </Badge>
                            <span>→</span>
                            <Badge variant="outline" className="text-xs">
                              {movement.destinationLocation?.name || "N/A"}
                            </Badge>
                          </div>
                        ) : movement.destinationLocation ? (
                          <Badge variant="outline" className="text-xs">
                            {movement.destinationLocation.name}
                          </Badge>
                        ) : (
                          <span className="text-gray-400">-</span>
                        )}
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
                      <TableCell className="text-sm text-gray-600">
                        {movement.isPosted
                          ? movement.postedByUserName
                          : movement.isRejected
                          ? movement.rejectedByUserName
                          : movement.createdByUserName || "Sistema"}
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          {!movement.isPosted && !movement.isRejected && (
                            <>
                              <Button
                                size="sm"
                                variant="default"
                                onClick={() => handlePostClick(movement)}
                                disabled={postMovement.isPending}
                                className="gap-1 bg-green-600 hover:bg-green-700"
                              >
                                <Send className="w-3 h-3" />
                                Postear
                              </Button>
                              <Button
                                size="sm"
                                variant="destructive"
                                onClick={() => handleRejectClick(movement)}
                                disabled={rejectMovement.isPending}
                                className="gap-1"
                              >
                                <XCircle className="w-3 h-3" />
                                Rechazar
                              </Button>
                            </>
                          )}
                          {movement.isPosted &&
                            movement.movementType !== "Sale" &&
                            movement.movementType !== "SaleCancellation" && (
                              <Button
                                size="sm"
                                variant="outline"
                                onClick={() =>
                                  handleUnpostMovement(movement.id)
                                }
                                disabled={unpostMovement.isPending}
                                className="gap-1"
                              >
                                Despostear
                              </Button>
                            )}
                        </div>
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
          </div>

          {/* Mobile Card View */}
          <div className="lg:hidden space-y-4">
            {movements && movements.length > 0 ? (
              movements.map((movement) => (
                <div
                  key={movement.id}
                  className={cn(
                    "border rounded-lg p-4 space-y-3 bg-white shadow-sm",
                    movement.isPosted && "bg-green-50 border-green-200",
                    movement.isRejected && "bg-red-50 border-red-200"
                  )}
                >
                  {/* Header with Status and Type */}
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <div className="font-medium text-lg">
                        {movement.productName}
                      </div>
                      <div className="text-sm text-gray-500">
                        SKU: {movement.variantSku}
                      </div>
                    </div>
                    {movement.isPosted ? (
                      <Badge variant="default" className="bg-green-600">
                        Posteado
                      </Badge>
                    ) : movement.isRejected ? (
                      <Badge variant="destructive">Rechazado</Badge>
                    ) : (
                      <Badge variant="secondary">Borrador</Badge>
                    )}
                  </div>

                  {/* Movement Type and Date */}
                  <div className="flex gap-2 items-center border-t pt-3">
                    {getMovementTypeBadge(movement.movementType)}
                    <span className="text-xs text-gray-500">
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
                    </span>
                  </div>

                  {/* Quantity and Stock Info */}
                  <div className="grid grid-cols-2 gap-3 text-sm border-t pt-3">
                    <div>
                      <div className="text-gray-600">Cantidad</div>
                      <div
                        className={`font-bold text-lg ${
                          movement.quantity >= 0
                            ? "text-green-600"
                            : "text-red-600"
                        }`}
                      >
                        {movement.quantity > 0
                          ? `+${movement.quantity}`
                          : movement.quantity}
                      </div>
                    </div>
                    <div>
                      <div className="text-gray-600">Costo Unit.</div>
                      <div className="font-medium">
                        {movement.unitCost
                          ? `$${movement.unitCost.toFixed(2)}`
                          : "-"}
                      </div>
                    </div>
                  </div>

                  {/* Location Info */}
                  {(movement.sourceLocation ||
                    movement.destinationLocation) && (
                    <div className="text-sm border-t pt-3">
                      <span className="text-gray-600">Ubicación: </span>
                      {movement.movementType === "Transfer" ? (
                        <div className="flex items-center gap-1 mt-1">
                          <Badge variant="outline" className="text-xs">
                            {movement.sourceLocation?.name || "N/A"}
                          </Badge>
                          <span>→</span>
                          <Badge variant="outline" className="text-xs">
                            {movement.destinationLocation?.name || "N/A"}
                          </Badge>
                        </div>
                      ) : (
                        <Badge variant="outline" className="text-xs ml-1">
                          {movement.destinationLocation?.name}
                        </Badge>
                      )}
                    </div>
                  )}

                  {/* User Info */}
                  <div className="text-sm border-t pt-3">
                    <span className="text-gray-600">Usuario: </span>
                    <span className="text-gray-700">
                      {movement.isPosted
                        ? movement.postedByUserName
                        : movement.isRejected
                        ? movement.rejectedByUserName
                        : movement.createdByUserName || "Sistema"}
                    </span>
                  </div>

                  {/* Actions */}
                  <div className="border-t pt-3 flex gap-2">
                    {!movement.isPosted && !movement.isRejected && (
                      <>
                        <Button
                          size="sm"
                          variant="default"
                          className="w-full gap-1 bg-green-600 hover:bg-green-700"
                          onClick={() => handlePostClick(movement)}
                          disabled={postMovement.isPending}
                        >
                          <Send className="w-3 h-3" />
                          Postear
                        </Button>
                        <Button
                          size="sm"
                          variant="destructive"
                          className="w-full gap-1"
                          onClick={() => handleRejectClick(movement)}
                          disabled={rejectMovement.isPending}
                        >
                          <XCircle className="w-3 h-3" />
                          Rechazar
                        </Button>
                      </>
                    )}
                    {movement.isPosted &&
                      movement.movementType !== "Sale" &&
                      movement.movementType !== "SaleCancellation" && (
                        <Button
                          size="sm"
                          variant="outline"
                          className="w-full"
                          onClick={() => handleUnpostMovement(movement.id)}
                          disabled={unpostMovement.isPending}
                        >
                          Despostear
                        </Button>
                      )}
                  </div>
                </div>
              ))
            ) : (
              <div className="text-center py-8 text-gray-500 border rounded-lg">
                No hay movimientos de inventario registrados
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      <AlertDialog open={isConfirmPostOpen} onOpenChange={setIsConfirmPostOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              ¿Está seguro que desea postear este movimiento?
            </AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción afectará permanentemente el inventario y no se puede
              deshacer directamente (requeriría un movimiento de ajuste).
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleConfirmPost}
              disabled={postMovement.isPending}
            >
              {postMovement.isPending ? "Posteando..." : "Postear Movimiento"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <AlertDialog
        open={isConfirmRejectOpen}
        onOpenChange={setIsConfirmRejectOpen}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              ¿Está seguro que desea rechazar este movimiento?
            </AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción marcará el movimiento como rechazado y no podrá ser
              posteado. Esta acción no se puede deshacer.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleConfirmReject}
              disabled={rejectMovement.isPending}
            >
              {rejectMovement.isPending
                ? "Rechazando..."
                : "Rechazar Movimiento"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

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
                Los movimientos se crean como <strong>borrador</strong>. Deberá
                postearlos para que afecten el inventario.
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
                          <span className="font-semibold">
                            {selectedVariant.productName}
                          </span>
                          <span className="text-xs text-gray-500">
                            SKU: {selectedVariant.sku} | Stock:{" "}
                            {selectedVariant.stock} | Costo Prom: $
                            {selectedVariant.averageCost.toFixed(2)}
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
                        <CommandEmpty>
                          No se encontraron productos.
                        </CommandEmpty>
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
                                <span className="font-semibold">
                                  {variant.productName}
                                </span>
                                <span className="text-xs text-gray-500">
                                  SKU: {variant.sku} | Stock: {variant.stock} |
                                  Precio: ${variant.price.toFixed(2)} | Costo
                                  Prom: ${variant.averageCost.toFixed(2)}
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
                        <p className="font-bold text-lg">
                          {selectedVariant.stock}
                        </p>
                      </div>
                      <div>
                        <span className="text-gray-600">Precio Venta:</span>
                        <p className="font-bold text-lg">
                          ${selectedVariant.price.toFixed(2)}
                        </p>
                      </div>
                      <div>
                        <span className="text-gray-600">Costo Promedio:</span>
                        <p className="font-bold text-lg">
                          ${selectedVariant.averageCost.toFixed(2)}
                        </p>
                      </div>
                    </div>
                  </div>
                )}
              </div>

              <div className="grid grid-cols-2 gap-4">
                {/* Movement Type */}
                <div className="space-y-2">
                  <Label className="text-base font-semibold">
                    Tipo de Movimiento
                  </Label>
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
                      {locations && locations.length >= 2 && (
                        <SelectItem value={StockMovementType.Transfer}>
                          <div className="flex items-center gap-2">
                            <Package className="w-4 h-4 text-purple-600" />
                            Transferencia
                          </div>
                        </SelectItem>
                      )}
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
                      <Badge variant="destructive" className="ml-2">
                        Requerido
                      </Badge>
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
                    required={
                      formData.movementType === StockMovementType.Purchase
                    }
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

              {/* Location Selectors */}
              {formData.movementType === StockMovementType.Transfer ? (
                // Para transferencias: mostrar origen y destino
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label className="text-base font-semibold flex items-center gap-2">
                      <Package className="w-4 h-4" />
                      Ubicación Origen
                      <Badge variant="destructive" className="ml-2">
                        Requerido
                      </Badge>
                    </Label>
                    <Select
                      value={formData.sourceLocationId || ""}
                      onValueChange={(value) =>
                        setFormData({ ...formData, sourceLocationId: value })
                      }
                    >
                      <SelectTrigger className="h-12">
                        <SelectValue placeholder="Seleccionar origen..." />
                      </SelectTrigger>
                      <SelectContent>
                        {locations?.map((location) => (
                          <SelectItem key={location.id} value={location.id}>
                            {location.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label className="text-base font-semibold flex items-center gap-2">
                      <Package className="w-4 h-4" />
                      Ubicación Destino
                      <Badge variant="destructive" className="ml-2">
                        Requerido
                      </Badge>
                    </Label>
                    <Select
                      value={formData.destinationLocationId || ""}
                      onValueChange={(value) =>
                        setFormData({
                          ...formData,
                          destinationLocationId: value,
                        })
                      }
                    >
                      <SelectTrigger className="h-12">
                        <SelectValue placeholder="Seleccionar destino..." />
                      </SelectTrigger>
                      <SelectContent>
                        {locations?.map((location) => (
                          <SelectItem key={location.id} value={location.id}>
                            {location.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                </div>
              ) : (
                // Para otros movimientos: solo ubicación de destino
                <div className="space-y-2">
                  <Label className="text-base font-semibold flex items-center gap-2">
                    <Package className="w-4 h-4" />
                    Ubicación
                  </Label>
                  <Select
                    value={formData.destinationLocationId || ""}
                    onValueChange={(value) =>
                      setFormData({ ...formData, destinationLocationId: value })
                    }
                  >
                    <SelectTrigger className="h-12">
                      <SelectValue placeholder="Seleccionar ubicación (opcional)..." />
                    </SelectTrigger>
                    <SelectContent>
                      {locations?.map((location) => (
                        <SelectItem key={location.id} value={location.id}>
                          {location.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-gray-500">
                    💡 Especifica donde ocurre el movimiento
                  </p>
                </div>
              )}

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
