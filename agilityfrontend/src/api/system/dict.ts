import request from '../../utils/request'
import type { PageParams, PageResult } from '../../types'

export interface DictTypeRecord {
  id: number
  name: string
  type: string
  status: number
  remark: string
  createTime: string
}

export interface DictItemRecord {
  id: number
  dictType: string
  label: string
  value: string
  sort: number
  status: number
  className: string
  createTime: string
}

export interface DictTypeQuery extends PageParams {
  name?: string
  type?: string
  status?: number
}

export interface DictItemQuery extends PageParams {
  dictType: string
  label?: string
  status?: number
}

export function getDictTypePage(params: DictTypeQuery): Promise<PageResult<DictTypeRecord>> {
  return request.get('/dict/type/page', { params })
}

export function getDictTypeList(): Promise<DictTypeRecord[]> {
  return request.get('/dict/type/list')
}

export function getDictType(id: number): Promise<DictTypeRecord> {
  return request.get(`/dict/type/${id}`)
}

export function addDictType(data: Partial<DictTypeRecord>): Promise<void> {
  return request.post('/dict/type', data)
}

export function updateDictType(id: number, data: Partial<DictTypeRecord>): Promise<void> {
  return request.put(`/dict/type/${id}`, data)
}

export function deleteDictType(id: number): Promise<void> {
  return request.delete(`/dict/type/${id}`)
}

export function getDictItemPage(params: DictItemQuery): Promise<PageResult<DictItemRecord>> {
  return request.get('/dict/item/page', { params })
}

export function getDictItems(dictType: string): Promise<DictItemRecord[]> {
  return request.get(`/dict/item/list/${dictType}`)
}

export function getDictItem(id: number): Promise<DictItemRecord> {
  return request.get(`/dict/item/${id}`)
}

export function addDictItem(data: Partial<DictItemRecord>): Promise<void> {
  return request.post('/dict/item', data)
}

export function updateDictItem(id: number, data: Partial<DictItemRecord>): Promise<void> {
  return request.put(`/dict/item/${id}`, data)
}

export function deleteDictItem(id: number): Promise<void> {
  return request.delete(`/dict/item/${id}`)
}