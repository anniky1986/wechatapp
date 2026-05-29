<script setup lang="ts">
import { useAppStore } from '../store/modules/app'
import Sidebar from './components/Sidebar.vue'
import HeaderBar from './components/Header.vue'

const appStore = useAppStore()
</script>

<template>
  <a-layout style="min-height: 100vh">
    <a-layout-sider
      v-model:collapsed="appStore.collapsed"
      collapsible
      :trigger="null"
      theme="dark"
      :width="220"
      :collapsed-width="80"
    >
      <div class="logo">
        <span v-if="!appStore.collapsed" class="logo-text">Agility Admin</span>
        <span v-else class="logo-text-collapsed">A</span>
      </div>
      <Sidebar />
    </a-layout-sider>
    <a-layout>
      <a-layout-header class="layout-header">
        <HeaderBar />
      </a-layout-header>
      <a-layout-content class="layout-content">
        <router-view />
      </a-layout-content>
    </a-layout>
  </a-layout>
</template>

<style lang="less" scoped>
.logo {
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.logo-text {
  font-size: 18px;
  font-weight: 700;
  color: #fff;
  white-space: nowrap;
}

.logo-text-collapsed {
  font-size: 22px;
  font-weight: 700;
  color: #fff;
}

.layout-header {
  background: #fff;
  padding: 0 24px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
  z-index: 10;
  height: 64px;
  line-height: 64px;
}

.layout-content {
  margin: 16px;
  padding: 16px;
  background: #fff;
  border-radius: 6px;
  min-height: 280px;
}
</style>