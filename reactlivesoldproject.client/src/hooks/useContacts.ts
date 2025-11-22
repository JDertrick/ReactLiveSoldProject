import { useState, useCallback } from 'react';
import api from '../services/api';
import { Contact, CreateContactDto, UpdateContactDto } from '../types/contact.types';

export const useContacts = () => {
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchContacts = useCallback(async (searchTerm?: string, status?: string) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (searchTerm) params.append('searchTerm', searchTerm);
      if (status) params.append('status', status);

      const response = await api.get<Contact[]>(`/Contact?${params.toString()}`);
      setContacts(response.data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al cargar contactos');
      console.error('Error fetching contacts:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  const getContactById = useCallback(async (id: string): Promise<Contact | null> => {
    try {
      const response = await api.get<Contact>(`/Contact/${id}`);
      return response.data;
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al obtener contacto');
      console.error('Error fetching contact:', err);
      return null;
    }
  }, []);

  const searchContacts = useCallback(async (searchTerm: string): Promise<Contact[]> => {
    try {
      const response = await api.get<Contact[]>(`/Contact/search?q=${encodeURIComponent(searchTerm)}`);
      return response.data;
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al buscar contactos');
      console.error('Error searching contacts:', err);
      return [];
    }
  }, []);

  const createContact = useCallback(async (data: CreateContactDto): Promise<Contact | null> => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.post<Contact>('/Contact', data);
      await fetchContacts();
      return response.data;
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al crear contacto');
      console.error('Error creating contact:', err);
      return null;
    } finally {
      setLoading(false);
    }
  }, [fetchContacts]);

  const updateContact = useCallback(async (id: string, data: UpdateContactDto): Promise<Contact | null> => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.put<Contact>(`/Contact/${id}`, data);
      await fetchContacts();
      return response.data;
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al actualizar contacto');
      console.error('Error updating contact:', err);
      return null;
    } finally {
      setLoading(false);
    }
  }, [fetchContacts]);

  const deleteContact = useCallback(async (id: string): Promise<boolean> => {
    setLoading(true);
    setError(null);
    try {
      await api.delete(`/Contact/${id}`);
      await fetchContacts();
      return true;
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al eliminar contacto');
      console.error('Error deleting contact:', err);
      return false;
    } finally {
      setLoading(false);
    }
  }, [fetchContacts]);

  return {
    contacts,
    loading,
    error,
    fetchContacts,
    getContactById,
    searchContacts,
    createContact,
    updateContact,
    deleteContact,
  };
};
