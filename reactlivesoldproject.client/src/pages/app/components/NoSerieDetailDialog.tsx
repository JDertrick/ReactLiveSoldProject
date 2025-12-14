import { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { toast } from 'sonner';
import { useNoSeries } from '../../../hooks/useNoSeries';
import { NoSerie, DocumentTypeLabels, NoSerieLine } from '../../../types/noserie.types';
import { Plus, Edit, Trash2, CheckCircle, XCircle } from 'lucide-react';
import NoSerieLineFormDialog from './NoSerieLineFormDialog';

interface NoSerieDetailDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  serie: NoSerie | null;
  onEdit?: (serie: NoSerie) => void;
}

const NoSerieDetailDialog = ({
  open,
  onOpenChange,
  serie,
  onEdit,
}: NoSerieDetailDialogProps) => {
  const { deleteLine } = useNoSeries();
  const [isLineFormOpen, setIsLineFormOpen] = useState(false);
  const [isDeleteLineDialogOpen, setIsDeleteLineDialogOpen] = useState(false);
  const [selectedLine, setSelectedLine] = useState<NoSerieLine | null>(null);
  const [lineToDelete, setLineToDelete] = useState<NoSerieLine | null>(null);

  if (!serie) return null;

  const handleAddLine = () => {
    setSelectedLine(null);
    setIsLineFormOpen(true);
  };

  const handleEditLine = (line: NoSerieLine) => {
    setSelectedLine(line);
    setIsLineFormOpen(true);
  };

  const handleDeleteLineClick = (line: NoSerieLine) => {
    setLineToDelete(line);
    setIsDeleteLineDialogOpen(true);
  };

  const handleDeleteLineConfirm = async () => {
    if (!lineToDelete) return;

    try {
      await deleteLine(lineToDelete.id);
      toast.success('Línea eliminada exitosamente');
      setIsDeleteLineDialogOpen(false);
      setLineToDelete(null);
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Error al eliminar la línea');
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-ES');
  };

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="max-w-5xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Detalle de Serie: {serie.code}</DialogTitle>
          </DialogHeader>

          <div className="space-y-6">
            {/* Serie Information */}
            <div className="bg-gray-50 p-4 rounded-lg space-y-2">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm font-medium text-gray-500">Código</p>
                  <p className="text-lg font-semibold">{serie.code}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-gray-500">Descripción</p>
                  <p className="text-lg">{serie.description}</p>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm font-medium text-gray-500">Tipo de Documento</p>
                  <div className="mt-1">
                    {serie.documentType !== undefined && serie.documentType !== null ? (
                      <Badge variant="outline">
                        {DocumentTypeLabels[serie.documentType]}
                      </Badge>
                    ) : (
                      <span className="text-gray-400">Sin tipo</span>
                    )}
                  </div>
                </div>
                <div className="flex gap-4">
                  <div>
                    <p className="text-sm font-medium text-gray-500">Por Defecto</p>
                    <div className="mt-1">
                      {serie.defaultNos ? (
                        <Badge variant="default">Sí</Badge>
                      ) : (
                        <Badge variant="secondary">No</Badge>
                      )}
                    </div>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-gray-500">Manual</p>
                    <div className="mt-1">
                      {serie.manualNos ? (
                        <Badge variant="default">Sí</Badge>
                      ) : (
                        <Badge variant="secondary">No</Badge>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Lines Section */}
            <div>
              <div className="flex justify-between items-center mb-3">
                <h3 className="text-lg font-semibold">Líneas de Numeración</h3>
                <Button size="sm" onClick={handleAddLine}>
                  <Plus className="h-4 w-4 mr-2" />
                  Agregar Línea
                </Button>
              </div>

              <div className="rounded-md border">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Fecha Inicio</TableHead>
                      <TableHead>Número Inicial</TableHead>
                      <TableHead>Número Final</TableHead>
                      <TableHead>Último Usado</TableHead>
                      <TableHead>Incremento</TableHead>
                      <TableHead>Advertencia</TableHead>
                      <TableHead className="text-center">Abierta</TableHead>
                      <TableHead className="text-right">Acciones</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {serie.noSerieLines && serie.noSerieLines.length > 0 ? (
                      serie.noSerieLines.map((line) => (
                        <TableRow key={line.id}>
                          <TableCell>{formatDate(line.startingDate)}</TableCell>
                          <TableCell className="font-mono">{line.startingNo}</TableCell>
                          <TableCell className="font-mono">{line.endingNo}</TableCell>
                          <TableCell className="font-mono">
                            {line.lastNoUsed || (
                              <span className="text-gray-400">Sin usar</span>
                            )}
                          </TableCell>
                          <TableCell>{line.incrementBy}</TableCell>
                          <TableCell className="font-mono">
                            {line.warningNo || (
                              <span className="text-gray-400">N/A</span>
                            )}
                          </TableCell>
                          <TableCell className="text-center">
                            {line.open ? (
                              <CheckCircle className="h-5 w-5 text-green-500 mx-auto" />
                            ) : (
                              <XCircle className="h-5 w-5 text-gray-300 mx-auto" />
                            )}
                          </TableCell>
                          <TableCell className="text-right">
                            <div className="flex justify-end gap-2">
                              <Button
                                variant="ghost"
                                size="sm"
                                onClick={() => handleEditLine(line)}
                              >
                                <Edit className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="sm"
                                onClick={() => handleDeleteLineClick(line)}
                              >
                                <Trash2 className="h-4 w-4 text-red-500" />
                              </Button>
                            </div>
                          </TableCell>
                        </TableRow>
                      ))
                    ) : (
                      <TableRow>
                        <TableCell colSpan={8} className="text-center py-8 text-gray-500">
                          No hay líneas configuradas. Agrega una línea para comenzar.
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </div>
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => onEdit?.(serie)}>
              Editar Serie
            </Button>
            <Button onClick={() => onOpenChange(false)}>Cerrar</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Line Form Dialog */}
      <NoSerieLineFormDialog
        open={isLineFormOpen}
        onOpenChange={setIsLineFormOpen}
        serieId={serie.id}
        line={selectedLine}
        onSuccess={() => {
          setIsLineFormOpen(false);
          setSelectedLine(null);
        }}
      />

      {/* Delete Line Confirmation */}
      <AlertDialog open={isDeleteLineDialogOpen} onOpenChange={setIsDeleteLineDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Eliminar línea?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción eliminará la línea de numeración. Si esta línea ya ha sido utilizada,
              podría afectar la integridad de los números asignados.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDeleteLineConfirm}
              className="bg-red-500 hover:bg-red-600"
            >
              Eliminar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
};

export default NoSerieDetailDialog;
