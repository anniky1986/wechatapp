<script setup lang="ts">
import { reactive, ref, onMounted, watch } from 'vue'
import { message } from 'ant-design-vue'
import type { Rule } from 'ant-design-vue/es/form'
import type { TableColumnsType } from 'ant-design-vue'
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
} from '@ant-design/icons-vue'
import dayjs from 'dayjs'
import { useUserStore } from '../../../store/modules/user'
import type { PageResult } from '../../../types'
import type { TenantRecord, TenantQuery } from '../../../api/system/tenant'
import {
  getTenantPage,
  addTenant,
  updateTenant,
  deleteTenant,
  setTenantStatus,
} from '../../../api/system/tenant'

interface TenantRow extends TenantRecord {
  tenantType?: number
  dbConnection?: string
}

const userStore = useUserStore()

const canAdd = userStore.hasPermission?.('system:tenant:add') ?? true
const canEdit = userStore.hasPermission?.('system:tenant:edit') ?? true
const canDelete = userStore.hasPermission?.('system:tenant:delete') ?? true

const tenantTypeOptions = [
  { label: 'ID隔离', value: 1 },
  { label: '数据库隔离', value: 2 },
]

const tenantTypeMap: Record<number, { color: string; text: string }> = {
  1: { color: 'blue', text: 'ID隔离' },
  2: { color: 'purple', text: '数据库隔离' },
}

const statusOptions = [
  { label: '启用', value: 1 },
  { label: '禁用', value: 0 },
]

const statusMap: Record<number, { color: string; text: string }> = {
  0: { color: 'error', text: '禁用' },
  1: { color: 'success', text: '启用' },
}

const searchForm = reactive({
  name: '',
  code: '',
  tenantType: undefined as number | undefined,
  status: undefined as number | undefined,
})

const pagination = reactive({
  current: 1,
  pageSize: 15,
  total: 0,
  showSizeChanger: true,
  showQuickJumper: true,
  pageSizeOptions: ['10', '15', '20', '50'],
  showTotal: (total: number) => `共 ${total} 条记录`,
})

const tableData = ref<TenantRow[]>([])
const tableLoading = ref(false)

const columns: TableColumnsType = [
  { title: '租户名称', dataIndex: 'name', key: 'name', width: 160 },
  { title: '租户编码', dataIndex: 'code', key: 'code', width: 160 },
  { title: '租户类型', dataIndex: 'tenantType', key: 'tenantType', width: 120, align: 'center' },
  { title: '数据库连接', dataIndex: 'dbConnection', key: 'dbConnection', width: 200, ellipsis: true },
  { title: '状态', dataIndex: 'status', key: 'status', width: 160, align: 'center' },
  { title: '过期时间', dataIndex: 'expireTime', key: 'expireTime', width: 170 },
  { title: '创建时间', dataIndex: 'createTime', key: 'createTime', width: 170 },
  { title: '操作', key: 'action', width: 180, fixed: 'right' },
]

async function fetchData() {
  tableLoading.value = true
  try {
    const query: TenantQuery = {
      page: pagination.current,
      pageSize: pagination.pageSize,
      name: searchForm.name || undefined,
      code: searchForm.code || undefined,
      status: searchForm.status,
    }
    const res: any = await getTenantPage(query)
    const data: PageResult<TenantRow> = res.data ?? res
    tableData.value = data.items
    pagination.total = data.total
  } catch {
    message.error('获取租户列表失败')
  } finally {
    tableLoading.value = false
  }
}

function handleSearch() {
  pagination.current = 1
  fetchData()
}

function handleReset() {
  searchForm.name = ''
  searchForm.code = ''
  searchForm.tenantType = undefined
  searchForm.status = undefined
  pagination.current = 1
  fetchData()
}

function handlePageChange(page: number, pageSize: number) {
  pagination.current = page
  pagination.pageSize = pageSize
  fetchData()
}

function maskDbConnection(str: string): string {
  if (!str || str.length <= 15) return str || '-'
  return str.substring(0, 12) + '...'
}

