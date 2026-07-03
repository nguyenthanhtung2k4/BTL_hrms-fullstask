<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { attendanceService } from '../../../services/attendance.service'
import { employeeService } from '../../../services/employee.service'
import { departmentService } from '../../../services/department.service'
import { useToastStore } from '../../../stores/toast'
import { exportToExcel } from '../../../utils/excel'
import type { AttendanceRecord, AttendanceAdjustment } from '../../../types/attendance.types'
import type { Employee, Department } from '../../../types/hr.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import { useAuthStore } from '../../../stores/auth.ts'
import AppButton from '../../../components/ui/AppButton.vue'

const toast = useToastStore()
const records = ref<AttendanceRecord[]>([])
const employees = ref<Employee[]>([])
const departments = ref<Department[]>([])
const loading = ref(false)

// Tabs: records (Bảng chấm công) or adjustments (Duyệt giải trình)
const activeMainTab = ref<'records' | 'adjustments'>('records')

// Records filters
const filterEmployee = ref('')
const filterDept = ref('')
const filterStatus = ref('')
const filterMonth = ref(new Date().getMonth() + 1)
const filterYear = ref(new Date().getFullYear())

// Adjustments state & filters
const adjustments = ref<AttendanceAdjustment[]>([])
const adjFilterStatus = ref('Pending')
const adjFilterEmployee = ref('')
const adjActionLoading = ref<Record<string, boolean>>({})

const months = Array.from({ length: 12 }, (_, i) => ({ value: i + 1, label: `Tháng ${i + 1}` }))
const years = [2024, 2025, 2026]

const authStore = useAuthStore()
const isManagerOnly = computed(() => authStore.hasRole('Manager') && !authStore.isAdmin && !authStore.isHR)
const canApprove = computed(() => authStore.isAdmin || authStore.isHR || authStore.hasRole('Manager'))

const recordColumns = [
  { key: 'employee', label: 'Nhân viên' },
  { key: 'workDate', label: 'Ngày làm việc' },
  { key: 'shiftName', label: 'Ca làm việc' },
  { key: 'checkInAt', label: 'Giờ vào / Lý do' },
  { key: 'checkOutAt', label: 'Giờ ra / Lý do' },
  { key: 'workedMinutes', label: 'Tổng giờ làm' },
  { key: 'status', label: 'Trạng thái' }
]

const adjustmentColumns = [
  { key: 'employee', label: 'Nhân viên' },
  { key: 'workDate', label: 'Ngày giải trình' },
  { key: 'shiftName', label: 'Ca đề xuất' },
  { key: 'proposedCheckIn', label: 'Check-in đề xuất' },
  { key: 'proposedCheckOut', label: 'Check-out đề xuất' },
  { key: 'reason', label: 'Lý do giải trình' },
  { key: 'status', label: 'Trạng thái / Thao tác' }
]

async function load() {
  loading.value = true
  try {
    const [resRecords, resEmployees, resDepts] = await Promise.all([
      attendanceService.getAll({ employeeId: filterEmployee.value || undefined, month: filterMonth.value, year: filterYear.value }),
      employeeService.getAll(),
      isManagerOnly.value ? departmentService.getMyDepartments() : departmentService.getAll(),
    ])
    records.value = resRecords
    employees.value = resEmployees
    departments.value = resDepts

    if (canApprove.value) {
      await loadAdjustments()
    }
  } catch {
    toast.error('Không thể tải dữ liệu')
  } finally {
    loading.value = false
  }
}

async function loadAdjustments() {
  try {
    adjustments.value = await attendanceService.getAdjustments({
      employeeId: adjFilterEmployee.value || undefined,
      status: adjFilterStatus.value || undefined
    })
  } catch {
    // Ignored
  }
}

