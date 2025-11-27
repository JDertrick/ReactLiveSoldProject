import { useState, useEffect } from 'react';
import { useProductVendors } from '../../hooks/useProductVendors';
import { useGetProducts } from '../../hooks/useProducts';
import { useGetVendors } from '../../hooks/useVendors';
import { ProductVendorDto } from '../../types/purchases.types';
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
import { Plus, Edit, Trash2, Star, Package } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';

const ProductVendorsPage = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearchTerm = useDebounce(searchTerm, 500);

  const { productVendors, loading, fetchProductVendors, createProductVendor, updateProductVendor, deleteProductVendor } =
    useProductVendors();

  const { data: productsData } = useGetProducts(1, 1000, 'all', '');
  const products = productsData?.items || [];
  const { data: vendors } = useGetVendors();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingRelation, setEditingRelation] = useState<ProductVendorDto | null>(null);

  const [formData, setFormData] = useState({
    productId: '',
    vendorId: '',
    vendorSku: '',
    unitCost: 0,
    minimumOrderQuantity: 1,
    leadTimeDays: 0,
    isPreferred: false,
  });

  const [alertDialog, setAlertDialog] = useState<AlertDialogState>({
    open: false,
    title: '',
    description: '',
  });

  useEffect(() => {
    fetchProductVendors();
  }, [fetchProductVendors]);

  const filteredRelations = productVendors.filter((relation) => {
    if (!debouncedSearchTerm) return true;
    const searchLower = debouncedSearchTerm.toLowerCase();
    return (
      relation.productName?.toLowerCase().includes(searchLower) ||
      relation.vendorName?.toLowerCase().includes(searchLower) ||
      relation.vendorSku?.toLowerCase().includes(searchLower)
    );
  });

  const handleOpenModal = (relation?: ProductVendorDto) => {
    if (relation) {
      setEditingRelation(relation);
      setFormData({
        productId: relation.productId,
        vendorId: relation.vendorId,
        vendorSku: relation.vendorSku || '',
        unitCost: relation.unitCost,
        minimumOrderQuantity: relation.minimumOrderQuantity,
        leadTimeDays: relation.leadTimeDays,
        isPreferred: relation.isPreferred,
      });
    } else {
      setEditingRelation(null);
      setFormData({
        productId: '',
        vendorId: '',
        vendorSku: '',
        unitCost: 0,
        minimumOrderQuantity: 1,
        leadTimeDays: 0,
        isPreferred: false,
      });
    }
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingRelation(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (editingRelation) {
        await updateProductVendor(editingRelation.id, formData);
        toast.success('Relación actualizada exitosamente');
      } else {
        await createProductVendor(formData);
        toast.success('Relación creada exitosamente');
      }
      handleCloseModal();
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: 'Error',
        description: error.message || 'Error al guardar la relación',
      });
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteProductVendor(id);
      toast.success('Relación eliminada exitosamente');
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: 'Error',
        description: error.message || 'Error al eliminar la relación',
      });
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN',
    }).format(amount);
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Productos - Proveedores</h1>
          <p className="text-muted-foreground mt-1">
            Gestiona la relación entre productos y sus proveedores
          </p>
        </div>
        <Button onClick={() => handleOpenModal()} size="lg" className="gap-2">
          <Plus className="w-5 h-5" />
          Nueva Relación
        </Button>
      </div>

      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <Input
              placeholder="Buscar por producto, proveedor o SKU..."
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
          ) : filteredRelations.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              No se encontraron relaciones producto-proveedor
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Producto</TableHead>
                  <TableHead>Proveedor</TableHead>
                  <TableHead>SKU Proveedor</TableHead>
                  <TableHead>Costo Unit.</TableHead>
                  <TableHead>Cant. Mín.</TableHead>
                  <TableHead>Tiempo Entrega</TableHead>
                  <TableHead>Estado</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredRelations.map((relation) => (
                  <TableRow key={relation.id}>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <Package className="h-4 w-4 text-muted-foreground" />
                        <span className="font-medium">{relation.productName}</span>
                      </div>
                    </TableCell>
                    <TableCell>{relation.vendorName}</TableCell>
                    <TableCell className="font-mono">{relation.vendorSku || '-'}</TableCell>
                    <TableCell className="font-medium">
                      {formatCurrency(relation.unitCost)}
                    </TableCell>
                    <TableCell>{relation.minimumOrderQuantity}</TableCell>
                    <TableCell>
                      <Badge variant="outline">{relation.leadTimeDays} días</Badge>
                    </TableCell>
                    <TableCell>
                      {relation.isPreferred ? (
                        <Badge className="gap-1">
                          <Star className="h-3 w-3 fill-current" />
                          Preferido
                        </Badge>
                      ) : (
                        <Badge variant="secondary">Alternativo</Badge>
                      )}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleOpenModal(relation)}
                        >
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleDelete(relation.id)}
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
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {editingRelation ? 'Editar Relación Producto-Proveedor' : 'Nueva Relación Producto-Proveedor'}
            </DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit}>
            <div className="space-y-4 py-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="productId">Producto *</Label>
                  <Select
                    value={formData.productId}
                    onValueChange={(value) => setFormData({ ...formData, productId: value })}
                    disabled={!!editingRelation}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione un producto" />
                    </SelectTrigger>
                    <SelectContent>
                      {products?.map((product) => (
                        <SelectItem key={product.productId} value={product.productId}>
                          {product.productName} {product.variantName ? `- ${product.variantName}` : ''}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="vendorId">Proveedor *</Label>
                  <Select
                    value={formData.vendorId}
                    onValueChange={(value) => setFormData({ ...formData, vendorId: value })}
                    disabled={!!editingRelation}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione un proveedor" />
                    </SelectTrigger>
                    <SelectContent>
                      {vendors?.map((vendor) => (
                        <SelectItem key={vendor.id} value={vendor.id}>
                          {vendor.contact?.company || `${vendor.contact?.firstName} ${vendor.contact?.lastName}`}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="vendorSku">SKU del Proveedor (Opcional)</Label>
                <Input
                  id="vendorSku"
                  value={formData.vendorSku}
                  onChange={(e) => setFormData({ ...formData, vendorSku: e.target.value })}
                  placeholder="Código del producto en el catálogo del proveedor"
                />
              </div>

              <div className="grid grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="unitCost">Costo Unitario *</Label>
                  <Input
                    id="unitCost"
                    type="number"
                    step="0.01"
                    min="0"
                    value={formData.unitCost}
                    onChange={(e) =>
                      setFormData({ ...formData, unitCost: parseFloat(e.target.value) || 0 })
                    }
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="minimumOrderQuantity">Cantidad Mínima *</Label>
                  <Input
                    id="minimumOrderQuantity"
                    type="number"
                    min="1"
                    value={formData.minimumOrderQuantity}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        minimumOrderQuantity: parseInt(e.target.value) || 1,
                      })
                    }
                    required
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="leadTimeDays">Días de Entrega *</Label>
                  <Input
                    id="leadTimeDays"
                    type="number"
                    min="0"
                    value={formData.leadTimeDays}
                    onChange={(e) =>
                      setFormData({ ...formData, leadTimeDays: parseInt(e.target.value) || 0 })
                    }
                    required
                  />
                </div>
              </div>

              <div className="flex items-center space-x-2 p-4 bg-muted rounded-lg">
                <Checkbox
                  id="isPreferred"
                  checked={formData.isPreferred}
                  onCheckedChange={(checked) =>
                    setFormData({ ...formData, isPreferred: checked as boolean })
                  }
                />
                <div className="flex-1">
                  <Label
                    htmlFor="isPreferred"
                    className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer"
                  >
                    Marcar como Proveedor Preferido
                  </Label>
                  <p className="text-xs text-muted-foreground mt-1">
                    Solo puede haber un proveedor preferido por producto
                  </p>
                </div>
              </div>
            </div>

            <DialogFooter>
              <Button type="button" variant="outline" onClick={handleCloseModal}>
                Cancelar
              </Button>
              <Button type="submit">
                {editingRelation ? 'Actualizar' : 'Crear'}
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

export default ProductVendorsPage;
