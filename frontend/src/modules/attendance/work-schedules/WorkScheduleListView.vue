<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { workScheduleService } from '../../../services/workSchedule.service'
import { employeeService } from '../../../services/employee.service'
import { shiftService } from '../../../services/shift.service'
import { departmentService } from '../../../services/department.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { WorkSchedule, Shift } from '../../../types/attendance.types'
import type { Employee, Department } from '../../../types/hr.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'
import AppModal from '../../../components/ui/AppModal.vue'
import AppInput from '../../../components/ui/AppInput.vue'
import AppConfirm from '../../../components/ui/AppConfirm.vue'
import ScheduleImportModal from './ScheduleImportModal.vue'
import {
  Calendar,
  Clock,
  Plus,
  Search,
  Edit,
  Trash2,
  ChevronLeft,
  ChevronRight,
  Grid,
  List,
  FileSpreadsheet
} from '@lucide/vue'

const auth = useAuthStore()
const toast = useToastStore()
const schedules = ref<WorkSchedule[]>([])
const employees = ref<Employee[]>([])
const shifts = ref<Shift[]>([])
const departments = ref<Department[]>([])
const loading = ref(false)
const showForm = ref(false)
const showImport = ref(false)
const deleteTarget = ref<WorkSchedule | null>(null)
const deleteLoading = ref(false)
const saving = ref(false)
const search = ref('')
const selectedDeptId = ref('')
const selectedStatus = ref('')
const sortByDate = ref<'desc' | 'asc'>('desc')

// View mode: 'grid' (Bảng tuần) or 'list' (Danh sách dòng)
const viewMode = ref<'grid' | 'list'>('grid')

// Grid helper functions and state
function getMonday(d: Date) {
  const date = new Date(d)
  const day = date.getDay()
  const diff = date.getDate() - day + (day === 0 ? -6 : 1) // adjust when day is sunday
  const monday = new Date(date.setDate(diff))
  monday.setHours(0, 0, 0, 0)
  return monday
}

const currentWeekStart = ref(getMonday(new Date()))

const currentWeekDays = computed(() => {
  const start = new Date(currentWeekStart.value)
  const days = []
  for (let i = 0; i < 7; i++) {
    const d = new Date(start)
    d.setDate(start.getDate() + i)
    days.push(d)
  }
  return days
})

function prevWeek() {
  const d = new Date(currentWeekStart.value)
  d.setDate(d.getDate() - 7)
  currentWeekStart.value = d
}

function nextWeek() {
  const d = new Date(currentWeekStart.value)
  d.setDate(d.getDate() + 7)
  currentWeekStart.value = d
}

function goToday() {
  currentWeekStart.value = getMonday(new Date())
}

function formatDateToYMD(date: Date) {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

// Form for bulk assignment
const form = ref({ employeeId: '', shiftId: '', startDate: '', endDate: '' })
const errors = ref<Record<string, string>>({})

// Form/State for editing single schedule
const editTarget = ref<WorkSchedule | null>(null)
const editForm = ref({ shiftId: '', status: '', workDate: '' })
const editErrors = ref<Record<string, string>>({})
const editSaving = ref(false)

const columns = [
  { key: 'employee', label: 'Nhân viên' },
  { key: 'shift', label: 'Ca làm việc' },
  { key: 'workDate', label: 'Ngày làm việc' },
  { key: 'status', label: 'Trạng thái' },
  ...(auth.isManager ? [{ key: 'actions', label: '', class: 'text-right' }] : []),
]

// Filter active employees only
const activeEmployees = computed(() => {
  return employees.value.filter((e) => e.status === 'Active')
})

// Filtered employees for the weekly grid rows (incorporating search + department filters)
const filteredEmployeesForGrid = computed(() => {
  let list = employees.value

  // For normal employee, restrict list to themselves
  if (!auth.isManager && !auth.isPayrollStaff) {
    list = list.filter((e) => e.id === auth.employeeId)
  } else {
    // If Admin/HR/Manager/PayrollStaff, apply filters
    if (selectedDeptId.value) {
      list = list.filter((e) => e.departmentId === selectedDeptId.value)
    }
  }

  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(
      (e) =>
        e.fullName.toLowerCase().includes(q) ||
        e.employeeCode?.toLowerCase().includes(q)
    )
  }

  // Sort: Active first, then by name
  return [...list].sort((a, b) => {
    if (a.status === 'Active' && b.status !== 'Active') return -1
    if (a.status !== 'Active' && b.status === 'Active') return 1
    return a.fullName.localeCompare(b.fullName)
  })
})

