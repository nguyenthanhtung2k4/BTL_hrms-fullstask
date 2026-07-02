<script setup lang="ts">
/**
 * TimesheetView — Dual view: Table (Manager/HR) + Calendar Grid (Employee)
 * Calendar: mỗi ô ngày có màu theo trạng thái chấm công
 */
import { ref, computed, onMounted } from 'vue'
import { timesheetService } from '../../../services/timesheet.service'
import { employeeService } from '../../../services/employee.service'
import { departmentService } from '../../../services/department.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import { useI18n } from 'vue-i18n'
import type { Timesheet, AttendanceRecord } from '../../../types/attendance.types'
import type { Employee, Department } from '../../../types/hr.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'
import { apiClient, extractData } from '../../../services/apiClient'

const auth = useAuthStore()
const toast = useToastStore()
const { t } = useI18n()

const WORK_DAY_MINUTES = 480

// ─── State ────────────────────────────────────────────────────────────────────
const timesheets = ref<Timesheet[]>([])
const employees = ref<Employee[]>([])
const departments = ref<Department[]>([])
const attendanceRecords = ref<AttendanceRecord[]>([])

const loading = ref(false)
const calculating = ref(false)
const viewMode = ref<'table' | 'calendar'>('calendar')

const filterMonth = ref(new Date().getMonth() + 1)
const filterYear = ref(new Date().getFullYear())
const filterDept = ref('')
const filterStatus = ref('')
const searchEmployee = ref('')

// ─── Columns ──────────────────────────────────────────────────────────────────
const columns = computed(() => [
  { key: 'employee', label: t('employee.fullName') },
  { key: 'workDays', label: t('timesheet.workedDays') },
  { key: 'totalHours', label: t('timesheet.totalHours') },
  { key: 'paidLeave', label: 'Phép CL' },
  { key: 'unpaidLeave', label: 'Phép KL' },
  { key: 'status', label: t('common.status') },
])

const isManagerOnly = computed(() => auth.hasRole('Manager') && !auth.isAdmin && !auth.isHR)


const months = Array.from({ length: 12 }, (_, i) => ({ value: i + 1, label: `Tháng ${i + 1}` }))
const years = [2024, 2025, 2026, 2027]

// ─── Calendar helpers ─────────────────────────────────────────────────────────
const calendarDays = computed(() => {
  const days: { date: Date; dayNum: number; isCurrentMonth: boolean }[] = []
  const first = new Date(filterYear.value, filterMonth.value - 1, 1)
  const last = new Date(filterYear.value, filterMonth.value, 0)
  const startDow = (first.getDay() + 6) % 7 // Mon=0

  for (let i = 0; i < startDow; i++) {
    const d = new Date(first)
    d.setDate(d.getDate() - (startDow - i))
    days.push({ date: d, dayNum: d.getDate(), isCurrentMonth: false })
  }
  for (let d = 1; d <= last.getDate(); d++) {
    days.push({ date: new Date(filterYear.value, filterMonth.value - 1, d), dayNum: d, isCurrentMonth: true })
  }
  const remaining = 42 - days.length
  for (let i = 1; i <= remaining; i++) {
    days.push({ date: new Date(filterYear.value, filterMonth.value, i), dayNum: i, isCurrentMonth: false })
  }
  return days
})

function toDateStr(d: Date): string {
  const yyyy = d.getFullYear()
  const mm = String(d.getMonth() + 1).padStart(2, '0')
  const dd = String(d.getDate()).padStart(2, '0')
  return `${yyyy}-${mm}-${dd}`
}

function getRecordForDay(date: Date): AttendanceRecord | null {
  const ds = toDateStr(date)
  return attendanceRecords.value.find((r) => r.workDate?.startsWith(ds)) ?? null
}

function getDayClass(date: Date, isCurrentMonth: boolean): string {
  if (!isCurrentMonth) return 'cal-day cal-day--out'
  const dow = date.getDay()
  if (dow === 0 || dow === 6) return 'cal-day cal-day--weekend'
  const rec = getRecordForDay(date)
  if (!rec) return 'cal-day cal-day--empty'
  if (rec.status === 'Late') return 'cal-day cal-day--late'
  if (rec.status === 'HalfDay') return 'cal-day cal-day--half'
  if (rec.workedMinutes >= WORK_DAY_MINUTES) return 'cal-day cal-day--full'
  return 'cal-day cal-day--partial'
}

