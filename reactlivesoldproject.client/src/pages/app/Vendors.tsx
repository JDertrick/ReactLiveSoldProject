import { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  useGetVendors,
  useDeleteVendor,
} from "../../hooks/useVendors";
import { Vendor } from "../../types/vendor.types";
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
import { Card, CardContent } from "@/components/ui/card";
import {
  Package,
  UserPlus,
  MoreHorizontal,
  Edit,
  Trash2,
  Building2,
  ShoppingCart,
  FileText,
  DollarSign,
  Truck,
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
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import VendorForm from "@/components/vendors/VendorForm";
import { useDebounce } from "@/hooks/useDebounce";
import { formatCurrency } from "@/utils/currencyHelper";
import { toast } from "sonner";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

const columnHelper = createColumnHelper<Vendor>();

const VendorsPage = () => {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("all"); // 'all', 'active', 'inactive'

  const debouncedSearchTerm = useDebounce(searchTerm, 500);
  const debouncedStatusFilter = useDebounce(statusFilter, 500);

  const { data: vendors } = useGetVendors(
    debouncedSearchTerm,
    debouncedStatusFilter
  );

  const deleteVendor = useDeleteVendor();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingVendor, setEditingVendor] = useState<Vendor | null>(null);
  const [deletingVendor, setDeletingVendor] = useState<Vendor | null>(null);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);

  const handleOpenModal = (vendor?: Vendor) => {
    setEditingVendor(vendor || null);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingVendor(null);
  };

  const handleDeleteClick = (vendor: Vendor) => {
    setDeletingVendor(vendor);
    setShowDeleteDialog(true);
  };

  const handleConfirmDelete = async () => {
    if (!deletingVendor) return;

    try {
      await deleteVendor.mutateAsync(deletingVendor.id);
      toast.success("Proveedor eliminado exitosamente");
      setShowDeleteDialog(false);
      setDeletingVendor(null);
    } catch (error: any) {
      toast.error(
        error.response?.data?.message ||
          "Error al eliminar el proveedor. Puede que tenga órdenes de compra asociadas."
      );
    }
  };

  const columns = [
    columnHelper.accessor(
      (row) => `${row.contact?.firstName || ""} ${row.contact?.lastName || ""}`,
      {
        id: "vendor",
        header: "Proveedor",
        cell: (info) => (
          <div className="flex items-center gap-3">
            <div className="bg-muted p-2 rounded-md">
              <Building2 className="h-5 w-5 text-muted-foreground" />
            </div>
            <div>
              <div className="font-medium">{info.getValue()}</div>
              <div className="text-sm text-muted-foreground">
                {info.row.original.contact?.email}
              </div>
              {info.row.original.vendorCode && (
                <div className="text-xs text-muted-foreground">
                  Código: {info.row.original.vendorCode}
                </div>
              )}
            </div>
          </div>
        ),
      }
    ),
    columnHelper.accessor("contact.company", {
      header: "Empresa",
      cell: (info) => (
        <div className="text-sm">{info.getValue() || "N/A"}</div>
      ),
    }),
    columnHelper.accessor("contact.phone", {
      header: "Contacto",
      cell: (info) => <div className="text-sm">{info.getValue() || "N/A"}</div>,
    }),
    columnHelper.accessor("assignedBuyerName", {
      header: "Comprador Asignado",
      cell: (info) => (
        <div className="text-sm">{info.getValue() || "Sin asignar"}</div>
      ),
    }),
    columnHelper.accessor("creditLimit", {
      header: "Límite de Crédito",
      cell: (info) => (
        <div className="font-medium text-blue-600">
          {info.getValue() ? formatCurrency(info.getValue()!) : "N/A"}
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
                <Edit className="mr-2 h-4 w-4" /> Editar Proveedor
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={() =>
                  navigate(`/app/purchase-orders?vendorId=${info.row.original.id}`)
                }
              >
                <ShoppingCart className="mr-2 h-4 w-4" /> Ver Órdenes de Compra
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() =>
                  navigate(`/app/purchase-receipts?vendorId=${info.row.original.id}`)
                }
              >
                <Truck className="mr-2 h-4 w-4" /> Ver Recepciones
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() =>
                  navigate(`/app/vendor-invoices?vendorId=${info.row.original.id}`)
                }
              >
                <FileText className="mr-2 h-4 w-4" /> Ver Facturas
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() =>
                  navigate(`/app/payments?vendorId=${info.row.original.id}`)
                }
              >
                <DollarSign className="mr-2 h-4 w-4" /> Ver Pagos
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={() => handleDeleteClick(info.row.original)}
                className="text-red-600"
              >
                <Trash2 className="mr-2 h-4 w-4" /> Eliminar Proveedor
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      ),
    }),
  ];

  const table = useReactTable({
    data: vendors || [],
    columns,
    getCoreRowModel: getCoreRowModel(),
  });

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center pb-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Proveedores</h1>
          <p className="text-muted-foreground mt-1">
            Administra tus proveedores y su información de contacto.
          </p>
        </div>
        <Button onClick={() => handleOpenModal()} size="lg" className="gap-2">
          <UserPlus className="w-5 h-5" />
          Agregar proveedor
        </Button>
      </div>

      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row justify-between items-center gap-4 mb-6">
            <div className="flex items-center gap-2 w-full sm:w-auto">
              <Input
                placeholder="Buscar por nombre, email, empresa..."
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
                      No se encontraron proveedores con los filtros actuales.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>
          <div className="flex items-center justify-between pt-4">
            <div className="text-sm text-muted-foreground">
              Mostrando {table.getRowModel().rows.length} de{" "}
              {vendors?.length || 0} proveedores.
            </div>
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
      <VendorForm
        isModalOpen={isModalOpen}
        handleCloseModal={handleCloseModal}
        editingVendor={editingVendor}
      />

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Estás seguro?</AlertDialogTitle>
            <AlertDialogDescription>
              Esta acción eliminará permanentemente al proveedor{" "}
              <span className="font-semibold">
                {deletingVendor?.contact?.firstName}{" "}
                {deletingVendor?.contact?.lastName}
              </span>
              . Esta acción no se puede deshacer.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setDeletingVendor(null)}>
              Cancelar
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={handleConfirmDelete}
              className="bg-red-600 hover:bg-red-700"
            >
              Eliminar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default VendorsPage;
