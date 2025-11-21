import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '../services/api';
import {
  InventoryAuditDto,
  InventoryAuditDetailDto,
  InventoryAuditItemDto,
  BlindCountItemDto,
  InventoryAuditSummaryDto,
  CreateInventoryAuditDto,
  UpdateAuditCountDto,
  BulkUpdateAuditCountDto,
  CompleteAuditDto,
} from '../types/inventoryAudit.types';

// Get all audits
export const useGetAudits = (status?: string) => {
  return useQuery({
    queryKey: ['inventoryAudits', status || ''],
    queryFn: async (): Promise<InventoryAuditDto[]> => {
      const params = status ? `?status=${status}` : '';
      const response = await apiClient.get(`/inventoryaudit${params}`);
      return response.data;
    },
  });
};

// Get audit by ID
export const useGetAuditById = (auditId: string) => {
  return useQuery({
    queryKey: ['inventoryAudit', auditId],
    queryFn: async (): Promise<InventoryAuditDetailDto> => {
      const response = await apiClient.get(`/inventoryaudit/${auditId}`);
      return response.data;
    },
    enabled: !!auditId,
  });
};

// Get blind count items (no theoretical stock)
export const useGetBlindCountItems = (auditId: string) => {
  return useQuery({
    queryKey: ['inventoryAudit', auditId, 'blindCount'],
    queryFn: async (): Promise<BlindCountItemDto[]> => {
      const response = await apiClient.get(`/inventoryaudit/${auditId}/blind-count`);
      return response.data;
    },
    enabled: !!auditId,
  });
};

// Get audit items (with theoretical stock for review)
export const useGetAuditItems = (auditId: string) => {
  return useQuery({
    queryKey: ['inventoryAudit', auditId, 'items'],
    queryFn: async (): Promise<InventoryAuditItemDto[]> => {
      const response = await apiClient.get(`/inventoryaudit/${auditId}/items`);
      return response.data;
    },
    enabled: !!auditId,
  });
};

// Get audit summary
export const useGetAuditSummary = (auditId: string) => {
  return useQuery({
    queryKey: ['inventoryAudit', auditId, 'summary'],
    queryFn: async (): Promise<InventoryAuditSummaryDto> => {
      const response = await apiClient.get(`/inventoryaudit/${auditId}/summary`);
      return response.data;
    },
    enabled: !!auditId,
  });
};

// Create audit
export const useCreateAudit = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateInventoryAuditDto): Promise<InventoryAuditDto> => {
      const response = await apiClient.post('/inventoryaudit', data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['inventoryAudits'] });
    },
  });
};

// Start audit
export const useStartAudit = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (auditId: string): Promise<InventoryAuditDto> => {
      const response = await apiClient.post(`/inventoryaudit/${auditId}/start`);
      return response.data;
    },
    onSuccess: (_, auditId) => {
      queryClient.invalidateQueries({ queryKey: ['inventoryAudit', auditId] });
      queryClient.invalidateQueries({ queryKey: ['inventoryAudits'] });
    },
  });
};

// Update single count
export const useUpdateCount = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      auditId,
      data,
    }: {
      auditId: string;
      data: UpdateAuditCountDto;
    }): Promise<InventoryAuditItemDto> => {
      const response = await apiClient.put(`/inventoryaudit/${auditId}/count`, data);
      return response.data;
    },
    onSuccess: (_, { auditId }) => {
      queryClient.invalidateQueries({ queryKey: ['inventoryAudit', auditId] });
      queryClient.invalidateQueries({ queryKey: ['inventoryAudits'] });
    },
  });
};

// Bulk update counts
export const useBulkUpdateCount = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      auditId,
      data,
    }: {
      auditId: string;
      data: BulkUpdateAuditCountDto;
    }): Promise<InventoryAuditItemDto[]> => {
      const response = await apiClient.put(`/inventoryaudit/${auditId}/bulk-count`, data);
      return response.data;
    },
    onSuccess: (_, { auditId }) => {
      queryClient.invalidateQueries({ queryKey: ['inventoryAudit', auditId] });
      queryClient.invalidateQueries({ queryKey: ['inventoryAudits'] });
    },
  });
};

// Complete audit
export const useCompleteAudit = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      auditId,
      data,
    }: {
      auditId: string;
      data: CompleteAuditDto;
    }): Promise<InventoryAuditDto> => {
      const response = await apiClient.post(`/inventoryaudit/${auditId}/complete`, data);
      return response.data;
    },
    onSuccess: (_, { auditId }) => {
      queryClient.invalidateQueries({ queryKey: ['inventoryAudit', auditId] });
      queryClient.invalidateQueries({ queryKey: ['inventoryAudits'] });
      queryClient.invalidateQueries({ queryKey: ['stockMovements'] });
    },
  });
};

// Cancel audit
export const useCancelAudit = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (auditId: string): Promise<InventoryAuditDto> => {
      const response = await apiClient.post(`/inventoryaudit/${auditId}/cancel`);
      return response.data;
    },
    onSuccess: (_, auditId) => {
      queryClient.invalidateQueries({ queryKey: ['inventoryAudit', auditId] });
      queryClient.invalidateQueries({ queryKey: ['inventoryAudits'] });
    },
  });
};
