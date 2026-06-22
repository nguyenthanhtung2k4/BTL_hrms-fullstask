<script setup lang="ts">
/**
 * DashboardView — Premium, Unified design, Responsive, supports Dark Mode & Multi-role layout.
 */
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { useToastStore } from '../../stores/toast'
import { useDashboard } from '../../composables/useDashboard'
import { leaveService } from '../../services/leave.service'
import { payrollPeriodService } from '../../services/payrollPeriod.service'
import { useTheme } from '../../composables/useTheme'
import { useI18n } from 'vue-i18n'
import StatCard from '../../components/layout/StatCard.vue'
import AppBadge from '../../components/ui/AppBadge.vue'
import AppButton from '../../components/ui/AppButton.vue'

// Import Lucide Icons
import {
  Users,
  CheckCircle,
  Calendar,
  AlertTriangle,
  TrendingUp,
  UserPlus,
  FileText,
  Activity,
  Clock,
  CreditCard,
  ArrowRight,
  RefreshCw,
  ShieldCheck,
  CheckSquare
} from '@lucide/vue'

// Import Chart.js components
import { Bar, Doughnut, Line } from 'vue-chartjs'
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  ArcElement,
  Filler
} from 'chart.js'

ChartJS.register(
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  ArcElement,
  Filler
)

const auth = useAuthStore()
const toast = useToastStore()
const router = useRouter()
const { isDark } = useTheme()
const { t } = useI18n()

const {
  loading,
  employees,
  pendingLeaves,
  allPayslips,
  myPayslips,
  myLeaves,
  newHires,
  expiringContracts,
  todayAttendance,
  attendanceHistory,
  weeklyAttendanceHistory,
  monthlyAttendanceHistory,
  payrollTrend,
  statusDist,
  deptDist,
  periodsNeedAction,
  userAccounts,
  load
} = useDashboard()

const attendanceInterval = ref<'day' | 'week' | 'month'>('day')
const actionLoading = ref<Record<string, boolean>>({})

onMounted(load)

const greeting = computed(() => {
  const h = new Date().getHours()
  if (h < 12) return t('dashboard.greeting_morning')
  if (h < 18) return t('dashboard.greeting_afternoon')
  return t('dashboard.greeting_evening')
})

// Formatting helpers
function getEmployeeName(employeeId: string | null) {
  if (!employeeId) return 'System Admin'
  const emp = employees.value.find(e => e.id === employeeId)
  return emp ? emp.fullName : 'Account Linked'
}

function fmtMoney(n: number) {
  return n.toLocaleString('vi-VN') + ' ₫'
}

function fmtDate(d: string) {
  if (!d) return '—'
  return new Date(d).toLocaleDateString('vi-VN')
}

// Inline Leave Approval/Rejection for managers/admins
async function handleLeave(id: string, status: 'Approved' | 'Rejected') {
  actionLoading.value[id] = true
  try {
    if (status === 'Approved') {
      await leaveService.approve(id)
      toast.success('Đã phê duyệt đơn nghỉ phép')
    } else {
      await leaveService.reject(id)
      toast.success('Đã từ chối đơn nghỉ phép')
    }
    await load()
  } catch {
    toast.error('Lỗi khi cập nhật trạng thái đơn nghỉ phép')
  } finally {
    actionLoading.value[id] = false
  }
}

// Inline Period calculation trigger
async function handleCalculatePeriod(periodId: string) {
  actionLoading.value[periodId] = true
  try {
    await payrollPeriodService.calculate(periodId)
    toast.success('Đã tính toán bảng lương thành công!')
    await load()
  } catch {
    toast.error('Lỗi khi tính toán lương')
  } finally {
    actionLoading.value[periodId] = false
  }
}

// Theme-aware Chart Styling
const textPrimaryColor = computed(() => isDark() ? '#f8fafc' : '#0f172a')
const borderLineColor = computed(() => isDark() ? 'rgba(255, 255, 255, 0.08)' : 'rgba(0, 0, 0, 0.04)')

const chartOptions = computed<any>(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'bottom',
      labels: {
        boxWidth: 8,
        boxHeight: 8,
        color: textPrimaryColor.value,
        font: { size: 11, family: 'Inter, system-ui' },
        padding: 16
      }
    }
  },
  scales: {
    x: {
      grid: { display: false },
      ticks: { color: textPrimaryColor.value, font: { size: 10 } }
    },
    y: {
      grid: { color: borderLineColor.value },
      ticks: { precision: 0, color: textPrimaryColor.value, font: { size: 10 } }
    }
  }
}))

const doughnutOptions = computed<any>(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'right',
      labels: {
        boxWidth: 10,
        color: textPrimaryColor.value,
        font: { size: 11, family: 'Inter, system-ui' },
        padding: 12
      }
    }
  }
}))

