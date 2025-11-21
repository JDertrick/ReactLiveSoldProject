import { useState, useMemo } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  useGetAuditById,
  useGetBlindCountItems,
  useGetAuditSummary,
  useUpdateCount,
  useCompleteAudit,
} from "../../hooks/useInventoryAudit";
import { BlindCountItemDto } from "../../types/inventoryAudit.types";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
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
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import {
  ArrowLeft,
  Search,
  CheckCircle2,
  Package,
  Eye,
  EyeOff,
  ClipboardCheck,
} from "lucide-react";
import { toast } from "sonner";
import { formatCurrency } from "@/utils/currencyHelper";

const InventoryAuditCountPage = () => {
  const { auditId } = useParams<{ auditId: string }>();
  const navigate = useNavigate();

  const [searchTerm, setSearchTerm] = useState("");
  const [showOnlyPending, setShowOnlyPending] = useState(false);
  const [countInputs, setCountInputs] = useState<Record<string, string>>({});
  const [isCompleteDialogOpen, setIsCompleteDialogOpen] = useState(false);
  const [autoPostAdjustments, setAutoPostAdjustments] = useState(false);
  const [isSummaryVisible, setIsSummaryVisible] = useState(false);

  const { data: audit, isLoading: auditLoading } = useGetAuditById(auditId || "");
  const { data: items, isLoading: itemsLoading, refetch: refetchItems } = useGetBlindCountItems(auditId || "");
  const { data: summary, refetch: refetchSummary } = useGetAuditSummary(auditId || "");
  const updateCount = useUpdateCount();
  const completeAudit = useCompleteAudit();

  const filteredItems = useMemo(() => {
    if (!items) return [];
    let filtered = items;

    if (showOnlyPending) {
      filtered = filtered.filter((i) => !i.isCounted);
    }

    if (searchTerm) {
      const search = searchTerm.toLowerCase();
      filtered = filtered.filter(
        (i) =>
          i.productName.toLowerCase().includes(search) ||
          i.variantSku.toLowerCase().includes(search)
      );
    }

    return filtered;
  }, [items, searchTerm, showOnlyPending]);

  const progress = useMemo(() => {
    if (!items || items.length === 0) return { counted: 0, total: 0, percent: 0 };
    const counted = items.filter((i) => i.isCounted).length;
    return {
      counted,
      total: items.length,
      percent: (counted / items.length) * 100,
    };
  }, [items]);

  const handleCountChange = (itemId: string, value: string) => {
    setCountInputs((prev) => ({ ...prev, [itemId]: value }));
  };

  const handleSaveCount = async (item: BlindCountItemDto) => {
    const inputValue = countInputs[item.id];
    if (inputValue === undefined || inputValue === "") {
      toast.error("Ingrese la cantidad contada");
      return;
    }

    const countedStock = parseInt(inputValue, 10);
    if (isNaN(countedStock) || countedStock < 0) {
      toast.error("La cantidad debe ser un número válido mayor o igual a 0");
      return;
    }

    try {
      await updateCount.mutateAsync({
        auditId: auditId!,
        data: {
          itemId: item.id,
          countedStock,
        },
      });
      toast.success(`Conteo guardado: ${item.variantSku}`);
      refetchItems();
      refetchSummary();
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Error al guardar el conteo");
    }
  };

  const handleCompleteAudit = async () => {
    if (progress.counted < progress.total) {
      toast.error("Debe completar el conteo de todos los items antes de finalizar");
      return;
    }

    try {
      await completeAudit.mutateAsync({
        auditId: auditId!,
        data: {
          autoPostAdjustments,
          notes: "Auditoría completada desde la interfaz web",
        },
      });
      toast.success("Auditoría completada exitosamente");
      setIsCompleteDialogOpen(false);
      navigate(`/app/inventory-audit/${auditId}`);
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Error al completar la auditoría");
    }
  };

  if (auditLoading || itemsLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <p>Cargando datos de auditoría...</p>
      </div>
    );
  }

  if (!audit) {
    return (
      <div className="flex justify-center items-center h-64">
        <p>Auditoría no encontrada</p>
      </div>
    );
  }

  if (audit.status !== "InProgress") {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={() => navigate("/app/inventory-audit")}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Volver
        </Button>
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-lg">
              Esta auditoría no está en estado de conteo.
            </p>
            <p className="text-muted-foreground">
              Estado actual: {audit.status}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" onClick={() => navigate("/app/inventory-audit")}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Volver
          </Button>
          <div>
            <h1 className="text-2xl font-bold">{audit.name}</h1>
            <p className="text-muted-foreground">Conteo Ciego de Inventario</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            onClick={() => setIsSummaryVisible(!isSummaryVisible)}
          >
            {isSummaryVisible ? <EyeOff className="mr-2 h-4 w-4" /> : <Eye className="mr-2 h-4 w-4" />}
            {isSummaryVisible ? "Ocultar Resumen" : "Ver Resumen"}
          </Button>
          <Button
            onClick={() => setIsCompleteDialogOpen(true)}
            disabled={progress.counted < progress.total}
          >
            <ClipboardCheck className="mr-2 h-4 w-4" />
            Finalizar Auditoría
          </Button>
        </div>
      </div>

      {/* Progress Card */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h3 className="text-lg font-semibold">Progreso del Conteo</h3>
              <p className="text-sm text-muted-foreground">
                {progress.counted} de {progress.total} variantes contadas
              </p>
            </div>
            <div className="text-right">
              <span className="text-3xl font-bold text-primary">
                {Math.round(progress.percent)}%
              </span>
            </div>
          </div>
          <Progress value={progress.percent} className="h-3" />
        </CardContent>
      </Card>

      {/* Summary Card (toggleable) */}
      {isSummaryVisible && summary && (
        <Card className="border-blue-200 bg-blue-50">
          <CardHeader>
            <CardTitle className="text-blue-800">Resumen de Varianzas (hasta ahora)</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div>
                <p className="text-sm text-muted-foreground">Items con diferencia</p>
                <p className="text-xl font-bold">{summary.itemsWithVariance}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Sobrantes (+)</p>
                <p className="text-xl font-bold text-green-600">
                  +{summary.totalPositiveVariance} uds
                </p>
                <p className="text-xs text-muted-foreground">
                  {formatCurrency(summary.totalPositiveVarianceValue)}
                </p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Faltantes (-)</p>
                <p className="text-xl font-bold text-red-600">
                  {summary.totalNegativeVariance} uds
                </p>
                <p className="text-xs text-muted-foreground">
                  {formatCurrency(Math.abs(summary.totalNegativeVarianceValue))}
                </p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Varianza Neta</p>
                <p className={`text-xl font-bold ${summary.netVariance >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                  {summary.netVariance >= 0 ? '+' : ''}{summary.netVariance} uds
                </p>
                <p className="text-xs text-muted-foreground">
                  {formatCurrency(summary.netVarianceValue)}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-4">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Buscar por nombre o SKU..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-10"
          />
        </div>
        <div className="flex items-center gap-2">
          <Checkbox
            id="showPending"
            checked={showOnlyPending}
            onCheckedChange={(checked) => setShowOnlyPending(checked as boolean)}
          />
          <Label htmlFor="showPending" className="text-sm">
            Solo pendientes ({progress.total - progress.counted})
          </Label>
        </div>
      </div>

      {/* Items Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {filteredItems.map((item) => (
          <Card
            key={item.id}
            className={`transition-all ${
              item.isCounted ? "border-green-200 bg-green-50/50" : ""
            }`}
          >
            <CardContent className="pt-4">
              <div className="flex gap-4">
                {/* Image */}
                <div className="w-16 h-16 bg-muted rounded-lg flex items-center justify-center overflow-hidden flex-shrink-0">
                  {item.variantImageUrl ? (
                    <img
                      src={item.variantImageUrl}
                      alt={item.productName}
                      className="w-full h-full object-cover"
                    />
                  ) : (
                    <Package className="h-8 w-8 text-muted-foreground" />
                  )}
                </div>

                {/* Info */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-start justify-between">
                    <div>
                      <h4 className="font-medium truncate">{item.productName}</h4>
                      <p className="text-sm text-muted-foreground">{item.variantSku}</p>
                      {(item.variantSize || item.variantColor) && (
                        <div className="flex gap-2 mt-1">
                          {item.variantSize && (
                            <Badge variant="outline" className="text-xs">
                              {item.variantSize}
                            </Badge>
                          )}
                          {item.variantColor && (
                            <Badge variant="outline" className="text-xs">
                              {item.variantColor}
                            </Badge>
                          )}
                        </div>
                      )}
                    </div>
                    {item.isCounted && (
                      <CheckCircle2 className="h-5 w-5 text-green-600 flex-shrink-0" />
                    )}
                  </div>

                  {/* Count Input */}
                  <div className="flex gap-2 mt-3">
                    <Input
                      type="number"
                      min="0"
                      placeholder="Cantidad"
                      value={countInputs[item.id] ?? (item.countedStock?.toString() || "")}
                      onChange={(e) => handleCountChange(item.id, e.target.value)}
                      className="w-24"
                    />
                    <Button
                      size="sm"
                      onClick={() => handleSaveCount(item)}
                      disabled={updateCount.isPending}
                    >
                      {item.isCounted ? "Actualizar" : "Guardar"}
                    </Button>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {filteredItems.length === 0 && (
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-muted-foreground">
              {showOnlyPending
                ? "¡Todos los items han sido contados!"
                : "No se encontraron items con los filtros actuales."}
            </p>
          </CardContent>
        </Card>
      )}

      {/* Complete Audit Dialog */}
      <AlertDialog open={isCompleteDialogOpen} onOpenChange={setIsCompleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Finalizar auditoría?</AlertDialogTitle>
            <AlertDialogDescription>
              Se generarán movimientos de ajuste para todas las varianzas detectadas.
              Esta acción no se puede deshacer.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <div className="py-4">
            <div className="flex items-center space-x-2">
              <Checkbox
                id="autoPost"
                checked={autoPostAdjustments}
                onCheckedChange={(checked) => setAutoPostAdjustments(checked as boolean)}
              />
              <Label htmlFor="autoPost">
                Postear ajustes automáticamente (actualizar inventario ahora)
              </Label>
            </div>
            <p className="text-sm text-muted-foreground mt-2">
              Si no marca esta opción, los ajustes se crearán como borradores para revisión.
            </p>
          </div>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleCompleteAudit}
              disabled={completeAudit.isPending}
            >
              {completeAudit.isPending ? "Finalizando..." : "Finalizar Auditoría"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default InventoryAuditCountPage;
