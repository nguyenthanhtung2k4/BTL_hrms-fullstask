<script setup lang="ts">
// Reusable premium FilterBar component
</script>

<template>
  <div class="filter-bar p-3.5 rounded-xl border mb-4">
    <div class="flex flex-wrap items-end gap-3 justify-between">
      <!-- Filter fields/dropdowns -->
      <div class="flex flex-wrap items-center gap-3 flex-1 min-w-[280px]">
        <slot name="filters" />
      </div>

      <!-- Action buttons -->
      <div class="flex items-center gap-2 mt-2 sm:mt-0">
        <slot name="actions" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.filter-bar {
  background-color: var(--bg-surface);
  border-color: var(--border);
  box-shadow: var(--shadow-sm);
  transition: all var(--transition-base);
}

[data-theme="dark"] .filter-bar {
  background-color: var(--bg-subtle);
  border-color: var(--border);
}
</style>
