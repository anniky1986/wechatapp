export interface PageParams {
  page: number
  pageSize: number
  sortField?: string
  sortOrder?: string
}

export interface PageResult<T> {
  total: number
  items: T[]
}

export interface ApiResponse<T = unknown> {
  code: number
  message: string
  data: T
  timestamp: number
}

export interface UserInfo {
  id: number
  userName: string
  nickName: string
  avatar: string
  email: string
  phone: string
  deptId: number
  deptName: string
  roles: string[]
  permissions: string[]
}

export interface MenuItem {
  id: number
  parentId: number
  name: string
  path: string
  component: string
  redirect: string
  icon: string
  permission: string
  menuType: number
  orderNo: number
  isHide: boolean
  keepAlive: boolean
  status: number
  isFrame: boolean
  frameSrc: string
  children: MenuItem[]
}

export interface InitData {
  userInfo: UserInfo
  menus: MenuItem[]
  permissions: string[]
  systemConfig: Record<string, unknown>
  dictData: Record<string, unknown>
}

export interface LoginParams {
  userName: string
  password: string
}

export interface LoginResult {
  token: string
  refreshToken: string
  userInfo: UserInfo
  menus: MenuItem[]
  permissions: string[]
}