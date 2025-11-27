import { useState, useCallback } from 'react';
import api from '../services/api';
import { VendorInvoiceDto, CreateVendorInvoiceDto } from '../types/purchases.types';

export const useVendorInvoices = () => {
  const [vendorInvoices, setVendorInvoices] = useState<VendorInvoiceDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchVendorInvoices = useCallback(async (vendorId?: string, status?: string) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (vendorId) params.append('vendorId', vendorId);
      if (status) params.append('status', status);

      const response = await api.get<VendorInvoiceDto[]>(
        `/VendorInvoice?${params.toString()}`
      );
      setVendorInvoices(response.data);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar facturas de proveedores';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const getVendorInvoiceById = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.get<VendorInvoiceDto>(`/VendorInvoice/${id}`);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar factura';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const createVendorInvoice = useCallback(async (data: CreateVendorInvoiceDto) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post<VendorInvoiceDto>('/VendorInvoice', data);
      setVendorInvoices((prev) => [...prev, response.data]);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al crear factura';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const updateVendorInvoice = useCallback(async (id: string, data: CreateVendorInvoiceDto) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.put<VendorInvoiceDto>(`/VendorInvoice/${id}`, data);
      setVendorInvoices((prev) =>
        prev.map((vi) => (vi.id === id ? response.data : vi))
      );
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al actualizar factura';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const deleteVendorInvoice = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      await api.delete(`/VendorInvoice/${id}`);
      setVendorInvoices((prev) => prev.filter((vi) => vi.id !== id));
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al eliminar factura';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    vendorInvoices,
    loading,
    error,
    fetchVendorInvoices,
    getVendorInvoiceById,
    createVendorInvoice,
    updateVendorInvoice,
    deleteVendorInvoice,
  };
};
