export interface User {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
    organizationId?: string;
    isSuperAdmin: boolean;
    phoneNumber?: string;
    createdAt?: string;
}

export interface CreateUserDto {
    email: string;
    password: string;
    firstName?: string;
    lastName?: string;
    organizationId: string;
    role: 'Seller' | 'Owner';
}