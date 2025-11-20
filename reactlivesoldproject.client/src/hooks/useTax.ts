import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { taxApi } from '../services/api';
import type {
  TaxConfigurationDto,
  UpdateTaxConfigurationDto,
  TaxRateDto,
  CreateTaxRateDto,
  UpdateTaxRateDto,
  TaxCalculationRequest,
  TaxCalculationResult,
} from '../types/tax.types';

// ==================== CONFIGURACIÓN ====================

// Get Tax Configuration
export const useGetTaxConfiguration = () => {
  return useQuery({
    queryKey: ['taxConfiguration'],
    queryFn: async (): Promise<TaxConfigurationDto> => {
      const response = await taxApi.getTaxConfiguration();
      return response.data;
    },
  });
};

// Update Tax Configuration
export const useUpdateTaxConfiguration = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: UpdateTaxConfigurationDto) => {
      const response = await taxApi.updateTaxConfiguration(data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['taxConfiguration'] });
      queryClient.invalidateQueries({ queryKey: ['taxRates'] });
    },
  });
};

// ==================== TASAS ====================

// Get All Tax Rates
export const useGetTaxRates = () => {
  return useQuery({
    queryKey: ['taxRates'],
    queryFn: async (): Promise<TaxRateDto[]> => {
      const response = await taxApi.getTaxRates();
      return response.data;
    },
  });
};

// Get Tax Rate by ID
export const useGetTaxRateById = (id: string | null) => {
  return useQuery({
    queryKey: ['taxRate', id],
    queryFn: async (): Promise<TaxRateDto> => {
      if (!id) throw new Error('Tax Rate ID is required');
      const response = await taxApi.getTaxRateById(id);
      return response.data;
    },
    enabled: !!id,
  });
};

// Get Default Tax Rate
export const useGetDefaultTaxRate = () => {
  return useQuery({
    queryKey: ['taxRate', 'default'],
    queryFn: async (): Promise<TaxRateDto> => {
      const response = await taxApi.getDefaultTaxRate();
      return response.data;
    },
  });
};

// Create Tax Rate
export const useCreateTaxRate = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateTaxRateDto) => {
      const response = await taxApi.createTaxRate(data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['taxRates'] });
      queryClient.invalidateQueries({ queryKey: ['taxConfiguration'] });
    },
  });
};

// Update Tax Rate
export const useUpdateTaxRate = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: UpdateTaxRateDto }) => {
      const response = await taxApi.updateTaxRate(id, data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['taxRates'] });
      queryClient.invalidateQueries({ queryKey: ['taxRate'] });
      queryClient.invalidateQueries({ queryKey: ['taxConfiguration'] });
    },
  });
};

// Delete Tax Rate
export const useDeleteTaxRate = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      const response = await taxApi.deleteTaxRate(id);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['taxRates'] });
      queryClient.invalidateQueries({ queryKey: ['taxConfiguration'] });
    },
  });
};

// ==================== CÁLCULOS ====================

// Calculate Tax
export const useCalculateTax = () => {
  return useMutation({
    mutationFn: async (data: TaxCalculationRequest): Promise<TaxCalculationResult> => {
      const response = await taxApi.calculateTax(data);
      return response.data;
    },
  });
};
