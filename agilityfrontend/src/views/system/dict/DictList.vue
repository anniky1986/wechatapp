<script setup lang="ts">
import { reactive, ref, shallowRef, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import type { Rule } from 'ant-design-vue/es/form'
import type { TableColumnsType } from 'ant-design-vue'
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
} from '@ant-design/icons-vue'
import { useDebounceFn } from '@vueuse/core'
import { useUserStore } from '../../../store/modules/user'
import type { PageResult } from '../../../types'
import type {
  DictTypeRecord,
  DictTypeQuery,
  DictItemRecord,
  DictItemQuery,
} from '../../../api/system/dict'
import {
  getDictTypePage,
  addDictType,
  updateDictType,
  deleteDictType,
  getDictItemPage,
  addDictItem,
  updateDictItem,
  deleteDictItem,
} from '../../../api/system/dict'

interface DictItemRow extends DictItemRecord {
  isDefault?: number
}

const userStore = useUserStore()

const canAddType = userStore.hasPermission?.('system:dict:add') ?? true
const canEditType = userStore.hasPermission?.('system:dict:edit') ?? true
const canDeleteType = userStore.hasPermission?.('system:dict:delete') ?? true
const canAddItem = userStore.hasPermission?.('system:dict:add') ?? true
const canEditItem = userStore.hasPermission?.('system:dict:edit') ?? true
const canDeleteItem = userStore.hasPermission?.('system:dict:delete') ?? true

const statusMap: Record<number, { color: string; text: string }> = {
  0: { color: 'error', text: '禁用' },
  1: { color: 'success', text: '启用' },
}

const selectedTypeId = ref<number | null>(null)
const selectedTypeName = ref('')
const selectedTypeType = ref('')

const typeSearchName = ref('')
const typePagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
  showSizeChanger: true,
  showQuickJumper: false,
  pageSizeOptions: ['5', '10', '15', '20'],
  showTotal: (total: number) => `共 ${total} 条`,
})

const typeTableData = ref<DictTypeRecord[]>([])
const typeTableLoading = shallowRef(false)

const typeColumns: TableColumnsType = [
  { title: '字典名称', dataIndex: 'name', key: 'name', width: 120 },
  { title: '字典编码', dataIndex: 'type', key: 'type', width: 140 },
  { title: '备注', dataIndex: 'remark', key: 'remark', width: 120, ellipsis: true },
  { title: '操作', key: 'action', width: 140 },
]

const itemPagination = reactive({
  current: 1,
  pageSize: 15,
  total: 0,
  showSizeChanger: true,
  showQuickJumper: true,
  pageSizeOptions: ['10', '15', '20', '50'],
  showTotal: (total: number) => `共 ${total} 条记录`,
})

const itemTableData = ref<DictItemRow[]>([])
const itemTableLoading = shallowRef(false)

const itemColumns: TableColumnsType = [
  { title: '字典标签', dataIndex: 'label', key: 'label', width: 140 },
  { title: '字典键值', dataIndex: 'value', key: 'value', width: 140 },
  { title: '排序', dataIndex: 'sort', key: 'sort', width: 80, align: 'center' },
  { title: '是否默认', dataIndex: 'isDefault', key: 'isDefault', width: 100, align: 'center' },
  { title: '状态', dataIndex: 'status', key: 'status', width: 100, align: 'center' },
  { title: '操作', key: 'action', width: 200, fixed: 'right' },
]

async function fetchTypeData() {
  typeTableLoading.value = true
  try {
    const query: DictTypeQuery = {
      page: typePagination.current,
      pageSize: typePagination.pageSize,
      name: typeSearchName.value || undefined,
    }
    const res: any = await getDictTypePage(query)
    const data: PageResult<DictTypeRecord> = res.data ?? res
    typeTableData.value = data.items
    typePagination.total = data.total
  } catch {
    message.error('获取字典类型列表失败')
  } finally {
    typeTableLoading.value = false
  }
}

async function fetchItemData() {
  if (!selectedTypeType.value) {
    itemTableData.value = []
    itemPagination.total = 0
    return
  }
  itemTableLoading.value = true
  try {
    const query: DictItemQuery = {
      page: itemPagination.current,
      pageSize: itemPagination.pageSize,
      dictType: selectedTypeType.value,
    }
    const res: any = await getDictItemPage(query)
    const data: PageResult<DictItemRow> = res.data ?? res
    itemTableData.value = data.items
    itemPagination.total = data.total
  } catch {
    message.error('获取字典项列表失败')
  } finally {
    itemTableLoading.value = false
  }
}

