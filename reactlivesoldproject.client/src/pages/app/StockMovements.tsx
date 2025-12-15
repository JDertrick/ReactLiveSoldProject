import { useState, useMemo } from "react";
import {
  useGetMovementsByOrganization,
  useCreateStockMovement,
  usePostStockMovement,
  useUnpostStockMovement,
  useRejectStockMovement,
} from "../../hooks/useStockMovements";
import { useGetProducts } from "../../hooks/useProducts";
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
import { Input } from "@/components/ui/input";
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
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Calendar } from "@/components/ui/calendar";
import { format } from "date-fns";
import {
  Package,
  Send,
  XCircle,
  MoreHorizontal,
  Calendar as CalendarIcon,
  Filter,
  CheckCircle2,
  XCircle as XCircleIcon,
  Clock,
  X as XIcon,
} from "lucide-react";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { cn } from "@/lib/utils";
import { toast } from "sonner";
import StockMovementComponentFormDialog from "@/components/stockmovement/StockMovementComponentFormDialog";

const StatCard = ({
  title,
  value,
  subtext,
  icon,
  colorClass,
  circleColorClass,
}: {
  title: string;
  value: string | number;
  subtext: string;
  icon: React.ReactNode;
  colorClass: string;
  circleColorClass: string;
}) => (
  <Card className="relative overflow-hidden">
    <div
      className={`absolute -top-4 -right-4 w-24 h-24 rounded-full ${circleColorClass} opacity-20`}
    ></div>
    <CardHeader>
      <div className="flex items-center gap-4">
        <div className={`rounded-full p-3 ${colorClass} bg-opacity-10`}>
          {icon}
        </div>
        <div>
          <CardDescription>{title}</CardDescription>
          <CardTitle className={`text-3xl font-bold ${colorClass}`}>
            {value}
          </CardTitle>
        </div>
      </div>
      {subtext && (
        <p className="text-xs text-muted-foreground mt-2">{subtext}</p>
      )}
    </CardHeader>
  </Card>
);

