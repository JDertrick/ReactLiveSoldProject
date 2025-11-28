import { useState, useEffect } from 'react';
import api from '../../services/api';
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
import { Textarea } from '@/components/ui/textarea';
import { useDebounce } from '@/hooks/useDebounce';
import { toast } from 'sonner';
import { Plus, Edit, Trash2, FileText } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

interface ChartOfAccountDto {
  id: string;
  accountCode: string;
  accountName: string;
  accountType: string;
  parentAccountId?: string;
  parentAccountName?: string;
  description?: string;
  isActive: boolean;
}

const accountTypes = [
  { value: 'Asset', label: 'Activo' },
  { value: 'Liability', label: 'Pasivo' },
  { value: 'Equity', label: 'Capital' },
  { value: 'Revenue', label: 'Ingreso' },
  { value: 'Expense', label: 'Gasto' },
  { value: 'CostOfGoodsSold', label: 'Costo de Ventas' },
];

const ChartOfAccountsPage = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearchTerm = useDebounce(searchTerm, 500);
  const [accounts, setAccounts] = useState<ChartOfAccountDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingAccount, setEditingAccount] = useState<ChartOfAccountDto | null>(null);

  const [formData, setFormData] = useState({
    accountCode: '',
    accountName: '',
    accountType: 'Asset',
    parentAccountId: '',
    description: '',
  });

  const [alertDialog, setAlertDialog] = useState<AlertDialogState>({
    open: false,
    title: '',
    description: '',
  });

  useEffect(() => {
    fetchAccounts();
  }, []);

  const fetchAccounts = async () => {
    setLoading(true);
    try {
      const response = await api.get('/Accounting/accounts');
      setAccounts(response.data);
    } catch (error: any) {
      toast.error('Error al cargar cuentas contables');
    } finally {
      setLoading(false);
    }
  };

  const filteredAccounts = accounts.filter((account) => {
    if (!debouncedSearchTerm) return true;
    const searchLower = debouncedSearchTerm.toLowerCase();
    return (
      account.accountCode.toLowerCase().includes(searchLower) ||
      account.accountName.toLowerCase().includes(searchLower) ||
      account.accountType.toLowerCase().includes(searchLower)
    );
  });

  const handleOpenModal = (account?: ChartOfAccountDto) => {
    if (account) {
      setEditingAccount(account);
      setFormData({
        accountCode: account.accountCode,
        accountName: account.accountName,
        accountType: account.accountType,
        parentAccountId: account.parentAccountId || '',
        description: account.description || '',
      });
    } else {
      setEditingAccount(null);
      setFormData({
        accountCode: '',
        accountName: '',
        accountType: 'Asset',
        parentAccountId: '',
        description: '',
      });
    }
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingAccount(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.accountCode || !formData.accountName) {
      toast.error('Código y nombre de cuenta son obligatorios');
      return;
    }

    try {
      if (editingAccount) {
        await api.put(`/Accounting/accounts/${editingAccount.id}`, {
          accountName: formData.accountName,
          accountType: formData.accountType,
          description: formData.description,
        });
        toast.success('Cuenta contable actualizada exitosamente');
      } else {
        await api.post('/Accounting/accounts', {
          accountCode: formData.accountCode,
          accountName: formData.accountName,
          accountType: formData.accountType,
          parentAccountId: formData.parentAccountId || null,
          description: formData.description,
        });
        toast.success('Cuenta contable creada exitosamente');
      }
      handleCloseModal();
      fetchAccounts();
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: 'Error',
        description: error.response?.data?.message || error.message || 'Error al guardar la cuenta contable',
      });
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await api.delete(`/Accounting/accounts/${id}`);
      toast.success('Cuenta contable eliminada exitosamente');
      fetchAccounts();
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: 'Error',
        description: error.response?.data?.message || error.message || 'Error al eliminar la cuenta contable',
      });
    }
  };

  const getAccountTypeBadge = (type: string) => {
    const typeConfig: Record<string, { variant: any; label: string }> = {
      Asset: { variant: 'default', label: 'Activo' },
      Liability: { variant: 'destructive', label: 'Pasivo' },
      Equity: { variant: 'secondary', label: 'Capital' },
      Revenue: { variant: 'default', label: 'Ingreso' },
      Expense: { variant: 'outline', label: 'Gasto' },
      CostOfGoodsSold: { variant: 'outline', label: 'Costo de Ventas' },
    };
    const config = typeConfig[type] || { variant: 'outline', label: type };
    return <Badge variant={config.variant}>{config.label}</Badge>;
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Catálogo de Cuentas</h1>
          <p className="text-muted-foreground mt-1">
            Gestiona el plan de cuentas contables de tu organización
          </p>
        </div>
        <Button onClick={() => handleOpenModal()} size="lg" className="gap-2">
          <Plus className="w-5 h-5" />
          Nueva Cuenta
        </Button>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center gap-4">
            <Input
              placeholder="Buscar por código, nombre o tipo..."
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
          ) : filteredAccounts.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              <FileText className="mx-auto h-12 w-12 mb-4 opacity-50" />
              <p>No se encontraron cuentas contables</p>
              <p className="text-sm mt-2">Crea tu primera cuenta para comenzar</p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Código</TableHead>
                  <TableHead>Nombre</TableHead>
                  <TableHead>Tipo</TableHead>
                  <TableHead>Cuenta Padre</TableHead>
                  <TableHead>Descripción</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredAccounts.map((account) => (
                  <TableRow key={account.id}>
                    <TableCell className="font-medium">{account.accountCode}</TableCell>
                    <TableCell>{account.accountName}</TableCell>
                    <TableCell>{getAccountTypeBadge(account.accountType)}</TableCell>
                    <TableCell>{account.parentAccountName || '-'}</TableCell>
                    <TableCell className="max-w-xs truncate">
                      {account.description || '-'}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleOpenModal(account)}
                        >
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleDelete(account.id)}
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
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>
              {editingAccount ? 'Editar Cuenta Contable' : 'Nueva Cuenta Contable'}
            </DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit}>
            <div className="space-y-4 py-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="accountCode">Código de Cuenta *</Label>
                  <Input
                    id="accountCode"
                    value={formData.accountCode}
                    onChange={(e) => setFormData({ ...formData, accountCode: e.target.value })}
                    placeholder="Ej: 1001"
                    required
                    disabled={!!editingAccount}
                  />
                  {editingAccount && (
                    <p className="text-xs text-muted-foreground">
                      El código no se puede modificar
                    </p>
                  )}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="accountType">Tipo de Cuenta *</Label>
                  <Select
                    value={formData.accountType}
                    onValueChange={(value) => setFormData({ ...formData, accountType: value })}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {accountTypes.map((type) => (
                        <SelectItem key={type.value} value={type.value}>
                          {type.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="accountName">Nombre de Cuenta *</Label>
                <Input
                  id="accountName"
                  value={formData.accountName}
                  onChange={(e) => setFormData({ ...formData, accountName: e.target.value })}
                  placeholder="Ej: Banco BBVA - Cuenta de Cheques"
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="parentAccountId">Cuenta Padre (Opcional)</Label>
                <Select
                  value={formData.parentAccountId || 'none'}
                  onValueChange={(value) =>
                    setFormData({ ...formData, parentAccountId: value === 'none' ? '' : value })
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Sin cuenta padre" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">Sin cuenta padre</SelectItem>
                    {accounts
                      .filter((acc) => acc.id !== editingAccount?.id)
                      .map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.accountCode} - {account.accountName}
                        </SelectItem>
                      ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="description">Descripción</Label>
                <Textarea
                  id="description"
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Descripción opcional de la cuenta..."
                  rows={3}
                />
              </div>
            </div>

            <DialogFooter>
              <Button type="button" variant="outline" onClick={handleCloseModal}>
                Cancelar
              </Button>
              <Button type="submit">
                {editingAccount ? 'Actualizar' : 'Crear'}
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

export default ChartOfAccountsPage;
