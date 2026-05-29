import request from '../../utils/request'

export interface SystemSetting {
  id: number
  name: string
  key: string
  value: string
  group: string
  remark: string
}

export function getSettingList(): Promise<SystemSetting[]> {
  return request.get('/setting/list')
}

export function getSettingByGroup(group: string): Promise<SystemSetting[]> {
  return request.get('/setting/group', { params: { group } })
}

export function getSetting(key: string): Promise<SystemSetting> {
  return request.get(`/setting/${key}`)
}

export function saveSetting(data: { key: string; value: string; remark?: string }): Promise<void> {
  return request.post('/setting', data)
}

export function updateSetting(id: number, data: { value: string; remark?: string }): Promise<void> {
  return request.put(`/setting/${id}`, data)
}

export function batchUpdateSetting(data: { id: number; value: string }[]): Promise<void> {
  return request.put('/setting/batch', data)
}

export function deleteSetting(id: number): Promise<void> {
  return request.delete(`/setting/${id}`)
}