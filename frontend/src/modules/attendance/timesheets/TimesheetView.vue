<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { timesheetService } from '../../../services/timesheet.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { Timesheet } from '../../../types/attendance.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'

const auth = useAuthStore()
const toast = useToastStore()

const WORK_DAY_MINUTES = 480 // 8h per day

const timesheets = ref<Timesheet[]>([])
const loading = ref(false)
const calculating = ref(false)
const filterMonth = ref(new Date().getMonth() + 1)
const filterYear = ref(new Date().getFullYear())

const columns = [
  { key: 'employee', label: 'Nhân viên' },
  { key: 'workDays', label: 'Ngày công' },
  { key: 'totalHours', label: 'Tổng giờ' },
  { key: 'paidLeave', label: 'Phép CL' },
  { key: 'unpaidLeave', label: 'Phép KL' },
  { key: 'status', label: 'Trạng thái' },
]

const months = Array.from({ length: 12 }, (_, i) => ({ value: i + 1, label: `Tháng ${i + 1}` }))
const years = [2024, 2025, 2026]

function workDays(t: Timesheet) {
  return (t.totalWorkedMinutes / WORK_DAY_MINUTES).toFixed(1)
}

function totalHours(t: Timesheet) {
  const h = Math.floor(t.totalWorkedMinutes / 60)
  const m = t.totalWorkedMinutes % 60
  return `${h}h ${m}m`
}

async function load() {
  loading.value = true
  try {
    timesheets.value = await timesheetService.getAll({
      month: filterMonth.value, year: filterYear.value,
      employeeId: auth.isManager ? undefined : auth.employeeId,
    })
  } catch { toast.error('Không thể tải bảng công') }
  finally { loading.value = false }
}

async function calculate() {
  calculating.value = true
  try {
    await timesheetService.calculate(filterMonth.value, filterYear.value)
    toast.success(`Đã tính bảng công Tháng ${filterMonth.value}/${filterYear.value}`)
    await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Tính bảng công thất bại') }
  finally { calculating.value = false }
}

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Bảng công" subtitle="Tổng hợp chấm công hàng tháng" :breadcrumbs="[{ label: 'Chấm công' }, { label: 'Bảng công' }]">
      <template #actions>
        <AppButton v-if="auth.isHR" :loading="calculating" variant="secondary" @click="calculate">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 11h.01M12 11h.01M15 11h.01M4 19h16a2 2 0 002-2V7a2 2 0 00-2-2H4a2 2 0 00-2 2v10a2 2 0 002 2z" /></svg>
          Tính bảng công
        </AppButton>
      </template>
    </PageHeader>

    <div class="mb-4 flex gap-3">
      <select v-model="filterMonth" class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500">
        <option v-for="m in months" :key="m.value" :value="m.value">{{ m.label }}</option>
      </select>
      <select v-model="filterYear" class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500">
        <option v-for="y in years" :key="y" :value="y">{{ y }}</option>
      </select>
      <button class="h-9 rounded-lg bg-emerald-600 px-4 text-sm font-medium text-white hover:bg-emerald-700" @click="load">Xem</button>
    </div>

    <AppTable :columns="columns" :rows="timesheets" :loading="loading" row-key="id" empty-text="Chưa có bảng công — hãy nhấn Tính bảng công">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-medium">{{ (row as Timesheet).employeeName }}</td>
        <td class="px-4 py-3 text-sm font-semibold text-emerald-700">{{ workDays(row as Timesheet) }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ totalHours(row as Timesheet) }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as Timesheet).paidLeaveDays > 0 ? (row as Timesheet).paidLeaveDays : '—' }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as Timesheet).unpaidLeaveDays > 0 ? (row as Timesheet).unpaidLeaveDays : '—' }}</td>
        <td class="px-4 py-3"><AppBadge :status="(row as Timesheet).status" /></td>
      </template>
    </AppTable>
  </div>
</template>
