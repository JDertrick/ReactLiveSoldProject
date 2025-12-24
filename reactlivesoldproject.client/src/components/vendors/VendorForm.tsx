import { useState, useEffect } from "react";
import { useCreateVendor, useUpdateVendor } from "@/hooks/useVendors";
import { useContacts } from "@/hooks/useContacts";
import { useGetUsers } from "@/hooks/useUsers";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "../ui/dialog";
import { CreateVendorDto, Vendor, UpdateVendorDto } from "@/types";
import {
  Contact as ContactType,
  CreateContactDto,
} from "@/types/contact.types";
import { Package, UserPlus } from "lucide-react";
import { Input } from "../ui/input";
import { Label } from "../ui/label";
import { Button } from "../ui/button";
import { ContactCombobox } from "../common/ContactCombobox";
import { ContactForm } from "../contacts/ContactForm";
import { Separator } from "../ui/separator";
import { toast } from "sonner";
import { AutoNumberInput } from "../common/AutoNumberInput";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../ui/select";

interface VendorFormProps {
  isModalOpen: boolean;
  handleCloseModal: () => void;
  editingVendor: Vendor | null;
  onSuccess?: () => void;
}

const VendorForm = ({
  isModalOpen,
  handleCloseModal,
  editingVendor,
  onSuccess,
}: VendorFormProps) => {
  const createVendor = useCreateVendor();
  const updateVendor = useUpdateVendor();
  const {
    contacts,
    fetchContacts,
    createContact,
    loading: contactsLoading,
  } = useContacts();
  const { data: users } = useGetUsers();

  console.log(users);

  const [selectedContact, setSelectedContact] = useState<ContactType | null>(
    null
  );
  const [showContactForm, setShowContactForm] = useState(false);
  const [vendorNo, setVendorNo] = useState<string>("");
  const [assignedBuyerId, setAssignedBuyerId] = useState<string>("");
  const [notes, setNotes] = useState("");
  const [paymentTerms, setPaymentTerms] = useState("");
  const [creditLimit, setCreditLimit] = useState("");
  const [isActive, setIsActive] = useState(true);

  useEffect(() => {
    if (isModalOpen) {
      fetchContacts();
    }
  }, [isModalOpen, fetchContacts]);

  useEffect(() => {
    if (editingVendor) {
      // Si estamos editando, cargar el contacto asociado
      if (editingVendor.contact) {
        setSelectedContact(editingVendor.contact);
      }
      setVendorNo(editingVendor.vendorNo || "");
      setAssignedBuyerId(editingVendor.assignedBuyerId || "");
      setNotes(editingVendor.notes || "");
      setPaymentTerms(editingVendor.paymentTerms || "");
      setCreditLimit(editingVendor.creditLimit?.toString() || "");
      setIsActive(editingVendor.isActive);
    } else {
      // Limpiar al crear nuevo
      setSelectedContact(null);
      setVendorNo("");
      setAssignedBuyerId("");
      setNotes("");
      setPaymentTerms("");
      setCreditLimit("");
      setIsActive(true);
    }
  }, [editingVendor]);

  const handleCreateContact = async (contactData: CreateContactDto) => {
    const newContact = await createContact(contactData);
    if (newContact) {
      setSelectedContact(newContact);
      setShowContactForm(false);
      toast.success(
        "El contacto ha sido creado y seleccionado automáticamente"
      );
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedContact) {
      toast.error("Debe seleccionar un contacto");
      return;
    }

    try {
      if (editingVendor) {
        // Actualizar proveedor existente
        const updateData: UpdateVendorDto = {
          contactId: selectedContact.id,
          assignedBuyerId:
            assignedBuyerId && assignedBuyerId !== "null"
              ? assignedBuyerId
              : undefined,
          notes: notes || undefined,
          paymentTerms: paymentTerms || undefined,
          creditLimit: creditLimit ? parseFloat(creditLimit) : undefined,
          isActive,
        };

        await updateVendor.mutateAsync({
          id: editingVendor.id,
          data: updateData,
        });

        toast.success("El proveedor ha sido actualizado exitosamente");
      } else {
        // Crear nuevo proveedor
        const createData: CreateVendorDto = {
          contactId: selectedContact.id,
          assignedBuyerId:
            assignedBuyerId && assignedBuyerId !== "null"
              ? assignedBuyerId
              : undefined,
          notes: notes || undefined,
          paymentTerms: paymentTerms || undefined,
          creditLimit: creditLimit ? parseFloat(creditLimit) : 0,
          isActive,
        };

        await createVendor.mutateAsync(createData);

        toast.success("El proveedor ha sido creado exitosamente");
      }

      handleCloseModal();
      if (onSuccess) onSuccess();
    } catch (error: any) {
      console.log("Error creating vendor:", error.response?.data);
      toast.error(
        error.response?.data?.message ||
          "Ocurrió un error al guardar el proveedor"
      );
    }
  };

  const handleCancel = () => {
    handleCloseModal();
    setShowContactForm(false);
    setSelectedContact(null);
    setVendorNo("");
    setAssignedBuyerId("");
    setNotes("");
    setPaymentTerms("");
    setCreditLimit("");
    setIsActive(true);
  };

  return (
    <Dialog open={isModalOpen} onOpenChange={handleCloseModal}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        {showContactForm ? (
          <>
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2">
                <UserPlus className="h-5 w-5" />
                Crear Nuevo Contacto
              </DialogTitle>
              <DialogDescription>
                Completa la información del contacto. Una vez creado, se
                seleccionará automáticamente para el proveedor.
              </DialogDescription>
            </DialogHeader>
            <ContactForm
              onSubmit={handleCreateContact}
              onCancel={() => setShowContactForm(false)}
              isLoading={contactsLoading}
            />
          </>
        ) : (
          <form onSubmit={handleSubmit}>
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2">
                <Package className="h-5 w-5" />
                {editingVendor ? "Editar Proveedor" : "Crear Proveedor"}
              </DialogTitle>
              <DialogDescription>
                {editingVendor
                  ? "Modifica la información del proveedor"
                  : "Selecciona o crea un contacto y completa la información del proveedor"}
              </DialogDescription>
            </DialogHeader>

            <div className="space-y-6 py-4">
              {/* Número de Proveedor */}
              <AutoNumberInput
                label="No. Proveedor"
                value={vendorNo}
                onChange={setVendorNo}
                allowManualEntry={false} // TODO: Obtener desde configuración de serie
                isEditing={!!editingVendor}
                placeholder="Se generará automáticamente"
              />

              <Separator />

              {/* Selección de contacto */}
              <div className="space-y-2">
                <Label>Contacto *</Label>
                <ContactCombobox
                  contacts={contacts}
                  selectedContact={selectedContact}
                  onSelectContact={setSelectedContact}
                  onCreateNew={() => setShowContactForm(true)}
                  disabled={!!editingVendor} // No permitir cambiar contacto al editar
                />
                {selectedContact && (
                  <div className="mt-2 p-3 bg-muted rounded-md text-sm">
                    <div className="font-medium">
                      {selectedContact.firstName} {selectedContact.lastName}
                    </div>
                    <div className="text-muted-foreground">
                      {selectedContact.email}
                    </div>
                    {selectedContact.phone && (
                      <div className="text-muted-foreground">
                        {selectedContact.phone}
                      </div>
                    )}
                    {selectedContact.company && (
                      <div className="text-muted-foreground">
                        {selectedContact.company}
                      </div>
                    )}
                  </div>
                )}
              </div>

              <Separator />

              {/* Información del proveedor */}
              <div className="space-y-4">
                <h3 className="text-sm font-medium">
                  Información del Proveedor
                </h3>

                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="assignedBuyer">Comprador Asignado</Label>
                    <Select
                      value={assignedBuyerId}
                      onValueChange={setAssignedBuyerId}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Seleccionar comprador" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="null">Sin asignar</SelectItem>
                        {users?.map((user) => (
                          <SelectItem key={user.id} value={user.id}>
                            {user.firstName} {user.lastName}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="paymentTerms">Términos de Pago</Label>
                    <Input
                      id="paymentTerms"
                      type="text"
                      value={paymentTerms}
                      onChange={(e) => setPaymentTerms(e.target.value)}
                      placeholder="Net 30, Net 60, etc."
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="creditLimit">Límite de Crédito</Label>
                    <Input
                      id="creditLimit"
                      type="number"
                      step="0.01"
                      value={creditLimit}
                      onChange={(e) => setCreditLimit(e.target.value)}
                      placeholder="0.00"
                    />
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="notes">Notas</Label>
                  <textarea
                    id="notes"
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                    placeholder="Notas adicionales sobre el proveedor..."
                    className="w-full min-h-[80px] px-3 py-2 rounded-md border border-input bg-background"
                  />
                </div>

                <div className="flex items-center space-x-2">
                  <input
                    id="isActive"
                    type="checkbox"
                    checked={isActive}
                    onChange={(e) => setIsActive(e.target.checked)}
                    className="h-4 w-4 rounded border-gray-300"
                  />
                  <Label htmlFor="isActive">Proveedor activo</Label>
                </div>
              </div>
            </div>

            {/* Botones de acción */}
            <div className="flex justify-end gap-4 pt-4 border-t">
              <Button
                type="button"
                variant="outline"
                onClick={handleCancel}
                disabled={createVendor.isPending || updateVendor.isPending}
              >
                Cancelar
              </Button>
              <Button
                type="submit"
                disabled={
                  createVendor.isPending ||
                  updateVendor.isPending ||
                  !selectedContact
                }
              >
                {createVendor.isPending || updateVendor.isPending
                  ? "Guardando..."
                  : editingVendor
                  ? "Actualizar"
                  : "Crear Proveedor"}
              </Button>
            </div>
          </form>
        )}
      </DialogContent>
    </Dialog>
  );
};

export default VendorForm;
