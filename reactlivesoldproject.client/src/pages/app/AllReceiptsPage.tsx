import { useState, useMemo } from "react";
import { useGetReceiptsByOrganization } from "../../hooks/useWallet";
import { useGetCustomers } from "../../hooks/useCustomers";
import { ReceiptDto } from "../../types/wallet.types";
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
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  ColumnDef,
  flexRender,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  SortingState,
  useReactTable,
} from "@tanstack/react-table";
import {
  ArrowUpDown,
  Filter as FilterIcon,
  Calendar as CalendarIcon,
  MoreHorizontal,
  CheckCircle2,
  XCircle as XCircleIcon,
  Clock,
  ReceiptText,
  Send,
  X as XIcon,
} from "lucide-react";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { format } from "date-fns";
import { cn } from "@/lib/utils";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { usePostReceipt, useRejectReceipt } from "@/hooks/useWallet";
import { toast } from "sonner";

const StatCard = ({ title, value, subtext, icon, colorClass, circleColorClass }: { title: string, value: string | number, subtext?: string, icon: React.ReactNode, colorClass: string, circleColorClass: string }) => (
    <Card className="relative overflow-hidden">
        <div className={`absolute -top-4 -right-4 w-24 h-24 rounded-full ${circleColorClass} opacity-20`}></div>
        <CardHeader>
            <div className="flex items-center gap-4">
                <div className={`rounded-full p-3 ${colorClass} bg-opacity-10`}>
                    {icon}
                </div>
                <div>
                    <CardDescription>{title}</CardDescription>
                    <CardTitle className={`text-3xl font-bold ${colorClass}`}>{value}</CardTitle>
                </div>
            </div>
             {subtext && <p className="text-xs text-muted-foreground mt-2">{subtext}</p>}
        </CardHeader>
    </Card>
);

