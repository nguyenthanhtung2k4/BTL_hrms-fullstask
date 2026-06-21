<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { attendanceService } from '../../../services/attendance.service'
import { employeeService } from '../../../services/employee.service'
import { departmentService } from '../../../services/department.service'
import { useToastStore } from '../../../stores/toast'
import type { AttendanceRecord } from '../../../types/attendance.types'
import type { Employee, Department } from '../../../types/hr.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'

const toast = useToastStore()
const records = ref<AttendanceRecord[]>([])
const employees = ref<Employee[]>([])
const departments = ref<Department[]>([])
const loading = ref(false)
const filterEmployee = ref('')
const filterMonth = ref(new Date().getMonth() + 1)
const filterYear = ref(new Date().getFullYear())

const columns = [
  { key: 'employee', label: 'Nhân viên' }, { key: 'date', label: 'Ngày' },
  { key: 'shift', label: 'Ca' }, { key: 'in', label: 'Giờ vào' }, { key: 'out', label: 'Giờ ra' }, { key: 'total', label: 'Tổng giờ' },
]

const months = Array.from({ length: 12 }, (_, i) => ({ value: i + 1, label: `Tháng ${i + 1}` }))
const years = [2024, 2025, 2026]

async function load() {
  loading.value = true
  try {
    [records.value, employees.value, departments.value] = await Promise.all([
      attendanceService.getAll({ employeeId: filterEmployee.value || undefined, month: filterMonth.value, year: filterYear.value }),
      employeeService.getAll(),
      departmentService.getAll(),
    ])
  } catch { toast.error('Không thể tải dữ liệu') }
  finally { loading.value = false }
}

function fmtTime(d?: string) { return d ? new Date(d).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '—' }
function fmtDate(d: string) { return new Date(d).toLocaleDateString('vi-VN') }
function fmtMin(m: number) { return m >= 60 ? `${Math.floor(m / 60)}h ${m % 60}m` : `${m}m` }

const { currentPage, perPage, paginatedData, total } = usePagination(records)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Bảng chấm công" subtitle="Lịch sử chấm công toàn bộ nhân viên" :breadcrumbs="[{ label: 'Chấm công' }, { label: 'Chấm công' }]" />
    <div class="mb-4 flex gap-3 flex-wrap">
      <select v-model="filterEmployee" class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500">
        <option value="">Tất cả nhân viên</option>
        <option v-for="e in employees" :key="e.id" :value="e.id">{{ e.fullName }}</option>
      </select>
      <select v-model="filterMonth" class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500">
        <option v-for="m in months" :key="m.value" :value="m.value">{{ m.label }}</option>
      </select>
      <select v-model="filterYear" class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500">
        <option v-for="y in years" :key="y" :value="y">{{ y }}</option>
      </select>
      <button class="h-9 rounded-lg bg-emerald-600 px-4 text-sm font-medium text-white hover:bg-emerald-700" @click="load">Xem</button>
    </div>
    <AppTable :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Không có dữ liệu chấm công">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-medium">{{ (row as AttendanceRecord).employeeName }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ fmtDate((row as AttendanceRecord).workDate) }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ (row as AttendanceRecord).shiftName ?? '—' }}</td>
        <td class="px-4 py-3 text-sm text-emerald-700">{{ fmtTime((row as AttendanceRecord).checkInAt) }}</td>
        <td class="px-4 py-3 text-sm text-blue-700">{{ fmtTime((row as AttendanceRecord).checkOutAt) }}</td>
        <td class="px-4 py-3 text-sm font-medium">{{ (row as AttendanceRecord).workedMinutes > 0 ? fmtMin((row as AttendanceRecord).workedMinutes) : '—' }}</td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />
  </div>
</template>

