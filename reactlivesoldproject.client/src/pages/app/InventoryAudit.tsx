import { useState, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import {
  useGetAudits,
  useCreateAudit,
  useStartAudit,
  useCancelAudit,
} from "../../hooks/useInventoryAudit";
import { InventoryAuditDto, CreateInventoryAuditDto } from "../../types/inventoryAudit.types";
import { CreateAuditWizard } from "../../components/inventoryaudit/CreateAuditWizard";
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  ClipboardList,
  Play,
  CheckCircle2,
  XCircle,
  Clock,
  MoreHorizontal,
  Plus,
  Eye,
  FileCheck,
  Warehouse,
} from "lucide-react";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { toast } from "sonner";
import { formatCurrency } from "@/utils/currencyHelper";

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

const InventoryAuditPage = () => {
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [searchTerm, setSearchTerm] = useState("");
  const [isWizardOpen, setIsWizardOpen] = useState(false);
  const [isConfirmStartOpen, setIsConfirmStartOpen] = useState(false);
  const [isConfirmCancelOpen, setIsConfirmCancelOpen] = useState(false);
  const [selectedAudit, setSelectedAudit] = useState<InventoryAuditDto | null>(null);

  const { data: audits, isLoading } = useGetAudits(
    statusFilter !== "all" ? statusFilter : undefined
  );
  const createAudit = useCreateAudit();
  const startAudit = useStartAudit();
  const cancelAudit = useCancelAudit();

  const filteredAudits = useMemo(() => {
    if (!audits) return [];
    return audits.filter(
      (a) =>
        a.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        a.createdByUserName?.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [audits, searchTerm]);

  const stats = useMemo(() => {
    if (!audits)
      return { total: 0, draft: 0, inProgress: 0, completed: 0, cancelled: 0 };

    return {
      total: audits.length,
      draft: audits.filter((a) => a.status === "Draft").length,
      inProgress: audits.filter((a) => a.status === "InProgress").length,
      completed: audits.filter((a) => a.status === "Completed").length,
      cancelled: audits.filter((a) => a.status === "Cancelled").length,
    };
  }, [audits]);

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
      <Badge variant="outline" className={className}>
        {icon}
        {label}
      </Badge>
    );
  };

  const handleCreateAudit = async (data: CreateInventoryAuditDto) => {
    try {
      const audit = await createAudit.mutateAsync(data);
      toast.success("Auditoría creada exitosamente. Snapshot del inventario tomado.");
      navigate(`/app/inventory-audit/${audit.id}`);
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Error al crear la auditoría");
      throw error;
    }
  };

  const handleStartClick = (audit: InventoryAuditDto) => {
    setSelectedAudit(audit);
    setIsConfirmStartOpen(true);
  };

  const handleCancelClick = (audit: InventoryAuditDto) => {
    setSelectedAudit(audit);
    setIsConfirmCancelOpen(true);
  };

  const handleConfirmStart = async () => {
    if (!selectedAudit) return;
    try {
      await startAudit.mutateAsync(selectedAudit.id);
      toast.success("Auditoría iniciada. Puede comenzar el conteo.");
      setIsConfirmStartOpen(false);
      navigate(`/app/inventory-audit/${selectedAudit.id}/count`);
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Error al iniciar la auditoría");
    }
  };

  const handleConfirmCancel = async () => {
    if (!selectedAudit) return;
    try {
      await cancelAudit.mutateAsync(selectedAudit.id);
      toast.success("Auditoría cancelada");
      setIsConfirmCancelOpen(false);
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Error al cancelar la auditoría");
    }
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <p>Cargando auditorías...</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Auditoría de Inventario
          </h1>
          <p className="text-muted-foreground mt-1">
            Gestiona tomas físicas de inventario con conteo ciego y snapshots.
          </p>
        </div>
        <Button
          onClick={() => setIsWizardOpen(true)}
          size="lg"
          className="gap-2"
        >
          <Plus className="w-5 h-5" />
          Nueva Auditoría
        </Button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatCard
          title="TOTAL AUDITORÍAS"
          value={stats.total}
          subtext=""
          icon={<ClipboardList className="w-6 h-6" />}
          colorClass="text-blue-500"
          circleColorClass="bg-blue-500"
        />
        <StatCard
          title="EN PROGRESO"
          value={stats.inProgress}
          icon={<Play className="w-6 h-6" />}
          colorClass="text-yellow-500"
          circleColorClass="bg-yellow-500"
          subtext="Conteo activo"
        />
        <StatCard
          title="COMPLETADAS"
          value={stats.completed}
          icon={<CheckCircle2 className="w-6 h-6" />}
          colorClass="text-green-500"
          circleColorClass="bg-green-500"
          subtext=""
        />
        <StatCard
          title="BORRADORES"
          value={stats.draft}
          icon={<Clock className="w-6 h-6" />}
          colorClass="text-orange-500"
          circleColorClass="bg-orange-500"
          subtext="Pendientes de iniciar"
        />
      </div>

      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row justify-between items-center gap-4 mb-6">
            <div className="flex items-center gap-2 w-full sm:w-auto">
              <Input
                placeholder="Buscar por nombre..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full sm:w-64"
              />
            </div>
            <div className="flex items-center gap-2">
              <Select
                value={statusFilter}
                onValueChange={(value) => setStatusFilter(value)}
              >
                <SelectTrigger className="w-[180px]">
                  <SelectValue placeholder="Estado" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos</SelectItem>
                  <SelectItem value="Draft">Borradores</SelectItem>
                  <SelectItem value="InProgress">En Progreso</SelectItem>
                  <SelectItem value="Completed">Completadas</SelectItem>
                  <SelectItem value="Cancelled">Canceladas</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>AUDITORÍA</TableHead>
                  <TableHead>ESTADO</TableHead>
                  <TableHead>PROGRESO</TableHead>
                  <TableHead>VARIANZA</TableHead>
                  <TableHead>FECHA</TableHead>
                  <TableHead className="text-right">ACCIÓN</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredAudits && filteredAudits.length > 0 ? (
                  filteredAudits.map((audit) => (
                    <TableRow key={audit.id} className="hover:bg-muted/50">
                      <TableCell>
                        <div className="flex items-center gap-3">
                          <div className="bg-muted p-2 rounded-md">
                            <ClipboardList className="h-5 w-5 text-muted-foreground" />
                          </div>
                          <div>
                            <div className="font-medium">{audit.name}</div>
                            <div className="text-xs text-muted-foreground">
                              {audit.totalVariants} variantes • Por {audit.createdByUserName}
                            </div>
                            {audit.scopeDescription && (
                              <div className="flex items-center gap-1 text-xs text-muted-foreground mt-1">
                                <Warehouse className="h-3 w-3" />
                                {audit.scopeDescription}
                              </div>
                            )}
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>{getStatusBadge(audit.status)}</TableCell>
                      <TableCell>
                        <div className="w-32">
                          <div className="flex justify-between text-xs mb-1">
                            <span>{audit.countedVariants}/{audit.totalVariants}</span>
                            <span>{Math.round(audit.progress)}%</span>
                          </div>
                          <Progress value={audit.progress} className="h-2" />
                        </div>
                      </TableCell>
                      <TableCell>
                        {audit.status === "Completed" ? (
                          <div>
                            <div className={`font-semibold ${audit.totalVariance >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                              {audit.totalVariance >= 0 ? '+' : ''}{audit.totalVariance} uds
                            </div>
                            <div className="text-xs text-muted-foreground">
                              {formatCurrency(Math.abs(audit.totalVarianceValue))}
                            </div>
                          </div>
                        ) : (
                          <span className="text-muted-foreground">-</span>
                        )}
                      </TableCell>
                      <TableCell>
                        <div className="font-medium">
                          {new Date(audit.snapshotTakenAt).toLocaleDateString("es-ES", {
                            day: "numeric",
                            month: "short",
                            year: "numeric",
                          })}
                        </div>
                        <div className="text-xs text-muted-foreground">
                          Snapshot
                        </div>
                      </TableCell>
                      <TableCell className="text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem
                              onClick={() => navigate(`/app/inventory-audit/${audit.id}`)}
                            >
                              <Eye className="mr-2 h-4 w-4" />
                              Ver Detalles
                            </DropdownMenuItem>
                            {audit.status === "Draft" && (
                              <>
                                <DropdownMenuItem onClick={() => handleStartClick(audit)}>
                                  <Play className="mr-2 h-4 w-4" />
                                  Iniciar Conteo
                                </DropdownMenuItem>
                                <DropdownMenuItem
                                  onClick={() => handleCancelClick(audit)}
                                  className="text-red-600 focus:text-red-600"
                                >
                                  <XCircle className="mr-2 h-4 w-4" />
                                  Cancelar
                                </DropdownMenuItem>
                              </>
                            )}
                            {audit.status === "InProgress" && (
                              <>
                                <DropdownMenuItem
                                  onClick={() => navigate(`/app/inventory-audit/${audit.id}/count`)}
                                >
                                  <FileCheck className="mr-2 h-4 w-4" />
                                  Continuar Conteo
                                </DropdownMenuItem>
                                <DropdownMenuItem
                                  onClick={() => handleCancelClick(audit)}
                                  className="text-red-600 focus:text-red-600"
                                >
                                  <XCircle className="mr-2 h-4 w-4" />
                                  Cancelar
                                </DropdownMenuItem>
                              </>
                            )}
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell colSpan={6} className="h-24 text-center">
                      No se encontraron auditorías.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      {/* Create Audit Wizard */}
      <CreateAuditWizard
        open={isWizardOpen}
        onClose={() => setIsWizardOpen(false)}
        onSubmit={handleCreateAudit}
        isLoading={createAudit.isPending}
      />

      {/* Confirm Start Dialog */}
      <AlertDialog open={isConfirmStartOpen} onOpenChange={setIsConfirmStartOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Iniciar conteo de inventario?</AlertDialogTitle>
            <AlertDialogDescription>
              Al iniciar, el estado de la auditoría cambiará a "En Progreso" y podrá comenzar a registrar los conteos físicos.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={handleConfirmStart} disabled={startAudit.isPending}>
              {startAudit.isPending ? "Iniciando..." : "Iniciar Conteo"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Confirm Cancel Dialog */}
      <AlertDialog open={isConfirmCancelOpen} onOpenChange={setIsConfirmCancelOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Cancelar esta auditoría?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción no se puede deshacer. Los conteos realizados se perderán y no se aplicarán ajustes al inventario.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Volver</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleConfirmCancel}
              disabled={cancelAudit.isPending}
              className="bg-red-600 hover:bg-red-700"
            >
              {cancelAudit.isPending ? "Cancelando..." : "Cancelar Auditoría"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default InventoryAuditPage;
