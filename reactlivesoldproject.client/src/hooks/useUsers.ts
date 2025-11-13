import { useQuery } from "@tanstack/react-query";
import apiClient from "../services/api";
import { User } from "../types";

export const useGetUserByOrganizationId = (organizationId: string) => {
    return useQuery({
        queryKey: ['user', organizationId],
        queryFn: async (): Promise<User[]> => {
            const response = await apiClient.get(`/users/${organizationId}`);
            return response.data;
        },
        enabled: !!organizationId,
    });
}