function getDayTooltip(date: Date): string {
  const rec = getRecordForDay(date)
  if (!rec) return ''
  const inTime = rec.checkInAt ? new Date(rec.checkInAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '—'
  const outTime = rec.checkOutAt ? new Date(rec.checkOutAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '—'
  return `Vào: ${inTime} | Ra: ${outTime}`
}

// ─── Table helpers ────────────────────────────────────────────────────────────
function workDays(t: Timesheet) { return (t.totalWorkedMinutes / WORK_DAY_MINUTES).toFixed(1) }
function totalHours(t: Timesheet) {
  const h = Math.floor(t.totalWorkedMinutes / 60)
  const m = t.totalWorkedMinutes % 60
  return `${h}h ${m}m`
}

// ─── Filters ──────────────────────────────────────────────────────────────────
const filteredTimesheets = computed(() => {
  let result = timesheets.value
  if (filterDept.value) {
    result = result.filter((ts) => employees.value.find((e) => e.id === ts.employeeId)?.departmentId === filterDept.value)
  }
  if (filterStatus.value) result = result.filter((ts) => ts.status === filterStatus.value)
  if (searchEmployee.value) {
    const q = searchEmployee.value.toLowerCase()
    result = result.filter((ts) => ts.employeeName?.toLowerCase().includes(q))
  }
  return result
})

const { currentPage, perPage, paginatedData, total } = usePagination(filteredTimesheets)

// ─── Summary stats (for calendar header) ──────────────────────────────────────
const calStats = computed(() => {
  const myTimesheet = timesheets.value.find((ts) => ts.employeeId === auth.employeeId)
  return {
    worked: myTimesheet ? (myTimesheet.totalWorkedMinutes / WORK_DAY_MINUTES).toFixed(1) : '0',
    paid: myTimesheet?.paidLeaveDays ?? 0,
    unpaid: myTimesheet?.unpaidLeaveDays ?? 0,
    status: myTimesheet?.status ?? '',
  }
})

// ─── Load ─────────────────────────────────────────────────────────────────────
async function load() {
  loading.value = true
  try {
    const params = {
      month: filterMonth.value,
      year: filterYear.value,
      employeeId: (auth.isManager || auth.isPayrollStaff) ? undefined : auth.employeeId ?? undefined,
    }
    let resTimesheets
    if (auth.isManager || auth.isPayrollStaff) {
      const [ts, emps, depts] = await Promise.all([
        timesheetService.getAll(params),
        employeeService.getAll(),
        isManagerOnly.value ? departmentService.getMyDepartments() : departmentService.getAll(), // ← sửa
      ])
      resTimesheets = ts
      employees.value = emps
      departments.value = depts
    } else {
      resTimesheets = await timesheetService.getAll(params)
      employees.value = []
      departments.value = []
    }
    timesheets.value = resTimesheets

    // Load attendance records for calendar view (own records)
    if (auth.employeeId) {
      const res = await apiClient.get<{ data: AttendanceRecord[] }>(
        '/api/v1/attendance/records',
        { params: { employeeId: auth.employeeId, month: filterMonth.value, year: filterYear.value } }
      )
      attendanceRecords.value = extractData(res)
    }
  } catch {
    toast.error(t('toast.loadFailed'))
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

// Switch view: Employee default calendar; Manager/HR/PayrollStaff default table
if (auth.isManager || auth.isPayrollStaff) viewMode.value = 'table'

onMounted(load)
</script>

<template>
  <div>
    <PageHeader :title="t('timesheet.title')"
      :subtitle="(auth.isManager || auth.isPayrollStaff) ? 'Tổng hợp chấm công hàng tháng' : 'Lịch chấm công của tôi'"
      :breadcrumbs="[{ label: t('nav.attendance') }, { label: t('nav.timesheets') }]">
      <template #actions>
        <!-- View toggle (chỉ Manager/HR/PayrollStaff) -->
        <div v-if="auth.isManager || auth.isPayrollStaff" class="ts-view-toggle">
          <button :class="['ts-toggle-btn', viewMode === 'table' ? 'ts-toggle-btn--active' : '']"
            @click="viewMode = 'table'">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M3 10h18M3 6h18M3 14h18M3 18h18" />
            </svg>
          </button>
          <button :class="['ts-toggle-btn', viewMode === 'calendar' ? 'ts-toggle-btn--active' : '']"
            @click="viewMode = 'calendar'">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </button>
        </div>

        <AppButton v-if="auth.isHR" :loading="calculating" variant="secondary" size="sm" @click="calculate">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 11h.01M12 11h.01M15 11h.01M4 19h16a2 2 0 002-2V7a2 2 0 00-2-2H4a2 2 0 00-2 2v10a2 2 0 002 2z" />
          </svg>
          {{ t('timesheet.generate') }}
        </AppButton>
      </template>
    </PageHeader>

    <!-- ── Bộ lọc ─────────────────────────────────────────────────────────── -->
    <div class="ts-filters">
      <div v-if="auth.isManager || auth.isPayrollStaff" class="ts-filter-field">
        <label>{{ t('common.search') }}</label>
        <input v-model="searchEmployee" type="text" :placeholder="t('employee.fullName') + '...'"
          class="ts-filter-input" />
      </div>
      <div v-if="auth.isManager || auth.isPayrollStaff" class="ts-filter-field">
        <label>{{ t('nav.departments') }}</label>
        <select v-model="filterDept" class="ts-filter-input">
          <option value="">{{ t('common.all') }}</option>
          <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
        </select>
      </div>
      <div v-if="auth.isManager || auth.isPayrollStaff" class="ts-filter-field">
        <label>{{ t('common.status') }}</label>
        <select v-model="filterStatus" class="ts-filter-input">
          <option value="">{{ t('common.all') }}</option>
          <option value="Calculated">Đã tính</option>
          <option value="Approved">Đã duyệt</option>
          <option value="Locked">Đã khóa</option>
        </select>
      </div>
      <div class="ts-filter-field">
        <label>{{ t('timesheet.month') }}</label>
        <select v-model="filterMonth" class="ts-filter-input">
          <option v-for="m in months" :key="m.value" :value="m.value">{{ m.label }}</option>
        </select>
      </div>
      <div class="ts-filter-field">
        <label>{{ t('timesheet.year') }}</label>
        <select v-model="filterYear" class="ts-filter-input">
          <option v-for="y in years" :key="y" :value="y">{{ y }}</option>
        </select>
      </div>
      <div class="ts-filter-field ts-filter-field--action">
        <label>&nbsp;</label>
        <AppButton variant="primary" size="sm" :loading="loading" @click="load">Xem kết quả</AppButton>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════════════════════ -->
    <!-- TABLE VIEW                                                              -->
    <!-- ═══════════════════════════════════════════════════════════════════════ -->
    <template v-if="viewMode === 'table'">
      <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="id"
        empty-text="Chưa có bảng công — hãy nhấn Tính bảng công">
        <template #default="{ row }">
          <td class="ts-td">{{ (row as Timesheet).employeeName }}</td>
          <td class="ts-td font-semibold" style="color: var(--color-success);">{{ workDays(row as Timesheet) }}</td>
          <td class="ts-td" style="color: var(--text-secondary);">{{ totalHours(row as Timesheet) }}</td>
          <td class="ts-td">{{ (row as Timesheet).paidLeaveDays > 0 ? (row as Timesheet).paidLeaveDays : '—' }}</td>
          <td class="ts-td">{{ (row as Timesheet).unpaidLeaveDays > 0 ? (row as Timesheet).unpaidLeaveDays : '—' }}</td>
          <td class="ts-td">
            <AppBadge :status="(row as Timesheet).status" />
          </td>
        </template>
      </AppTable>
      <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event"
        @per-page-change="perPage = $event" />
    </template>

    <!-- ═══════════════════════════════════════════════════════════════════════ -->
    <!-- CALENDAR VIEW                                                           -->
    <!-- ═══════════════════════════════════════════════════════════════════════ -->
    <template v-else>
      <!-- Stats bar -->
      <div class="cal-stats">
        <div class="cal-stat">
          <span class="cal-stat__label">Ngày công</span>
          <span class="cal-stat__val" style="color: var(--color-success);">{{ calStats.worked }}</span>
        </div>
        <div class="cal-stat">
          <span class="cal-stat__label">Phép hưởng lương</span>
          <span class="cal-stat__val" style="color: var(--color-info);">{{ calStats.paid }}</span>
        </div>
        <div class="cal-stat">
          <span class="cal-stat__label">Phép không lương</span>
          <span class="cal-stat__val" style="color: var(--color-warning);">{{ calStats.unpaid }}</span>
        </div>
        <div class="cal-stat">
          <span class="cal-stat__label">Trạng thái</span>
          <AppBadge v-if="calStats.status" :status="calStats.status" />
          <span v-else class="text-xs" style="color: var(--text-tertiary);">Chưa có</span>
        </div>
      </div>

      <!-- Legend -->
      <div class="cal-legend">
        <span class="cal-legend-item"><span class="cal-dot cal-dot--full"></span>Đủ công</span>
        <span class="cal-legend-item"><span class="cal-dot cal-dot--late"></span>Đi trễ</span>
        <span class="cal-legend-item"><span class="cal-dot cal-dot--half"></span>Nửa ngày</span>
        <span class="cal-legend-item"><span class="cal-dot cal-dot--partial"></span>Thiếu giờ</span>
        <span class="cal-legend-item"><span class="cal-dot cal-dot--empty"></span>Vắng</span>
        <span class="cal-legend-item"><span class="cal-dot cal-dot--weekend"></span>Cuối tuần</span>
      </div>

      <!-- Calendar grid -->
      <div v-if="loading" class="cal-skeleton">
        <div v-for="n in 42" :key="n" class="cal-skeleton__cell" />
      </div>
      <div v-else class="cal-grid-container">
        <!-- Day headers -->
        <div class="cal-grid-header">
          <div v-for="d in ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN']" :key="d" class="cal-dow">{{ d }}</div>
        </div>
        <!-- Day cells -->
        <div class="cal-grid">
          <div v-for="day in calendarDays" :key="day.date.toISOString()"
            :class="getDayClass(day.date, day.isCurrentMonth)" :title="getDayTooltip(day.date)">
            <span class="cal-day__num">{{ day.dayNum }}</span>
            <template v-if="day.isCurrentMonth && getRecordForDay(day.date)">
              <span class="cal-day__time">
                {{ new Date(getRecordForDay(day.date)!.checkInAt).toLocaleTimeString('vi-VN', {
                  hour: '2-digit', minute:
                '2-digit' }) }}
              </span>
            </template>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
/* ── Filters ────────────────────────────────────────────────────────────────── */
.ts-filters {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  align-items: flex-end;
  padding: 1rem 1.25rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border);
  background: var(--bg-subtle);
  margin-bottom: 1.25rem;
}

.ts-filter-field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  min-width: 9rem;
}

.ts-filter-field label {
  font-size: 0.6875rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-tertiary);
}

.ts-filter-input {
  height: 2.25rem;
  border-radius: var(--radius-sm);
  border: 1px solid var(--border-strong);
  background: var(--bg-surface);
  color: var(--text-primary);
  padding: 0 0.625rem;
  font-size: 0.875rem;
  outline: none;
  transition: border-color var(--transition-fast);
}

.ts-filter-input:focus {
  border-color: var(--color-primary);
}

.ts-filter-field--action {
  justify-content: flex-end;
}

/* ── Table ──────────────────────────────────────────────────────────────────── */
.ts-td {
  padding: 0.75rem 1.25rem;
  font-size: 0.875rem;
  border-bottom: 1px solid var(--border);
  color: var(--text-primary);
}

/* ── View toggle ────────────────────────────────────────────────────────────── */
.ts-view-toggle {
  display: flex;
  border: 1px solid var(--border-strong);
  border-radius: var(--radius-sm);
  overflow: hidden;
}

.ts-toggle-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2.25rem;
  height: 2.25rem;
  background: var(--bg-surface);
  color: var(--text-secondary);
  border: none;
  cursor: pointer;
  transition: background var(--transition-fast), color var(--transition-fast);
}

