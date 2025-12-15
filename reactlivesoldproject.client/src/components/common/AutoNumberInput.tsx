import { Input } from "../ui/input";
import { Label } from "../ui/label";
import { Info } from "lucide-react";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "../ui/tooltip";

interface AutoNumberInputProps {
  label: string;
  value?: string;
  onChange?: (value: string) => void;
  allowManualEntry?: boolean;
  isEditing?: boolean;
  placeholder?: string;
  helpText?: string;
}

/**
 * AutoNumberInput - Componente para mostrar/ingresar números automáticos
 *
 * @param label - Etiqueta del campo
 * @param value - Valor actual del número
 * @param onChange - Función para manejar cambios (solo si allowManualEntry es true)
 * @param allowManualEntry - Si se permite ingreso manual (depende de la configuración de la serie)
 * @param isEditing - Si se está editando un registro existente
 * @param placeholder - Texto de placeholder
 * @param helpText - Texto de ayuda adicional
 */
export const AutoNumberInput = ({
  label,
  value,
  onChange,
  allowManualEntry = false,
  isEditing = false,
  placeholder = "Se generará automáticamente",
  helpText,
}: AutoNumberInputProps) => {
  // Al editar, siempre mostrar el número en modo solo lectura
  if (isEditing) {
    return (
      <div className="space-y-2">
        <Label>{label}</Label>
        <div className="flex items-center gap-2">
          <Input
            value={value || ""}
            disabled
            className="bg-muted"
          />
          <TooltipProvider>
            <Tooltip>
              <TooltipTrigger>
                <Info className="h-4 w-4 text-muted-foreground" />
              </TooltipTrigger>
              <TooltipContent>
                <p>El número no se puede modificar después de la creación</p>
              </TooltipContent>
            </Tooltip>
          </TooltipProvider>
        </div>
      </div>
    );
  }

  // Al crear nuevo: permitir ingreso manual si está habilitado
  if (allowManualEntry && onChange) {
    return (
      <div className="space-y-2">
        <Label>{label}</Label>
        <div className="space-y-1">
          <Input
            value={value || ""}
            onChange={(e) => onChange(e.target.value)}
            placeholder={placeholder}
          />
          {helpText && (
            <p className="text-xs text-muted-foreground">{helpText}</p>
          )}
          <p className="text-xs text-muted-foreground">
            Dejar en blanco para generar automáticamente
          </p>
        </div>
      </div>
    );
  }

  // Por defecto: mostrar en modo solo lectura (se generará automáticamente)
  return (
    <div className="space-y-2">
      <Label>{label}</Label>
      <div className="flex items-center gap-2">
        <Input
          value={value || ""}
          disabled
          placeholder={placeholder}
          className="bg-muted"
        />
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger>
              <Info className="h-4 w-4 text-muted-foreground" />
            </TooltipTrigger>
            <TooltipContent>
              <p>Este número se generará automáticamente al guardar</p>
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>
      </div>
    </div>
  );
};
