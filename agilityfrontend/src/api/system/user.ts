import request from '../../utils/request'
import type { PageParams, PageResult } from '../../types'

export interface UserRecord {
  id: number
  userName: string
  nickName: string
  email: string
  phone: string
  avatar: string
  deptId: number
  deptName: string
  status: number
  roles: string[]
  createTime: string
  lastLoginTime: string
}

export interface UserQuery extends PageParams {
  userName?: string
  phone?: string
  status?: number
  deptId?: number
}

export function getUserPage(params: UserQuery): Promise<PageResult<UserRecord>> {
  return request.get('/user/page', { params })
}

export function getUser(id: number): Promise<UserRecord> {
  return request.get(`/user/${id}`)
}

export function addUser(data: Partial<UserRecord>): Promise<void> {
  return request.post('/user', data)
}

export function updateUser(id: number, data: Partial<UserRecord>): Promise<void> {
  return request.put(`/user/${id}`, data)
}

export function deleteUser(id: number): Promise<void> {
  return request.delete(`/user/${id}`)
}

export function setUserStatus(id: number, status: number): Promise<void> {
  return request.put(`/user/${id}/status`, { status })
}

export function resetPassword(id: number, password: string): Promise<void> {
  return request.put(`/user/${id}/reset-password`, { password })
}