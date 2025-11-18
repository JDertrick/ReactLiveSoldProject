import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '../services/api';
import { Product, Tag, CreateProductDto, UpdateProductDto, CreateProductVariantDto } from '../types';

// Get All Products
export const useGetProducts = (includeUnpublished: boolean = false) => {
  return useQuery({
    queryKey: ['products', includeUnpublished],
    queryFn: async (): Promise<Product[]> => {
      const response = await apiClient.get(`/product?includeUnpublished=${includeUnpublished}`);
      return response.data;
    },
  });
};

// Search Products
export const useSearchProducts = (searchTerm: string) => {
  return useQuery({
    queryKey: ['products', 'search', searchTerm],
    queryFn: async (): Promise<Product[]> => {
      const response = await apiClient.get(`/product/search?q=${searchTerm}`);
      return response.data;
    },
    enabled: searchTerm.length > 0,
  });
};

// Get Tags
export const useGetTags = () => {
  return useQuery({
    queryKey: ['tags'],
    queryFn: async (): Promise<Tag[]> => {
      const response = await apiClient.get('/product/tags');
      return response.data;
    },
  });
};

// Create Product
export const useCreateProduct = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateProductDto): Promise<Product> => {
      const response = await apiClient.post('/product', data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
  });
};

// Update Product
export const useUpdateProduct = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: UpdateProductDto }): Promise<Product> => {
      const response = await apiClient.put(`/product/${id}`, data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
  });
};

// Update Product Variants
export const useUpdateProductVariants = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ productId, variants }: { productId: string; variants: CreateProductVariantDto[] }): Promise<Product> => {
      // Assume backend has endpoint to update variants
      // Adjust URL if different
      const response = await apiClient.put(`/product/${productId}/variants`, { variants });
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
  });
};

// Upload Product Image
export const useUploadProductImage = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ productId, image }: { productId: string; image: File }): Promise<Product> => {
      const formData = new FormData();
      formData.append('image', image);

      const response = await apiClient.post(`/product/${productId}/image`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
  });
};
