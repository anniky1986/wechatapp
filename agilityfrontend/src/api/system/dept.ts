import request from '../../utils/request'

export interface DeptRecord {
  id: number
  parentId: number
  name: string
  sort: number
  leader: string
  phone: string
  email: string
  status: number
  createTime: string
  children: DeptRecord[]
}

export function getDeptTree(): Promise<DeptRecord[]> {
  return request.get('/dept/tree')
}

export function getDept(id: number): Promise<DeptRecord> {
  return request.get(`/dept/${id}`)
}

export function addDept(data: Partial<DeptRecord>): Promise<void> {
  return request.post('/dept', data)
}

export function updateDept(id: number, data: Partial<DeptRecord>): Promise<void> {
  return request.put(`/dept/${id}`, data)
}

export function deleteDept(id: number): Promise<void> {
  return request.delete(`/dept/${id}`)
}