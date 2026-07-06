<script setup lang="ts">
// AppConfirm — Confirmation dialog with Dark Mode support and i18n-friendly text

defineProps<{
  title: string
  message?: string
  confirmText?: string
  cancelText?: string
  danger?: boolean
  loading?: boolean
}>()

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()
</script>

<template>
  <Teleport to="body">
    <div
      class="confirm-backdrop"
      @mousedown.self="emit('cancel')"
    >
      <div class="confirm-panel">
        <!-- Icon + Text -->
        <div class="confirm-body">
          <div :class="['confirm-icon', danger ? 'confirm-icon--danger' : 'confirm-icon--warning']">
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path
                stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M12 9v4m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"
              />
            </svg>
          </div>
          <div class="confirm-text">
            <h3 class="confirm-title">{{ title }}</h3>
            <p v-if="message" class="confirm-message">{{ message }}</p>
          </div>
        </div>

        <!-- Actions -->
        <div class="confirm-actions">
          <button
            class="confirm-btn confirm-btn--cancel"
            type="button"
            :disabled="loading"
            @click="emit('cancel')"
          >
            {{ cancelText ?? 'Hủy' }}
          </button>
          <button
            :class="['confirm-btn', danger ? 'confirm-btn--danger' : 'confirm-btn--primary']"
            type="button"
            :disabled="loading"
            @click="emit('confirm')"
          >
            <svg v-if="loading" class="h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
            {{ confirmText ?? 'Xác nhận' }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.confirm-backdrop {
  position: fixed;
  inset: 0;
  z-index: 60;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  background-color: rgba(0, 0, 0, 0.55);
  backdrop-filter: blur(2px);
  animation: backdropIn 150ms ease-out;
}

@keyframes backdropIn {
  from { opacity: 0; }
  to   { opacity: 1; }
}

.confirm-panel {
  width: 100%;
  max-width: 22rem;
  border-radius: var(--radius-lg);
  background-color: var(--bg-surface);
  border: 1px solid var(--border);
  box-shadow: var(--shadow-xl);
  padding: 1.25rem;
  animation: panelIn 200ms cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes panelIn {
  from { opacity: 0; transform: scale(0.94) translateY(10px); }
  to   { opacity: 1; transform: scale(1)    translateY(0); }
}

.confirm-body {
  display: flex;
  align-items: flex-start;
  gap: 0.875rem;
}

.confirm-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 50%;
}

.confirm-icon--warning {
  background-color: var(--color-warning-light);
  color: var(--color-warning);
}

.confirm-icon--danger {
  background-color: var(--color-danger-light);
  color: var(--color-danger);
}

.confirm-title {
  font-size: 0.9375rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.confirm-message {
  margin-top: 0.25rem;
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.confirm-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.625rem;
  margin-top: 1.25rem;
}

.confirm-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
  padding: 0 1rem;
  height: 2.25rem;
  border-radius: var(--radius-sm);
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: background-color var(--transition-fast), opacity var(--transition-fast);
  border: 1px solid transparent;
}
.confirm-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.confirm-btn--cancel {
  background-color: var(--bg-surface);
  color: var(--text-primary);
  border-color: var(--border-strong);
}
.confirm-btn--cancel:hover:not(:disabled) {
  background-color: var(--bg-subtle);
}

.confirm-btn--primary {
  background-color: var(--color-primary);
  color: white;
}
.confirm-btn--primary:hover:not(:disabled) {
  background-color: var(--color-primary-hover);
}

.confirm-btn--danger {
  background-color: var(--color-danger);
  color: white;
}
.confirm-btn--danger:hover:not(:disabled) {
  opacity: 0.88;
}
</style>