const StockMovementsPage = () => {
  const [fromDate, setFromDate] = useState<Date | undefined>(undefined);
  const [toDate, setToDate] = useState<Date | undefined>(undefined);
  const [searchTerm, setSearchTerm] = useState("");
  const { data: movements, isLoading } = useGetMovementsByOrganization(
    fromDate?.toISOString(),
    toDate?.toISOString()
  );
  const { data: productsPagedResult } = useGetProducts(1, 9999, "all", ""); // Fetch all products for stock movements
  const createMovement = useCreateStockMovement();
  const postMovement = usePostStockMovement();
  const unpostMovement = useUnpostStockMovement();
  const rejectMovement = useRejectStockMovement();

  const productVariants = useMemo(
    () => productsPagedResult?.items ?? [],
    [productsPagedResult]
  );

  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [selectedMovement, setSelectedMovement] =
    useState<StockMovementDto | null>(null);
  const [isConfirmPostOpen, setIsConfirmPostOpen] = useState(false);
  const [isConfirmRejectOpen, setIsConfirmRejectOpen] = useState(false);

  const filteredMovements = useMemo(() => {
    if (!movements) return [];
    return movements.filter(
      (m) =>
        m.productName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        m.variantSku.toLowerCase().includes(searchTerm.toLowerCase()) ||
        m.createdByUserName?.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [movements, searchTerm]);

  const stats = useMemo(() => {
    if (!movements)
      return { total: 0, posted: 0, draft: 0, rejected: 0, today: 0 };

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return {
      total: movements.length,
      posted: movements.filter((m) => m.isPosted).length,
      draft: movements.filter((m) => !m.isPosted && !m.isRejected).length,
      rejected: movements.filter((m) => m.isRejected).length,
      today: movements.filter((m) => new Date(m.createdAt) >= today).length,
    };
  }, [movements]);

  const getMovementTypeDisplay = (movementType: string) => {
    const config = {
      InitialStock: {
        label: "Inicial",
        className: "bg-blue-100 text-blue-800",
      },
      Purchase: { label: "Compra", className: "bg-purple-100 text-purple-800" },
      Sale: { label: "Venta", className: "bg-red-100 text-red-800" },
      Return: {
        label: "Devolución",
        className: "bg-yellow-100 text-yellow-800",
      },
      Adjustment: {
        label: "Ajuste",
        className: "bg-indigo-100 text-indigo-800",
      },
      Loss: { label: "Pérdida", className: "bg-pink-100 text-pink-800" },
      Transfer: {
        label: "Transferencia",
        className: "bg-gray-100 text-gray-800",
      },
      SaleCancellation: {
        label: "Cancelación",
        className: "bg-green-100 text-green-800",
      },
    }[movementType] || {
      label: movementType,
      className: "bg-gray-100 text-gray-800",
    };

    return (
      <Badge
        variant="outline"
        className={`border-transparent ${config.className}`}
      >
        {config.label}
      </Badge>
    );
  };

  const handleSubmit = async (formData: CreateStockMovementDto) => {
    if (!formData.productVariantId) {
      toast.error("Debe seleccionar una variante de producto");
      return;
    }
    if (formData.quantity === 0) {
      toast.error("La cantidad no puede ser 0");
      return;
    }
    if (
      formData.movementType === StockMovementType.Purchase &&
      !formData.unitCost
    ) {
      toast.error("Las compras deben incluir un costo unitario");
      return;
    }
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
      <div className="flex justify-center items-center h-64">
        <p>Cargando movimientos...</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Libro de Movimientos
          </h1>
          <p className="text-muted-foreground mt-1">
            Gestiona el flujo de entrada y salida con control de costos.
          </p>
        </div>
        <Button
          onClick={() => setIsAddModalOpen(true)}
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
          Agregar ajuste
        </Button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatCard
          title="TOTAL MOVIMIENTOS"
          value={stats.total}
          subtext={`+${stats.today} hoy`}
          icon={<Package className="w-6 h-6" />}
          colorClass="text-blue-500"
          circleColorClass="bg-blue-500"
        />
        <StatCard
          title="POSTEADOS"
          value={stats.posted}
          icon={<CheckCircle2 className="w-6 h-6" />}
          colorClass="text-green-500"
          circleColorClass="bg-green-500"
          subtext=" "
        />
        <StatCard
          title="BORRADORES"
          value={stats.draft}
          icon={<Clock className="w-6 h-6" />}
          colorClass="text-orange-500"
          circleColorClass="bg-orange-500"
          subtext="Pendientes"
        />
        <StatCard
          title="RECHAZADOS"
          value={stats.rejected}
          icon={<XCircleIcon className="w-6 h-6" />}
          colorClass="text-red-500"
          circleColorClass="bg-red-500"
          subtext=" "
        />
      </div>

      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row justify-between items-center gap-4 mb-6">
            <div className="flex items-center gap-2">
              <Popover>
                <PopoverTrigger asChild>
                  <div className="relative w-[180px]">
                    <CalendarIcon className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                      className={cn(
                        "w-full justify-start text-left font-normal pl-10",
                        !fromDate && "text-muted-foreground"
                      )}
                      value={fromDate ? format(fromDate, "dd/MM/yyyy") : ""}
                      placeholder="Desde"
                      readOnly
                    />
                    {fromDate && (
                      <Button
                        variant="ghost"
                        size="icon"
                        className="absolute right-1 top-1/2 -translate-y-1/2 h-6 w-6"
                        onClick={(e) => {
                          e.stopPropagation();
                          setFromDate(undefined);
                        }}
                      >
                        <XIcon className="h-4 w-4" />
                      </Button>
                    )}
                  </div>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0">
                  <Calendar
                    mode="single"
                    selected={fromDate}
                    onSelect={setFromDate}
                    initialFocus
                  />
                </PopoverContent>
              </Popover>
              <span>→</span>
              <Popover>
                <PopoverTrigger asChild>
                  <div className="relative w-[180px]">
                    <CalendarIcon className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                      className={cn(
                        "w-full justify-start text-left font-normal pl-10",
                        !toDate && "text-muted-foreground"
                      )}
                      value={toDate ? format(toDate, "dd/MM/yyyy") : ""}
                      placeholder="Hasta"
                      readOnly
                    />
                    {toDate && (
                      <Button
                        variant="ghost"
                        size="icon"
                        className="absolute right-1 top-1/2 -translate-y-1/2 h-6 w-6"
                        onClick={(e) => {
                          e.stopPropagation();
                          setToDate(undefined);
                        }}
                      >
                        <XIcon className="h-4 w-4" />
                      </Button>
                    )}
                  </div>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0">
                  <Calendar
                    mode="single"
                    selected={toDate}
                    onSelect={setToDate}
                    initialFocus
                  />
                </PopoverContent>
              </Popover>
            </div>
            <div className="flex items-center gap-2 w-full sm:w-auto">
              <Input
                placeholder="Buscar SKU, usuario..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full sm:w-64"
              />
              <Button
                variant="outline"
                size="icon"
                onClick={() => {
                  setFromDate(undefined);
                  setToDate(undefined);
                  setSearchTerm("");
                }}
              >
                <Filter className="h-4 w-4" />
              </Button>
            </div>
          </div>

          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>NO. MOVIMIENTO</TableHead>
                  <TableHead>PRODUCTO / SKU</TableHead>
                  <TableHead>ESTADO</TableHead>
                  <TableHead>TIPO</TableHead>
                  <TableHead>UBICACIÓN</TableHead>
                  <TableHead>FECHA & USUARIO</TableHead>
                  <TableHead className="text-right">CANTIDAD</TableHead>
                  <TableHead className="text-right">COSTO</TableHead>
                  <TableHead className="text-right">ACCIÓN</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredMovements && filteredMovements.length > 0 ? (
                  filteredMovements.map((movement) => (
                    <TableRow key={movement.id} className="hover:bg-muted/50">
                      <TableCell>
                        <div className="text-sm font-mono text-muted-foreground">
                          {movement.movementNumber || "-"}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-3">
                          <div className="bg-muted p-2 rounded-md">
                            <Package className="h-5 w-5 text-muted-foreground" />
                          </div>
                          <div>
                            <div className="font-medium">
                              {movement.productName}
                            </div>
                            <div className="text-xs text-muted-foreground">
                              {movement.variantSku}
                            </div>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        {movement.isPosted ? (
                          <Badge
                            variant="outline"
                            className="text-green-600 border-green-600/50 bg-green-600/10"
                          >
                            <CheckCircle2 className="w-3.5 h-3.5 mr-1" />
                            Posteado
                          </Badge>
                        ) : movement.isRejected ? (
                          <Badge
                            variant="outline"
                            className="text-red-600 border-red-600/50 bg-red-600/10"
                          >
                            <XCircleIcon className="w-3.5 h-3.5 mr-1" />
                            Rechazado
                          </Badge>
                        ) : (
                          <Badge
                            variant="outline"
                            className="text-orange-600 border-orange-600/50 bg-orange-600/10"
                          >
                            <Clock className="w-3.5 h-3.5 mr-1" />
                            Borrador
                          </Badge>
                        )}
                      </TableCell>
                      <TableCell>
                        {getMovementTypeDisplay(movement.movementType)}
                      </TableCell>
                      <TableCell>
                        {movement.movementType === "Transfer" ? (
                          <div className="text-sm">
                            <div className="flex items-center gap-1">
                              <span className="text-muted-foreground">
                                {movement.sourceLocation?.name || "Sin ubicación"}
                              </span>
                              <span className="text-muted-foreground">→</span>
                              <span className="font-medium">
                                {movement.destinationLocation?.name || "Sin ubicación"}
                              </span>
                            </div>
                          </div>
                        ) : (
                          <div className="text-sm">
                            {movement.destinationLocation?.name ||
                             movement.sourceLocation?.name ||
                             <span className="text-muted-foreground">-</span>}
                          </div>
                        )}
                      </TableCell>
                      <TableCell>
                        <div className="font-medium">
                          {new Date(movement.createdAt).toLocaleDateString(
                            "es-ES",
                            {
                              month: "short",
                              day: "numeric",
                              hour: "2-digit",
                              minute: "2-digit",
                            }
                          )}
                        </div>
                        <div className="text-xs text-muted-foreground">
                          {movement.isPosted
                            ? movement.postedByUserName
                            : movement.isRejected
                            ? movement.rejectedByUserName
                            : movement.createdByUserName || "Sistema"}
                        </div>
                      </TableCell>
                      <TableCell
                        className={`text-right font-semibold text-base ${
                          movement.quantity >= 0
                            ? "text-green-600"
                            : "text-red-600"
                        }`}
                      >
                        {movement.quantity > 0
                          ? `+${movement.quantity}`
                          : movement.quantity}
                      </TableCell>
                      <TableCell className="text-right font-medium">
                        {movement.unitCost
                          ? `$${movement.unitCost.toFixed(2)}`
                          : "-"}
                      </TableCell>
                      <TableCell className="text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            {!movement.isPosted && !movement.isRejected && (
                              <>
                                <DropdownMenuItem
                                  onClick={() => handlePostClick(movement)}
                                >
                                  <Send className="mr-2 h-4 w-4" />
                                  Postear
                                </DropdownMenuItem>
                                <DropdownMenuItem
                                  onClick={() => handleRejectClick(movement)}
                                  className="text-red-600 focus:text-red-600"
                                >
                                  <XCircle className="mr-2 h-4 w-4" />
                                  Rechazar
                                </DropdownMenuItem>
                              </>
                            )}
                            {movement.isPosted &&
                              movement.movementType !== "Sale" &&
                              movement.movementType !== "SaleCancellation" && (
                                <DropdownMenuItem
                                  onClick={() =>
                                    handleUnpostMovement(movement.id)
                                  }
                                >
                                  Despostear
                                </DropdownMenuItem>
                              )}
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell colSpan={9} className="h-24 text-center">
                      No se encontraron movimientos.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>
          <div className="flex items-center justify-between pt-4">
            <div className="text-sm text-muted-foreground">
              Mostrando {filteredMovements.length} de {movements?.length || 0}{" "}
              movimientos.
            </div>
            {/* TODO: Implement proper pagination */}
            <div className="flex items-center gap-2">
              <Button variant="outline" size="sm">
                Anterior
              </Button>
              <Button variant="outline" size="sm">
                Siguiente
              </Button>
            </div>
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

      {/* Add Stock Movement Modal */}
      <StockMovementComponentFormDialog
        isAddModalOpen={isAddModalOpen}
        productVariants={productVariants}
        onClose={() => setIsAddModalOpen(false)}
        onSubmit={handleSubmit}
      />
    </div>
  );
};

export default StockMovementsPage;
