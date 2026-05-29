<script setup lang="ts">
import { h } from 'vue'
import { useAppStore } from '../../store/modules/app'
import { useUserStore } from '../../store/modules/user'
import {
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  UserOutlined,
  LogoutOutlined,
  SettingOutlined,
} from '@ant-design/icons-vue'

const appStore = useAppStore()
const userStore = useUserStore()

function handleLogout() {
  userStore.logout()
}

function handleUserMenuClick(info: { key: string }) {
  if (info.key === 'logout') {
    handleLogout()
  }
}
</script>

<template>
  <div class="header-left">
    <span class="collapse-btn" @click="appStore.toggleCollapsed()">
      <MenuUnfoldOutlined v-if="appStore.collapsed" />
      <MenuFoldOutlined v-else />
    </span>
  </div>

  <div class="header-right">
    <a-dropdown>
      <span class="user-info">
        <a-avatar size="small">
          <template #icon>
            <UserOutlined />
          </template>
        </a-avatar>
        <span class="user-name">
          {{ userStore.userInfo?.nickName || userStore.userInfo?.userName || '用户' }}
        </span>
      </span>
      <template #overlay>
        <a-menu @click="handleUserMenuClick">
          <a-menu-item key="profile">
            <UserOutlined />
            <span style="margin-left: 8px">个人中心</span>
          </a-menu-item>
          <a-menu-item key="password">
            <SettingOutlined />
            <span style="margin-left: 8px">修改密码</span>
          </a-menu-item>
          <a-menu-divider />
          <a-menu-item key="logout">
            <LogoutOutlined />
            <span style="margin-left: 8px">退出登录</span>
          </a-menu-item>
        </a-menu>
      </template>
    </a-dropdown>
  </div>
</template>

<style lang="less" scoped>
.header-left {
  display: flex;
  align-items: center;
}

.collapse-btn {
  font-size: 18px;
  cursor: pointer;
  color: #333;
  padding: 4px;
  border-radius: 4px;
  transition: background-color 0.2s;

  &:hover {
    background-color: #f0f0f0;
  }
}

.header-right {
  display: flex;
  align-items: center;

  .user-info {
    display: flex;
    align-items: center;
    gap: 8px;
    cursor: pointer;
    padding: 4px 8px;
    border-radius: 4px;
    transition: background-color 0.2s;

    &:hover {
      background-color: #f0f0f0;
    }
  }

  .user-name {
    font-size: 14px;
    color: #333;
  }
}
</style>