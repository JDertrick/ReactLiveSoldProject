import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { toast } from 'sonner';
import { useNoSeries } from '../../../hooks/useNoSeries';
import {
  NoSerieLine,
  CreateNoSerieLineDto,
  UpdateNoSerieLineDto,
} from '../../../types/noserie.types';

interface NoSerieLineFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  serieId: string;
  line?: NoSerieLine | null;
  onSuccess?: () => void;
}

const NoSerieLineFormDialog = ({
  open,
  onOpenChange,
  serieId,
  line,
  onSuccess,
}: NoSerieLineFormDialogProps) => {
  const { addLine, updateLine } = useNoSeries();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [formData, setFormData] = useState({
    startingDate: '',
    startingNo: '',
    endingNo: '',
    incrementBy: 1,
    warningNo: '',
    open: true,
  });

  useEffect(() => {
    if (line) {
      setFormData({
        startingDate: line.startingDate.split('T')[0],
        startingNo: line.startingNo,
        endingNo: line.endingNo,
        incrementBy: line.incrementBy,
        warningNo: line.warningNo || '',
        open: line.open,
      });
    } else {
      // Default to today
      const today = new Date().toISOString().split('T')[0];
      setFormData({
        startingDate: today,
        startingNo: '',
        endingNo: '',
        incrementBy: 1,
        warningNo: '',
        open: true,
      });
    }
  }, [line, open]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      if (line) {
        // Update
        const updateData: UpdateNoSerieLineDto = {
          startingDate: formData.startingDate,
          startingNo: formData.startingNo,
          endingNo: formData.endingNo,
          incrementBy: formData.incrementBy,
          warningNo: formData.warningNo || undefined,
          open: formData.open,
        };

        await updateLine({ lineId: line.id, data: updateData });
        toast.success('Línea actualizada exitosamente');
      } else {
        // Create
        const createData: CreateNoSerieLineDto = {
          startingDate: formData.startingDate,
          startingNo: formData.startingNo,
          endingNo: formData.endingNo,
          incrementBy: formData.incrementBy,
          warningNo: formData.warningNo || undefined,
          open: formData.open,
        };

        await addLine({ serieId, data: createData });
        toast.success('Línea creada exitosamente');
      }

      onSuccess?.();
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Error al guardar la línea');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>
            {line ? 'Editar Línea de Numeración' : 'Nueva Línea de Numeración'}
          </DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Starting Date */}
          <div>
            <Label htmlFor="startingDate">
              Fecha de Inicio <span className="text-red-500">*</span>
            </Label>
            <Input
              id="startingDate"
              type="date"
              value={formData.startingDate}
              onChange={(e) => setFormData({ ...formData, startingDate: e.target.value })}
              required
            />
            <p className="text-xs text-gray-500 mt-1">
              La línea será válida desde esta fecha en adelante
            </p>
          </div>

          {/* Starting Number */}
          <div>
            <Label htmlFor="startingNo">
              Número Inicial <span className="text-red-500">*</span>
            </Label>
            <Input
              id="startingNo"
              value={formData.startingNo}
              onChange={(e) => setFormData({ ...formData, startingNo: e.target.value })}
              placeholder="Ej: INV-0001, CUST-0001"
              maxLength={20}
              required
            />
            <p className="text-xs text-gray-500 mt-1">
              Primer número del rango (debe tener parte numérica al final)
            </p>
          </div>

          {/* Ending Number */}
          <div>
            <Label htmlFor="endingNo">
              Número Final <span className="text-red-500">*</span>
            </Label>
            <Input
              id="endingNo"
              value={formData.endingNo}
              onChange={(e) => setFormData({ ...formData, endingNo: e.target.value })}
              placeholder="Ej: INV-9999, CUST-9999"
              maxLength={20}
              required
            />
            <p className="text-xs text-gray-500 mt-1">Último número del rango</p>
          </div>

          {/* Increment By */}
          <div>
            <Label htmlFor="incrementBy">
              Incremento <span className="text-red-500">*</span>
            </Label>
            <Input
              id="incrementBy"
              type="number"
              min={1}
              max={100}
              value={formData.incrementBy}
              onChange={(e) =>
                setFormData({ ...formData, incrementBy: parseInt(e.target.value) || 1 })
              }
              required
            />
            <p className="text-xs text-gray-500 mt-1">
              Cantidad a incrementar (normalmente 1)
            </p>
          </div>

          {/* Warning Number (optional) */}
          <div>
            <Label htmlFor="warningNo">Número de Advertencia (Opcional)</Label>
            <Input
              id="warningNo"
              value={formData.warningNo}
              onChange={(e) => setFormData({ ...formData, warningNo: e.target.value })}
              placeholder="Ej: INV-9900"
              maxLength={20}
            />
            <p className="text-xs text-gray-500 mt-1">
              Número que activará una advertencia cuando se acerque al final
            </p>
          </div>

          {/* Open Checkbox */}
          <div className="flex items-center space-x-2">
            <Checkbox
              id="open"
              checked={formData.open}
              onCheckedChange={(checked) => setFormData({ ...formData, open: checked as boolean })}
            />
            <Label htmlFor="open" className="cursor-pointer">
              Línea abierta (disponible para uso)
            </Label>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancelar
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Guardando...' : line ? 'Actualizar' : 'Crear'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};

export default NoSerieLineFormDialog;
