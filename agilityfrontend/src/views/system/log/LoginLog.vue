<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import type { TableColumnsType } from 'ant-design-vue'
import { ExportOutlined, SearchOutlined, ReloadOutlined } from '@ant-design/icons-vue'
import dayjs from 'dayjs'
import type { Dayjs } from 'dayjs'
import type { PageResult } from '../../../types'
import {
  getLoginLogPage,
  exportLoginLog,
  type LoginLogRecord,
  type LoginLogQuery,
} from '../../../api/system/log'

const searchForm = reactive<LoginLogQuery>({
  page: 1,
  pageSize: 15,
  userName: '',
  status: undefined,
  startTime: undefined,
  endTime: undefined,
})

const dateRange = ref<[Dayjs, Dayjs] | null>(null)

const statusOptions = [
  { label: '成功', value: 1 },
  { label: '失败', value: 0 },
]

const loginTypeMap: Record<string, { color: string; text: string }> = {
  password: { color: 'blue', text: '密码' },
  sms: { color: 'purple', text: '短信' },
  third_party: { color: 'cyan', text: '第三方' },
  oauth2: { color: 'geekblue', text: 'OAuth2' },
  ldap: { color: 'orange', text: 'LDAP' },
}

const statusMap: Record<number, { color: string; text: string }> = {
  0: { color: 'error', text: '失败' },
  1: { color: 'success', text: '成功' },
}

const pagination = reactive({
  current: 1,
  pageSize: 15,
  total: 0,
  showSizeChanger: true,
  showQuickJumper: true,
  pageSizeOptions: ['10', '15', '20', '50'],
  showTotal: (total: number) => `共 ${total} 条记录`,
})

const tableData = ref<LoginLogRecord[]>([])
const tableLoading = ref(false)

const columns: TableColumnsType = [
  { title: '用户名', dataIndex: 'userName', key: 'userName', width: 120 },
  { title: '登录类型', dataIndex: 'loginType', key: 'loginType', width: 100, align: 'center' },
  { title: '登录状态', dataIndex: 'status', key: 'status', width: 100, align: 'center' },
  { title: '登录信息', dataIndex: 'message', key: 'message', width: 200, ellipsis: true },
  { title: 'IP地址', dataIndex: 'ip', key: 'ip', width: 140 },
  { title: 'User-Agent', dataIndex: 'userAgent', key: 'userAgent', width: 240, ellipsis: true },
  { title: '登录时间', dataIndex: 'createTime', key: 'createTime', width: 170 },
]

function truncateText(text: string | undefined, max: number): string {
  if (!text) return '-'
  return text.length > max ? text.substring(0, max) + '...' : text
}

function getLoginTypeTag(type: string | undefined) {
  if (!type) return { color: 'default', text: '-' }
  const lower = type.toLowerCase()
  return loginTypeMap[lower] || { color: 'default', text: type }
}

async function fetchData() {
  tableLoading.value = true
  try {
    searchForm.page = pagination.current
    searchForm.pageSize = pagination.pageSize
    if (dateRange.value) {
      searchForm.startTime = dateRange.value[0].format('YYYY-MM-DD HH:mm:ss')
      searchForm.endTime = dateRange.value[1].format('YYYY-MM-DD HH:mm:ss')
    } else {
      searchForm.startTime = undefined
      searchForm.endTime = undefined
    }
    const res: any = await getLoginLogPage({ ...searchForm })
    const data: PageResult<LoginLogRecord> = res.data ?? res
    tableData.value = data.items
    pagination.total = data.total
  } catch {
    message.error('获取登录日志失败')
  } finally {
    tableLoading.value = false
  }
}

function handleSearch() {
  pagination.current = 1
  fetchData()
}

function handleReset() {
  searchForm.userName = ''
  searchForm.status = undefined
  dateRange.value = null
  pagination.current = 1
  fetchData()
}

function handlePageChange(page: number, pageSize: number) {
  pagination.current = page
  pagination.pageSize = pageSize
  fetchData()
}

