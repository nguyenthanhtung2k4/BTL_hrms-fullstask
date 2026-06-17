<script setup lang="ts" generic="T">
// AppTable — Generic table với loading skeleton và empty state

defineProps<{
  loading?: boolean
  columns: { key: string; label: string; class?: string }[]
  rows: T[]
  emptyText?: string
  rowKey?: keyof T
}>()
</script>

<template>
  <div class="overflow-x-auto rounded-lg border border-slate-200 bg-white">
    <table class="w-full min-w-max text-left text-sm">
      <thead class="bg-slate-50 text-xs uppercase tracking-wide text-slate-500">
        <tr>
          <th
            v-for="col in columns"
            :key="col.key"
            :class="['px-4 py-3 font-medium', col.class ?? '']"
          >
            {{ col.label }}
          </th>
        </tr>
      </thead>

      <tbody class="divide-y divide-slate-100">
        <!-- Loading skeleton -->
        <template v-if="loading">
          <tr v-for="n in 5" :key="n" class="animate-pulse">
            <td v-for="col in columns" :key="col.key" class="px-4 py-3">
              <div class="h-4 w-3/4 rounded bg-slate-200"></div>
            </td>
          </tr>
        </template>

        <!-- Empty state -->
        <tr v-else-if="rows.length === 0">
          <td :colspan="columns.length" class="px-4 py-12 text-center text-slate-400">
            <div class="flex flex-col items-center gap-2">
              <svg class="h-10 w-10 text-slate-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                  d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
              </svg>
              <span class="text-sm">{{ emptyText ?? 'Không có dữ liệu' }}</span>
            </div>
          </td>
        </tr>

        <!-- Data rows -->
        <template v-else>
          <tr
            v-for="(row, i) in rows"
            :key="rowKey ? String(row[rowKey]) : i"
            class="hover:bg-slate-50 transition-colors"
          >
            <slot :row="row" :index="i" />
          </tr>
        </template>
      </tbody>
    </table>
  </div>
</template>
