import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { UserInfo, MenuItem, LoginParams, InitData } from '../../types'
import { login as loginApi, init as initApi, logout as logoutApi } from '../../api/system/auth'
import { setToken, removeToken, getToken } from '../../utils/storage'
import router from '../../router'
import { generateRoutes } from '../../router/generator'

export const useUserStore = defineStore('user', () => {
  const userInfo = ref<UserInfo | null>(null)
  const menus = ref<MenuItem[]>([])
  const permissions = ref<string[]>([])
  const systemConfig = ref<Record<string, unknown>>({})
  const dictData = ref<Record<string, unknown>>({})
  const token = ref<string | null>(getToken())

  const isLoggedIn = computed(() => !!token.value)

  function hasPermission(perm: string): boolean {
    return permissions.value.includes(perm)
  }

  async function login(params: LoginParams) {
    const res = await loginApi(params)
    setToken(res.token)
    token.value = res.token
    userInfo.value = res.userInfo
    menus.value = res.menus
    permissions.value = res.permissions
    await generateRoutes(res.menus)
    return res
  }

  async function init() {
    const res = await initApi()
    userInfo.value = res.userInfo
    menus.value = res.menus
    permissions.value = res.permissions
    systemConfig.value = res.systemConfig
    dictData.value = res.dictData
    return res
  }

  async function logout() {
    try {
      await logoutApi()
    } catch {
      // ignore logout error
    }
    removeToken()
    token.value = null
    userInfo.value = null
    menus.value = []
    permissions.value = []
    router.push('/login')
  }

  return {
    userInfo,
    menus,
    permissions,
    systemConfig,
    dictData,
    token,
    isLoggedIn,
    hasPermission,
    login,
    init,
    logout,
  }
})