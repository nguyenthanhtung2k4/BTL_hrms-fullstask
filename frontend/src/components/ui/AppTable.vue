<script setup lang="ts" generic="T">
// AppTable — Generic table với loading skeleton, empty state và sắp xếp cột thông minh
import { ref, computed } from 'vue'

const props = defineProps<{
  loading?: boolean
  columns: { key: string; label: string; class?: string }[]
  rows: T[]
  emptyText?: string
  rowKey?: keyof T
}>()

const sortKey = ref<string | null>(null)
const sortOrder = ref<'asc' | 'desc' | null>(null)

function handleSort(key: string) {
  if (key === 'actions' || !key) return
  
  if (sortKey.value === key) {
    if (sortOrder.value === 'asc') {
      sortOrder.value = 'desc'
    } else if (sortOrder.value === 'desc') {
      sortOrder.value = null
      sortKey.value = null
    } else {
      sortOrder.value = 'asc'
    }
  } else {
    sortKey.value = key
    sortOrder.value = 'asc'
  }
}

const sortedRows = computed(() => {
  if (!sortKey.value || !sortOrder.value) return props.rows
  
  const key = sortKey.value
  const order = sortOrder.value === 'asc' ? 1 : -1
  
  return [...props.rows].sort((a: any, b: any) => {
    let valA = a[key]
    let valB = b[key]
    
    // Giải quyết các trường lồng nhau hoặc quy chuẩn hóa dữ liệu
    if (key === 'employee' && a.employeeName) { valA = a.employeeName; valB = b.employeeName }
    else if (key === 'shift' && a.shiftName) { valA = a.shiftName; valB = b.shiftName }
    else if (key === 'department' && a.departmentName) { valA = a.departmentName; valB = b.departmentName }
    else if (key === 'position' && a.positionName) { valA = a.positionName; valB = b.positionName }
    
    if (valA === undefined || valA === null) valA = ''
    if (valB === undefined || valB === null) valB = ''
    
    // Nếu là chuỗi, dùng localeCompare hỗ trợ tiếng Việt
    if (typeof valA === 'string' && typeof valB === 'string') {
      return valA.localeCompare(valB, 'vi', { numeric: true }) * order
    }
    
    if (valA < valB) return -1 * order
    if (valA > valB) return 1 * order
    return 0
  })
})
</script>

<template>
  <div class="overflow-x-auto rounded-2xl border border-slate-200 bg-white shadow-sm transition-all duration-300">
    <table class="w-full min-w-max text-left text-sm border-collapse">
      <thead class="bg-slate-50 border-b border-slate-150 text-xs uppercase tracking-wider text-slate-500 font-semibold select-none">
        <tr>
          <th
            v-for="col in columns"
            :key="col.key"
            :class="[
              'px-5 py-3.5 transition-colors duration-250',
              col.key !== 'actions' && col.key ? 'cursor-pointer hover:bg-slate-100 hover:text-slate-800' : '',
              col.class ?? ''
            ]"
            @click="handleSort(col.key)"
          >
            <div class="flex items-center gap-1.5">
              <span>{{ col.label }}</span>
              
              <!-- Icon Sắp xếp -->
              <span v-if="col.key !== 'actions' && col.key" class="inline-flex flex-col text-[10px] text-slate-400">
                <svg
                  v-if="sortKey !== col.key || sortOrder === 'asc'"
                  class="h-2 w-2"
                  :class="{ 'text-emerald-600': sortKey === col.key && sortOrder === 'asc' }"
                  fill="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path d="M12 4l-8 8h16z" />
                </svg>
                <svg
                  v-if="sortKey !== col.key || sortOrder === 'desc'"
                  class="h-2 w-2 mt-0.5"
                  :class="{ 'text-emerald-600': sortKey === col.key && sortOrder === 'desc' }"
                  fill="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path d="M12 20l-8-8h16z" />
                </svg>
              </span>
            </div>
          </th>
        </tr>
      </thead>

      <tbody class="divide-y divide-slate-100">
        <!-- Loading skeleton -->
        <template v-if="loading">
          <tr v-for="n in 5" :key="n" class="animate-pulse">
            <td v-for="col in columns" :key="col.key" class="px-5 py-4">
              <div class="h-4 w-3/4 rounded bg-slate-100"></div>
            </td>
          </tr>
        </template>

        <!-- Empty state -->
        <tr v-else-if="sortedRows.length === 0">
          <td :colspan="columns.length" class="px-5 py-14 text-center text-slate-400">
            <div class="flex flex-col items-center gap-2">
              <div class="w-12 h-12 rounded-full bg-slate-50 flex items-center justify-center mb-1 text-slate-350">
                <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                    d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
                </svg>
              </div>
              <span class="text-sm font-medium">{{ emptyText ?? 'Không có dữ liệu' }}</span>
            </div>
          </td>
        </tr>

        <!-- Data rows -->
        <template v-else>
          <tr
            v-for="(row, i) in sortedRows"
            :key="rowKey ? String(row[rowKey]) : i"
            class="hover:bg-slate-50/70 transition-colors duration-150"
          >
            <slot :row="row" :index="i" />
          </tr>
        </template>
      </tbody>
    </table>
  </div>
</template>