async function handleExport() {
  try {
    if (dateRange.value) {
      searchForm.startTime = dateRange.value[0].format('YYYY-MM-DD HH:mm:ss')
      searchForm.endTime = dateRange.value[1].format('YYYY-MM-DD HH:mm:ss')
    } else {
      searchForm.startTime = undefined
      searchForm.endTime = undefined
    }
    searchForm.page = 1
    searchForm.pageSize = 999999
    const res: any = await exportLoginLog({ ...searchForm })
    const blob = res instanceof Blob ? res : new Blob([res])
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `登录日志_${dayjs().format('YYYYMMDDHHmmss')}.xlsx`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
    message.success('导出成功')
  } catch {
    message.error('导出失败')
  }
}

const datePresets = ref([
  { label: '今天', value: [dayjs().startOf('day'), dayjs().endOf('day')] },
  { label: '昨天', value: [dayjs().subtract(1, 'day').startOf('day'), dayjs().subtract(1, 'day').endOf('day')] },
  { label: '最近7天', value: [dayjs().subtract(7, 'day').startOf('day'), dayjs().endOf('day')] },
  { label: '最近30天', value: [dayjs().subtract(30, 'day').startOf('day'), dayjs().endOf('day')] },
])

onMounted(() => {
  fetchData()
})
</script>

<template>
  <div class="login-log">
    <a-card class="search-card" :bordered="false">
      <a-form layout="inline" :model="searchForm">
        <a-form-item label="用户名">
          <a-input
            v-model:value="searchForm.userName"
            placeholder="请输入用户名"
            allow-clear
            style="width: 160px"
            @press-enter="handleSearch"
          />
        </a-form-item>
        <a-form-item label="登录状态">
          <a-select
            v-model:value="searchForm.status"
            placeholder="请选择状态"
            allow-clear
            style="width: 130px"
            :options="statusOptions"
          />
        </a-form-item>
        <a-form-item label="登录时间">
          <a-range-picker
            v-model:value="dateRange"
            :presets="datePresets"
            show-time
            format="YYYY-MM-DD HH:mm:ss"
            :placeholder="['开始时间', '结束时间']"
            style="width: 360px"
          />
        </a-form-item>
        <a-form-item>
          <a-space>
            <a-button type="primary" @click="handleSearch">
              <template #icon>
                <SearchOutlined />
              </template>
              查询
            </a-button>
            <a-button @click="handleReset">
              <template #icon>
                <ReloadOutlined />
              </template>
              重置
            </a-button>
          </a-space>
        </a-form-item>
      </a-form>
    </a-card>

    <a-card class="table-card" :bordered="false">
      <div class="toolbar">
        <a-button @click="handleExport">
          <template #icon>
            <ExportOutlined />
          </template>
          导出
        </a-button>
      </div>

      <a-table
        :columns="columns"
        :data-source="tableData"
        :loading="tableLoading"
        :pagination="pagination"
        :scroll="{ x: 1100 }"
        row-key="id"
        @change="handlePageChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'loginType'">
            <a-tag :color="getLoginTypeTag(record.loginType).color">
              {{ getLoginTypeTag(record.loginType).text }}
            </a-tag>
          </template>
          <template v-else-if="column.key === 'status'">
            <a-tag :color="statusMap[record.status]?.color">
              {{ statusMap[record.status]?.text }}
            </a-tag>
          </template>
          <template v-else-if="column.key === 'userAgent'">
            <a-tooltip :title="record.userAgent || ''" placement="topLeft">
              <span>{{ truncateText(record.userAgent, 40) }}</span>
            </a-tooltip>
          </template>
        </template>
      </a-table>
    </a-card>
  </div>
</template>

<style lang="less" scoped>
.login-log {
  padding: 16px;

  .search-card {
    margin-bottom: 16px;

    :deep(.ant-form-item) {
      margin-bottom: 12px;
    }
  }

  .table-card {
    .toolbar {
      margin-bottom: 16px;
    }
  }
}
</style>