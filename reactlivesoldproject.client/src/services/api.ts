import axios from 'axios';
import type {
  TaxConfigurationDto,
  UpdateTaxConfigurationDto,
  TaxRateDto,
  CreateTaxRateDto,
  UpdateTaxRateDto,
  TaxCalculationRequest,
  TaxCalculationResult,
} from '../types/tax.types';

// Crear instancia de Axios
// En desarrollo, Vite proxy redirigirá /api a http://localhost:5165/api
// En producción, se usará el mismo dominio
export const apiClient = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para agregar el token JWT a todas las peticiones
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

import { useAuthStore } from '../store/authStore';

// Interceptor para manejar errores de respuesta
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const requestUrl = error.config?.url || '';

      // NO redirigir si es un error de login (credenciales incorrectas)
      const isLoginEndpoint =
        requestUrl.includes('/auth/employee-login') ||
        requestUrl.includes('/auth/portal/login');

      if (!isLoginEndpoint) {
        // Token inválido o expirado en endpoint protegido
        useAuthStore.getState().logout();

        // Redirigir según el contexto
        const isPortalRoute = window.location.pathname.includes('/portal/');
        if (isPortalRoute) {
          // Extraer orgSlug de la URL actual
          const match = window.location.pathname.match(/\/portal\/([^\/]+)/);
          const orgSlug = match ? match[1] : 'login';
          window.location.href = `/portal/${orgSlug}/login`;
        } else {
          window.location.href = '/login';
        }
      }
    }
    return Promise.reject(error);
  }
);

// ==================== TAX API ENDPOINTS ====================

export const taxApi = {
  // Configuración
  getTaxConfiguration: () =>
    apiClient.get<TaxConfigurationDto>('/tax/configuration'),

  updateTaxConfiguration: (data: UpdateTaxConfigurationDto) =>
    apiClient.put('/tax/configuration', data),

  // Tasas
  getTaxRates: () =>
    apiClient.get<TaxRateDto[]>('/tax/rates'),

  getTaxRateById: (id: string) =>
    apiClient.get<TaxRateDto>(`/tax/rates/${id}`),

  getDefaultTaxRate: () =>
    apiClient.get<TaxRateDto>('/tax/rates/default'),

  createTaxRate: (data: CreateTaxRateDto) =>
    apiClient.post<TaxRateDto>('/tax/rates', data),

  updateTaxRate: (id: string, data: UpdateTaxRateDto) =>
    apiClient.put<TaxRateDto>(`/tax/rates/${id}`, data),

  deleteTaxRate: (id: string) =>
    apiClient.delete(`/tax/rates/${id}`),

  // Cálculos
  calculateTax: (data: TaxCalculationRequest) =>
    apiClient.post<TaxCalculationResult>('/tax/calculate', data),
};

export default apiClient;
