// Product Types
export interface Tag {
  id: string;
  organizationId: string;
  name: string;
}

// Alias for Tag (for backward compatibility)
export type TagDto = Tag;

export interface ProductVariant {
  id: string;
  productId: string;
  sku?: string;
  price: number;
  stockQuantity: number;
  averageCost: number;
  attributes?: string;  // JSON string
  imageUrl?: string;
  createdAt: string;
  updatedAt: string;
  size: string;
  color: string;
}

export interface Product {
  id: string;
  organizationId: string;
  name: string;
  description?: string;
  productType: string;
  basePrice: number;
  imageUrl: string;
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
  attributes?: string;  // JSON string: {color, size, etc}
  imageUrl?: string;
}

export interface CreateProductDto {
  name: string;
  description?: string;
  productType: string;
  isPublished: boolean;
  basePrice: number;
  imageUrl: string;
  tagIds: string[];
  variants: CreateProductVariantDto[];
}

export interface UpdateProductDto {
  name: string;
  description?: string;
  productType: string;
  basePrice: number;
  imageUrl: string;
  isPublished: boolean;
  tagIds: string[];
  variants?: CreateProductVariantDto[];
}

// Helper type for variant form input
export interface ProductVariantDto {
  id?: string; // Optional - only present for existing variants
  sku: string;
  size?: string;
  color?: string;
  price: number;
  stock: number;
  stockQuantity: number; // Alias for stock
  imageUrl?: string;
  attributes?: string; // JSON string
}

// Form input type for variant (string values for number inputs)
export interface VariantFormInput {
  sku: string;
  size?: string;
  color?: string;
  price: string | number;
  stockQuantity: string | number;
  stock?: string | number;
  imageUrl?: string;
}
