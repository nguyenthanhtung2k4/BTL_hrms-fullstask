<script setup lang="ts">
// PageHeader — Page title, breadcrumbs, and action slot with Dark Mode support

import { RouterLink } from 'vue-router'

defineProps<{
  title: string
  subtitle?: string
  breadcrumbs?: { label: string; to?: string }[]
}>()
</script>

<template>
  <div class="page-header">
    <!-- Breadcrumb -->
    <nav v-if="breadcrumbs?.length" class="page-header__breadcrumb">
      <template v-for="(crumb, i) in breadcrumbs" :key="i">
        <span v-if="i > 0" class="page-header__sep">›</span>
        <RouterLink v-if="crumb.to" :to="crumb.to" class="page-header__crumb-link">
          {{ crumb.label }}
        </RouterLink>
        <span v-else class="page-header__crumb-current">{{ crumb.label }}</span>
      </template>
    </nav>

    <!-- Title row -->
    <div class="page-header__row">
      <div>
        <h1 class="page-header__title">{{ title }}</h1>
        <p v-if="subtitle" class="page-header__subtitle">{{ subtitle }}</p>
      </div>
      <!-- Action buttons slot -->
      <div v-if="$slots.actions" class="page-header__actions">
        <slot name="actions" />
      </div>
    </div>

    <!-- Divider -->
    <div class="page-header__divider"></div>
  </div>
</template>

<style scoped>
.page-header {
  margin-bottom: 1.5rem;
}

.page-header__breadcrumb {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  margin-bottom: 0.5rem;
  font-size: 0.75rem;
  color: var(--text-tertiary);
}

.page-header__sep {
  user-select: none;
  opacity: 0.6;
}

.page-header__crumb-link {
  color: var(--text-tertiary);
  text-decoration: none;
  transition: color var(--transition-fast);
}
.page-header__crumb-link:hover {
  color: var(--text-primary);
}

.page-header__crumb-current {
  color: var(--text-secondary);
  font-weight: 500;
}

.page-header__row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
}

.page-header__title {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--text-primary);
  margin: 0;
  letter-spacing: -0.01em;
}

.page-header__subtitle {
  margin: 0.25rem 0 0;
  font-size: 0.875rem;
  color: var(--text-secondary);
}

.page-header__actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.page-header__divider {
  height: 1px;
  background-color: var(--border);
  margin-top: 1rem;
}
</style>