.ts-toggle-btn--active {
  background: var(--color-primary);
  color: white;
}

/* ── Calendar Stats ─────────────────────────────────────────────────────────── */
.cal-stats {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  padding: 0.875rem 1.25rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border);
  background: var(--bg-surface);
  margin-bottom: 1rem;
}

.cal-stat {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  min-width: 6rem;
}

.cal-stat__label {
  font-size: 0.6875rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-tertiary);
}

.cal-stat__val {
  font-size: 1.375rem;
  font-weight: 700;
  color: var(--text-primary);
}

/* ── Legend ─────────────────────────────────────────────────────────────────── */
.cal-legend {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem 1.25rem;
  margin-bottom: 0.75rem;
  font-size: 0.75rem;
  color: var(--text-secondary);
}

.cal-legend-item {
  display: flex;
  align-items: center;
  gap: 0.375rem;
}

.cal-dot {
  width: 0.75rem;
  height: 0.75rem;
  border-radius: 50%;
}

.cal-dot--full {
  background: var(--color-success);
}

.cal-dot--late {
  background: var(--color-warning);
}

.cal-dot--half {
  background: var(--color-info);
}

.cal-dot--partial {
  background: hsl(270, 60%, 60%);
}

.cal-dot--empty {
  background: var(--color-danger);
}

