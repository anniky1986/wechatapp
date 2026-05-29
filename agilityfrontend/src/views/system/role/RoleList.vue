<script setup lang="ts">
import { reactive, ref, shallowRef, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import type { Rule } from 'ant-design-vue/es/form'
import type { TableColumnsType } from 'ant-design-vue'
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  ApartmentOutlined,
  SafetyCertificateOutlined,
} from '@ant-design/icons-vue'
import { useDebounceFn } from '@vueuse/core'
import { useUserStore } from '../../../store/modules/user'
import type { MenuItem, PageResult } from '../../../types'
import {
  getRolePage,
  addRole,
  updateRole,
  deleteRole,
  setRoleStatus,
  setRoleMenus,
  getRoleMenuIds,
  setRoleDataScope,
  getRoleDataScope,
  type RoleRecord,
  type RoleQuery,
} from '../../../api/system/role'
import { getMenuTree } from '../../../api/system/menu'
import { getDeptTree, type DeptRecord } from '../../../api/system/dept'

interface RoleRow extends RoleRecord {
  sort?: number
  dataScope?: number
  remark?: string
}

const userStore = useUserStore()

const canAdd = userStore.hasPermission?.('system:role:add') ?? true
const canEdit = userStore.hasPermission?.('system:role:edit') ?? true
const canDelete = userStore.hasPermission?.('system:role:delete') ?? true
const canAssignMenu = userStore.hasPermission?.('system:role:assignMenu') ?? true
const canAssignData = userStore.hasPermission?.('system:role:assignData') ?? true

const dataScopeOptions = [
  { label: '全部数据权限', value: 1 },
  { label: '本部门数据权限', value: 2 },
  { label: '本部门及以下数据权限', value: 3 },
  { label: '仅本人数据权限', value: 4 },
  { label: '自定义数据权限', value: 5 },
]

const dataScopeMap: Record<number, string> = {
  1: '全部数据',
  2: '本部门',
  3: '本部门及以下',
  4: '仅本人',
  5: '自定义',
}

const statusOptions = [
  { label: '启用', value: 1 },
  { label: '禁用', value: 0 },
]

const statusMap: Record<number, { color: string; text: string }> = {
  0: { color: 'error', text: '禁用' },
  1: { color: 'success', text: '启用' },
}