function handleTypeSearch() {
  typePagination.current = 1
  fetchTypeData()
}

const debouncedTypeSearch = useDebounceFn(handleTypeSearch, 300)

function handleTypePageChange(page: number, pageSize: number) {
  typePagination.current = page
  typePagination.pageSize = pageSize
  fetchTypeData()
}

function handleItemPageChange(page: number, pageSize: number) {
  itemPagination.current = page
  itemPagination.pageSize = pageSize
  fetchItemData()
}

function selectType(record: DictTypeRecord) {
  selectedTypeId.value = record.id
  selectedTypeName.value = record.name
  selectedTypeType.value = record.type
  itemPagination.current = 1
  fetchItemData()
}

async function handleDeleteType(record: DictTypeRecord) {
  try {
    await deleteDictType(record.id)
    message.success('删除成功')
    if (selectedTypeId.value === record.id) {
      selectedTypeId.value = null
      selectedTypeName.value = ''
      selectedTypeType.value = ''
      itemTableData.value = []
      itemPagination.total = 0
    }
    fetchTypeData()
  } catch {
    message.error('删除失败')
  }
}

async function handleDeleteItem(record: DictItemRow) {
  try {
    await deleteDictItem(record.id)
    message.success('删除成功')
    fetchItemData()
  } catch {
    message.error('删除失败')
  }
}

const typeFormVisible = ref(false)
const typeFormTitle = ref('新增字典类型')
const typeFormRef = ref()
const typeFormLoading = ref(false)
const isTypeEdit = ref(false)
const currentTypeId = ref<number | null>(null)

const typeFormState = reactive({
  name: '',
  type: '',
  remark: '',
})