const AllReceiptsPage = () => {
  const [customerId, setCustomerId] = useState("");
  const [status, setStatus] = useState("");
  const [fromDate, setFromDate] = useState<Date | undefined>(undefined);
  const [toDate, setToDate] = useState<Date | undefined>(undefined);
  const [searchTerm, setSearchTerm] = useState("");

  const { data: receipts, isLoading } = useGetReceiptsByOrganization(
    customerId,
    status,
    fromDate?.toISOString(),
    toDate?.toISOString()
  );
  const { data: customers } = useGetCustomers();

  const postReceipt = usePostReceipt();
  const rejectReceipt = useRejectReceipt();

  const [sorting, setSorting] = useState<SortingState>([]);

  const stats = useMemo(() => {
    if (!receipts) return { total: 0, posted: 0, draft: 0, rejected: 0 };

    return {
      total: receipts.length,
      posted: receipts.filter((r) => r.isPosted).length,
      draft: receipts.filter((r) => !r.isPosted && !r.isRejected).length,
      rejected: receipts.filter((r) => r.isRejected).length,
    };
  }, [receipts]);

  const getReceiptTypeBadge = (type: string) => {
    const isDeposit = type === "Deposit";
    return (
      <Badge
        variant="outline"
        className={cn(
          "border-transparent",
          isDeposit
            ? "bg-green-100 text-green-800"
            : "bg-red-100 text-red-800"
        )}
      >
        {isDeposit ? "Depósito" : "Retiro"}
      </Badge>
    );
  };

  const handlePostReceipt = async (receiptId: string) => {
    try {
      await postReceipt.mutateAsync(receiptId);
      toast.success("Recibo posteado correctamente.");
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Error al postear recibo.");
    }
  };

  const handleRejectReceipt = async (receiptId: string) => {
    try {
      await rejectReceipt.mutateAsync(receiptId);
      toast.success("Recibo rechazado correctamente.");
    } catch (error: any) {
      toast.error(error.response?.data?.message || "Error al rechazar recibo.");
    }
  };

  const columns: ColumnDef<ReceiptDto>[] = useMemo(
    () => [
      {
        accessorKey: "customerName",
        header: "CLIENTE",
        cell: ({ row }) => (
          <div className="font-medium">{row.original.customerName}</div>
        ),
      },
      {
        accessorKey: "status",
        header: "ESTADO",
        cell: ({ row }) => {
          const { isPosted, isRejected } = row.original;
          if (isPosted) {
            return (
              <Badge
                variant="outline"
                className="text-green-600 border-green-600/50 bg-green-600/10"
              >
                <CheckCircle2 className="w-3.5 h-3.5 mr-1" />
                Posteado
              </Badge>
            );
          }
          if (isRejected) {
            return (
              <Badge
                variant="outline"
                className="text-red-600 border-red-600/50 bg-red-600/10"
              >
                <XCircleIcon className="w-3.5 h-3.5 mr-1" />
                Rechazado
              </Badge>
            );
          }
          return (
            <Badge
              variant="outline"
              className="text-orange-600 border-orange-600/50 bg-orange-600/10"
            >
              <Clock className="w-3.5 h-3.5 mr-1" />
              Borrador
            </Badge>
          );
        },
      },
      {
        accessorKey: "type",
        header: "TIPO",
        cell: ({ row }) => getReceiptTypeBadge(row.original.type),
      },
      {
        accessorKey: "createdAt",
        header: ({ column }) => (
          <Button
            variant="ghost"
            onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
          >
            FECHA & USUARIO
            <ArrowUpDown className="ml-2 h-4 w-4" />
          </Button>
        ),
        cell: ({ row }) => (
          <div>
            <div className="font-medium">
              {new Date(row.original.createdAt).toLocaleDateString("es-ES", {
                month: "short",
                day: "numeric",
                hour: "2-digit",
                minute: "2-digit",
              })}
            </div>
            <div className="text-xs text-muted-foreground">
              {row.original.createdByUserName}
            </div>
          </div>
        ),
      },
      {
        accessorKey: "totalAmount",
        header: () => <div className="text-right">MONTO</div>,
        cell: ({ row }) => {
          const amount = parseFloat(row.original.totalAmount.toString());
          const isDeposit = row.original.type === "Deposit";
          return (
            <div
              className={`text-right font-semibold text-base ${
                isDeposit ? "text-green-600" : "text-red-600"
              }`}
            >
              {amount.toLocaleString("es-ES", {
                style: "currency",
                currency: "USD",
              })}
            </div>
          );
        },
      },
      {
        id: "actions",
        header: () => <div className="text-right">ACCIÓN</div>,
        cell: ({ row }) => (
          <div className="text-right">
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="h-8 w-8 p-0">
                  <span className="sr-only">Open menu</span>
                  <MoreHorizontal className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                {!row.original.isPosted && !row.original.isRejected && (
                  <>
                    <DropdownMenuItem
                      onClick={() => handlePostReceipt(row.original.id)}
                    >
                      <Send className="mr-2 h-4 w-4" />
                      Postear
                    </DropdownMenuItem>
                    <DropdownMenuItem
                      onClick={() => handleRejectReceipt(row.original.id)}
                      className="text-red-600 focus:text-red-600"
                    >
                      <XCircleIcon className="mr-2 h-4 w-4" />
                      Rechazar
                    </DropdownMenuItem>
                  </>
                )}
                {/* Add other actions like View Details if needed */}
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        ),
      },
    ],
    []
  );

  const data = useMemo(() => {
    if (!receipts) return [];
    return receipts.filter(
      (r) =>
        r.customerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        r.notes?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        r.createdByUserName?.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [receipts, searchTerm]);

  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    onSortingChange: setSorting,
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    state: {
      sorting,
    },
  });

  const clearFilters = () => {
    setCustomerId("");
    setStatus("");
    setFromDate(undefined);
    setToDate(undefined);
    setSearchTerm("");
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Todos los Recibos</h1>
        <p className="text-muted-foreground mt-1">
          Gestione los recibos de sus clientes, depósitos y retiros.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatCard
          title="TOTAL RECIBOS"
          value={stats.total}
          icon={<ReceiptText className="w-6 h-6" />}
          colorClass="text-blue-500"
          circleColorClass="bg-blue-500"
        />
        <StatCard
          title="POSTEADOS"
          value={stats.posted}
          icon={<CheckCircle2 className="w-6 h-6" />}
          colorClass="text-green-500"
          circleColorClass="bg-green-500"
        />
        <StatCard
          title="BORRADORES"
          value={stats.draft}
          subtext="Pendientes"
          icon={<Clock className="w-6 h-6" />}
          colorClass="text-orange-500"
          circleColorClass="bg-orange-500"
        />
        <StatCard
          title="RECHAZADOS"
          value={stats.rejected}
          icon={<XCircleIcon className="w-6 h-6" />}
          colorClass="text-red-500"
          circleColorClass="bg-red-500"
        />
      </div>

      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row justify-between items-center gap-4 mb-6">
            <div className="flex items-center gap-2">
              <Popover>
                <PopoverTrigger asChild>
                  <div className="relative w-[180px]">
                    <CalendarIcon className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                      className={cn(
                        "w-full justify-start text-left font-normal pl-10",
                        !fromDate && "text-muted-foreground"
                      )}
                      value={fromDate ? format(fromDate, "dd/MM/yyyy") : ""}
                      placeholder="Desde"
                      readOnly
                    />
                    {fromDate && (
                        <Button
                        variant="ghost"
                        size="icon"
                        className="absolute right-1 top-1/2 -translate-y-1/2 h-6 w-6"
                        onClick={(e) => {
                            e.stopPropagation();
                            setFromDate(undefined);
                        }}
                        >
                        <XIcon className="h-4 w-4" />
                        </Button>
                    )}
                  </div>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0">
                  <Calendar
                    mode="single"
                    selected={fromDate}
                    onSelect={setFromDate}
                    initialFocus
                  />
                </PopoverContent>
              </Popover>
              <span>→</span>
              <Popover>
                <PopoverTrigger asChild>
                    <div className="relative w-[180px]">
                        <CalendarIcon className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                        <Input
                        className={cn(
                            "w-full justify-start text-left font-normal pl-10",
                            !toDate && "text-muted-foreground"
                        )}
                        value={toDate ? format(toDate, "dd/MM/yyyy") : ""}
                        placeholder="Hasta"
                        readOnly
                        />
                        {toDate && (
                            <Button
                            variant="ghost"
                            size="icon"
                            className="absolute right-1 top-1/2 -translate-y-1/2 h-6 w-6"
                            onClick={(e) => {
                                e.stopPropagation();
                                setToDate(undefined);
                            }}
                            >
                            <XIcon className="h-4 w-4" />
                            </Button>
                        )}
                    </div>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0">
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
                placeholder="Buscar cliente, notas..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full sm:w-64"
              />
              <Button variant="outline" size="icon" onClick={clearFilters}>
                <FilterIcon className="h-4 w-4" />
              </Button>
            </div>
          </div>

          <div className="rounded-md border">
            <Table>
              <TableHeader>
                {table.getHeaderGroups().map((headerGroup) => (
                  <TableRow key={headerGroup.id}>
                    {headerGroup.headers.map((header) => (
                      <TableHead key={header.id}>
                        {header.isPlaceholder
                          ? null
                          : flexRender(
                              header.column.columnDef.header,
                              header.getContext()
                            )}
                      </TableHead>
                    ))}
                  </TableRow>
                ))}
              </TableHeader>
              <TableBody>
                {table.getRowModel().rows?.length ? (
                  table.getRowModel().rows.map((row) => (
                    <TableRow
                      key={row.id}
                      data-state={row.getIsSelected() && "selected"}
                      className="hover:bg-muted/50"
                    >
                      {row.getVisibleCells().map((cell) => (
                        <TableCell key={cell.id}>
                          {flexRender(
                            cell.column.columnDef.cell,
                            cell.getContext()
                          )}
                        </TableCell>
                      ))}
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell
                      colSpan={columns.length}
                      className="h-24 text-center"
                    >
                      No se encontraron recibos.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>
          <div className="flex items-center justify-end space-x-2 py-4">
            <div className="flex-1 text-sm text-muted-foreground">
                Mostrando {table.getFilteredRowModel().rows.length} de {data.length} recibos.
            </div>
            <Button
              variant="outline"
              size="sm"
              onClick={() => table.previousPage()}
              disabled={!table.getCanPreviousPage()}
            >
              Anterior
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => table.nextPage()}
              disabled={!table.getCanNextPage()}
            >
              Siguiente
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default AllReceiptsPage;
