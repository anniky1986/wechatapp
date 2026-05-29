import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'

const {
  mockLoginApi,
  mockInitApi,
  mockLogoutApi,
  mockSetToken,
  mockRemoveToken,
  mockGenerateRoutes,
  mockRouterPush,
} = vi.hoisted(() => ({
  mockLoginApi: vi.fn(),
  mockInitApi: vi.fn(),
  mockLogoutApi: vi.fn(),
  mockSetToken: vi.fn(),
  mockRemoveToken: vi.fn(),
  mockGenerateRoutes: vi.fn(),
  mockRouterPush: vi.fn(),
}))

vi.mock('../../api/system/auth', () => ({
  login: mockLoginApi,
  init: mockInitApi,
  logout: mockLogoutApi,
}))

vi.mock('../../utils/storage', () => ({
  setToken: mockSetToken,
  removeToken: mockRemoveToken,
  getToken: () => 'existing-token',
}))

vi.mock('../../router', () => ({
  default: { push: mockRouterPush, getRoutes: vi.fn(() => []), addRoute: vi.fn() },
}))

vi.mock('../../router/generator', () => ({
  generateRoutes: mockGenerateRoutes,
}))

import { useUserStore } from '../../store/modules/user'

describe('userStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('login', () => {
    const mockLoginResult = {
      token: 'test-token',
      userInfo: {
        id: 1,
        userName: 'admin',
        nickName: 'Admin',
        avatar: '',
        email: 'admin@test.com',
        phone: '13800138000',
        deptId: 1,
        deptName: '技术部',
        roles: ['admin'],
        permissions: ['user:list'],
      },
      menus: [
        {
          id: 1,
          parentId: 0,
          name: 'Dashboard',
          path: '/dashboard',
          component: 'dashboard/Dashboard',
          redirect: '',
          icon: 'DashboardOutlined',
          permission: '',
          menuType: 2,
          orderNo: 1,
          isHide: false,
          keepAlive: false,
          status: 1,
          isFrame: false,
          frameSrc: '',
          children: [],
        },
      ],
      permissions: ['user:list', 'user:add'],
    }

    it('应该更新 token 并调用 setToken', async () => {
      mockLoginApi.mockResolvedValue(mockLoginResult)

      const store = useUserStore()
      await store.login({ userName: 'admin', password: '123456' })

      expect(mockSetToken).toHaveBeenCalledWith('test-token')
      expect(store.token).toBe('test-token')
    })

    it('应该更新 userInfo', async () => {
      mockLoginApi.mockResolvedValue(mockLoginResult)

      const store = useUserStore()
      await store.login({ userName: 'admin', password: '123456' })

      expect(store.userInfo).toEqual(mockLoginResult.userInfo)
    })

    it('应该更新 menus', async () => {
      mockLoginApi.mockResolvedValue(mockLoginResult)

      const store = useUserStore()
      await store.login({ userName: 'admin', password: '123456' })

      expect(store.menus).toEqual(mockLoginResult.menus)
    })

    it('应该更新 permissions', async () => {
      mockLoginApi.mockResolvedValue(mockLoginResult)

      const store = useUserStore()
      await store.login({ userName: 'admin', password: '123456' })

      expect(store.permissions).toEqual(mockLoginResult.permissions)
    })

    it('应该调用 generateRoutes', async () => {
      mockLoginApi.mockResolvedValue(mockLoginResult)

      const store = useUserStore()
      await store.login({ userName: 'admin', password: '123456' })

      expect(mockGenerateRoutes).toHaveBeenCalledWith(mockLoginResult.menus)
    })

    it('应该返回 login 结果', async () => {
      mockLoginApi.mockResolvedValue(mockLoginResult)

      const store = useUserStore()
      const result = await store.login({ userName: 'admin', password: '123456' })

      expect(result).toEqual(mockLoginResult)
    })
  })

  describe('logout', () => {
    it('应该清除 token', async () => {
      mockLogoutApi.mockResolvedValue(undefined)

      const store = useUserStore()
      store.token = 'some-token'
      store.userInfo = { id: 1, userName: 'admin', nickName: '', avatar: '', email: '', phone: '', deptId: 0, deptName: '', roles: [], permissions: [] }
      store.menus = [{ id: 1, parentId: 0, name: '', path: '', component: '', redirect: '', icon: '', permission: '', menuType: 0, orderNo: 0, isHide: false, keepAlive: false, status: 0, isFrame: false, frameSrc: '', children: [] }]
      store.permissions = ['perm1']

      await store.logout()

      expect(store.token).toBeNull()
    })

    it('应该清除 userInfo', async () => {
      mockLogoutApi.mockResolvedValue(undefined)

      const store = useUserStore()
      store.userInfo = { id: 1, userName: 'admin', nickName: '', avatar: '', email: '', phone: '', deptId: 0, deptName: '', roles: [], permissions: [] }

      await store.logout()

      expect(store.userInfo).toBeNull()
    })

    it('应该清除 menus', async () => {
      mockLogoutApi.mockResolvedValue(undefined)

      const store = useUserStore()
      store.menus = [{ id: 1, parentId: 0, name: '', path: '', component: '', redirect: '', icon: '', permission: '', menuType: 0, orderNo: 0, isHide: false, keepAlive: false, status: 0, isFrame: false, frameSrc: '', children: [] }]

      await store.logout()

      expect(store.menus).toEqual([])
    })

    it('应该清除 permissions', async () => {
      mockLogoutApi.mockResolvedValue(undefined)

      const store = useUserStore()
      store.permissions = ['perm1', 'perm2']

      await store.logout()

      expect(store.permissions).toEqual([])
    })

    it('应该调用 removeToken', async () => {
      mockLogoutApi.mockResolvedValue(undefined)

      const store = useUserStore()
      await store.logout()

      expect(mockRemoveToken).toHaveBeenCalled()
    })

    it('应该路由跳转到 /login', async () => {
      mockLogoutApi.mockResolvedValue(undefined)

      const store = useUserStore()
      await store.logout()

      expect(mockRouterPush).toHaveBeenCalledWith('/login')
    })

    it('logout API 失败时也应该清除状态', async () => {
      mockLogoutApi.mockRejectedValue(new Error('Network error'))

      const store = useUserStore()
      store.token = 'some-token'

      await store.logout()

      expect(store.token).toBeNull()
      expect(mockRemoveToken).toHaveBeenCalled()
      expect(mockRouterPush).toHaveBeenCalledWith('/login')
    })
  })

  describe('hasPermission', () => {
    it('权限存在时返回 true', () => {
      const store = useUserStore()
      store.permissions = ['user:list', 'user:add', 'user:delete']

      expect(store.hasPermission('user:list')).toBe(true)
    })

    it('权限不存在时返回 false', () => {
      const store = useUserStore()
      store.permissions = ['user:list', 'user:add']

      expect(store.hasPermission('user:delete')).toBe(false)
    })

    it('权限列表为空时返回 false', () => {
      const store = useUserStore()
      store.permissions = []

      expect(store.hasPermission('any')).toBe(false)
    })
  })

  describe('isLoggedIn', () => {
    it('有 token 时返回 true', () => {
      const store = useUserStore()
      store.token = 'some-token'

      expect(store.isLoggedIn).toBe(true)
    })

    it('没有 token 时返回 false', () => {
      const store = useUserStore()
      store.token = null

      expect(store.isLoggedIn).toBe(false)
    })
  })

  describe('init', () => {
    const mockInitData = {
      userInfo: {
        id: 1,
        userName: 'admin',
        nickName: 'Admin',
        avatar: '',
        email: '',
        phone: '',
        deptId: 1,
        deptName: '技术部',
        roles: ['admin'],
        permissions: [],
      },
      menus: [
        {
          id: 1,
          parentId: 0,
          name: 'Dashboard',
          path: '/dashboard',
          component: '',
          redirect: '',
          icon: '',
          permission: '',
          menuType: 2,
          orderNo: 1,
          isHide: false,
          keepAlive: false,
          status: 1,
          isFrame: false,
          frameSrc: '',
          children: [],
        },
      ],
      permissions: ['user:list'],
      systemConfig: { theme: 'light' },
      dictData: { status: [{ label: '启用', value: 1 }] },
    }

    it('应该填充 userInfo', async () => {
      mockInitApi.mockResolvedValue(mockInitData)

      const store = useUserStore()
      await store.init()

      expect(store.userInfo).toEqual(mockInitData.userInfo)
    })

    it('应该填充 menus', async () => {
      mockInitApi.mockResolvedValue(mockInitData)

      const store = useUserStore()
      await store.init()

      expect(store.menus).toEqual(mockInitData.menus)
    })

    it('应该填充 permissions', async () => {
      mockInitApi.mockResolvedValue(mockInitData)

      const store = useUserStore()
      await store.init()

      expect(store.permissions).toEqual(mockInitData.permissions)
    })

    it('应该填充 systemConfig', async () => {
      mockInitApi.mockResolvedValue(mockInitData)

      const store = useUserStore()
      await store.init()

      expect(store.systemConfig).toEqual(mockInitData.systemConfig)
    })

    it('应该填充 dictData', async () => {
      mockInitApi.mockResolvedValue(mockInitData)

      const store = useUserStore()
      await store.init()

      expect(store.dictData).toEqual(mockInitData.dictData)
    })

    it('应该返回 init 结果', async () => {
      mockInitApi.mockResolvedValue(mockInitData)

      const store = useUserStore()
      const result = await store.init()

      expect(result).toEqual(mockInitData)
    })
  })
})