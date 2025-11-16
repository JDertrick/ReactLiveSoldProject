export interface Category {
    id: string;
    organizationId: string;
    name: string;
    description?: string;
    parentId?: string;
    children: Category[];
    createdAt: string;
    updatedAt: string;
}
