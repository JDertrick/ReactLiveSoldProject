import { useState, useCallback } from 'react';
import api from '../services/api';
import {
  ProductVendorDto,
  CreateProductVendorDto,
  UpdateProductVendorDto
} from '../types/purchases.types';

export const useProductVendors = () => {
  const [productVendors, setProductVendors] = useState<ProductVendorDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchProductVendors = useCallback(async (productId?: string, vendorId?: string) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (productId) params.append('productId', productId);
      if (vendorId) params.append('vendorId', vendorId);

      const response = await api.get<ProductVendorDto[]>(
        `/ProductVendor?${params.toString()}`
      );
      setProductVendors(response.data);
      return response.data;
    } catch (err: any) {
      const errorMessage =
        err.response?.data?.message || 'Error al cargar relaciones producto-proveedor';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const getProductVendorById = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.get<ProductVendorDto>(`/ProductVendor/${id}`);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al cargar relación';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const createProductVendor = useCallback(async (data: CreateProductVendorDto) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post<ProductVendorDto>('/ProductVendor', data);
      setProductVendors((prev) => [...prev, response.data]);
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al crear relación';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const updateProductVendor = useCallback(async (id: string, data: UpdateProductVendorDto) => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.put<ProductVendorDto>(`/ProductVendor/${id}`, data);
      setProductVendors((prev) => prev.map((pv) => (pv.id === id ? response.data : pv)));
      return response.data;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al actualizar relación';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  const deleteProductVendor = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      await api.delete(`/ProductVendor/${id}`);
      setProductVendors((prev) => prev.filter((pv) => pv.id !== id));
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Error al eliminar relación';
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, []);

  return {
    productVendors,
    loading,
    error,
    fetchProductVendors,
    getProductVendorById,
    createProductVendor,
    updateProductVendor,
    deleteProductVendor,
  };
};
