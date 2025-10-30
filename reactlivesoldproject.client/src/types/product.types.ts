// Product Types
export interface Tag {
  id: string;
  organizationId: string;
  name: string;
}

export interface ProductVariant {
  id: string;
  productId: string;
  sku?: string;
  price: number;
  stockQuantity: number;
  attributes?: string;
  imageUrl?: string;
  createdAt: string;
  updatedAt: string;
}

export interface Product {
  id: string;
  organizationId: string;
  name: string;
  description?: string;
  productType: string;
  isPublished: boolean;
  tags: Tag[];
  variants: ProductVariant[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateProductVariantDto {
  sku?: string;
  price: number;
  stockQuantity: number;
  attributes?: string;
  imageUrl?: string;
}

export interface CreateProductDto {
  name: string;
  description?: string;
  productType: string;
  isPublished: boolean;
  tagIds: string[];
  variants: CreateProductVariantDto[];
}

export interface UpdateProductDto {
  name: string;
  description?: string;
  productType: string;
  isPublished: boolean;
  tagIds: string[];
}