// Chart Datas
const statusChartData = computed<any>(() => ({
  labels: ['Active', 'Inactive', 'On Leave', 'Resigned'],
  datasets: [
    {
      data: [
        statusDist.value.Active ?? 0,
        statusDist.value.Inactive ?? 0,
        statusDist.value.OnLeave ?? 0,
        statusDist.value.Resigned ?? 0
      ],
      backgroundColor: ['#10b981', '#94a3b8', '#fbbf24', '#f87171'],
      borderWidth: 0,
      hoverOffset: 6
    }
  ]
}))

const deptChartData = computed<any>(() => ({
  labels: deptDist.value.map(d => d.name),
  datasets: [
    {
      label: t('employee.title'),
      data: deptDist.value.map(d => d.count),
      backgroundColor: 'rgba(59, 130, 246, 0.85)',
      hoverBackgroundColor: 'rgba(59, 130, 246, 1)',
      borderRadius: 6,
      barThickness: 16
    }
  ]
}))

const payrollChartData = computed<any>(() => ({
  labels: payrollTrend.value.map(p => p.name),
  datasets: [
    {
      label: 'Gross',
      data: payrollTrend.value.map(p => p.gross),
      borderColor: '#f59e0b',
      backgroundColor: 'transparent',
      borderWidth: 2,
      pointBackgroundColor: '#f59e0b',
      tension: 0.3
    },
    {
      label: 'Net',
      data: payrollTrend.value.map(p => p.net),
      borderColor: '#10b981',
      backgroundColor: 'rgba(16, 185, 129, 0.06)',
      borderWidth: 3,
      fill: true,
      pointBackgroundColor: '#10b981',
      tension: 0.3
    }
  ]
}))

const attendanceHistoryChartData = computed<any>(() => {
  let labels: string[] = []
  let data: number[] = []

  if (attendanceInterval.value === 'day') {
    labels = attendanceHistory.value.map(a => a.date)
    data = attendanceHistory.value.map(a => a.count)
  } else if (attendanceInterval.value === 'week') {
    labels = weeklyAttendanceHistory.value.map(a => a.name)
    data = weeklyAttendanceHistory.value.map(a => a.count)
  } else if (attendanceInterval.value === 'month') {
    labels = monthlyAttendanceHistory.value.map(a => a.name)
    data = monthlyAttendanceHistory.value.map(a => a.count)
  }

  return {
    labels,
    datasets: [
      {
        label: t('nav.attendance'),
        data,
        backgroundColor: 'rgba(16, 185, 129, 0.8)',
        hoverBackgroundColor: 'rgba(16, 185, 129, 1)',
        borderRadius: 4,
        barThickness: attendanceInterval.value === 'day' ? 14 : (attendanceInterval.value === 'week' ? 20 : 28)
      }
    ]
  }
})

const myPayslipsChartData = computed<any>(() => ({
  labels: myPayslips.value.slice(-6).map(p => p.fullName.split(' ')[0]),
  datasets: [
    {
      label: 'Net',
      data: myPayslips.value.slice(-6).map(p => p.netSalary),
      backgroundColor: 'rgba(16, 185, 129, 0.85)',
      borderRadius: 6,
      barThickness: 20
    }
  ]
}))
</script>

