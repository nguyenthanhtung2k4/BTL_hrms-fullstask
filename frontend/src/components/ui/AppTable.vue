<script setup lang="ts">
// AppTable — Sortable, generic table with skeleton loading, empty state, and Dark Mode support

import { ref, computed } from 'vue'

const props = defineProps<{
  loading?: boolean
  columns: { key: string; label: string; class?: string }[]
  rows: any[]
  emptyText?: string
  rowKey?: string
  pageSize?: number
}>()

const sortKey = ref<string | null>(null)
const sortOrder = ref<'asc' | 'desc' | null>(null)
const currentPage = ref(1)

function handleSort(key: string) {
  if (key === 'actions' || !key) return
  if (sortKey.value === key) {
    if (sortOrder.value === 'asc') sortOrder.value = 'desc'
    else if (sortOrder.value === 'desc') { sortOrder.value = null; sortKey.value = null }
    else sortOrder.value = 'asc'
  } else {
    sortKey.value = key
    sortOrder.value = 'asc'
  }
  currentPage.value = 1 // reset page on sort
}

const sortedRows = computed(() => {
  if (!sortKey.value || !sortOrder.value) return props.rows
  const key = sortKey.value
  const order = sortOrder.value === 'asc' ? 1 : -1
  return [...props.rows].sort((a: any, b: any) => {
    let valA = a[key], valB = b[key]
    // Handle common virtual column mappings
    if (key === 'employee' && a.employeeName) { valA = a.employeeName; valB = b.employeeName }
    else if (key === 'dept' && a.departmentName) { valA = a.departmentName; valB = b.departmentName }
    else if (key === 'shift' && a.shiftName) { valA = a.shiftName; valB = b.shiftName }
    else if (key === 'department' && a.departmentName) { valA = a.departmentName; valB = b.departmentName }
    else if (key === 'position' && a.positionName) { valA = a.positionName; valB = b.positionName }
    if (valA === undefined || valA === null) valA = ''
    if (valB === undefined || valB === null) valB = ''
    if (typeof valA === 'string' && typeof valB === 'string')
      return valA.localeCompare(valB, 'vi', { numeric: true }) * order
    return (valA < valB ? -1 : valA > valB ? 1 : 0) * order
  })
})

const totalPages = computed(() => {
  if (!props.pageSize || props.pageSize <= 0) return 1
  return Math.max(1, Math.ceil(sortedRows.value.length / props.pageSize))
})

const paginatedRows = computed(() => {
  if (!props.pageSize || props.pageSize <= 0) return sortedRows.value
  // Auto-correct out-of-bounds page
  if (currentPage.value > totalPages.value) currentPage.value = totalPages.value
  
  const start = (currentPage.value - 1) * props.pageSize
  return sortedRows.value.slice(start, start + props.pageSize)
})

function prevPage() {
  if (currentPage.value > 1) currentPage.value--
}

function nextPage() {
  if (currentPage.value < totalPages.value) currentPage.value++
}

function goToPage(p: number) {
  if (p >= 1 && p <= totalPages.value) currentPage.value = p
}
</script>

<template>
  <div class="app-table-wrap">
    <table class="app-table">
      <!-- Header -->
      <thead class="app-table__head">
        <tr>
          <th
            v-for="col in columns"
            :key="col.key"
            :class="[
              'app-table__th',
              col.key !== 'actions' && col.key !== 'select' && col.key ? 'app-table__th--sortable' : '',
              col.class ?? '',
            ]"
            @click="col.key !== 'select' ? handleSort(col.key) : undefined"
          >
            <div class="app-table__th-inner">
              <!-- Named slot per column header, falls back to label text -->
              <slot :name="`header-${col.key}`">
                <span>{{ col.label }}</span>
              </slot>
              <!-- Sort icons (skip for 'select' and 'actions') -->
              <span v-if="col.key !== 'actions' && col.key !== 'select' && col.key" class="app-table__sort-icons">
                <svg
                  class="app-table__sort-icon"
                  :class="{ 'app-table__sort-icon--active': sortKey === col.key && sortOrder === 'asc' }"
                  fill="currentColor" viewBox="0 0 24 24"
                ><path d="M12 4l-8 8h16z" /></svg>
                <svg
                  class="app-table__sort-icon"
                  :class="{ 'app-table__sort-icon--active': sortKey === col.key && sortOrder === 'desc' }"
                  fill="currentColor" viewBox="0 0 24 24"
                ><path d="M12 20l-8-8h16z" /></svg>
              </span>
            </div>
          </th>
        </tr>
      </thead>

      <tbody class="app-table__body">
        <!-- Loading skeleton -->
        <template v-if="loading">
          <tr v-for="n in 5" :key="n" class="app-table__skeleton-row">
            <td v-for="col in columns" :key="col.key" class="app-table__td">
              <div class="app-table__skeleton-cell" :style="{ width: n % 2 === 0 ? '60%' : '80%' }"></div>
            </td>
          </tr>
        </template>

        <!-- Empty state -->
        <tr v-else-if="sortedRows.length === 0">
          <td :colspan="columns.length" class="app-table__empty">
            <div class="app-table__empty-inner">
              <div class="app-table__empty-icon">
                <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                    d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
                </svg>
              </div>
              <span class="app-table__empty-text">{{ emptyText ?? 'Không có dữ liệu' }}</span>
            </div>
          </td>
        </tr>

        <!-- Data rows -->
        <template v-else>
          <tr
            v-for="(row, i) in paginatedRows"
            :key="rowKey ? row[rowKey] : i"
            class="app-table__row"
          >
            <slot :row="row" :index="i" />
          </tr>
        </template>
      </tbody>
    </table>
    
    <!-- Pagination controls -->
    <div v-if="pageSize && totalPages > 1" class="app-table__pagination">
      <span class="app-table__pagination-info">
        Hiển thị {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, sortedRows.length) }} trong số {{ sortedRows.length }} kết quả
      </span>
      <div class="app-table__pagination-actions">
        <button class="app-table__page-btn" :disabled="currentPage === 1" @click="prevPage">
          <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" /></svg>
        </button>
        <button 
          v-for="p in totalPages" 
          :key="p" 
          class="app-table__page-btn" 
          :class="{ 'app-table__page-btn--active': p === currentPage }"
          @click="goToPage(p)"
        >
          {{ p }}
        </button>
        <button class="app-table__page-btn" :disabled="currentPage === totalPages" @click="nextPage">
          <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" /></svg>
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.app-table-wrap {
  overflow-x: auto;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border);
  background-color: var(--bg-surface);
  box-shadow: var(--shadow-sm);
  transition: background-color var(--transition-base), border-color var(--transition-base);
}

