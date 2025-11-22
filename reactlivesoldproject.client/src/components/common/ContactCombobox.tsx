import { useState } from "react";
import { Check, ChevronsUpDown, Search, UserPlus } from "lucide-react";
import { cn } from "../../lib/utils";
import { Button } from "../ui/button";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "../ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "../ui/popover";
import { Contact } from "../../types/contact.types";

interface ContactComboboxProps {
  contacts: Contact[] | undefined;
  selectedContact: Contact | null;
  onSelectContact: (contact: Contact | null) => void;
  onCreateNew?: () => void;
  disabled?: boolean;
}

export function ContactCombobox({
  contacts,
  selectedContact,
  onSelectContact,
  onCreateNew,
  disabled = false,
}: ContactComboboxProps) {
  const [open, setOpen] = useState(false);

  const getContactDisplayName = (contact: Contact) => {
    const fullName = `${contact.firstName || ''} ${contact.lastName || ''}`.trim();
    return fullName || contact.email;
  };

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className="w-full justify-between"
          disabled={disabled}
        >
          {selectedContact ? (
            <div className="flex items-center justify-between w-full">
              <span className="truncate">
                {getContactDisplayName(selectedContact)}
              </span>
              <span className="text-xs text-gray-500 ml-2">
                {selectedContact.email}
              </span>
            </div>
          ) : (
            <div className="flex items-center text-gray-500">
              <Search className="mr-2 h-4 w-4" />
              Buscar contacto...
            </div>
          )}
          <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[400px] p-0">
        <Command>
          <CommandInput placeholder="Buscar por nombre, email o empresa..." />
          <CommandList>
            <CommandEmpty>
              <div className="p-4 text-center">
                <p className="text-sm text-muted-foreground mb-3">
                  No se encontraron contactos
                </p>
                {onCreateNew && (
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      setOpen(false);
                      onCreateNew();
                    }}
                    className="w-full"
                  >
                    <UserPlus className="mr-2 h-4 w-4" />
                    Crear nuevo contacto
                  </Button>
                )}
              </div>
            </CommandEmpty>
            <CommandGroup>
              {onCreateNew && (
                <CommandItem
                  onSelect={() => {
                    setOpen(false);
                    onCreateNew();
                  }}
                  className="border-b"
                >
                  <UserPlus className="mr-2 h-4 w-4" />
                  <span className="font-medium">Crear nuevo contacto</span>
                </CommandItem>
              )}
              {contacts?.map((contact) => (
                <CommandItem
                  key={contact.id}
                  value={`${contact.firstName || ''} ${contact.lastName || ''} ${contact.email} ${contact.company || ''}`}
                  onSelect={() => {
                    onSelectContact(
                      selectedContact?.id === contact.id ? null : contact
                    );
                    setOpen(false);
                  }}
                >
                  <Check
                    className={cn(
                      "mr-2 h-4 w-4",
                      selectedContact?.id === contact.id
                        ? "opacity-100"
                        : "opacity-0"
                    )}
                  />
                  <div className="flex flex-col flex-1 min-w-0">
                    <div className="flex items-center justify-between">
                      <span className="font-medium truncate">
                        {getContactDisplayName(contact)}
                      </span>
                      {!contact.isActive && (
                        <span className="ml-2 text-xs bg-gray-100 px-2 py-0.5 rounded">
                          Inactivo
                        </span>
                      )}
                    </div>
                    <div className="flex items-center text-xs text-gray-500">
                      <span className="truncate">{contact.email}</span>
                      {contact.company && (
                        <span className="ml-2 truncate">• {contact.company}</span>
                      )}
                    </div>
                  </div>
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}
