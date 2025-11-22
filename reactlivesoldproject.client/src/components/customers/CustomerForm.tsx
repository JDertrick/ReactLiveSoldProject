import { useState, useEffect } from "react";
import { useCreateCustomer, useUpdateCustomer } from "@/hooks/useCustomers";
import { useContacts } from "@/hooks/useContacts";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "../ui/dialog";
import { CreateCustomerDto, Customer, UpdateCustomerDto } from "@/types";
import {
  Contact as ContactType,
  CreateContactDto,
} from "@/types/contact.types";
import { UserCircle, UserPlus } from "lucide-react";
import { Input } from "../ui/input";
import { Label } from "../ui/label";
import { Button } from "../ui/button";
import { ContactCombobox } from "../common/ContactCombobox";
import { ContactForm } from "../contacts/ContactForm";
import { Separator } from "../ui/separator";
import { toast } from "sonner";

interface CustomerFormProps {
  isModalOpen: boolean;
  handleCloseModal: () => void;
  editingCustomer: Customer | null;
  onSuccess?: () => void;
}

const CustomerForm = ({
  isModalOpen,
  handleCloseModal,
  editingCustomer,
  onSuccess,
}: CustomerFormProps) => {
  const createCustomer = useCreateCustomer();
  const updateCustomer = useUpdateCustomer();
  const {
    contacts,
    fetchContacts,
    createContact,
    loading: contactsLoading,
  } = useContacts();

  const [selectedContact, setSelectedContact] = useState<ContactType | null>(
    null
  );
  const [showContactForm, setShowContactForm] = useState(false);
  const [password, setPassword] = useState("");
  const [notes, setNotes] = useState("");
  const [isActive, setIsActive] = useState(true);

  useEffect(() => {
    if (isModalOpen) {
      fetchContacts();
    }
  }, [isModalOpen, fetchContacts]);

  useEffect(() => {
    if (editingCustomer) {
      // Si estamos editando, cargar el contacto asociado
      if (editingCustomer.contact) {
        setSelectedContact(editingCustomer.contact);
      }
      setNotes(editingCustomer.notes || "");
      setIsActive(editingCustomer.isActive);
    } else {
      // Limpiar al crear nuevo
      setSelectedContact(null);
      setPassword("");
      setNotes("");
      setIsActive(true);
    }
  }, [editingCustomer]);

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

    if (!editingCustomer && !password) {
      toast.error("La contraseña es obligatoria");
      return;
    }

    try {
      if (editingCustomer) {
        // Actualizar cliente existente
        const updateData: UpdateCustomerDto = {
          email: selectedContact.email,
          firstName: selectedContact.firstName,
          lastName: selectedContact.lastName,
          phone: selectedContact.phone,
          address: selectedContact.address,
          city: selectedContact.city,
          state: selectedContact.state,
          postalCode: selectedContact.postalCode,
          country: selectedContact.country,
          company: selectedContact.company,
          notes,
          isActive,
          password: password || undefined,
        };

        await updateCustomer.mutateAsync({
          id: editingCustomer.id,
          data: updateData,
        });

        toast.success("El cliente ha sido actualizado exitosamente");
      } else {
        // Crear nuevo cliente
        const createData: CreateCustomerDto = {
          email: selectedContact.email,
          firstName: selectedContact.firstName,
          lastName: selectedContact.lastName,
          phone: selectedContact.phone,
          address: selectedContact.address,
          city: selectedContact.city,
          state: selectedContact.state,
          postalCode: selectedContact.postalCode,
          country: selectedContact.country,
          company: selectedContact.company,
          password,
          notes,
          isActive,
        };

        await createCustomer.mutateAsync(createData);

        toast.success("El cliente ha sido creado exitosamente");
      }

      handleCloseModal();
      if (onSuccess) onSuccess();
    } catch (error: any) {
      toast.error(
        error.response?.data?.message ||
          "Ocurrió un error al guardar el cliente"
      );
    }
  };

  const handleCancel = () => {
    handleCloseModal();
    setShowContactForm(false);
    setSelectedContact(null);
    setPassword("");
    setNotes("");
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
                seleccionará automáticamente para el cliente.
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
                <UserCircle className="h-5 w-5" />
                {editingCustomer ? "Editar Cliente" : "Crear Cliente"}
              </DialogTitle>
              <DialogDescription>
                {editingCustomer
                  ? "Modifica la información del cliente"
                  : "Selecciona o crea un contacto y proporciona las credenciales"}
              </DialogDescription>
            </DialogHeader>

            <div className="space-y-6 py-4">
              {/* Selección de contacto */}
              <div className="space-y-2">
                <Label>Contacto *</Label>
                <ContactCombobox
                  contacts={contacts}
                  selectedContact={selectedContact}
                  onSelectContact={setSelectedContact}
                  onCreateNew={() => setShowContactForm(true)}
                  disabled={!!editingCustomer} // No permitir cambiar contacto al editar
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

              {/* Información del cliente */}
              <div className="space-y-4">
                <h3 className="text-sm font-medium">
                  Credenciales del Cliente
                </h3>

                <div className="space-y-2">
                  <Label htmlFor="password">
                    Contraseña {!editingCustomer && "*"}
                  </Label>
                  <Input
                    id="password"
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    placeholder={
                      editingCustomer
                        ? "Dejar en blanco para mantener"
                        : "Contraseña"
                    }
                    required={!editingCustomer}
                  />
                  {editingCustomer && (
                    <p className="text-xs text-muted-foreground">
                      Deja en blanco para mantener la contraseña actual
                    </p>
                  )}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="notes">Notas</Label>
                  <textarea
                    id="notes"
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                    placeholder="Notas adicionales sobre el cliente..."
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
                  <Label htmlFor="isActive">Cliente activo</Label>
                </div>
              </div>
            </div>

            {/* Botones de acción */}
            <div className="flex justify-end gap-4 pt-4 border-t">
              <Button
                type="button"
                variant="outline"
                onClick={handleCancel}
                disabled={createCustomer.isPending || updateCustomer.isPending}
              >
                Cancelar
              </Button>
              <Button
                type="submit"
                disabled={
                  createCustomer.isPending ||
                  updateCustomer.isPending ||
                  !selectedContact
                }
              >
                {createCustomer.isPending || updateCustomer.isPending
                  ? "Guardando..."
                  : editingCustomer
                  ? "Actualizar"
                  : "Crear Cliente"}
              </Button>
            </div>
          </form>
        )}
      </DialogContent>
    </Dialog>
  );
};

export default CustomerForm;
