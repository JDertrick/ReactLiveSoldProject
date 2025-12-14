import { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
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
import { toast } from "sonner";
import { useNoSeries } from "../../../hooks/useNoSeries";
import {
  NoSerie,
  CreateNoSerieDto,
  UpdateNoSerieDto,
  DocumentTypeLabels,
} from "../../../types/noserie.types";

interface NoSerieFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  serie?: NoSerie | null;
  onSuccess?: () => void;
}

const NoSerieFormDialog = ({
  open,
  onOpenChange,
  serie,
  onSuccess,
}: NoSerieFormDialogProps) => {
  const { createSerie, updateSerie } = useNoSeries();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [formData, setFormData] = useState({
    code: "",
    description: "",
    documentType: "" as string,
    defaultNos: false,
    manualNos: false,
    dateOrder: false,
  });

  useEffect(() => {
    if (serie) {
      setFormData({
        code: serie.code,
        description: serie.description,
        documentType: serie.documentType?.toString() || "",
        defaultNos: serie.defaultNos,
        manualNos: serie.manualNos,
        dateOrder: serie.dateOrder,
      });
    } else {
      setFormData({
        code: "",
        description: "",
        documentType: "",
        defaultNos: false,
        manualNos: false,
        dateOrder: false,
      });
    }
  }, [serie, open]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      if (serie) {
        // Update
        const updateData: UpdateNoSerieDto = {
          description: formData.description,
          documentType: formData.documentType
            ? parseInt(formData.documentType)
            : undefined,
          defaultNos: formData.defaultNos,
          manualNos: formData.manualNos,
          dateOrder: formData.dateOrder,
        };

        await updateSerie({ id: serie.id, data: updateData });
        toast.success("Serie actualizada exitosamente");
      } else {
        // Create
        const createData: CreateNoSerieDto = {
          code: formData.code,
          description: formData.description,
          documentType: formData.documentType
            ? parseInt(formData.documentType)
            : undefined,
          defaultNos: formData.defaultNos,
          manualNos: formData.manualNos,
          dateOrder: formData.dateOrder,
        };

        await createSerie(createData);
        toast.success("Serie creada exitosamente");
      }

      onSuccess?.();
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Error al guardar la serie");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>
            {serie ? "Editar Serie Numérica" : "Nueva Serie Numérica"}
          </DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Code - solo en creación */}
          {!serie && (
            <div>
              <Label htmlFor="code">
                Código <span className="text-red-500">*</span>
              </Label>
              <Input
                id="code"
                value={formData.code}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    code: e.target.value.toUpperCase(),
                  })
                }
                placeholder="Ej: CUST, INV, PO"
                maxLength={20}
                required
              />
              <p className="text-xs text-gray-500 mt-1">
                Código único para identificar la serie (máx. 20 caracteres)
              </p>
            </div>
          )}

          {/* Description */}
          <div>
            <Label htmlFor="description">
              Descripción <span className="text-red-500">*</span>
            </Label>
            <Input
              id="description"
              value={formData.description}
              onChange={(e) =>
                setFormData({ ...formData, description: e.target.value })
              }
              placeholder="Ej: Numeración para Clientes"
              maxLength={1000}
              required
            />
          </div>

          {/* Document Type */}
          <div>
            <Label htmlFor="documentType">Tipo de Documento</Label>
            <Select
              value={formData.documentType}
              onValueChange={(value) =>
                setFormData({ ...formData, documentType: value })
              }
            >
              <SelectTrigger>
                <SelectValue placeholder="Seleccionar tipo..." />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="undefine">Sin tipo específico</SelectItem>
                {Object.entries(DocumentTypeLabels).map(([key, label]) => (
                  <SelectItem key={key} value={key}>
                    {label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <p className="text-xs text-gray-500 mt-1">
              Asociar esta serie a un tipo de documento
            </p>
          </div>

          {/* Checkboxes */}
          <div className="space-y-3">
            <div className="flex items-center space-x-2">
              <Checkbox
                id="defaultNos"
                checked={formData.defaultNos}
                onCheckedChange={(checked) =>
                  setFormData({ ...formData, defaultNos: checked as boolean })
                }
              />
              <Label htmlFor="defaultNos" className="cursor-pointer">
                Serie por defecto para este tipo de documento
              </Label>
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="manualNos"
                checked={formData.manualNos}
                onCheckedChange={(checked) =>
                  setFormData({ ...formData, manualNos: checked as boolean })
                }
              />
              <Label htmlFor="manualNos" className="cursor-pointer">
                Permitir numeración manual
              </Label>
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="dateOrder"
                checked={formData.dateOrder}
                onCheckedChange={(checked) =>
                  setFormData({ ...formData, dateOrder: checked as boolean })
                }
              />
              <Label htmlFor="dateOrder" className="cursor-pointer">
                Validar orden cronológico de fechas
              </Label>
            </div>
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Cancelar
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Guardando..." : serie ? "Actualizar" : "Crear"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default NoSerieFormDialog;
