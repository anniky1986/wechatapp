import request from '../../utils/request'
import type { PageParams, PageResult } from '../../types'

export interface RoleRecord {
  id: number
  name: string
  code: string
  description: string
  status: number
  menuIds: number[]
  createTime: string
}

export interface RoleQuery extends PageParams {
  name?: string
  code?: string
  status?: number
}

export function getRolePage(params: RoleQuery): Promise<PageResult<RoleRecord>> {
  return request.get('/role/page', { params })
}

export function getRoleList(): Promise<RoleRecord[]> {
  return request.get('/role/list')
}

export function getRole(id: number): Promise<RoleRecord> {
  return request.get(`/role/${id}`)
}

export function addRole(data: Partial<RoleRecord>): Promise<void> {
  return request.post('/role', data)
}

export function updateRole(id: number, data: Partial<RoleRecord>): Promise<void> {
  return request.put(`/role/${id}`, data)
}

export function deleteRole(id: number): Promise<void> {
  return request.delete(`/role/${id}`)
}

export function setRoleStatus(id: number, status: number): Promise<void> {
  return request.put(`/role/${id}/status`, { status })
}

export function setRoleMenus(roleId: number, menuIds: number[]): Promise<void> {
  return request.put(`/role/${roleId}/menus`, { menuIds })
}

export function getRoleMenuIds(roleId: number): Promise<number[]> {
  return request.get(`/role/${roleId}/menus`)
}

export function setRoleDataScope(roleId: number, dataScope: number, deptIds?: number[]): Promise<void> {
  return request.put(`/role/${roleId}/data-scope`, { dataScope, deptIds })
}

export function getRoleDataScope(roleId: number): Promise<{ dataScope: number; deptIds: number[] }> {
  return request.get(`/role/${roleId}/data-scope`)
}