.cal-dot--weekend {
  background: var(--bg-muted);
  border: 1px solid var(--border-strong);
}

/* ── Calendar Grid ──────────────────────────────────────────────────────────── */
.cal-grid-container {
  border-radius: var(--radius-lg);
  border: 1px solid var(--border);
  background: var(--bg-surface);
  overflow: hidden;
}

.cal-grid-header,
.cal-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
}

.cal-grid-header {
  border-bottom: 1px solid var(--border);
}

.cal-dow {
  padding: 0.5rem;
  text-align: center;
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-tertiary);
  background: var(--bg-subtle);
}

.cal-day {
  position: relative;
  min-height: 4rem;
  padding: 0.375rem 0.5rem;
  border-right: 1px solid var(--border);
  border-bottom: 1px solid var(--border);
  cursor: default;
  transition: background-color var(--transition-fast);
}

.cal-day:nth-child(7n) {
  border-right: none;
}

.cal-day__num {
  display: block;
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 0.25rem;
}

.cal-day__time {
  display: block;
  font-size: 0.625rem;
  font-family: 'JetBrains Mono', monospace;
  color: var(--text-tertiary);
}

/* Variants */
.cal-day--out {
  background: var(--bg-page);
}

.cal-day--out .cal-day__num {
  color: var(--text-tertiary);
  opacity: 0.4;
}

