<script setup lang="ts">
const props = defineProps<{
  total: number
  perPage?: number
  current: number
}>()

const emit = defineEmits<{ change: [page: number] }>()

const perPage = props.perPage ?? 10
const totalPages = Math.ceil(props.total / perPage)

function pages() {
  const arr: (number | '...')[] = []
  for (let i = 1; i <= totalPages; i++) {
    if (i === 1 || i === totalPages || Math.abs(i - props.current) <= 1) {
      arr.push(i)
    } else if (arr[arr.length - 1] !== '...') {
      arr.push('...')
    }
  }
  return arr
}
</script>

<template>
  <div v-if="totalPages > 1" class="flex items-center justify-between px-1 py-3 text-sm text-slate-600">
    <span>Trang {{ current }} / {{ totalPages }} ({{ total }} kết quả)</span>
    <div class="flex gap-1">
      <button
        :disabled="current === 1"
        class="rounded border border-slate-200 px-2.5 py-1 hover:bg-slate-100 disabled:opacity-40"
        @click="emit('change', current - 1)"
      >‹</button>
      <template v-for="p in pages()" :key="p">
        <span v-if="p === '...'" class="px-2 py-1">…</span>
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
        class="rounded border border-slate-200 px-2.5 py-1 hover:bg-slate-100 disabled:opacity-40"
        @click="emit('change', current + 1)"
      >›</button>
    </div>
  </div>
</template>
