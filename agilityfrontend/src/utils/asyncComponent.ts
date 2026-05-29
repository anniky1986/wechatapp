import { defineAsyncComponent, type Component, h } from 'vue'
import SkeletonLoader from '../components/common/SkeletonLoader.vue'

interface AsyncComponentOptions {
  type?: 'card' | 'table' | 'list' | 'form'
  rows?: number
  delay?: number
  timeout?: number
}

export function createAsyncComponent(
  loader: () => Promise<Component>,
  options: AsyncComponentOptions = {},
) {
  const { type = 'table', rows = 5, delay = 200, timeout = 30000 } = options

  return defineAsyncComponent({
    loader,
    loadingComponent: h(SkeletonLoader, { type, rows }),
    delay,
    timeout,
    errorComponent: h('div', { class: 'async-error' }, '组件加载失败，请刷新页面重试'),
    suspensible: false,
  })
}