async function handleStatusChange(checked: boolean, record: TenantRow) {
  const newStatus = checked ? 1 : 0
  try {
    await setTenantStatus(record.id, newStatus)
    message.success(`${checked ? '启用' : '禁用'}成功`)
    record.status = newStatus
  } catch {
    message.error('状态修改失败')
  }
}

async function handleDelete(record: TenantRow) {
  try {
    await deleteTenant(record.id)
    message.success('删除成功')
    fetchData()
  } catch {
    message.error('删除失败')
  }
}

const formVisible = ref(false)
const formTitle = ref('新增租户')
const formRef = ref()
const formLoading = ref(false)
const isEdit = ref(false)
const currentTenantId = ref<number | null>(null)

const formState = reactive({
  name: '',
  code: '',
  tenantType: 1,
  dbConnection: '',
  expireTime: '' as string,
  status: true as boolean,
})

const formRules: Record<string, Rule[]> = {
  name: [
    { required: true, message: '请输入租户名称', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
  code: [
    { required: true, message: '请输入租户编码', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
  dbConnection: [
    { required: true, message: '请输入数据库连接串', trigger: 'blur' },
  ],
}

watch(
  () => formState.tenantType,
  (val) => {
    if (val !== 2) {
      formState.dbConnection = ''
    }
  },
)

function resetForm() {
  formState.name = ''
  formState.code = ''
  formState.tenantType = 1
  formState.dbConnection = ''
  formState.expireTime = ''
  formState.status = true
  currentTenantId.value = null
  formRef.value?.clearValidate()
}

function handleAdd() {
  isEdit.value = false
  formTitle.value = '新增租户'
  resetForm()
  formVisible.value = true
}

function handleEdit(record: TenantRow) {
  isEdit.value = true
  formTitle.value = '编辑租户'
  resetForm()
  currentTenantId.value = record.id
  formState.name = record.name
  formState.code = record.code
  formState.tenantType = record.tenantType ?? 1
  formState.dbConnection = record.dbConnection ?? ''
  formState.expireTime = record.expireTime ?? ''
  formState.status = record.status === 1
  formVisible.value = true
}

async function handleFormSubmit() {
  try {
    await formRef.value?.validate()
  } catch {
    return
  }
  formLoading.value = true
  try {
    const payload: Record<string, any> = {
      name: formState.name,
      code: formState.code,
      tenantType: formState.tenantType,
      status: formState.status ? 1 : 0,
      expireTime: formState.expireTime || undefined,
    }
    if (formState.tenantType === 2) {
      payload.dbConnection = formState.dbConnection
    }
    if (!isEdit.value) {
      await addTenant(payload)
      message.success('新增租户成功')
    } else {
      await updateTenant(currentTenantId.value!, payload)
      message.success('编辑租户成功')
    }
    formVisible.value = false
    fetchData()
  } catch {
    message.error(isEdit.value ? '编辑租户失败' : '新增租户失败')
  } finally {
    formLoading.value = false
  }
}

function getFormRules(): Record<string, Rule[]> {
  const rules: Record<string, Rule[]> = { ...formRules }
  if (formState.tenantType !== 2) {
    delete rules.dbConnection
  }
  return rules
}

onMounted(() => {
  fetchData()
})
</script>

<template>
  <div class="tenant-list">
    <a-card class="search-card" :bordered="false">
      <a-form layout="inline" :model="searchForm">
        <a-form-item label="租户名称">
          <a-input
            v-model:value="searchForm.name"
            placeholder="请输入租户名称"
            allow-clear
            style="width: 180px"
            @press-enter="handleSearch"
          />
        </a-form-item>
        <a-form-item label="租户编码">
          <a-input
            v-model:value="searchForm.code"
            placeholder="请输入租户编码"
            allow-clear
            style="width: 180px"
            @press-enter="handleSearch"
          />
        </a-form-item>
        <a-form-item label="租户类型">
          <a-select
            v-model:value="searchForm.tenantType"
            placeholder="请选择租户类型"
            allow-clear
            style="width: 150px"
            :options="tenantTypeOptions"
          />
        </a-form-item>
        <a-form-item label="状态">
          <a-select
            v-model:value="searchForm.status"
            placeholder="请选择状态"
            allow-clear
            style="width: 140px"
            :options="statusOptions"
          />
        </a-form-item>
        <a-form-item>
          <a-space>
            <a-button type="primary" @click="handleSearch">查询</a-button>
            <a-button @click="handleReset">重置</a-button>
          </a-space>
        </a-form-item>
      </a-form>
    </a-card>

    <a-card class="table-card" :bordered="false">
      <div class="toolbar">
        <a-button v-if="canAdd" type="primary" @click="handleAdd">
          <template #icon>
            <PlusOutlined />
          </template>
          新增
        </a-button>
      </div>

      <a-table
        :columns="columns"
        :data-source="tableData"
        :loading="tableLoading"
        :pagination="pagination"
        :scroll="{ x: 1300 }"
        row-key="id"
        @change="handlePageChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'tenantType'">
            <a-tag :color="tenantTypeMap[record.tenantType]?.color">
              {{ tenantTypeMap[record.tenantType]?.text ?? record.tenantType ?? '-' }}
            </a-tag>
          </template>
          <template v-else-if="column.key === 'dbConnection'">
            <a-tooltip :title="record.dbConnection">
              <span>{{ maskDbConnection(record.dbConnection) }}</span>
            </a-tooltip>
          </template>
          <template v-else-if="column.key === 'status'">
            <a-space>
              <a-tag :color="statusMap[record.status]?.color">
                {{ statusMap[record.status]?.text }}
              </a-tag>
              <a-switch
                :checked="record.status === 1"
                checked-children="启用"
                un-checked-children="禁用"
                size="small"
                @change="(checked: boolean) => handleStatusChange(checked, record)"
              />
            </a-space>
          </template>
          <template v-else-if="column.key === 'action'">
            <a-space>
              <a-button v-if="canEdit" type="link" size="small" @click="handleEdit(record)">
                <template #icon>
                  <EditOutlined />
                </template>
                编辑
              </a-button>
              <a-popconfirm
                v-if="canDelete"
                title="确定要删除该租户吗？"
                ok-text="确定"
                cancel-text="取消"
                @confirm="handleDelete(record)"
              >
                <a-button type="link" size="small" danger>
                  <template #icon>
                    <DeleteOutlined />
                  </template>
                  删除
                </a-button>
              </a-popconfirm>
            </a-space>
          </template>
        </template>
      </a-table>
    </a-card>

    <a-modal
      v-model:open="formVisible"
      :title="formTitle"
      :confirm-loading="formLoading"
      width="580px"
      @ok="handleFormSubmit"
      @cancel="resetForm"
    >
      <a-form
        ref="formRef"
        :model="formState"
        :rules="getFormRules()"
        :label-col="{ span: 6 }"
        :wrapper-col="{ span: 16 }"
      >
        <a-form-item label="租户名称" name="name">
          <a-input v-model:value="formState.name" placeholder="请输入租户名称" />
        </a-form-item>
        <a-form-item label="租户编码" name="code">
          <a-input v-model:value="formState.code" placeholder="请输入租户编码" :disabled="isEdit" />
        </a-form-item>
        <a-form-item label="租户类型" name="tenantType">
          <a-radio-group v-model:value="formState.tenantType" :options="tenantTypeOptions" />
        </a-form-item>
        <a-form-item v-if="formState.tenantType === 2" label="数据库连接串" name="dbConnection">
          <a-textarea v-model:value="formState.dbConnection" placeholder="请输入数据库连接串" :rows="3" />
        </a-form-item>
        <a-form-item label="过期时间" name="expireTime">
          <a-date-picker
            v-model:value="formState.expireTime"
            show-time
            format="YYYY-MM-DD HH:mm:ss"
            value-format="YYYY-MM-DD HH:mm:ss"
            placeholder="请选择过期时间"
            style="width: 100%"
          />
        </a-form-item>
        <a-form-item label="状态" name="status">
          <a-switch
            v-model:checked="formState.status"
            checked-children="启用"
            un-checked-children="禁用"
          />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<style lang="less" scoped>
.tenant-list {
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