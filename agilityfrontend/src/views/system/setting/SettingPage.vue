<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import { SaveOutlined } from '@ant-design/icons-vue'
import {
  getSettingByGroup,
  updateSetting,
  type SystemSetting,
} from '../../../api/system/setting'

interface SettingItem {
  id: number
  name: string
  key: string
  value: string
  group: string
  remark: string
  editValue: string
  dirty: boolean
  valueType: 'string' | 'boolean' | 'number' | 'json'
}

const tabs = [
  { key: 'FileStorage', tab: '文件存储' },
  { key: 'Security', tab: '安全设置' },
  { key: 'Log', tab: '日志设置' },
  { key: 'Cache', tab: '缓存设置' },
  { key: 'Feature', tab: '功能设置' },
  { key: 'System', tab: '系统设置' },
  { key: 'Email', tab: '邮件设置' },
  { key: 'Tenant', tab: '租户设置' },
]

const activeTab = ref('FileStorage')
const allSettings = ref<Map<string, SettingItem[]>>(new Map())
const settingsLoading = ref(false)
const saveLoading = ref(false)

const currentSettings = computed(() => {
  return allSettings.value.get(activeTab.value) || []
})

function detectValueType(value: string): 'string' | 'boolean' | 'number' | 'json' {
  if (value === 'true' || value === 'false') return 'boolean'
  const trimmed = value.trim()
  if ((trimmed.startsWith('{') && trimmed.endsWith('}')) ||
      (trimmed.startsWith('[') && trimmed.endsWith(']'))) {
    return 'json'
  }
  if (/^-?\d+$/.test(trimmed)) return 'number'
  return 'string'
}

function parseJsonValue(value: string): string {
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

async function loadGroupSettings(group: string) {
  if (allSettings.value.has(group)) return
  settingsLoading.value = true
  try {
    const res: any = await getSettingByGroup(group)
    const data: SystemSetting[] = res.data ?? res
    const items: SettingItem[] = (data || []).map((s) => {
      const valueType = detectValueType(s.value)
      return {
        id: s.id,
        name: s.name,
        key: s.key,
        value: s.value,
        group: s.group || group,
        remark: s.remark,
        editValue: valueType === 'json' ? parseJsonValue(s.value) : s.value,
        dirty: false,
        valueType,
      }
    })
    allSettings.value.set(group, items)
  } catch {
    message.error(`加载"${group}"设置失败`)
  } finally {
    settingsLoading.value = false
  }
}

function handleTabChange(key: string) {
  activeTab.value = key
  loadGroupSettings(key)
}

function onValueChange(item: SettingItem, newValue: any) {
  const strValue = item.valueType === 'boolean'
    ? String(newValue)
    : String(newValue ?? '')
  item.editValue = strValue
  item.dirty = strValue !== item.value
}

async function handleSave() {
  const updates: { id: number; value: string }[] = []
  for (const [, items] of allSettings.value) {
    for (const item of items) {
      if (item.dirty) {
        let finalValue = item.editValue
        if (item.valueType === 'json') {
          try {
            JSON.parse(finalValue)
          } catch {
            message.error(`设置项 "${item.name}" 的 JSON 格式不正确`)
            return
          }
        }
        updates.push({ id: item.id, value: finalValue })
      }
    }
  }
  if (updates.length === 0) {
    message.info('没有需要保存的更改')
    return
  }
  saveLoading.value = true
  try {
    const promises = updates.map((u) => updateSetting(u.id, { value: u.value }))
    await Promise.all(promises)
    for (const [, items] of allSettings.value) {
      for (const item of items) {
        if (item.dirty) {
          item.value = item.editValue
          item.dirty = false
        }
      }
    }
    message.success('保存成功')
  } catch {
    message.error('保存失败')
  } finally {
    saveLoading.value = false
  }
}

function getSettingLabel(item: SettingItem): string {
  return item.remark ? `${item.name} (${item.key})` : item.name
}

function getSettingHint(item: SettingItem): string {
  return item.remark || ''
}

onMounted(() => {
  loadGroupSettings(activeTab.value)
})
</script>

<template>
  <div class="setting-page">
    <a-card class="main-card" :bordered="false">
      <a-tabs v-model:activeKey="activeTab" @change="handleTabChange">
        <a-tab-pane v-for="tab in tabs" :key="tab.key" :tab="tab.tab" />
      </a-tabs>

      <a-spin :spinning="settingsLoading">
        <a-form
          :label-col="{ span: 5 }"
          :wrapper-col="{ span: 17 }"
          class="setting-form"
        >
          <template v-if="currentSettings.length === 0 && !settingsLoading">
            <a-empty description="暂无设置项" style="padding: 40px 0" />
          </template>

          <template v-for="item in currentSettings" :key="item.id">
            <template v-if="item.valueType === 'boolean'">
              <a-form-item :label="item.name" :help="getSettingHint(item)">
                <a-switch
                  :checked="item.editValue === 'true'"
                  checked-children="启用"
                  un-checked-children="禁用"
                  @change="(val: boolean) => onValueChange(item, val)"
                />
              </a-form-item>
            </template>

            <template v-else-if="item.valueType === 'number'">
              <a-form-item :label="item.name" :help="getSettingHint(item)">
                <a-input-number
                  :value="Number(item.editValue)"
                  style="width: 100%; max-width: 360px"
                  @change="(val: number | null) => onValueChange(item, val)"
                />
              </a-form-item>
            </template>

            <template v-else-if="item.valueType === 'json'">
              <a-form-item :label="item.name" :help="getSettingHint(item)">
                <a-textarea
                  :value="item.editValue"
                  :rows="6"
                  style="max-width: 600px; font-family: monospace"
                  @change="(e: any) => onValueChange(item, e.target.value)"
                />
              </a-form-item>
            </template>

            <template v-else>
              <a-form-item :label="item.name" :help="getSettingHint(item)">
                <a-input
                  :value="item.editValue"
                  style="max-width: 480px"
                  @change="(e: any) => onValueChange(item, e.target.value)"
                />
              </a-form-item>
            </template>
          </template>
        </a-form>
      </a-spin>
    </a-card>

    <div class="save-bar">
      <a-button
        type="primary"
        size="large"
        :loading="saveLoading"
        @click="handleSave"
      >
        <template #icon>
          <SaveOutlined />
        </template>
        保存设置
      </a-button>
    </div>
  </div>
</template>

<style lang="less" scoped>
.setting-page {
  padding: 16px;

  .main-card {
    margin-bottom: 16px;

    .setting-form {
      margin-top: 16px;
      max-width: 800px;

      :deep(.ant-form-item) {
        margin-bottom: 20px;
      }

      :deep(.ant-form-item-label) {
        min-width: 140px;
      }
    }
  }

  .save-bar {
    text-align: center;
    padding: 16px 0;
  }
}
</style>