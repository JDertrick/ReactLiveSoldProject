import { useState } from "react";
import {
  Dialog,
  DialogBackdrop,
  DialogPanel,
  DialogTitle,
} from "@headlessui/react";
import {
  useGetCustomers,
  useCreateCustomer,
  useUpdateCustomer,
} from "../../hooks/useCustomers";
import {
  CreateCustomerDto,
  UpdateCustomerDto,
  Customer,
} from "../../types/customer.types";
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
  CustomerProfileTab,
  CustomerOrdersTab,
} from "../../components/customers";
import {
  createColumnHelper,
  flexRender,
  getCoreRowModel,
  useReactTable,
} from "@tanstack/react-table";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

const columnHelper = createColumnHelper<Customer>();

const CustomersPage = () => {
  const navigate = useNavigate();
  const { data: customers, isLoading } = useGetCustomers();
  const createCustomer = useCreateCustomer();
  const updateCustomer = useUpdateCustomer();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [editingCustomer, setEditingCustomer] = useState<Customer | null>(null);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(
    null
  );
  const [activeTab, setActiveTab] = useState<"profile" | "orders">("profile");

  const [formData, setFormData] = useState<
    CreateCustomerDto | UpdateCustomerDto
  >({
    firstName: "",
    lastName: "",
    email: "",
    phone: "",
    password: "",
    isActive: true,
  });

  const handleOpenModal = (customer?: Customer) => {
    if (customer) {
      setEditingCustomer(customer);
      setFormData({
        firstName: customer.firstName,
        lastName: customer.lastName,
        email: customer.email,
        phone: customer.phone,
        isActive: customer.isActive,
      });
    } else {
      setEditingCustomer(null);
      setFormData({
        firstName: "",
        lastName: "",
        email: "",
        phone: "",
        password: "",
        isActive: true,
      });
    }
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setEditingCustomer(null);
  };

  const handleOpenDetailModal = (customer: Customer) => {
    setSelectedCustomer(customer);
    setActiveTab("profile");
    setIsDetailModalOpen(true);
  };

  const handleCloseDetailModal = () => {
    setIsDetailModalOpen(false);
    setSelectedCustomer(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (editingCustomer) {
        await updateCustomer.mutateAsync({
          id: editingCustomer.id,
          data: formData as UpdateCustomerDto,
        });
      } else {
        await createCustomer.mutateAsync(formData as CreateCustomerDto);
      }
      handleCloseModal();
    } catch (error) {
      console.error("Error saving customer:", error);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const { name, value, type } = e.target;
    setFormData({
      ...formData,
      [name]:
        type === "checkbox" ? (e.target as HTMLInputElement).checked : value,
    });
  };

  const columns = [
    columnHelper.accessor((row) => `${row.firstName} ${row.lastName}`, {
      id: "customer",
      header: "Cliente",
      cell: (info) => (
        <div>
          <div className="font-medium">{info.getValue()}</div>
          <div className="text-sm text-muted-foreground">
            {info.row.original.email}
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
        <div className="font-medium">${(info.getValue() ?? 0).toFixed(2)}</div>
      ),
    }),
    columnHelper.accessor("isActive", {
      header: "Estado",
      cell: (info) => (
        <Badge variant={info.getValue() ? "default" : "destructive"}>
          {info.getValue() ? "Activo" : "Inactivo"}
        </Badge>
      ),
    }),
    columnHelper.display({
      id: "actions",
      header: "Acciones",
      cell: (info) => (
        <div className="flex gap-2 justify-end">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => handleOpenDetailModal(info.row.original)}
          >
            Ver
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => handleOpenModal(info.row.original)}
          >
            Editar
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() =>
              navigate(`/app/customers/${info.row.original.id}/wallet`)
            }
            className="text-green-600 hover:text-green-700"
          >
            Billetera
          </Button>
        </div>
      ),
    }),
  ];

  const table = useReactTable({
    data: customers || [],
    columns,
    getCoreRowModel: getCoreRowModel(),
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-2xl font-bold text-gray-900">Clientes</h1>
          <p className="mt-2 text-sm text-gray-700">
            Maneja tus clientes, billetera y transacciones.
          </p>
        </div>
        <div className="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
          <Button onClick={() => handleOpenModal()}>
            <svg
              className="w-5 h-5"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"
              />
            </svg>
            Agregar cliente
          </Button>
        </div>
      </div>

      {/* Desktop Table View */}
      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <div>
              <CardTitle>Lista de clientes</CardTitle>
            </div>
          </div>
        </CardHeader>
        <CardContent>
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
                    No se encontraron clientes. Agrega tu primer cliente para comenzar.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Mobile Card View */}
      <div className="mt-8 md:hidden space-y-4">
        {table.getRowModel().rows?.length ? (
          table.getRowModel().rows.map((row) => {
            const customer = row.original;
            return (
              <div
                key={row.id}
                className="bg-white border rounded-lg p-4 shadow-sm space-y-3"
              >
                <div className="flex justify-between items-start">
                  <div>
                    <div className="font-medium text-lg">
                      {customer.firstName} {customer.lastName}
                    </div>
                    <div className="text-sm text-muted-foreground">
                      {customer.email}
                    </div>
                  </div>
                  <Badge
                    variant={customer.isActive ? "default" : "destructive"}
                  >
                    {customer.isActive ? "Activo" : "Inactivo"}
                  </Badge>
                </div>

                <div className="grid grid-cols-2 gap-3 text-sm border-t pt-3">
                  <div>
                    <div className="text-muted-foreground">Teléfono</div>
                    <div className="font-medium">{customer.phone}</div>
                  </div>
                  <div>
                    <div className="text-muted-foreground">Billetera</div>
                    <div className="font-medium">
                      ${(customer.wallet?.balance ?? 0).toFixed(2)}
                    </div>
                  </div>
                </div>

                <div className="grid grid-cols-3 gap-2 pt-2 border-t">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => handleOpenDetailModal(customer)}
                  >
                    Ver
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => handleOpenModal(customer)}
                  >
                    Editar
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() =>
                      navigate(`/app/customers/${customer.id}/wallet`)
                    }
                    className="text-green-600 hover:text-green-700"
                  >
                    Billetera
                  </Button>
                </div>
              </div>
            );
          })
        ) : (
          <div className="bg-white border rounded-lg p-8 text-center">
            <p className="text-muted-foreground">
              No se encontraron clientes. Agrega tu primer cliente para comenzar.
            </p>
          </div>
        )}
      </div>

      {/* Create/Edit Modal */}
      <Dialog
        open={isModalOpen}
        onClose={handleCloseModal}
        className="relative z-10"
      >
        <DialogBackdrop
          transition
          className="fixed inset-0 bg-gray-500/75 transition-opacity data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in"
        />

        <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <DialogPanel
              transition
              className="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all data-closed:translate-y-4 data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in sm:my-8 sm:w-full sm:max-w-lg data-closed:sm:translate-y-0 data-closed:sm:scale-95"
            >
              <form onSubmit={handleSubmit}>
                <div className="bg-white px-4 pt-5 pb-4 sm:p-6">
                  <DialogTitle
                    as="h3"
                    className="text-lg leading-6 font-medium text-gray-900"
                  >
                    {editingCustomer ? "Editar Cliente" : "Crear Cliente"}
                  </DialogTitle>
                  <div className="mt-6 space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label
                          htmlFor="firstName"
                          className="block text-sm font-medium text-gray-700"
                        >
                          Nombre
                        </label>
                        <input
                          type="text"
                          name="firstName"
                          id="firstName"
                          required
                          value={formData.firstName}
                          onChange={handleChange}
                          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                        />
                      </div>

                      <div>
                        <label
                          htmlFor="lastName"
                          className="block text-sm font-medium text-gray-700"
                        >
                          Apellido
                        </label>
                        <input
                          type="text"
                          name="lastName"
                          id="lastName"
                          required
                          value={formData.lastName}
                          onChange={handleChange}
                          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                        />
                      </div>
                    </div>

                    <div>
                      <label
                        htmlFor="email"
                        className="block text-sm font-medium text-gray-700"
                      >
                        Correo Electrónico
                      </label>
                      <input
                        type="email"
                        name="email"
                        id="email"
                        required
                        value={formData.email}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>

                    <div>
                      <label
                        htmlFor="phoneNumber"
                        className="block text-sm font-medium text-gray-700"
                      >
                        Número de Teléfono
                      </label>
                      <input
                        type="tel"
                        name="phoneNumber"
                        id="phoneNumber"
                        required
                        value={formData.phone}
                        onChange={handleChange}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>

                    {!editingCustomer && (
                      <div>
                        <label
                          htmlFor="password"
                          className="block text-sm font-medium text-gray-700"
                        >
                          Contraseña
                        </label>
                        <input
                          type="password"
                          name="password"
                          id="password"
                          required
                          value={(formData as CreateCustomerDto).password || ""}
                          onChange={handleChange}
                          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                        />
                      </div>
                    )}

                    <div className="flex items-center">
                      <input
                        id="isActive"
                        name="isActive"
                        type="checkbox"
                        checked={formData.isActive}
                        onChange={handleChange}
                        className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                      />
                      <label
                        htmlFor="isActive"
                        className="ml-2 block text-sm text-gray-900"
                      >
                        Activo
                      </label>
                    </div>
                  </div>
                </div>

                <div className="bg-gray-50 px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6">
                  <button
                    type="submit"
                    disabled={
                      createCustomer.isPending || updateCustomer.isPending
                    }
                    className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50"
                  >
                    {createCustomer.isPending || updateCustomer.isPending
                      ? "Guardando..."
                      : "Guardar"}
                  </button>
                  <button
                    type="button"
                    onClick={handleCloseModal}
                    className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:w-auto sm:text-sm"
                  >
                    Cancelar
                  </button>
                </div>
              </form>
            </DialogPanel>
          </div>
        </div>
      </Dialog>

      {/* Customer Detail Modal */}
      <Dialog
        open={isDetailModalOpen}
        onClose={handleCloseDetailModal}
        className="relative z-10"
      >
        <DialogBackdrop
          transition
          className="fixed inset-0 bg-gray-500/75 transition-opacity data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in"
        />

        <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <DialogPanel
              transition
              className="relative transform overflow-hidden rounded-2xl bg-white text-left shadow-xl transition-all data-closed:translate-y-4 data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in sm:my-8 sm:w-full sm:max-w-4xl data-closed:sm:translate-y-0 data-closed:sm:scale-95"
            >
              <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                <div className="flex justify-between items-start mb-4">
                  <DialogTitle
                    as="h3"
                    className="text-lg leading-6 font-medium text-gray-900"
                  >
                    {selectedCustomer?.firstName} {selectedCustomer?.lastName}
                  </DialogTitle>
                  <button
                    onClick={handleCloseDetailModal}
                    className="text-gray-400 hover:text-gray-500"
                  >
                    <svg
                      className="h-6 w-6"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M6 18L18 6M6 6l12 12"
                      />
                    </svg>
                  </button>
                </div>

                {/* Tabs */}
                <div className="border-b border-gray-200">
                  <nav className="-mb-px flex space-x-8">
                    <button
                      data-tab="profile"
                      onClick={() => setActiveTab("profile")}
                      className={`${
                        activeTab === "profile"
                          ? "border-indigo-500 text-indigo-600"
                          : "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300"
                      } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm`}
                    >
                      Perfil
                    </button>
                    <button
                      data-tab="orders"
                      onClick={() => setActiveTab("orders")}
                      className={`${
                        activeTab === "orders"
                          ? "border-indigo-500 text-indigo-600"
                          : "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300"
                      } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm`}
                    >
                      Pedidos
                    </button>
                  </nav>
                </div>

                {/* Tab Content */}
                <div className="mt-6">
                  {activeTab === "profile" && selectedCustomer && (
                    <CustomerProfileTab customer={selectedCustomer} />
                  )}

                  {activeTab === "orders" && selectedCustomer && (
                    <CustomerOrdersTab customer={selectedCustomer} />
                  )}
                </div>
              </div>
            </DialogPanel>
          </div>
        </div>
      </Dialog>
    </div>
  );
};

export default CustomersPage;
