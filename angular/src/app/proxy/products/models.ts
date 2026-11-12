import type { ExternalProductLookupStatus } from './external-product-lookup-status.enum';
import type { AuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateProductDto {
  name: string;
  brand: string;
}

export interface ExternalProductDto {
  barcode?: string;
  name?: string | null;
  brand?: string | null;
  imageUrl?: string | null;
  ingredients?: string | null;
  nutriScore?: string | null;
}

export interface ExternalProductResultDto {
  status?: ExternalProductLookupStatus;
  product?: ExternalProductDto | null;
  message?: string | null;
}

export interface GetProductByBarcodeDto {
  barcode: string;
}

export interface ProductDto extends AuditedEntityDto<string> {
  name?: string;
  brand?: string;
}
