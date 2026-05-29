import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import IconSelector from '../../common/IconSelector.vue'

function stubAInput() {
  return {
    template: '<input class="mock-input" />',
    props: ['value', 'placeholder', 'readonly'],
  }
}

function stubAModal() {
  return {
    template: '<div><slot /></div>',
    props: ['open', 'title', 'footer', 'width'],
  }
}

describe('IconSelector', () => {
  it('应该正常渲染组件', () => {
    const wrapper = mount(IconSelector, {
      global: {
        stubs: {
          'a-input': stubAInput(),
          'a-modal': stubAModal(),
        },
      },
    })
    expect(wrapper.exists()).toBe(true)
    expect(wrapper.find('.icon-selector').exists()).toBe(true)
  })

  it('应该渲染 input 组件', () => {
    const wrapper = mount(IconSelector, {
      global: {
        stubs: {
          'a-input': stubAInput(),
          'a-modal': stubAModal(),
        },
      },
    })
    expect(wrapper.find('.mock-input').exists()).toBe(true)
  })

  it('点击 input 应该打开 modal', async () => {
    const wrapper = mount(IconSelector, {
      global: {
        stubs: {
          'a-input': {
            template: '<input class="mock-input" @click="$emit(\'click\')" />',
            props: ['value', 'placeholder', 'readonly'],
            emits: ['click'],
          },
          'a-modal': {
            template: '<div v-if="open" class="mock-modal"><slot /></div>',
            props: ['open', 'title', 'footer', 'width'],
          },
        },
      },
    })

    expect(wrapper.find('.mock-modal').exists()).toBe(false)

    await wrapper.find('.mock-input').trigger('click')

    expect(wrapper.find('.mock-modal').exists()).toBe(true)
  })
})