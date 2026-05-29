<script setup lang="ts">
import { reactive, ref, shallowRef, onMounted, nextTick, watch } from 'vue'
import { message } from 'ant-design-vue'
import type { Rule } from 'ant-design-vue/es/form'
import type { TableColumnsType } from 'ant-design-vue'
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  ReloadOutlined,
  ExpandAltOutlined,
  CompressOutlined,
  FolderOutlined,
  MenuOutlined,
  AppstoreOutlined,
} from '@ant-design/icons-vue'
import type { MenuItem } from '../../../types'
import {
  getMenuTree,
  addMenu,
  updateMenu,
  deleteMenu,
  syncMenuPermissions,
} from '../../../api/system/menu'
import IconSelector from '../../../components/common/IconSelector.vue'
import { useUserStore } from '../../../store/modules/user'

const userStore = useUserStore()

const canAdd = userStore.hasPermission?.('system:menu:add') ?? true
const canEdit = userStore.hasPermission?.('system:menu:edit') ?? true
const canDelete = userStore.hasPermission?.('system:menu:delete') ?? true
const canSync = userStore.hasPermission?.('system:menu:sync') ?? true

const menuTreeData = ref<MenuItem[]>([])
const tableLoading = shallowRef(false)
const expandedRowKeys = ref<number[]>([])
const allExpanded = ref(false)

const menuTypeOptions = [
  { label: '目录', value: 1 },
  { label: '菜单', value: 2 },
  { label: '按钮', value: 3 },
]

const menuTypeMap: Record<number, { color: string; text: string; icon: any }> = {
  1: { color: 'blue', text: '目录', icon: FolderOutlined },
  2: { color: 'green', text: '菜单', icon: MenuOutlined },
  3: { color: 'orange', text: '按钮', icon: AppstoreOutlined },
}

const statusMap: Record<number, { color: string; text: string }> = {
  0: { color: 'error', text: '禁用' },
  1: { color: 'success', text: '启用' },
}

const columns: TableColumnsType = [
  { title: '菜单名称', dataIndex: 'name', key: 'name', width: 200 },
  { title: '图标', dataIndex: 'icon', key: 'icon', width: 70, align: 'center' },
  { title: '路由路径', dataIndex: 'path', key: 'path', width: 180, ellipsis: true },
  { title: '组件路径', dataIndex: 'component', key: 'component', width: 200, ellipsis: true },
  { title: '权限标识', dataIndex: 'permission', key: 'permission', width: 180, ellipsis: true },
  { title: '菜单类型', dataIndex: 'menuType', key: 'menuType', width: 100, align: 'center' },
  { title: '排序', dataIndex: 'orderNo', key: 'orderNo', width: 70, align: 'center' },
  { title: '状态', dataIndex: 'status', key: 'status', width: 80, align: 'center' },
  { title: '隐藏', dataIndex: 'isHide', key: 'isHide', width: 70, align: 'center' },
  { title: '缓存', dataIndex: 'keepAlive', key: 'keepAlive', width: 70, align: 'center' },
  { title: '操作', key: 'action', width: 240, fixed: 'right' },
]

function collectAllIds(nodes: MenuItem[]): number[] {
  const ids: number[] = []
  function walk(list: MenuItem[]) {
    for (const node of list) {
      ids.push(node.id)
      if (node.children && node.children.length > 0) {
        walk(node.children)
      }
    }
  }
  walk(nodes)
  return ids
}

async function fetchMenuTree() {
  tableLoading.value = true
  try {
    const res: any = await getMenuTree()
    menuTreeData.value = res.data ?? res
    if (allExpanded.value) {
      expandedRowKeys.value = collectAllIds(menuTreeData.value)
    }
  } catch {
    message.error('获取菜单列表失败')
  } finally {
    tableLoading.value = false
  }
}

function handleExpandAll() {
  allExpanded.value = true
  expandedRowKeys.value = collectAllIds(menuTreeData.value)
}

function handleCollapseAll() {
  allExpanded.value = false
  expandedRowKeys.value = []
}

function handleExpandedRowsChange(keys: readonly (string | number)[]) {
  expandedRowKeys.value = keys as number[]
}

async function handleSyncPermissions() {
  tableLoading.value = true
  try {
    await syncMenuPermissions()
    message.success('权限同步成功')
    await fetchMenuTree()
  } catch {
    message.error('权限同步失败')
  } finally {
    tableLoading.value = false
  }
}

