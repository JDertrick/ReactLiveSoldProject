import { useState } from "react";
import {
  useGetCustomers,
  useCreateCustomer,
  useUpdateCustomer,
  useGetCustomerStats,
} from "../../hooks/useCustomers";
import { Customer } from "../../types/customer.types";
import { Button } from "../../components/ui/button";
import { Badge } from "../../components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "../../components/ui/table";
import {
  createColumnHelper,
  flexRender,
  getCoreRowModel,
  useReactTable,
} from "@tanstack/react-table";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Users,
  Wallet,
  UserPlus,
  MoreHorizontal,
  CreditCard,
  Edit,
} from "lucide-react";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import CustomerForm from "@/components/customers/CustomerForm";
import { useDebounce } from "@/hooks/useDebounce";
import { formatCurrency } from "@/utils/currencyHelper";

const columnHelper = createColumnHelper<Customer>();

const StatCard = ({
  title,
  value,
  subtext,
  icon,
  colorClass,
  circleColorClass,
}: {
  title: string;
  value: string | number;
  subtext: string;
  icon: React.ReactNode;
  colorClass: string;
  circleColorClass: string;
}) => (
  <Card className="relative overflow-hidden">
    <div
      className={`absolute -top-4 -right-4 w-24 h-24 rounded-full ${circleColorClass} opacity-20`}
    ></div>
    <CardHeader>
      <div className="flex items-center gap-4">
        <div className={`rounded-full p-3 ${colorClass} bg-opacity-10`}>
          {icon}
        </div>
        <div>
          <CardTitle className="text-sm font-medium text-muted-foreground">
            {title}
          </CardTitle>
          <p className={`text-3xl font-bold ${colorClass}`}>{value}</p>
        </div>
      </div>
      {subtext && (
        <p className="text-xs text-muted-foreground mt-2">{subtext}</p>
      )}
    </CardHeader>
  </Card>
);

const CustomersPage = () => {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("all"); // 'all', 'active', 'inactive'

  const debouncedSearchTerm = useDebounce(searchTerm, 500);
  const debouncedStatusFilter = useDebounce(statusFilter, 500);

  const { data: customers } = useGetCustomers(
    debouncedSearchTerm,
    debouncedStatusFilter
  );
  const { data: stats } = useGetCustomerStats();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCustomer, setEditingCustomer] = useState<Customer | null>(null);

  const handleOpenModal = (customer?: Customer) => {
    setEditingCustomer(customer || null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingCustomer(null);
  };

  const columns = [
    columnHelper.accessor("customerNo", {
      header: "No. Cliente",
      cell: (info) => (
        <div className="text-sm font-mono text-muted-foreground">
          {info.getValue() || "-"}
        </div>
      ),
    }),
    columnHelper.accessor((row) => `${row.firstName} ${row.lastName}`, {
      id: "customer",
      header: "Cliente",
      cell: (info) => (
        <div className="flex items-center gap-3">
          <div className="bg-muted p-2 rounded-md">
            <Users className="h-5 w-5 text-muted-foreground" />
          </div>
          <div>
            <div className="font-medium">{info.getValue()}</div>
            <div className="text-sm text-muted-foreground">
              {info.row.original.email}
            </div>
          </div>
        </div>
      ),
    }),
    columnHelper.accessor("phone", {
      header: "Contacto",
      cell: (info) => <div className="text-sm">{info.getValue()}</div>,
    }),
    columnHelper.accessor("wallet.balance", {
      header: "Billetera",
      cell: (info) => (
        <div className="font-medium text-green-600">
          {formatCurrency(info.getValue() ?? 0)}
          {/* ${(info.getValue() ?? 0).toFixed(2)} */}
        </div>
      ),
    }),
    columnHelper.accessor("isActive", {
      header: "Estado",
      cell: (info) => (
        <Badge
          variant={info.getValue() ? "default" : "destructive"}
          className={
            info.getValue()
              ? "bg-green-100 text-green-800"
              : "bg-red-100 text-red-800"
          }
        >
          {info.getValue() ? "Activo" : "Inactivo"}
        </Badge>
      ),
    }),
    columnHelper.display({
      id: "actions",
      header: "Acciones",
      cell: (info) => (
        <div className="text-right">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon">
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem
                onClick={() => handleOpenModal(info.row.original)}
              >
                <Edit className="mr-2 h-4 w-4" /> Editar Cliente
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() =>
                  navigate(`/app/customers/${info.row.original.id}/wallet`)
                }
              >
                <CreditCard className="mr-2 h-4 w-4" /> Administrar Billetera
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      ),
    }),
  ];

  const table = useReactTable({
    data: customers || [],
    columns,
    getCoreRowModel: getCoreRowModel(),
  });

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Clientes</h1>
          <p className="text-muted-foreground mt-1">
            Administra tus clientes, sus billeteras y su actividad.
          </p>
        </div>
        <Button onClick={() => handleOpenModal()} size="lg" className="gap-2">
          <UserPlus className="w-5 h-5" />
          Agregar cliente
        </Button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <StatCard
          title="Total Clientes"
          value={stats?.totalCustomers ?? 0}
          subtext=""
          icon={<Users className="w-6 h-6" />}
          colorClass="text-blue-500"
          circleColorClass="bg-blue-500"
        />
        <StatCard
          title="Billetera Total"
          value={`${formatCurrency(stats?.totalWalletSum ?? 0)}`}
          icon={<Wallet className="w-6 h-6" />}
          colorClass="text-green-500"
          circleColorClass="bg-green-500"
          subtext="Suma de todas las billeteras"
        />
        <StatCard
          title="Nuevos (Mes)"
          value={stats?.newCustomersThisMonth ?? 0}
          icon={<UserPlus className="w-6 h-6" />}
          colorClass="text-orange-500"
          circleColorClass="bg-orange-500"
          subtext="Clientes registrados este mes"
        />
      </div>

      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row justify-between items-center gap-4 mb-6">
            <div className="flex items-center gap-2 w-full sm:w-auto">
              <Input
                placeholder="Buscar por nombre, email, contacto..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full sm:w-64"
              />
            </div>
            <div className="flex items-center gap-2">
              <Select
                value={statusFilter}
                onValueChange={(value) => setStatusFilter(value)}
              >
                <SelectTrigger className="w-[180px]">
                  <SelectValue placeholder="Estado" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos</SelectItem>
                  <SelectItem value="active">Activos</SelectItem>
                  <SelectItem value="inactive">Inactivos</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                {table.getHeaderGroups().map((headerGroup) => (
                  <TableRow key={headerGroup.id}>
                    {headerGroup.headers.map((header) => (
                      <TableHead
                        key={header.id}
                        className={header.id === "actions" ? "text-right" : ""}
                      >
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
                      No se encontraron clientes con los filtros actuales.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>
          <div className="flex items-center justify-between pt-4">
            <div className="text-sm text-muted-foreground">
              Mostrando {table.getRowModel().rows.length} de{" "}
              {customers?.length || 0} clientes.
            </div>
            {/* TODO: Implement proper pagination */}
            <div className="flex items-center gap-2">
              <Button variant="outline" size="sm" disabled>
                Anterior
              </Button>
              <Button variant="outline" size="sm" disabled>
                Siguiente
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Modal */}
      <CustomerForm
        isModalOpen={isModalOpen}
        handleCloseModal={handleCloseModal}
        editingCustomer={editingCustomer}
      />
    </div>
  );
};

export default CustomersPage;
