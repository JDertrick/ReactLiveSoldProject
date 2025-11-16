import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '../services/api';
import { Wallet, WalletTransaction, CreateWalletTransactionDto, Receipt, CreateReceiptDto } from '../types/wallet.types';

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

// Create Receipt
export const useCreateReceipt = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateReceiptDto): Promise<Receipt> => {
      const response = await apiClient.post('/wallet/receipt', data);
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['wallet', variables.customerId] });
      queryClient.invalidateQueries({ queryKey: ['walletTransactions', variables.customerId] });
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
      queryClient.invalidateQueries({ queryKey: ['customers'] });
      queryClient.invalidateQueries({ queryKey: ['receipts', variables.customerId] });
    },
  });
};

// Get Receipts for a Customer
export const useGetReceipts = (customerId: string) => {
  return useQuery({
    queryKey: ['receipts', customerId],
    queryFn: async (): Promise<Receipt[]> => {
      const response = await apiClient.get(`/wallet/customer/${customerId}/receipts`);
      return response.data;
    },
    enabled: !!customerId,
  });
};

// Post Receipt
export const usePostReceipt = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (receiptId: string): Promise<Receipt> => {
      const response = await apiClient.post(`/wallet/receipt/${receiptId}/post`);
      return response.data;
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['wallet', data.customerId] });
      queryClient.invalidateQueries({ queryKey: ['walletTransactions', data.customerId] });
      queryClient.invalidateQueries({ queryKey: ['wallets'] });
      queryClient.invalidateQueries({ queryKey: ['customers'] });
      queryClient.invalidateQueries({ queryKey: ['customer', data.customerId] }); // Invalidate the specific customer query
      queryClient.invalidateQueries({ queryKey: ['receipts', data.customerId] });
    },
  });
};

// Reject Receipt
export const useRejectReceipt = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (receiptId: string): Promise<Receipt> => {
      const response = await apiClient.post(`/wallet/receipt/${receiptId}/reject`);
      return response.data;
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['receipts', data.customerId] });
    },
  });
};

// Get All Receipts in Organization
export const useGetReceiptsByOrganization = (
  customerId?: string,
  status?: string,
  fromDate?: string,
  toDate?: string
) => {
  return useQuery({
    queryKey: ['receipts', 'organization', customerId || '', status || '', fromDate || '', toDate || ''],
    queryFn: async (): Promise<Receipt[]> => {
      const params = new URLSearchParams();
      if (customerId) params.append('customerId', customerId);
      if (status) params.append('status', status);
      if (fromDate) params.append('fromDate', fromDate);
      if (toDate) params.append('toDate', toDate);

      const response = await apiClient.get(`/wallet/receipts?${params.toString()}`);
      return response.data;
    },
    staleTime: 30000, // 30 seconds
    refetchOnWindowFocus: false,
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
