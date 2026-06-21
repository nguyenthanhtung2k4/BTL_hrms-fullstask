<script setup lang="ts">
// AppButton — Reusable button with variants, sizes, loading state, and Dark Mode support

defineProps<{
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost' | 'success'
  size?: 'xs' | 'sm' | 'md' | 'lg'
  loading?: boolean
  disabled?: boolean
  type?: 'button' | 'submit' | 'reset'
}>()
</script>

<template>
  <button
    :type="type ?? 'button'"
    :disabled="disabled || loading"
    :class="[
      'btn',
      `btn--${variant ?? 'primary'}`,
      `btn--${size ?? 'md'}`,
      (disabled || loading) ? 'btn--disabled' : '',
    ]"
  >
    <!-- Loading spinner -->
    <svg
      v-if="loading"
      class="animate-spin flex-shrink-0"
      :class="size === 'xs' || size === 'sm' ? 'h-3 w-3' : 'h-4 w-4'"
      xmlns="http://www.w3.org/2000/svg"
      fill="none"
      viewBox="0 0 24 24"
    >
      <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
    </svg>
    <slot />
  </button>
</template>

<style scoped>
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-md);
  font-weight: 500;
  font-size: 0.875rem;
  line-height: 1;
  border: 1px solid transparent;
  cursor: pointer;
  transition: background-color var(--transition-fast), border-color var(--transition-fast),
    color var(--transition-fast), opacity var(--transition-fast), box-shadow var(--transition-fast);
  white-space: nowrap;
  outline: none;
  gap: 0.5rem;
}
.btn:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}

/* Sizes */
.btn--xs  { height: 1.625rem; padding: 0 0.5rem;  font-size: 0.75rem;  gap: 0.25rem; }
.btn--sm  { height: 2rem;     padding: 0 0.75rem; font-size: 0.75rem;  gap: 0.375rem; }
.btn--md  { height: 2.25rem;  padding: 0 1rem;    font-size: 0.875rem; }
.btn--lg  { height: 2.75rem;  padding: 0 1.25rem; font-size: 0.875rem; }

/* Variants */
.btn--primary {
  background-color: var(--color-primary);
  color: var(--text-inverse);
  border-color: var(--color-primary);
}
.btn--primary:hover:not(:disabled) {
  background-color: var(--color-primary-hover);
  border-color: var(--color-primary-hover);
}

.btn--secondary {
  background-color: var(--bg-surface);
  color: var(--text-primary);
  border-color: var(--border-strong);
}
.btn--secondary:hover:not(:disabled) {
  background-color: var(--bg-subtle);
}

.btn--danger {
  background-color: var(--color-danger);
  color: white;
  border-color: var(--color-danger);
}
.btn--danger:hover:not(:disabled) {
  opacity: 0.88;
}

.btn--ghost {
  background-color: transparent;
  color: var(--text-secondary);
  border-color: transparent;
}
.btn--ghost:hover:not(:disabled) {
  background-color: var(--bg-subtle);
  color: var(--text-primary);
}

.btn--success {
  background-color: var(--color-success);
  color: white;
  border-color: var(--color-success);
}
.btn--success:hover:not(:disabled) {
  opacity: 0.88;
}

/* Disabled state */
.btn--disabled {
  opacity: 0.5;
  cursor: not-allowed;
  pointer-events: none;
}
</style>
