// Enums
export enum TaxSystemType {
  None = 0,
  VAT = 1,
  SalesTax = 2,
  GST = 3,
  IGV = 4,
}

export enum TaxApplicationMode {
  TaxIncluded = 0,
  TaxExcluded = 1,
}

// DTOs
export interface TaxRateDto {
  id: string;
  organizationId: string;
  name: string;
  rate: number;
  isDefault: boolean;
  isActive: boolean;
  description?: string;
  effectiveFrom?: string;
  effectiveTo?: string;
  createdAt: string;
  updatedAt: string;
}

export interface TaxConfigurationDto {
  taxEnabled: boolean;
  taxSystemType: TaxSystemType;
  taxDisplayName?: string;
  taxApplicationMode: TaxApplicationMode;
  defaultTaxRateId?: string;
  taxRates: TaxRateDto[];
}

export interface CreateTaxRateDto {
  name: string;
  rate: number;
  isDefault?: boolean;
  isActive?: boolean;
  description?: string;
  effectiveFrom?: string;
  effectiveTo?: string;
}

export interface UpdateTaxRateDto {
  id: string;
  name: string;
  rate: number;
  isDefault?: boolean;
  isActive?: boolean;
  description?: string;
  effectiveFrom?: string;
  effectiveTo?: string;
}

export interface UpdateTaxConfigurationDto {
  taxEnabled: boolean;
  taxSystemType: TaxSystemType;
  taxDisplayName?: string;
  taxApplicationMode: TaxApplicationMode;
  defaultTaxRateId?: string;
}

export interface TaxCalculationRequest {
  amount: number;
  taxRateId?: string;
  priceIncludesTax?: boolean;
}

export interface TaxCalculationResult {
  baseAmount: number;
  taxAmount: number;
  totalAmount: number;
  taxRate: number;
  taxRateName?: string;
}

// Helper function para obtener el nombre del tipo de sistema de impuestos
export const getTaxSystemTypeName = (type: TaxSystemType): string => {
  switch (type) {
    case TaxSystemType.None:
      return 'Sin impuestos';
    case TaxSystemType.VAT:
      return 'IVA (Impuesto al Valor Agregado)';
    case TaxSystemType.SalesTax:
      return 'Sales Tax';
    case TaxSystemType.GST:
      return 'GST (Goods and Services Tax)';
    case TaxSystemType.IGV:
      return 'IGV (Impuesto General a las Ventas)';
    default:
      return 'Desconocido';
  }
};

// Helper function para obtener el nombre del modo de aplicación
export const getTaxApplicationModeName = (mode: TaxApplicationMode): string => {
  switch (mode) {
    case TaxApplicationMode.TaxIncluded:
      return 'Precio incluye impuestos';
    case TaxApplicationMode.TaxExcluded:
      return 'Impuesto se agrega al precio';
    default:
      return 'Desconocido';
  }
};

// Helper function para convertir string del backend a enum TaxSystemType
export const parseTaxSystemType = (value: any): TaxSystemType => {
  if (typeof value === 'number') return value as TaxSystemType;

  const stringValue = String(value);
  switch (stringValue) {
    case 'None':
    case '0':
      return TaxSystemType.None;
    case 'VAT':
    case '1':
      return TaxSystemType.VAT;
    case 'SalesTax':
    case '2':
      return TaxSystemType.SalesTax;
    case 'GST':
    case '3':
      return TaxSystemType.GST;
    case 'IGV':
    case '4':
      return TaxSystemType.IGV;
    default:
      return TaxSystemType.None;
  }
};

// Helper function para convertir string del backend a enum TaxApplicationMode
export const parseTaxApplicationMode = (value: any): TaxApplicationMode => {
  if (typeof value === 'number') return value as TaxApplicationMode;

  const stringValue = String(value);
  switch (stringValue) {
    case 'TaxIncluded':
    case '0':
      return TaxApplicationMode.TaxIncluded;
    case 'TaxExcluded':
    case '1':
      return TaxApplicationMode.TaxExcluded;
    default:
      return TaxApplicationMode.TaxIncluded;
  }
};
