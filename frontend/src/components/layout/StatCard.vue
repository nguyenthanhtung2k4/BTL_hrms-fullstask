<script setup lang="ts">
defineProps<{
  title: string
  value: string | number
  subtitle?: string
  color?: 'emerald' | 'blue' | 'amber' | 'red' | 'violet' | 'cyan'
  loading?: boolean
}>()

const colorMap: Record<string, string> = {
  emerald: 'text-emerald-600 bg-emerald-50',
  blue:    'text-blue-600 bg-blue-50',
  amber:   'text-amber-600 bg-amber-50',
  red:     'text-red-600 bg-red-50',
  violet:  'text-violet-600 bg-violet-50',
  cyan:    'text-cyan-600 bg-cyan-50',
}
</script>

<template>
  <div class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm hover:shadow-md transition-shadow">
    <div class="flex items-center justify-between">
      <span class="text-sm font-medium text-slate-500">{{ title }}</span>
      <div
        v-if="$slots.icon"
        :class="['rounded-lg p-2', colorMap[color ?? 'emerald']]"
      >
        <slot name="icon" />
      </div>
    </div>

    <div v-if="loading" class="mt-3 h-8 w-24 animate-pulse rounded bg-slate-200" />
    <div v-else class="mt-2">
      <div class="text-3xl font-bold text-slate-900">{{ value }}</div>
      <p v-if="subtitle" class="mt-1 text-xs text-slate-500">{{ subtitle }}</p>
    </div>
  </div>
</template>
