import { useState } from "react";
import { Check, ChevronsUpDown, Search } from "lucide-react";
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
import { Customer } from "../../types/customer.types";

interface CustomerComboboxProps {
  customers: Customer[] | undefined;
  selectedCustomer: Customer | null;
  onSelectCustomer: (customer: Customer | null) => void;
  disabled?: boolean;
}

export function CustomerCombobox({
  customers,
  selectedCustomer,
  onSelectCustomer,
  disabled = false,
}: CustomerComboboxProps) {
  const [open, setOpen] = useState(false);

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
          {selectedCustomer ? (
            <div className="flex items-center justify-between w-full">
              <span className="truncate">
                {selectedCustomer.firstName} {selectedCustomer.lastName}
              </span>
              <span className="text-xs text-gray-500 ml-2">
                ${selectedCustomer.wallet?.balance.toFixed(2) || '0.00'}
              </span>
            </div>
          ) : (
            <div className="flex items-center text-gray-500">
              <Search className="mr-2 h-4 w-4" />
              Buscar cliente...
            </div>
          )}
          <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[400px] p-0">
        <Command>
          <CommandInput placeholder="Buscar por nombre, email o teléfono..." />
          <CommandList>
            <CommandEmpty>No se encontraron clientes.</CommandEmpty>
            <CommandGroup>
              {customers?.map((customer) => (
                <CommandItem
                  key={customer.id}
                  value={`${customer.firstName} ${customer.lastName} ${customer.email} ${customer.phoneNumber}`}
                  onSelect={() => {
                    onSelectCustomer(
                      selectedCustomer?.id === customer.id ? null : customer
                    );
                    setOpen(false);
                  }}
                >
                  <Check
                    className={cn(
                      "mr-2 h-4 w-4",
                      selectedCustomer?.id === customer.id
                        ? "opacity-100"
                        : "opacity-0"
                    )}
                  />
                  <div className="flex-1 flex justify-between items-center">
                    <div>
                      <p className="text-sm font-medium">
                        {customer.firstName} {customer.lastName}
                      </p>
                      <p className="text-xs text-gray-500">
                        {customer.email} • {customer.phoneNumber}
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-bold text-green-600">
                        ${customer.wallet?.balance.toFixed(2) || '0.00'}
                      </p>
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
