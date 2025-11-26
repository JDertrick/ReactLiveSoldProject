import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '../services/api';
import { Vendor, CreateVendorDto, UpdateVendorDto } from '../types';

// Get All Vendors
export const useGetVendors = (searchTerm?: string, status?: string) => {
  return useQuery({
    queryKey: ['vendors', searchTerm, status],
    queryFn: async (): Promise<Vendor[]> => {
      const response = await apiClient.get('/vendor', {
        params: { searchTerm, status },
      });
      return response.data;
    },
  });
};

// Get Vendor by ID
export const useGetVendor = (id: string) => {
  return useQuery({
    queryKey: ['vendor', id],
    queryFn: async (): Promise<Vendor> => {
      const response = await apiClient.get(`/vendor/${id}`);
      return response.data;
    },
    enabled: !!id,
  });
};

// Search Vendors
export const useSearchVendors = (searchTerm: string) => {
  return useQuery({
    queryKey: ['vendors', 'search', searchTerm],
    queryFn: async (): Promise<Vendor[]> => {
      const response = await apiClient.get(`/vendor/search?q=${searchTerm}`);
      return response.data;
    },
    enabled: searchTerm.length > 0,
  });
};

// Create Vendor
export const useCreateVendor = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateVendorDto): Promise<Vendor> => {
      const response = await apiClient.post('/vendor', data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vendors'] });
    },
  });
};

// Update Vendor
export const useUpdateVendor = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: UpdateVendorDto }): Promise<Vendor> => {
      const response = await apiClient.put(`/vendor/${id}`, data);
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['vendors'] });
      queryClient.invalidateQueries({ queryKey: ['vendor', variables.id] });
    },
  });
};

// Delete Vendor
export const useDeleteVendor = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string): Promise<void> => {
      await apiClient.delete(`/vendor/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vendors'] });
    },
  });
};
