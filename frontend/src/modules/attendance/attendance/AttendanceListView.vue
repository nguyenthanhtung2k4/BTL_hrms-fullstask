<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
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
import { useAuthStore } from '../../../stores/auth.ts'

const toast = useToastStore()
const records = ref<AttendanceRecord[]>([])
const employees = ref<Employee[]>([])
const departments = ref<Department[]>([])
const loading = ref(false)
const filterEmployee = ref('')
const filterDept = ref('')
const filterStatus = ref('')
const filterMonth = ref(new Date().getMonth() + 1)
const filterYear = ref(new Date().getFullYear())

const columns = [
  { key: 'employee', label: 'Nhân viên' },
  { key: 'workDate', label: 'Ngày' },
  { key: 'shiftName', label: 'Ca' },
  { key: 'checkInAt', label: 'Giờ vào' },
  { key: 'checkOutAt', label: 'Giờ ra' },
  { key: 'workedMinutes', label: 'Tổng giờ' },
]

const months = Array.from({ length: 12 }, (_, i) => ({ value: i + 1, label: `Tháng ${i + 1}` }))
const years = [2024, 2025, 2026]

const authStore = useAuthStore()
const isManagerOnly = computed(() => authStore.hasRole('Manager') && !authStore.isAdmin && !authStore.isHR)

async function load() {
  loading.value = true
  try {
    const [resRecords, resEmployees, resDepts] = await Promise.all([
      attendanceService.getAll({ employeeId: filterEmployee.value || undefined, month: filterMonth.value, year: filterYear.value }),
      employeeService.getAll(),
      isManagerOnly.value ? departmentService.getMyDepartments() : departmentService.getAll(), // ← thay đổi
    ])
    records.value = resRecords
    employees.value = resEmployees
    departments.value = resDepts
  } catch {
    toast.error('Không thể tải dữ liệu')
  } finally {
    loading.value = false
  }
}

function fmtTime(d?: string) {
  return d ? new Date(d).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '—'
}

function fmtDate(d: string) {
  return new Date(d).toLocaleDateString('vi-VN')
}

function fmtMin(m: number) {
  return m >= 60 ? `${Math.floor(m / 60)}h ${m % 60}m` : `${m}m`
}

const filteredRecords = computed(() => {
  let result = records.value
  
  if (filterDept.value) {
    result = result.filter((r) => {
      const emp = employees.value.find((e) => e.id === r.employeeId)
      return emp?.departmentId === filterDept.value
    })
  }
  
  if (filterStatus.value) {
    result = result.filter((r) => r.status === filterStatus.value)
  }
  
  return result
})

const { currentPage, perPage, paginatedData, total } = usePagination(filteredRecords)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Bảng chấm công" subtitle="Lịch sử chấm công toàn bộ nhân viên" :breadcrumbs="[{ label: 'Chấm công' }, { label: 'Chấm công' }]" />
    <!-- Thanh tìm kiếm & bộ lọc -->
    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4 mb-6 bg-slate-50 p-4 rounded-2xl border border-slate-150 shadow-sm">
      <!-- Nhân viên -->
      <div class="flex flex-col">
        <label class="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-1.5">Nhân viên</label>
        <div class="relative">
          <select
            v-model="filterEmployee"
            class="h-10 w-full rounded-xl border border-slate-300 bg-white px-3 pr-10 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 appearance-none"
          >
            <option value="">Tất cả nhân viên</option>
            <option v-for="e in employees" :key="e.id" :value="e.id">{{ e.fullName }}</option>
          </select>
          <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
          </div>
        </div>
      </div>

      <!-- Phòng ban -->
      <div class="flex flex-col">
        <label class="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-1.5">Phòng ban</label>
        <div class="relative">
          <select
            v-model="filterDept"
            class="h-10 w-full rounded-xl border border-slate-300 bg-white px-3 pr-10 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 appearance-none"
          >
            <option value="">Tất cả phòng ban</option>
            <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
          </select>
          <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
          </div>
        </div>
      </div>

      <!-- Trạng thái -->
      <div class="flex flex-col">
        <label class="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-1.5">Trạng thái</label>
        <div class="relative">
          <select
            v-model="filterStatus"
            class="h-10 w-full rounded-xl border border-slate-300 bg-white px-3 pr-10 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 appearance-none"
          >
            <option value="">Tất cả trạng thái</option>
            <option value="Completed">Hoàn thành</option>
            <option value="CheckedIn">Đang làm</option>
            <option value="Absent">Vắng mặt</option>
          </select>
          <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
          </div>
        </div>
      </div>

      <!-- Tháng -->
      <div class="flex flex-col">
        <label class="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-1.5">Tháng</label>
        <div class="relative">
          <select
            v-model="filterMonth"
            class="h-10 w-full rounded-xl border border-slate-300 bg-white px-3 pr-10 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 appearance-none"
          >
            <option v-for="m in months" :key="m.value" :value="m.value">{{ m.label }}</option>
          </select>
          <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
          </div>
        </div>
      </div>

      <!-- Năm -->
      <div class="flex flex-col">
        <label class="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-1.5">Năm</label>
        <div class="relative">
          <select
            v-model="filterYear"
            class="h-10 w-full rounded-xl border border-slate-300 bg-white px-3 pr-10 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 appearance-none"
          >
            <option v-for="y in years" :key="y" :value="y">{{ y }}</option>
          </select>
          <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
          </div>
        </div>
      </div>

      <!-- Hành động -->
      <div class="flex flex-col justify-end">
        <button
          class="h-10 w-full rounded-xl bg-gradient-to-r from-emerald-600 to-teal-600 text-white font-medium text-sm hover:from-emerald-700 hover:to-teal-700 transition-all shadow-md active:scale-[0.98]"
          @click="load"
        >
          Lọc dữ liệu
        </button>
      </div>
    </div>

    <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Không có dữ liệu chấm công">
      <template #default="{ row }">
        <td class="px-5 py-4 text-sm font-medium text-slate-900">{{ (row as AttendanceRecord).employeeName }}</td>
        <td class="px-5 py-4 text-sm text-slate-650 font-medium">{{ fmtDate((row as AttendanceRecord).workDate) }}</td>
        <td class="px-5 py-4 text-sm text-slate-600">{{ (row as AttendanceRecord).shiftName ?? '—' }}</td>
        <td class="px-5 py-4 text-sm text-emerald-700 font-mono font-semibold">{{ fmtTime((row as AttendanceRecord).checkInAt) }}</td>
        <td class="px-5 py-4 text-sm text-blue-700 font-mono font-semibold">{{ fmtTime((row as AttendanceRecord).checkOutAt) }}</td>
        <td class="px-5 py-4 text-sm font-semibold text-slate-800">{{ (row as AttendanceRecord).workedMinutes > 0 ? fmtMin((row as AttendanceRecord).workedMinutes) : '—' }}</td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />
  </div>
</template>

