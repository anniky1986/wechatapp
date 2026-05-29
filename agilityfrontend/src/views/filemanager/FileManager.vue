<script setup lang="ts">
import { reactive, ref, computed, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import type { TableColumnsType } from 'ant-design-vue'
import {
  UploadOutlined,
  FolderAddOutlined,
  AppstoreOutlined,
  UnorderedListOutlined,
  DownloadOutlined,
  EyeOutlined,
  DeleteOutlined,
  EditOutlined,
  SwapOutlined,
  FolderOutlined,
  FileOutlined,
  FileImageOutlined,
  FilePdfOutlined,
  FileWordOutlined,
  FileExcelOutlined,
  FilePptOutlined,
  FileTextOutlined,
  FileZipOutlined,
  VideoCameraOutlined,
  AudioOutlined,
  HomeOutlined,
} from '@ant-design/icons-vue'
import type { FolderRecord } from '@/api/filemanager/folder'
import {
  getFolderTree,
  addFolder,
  updateFolder,
  deleteFolder,
} from '@/api/filemanager/folder'
import type { FileRecord } from '@/api/filemanager/file'
import {
  getFileList,
  uploadFile,
  downloadFile,
  previewUrl,
  deleteFile,
  renameFile,
  moveFile,
} from '@/api/filemanager/file'

interface BreadcrumbItem {
  id: number | null
  name: string
}

interface UploadingItem {
  uid: string
  name: string
  percent: number
  status: 'uploading' | 'done' | 'error'
}

interface TreeNodeData {
  title: string
  key: number
  children?: TreeNodeData[]
  isLeaf?: boolean
  slots?: Record<string, string>
  scopedSlots?: Record<string, string>
}

const viewMode = ref<'grid' | 'list'>('grid')
const currentFolderId = ref<number | null>(null)
const breadcrumb = ref<BreadcrumbItem[]>([{ id: null, name: '全部文件' }])
const folderTree = ref<FolderRecord[]>([])
const files = ref<FileRecord[]>([])
const filesLoading = ref(false)
const searchKeyword = ref('')
const uploadList = ref<UploadingItem[]>([])
const treeExpandedKeys = ref<number[]>([])

const contextMenuVisible = ref(false)
const contextMenuPos = reactive({ x: 0, y: 0 })
const contextMenuFolder = ref<FolderRecord | null>(null)

const folderModalVisible = ref(false)
const folderModalTitle = ref('新建文件夹')
const folderModalLoading = ref(false)
const folderFormRef = ref()
const folderForm = reactive({ name: '' })
const editingFolder = ref<FolderRecord | null>(null)

const renameModalVisible = ref(false)
const renameModalTitle = ref('重命名')
const renameModalLoading = ref(false)
const renameFormRef = ref()
const renameForm = reactive({ name: '' })
const renamingFile = ref<FileRecord | null>(null)

const moveModalVisible = ref(false)
const moveModalLoading = ref(false)
const movingFile = ref<FileRecord | null>(null)
const moveTargetFolderId = ref<number | null>(null)

const previewVisible = ref(false)
const previewFile = ref<FileRecord | null>(null)

const fileIconMap: Record<string, any> = {
  image: FileImageOutlined,
  pdf: FilePdfOutlined,
  word: FileWordOutlined,
  doc: FileWordOutlined,
  docx: FileWordOutlined,
  excel: FileExcelOutlined,
  xls: FileExcelOutlined,
  xlsx: FileExcelOutlined,
  ppt: FilePptOutlined,
  pptx: FilePptOutlined,
  txt: FileTextOutlined,
  zip: FileZipOutlined,
  rar: FileZipOutlined,
  '7z': FileZipOutlined,
  gz: FileZipOutlined,
  video: VideoCameraOutlined,
  audio: AudioOutlined,
}

const columns: TableColumnsType = [
  { title: '文件名', dataIndex: 'name', key: 'name', width: 260, ellipsis: true },
  { title: '大小', dataIndex: 'size', key: 'size', width: 100, align: 'right' },
  { title: '类型', dataIndex: 'extension', key: 'extension', width: 90, align: 'center' },
  { title: '上传时间', dataIndex: 'createTime', key: 'createTime', width: 170 },
  { title: '操作', key: 'action', width: 260, fixed: 'right' },
]

function getFileIcon(record: FileRecord) {
  const ext = (record.extension || '').toLowerCase()
  if (record.mimeType?.startsWith('image/')) return fileIconMap.image
  if (record.mimeType?.startsWith('video/')) return fileIconMap.video
  if (record.mimeType?.startsWith('audio/')) return fileIconMap.audio
  return fileIconMap[ext] || FileOutlined
}

function formatFileSize(bytes: number | undefined): string {
  if (!bytes) return '-'
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
  if (bytes < 1024 * 1024 * 1024) return (bytes / (1024 * 1024)).toFixed(1) + ' MB'
  return (bytes / (1024 * 1024 * 1024)).toFixed(2) + ' GB'
}

function isImageFile(record: FileRecord): boolean {
  return !!record.mimeType?.startsWith('image/')
}

function isPreviewable(record: FileRecord): boolean {
  return isImageFile(record)
}

const filteredFiles = computed(() => {
  if (!searchKeyword.value) return files.value
  const kw = searchKeyword.value.toLowerCase()
  return files.value.filter((f) => f.name.toLowerCase().includes(kw))
})

async function loadFolderTree() {
  try {
    const res: any = await getFolderTree()
    folderTree.value = res.data ?? res
  } catch {
    message.error('加载文件夹树失败')
  }
}

async function loadFiles() {
  filesLoading.value = true
  try {
    const res: any = await getFileList(currentFolderId.value ?? undefined)
    files.value = res.data ?? res
  } catch {
    message.error('加载文件列表失败')
  } finally {
    filesLoading.value = false
  }
}

function handleFolderSelect(_selectedKeys: (string | number)[], info: any) {
  const node = info.node
  if (node && node.dataRef) {
    const folder: FolderRecord = node.dataRef
    navigateToFolder(folder)
  }
}

function navigateToFolder(folder: FolderRecord) {
  currentFolderId.value = folder.id
  updateBreadcrumbFromTree(folder.id)
  searchKeyword.value = ''
  loadFiles()
}

function handleBreadcrumbClick(item: BreadcrumbItem) {
  if (item.id === currentFolderId.value) return
  currentFolderId.value = item.id
  const idx = breadcrumb.value.findIndex((b) => b.id === item.id)
  if (idx >= 0) {
    breadcrumb.value = breadcrumb.value.slice(0, idx + 1)
  }
  searchKeyword.value = ''
  loadFiles()
}

function handleHomeClick() {
  currentFolderId.value = null
  breadcrumb.value = [{ id: null, name: '全部文件' }]
  searchKeyword.value = ''
  loadFiles()
}

function findFolderPath(targetId: number, nodes: FolderRecord[], path: BreadcrumbItem[]): boolean {
  for (const node of nodes) {
    path.push({ id: node.id, name: node.name })
    if (node.id === targetId) return true
    if (node.children && node.children.length > 0) {
      if (findFolderPath(targetId, node.children, path)) return true
    }
    path.pop()
  }
  return false
}

function updateBreadcrumbFromTree(folderId: number) {
  const path: BreadcrumbItem[] = [{ id: null, name: '全部文件' }]
  if (findFolderPath(folderId, folderTree.value, path)) {
    breadcrumb.value = path
  }
}

function handleContextMenu(e: MouseEvent, folder: FolderRecord) {
  e.preventDefault()
  contextMenuPos.x = e.clientX
  contextMenuPos.y = e.clientY
  contextMenuFolder.value = folder
  contextMenuVisible.value = true
}

function handleTreeContextMenu(info: any) {
  const node = info.node
  if (node && node.dataRef) {
    const event = info.event as MouseEvent
    handleContextMenu(event, node.dataRef)
  }
}

function openAddFolder(parent?: FolderRecord) {
  folderModalTitle.value = parent ? `在"${parent.name}"下新建文件夹` : '新建文件夹'
  editingFolder.value = parent || null
  folderForm.name = ''
  folderFormRef.value?.clearValidate()
  folderModalVisible.value = true
}

function openRenameFolder(folder: FolderRecord) {
  folderModalTitle.value = '重命名文件夹'
  editingFolder.value = folder
  folderForm.name = folder.name
  folderFormRef.value?.clearValidate()
  folderModalVisible.value = true
}

async function handleFolderSubmit() {
  try {
    await folderFormRef.value?.validate()
  } catch {
    return
  }
  folderModalLoading.value = true
  try {
    if (editingFolder.value && folderModalTitle.value === '重命名文件夹') {
      await updateFolder(editingFolder.value.id, { name: folderForm.name })
      message.success('重命名成功')
    } else {
      await addFolder({
        name: folderForm.name,
        parentId: editingFolder.value?.id,
      })
      message.success('新建文件夹成功')
    }
    folderModalVisible.value = false
    await loadFolderTree()
    loadFiles()
  } catch {
    message.error(editingFolder.value ? '重命名失败' : '新建文件夹失败')
  } finally {
    folderModalLoading.value = false
  }
}

async function handleDeleteFolder(folder: FolderRecord) {
  try {
    await deleteFolder(folder.id)
    message.success('删除成功')
    if (currentFolderId.value === folder.id) {
      handleHomeClick()
    }
    await loadFolderTree()
    loadFiles()
  } catch {
    message.error('删除失败')
  }
}

function handleUploadChange(info: any) {
  const { file } = info
  if (file.status === 'uploading') {
    const existing = uploadList.value.find((u) => u.uid === file.uid)
    if (existing) {
      existing.percent = file.percent || 0
    } else {
      uploadList.value.push({
        uid: file.uid,
        name: file.name,
        percent: file.percent || 0,
        status: 'uploading',
      })
    }
  } else if (file.status === 'done') {
    message.success(`${file.name} 上传成功`)
    uploadList.value = uploadList.value.filter((u) => u.uid !== file.uid)
    loadFiles()
  } else if (file.status === 'error') {
    message.error(`${file.name} 上传失败`)
    const item = uploadList.value.find((u) => u.uid === file.uid)
    if (item) item.status = 'error'
  }
}

async function customUpload(options: any) {
  const { file, onProgress, onSuccess, onError } = options
  const formData = new FormData()
  formData.append('file', file)
  if (currentFolderId.value) {
    formData.append('folderId', String(currentFolderId.value))
  }
  try {
    await uploadFile(formData, (percent: number) => {
      onProgress({ percent })
    })
    onSuccess({}, file)
  } catch (err: any) {
    onError(err)
  }
}

async function handleDownload(record: FileRecord) {
  try {
    await downloadFile(record.id)
    message.success('下载已开始')
  } catch {
    message.error('下载失败')
  }
}

function handlePreview(record: FileRecord) {
  if (isImageFile(record)) {
    previewFile.value = record
    previewVisible.value = true
  } else {
    message.info('暂不支持预览该类型文件')
  }
}

async function handleDeleteFile(record: FileRecord) {
  try {
    await deleteFile(record.id)
    message.success('删除成功')
    loadFiles()
  } catch {
    message.error('删除失败')
  }
}

function openRenameFile(record: FileRecord) {
  renamingFile.value = record
  renameForm.name = record.name
  renameFormRef.value?.clearValidate()
  renameModalVisible.value = true
}

async function handleRenameSubmit() {
  if (!renamingFile.value) return
  try {
    await renameFormRef.value?.validate()
  } catch {
    return
  }
  renameModalLoading.value = true
  try {
    await renameFile(renamingFile.value.id, renameForm.name)
    message.success('重命名成功')
    renameModalVisible.value = false
    loadFiles()
  } catch {
    message.error('重命名失败')
  } finally {
    renameModalLoading.value = false
  }
}

function openMoveFile(record: FileRecord) {
  movingFile.value = record
  moveTargetFolderId.value = null
  moveModalVisible.value = true
}

async function handleMoveSubmit() {
  if (!movingFile.value || moveTargetFolderId.value === null) return
  moveModalLoading.value = true
  try {
    await moveFile(movingFile.value.id, moveTargetFolderId.value)
    message.success('移动成功')
    moveModalVisible.value = false
    loadFiles()
  } catch {
    message.error('移动失败')
  } finally {
    moveModalLoading.value = false
  }
}

const treeData = computed<TreeNodeData[]>(() => {
  function convert(nodes: FolderRecord[]): TreeNodeData[] {
    return nodes.map((node) => ({
      title: node.name,
      key: node.id,
      children: node.children && node.children.length > 0 ? convert(node.children) : undefined,
      isLeaf: !node.children || node.children.length === 0,
      dataRef: node,
    }))
  }
  return convert(folderTree.value)
})

function moveTargetTreeData(): TreeNodeData[] {
  function convert(nodes: FolderRecord[], excludeId: number | undefined): TreeNodeData[] {
    return nodes
      .filter((n) => n.id !== excludeId)
      .map((n) => ({
        title: n.name,
        key: n.id,
        children: n.children ? convert(n.children, excludeId) : undefined,
        isLeaf: !n.children || n.children.length === 0,
      }))
  }
  return convert(folderTree.value, movingFile.value?.folderId)
}

function handleMoveTreeSelect(selectedKeys: (string | number)[]) {
  if (selectedKeys.length > 0) {
    moveTargetFolderId.value = selectedKeys[0] as number
  }
}

onMounted(() => {
  loadFolderTree()
  loadFiles()
})
</script>

<template>
  <div class="file-manager">
    <a-row :gutter="16" style="height: 100%">
      <a-col :span="5">
        <a-card class="folder-panel" :bordered="false" title="文件夹">
          <template #extra>
            <a-button type="link" size="small" @click="openAddFolder()">
              <template #icon>
                <FolderAddOutlined />
              </template>
            </a-button>
          </template>
          <a-tree
            v-if="treeData.length > 0"
            :tree-data="treeData"
            :expanded-keys="treeExpandedKeys"
            :selected-keys="currentFolderId ? [currentFolderId] : []"
            show-line
            block-node
            @select="handleFolderSelect"
            @right-click="handleTreeContextMenu"
            @expand="(keys: (string | number)[]) => treeExpandedKeys = keys as number[]"
          >
            <template #title="{ dataRef, title }">
              <a-dropdown :trigger="['contextmenu']">
                <span class="tree-node-title">
                  <FolderOutlined style="margin-right: 6px; color: #faad14" />
                  <span>{{ title }}</span>
                </span>
                <template #overlay>
                  <a-menu>
                    <a-menu-item key="add" @click="openAddFolder(dataRef)">
                      <FolderAddOutlined />
                      <span style="margin-left: 8px">新建子文件夹</span>
                    </a-menu-item>
                    <a-menu-item key="rename" @click="openRenameFolder(dataRef)">
                      <EditOutlined />
                      <span style="margin-left: 8px">重命名</span>
                    </a-menu-item>
                    <a-menu-divider />
                    <a-menu-item key="delete" danger @click="handleDeleteFolder(dataRef)">
                      <DeleteOutlined />
                      <span style="margin-left: 8px">删除</span>
                    </a-menu-item>
                  </a-menu>
                </template>
              </a-dropdown>
            </template>
          </a-tree>
          <a-empty v-else description="暂无文件夹" />
        </a-card>
      </a-col>

      <a-col :span="19">
        <a-card class="file-panel" :bordered="false">
          <div class="file-breadcrumb">
            <a-breadcrumb>
              <a-breadcrumb-item>
                <a @click="handleHomeClick">
                  <HomeOutlined />
                  <span style="margin-left: 4px">全部文件</span>
                </a>
              </a-breadcrumb-item>
              <a-breadcrumb-item
                v-for="item in breadcrumb.slice(1)"
                :key="item.id"
              >
                <a @click="handleBreadcrumbClick(item)">{{ item.name }}</a>
              </a-breadcrumb-item>
            </a-breadcrumb>
          </div>

          <div class="file-toolbar">
            <a-space>
              <a-upload
                :custom-request="customUpload"
                :show-upload-list="false"
                multiple
                @change="handleUploadChange"
              >
                <a-button type="primary">
                  <template #icon>
                    <UploadOutlined />
                  </template>
                  上传文件
                </a-button>
              </a-upload>
              <a-button @click="openAddFolder()">
                <template #icon>
                  <FolderAddOutlined />
                </template>
                新建文件夹
              </a-button>
            </a-space>

            <a-space>
              <a-input-search
                v-model:value="searchKeyword"
                placeholder="搜索文件..."
                style="width: 220px"
                allow-clear
              />
              <a-radio-group v-model:value="viewMode" size="small" button-style="solid">
                <a-radio-button value="grid">
                  <AppstoreOutlined />
                </a-radio-button>
                <a-radio-button value="list">
                  <UnorderedListOutlined />
                </a-radio-button>
              </a-radio-group>
            </a-space>
          </div>

          <div v-if="uploadList.length > 0" class="upload-progress-bar">
            <div v-for="item in uploadList" :key="item.uid" class="upload-progress-item">
              <span class="upload-filename">{{ item.name }}</span>
              <a-progress
                :percent="item.percent"
                :status="item.status === 'error' ? 'exception' : 'active'"
                size="small"
                style="flex: 1; max-width: 300px"
              />
            </div>
          </div>

          <a-spin :spinning="filesLoading">
            <template v-if="viewMode === 'grid'">
              <div v-if="filteredFiles.length === 0 && !filesLoading" class="empty-state">
                <a-empty description="暂无文件" />
              </div>
              <div v-else class="file-grid">
                <div
                  v-for="file in filteredFiles"
                  :key="file.id"
                  class="file-grid-item"
                >
                  <div class="file-grid-card">
                    <div class="file-grid-icon">
                      <component
                        :is="getFileIcon(file)"
                        :style="{ fontSize: '40px', color: isImageFile(file) ? '#1677ff' : '#8c8c8c' }"
                      />
                    </div>
                    <a-tooltip :title="file.name">
                      <div class="file-grid-name">{{ file.name }}</div>
                    </a-tooltip>
                    <div class="file-grid-meta">
                      <span>{{ formatFileSize(file.size) }}</span>
                      <span>{{ file.createTime }}</span>
                    </div>
                    <div class="file-grid-actions">
                      <a-space size="small">
                        <a-tooltip title="下载">
                          <a-button
                            type="text"
                            size="small"
                            @click="handleDownload(file)"
                          >
                            <template #icon>
                              <DownloadOutlined />
                            </template>
                          </a-button>
                        </a-tooltip>
                        <a-tooltip v-if="isPreviewable(file)" title="预览">
                          <a-button
                            type="text"
                            size="small"
                            @click="handlePreview(file)"
                          >
                            <template #icon>
                              <EyeOutlined />
                            </template>
                          </a-button>
                        </a-tooltip>
                        <a-tooltip title="重命名">
                          <a-button
                            type="text"
                            size="small"
                            @click="openRenameFile(file)"
                          >
                            <template #icon>
                              <EditOutlined />
                            </template>
                          </a-button>
                        </a-tooltip>
                        <a-tooltip title="移动">
                          <a-button
                            type="text"
                            size="small"
                            @click="openMoveFile(file)"
                          >
                            <template #icon>
                              <SwapOutlined />
                            </template>
                          </a-button>
                        </a-tooltip>
                        <a-popconfirm
                          title="确定要删除该文件吗？"
                          ok-text="确定"
                          cancel-text="取消"
                          @confirm="handleDeleteFile(file)"
                        >
                          <a-tooltip title="删除">
                            <a-button
                              type="text"
                              size="small"
                              danger
                            >
                              <template #icon>
                                <DeleteOutlined />
                              </template>
                            </a-button>
                          </a-tooltip>
                        </a-popconfirm>
                      </a-space>
                    </div>
                  </div>
                </div>
              </div>
            </template>

            <template v-else>
              <div v-if="filteredFiles.length === 0 && !filesLoading" class="empty-state">
                <a-empty description="暂无文件" />
              </div>
              <a-table
                v-else
                :columns="columns"
                :data-source="filteredFiles"
                :loading="filesLoading"
                :pagination="{ pageSize: 15, showSizeChanger: true, showTotal: (t: number) => `共 ${t} 个文件` }"
                :scroll="{ x: 880 }"
                row-key="id"
                size="middle"
              >
                <template #bodyCell="{ column, record }">
                  <template v-if="column.key === 'name'">
                    <a-space>
                      <component
                        :is="getFileIcon(record)"
                        :style="{ fontSize: '18px', color: isImageFile(record) ? '#1677ff' : '#8c8c8c' }"
                      />
                      <a-tooltip :title="record.name">
                        <span class="file-name-text">{{ record.name }}</span>
                      </a-tooltip>
                    </a-space>
                  </template>
                  <template v-else-if="column.key === 'size'">
                    {{ formatFileSize(record.size) }}
                  </template>
                  <template v-else-if="column.key === 'extension'">
                    <a-tag v-if="record.extension" color="blue">{{ record.extension.toUpperCase() }}</a-tag>
                    <span v-else>-</span>
                  </template>
                  <template v-else-if="column.key === 'action'">
                    <a-space size="small">
                      <a-button type="link" size="small" @click="handleDownload(record)">
                        <template #icon>
                          <DownloadOutlined />
                        </template>
                        下载
                      </a-button>
                      <a-button
                        v-if="isPreviewable(record)"
                        type="link"
                        size="small"
                        @click="handlePreview(record)"
                      >
                        <template #icon>
                          <EyeOutlined />
                        </template>
                        预览
                      </a-button>
                      <a-button type="link" size="small" @click="openRenameFile(record)">
                        <template #icon>
                          <EditOutlined />
                        </template>
                        重命名
                      </a-button>
                      <a-button type="link" size="small" @click="openMoveFile(record)">
                        <template #icon>
                          <SwapOutlined />
                        </template>
                        移动
                      </a-button>
                      <a-popconfirm
                        title="确定要删除该文件吗？"
                        ok-text="确定"
                        cancel-text="取消"
                        @confirm="handleDeleteFile(record)"
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
            </template>
          </a-spin>
        </a-card>
      </a-col>
    </a-row>

    <a-modal
      v-model:open="folderModalVisible"
      :title="folderModalTitle"
      :confirm-loading="folderModalLoading"
      width="420px"
      @ok="handleFolderSubmit"
    >
      <a-form
        ref="folderFormRef"
        :model="folderForm"
        :rules="{ name: [{ required: true, message: '请输入文件夹名称', trigger: 'blur' }] }"
        :label-col="{ span: 5 }"
        :wrapper-col="{ span: 17 }"
      >
        <a-form-item label="名称" name="name">
          <a-input
            v-model:value="folderForm.name"
            placeholder="请输入文件夹名称"
          />
        </a-form-item>
      </a-form>
    </a-modal>

    <a-modal
      v-model:open="renameModalVisible"
      :title="renameModalTitle"
      :confirm-loading="renameModalLoading"
      width="420px"
      @ok="handleRenameSubmit"
    >
      <a-form
        ref="renameFormRef"
        :model="renameForm"
        :rules="{ name: [{ required: true, message: '请输入名称', trigger: 'blur' }] }"
        :label-col="{ span: 5 }"
        :wrapper-col="{ span: 17 }"
      >
        <a-form-item label="名称" name="name">
          <a-input
            v-model:value="renameForm.name"
            placeholder="请输入新名称"
          />
        </a-form-item>
      </a-form>
    </a-modal>

    <a-modal
      v-model:open="moveModalVisible"
      title="移动到"
      :confirm-loading="moveModalLoading"
      width="420px"
      @ok="handleMoveSubmit"
    >
      <a-tree
        v-if="folderTree.length > 0"
        :tree-data="moveTargetTreeData()"
        :selected-keys="moveTargetFolderId ? [moveTargetFolderId] : []"
        :default-expanded-keys="treeExpandedKeys"
        show-line
        block-node
        @select="handleMoveTreeSelect"
      >
        <template #title="{ title }">
          <span>
            <FolderOutlined style="margin-right: 6px; color: #faad14" />
            {{ title }}
          </span>
        </template>
      </a-tree>
      <a-empty v-else description="暂无文件夹" />
    </a-modal>

    <a-modal
      v-model:open="previewVisible"
      title="预览"
      :footer="null"
      width="800px"
      @cancel="previewFile = null"
    >
      <template v-if="previewFile">
        <img
          :src="previewUrl(previewFile.id)"
          :alt="previewFile.name"
          style="width: 100%; max-height: 70vh; object-fit: contain"
        />
      </template>
    </a-modal>
  </div>
</template>

<style lang="less" scoped>
.file-manager {
  padding: 16px;
  height: calc(100vh - 120px);
  min-height: 600px;

  .folder-panel {
    height: 100%;
    overflow-y: auto;

    .tree-node-title {
      display: inline-flex;
      align-items: center;
      cursor: pointer;
      user-select: none;
    }
  }

  .file-panel {
    height: 100%;
    display: flex;
    flex-direction: column;

    .file-breadcrumb {
      margin-bottom: 12px;
      padding-bottom: 12px;
      border-bottom: 1px solid #f0f0f0;
    }

    .file-toolbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 12px;
      flex-wrap: wrap;
      gap: 8px;
    }

    .upload-progress-bar {
      margin-bottom: 12px;
      padding: 8px 12px;
      background: #fafafa;
      border-radius: 6px;

      .upload-progress-item {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 4px 0;

        .upload-filename {
          flex-shrink: 0;
          min-width: 120px;
          font-size: 13px;
          overflow: hidden;
          text-overflow: ellipsis;
          white-space: nowrap;
        }
      }
    }

    .empty-state {
      padding: 60px 0;
    }

    .file-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
      gap: 12px;
      overflow-y: auto;
      flex: 1;

      .file-grid-item {
        .file-grid-card {
          border: 1px solid #f0f0f0;
          border-radius: 8px;
          padding: 16px 12px 12px;
          text-align: center;
          transition: all 0.2s;
          cursor: default;
          background: #fff;

          &:hover {
            border-color: #1677ff;
            box-shadow: 0 2px 8px rgba(22, 119, 255, 0.1);
          }

          .file-grid-icon {
            padding: 12px 0 8px;
          }

          .file-grid-name {
            font-size: 13px;
            font-weight: 500;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
            margin-bottom: 6px;
            color: #262626;
          }

          .file-grid-meta {
            font-size: 11px;
            color: #8c8c8c;
            display: flex;
            justify-content: space-between;
            margin-bottom: 10px;
          }

          .file-grid-actions {
            border-top: 1px solid #f5f5f5;
            padding-top: 8px;
          }
        }
      }
    }

    .file-name-text {
      display: inline-block;
      max-width: 200px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
  }
}
</style>