const filtered = computed(() => {
  let result = schedules.value

  // Lọc theo phòng ban
  if (selectedDeptId.value) {
    result = result.filter((s) => {
      const emp = employees.value.find((e) => e.id === s.employeeId)
      return emp?.departmentId === selectedDeptId.value
    })
  }

  // Lọc theo trạng thái
  if (selectedStatus.value) {
    result = result.filter((s) => s.status === selectedStatus.value)
  }

  if (search.value) {
    const q = search.value.toLowerCase()
    result = result.filter(
      (s) =>
        s.employeeName?.toLowerCase().includes(q) ||
        s.shiftName?.toLowerCase().includes(q) ||
        s.workDate?.includes(q)
    )
  }
  // Sắp xếp theo ngày làm việc
  return [...result].sort((a, b) => {
    const timeA = new Date(a.workDate).getTime()
    const timeB = new Date(b.workDate).getTime()
    return sortByDate.value === 'desc' ? timeB - timeA : timeA - timeB
  })
})

async function load() {
  loading.value = true
  try {
    // Quyền view-all: Admin, HR, Manager, PayrollStaff
    const canViewAll = auth.isManager || auth.isPayrollStaff
    const params = canViewAll ? undefined : { employeeId: auth.employeeId }
    const resSchedules = await workScheduleService.getAll(params || {})
    schedules.value = resSchedules
    
    // Luôn tải danh sách nhân viên & phòng ban để phục vụ mapping lọc
    const [resEmployees, resShifts, resDepts] = await Promise.all([
      employeeService.getAll(),
      shiftService.getAll(),
      departmentService.getAll()
    ])
    employees.value = resEmployees
    shifts.value = resShifts
    departments.value = resDepts
  } catch {
    toast.error('Không thể tải dữ liệu lịch làm việc')
  } finally {
    loading.value = false
  }
}

function validate() {
  errors.value = {}
  if (!form.value.employeeId) errors.value.employeeId = 'Nhân viên bắt buộc'
  if (!form.value.shiftId) errors.value.shiftId = 'Ca làm bắt buộc'
  if (!form.value.startDate) errors.value.startDate = 'Từ ngày bắt buộc'
  if (!form.value.endDate) errors.value.endDate = 'Đến ngày bắt buộc'
  if (form.value.startDate && form.value.endDate && form.value.startDate > form.value.endDate) {
    errors.value.endDate = 'Đến ngày phải sau hoặc bằng từ ngày'
  }
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    const start = new Date(form.value.startDate)
    const end = new Date(form.value.endDate)
    const promises = []
    
    // Lặp qua từng ngày trong khoảng thời gian được phân để lưu
    for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
      const workDateStr = formatDateToYMD(d)
      promises.push(
        workScheduleService.create({
          employeeId: form.value.employeeId,
          shiftId: form.value.shiftId,
          workDate: workDateStr
        })
      )
    }
    
    await Promise.all(promises)
    toast.success('Phân lịch làm việc thành công')
    showForm.value = false
    // Reset form
    form.value = { employeeId: '', shiftId: '', startDate: '', endDate: '' }
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message ?? 'Lưu lịch làm việc thất bại')
  } finally {
    saving.value = false
  }
}

// Single edit modal triggers
function openEdit(schedule: WorkSchedule) {
  editTarget.value = schedule
  editForm.value = {
    shiftId: schedule.shiftId,
    status: schedule.status || 'Planned',
    workDate: schedule.workDate
  }
  editErrors.value = {}
}

