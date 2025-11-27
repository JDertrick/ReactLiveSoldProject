import { useState, useCallback } from 'react';
import api from '../services/api';
import { PaymentDto, CreatePaymentDto, PaymentStatus } from '../types/purchases.types';

export const usePayments = () => {
  const [payments, setPayments] = useState<PaymentDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchPayments = useCallback(async (vendorId?: string, status?: string) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (vendorId) params.append('vendorId', vendorId);
      if (status) params.append('status', status);

      const response = await api.get<PaymentDto[]>(`/Payment?${params.toString()}`);
      setPayments(response.data);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar pagos';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const getPaymentById = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.get<PaymentDto>(`/Payment/${id}`);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar pago';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const createPayment = useCallback(async (data: CreatePaymentDto) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post<PaymentDto>('/Payment', data);
      setPayments((prev) => [...prev, response.data]);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al crear pago';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const approvePayment = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post<PaymentDto>(`/Payment/${id}/approve`);
      setPayments((prev) => prev.map((p) => (p.id === id ? response.data : p)));
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al aprobar pago';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const rejectPayment = useCallback(async (id: string, reason: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post<PaymentDto>(`/Payment/${id}/reject`, { reason });
      setPayments((prev) => prev.map((p) => (p.id === id ? response.data : p)));
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al rechazar pago';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const postPayment = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post<PaymentDto>(`/Payment/${id}/post`);
      setPayments((prev) => prev.map((p) => (p.id === id ? response.data : p)));
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al contabilizar pago';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const deletePayment = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      await api.delete(`/Payment/${id}`);
      setPayments((prev) => prev.filter((p) => p.id !== id));
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al eliminar pago';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    payments,
    loading,
    error,
    fetchPayments,
    getPaymentById,
    createPayment,
    approvePayment,
    rejectPayment,
    postPayment,
    deletePayment,
  };
};