function handleDelete(record: MenuItem) {
  if (record.children && record.children.length > 0) {
    message.warning('该菜单下存在子菜单，无法删除')
    return
  }
  ;(async () => {
    try {
      await deleteMenu(record.id)
      message.success('删除成功')
      fetchMenuTree()
    } catch {
      message.error('删除失败')
    }
  })()
}

const formVisible = ref(false)
const formTitle = ref('新增菜单')
const formRef = ref()
const formLoading = ref(false)
const iconSelectorRef = ref()
const isEdit = ref(false)
const currentMenuId = ref<number | null>(null)

const formState = reactive({
  parentId: 0,
  name: '',
  icon: '',
  menuType: 1,
  orderNo: 0,
  status: true as boolean,
  path: '',
  component: '',
  redirect: '',
  permission: '',
  isHide: false,
  keepAlive: false,
  isFrame: false,
  frameSrc: '',
})

const formRules: Record<string, Rule[]> = {
  name: [
    { required: true, message: '请输入菜单名称', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
  orderNo: [{ required: true, message: '请输入排序', trigger: 'blur' }],
}

const menuTypeRules: Record<string, Rule[]> = {
  path: [{ required: true, message: '请输入路由路径', trigger: 'blur' }],
  component: [{ required: true, message: '请输入组件路径', trigger: 'blur' }],
  permission: [{ required: true, message: '请输入权限标识', trigger: 'blur' }],
}

function getActiveRules(): Record<string, Rule[]> {
  const rules: Record<string, Rule[]> = { ...formRules }
  if (formState.menuType === 1) {
    rules.path = [{ required: true, message: '请输入路由路径', trigger: 'blur' }]
  } else if (formState.menuType === 2) {
    rules.path = [{ required: true, message: '请输入路由路径', trigger: 'blur' }]
    rules.component = [{ required: true, message: '请输入组件路径', trigger: 'blur' }]
  } else if (formState.menuType === 3) {
    rules.permission = [{ required: true, message: '请输入权限标识', trigger: 'blur' }]
  }
  return rules
}

function resetForm() {
  formState.parentId = 0
  formState.name = ''
  formState.icon = ''
  formState.menuType = 1
  formState.orderNo = 0
  formState.status = true
  formState.path = ''
  formState.component = ''
  formState.redirect = ''
  formState.permission = ''
  formState.isHide = false
  formState.keepAlive = false
  formState.isFrame = false
  formState.frameSrc = ''
  currentMenuId.value = null
  formRef.value?.clearValidate()
}

function handleAdd() {
  isEdit.value = false
  formTitle.value = '新增菜单'
  resetForm()
  showIconInSelector('')
  formVisible.value = true
}

function handleAddChild(parent: MenuItem) {
  isEdit.value = false
  formTitle.value = '新增子菜单'
  resetForm()
  formState.parentId = parent.id
  showIconInSelector('')
  formVisible.value = true
}

function handleEdit(record: MenuItem) {
  isEdit.value = true
  formTitle.value = '编辑菜单'
  resetForm()
  currentMenuId.value = record.id
  formState.parentId = record.parentId ?? 0
  formState.name = record.name
  formState.menuType = record.menuType
  formState.orderNo = record.orderNo ?? 0
  formState.status = record.status === 1
  formState.path = record.path ?? ''
  formState.component = record.component ?? ''
  formState.redirect = record.redirect ?? ''
  formState.permission = record.permission ?? ''
  formState.isHide = record.isHide ?? false
  formState.keepAlive = record.keepAlive ?? false
  formState.isFrame = record.isFrame ?? false
  formState.frameSrc = record.frameSrc ?? ''
  showIconInSelector(record.icon ?? '')
  formVisible.value = true
}

function showIconInSelector(iconName: string) {
  formState.icon = iconName
  nextTick(() => {
    if (iconSelectorRef.value) {
      iconSelectorRef.value.selectedIcon = iconName
    }
  })
}

function onIconSelectorOpen() {
  iconSelectorRef.value?.open()
}

watch(
  () => iconSelectorRef.value?.selectedIcon,
  (val: string | undefined) => {
    if (val !== undefined) {
      formState.icon = val
    }
  },
)

async function handleFormSubmit() {
  const rules = getActiveRules()
  try {
    await formRef.value?.validate()
    if (formState.parentId === currentMenuId.value) {
      message.error('上级菜单不能选择自身')
      return
    }
  } catch {
    return
  }
  formLoading.value = true
  try {
    const icon = iconSelectorRef.value?.selectedIcon ?? formState.icon
    const payload: Record<string, any> = {
      parentId: formState.parentId,
      name: formState.name,
      icon: icon || undefined,
      menuType: formState.menuType,
      orderNo: formState.orderNo,
      status: formState.status ? 1 : 0,
    }
    if (formState.menuType === 1) {
      payload.path = formState.path
      payload.redirect = formState.redirect || undefined
      payload.isHide = formState.isHide
      payload.keepAlive = formState.keepAlive
    } else if (formState.menuType === 2) {
      payload.path = formState.path
      payload.component = formState.component
      payload.isHide = formState.isHide
      payload.keepAlive = formState.keepAlive
      payload.isFrame = formState.isFrame
      payload.frameSrc = formState.isFrame ? (formState.frameSrc || undefined) : undefined
    } else if (formState.menuType === 3) {
      payload.permission = formState.permission
    }
    if (!isEdit.value) {
      await addMenu(payload)
      message.success('新增菜单成功')
    } else {
      await updateMenu(currentMenuId.value!, payload)
      message.success('编辑菜单成功')
    }
    formVisible.value = false
    fetchMenuTree()
  } catch {
    message.error(isEdit.value ? '编辑菜单失败' : '新增菜单失败')
  } finally {
    formLoading.value = false
  }
}

function onMenuTypeChange() {
  formState.path = ''
  formState.component = ''
  formState.redirect = ''
  formState.permission = ''
  formState.isHide = false
  formState.keepAlive = false
  formState.isFrame = false
  formState.frameSrc = ''
  nextTick(() => {
    formRef.value?.clearValidate()
  })
}

function formatMenuTreeForSelect(nodes: MenuItem[], excludeId?: number): any[] {
  return nodes
    .filter((node) => node.id !== excludeId)
    .map((node) => ({
      title: node.name,
      value: node.id,
      children: node.children && node.children.length > 0
        ? formatMenuTreeForSelect(node.children, excludeId)
        : undefined,
    }))
}

onMounted(() => {
  fetchMenuTree()
})
</script>

<template>
  <div class="menu-list">
    <a-card class="table-card" :bordered="false">
      <div class="toolbar">
        <a-space>
          <a-button v-if="canAdd" type="primary" @click="handleAdd">
            <template #icon>
              <PlusOutlined />
            </template>
            新增菜单
          </a-button>
          <a-button v-if="canSync" @click="handleSyncPermissions" :loading="tableLoading">
            <template #icon>
              <ReloadOutlined />
            </template>
            同步权限
          </a-button>
          <a-button @click="handleExpandAll">
            <template #icon>
              <ExpandAltOutlined />
            </template>
            展开全部
          </a-button>
          <a-button @click="handleCollapseAll">
            <template #icon>
              <CompressOutlined />
            </template>
            折叠全部
          </a-button>
        </a-space>
      </div>

      <a-table
        :columns="columns"
        :data-source="menuTreeData"
        :loading="tableLoading"
        :pagination="false"
        :scroll="{ x: 1500 }"
        row-key="id"
        :children-column-name="'children'"
        :expanded-row-keys="expandedRowKeys"
        :default-expand-all-rows="false"
        @expanded-rows-change="handleExpandedRowsChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'icon'">
            <component
              v-if="record.icon"
              :is="record.icon"
              style="font-size: 18px"
            />
            <span v-else style="color: #ccc">-</span>
          </template>
          <template v-else-if="column.key === 'menuType'">
            <a-tag :color="menuTypeMap[record.menuType]?.color">
              <template #icon>
                <component :is="menuTypeMap[record.menuType]?.icon" />
              </template>
              {{ menuTypeMap[record.menuType]?.text }}
            </a-tag>
          </template>
          <template v-else-if="column.key === 'status'">
            <a-tag :color="statusMap[record.status]?.color">
              {{ statusMap[record.status]?.text }}
            </a-tag>
          </template>
          <template v-else-if="column.key === 'isHide'">
            <a-tag :color="record.isHide ? 'warning' : 'default'">
              {{ record.isHide ? '是' : '否' }}
            </a-tag>
          </template>
          <template v-else-if="column.key === 'keepAlive'">
            <a-tag :color="record.keepAlive ? 'processing' : 'default'">
              {{ record.keepAlive ? '是' : '否' }}
            </a-tag>
          </template>
          <template v-else-if="column.key === 'action'">
            <a-space>
              <a-button
                v-if="canAdd && record.menuType !== 3"
                type="link"
                size="small"
                @click="handleAddChild(record)"
              >
                <template #icon>
                  <PlusOutlined />
                </template>
                新增子级
              </a-button>
              <a-button v-if="canEdit" type="link" size="small" @click="handleEdit(record)">
                <template #icon>
                  <EditOutlined />
                </template>
                编辑
              </a-button>
              <a-popconfirm
                v-if="canDelete"
                :title="record.children && record.children.length > 0 ? '存在子菜单，确认删除？子菜单也将被删除' : '确定要删除该菜单吗？'"
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
      width="680px"
      @ok="handleFormSubmit"
      @cancel="resetForm"
    >
      <a-form
        ref="formRef"
        :model="formState"
        :rules="getActiveRules()"
        :label-col="{ span: 5 }"
        :wrapper-col="{ span: 17 }"
      >
        <a-form-item label="上级菜单" name="parentId">
          <a-tree-select
            v-model:value="formState.parentId"
            :tree-data="formatMenuTreeForSelect(menuTreeData, currentMenuId ?? undefined)"
            :field-names="{ label: 'title', value: 'value', children: 'children' }"
            placeholder="请选择上级菜单（根目录不选）"
            allow-clear
            tree-default-expand-all
            style="width: 100%"
          />
        </a-form-item>
        <a-form-item label="菜单类型" name="menuType">
          <a-radio-group
            v-model:value="formState.menuType"
            :options="menuTypeOptions"
            :disabled="isEdit"
            @change="onMenuTypeChange"
          />
        </a-form-item>
        <a-form-item label="菜单名称" name="name">
          <a-input v-model:value="formState.name" placeholder="请输入菜单名称" />
        </a-form-item>
        <a-form-item label="菜单图标" name="icon">
          <div class="icon-select-wrapper">
            <a-input
              :value="formState.icon"
              readonly
              placeholder="请选择图标"
              @click="onIconSelectorOpen"
            >
              <template #addonAfter>
                <component
                  v-if="formState.icon"
                  :is="formState.icon"
                  style="font-size: 16px"
                />
                <span v-else style="color: #ccc">无</span>
              </template>
            </a-input>
            <IconSelector ref="iconSelectorRef" style="display: none" />
          </div>
        </a-form-item>
        <a-form-item label="显示排序" name="orderNo">
          <a-input-number v-model:value="formState.orderNo" :min="0" style="width: 100%" placeholder="请输入排序号" />
        </a-form-item>

        <template v-if="formState.menuType === 1">
          <a-form-item label="路由路径" name="path">
            <a-input v-model:value="formState.path" placeholder="请输入路由路径，如 /system" />
          </a-form-item>
          <a-form-item label="重定向" name="redirect">
            <a-input v-model:value="formState.redirect" placeholder="请输入重定向地址" />
          </a-form-item>
          <a-form-item label="是否隐藏" name="isHide">
            <a-switch v-model:checked="formState.isHide" checked-children="隐藏" un-checked-children="显示" />
          </a-form-item>
          <a-form-item label="页面缓存" name="keepAlive">
            <a-switch v-model:checked="formState.keepAlive" checked-children="缓存" un-checked-children="不缓存" />
          </a-form-item>
        </template>

        <template v-if="formState.menuType === 2">
          <a-form-item label="路由路径" name="path">
            <a-input v-model:value="formState.path" placeholder="请输入路由路径，如 /user" />
          </a-form-item>
          <a-form-item label="组件路径" name="component">
            <a-input v-model:value="formState.component" placeholder="请输入组件路径，如 system/user/index" />
          </a-form-item>
          <a-form-item label="是否隐藏" name="isHide">
            <a-switch v-model:checked="formState.isHide" checked-children="隐藏" un-checked-children="显示" />
          </a-form-item>
          <a-form-item label="页面缓存" name="keepAlive">
            <a-switch v-model:checked="formState.keepAlive" checked-children="缓存" un-checked-children="不缓存" />
          </a-form-item>
          <a-form-item label="是否外链" name="isFrame">
            <a-switch v-model:checked="formState.isFrame" checked-children="外链" un-checked-children="内部" />
          </a-form-item>
          <a-form-item v-if="formState.isFrame" label="外链地址" name="frameSrc">
            <a-input v-model:value="formState.frameSrc" placeholder="请输入外链地址，如 https://example.com" />
          </a-form-item>
        </template>

        <template v-if="formState.menuType === 3">
          <a-form-item label="权限标识" name="permission">
            <a-input v-model:value="formState.permission" placeholder="请输入权限标识，如 system:user:add" />
          </a-form-item>
        </template>

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
.menu-list {
  padding: 16px;

  .table-card {
    .toolbar {
      margin-bottom: 16px;
    }
  }
}
</style>