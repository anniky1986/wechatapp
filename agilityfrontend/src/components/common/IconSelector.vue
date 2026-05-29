<script setup lang="ts">
import { ref } from 'vue'
import * as Icons from '@ant-design/icons-vue'

const selectedIcon = ref('')
const visible = ref(false)

const iconList = Object.keys(Icons)
  .filter((key) => key.endsWith('Outlined') || key.endsWith('Filled') || key.endsWith('TwoTone'))
  .slice(0, 100)

function open() {
  visible.value = true
}

function handleSelect(iconName: string) {
  selectedIcon.value = iconName
  visible.value = false
}

defineExpose({ open, selectedIcon })
</script>

<template>
  <div class="icon-selector">
    <a-input
      v-model:value="selectedIcon"
      readonly
      placeholder="请选择图标"
      @click="open"
    >
      <template #addonAfter>
        <component :is="(Icons as Record<string, unknown>)[selectedIcon]" v-if="selectedIcon" />
      </template>
    </a-input>

    <a-modal
      v-model:open="visible"
      title="选择图标"
      :footer="null"
      width="640px"
    >
      <div class="icon-grid">
        <div
          v-for="iconName in iconList"
          :key="iconName"
          class="icon-item"
          :class="{ active: selectedIcon === iconName }"
          @click="handleSelect(iconName)"
        >
          <component :is="(Icons as Record<string, unknown>)[iconName]" />
          <span class="icon-label">{{ iconName }}</span>
        </div>
      </div>
    </a-modal>
  </div>
</template>

<style lang="less" scoped>
.icon-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 12px;
  max-height: 400px;
  overflow-y: auto;
  padding: 8px;
}

.icon-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: 12px 4px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 24px;
  border: 1px solid transparent;

  &:hover {
    background-color: #f0f0f0;
  }

  &.active {
    border-color: #1677ff;
    background-color: #e6f4ff;
    color: #1677ff;
  }
}

.icon-label {
  font-size: 10px;
  color: #999;
  text-align: center;
  word-break: break-all;
  line-height: 1.2;
}
</style>