// Quick assign single day cell click
function quickAssign(employeeId: string, date: Date) {
  if (!auth.isManager) return
  const dateStr = formatDateToYMD(date)
  form.value = {
    employeeId,
    shiftId: shifts.value[0]?.id || '',
    startDate: dateStr,
    endDate: dateStr
  }
  showForm.value = true
}

function validateEdit() {
  editErrors.value = {}
  if (!editForm.value.shiftId) editErrors.value.shiftId = 'Ca làm bắt buộc'
  if (!editForm.value.workDate) editErrors.value.workDate = 'Ngày làm bắt buộc'
  return Object.keys(editErrors.value).length === 0
}

async function saveEdit() {
  if (!editTarget.value || !validateEdit()) return
  editSaving.value = true
  try {
    await workScheduleService.update(editTarget.value.id, {
      shiftId: editForm.value.shiftId,
      status: editForm.value.status,
      workDate: editForm.value.workDate
    })
    toast.success('Cập nhật lịch làm việc thành công')
    editTarget.value = null
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message ?? 'Cập nhật lịch thất bại')
  } finally {
    editSaving.value = false
  }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try {
    await workScheduleService.delete(deleteTarget.value.id)
    toast.success('Đã xóa lịch làm việc')
    deleteTarget.value = null
    await load()
  } catch {
    toast.error('Xóa lịch làm việc thất bại')
  } finally {
    deleteLoading.value = false
  }
}

function fmt(d: string) {
  if (!d) return ''
  return new Date(d).toLocaleDateString('vi-VN')
}

// Styling classes for status badges
function getStatusClass(status: string) {
  switch (status) {
    case 'Completed':
      return 'bg-emerald-50 text-emerald-700 border-emerald-150'
    case 'Absent':
      return 'bg-rose-50 text-rose-700 border-rose-150'
    case 'Planned':
    default:
      return 'bg-blue-50 text-blue-700 border-blue-150'
  }
}

function getStatusLabel(status: string) {
  switch (status) {
    case 'Completed':
      return 'Completed'
    case 'Absent':
      return 'Absent'
    case 'Planned':
    default:
      return 'Planned'
  }
}

// Find schedule helper for the weekly grid cells
function findSchedule(employeeId: string, date: Date): WorkSchedule | undefined {
  const dateStr = formatDateToYMD(date)
  return schedules.value.find(
    (s) => s.employeeId === employeeId && s.workDate === dateStr
  )
}

