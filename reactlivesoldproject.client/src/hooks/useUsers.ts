import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import apiClient from "../services/api";
import { User, CreateUserDto } from "../types";

// Hook para obtener usuarios de la organización del usuario autenticado (Employee)
export const useGetUsers = () => {
    return useQuery({
        queryKey: ['users'],
        queryFn: async (): Promise<User[]> => {
            const response = await apiClient.get('/user');
            return response.data;
        },
    });
}

// Hook para SuperAdmin - obtener usuarios de una organización específica
export const useGetUserByOrganizationId = (organizationId: string) => {
    return useQuery({
        queryKey: ['users', organizationId],
        queryFn: async (): Promise<User[]> => {
            const response = await apiClient.get(`/user/users/${organizationId}`);
            return response.data;
        },
        enabled: !!organizationId,
    });
}

export const useCreateUser = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (dto: CreateUserDto): Promise<User> => {
            const response = await apiClient.post('/user', dto);
            return response.data;
        },
        onSuccess: (data) => {
            // Invalidar la lista de usuarios de la organización
            queryClient.invalidateQueries({ queryKey: ['users', data.organizationId] });
            queryClient.invalidateQueries({ queryKey: ['users'] });
        },
    });
}