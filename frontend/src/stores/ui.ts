import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useUiStore = defineStore('ui', () => {
  const sidebarCollapsed = ref(false)
  const globalLoading = ref(false)

  function toggleSidebar() {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }

  function setGlobalLoading(val: boolean) {
    globalLoading.value = val
  }

  return { sidebarCollapsed, globalLoading, toggleSidebar, setGlobalLoading }
})
