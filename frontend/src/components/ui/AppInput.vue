<script setup lang="ts">
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
  <div class="flex flex-col space-y-1.5">
    <label v-if="label" :for="id" class="text-sm font-medium text-slate-700">
      {{ label }}
      <span v-if="required" class="ml-1 text-red-500">*</span>
    </label>
    <input
      :id="id"
      :type="type ?? 'text'"
      :value="modelValue ?? ''"
      :placeholder="placeholder"
      :required="required"
      :disabled="disabled"
      :readonly="readonly"
      :class="[
        'h-10 w-full rounded-lg border border-slate-200 bg-white px-3.5 text-sm text-slate-900 outline-none transition-all duration-200',
        'placeholder:text-slate-400',
        error
          ? 'border-red-400 bg-red-50 focus:border-red-500 focus:ring-4 focus:ring-red-100'
          : 'focus:border-blue-500 focus:ring-4 focus:ring-blue-100',
        (disabled || readonly) ? 'cursor-not-allowed bg-slate-50 text-slate-500' : '',
      ]"
      @input="handleInput"
    />
    <p v-if="error" class="text-xs text-red-500">{{ error }}</p>
    <p v-else-if="hint" class="text-xs text-slate-500">{{ hint }}</p>
  </div>
</template>
