<script setup lang="ts">
// AppBadge — Status badges using CSS variables for full Dark Mode support

defineProps<{
  status: string
}>()

type BadgeConfig = { type: string; label: string }

const map: Record<string, BadgeConfig> = {
  // Employee status
  Active:      { type: 'success', label: 'Đang làm' },
  Inactive:    { type: 'neutral', label: 'Ngưng' },
  OnLeave:     { type: 'warning', label: 'Nghỉ phép' },
  Resigned:    { type: 'danger',  label: 'Đã nghỉ' },
  // Contract status
  Expired:     { type: 'warning', label: 'Hết hạn' },
  Terminated:  { type: 'danger',  label: 'Chấm dứt' },
  // Leave & Approval status
  Pending:     { type: 'warning', label: 'Chờ duyệt' },
  Approved:    { type: 'success', label: 'Đã duyệt' },
  Rejected:    { type: 'danger',  label: 'Từ chối' },
  Cancelled:   { type: 'neutral', label: 'Đã hủy' },
  // Payroll
  Draft:       { type: 'info',    label: 'Nháp' },
  Calculated:  { type: 'warning', label: 'Đã tính' },
  Closed:      { type: 'success', label: 'Đã đóng' },
  Open:        { type: 'info',    label: 'Đang mở' },
  Paid:        { type: 'success', label: 'Đã thanh toán' },
  // Timesheet
  Submitted:   { type: 'info',    label: 'Đã gửi' },
  // Generic boolean
  true:        { type: 'success', label: 'Kích hoạt' },
  false:       { type: 'neutral', label: 'Tắt' },
}
</script>

<template>
  <span :class="['badge', `badge--${map[status]?.type ?? 'neutral'}`]">
    {{ map[status]?.label ?? status }}
  </span>
</template>

<style scoped>
.badge {
  display: inline-flex;
  align-items: center;
  border-radius: var(--radius-full);
  padding: 0.125rem 0.625rem;
  font-size: 0.6875rem;
  font-weight: 600;
  letter-spacing: 0.01em;
  white-space: nowrap;
}

.badge--success {
  background-color: var(--color-success-light);
  color: var(--color-success);
}
.badge--warning {
  background-color: var(--color-warning-light);
  color: hsl(36, 80%, 30%);
}
[data-theme="dark"] .badge--warning {
  color: hsl(45, 93%, 65%);
}
.badge--danger {
  background-color: var(--color-danger-light);
  color: var(--color-danger);
}
.badge--info {
  background-color: var(--color-info-light);
  color: var(--color-info);
}
.badge--neutral {
  background-color: var(--bg-muted);
  color: var(--text-secondary);
}
</style>
