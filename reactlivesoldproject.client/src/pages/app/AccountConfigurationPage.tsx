import { useState, useEffect } from 'react';
import { useAccountConfiguration } from '../../hooks/useAccountConfiguration';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { toast } from 'sonner';
import { Save, AlertCircle } from 'lucide-react';
import api from '../../services/api';

interface ChartOfAccount {
  id: string;
  accountCode: string;
  accountName: string;
  accountType: string;
}

const AccountConfigurationPage = () => {
  const { configuration, loading, fetchConfiguration, updateConfiguration } = useAccountConfiguration();
  const [chartOfAccounts, setChartOfAccounts] = useState<ChartOfAccount[]>([]);
  const [loadingAccounts, setLoadingAccounts] = useState(true);
  const [formData, setFormData] = useState({
    inventoryAccountId: '',
    accountsPayableAccountId: '',
    accountsReceivableAccountId: '',
    salesRevenueAccountId: '',
    costOfGoodsSoldAccountId: '',
    taxPayableAccountId: '',
    taxReceivableAccountId: '',
    cashAccountId: '',
    defaultBankAccountId: '',
  });

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    if (configuration) {
      setFormData({
        inventoryAccountId: configuration.inventoryAccountId || '',
        accountsPayableAccountId: configuration.accountsPayableAccountId || '',
        accountsReceivableAccountId: configuration.accountsReceivableAccountId || '',
        salesRevenueAccountId: configuration.salesRevenueAccountId || '',
        costOfGoodsSoldAccountId: configuration.costOfGoodsSoldAccountId || '',
        taxPayableAccountId: configuration.taxPayableAccountId || '',
        taxReceivableAccountId: configuration.taxReceivableAccountId || '',
        cashAccountId: configuration.cashAccountId || '',
        defaultBankAccountId: configuration.defaultBankAccountId || '',
      });
    }
  }, [configuration]);

  const loadData = async () => {
    try {
      setLoadingAccounts(true);
      const [configResponse, accountsResponse] = await Promise.all([
        fetchConfiguration(),
        api.get<ChartOfAccount[]>('/Accounting/accounts'),
      ]);
      setChartOfAccounts(accountsResponse.data);
    } catch (error: any) {
      console.error('Error loading data:', error);
      toast.error('Error al cargar los datos');
    } finally {
      setLoadingAccounts(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      // Filtrar campos vacíos para no enviar strings vacíos
      const cleanData: any = {};
      Object.entries(formData).forEach(([key, value]) => {
        if (value && value !== '') {
          cleanData[key] = value;
        }
      });

      await updateConfiguration(cleanData);
      toast.success('Configuración guardada exitosamente');
    } catch (error: any) {
      toast.error(error.message || 'Error al guardar la configuración');
    }
  };

  const accountsByType = {
    Asset: chartOfAccounts.filter((acc) => acc.accountType === 'Asset'),
    Liability: chartOfAccounts.filter((acc) => acc.accountType === 'Liability'),
    Equity: chartOfAccounts.filter((acc) => acc.accountType === 'Equity'),
    Income: chartOfAccounts.filter((acc) => acc.accountType === 'Income'),
    Expense: chartOfAccounts.filter((acc) => acc.accountType === 'Expense'),
  };

  if (loading || loadingAccounts) {
    return (
      <div className="flex justify-center items-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Configuración de Cuentas Contables</h1>
        <p className="text-muted-foreground mt-1">
          Configure las cuentas contables por defecto para transacciones automáticas
        </p>
      </div>

      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <div className="flex items-start gap-3">
          <AlertCircle className="h-5 w-5 text-blue-600 mt-0.5" />
          <div className="flex-1">
            <p className="text-sm text-blue-900 font-medium">Importante</p>
            <p className="text-sm text-blue-800 mt-1">
              Estas cuentas se usarán automáticamente para generar asientos contables en operaciones
              como recepciones de compra, ventas, pagos, etc. Consulte con su contador antes de
              realizar cambios.
            </p>
          </div>
        </div>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="grid gap-6">
          {/* Cuentas de Inventario */}
          <Card>
            <CardHeader>
              <CardTitle>Cuentas de Inventario y Compras</CardTitle>
              <CardDescription>
                Cuentas utilizadas para el registro de inventario y compras a proveedores
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="inventoryAccountId">Inventario (Activo)</Label>
                  <Select
                    value={formData.inventoryAccountId || 'none'}
                    onValueChange={(value) =>
                      setFormData({ ...formData, inventoryAccountId: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione una cuenta" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Sin configurar</SelectItem>
                      {accountsByType.Asset.map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.accountCode} - {account.accountName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">
                    Se carga cuando se recibe mercancía
                  </p>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="accountsPayableAccountId">Cuentas por Pagar (Pasivo)</Label>
                  <Select
                    value={formData.accountsPayableAccountId || 'none'}
                    onValueChange={(value) =>
                      setFormData({ ...formData, accountsPayableAccountId: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione una cuenta" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Sin configurar</SelectItem>
                      {accountsByType.Liability.map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.accountCode} - {account.accountName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">
                    Se abona cuando se recibe mercancía
                  </p>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="costOfGoodsSoldAccountId">Costo de Ventas (Gasto)</Label>
                  <Select
                    value={formData.costOfGoodsSoldAccountId || 'none'}
                    onValueChange={(value) =>
                      setFormData({ ...formData, costOfGoodsSoldAccountId: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione una cuenta" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Sin configurar</SelectItem>
                      {accountsByType.Expense.map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.accountCode} - {account.accountName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">Se carga al vender mercancía</p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Cuentas de Ventas */}
          <Card>
            <CardHeader>
              <CardTitle>Cuentas de Ventas e Ingresos</CardTitle>
              <CardDescription>
                Cuentas utilizadas para el registro de ventas e ingresos
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="salesRevenueAccountId">Ingresos por Ventas (Ingreso)</Label>
                  <Select
                    value={formData.salesRevenueAccountId || 'none'}
                    onValueChange={(value) =>
                      setFormData({ ...formData, salesRevenueAccountId: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione una cuenta" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Sin configurar</SelectItem>
                      {accountsByType.Income.map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.accountCode} - {account.accountName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">Se abona al realizar una venta</p>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="accountsReceivableAccountId">
                    Cuentas por Cobrar (Activo)
                  </Label>
                  <Select
                    value={formData.accountsReceivableAccountId || 'none'}
                    onValueChange={(value) =>
                      setFormData({ ...formData, accountsReceivableAccountId: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione una cuenta" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Sin configurar</SelectItem>
                      {accountsByType.Asset.map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.accountCode} - {account.accountName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">
                    Se carga en ventas a crédito
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Cuentas de Impuestos */}
          <Card>
            <CardHeader>
              <CardTitle>Cuentas de Impuestos (IVA)</CardTitle>
              <CardDescription>
                Cuentas para el registro de IVA por pagar y acreditable
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="taxPayableAccountId">IVA por Pagar (Pasivo)</Label>
                  <Select
                    value={formData.taxPayableAccountId || 'none'}
                    onValueChange={(value) =>
                      setFormData({ ...formData, taxPayableAccountId: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione una cuenta" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Sin configurar</SelectItem>
                      {accountsByType.Liability.map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.accountCode} - {account.accountName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">
                    IVA generado en ventas (trasladado)
                  </p>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="taxReceivableAccountId">IVA Acreditable (Activo)</Label>
                  <Select
                    value={formData.taxReceivableAccountId || 'none'}
                    onValueChange={(value) =>
                      setFormData({ ...formData, taxReceivableAccountId: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione una cuenta" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Sin configurar</SelectItem>
                      {accountsByType.Asset.map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.accountCode} - {account.accountName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">IVA pagado en compras</p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Cuentas de Efectivo/Banco */}
          <Card>
            <CardHeader>
              <CardTitle>Cuentas de Efectivo y Bancos</CardTitle>
              <CardDescription>
                Cuentas utilizadas para operaciones de caja y bancos
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="cashAccountId">Caja/Efectivo (Activo)</Label>
                  <Select
                    value={formData.cashAccountId || 'none'}
                    onValueChange={(value) => setFormData({ ...formData, cashAccountId: value })}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione una cuenta" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Sin configurar</SelectItem>
                      {accountsByType.Asset.map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.accountCode} - {account.accountName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">
                    Cuenta de efectivo para ventas en caja
                  </p>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="defaultBankAccountId">Banco por Defecto (Activo)</Label>
                  <Select
                    value={formData.defaultBankAccountId || 'none'}
                    onValueChange={(value) =>
                      setFormData({ ...formData, defaultBankAccountId: value })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Seleccione una cuenta" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">Sin configurar</SelectItem>
                      {accountsByType.Asset.map((account) => (
                        <SelectItem key={account.id} value={account.id}>
                          {account.accountCode} - {account.accountName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-muted-foreground">
                    Cuenta bancaria predeterminada
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Botón Guardar */}
          <div className="flex justify-end">
            <Button type="submit" size="lg" disabled={loading} className="gap-2">
              <Save className="h-4 w-4" />
              {loading ? 'Guardando...' : 'Guardar Configuración'}
            </Button>
          </div>
        </div>
      </form>
    </div>
  );
};

export default AccountConfigurationPage;
