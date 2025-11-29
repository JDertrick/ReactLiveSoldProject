// src/pages/app/Accounting.tsx
import { useState, useMemo } from "react";
import {
    useGetChartOfAccounts,
    useGetJournalEntries,
    useCreateChartOfAccount,
} from "../../hooks/useAccounting";
import {
    ChartOfAccountDto,
    CreateChartOfAccountDto,
    JournalEntryDto,
    JournalEntryLineDto,
} from "../../types/accounting.types";

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
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Input } from "@/components/ui/input";
import { Calendar } from "@/components/ui/calendar";
import { format } from "date-fns";
import {
  BookUser,
  BookCopy,
  PlusCircle,
  Calendar as CalendarIcon,
  Filter,
  ChevronDown,
  ChevronRight,
  Loader2,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { toast } from "sonner";
import CreateAccountDialog from "@/components/accounting/CreateAccountDialog";

// Placeholder para las tarjetas de estadísticas
const StatCard = ({ title, value, icon }: { title: string, value: string | number, icon: React.ReactNode }) => (
    <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">{title}</CardTitle>
            {icon}
        </CardHeader>
        <CardContent>
            <div className="text-2xl font-bold">{value}</div>
        </CardContent>
    </Card>
);

// Componente para una fila de Asiento Contable expandible
const JournalEntryRow = ({ entry }: { entry: JournalEntryDto }) => {
    const [isOpen, setIsOpen] = useState(false);
    const totalDebit = useMemo(() => {
        if (!entry.lines || entry.lines.length === 0) return 0;
        return entry.lines.reduce((sum, line) => sum + (line.debit || 0), 0);
    }, [entry.lines]);

    return (
        <>
            <TableRow onClick={() => setIsOpen(!isOpen)} className="cursor-pointer">
                <TableCell>
                    <Button variant="ghost" size="icon">
                        {isOpen ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
                    </Button>
                </TableCell>
                <TableCell>{new Date(entry.entryDate).toLocaleDateString("es-ES")}</TableCell>
                <TableCell>
                    <div className="font-medium">{entry.description}</div>
                    <div className="text-xs text-muted-foreground">{entry.referenceNumber}</div>
                </TableCell>
                <TableCell className="text-right font-mono">{totalDebit.toLocaleString("es-ES", { style: "currency", currency: "USD" })}</TableCell>
            </TableRow>
            {isOpen && (
                <TableRow className="bg-muted/50 hover:bg-muted/50">
                    <TableCell colSpan={4}>
                        <div className="p-4">
                            <Table size="sm">
                                <TableHeader>
                                    <TableRow>
                                        <TableHead>Cuenta</TableHead>
                                        <TableHead className="text-right">Débito</TableHead>
                                        <TableHead className="text-right">Crédito</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {entry.lines.map((line: JournalEntryLineDto) => (
                                        <TableRow key={line.id}>
                                            <TableCell>
                                                <div className="font-medium">{line.accountName}</div>
                                                <div className="text-xs text-muted-foreground">{line.accountCode}</div>
                                            </TableCell>
                                            <TableCell className="text-right font-mono">{line.debit > 0 ? line.debit.toLocaleString("es-ES", { style: "currency", currency: "USD" }) : "-"}</TableCell>
                                            <TableCell className="text-right font-mono">{line.credit > 0 ? line.credit.toLocaleString("es-ES", { style: "currency", currency: "USD" }) : "-"}</TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </div>
                    </TableCell>
                </TableRow>
            )}
        </>
    );
};

const AccountingPage = () => {
    // State for date filters
    const [fromDate, setFromDate] = useState<Date | undefined>(undefined);
    const [toDate, setToDate] = useState<Date | undefined>(undefined);
    const [searchTerm, setSearchTerm] = useState("");
    const [isCreateAccountOpen, setCreateAccountOpen] = useState(false);

    // React Query Hooks
    const { data: chartOfAccounts, isLoading: isLoadingAccounts } = useGetChartOfAccounts();
    const { data: journalEntries, isLoading: isLoadingEntries } = useGetJournalEntries(
        fromDate?.toISOString(),
        toDate?.toISOString()
    );
    const createAccountMutation = useCreateChartOfAccount();

    const filteredJournalEntries = useMemo(() => {
        if (!journalEntries) return [];
        return journalEntries.filter(
            (e) =>
                e.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
                (e.referenceNumber && e.referenceNumber.toLowerCase().includes(searchTerm.toLowerCase()))
        );
    }, [journalEntries, searchTerm]);

    const handleCreateAccount = async (data: CreateChartOfAccountDto) => {
        try {
            await createAccountMutation.mutateAsync(data);
            toast.success("Cuenta contable creada exitosamente.");
            setCreateAccountOpen(false);
        } catch (error: any) {
            toast.error(error.response?.data?.message || "Error al crear la cuenta.");
        }
    };

    return (
        <div className="space-y-6">
            <h1 className="text-3xl font-bold tracking-tight">Contabilidad</h1>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                <StatCard title="Cuentas Activas" value={chartOfAccounts?.length ?? 0} icon={<BookUser className="text-muted-foreground" />} />
                <StatCard title="Asientos Registrados" value={journalEntries?.length ?? 0} icon={<BookCopy className="text-muted-foreground" />} />
            </div>

            <Tabs defaultValue="journal">
                <TabsList className="grid w-full grid-cols-2">
                    <TabsTrigger value="journal">Asientos Contables</TabsTrigger>
                    <TabsTrigger value="accounts">Plan de Cuentas</TabsTrigger>
                </TabsList>

                {/* TABLA DE ASIENTOS CONTABLES */}
                <TabsContent value="journal">
                    <Card>
                        <CardHeader>
                            <CardTitle>Libro Diario</CardTitle>
                            <CardDescription>Registro cronológico de todas las transacciones contables.</CardDescription>
                        </CardHeader>
                        <CardContent>
                            <div className="flex flex-col sm:flex-row justify-between items-center gap-4 mb-6">
                                <div className="flex items-center gap-2">
                                     <Popover>
                                        <PopoverTrigger asChild>
                                        <Button
                                            variant={"outline"}
                                            className={cn(
                                            "w-[240px] justify-start text-left font-normal",
                                            !fromDate && "text-muted-foreground"
                                            )}
                                        >
                                            <CalendarIcon className="mr-2 h-4 w-4" />
                                            {fromDate ? format(fromDate, "dd/MM/yyyy") : <span>Desde</span>}
                                        </Button>
                                        </PopoverTrigger>
                                        <PopoverContent className="w-auto p-0" align="start">
                                        <Calendar
                                            mode="single"
                                            selected={fromDate}
                                            onSelect={setFromDate}
                                            initialFocus
                                        />
                                        </PopoverContent>
                                    </Popover>
                                    <Popover>
                                        <PopoverTrigger asChild>
                                        <Button
                                            variant={"outline"}
                                            className={cn(
                                            "w-[240px] justify-start text-left font-normal",
                                            !toDate && "text-muted-foreground"
                                            )}
                                        >
                                            <CalendarIcon className="mr-2 h-4 w-4" />
                                            {toDate ? format(toDate, "dd/MM/yyyy") : <span>Hasta</span>}
                                        </Button>
                                        </PopoverTrigger>
                                        <PopoverContent className="w-auto p-0" align="start">
                                        <Calendar
                                            mode="single"
                                            selected={toDate}
                                            onSelect={setToDate}
                                            initialFocus
                                        />
                                        </PopoverContent>
                                    </Popover>
                                </div>
                                 <div className="flex items-center gap-2 w-full sm:w-auto">
                                    <Input
                                        placeholder="Buscar por descripción..."
                                        value={searchTerm}
                                        onChange={(e) => setSearchTerm(e.target.value)}
                                        className="w-full sm:w-64"
                                    />
                                </div>
                            </div>

                            <div className="overflow-x-auto">
                                <Table>
                                    <TableHeader>
                                        <TableRow>
                                            <TableHead className="w-[50px]"></TableHead>
                                            <TableHead>Fecha</TableHead>
                                            <TableHead>Descripción</TableHead>
                                            <TableHead className="text-right">Monto</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {isLoadingEntries ? (
                                            <TableRow><TableCell colSpan={4} className="h-24 text-center"><Loader2 className="mx-auto h-6 w-6 animate-spin" /></TableCell></TableRow>
                                        ) : filteredJournalEntries.length > 0 ? (
                                            filteredJournalEntries.map((entry) => (
                                                <JournalEntryRow key={entry.id} entry={entry} />
                                            ))
                                        ) : (
                                            <TableRow><TableCell colSpan={4} className="h-24 text-center">No se encontraron asientos contables.</TableCell></TableRow>
                                        )}
                                    </TableBody>
                                </Table>
                            </div>
                        </CardContent>
                    </Card>
                </TabsContent>

                {/* TABLA DE PLAN DE CUENTAS */}
                <TabsContent value="accounts">
                    <Card>
                        <CardHeader className="flex flex-row items-center justify-between">
                            <div>
                                <CardTitle>Plan de Cuentas</CardTitle>
                                <CardDescription>Catálogo de todas las cuentas contables de la organización.</CardDescription>
                            </div>
                            <Button onClick={() => setCreateAccountOpen(true)}><PlusCircle className="mr-2 h-4 w-4" /> Crear Cuenta</Button>
                        </CardHeader>
                        <CardContent>
                            <div className="overflow-x-auto">
                                <Table>
                                    <TableHeader>
                                        <TableRow>
                                            <TableHead>Código</TableHead>
                                            <TableHead>Nombre</TableHead>
                                            <TableHead>Tipo</TableHead>
                                            <TableHead>Descripción</TableHead>
                                            <TableHead>Sistema</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {isLoadingAccounts ? (
                                            <TableRow><TableCell colSpan={5} className="h-24 text-center"><Loader2 className="mx-auto h-6 w-6 animate-spin" /></TableCell></TableRow>
                                        ) : chartOfAccounts && chartOfAccounts.length > 0 ? (
                                            chartOfAccounts.map((account: ChartOfAccountDto) => (
                                                <TableRow key={account.id}>
                                                    <TableCell className="font-mono">{account.accountCode}</TableCell>
                                                    <TableCell className="font-medium">{account.accountName}</TableCell>
                                                    <TableCell>{account.accountType}</TableCell>
                                                    <TableCell className="text-muted-foreground">{account.description}</TableCell>
                                                    <TableCell>{account.systemAccountType ? "Sí" : "No"}</TableCell>
                                                </TableRow>
                                            ))
                                        ) : (
                                            <TableRow><TableCell colSpan={5} className="h-24 text-center">No hay cuentas definidas.</TableCell></TableRow>
                                        )}
                                    </TableBody>
                                </Table>
                            </div>
                        </CardContent>
                    </Card>
                </TabsContent>
            </Tabs>
            <CreateAccountDialog
                isOpen={isCreateAccountOpen}
                onClose={() => setCreateAccountOpen(false)}
                onSubmit={handleCreateAccount}
                isLoading={createAccountMutation.isPending}
            />
        </div>
    );
};

export default AccountingPage;
