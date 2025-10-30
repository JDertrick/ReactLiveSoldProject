import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '../services/api';
import { Organization, OrganizationPublic, CreateOrganizationDto } from '../types';

// Get Organization by Slug (Public)
export const useGetOrganizationBySlug = (slug: string) => {
  return useQuery({
    queryKey: ['organization', slug],
    queryFn: async (): Promise<OrganizationPublic> => {
      const response = await apiClient.get(`/public/organization-by-slug/${slug}`);
      return response.data;
    },
    enabled: !!slug,
  });
};

// Get All Organizations (SuperAdmin only)
export const useGetOrganizations = () => {
  return useQuery({
    queryKey: ['organizations'],
    queryFn: async (): Promise<Organization[]> => {
      const response = await apiClient.get('/superadmin/organizations');
      return response.data;
    },
  });
};

// Get Organization by ID (SuperAdmin only)
export const useGetOrganization = (id: string) => {
  return useQuery({
    queryKey: ['organization', id],
    queryFn: async (): Promise<Organization> => {
      const response = await apiClient.get(`/superadmin/organizations/${id}`);
      return response.data;
    },
    enabled: !!id,
  });
};

// Create Organization (SuperAdmin only)
export const useCreateOrganization = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateOrganizationDto): Promise<Organization> => {
      const response = await apiClient.post('/superadmin/organizations', data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['organizations'] });
    },
  });
};

// Update Organization (SuperAdmin only)
export const useUpdateOrganization = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: CreateOrganizationDto }): Promise<Organization> => {
      const response = await apiClient.put(`/superadmin/organizations/${id}`, data);
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['organizations'] });
      queryClient.invalidateQueries({ queryKey: ['organization', variables.id] });
    },
  });
};

// Delete Organization (SuperAdmin only)
export const useDeleteOrganization = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string): Promise<void> => {
      await apiClient.delete(`/superadmin/organizations/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['organizations'] });
    },
  });
};
