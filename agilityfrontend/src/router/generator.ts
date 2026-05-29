import type { RouteRecordRaw } from 'vue-router'
import type { MenuItem } from '../types'
import BasicLayout from '../layouts/BasicLayout.vue'

const viewModules: Record<string, () => Promise<unknown>> = {
  'dashboard/Dashboard': () => import(/* webpackChunkName: "dashboard" */ '../views/dashboard/Dashboard.vue'),
  'system/user/UserList': () => import(/* webpackChunkName: "system-user" */ '../views/system/user/UserList.vue'),
  'system/role/RoleList': () => import(/* webpackChunkName: "system-role" */ '../views/system/role/RoleList.vue'),
  'system/menu/MenuList': () => import(/* webpackChunkName: "system-menu" */ '../views/system/menu/MenuList.vue'),
  'system/dept/DeptList': () => import(/* webpackChunkName: "system-dept" */ '../views/system/dept/DeptList.vue'),
  'system/tenant/TenantList': () => import(/* webpackChunkName: "system-tenant" */ '../views/system/tenant/TenantList.vue'),
  'system/dict/DictList': () => import(/* webpackChunkName: "system-dict" */ '../views/system/dict/DictList.vue'),
  'system/log/OperationLog': () => import(/* webpackChunkName: "system-log" */ '../views/system/log/OperationLog.vue'),
  'system/log/LoginLog': () => import(/* webpackChunkName: "system-log" */ '../views/system/log/LoginLog.vue'),
  'system/setting/SettingPage': () => import(/* webpackChunkName: "system-setting" */ '../views/system/setting/SettingPage.vue'),
  'filemanager/FileManager': () => import(/* webpackChunkName: "filemanager" */ '../views/filemanager/FileManager.vue'),
}

function menuTypeDirectory(item: MenuItem): boolean {
  return item.menuType === 1
}

function menuTypeMenu(item: MenuItem): boolean {
  return item.menuType === 2
}

function buildRoutes(menus: MenuItem[]): RouteRecordRaw[] {
  const routes: RouteRecordRaw[] = []

  for (const menu of menus) {
    if (menu.isHide) continue

    if (menuTypeDirectory(menu) && menu.children && menu.children.length > 0) {
      const children = buildRoutes(menu.children)
      if (children.length > 0) {
        routes.push({
          path: menu.path,
          name: menu.name,
          ...(menu.redirect ? { redirect: menu.redirect } : {}),
          meta: {
            title: menu.name,
            icon: menu.icon,
            keepAlive: menu.keepAlive,
          },
          children,
        } as RouteRecordRaw)
      }
    } else if (menuTypeMenu(menu)) {
      const componentPath = menu.component
      const viewLoader = componentPath ? viewModules[componentPath] : undefined

      if (viewLoader && componentPath) {
        routes.push({
          path: menu.path,
          name: menu.name,
          component: viewLoader,
          meta: {
            title: menu.name,
            icon: menu.icon,
            keepAlive: menu.keepAlive,
            permission: menu.permission,
            isFrame: menu.isFrame,
            frameSrc: menu.frameSrc,
          },
        })
      } else {
        routes.push({
          path: menu.path,
          name: menu.name,
          ...(menu.redirect ? { redirect: menu.redirect } : {}),
          meta: {
            title: menu.name,
            icon: menu.icon,
            keepAlive: menu.keepAlive,
            permission: menu.permission,
            isFrame: menu.isFrame,
            frameSrc: menu.frameSrc,
          },
        } as RouteRecordRaw)
      }
    }
  }

  return routes
}

let dynamicRoutes: RouteRecordRaw[] = []

export async function generateRoutes(menus: MenuItem[]): Promise<RouteRecordRaw[]> {
  const children = buildRoutes(menus)
  dynamicRoutes = [
    {
      path: '/',
      name: 'Root',
      component: BasicLayout,
      redirect: children.length > 0 ? children[0].path : '/dashboard',
      children: [
        ...children,
        {
          path: '/:pathMatch(.*)*',
          name: 'NotFound',
          component: () => import(/* webpackChunkName: "error" */ '../views/system/error/NotFound.vue'),
          meta: { title: '404', hideInMenu: true },
        },
      ],
    },
  ]
  return dynamicRoutes
}

export function getDynamicRoutes(): RouteRecordRaw[] {
  return dynamicRoutes
}