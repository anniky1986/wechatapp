import request from '../../utils/request'
import type { PageParams, PageResult } from '../../types'

export interface TenantRecord {
  id: number
  name: string
  code: string
  contactName: string
  contactPhone: string
  status: number
  expireTime: string
  createTime: string
}

export interface TenantQuery extends PageParams {
  name?: string
  code?: string
  status?: number
}

export function getTenantPage(params: TenantQuery): Promise<PageResult<TenantRecord>> {
  return request.get('/tenant/page', { params })
}

export function getTenant(id: number): Promise<TenantRecord> {
  return request.get(`/tenant/${id}`)
}

export function addTenant(data: Partial<TenantRecord>): Promise<void> {
  return request.post('/tenant', data)
}

export function updateTenant(id: number, data: Partial<TenantRecord>): Promise<void> {
  return request.put(`/tenant/${id}`, data)
}

export function deleteTenant(id: number): Promise<void> {
  return request.delete(`/tenant/${id}`)
}

export function setTenantStatus(id: number, status: number): Promise<void> {
  return request.put(`/tenant/${id}/status`, { status })
}