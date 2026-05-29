<script setup lang="ts">
import { useUserStore } from '../../store/modules/user'
import {
  TeamOutlined,
  SafetyOutlined,
  FileTextOutlined,
  SettingOutlined,
} from '@ant-design/icons-vue'
import { h, type VNode } from 'vue'

const userStore = useUserStore()

interface StatCard {
  title: string
  value: string
  icon: () => VNode
  color: string
  bgColor: string
}

const stats: StatCard[] = [
  {
    title: '用户数',
    value: '1,280',
    icon: () => h(TeamOutlined),
    color: '#1677ff',
    bgColor: '#e6f4ff',
  },
  {
    title: '角色数',
    value: '12',
    icon: () => h(SafetyOutlined),
    color: '#52c41a',
    bgColor: '#f6ffed',
  },
  {
    title: '菜单数',
    value: '45',
    icon: () => h(FileTextOutlined),
    color: '#fa8c16',
    bgColor: '#fff7e6',
  },
  {
    title: '系统设置',
    value: '8',
    icon: () => h(SettingOutlined),
    color: '#722ed1',
    bgColor: '#f9f0ff',
  },
]
</script>

<template>
  <div class="dashboard">
    <div class="dashboard-header">
      <h2 class="welcome-text">
        欢迎回来，{{ userStore.userInfo?.nickName || userStore.userInfo?.userName || '管理员' }}
      </h2>
      <p class="welcome-desc">以下是系统运行概况</p>
    </div>

    <a-row :gutter="[16, 16]">
      <a-col v-for="(stat, index) in stats" :key="index" :xs="24" :sm="12" :lg="6">
        <a-card class="stat-card" hoverable>
          <div class="stat-content">
            <div class="stat-info">
              <span class="stat-title">{{ stat.title }}</span>
              <span class="stat-value">{{ stat.value }}</span>
            </div>
            <div
              class="stat-icon"
              :style="{ color: stat.color, backgroundColor: stat.bgColor }"
            >
              <component :is="stat.icon" />
            </div>
          </div>
        </a-card>
      </a-col>
    </a-row>

    <a-row :gutter="[16, 16]" style="margin-top: 16px">
      <a-col :xs="24" :lg="16">
        <a-card title="系统公告" hoverable>
          <a-empty description="暂无公告" />
        </a-card>
      </a-col>
      <a-col :xs="24" :lg="8">
        <a-card title="快捷操作" hoverable>
          <a-space direction="vertical" style="width: 100%">
            <a-button type="primary" block ghost>用户管理</a-button>
            <a-button block ghost>角色管理</a-button>
            <a-button block ghost>菜单管理</a-button>
          </a-space>
        </a-card>
      </a-col>
    </a-row>
  </div>
</template>

<style lang="less" scoped>
.dashboard {
  .dashboard-header {
    margin-bottom: 24px;

    .welcome-text {
      font-size: 22px;
      font-weight: 600;
      color: #1a1a1a;
      margin: 0 0 8px;
    }

    .welcome-desc {
      font-size: 14px;
      color: #666;
      margin: 0;
    }
  }

  .stat-card {
    border-radius: 8px;
  }

  .stat-content {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  .stat-info {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .stat-title {
    font-size: 14px;
    color: #666;
  }

  .stat-value {
    font-size: 28px;
    font-weight: 700;
    color: #1a1a1a;
  }

  .stat-icon {
    width: 56px;
    height: 56px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 24px;
  }
}
</style>