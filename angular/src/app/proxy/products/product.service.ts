import type { CreateUpdateProductDto, ExternalProductResultDto, GetProductByBarcodeDto, ProductDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUpdateProductDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto>({
      method: 'POST',
      headers: { Accept: 'application/json' },
      url: '/api/app/product',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/product/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: `/api/app/product/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getByBarcode = (input: GetProductByBarcodeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExternalProductResultDto>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/product/by-barcode',
      params: { barcode: input.barcode },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProductDto>>({
      method: 'GET',
      headers: { Accept: 'application/json' },
      url: '/api/app/product',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateProductDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductDto>({
      method: 'PUT',
      headers: { Accept: 'application/json' },
      url: `/api/app/product/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}