const { currentPage, perPage, paginatedData, total } = usePagination(filtered)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Lịch làm việc" subtitle="Phân ca làm việc và theo dõi lịch trình của nhân sự" :breadcrumbs="[{ label: 'Chấm công' }, { label: 'Lịch làm việc' }]">
      <template #actions>
        <div class="flex items-center gap-2">
          <!-- Toggle View Mode: Grid / List -->
          <div class="inline-flex rounded-xl bg-slate-100 p-1 border border-slate-200 mr-2 shadow-sm">
            <button
              type="button"
              @click="viewMode = 'grid'"
              :class="['p-1.5 rounded-lg transition-all', viewMode === 'grid' ? 'bg-white text-emerald-600 shadow-sm' : 'text-slate-500 hover:text-slate-700']"
              title="Bảng tuần"
            >
              <Grid class="h-4 w-4" />
            </button>
            <button
              type="button"
              @click="viewMode = 'list'"
              :class="['p-1.5 rounded-lg transition-all', viewMode === 'list' ? 'bg-white text-emerald-600 shadow-sm' : 'text-slate-500 hover:text-slate-700']"
              title="Danh sách"
            >
              <List class="h-4 w-4" />
            </button>
          </div>

          <template v-if="auth.isManager">
            <AppButton variant="secondary" @click="showImport = true" class="flex items-center gap-1.5 border-slate-300 hover:border-emerald-300">
              <FileSpreadsheet class="h-4 w-4 text-emerald-600" />
              <span>Nhập Excel</span>
            </AppButton>
            <AppButton @click="showForm = true" class="flex items-center gap-1.5 shadow-md shadow-emerald-100">
              <Plus class="h-4 w-4" />
              <span>Phân lịch</span>
            </AppButton>
          </template>
        </div>
      </template>
    </PageHeader>

    <!-- Combined Filter and Week Navigation Bar -->
    <div class="bg-white border border-slate-200 rounded-2xl p-3.5 shadow-sm mb-4 space-y-3">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <!-- Left: Search and Department Filters (Compact) -->
        <div class="flex flex-wrap items-center gap-3 flex-1 min-w-[280px]">
          <!-- Compact Search Input -->
          <div class="relative w-full sm:max-w-xs">
            <input
              v-model="search"
              type="text"
              placeholder="Tìm nhân viên, ca, ngày..."
              class="h-9 w-full rounded-xl border border-slate-250 bg-slate-50/50 px-3 pl-9 text-xs outline-none transition-all focus:border-emerald-500 focus:bg-white focus:ring-2 focus:ring-emerald-100 placeholder:text-slate-400"
            />
            <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none text-slate-400">
              <Search class="h-3.5 w-3.5" />
            </div>
          </div>

          <!-- Compact Department Dropdown -->
          <div v-if="auth.isManager || auth.isPayrollStaff" class="relative w-full sm:w-48">
            <select
              v-model="selectedDeptId"
              class="h-9 w-full rounded-xl border border-slate-250 bg-slate-50/50 px-3 pr-8 text-xs outline-none transition-all focus:border-emerald-500 focus:bg-white focus:ring-2 focus:ring-emerald-100 appearance-none text-slate-700 font-medium"
            >
              <option value="">Tất cả phòng ban</option>
              <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
            </select>
            <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400">
              <svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
            </div>
          </div>

          <!-- Compact Status Dropdown (List View only) -->
          <div v-if="viewMode === 'list'" class="relative w-full sm:w-40">
            <select
              v-model="selectedStatus"
              class="h-9 w-full rounded-xl border border-slate-250 bg-slate-50/50 px-3 pr-8 text-xs outline-none transition-all focus:border-emerald-500 focus:bg-white focus:ring-2 focus:ring-emerald-100 appearance-none text-slate-700 font-medium"
            >
              <option value="">Tất cả trạng thái</option>
              <option value="Planned">Planned</option>
              <option value="Completed">Completed</option>
              <option value="Absent">Absent</option>
            </select>
            <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400">
              <svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
            </div>
          </div>

          <!-- Compact Sort Dropdown (List View only) -->
          <div v-if="viewMode === 'list'" class="relative w-full sm:w-40">
            <select
              v-model="sortByDate"
              class="h-9 w-full rounded-xl border border-slate-250 bg-slate-50/50 px-3 pr-8 text-xs outline-none transition-all focus:border-emerald-500 focus:bg-white focus:ring-2 focus:ring-emerald-100 appearance-none text-slate-700 font-medium"
            >
              <option value="desc">Mới nhất trước</option>
              <option value="asc">Cũ nhất trước</option>
            </select>
            <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400">
              <svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
            </div>
          </div>
        </div>

        <!-- Right: Week Nav (Grid View only) -->
        <div v-if="viewMode === 'grid'" class="flex items-center gap-2">
          <button @click="prevWeek" class="h-8.5 w-8.5 flex items-center justify-center rounded-xl border border-slate-200 bg-white text-slate-600 hover:text-emerald-600 hover:border-emerald-300 hover:bg-emerald-50/20 transition-all" title="Tuần trước">
            <ChevronLeft class="h-4.5 w-4.5" />
          </button>
          <button @click="goToday" class="h-8.5 px-3 rounded-xl border border-slate-200 bg-white text-xs font-semibold text-slate-700 hover:text-emerald-600 hover:border-emerald-300 hover:bg-emerald-50/20 transition-all">
            Tuần này
          </button>
          <button @click="nextWeek" class="h-8.5 w-8.5 flex items-center justify-center rounded-xl border border-slate-200 bg-white text-slate-600 hover:text-emerald-600 hover:border-emerald-300 hover:bg-emerald-50/20 transition-all" title="Tuần sau">
            <ChevronRight class="h-4.5 w-4.5" />
          </button>
        </div>
      </div>

      <!-- Date display row (Grid View only) -->
      <div v-if="viewMode === 'grid'" class="flex items-center justify-between border-t border-slate-100 pt-2.5">
        <div class="text-xs font-bold text-slate-700 flex items-center gap-2 bg-slate-50 px-3 py-1.5 rounded-lg border border-slate-100">
          <Calendar class="h-4 w-4 text-emerald-600" />
          <span>Lịch làm việc từ <span class="text-emerald-700 font-extrabold">{{ fmt(formatDateToYMD(currentWeekDays[0])) }}</span> đến <span class="text-emerald-700 font-extrabold">{{ fmt(formatDateToYMD(currentWeekDays[6])) }}</span></span>
        </div>
        <div class="text-[11px] text-slate-400 font-medium">
          Mẹo: Nhấp dấu cộng (+) để thêm lịch nhanh cho nhân viên
        </div>
      </div>
    </div>

    <!-- 1. BẢNG TUẦN (GRID VIEW) -->
    <div v-if="viewMode === 'grid'" class="overflow-x-auto rounded-2xl border border-slate-150 shadow-sm bg-white">
      <table class="min-w-full divide-y divide-slate-150">
        <thead class="bg-slate-50/70 backdrop-blur-sm">
          <tr>
            <th class="px-4 py-4 text-left text-[11px] font-bold text-slate-500 uppercase tracking-wider sticky left-0 bg-slate-50/95 border-r border-slate-200 z-10 w-[240px] shadow-[2px_0_5px_rgba(0,0,0,0.02)]">
              Nhân viên
            </th>
            <th v-for="(day, index) in currentWeekDays" :key="index" class="px-3 py-4 text-center text-xs font-bold border-r border-slate-250 last:border-r-0 min-w-[140px]">
              <div class="text-slate-600 font-semibold">{{ ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật'][index] }}</div>
              <div class="text-[10px] text-slate-400 font-bold mt-0.5">{{ day.getDate() }}/{{ day.getMonth() + 1 }}</div>
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-slate-150 bg-white">
          <tr v-if="filteredEmployeesForGrid.length === 0">
            <td colspan="8" class="px-6 py-12 text-center text-sm text-slate-400 font-medium">
              Không tìm thấy nhân viên phù hợp bộ lọc.
            </td>
          </tr>
          <tr v-for="emp in filteredEmployeesForGrid" :key="emp.id" class="hover:bg-slate-50/40 transition-colors group">
            <!-- Cột tên nhân viên -->
            <td class="px-4 py-3.5 sticky left-0 bg-white group-hover:bg-slate-50/90 z-10 border-r border-slate-200 w-[240px] shadow-[2px_0_5px_rgba(0,0,0,0.02)] transition-colors">
              <div class="flex items-center gap-2.5">
                <div class="h-9 w-9 rounded-2xl bg-gradient-to-br from-emerald-400 to-teal-500 text-white flex items-center justify-center font-extrabold text-xs uppercase shadow-sm shrink-0">
                  {{ emp.fullName.charAt(0) }}
                </div>
                <div class="min-w-0">
                  <div class="font-bold text-slate-800 text-sm truncate" :title="emp.fullName">
                    {{ emp.fullName }}
                  </div>
                  <div class="text-[10px] text-slate-400 font-bold tracking-wide mt-0.5 uppercase">
                    {{ emp.departmentName || 'Chưa gán phòng' }}
                  </div>
                </div>
              </div>
            </td>

            <!-- Các cột ca làm của các thứ -->
            <td v-for="(day, idx) in currentWeekDays" :key="idx" class="px-2.5 py-3.5 border-r border-slate-100 last:border-r-0 text-center">
              <div v-if="findSchedule(emp.id, day)" class="group/cell relative p-2.5 rounded-xl border border-slate-200 bg-white shadow-sm flex flex-col items-center gap-1.5 transition-all hover:border-emerald-400 hover:shadow-md hover:-translate-y-0.5">
                <div class="text-[11px] font-bold text-slate-700 truncate max-w-full">
                  {{ findSchedule(emp.id, day)?.shiftName }}
                </div>
                <span :class="['inline-flex items-center px-2 py-0.5 rounded-full text-[9px] font-extrabold tracking-wider border', getStatusClass(findSchedule(emp.id, day)!.status)]">
                  {{ getStatusLabel(findSchedule(emp.id, day)!.status) }}
                </span>

                <!-- Quick actions on cell hover -->
                <div v-if="auth.isManager" class="absolute inset-0 bg-slate-900/5 rounded-xl opacity-0 group-hover/cell:opacity-100 flex items-center justify-center gap-1.5 transition-all">
                  <button
                    type="button"
                    @click="openEdit(findSchedule(emp.id, day)!)"
                    class="h-6 w-6 rounded bg-white text-emerald-600 border border-slate-200 shadow-md flex items-center justify-center hover:bg-emerald-50 transition-colors"
                    title="Sửa"
                  >
                    <Edit class="h-3.5 w-3.5" />
                  </button>
                  <button
                    type="button"
                    @click="deleteTarget = findSchedule(emp.id, day)!"
                    class="h-6 w-6 rounded bg-white text-red-600 border border-slate-200 shadow-md flex items-center justify-center hover:bg-red-50 transition-colors"
                    title="Xóa"
                  >
                    <Trash2 class="h-3.5 w-3.5" />
                  </button>
                </div>
              </div>

              <!-- Cell empty: Allow quick assigning if user is Manager -->
              <div v-else>
                <button
                  v-if="auth.isManager"
                  type="button"
                  @click="quickAssign(emp.id, day)"
                  class="h-10 w-full rounded-xl border-2 border-dashed border-slate-150 hover:border-emerald-300 hover:bg-emerald-50/20 flex items-center justify-center text-slate-350 hover:text-emerald-600 transition-all"
                  title="Click để phân lịch"
                >
                  <Plus class="h-4 w-4" />
                </button>
                <div v-else class="text-xs text-slate-300 italic py-2">--</div>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- 2. BẢNG DANH SÁCH (LIST VIEW) -->
    <div v-else>
      <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có lịch làm việc nào">
        <template #default="{ row }">
          <td class="px-4 py-3 text-sm font-medium text-slate-900">
            <div class="flex items-center gap-2.5">
              <div class="h-9 w-9 rounded-2xl bg-gradient-to-br from-emerald-400 to-teal-500 text-white flex items-center justify-center font-bold text-xs uppercase">
                {{ (row as WorkSchedule).employeeName?.charAt(0) || 'E' }}
              </div>
              <div>
                <div class="font-bold text-slate-800">{{ (row as WorkSchedule).employeeName }}</div>
              </div>
            </div>
          </td>
          <td class="px-4 py-3 text-sm text-slate-700">
            <div class="flex items-center gap-1.5 text-slate-600 font-semibold">
              <Clock class="h-3.5 w-3.5 text-slate-400" />
              {{ (row as WorkSchedule).shiftName }}
            </div>
          </td>
          <td class="px-4 py-3 text-sm text-slate-500 font-medium">
            <div class="flex items-center gap-1.5">
              <Calendar class="h-3.5 w-3.5 text-slate-400" />
              {{ fmt((row as WorkSchedule).workDate) }}
            </div>
          </td>
          <td class="px-4 py-3 text-sm">
            <span
              :class="['inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold border', getStatusClass((row as WorkSchedule).status)]"
            >
              {{ getStatusLabel((row as WorkSchedule).status) }}
            </span>
          </td>
          <td v-if="auth.isManager" class="px-4 py-3 text-right">
            <div class="flex justify-end gap-1.5">
              <button
                type="button"
                @click="openEdit(row as WorkSchedule)"
                class="inline-flex items-center justify-center p-1 text-slate-500 hover:text-emerald-600 hover:bg-emerald-50 rounded-xl transition-colors"
                title="Chỉnh sửa"
              >
                <Edit class="h-4 w-4" />
              </button>
              <button
                type="button"
                @click="deleteTarget = row as WorkSchedule"
                class="inline-flex items-center justify-center p-1 text-slate-500 hover:text-red-600 hover:bg-red-50 rounded-xl transition-colors"
                title="Xóa"
              >
                <Trash2 class="h-4 w-4" />
              </button>
            </div>
          </td>
        </template>
      </AppTable>
      <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />
    </div>

    <!-- Modal Phân Lịch -->
    <AppModal v-if="showForm" title="Phân lịch làm việc" @close="showForm = false">
      <div class="space-y-4">
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Nhân viên <span class="text-red-500">*</span></label>
          <select v-model="form.employeeId" :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none focus:ring-1 focus:ring-emerald-400', errors.employeeId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn nhân viên --</option>
            <option v-for="e in activeEmployees" :key="e.id" :value="e.id">{{ e.fullName }} (Active)</option>
          </select>
          <p v-if="errors.employeeId" class="text-xs text-red-500">{{ errors.employeeId }}</p>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Ca làm việc <span class="text-red-500">*</span></label>
          <select v-model="form.shiftId" :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none focus:ring-1 focus:ring-emerald-400', errors.shiftId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn ca --</option>
            <option v-for="s in shifts.filter(s => s.isActive)" :key="s.id" :value="s.id">{{ s.name }} ({{ s.startTime }}-{{ s.endTime }})</option>
          </select>
          <p v-if="errors.shiftId" class="text-xs text-red-500">{{ errors.shiftId }}</p>
        </div>
        <div class="grid grid-cols-2 gap-3">
          <AppInput id="ws-start" v-model="form.startDate" label="Từ ngày" type="date" required :error="errors.startDate" />
          <AppInput id="ws-end" v-model="form.endDate" label="Đến ngày" type="date" required :error="errors.endDate" />
        </div>
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="showForm = false">Hủy</AppButton>
        <AppButton :loading="saving" @click="save">Phân lịch</AppButton>
      </template>
    </AppModal>

    <!-- Modal Chỉnh Sửa Lịch Đơn Lẻ -->
    <AppModal v-if="editTarget" title="Chỉnh sửa lịch làm việc" @close="editTarget = null">
      <div class="space-y-4">
        <div class="bg-slate-50 p-3.5 rounded-xl border border-slate-150 mb-2">
          <div class="text-xs text-slate-500 font-semibold uppercase tracking-wider">Nhân viên</div>
          <div class="text-sm font-bold text-slate-800 mt-0.5">{{ editTarget.employeeName }}</div>
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Ngày làm việc <span class="text-red-500">*</span></label>
          <AppInput id="ws-edit-date" v-model="editForm.workDate" type="date" required :error="editErrors.workDate" />
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Ca làm việc <span class="text-red-500">*</span></label>
          <select v-model="editForm.shiftId" :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none focus:ring-1 focus:ring-emerald-400', editErrors.shiftId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn ca --</option>
            <option v-for="s in shifts.filter(s => s.isActive)" :key="s.id" :value="s.id">{{ s.name }} ({{ s.startTime }}-{{ s.endTime }})</option>
          </select>
          <p v-if="editErrors.shiftId" class="text-xs text-red-500">{{ editErrors.shiftId }}</p>
        </div>

        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Trạng thái <span class="text-red-500">*</span></label>
          <select v-model="editForm.status" class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-400">
            <option value="Planned">Planned</option>
            <option value="Completed">Completed</option>
            <option value="Absent">Absent</option>
          </select>
        </div>
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="editTarget = null">Hủy</AppButton>
        <AppButton :loading="editSaving" @click="saveEdit">Cập nhật</AppButton>
      </template>
    </AppModal>

    <!-- Excel Import Modal -->
    <ScheduleImportModal
      :is-open="showImport"
      :employees="employees"
      :shifts="shifts"
      @close="showImport = false"
      @imported="load"
    />

    <!-- Xác nhận xóa -->
    <AppConfirm v-if="deleteTarget" title="Xóa lịch làm việc" message="Bạn chắc chắn muốn xóa lịch này?" confirm-text="Xóa" :danger="true" :loading="deleteLoading" @confirm="confirmDelete" @cancel="deleteTarget = null" />
  </div>
</template>
