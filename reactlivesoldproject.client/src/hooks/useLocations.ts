import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Location } from '../types/location.types';
import api from '../services/api';

const fetchLocations = async (): Promise<Location[]> => {
  const response = await api.get('/Location');
  return response.data;
};

const createLocation = async (newLocation: Partial<Location>): Promise<Location> => {
  const response = await api.post('/Location', newLocation);
  return response.data;
};

const updateLocation = async ({ id, data }: { id: string, data: Partial<Location> }): Promise<Location> => {
  const response = await api.put(`/Location/${id}`, data);
  return response.data;
};

const deleteLocation = async (id: string): Promise<void> => {
  await api.delete(`/Location/${id}`);
};

export const useLocations = () => {
  const queryClient = useQueryClient();

  const { data: locations, isLoading, error } = useQuery<Location[], Error>({
    queryKey: ['locations'],
    queryFn: fetchLocations,
  });

  const createMutation = useMutation<Location, Error, Partial<Location>>({
    mutationFn: createLocation,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['locations'] });
    },
  });

  const updateMutation = useMutation<Location, Error, { id: string, data: Partial<Location> }>({
    mutationFn: updateLocation,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['locations'] });
    },
  });

  const deleteMutation = useMutation<void, Error, string>({
    mutationFn: deleteLocation,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['locations'] });
    },
  });

  return {
    locations,
    isLoading,
    error,
    createLocation: createMutation.mutateAsync,
    updateLocation: updateMutation.mutateAsync,
    deleteLocation: deleteMutation.mutateAsync,
  };
};