.cal-day--weekend {
  background: var(--bg-subtle);
}

.cal-day--weekend .cal-day__num {
  color: var(--text-tertiary);
}

.cal-day--empty:hover {
  background: color-mix(in srgb, var(--color-danger) 5%, transparent);
}

.cal-day--full {
  background: color-mix(in srgb, var(--color-success) 8%, var(--bg-surface));
  border-left: 3px solid var(--color-success);
}

.cal-day--late {
  background: color-mix(in srgb, var(--color-warning) 10%, var(--bg-surface));
  border-left: 3px solid var(--color-warning);
}

.cal-day--half {
  background: color-mix(in srgb, var(--color-info) 8%, var(--bg-surface));
  border-left: 3px solid var(--color-info);
}

.cal-day--partial {
  background: color-mix(in srgb, hsl(270, 60%, 60%) 8%, var(--bg-surface));
  border-left: 3px solid hsl(270, 60%, 60%);
}

/* Skeleton */
.cal-skeleton {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.cal-skeleton__cell {
  height: 4rem;
  border-right: 1px solid var(--border);
  border-bottom: 1px solid var(--border);
  background: var(--bg-muted);
  animation: pulse 1.5s ease-in-out infinite;
}

@keyframes pulse {

  0%,
  100% {
    opacity: 1
  }

  50% {
    opacity: .5
  }
}
</style>
