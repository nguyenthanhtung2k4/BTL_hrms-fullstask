<script setup lang="ts">
import { RouterLink } from 'vue-router'

defineProps<{
  title: string
  subtitle?: string
  breadcrumbs?: { label: string; to?: string }[]
}>()
</script>

<template>
  <div class="mb-6">
    <!-- Breadcrumb -->
    <nav v-if="breadcrumbs?.length" class="mb-2 flex items-center gap-1.5 text-xs text-slate-500">
      <template v-for="(crumb, i) in breadcrumbs" :key="i">
        <span v-if="i > 0" class="select-none">›</span>
        <RouterLink v-if="crumb.to" :to="crumb.to" class="hover:text-slate-900 transition-colors">
          {{ crumb.label }}
        </RouterLink>
        <span v-else class="text-slate-700 font-medium">{{ crumb.label }}</span>
      </template>
    </nav>

    <div class="flex flex-wrap items-center justify-between gap-3">
      <div>
        <h1 class="text-xl font-bold text-slate-900">{{ title }}</h1>
        <p v-if="subtitle" class="mt-0.5 text-sm text-slate-500">{{ subtitle }}</p>
      </div>
      <!-- Action buttons slot -->
      <div v-if="$slots.actions" class="flex items-center gap-2">
        <slot name="actions" />
      </div>
    </div>
  </div>
</template>
