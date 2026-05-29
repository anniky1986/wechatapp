import axios from 'axios'
import type { AxiosInstance, AxiosResponse } from 'axios'
import { message } from 'ant-design-vue'
import { ResultEnum } from '../enums/httpEnum'
import router from '../router'

const instance: AxiosInstance = axios.create({
  baseURL: '/api',
  timeout: 30000,
  headers: { 'Content-Type': 'application/json;charset=UTF-8' },
})

instance.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

instance.interceptors.response.use(
  (response: AxiosResponse) => {
    const { data } = response
    if (data.code === ResultEnum.TIMEOUT) {
      localStorage.clear()
      router.push('/login')
      return Promise.reject(new Error('Token expired'))
    }
    return data
  },
  (error) => {
    if (error.response?.status === 401) {
      localStorage.clear()
      router.push('/login')
    }
    message.error(error.message || 'Request failed')
    return Promise.reject(error)
  }
)

export default instance