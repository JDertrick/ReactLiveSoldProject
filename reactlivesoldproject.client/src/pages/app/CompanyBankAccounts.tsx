import { useState, useEffect } from 'react';
import { useCompanyBankAccounts } from '../../hooks/useCompanyBankAccounts';
import { CompanyBankAccountDto } from '../../types/purchases.types';
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
import { Plus, Edit, Trash2, Building2 } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import api from '../../services/api';

const CompanyBankAccountsPage = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearchTerm = useDebounce(searchTerm, 500);

  const { companyBankAccounts, loading, fetchCompanyBankAccounts, createCompanyBankAccount, updateCompanyBankAccount, deleteCompanyBankAccount } =
    useCompanyBankAccounts();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingAccount, setEditingAccount] = useState<CompanyBankAccountDto | null>(null);
  const [glAccounts, setGlAccounts] = useState<any[]>([]);

  const [formData, setFormData] = useState({
    bankName: '',
    accountNumber: '',
    currency: 'MXN',
    currentBalance: 0,
    glAccountId: '',
  });

  const [alertDialog, setAlertDialog] = useState<AlertDialogState>({
    open: false,
    title: '',
    description: '',
  });

  useEffect(() => {
    fetchCompanyBankAccounts();
    fetchGLAccounts();
  }, [fetchCompanyBankAccounts]);

  const fetchGLAccounts = async () => {
    try {
      const response = await api.get('/Accounting/accounts');
      // Filtrar solo cuentas de tipo Asset (Bank)
      const bankAccounts = response.data.filter((acc: any) =>
        acc.accountType === 'Asset' && (acc.accountName.toLowerCase().includes('banco') || acc.accountName.toLowerCase().includes('bank'))
      );
      setGlAccounts(bankAccounts.length > 0 ? bankAccounts : response.data.filter((acc: any) => acc.accountType === 'Asset'));
    } catch (error) {
      console.error('Error al cargar cuentas contables:', error);
    }
  };

  const filteredAccounts = companyBankAccounts.filter((account) => {
    if (!debouncedSearchTerm) return true;
    const searchLower = debouncedSearchTerm.toLowerCase();
    return (
      account.bankName.toLowerCase().includes(searchLower) ||
      account.accountNumber.toLowerCase().includes(searchLower) ||
      account.currency.toLowerCase().includes(searchLower)
    );
  });

  const handleOpenModal = (account?: CompanyBankAccountDto) => {
    if (account) {
      setEditingAccount(account);
      setFormData({
        bankName: account.bankName,
        accountNumber: account.accountNumber,
        currency: account.currency,
        currentBalance: account.currentBalance,
        glAccountId: account.glAccountId || '',
      });
    } else {
      setEditingAccount(null);
      setFormData({
        bankName: '',
        accountNumber: '',
        currency: 'MXN',
        currentBalance: 0,
        glAccountId: '',
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

    if (!formData.glAccountId) {
      toast.error('Debe seleccionar una cuenta contable');
      return;
    }

    try {
      if (editingAccount) {
        await updateCompanyBankAccount(editingAccount.id, formData);
        toast.success('Cuenta bancaria actualizada exitosamente');
      } else {
        await createCompanyBankAccount(formData);
        toast.success('Cuenta bancaria creada exitosamente');
      }
      handleCloseModal();
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: 'Error',
        description: error.message || 'Error al guardar la cuenta bancaria',
      });
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteCompanyBankAccount(id);
      toast.success('Cuenta bancaria eliminada exitosamente');
    } catch (error: any) {
      setAlertDialog({
        open: true,
        title: 'Error',
        description: error.message || 'Error al eliminar la cuenta bancaria',
      });
    }
  };

  const formatCurrency = (amount: number, currency: string = 'CO') => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: currency,
    }).format(amount);
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Cuentas Bancarias de la Empresa</h1>
          <p className="text-muted-foreground mt-1">
            Gestiona las cuentas bancarias de tu empresa
          </p>
        </div>
        <Button onClick={() => handleOpenModal()} size="lg" className="gap-2">
          <Plus className="w-5 h-5" />
          Nueva Cuenta Bancaria
        </Button>
      </div>

      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <Input
              placeholder="Buscar por banco, número de cuenta o moneda..."
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
              No se encontraron cuentas bancarias
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Banco</TableHead>
                  <TableHead>Número de Cuenta</TableHead>
                  <TableHead>Moneda</TableHead>
                  <TableHead className="text-right">Saldo Actual</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredAccounts.map((account) => (
                  <TableRow key={account.id}>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <Building2 className="h-4 w-4 text-muted-foreground" />
                        <span className="font-medium">{account.bankName}</span>
                      </div>
                    </TableCell>
                    <TableCell className="font-mono">{account.accountNumber}</TableCell>
                    <TableCell>
                      <Badge variant="outline">{account.currency}</Badge>
                    </TableCell>
                    <TableCell className="text-right font-medium">
                      {formatCurrency(account.currentBalance, account.currency)}
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
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>
              {editingAccount ? 'Editar Cuenta Bancaria' : 'Nueva Cuenta Bancaria'}
            </DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit}>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="bankName">Nombre del Banco *</Label>
                <Input
                  id="bankName"
                  value={formData.bankName}
                  onChange={(e) => setFormData({ ...formData, bankName: e.target.value })}
                  placeholder="Ej: BBVA Bancomer"
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="accountNumber">Número de Cuenta *</Label>
                <Input
                  id="accountNumber"
                  value={formData.accountNumber}
                  onChange={(e) => setFormData({ ...formData, accountNumber: e.target.value })}
                  placeholder="Ej: 0123456789"
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="currency">Moneda *</Label>
                <Select
                  value={formData.currency}
                  onValueChange={(value) => setFormData({ ...formData, currency: value })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="CO">CO - Peso Colombiano</SelectItem>
                    {/* <SelectItem value="USD">USD - Dólar Americano</SelectItem>
                    <SelectItem value="EUR">EUR - Euro</SelectItem> */}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="currentBalance">Saldo Actual *</Label>
                <Input
                  id="currentBalance"
                  type="number"
                  step="0.01"
                  value={formData.currentBalance}
                  onChange={(e) =>
                    setFormData({ ...formData, currentBalance: parseFloat(e.target.value) || 0 })
                  }
                  required
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="glAccountId">Cuenta Contable *</Label>
                <Select
                  value={formData.glAccountId}
                  onValueChange={(value) => setFormData({ ...formData, glAccountId: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Seleccione una cuenta contable" />
                  </SelectTrigger>
                  <SelectContent>
                    {glAccounts.map((account) => (
                      <SelectItem key={account.id} value={account.id}>
                        {account.accountCode} - {account.accountName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
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

export default CompanyBankAccountsPage;
