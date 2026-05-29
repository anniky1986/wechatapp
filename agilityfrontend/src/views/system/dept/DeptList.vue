<script setup lang="ts">
import { reactive, ref, onMounted, nextTick } from 'vue'
import { message } from 'ant-design-vue'
import type { Rule } from 'ant-design-vue/es/form'
import type { TableColumnsType } from 'ant-design-vue'
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  ExpandAltOutlined,
  CompressOutlined,
} from '@ant-design/icons-vue'
import { useUserStore } from '../../../store/modules/user'
import type { DeptRecord } from '../../../api/system/dept'
import {
  getDeptTree,
  addDept,
  updateDept,
  deleteDept,
} from '../../../api/system/dept'

const userStore = useUserStore()

const canAdd = userStore.hasPermission?.('system:dept:add') ?? true
const canEdit = userStore.hasPermission?.('system:dept:edit') ?? true
const canDelete = userStore.hasPermission?.('system:dept:delete') ?? true

const tableData = ref<DeptRecord[]>([])
const tableLoading = ref(false)
const expandedRowKeys = ref<number[]>([])
const allExpanded = ref(false)

const statusMap: Record<number, { color: string; text: string }> = {
  0: { color: 'error', text: '禁用' },
  1: { color: 'success', text: '启用' },
}

const columns: TableColumnsType = [
  { title: '部门名称', dataIndex: 'name', key: 'name', width: 220 },
  { title: '负责人', dataIndex: 'leader', key: 'leader', width: 120 },
  { title: '联系电话', dataIndex: 'phone', key: 'phone', width: 140 },
  { title: '排序', dataIndex: 'sort', key: 'sort', width: 80, align: 'center' },
  { title: '状态', dataIndex: 'status', key: 'status', width: 160, align: 'center' },
  { title: '创建时间', dataIndex: 'createTime', key: 'createTime', width: 170 },
  { title: '操作', key: 'action', width: 260, fixed: 'right' },
]

