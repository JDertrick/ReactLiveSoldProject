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
import { ArrowUpDown, Filter, CalendarIcon } from "lucide-react";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { format } from "date-fns";
import { cn } from "@/lib/utils";

const AllReceiptsPage = () => {
  const [customerId, setCustomerId] = useState("");
  const [status, setStatus] = useState("");
  const [fromDateString, setFromDateString] = useState<string | undefined>(
    undefined
  );
  const [toDateString, setToDateString] = useState<string | undefined>(
    undefined
  );

  const { data: receipts, isLoading } = useGetReceiptsByOrganization(
    customerId,
    status,
    fromDateString,
    toDateString
  );

  const { data: customers } = useGetCustomers();

  const [sorting, setSorting] = useState<SortingState>([]);

  const columns: ColumnDef<ReceiptDto>[] = useMemo(
    () => [
      {
        accessorKey: "createdAt",
        header: ({ column }) => (
          <Button
            variant="ghost"
            onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
          >
            Fecha
            <ArrowUpDown className="ml-2 h-4 w-4" />
          </Button>
        ),
        cell: ({ row }) =>
          new Date(row.original.createdAt).toLocaleDateString("es-ES", {
            year: "numeric",
            month: "short",
            day: "numeric",
          }),
      },
      {
        accessorKey: "status",
        header: "Estado",
        cell: ({ row }) => {
          const { isPosted, isRejected } = row.original;
          if (isPosted) {
            return <Badge className="bg-green-600">Posteado</Badge>;
          }
          if (isRejected) {
            return <Badge variant="destructive">Rechazado</Badge>;
          }
          return <Badge variant="secondary">Borrador</Badge>;
        },
      },
      {
        accessorKey: "customerName",
        header: "Cliente",
      },
      {
        accessorKey: "type",
        header: "Tipo",
        cell: ({ row }) => {
          const isDeposit = row.original.type === "Deposit";
          return (
            <Badge variant={isDeposit ? "default" : "secondary"}>
              {isDeposit ? "Depósito" : "Retiro"}
            </Badge>
          );
        },
      },
      {
        accessorKey: "totalAmount",
        header: () => <div className="text-right">Monto</div>,
        cell: ({ row }) => {
          const amount = parseFloat(row.getValue("totalAmount"));
          const isDeposit = row.original.type === "Deposit";
          return (
            <div
              className={`text-right font-medium ${
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
        accessorKey: "createdByUserName",
        header: "Creado por",
      },
      {
        accessorKey: "postedByUserName",
        header: "Posteado por",
        cell: ({ row }) => row.original.postedByUserName || "-",
      },
      {
        accessorKey: "rejectedByUserName",
        header: "Rechazado por",
        cell: ({ row }) => row.original.rejectedByUserName || "-",
      },
    ],
    []
  );

  const data = useMemo(() => receipts || [], [receipts]);

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

  const handleFilterChange = (key: string, value: any) => {
    const finalValue = value === "all" ? "" : value;

    switch (key) {
      case "customerId":
        setCustomerId(finalValue);
        break;
      case "status":
        setStatus(finalValue);
        break;
      case "fromDate":
        setFromDateString(finalValue ? finalValue.toISOString() : undefined);
        break;
      case "toDate":
        setToDateString(finalValue ? finalValue.toISOString() : undefined);
        break;
    }
  };

  const clearFilters = () => {
    setCustomerId("");
    setStatus("");
    setFromDateString(undefined);
    setToDateString(undefined);
  };

  return (
    <div className="container mx-auto py-6 space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Todos los Recibos
          </h1>
          <p className="text-gray-500 mt-1">
            Historial completo de recibos de la organización
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Filter className="w-5 h-5" />
            Filtros
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="space-y-2">
              <Label>Cliente</Label>
              <Select
                value={customerId || "all"}
                onValueChange={(value) =>
                  handleFilterChange("customerId", value)
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos los clientes" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos los clientes</SelectItem>
                  {customers?.map((c) => (
                    <SelectItem key={c.id} value={c.id}>
                      {c.firstName} {c.lastName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Estado</Label>
              <Select
                value={status || "all"}
                onValueChange={(value) => handleFilterChange("status", value)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos los estados" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos los estados</SelectItem>
                  <SelectItem value="draft">Borrador</SelectItem>
                  <SelectItem value="posted">Posteado</SelectItem>
                  <SelectItem value="rejected">Rechazado</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Desde</Label>
              <Popover>
                <PopoverTrigger asChild>
                  <Button
                    variant={"outline"}
                    className={cn(
                      "w-full justify-start text-left font-normal",
                      !fromDateString && "text-muted-foreground"
                    )}
                  >
                    <CalendarIcon className="mr-2 h-4 w-4" />
                    {fromDateString ? (
                      format(new Date(fromDateString), "PPP")
                    ) : (
                      <span>Seleccionar fecha</span>
                    )}
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0">
                  <Calendar
                    mode="single"
                    selected={
                      fromDateString ? new Date(fromDateString) : undefined
                    }
                    onSelect={(date) => handleFilterChange("fromDate", date)}
                    initialFocus
                  />
                </PopoverContent>
              </Popover>
            </div>
            <div className="space-y-2">
              <Label>Hasta</Label>
              <Popover>
                <PopoverTrigger asChild>
                  <Button
                    variant={"outline"}
                    className={cn(
                      "w-full justify-start text-left font-normal",
                      !toDateString && "text-muted-foreground"
                    )}
                  >
                    <CalendarIcon className="mr-2 h-4 w-4" />
                    {toDateString ? (
                      format(new Date(toDateString), "PPP")
                    ) : (
                      <span>Seleccionar fecha</span>
                    )}
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0">
                  <Calendar
                    mode="single"
                    selected={toDateString ? new Date(toDateString) : undefined}
                    onSelect={(date) => handleFilterChange("toDate", date)}
                    initialFocus
                  />
                </PopoverContent>
              </Popover>
            </div>
          </div>
          <Button variant="outline" onClick={clearFilters}>
            Limpiar Filtros
          </Button>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Historial de Recibos</CardTitle>
          <CardDescription>
            {receipts?.length ?? 0} recibos encontrados
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <p>Cargando...</p>
          ) : (
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
                        No se encontraron resultados.
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </div>
          )}
          <div className="flex items-center justify-end space-x-2 py-4">
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
