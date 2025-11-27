import { useState, useCallback } from 'react';
import api from '../services/api';
import {
  CompanyBankAccountDto,
  CreateCompanyBankAccountDto,
  UpdateCompanyBankAccountDto
} from '../types/purchases.types';

export const useCompanyBankAccounts = () => {
  const [companyBankAccounts, setCompanyBankAccounts] = useState<CompanyBankAccountDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchCompanyBankAccounts = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.get<CompanyBankAccountDto[]>('/CompanyBankAccount');
      setCompanyBankAccounts(response.data);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar cuentas bancarias';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const getCompanyBankAccountById = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.get<CompanyBankAccountDto>(`/CompanyBankAccount/${id}`);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar cuenta bancaria';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const createCompanyBankAccount = useCallback(async (data: CreateCompanyBankAccountDto) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post<CompanyBankAccountDto>('/CompanyBankAccount', data);
      setCompanyBankAccounts((prev) => [...prev, response.data]);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al crear cuenta bancaria';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const updateCompanyBankAccount = useCallback(
    async (id: string, data: UpdateCompanyBankAccountDto) => {
      setLoading(true);
      setError(null);
      try {
        const response = await api.put<CompanyBankAccountDto>(`/CompanyBankAccount/${id}`, data);
        setCompanyBankAccounts((prev) => prev.map((cba) => (cba.id === id ? response.data : cba)));
        return response.data;
      } catch (err: any) {
        const errorMessage = err.response?.data?.message || 'Error al actualizar cuenta bancaria';
        setError(errorMessage);
        throw new Error(errorMessage);
      } finally {
        setLoading(false);
      }
    },
    []
  );

  const deleteCompanyBankAccount = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      await api.delete(`/CompanyBankAccount/${id}`);
      setCompanyBankAccounts((prev) => prev.filter((cba) => cba.id !== id));
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al eliminar cuenta bancaria';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    companyBankAccounts,
    loading,
    error,
    fetchCompanyBankAccounts,
    getCompanyBankAccountById,
    createCompanyBankAccount,
    updateCompanyBankAccount,
    deleteCompanyBankAccount,
  };
};
