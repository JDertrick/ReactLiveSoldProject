// Types for Inventory Audit Module

export type InventoryAuditStatus = 'Draft' | 'InProgress' | 'Completed' | 'Cancelled';

export interface InventoryAuditDto {
  id: string;
  organizationId: string;
  name: string;
  description?: string;
  status: InventoryAuditStatus;
  snapshotTakenAt: string;
  startedAt?: string;
  completedAt?: string;
  createdByUserId: string;
  createdByUserName?: string;
  completedByUserId?: string;
  completedByUserName?: string;
  totalVariants: number;
  countedVariants: number;
  totalVariance: number;
  totalVarianceValue: number;
  notes?: string;
  createdAt: string;
  updatedAt: string;
  progress: number;
  // Scope fields
  scopeType: string;
  locationId?: string;
  locationName?: string;
  scopeDescription?: string;
}

export interface InventoryAuditItemDto {
  id: string;
  inventoryAuditId: string;
  productVariantId: string;
  productName: string;
  variantSku: string;
  variantSize?: string;
  variantColor?: string;
  variantImageUrl?: string;
  theoreticalStock: number;
  countedStock?: number;
  variance?: number;
  varianceValue?: number;
  snapshotAverageCost: number;
  countedByUserId?: string;
  countedByUserName?: string;
  countedAt?: string;
  adjustmentMovementId?: string;
  notes?: string;
  isCounted: boolean;
  hasVariance: boolean;
}

export interface InventoryAuditDetailDto extends InventoryAuditDto {
  items: InventoryAuditItemDto[];
}

export interface BlindCountItemDto {
  id: string;
  productVariantId: string;
  productName: string;
  variantSku: string;
  variantSize?: string;
  variantColor?: string;
  variantImageUrl?: string;
  countedStock?: number;
  isCounted: boolean;
  notes?: string;
}

export type AuditScopeType = 'Total' | 'Partial';

export interface CreateInventoryAuditDto {
  name: string;
  description?: string;
  notes?: string;
  scopeType: AuditScopeType;
  locationId?: string;
  categoryIds?: string[];
  tagIds?: string[];
  randomSampleCount?: number;
  excludeAuditedInLastDays?: number;
}

export interface UpdateAuditCountDto {
  itemId: string;
  countedStock: number;
  notes?: string;
}

export interface BulkUpdateAuditCountDto {
  counts: UpdateAuditCountDto[];
}

export interface InventoryAuditSummaryDto {
  auditId: string;
  auditName: string;
  status: string;
  totalVariants: number;
  countedVariants: number;
  pendingVariants: number;
  progress: number;
  itemsWithVariance: number;
  itemsWithPositiveVariance: number;
  itemsWithNegativeVariance: number;
  itemsWithNoVariance: number;
  totalPositiveVariance: number;
  totalNegativeVariance: number;
  netVariance: number;
  totalPositiveVarianceValue: number;
  totalNegativeVarianceValue: number;
  netVarianceValue: number;
}

export interface CompleteAuditDto {
  autoPostAdjustments: boolean;
  notes?: string;
}