const searchForm = reactive<RoleQuery>({
  page: 1,
  pageSize: 15,
  name: '',
  status: undefined,
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

const tableData = ref<RoleRow[]>([])
const tableLoading = shallowRef(false)

const columns: TableColumnsType = [
  { title: '角色名称', dataIndex: 'name', key: 'name', width: 150 },
  { title: '角色编码', dataIndex: 'code', key: 'code', width: 150 },
  { title: '数据范围', dataIndex: 'dataScope', key: 'dataScope', width: 140 },
  { title: '排序', dataIndex: 'sort', key: 'sort', width: 80, align: 'center' },
  { title: '状态', dataIndex: 'status', key: 'status', width: 100, align: 'center' },
  { title: '备注', dataIndex: 'remark', key: 'remark', width: 200, ellipsis: true },
  { title: '创建时间', dataIndex: 'createTime', key: 'createTime', width: 170 },
  { title: '操作', key: 'action', width: 320, fixed: 'right' },
]

async function fetchData() {
  tableLoading.value = true
  try {
    searchForm.page = pagination.current
    searchForm.pageSize = pagination.pageSize
    const res: any = await getRolePage({ ...searchForm })
    const data: PageResult<RoleRow> = res.data ?? res
    tableData.value = data.items
    pagination.total = data.total
  } catch {
    message.error('获取角色列表失败')
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
  searchForm.name = ''
  searchForm.status = undefined
  pagination.current = 1
  fetchData()
}

function handlePageChange(page: number, pageSize: number) {
  pagination.current = page
  pagination.pageSize = pageSize
  fetchData()
}

const formVisible = ref(false)
const formTitle = ref('新增角色')
const formRef = ref()
const formLoading = ref(false)
const isEdit = ref(false)
const currentRoleId = ref<number | null>(null)

const formState = reactive({
  name: '',
  code: '',
  sort: 0,
  dataScope: 1,
  status: true as boolean,
  remark: '',
})

const formRules: Record<string, Rule[]> = {
  name: [
    { required: true, message: '请输入角色名称', trigger: 'blur' },
    { min: 1, max: 30, message: '长度在 1 到 30 个字符', trigger: 'blur' },
  ],
  code: [
    { required: true, message: '请输入角色编码', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
  sort: [{ required: true, message: '请输入排序', trigger: 'blur' }],
}

function resetForm() {
  formState.name = ''
  formState.code = ''
  formState.sort = 0
  formState.dataScope = 1
  formState.status = true
  formState.remark = ''
  currentRoleId.value = null
  formRef.value?.clearValidate()
}

function handleAdd() {
  isEdit.value = false
  formTitle.value = '新增角色'
  resetForm()
  formVisible.value = true
}

function handleEdit(record: RoleRow) {
  isEdit.value = true
  formTitle.value = '编辑角色'
  resetForm()
  currentRoleId.value = record.id
  formState.name = record.name
  formState.code = record.code
  formState.sort = record.sort ?? 0
  formState.dataScope = record.dataScope ?? 1
  formState.status = record.status === 1
  formState.remark = record.remark ?? ''
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
      sort: formState.sort,
      dataScope: formState.dataScope,
      status: formState.status ? 1 : 0,
      remark: formState.remark || undefined,
    }
    if (!isEdit.value) {
      await addRole(payload)
      message.success('新增角色成功')
    } else {
      await updateRole(currentRoleId.value!, payload)
      message.success('编辑角色成功')
    }
    formVisible.value = false
    fetchData()
  } catch {
    message.error(isEdit.value ? '编辑角色失败' : '新增角色失败')
  } finally {
    formLoading.value = false
  }
}

async function handleStatusChange(checked: boolean, record: RoleRow) {
  const newStatus = checked ? 1 : 0
  try {
    await setRoleStatus(record.id, newStatus)
    message.success(`${checked ? '启用' : '禁用'}成功`)
    record.status = newStatus
  } catch {
    message.error('状态修改失败')
  }
}

async function handleDelete(record: RoleRow) {
  try {
    await deleteRole(record.id)
    message.success('删除成功')
    fetchData()
  } catch {
    message.error('删除失败')
  }
}

const menuAssignVisible = ref(false)
const menuAssignLoading = ref(false)
const menuAssignRoleId = ref<number | null>(null)
const menuAssignRoleName = ref('')
const menuTreeData = ref<MenuItem[]>([])
const checkedMenuKeys = ref<number[]>([])
const allMenuKeys = ref<number[]>([])

function collectAllKeys(nodes: MenuItem[]): number[] {
  const keys: number[] = []
  function walk(list: MenuItem[]) {
    for (const node of list) {
      keys.push(node.id)
      if (node.children && node.children.length > 0) {
        walk(node.children)
      }
    }
  }
  walk(nodes)
  return keys
}

async function openMenuAssign(record: RoleRow) {
  menuAssignRoleId.value = record.id
  menuAssignRoleName.value = record.name
  menuAssignLoading.value = false
  checkedMenuKeys.value = []
  allMenuKeys.value = []
  try {
    const res: any = await getMenuTree()
    menuTreeData.value = res.data ?? res
    allMenuKeys.value = collectAllKeys(menuTreeData.value)
    const menuIdsRes: any = await getRoleMenuIds(record.id)
    checkedMenuKeys.value = menuIdsRes.data ?? menuIdsRes ?? record.menuIds ?? []
  } catch {
    message.error('加载菜单数据失败')
  }
  menuAssignVisible.value = true
}

function handleMenuCheck(checkedKeys: any, _info: any) {
  checkedMenuKeys.value = checkedKeys.checked ?? checkedKeys
}

async function handleMenuAssignConfirm() {
  if (!menuAssignRoleId.value) return
  menuAssignLoading.value = true
  try {
    await setRoleMenus(menuAssignRoleId.value, checkedMenuKeys.value)
    message.success('菜单权限分配成功')
    menuAssignVisible.value = false
    fetchData()
  } catch {
    message.error('菜单权限分配失败')
  } finally {
    menuAssignLoading.value = false
  }
}

const dataScopeVisible = ref(false)
const dataScopeLoading = ref(false)
const dataScopeRoleId = ref<number | null>(null)
const dataScopeRoleName = ref('')
const deptTreeData = ref<DeptRecord[]>([])
const checkedDeptKeys = ref<number[]>([])

function collectDeptKeys(nodes: DeptRecord[]): number[] {
  const keys: number[] = []
  function walk(list: DeptRecord[]) {
    for (const node of list) {
      keys.push(node.id)
      if (node.children && node.children.length > 0) {
        walk(node.children)
      }
    }
  }
  walk(nodes)
  return keys
}

async function openDataScope(record: RoleRow) {
  dataScopeRoleId.value = record.id
  dataScopeRoleName.value = record.name
  dataScopeLoading.value = false
  checkedDeptKeys.value = []
  try {
    const res: any = await getDeptTree()
    deptTreeData.value = res.data ?? res
    const scopeRes: any = await getRoleDataScope(record.id)
    const scopeData = scopeRes.data ?? scopeRes
    checkedDeptKeys.value = scopeData?.deptIds ?? []
  } catch {
    message.error('加载部门数据失败')
  }
  dataScopeVisible.value = true
}

function handleDeptCheck(checkedKeys: any, _info: any) {
  checkedDeptKeys.value = checkedKeys.checked ?? checkedKeys
}

async function handleDataScopeConfirm() {
  if (!dataScopeRoleId.value) return
  dataScopeLoading.value = true
  try {
    await setRoleDataScope(dataScopeRoleId.value, 5, checkedDeptKeys.value)
    message.success('数据权限分配成功')
    dataScopeVisible.value = false
    fetchData()
  } catch {
    message.error('数据权限分配失败')
  } finally {
    dataScopeLoading.value = false
  }
}

function formatTreeData(nodes: MenuItem[]): any[] {
  return nodes.map((node) => ({
    title: node.name,
    key: node.id,
    children: node.children && node.children.length > 0 ? formatTreeData(node.children) : undefined,
  }))
}

function formatDeptTreeData(nodes: DeptRecord[]): any[] {
  return nodes.map((node) => ({
    title: node.name,
    key: node.id,
    children: node.children && node.children.length > 0 ? formatDeptTreeData(node.children) : undefined,
  }))
}

onMounted(() => {
  fetchData()
})
</script>

<template>
  <div class="role-list">
    <a-card v-memo="[searchForm.name, searchForm.status]" class="search-card" :bordered="false">
      <a-form layout="inline" :model="searchForm">
        <a-form-item label="角色名称">
          <a-input
            v-model:value="searchForm.name"
            placeholder="请输入角色名称"
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
        :scroll="{ x: 1280 }"
        row-key="id"
        @change="handlePageChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'dataScope'">
            <span>{{ dataScopeMap[record.dataScope] ?? record.dataScope ?? '-' }}</span>
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
              <a-button v-if="canAssignMenu" type="link" size="small" @click="openMenuAssign(record)">
                <template #icon>
                  <ApartmentOutlined />
                </template>
                分配菜单
              </a-button>
              <a-button v-if="canAssignData" type="link" size="small" @click="openDataScope(record)">
                <template #icon>
                  <SafetyCertificateOutlined />
                </template>
                数据权限
              </a-button>
              <a-popconfirm
                v-if="canDelete"
                title="确定要删除该角色吗？"
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
        <a-form-item label="角色名称" name="name">
          <a-input v-model:value="formState.name" placeholder="请输入角色名称" />
        </a-form-item>
        <a-form-item label="角色编码" name="code">
          <a-input v-model:value="formState.code" placeholder="请输入角色编码" :disabled="isEdit" />
        </a-form-item>
        <a-form-item label="排序" name="sort">
          <a-input-number v-model:value="formState.sort" :min="0" style="width: 100%" placeholder="请输入排序号" />
        </a-form-item>
        <a-form-item label="数据范围" name="dataScope">
          <a-radio-group v-model:value="formState.dataScope" :options="dataScopeOptions" />
        </a-form-item>
        <a-form-item label="状态" name="status">
          <a-switch
            v-model:checked="formState.status"
            checked-children="启用"
            un-checked-children="禁用"
          />
        </a-form-item>
        <a-form-item label="备注" name="remark">
          <a-textarea v-model:value="formState.remark" placeholder="请输入备注" :rows="3" />
        </a-form-item>
      </a-form>
    </a-modal>

    <a-modal
      v-model:open="menuAssignVisible"
      :title="`分配菜单 - ${menuAssignRoleName}`"
      :confirm-loading="menuAssignLoading"
      width="520px"
      @ok="handleMenuAssignConfirm"
    >
      <a-tree
        v-if="menuTreeData.length > 0"
        checkable
        :tree-data="formatTreeData(menuTreeData)"
        :checked-keys="checkedMenuKeys"
        default-expand-all
        :check-strictly="false"
        @check="handleMenuCheck"
      />
      <a-empty v-else description="暂无菜单数据" />
    </a-modal>

    <a-modal
      v-model:open="dataScopeVisible"
      :title="`数据权限 - ${dataScopeRoleName}`"
      :confirm-loading="dataScopeLoading"
      width="520px"
      @ok="handleDataScopeConfirm"
    >
      <a-tree
        v-if="deptTreeData.length > 0"
        checkable
        :tree-data="formatDeptTreeData(deptTreeData)"
        :checked-keys="checkedDeptKeys"
        default-expand-all
        :check-strictly="false"
        @check="handleDeptCheck"
      />
      <a-empty v-else description="暂无部门数据" />
    </a-modal>
  </div>
</template>

<style lang="less" scoped>
.role-list {
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