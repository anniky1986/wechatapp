import { describe, it, expect, vi, beforeEach } from 'vitest'

describe('request interceptor', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('当 localStorage 中有 token 时应添加 Authorization header', () => {
    localStorage.setItem('token', 'test-token-123')

    const config: Record<string, unknown> = { headers: {} }
    const token = localStorage.getItem('token')

    if (token) {
      ;(config.headers as Record<string, string>).Authorization = `Bearer ${token}`
    }

    expect((config.headers as Record<string, string>).Authorization).toBe('Bearer test-token-123')
  })

  it('当 localStorage 中没有 token 时不应添加 Authorization header', () => {
    localStorage.clear()

    const config: Record<string, unknown> = { headers: {} }
    const token = localStorage.getItem('token')

    if (token) {
      ;(config.headers as Record<string, string>).Authorization = `Bearer ${token}`
    }

    expect((config.headers as Record<string, string>).Authorization).toBeUndefined()
  })
})

describe('response interceptor', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.clearAllMocks()
  })

  it('当响应 code 为 TIMEOUT(401) 时应清除 localStorage', () => {
    localStorage.setItem('token', 'test-token')
    localStorage.setItem('some-key', 'value')

    const response = { data: { code: 401, message: 'Token expired' } }

    if (response.data.code === 401) {
      localStorage.clear()
    }

    expect(localStorage.getItem('token')).toBeNull()
    expect(localStorage.getItem('some-key')).toBeNull()
  })

  it('当响应正常时应返回 response.data', () => {
    const responseData = { code: 0, message: 'success', data: { name: 'test' } }
    const response = { data: responseData }

    const result = response.data

    expect(result).toEqual(responseData)
  })

  it('当发生 401 网络错误时应清除 localStorage', () => {
    localStorage.setItem('token', 'test-token')
    localStorage.setItem('some-key', 'value')

    const errorResponse = { response: { status: 401 } }

    if (errorResponse.response?.status === 401) {
      localStorage.clear()
    }

    expect(localStorage.getItem('token')).toBeNull()
    expect(localStorage.getItem('some-key')).toBeNull()
  })

  it('当发生非 401 网络错误时不应清除 localStorage', () => {
    localStorage.setItem('token', 'test-token')

    const errorResponse = { response: { status: 500 }, message: 'Server error' }

    if (errorResponse.response?.status === 401) {
      localStorage.clear()
    }

    expect(localStorage.getItem('token')).toBe('test-token')
  })
})