<template>
  <div class="dash-container">
    <!-- Header panel with greeting and refresh -->
    <div class="dash-greeting-card">
      <div class="dash-greeting-card__info">
        <h1 class="dash-greeting-card__title">{{ greeting }}, {{ auth.displayName }} 👋</h1>
        <p class="dash-greeting-card__subtitle">
          {{ new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) }}
        </p>
      </div>
      <div>
        <button class="dash-refresh-btn" @click="load">
          <RefreshCw class="h-4 w-4" :class="{ 'animate-spin': loading }" />
          {{ t('dashboard.refresh') }}
        </button>
      </div>
    </div>

    <!-- Loading Skeleton overlay when full reloading -->
    <div v-if="loading && employees.length === 0" class="dash-skeleton-grid">
      <div v-for="n in 4" :key="n" class="dash-skeleton-card" />
    </div>

    <div v-else class="space-y-6">
      <!-- 👑 ROLE: ADMIN / HR DASHBOARD -->
      <div v-if="auth.isHR || auth.isAdmin" class="space-y-6">
        <!-- Row 1: Stat Cards -->
        <div class="dash-stats-grid">
          <StatCard :title="t('dashboard.totalEmployees')" :value="employees.length" :subtitle="t('employee.list')" color="emerald" :loading="loading">
            <template #icon><Users class="h-5 w-5" /></template>
          </StatCard>
          <StatCard :title="t('dashboard.presentToday')" :value="`${todayAttendance.checkedIn} NV`" :subtitle="`Tỷ lệ: ${todayAttendance.rate}%`" color="blue" :loading="loading">
            <template #icon><CheckCircle class="h-5 w-5" /></template>
          </StatCard>
          <StatCard title="Nhân viên mới" :value="newHires.length" subtitle="Gia nhập tháng này" color="violet" :loading="loading">
            <template #icon><UserPlus class="h-5 w-5" /></template>
          </StatCard>
          <StatCard title="Hợp đồng hết hạn" :value="expiringContracts.length" subtitle="Trong vòng 30 ngày" color="red" :loading="loading">
            <template #icon><AlertTriangle class="h-5 w-5" /></template>
          </StatCard>
        </div>

        <!-- Row 2: Analysis Charts -->
        <div class="dash-chart-row-1">
          <!-- Human Resource Status -->
          <div class="dash-chart-card">
            <h3 class="dash-chart-card__title">
              <Activity class="h-4 w-4 text-success" /> Trạng thái nhân sự
            </h3>
            <div class="dash-chart-card__canvas-container">
              <Doughnut :data="statusChartData" :options="doughnutOptions" />
            </div>
          </div>

          <!-- Employees by Department -->
          <div class="dash-chart-card dash-chart-card--span-2">
            <h3 class="dash-chart-card__title">
              <Users class="h-4 w-4 text-info" /> Nhân viên theo phòng ban
            </h3>
            <div class="dash-chart-card__canvas-container">
              <Bar :data="deptChartData" :options="chartOptions" />
            </div>
          </div>
        </div>

        <!-- Row 3: Payroll Trend & Attendance History -->
        <div class="dash-chart-row-2">
          <!-- Payroll Trend -->
          <div class="dash-chart-card">
            <h3 class="dash-chart-card__title">
              <TrendingUp class="h-4 w-4 text-warning" /> Biến động quỹ lương (Net & Gross)
            </h3>
            <div class="dash-chart-card__canvas-container">
              <Line v-if="payrollTrend.length > 0" :data="payrollChartData" :options="chartOptions" />
              <div v-else class="dash-chart-card__empty">
                Chưa có dữ liệu phiếu lương nào để phân tích.
              </div>
            </div>
          </div>

          <!-- Attendance Trends -->
          <div class="dash-chart-card">
            <div class="dash-chart-card__header-row">
              <h3 class="dash-chart-card__title">
                <Clock class="h-4 w-4 text-success" /> Tần suất chấm công
              </h3>
              <div class="dash-interval-toggle">
                <button :class="{ active: attendanceInterval === 'day' }" @click="attendanceInterval = 'day'">Ngày</button>
                <button :class="{ active: attendanceInterval === 'week' }" @click="attendanceInterval = 'week'">Tuần</button>
                <button :class="{ active: attendanceInterval === 'month' }" @click="attendanceInterval = 'month'">Tháng</button>
              </div>
            </div>
            <div class="dash-chart-card__canvas-container">
              <Bar :data="attendanceHistoryChartData" :options="chartOptions" />
            </div>
          </div>
        </div>

        <!-- Row 4: Actionable Lists -->
        <div class="dash-grid-two-col">
          <!-- Leaves waiting for approval -->
          <div class="dash-list-card">
            <div class="dash-list-card__header">
              <h3 class="dash-list-card__title">
                <Calendar class="h-4 w-4 text-success" /> {{ t('dashboard.pendingLeaves') }}
              </h3>
              <span class="dash-list-card__link" @click="router.push('/attendance/leaves')">
                Xem tất cả <ArrowRight class="h-3 w-3" />
              </span>
            </div>
            <div v-if="pendingLeaves.length === 0" class="dash-list-card__empty">
              {{ t('dashboard.noPendingLeaves') }}
            </div>
            <div v-else class="dash-list-card__items">
              <div v-for="l in pendingLeaves.slice(0, 5)" :key="l.id" class="dash-list-card__item">
                <div class="dash-list-card__item-left">
                  <p class="dash-item-title">{{ l.employeeName }}</p>
                  <p class="dash-item-desc">{{ l.leaveTypeName }} ({{ l.totalDays }} ngày: {{ fmtDate(l.fromDate) }} - {{ fmtDate(l.toDate) }})</p>
                </div>
                <div class="dash-list-card__actions">
                  <AppButton size="sm" variant="success" :loading="actionLoading[l.id]" @click="handleLeave(l.id, 'Approved')">Duyệt</AppButton>
                  <AppButton size="sm" variant="danger" :loading="actionLoading[l.id]" @click="handleLeave(l.id, 'Rejected')">Từ chối</AppButton>
                </div>
              </div>
            </div>
          </div>

          <!-- Expiring Contracts -->
          <div class="dash-list-card">
            <div class="dash-list-card__header">
              <h3 class="dash-list-card__title">
                <FileText class="h-4 w-4 text-danger" /> Hợp đồng sắp hết hạn (30 ngày)
              </h3>
              <span class="dash-list-card__link" @click="router.push('/hr/contracts')">
                Xem tất cả <ArrowRight class="h-3 w-3" />
              </span>
            </div>
            <div v-if="expiringContracts.length === 0" class="dash-list-card__empty">
              Không có hợp đồng nào sắp hết hạn.
            </div>
            <div v-else class="dash-list-card__items">
              <div v-for="c in expiringContracts.slice(0, 5)" :key="c.id" class="dash-list-card__item">
                <div class="dash-list-card__item-left">
                  <p class="dash-item-title">{{ c.employeeName }}</p>
                  <p class="dash-item-desc">Mã: {{ c.contractNumber }} · Loại: {{ c.contractType }}</p>
                </div>
                <div class="dash-list-card__badge-col">
                  <p class="dash-item-val font-semibold text-danger">{{ fmtDate(c.endDate || '') }}</p>
                  <p class="dash-item-label">Hết hạn</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Row 5: System Login sessions (ADMIN only) -->
        <div v-if="auth.isAdmin" class="dash-table-card">
          <div class="dash-table-card__header">
            <h3 class="dash-table-card__title">
              <ShieldCheck class="h-4 w-4 text-success" /> {{ t('dashboard.latestLogins') }}
            </h3>
            <div class="dash-table-card__stats">
              <span>Đang hoạt động: <strong style="color: var(--color-success);">{{ userAccounts.filter(u => u.isActive).length }}</strong></span>
              <span>Khóa: <strong style="color: var(--color-danger);">{{ userAccounts.filter(u => !u.isActive).length }}</strong></span>
            </div>
          </div>
          <div v-if="userAccounts.length === 0" class="dash-table-card__empty">
            Không có dữ liệu tài khoản hệ thống.
          </div>
          <div v-else class="dash-table-container">
            <table class="dash-table">
              <thead>
                <tr>
                  <th>Nhân viên</th>
                  <th>Email</th>
                  <th>{{ t('dashboard.role') }}</th>
                  <th>Trạng thái</th>
                  <th class="text-right">{{ t('dashboard.lastLogin') }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="u in [...userAccounts].sort((a,b) => new Date(b.lastLoginAt || 0).getTime() - new Date(a.lastLoginAt || 0).getTime()).slice(0, 5)" :key="u.id">
                  <td class="font-semibold">{{ getEmployeeName(u.employeeId) }}</td>
                  <td class="font-mono text-xs" style="color: var(--text-secondary);">{{ u.email }}</td>
                  <td>
                    <div class="dash-badge-row">
                      <span v-for="role in u.roles" :key="role" class="dash-small-badge">{{ role }}</span>
                    </div>
                  </td>
                  <td>
                    <span :class="['dash-status-dot', u.isActive ? 'dash-status-dot--active' : 'dash-status-dot--locked']">
                      {{ u.isActive ? 'Hoạt động' : 'Bị khóa' }}
                    </span>
                  </td>
                  <td class="text-right" style="color: var(--text-secondary);">
                    {{ u.lastLoginAt ? new Date(u.lastLoginAt).toLocaleString('vi-VN') : 'Chưa từng đăng nhập' }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- 👔 ROLE: MANAGER DASHBOARD -->
      <div v-else-if="auth.isManager" class="space-y-6">
        <div class="dash-stats-grid">
          <StatCard :title="t('dashboard.managerTeam')" :value="employees.length" subtitle="Trực thuộc quyền quản lý" color="emerald" :loading="loading">
            <template #icon><Users class="h-5 w-5" /></template>
          </StatCard>
          <StatCard :title="t('dashboard.onLeaveTeam')" :value="employees.filter(e => e.status === 'OnLeave').length" subtitle="Nghỉ phép hôm nay" color="amber" :loading="loading">
            <template #icon><Calendar class="h-5 w-5" /></template>
          </StatCard>
          <StatCard :title="t('dashboard.pendingLeaves')" :value="pendingLeaves.length" subtitle="Cần xem xét" color="red" :loading="loading">
            <template #icon><Clock class="h-5 w-5" /></template>
          </StatCard>
        </div>

        <div class="dash-chart-row-1">
          <!-- Team attendance history -->
          <div class="dash-chart-card dash-chart-card--span-2">
            <h3 class="dash-chart-card__title">
              <Clock class="h-4 w-4 text-success" /> {{ t('dashboard.attendanceTrendDept') }}
            </h3>
            <div class="dash-chart-card__canvas-container">
              <Bar :data="attendanceHistoryChartData" :options="chartOptions" />
            </div>
          </div>

          <!-- Team quick leave approval -->
          <div class="dash-list-card">
            <h3 class="dash-list-card__title mb-4">
              <CheckSquare class="h-4 w-4 text-success" /> {{ t('dashboard.leaveApprovalQuick') }}
            </h3>
            <div v-if="pendingLeaves.length === 0" class="dash-list-card__empty">
              {{ t('dashboard.noPendingLeaves') }}
            </div>
            <div v-else class="dash-action-list">
              <div v-for="l in pendingLeaves" :key="l.id" class="dash-action-item">
                <div>
                  <p class="font-semibold text-sm">{{ l.employeeName }}</p>
                  <p class="text-xs" style="color: var(--text-secondary);">{{ l.leaveTypeName }} · {{ l.totalDays }} ngày</p>
                  <p class="text-xs italic mt-1" style="color: var(--text-tertiary);">{{ t('dashboard.reason') }}: "{{ l.reason }}"</p>
                </div>
                <div class="dash-action-item__btns">
                  <AppButton size="sm" variant="success" :loading="actionLoading[l.id]" @click="handleLeave(l.id, 'Approved')">Duyệt</AppButton>
                  <AppButton size="sm" variant="danger" :loading="actionLoading[l.id]" @click="handleLeave(l.id, 'Rejected')">Từ chối</AppButton>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 💰 ROLE: PAYROLL STAFF DASHBOARD -->
      <div v-else-if="auth.isPayrollStaff" class="space-y-6">
        <div class="dash-stats-grid">
          <StatCard :title="t('dashboard.openPeriods')" :value="periodsNeedAction.length" subtitle="Cần xử lý" color="blue" :loading="loading">
            <template #icon><Calendar class="h-5 w-5" /></template>
          </StatCard>
          <StatCard :title="t('dashboard.totalPayslips')" :value="allPayslips.length" subtitle="Tất cả thời kỳ" color="emerald" :loading="loading">
            <template #icon><FileText class="h-5 w-5" /></template>
          </StatCard>
          <StatCard :title="t('dashboard.lastPeriodCost')" :value="allPayslips.slice(-1)[0] ? fmtMoney(allPayslips.slice(-1)[0].netSalary) : '0 ₫'" subtitle="Kỳ lương gần nhất" color="violet" :loading="loading">
            <template #icon><CreditCard class="h-5 w-5" /></template>
          </StatCard>
        </div>

        <div class="dash-chart-row-1">
          <!-- Payroll Expenses -->
          <div class="dash-chart-card dash-chart-card--span-2">
            <h3 class="dash-chart-card__title">
              <TrendingUp class="h-4 w-4 text-success" /> Biến động chi lương (Net & Gross)
            </h3>
            <div class="dash-chart-card__canvas-container">
              <Line v-if="payrollTrend.length > 0" :data="payrollChartData" :options="chartOptions" />
              <div v-else class="dash-chart-card__empty">
                Chưa có dữ liệu phiếu lương.
              </div>
            </div>
          </div>

          <!-- Open periods action -->
          <div class="dash-list-card">
            <h3 class="dash-list-card__title mb-4">
              <Calendar class="h-4 w-4 text-info" /> {{ t('dashboard.payrollPeriodsAction') }}
            </h3>
            <div v-if="periodsNeedAction.length === 0" class="dash-list-card__empty">
              {{ t('dashboard.payrollPeriodsAllClosed') }}
            </div>
            <div v-else class="dash-action-list">
              <div v-for="p in periodsNeedAction" :key="p.id" class="dash-action-item">
                <div class="w-full">
                  <div class="flex items-center justify-between">
                    <span class="font-semibold text-sm">{{ p.name }}</span>
                    <AppBadge :status="p.status" />
                  </div>
                  <p class="text-xs mt-1" style="color: var(--text-secondary);">Hạn: {{ fmtDate(p.fromDate) }} - {{ fmtDate(p.toDate) }}</p>
                  <div class="flex justify-end mt-3">
                    <AppButton v-if="p.status === 'Draft'" size="sm" variant="primary" :loading="actionLoading[p.id]" @click="handleCalculatePeriod(p.id)">
                      {{ t('dashboard.payrollCalculateBtn') }}
                    </AppButton>
                    <AppButton v-else-if="p.status === 'Calculated'" size="sm" variant="success" @click="router.push(`/payroll/periods`)">
                      {{ t('dashboard.viewAndClosePeriod') }}
                    </AppButton>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 👤 ROLE: EMPLOYEE DASHBOARD -->
      <div v-else-if="auth.isEmployee" class="space-y-6">
        <div class="dash-grid-three-col">
          <!-- Cổng thông tin & Thao tác nhanh -->
          <div class="dash-portal-card">
            <div>
              <h3 class="dash-portal-card__title">{{ t('dashboard.personalPortal') }}</h3>
              <p class="dash-portal-card__desc">{{ t('dashboard.personalPortalSub') }}</p>
            </div>
            <div class="dash-portal-card__actions">
              <button class="dash-portal-btn dash-portal-btn--primary" @click="router.push('/attendance/checkin')">
                🕑 {{ t('dashboard.quickCheckin') }}
              </button>
              <button class="dash-portal-btn dash-portal-btn--outline" @click="router.push('/attendance/leaves')">
                🏖️ {{ t('dashboard.registerLeave') }}
              </button>
            </div>
          </div>

          <!-- Personal Net Salary Chart -->
          <div class="dash-chart-card">
            <h3 class="dash-chart-card__title">
              <TrendingUp class="h-4 w-4 text-success" /> {{ t('dashboard.netSalaryTrend6') }}
            </h3>
            <div class="dash-chart-card__canvas-container" style="height: 11rem;">
              <Bar v-if="myPayslips.length > 0" :data="myPayslipsChartData" :options="chartOptions" />
              <div v-else class="dash-chart-card__empty">
                Chưa có dữ liệu phiếu lương.
              </div>
            </div>
            <div class="dash-card-footer">
              <span class="dash-footer-link" @click="router.push('/payroll/my-payslip')">
                {{ t('dashboard.viewPayslipsDetail') }} →
              </span>
            </div>
          </div>

          <!-- Leaves overview -->
          <div class="dash-list-card">
            <h3 class="dash-list-card__title">
              <Calendar class="h-4 w-4 text-success" /> {{ t('dashboard.myLeavesRecent') }}
            </h3>
            <div v-if="myLeaves.length === 0" class="dash-list-card__empty">
              {{ t('dashboard.noLeavesRegistered') }}
            </div>
            <div v-else class="dash-list-card__items">
              <div v-for="l in myLeaves.slice(-3).reverse()" :key="l.id" class="dash-list-card__item pb-2 border-b last:border-0 last:pb-0">
                <div>
                  <p class="dash-item-title">{{ l.leaveTypeName }}</p>
                  <p class="dash-item-desc">{{ l.totalDays }} ngày ({{ fmtDate(l.fromDate) }})</p>
                </div>
                <AppBadge :status="l.status" />
              </div>
            </div>
            <div class="dash-card-footer">
              <span class="dash-footer-link" @click="router.push('/attendance/leaves')">
                {{ t('dashboard.leavesManagement') }} →
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- 🏥 System Health Connection Status Bar (Visible to Admin/HR/PayrollStaff) -->
      <div v-if="auth.isHR || auth.isAdmin || auth.isPayrollStaff" class="dash-health-card">
        <h3 class="dash-health-card__title">
          <Activity class="h-4 w-4 text-success" /> {{ t('dashboard.systemHealth') }}
        </h3>
        <div class="dash-health-grid">
          <div class="dash-health-item">
            <span class="dash-health-item__name">HR Core API</span>
            <span class="dash-health-item__status">
              <span class="dash-pulse-dot"></span> {{ t('dashboard.healthActive') }}
            </span>
          </div>
          <div class="dash-health-item">
            <span class="dash-health-item__name">Attendance API</span>
            <span class="dash-health-item__status">
              <span class="dash-pulse-dot"></span> {{ t('dashboard.healthActive') }}
            </span>
          </div>
          <div class="dash-health-item">
            <span class="dash-health-item__name">Payroll API</span>
            <span class="dash-health-item__status">
              <span class="dash-pulse-dot"></span> {{ t('dashboard.healthActive') }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dash-container {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

/* Greeting card styling */
.dash-greeting-card {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding: 1.5rem;
  border-radius: var(--radius-xl);
  border: 1px solid var(--border);
  background-color: var(--bg-surface);
  box-shadow: var(--shadow-sm);
  transition: border-color var(--transition-base), background-color var(--transition-base);
}
@media (min-width: 640px) {
  .dash-greeting-card {
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
  }
}
.dash-greeting-card__title {
  font-size: 1.375rem;
  font-weight: 700;
  letter-spacing: -0.02em;
  color: var(--text-primary);
  margin: 0;
}
.dash-greeting-card__subtitle {
  font-size: 0.8125rem;
  font-weight: 500;
  color: var(--text-secondary);
  margin: 0.25rem 0 0;
}
.dash-refresh-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  font-size: 0.8125rem;
  font-weight: 600;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-strong);
  background-color: var(--bg-subtle);
  color: var(--text-secondary);
  cursor: pointer;
  transition: all var(--transition-fast);
}
.dash-refresh-btn:hover {
  background-color: var(--border);
  color: var(--text-primary);
}

/* Stat Grid */
.dash-stats-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1.25rem;
}
@media (min-width: 640px) {
  .dash-stats-grid { grid-template-columns: repeat(2, 1fr); }
}
@media (min-width: 1024px) {
  .dash-stats-grid { grid-template-columns: repeat(4, 1fr); }
}

/* Skeletons */
.dash-skeleton-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(14rem, 1fr));
  gap: 1.25rem;
}
.dash-skeleton-card {
  height: 6.5rem;
  border-radius: var(--radius-lg);
  background-color: var(--bg-muted);
  animation: pulse 1.5s ease-in-out infinite;
}

