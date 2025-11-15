import { useState, useEffect } from "react";
import { useAuthStore } from "../../store/authStore";
import { useOrganizations } from "../../hooks/useOrganizations";
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
import { Building2, Palette, Mail, Image } from "lucide-react";
import { CustomizationSettings } from "../../types/organization.types";

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

  const [formData, setFormData] = useState({
    name: "",
    logoUrl: "",
    primaryContactEmail: "",
  });

  const [customization, setCustomization] = useState<CustomizationSettings>(defaultColors);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (organization) {
      setFormData({
        name: organization.name,
        logoUrl: organization.logoUrl || "",
        primaryContactEmail: organization.primaryContactEmail,
      });

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
      await updateOrganization.mutateAsync({
        id: organizationId,
        data: {
          ...formData,
          customizationSettings: JSON.stringify(customization),
        },
      });

      // Apply colors immediately
      applyCustomColors(customization);

      onClose();
    } catch (error) {
      console.error("Error updating organization:", error);
    } finally {
      setIsSaving(false);
    }
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
          <TabsList className="grid w-full grid-cols-2">
            <TabsTrigger value="general" className="gap-2">
              <Building2 className="w-4 h-4" />
              General
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
              <Label className="text-base font-semibold flex items-center gap-2">
                <Image className="w-4 h-4" />
                URL del Logo
              </Label>
              <Input
                value={formData.logoUrl}
                onChange={(e) => setFormData({ ...formData, logoUrl: e.target.value })}
                placeholder="https://ejemplo.com/logo.png"
                className="h-12"
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
