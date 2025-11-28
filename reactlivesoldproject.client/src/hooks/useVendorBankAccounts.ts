import { useState, useCallback } from 'react';
import api from '../services/api';
import {
  VendorBankAccountDto,
  CreateVendorBankAccountDto,
  UpdateVendorBankAccountDto
} from '../types/purchases.types';

export const useVendorBankAccounts = () => {
  const [vendorBankAccounts, setVendorBankAccounts] = useState<VendorBankAccountDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchVendorBankAccounts = useCallback(async (vendorId: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.get<VendorBankAccountDto[]>(`/VendorBankAccount/vendor/${vendorId}`);
      setVendorBankAccounts(response.data);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar cuentas bancarias del proveedor';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const getVendorBankAccountById = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.get<VendorBankAccountDto>(`/VendorBankAccount/${id}`);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar cuenta bancaria';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const createVendorBankAccount = useCallback(async (data: CreateVendorBankAccountDto) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post<VendorBankAccountDto>('/VendorBankAccount', data);
      setVendorBankAccounts((prev) => [...prev, response.data]);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al crear cuenta bancaria';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const updateVendorBankAccount = useCallback(
    async (id: string, data: UpdateVendorBankAccountDto) => {
      setLoading(true);
      setError(null);
      try {
        const response = await api.put<VendorBankAccountDto>(`/VendorBankAccount/${id}`, data);
        setVendorBankAccounts((prev) => prev.map((vba) => (vba.id === id ? response.data : vba)));
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

  const deleteVendorBankAccount = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      await api.delete(`/VendorBankAccount/${id}`);
      setVendorBankAccounts((prev) => prev.filter((vba) => vba.id !== id));
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al eliminar cuenta bancaria';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    vendorBankAccounts,
    loading,
    error,
    fetchVendorBankAccounts,
    getVendorBankAccountById,
    createVendorBankAccount,
    updateVendorBankAccount,
    deleteVendorBankAccount,
  };
};
