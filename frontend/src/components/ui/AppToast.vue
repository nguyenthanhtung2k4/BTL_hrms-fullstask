<script setup lang="ts">
// AppToast — Global toast notifications with Dark Mode support and smooth animations

import { useToastStore } from '../../stores/toast'

const store = useToastStore()

const iconMap: Record<string, string> = {
  success: 'M5 13l4 4L19 7',
  error:   'M6 18L18 6M6 6l12 12',
  warning: 'M12 9v4m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z',
  info:    'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
}
</script>

<template>
  <Teleport to="body">
    <div class="toast-container">
      <TransitionGroup
        enter-active-class="toast-enter-active"
        enter-from-class="toast-enter-from"
        enter-to-class="toast-enter-to"
        leave-active-class="toast-leave-active"
        leave-from-class="toast-leave-from"
        leave-to-class="toast-leave-to"
      >
        <div
          v-for="toast in store.toasts"
          :key="toast.id"
          :class="['toast', `toast--${toast.type}`]"
        >
          <!-- Icon -->
          <div :class="['toast__icon', `toast__icon--${toast.type}`]">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
              <path stroke-linecap="round" stroke-linejoin="round" :d="iconMap[toast.type]" />
            </svg>
          </div>
          <!-- Message -->
          <span class="toast__msg">{{ toast.message }}</span>
          <!-- Close -->
          <button class="toast__close" @click="store.remove(toast.id)">
            <svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<style scoped>
.toast-container {
  position: fixed;
  bottom: 1.25rem;
  right: 1.25rem;
  z-index: 100;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  width: 20rem;
  max-width: calc(100vw - 2rem);
  pointer-events: none;
}

.toast {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  padding: 0.75rem 1rem;
  border-radius: var(--radius-md);
  border: 1px solid transparent;
  background-color: var(--bg-surface);
  box-shadow: var(--shadow-lg);
  pointer-events: auto;
  font-size: 0.875rem;
}

/* Variants */
.toast--success { border-color: var(--color-success); }
.toast--error   { border-color: var(--color-danger); }
.toast--warning { border-color: var(--color-warning); }
.toast--info    { border-color: var(--color-info); }

.toast__icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 1.5rem;
  height: 1.5rem;
  border-radius: 50%;
  flex-shrink: 0;
}

.toast__icon--success { background: var(--color-success-light); color: var(--color-success); }
.toast__icon--error   { background: var(--color-danger-light);  color: var(--color-danger); }
.toast__icon--warning { background: var(--color-warning-light); color: hsl(36, 70%, 35%); }
.toast__icon--info    { background: var(--color-info-light);    color: var(--color-info); }

[data-theme="dark"] .toast__icon--warning { color: hsl(45, 93%, 65%); }

.toast__msg {
  flex: 1;
  font-weight: 500;
  color: var(--text-primary);
  line-height: 1.4;
}

.toast__close {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 1.5rem;
  height: 1.5rem;
  border-radius: var(--radius-sm);
  border: none;
  background: transparent;
  color: var(--text-tertiary);
  cursor: pointer;
  opacity: 0.7;
  transition: opacity var(--transition-fast), background-color var(--transition-fast);
}
.toast__close:hover {
  opacity: 1;
  background-color: var(--bg-subtle);
}

/* Animations */
.toast-enter-active {
  transition: all 250ms cubic-bezier(0.16, 1, 0.3, 1);
}
.toast-leave-active {
  transition: all 200ms ease-in;
}
.toast-enter-from {
  opacity: 0;
  transform: translateX(1rem) translateY(0.5rem);
}
.toast-enter-to {
  opacity: 1;
  transform: translateX(0) translateY(0);
}
.toast-leave-from {
  opacity: 1;
  transform: translateX(0);
}
.toast-leave-to {
  opacity: 0;
  transform: translateX(100%);
}
</style>
