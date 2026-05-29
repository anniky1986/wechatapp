<script setup lang="ts">
import { reactive, ref, shallowRef, computed, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import type { Rule } from 'ant-design-vue/es/form'
import type { TableColumnsType } from 'ant-design-vue'
import {
  PlusOutlined,
  ExportOutlined,
  EditOutlined,
  DeleteOutlined,
  KeyOutlined,
} from '@ant-design/icons-vue'
import { useDebounceFn } from '@vueuse/core'
import { useUserStore } from '../../../store/modules/user'
import type { PageResult } from '../../../types'
import {
  getUserPage,
  addUser,
  updateUser,
  deleteUser,
  setUserStatus,
  resetPassword,
  type UserRecord,
  type UserQuery,
} from '../../../api/system/user'
import { getDeptTree, type DeptRecord } from '../../../api/system/dept'
import { getRoleList, type RoleRecord } from '../../../api/system/role'

const userStore = useUserStore()

const searchForm = reactive<UserQuery>({
  page: 1,
  pageSize: 15,
  userName: '',
  phone: '',
  status: undefined,
  deptId: undefined,
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

const tableData = ref<UserRecord[]>([])
const tableLoading = shallowRef(false)

const deptTree = shallowRef<DeptRecord[]>([])
const roleList = shallowRef<RoleRecord[]>([])

const statusOptions = [
  { label: '启用', value: 1 },
  { label: '禁用', value: 0 },
]

const statusMap: Record<number, { color: string; text: string }> = {
  0: { color: 'error', text: '禁用' },
  1: { color: 'success', text: '启用' },
}

const columns: TableColumnsType = [
  { title: '用户名', dataIndex: 'userName', key: 'userName', width: 120 },
  { title: '昵称', dataIndex: 'nickName', key: 'nickName', width: 120 },
  { title: '邮箱', dataIndex: 'email', key: 'email', width: 200, ellipsis: true },
  { title: '手机号', dataIndex: 'phone', key: 'phone', width: 140 },
  { title: '部门', dataIndex: 'deptName', key: 'deptName', width: 140 },
  { title: '状态', dataIndex: 'status', key: 'status', width: 80 },
  { title: '角色', dataIndex: 'roles', key: 'roles', width: 200 },
  { title: '创建时间', dataIndex: 'createTime', key: 'createTime', width: 170 },
  { title: '最后登录', dataIndex: 'lastLoginTime', key: 'lastLoginTime', width: 170 },
  { title: '操作', key: 'action', width: 220, fixed: 'right' },
]

async function fetchData() {
  tableLoading.value = true
  try {
    searchForm.page = pagination.current
    searchForm.pageSize = pagination.pageSize
    const res: any = await getUserPage({ ...searchForm })
    const data: PageResult<UserRecord> = res.data ?? res
    tableData.value = data.items
    pagination.total = data.total
  } catch {
    message.error('获取用户列表失败')
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
  searchForm.phone = ''
  searchForm.status = undefined
  searchForm.deptId = undefined
  pagination.current = 1
  fetchData()
}

function handlePageChange(page: number, pageSize: number) {
  pagination.current = page
  pagination.pageSize = pageSize
  fetchData()
}

// ---------- Add / Edit Modal ----------
const formVisible = ref(false)
const formTitle = ref('新增用户')
const formRef = ref()
const formLoading = ref(false)
const isEdit = ref(false)
const currentUserId = ref<number | null>(null)

const formState = reactive({
  userName: '',
  password: '',
  nickName: '',
  email: '',
  phone: '',
  deptId: undefined as number | undefined,
  roleIds: [] as number[],
  status: true as boolean,
})

const formRules = computed<Record<string, Rule[]>>(() => {
  const rules: Record<string, Rule[]> = {
    nickName: [{ required: true, message: '请输入昵称', trigger: 'blur' }],
    phone: [{ pattern: /^1[3-9]\d{9}$/, message: '请输入正确的手机号码', trigger: 'blur' }],
    email: [{ type: 'email' as any, message: '请输入正确的邮箱地址', trigger: 'blur' }],
  }
  if (isEdit.value) {
    rules.userName = [{ required: true, message: '请输入用户名', trigger: 'blur' }]
  } else {
    rules.userName = [
      { required: true, message: '请输入用户名', trigger: 'blur' },
      { min: 3, max: 20, message: '长度在 3 到 20 个字符', trigger: 'blur' },
    ]
    rules.password = [
      { required: true, message: '请输入密码', trigger: 'blur' },
      { min: 6, message: '密码长度不能少于6位', trigger: 'blur' },
    ]
  }
  return rules
})

function resetForm() {
  formState.userName = ''
  formState.password = ''
  formState.nickName = ''
  formState.email = ''
  formState.phone = ''
  formState.deptId = undefined
  formState.roleIds = []
  formState.status = true
  currentUserId.value = null
  formRef.value?.clearValidate()
}

function handleAdd() {
  isEdit.value = false
  formTitle.value = '新增用户'
  resetForm()
  formVisible.value = true
}

function handleEdit(record: UserRecord) {
  isEdit.value = true
  formTitle.value = '编辑用户'
  resetForm()
  currentUserId.value = record.id
  formState.userName = record.userName
  formState.nickName = record.nickName
  formState.email = record.email ?? ''
  formState.phone = record.phone ?? ''
  formState.deptId = record.deptId
  formState.roleIds = (record.roles ?? []) as any
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
      userName: formState.userName,
      nickName: formState.nickName,
      email: formState.email || undefined,
      phone: formState.phone || undefined,
      deptId: formState.deptId,
      roles: formState.roleIds,
      status: formState.status ? 1 : 0,
    }
    if (!isEdit.value) {
      payload.password = formState.password
      await addUser(payload as any)
      message.success('新增用户成功')
    } else {
      await updateUser(currentUserId.value!, payload as any)
      message.success('编辑用户成功')
    }
    formVisible.value = false
    fetchData()
  } catch {
    message.error(isEdit.value ? '编辑用户失败' : '新增用户失败')
  } finally {
    formLoading.value = false
  }
}

// ---------- Status Switch ----------
async function handleStatusChange(checked: boolean, record: UserRecord) {
  const newStatus = checked ? 1 : 0
  try {
    await setUserStatus(record.id, newStatus)
    message.success(`${checked ? '启用' : '禁用'}成功`)
    record.status = newStatus
  } catch {
    message.error('状态修改失败')
  }
}

// ---------- Delete ----------
async function handleDelete(record: UserRecord) {
  try {
    await deleteUser(record.id)
    message.success('删除成功')
    fetchData()
  } catch {
    message.error('删除失败')
  }
}

// ---------- Reset Password ----------
const resetPwdVisible = ref(false)
const resetPwdLoading = ref(false)
const resetPwdUserId = ref<number | null>(null)
const resetPwdFormRef = ref()
const resetPwdForm = reactive({
  password: '',
  confirmPassword: '',
})

function validateConfirmPassword(_rule: Rule, value: string) {
  if (!value) {
    return Promise.reject('请确认密码')
  }
  if (value !== resetPwdForm.password) {
    return Promise.reject('两次输入的密码不一致')
  }
  return Promise.resolve()
}

const resetPwdRules: Record<string, Rule[]> = {
  password: [
    { required: true, message: '请输入新密码', trigger: 'blur' },
    { min: 6, message: '密码长度不能少于6位', trigger: 'blur' },
  ],
  confirmPassword: [
    { required: true, validator: validateConfirmPassword, trigger: 'blur' },
  ],
}

function openResetPwd(record: UserRecord) {
  resetPwdUserId.value = record.id
  resetPwdForm.password = ''
  resetPwdForm.confirmPassword = ''
  resetPwdFormRef.value?.clearValidate()
  resetPwdVisible.value = true
}

async function handleResetPwdConfirm() {
  try {
    await resetPwdFormRef.value?.validate()
  } catch {
    return
  }
  resetPwdLoading.value = true
  try {
    await resetPassword(resetPwdUserId.value!, resetPwdForm.password)
    message.success('密码重置成功')
    resetPwdVisible.value = false
  } catch {
    message.error('密码重置失败')
  } finally {
    resetPwdLoading.value = false
  }
}

// ---------- Export ----------
function handleExport() {
  message.info('导出功能开发中')
}

// ---------- Load dropdown data ----------
async function loadDeptTree() {
  try {
    const res: any = await getDeptTree()
    deptTree.value = res.data ?? res
  } catch {
    message.error('加载部门树失败')
  }
}

async function loadRoleList() {
  try {
    const res: any = await getRoleList()
    roleList.value = res.data ?? res
  } catch {
    message.error('加载角色列表失败')
  }
}

onMounted(() => {
  loadDeptTree()
  loadRoleList()
  fetchData()
})
</script>

<template>
  <div class="user-list">
    <!-- Search Bar -->
    <a-card v-memo="[searchForm.userName, searchForm.phone, searchForm.status, searchForm.deptId]" class="search-card" :bordered="false">
      <a-form layout="inline" :model="searchForm">
        <a-form-item label="用户名">
          <a-input
            v-model:value="searchForm.userName"
            placeholder="请输入用户名"
            allow-clear
            style="width: 180px"
            @press-enter="handleSearch"
            @input="debouncedSearch"
          />
        </a-form-item>
        <a-form-item label="手机号">
          <a-input
            v-model:value="searchForm.phone"
            placeholder="请输入手机号"
            allow-clear
            style="width: 180px"
            @press-enter="handleSearch"
            @input="debouncedSearch"
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
        <a-form-item label="部门">
          <a-tree-select
            v-model:value="searchForm.deptId"
            :tree-data="deptTree"
            :field-names="{ label: 'name', value: 'id', children: 'children' }"
            placeholder="请选择部门"
            allow-clear
            tree-default-expand-all
            style="width: 200px"
          />
        </a-form-item>
        <a-form-item>
          <a-space>
            <a-button type="primary" @click="handleSearch">
              <template #icon>
                <span style="font-size: 14px">&#128269;</span>
              </template>
              查询
            </a-button>
            <a-button @click="handleReset">重置</a-button>
          </a-space>
        </a-form-item>
      </a-form>
    </a-card>

    <!-- Toolbar & Table -->
    <a-card class="table-card" :bordered="false">
      <div class="toolbar">
        <a-space>
          <a-button type="primary" @click="handleAdd">
            <template #icon>
              <PlusOutlined />
            </template>
            新增
          </a-button>
          <a-button @click="handleExport">
            <template #icon>
              <ExportOutlined />
            </template>
            导出
          </a-button>
        </a-space>
      </div>

      <a-table
        :columns="columns"
        :data-source="tableData"
        :loading="tableLoading"
        :pagination="pagination"
        :scroll="{ x: 1400 }"
        row-key="id"
        @change="handlePageChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'status'">
            <a-switch
              :checked="record.status === 1"
              checked-children="启用"
              un-checked-children="禁用"
              size="small"
              @change="(checked: boolean) => handleStatusChange(checked, record)"
            />
          </template>
          <template v-else-if="column.key === 'roles'">
            <a-space wrap :size="[0, 4]">
              <a-tag
                v-for="(role, index) in (record.roles ?? [])"
                :key="index"
                color="blue"
              >
                {{ role }}
              </a-tag>
              <span v-if="!record.roles || record.roles.length === 0" style="color: #999">
                暂无角色
              </span>
            </a-space>
          </template>
          <template v-else-if="column.key === 'action'">
            <a-space>
              <a-button type="link" size="small" @click="handleEdit(record)">
                <template #icon>
                  <EditOutlined />
                </template>
                编辑
              </a-button>
              <a-button type="link" size="small" @click="openResetPwd(record)">
                <template #icon>
                  <KeyOutlined />
                </template>
                重置密码
              </a-button>
              <a-popconfirm
                title="确定要删除该用户吗？"
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

    <!-- Add / Edit Modal -->
    <a-modal
      v-model:open="formVisible"
      :title="formTitle"
      :confirm-loading="formLoading"
      width="600px"
      @ok="handleFormSubmit"
      @cancel="resetForm"
    >
      <a-form
        ref="formRef"
        :model="formState"
        :rules="formRules"
        :label-col="{ span: 5 }"
        :wrapper-col="{ span: 17 }"
      >
        <a-form-item label="用户名" name="userName">
          <a-input
            v-model:value="formState.userName"
            placeholder="请输入用户名"
            :disabled="isEdit"
          />
        </a-form-item>
        <a-form-item v-if="!isEdit" label="密码" name="password">
          <a-input-password
            v-model:value="formState.password"
            placeholder="请输入密码"
          />
        </a-form-item>
        <a-form-item label="昵称" name="nickName">
          <a-input
            v-model:value="formState.nickName"
            placeholder="请输入昵称"
          />
        </a-form-item>
        <a-form-item label="邮箱" name="email">
          <a-input
            v-model:value="formState.email"
            placeholder="请输入邮箱"
          />
        </a-form-item>
        <a-form-item label="手机号" name="phone">
          <a-input
            v-model:value="formState.phone"
            placeholder="请输入手机号"
          />
        </a-form-item>
        <a-form-item label="部门" name="deptId">
          <a-tree-select
            v-model:value="formState.deptId"
            :tree-data="deptTree"
            :field-names="{ label: 'name', value: 'id', children: 'children' }"
            placeholder="请选择部门"
            allow-clear
            tree-default-expand-all
          />
        </a-form-item>
        <a-form-item label="角色分配" name="roleIds">
          <a-select
            v-model:value="formState.roleIds"
            mode="multiple"
            placeholder="请选择角色"
            :options="roleList.map(r => ({ label: r.name, value: r.id }))"
            allow-clear
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

    <!-- Reset Password Modal -->
    <a-modal
      v-model:open="resetPwdVisible"
      title="重置密码"
      :confirm-loading="resetPwdLoading"
      width="460px"
      @ok="handleResetPwdConfirm"
    >
      <a-form
        ref="resetPwdFormRef"
        :model="resetPwdForm"
        :rules="resetPwdRules"
        :label-col="{ span: 5 }"
        :wrapper-col="{ span: 17 }"
      >
        <a-form-item label="新密码" name="password">
          <a-input-password
            v-model:value="resetPwdForm.password"
            placeholder="请输入新密码"
          />
        </a-form-item>
        <a-form-item label="确认密码" name="confirmPassword">
          <a-input-password
            v-model:value="resetPwdForm.confirmPassword"
            placeholder="请再次输入新密码"
          />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<style lang="less" scoped>
.user-list {
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