const typeFormRules: Record<string, Rule[]> = {
  name: [
    { required: true, message: '请输入字典名称', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
  type: [
    { required: true, message: '请输入字典编码', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
}

function resetTypeForm() {
  typeFormState.name = ''
  typeFormState.type = ''
  typeFormState.remark = ''
  currentTypeId.value = null
  typeFormRef.value?.clearValidate()
}

function handleAddType() {
  isTypeEdit.value = false
  typeFormTitle.value = '新增字典类型'
  resetTypeForm()
  typeFormVisible.value = true
}

function handleEditType(record: DictTypeRecord) {
  isTypeEdit.value = true
  typeFormTitle.value = '编辑字典类型'
  resetTypeForm()
  currentTypeId.value = record.id
  typeFormState.name = record.name
  typeFormState.type = record.type
  typeFormState.remark = record.remark ?? ''
  typeFormVisible.value = true
}

async function handleTypeFormSubmit() {
  try {
    await typeFormRef.value?.validate()
  } catch {
    return
  }
  typeFormLoading.value = true
  try {
    const payload: Record<string, any> = {
      name: typeFormState.name,
      type: typeFormState.type,
      remark: typeFormState.remark || undefined,
    }
    if (!isTypeEdit.value) {
      await addDictType(payload)
      message.success('新增字典类型成功')
    } else {
      await updateDictType(currentTypeId.value!, payload)
      message.success('编辑字典类型成功')
    }
    typeFormVisible.value = false
    fetchTypeData()
  } catch {
    message.error(isTypeEdit.value ? '编辑字典类型失败' : '新增字典类型失败')
  } finally {
    typeFormLoading.value = false
  }
}

const itemFormVisible = ref(false)
const itemFormTitle = ref('新增字典项')
const itemFormRef = ref()
const itemFormLoading = ref(false)
const isItemEdit = ref(false)
const currentItemId = ref<number | null>(null)

const itemFormState = reactive({
  label: '',
  value: '',
  sort: 0,
  className: '' as string,
  isDefault: false,
  status: true as boolean,
})

const itemFormRules: Record<string, Rule[]> = {
  label: [
    { required: true, message: '请输入字典标签', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
  value: [
    { required: true, message: '请输入字典键值', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
  sort: [{ required: true, message: '请输入排序', trigger: 'blur' }],
}

function resetItemForm() {
  itemFormState.label = ''
  itemFormState.value = ''
  itemFormState.sort = 0
  itemFormState.className = ''
  itemFormState.isDefault = false
  itemFormState.status = true
  currentItemId.value = null
  itemFormRef.value?.clearValidate()
}

function handleAddItem() {
  if (!selectedTypeType.value) {
    message.warning('请先选择字典类型')
    return
  }
  isItemEdit.value = false
  itemFormTitle.value = '新增字典项'
  resetItemForm()
  itemFormVisible.value = true
}

function handleEditItem(record: DictItemRow) {
  isItemEdit.value = true
  itemFormTitle.value = '编辑字典项'
  resetItemForm()
  currentItemId.value = record.id
  itemFormState.label = record.label
  itemFormState.value = record.value
  itemFormState.sort = record.sort ?? 0
  itemFormState.className = record.className ?? ''
  itemFormState.isDefault = record.isDefault === 1
  itemFormState.status = record.status === 1
  itemFormVisible.value = true
}

async function handleItemFormSubmit() {
  if (!selectedTypeType.value) {
    message.warning('字典类型信息缺失')
    return
  }
  try {
    await itemFormRef.value?.validate()
  } catch {
    return
  }
  itemFormLoading.value = true
  try {
    const payload: Record<string, any> = {
      dictType: selectedTypeType.value,
      label: itemFormState.label,
      value: itemFormState.value,
      sort: itemFormState.sort,
      className: itemFormState.className || undefined,
      isDefault: itemFormState.isDefault ? 1 : 0,
      status: itemFormState.status ? 1 : 0,
    }
    if (!isItemEdit.value) {
      await addDictItem(payload)
      message.success('新增字典项成功')
    } else {
      await updateDictItem(currentItemId.value!, payload)
      message.success('编辑字典项成功')
    }
    itemFormVisible.value = false
    fetchItemData()
  } catch {
    message.error(isItemEdit.value ? '编辑字典项失败' : '新增字典项失败')
  } finally {
    itemFormLoading.value = false
  }
}

onMounted(() => {
  fetchTypeData()
})
</script>

<template>
  <div class="dict-list">
    <div class="dict-container">
      <div class="dict-left">
        <a-card :bordered="false" title="字典类型" size="small">
          <template #extra>
            <a-button v-if="canAddType" type="primary" size="small" @click="handleAddType">
              <template #icon>
                <PlusOutlined />
              </template>
              新增
            </a-button>
          </template>

          <div class="type-search">
            <a-input-search
              v-model:value="typeSearchName"
              placeholder="搜索字典名称"
              allow-clear
              @search="handleTypeSearch"
              @input="debouncedTypeSearch"
            />
          </div>

          <a-table
            :columns="typeColumns"
            :data-source="typeTableData"
            :loading="typeTableLoading"
            :pagination="typePagination"
            row-key="id"
            size="small"
            @change="handleTypePageChange"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'name'">
                <a
                  :style="{
                    fontWeight: selectedTypeId === record.id ? 'bold' : 'normal',
                    color: selectedTypeId === record.id ? '#1890ff' : 'inherit',
                  }"
                  @click="selectType(record)"
                >
                  {{ record.name }}
                </a>
              </template>
              <template v-else-if="column.key === 'action'">
                <a-space :size="0">
                  <a-button v-if="canEditType" type="link" size="small" @click="handleEditType(record)">
                    <template #icon>
                      <EditOutlined />
                    </template>
                  </a-button>
                  <a-popconfirm
                    v-if="canDeleteType"
                    title="确定要删除该字典类型吗？"
                    ok-text="确定"
                    cancel-text="取消"
                    @confirm="handleDeleteType(record)"
                  >
                    <a-button type="link" size="small" danger>
                      <template #icon>
                        <DeleteOutlined />
                      </template>
                    </a-button>
                  </a-popconfirm>
                </a-space>
              </template>
            </template>
          </a-table>
        </a-card>
      </div>

      <div class="dict-right">
        <a-card :bordered="false" size="small">
          <template #title>
            <span>
              字典项列表
              <template v-if="selectedTypeName">
                -
                <a-tag color="blue">{{ selectedTypeName }}</a-tag>
              </template>
            </span>
          </template>
          <template #extra>
            <a-button
              v-if="canAddItem && selectedTypeType"
              type="primary"
              size="small"
              @click="handleAddItem"
            >
              <template #icon>
                <PlusOutlined />
              </template>
              新增字典项
            </a-button>
          </template>

          <div v-if="!selectedTypeType" class="empty-hint">
            <a-empty description="请在左侧选择字典类型，查看对应的字典项" />
          </div>

          <a-table
            v-else
            :columns="itemColumns"
            :data-source="itemTableData"
            :loading="itemTableLoading"
            :pagination="itemPagination"
            :scroll="{ x: 800 }"
            row-key="id"
            @change="handleItemPageChange"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'isDefault'">
                <a-tag :color="record.isDefault === 1 ? 'blue' : 'default'">
                  {{ record.isDefault === 1 ? '是' : '否' }}
                </a-tag>
              </template>
              <template v-else-if="column.key === 'status'">
                <a-tag :color="statusMap[record.status]?.color">
                  {{ statusMap[record.status]?.text }}
                </a-tag>
              </template>
              <template v-else-if="column.key === 'action'">
                <a-space>
                  <a-button v-if="canEditItem" type="link" size="small" @click="handleEditItem(record)">
                    <template #icon>
                      <EditOutlined />
                    </template>
                    编辑
                  </a-button>
                  <a-popconfirm
                    v-if="canDeleteItem"
                    title="确定要删除该字典项吗？"
                    ok-text="确定"
                    cancel-text="取消"
                    @confirm="handleDeleteItem(record)"
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
      </div>
    </div>

    <a-modal
      v-model:open="typeFormVisible"
      :title="typeFormTitle"
      :confirm-loading="typeFormLoading"
      width="520px"
      @ok="handleTypeFormSubmit"
      @cancel="resetTypeForm"
    >
      <a-form
        ref="typeFormRef"
        :model="typeFormState"
        :rules="typeFormRules"
        :label-col="{ span: 5 }"
        :wrapper-col="{ span: 17 }"
      >
        <a-form-item label="字典名称" name="name">
          <a-input v-model:value="typeFormState.name" placeholder="请输入字典名称" />
        </a-form-item>
        <a-form-item label="字典编码" name="type">
          <a-input
            v-model:value="typeFormState.type"
            placeholder="请输入字典编码"
            :disabled="isTypeEdit"
          />
        </a-form-item>
        <a-form-item label="备注" name="remark">
          <a-textarea v-model:value="typeFormState.remark" placeholder="请输入备注" :rows="2" />
        </a-form-item>
      </a-form>
    </a-modal>

    <a-modal
      v-model:open="itemFormVisible"
      :title="itemFormTitle"
      :confirm-loading="itemFormLoading"
      width="520px"
      @ok="handleItemFormSubmit"
      @cancel="resetItemForm"
    >
      <a-form
        ref="itemFormRef"
        :model="itemFormState"
        :rules="itemFormRules"
        :label-col="{ span: 5 }"
        :wrapper-col="{ span: 17 }"
      >
        <a-form-item label="字典标签" name="label">
          <a-input v-model:value="itemFormState.label" placeholder="请输入字典标签" />
        </a-form-item>
        <a-form-item label="字典键值" name="value">
          <a-input v-model:value="itemFormState.value" placeholder="请输入字典键值" />
        </a-form-item>
        <a-form-item label="排序" name="sort">
          <a-input-number v-model:value="itemFormState.sort" :min="0" style="width: 100%" placeholder="请输入排序号" />
        </a-form-item>
        <a-form-item label="样式类名" name="className">
          <a-input v-model:value="itemFormState.className" placeholder="请输入样式类名" />
        </a-form-item>
        <a-form-item label="是否默认" name="isDefault">
          <a-switch
            v-model:checked="itemFormState.isDefault"
            checked-children="是"
            un-checked-children="否"
          />
        </a-form-item>
        <a-form-item label="状态" name="status">
          <a-switch
            v-model:checked="itemFormState.status"
            checked-children="启用"
            un-checked-children="禁用"
          />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<style lang="less" scoped>
.dict-list {
  padding: 16px;
  height: 100%;

  .dict-container {
    display: flex;
    gap: 16px;
    height: 100%;

    .dict-left {
      flex: 0 0 420px;
      min-width: 0;

      .type-search {
        margin-bottom: 12px;
      }
    }

    .dict-right {
      flex: 1;
      min-width: 0;

      .empty-hint {
        display: flex;
        align-items: center;
        justify-content: center;
        min-height: 300px;
      }
    }
  }
}
</style>