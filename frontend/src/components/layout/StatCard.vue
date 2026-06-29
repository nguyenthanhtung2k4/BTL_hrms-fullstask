<script setup lang="ts">
defineProps<{
  title: string
  value: string | number
  subtitle?: string
  color?: 'emerald' | 'blue' | 'amber' | 'red' | 'violet' | 'cyan'
  loading?: boolean
}>()
</script>

<template>
  <div class="stat-card">
    <div class="stat-card__header">
      <span class="stat-card__title">{{ title }}</span>
      <div v-if="$slots.icon" :class="['stat-card__icon', `stat-card__icon--${color ?? 'emerald'}`]">
        <slot name="icon" />
      </div>
    </div>

    <div v-if="loading" class="stat-card__loading" />
    <div v-else class="stat-card__content">
      <div class="stat-card__value">{{ value }}</div>
      <p v-if="subtitle" class="stat-card__subtitle">{{ subtitle }}</p>
    </div>
  </div>
</template>

<style scoped>
.stat-card {
  border-radius: var(--radius-lg);
  border: 1px solid var(--border);
  background-color: var(--bg-surface);
  padding: 1.25rem;
  box-shadow: var(--shadow-sm);
  transition: transform var(--transition-base), box-shadow var(--transition-base), border-color var(--transition-base);
  cursor: default;
  will-change: transform, box-shadow, border-color;
}
.stat-card:hover {
  transform: translateY(-4px) scale(1.005);
  box-shadow: var(--shadow-lg);
  border-color: color-mix(in srgb, var(--color-primary) 25%, var(--border-strong));
}

.stat-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
}
.stat-card__title {
  font-size: 0.8125rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-secondary);
}

.stat-card__icon {
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-sm);
  padding: 0.5rem;
}
.stat-card__icon--emerald { background-color: var(--color-success-light); color: var(--color-success); }
.stat-card__icon--blue    { background-color: var(--color-info-light); color: var(--color-info); }
.stat-card__icon--amber   { background-color: var(--color-warning-light); color: var(--color-warning); }
.stat-card__icon--red     { background-color: var(--color-danger-light); color: var(--color-danger); }
.stat-card__icon--violet  { background-color: hsla(270, 70%, 50%, 0.1); color: hsl(270, 70%, 50%); }
.stat-card__icon--cyan    { background-color: hsla(190, 70%, 50%, 0.1); color: hsl(190, 70%, 50%); }

.stat-card__loading {
  margin-top: 0.75rem;
  height: 2rem;
  width: 6rem;
  border-radius: var(--radius-sm);
  background-color: var(--bg-muted);
  animation: pulse 1.5s ease-in-out infinite;
}
@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.stat-card__content {
  margin-top: 0.5rem;
}
.stat-card__value {
  font-size: 1.75rem;
  font-weight: 700;
  color: var(--text-primary);
  line-height: 1.2;
}
.stat-card__subtitle {
  font-size: 0.75rem;
  color: var(--text-tertiary);
  margin-top: 0.25rem;
}
</style>

