import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import { router } from './router'
import { i18n } from './locales'
import { useTheme } from './composables/useTheme'
import './style.css'

// Init theme before mount to prevent flash
const { initTheme } = useTheme()
initTheme()

createApp(App)
  .use(createPinia())
  .use(router)
  .use(i18n)
  .mount('#app')