function collectAllIds(nodes: DeptRecord[]): number[] {
  const ids: number[] = []
  function walk(list: DeptRecord[]) {
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

async function fetchData() {
  tableLoading.value = true
  try {
    const res: any = await getDeptTree()
    tableData.value = res.data ?? res
    if (allExpanded.value) {
      expandedRowKeys.value = collectAllIds(tableData.value)
    }
  } catch {
    message.error('获取部门列表失败')
  } finally {
    tableLoading.value = false
  }
}

function handleExpandAll() {
  allExpanded.value = true
  expandedRowKeys.value = collectAllIds(tableData.value)
}

function handleCollapseAll() {
  allExpanded.value = false
  expandedRowKeys.value = []
}

function handleExpandedRowsChange(keys: readonly (string | number)[]) {
  expandedRowKeys.value = keys as number[]
}

async function handleStatusChange(checked: boolean, record: DeptRecord) {
  const newStatus = checked ? 1 : 0
  try {
    await updateDept(record.id, { status: newStatus })
    message.success(`${checked ? '启用' : '禁用'}成功`)
    record.status = newStatus
  } catch {
    message.error('状态修改失败')
  }
}

async function handleDelete(record: DeptRecord) {
  if (record.children && record.children.length > 0) {
    message.warning('该部门下存在子部门，无法删除')
    return
  }
  try {
    await deleteDept(record.id)
    message.success('删除成功')
    fetchData()
  } catch {
    message.error('删除失败')
  }
}

const formVisible = ref(false)
const formTitle = ref('新增部门')
const formRef = ref()
const formLoading = ref(false)
const isEdit = ref(false)
const currentDeptId = ref<number | null>(null)

const formState = reactive({
  parentId: 0 as number | undefined,
  name: '',
  leader: '',
  phone: '',
  sort: 0,
  status: true as boolean,
})

const formRules: Record<string, Rule[]> = {
  name: [
    { required: true, message: '请输入部门名称', trigger: 'blur' },
    { min: 1, max: 30, message: '长度在 1 到 30 个字符', trigger: 'blur' },
  ],
  sort: [{ required: true, message: '请输入排序', trigger: 'blur' }],
}

function resetForm() {
  formState.parentId = 0
  formState.name = ''
  formState.leader = ''
  formState.phone = ''
  formState.sort = 0
  formState.status = true
  currentDeptId.value = null
  formRef.value?.clearValidate()
}

function handleAdd() {
  isEdit.value = false
  formTitle.value = '新增部门'
  resetForm()
  formVisible.value = true
}

function handleAddChild(parent: DeptRecord) {
  isEdit.value = false
  formTitle.value = '新增子部门'
  resetForm()
  formState.parentId = parent.id
  formVisible.value = true
}

function handleEdit(record: DeptRecord) {
  isEdit.value = true
  formTitle.value = '编辑部门'
  resetForm()
  currentDeptId.value = record.id
  formState.parentId = record.parentId ?? 0
  formState.name = record.name
  formState.leader = record.leader ?? ''
  formState.phone = record.phone ?? ''
  formState.sort = record.sort ?? 0
  formState.status = record.status === 1
  formVisible.value = true
}

async function handleFormSubmit() {
  try {
    await formRef.value?.validate()
    if (formState.parentId === currentDeptId.value) {
      message.error('上级部门不能选择自身')
      return
    }
  } catch {
    return
  }
  formLoading.value = true
  try {
    const payload: Record<string, any> = {
      parentId: formState.parentId || 0,
      name: formState.name,
      leader: formState.leader || undefined,
      phone: formState.phone || undefined,
      sort: formState.sort,
      status: formState.status ? 1 : 0,
    }
    if (!isEdit.value) {
      await addDept(payload as any)
      message.success('新增部门成功')
    } else {
      await updateDept(currentDeptId.value!, payload as any)
      message.success('编辑部门成功')
    }
    formVisible.value = false
    fetchData()
  } catch {
    message.error(isEdit.value ? '编辑部门失败' : '新增部门失败')
  } finally {
    formLoading.value = false
  }
}

function formatDeptTreeForSelect(nodes: DeptRecord[], excludeId?: number): any[] {
  return nodes
    .filter((node) => node.id !== excludeId)
    .map((node) => ({
      title: node.name,
      value: node.id,
      children: node.children && node.children.length > 0
        ? formatDeptTreeForSelect(node.children, excludeId)
        : undefined,
    }))
}

onMounted(() => {
  fetchData()
})
</script>

<template>
  <div class="dept-list">
    <a-card class="table-card" :bordered="false">
      <div class="toolbar">
        <a-space>
          <a-button v-if="canAdd" type="primary" @click="handleAdd">
            <template #icon>
              <PlusOutlined />
            </template>
            新增部门
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
        :data-source="tableData"
        :loading="tableLoading"
        :pagination="false"
        :scroll="{ x: 1100 }"
        row-key="id"
        :children-column-name="'children'"
        :expanded-row-keys="expandedRowKeys"
        :default-expand-all-rows="false"
        @expanded-rows-change="handleExpandedRowsChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'status'">
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
              <a-button v-if="canAdd" type="link" size="small" @click="handleAddChild(record)">
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
                :title="record.children && record.children.length > 0 ? '该部门下存在子部门，确认删除？' : '确定要删除该部门吗？'"
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
      width="560px"
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
        <a-form-item label="上级部门" name="parentId">
          <a-tree-select
            v-model:value="formState.parentId"
            :tree-data="formatDeptTreeForSelect(tableData, currentDeptId ?? undefined)"
            :field-names="{ label: 'title', value: 'value', children: 'children' }"
            placeholder="请选择上级部门（不选则为顶级部门）"
            allow-clear
            tree-default-expand-all
            style="width: 100%"
          />
        </a-form-item>
        <a-form-item label="部门名称" name="name">
          <a-input v-model:value="formState.name" placeholder="请输入部门名称" />
        </a-form-item>
        <a-form-item label="负责人" name="leader">
          <a-input v-model:value="formState.leader" placeholder="请输入负责人" />
        </a-form-item>
        <a-form-item label="联系电话" name="phone">
          <a-input v-model:value="formState.phone" placeholder="请输入联系电话" />
        </a-form-item>
        <a-form-item label="排序" name="sort">
          <a-input-number v-model:value="formState.sort" :min="0" style="width: 100%" placeholder="请输入排序号" />
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
.dept-list {
  padding: 16px;

  .table-card {
    .toolbar {
      margin-bottom: 16px;
    }
  }
}
</style>