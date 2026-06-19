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
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm"
      @mousedown.self="closable !== false && $emit('close')"
    >
      <!-- Panel -->
      <div
        :class="[
          'relative w-full rounded-xl bg-white shadow-2xl flex flex-col max-h-[90vh]',
          sizeClass[size ?? 'md'],
        ]"
        role="dialog"
        aria-modal="true"
      >
        <!-- Header -->
        <div
          v-if="title || closable !== false"
          class="flex items-center justify-between border-b border-slate-200 px-6 py-4 flex-shrink-0"
        >
          <h2 class="text-base font-semibold text-slate-900">{{ title }}</h2>
          <button
            v-if="closable !== false"
            class="rounded p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700 transition-colors"
            type="button"
            @click="$emit('close')"
          >
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Body -->
        <div class="overflow-y-auto flex-1 px-6 py-5">
          <slot />
        </div>

        <!-- Footer -->
        <div
          v-if="$slots.footer"
          class="flex justify-end gap-3 border-t border-slate-200 px-6 py-4 flex-shrink-0"
        >
          <slot name="footer" />
        </div>
      </div>
    </div>
  </Teleport>
</template>
