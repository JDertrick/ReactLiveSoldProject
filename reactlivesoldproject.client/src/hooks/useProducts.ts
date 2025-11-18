import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '../services/api';
import { PagedResult, Product, Tag, CreateProductDto, UpdateProductDto, CreateProductVariantDto, VariantProductDto, ProductVariantDto } from '../types';

// Get All Products (Paginated)
export const useGetProducts = (page: number, pageSize: number, status: string, searchTerm: string) => {
  return useQuery({
    queryKey: ['products', page, pageSize, status, searchTerm],
    queryFn: async (): Promise<PagedResult<VariantProductDto>> => {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
      });
      if (status) {
        params.append('status', status);
      }
      if (searchTerm) {
        params.append('searchTerm', searchTerm);
      }
      const response = await apiClient.get(`/product?${params.toString()}`);
      return response.data;
    },
  });
};

// Get Single Product by ID
export const useGetProduct = (productId: string | null) => {
  return useQuery({
    queryKey: ['product', productId],
    queryFn: async (): Promise<Product> => {
      if (!productId) throw new Error('Product ID is required');
      const response = await apiClient.get(`/product/${productId}`);
      return response.data;
    },
    enabled: !!productId,
  });
};

// Search Products
export const useSearchProducts = (searchTerm: string) => {
  return useQuery({
    queryKey: ['products', 'search', searchTerm],
    queryFn: async (): Promise<ProductVariantDto[]> => {
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

// Upload Variant Image
export const useUploadVariantImage = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ variantId, image }: { variantId: string; image: File }) => {
      const formData = new FormData();
      formData.append('image', image);

      const response = await apiClient.post(`/product/variants/${variantId}/image`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      return response.data;
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      if (data?.productId) {
        queryClient.invalidateQueries({ queryKey: ['product', data.productId] });
      }
    },
  });
};

// Add Variant to Product
export const useAddVariant = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ productId, variant }: { productId: string; variant: CreateProductVariantDto }) => {
      const response = await apiClient.post(`/product/${productId}/variants`, variant);
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      queryClient.invalidateQueries({ queryKey: ['product', variables.productId] });
    },
  });
};

// Update Variant
export const useUpdateVariant = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ variantId, variant }: { variantId: string; variant: CreateProductVariantDto }) => {
      const response = await apiClient.put(`/product/variants/${variantId}`, variant);
      return response.data;
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      if (data?.productId) {
        queryClient.invalidateQueries({ queryKey: ['product', data.productId] });
      }
    },
  });
};

// Delete Variant
export const useDeleteVariant = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (variantId: string) => {
      const response = await apiClient.delete(`/product/variants/${variantId}`);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      queryClient.invalidateQueries({ queryKey: ['product'] });
    },
  });
};
