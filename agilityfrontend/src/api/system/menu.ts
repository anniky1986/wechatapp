import request from '../../utils/request'
import type { MenuItem } from '../../types'

export function getMenuTree(): Promise<MenuItem[]> {
  return request.get('/menu/tree')
}

export function getMenuList(): Promise<MenuItem[]> {
  return request.get('/menu/list')
}

export function getMenu(id: number): Promise<MenuItem> {
  return request.get(`/menu/${id}`)
}

export function addMenu(data: Partial<MenuItem>): Promise<void> {
  return request.post('/menu', data)
}

export function updateMenu(id: number, data: Partial<MenuItem>): Promise<void> {
  return request.put(`/menu/${id}`, data)
}

export function deleteMenu(id: number): Promise<void> {
  return request.delete(`/menu/${id}`)
}

export function syncMenuPermissions(): Promise<void> {
  return request.post('/menu/sync')
}