/* Chart Cards and grids */
.dash-chart-row-1 {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1.5rem;
}
@media (min-width: 1024px) {
  .dash-chart-row-1 { grid-template-columns: repeat(3, 1fr); }
}
.dash-chart-card--span-2 {
  grid-column: span 1;
}
@media (min-width: 1024px) {
  .dash-chart-card--span-2 { grid-column: span 2; }
}

.dash-chart-row-2 {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1.5rem;
}
@media (min-width: 1024px) {
  .dash-chart-row-2 { grid-template-columns: repeat(2, 1fr); }
}

.dash-chart-card {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 1.25rem;
  border-radius: var(--radius-xl);
  border: 1px solid var(--border);
  background-color: var(--bg-surface);
  box-shadow: var(--shadow-sm);
}
.dash-chart-card__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0 0 1rem;
}
.dash-chart-card__header-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1rem;
}
.dash-chart-card__canvas-container {
  position: relative;
  height: 15rem;
  width: 100%;
}
.dash-chart-card__empty {
  display: grid;
  place-items: center;
  height: 100%;
  color: var(--text-tertiary);
  font-size: 0.8125rem;
  text-align: center;
}

.dash-interval-toggle {
  display: flex;
  background-color: var(--bg-subtle);
  border: 1px solid var(--border);
  padding: 2px;
  border-radius: var(--radius-sm);
}
.dash-interval-toggle button {
  padding: 0.25rem 0.5rem;
  font-size: 0.6875rem;
  font-weight: 600;
  border-radius: 4px;
  border: none;
  background: transparent;
  color: var(--text-secondary);
  cursor: pointer;
  transition: all var(--transition-fast);
}
.dash-interval-toggle button.active {
  background-color: var(--bg-surface);
  color: var(--text-primary);
  box-shadow: var(--shadow-sm);
}

