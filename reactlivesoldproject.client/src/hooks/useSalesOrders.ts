import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '../services/api';
import { SalesOrder, CreateSalesOrderDto, CreateSalesOrderItemDto, UpdateSalesOrderItemDto } from '../types/salesorder.types';

// Get All Sales Orders for Organization
export const useGetSalesOrders = (status?: string) => {
  return useQuery({
    queryKey: ['salesOrders', status],
    queryFn: async (): Promise<SalesOrder[]> => {
      const params = status ? `?status=${status}` : '';
      const response = await apiClient.get(`/salesorder${params}`);
      return response.data;
    },
  });
};

// Get Sales Order by ID
export const useGetSalesOrder = (id: string) => {
  return useQuery({
    queryKey: ['salesOrder', id],
    queryFn: async (): Promise<SalesOrder> => {
      const response = await apiClient.get(`/salesorder/${id}`);
      return response.data;
    },
    enabled: !!id,
  });
};

// Get Sales Orders by Customer
export const useGetCustomerOrders = (customerId: string) => {
  return useQuery({
    queryKey: ['salesOrders', 'customer', customerId],
    queryFn: async (): Promise<SalesOrder[]> => {
      const response = await apiClient.get(`/salesorder/customer/${customerId}`);
      return response.data;
    },
    enabled: !!customerId,
  });
};

// Create Sales Order
export const useCreateSalesOrder = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateSalesOrderDto): Promise<SalesOrder> => {
      const response = await apiClient.post('/salesorder', data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['salesOrders'] });
    },
  });
};

// Update Sales Order Status
export const useUpdateSalesOrderStatus = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, status }: { id: string; status: string }): Promise<SalesOrder> => {
      const response = await apiClient.patch(`/salesorder/${id}/status`, { status });
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['salesOrders'] });
      queryClient.invalidateQueries({ queryKey: ['salesOrder', variables.id] });
    },
  });
};

// Add Item to Order
export const useAddItemToOrder = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ orderId, item }: { orderId: string; item: CreateSalesOrderItemDto }): Promise<SalesOrder> => {
      const response = await apiClient.post(`/salesorder/${orderId}/items`, item);
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['salesOrders'] });
      queryClient.invalidateQueries({ queryKey: ['salesOrder', variables.orderId] });
    },
  });
};

// Update Item in Order
export const useUpdateItemInOrder = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ orderId, itemId, data }: { orderId: string; itemId: string; data: UpdateSalesOrderItemDto }): Promise<SalesOrder> => {
      const response = await apiClient.put(`/salesorder/${orderId}/items/${itemId}`, data);
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['salesOrders'] });
      queryClient.invalidateQueries({ queryKey: ['salesOrder', variables.orderId] });
    },
  });
};

// Remove Item from Order
export const useRemoveItemFromOrder = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ orderId, itemId }: { orderId: string; itemId: string }): Promise<SalesOrder> => {
      const response = await apiClient.delete(`/salesorder/${orderId}/items/${itemId}`);
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['salesOrders'] });
      queryClient.invalidateQueries({ queryKey: ['salesOrder', variables.orderId] });
    },
  });
};

// Finalize Order
export const useFinalizeOrder = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (orderId: string): Promise<SalesOrder> => {
      const response = await apiClient.post(`/salesorder/${orderId}/finalize`);
      return response.data;
    },
    onSuccess: (_, orderId) => {
      queryClient.invalidateQueries({ queryKey: ['salesOrders'] });
      queryClient.invalidateQueries({ queryKey: ['salesOrder', orderId] });
      queryClient.invalidateQueries({ queryKey: ['customers'] });
    },
  });
};

// Cancel Sales Order
export const useCancelSalesOrder = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string): Promise<void> => {
      await apiClient.post(`/salesorder/${id}/cancel`);
    },
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['salesOrders'] });
      queryClient.invalidateQueries({ queryKey: ['salesOrder', id] });
    },
  });
};
