import type { AuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { PantryItemStatus } from './pantry-item-status.enum';

export interface GetPantryItemsInputDto extends PagedAndSortedResultRequestDto {
}

export interface PantryItemDto extends AuditedEntityDto<string> {
  userId?: string;
  productId?: string;
  productName?: string;
  status?: PantryItemStatus;
  expirationDate?: string | null;
  quantity?: number | null;
  unit?: string | null;
}

export interface UpdatePantryItemDto {
  quantity?: number | null;
  unit?: string | null;
  expirationDate?: string | null;
}
