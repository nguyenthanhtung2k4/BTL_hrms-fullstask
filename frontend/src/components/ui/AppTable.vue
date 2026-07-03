<script setup lang="ts">
// AppTable — Sortable, generic table with skeleton loading, empty state, and Dark Mode support

import { ref, computed, watch } from 'vue'

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
const pageSize = ref(props.pageSize ?? 10)

watch(() => props.pageSize, (val) => {
  if (val !== undefined) {
    pageSize.value = val
  }
})

const PAGE_SIZE_OPTIONS = [10, 20, 50, 100]

const pages = computed(() => {
  const arr: (number | '...')[] = []
  const total = totalPages.value
  const current = currentPage.value
  for (let i = 1; i <= total; i++) {
    if (i === 1 || i === total || Math.abs(i - current) <= 1) {
      arr.push(i)
    } else if (arr[arr.length - 1] !== '...') {
      arr.push('...')
    }
  }
  return arr
})

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
    
    // Fallbacks for common model fields
    if (key === 'employee') {
      valA = a.employeeName || a.fullName || a.employee?.fullName || a.employee?.employeeName || ''
      valB = b.employeeName || b.fullName || b.employee?.fullName || b.employee?.employeeName || ''
    } else if (key === 'dept' || key === 'department') {
      valA = a.departmentName || a.deptName || a.department?.name || ''
      valB = b.departmentName || b.deptName || b.department?.name || ''
    } else if (key === 'position') {
      valA = a.positionName || a.position?.name || ''
      valB = b.positionName || b.position?.name || ''
    } else if (key === 'shift' || key === 'shiftName') {
      valA = a.shiftName || a.shift?.name || ''
      valB = b.shiftName || b.shift?.name || ''
    } else if (key === 'code') {
      valA = a.employeeCode || a.code || ''
      valB = b.employeeCode || b.code || ''
    } else if (key === 'name') {
      valA = a.fullName || a.employeeName || ''
      valB = b.fullName || b.employeeName || ''
    }
    
    if (valA === undefined || valA === null) valA = ''
    if (valB === undefined || valB === null) valB = ''
    if (typeof valA === 'string' && typeof valB === 'string')
      return valA.localeCompare(valB, 'vi', { numeric: true }) * order
    return (valA < valB ? -1 : valA > valB ? 1 : 0) * order
  })
})

const totalPages = computed(() => {
  if (!pageSize.value || pageSize.value <= 0) return 1
  return Math.max(1, Math.ceil(sortedRows.value.length / pageSize.value))
})

const paginatedRows = computed(() => {
  if (!pageSize.value || pageSize.value <= 0) return sortedRows.value
  // Auto-correct out-of-bounds page
  if (currentPage.value > totalPages.value) currentPage.value = totalPages.value
  
  const start = (currentPage.value - 1) * pageSize.value
  return sortedRows.value.slice(start, start + pageSize.value)
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
              <span 
                v-if="col.key !== 'actions' && col.key !== 'select' && col.key" 
                class="app-table__sort-icons"
                :class="{ 'app-table__sort-icons--active': sortKey === col.key }"
              >
                <svg
                  class="app-table__sort-icon"
                  :class="{ 'app-table__sort-icon--active': sortKey === col.key && sortOrder === 'asc' }"
                  fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3"
                ><path stroke-linecap="round" stroke-linejoin="round" d="M5 15l7-7 7 7" /></svg>
                <svg
                  class="app-table__sort-icon"
                  :class="{ 'app-table__sort-icon--active': sortKey === col.key && sortOrder === 'desc' }"
                  fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3"
                ><path stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7" /></svg>
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
    <div v-if="props.pageSize && totalPages > 1" class="app-table__pagination">
      <!-- Left: per-page selector + total -->
      <div class="pagination__left">
        <span class="pagination__label">Hiển thị</span>
        <select class="pagination__select" v-model="pageSize" @change="currentPage = 1">
          <option v-for="opt in PAGE_SIZE_OPTIONS" :key="opt" :value="opt">{{ opt }}</option>
        </select>
        <span class="pagination__label">
          dòng&nbsp;/&nbsp;trang &nbsp;·&nbsp;
          <strong>{{ sortedRows.length }}</strong> kết quả
        </span>
      </div>

      <!-- Right: page buttons -->
      <div class="pagination__right">
        <span class="pagination__info">Trang {{ currentPage }} / {{ totalPages }}</span>
        <div class="pagination__btns">
          <button
            class="pagination__btn"
            :disabled="currentPage === 1"
            @click="prevPage"
          >‹</button>

          <template v-for="p in pages" :key="p">
            <span v-if="p === '...'" class="pagination__ellipsis">…</span>
            <button
              v-else
              :class="['pagination__btn', p === currentPage ? 'pagination__btn--active' : '']"
              @click="goToPage(p as number)"
            >{{ p }}</button>
          </template>

          <button
            class="pagination__btn"
            :disabled="currentPage === totalPages"
            @click="nextPage"
          >›</button>
        </div>
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
  flex-wrap: wrap;
  gap: 0.5rem;
}

.pagination__left,
.pagination__right {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.pagination__label {
  color: var(--text-tertiary);
  font-size: 0.8125rem;
}

.pagination__select {
  height: 1.875rem;
  padding: 0 0.375rem;
  border-radius: var(--radius-sm);
  border: 1px solid var(--border-strong);
  background-color: var(--bg-surface);
  color: var(--text-primary);
  font-size: 0.8125rem;
  outline: none;
  cursor: pointer;
  transition: border-color var(--transition-fast);
}
.pagination__select:focus {
  border-color: var(--color-primary);
}

.pagination__info {
  color: var(--text-tertiary);
  font-size: 0.75rem;
}

.pagination__btns {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.pagination__btn {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 1.875rem;
  height: 1.875rem;
  padding: 0 0.375rem;
  border-radius: var(--radius-sm);
  border: 1px solid var(--border);
  background-color: var(--bg-surface);
  color: var(--text-secondary);
  font-size: 0.8125rem;
  cursor: pointer;
  transition: background-color var(--transition-fast), border-color var(--transition-fast),
    color var(--transition-fast);
}
.pagination__btn:hover:not(:disabled) {
  background-color: var(--bg-subtle);
  color: var(--text-primary);
}
.pagination__btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.pagination__btn--active {
  border-color: var(--color-primary) !important;
  background-color: var(--color-primary-light) !important;
  color: var(--color-primary-text) !important;
  font-weight: 600;
}

.pagination__ellipsis {
  padding: 0 0.25rem;
  color: var(--text-tertiary);
  font-size: 0.875rem;
}

.app-table__sort-icons {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  margin-left: 0.375rem;
  width: 0.75rem;
  height: 0.75rem;
  opacity: 0.35;
  transition: opacity var(--transition-fast);
}

.app-table__th--sortable:hover .app-table__sort-icons,
.app-table__sort-icons--active {
  opacity: 1 !important;
}

.app-table__sort-icon {
  width: 0.55rem;
  height: 0.55rem;
  color: var(--text-tertiary);
  transition: color var(--transition-fast), transform var(--transition-fast);
}

.app-table__sort-icon--active {
  color: #10b981 !important; /* Emerald-500 */
}
</style>
