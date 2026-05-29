<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router'
import { useUserStore } from '../../store/modules/user'
import { computed } from 'vue'
import type { MenuItem } from '../../types'
import * as Icons from '@ant-design/icons-vue'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()

const menus = computed(() => userStore.menus)

const selectedKeys = computed(() => [route.path])
const openKeys = computed(() => {
  const matched = route.matched
  return matched.slice(0, -1).map((r) => r.path)
})

function renderIcon(iconName: string | undefined) {
  if (!iconName) return undefined
  const IconComponent = (Icons as Record<string, unknown>)[iconName]
  return IconComponent || undefined
}

function filterMenuItems(items: MenuItem[]): MenuItem[] {
  return items
    .filter((item) => !item.isHide && item.menuType !== 3)
    .map((item) => ({
      ...item,
      children: item.children ? filterMenuItems(item.children) : [],
    }))
    .filter((item) => item.menuType !== 1 || (item.children && item.children.length > 0))
}

const filteredMenus = computed(() => filterMenuItems(menus.value))

function handleMenuClick(info: { key: string }) {
  router.push(info.key)
}
</script>

<template>
  <a-menu
    mode="inline"
    theme="dark"
    :selected-keys="selectedKeys"
    :open-keys="openKeys"
    :inline-collapsed="false"
    @click="handleMenuClick"
  >
    <template v-for="menu in filteredMenus" :key="menu.id">
      <a-sub-menu v-if="menu.children && menu.children.length > 0" :key="menu.path">
        <template #icon>
          <component :is="renderIcon(menu.icon)" v-if="menu.icon" />
        </template>
        <template #title>
          <span>{{ menu.name }}</span>
        </template>
        <a-menu-item
          v-for="child in menu.children"
          :key="child.path"
        >
          <template #icon>
            <component :is="renderIcon(child.icon)" v-if="child.icon" />
          </template>
          <span>{{ child.name }}</span>
        </a-menu-item>
      </a-sub-menu>
      <a-menu-item v-else :key="menu.path">
        <template #icon>
          <component :is="renderIcon(menu.icon)" v-if="menu.icon" />
        </template>
        <span>{{ menu.name }}</span>
      </a-menu-item>
    </template>
  </a-menu>
</template>