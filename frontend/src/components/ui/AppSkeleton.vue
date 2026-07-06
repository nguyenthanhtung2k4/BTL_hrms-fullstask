<script setup lang="ts">
// AppSkeleton - Reusable skeleton loading placeholders with pulse animation

withDefaults(
  defineProps<{
    type?: 'line' | 'circle' | 'card' | 'table-row' | 'custom'
    count?: number
    width?: string
    height?: string
    cols?: number // For table-row mode
  }>(),
  {
    type: 'line',
    count: 1,
    cols: 5,
  }
)
</script>

<template>
  <!-- Line Skeleton -->
  <template v-if="type === 'line'">
    <div
      v-for="i in count"
      :key="'line-' + i"
      class="animate-pulse bg-slate-250 rounded"
      :style="{ width: width || '100%', height: height || '1rem' }"
    ></div>
  </template>

  <!-- Circle Skeleton -->
  <template v-else-if="type === 'circle'">
    <div
      v-for="i in count"
      :key="'circle-' + i"
      class="animate-pulse bg-slate-250 rounded-full shrink-0"
      :style="{ width: width || '2.5rem', height: height || '2.5rem' }"
    ></div>
  </template>

  <!-- Card Skeleton -->
  <template v-else-if="type === 'card'">
    <div
      v-for="i in count"
      :key="'card-' + i"
      class="animate-pulse rounded-xl border border-slate-150 bg-slate-50 flex flex-col gap-2 p-3"
      :style="{ width: width || '100%', height: height || 'auto' }"
    >
      <div class="h-3 bg-slate-250 rounded w-3/4"></div>
      <div class="h-4 bg-slate-200 rounded-full w-1/2"></div>
    </div>
  </template>

  <!-- Table Row Skeleton -->
  <template v-else-if="type === 'table-row'">
    <tr v-for="i in count" :key="'tr-' + i" class="animate-pulse border-b border-slate-150 last:border-b-0">
      <!-- Cột đầu tiên (Tên/Avatar nhân viên) -->
      <td class="px-4 py-3.5 sticky left-0 bg-white z-10 border-r border-slate-200 w-[240px] shadow-[2px_0_5px_rgba(0,0,0,0.02)]">
        <div class="flex items-center gap-2.5">
          <div class="h-9 w-9 rounded-2xl bg-slate-200 shrink-0"></div>
          <div class="min-w-0 flex-1 space-y-1.5">
            <div class="h-3.5 bg-slate-200 rounded w-5/6"></div>
            <div class="h-2.5 bg-slate-150 rounded w-1/2"></div>
          </div>
        </div>
      </td>
      <!-- Các cột dữ liệu -->
      <td v-for="j in cols" :key="'td-' + j" class="px-2.5 py-3.5 border-r border-slate-100 last:border-r-0">
        <div class="h-[60px] rounded-xl border border-slate-150 bg-slate-50 flex flex-col items-center justify-center gap-1.5 p-2">
          <div class="h-2.5 bg-slate-250 rounded w-3/4"></div>
          <div class="h-4 bg-slate-200 rounded-full w-2/3"></div>
        </div>
      </td>
    </tr>
  </template>

  <!-- Custom Skeleton Slot -->
  <template v-else-if="type === 'custom'">
    <div class="animate-pulse">
      <slot />
    </div>
  </template>
</template>