/* Grids for actionable cards */
.dash-grid-two-col {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1.5rem;
}
@media (min-width: 1024px) {
  .dash-grid-two-col { grid-template-columns: repeat(2, 1fr); }
}

.dash-grid-three-col {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1.5rem;
}
@media (min-width: 1024px) {
  .dash-grid-three-col { grid-template-columns: repeat(3, 1fr); }
}

/* List card styles */
.dash-list-card {
  padding: 1.25rem;
  border-radius: var(--radius-xl);
  border: 1px solid var(--border);
  background-color: var(--bg-surface);
  box-shadow: var(--shadow-sm);
  display: flex;
  flex-direction: column;
}
.dash-list-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1rem;
}
.dash-list-card__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}
.dash-list-card__link {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--color-primary-text);
  cursor: pointer;
}
.dash-list-card__link:hover {
  text-decoration: underline;
}
.dash-list-card__empty {
  display: grid;
  place-items: center;
  padding: 3rem 1rem;
  color: var(--text-tertiary);
  font-size: 0.8125rem;
  text-align: center;
}
.dash-list-card__items {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}
.dash-list-card__item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid var(--border);
}
.dash-list-card__item:last-child {
  border-bottom: none;
  padding-bottom: 0;
}
.dash-list-card__item-left {
  display: flex;
  flex-direction: column;
  gap: 0.125rem;
}
.dash-item-title {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}
.dash-item-desc {
  font-size: 0.75rem;
  color: var(--text-secondary);
  margin: 0;
}
.dash-list-card__actions {
  display: flex;
  gap: 0.5rem;
}
.dash-list-card__badge-col {
  text-align: right;
}
.dash-item-val {
  font-size: 0.8125rem;
  margin: 0;
}
.dash-item-label {
  font-size: 0.6875rem;
  color: var(--text-tertiary);
  margin: 0;
}

