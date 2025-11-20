import React from 'react';
import { useGetTaxRates } from '../../hooks/useTax';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../ui/select';
import { Label } from '../ui/label';

interface TaxRateSelectorProps {
  value?: string;
  onValueChange: (value: string) => void;
  label?: string;
  placeholder?: string;
  disabled?: boolean;
  required?: boolean;
}

const TaxRateSelector: React.FC<TaxRateSelectorProps> = ({
  value,
  onValueChange,
  label = 'Tasa de Impuesto',
  placeholder = 'Seleccione una tasa',
  disabled = false,
  required = false,
}) => {
  const { data: taxRates, isLoading } = useGetTaxRates();

  const activeTaxRates = taxRates?.filter(rate => rate.isActive) || [];

  return (
    <div className="space-y-2">
      {label && (
        <Label className="text-xs font-semibold text-gray-500 tracking-wider">
          {label} {required && <span className="text-red-500">*</span>}
        </Label>
      )}
      <Select
        value={value || ''}
        onValueChange={onValueChange}
        disabled={disabled || isLoading}
      >
        <SelectTrigger className="w-full">
          <SelectValue placeholder={isLoading ? 'Cargando...' : placeholder} />
        </SelectTrigger>
        <SelectContent>
          {activeTaxRates.length === 0 ? (
            <SelectItem value="none" disabled>
              No hay tasas de impuesto activas
            </SelectItem>
          ) : (
            activeTaxRates.map((rate) => (
              <SelectItem key={rate.id} value={rate.id}>
                {rate.name} ({(rate.rate * 100).toFixed(2)}%)
                {rate.isDefault && ' (Predeterminada)'}
              </SelectItem>
            ))
          )}
        </SelectContent>
      </Select>
    </div>
  );
};

export default TaxRateSelector;
