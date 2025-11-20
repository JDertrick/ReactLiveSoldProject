import { useState, useEffect } from "react";
import {
  useGetTaxConfiguration,
  useUpdateTaxConfiguration,
  useGetTaxRates,
  useCreateTaxRate,
  useUpdateTaxRate,
  useDeleteTaxRate,
} from "../../hooks/useTax";
import {
  TaxSystemType,
  TaxApplicationMode,
  UpdateTaxConfigurationDto,
  CreateTaxRateDto,
  UpdateTaxRateDto,
  TaxRateDto,
  getTaxSystemTypeName,
  getTaxApplicationModeName,
  parseTaxSystemType,
  parseTaxApplicationMode,
} from "../../types/tax.types";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { toast } from "sonner";
import { Pencil, Trash2, Plus, Save } from "lucide-react";
import { CustomAlertDialog } from "@/components/common/AlertDialog";

const TaxSettings = () => {
  const { data: taxConfig, isLoading: isLoadingConfig } =
    useGetTaxConfiguration();
  const { data: taxRates, isLoading: isLoadingRates } = useGetTaxRates();
  const updateConfig = useUpdateTaxConfiguration();
  const createRate = useCreateTaxRate();
  const updateRate = useUpdateTaxRate();
  const deleteRate = useDeleteTaxRate();

  // Configuration state
  const [configData, setConfigData] = useState<UpdateTaxConfigurationDto>({
    taxEnabled: false,
    taxSystemType: TaxSystemType.None,
    taxDisplayName: "",
    taxApplicationMode: TaxApplicationMode.TaxIncluded,
    defaultTaxRateId: undefined,
  });

  // Rate modal state
  const [isRateModalOpen, setIsRateModalOpen] = useState(false);
  const [editingRate, setEditingRate] = useState<TaxRateDto | null>(null);
  const [rateFormData, setRateFormData] = useState<CreateTaxRateDto>({
    name: "",
    rate: 0,
    isDefault: false,
    isActive: true,
    description: "",
  });

  // Alert dialog state
  const [alertDialog, setAlertDialog] = useState({
    open: false,
    title: "",
    description: "",
  });

  // Load config data when available
  useEffect(() => {
    if (taxConfig) {
      console.log('Loading tax config:', taxConfig);
      console.log('Tax System Type (raw):', taxConfig.taxSystemType);
      console.log('Tax Application Mode (raw):', taxConfig.taxApplicationMode);

      const parsedSystemType = parseTaxSystemType(taxConfig.taxSystemType);
      const parsedApplicationMode = parseTaxApplicationMode(taxConfig.taxApplicationMode);

      console.log('Tax System Type (parsed):', parsedSystemType);
      console.log('Tax Application Mode (parsed):', parsedApplicationMode);

      const newConfig = {
        taxEnabled: taxConfig.taxEnabled,
        taxSystemType: parsedSystemType,
        taxDisplayName: taxConfig.taxDisplayName,
        taxApplicationMode: parsedApplicationMode,
        defaultTaxRateId: taxConfig.defaultTaxRateId,
      };

      console.log('Setting config data to:', newConfig);
      setConfigData(newConfig);
    }
  }, [taxConfig]);

  // Monitor configData changes
  useEffect(() => {
    console.log('configData updated:', configData);
  }, [configData]);

  const handleSaveConfiguration = async () => {
    try {
      await updateConfig.mutateAsync(configData);
      toast.success("Configuración de impuestos actualizada correctamente");
    } catch (error: any) {
      console.error("Error updating tax configuration:", error);
      toast.error(
        error.response?.data?.message || "Error al actualizar la configuración"
      );
    }
  };

  const handleOpenRateModal = (rate?: TaxRateDto) => {
    if (rate) {
      setEditingRate(rate);
      setRateFormData({
        name: rate.name,
        rate: rate.rate,
        isDefault: rate.isDefault,
        isActive: rate.isActive,
        description: rate.description,
        effectiveFrom: rate.effectiveFrom,
        effectiveTo: rate.effectiveTo,
      });
    } else {
      setEditingRate(null);
      setRateFormData({
        name: "",
        rate: 0,
        isDefault: false,
        isActive: true,
        description: "",
      });
    }
    setIsRateModalOpen(true);
  };

  const handleCloseRateModal = () => {
    setIsRateModalOpen(false);
    setEditingRate(null);
  };

  const handleSaveRate = async () => {
    try {
      if (editingRate) {
        await updateRate.mutateAsync({
          id: editingRate.id,
          data: { ...rateFormData, id: editingRate.id } as UpdateTaxRateDto,
        });
        toast.success("Tasa de impuesto actualizada correctamente");
      } else {
        await createRate.mutateAsync(rateFormData);
        toast.success("Tasa de impuesto creada correctamente");
      }
      handleCloseRateModal();
    } catch (error: any) {
      console.error("Error saving tax rate:", error);
      toast.error(error.response?.data?.message || "Error al guardar la tasa");
    }
  };

  const handleDeleteRate = async (rateId: string) => {
    try {
      await deleteRate.mutateAsync(rateId);
      toast.success("Tasa de impuesto eliminada correctamente");
    } catch (error: any) {
      console.error("Error deleting tax rate:", error);
      setAlertDialog({
        open: true,
        title: "Error",
        description:
          error.response?.data?.message || "Error al eliminar la tasa",
      });
    }
  };

  if (isLoadingConfig) {
    return <div className="p-6">Cargando configuración...</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Configuración de Impuestos
          </h1>
          <p className="text-muted-foreground mt-1">
            Habilita y configura los impuestos.
          </p>
        </div>
      </div>
      {/* Tax Configuration Card */}
      <Card>
        <CardHeader>
          <CardTitle>Configuración General</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Tax Enabled */}
          <div className="flex items-center space-x-2">
            <Checkbox
              id="taxEnabled"
              checked={configData.taxEnabled}
              onCheckedChange={(checked) =>
                setConfigData({ ...configData, taxEnabled: checked as boolean })
              }
            />
            <Label
              htmlFor="taxEnabled"
              className="text-xs font-semibold text-gray-500 tracking-wider"
            >
              Habilitar sistema de impuestos
            </Label>
          </div>

          <div className="grid grid-cols-2 gap-4">
            {/* Tax System Type */}
            <div className="space-y-2">
              <Label className="text-xs font-semibold text-gray-500 tracking-wider">
                Tipo de Sistema de Impuestos
              </Label>
              <Select
                key={`tax-system-${configData.taxSystemType}`}
                value={configData.taxSystemType.toString()}
                onValueChange={(value) => {
                  console.log('Tax system type changed to:', value);
                  setConfigData({
                    ...configData,
                    taxSystemType: parseInt(value) as TaxSystemType,
                  });
                }}
                disabled={!configData.taxEnabled}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Seleccione un tipo" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value={TaxSystemType.None.toString()}>
                    {getTaxSystemTypeName(TaxSystemType.None)}
                  </SelectItem>
                  <SelectItem value={TaxSystemType.VAT.toString()}>
                    {getTaxSystemTypeName(TaxSystemType.VAT)}
                  </SelectItem>
                  <SelectItem value={TaxSystemType.SalesTax.toString()}>
                    {getTaxSystemTypeName(TaxSystemType.SalesTax)}
                  </SelectItem>
                  <SelectItem value={TaxSystemType.GST.toString()}>
                    {getTaxSystemTypeName(TaxSystemType.GST)}
                  </SelectItem>
                  <SelectItem value={TaxSystemType.IGV.toString()}>
                    {getTaxSystemTypeName(TaxSystemType.IGV)}
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Tax Display Name */}
            <div className="space-y-2">
              <Label className="text-xs font-semibold text-gray-500 tracking-wider">
                Nombre para Mostrar (Opcional)
              </Label>
              <Input
                placeholder="Ej: IVA, Tax, Impuesto"
                value={configData.taxDisplayName || ""}
                onChange={(e) =>
                  setConfigData({
                    ...configData,
                    taxDisplayName: e.target.value,
                  })
                }
                disabled={!configData.taxEnabled}
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            {/* Tax Application Mode */}
            <div className="space-y-2">
              <Label className="text-xs font-semibold text-gray-500 tracking-wider">
                Modo de Aplicación del Impuesto
              </Label>
              <Select
                key={`tax-mode-${configData.taxApplicationMode}`}
                value={configData.taxApplicationMode.toString()}
                onValueChange={(value) => {
                  console.log('Tax application mode changed to:', value);
                  setConfigData({
                    ...configData,
                    taxApplicationMode: parseInt(value) as TaxApplicationMode,
                  });
                }}
                disabled={!configData.taxEnabled}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Seleccione un modo" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value={TaxApplicationMode.TaxIncluded.toString()}>
                    {getTaxApplicationModeName(TaxApplicationMode.TaxIncluded)}
                  </SelectItem>
                  <SelectItem value={TaxApplicationMode.TaxExcluded.toString()}>
                    {getTaxApplicationModeName(TaxApplicationMode.TaxExcluded)}
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Default Tax Rate */}
            <div className="space-y-2">
              <Label className="text-xs font-semibold text-gray-500 tracking-wider">
                Tasa de Impuesto Predeterminada
              </Label>
              <Select
                key={`tax-rate-${configData.defaultTaxRateId || 'none'}`}
                value={configData.defaultTaxRateId || "__none__"}
                onValueChange={(value) =>
                  setConfigData({
                    ...configData,
                    defaultTaxRateId: value === "__none__" ? undefined : value,
                  })
                }
                disabled={!configData.taxEnabled}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Seleccione una tasa" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="__none__">
                    Sin tasa predeterminada
                  </SelectItem>
                  {taxRates
                    ?.filter((r) => r.isActive)
                    .map((rate) => (
                      <SelectItem key={rate.id} value={rate.id}>
                        {rate.name} ({(rate.rate * 100).toFixed(2)}%)
                      </SelectItem>
                    ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <Button
            onClick={handleSaveConfiguration}
            disabled={updateConfig.isPending}
          >
            <Save className="mr-2 h-4 w-4" />
            {updateConfig.isPending ? "Guardando..." : "Guardar Configuración"}
          </Button>
        </CardContent>
      </Card>

      {/* Tax Rates Management */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>Tasas de Impuesto</CardTitle>
          <Button onClick={() => handleOpenRateModal()} size="sm">
            <Plus className="mr-2 h-4 w-4" />
            Nueva Tasa
          </Button>
        </CardHeader>
        <CardContent>
          {isLoadingRates ? (
            <div>Cargando tasas...</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nombre</TableHead>
                  <TableHead>Tasa</TableHead>
                  <TableHead>Estado</TableHead>
                  <TableHead>Descripción</TableHead>
                  <TableHead>Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {taxRates?.length === 0 ? (
                  <TableRow>
                    <TableCell
                      colSpan={5}
                      className="text-center text-gray-500"
                    >
                      No hay tasas de impuesto configuradas
                    </TableCell>
                  </TableRow>
                ) : (
                  taxRates?.map((rate) => (
                    <TableRow key={rate.id}>
                      <TableCell className="font-medium">
                        {rate.name}
                        {rate.isDefault && (
                          <Badge variant="secondary" className="ml-2">
                            Predeterminada
                          </Badge>
                        )}
                      </TableCell>
                      <TableCell>{(rate.rate * 100).toFixed(2)}%</TableCell>
                      <TableCell>
                        <Badge
                          variant={rate.isActive ? "default" : "secondary"}
                        >
                          {rate.isActive ? "Activa" : "Inactiva"}
                        </Badge>
                      </TableCell>
                      <TableCell className="max-w-xs truncate">
                        {rate.description || "-"}
                      </TableCell>
                      <TableCell>
                        <div className="flex gap-2">
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleOpenRateModal(rate)}
                          >
                            <Pencil className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleDeleteRate(rate.id)}
                          >
                            <Trash2 className="h-4 w-4 text-red-500" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Tax Rate Modal */}
      <Dialog open={isRateModalOpen} onOpenChange={setIsRateModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editingRate
                ? "Editar Tasa de Impuesto"
                : "Nueva Tasa de Impuesto"}
            </DialogTitle>
          </DialogHeader>

          <div className="space-y-4">
            <div className="space-y-2">
              <Label>
                Nombre <span className="text-red-500">*</span>
              </Label>
              <Input
                placeholder="Ej: IVA 19%"
                value={rateFormData.name}
                onChange={(e) =>
                  setRateFormData({ ...rateFormData, name: e.target.value })
                }
              />
            </div>

            <div className="space-y-2">
              <Label>
                Tasa (0.00 - 1.00) <span className="text-red-500">*</span>
              </Label>
              <Input
                type="number"
                step="0.01"
                min="0"
                max="1"
                placeholder="Ej: 0.19 para 19%"
                value={rateFormData.rate}
                onChange={(e) =>
                  setRateFormData({
                    ...rateFormData,
                    rate: parseFloat(e.target.value) || 0,
                  })
                }
              />
              <p className="text-sm text-gray-500">
                Porcentaje: {(rateFormData.rate * 100).toFixed(2)}%
              </p>
            </div>

            <div className="space-y-2">
              <Label>Descripción</Label>
              <Textarea
                placeholder="Descripción opcional de la tasa"
                value={rateFormData.description || ""}
                onChange={(e) =>
                  setRateFormData({
                    ...rateFormData,
                    description: e.target.value,
                  })
                }
                rows={3}
              />
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="isActive"
                checked={rateFormData.isActive}
                onCheckedChange={(checked) =>
                  setRateFormData({
                    ...rateFormData,
                    isActive: checked as boolean,
                  })
                }
              />
              <Label htmlFor="isActive">Activa</Label>
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="isDefault"
                checked={rateFormData.isDefault}
                onCheckedChange={(checked) =>
                  setRateFormData({
                    ...rateFormData,
                    isDefault: checked as boolean,
                  })
                }
              />
              <Label htmlFor="isDefault">Establecer como predeterminada</Label>
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={handleCloseRateModal}>
              Cancelar
            </Button>
            <Button
              onClick={handleSaveRate}
              disabled={createRate.isPending || updateRate.isPending}
            >
              {createRate.isPending || updateRate.isPending
                ? "Guardando..."
                : "Guardar"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Alert Dialog */}
      <CustomAlertDialog
        open={alertDialog.open}
        title={alertDialog.title}
        description={alertDialog.description}
        onClose={() => setAlertDialog({ ...alertDialog, open: false })}
      />
    </div>
  );
};

export default TaxSettings;
