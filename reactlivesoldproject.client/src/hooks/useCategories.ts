import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Category } from '../types/category.types';
import api from '../services/api';

const fetchCategories = async (): Promise<Category[]> => {
  const response = await api.get('/Category');
  return response.data;
};

const createCategory = async (newCategory: Partial<Category>): Promise<Category> => {
  const response = await api.post('/Category', newCategory);
  return response.data;
};

const updateCategory = async ({ id, data }: { id: string, data: Partial<Category> }): Promise<Category> => {
  const response = await api.put(`/Category/${id}`, data);
  return response.data;
};

const deleteCategory = async (id: string): Promise<void> => {
  await api.delete(`/Category/${id}`);
};

export const useCategories = () => {
  const queryClient = useQueryClient();

  const { data: categories, isLoading, error } = useQuery<Category[], Error>({
    queryKey: ['categories'],
    queryFn: fetchCategories,
  });

  const createMutation = useMutation<Category, Error, Partial<Category>>({
    mutationFn: createCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
    },
  });

  const updateMutation = useMutation<Category, Error, { id: string, data: Partial<Category> }>({
    mutationFn: updateCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
    },
  });

  const deleteMutation = useMutation<void, Error, string>({
    mutationFn: deleteCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
    },
  });

  return {
    categories,
    isLoading,
    error,
    createCategory: createMutation.mutateAsync,
    updateCategory: updateMutation.mutateAsync,
    deleteCategory: deleteMutation.mutateAsync,
  };
};
