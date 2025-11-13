import { useMutation, useQuery } from '@tanstack/react-query';
import apiClient from '../services/api';
import { useAuthStore } from '../store/authStore';
import {
  LoginRequest,
  CustomerPortalLoginRequest,
  LoginResponse,
  UserProfile,
  CustomerProfile,
} from '../types';

// Employee Login
export const useEmployeeLogin = () => {
  const login = useAuthStore((state) => state.login);

  return useMutation({
    mutationFn: async (data: LoginRequest): Promise<LoginResponse> => {
      const response = await apiClient.post('/auth/employee-login', data);
      return response.data;
    },
    onSuccess: (data) => {
      const user = {
        id: data.user.id,
        email: data.user.email,
        firstName: data.user.firstName,
        lastName: data.user.lastName,
        role: data.user.role,
        organizationId: data.user.organizationId,
      };
      login(data.token, user);
    },
  });
};

// Customer Portal Login
export const useCustomerPortalLogin = () => {
  const login = useAuthStore((state) => state.login);

  return useMutation({
    mutationFn: async (data: CustomerPortalLoginRequest): Promise<LoginResponse> => {
      const response = await apiClient.post('/auth/portal/login', data);
      return response.data;
    },
    onSuccess: (data) => {
      const user = {
        id: data.user.id,
        email: data.user.email,
        firstName: data.user.firstName,
        lastName: data.user.lastName,
        role: data.user.role,
        organizationId: data.user.organizationId,
        organizationSlug: data.user.organizationSlug,
        organizationName: data.user.organizationName,
        organizationLogoUrl: data.user.organizationLogoUrl,
      };
      login(data.token, user);
    },
  });
};

// Get Current User Profile
export const useGetProfile = () => {
  return useQuery({
    queryKey: ['profile'],
    queryFn: async (): Promise<UserProfile | CustomerProfile> => {
      const response = await apiClient.get('/auth/me');
      return response.data;
    },
    enabled: useAuthStore.getState().isAuthenticated,
  });
};
