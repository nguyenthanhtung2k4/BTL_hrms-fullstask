import { defineStore } from 'pinia'
import { ref } from 'vue'

export type ToastType = 'success' | 'error' | 'warning' | 'info'

export interface Toast {
  id: number
  type: ToastType
  message: string
  duration?: number
}

export const useToastStore = defineStore('toast', () => {
  const toasts = ref<Toast[]>([])
  let idCounter = 0

  function add(type: ToastType, message: string, duration = 3500) {
    const id = ++idCounter
    toasts.value.push({ id, type, message, duration })
    setTimeout(() => remove(id), duration)
  }

  function remove(id: number) {
    const idx = toasts.value.findIndex((t) => t.id === id)
    if (idx !== -1) toasts.value.splice(idx, 1)
  }

  const success = (message: string, duration?: number) => add('success', message, duration)
  const error = (message: string, duration?: number) => add('error', message, duration)
  const warning = (message: string, duration?: number) => add('warning', message, duration)
  const info = (message: string, duration?: number) => add('info', message, duration)

  return { toasts, success, error, warning, info, remove }
})
