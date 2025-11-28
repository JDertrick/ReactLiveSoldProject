import { useState, useCallback } from 'react';
import api from '../services/api';
import {
  OrganizationAccountConfigurationDto,
  UpdateOrganizationAccountConfigurationDto,
} from '../types/accounting.types';

export const useAccountConfiguration = () => {
  const [configuration, setConfiguration] = useState<OrganizationAccountConfigurationDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchConfiguration = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.get<OrganizationAccountConfigurationDto>(
        '/OrganizationAccountConfiguration'
      );
      setConfiguration(response.data);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar la configuración';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const updateConfiguration = useCallback(async (data: UpdateOrganizationAccountConfigurationDto) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.put<OrganizationAccountConfigurationDto>(
        '/OrganizationAccountConfiguration',
        data
      );
      setConfiguration(response.data);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al guardar la configuración';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    configuration,
    loading,
    error,
    fetchConfiguration,
    updateConfiguration,
  };
};
