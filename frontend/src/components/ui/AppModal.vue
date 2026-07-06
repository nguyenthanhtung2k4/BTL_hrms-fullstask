<script setup lang="ts">
// AppModal — Dialog/modal with teleport, ESC to close, click-outside close, Dark Mode support

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
  <Teleport to="body">
    <!-- Backdrop -->
    <div
      class="modal-backdrop"
      @mousedown.self="closable !== false && $emit('close')"
    >
      <!-- Panel -->
      <div
        :class="['modal-panel', sizeClass[size ?? 'md']]"
        role="dialog"
        aria-modal="true"
      >
        <!-- Header -->
        <div v-if="title || closable !== false" class="modal-header">
          <h2 class="modal-title">{{ title }}</h2>
          <button
            v-if="closable !== false"
            class="modal-close-btn"
            type="button"
            @click="$emit('close')"
          >
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Body -->
        <div class="modal-body">
          <slot />
        </div>

        <!-- Footer -->
        <div v-if="$slots.footer" class="modal-footer">
          <slot name="footer" />
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  z-index: 50;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  background-color: rgba(0, 0, 0, 0.55);
  backdrop-filter: blur(2px);
  animation: backdropFadeIn 150ms ease-out;
}

@keyframes backdropFadeIn {
  from { opacity: 0; }
  to   { opacity: 1; }
}

.modal-panel {
  position: relative;
  width: 100%;
  border-radius: var(--radius-lg);
  background-color: var(--bg-surface);
  box-shadow: var(--shadow-xl);
  display: flex;
  flex-direction: column;
  max-height: 90vh;
  border: 1px solid var(--border);
  animation: modalSlideIn 200ms cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes modalSlideIn {
  from { opacity: 0; transform: scale(0.96) translateY(8px); }
  to   { opacity: 1; transform: scale(1)    translateY(0); }
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.5rem;
  border-bottom: 1px solid var(--border);
  flex-shrink: 0;
}

.modal-title {
  font-size: 0.9375rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.modal-close-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border-radius: var(--radius-sm);
  border: none;
  background: transparent;
  color: var(--text-tertiary);
  cursor: pointer;
  transition: background-color var(--transition-fast), color var(--transition-fast);
}
.modal-close-btn:hover {
  background-color: var(--bg-subtle);
  color: var(--text-primary);
}

.modal-body {
  overflow-y: auto;
  flex: 1;
  padding: 1.25rem 1.5rem;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border-top: 1px solid var(--border);
  flex-shrink: 0;
}
</style>
