import {
  Box,
  Check,
  ChevronsUpDown,
  ClipboardPlus,
  DollarSign,
  Hash,
  Info,
  MapPin,
  Package,
  Search,
  TrendingDown,
  TrendingUp,
  X,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  CreateStockMovementDto,
  StockMovementType,
} from "../../types/stockmovement.types";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useMemo, useState } from "react";
import { VariantProductDto } from "@/types";
import { cn } from "@/lib/utils";
import { useCreateStockMovement } from "@/hooks/useStockMovements";
import { useLocations } from "@/hooks/useLocations";
import { Textarea } from "../ui/textarea";

interface StockMovementComponentFormDialogProps {
  isAddModalOpen: boolean;
  productVariants: VariantProductDto[];
  onClose: () => void;
  onSubmit: (formData: CreateStockMovementDto) => Promise<void>;
}

const StockMovementComponentFormDialog = ({
  isAddModalOpen,
  productVariants,
  onClose,
  onSubmit,
}: StockMovementComponentFormDialogProps) => {
  const createMovement = useCreateStockMovement();
  const { locations } = useLocations();

  const [openCombobox, setOpenCombobox] = useState(false);
  const [formData, setFormData] = useState<CreateStockMovementDto>({
    productVariantId: "",
    movementType: StockMovementType.Adjustment,
    quantity: 0,
    notes: "",
    reference: "",
    unitCost: undefined,
    sourceLocationId: undefined,
    destinationLocationId: undefined,
  });

  const productVariantOptions = useMemo(() => {
    if (!productVariants) return [];

    return (
      productVariants.map((variant) => ({
        value: variant.id,
        label: `${variant.productName} - ${variant.sku || "Sin SKU"}`,
        productName: variant.productName,
        sku: variant.sku || "Sin SKU",
        stock: variant.stockQuantity,
        price: variant.price,
        averageCost: variant.price,
      })) || []
    );
  }, [productVariants]);

  const selectedVariant = productVariantOptions.find(
    (v) => v.value === formData.productVariantId
  );

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      await onSubmit(formData);
    } catch (error) {
      console.error("Error al guardar producto:", error);
      throw error;
    }
  };

  return (
    <>
      <Dialog open={isAddModalOpen} onOpenChange={onClose}>
        <DialogContent className="max-w-xl max-h-[90vh] overflow-y-auto p-0">
          <form onSubmit={handleSubmit}>
            <DialogHeader className="p-6 pb-4">
              <div className="flex items-center gap-4">
                <div className="bg-indigo-100 p-3 rounded-lg">
                  <Box className="w-6 h-6 text-indigo-600" />
                </div>
                <div>
                  <DialogTitle className="text-xl font-bold">
                    Registrar Movimiento
                  </DialogTitle>
                  <DialogDescription className="text-sm text-gray-500">
                    Los movimientos se crean como{" "}
                    <span className="font-semibold text-gray-700">
                      borrador
                    </span>{" "}
                    hasta ser posteados.
                  </DialogDescription>
                </div>
              </div>
            </DialogHeader>

            <div className="p-6 space-y-6">
              {/* Product Variant Search */}
              <div className="space-y-2">
                <Label className="text-xs font-semibold text-gray-500 tracking-wider">
                  PRODUCTO / VARIANTE
                </Label>
                <Popover open={openCombobox} onOpenChange={setOpenCombobox}>
                  <PopoverTrigger asChild>
                    <Button
                      variant="outline"
                      role="combobox"
                      aria-expanded={openCombobox}
                      className="w-full justify-between h-12 text-gray-500"
                    >
                      <div className="flex items-center">
                        <Search className="mr-2 h-4 w-4 shrink-0 opacity-50" />
                        {selectedVariant ? (
                          <span className="text-black">
                            {selectedVariant.label}
                          </span>
                        ) : (
                          "Buscar producto por nombre o SKU..."
                        )}
                      </div>
                      <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-130 p-0">
                    <Command>
                      <CommandInput placeholder="Buscar producto o SKU..." />
                      <CommandList>
                        <CommandEmpty>
                          No se encontraron productos.
                        </CommandEmpty>
                        <CommandGroup>
                          {productVariantOptions.map((variant) => (
                            <CommandItem
                              key={variant.value}
                              value={variant.label}
                              onSelect={() => {
                                setFormData({
                                  ...formData,
                                  productVariantId: variant.value,
                                });
                                setOpenCombobox(false);
                              }}
                              className="py-2"
                            >
                              <Check
                                className={cn(
                                  "mr-2 h-4 w-4",
                                  formData.productVariantId === variant.value
                                    ? "opacity-100"
                                    : "opacity-0"
                                )}
                              />
                              <div className="flex flex-col">
                                <span>{variant.productName}</span>
                                <span className="text-xs text-gray-500">
                                  SKU: {variant.sku}
                                </span>
                              </div>
                            </CommandItem>
                          ))}
                        </CommandGroup>
                      </CommandList>
                    </Command>
                  </PopoverContent>
                </Popover>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label className="text-xs font-semibold text-gray-500 tracking-wider">
                    TIPO DE MOVIMIENTO
                  </Label>
                  <Select
                    value={formData.movementType}
                    onValueChange={(value: string) =>
                      setFormData({ ...formData, movementType: value })
                    }
                  >
                    <SelectTrigger className="h-12">
                      <SelectValue placeholder="Seleccionar tipo..." />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value={StockMovementType.Adjustment}>
                        Ajuste de Inventario
                      </SelectItem>
                      <SelectItem value={StockMovementType.Purchase}>
                        Compra
                      </SelectItem>
                      <SelectItem value={StockMovementType.Loss}>
                        Pérdida
                      </SelectItem>
                      <SelectItem value={StockMovementType.Return}>
                        Devolución
                      </SelectItem>
                      {locations && locations.length >= 2 && (
                        <SelectItem value={StockMovementType.Transfer}>
                          Transferencia
                        </SelectItem>
                      )}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label className="text-xs font-semibold text-gray-500 tracking-wider">
                    CANTIDAD
                  </Label>
                  <div className="relative">
                    <Hash className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
                    <Input
                      type="number"
                      value={formData.quantity}
                      onChange={(e) =>
                        setFormData({
                          ...formData,
                          quantity: parseInt(e.target.value) || 0,
                        })
                      }
                      placeholder="0"
                      className="pl-9"
                      required
                    />
                  </div>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label className="text-xs font-semibold text-gray-500 tracking-wider">
                    COSTO UNITARIO
                  </Label>
                  <div className="relative">
                    <DollarSign className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
                    <Input
                      type="number"
                      step="0.01"
                      value={formData.unitCost || ""}
                      onChange={(e) =>
                        setFormData({
                          ...formData,
                          unitCost: e.target.value
                            ? parseFloat(e.target.value)
                            : undefined,
                        })
                      }
                      placeholder="0.00"
                      className="h-12 pl-9"
                      required={
                        formData.movementType === StockMovementType.Purchase
                      }
                    />
                  </div>
                  <p className="text-xs text-gray-500 flex items-center gap-1">
                    <Info className="w-3 h-3" />
                    Usado para cálculo de costo promedio
                  </p>
                </div>

                <div className="space-y-2">
                  <Label className="text-xs font-semibold text-gray-500 tracking-wider">
                    REFERENCIA
                  </Label>
                  <Input
                    value={formData.reference || ""}
                    onChange={(e) =>
                      setFormData({ ...formData, reference: e.target.value })
                    }
                    placeholder="Ej: Factura #123"
                    className="h-12"
                  />
                </div>
              </div>

              {/* Location Selector */}
              <div className="space-y-2">
                <Label className="text-xs font-semibold text-gray-500 tracking-wider">
                  UBICACIÓN
                </Label>
                <Select
                  value={formData.destinationLocationId || ""}
                  onValueChange={(value) =>
                    setFormData({ ...formData, destinationLocationId: value })
                  }
                >
                  <SelectTrigger className="h-12">
                    <div className="flex items-center text-gray-500">
                      <MapPin className="mr-2 h-4 w-4" />
                      <SelectValue placeholder="Seleccionar ubicación..." />
                    </div>
                  </SelectTrigger>
                  <SelectContent>
                    {locations?.map((location) => (
                      <SelectItem key={location.id} value={location.id}>
                        {location.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              {/* Notes */}
              <div className="space-y-2">
                <Label className="text-xs font-semibold text-gray-500 tracking-wider">
                  NOTAS
                </Label>
                <Textarea
                  value={formData.notes || ""}
                  onChange={(e) =>
                    setFormData({ ...formData, notes: e.target.value })
                  }
                  placeholder="Descripción del movimiento..."
                  className="min-h-[100px] resize-none"
                />
              </div>
            </div>

            <DialogFooter className="bg-gray-50 px-6 py-4 rounded-b-lg flex justify-end gap-2">
              <Button type="button" variant="ghost" onClick={() => onClose()}>
                Cancelar
              </Button>
              <Button
                type="submit"
                disabled={createMovement.isPending}
                className="bg-slate-900 text-white hover:bg-slate-800"
              >
                <ClipboardPlus className="w-4 h-4 mr-2" />
                {createMovement.isPending ? "Guardando..." : "Crear Movimiento"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </>
  );
};

export default StockMovementComponentFormDialog;
