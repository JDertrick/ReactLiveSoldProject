import {
  Check,
  ChevronsUpDown,
  DollarSign,
  Hash,
  Package,
  TrendingDown,
  TrendingUp,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
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
import { Textarea } from "@/components/ui/textarea";
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
    movementType: StockMovementType.Adjustment, // Default to Adjustment
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
      // Guardar el producto
      await onSubmit(formData);
    } catch (error) {
      console.error("Error al guardar producto:", error);
      throw error;
    }
  };

  return (
    <>
      <Dialog open={isAddModalOpen} onOpenChange={onClose}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <form onSubmit={handleSubmit}>
            <DialogHeader>
              <DialogTitle className="text-2xl font-bold flex items-center gap-2">
                <Package className="w-6 h-6" />
                Registrar Movimiento de Inventario
              </DialogTitle>
              <DialogDescription className="text-base">
                Los movimientos se crean como <strong>borrador</strong>. Deberá
                postearlos para que afecten el inventario.
              </DialogDescription>
            </DialogHeader>

            <div className="space-y-6 py-6">
              {/* Product Variant Search */}
              <div className="space-y-2">
                <Label className="text-base font-semibold flex items-center gap-2">
                  <Package className="w-4 h-4" />
                  Producto / Variante
                </Label>
                <Popover open={openCombobox} onOpenChange={setOpenCombobox}>
                  <PopoverTrigger asChild>
                    <Button
                      variant="outline"
                      role="combobox"
                      aria-expanded={openCombobox}
                      className="w-full justify-between h-auto min-h-[50px]"
                    >
                      {selectedVariant ? (
                        <div className="flex flex-col items-start gap-1 text-left">
                          <span className="font-semibold">
                            {selectedVariant.productName}
                          </span>
                          <span className="text-xs text-gray-500">
                            SKU: {selectedVariant.sku} | Stock:{" "}
                            {selectedVariant.stock} | Costo Prom: $
                            {selectedVariant.averageCost.toFixed(2)}
                          </span>
                        </div>
                      ) : (
                        "Buscar producto..."
                      )}
                      <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-[600px] p-0">
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
                              className="py-3"
                            >
                              <Check
                                className={cn(
                                  "mr-2 h-4 w-4",
                                  formData.productVariantId === variant.value
                                    ? "opacity-100"
                                    : "opacity-0"
                                )}
                              />
                              <div className="flex flex-col gap-1">
                                <span className="font-semibold">
                                  {variant.productName}
                                </span>
                                <span className="text-xs text-gray-500">
                                  SKU: {variant.sku} | Stock: {variant.stock} |
                                  Precio: ${variant.price.toFixed(2)} | Costo
                                  Prom: ${variant.averageCost.toFixed(2)}
                                </span>
                              </div>
                            </CommandItem>
                          ))}
                        </CommandGroup>
                      </CommandList>
                    </Command>
                  </PopoverContent>
                </Popover>
                {selectedVariant && (
                  <div className="bg-blue-50 border border-blue-200 rounded-md p-3 mt-2">
                    <div className="grid grid-cols-3 gap-2 text-sm">
                      <div>
                        <span className="text-gray-600">Stock Actual:</span>
                        <p className="font-bold text-lg">
                          {selectedVariant.stock}
                        </p>
                      </div>
                      <div>
                        <span className="text-gray-600">Precio Venta:</span>
                        <p className="font-bold text-lg">
                          ${selectedVariant.price.toFixed(2)}
                        </p>
                      </div>
                      <div>
                        <span className="text-gray-600">Costo Promedio:</span>
                        <p className="font-bold text-lg">
                          ${selectedVariant.averageCost.toFixed(2)}
                        </p>
                      </div>
                    </div>
                  </div>
                )}
              </div>

              <div className="grid grid-cols-2 gap-4">
                {/* Movement Type */}
                <div className="space-y-2">
                  <Label className="text-base font-semibold">
                    Tipo de Movimiento
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
                      <SelectItem value={StockMovementType.Purchase}>
                        <div className="flex items-center gap-2">
                          <TrendingUp className="w-4 h-4 text-green-600" />
                          Compra
                        </div>
                      </SelectItem>
                      <SelectItem value={StockMovementType.Loss}>
                        <div className="flex items-center gap-2">
                          <TrendingDown className="w-4 h-4 text-red-600" />
                          Pérdida
                        </div>
                      </SelectItem>
                      <SelectItem value={StockMovementType.Return}>
                        <div className="flex items-center gap-2">
                          <TrendingUp className="w-4 h-4 text-blue-600" />
                          Devolución
                        </div>
                      </SelectItem>
                      {locations && locations.length >= 2 && (
                        <SelectItem value={StockMovementType.Transfer}>
                          <div className="flex items-center gap-2">
                            <Package className="w-4 h-4 text-purple-600" />
                            Transferencia
                          </div>
                        </SelectItem>
                      )}
                    </SelectContent>
                  </Select>
                </div>

                {/* Quantity */}
                <div className="space-y-2">
                  <Label className="text-base font-semibold flex items-center gap-2">
                    <Hash className="w-4 h-4" />
                    Cantidad
                  </Label>
                  <Input
                    type="number"
                    value={formData.quantity}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        quantity: parseInt(e.target.value) || 0,
                      })
                    }
                    placeholder="Ej: 10"
                    className="h-12 text-lg"
                    required
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                {/* Unit Cost */}
                <div className="space-y-2">
                  <Label className="text-base font-semibold flex items-center gap-2">
                    <DollarSign className="w-4 h-4" />
                    Costo Unitario
                    {formData.movementType === StockMovementType.Purchase && (
                      <Badge variant="destructive" className="ml-2">
                        Requerido
                      </Badge>
                    )}
                  </Label>
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
                    placeholder="$0.00"
                    className="h-12 text-lg"
                    required={
                      formData.movementType === StockMovementType.Purchase
                    }
                  />
                  <p className="text-xs text-gray-500">
                    💡 Se usa para calcular el costo promedio ponderado
                  </p>
                </div>

                {/* Reference */}
                <div className="space-y-2">
                  <Label className="text-base font-semibold">Referencia</Label>
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

              {/* Location Selectors */}
              {formData.movementType === StockMovementType.Transfer ? (
                // Para transferencias: mostrar origen y destino
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label className="text-base font-semibold flex items-center gap-2">
                      <Package className="w-4 h-4" />
                      Ubicación Origen
                      <Badge variant="destructive" className="ml-2">
                        Requerido
                      </Badge>
                    </Label>
                    <Select
                      value={formData.sourceLocationId || ""}
                      onValueChange={(value) =>
                        setFormData({ ...formData, sourceLocationId: value })
                      }
                    >
                      <SelectTrigger className="h-12">
                        <SelectValue placeholder="Seleccionar origen..." />
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

                  <div className="space-y-2">
                    <Label className="text-base font-semibold flex items-center gap-2">
                      <Package className="w-4 h-4" />
                      Ubicación Destino
                      <Badge variant="destructive" className="ml-2">
                        Requerido
                      </Badge>
                    </Label>
                    <Select
                      value={formData.destinationLocationId || ""}
                      onValueChange={(value) =>
                        setFormData({
                          ...formData,
                          destinationLocationId: value,
                        })
                      }
                    >
                      <SelectTrigger className="h-12">
                        <SelectValue placeholder="Seleccionar destino..." />
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
                </div>
              ) : (
                // Para otros movimientos: solo ubicación de destino
                <div className="space-y-2">
                  <Label className="text-base font-semibold flex items-center gap-2">
                    <Package className="w-4 h-4" />
                    Ubicación
                  </Label>
                  <Select
                    value={formData.destinationLocationId || ""}
                    onValueChange={(value) =>
                      setFormData({ ...formData, destinationLocationId: value })
                    }
                  >
                    <SelectTrigger className="h-12">
                      <SelectValue placeholder="Seleccionar ubicación (opcional)..." />
                    </SelectTrigger>
                    <SelectContent>
                      {locations?.map((location) => (
                        <SelectItem key={location.id} value={location.id}>
                          {location.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <p className="text-xs text-gray-500">
                    💡 Especifica donde ocurre el movimiento
                  </p>
                </div>
              )}

              {/* Notes */}
              <div className="space-y-2">
                <Label className="text-base font-semibold">Notas</Label>
                <Textarea
                  value={formData.notes || ""}
                  onChange={(e) =>
                    setFormData({ ...formData, notes: e.target.value })
                  }
                  placeholder="Descripción del movimiento (opcional)..."
                  className="min-h-[100px] resize-none"
                />
              </div>
            </div>

            <DialogFooter className="gap-2">
              <Button
                type="button"
                variant="outline"
                onClick={() => onClose()}
                className="px-6"
              >
                Cancelar
              </Button>
              <Button
                type="submit"
                disabled={createMovement.isPending}
                className="px-6"
              >
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
