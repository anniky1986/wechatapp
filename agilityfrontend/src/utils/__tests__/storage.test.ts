import { describe, it, expect, beforeEach } from 'vitest'
import {
  getToken,
  setToken,
  getRefreshToken,
  setRefreshToken,
  removeToken,
} from '../storage'

describe('storage', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  describe('setToken / getToken', () => {
    it('应该设置并获取 token', () => {
      setToken('test-token-123')
      expect(getToken()).toBe('test-token-123')
    })

    it('token 不存在时应该返回 null', () => {
      expect(getToken()).toBeNull()
    })

    it('应该覆盖已存在的 token', () => {
      setToken('first-token')
      setToken('second-token')
      expect(getToken()).toBe('second-token')
    })
  })

  describe('setRefreshToken / getRefreshToken', () => {
    it('应该设置并获取 refreshToken', () => {
      setRefreshToken('refresh-token-456')
      expect(getRefreshToken()).toBe('refresh-token-456')
    })

    it('refreshToken 不存在时应该返回 null', () => {
      expect(getRefreshToken()).toBeNull()
    })

    it('应该覆盖已存在的 refreshToken', () => {
      setRefreshToken('first-refresh')
      setRefreshToken('second-refresh')
      expect(getRefreshToken()).toBe('second-refresh')
    })
  })

  describe('removeToken', () => {
    it('应该同时清除 token 和 refreshToken', () => {
      setToken('token-value')
      setRefreshToken('refresh-value')

      removeToken()

      expect(getToken()).toBeNull()
      expect(getRefreshToken()).toBeNull()
    })

    it('当 token 不存在时调用不应报错', () => {
      expect(() => removeToken()).not.toThrow()
    })

    it('应该只清除 token 和 refreshToken，不清除其他数据', () => {
      localStorage.setItem('otherKey', 'otherValue')
      setToken('token-value')
      setRefreshToken('refresh-value')

      removeToken()

      expect(getToken()).toBeNull()
      expect(getRefreshToken()).toBeNull()
      expect(localStorage.getItem('otherKey')).toBe('otherValue')
    })
  })
})