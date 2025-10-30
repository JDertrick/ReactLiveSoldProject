import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '../services/api';
import { Customer, CreateCustomerDto, UpdateCustomerDto } from '../types';

// Get All Customers
export const useGetCustomers = () => {
  return useQuery({
    queryKey: ['customers'],
    queryFn: async (): Promise<Customer[]> => {
      const response = await apiClient.get('/customer');
      return response.data;
    },
  });
};

// Get Customer by ID
export const useGetCustomer = (id: string) => {
  return useQuery({
    queryKey: ['customer', id],
    queryFn: async (): Promise<Customer> => {
      const response = await apiClient.get(`/customer/${id}`);
      return response.data;
    },
    enabled: !!id,
  });
};

// Search Customers
export const useSearchCustomers = (searchTerm: string) => {
  return useQuery({
    queryKey: ['customers', 'search', searchTerm],
    queryFn: async (): Promise<Customer[]> => {
      const response = await apiClient.get(`/customer/search?q=${searchTerm}`);
      return response.data;
    },
    enabled: searchTerm.length > 0,
  });
};

// Create Customer
export const useCreateCustomer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateCustomerDto): Promise<Customer> => {
      const response = await apiClient.post('/customer', data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['customers'] });
    },
  });
};

// Update Customer
export const useUpdateCustomer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: UpdateCustomerDto }): Promise<Customer> => {
      const response = await apiClient.put(`/customer/${id}`, data);
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['customers'] });
      queryClient.invalidateQueries({ queryKey: ['customer', variables.id] });
    },
  });
};

// Delete Customer
export const useDeleteCustomer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string): Promise<void> => {
      await apiClient.delete(`/customer/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['customers'] });
    },
  });
};
