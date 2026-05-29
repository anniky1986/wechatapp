import { describe, it, expect, vi } from 'vitest'
import { createAsyncComponent } from '../asyncComponent'

vi.mock('../../components/common/SkeletonLoader.vue', () => ({
  default: {
    name: 'SkeletonLoader',
    props: ['type', 'rows'],
    template: '<div class="skeleton-loader"></div>',
  },
}))

describe('createAsyncComponent', () => {
  it('应该返回一个组件', () => {
    const loader = vi.fn().mockResolvedValue({
      name: 'TestComponent',
      template: '<div>Test</div>',
    })

    const component = createAsyncComponent(loader)
    expect(component).toBeDefined()
    expect(typeof component).toBe('object')
  })

  it('应该接受自定义 options', () => {
    const loader = vi.fn().mockResolvedValue({
      name: 'TestComponent',
      template: '<div>Test</div>',
    })

    const component = createAsyncComponent(loader, {
      type: 'card',
      rows: 10,
      delay: 500,
      timeout: 60000,
    })

    expect(component).toBeDefined()
  })

  it('应该使用默认 options', () => {
    const loader = vi.fn().mockResolvedValue({
      name: 'TestComponent',
      template: '<div>Test</div>',
    })

    const component = createAsyncComponent(loader)
    expect(component).toBeDefined()
  })

  it('不同的 loader 应返回不同的组件', () => {
    const loader1 = vi.fn().mockResolvedValue({ name: 'Comp1' })
    const loader2 = vi.fn().mockResolvedValue({ name: 'Comp2' })

    const comp1 = createAsyncComponent(loader1)
    const comp2 = createAsyncComponent(loader2)

    expect(comp1).not.toBe(comp2)
  })
})