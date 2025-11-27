import { useState, useCallback } from 'react';
import api from '../services/api';
import {
  PaymentTermsDto,
  CreatePaymentTermsDto,
  UpdatePaymentTermsDto
} from '../types/purchases.types';

export const usePaymentTerms = () => {
  const [paymentTerms, setPaymentTerms] = useState<PaymentTermsDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchPaymentTerms = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.get<PaymentTermsDto[]>('/PaymentTerms');
      setPaymentTerms(response.data);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar términos de pago';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const getPaymentTermsById = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.get<PaymentTermsDto>(`/PaymentTerms/${id}`);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar término de pago';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const createPaymentTerms = useCallback(async (data: CreatePaymentTermsDto) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post<PaymentTermsDto>('/PaymentTerms', data);
      setPaymentTerms((prev) => [...prev, response.data]);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al crear término de pago';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const updatePaymentTerms = useCallback(async (id: string, data: UpdatePaymentTermsDto) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.put<PaymentTermsDto>(`/PaymentTerms/${id}`, data);
      setPaymentTerms((prev) => prev.map((pt) => (pt.id === id ? response.data : pt)));
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al actualizar término de pago';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const deletePaymentTerms = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      await api.delete(`/PaymentTerms/${id}`);
      setPaymentTerms((prev) => prev.filter((pt) => pt.id !== id));
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al eliminar término de pago';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    paymentTerms,
    loading,
    error,
    fetchPaymentTerms,
    getPaymentTermsById,
    createPaymentTerms,
    updatePaymentTerms,
    deletePaymentTerms,
  };
};
