import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../services/api';
import {
  NoSerie,
  CreateNoSerieDto,
  UpdateNoSerieDto,
  CreateNoSerieLineDto,
  UpdateNoSerieLineDto,
  NoSerieLine,
  DocumentType,
  NextNumberResponse,
  ValidateNumberResponse,
} from '../types/noserie.types';

// Fetch all series
const fetchNoSeries = async (): Promise<NoSerie[]> => {
  const response = await api.get('/SerieNo');
  return response.data;
};

// Fetch single serie by ID
const fetchNoSerieById = async (id: string): Promise<NoSerie> => {
  const response = await api.get(`/SerieNo/${id}`);
  return response.data;
};

// Fetch serie by code
const fetchNoSerieByCode = async (code: string): Promise<NoSerie> => {
  const response = await api.get(`/SerieNo/code/${code}`);
  return response.data;
};

// Fetch series by document type
const fetchNoSeriesByType = async (documentType: DocumentType): Promise<NoSerie[]> => {
  const response = await api.get(`/SerieNo/type/${documentType}`);
  return response.data;
};

// Get default serie by document type
const fetchDefaultSerieByType = async (documentType: DocumentType): Promise<NoSerie | null> => {
  try {
    const response = await api.get(`/SerieNo/type/${documentType}/default`);
    return response.data;
  } catch (error) {
    return null;
  }
};

// Create new serie
const createNoSerie = async (data: CreateNoSerieDto): Promise<NoSerie> => {
  const response = await api.post('/SerieNo', data);
  return response.data;
};

// Update serie
const updateNoSerie = async ({ id, data }: { id: string; data: UpdateNoSerieDto }): Promise<NoSerie> => {
  const response = await api.put(`/SerieNo/${id}`, data);
  return response.data;
};

// Delete serie
const deleteNoSerie = async (id: string): Promise<void> => {
  await api.delete(`/SerieNo/${id}`);
};

// Add line to serie
const addNoSerieLine = async ({
  serieId,
  data,
}: {
  serieId: string;
  data: CreateNoSerieLineDto;
}): Promise<NoSerieLine> => {
  const response = await api.post(`/SerieNo/${serieId}/lines`, data);
  return response.data;
};

// Update line
const updateNoSerieLine = async ({
  lineId,
  data,
}: {
  lineId: string;
  data: UpdateNoSerieLineDto;
}): Promise<NoSerieLine> => {
  const response = await api.put(`/SerieNo/lines/${lineId}`, data);
  return response.data;
};

// Delete line
const deleteNoSerieLine = async (lineId: string): Promise<void> => {
  await api.delete(`/SerieNo/lines/${lineId}`);
};

// Get next number by code
const getNextNumber = async (code: string, date?: Date): Promise<NextNumberResponse> => {
  const params = date ? { date: date.toISOString() } : {};
  const response = await api.get(`/SerieNo/next/${code}`, { params });
  return response.data;
};

// Get next number by document type
const getNextNumberByType = async (
  documentType: DocumentType,
  date?: Date
): Promise<NextNumberResponse> => {
  const params = date ? { date: date.toISOString() } : {};
  const response = await api.get(`/SerieNo/next/type/${documentType}`, { params });
  return response.data;
};

// Validate number
const validateNumber = async (code: string, number: string): Promise<ValidateNumberResponse> => {
  const response = await api.get(`/SerieNo/validate/${code}/${number}`);
  return response.data;
};

// Check if number is available
const isNumberAvailable = async (code: string, number: string): Promise<boolean> => {
  const response = await api.get(`/SerieNo/available/${code}/${number}`);
  return response.data.isAvailable;
};

export const useNoSeries = () => {
  const queryClient = useQueryClient();

  // Fetch all series
  const {
    data: series,
    isLoading,
    error,
  } = useQuery<NoSerie[], Error>({
    queryKey: ['noSeries'],
    queryFn: fetchNoSeries,
  });

  // Create mutation
  const createMutation = useMutation<NoSerie, Error, CreateNoSerieDto>({
    mutationFn: createNoSerie,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['noSeries'] });
    },
  });

  // Update mutation
  const updateMutation = useMutation<NoSerie, Error, { id: string; data: UpdateNoSerieDto }>({
    mutationFn: updateNoSerie,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['noSeries'] });
    },
  });

  // Delete mutation
  const deleteMutation = useMutation<void, Error, string>({
    mutationFn: deleteNoSerie,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['noSeries'] });
    },
  });

  // Add line mutation
  const addLineMutation = useMutation<
    NoSerieLine,
    Error,
    { serieId: string; data: CreateNoSerieLineDto }
  >({
    mutationFn: addNoSerieLine,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['noSeries'] });
    },
  });

  // Update line mutation
  const updateLineMutation = useMutation<
    NoSerieLine,
    Error,
    { lineId: string; data: UpdateNoSerieLineDto }
  >({
    mutationFn: updateNoSerieLine,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['noSeries'] });
    },
  });

  // Delete line mutation
  const deleteLineMutation = useMutation<void, Error, string>({
    mutationFn: deleteNoSerieLine,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['noSeries'] });
    },
  });

  return {
    // Data
    series,
    isLoading,
    error,

    // CRUD operations
    createSerie: createMutation.mutateAsync,
    updateSerie: updateMutation.mutateAsync,
    deleteSerie: deleteMutation.mutateAsync,

    // Line operations
    addLine: addLineMutation.mutateAsync,
    updateLine: updateLineMutation.mutateAsync,
    deleteLine: deleteLineMutation.mutateAsync,

    // Helper functions (not using mutations)
    getSerieById: fetchNoSerieById,
    getSerieByCode: fetchNoSerieByCode,
    getSeriesByType: fetchNoSeriesByType,
    getDefaultSerieByType: fetchDefaultSerieByType,
    getNextNumber,
    getNextNumberByType,
    validateNumber,
    isNumberAvailable,
  };
};
