import request from '../../utils/request'

export interface FolderRecord {
  id: number
  name: string
  parentId: number
  sort: number
  createTime: string
  children: FolderRecord[]
}

export function getFolderTree(): Promise<FolderRecord[]> {
  return request.get('/folder/tree')
}

export function getFolder(id: number): Promise<FolderRecord> {
  return request.get(`/folder/${id}`)
}

export function addFolder(data: { name: string; parentId?: number; sort?: number }): Promise<void> {
  return request.post('/folder', data)
}

export function updateFolder(id: number, data: { name: string; sort?: number }): Promise<void> {
  return request.put(`/folder/${id}`, data)
}

export function deleteFolder(id: number): Promise<void> {
  return request.delete(`/folder/${id}`)
}