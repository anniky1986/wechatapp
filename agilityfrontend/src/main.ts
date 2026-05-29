import { createApp } from 'vue'
import Antd from 'ant-design-vue'
import 'ant-design-vue/dist/reset.css'
import dayjs from 'dayjs'
import 'dayjs/locale/zh-cn'
import App from './App.vue'
import pinia from './store'
import router from './router'
import './style/index.less'

dayjs.locale('zh-cn')

const app = createApp(App)
app.use(pinia)
app.use(router)
app.use(Antd)
app.mount('#app')