/* Quick Action / Portal lists for employee */
.dash-portal-card {
  background-image: linear-gradient(135deg, var(--color-primary) 0%, var(--color-primary-hover) 100%);
  padding: 1.5rem;
  border-radius: var(--radius-xl);
  color: white;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  box-shadow: 0 8px 30px color-mix(in srgb, var(--color-primary) 20%, transparent);
}
.dash-portal-card__title {
  font-size: 1.125rem;
  font-weight: 700;
  margin: 0;
}
.dash-portal-card__desc {
  font-size: 0.8125rem;
  line-height: 1.5;
  margin: 0.5rem 0 0;
  opacity: 0.9;
}
.dash-portal-card__actions {
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
  margin-top: 1.5rem;
}
.dash-portal-btn {
  width: 100%;
  height: 2.5rem;
  font-size: 0.8125rem;
  font-weight: 700;
  border-radius: var(--radius-md);
  border: none;
  cursor: pointer;
  transition: all var(--transition-fast);
  display: inline-flex;
  align-items: center;
  justify-content: center;
}
.dash-portal-btn--primary {
  background-color: white;
  color: var(--color-primary-text);
}
.dash-portal-btn--primary:hover {
  background-color: var(--color-primary-light);
}
.dash-portal-btn--outline {
  background-color: transparent;
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.3);
}
.dash-portal-btn--outline:hover {
  background-color: rgba(255, 255, 255, 0.1);
  border-color: white;
}

