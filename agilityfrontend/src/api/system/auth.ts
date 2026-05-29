import request from '../../utils/request'
import type { LoginParams, LoginResult, InitData } from '../../types'

export function login(data: LoginParams): Promise<LoginResult> {
  return request.post('/auth/login', data)
}

export function init(): Promise<InitData> {
  return request.post('/auth/init')
}

export function logout(): Promise<void> {
  return request.post('/auth/logout')
}

export function refreshToken(token: string): Promise<{ token: string; refreshToken: string }> {
  return request.post('/auth/refresh-token', { refreshToken: token })
}

export function changePassword(data: { oldPassword: string; newPassword: string }): Promise<void> {
  return request.post('/auth/change-password', data)
}