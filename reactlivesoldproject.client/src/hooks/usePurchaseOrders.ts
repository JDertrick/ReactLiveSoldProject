import { useState, useCallback } from 'react';
import api from '../services/api';
import {
  PurchaseOrderDto,
  CreatePurchaseOrderDto,
  PurchaseOrderStatus
} from '../types/purchases.types';

export const usePurchaseOrders = () => {
  const [purchaseOrders, setPurchaseOrders] = useState<PurchaseOrderDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchPurchaseOrders = useCallback(async (vendorId?: string, status?: string) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (vendorId) params.append('vendorId', vendorId);
      if (status) params.append('status', status);

      const response = await api.get<PurchaseOrderDto[]>(
        `/PurchaseOrder?${params.toString()}`
      );
      setPurchaseOrders(response.data);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar órdenes de compra';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const getPurchaseOrderById = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.get<PurchaseOrderDto>(`/PurchaseOrder/${id}`);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar orden de compra';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const createPurchaseOrder = useCallback(async (data: CreatePurchaseOrderDto) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post<PurchaseOrderDto>('/PurchaseOrder', data);
      setPurchaseOrders((prev) => [...prev, response.data]);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al crear orden de compra';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const updatePurchaseOrderStatus = useCallback(async (id: string, status: PurchaseOrderStatus) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.put<PurchaseOrderDto>(
        `/PurchaseOrder/${id}/status`,
        { status }
      );
      setPurchaseOrders((prev) =>
        prev.map((po) => (po.id === id ? response.data : po))
      );
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al actualizar estado';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const deletePurchaseOrder = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      await api.delete(`/PurchaseOrder/${id}`);
      setPurchaseOrders((prev) => prev.filter((po) => po.id !== id));
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al eliminar orden de compra';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    purchaseOrders,
    loading,
    error,
    fetchPurchaseOrders,
    getPurchaseOrderById,
    createPurchaseOrder,
    updatePurchaseOrderStatus,
    deletePurchaseOrder,
  };
};
