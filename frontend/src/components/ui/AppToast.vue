<script setup lang="ts">
import { useToastStore } from '../../stores/toast'

const store = useToastStore()

const iconMap: Record<string, string> = {
  success: 'M5 13l4 4L19 7',
  error:   'M6 18L18 6M6 6l12 12',
  warning: 'M12 9v4m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z',
  info:    'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
}

const colorMap: Record<string, string> = {
  success: 'bg-emerald-50 border-emerald-300 text-emerald-800',
  error:   'bg-red-50 border-red-300 text-red-800',
  warning: 'bg-amber-50 border-amber-300 text-amber-800',
  info:    'bg-blue-50 border-blue-300 text-blue-800',
}

const iconColorMap: Record<string, string> = {
  success: 'text-emerald-600',
  error:   'text-red-600',
  warning: 'text-amber-600',
  info:    'text-blue-600',
}
</script>

<template>
  <Teleport to="body">
    <div class="fixed bottom-5 right-5 z-[100] flex flex-col gap-2 max-w-sm w-full pointer-events-none">
      <TransitionGroup
        enter-active-class="transition-all duration-300 ease-out"
        enter-from-class="opacity-0 translate-y-4"
        enter-to-class="opacity-100 translate-y-0"
        leave-active-class="transition-all duration-200 ease-in"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0 translate-x-4"
      >
        <div
          v-for="toast in store.toasts"
          :key="toast.id"
          :class="[
            'flex items-start gap-3 rounded-lg border px-4 py-3 shadow-lg pointer-events-auto',
            colorMap[toast.type],
          ]"
        >
          <svg
            :class="['h-5 w-5 flex-shrink-0 mt-0.5', iconColorMap[toast.type]]"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            stroke-width="2"
          >
            <path stroke-linecap="round" stroke-linejoin="round" :d="iconMap[toast.type]" />
          </svg>
          <span class="flex-1 text-sm font-medium">{{ toast.message }}</span>
          <button
            class="ml-auto flex-shrink-0 rounded p-0.5 opacity-60 hover:opacity-100"
            @click="store.remove(toast.id)"
          >
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>
