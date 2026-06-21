<script setup lang="ts">
// AppPagination — Page navigation with rows-per-page selector and Dark Mode support

import { computed } from 'vue'

const props = defineProps<{
  total: number
  perPage?: number
  current: number
}>()

const emit = defineEmits<{
  change: [page: number]
  perPageChange: [perPage: number]
}>()

const PAGE_SIZE_OPTIONS = [10, 20, 50, 100]
const perPage = computed(() => props.perPage ?? 10)
const totalPages = computed(() => Math.ceil(props.total / perPage.value))

function pages() {
  const arr: (number | '...')[] = []
  for (let i = 1; i <= totalPages.value; i++) {
    if (i === 1 || i === totalPages.value || Math.abs(i - props.current) <= 1) {
      arr.push(i)
    } else if (arr[arr.length - 1] !== '...') {
      arr.push('...')
    }
  }
  return arr
}

function onPerPageChange(event: Event) {
  const val = Number((event.target as HTMLSelectElement).value)
  emit('perPageChange', val)
}
</script>

<template>
  <div class="pagination">
    <!-- Left: per-page selector + total -->
    <div class="pagination__left">
      <span class="pagination__label">Hiển thị</span>
      <select class="pagination__select" :value="perPage" @change="onPerPageChange">
        <option v-for="opt in PAGE_SIZE_OPTIONS" :key="opt" :value="opt">{{ opt }}</option>
      </select>
      <span class="pagination__label">
        dòng&nbsp;/&nbsp;trang &nbsp;·&nbsp;
        <strong>{{ total }}</strong> kết quả
      </span>
    </div>

    <!-- Right: page buttons -->
    <div v-if="totalPages > 1" class="pagination__right">
      <span class="pagination__info">Trang {{ current }} / {{ totalPages }}</span>
      <div class="pagination__btns">
        <button
          class="pagination__btn"
          :disabled="current === 1"
          @click="emit('change', current - 1)"
        >‹</button>

        <template v-for="p in pages()" :key="p">
          <span v-if="p === '...'" class="pagination__ellipsis">…</span>
          <button
            v-else
            :class="['pagination__btn', p === current ? 'pagination__btn--active' : '']"
            @click="emit('change', p as number)"
          >{{ p }}</button>
        </template>

        <button
          class="pagination__btn"
          :disabled="current === totalPages"
          @click="emit('change', current + 1)"
        >›</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.pagination {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 0.5rem;
  padding: 0.625rem 0.25rem 0.25rem;
  font-size: 0.8125rem;
}

.pagination__left,
.pagination__right {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.pagination__label {
  color: var(--text-tertiary);
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
  border-color: var(--color-primary);
  background-color: var(--color-primary-light);
  color: var(--color-primary-text);
  font-weight: 600;
}

.pagination__ellipsis {
  padding: 0 0.25rem;
  color: var(--text-tertiary);
  font-size: 0.875rem;
}
</style>
