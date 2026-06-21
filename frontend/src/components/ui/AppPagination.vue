<script setup lang="ts">
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
  <div class="flex items-center justify-between px-1 py-3 text-sm text-slate-600 flex-wrap gap-2">
    <!-- Left: rows per page selector + total count -->
    <div class="flex items-center gap-2">
      <span class="text-slate-500">Hiển thị</span>
      <select
        :value="perPage"
        class="h-8 rounded-lg border border-slate-200 bg-white px-2 text-sm outline-none focus:border-emerald-500 cursor-pointer"
        @change="onPerPageChange"
      >
        <option v-for="opt in PAGE_SIZE_OPTIONS" :key="opt" :value="opt">{{ opt }}</option>
      </select>
      <span class="text-slate-500">dòng&nbsp;/&nbsp;trang &nbsp;·&nbsp; <strong>{{ total }}</strong> kết quả</span>
    </div>

    <!-- Right: page navigation -->
    <div v-if="totalPages > 1" class="flex items-center gap-2">
      <span class="text-slate-400 text-xs">Trang {{ current }} / {{ totalPages }}</span>
      <div class="flex gap-1">
        <button
          :disabled="current === 1"
          class="rounded border border-slate-200 px-2.5 py-1 hover:bg-slate-100 disabled:opacity-40 transition-colors"
          @click="emit('change', current - 1)"
        >‹</button>
        <template v-for="p in pages()" :key="p">
          <span v-if="p === '...'" class="px-2 py-1 text-slate-400">…</span>
          <button
            v-else
            :class="[
              'rounded border px-2.5 py-1 transition-colors',
              p === current
                ? 'border-emerald-500 bg-emerald-50 text-emerald-700 font-semibold'
                : 'border-slate-200 hover:bg-slate-100',
            ]"
            @click="emit('change', p as number)"
          >{{ p }}</button>
        </template>
        <button
          :disabled="current === totalPages"
          class="rounded border border-slate-200 px-2.5 py-1 hover:bg-slate-100 disabled:opacity-40 transition-colors"
          @click="emit('change', current + 1)"
        >›</button>
      </div>
    </div>
  </div>
</template>
