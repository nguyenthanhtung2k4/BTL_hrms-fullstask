<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'

const props = defineProps<{
  title?: string
  size?: 'sm' | 'md' | 'lg' | 'xl'
  closable?: boolean
}>()

const emit = defineEmits<{ close: [] }>()

const sizeClass: Record<string, string> = {
  sm: 'max-w-sm',
  md: 'max-w-lg',
  lg: 'max-w-2xl',
  xl: 'max-w-4xl',
}

function onEsc(e: KeyboardEvent) {
  if (e.key === 'Escape' && props.closable !== false) emit('close')
}
onMounted(() => document.addEventListener('keydown', onEsc))
onUnmounted(() => document.removeEventListener('keydown', onEsc))
</script>

<template>
  <!-- Backdrop -->
  <Teleport to="body">
    <div
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4 backdrop-blur-sm"
      @mousedown.self="closable !== false && $emit('close')"
    >
      <!-- Panel -->
      <div
        :class="[
          'relative flex max-h-[92vh] w-full flex-col overflow-hidden rounded-2xl bg-white shadow-[0_24px_60px_rgba(15,23,42,0.18)]',
          sizeClass[size ?? 'md'],
        ]"
        role="dialog"
        aria-modal="true"
      >
        <!-- Header -->
        <div
          v-if="title || closable !== false"
          class="flex flex-shrink-0 items-center justify-between px-6 pb-0 pt-6"
        >
          <h2 class="text-lg font-semibold text-slate-900">{{ title }}</h2>
          <button
            v-if="closable !== false"
            class="rounded-lg p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-700"
            type="button"
            @click="$emit('close')"
          >
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Body -->
        <div class="flex-1 overflow-y-auto px-6 py-6">
          <slot />
        </div>

        <!-- Footer -->
        <div
          v-if="$slots.footer"
          class="flex flex-shrink-0 justify-end gap-3 px-6 pb-6 pt-0"
        >
          <slot name="footer" />
        </div>
      </div>
    </div>
  </Teleport>
</template>
