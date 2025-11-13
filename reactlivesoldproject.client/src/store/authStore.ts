import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface User {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  role: string;
  organizationId?: string;
  organizationSlug?: string;
  organizationName?: string;
  organizationLogoUrl?: string;
}

interface AuthState {
  token: string | null;
  user: User | null;
  isAuthenticated: boolean;
  isEmployee: boolean;
  isCustomer: boolean;
  isSuperAdmin: boolean;
  isOwner: boolean;
  isSeller: boolean;
  login: (token: string, user: User) => void;
  logout: () => void;
  updateUser: (user: Partial<User>) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      token: null,
      user: null,
      isAuthenticated: false,
      isEmployee: false,
      isCustomer: false,
      isSuperAdmin: false,
      isOwner: false,
      isSeller: false,

      login: (token: string, user: User) => {
        set({
          token,
          user,
          isAuthenticated: true,
          isEmployee: user.role !== 'Customer',
          isCustomer: user.role === 'Customer',
          isSuperAdmin: user.role === 'SuperAdmin',
          isOwner: user.role === 'Owner',
          isSeller: user.role === 'Seller',
        });
        localStorage.setItem('token', token);
        localStorage.setItem('user', JSON.stringify(user));
      },

      logout: () => {
        set({
          token: null,
          user: null,
          isAuthenticated: false,
          isEmployee: false,
          isCustomer: false,
          isSuperAdmin: false,
          isOwner: false,
          isSeller: false,
        });
        localStorage.removeItem('token');
        localStorage.removeItem('user');
      },

      updateUser: (userData: Partial<User>) => {
        const currentUser = get().user;
        if (currentUser) {
          const updatedUser = { ...currentUser, ...userData };
          set({ user: updatedUser });
          localStorage.setItem('user', JSON.stringify(updatedUser));
        }
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        token: state.token,
        user: state.user,
        isAuthenticated: state.isAuthenticated,
        isEmployee: state.isEmployee,
        isCustomer: state.isCustomer,
        isSuperAdmin: state.isSuperAdmin,
        isOwner: state.isOwner,
        isSeller: state.isSeller,
      }),
    }
  )
);
