<script setup lang="ts">
import { reactive, ref, shallowRef, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import type { TableColumnsType } from 'ant-design-vue'
import { ExportOutlined, SearchOutlined, ReloadOutlined } from '@ant-design/icons-vue'
import dayjs from 'dayjs'
import type { Dayjs } from 'dayjs'
import { useDebounceFn } from '@vueuse/core'
import type { PageResult } from '../../../types'
import {
  getOperationLogPage,
  exportOperationLog,
  type OperationLogRecord,
  type OperationLogQuery,
} from '../../../api/system/log'

const searchForm = reactive<OperationLogQuery>({
  page: 1,
  pageSize: 15,
  userName: '',
  module: '',
  startTime: undefined,
  endTime: undefined,
})

const dateRange = ref<[Dayjs, Dayjs] | null>(null)

const pagination = reactive({
  current: 1,
  pageSize: 15,
  total: 0,
  showSizeChanger: true,
  showQuickJumper: true,
  pageSizeOptions: ['10', '15', '20', '50'],
  showTotal: (total: number) => `共 ${total} 条记录`,
})

const tableData = ref<OperationLogRecord[]>([])
const tableLoading = shallowRef(false)

const httpMethodColorMap: Record<string, string> = {
  GET: 'blue',
  POST: 'green',
  PUT: 'orange',
  DELETE: 'red',
  PATCH: 'purple',
  HEAD: 'cyan',
  OPTIONS: 'geekblue',
}

const columns: TableColumnsType = [
  { title: '操作人员', dataIndex: 'userName', key: 'userName', width: 120 },
  { title: '所属模块', dataIndex: 'module', key: 'module', width: 120 },
  { title: '操作描述', dataIndex: 'action', key: 'action', width: 180, ellipsis: true },
  { title: '请求URL', dataIndex: 'requestUrl', key: 'requestUrl', width: 240, ellipsis: true },
  { title: '请求方式', dataIndex: 'requestMethod', key: 'requestMethod', width: 90, align: 'center' },
  { title: '操作IP', dataIndex: 'ip', key: 'ip', width: 140 },
  { title: '耗时(ms)', dataIndex: 'costTime', key: 'costTime', width: 100, align: 'center' },
  { title: '执行时间', dataIndex: 'createTime', key: 'createTime', width: 170 },
]

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
    const res: any = await getOperationLogPage({ ...searchForm })
    const data: PageResult<OperationLogRecord> = res.data ?? res
    tableData.value = data.items
    pagination.total = data.total
  } catch {
    message.error('获取操作日志失败')
  } finally {
    tableLoading.value = false
  }
}

function handleSearch() {
  pagination.current = 1
  fetchData()
}

const debouncedSearch = useDebounceFn(handleSearch, 300)

function handleReset() {
  searchForm.userName = ''
  searchForm.module = ''
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
    const res: any = await exportOperationLog({ ...searchForm })
    const blob = res instanceof Blob ? res : new Blob([res])
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `操作日志_${dayjs().format('YYYYMMDDHHmmss')}.xlsx`
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
  <div class="operation-log">
    <a-card v-memo="[searchForm.userName, searchForm.module]" class="search-card" :bordered="false">
      <a-form layout="inline" :model="searchForm">
        <a-form-item label="用户名">
          <a-input
            v-model:value="searchForm.userName"
            placeholder="请输入用户名"
            allow-clear
            style="width: 160px"
            @press-enter="handleSearch"
            @input="debouncedSearch"
          />
        </a-form-item>
        <a-form-item label="操作模块">
          <a-input
            v-model:value="searchForm.module"
            placeholder="请输入模块"
            allow-clear
            style="width: 160px"
            @press-enter="handleSearch"
            @input="debouncedSearch"
          />
        </a-form-item>
        <a-form-item label="操作时间">
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
        :scroll="{ x: 1200 }"
        row-key="id"
        @change="handlePageChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'requestUrl'">
            <a-tooltip :title="record.requestUrl" placement="topLeft">
              <span class="ellipsis-text">{{ record.requestUrl }}</span>
            </a-tooltip>
          </template>
          <template v-else-if="column.key === 'requestMethod'">
            <a-tag :color="httpMethodColorMap[record.requestMethod] || 'default'">
              {{ record.requestMethod }}
            </a-tag>
          </template>
          <template v-else-if="column.key === 'costTime'">
            <span :style="{ color: record.costTime > 1000 ? '#ff4d4f' : undefined }">
              {{ record.costTime }}
            </span>
          </template>
        </template>
      </a-table>
    </a-card>
  </div>
</template>

<style lang="less" scoped>
.operation-log {
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

  .ellipsis-text {
    display: block;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    max-width: 220px;
  }
}
</style>