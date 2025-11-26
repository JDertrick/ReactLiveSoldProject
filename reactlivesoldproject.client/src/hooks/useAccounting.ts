import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import apiClient from "../services/api";
import { ChartOfAccountDto, CreateChartOfAccountDto, JournalEntryDto } from "@/types/accounting.types";

//================================================================================
// API Functions
//================================================================================

const getChartOfAccounts = async (): Promise<ChartOfAccountDto[]> => {
    const response = await apiClient.get('/accounting/accounts');
    return response.data;
};

const createChartOfAccount = async (accountData: CreateChartOfAccountDto): Promise<ChartOfAccountDto> => {
    const response = await apiClient.post('/accounting/accounts', accountData);
    return response.data;
};

const getJournalEntries = async (fromDate?: string, toDate?: string): Promise<JournalEntryDto[]> => {
    const params = new URLSearchParams();
    if (fromDate) params.append("fromDate", fromDate);
    if (toDate) params.append("toDate", toDate);

    const response = await apiClient.get('/accounting/journalentries', { params });
    return response.data;
};

//================================================================================
// React Query Hooks
//================================================================================

export const useGetChartOfAccounts = () => {
    return useQuery<ChartOfAccountDto[], Error>({
        queryKey: ["chartOfAccounts"],
        queryFn: getChartOfAccounts,
    });
};

export const useCreateChartOfAccount = () => {
    const queryClient = useQueryClient();
    return useMutation<ChartOfAccountDto, Error, CreateChartOfAccountDto>({
        mutationFn: createChartOfAccount,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["chartOfAccounts"] });
        },
    });
};

export const useGetJournalEntries = (fromDate?: string, toDate?: string) => {
    return useQuery<JournalEntryDto[], Error>({
        queryKey: ["journalEntries", fromDate, toDate],
        queryFn: () => getJournalEntries(fromDate, toDate),
    });
};
