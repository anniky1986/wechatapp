/// <reference types="vite/client" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<object, object, unknown>
  export default component
}

declare module 'nprogress' {
  interface NProgressOptions {
    minimum?: number
    template?: string
    easing?: string
    speed?: number
    trickle?: boolean
    trickleSpeed?: number
    showSpinner?: boolean
    parent?: string
  }

  interface NProgress {
    version: string
    settings: NProgressOptions
    configure(options: Partial<NProgressOptions>): NProgress
    start(): NProgress
    done(): NProgress
    set(value: number): NProgress
    inc(amount?: number): NProgress
  }

  const nprogress: NProgress
  export default nprogress
}