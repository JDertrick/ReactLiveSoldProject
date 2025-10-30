import { create } from 'zustand';

interface PortalBrandState {
  organizationSlug: string | null;
  organizationName: string | null;
  logoUrl: string | null;
  isLoading: boolean;
  error: string | null;
  setBrand: (slug: string, name: string, logoUrl: string | null) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  clearBrand: () => void;
}

export const usePortalBrandStore = create<PortalBrandState>((set) => ({
  organizationSlug: null,
  organizationName: null,
  logoUrl: null,
  isLoading: false,
  error: null,

  setBrand: (slug: string, name: string, logoUrl: string | null) => {
    set({
      organizationSlug: slug,
      organizationName: name,
      logoUrl,
      error: null,
    });
  },

  setLoading: (loading: boolean) => {
    set({ isLoading: loading });
  },

  setError: (error: string | null) => {
    set({ error, isLoading: false });
  },

  clearBrand: () => {
    set({
      organizationSlug: null,
      organizationName: null,
      logoUrl: null,
      error: null,
      isLoading: false,
    });
  },
}));
