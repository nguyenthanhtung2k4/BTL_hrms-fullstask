import { onBeforeRouteLeave } from 'vue-router'
import { onBeforeUnmount, type Ref } from 'vue'

export function useFormGuard(isDirty: Ref<boolean>) {
  // Navigation guard within the app
  onBeforeRouteLeave((_to, _from, next) => {
    if (isDirty.value) {
      const confirmLeave = window.confirm('Bạn có thay đổi chưa lưu. Bạn có chắc chắn muốn rời khỏi trang này?')
      if (confirmLeave) {
        next()
      } else {
        next(false)
      }
    } else {
      next()
    }
  })

  // Guard for page reload/tab close
  const handleBeforeUnload = (e: BeforeUnloadEvent) => {
    if (isDirty.value) {
      e.preventDefault()
      e.returnValue = 'Bạn có thay đổi chưa lưu. Bạn có chắc chắn muốn rời khỏi trang này?'
    }
  }

  window.addEventListener('beforeunload', handleBeforeUnload)

  onBeforeUnmount(() => {
    window.removeEventListener('beforeunload', handleBeforeUnload)
  })
}
