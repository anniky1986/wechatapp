import type { RouteRecordRaw } from 'vue-router'

const LoginPage = () => import('../views/login/LoginPage.vue')

const staticRoutes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'Login',
    component: LoginPage,
    meta: { title: '登录', hideInMenu: true },
  },
]

export default staticRoutes