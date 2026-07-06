<script setup lang="ts">
// AppInput — Form input with label, error state, and Dark Mode support

defineProps<{
  label?: string
  modelValue?: string | number | null
  type?: string
  placeholder?: string
  required?: boolean
  disabled?: boolean
  readonly?: boolean
  error?: string
  hint?: string
  id?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

function handleInput(event: Event) {
  emit('update:modelValue', (event.target as HTMLInputElement).value)
}
</script>

<template>
  <div class="app-input-wrap">
    <label v-if="label" :for="id" class="app-input-label">
      {{ label }}
      <span v-if="required" class="app-input-required">*</span>
    </label>
    <input
      :id="id"
      :type="type ?? 'text'"
      :value="modelValue ?? ''"
      :placeholder="placeholder"
      :required="required"
      :disabled="disabled"
      :readonly="readonly"
      :class="['app-input', error ? 'app-input--error' : '', (disabled || readonly) ? 'app-input--disabled' : '']"
      @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
    />
    <p v-if="error" class="app-input-hint app-input-hint--error">{{ error }}</p>
    <p v-else-if="hint" class="app-input-hint">{{ hint }}</p>
  </div>
</template>

<style scoped>
.app-input-wrap {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.app-input-label {
  font-size: 0.8125rem;
  font-weight: 500;
  color: var(--text-primary);
}

.app-input-required {
  color: var(--color-danger);
  margin-left: 2px;
}

.app-input {
  height: 2.25rem;
  width: 100%;
  border-radius: var(--radius-sm);
  border: 1px solid var(--border-strong);
  background-color: var(--bg-surface);
  color: var(--text-primary);
  padding: 0 0.75rem;
  font-size: 0.875rem;
  outline: none;
  transition: border-color var(--transition-fast), box-shadow var(--transition-fast),
    background-color var(--transition-base);
}

.app-input::placeholder {
  color: var(--text-tertiary);
}

.app-input:focus {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-primary) 15%, transparent);
}

.app-input--error {
  border-color: var(--color-danger) !important;
  background-color: var(--color-danger-light);
}

.app-input--error:focus {
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-danger) 15%, transparent) !important;
}

.app-input--disabled {
  background-color: var(--bg-muted);
  color: var(--text-tertiary);
  cursor: not-allowed;
}

.app-input-hint {
  font-size: 0.75rem;
  color: var(--text-tertiary);
}

.app-input-hint--error {
  color: var(--color-danger);
}
</style>
