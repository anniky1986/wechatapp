import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAppStore } from '../../store/modules/app'

describe('appStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  describe('默认值', () => {
    it('collapsed 默认值为 false', () => {
      const store = useAppStore()
      expect(store.collapsed).toBe(false)
    })

    it('theme 默认值为 light', () => {
      const store = useAppStore()
      expect(store.theme).toBe('light')
    })

    it('layoutMode 默认值为 side', () => {
      const store = useAppStore()
      expect(store.layoutMode).toBe('side')
    })

    it('primaryColor 默认值为 #1677ff', () => {
      const store = useAppStore()
      expect(store.primaryColor).toBe('#1677ff')
    })
  })

  describe('toggleCollapsed', () => {
    it('应该将 collapsed 从 false 切换为 true', () => {
      const store = useAppStore()
      expect(store.collapsed).toBe(false)
      store.toggleCollapsed()
      expect(store.collapsed).toBe(true)
    })

    it('应该将 collapsed 从 true 切换为 false', () => {
      const store = useAppStore()
      store.collapsed = true
      store.toggleCollapsed()
      expect(store.collapsed).toBe(false)
    })

    it('多次调用应该正确切换', () => {
      const store = useAppStore()
      store.toggleCollapsed()
      expect(store.collapsed).toBe(true)
      store.toggleCollapsed()
      expect(store.collapsed).toBe(false)
      store.toggleCollapsed()
      expect(store.collapsed).toBe(true)
    })
  })

  describe('setTheme', () => {
    it('应该将 theme 设置为 dark', () => {
      const store = useAppStore()
      store.setTheme('dark')
      expect(store.theme).toBe('dark')
    })

    it('应该将 theme 设置为 light', () => {
      const store = useAppStore()
      store.theme = 'dark'
      store.setTheme('light')
      expect(store.theme).toBe('light')
    })
  })
})