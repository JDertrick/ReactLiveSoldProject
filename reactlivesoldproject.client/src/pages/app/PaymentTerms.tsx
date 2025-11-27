import { useState, useEffect } from 'react';
import { usePaymentTerms } from '../../hooks/usePaymentTerms';
import { PaymentTermsDto } from '../../types/purchases.types';
import { CustomAlertDialog } from '@/components/common/AlertDialog';
import { AlertDialogState } from '@/types/alertdialogstate.type';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useDebounce } from '@/hooks/useDebounce';
import { toast } from 'sonner';
import { Plus, Edit, Trash2, Calendar } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Textarea } from '@/components/ui/textarea';

const PaymentTermsPage = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearchTerm = useDebounce(searchTerm, 500);

  const { paymentTerms, loading, fetchPaymentTerms, createPaymentTerms, updatePaymentTerms, deletePaymentTerms } =
    usePaymentTerms();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingTerm, setEditingTerm] = useState<PaymentTermsDto | null>(null);

  const [formData, setFormData] = useState({
    code: '',
    description: '',
    dueDays: 0,
    discountPercentage: 0,
    discountDays: 0,
  });

  const [alertDialog, setAlertDialog] = useState<AlertDialogState>({
    open: false,
    title: '',
    description: '',
  });

  useEffect(() => {
    fetchPaymentTerms();
  }, [fetchPaymentTerms]);

  const filteredTerms = paymentTerms.filter((term) => {
    if (!debouncedSearchTerm) return true;
    const searchLower = debouncedSearchTerm.toLowerCase();
    return (
      term.code.toLowerCase().includes(searchLower) ||
      term.description.toLowerCase().includes(searchLower)
    );
  });

  const handleOpenModal = (term?: PaymentTermsDto) => {
    if (term) {
      setEditingTerm(term);
      setFormData({
        code: term.code,
        description: term.description,
        dueDays: term.dueDays,
        discountPercentage: term.discountPercentage,
        discountDays: term.discountDays,
      });
    } else {
      setEditingTerm(null);
      setFormData({
        code: '',
        description: '',
        dueDays: 0,
        discountPercentage: 0,
        discountDays: 0,
      });
    }
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingTerm(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (editingTerm) {
        await updatePaymentTerms(editingTerm.id, formData);
        toast.success('Término de pago actualizado exitosamente');
      } else {
        await createPaymentTerms(formData);
        toast.success('Término de pago creado exitosamente');
      }
      handleCloseModal();
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: 'Error',
        description: error.message || 'Error al guardar el término de pago',
      });
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await deletePaymentTerms(id);
      toast.success('Término de pago eliminado exitosamente');
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: 'Error',
        description: error.message || 'Error al eliminar el término de pago',
      });
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Términos de Pago</h1>
          <p className="text-muted-foreground mt-1">
            Define los términos de pago para tus proveedores
          </p>
        </div>
        <Button onClick={() => handleOpenModal()} size="lg" className="gap-2">
          <Plus className="w-5 h-5" />
          Nuevo Término de Pago
        </Button>
      </div>

      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <Input
              placeholder="Buscar por código o descripción..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="max-w-sm"
            />
          </div>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="flex justify-center items-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
            </div>
          ) : filteredTerms.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              No se encontraron términos de pago
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Código</TableHead>
                  <TableHead>Descripción</TableHead>
                  <TableHead>Días de Vencimiento</TableHead>
                  <TableHead>Descuento</TableHead>
                  <TableHead>Días Descuento</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredTerms.map((term) => (
                  <TableRow key={term.id}>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <Calendar className="h-4 w-4 text-muted-foreground" />
                        <span className="font-medium font-mono">{term.code}</span>
                      </div>
                    </TableCell>
                    <TableCell>{term.description}</TableCell>
                    <TableCell>
                      <Badge variant="outline">{term.dueDays} días</Badge>
                    </TableCell>
                    <TableCell>
                      {term.discountPercentage > 0 ? (
                        <Badge variant="secondary">{term.discountPercentage}%</Badge>
                      ) : (
                        <span className="text-muted-foreground">-</span>
                      )}
                    </TableCell>
                    <TableCell>
                      {term.discountDays > 0 ? (
                        <span>{term.discountDays} días</span>
                      ) : (
                        <span className="text-muted-foreground">-</span>
                      )}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleOpenModal(term)}
                        >
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleDelete(term.id)}
                        >
                          <Trash2 className="h-4 w-4 text-destructive" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Modal de Formulario */}
      <Dialog open={isModalOpen} onOpenChange={setIsModalOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>
              {editingTerm ? 'Editar Término de Pago' : 'Nuevo Término de Pago'}
            </DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit}>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="code">Código *</Label>
                <Input
                  id="code"
                  value={formData.code}
                  onChange={(e) => setFormData({ ...formData, code: e.target.value })}
                  placeholder="Ej: NET30, 2/10NET30"
                  required
                />
                <p className="text-xs text-muted-foreground">
                  Código único para identificar el término
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="description">Descripción *</Label>
                <Textarea
                  id="description"
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Ej: Neto 30 días, 2% descuento si se paga en 10 días"
                  rows={3}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="dueDays">Días de Vencimiento *</Label>
                <Input
                  id="dueDays"
                  type="number"
                  min="0"
                  value={formData.dueDays}
                  onChange={(e) =>
                    setFormData({ ...formData, dueDays: parseInt(e.target.value) || 0 })
                  }
                  required
                />
                <p className="text-xs text-muted-foreground">
                  Días hasta el vencimiento del pago
                </p>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="discountPercentage">Descuento % (Opcional)</Label>
                  <Input
                    id="discountPercentage"
                    type="number"
                    step="0.01"
                    min="0"
                    max="100"
                    value={formData.discountPercentage}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        discountPercentage: parseFloat(e.target.value) || 0,
                      })
                    }
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="discountDays">Días Descuento (Opcional)</Label>
                  <Input
                    id="discountDays"
                    type="number"
                    min="0"
                    value={formData.discountDays}
                    onChange={(e) =>
                      setFormData({ ...formData, discountDays: parseInt(e.target.value) || 0 })
                    }
                  />
                </div>
              </div>

              <div className="bg-muted p-3 rounded-md text-sm">
                <p className="font-medium mb-1">Ejemplo:</p>
                <p className="text-muted-foreground">
                  "2/10 Net 30" = 2% descuento si se paga en 10 días, de lo contrario neto a 30 días
                </p>
              </div>
            </div>

            <DialogFooter>
              <Button type="button" variant="outline" onClick={handleCloseModal}>
                Cancelar
              </Button>
              <Button type="submit">
                {editingTerm ? 'Actualizar' : 'Crear'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <CustomAlertDialog
        open={alertDialog.open}
        onClose={() => setAlertDialog({ ...alertDialog, open: false })}
        title={alertDialog.title}
        description={alertDialog.description}
      />
    </div>
  );
};

export default PaymentTermsPage;
