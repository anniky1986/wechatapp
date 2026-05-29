import { describe, it, expect } from 'vitest'
import type { PageParams, LoginParams, ApiResponse } from '../index'

describe('types', () => {
  describe('PageParams', () => {
    it('PageParams 应有 page 和 pageSize 属性', () => {
      const params: PageParams = { page: 1, pageSize: 10 }
      expect(params.page).toBe(1)
      expect(params.pageSize).toBe(10)
    })

    it('PageParams 的 sortField 和 sortOrder 应为可选', () => {
      const params: PageParams = { page: 1, pageSize: 10, sortField: 'name', sortOrder: 'asc' }
      expect(params.sortField).toBe('name')
      expect(params.sortOrder).toBe('asc')
    })

    it('PageParams 可以省略可选字段', () => {
      const params: PageParams = { page: 2, pageSize: 20 }
      expect(params.sortField).toBeUndefined()
      expect(params.sortOrder).toBeUndefined()
    })
  })

  describe('LoginParams', () => {
    it('LoginParams 应有 userName 和 password 属性', () => {
      const params: LoginParams = { userName: 'admin', password: '123456' }
      expect(params.userName).toBe('admin')
      expect(params.password).toBe('123456')
    })
  })

  describe('ApiResponse', () => {
    it('ApiResponse 应有 code, message, data, timestamp', () => {
      const response: ApiResponse<string> = {
        code: 0,
        message: 'success',
        data: 'test',
        timestamp: Date.now(),
      }
      expect(response.code).toBe(0)
      expect(response.message).toBe('success')
      expect(response.data).toBe('test')
      expect(typeof response.timestamp).toBe('number')
    })

    it('ApiResponse 泛型应支持复杂类型', () => {
      interface User { id: number; name: string }
      const response: ApiResponse<User> = {
        code: 0,
        message: 'ok',
        data: { id: 1, name: 'admin' },
        timestamp: 1234567890,
      }
      expect(response.data.id).toBe(1)
      expect(response.data.name).toBe('admin')
    })

    it('ApiResponse 不传泛型时 T 默认为 unknown', () => {
      const response: ApiResponse = {
        code: -1,
        message: 'error',
        data: { anything: true },
        timestamp: 0,
      }
      expect(response.code).toBe(-1)
    })
  })
})