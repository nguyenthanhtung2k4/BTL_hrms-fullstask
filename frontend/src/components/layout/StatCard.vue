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
  blue:    'text-[#2563EB] bg-blue-50',
  amber:   'text-amber-600 bg-amber-50',
  red:     'text-red-600 bg-red-50',
  violet:  'text-violet-600 bg-violet-50',
  cyan:    'text-cyan-600 bg-cyan-50',
}
</script>

<template>
  <div class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm transition transform duration-200 hover:-translate-y-1 hover:shadow-lg">
    <div class="flex items-center justify-between">
      <span class="text-sm font-semibold text-slate-500">{{ title }}</span>
      <div v-if="$slots.icon" :class="['grid h-10 w-10 place-items-center rounded-lg', colorMap[color ?? 'blue']]">
        <slot name="icon" />
      </div>
    </div>

    <div v-if="loading" class="mt-4 h-9 w-24 animate-pulse rounded bg-slate-200" />
    <div v-else class="mt-3">
      <div class="text-3xl font-extrabold tracking-tight text-slate-900">{{ value }}</div>
      <p v-if="subtitle" class="mt-1 text-sm text-slate-500">{{ subtitle }}</p>
    </div>
  </div>
</template>