.dash-card-footer {
  margin-top: auto;
  padding-top: 0.75rem;
  border-top: 1px solid var(--border);
}
.dash-footer-link {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--color-primary-text);
  cursor: pointer;
}
.dash-footer-link:hover {
  text-decoration: underline;
}

/* Action Items for quick actions */
.dash-action-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}
.dash-action-item {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 0.75rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border);
  background-color: var(--bg-subtle);
  transition: border-color var(--transition-fast);
}
.dash-action-item:hover {
  border-color: var(--border-strong);
}
.dash-action-item__btns {
  display: flex;
  gap: 0.375rem;
  margin-top: 0.5rem;
}

/* Table card styling */
.dash-table-card {
  padding: 1.25rem;
  border-radius: var(--radius-xl);
  border: 1px solid var(--border);
  background-color: var(--bg-surface);
  box-shadow: var(--shadow-sm);
}
.dash-table-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid var(--border);
}
.dash-table-card__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}
.dash-table-card__stats {
  display: flex;
  gap: 1rem;
  font-size: 0.75rem;
  font-weight: 500;
  color: var(--text-secondary);
}
.dash-table-card__empty {
  display: grid;
  place-items: center;
  padding: 3rem 1rem;
  color: var(--text-tertiary);
  font-size: 0.8125rem;
}
.dash-table-container {
  overflow-x: auto;
}
.dash-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.75rem;
  text-align: left;
}
.dash-table th {
  padding: 0.5rem 0.75rem;
  font-weight: 600;
  color: var(--text-tertiary);
  text-transform: uppercase;
  border-bottom: 1px solid var(--border);
}
.dash-table td {
  padding: 0.75rem;
  border-bottom: 1px solid var(--border-strong);
  color: var(--text-primary);
}
.dash-table tr:last-child td {
  border-bottom: none;
}
.dash-badge-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
}
.dash-small-badge {
  padding: 0.125rem 0.375rem;
  border-radius: 4px;
  background-color: var(--bg-subtle);
  border: 1px solid var(--border);
  color: var(--text-secondary);
  font-size: 0.625rem;
  font-weight: 500;
}
.dash-status-dot {
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
  font-weight: 600;
}
.dash-status-dot::before {
  content: "";
  width: 6px;
  height: 6px;
  border-radius: 50%;
}
.dash-status-dot--active { color: var(--color-success); }
.dash-status-dot--active::before { background-color: var(--color-success); }
.dash-status-dot--locked { color: var(--color-danger); }
.dash-status-dot--locked::before { background-color: var(--color-danger); }

/* Health connections bar */
.dash-health-card {
  padding: 1.25rem;
  border-radius: var(--radius-xl);
  border: 1px solid var(--border);
  background-color: var(--bg-surface);
  box-shadow: var(--shadow-sm);
}
.dash-health-card__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0 0 1rem;
}
.dash-health-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
}
@media (min-width: 768px) {
  .dash-health-grid { grid-template-columns: repeat(3, 1fr); }
}
.dash-health-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.75rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border);
  background-color: var(--bg-subtle);
}
.dash-health-item__name {
  font-weight: 600;
  color: var(--text-secondary);
  font-size: 0.8125rem;
}
.dash-health-item__status {
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
  font-weight: 700;
  color: var(--color-success);
  font-size: 0.8125rem;
}
.dash-pulse-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background-color: var(--color-success);
  animation: ping 1.5s infinite;
}
@keyframes ping {
  0% { transform: scale(1); opacity: 1; }
  100% { transform: scale(2.5); opacity: 0; }
}
</style>
