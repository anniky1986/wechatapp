import request from '../../utils/request'
import type { PageParams, PageResult } from '../../types'

export interface OperationLogRecord {
  id: number
  userId: number
  userName: string
  module: string
  action: string
  target: string
  ip: string
  userAgent: string
  requestMethod: string
  requestUrl: string
  requestParams: string
  status: number
  errorMsg: string
  costTime: number
  createTime: string
}

export interface LoginLogRecord {
  id: number
  userId: number
  userName: string
  ip: string
  userAgent: string
  status: number
  message: string
  loginType: string
  createTime: string
}

export interface OperationLogQuery extends PageParams {
  userName?: string
  module?: string
  startTime?: string
  endTime?: string
  status?: number
}

export interface LoginLogQuery extends PageParams {
  userName?: string
  startTime?: string
  endTime?: string
  status?: number
}

export function getOperationLogPage(params: OperationLogQuery): Promise<PageResult<OperationLogRecord>> {
  return request.get('/log/operation/page', { params })
}

export function deleteOperationLog(ids: number[]): Promise<void> {
  return request.delete('/log/operation', { data: { ids } })
}

export function clearOperationLog(): Promise<void> {
  return request.delete('/log/operation/clear')
}

export function getLoginLogPage(params: LoginLogQuery): Promise<PageResult<LoginLogRecord>> {
  return request.get('/log/login/page', { params })
}

export function deleteLoginLog(ids: number[]): Promise<void> {
  return request.delete('/log/login', { data: { ids } })
}

export function clearLoginLog(): Promise<void> {
  return request.delete('/log/login/clear')
}

export function exportOperationLog(params: OperationLogQuery): Promise<void> {
  return request.get('/log/operation/export', { params, responseType: 'blob' })
}

export function exportLoginLog(params: LoginLogQuery): Promise<void> {
  return request.get('/log/login/export', { params, responseType: 'blob' })
}