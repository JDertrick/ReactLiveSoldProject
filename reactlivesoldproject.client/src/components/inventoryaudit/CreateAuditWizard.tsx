import { useState, useMemo } from "react";
import { useCategories } from "../../hooks/useCategories";
import { useLocations } from "../../hooks/useLocations";
import { useGetTags, useGetProducts } from "../../hooks/useProducts";
import { CreateInventoryAuditDto, AuditScopeType } from "../../types/inventoryAudit.types";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Warehouse,
  ClipboardList,
  Filter,
  Shuffle,
  Clock,
  Package,
  Tag,
  FolderTree,
  X,
  ListPlus,
  Search,
} from "lucide-react";
import { useDebounce } from "@uidotdev/usehooks";

interface CreateAuditWizardProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateInventoryAuditDto) => Promise<void>;
  isLoading?: boolean;
}

const ProductSelectItem = ({ item, isSelected, onToggle } : any) => (
    <div
      key={item.id}
      className={`flex items-center gap-3 p-2 rounded-md transition-colors ${
        isSelected ? 'bg-primary/10' : 'hover:bg-muted/50'
      }`}
    >
      <Checkbox
        id={`product-${item.id}`}
        checked={isSelected}
        onCheckedChange={onToggle}
      />
      <div className="flex-1">
        <Label htmlFor={`product-${item.id}`} className="font-normal cursor-pointer">
          {item.name}
        </Label>
        <p className="text-xs text-muted-foreground">{item.sku}</p>
      </div>
    </div>
  );

