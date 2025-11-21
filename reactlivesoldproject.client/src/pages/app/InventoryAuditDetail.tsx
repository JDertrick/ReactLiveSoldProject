import { useState, useMemo } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  useGetAuditById,
  useGetAuditItems,
  useGetAuditSummary,
  useStartAudit,
} from "../../hooks/useInventoryAudit";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  ArrowLeft,
  Play,
  CheckCircle2,
  Clock,
  XCircle,
  Package,
  TrendingUp,
  TrendingDown,
  Minus,
} from "lucide-react";
import { toast } from "sonner";
import { formatCurrency } from "@/utils/currencyHelper";

const InventoryAuditDetailPage = () => {
  const { auditId } = useParams<{ auditId: string }>();
  const navigate = useNavigate();

  const [searchTerm, setSearchTerm] = useState("");
  const [varianceFilter, setVarianceFilter] = useState<string>("all");

  const { data: audit, isLoading: auditLoading } = useGetAuditById(auditId || "");
  const { data: items, isLoading: itemsLoading } = useGetAuditItems(auditId || "");
  const { data: summary } = useGetAuditSummary(auditId || "");
  const startAudit = useStartAudit();

  const filteredItems = useMemo(() => {
    if (!items) return [];
    let filtered = items;

    if (varianceFilter === "positive") {
      filtered = filtered.filter((i) => i.variance && i.variance > 0);
    } else if (varianceFilter === "negative") {
      filtered = filtered.filter((i) => i.variance && i.variance < 0);
    } else if (varianceFilter === "none") {
      filtered = filtered.filter((i) => i.variance === 0);
    } else if (varianceFilter === "pending") {
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
  }, [items, searchTerm, varianceFilter]);

  const handleStartAudit = async () => {
    try {
      await startAudit.mutateAsync(auditId!);
      toast.success("Auditoría iniciada");
      navigate(`/app/inventory-audit/${auditId}/count`);
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Error al iniciar la auditoría");
    }
  };

  const getStatusBadge = (status: string) => {
    const config: Record<string, { label: string; className: string; icon: React.ReactNode }> = {
      Draft: {
        label: "Borrador",
        className: "text-orange-600 border-orange-600/50 bg-orange-600/10",
        icon: <Clock className="w-3.5 h-3.5 mr-1" />,
      },
      InProgress: {
        label: "En Progreso",
        className: "text-blue-600 border-blue-600/50 bg-blue-600/10",
        icon: <Play className="w-3.5 h-3.5 mr-1" />,
      },
      Completed: {
        label: "Completada",
        className: "text-green-600 border-green-600/50 bg-green-600/10",
        icon: <CheckCircle2 className="w-3.5 h-3.5 mr-1" />,
      },
      Cancelled: {
        label: "Cancelada",
        className: "text-red-600 border-red-600/50 bg-red-600/10",
        icon: <XCircle className="w-3.5 h-3.5 mr-1" />,
      },
    };

    const { label, className, icon } = config[status] || config.Draft;
    return (
      <Badge variant="outline" className={`${className} text-base px-3 py-1`}>
        {icon}
        {label}
      </Badge>
    );
  };

  if (auditLoading || itemsLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <p>Cargando auditoría...</p>
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
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold">{audit.name}</h1>
              {getStatusBadge(audit.status)}
            </div>
            <p className="text-muted-foreground">
              Creada el {new Date(audit.snapshotTakenAt).toLocaleDateString("es-ES")} por {audit.createdByUserName}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          {audit.status === "Draft" && (
            <Button onClick={handleStartAudit} disabled={startAudit.isPending}>
              <Play className="mr-2 h-4 w-4" />
              {startAudit.isPending ? "Iniciando..." : "Iniciar Conteo"}
            </Button>
          )}
          {audit.status === "InProgress" && (
            <Button onClick={() => navigate(`/app/inventory-audit/${auditId}/count`)}>
              <Play className="mr-2 h-4 w-4" />
              Continuar Conteo
            </Button>
          )}
        </div>
      </div>

      {/* Progress Card */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h3 className="text-lg font-semibold">Progreso del Conteo</h3>
              <p className="text-sm text-muted-foreground">
                {audit.countedVariants} de {audit.totalVariants} variantes contadas
              </p>
            </div>
            <div className="text-right">
              <span className="text-3xl font-bold text-primary">
                {Math.round(audit.progress)}%
              </span>
            </div>
          </div>
          <Progress value={audit.progress} className="h-3" />
        </CardContent>
      </Card>

      {/* Summary Cards */}
      {summary && audit.status !== "Draft" && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Card className="border-l-4 border-l-blue-500">
            <CardContent className="pt-4">
              <div className="flex items-center gap-3">
                <Package className="h-8 w-8 text-blue-500" />
                <div>
                  <p className="text-sm text-muted-foreground">Items con Varianza</p>
                  <p className="text-2xl font-bold">{summary.itemsWithVariance}</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card className="border-l-4 border-l-green-500">
            <CardContent className="pt-4">
              <div className="flex items-center gap-3">
                <TrendingUp className="h-8 w-8 text-green-500" />
                <div>
                  <p className="text-sm text-muted-foreground">Sobrantes</p>
                  <p className="text-2xl font-bold text-green-600">
                    +{summary.totalPositiveVariance}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {formatCurrency(summary.totalPositiveVarianceValue)}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card className="border-l-4 border-l-red-500">
            <CardContent className="pt-4">
              <div className="flex items-center gap-3">
                <TrendingDown className="h-8 w-8 text-red-500" />
                <div>
                  <p className="text-sm text-muted-foreground">Faltantes</p>
                  <p className="text-2xl font-bold text-red-600">
                    {summary.totalNegativeVariance}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {formatCurrency(Math.abs(summary.totalNegativeVarianceValue))}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card className="border-l-4 border-l-purple-500">
            <CardContent className="pt-4">
              <div className="flex items-center gap-3">
                <Minus className="h-8 w-8 text-purple-500" />
                <div>
                  <p className="text-sm text-muted-foreground">Varianza Neta</p>
                  <p className={`text-2xl font-bold ${summary.netVariance >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                    {summary.netVariance >= 0 ? '+' : ''}{summary.netVariance}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {formatCurrency(summary.netVarianceValue)}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Items Table */}
      <Card>
        <CardHeader>
          <CardTitle>Detalle de Items</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col sm:flex-row justify-between items-center gap-4 mb-6">
            <Input
              placeholder="Buscar por nombre o SKU..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full sm:w-64"
            />
            <Select value={varianceFilter} onValueChange={setVarianceFilter}>
              <SelectTrigger className="w-[180px]">
                <SelectValue placeholder="Filtrar por varianza" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos</SelectItem>
                <SelectItem value="positive">Sobrantes (+)</SelectItem>
                <SelectItem value="negative">Faltantes (-)</SelectItem>
                <SelectItem value="none">Sin diferencia</SelectItem>
                <SelectItem value="pending">Sin contar</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>PRODUCTO / SKU</TableHead>
                  <TableHead className="text-right">STOCK TEÓRICO</TableHead>
                  <TableHead className="text-right">CONTEO FÍSICO</TableHead>
                  <TableHead className="text-right">VARIANZA</TableHead>
                  <TableHead className="text-right">VALOR</TableHead>
                  <TableHead>CONTADO POR</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredItems && filteredItems.length > 0 ? (
                  filteredItems.map((item) => (
                    <TableRow key={item.id} className="hover:bg-muted/50">
                      <TableCell>
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 bg-muted rounded-md flex items-center justify-center overflow-hidden">
                            {item.variantImageUrl ? (
                              <img
                                src={item.variantImageUrl}
                                alt={item.productName}
                                className="w-full h-full object-cover"
                              />
                            ) : (
                              <Package className="h-5 w-5 text-muted-foreground" />
                            )}
                          </div>
                          <div>
                            <div className="font-medium">{item.productName}</div>
                            <div className="text-xs text-muted-foreground">
                              {item.variantSku}
                              {item.variantSize && ` • ${item.variantSize}`}
                              {item.variantColor && ` • ${item.variantColor}`}
                            </div>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell className="text-right font-medium">
                        {item.theoreticalStock}
                      </TableCell>
                      <TableCell className="text-right font-medium">
                        {item.isCounted ? item.countedStock : (
                          <span className="text-muted-foreground">-</span>
                        )}
                      </TableCell>
                      <TableCell className="text-right">
                        {item.isCounted ? (
                          <span
                            className={`font-bold ${
                              item.variance === 0
                                ? "text-muted-foreground"
                                : item.variance! > 0
                                ? "text-green-600"
                                : "text-red-600"
                            }`}
                          >
                            {item.variance === 0
                              ? "0"
                              : item.variance! > 0
                              ? `+${item.variance}`
                              : item.variance}
                          </span>
                        ) : (
                          <span className="text-muted-foreground">-</span>
                        )}
                      </TableCell>
                      <TableCell className="text-right">
                        {item.isCounted && item.varianceValue !== 0 ? (
                          <span
                            className={`text-sm ${
                              item.varianceValue! >= 0 ? "text-green-600" : "text-red-600"
                            }`}
                          >
                            {formatCurrency(item.varianceValue!)}
                          </span>
                        ) : (
                          <span className="text-muted-foreground">-</span>
                        )}
                      </TableCell>
                      <TableCell>
                        {item.countedByUserName ? (
                          <div>
                            <div className="text-sm">{item.countedByUserName}</div>
                            <div className="text-xs text-muted-foreground">
                              {item.countedAt &&
                                new Date(item.countedAt).toLocaleDateString("es-ES", {
                                  day: "numeric",
                                  month: "short",
                                  hour: "2-digit",
                                  minute: "2-digit",
                                })}
                            </div>
                          </div>
                        ) : (
                          <Badge variant="outline" className="text-orange-600">
                            Pendiente
                          </Badge>
                        )}
                      </TableCell>
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell colSpan={6} className="h-24 text-center">
                      No se encontraron items con los filtros actuales.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default InventoryAuditDetailPage;
