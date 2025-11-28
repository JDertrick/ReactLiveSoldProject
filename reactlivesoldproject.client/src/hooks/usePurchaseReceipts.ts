import { useState, useCallback } from 'react';
import api from '../services/api';
import {
  PurchaseReceiptDto,
  CreatePurchaseReceiptDto,
  ReceivePurchaseDto
} from '../types/purchases.types';

export const usePurchaseReceipts = () => {
  const [purchaseReceipts, setPurchaseReceipts] = useState<PurchaseReceiptDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchPurchaseReceipts = useCallback(async (searchTerm?: string, status?: string) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (searchTerm) params.append('searchTerm', searchTerm);
      if (status) params.append('status', status);

      const response = await api.get<PurchaseReceiptDto[]>(
        `/PurchaseReceipt?${params.toString()}`
      );
      setPurchaseReceipts(response.data);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar recepciones de compra';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const getPurchaseReceiptById = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.get<PurchaseReceiptDto>(`/PurchaseReceipt/${id}`);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar recepción';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const createPurchaseReceipt = useCallback(async (data: CreatePurchaseReceiptDto) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post<PurchaseReceiptDto>('/PurchaseReceipt', data);
      setPurchaseReceipts((prev) => [...prev, response.data]);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al crear recepción';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const receivePurchase = useCallback(async (id: string, data: ReceivePurchaseDto) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post<{ message: string; receipt: PurchaseReceiptDto }>(
        `/PurchaseReceipt/${id}/receive`,
        data
      );
      setPurchaseReceipts((prev) =>
        prev.map((pr) => (pr.id === id ? response.data.receipt : pr))
      );
      return response.data.receipt;
    } catch (err: any) {
      const errorMessage = err.response?.data?.error || err.response?.data?.message || 'Error al recibir compra';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const deletePurchaseReceipt = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      await api.delete(`/PurchaseReceipt/${id}`);
      setPurchaseReceipts((prev) => prev.filter((pr) => pr.id !== id));
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al eliminar recepción';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    purchaseReceipts,
    loading,
    error,
    fetchPurchaseReceipts,
    getPurchaseReceiptById,
    createPurchaseReceipt,
    receivePurchase,
    deletePurchaseReceipt,
  };
};