export const CreateAuditWizard = ({
  open,
  onClose,
  onSubmit,
  isLoading = false,
}: CreateAuditWizardProps) => {
  const { categories } = useCategories();
  const { locations } = useLocations();
  const { data: tags } = useGetTags();

  const [step, setStep] = useState(1);
  const [partialType, setPartialType] = useState<"Filter" | "Manual">("Filter");
  const [productSearch, setProductSearch] = useState("");
  const debouncedSearchTerm = useDebounce(productSearch, 300);

  const { data: productsData } = useGetProducts(
    1,
    1000,
    "Published",
    debouncedSearchTerm
  );
  const allProducts = useMemo(() => productsData?.items || [], [productsData]);

  const [formData, setFormData] = useState<CreateInventoryAuditDto>({
    name: "",
    description: "",
    notes: "",
    scopeType: "Total",
    locationId: undefined,
    categoryIds: [],
    tagIds: [],
    productVariantIds: [],
    randomSampleCount: 0,
    excludeAuditedInLastDays: 0,
  });

  const handleClose = () => {
    setStep(1);
    setPartialType("Filter");
    setProductSearch("");
    setFormData({
      name: "",
      description: "",
      notes: "",
      scopeType: "Total",
      locationId: undefined,
      categoryIds: [],
      tagIds: [],
      productVariantIds: [],
      randomSampleCount: 0,
      excludeAuditedInLastDays: 0,
    });
    onClose();
  };

  const handleSubmit = async () => {
    let submissionData = { ...formData };
    if (formData.scopeType === "Partial") {
      if (partialType === "Manual") {
        submissionData = {
            ...submissionData,
            scopeType: "Manual",
            // Clear other filters if manual is selected
            categoryIds: [],
            tagIds: [],
            randomSampleCount: 0,
        };
      } else {
        submissionData = {
            ...submissionData,
            productVariantIds: [],
        }
      }
    }

    await onSubmit(submissionData);
    handleClose();
  };

  const toggleCategory = (categoryId: string) => {
    setFormData((prev) => ({
      ...prev,
      categoryIds: prev.categoryIds?.includes(categoryId)
        ? prev.categoryIds.filter((id) => id !== categoryId)
        : [...(prev.categoryIds || []), categoryId],
    }));
  };

  const toggleTag = (tagId: string) => {
    setFormData((prev) => ({
      ...prev,
      tagIds: prev.tagIds?.includes(tagId)
        ? prev.tagIds.filter((id) => id !== tagId)
        : [...(prev.tagIds || []), tagId],
    }));
  };

  const toggleProduct = (variantId: string) => {
    setFormData((prev) => ({
        ...prev,
        productVariantIds: prev.productVariantIds?.includes(variantId)
            ? prev.productVariantIds.filter((id) => id !== variantId)
            : [...(prev.productVariantIds || []), variantId],
    }));
  }

  const canProceed = () => {
    if (step === 1) {
      return formData.name.trim().length > 0;
    }
    if (step === 3 && formData.scopeType === 'Partial' && partialType === 'Manual') {
        return formData.productVariantIds && formData.productVariantIds.length > 0;
    }
    return true;
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <ClipboardList className="h-5 w-5" />
            Nueva Auditoría de Inventario
          </DialogTitle>
          <DialogDescription>
            {step === 1 && "Configura los detalles básicos de la auditoría"}
            {step === 2 && "Selecciona el alcance de la auditoría"}
            {step === 3 &&
              (formData.scopeType === "Partial"
                ? "Define el alcance de tu auditoría parcial"
                : "Revisa la configuración")}
          </DialogDescription>
        </DialogHeader>

        {/* Progress Steps */}
        <div className="flex items-center justify-center gap-2 py-4">
          {[1, 2, 3].map((s) => (
            <div
              key={s}
              className={`flex items-center ${s < 3 ? "flex-1" : ""}`}
            >
              <div
                className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium transition-colors ${
                  s <= step
                    ? "bg-primary text-primary-foreground"
                    : "bg-muted text-muted-foreground"
                }`}
              >
                {s}
              </div>
              {s < 3 && (
                <div
                  className={`flex-1 h-1 mx-2 rounded ${
                    s < step ? "bg-primary" : "bg-muted"
                  }`}
                />
              )}
            </div>
          ))}
        </div>

        {/* Step 1: Basic Info */}
        {step === 1 && (
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Nombre de la Auditoría *</Label>
              <Input
                id="name"
                placeholder="Ej: Auditoría Mensual Enero 2025"
                value={formData.name}
                onChange={(e) =>
                  setFormData({ ...formData, name: e.target.value })
                }
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="location">Bodega / Ubicación</Label>
              <Select
                value={formData.locationId || "all"}
                onValueChange={(value) =>
                  setFormData({
                    ...formData,
                    locationId: value === "all" ? undefined : value,
                  })
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todas las bodegas" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">
                    <div className="flex items-center gap-2">
                      <Warehouse className="h-4 w-4" />
                      Todas las bodegas
                    </div>
                  </SelectItem>
                  {locations?.map((loc) => (
                    <SelectItem key={loc.id} value={loc.id}>
                      <div className="flex items-center gap-2">
                        <Warehouse className="h-4 w-4" />
                        {loc.name}
                      </div>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="description">Descripción (opcional)</Label>
              <Textarea
                id="description"
                placeholder="Describe el propósito de esta auditoría..."
                value={formData.description || ""}
                onChange={(e) =>
                  setFormData({ ...formData, description: e.target.value })
                }
                rows={2}
              />
            </div>
          </div>
        )}

        {/* Step 2: Scope Type */}
        {step === 2 && (
          <div className="space-y-4">
            <Label className="text-base font-medium">Tipo de Auditoría</Label>
            <RadioGroup
              value={formData.scopeType}
              onValueChange={(value: AuditScopeType) =>
                setFormData({ ...formData, scopeType: value })
              }
              className="grid grid-cols-1 gap-4"
            >
              <Card
                className={`cursor-pointer transition-all ${
                  formData.scopeType === "Total"
                    ? "border-primary ring-2 ring-primary/20"
                    : "hover:border-primary/50"
                }`}
                onClick={() => setFormData({ ...formData, scopeType: "Total" })}
              >
                <CardContent className="p-4">
                  <div className="flex items-start gap-4">
                    <RadioGroupItem value="Total" id="total" className="mt-1" />
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <Package className="h-5 w-5 text-primary" />
                        <Label htmlFor="total" className="text-base font-medium cursor-pointer">
                          Auditoría Total
                        </Label>
                      </div>
                      <p className="text-sm text-muted-foreground mt-1">
                        Incluye todos los SKUs activos del inventario.
                      </p>
                    </div>
                  </div>
                </CardContent>
              </Card>

              <Card
                className={`cursor-pointer transition-all ${
                  formData.scopeType === "Partial"
                    ? "border-primary ring-2 ring-primary/20"
                    : "hover:border-primary/50"
                }`}
                onClick={() => setFormData({ ...formData, scopeType: "Partial" })}
              >
                <CardContent className="p-4">
                  <div className="flex items-start gap-4">
                    <RadioGroupItem value="Partial" id="partial" className="mt-1" />
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <Filter className="h-5 w-5 text-primary" />
                        <Label htmlFor="partial" className="text-base font-medium cursor-pointer">
                          Auditoría Parcial / Manual
                        </Label>
                      </div>
                      <p className="text-sm text-muted-foreground mt-1">
                        Filtra por criterios o selecciona manualmente los productos a auditar.
                      </p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </RadioGroup>
          </div>
        )}

        {/* Step 3: Filters / Manual Selection */}
        {step === 3 && (
          <div className="space-y-6">
            {formData.scopeType === "Total" ? (
              <div className="text-center py-8">
                <Package className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
                <p className="text-muted-foreground">
                  Has seleccionado una auditoría total.
                </p>
                <p className="text-sm text-muted-foreground">
                  Se incluirán todos los SKUs activos del inventario.
                </p>
              </div>
            ) : (
              <>
                <RadioGroup
                  value={partialType}
                  onValueChange={(value: "Filter" | "Manual") => setPartialType(value)}
                  className="grid grid-cols-2 gap-4"
                >
                  <Label
                    htmlFor="filter-mode"
                    className={`flex items-center justify-center gap-2 p-3 rounded-md border cursor-pointer transition-colors ${
                      partialType === "Filter"
                        ? "bg-primary text-primary-foreground border-primary"
                        : "hover:bg-muted/50"
                    }`}
                  >
                    <RadioGroupItem value="Filter" id="filter-mode" className="sr-only" />
                    <Filter className="h-4 w-4" />
                    Automática (Filtros)
                  </Label>
                  <Label
                    htmlFor="manual-mode"
                    className={`flex items-center justify-center gap-2 p-3 rounded-md border cursor-pointer transition-colors ${
                      partialType === "Manual"
                        ? "bg-primary text-primary-foreground border-primary"
                        : "hover:bg-muted/50"
                    }`}
                  >
                    <RadioGroupItem value="Manual" id="manual-mode" className="sr-only" />
                    <ListPlus className="h-4 w-4" />
                    Manual
                  </Label>
                </RadioGroup>

                {partialType === "Filter" ? (
                    <div className="space-y-6 pt-4 border-t">
                        {/* Category Filter */}
                        <div className="space-y-3">
                        <div className="flex items-center gap-2">
                            <FolderTree className="h-4 w-4 text-muted-foreground" />
                            <Label className="text-sm font-medium">Por Categoría</Label>
                        </div>
                        <div className="flex flex-wrap gap-2">
                            {categories?.map((cat) => (
                            <Badge
                                key={cat.id}
                                variant={
                                formData.categoryIds?.includes(cat.id)
                                    ? "default"
                                    : "outline"
                                }
                                className="cursor-pointer"
                                onClick={() => toggleCategory(cat.id)}
                            >
                                {cat.name}
                                {formData.categoryIds?.includes(cat.id) && (
                                <X className="h-3 w-3 ml-1" />
                                )}
                            </Badge>
                            ))}
                        </div>
                        </div>

                        {/* Tag Filter */}
                        <div className="space-y-3">
                        <div className="flex items-center gap-2">
                            <Tag className="h-4 w-4 text-muted-foreground" />
                            <Label className="text-sm font-medium">Por Tag / Proveedor</Label>
                        </div>
                        <div className="flex flex-wrap gap-2">
                            {tags?.map((tag) => (
                            <Badge
                                key={tag.id}
                                variant={
                                formData.tagIds?.includes(tag.id) ? "default" : "outline"
                                }
                                className="cursor-pointer"
                                onClick={() => toggleTag(tag.id)}
                            >
                                {tag.name}
                                {formData.tagIds?.includes(tag.id) && (
                                <X className="h-3 w-3 ml-1" />
                                )}
                            </Badge>
                            ))}
                        </div>
                        </div>

                        {/* Random Sample */}
                        <div className="space-y-3">
                          <div className="flex items-center gap-2">
                              <Shuffle className="h-4 w-4 text-muted-foreground" />
                              <Label className="text-sm font-medium">Muestra Aleatoria</Label>
                          </div>
                          <div className="flex items-center gap-3">
                              <Input
                              type="number" min="0" placeholder="0" className="w-24"
                              value={formData.randomSampleCount || ""}
                              onChange={(e) => setFormData({ ...formData, randomSampleCount: parseInt(e.target.value) || 0 })}
                              />
                              <span className="text-sm text-muted-foreground">
                              productos (0 = todos los que coincidan)
                              </span>
                          </div>
                        </div>

                        {/* Exclude Recently Audited */}
                        <div className="space-y-3">
                            <div className="flex items-center gap-2">
                                <Clock className="h-4 w-4 text-muted-foreground" />
                                <Label className="text-sm font-medium">Excluir Auditados Recientemente</Label>
                            </div>
                            <div className="flex items-center gap-3">
                                <Input
                                type="number" min="0" placeholder="0" className="w-24"
                                value={formData.excludeAuditedInLastDays || ""}
                                onChange={(e) => setFormData({ ...formData, excludeAuditedInLastDays: parseInt(e.target.value) || 0 })}
                                />
                                <span className="text-sm text-muted-foreground">
                                días (0 = incluir todos)
                                </span>
                            </div>
                        </div>
                    </div>
                ) : (
                    <div className="space-y-4 pt-4 border-t">
                        <div className="relative">
                            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                            <Input
                                placeholder="Buscar producto por nombre o SKU..."
                                value={productSearch}
                                onChange={(e) => setProductSearch(e.target.value)}
                                className="pl-10"
                            />
                        </div>

                        <p className="text-sm text-muted-foreground">
                            Seleccionados: {formData.productVariantIds?.length || 0}
                        </p>
                        
                        <ScrollArea className="h-64 border rounded-md">
                            <div className="p-4 space-y-2">
                            {allProducts.length > 0 ? (
                                allProducts.map((p) => (
                                    <ProductSelectItem
                                        key={p.id}
                                        item={p}
                                        isSelected={formData.productVariantIds?.includes(p.id) || false}
                                        onToggle={() => toggleProduct(p.id)}
                                    />
                                ))
                            ) : (
                                <p className="text-sm text-muted-foreground text-center py-4">
                                    {debouncedSearchTerm ? 'No se encontraron productos.' : 'Escribe para buscar...'}
                                </p>
                            )}
                            </div>
                        </ScrollArea>
                    </div>
                )}
              </>
            )}
          </div>
        )}

        <DialogFooter className="flex gap-2 sm:gap-0 pt-4">
          {step > 1 && (
            <Button
              variant="outline"
              onClick={() => setStep(step - 1)}
              disabled={isLoading}
            >
              Anterior
            </Button>
          )}
          <div className="flex-1" />
          {step < 3 ? (
            <Button onClick={() => setStep(step + 1)} disabled={!canProceed()}>
              Siguiente
            </Button>
          ) : (
            <Button onClick={handleSubmit} disabled={isLoading || !canProceed()}>
              {isLoading ? "Creando..." : "Crear Auditoría"}
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};

export default CreateAuditWizard;
