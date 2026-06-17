<script setup lang="ts">
// AppBadge — Status badges với màu sắc chuẩn theo đề tài

defineProps<{
  status: string
}>()

type BadgeConfig = { bg: string; text: string; label?: string }

const map: Record<string, BadgeConfig> = {
  // Employee status
  Active:     { bg: 'bg-emerald-100', text: 'text-emerald-700', label: 'Đang làm' },
  Inactive:   { bg: 'bg-slate-100',   text: 'text-slate-600',   label: 'Ngưng' },
  OnLeave:    { bg: 'bg-amber-100',   text: 'text-amber-700',   label: 'Nghỉ phép' },
  Resigned:   { bg: 'bg-red-100',     text: 'text-red-700',     label: 'Đã nghỉ' },

  // Contract status
  Expired:    { bg: 'bg-orange-100',  text: 'text-orange-700',  label: 'Hết hạn' },
  Terminated: { bg: 'bg-red-100',     text: 'text-red-700',     label: 'Chấm dứt' },

  // Leave status
  Pending:    { bg: 'bg-amber-100',   text: 'text-amber-700',   label: 'Chờ duyệt' },
  Approved:   { bg: 'bg-emerald-100', text: 'text-emerald-700', label: 'Đã duyệt' },
  Rejected:   { bg: 'bg-red-100',     text: 'text-red-700',     label: 'Từ chối' },
  Cancelled:  { bg: 'bg-slate-100',   text: 'text-slate-600',   label: 'Đã hủy' },

  // Payroll period status
  Draft:      { bg: 'bg-blue-100',    text: 'text-blue-700',    label: 'Nháp' },
  Calculated: { bg: 'bg-amber-100',   text: 'text-amber-700',   label: 'Đã tính' },
  Closed:     { bg: 'bg-emerald-100', text: 'text-emerald-700', label: 'Đã đóng' },

  // Shift / generic
  true:       { bg: 'bg-emerald-100', text: 'text-emerald-700', label: 'Kích hoạt' },
  false:      { bg: 'bg-slate-100',   text: 'text-slate-500',   label: 'Tắt' },
}
</script>

<template>
  <span
    :class="[
      'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
      map[status]?.bg ?? 'bg-slate-100',
      map[status]?.text ?? 'text-slate-600',
    ]"
  >
    {{ map[status]?.label ?? status }}
  </span>
</template>
