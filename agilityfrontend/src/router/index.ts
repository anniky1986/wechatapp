import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import NProgress from 'nprogress'
import 'nprogress/nprogress.css'
import staticRoutes from './routes'
import { getDynamicRoutes } from './generator'
import { useUserStore } from '../store/modules/user'
import pinia from '../store'

NProgress.configure({ showSpinner: false })

const router = createRouter({
  history: createWebHistory(),
  routes: [...staticRoutes],
})

router.beforeEach(async (to, _from, next) => {
  NProgress.start()

  const userStore = useUserStore(pinia)

  if (to.path === '/login') {
    NProgress.done()
    next()
    return
  }

  const token = userStore.token
  if (!token) {
    NProgress.done()
    next({ path: '/login', replace: true })
    return
  }

  if (!userStore.userInfo) {
    try {
      await userStore.init()
      await userStore.init
    } catch {
      userStore.logout()
      NProgress.done()
      return
    }
  }

  const dynamicRoutes = getDynamicRoutes()
  if (dynamicRoutes.length > 0) {
    const hasAdded = router.getRoutes().some((r) => r.name === 'Root')
    if (!hasAdded) {
      for (const route of dynamicRoutes) {
        router.addRoute(route)
      }
      next({ ...to, replace: true })
      return
    }
  }

  next()
})

router.afterEach(() => {
  NProgress.done()
})

export default router