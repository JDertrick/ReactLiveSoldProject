import { useState, useEffect } from "react";
import { useAuthStore } from "../../store/authStore";
import { useOrganizations, useUploadOrganizationLogo } from "../../hooks/useOrganizations";
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
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Building2, Palette, Mail, DollarSign } from "lucide-react";
import { CustomizationSettings, CostMethod } from "../../types/organization.types";
import { ImageUpload } from "@/components/ui/image-upload";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";

interface OrganizationSettingsModalProps {
  open: boolean;
  onClose: () => void;
}

const defaultColors = {
  primaryColor: "#4F46E5",
  secondaryColor: "#818CF8",
  accentColor: "#EC4899",
  sidebarBg: "#111827",
  sidebarText: "#D1D5DB",
  sidebarActiveBg: "#1F2937",
  sidebarActiveText: "#FFFFFF",
};

export const OrganizationSettingsModal = ({
  open,
  onClose,
}: OrganizationSettingsModalProps) => {
  const { organizationId } = useAuthStore();
  const { data: organization, isLoading } = useOrganizations(organizationId || "");
  const { updateOrganization } = useOrganizations();
  const uploadLogo = useUploadOrganizationLogo();

  const [selectedLogo, setSelectedLogo] = useState<File | null>(null);
  const [formData, setFormData] = useState({
    name: "",
    logoUrl: "",
    primaryContactEmail: "",
  });

  const [customization, setCustomization] = useState<CustomizationSettings>(defaultColors);
  const [costMethod, setCostMethod] = useState<CostMethod>(CostMethod.FIFO);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (organization) {
      setFormData({
        name: organization.name,
        logoUrl: organization.logoUrl || "",
        primaryContactEmail: organization.primaryContactEmail,
      });

      // Set cost method
      setCostMethod(organization.costMethod === 'AverageCost' ? CostMethod.AverageCost : CostMethod.FIFO);

      // Parse customization settings if exists
      if (organization.customizationSettings) {
        try {
          const parsed = JSON.parse(organization.customizationSettings);
          setCustomization({ ...defaultColors, ...parsed });
        } catch (e) {
          setCustomization(defaultColors);
        }
      }
    }
  }, [organization]);

  const handleSave = async () => {
    if (!organizationId) return;

    setIsSaving(true);
    try {
      let updatedLogoUrl = formData.logoUrl;

      // Si hay un logo seleccionado, subirlo primero
      if (selectedLogo) {
        const result = await uploadLogo.mutateAsync(selectedLogo);
        updatedLogoUrl = result.logoUrl || "";

        // Actualizar el formData con la nueva URL
        setFormData(prev => ({
          ...prev,
          logoUrl: updatedLogoUrl
        }));
      }

      // Prepare data - convert empty strings to undefined for optional fields
      const dataToSend = {
        name: formData.name,
        logoUrl: updatedLogoUrl || undefined,
        primaryContactEmail: formData.primaryContactEmail,
        customizationSettings: JSON.stringify(customization),
        costMethod: costMethod,
      };

      await updateOrganization.mutateAsync(dataToSend);

      // Apply colors immediately
      applyCustomColors(customization);

      // Limpiar la imagen seleccionada
      setSelectedLogo(null);

      onClose();
    } catch (error) {
      console.error("Error updating organization:", error);
      alert("Error al guardar la configuración. Por favor verifica los datos.");
    } finally {
      setIsSaving(false);
    }
  };

  const handleLogoSelect = (file: File) => {
    setSelectedLogo(file);
  };

  const applyCustomColors = (colors: CustomizationSettings) => {
    const root = document.documentElement;
    if (colors.primaryColor) root.style.setProperty("--primary-color", colors.primaryColor);
    if (colors.sidebarBg) root.style.setProperty("--sidebar-bg", colors.sidebarBg);
    if (colors.sidebarText) root.style.setProperty("--sidebar-text", colors.sidebarText);
    if (colors.sidebarActiveBg) root.style.setProperty("--sidebar-active-bg", colors.sidebarActiveBg);
    if (colors.sidebarActiveText) root.style.setProperty("--sidebar-active-text", colors.sidebarActiveText);
  };

  const resetToDefaults = () => {
    setCustomization(defaultColors);
  };

  if (isLoading) {
    return null;
  }

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-2xl font-bold flex items-center gap-2">
            <Building2 className="w-6 h-6" />
            Configuración de Organización
          </DialogTitle>
          <DialogDescription>
            Personaliza la información y apariencia de tu organización
          </DialogDescription>
        </DialogHeader>

        <Tabs defaultValue="general" className="w-full">
          <TabsList className="grid w-full grid-cols-3">
            <TabsTrigger value="general" className="gap-2">
              <Building2 className="w-4 h-4" />
              General
            </TabsTrigger>
            <TabsTrigger value="costs" className="gap-2">
              <DollarSign className="w-4 h-4" />
              Costos
            </TabsTrigger>
            <TabsTrigger value="appearance" className="gap-2">
              <Palette className="w-4 h-4" />
              Apariencia
            </TabsTrigger>
          </TabsList>

          <TabsContent value="general" className="space-y-4 mt-4">
            <div className="space-y-2">
              <Label className="text-base font-semibold flex items-center gap-2">
                <Building2 className="w-4 h-4" />
                Nombre de la Organización
              </Label>
              <Input
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="Mi Empresa S.A."
                className="h-12"
              />
            </div>

            <div className="space-y-2">
              <Label className="text-base font-semibold">
                Slug (No editable)
              </Label>
              <Input
                value={organization?.slug || ""}
                disabled
                className="h-12 bg-gray-100 cursor-not-allowed"
              />
              <p className="text-xs text-gray-500">
                El slug es único y no puede modificarse
              </p>
            </div>

            <div className="space-y-2">
              <ImageUpload
                label="Logo de la Organización"
                currentImageUrl={formData.logoUrl}
                onImageSelect={handleLogoSelect}
                maxSizeMB={5}
              />
            </div>

            <div className="space-y-2">
              <Label className="text-base font-semibold flex items-center gap-2">
                <Mail className="w-4 h-4" />
                Email de Contacto
              </Label>
              <Input
                type="email"
                value={formData.primaryContactEmail}
                onChange={(e) =>
                  setFormData({ ...formData, primaryContactEmail: e.target.value })
                }
                placeholder="contacto@empresa.com"
                className="h-12"
              />
            </div>
          </TabsContent>

          <TabsContent value="costs" className="space-y-4 mt-4">
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-4">
              <p className="text-sm text-blue-800 font-semibold mb-2">
                📊 Configuración de Método de Costeo
              </p>
              <p className="text-sm text-blue-700">
                Selecciona el método que deseas usar para calcular el costo de tus productos.
                <strong className="block mt-2">
                  Importante: Ambos métodos se calculan SIEMPRE en segundo plano,
                </strong>
                pero solo el seleccionado se usará en reportes y cálculos de rentabilidad.
              </p>
            </div>

            <div className="space-y-4">
              <Label className="text-base font-semibold">
                Método de Costeo de Inventario
              </Label>

              <RadioGroup value={costMethod} onValueChange={(value) => setCostMethod(value as CostMethod)}>
                <div className="flex items-start space-x-3 p-4 border rounded-lg hover:bg-gray-50 transition-colors">
                  <RadioGroupItem value={CostMethod.FIFO} id="fifo" className="mt-1" />
                  <Label htmlFor="fifo" className="flex-1 cursor-pointer">
                    <div className="font-semibold text-base mb-1">FIFO (First In, First Out)</div>
                    <p className="text-sm text-gray-600">
                      Las primeras unidades que entran al inventario son las primeras en salir.
                      Útil cuando los productos tienen fecha de vencimiento o se prefiere
                      mantener el inventario rotando.
                    </p>
                    <div className="mt-2 text-xs text-gray-500">
                      ✓ Refleja mejor el flujo físico del inventario<br/>
                      ✓ Reduce riesgo de obsolescencia<br/>
                      ✓ Recomendado para productos perecederos
                    </div>
                  </Label>
                </div>

                <div className="flex items-start space-x-3 p-4 border rounded-lg hover:bg-gray-50 transition-colors">
                  <RadioGroupItem value={CostMethod.AverageCost} id="average" className="mt-1" />
                  <Label htmlFor="average" className="flex-1 cursor-pointer">
                    <div className="font-semibold text-base mb-1">Costo Promedio Ponderado</div>
                    <p className="text-sm text-gray-600">
                      Calcula el costo promedio de todas las unidades disponibles.
                      Suaviza las fluctuaciones de precio y simplifica la contabilidad.
                    </p>
                    <div className="mt-2 text-xs text-gray-500">
                      ✓ Suaviza variaciones de precio<br/>
                      ✓ Más simple de calcular<br/>
                      ✓ Recomendado para productos homogéneos
                    </div>
                  </Label>
                </div>
              </RadioGroup>

              <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                <p className="text-sm text-yellow-800">
                  <strong>💡 Nota:</strong> Puedes cambiar el método en cualquier momento.
                  Los cálculos históricos no se modifican, pero los nuevos reportes usarán el método seleccionado.
                  Los lotes FIFO siempre se mantienen para garantizar trazabilidad.
                </p>
              </div>
            </div>
          </TabsContent>

          <TabsContent value="appearance" className="space-y-4 mt-4">
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-4">
              <p className="text-sm text-blue-700">
                💡 Los colores se aplicarán inmediatamente al menú lateral y componentes de la interfaz.
              </p>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label className="text-sm font-semibold">Color Principal</Label>
                <div className="flex gap-2">
                  <Input
                    type="color"
                    value={customization.primaryColor || defaultColors.primaryColor}
                    onChange={(e) =>
                      setCustomization({ ...customization, primaryColor: e.target.value })
                    }
                    className="h-12 w-20"
                  />
                  <Input
                    type="text"
                    value={customization.primaryColor || defaultColors.primaryColor}
                    onChange={(e) =>
                      setCustomization({ ...customization, primaryColor: e.target.value })
                    }
                    className="h-12 flex-1"
                    placeholder="#4F46E5"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label className="text-sm font-semibold">Color Secundario</Label>
                <div className="flex gap-2">
                  <Input
                    type="color"
                    value={customization.secondaryColor || defaultColors.secondaryColor}
                    onChange={(e) =>
                      setCustomization({ ...customization, secondaryColor: e.target.value })
                    }
                    className="h-12 w-20"
                  />
                  <Input
                    type="text"
                    value={customization.secondaryColor || defaultColors.secondaryColor}
                    onChange={(e) =>
                      setCustomization({ ...customization, secondaryColor: e.target.value })
                    }
                    className="h-12 flex-1"
                    placeholder="#818CF8"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label className="text-sm font-semibold">Fondo del Menú</Label>
                <div className="flex gap-2">
                  <Input
                    type="color"
                    value={customization.sidebarBg || defaultColors.sidebarBg}
                    onChange={(e) =>
                      setCustomization({ ...customization, sidebarBg: e.target.value })
                    }
                    className="h-12 w-20"
                  />
                  <Input
                    type="text"
                    value={customization.sidebarBg || defaultColors.sidebarBg}
                    onChange={(e) =>
                      setCustomization({ ...customization, sidebarBg: e.target.value })
                    }
                    className="h-12 flex-1"
                    placeholder="#111827"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label className="text-sm font-semibold">Texto del Menú</Label>
                <div className="flex gap-2">
                  <Input
                    type="color"
                    value={customization.sidebarText || defaultColors.sidebarText}
                    onChange={(e) =>
                      setCustomization({ ...customization, sidebarText: e.target.value })
                    }
                    className="h-12 w-20"
                  />
                  <Input
                    type="text"
                    value={customization.sidebarText || defaultColors.sidebarText}
                    onChange={(e) =>
                      setCustomization({ ...customization, sidebarText: e.target.value })
                    }
                    className="h-12 flex-1"
                    placeholder="#D1D5DB"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label className="text-sm font-semibold">Fondo Activo</Label>
                <div className="flex gap-2">
                  <Input
                    type="color"
                    value={customization.sidebarActiveBg || defaultColors.sidebarActiveBg}
                    onChange={(e) =>
                      setCustomization({ ...customization, sidebarActiveBg: e.target.value })
                    }
                    className="h-12 w-20"
                  />
                  <Input
                    type="text"
                    value={customization.sidebarActiveBg || defaultColors.sidebarActiveBg}
                    onChange={(e) =>
                      setCustomization({ ...customization, sidebarActiveBg: e.target.value })
                    }
                    className="h-12 flex-1"
                    placeholder="#1F2937"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label className="text-sm font-semibold">Texto Activo</Label>
                <div className="flex gap-2">
                  <Input
                    type="color"
                    value={customization.sidebarActiveText || defaultColors.sidebarActiveText}
                    onChange={(e) =>
                      setCustomization({ ...customization, sidebarActiveText: e.target.value })
                    }
                    className="h-12 w-20"
                  />
                  <Input
                    type="text"
                    value={customization.sidebarActiveText || defaultColors.sidebarActiveText}
                    onChange={(e) =>
                      setCustomization({ ...customization, sidebarActiveText: e.target.value })
                    }
                    className="h-12 flex-1"
                    placeholder="#FFFFFF"
                  />
                </div>
              </div>
            </div>

            <Button
              type="button"
              variant="outline"
              onClick={resetToDefaults}
              className="w-full"
            >
              Restablecer Colores por Defecto
            </Button>
          </TabsContent>
        </Tabs>

        <DialogFooter className="gap-2">
          <Button type="button" variant="outline" onClick={onClose}>
            Cancelar
          </Button>
          <Button onClick={handleSave} disabled={isSaving}>
            {isSaving ? "Guardando..." : "Guardar Cambios"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
