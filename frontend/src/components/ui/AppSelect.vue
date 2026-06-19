<script setup lang="ts">
defineProps<{
  label?: string
  modelValue?: string | number | null
  required?: boolean
  disabled?: boolean
  error?: string
  hint?: string
  id?: string
  placeholder?: string
}>()

defineEmits<{
  'update:modelValue': [value: string]
}>()
</script>

<template>
  <div class="flex flex-col gap-1">
    <label v-if="label" :for="id" class="text-sm font-medium text-slate-700">
      {{ label }}
      <span v-if="required" class="text-red-500 ml-0.5">*</span>
    </label>
    <select
      :id="id"
      :value="modelValue ?? ''"
      :required="required"
      :disabled="disabled"
      :class="[
        'h-9 w-full rounded border px-3 text-sm outline-none transition-colors appearance-none bg-white',
        error
          ? 'border-red-400 focus:border-red-500 focus:ring-1 focus:ring-red-500'
          : 'border-slate-300 focus:border-emerald-500 focus:ring-1 focus:ring-emerald-500',
        disabled ? 'cursor-not-allowed bg-slate-50 text-slate-500' : '',
      ]"
      @change="$emit('update:modelValue', ($event.target as HTMLSelectElement).value)"
    >
      <option v-if="placeholder" value="" disabled>{{ placeholder }}</option>
      <slot />
    </select>
    <p v-if="error" class="text-xs text-red-500">{{ error }}</p>
    <p v-else-if="hint" class="text-xs text-slate-500">{{ hint }}</p>
  </div>
</template>