async function approveAdj(id: string) {
  adjActionLoading.value[id] = true
  try {
    await attendanceService.approveAdjustment(id)
    toast.success('Phê duyệt đơn giải trình thành công!')
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message ?? 'Phê duyệt đơn thất bại')
  } finally {
    adjActionLoading.value[id] = false
  }
}

async function rejectAdj(id: string) {
  adjActionLoading.value[id] = true
  try {
    await attendanceService.rejectAdjustment(id)
    toast.success('Từ chối đơn giải trình thành công!')
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message ?? 'Từ chối đơn thất bại')
  } finally {
    adjActionLoading.value[id] = false
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

// Dynamic stats calculations
const stats = computed(() => {
  const list = filteredRecords.value
  const active = list.filter((r) => r.status === 'CheckedIn').length
  const completed = list.filter((r) => r.status === 'Completed').length
  const late = list.filter((r) => !!r.checkInReason).length
  const totalMinutes = list.reduce((sum, r) => sum + r.workedMinutes, 0)
  const avgHours = list.length > 0 ? (totalMinutes / list.length / 60).toFixed(1) : '0'

  return {
    active,
    completed,
    late,
    avgHours
  }
})

function handleExport() {
  if (filteredRecords.value.length === 0) {
    toast.error('Không có dữ liệu để xuất Excel.')
    return
  }

  const exportData = filteredRecords.value.map((r) => ({
    'Họ Tên': r.employeeName,
    'Ngày Làm Việc': fmtDate(r.workDate),
    'Ca Làm': r.shiftName || '—',
    'Giờ Check-in': fmtTime(r.checkInAt),
    'Lý do Check-in': r.checkInReason || '—',
    'Giờ Check-out': fmtTime(r.checkOutAt),
    'Lý do Check-out': r.checkOutReason || '—',
    'Số Giờ Làm': (r.workedMinutes / 60).toFixed(2),
    'Trạng Thái': r.status === 'Completed' ? 'Hoàn thành' : r.status === 'CheckedIn' ? 'Đang làm' : r.status
  }))

  try {
    exportToExcel(exportData, `Bang_Cham_Cong_Thang_${filterMonth.value}_${filterYear.value}`, 'Bảng Chấm Công')
    toast.success('Xuất file Excel thành công!')
  } catch (err: any) {
    toast.error(err.message || 'Xuất Excel thất bại.')
  }
}

onMounted(load)
</script>

<template>
  <div class="space-y-6">
    <PageHeader title="Bảng chấm công" subtitle="Quản lý lịch sử chấm công & giải trình của nhân viên" :breadcrumbs="[{ label: 'Chấm công' }, { label: 'Bảng công' }]" />

    <!-- Main Navigation Tabs -->
    <div class="border-b border-slate-200">
      <nav class="flex space-x-6" aria-label="Tabs">
        <button
          @click="activeMainTab = 'records'"
          :class="[
            'pb-4 px-1 text-sm font-bold border-b-2 transition-all outline-none',
            activeMainTab === 'records'
              ? 'border-emerald-600 text-emerald-600'
              : 'border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300'
          ]"
        >
          Bảng chấm công
        </button>
        <button
          v-if="canApprove"
          @click="activeMainTab = 'adjustments'"
          :class="[
            'pb-4 px-1 text-sm font-bold border-b-2 transition-all outline-none',
            activeMainTab === 'adjustments'
              ? 'border-emerald-600 text-emerald-600'
              : 'border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300'
          ]"
        >
          Duyệt đơn giải trình
        </button>
      </nav>
    </div>

    <div v-if="activeMainTab === 'records'" class="space-y-6">

      <!-- Filters & Action Bar (Compact & Space-Optimized) -->
      <div class="bg-white p-3.5 rounded-2xl border border-slate-200 shadow-sm mb-4">
        <div class="flex flex-wrap items-end gap-3 justify-between">
          <!-- Filter Fields Grid -->
          <div class="flex flex-wrap items-center gap-3 flex-1 min-w-[280px]">
            <!-- Nhân viên -->
            <div class="flex flex-col min-w-[140px] flex-1 sm:max-w-xs">
              <label class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">Nhân viên</label>
              <div class="relative">
                <select
                  v-model="filterEmployee"
                  class="h-9 w-full rounded-xl border border-slate-250 bg-slate-50/50 px-3 pr-8 text-xs outline-none transition-all focus:border-emerald-500 focus:bg-white focus:ring-2 focus:ring-emerald-100 appearance-none text-slate-700 font-medium"
                >
                  <option value="">Tất cả nhân viên</option>
                  <option v-for="e in employees" :key="e.id" :value="e.id">{{ e.fullName }}</option>
                </select>
                <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-2.5 text-slate-400">
                  <svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
                </div>
              </div>
            </div>

            <!-- Phòng ban -->
            <div class="flex flex-col min-w-[130px] flex-1 sm:max-w-[200px]">
              <label class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">Phòng ban</label>
              <div class="relative">
                <select
                  v-model="filterDept"
                  class="h-9 w-full rounded-xl border border-slate-250 bg-slate-50/50 px-3 pr-8 text-xs outline-none transition-all focus:border-emerald-500 focus:bg-white focus:ring-2 focus:ring-emerald-100 appearance-none text-slate-700 font-medium"
                >
                  <option value="">Tất cả phòng ban</option>
                  <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
                </select>
                <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-2.5 text-slate-400">
                  <svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
                </div>
              </div>
            </div>

            <!-- Trạng thái -->
            <div class="flex flex-col min-w-[120px] flex-1 sm:max-w-[160px]">
              <label class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">Trạng thái</label>
              <div class="relative">
                <select
                  v-model="filterStatus"
                  class="h-9 w-full rounded-xl border border-slate-250 bg-slate-50/50 px-3 pr-8 text-xs outline-none transition-all focus:border-emerald-500 focus:bg-white focus:ring-2 focus:ring-emerald-100 appearance-none text-slate-700 font-medium"
                >
                  <option value="">Tất cả trạng thái</option>
                  <option value="Completed">Hoàn thành</option>
                  <option value="CheckedIn">Đang làm</option>
                  <option value="Absent">Vắng mặt</option>
                </select>
                <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-2.5 text-slate-400">
                  <svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
                </div>
              </div>
            </div>

            <!-- Tháng -->
            <div class="flex flex-col min-w-[80px] flex-1 sm:max-w-[120px]">
              <label class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">Tháng</label>
              <div class="relative">
                <select
                  v-model="filterMonth"
                  class="h-9 w-full rounded-xl border border-slate-250 bg-slate-50/50 px-3 pr-8 text-xs outline-none transition-all focus:border-emerald-500 focus:bg-white focus:ring-2 focus:ring-emerald-100 appearance-none text-slate-700 font-medium"
                >
                  <option v-for="m in months" :key="m.value" :value="m.value">{{ m.label }}</option>
                </select>
                <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-2.5 text-slate-400">
                  <svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
                </div>
              </div>
            </div>

            <!-- Năm -->
            <div class="flex flex-col min-w-[70px] flex-1 sm:max-w-[100px]">
              <label class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">Năm</label>
              <div class="relative">
                <select
                  v-model="filterYear"
                  class="h-9 w-full rounded-xl border border-slate-250 bg-slate-50/50 px-3 pr-8 text-xs outline-none transition-all focus:border-emerald-500 focus:bg-white focus:ring-2 focus:ring-emerald-100 appearance-none text-slate-700 font-medium"
                >
                  <option v-for="y in years" :key="y" :value="y">{{ y }}</option>
                </select>
                <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-2.5 text-slate-400">
                  <svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
                </div>
              </div>
            </div>
          </div>

          <!-- Buttons -->
          <div class="flex items-center gap-2 mt-2 sm:mt-0">
            <button
              @click="handleExport"
              class="h-9 inline-flex items-center justify-center px-4 rounded-xl border border-slate-250 bg-white text-xs font-semibold text-slate-700 hover:bg-slate-50 hover:text-slate-900 transition-colors gap-1.5"
            >
              <svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" /></svg>
              Xuất Excel
            </button>
            <button
              @click="load"
              class="h-9 inline-flex items-center justify-center px-5 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-xs font-semibold text-white transition-colors"
            >
              Lọc dữ liệu
            </button>
          </div>
        </div>
      </div>

      <!-- Table -->
      <div class="bg-white border border-slate-200 rounded-3xl overflow-hidden shadow-sm animate-fade-in">
        <AppTable :page-size="10" :columns="recordColumns" :rows="filteredRecords" :loading="loading" row-key="id" empty-text="Không có dữ liệu chấm công">
          <template #default="{ row }">
            <td class="px-5 py-4 text-sm font-semibold text-slate-900">{{ (row as AttendanceRecord).employeeName }}</td>
            <td class="px-5 py-4 text-sm text-slate-650 font-medium">{{ fmtDate((row as AttendanceRecord).workDate) }}</td>
            <td class="px-5 py-4 text-sm text-slate-600 font-medium">{{ (row as AttendanceRecord).shiftName ?? '—' }}</td>
            <td class="px-5 py-4">
              <div>
                <span class="text-sm text-emerald-700 font-mono font-semibold">{{ fmtTime((row as AttendanceRecord).checkInAt) }}</span>
                <p v-if="(row as AttendanceRecord).checkInReason" class="text-[11px] text-amber-700 italic mt-0.5 font-medium max-w-[180px] truncate" :title="(row as AttendanceRecord).checkInReason">Lý do: {{ (row as AttendanceRecord).checkInReason }}</p>
              </div>
            </td>
            <td class="px-5 py-4">
              <div>
                <span class="text-sm text-blue-700 font-mono font-semibold">{{ fmtTime((row as AttendanceRecord).checkOutAt) }}</span>
                <p v-if="(row as AttendanceRecord).checkOutReason" class="text-[11px] text-amber-700 italic mt-0.5 font-medium max-w-[180px] truncate" :title="(row as AttendanceRecord).checkOutReason">Lý do: {{ (row as AttendanceRecord).checkOutReason }}</p>
              </div>
            </td>
            <td class="px-5 py-4 text-sm font-bold text-slate-800">{{ (row as AttendanceRecord).workedMinutes > 0 ? fmtMin((row as AttendanceRecord).workedMinutes) : '—' }}</td>
            <td class="px-5 py-4 text-sm">
              <span
                class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold border"
                :class="[
                  (row as AttendanceRecord).status === 'Completed' ? 'bg-emerald-50 text-emerald-700 border-emerald-100' :
                  (row as AttendanceRecord).status === 'CheckedIn' ? 'bg-amber-50 text-amber-700 border-amber-100' :
                  'bg-slate-50 text-slate-600 border-slate-100'
                ]"
              >
                {{ (row as AttendanceRecord).status === 'Completed' ? 'Hoàn thành' : (row as AttendanceRecord).status === 'CheckedIn' ? 'Đang làm' : (row as AttendanceRecord).status }}
              </span>
            </td>
          </template>
        </AppTable>
      </div>
    </div>

    <!-- TAB 2: Correction Requests Approval List -->
    <div v-else-if="canApprove" class="space-y-6">
      <!-- Filters -->
      <div class="bg-slate-50 p-5 rounded-2xl border border-slate-200 shadow-sm space-y-4">
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
          <!-- Nhân viên -->
          <div class="flex flex-col">
            <label class="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-1.5">Nhân viên</label>
            <div class="relative">
              <select
                v-model="adjFilterEmployee"
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

          <!-- Trạng thái -->
          <div class="flex flex-col">
            <label class="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-1.5">Trạng thái giải trình</label>
            <div class="relative">
              <select
                v-model="adjFilterStatus"
                class="h-10 w-full rounded-xl border border-slate-300 bg-white px-3 pr-10 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 appearance-none"
              >
                <option value="">Tất cả trạng thái</option>
                <option value="Pending">Chờ duyệt</option>
                <option value="Approved">Đã duyệt</option>
                <option value="Rejected">Từ chối</option>
              </select>
              <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400">
                <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
              </div>
            </div>
          </div>

          <!-- Filter Action -->
          <div class="flex flex-col justify-end">
            <AppButton variant="primary" @click="loadAdjustments" class="h-10 w-full">
              Lọc danh sách
            </AppButton>
          </div>
        </div>
      </div>

      <!-- Table -->
      <div class="bg-white border border-slate-200 rounded-3xl overflow-hidden shadow-sm animate-fade-in">
        <AppTable :page-size="10" :columns="adjustmentColumns" :rows="adjustments" :loading="loading" row-key="id" empty-text="Không có đơn giải trình nào phù hợp">
          <template #default="{ row }">
            <td class="px-5 py-4 text-sm font-semibold text-slate-900">{{ (row as AttendanceAdjustment).employeeName }}</td>
            <td class="px-5 py-4 text-sm text-slate-650 font-medium">{{ fmtDate((row as AttendanceAdjustment).workDate) }}</td>
            <td class="px-5 py-4 text-sm text-slate-600 font-medium">{{ (row as AttendanceAdjustment).shiftName }}</td>
            <td class="px-5 py-4 text-sm text-emerald-700 font-mono font-semibold">
              {{ (row as AttendanceAdjustment).proposedCheckIn ? fmtTime((row as AttendanceAdjustment).proposedCheckIn) : '—' }}
            </td>
            <td class="px-5 py-4 text-sm text-blue-700 font-mono font-semibold">
              {{ (row as AttendanceAdjustment).proposedCheckOut ? fmtTime((row as AttendanceAdjustment).proposedCheckOut) : '—' }}
            </td>
            <td class="px-5 py-4 text-sm text-slate-600 max-w-[220px] truncate" :title="(row as AttendanceAdjustment).reason">
              {{ (row as AttendanceAdjustment).reason }}
            </td>
            <td class="px-5 py-4 text-sm">
              <div v-if="(row as AttendanceAdjustment).status === 'Pending'" class="flex gap-2">
                <AppButton
                  size="sm"
                  class="bg-emerald-600 text-white border-0 hover:bg-emerald-700 py-1 px-3 rounded-lg text-xs"
                  :loading="adjActionLoading[(row as AttendanceAdjustment).id]"
                  @click="approveAdj((row as AttendanceAdjustment).id)"
                >
                  Duyệt
                </AppButton>
                <AppButton
                  size="sm"
                  class="bg-rose-600 text-white border-0 hover:bg-rose-700 py-1 px-3 rounded-lg text-xs"
                  :loading="adjActionLoading[(row as AttendanceAdjustment).id]"
                  @click="rejectAdj((row as AttendanceAdjustment).id)"
                >
                  Từ chối
                </AppButton>
              </div>
              <div v-else>
                <span
                  class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold border"
                  :class="[
                    (row as AttendanceAdjustment).status === 'Approved' ? 'bg-emerald-50 text-emerald-700 border-emerald-100' :
                    'bg-rose-50 text-rose-700 border-rose-100'
                  ]"
                >
                  {{ (row as AttendanceAdjustment).status === 'Approved' ? 'Đã duyệt' : 'Đã từ chối' }}
                </span>
                <p v-if="(row as AttendanceAdjustment).handledByName" class="text-[10px] text-slate-400 mt-1">Duyệt bởi: {{ (row as AttendanceAdjustment).handledByName }}</p>
              </div>
            </td>
          </template>
        </AppTable>
      </div>
    </div>
  </div>
</template>
