<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { timesheetService } from '../../../services/timesheet.service'
import { employeeService } from '../../../services/employee.service'
import { departmentService } from '../../../services/department.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { Timesheet } from '../../../types/attendance.types'
import type { Employee, Department } from '../../../types/hr.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'

const auth = useAuthStore()
const toast = useToastStore()

const WORK_DAY_MINUTES = 480 // 8h per day

const timesheets = ref<Timesheet[]>([])
const employees = ref<Employee[]>([])
const departments = ref<Department[]>([])
const loading = ref(false)
const calculating = ref(false)
const filterMonth = ref(new Date().getMonth() + 1)
const filterYear = ref(new Date().getFullYear())
const filterDept = ref('')
const filterStatus = ref('')
const searchEmployee = ref('')

const columns = [
  { key: 'employee', label: 'Nhân viên' },
  { key: 'workDays', label: 'Ngày công' },
  { key: 'totalHours', label: 'Tổng giờ' },
  { key: 'paidLeaveDays', label: 'Phép CL' },
  { key: 'unpaidLeaveDays', label: 'Phép KL' },
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
    const [resTimesheets, resEmployees, resDepts] = await Promise.all([
      timesheetService.getAll({
        month: filterMonth.value, year: filterYear.value,
        employeeId: auth.isManager ? undefined : auth.employeeId,
      }),
      employeeService.getAll(),
      departmentService.getAll(),
    ])
    timesheets.value = resTimesheets
    employees.value = resEmployees
    departments.value = resDepts
  } catch {
    toast.error('Không thể tải bảng công')
  } finally {
    loading.value = false
  }
}

async function calculate() {
  calculating.value = true
  try {
    await timesheetService.calculate(filterMonth.value, filterYear.value)
    toast.success(`Đã tính bảng công Tháng ${filterMonth.value}/${filterYear.value}`)
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message ?? 'Tính bảng công thất bại')
  } finally {
    calculating.value = false
  }
}

const filteredTimesheets = computed(() => {
  let result = timesheets.value
  
  if (filterDept.value) {
    result = result.filter((t) => {
      const emp = employees.value.find((e) => e.id === t.employeeId)
      return emp?.departmentId === filterDept.value
    })
  }
  
  if (filterStatus.value) {
    result = result.filter((t) => t.status === filterStatus.value)
  }
  
  if (searchEmployee.value) {
    const q = searchEmployee.value.toLowerCase()
    result = result.filter((t) => t.employeeName?.toLowerCase().includes(q))
  }
  
  return result
})

const { currentPage, perPage, paginatedData, total } = usePagination(filteredTimesheets)

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

    <!-- Thanh tìm kiếm & bộ lọc -->
    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4 mb-6 bg-slate-50 p-4 rounded-2xl border border-slate-150 shadow-sm">
      <!-- Tìm kiếm -->
      <div class="flex flex-col">
        <label class="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-1.5">Tìm kiếm</label>
        <div class="relative">
          <input
            v-model="searchEmployee"
            type="text"
            placeholder="Tên nhân viên..."
            class="h-10 w-full rounded-xl border border-slate-300 bg-white px-3.5 pl-9 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100"
          />
          <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none text-slate-400">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" /></svg>
          </div>
        </div>
      </div>

      <!-- Phòng ban (chỉ hiển thị cho vai trò quản lý/HR/admin) -->
      <div v-if="auth.isManager" class="flex flex-col">
        <label class="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-1.5">Phòng ban</label>
        <div class="relative">
          <select
            v-model="filterDept"
            class="h-10 w-full rounded-xl border border-slate-300 bg-white px-3 pr-10 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 appearance-none"
          >
            <option value="">Tất cả phòng ban</option>
            <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
          </select>
          <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3.5 text-slate-400">
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
            <option value="Calculated">Đã tính</option>
            <option value="Approved">Đã duyệt</option>
            <option value="Locked">Đã khóa</option>
          </select>
          <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3.5 text-slate-400">
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
          <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3.5 text-slate-400">
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
          <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3.5 text-slate-400">
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
          Xem kết quả
        </button>
      </div>
    </div>

    <AppTable :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có bảng công — hãy nhấn Tính bảng công">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-medium">{{ (row as Timesheet).employeeName }}</td>
        <td class="px-4 py-3 text-sm font-semibold text-emerald-700">{{ workDays(row as Timesheet) }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ totalHours(row as Timesheet) }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as Timesheet).paidLeaveDays > 0 ? (row as Timesheet).paidLeaveDays : '—' }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as Timesheet).unpaidLeaveDays > 0 ? (row as Timesheet).unpaidLeaveDays : '—' }}</td>
        <td class="px-4 py-3"><AppBadge :status="(row as Timesheet).status" /></td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />
  </div>
</template>

