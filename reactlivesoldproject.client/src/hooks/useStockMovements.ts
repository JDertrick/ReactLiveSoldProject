import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '../services/api';
import { StockMovementDto, CreateStockMovementDto } from '../types/stockmovement.types';

// Get Stock Movements by Product Variant
export const useGetMovementsByVariant = (productVariantId: string) => {
  return useQuery({
    queryKey: ['stockMovements', 'variant', productVariantId],
    queryFn: async (): Promise<StockMovementDto[]> => {
      const response = await apiClient.get(`/stockmovement/variant/${productVariantId}`);
      return response.data;
    },
    enabled: !!productVariantId,
  });
};

// Get Stock Movements by Organization (with optional date filters)
export const useGetMovementsByOrganization = (fromDate?: string, toDate?: string) => {
  return useQuery({
    queryKey: ['stockMovements', 'organization', fromDate, toDate],
    queryFn: async (): Promise<StockMovementDto[]> => {
      const params = new URLSearchParams();
      if (fromDate) params.append('fromDate', fromDate);
      if (toDate) params.append('toDate', toDate);

      const response = await apiClient.get(`/stockmovement?${params.toString()}`);
      return response.data;
    },
  });
};

// Get Current Stock for a Product Variant
export const useGetCurrentStock = (productVariantId: string) => {
  return useQuery({
    queryKey: ['currentStock', productVariantId],
    queryFn: async (): Promise<{ stock: number }> => {
      const response = await apiClient.get(`/stockmovement/variant/${productVariantId}/current-stock`);
      return response.data;
    },
    enabled: !!productVariantId,
  });
};

// Create Stock Movement (manual adjustments, purchases, etc.)
export const useCreateStockMovement = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateStockMovementDto): Promise<StockMovementDto> => {
      const response = await apiClient.post('/stockmovement', data);
      return response.data;
    },
    onSuccess: (data) => {
      // Invalidate relevant queries
      queryClient.invalidateQueries({ queryKey: ['stockMovements'] });
      queryClient.invalidateQueries({ queryKey: ['currentStock', data.productVariantId] });
      queryClient.invalidateQueries({ queryKey: ['products'] });
      queryClient.invalidateQueries({ queryKey: ['productVariants'] });
    },
  });
};

// Post Stock Movement (applies movement to inventory and calculates weighted average cost)
export const usePostStockMovement = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (movementId: string): Promise<StockMovementDto> => {
      const response = await apiClient.post(`/stockmovement/${movementId}/post`);
      return response.data;
    },
    onSuccess: (data) => {
      // Invalidate relevant queries
      queryClient.invalidateQueries({ queryKey: ['stockMovements'] });
      queryClient.invalidateQueries({ queryKey: ['currentStock', data.productVariantId] });
      queryClient.invalidateQueries({ queryKey: ['products'] });
      queryClient.invalidateQueries({ queryKey: ['productVariants'] });
    },
  });
};

// Unpost Stock Movement (reverses a posted movement - only last posted movement)
export const useUnpostStockMovement = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (movementId: string): Promise<StockMovementDto> => {
      const response = await apiClient.post(`/stockmovement/${movementId}/unpost`);
      return response.data;
    },
    onSuccess: (data) => {
      // Invalidate relevant queries
      queryClient.invalidateQueries({ queryKey: ['stockMovements'] });
      queryClient.invalidateQueries({ queryKey: ['currentStock', data.productVariantId] });
      queryClient.invalidateQueries({ queryKey: ['products'] });
      queryClient.invalidateQueries({ queryKey: ['productVariants'] });
    },
  });
};
