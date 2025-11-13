import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '../services/api';
import { Wallet, WalletTransaction, CreateWalletTransactionDto } from '../types/wallet.types';

// Get Customer Wallet
export const useGetCustomerWallet = (customerId: string) => {
  return useQuery({
    queryKey: ['wallet', customerId],
    queryFn: async (): Promise<Wallet> => {
      const response = await apiClient.get(`/wallet/customer/${customerId}`);
      return response.data;
    },
    enabled: !!customerId,
  });
};

// Get Wallet Transactions
export const useGetWalletTransactions = (customerId: string) => {
  return useQuery({
    queryKey: ['walletTransactions', customerId],
    queryFn: async (): Promise<WalletTransaction[]> => {
      const response = await apiClient.get(`/wallet/customer/${customerId}/transactions`);
      return response.data;
    },
    enabled: !!customerId,
  });
};

// Get All Wallets in Organization
export const useGetAllWallets = () => {
  return useQuery({
    queryKey: ['wallets'],
    queryFn: async (): Promise<Wallet[]> => {
      const response = await apiClient.get('/wallet');
      return response.data;
    },
  });
};

// Create Wallet Transaction (Add or Deduct Funds)
export const useCreateWalletTransaction = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateWalletTransactionDto): Promise<WalletTransaction> => {
      // Determine endpoint based on transaction type
      const endpoint = data.type.toLowerCase() === 'deposit' ? '/wallet/deposit' : '/wallet/withdrawal';
      const response = await apiClient.post(endpoint, data);
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['wallet', variables.customerId] });
      queryClient.invalidateQueries({ queryKey: ['walletTransactions', variables.customerId] });
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
      queryClient.invalidateQueries({ queryKey: ['customers'] });
    },
  });
};

// Add Funds to Wallet (convenience wrapper)
export const useAddFundsToWallet = () => {
  const createTransaction = useCreateWalletTransaction();

  return {
    ...createTransaction,
    mutateAsync: (customerId: string, amount: number, notes?: string) =>
      createTransaction.mutateAsync({
        customerId,
        type: 'Credit',
        amount,
        notes,
      }),
  };
};

// Deduct Funds from Wallet (convenience wrapper)
export const useDeductFundsFromWallet = () => {
  const createTransaction = useCreateWalletTransaction();

  return {
    ...createTransaction,
    mutateAsync: (customerId: string, amount: number, notes?: string) =>
      createTransaction.mutateAsync({
        customerId,
        type: 'Debit',
        amount,
        notes,
      }),
  };
};
