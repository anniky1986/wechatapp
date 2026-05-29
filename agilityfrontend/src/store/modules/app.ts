import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useAppStore = defineStore('app', () => {
  const collapsed = ref(false)
  const theme = ref('light')
  const layoutMode = ref('side')
  const primaryColor = ref('#1677ff')

  function toggleCollapsed() {
    collapsed.value = !collapsed.value
  }

  function setTheme(mode: string) {
    theme.value = mode
  }

  return {
    collapsed,
    theme,
    layoutMode,
    primaryColor,
    toggleCollapsed,
    setTheme,
  }
})