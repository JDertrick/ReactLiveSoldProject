import { useState, useEffect } from "react";
import { useContacts } from "../../hooks/useContacts";
import { Contact, CreateContactDto, UpdateContactDto } from "../../types/contact.types";
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
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Users,
  UserPlus,
  MoreHorizontal,
  Edit,
  Trash2,
  Building2,
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
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { ContactForm } from "@/components/contacts/ContactForm";
import { useDebounce } from "@/hooks/useDebounce";
import { useToast } from "@/hooks/use-toast";

const columnHelper = createColumnHelper<Contact>();

const ContactsPage = () => {
  const { toast } = useToast();
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingContact, setEditingContact] = useState<Contact | undefined>(undefined);

  const debouncedSearchTerm = useDebounce(searchTerm, 500);

  const {
    contacts,
    loading,
    error,
    fetchContacts,
    createContact,
    updateContact,
    deleteContact,
  } = useContacts();

  useEffect(() => {
    fetchContacts(debouncedSearchTerm, statusFilter);
  }, [debouncedSearchTerm, statusFilter, fetchContacts]);

  const handleCreateContact = async (data: CreateContactDto) => {
    const result = await createContact(data);
    if (result) {
      toast({
        title: "Contacto creado",
        description: "El contacto ha sido creado exitosamente",
      });
      setIsDialogOpen(false);
    } else {
      toast({
        title: "Error",
        description: error || "No se pudo crear el contacto",
        variant: "destructive",
      });
    }
  };

  const handleUpdateContact = async (data: UpdateContactDto) => {
    if (!editingContact) return;

    const result = await updateContact(editingContact.id, data);
    if (result) {
      toast({
        title: "Contacto actualizado",
        description: "El contacto ha sido actualizado exitosamente",
      });
      setIsDialogOpen(false);
      setEditingContact(undefined);
    } else {
      toast({
        title: "Error",
        description: error || "No se pudo actualizar el contacto",
        variant: "destructive",
      });
    }
  };

  const handleDeleteContact = async (id: string) => {
    if (!confirm("¿Está seguro de eliminar este contacto?")) return;

    const result = await deleteContact(id);
    if (result) {
      toast({
        title: "Contacto eliminado",
        description: "El contacto ha sido eliminado exitosamente",
      });
    } else {
      toast({
        title: "Error",
        description: error || "No se pudo eliminar el contacto",
        variant: "destructive",
      });
    }
  };

  const openCreateDialog = () => {
    setEditingContact(undefined);
    setIsDialogOpen(true);
  };

  const openEditDialog = (contact: Contact) => {
    setEditingContact(contact);
    setIsDialogOpen(true);
  };

  const closeDialog = () => {
    setIsDialogOpen(false);
    setEditingContact(undefined);
  };

  const columns = [
    columnHelper.accessor("firstName", {
      header: "Nombre",
      cell: (info) => {
        const contact = info.row.original;
        const fullName = `${contact.firstName || ''} ${contact.lastName || ''}`.trim() || 'Sin nombre';
        return <div className="font-medium">{fullName}</div>;
      },
    }),
    columnHelper.accessor("email", {
      header: "Email",
      cell: (info) => <div className="text-sm">{info.getValue()}</div>,
    }),
    columnHelper.accessor("phone", {
      header: "Teléfono",
      cell: (info) => <div className="text-sm">{info.getValue() || '-'}</div>,
    }),
    columnHelper.accessor("company", {
      header: "Empresa",
      cell: (info) => {
        const company = info.getValue();
        return company ? (
          <div className="flex items-center gap-2">
            <Building2 className="h-4 w-4 text-muted-foreground" />
            <span className="text-sm">{company}</span>
          </div>
        ) : (
          <span className="text-sm text-muted-foreground">-</span>
        );
      },
    }),
    columnHelper.accessor("jobTitle", {
      header: "Puesto",
      cell: (info) => <div className="text-sm">{info.getValue() || '-'}</div>,
    }),
    columnHelper.accessor("isActive", {
      header: "Estado",
      cell: (info) => (
        <Badge variant={info.getValue() ? "default" : "secondary"}>
          {info.getValue() ? "Activo" : "Inactivo"}
        </Badge>
      ),
    }),
    columnHelper.display({
      id: "actions",
      header: "Acciones",
      cell: (info) => (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" className="h-8 w-8 p-0">
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={() => openEditDialog(info.row.original)}>
              <Edit className="mr-2 h-4 w-4" />
              Editar
            </DropdownMenuItem>
            <DropdownMenuItem
              onClick={() => handleDeleteContact(info.row.original.id)}
              className="text-red-600"
            >
              <Trash2 className="mr-2 h-4 w-4" />
              Eliminar
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      ),
    }),
  ];

  const table = useReactTable({
    data: contacts,
    columns,
    getCoreRowModel: getCoreRowModel(),
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Contactos</h1>
          <p className="text-muted-foreground">
            Gestiona los contactos de tu organización
          </p>
        </div>
        <Button onClick={openCreateDialog}>
          <UserPlus className="mr-2 h-4 w-4" />
          Nuevo Contacto
        </Button>
      </div>

      {/* Stats */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Contactos</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{contacts.length}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Activos</CardTitle>
            <Users className="h-4 w-4 text-green-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {contacts.filter((c) => c.isActive).length}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Con Empresa</CardTitle>
            <Building2 className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {contacts.filter((c) => c.company).length}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="flex-1">
              <Input
                placeholder="Buscar por nombre, email, empresa..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full"
              />
            </div>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger className="w-full sm:w-[180px]">
                <SelectValue placeholder="Estado" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos</SelectItem>
                <SelectItem value="active">Activos</SelectItem>
                <SelectItem value="inactive">Inactivos</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Table */}
      <Card>
        <CardContent className="pt-6">
          {loading ? (
            <div className="text-center py-8">Cargando contactos...</div>
          ) : contacts.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              No se encontraron contactos
            </div>
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
                  {table.getRowModel().rows.map((row) => (
                    <TableRow key={row.id}>
                      {row.getVisibleCells().map((cell) => (
                        <TableCell key={cell.id}>
                          {flexRender(cell.column.columnDef.cell, cell.getContext())}
                        </TableCell>
                      ))}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Dialog */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              {editingContact ? "Editar Contacto" : "Nuevo Contacto"}
            </DialogTitle>
            <DialogDescription>
              {editingContact
                ? "Modifica la información del contacto"
                : "Completa la información para crear un nuevo contacto"}
            </DialogDescription>
          </DialogHeader>
          <ContactForm
            contact={editingContact}
            onSubmit={editingContact ? handleUpdateContact : handleCreateContact}
            onCancel={closeDialog}
            isLoading={loading}
          />
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default ContactsPage;