.app-table {
  width: 100%;
  min-width: max-content;
  border-collapse: collapse;
  font-size: 0.875rem;
  text-align: left;
}

/* Head */
.app-table__head {
  border-bottom: 1px solid var(--border);
}

.app-table__th {
  padding: 0.75rem 1.25rem;
  font-size: 0.6875rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--text-tertiary);
  background-color: var(--bg-subtle);
  white-space: nowrap;
  user-select: none;
  transition: color var(--transition-fast), background-color var(--transition-fast);
}

.app-table__th--sortable {
  cursor: pointer;
}
.app-table__th--sortable:hover {
  background-color: var(--bg-muted);
  color: var(--text-secondary);
}

.app-table__th-inner {
  display: flex;
  align-items: center;
  gap: 0.375rem;
}

.app-table__sort-icons {
  display: inline-flex;
  flex-direction: column;
  gap: 1px;
  opacity: 0.4;
}

.app-table__sort-icon {
  width: 0.5rem;
  height: 0.5rem;
  color: var(--text-tertiary);
}

.app-table__sort-icon--active {
  color: var(--color-primary);
  opacity: 1;
}

/* Body */
.app-table__body {
  /* divide-y equivalent via row border */
}

.app-table__td {
  padding: 0.875rem 1.25rem;
  color: var(--text-primary);
  border-bottom: 1px solid var(--border);
  transition: background-color var(--transition-fast);
}

/* Data rows */
.app-table__row {
  transition: background-color var(--transition-fast);
}
.app-table__row:hover :deep(td) {
  background-color: var(--bg-subtle);
}
.app-table__row:last-child :deep(td) {
  border-bottom: none;
}

/* Skeleton */
.app-table__skeleton-row {
  animation: pulse 1.5s linear infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.app-table__skeleton-cell {
  height: 1rem;
  border-radius: var(--radius-sm);
  background-color: var(--bg-muted);
}

/* Empty state */
.app-table__empty {
  padding: 3.5rem 1rem;
  text-align: center;
  border-bottom: none;
}

.app-table__empty-inner {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.625rem;
}

.app-table__empty-icon {
  width: 3rem;
  height: 3rem;
  border-radius: 50%;
  background-color: var(--bg-subtle);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-tertiary);
}

.app-table__empty-text {
  font-size: 0.875rem;
  color: var(--text-tertiary);
  font-weight: 500;
}

/* Pagination */
.app-table__pagination {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.25rem;
  border-top: 1px solid var(--border);
  background-color: var(--bg-surface);
  border-radius: 0 0 var(--radius-lg) var(--radius-lg);
}

.app-table__pagination-info {
  font-size: 0.8125rem;
  color: var(--text-tertiary);
}

.app-table__pagination-actions {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.app-table__page-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 2rem;
  height: 2rem;
  padding: 0 0.5rem;
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  background-color: var(--bg-surface);
  color: var(--text-secondary);
  font-size: 0.8125rem;
  font-weight: 500;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.app-table__page-btn:hover:not(:disabled) {
  background-color: var(--bg-subtle);
  border-color: var(--color-primary-light);
  color: var(--color-primary);
}

.app-table__page-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.app-table__page-btn--active {
  background-color: var(--color-primary) !important;
  border-color: var(--color-primary) !important;
  color: white !important;
}
</style>
