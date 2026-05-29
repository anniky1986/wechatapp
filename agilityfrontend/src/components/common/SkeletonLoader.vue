<script setup lang="ts">
defineProps<{
  type?: 'card' | 'table' | 'list' | 'form'
  rows?: number
}>()
</script>

<template>
  <div class="skeleton-loader">
    <template v-if="type === 'card'">
      <div class="skeleton-card">
        <a-skeleton active :paragraph="{ rows: 1 }" />
        <a-skeleton active :paragraph="{ rows: 2 }" />
      </div>
    </template>

    <template v-else-if="type === 'table'">
      <a-skeleton active :paragraph="{ rows: rows || 5 }" />
    </template>

    <template v-else-if="type === 'list'">
      <div v-for="i in rows || 5" :key="i" class="skeleton-list-item">
        <a-skeleton active avatar :paragraph="{ rows: 1 }" />
      </div>
    </template>

    <template v-else-if="type === 'form'">
      <a-skeleton active :paragraph="{ rows: rows || 6 }" />
    </template>

    <template v-else>
      <a-skeleton active :paragraph="{ rows: rows || 3 }" />
    </template>
  </div>
</template>

<style lang="less" scoped>
.skeleton-loader {
  padding: 16px;
}

.skeleton-card {
  display: flex;
  gap: 24px;

  > * {
    flex: 1;
  }
}

.skeleton-list-item {
  padding: 12px 0;
  border-bottom: 1px solid #f0f0f0;

  &:last-child {
    border-bottom: none;
  }
}
</style>