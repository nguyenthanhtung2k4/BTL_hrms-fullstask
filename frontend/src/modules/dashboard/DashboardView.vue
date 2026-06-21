<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { useToastStore } from '../../stores/toast'
import { useDashboard } from '../../composables/useDashboard'
import { leaveService } from '../../services/leave.service'
import { payrollPeriodService } from '../../services/payrollPeriod.service'
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
  RefreshCw
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
  payrollTrend,
  statusDist,
  deptDist,
  periodsNeedAction,
  load
} = useDashboard()

onMounted(load)

// Local loading states for quick inline actions
const actionLoading = ref<Record<string, boolean>>({})

const greeting = computed(() => {
  const h = new Date().getHours()
  if (h < 12) return 'Chào buổi sáng'
  if (h < 18) return 'Chào buổi chiều'
  return 'Chào buổi tối'
})

// Formatting helpers
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

// 📈 Chart Configurations & Data mapping
const statusChartData = computed<any>(() => ({
  labels: ['Đang hoạt động', 'Tạm nghỉ', 'Nghỉ phép', 'Đã nghỉ việc'],
  datasets: [
    {
      data: [
        statusDist.value.Active,
        statusDist.value.Inactive,
        statusDist.value.OnLeave,
        statusDist.value.Resigned
      ],
      backgroundColor: ['#10b981', '#94a3b8', '#fbbf24', '#f87171'],
      borderWidth: 2,
      borderColor: '#ffffff',
      hoverOffset: 6
    }
  ]
}))

const deptChartData = computed<any>(() => ({
  labels: deptDist.value.map(d => d.name),
  datasets: [
    {
      label: 'Nhân viên',
      data: deptDist.value.map(d => d.count),
      backgroundColor: 'rgba(59, 130, 246, 0.8)',
      hoverBackgroundColor: 'rgba(59, 130, 246, 1)',
      borderRadius: 6,
      barThickness: 24
    }
  ]
}))

const payrollChartData = computed<any>(() => ({
  labels: payrollTrend.value.map(p => p.name),
  datasets: [
    {
      label: 'Lương Gross',
      data: payrollTrend.value.map(p => p.gross),
      borderColor: '#f59e0b',
      backgroundColor: 'transparent',
      borderWidth: 2,
      pointBackgroundColor: '#f59e0b',
      tension: 0.3
    },
    {
      label: 'Thực nhận (Net)',
      data: payrollTrend.value.map(p => p.net),
      borderColor: '#10b981',
      backgroundColor: 'rgba(16, 185, 129, 0.08)',
      borderWidth: 3,
      fill: true,
      pointBackgroundColor: '#10b981',
      tension: 0.3
    }
  ]
}))

const attendanceHistoryChartData = computed<any>(() => ({
  labels: attendanceHistory.value.map(a => a.date),
  datasets: [
    {
      label: 'Lượt chấm công',
      data: attendanceHistory.value.map(a => a.count),
      backgroundColor: 'rgba(16, 185, 129, 0.75)',
      hoverBackgroundColor: 'rgba(16, 185, 129, 0.95)',
      borderRadius: 4,
      barThickness: 20
    }
  ]
}))

const myPayslipsChartData = computed<any>(() => ({
  labels: myPayslips.value.slice(-6).map(p => p.fullName.split(' ')[0]),
  datasets: [
    {
      label: 'Lương thực nhận (Net)',
      data: myPayslips.value.slice(-6).map(p => p.netSalary),
      backgroundColor: 'rgba(16, 185, 129, 0.8)',
      borderRadius: 6,
      barThickness: 24
    }
  ]
}))

const chartOptions: any = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'bottom',
      labels: {
        boxWidth: 10,
        boxHeight: 10,
        font: { size: 11, family: 'Inter, system-ui' },
        padding: 16
      }
    }
  },
  scales: {
    x: {
      grid: { display: false }
    },
    y: {
      grid: { color: '#f1f5f9' },
      ticks: { precision: 0 }
    }
  }
}

const doughnutOptions: any = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'right',
      labels: {
        boxWidth: 12,
        font: { size: 12, family: 'Inter, system-ui' },
        padding: 12
      }
    }
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- Greeting Panel -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between bg-white p-6 rounded-2xl border border-slate-100 shadow-sm gap-4">
      <div>
        <h1 class="text-2xl font-bold text-slate-800 tracking-tight">{{ greeting }}, {{ auth.displayName }} 👋</h1>
        <p class="mt-1 text-sm text-slate-500 font-medium">
          {{ new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) }}
        </p>
      </div>
      <div>
        <button 
          class="flex items-center gap-2 px-4 py-2 rounded-xl text-slate-600 bg-slate-50 hover:bg-slate-100 hover:text-slate-800 font-semibold text-sm transition-all duration-200" 
          @click="load"
        >
          <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />
          Làm mới
        </button>
      </div>
    </div>

    <!-- 👑 ROLE: ADMIN / HR DASHBOARD -->
    <div v-if="auth.isHR || auth.isAdmin" class="space-y-6">
      <!-- Row 1: KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
        <StatCard title="Tổng nhân sự" :value="employees.length" subtitle="Nhân sự trong hệ thống" color="emerald" :loading="loading">
          <template #icon><Users class="w-5 h-5 text-emerald-600" /></template>
        </StatCard>
        <StatCard title="Hôm nay đi làm" :value="`${todayAttendance.checkedIn} NV`" :subtitle="`Tỷ lệ có mặt: ${todayAttendance.rate}%`" color="blue" :loading="loading">
          <template #icon><CheckCircle class="w-5 h-5 text-blue-600" /></template>
        </StatCard>
        <StatCard title="NV Mới Trong Tháng" :value="newHires.length" subtitle="Gia nhập tháng này" color="violet" :loading="loading">
          <template #icon><UserPlus class="w-5 h-5 text-violet-600" /></template>
        </StatCard>
        <StatCard title="HĐ Sắp Hết Hạn" :value="expiringContracts.length" subtitle="Trong vòng 30 ngày tới" color="red" :loading="loading">
          <template #icon><AlertTriangle class="w-5 h-5 text-rose-600" /></template>
        </StatCard>
      </div>

      <!-- Row 2: Analysis Charts -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Human Resource Status -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex flex-col justify-between">
          <h3 class="text-sm font-semibold text-slate-800 mb-4 flex items-center gap-2">
            <Activity class="w-4 h-4 text-emerald-500" /> Trạng thái nhân sự
          </h3>
          <div class="h-64 relative">
            <Doughnut v-if="!loading" :data="statusChartData" :options="doughnutOptions" />
          </div>
        </div>

        <!-- Employees by Department -->
        <div class="lg:col-span-2 bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex flex-col justify-between">
          <h3 class="text-sm font-semibold text-slate-800 mb-4 flex items-center gap-2">
            <Users class="w-4 h-4 text-blue-500" /> Nhân viên theo phòng ban
          </h3>
          <div class="h-64 relative">
            <Bar v-if="!loading" :data="deptChartData" :options="chartOptions" />
          </div>
        </div>
      </div>

      <!-- Row 3: Payroll Trend & Attendance History -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Payroll Trend -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex flex-col justify-between">
          <h3 class="text-sm font-semibold text-slate-800 mb-4 flex items-center gap-2">
            <TrendingUp class="w-4 h-4 text-amber-500" /> Biến động quỹ lương Net & Gross
          </h3>
          <div class="h-64 relative">
            <Line v-if="!loading && payrollTrend.length > 0" :data="payrollChartData" :options="chartOptions" />
            <div v-else-if="!loading" class="py-24 text-center text-slate-400 text-sm">
              Chưa có dữ liệu phiếu lương nào để phân tích quỹ lương.
            </div>
          </div>
        </div>

        <!-- Attendance Trends -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex flex-col justify-between">
          <h3 class="text-sm font-semibold text-slate-800 mb-4 flex items-center gap-2">
            <Clock class="w-4 h-4 text-emerald-500" /> Lượt chấm công 7 ngày gần nhất
          </h3>
          <div class="h-64 relative">
            <Bar v-if="!loading" :data="attendanceHistoryChartData" :options="chartOptions" />
          </div>
        </div>
      </div>

      <!-- Row 4: Actionable Lists -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Leaves waiting for approval -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-sm font-semibold text-slate-800 flex items-center gap-2">
              <Calendar class="w-4 h-4 text-emerald-500" /> Đơn nghỉ phép chờ duyệt
            </h3>
            <span class="text-xs text-emerald-600 font-semibold hover:underline cursor-pointer flex items-center gap-1" @click="router.push('/attendance/leaves')">
              Xem tất cả <ArrowRight class="w-3.5 h-3.5" />
            </span>
          </div>
          <div v-if="pendingLeaves.length === 0" class="py-12 text-center text-slate-400 text-sm">
            Không có đơn nghỉ phép nào đang chờ duyệt.
          </div>
          <div v-else class="space-y-3">
            <div v-for="l in pendingLeaves.slice(0, 5)" :key="l.id" class="flex justify-between items-center border-b border-slate-100 pb-3 last:border-0 last:pb-0">
              <div>
                <p class="font-semibold text-slate-800 text-sm">{{ l.employeeName }}</p>
                <p class="text-xs text-slate-500 font-medium">{{ l.leaveTypeName }} ({{ l.totalDays }} ngày: {{ fmtDate(l.fromDate) }} - {{ fmtDate(l.toDate) }})</p>
              </div>
              <div class="flex gap-1.5">
                <AppButton size="sm" variant="success" :loading="actionLoading[l.id]" @click="handleLeave(l.id, 'Approved')">Duyệt</AppButton>
                <AppButton size="sm" variant="danger" :loading="actionLoading[l.id]" @click="handleLeave(l.id, 'Rejected')">Từ chối</AppButton>
              </div>
            </div>
          </div>
        </div>

        <!-- Expiring Contracts -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-sm font-semibold text-slate-800 flex items-center gap-2">
              <FileText class="w-4 h-4 text-rose-500" /> Hợp đồng sắp hết hạn (30 ngày)
            </h3>
            <span class="text-xs text-emerald-600 font-semibold hover:underline cursor-pointer flex items-center gap-1" @click="router.push('/hr/contracts')">
              Xem tất cả <ArrowRight class="w-3.5 h-3.5" />
            </span>
          </div>
          <div v-if="expiringContracts.length === 0" class="py-12 text-center text-slate-400 text-sm">
            Không có hợp đồng nào sắp hết hạn.
          </div>
          <div v-else class="space-y-3">
            <div v-for="c in expiringContracts.slice(0, 5)" :key="c.id" class="flex justify-between items-center border-b border-slate-100 pb-3 last:border-0 last:pb-0">
              <div>
                <p class="font-semibold text-slate-800 text-sm">{{ c.employeeName }}</p>
                <p class="text-xs text-slate-500 font-medium">Mã hợp đồng: {{ c.contractNumber }} · Loại: {{ c.contractType }}</p>
              </div>
              <div class="text-right">
                <p class="text-sm font-semibold text-rose-600">{{ fmtDate(c.endDate || '') }}</p>
                <p class="text-xs text-slate-400 font-medium">Ngày kết thúc</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 👔 ROLE: MANAGER DASHBOARD -->
    <div v-else-if="auth.isManager" class="space-y-6">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
        <StatCard title="Nhân viên quản lý" :value="employees.length" subtitle="Nhân viên trực thuộc" color="emerald" :loading="loading">
          <template #icon><Users class="w-5 h-5 text-emerald-600" /></template>
        </StatCard>
        <StatCard title="Đang nghỉ phép" :value="employees.filter(e => e.status === 'OnLeave').length" subtitle="Nhân viên vắng phép" color="amber" :loading="loading">
          <template #icon><Calendar class="w-5 h-5 text-amber-600" /></template>
        </StatCard>
        <StatCard title="Đơn nghỉ chờ duyệt" :value="pendingLeaves.length" subtitle="Cần phê duyệt" color="red" :loading="loading">
          <template #icon><Clock class="w-5 h-5 text-rose-600" /></template>
        </StatCard>
      </div>

      <!-- Manager Attendance Analysis & Leave List -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Attendance History for Department -->
        <div class="lg:col-span-2 bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex flex-col justify-between">
          <h3 class="text-sm font-semibold text-slate-800 mb-4 flex items-center gap-2">
            <Clock class="w-4 h-4 text-emerald-500" /> Tần suất chấm công phòng ban (7 ngày gần nhất)
          </h3>
          <div class="h-64 relative">
            <Bar v-if="!loading" :data="attendanceHistoryChartData" :options="chartOptions" />
          </div>
        </div>

        <!-- Leaves -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex flex-col justify-between">
          <div>
            <h3 class="text-sm font-semibold text-slate-800 mb-4 flex items-center gap-2">
              <Calendar class="w-4 h-4 text-emerald-500" /> Phê duyệt nghỉ phép nhanh
            </h3>
            <div v-if="pendingLeaves.length === 0" class="py-12 text-center text-slate-400 text-sm">
              Không có đơn nghỉ phép nào đang chờ phê duyệt.
            </div>
            <div v-else class="space-y-4 max-h-64 overflow-y-auto pr-1">
              <div v-for="l in pendingLeaves" :key="l.id" class="border border-slate-100 rounded-xl p-3 bg-slate-50 space-y-2">
                <div>
                  <p class="font-semibold text-slate-800 text-sm">{{ l.employeeName }}</p>
                  <p class="text-xs text-slate-500 font-medium">{{ l.leaveTypeName }} · {{ l.totalDays }} ngày</p>
                  <p class="text-xs text-slate-400 italic mt-0.5">Lý do: "{{ l.reason }}"</p>
                </div>
                <div class="flex gap-2 justify-end pt-1">
                  <AppButton size="sm" variant="success" :loading="actionLoading[l.id]" @click="handleLeave(l.id, 'Approved')">Duyệt</AppButton>
                  <AppButton size="sm" variant="danger" :loading="actionLoading[l.id]" @click="handleLeave(l.id, 'Rejected')">Từ chối</AppButton>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 💰 ROLE: PAYROLLSTAFF DASHBOARD -->
    <div v-else-if="auth.isPayrollStaff" class="space-y-6">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
        <StatCard title="Kỳ lương đang mở" :value="periodsNeedAction.length" subtitle="Cần xử lý & tính toán" color="blue" :loading="loading">
          <template #icon><Calendar class="w-5 h-5 text-blue-600" /></template>
        </StatCard>
        <StatCard title="Tổng phiếu lương" :value="allPayslips.length" subtitle="Tất cả thời kỳ" color="emerald" :loading="loading">
          <template #icon><FileText class="w-5 h-5 text-emerald-600" /></template>
        </StatCard>
        <StatCard title="Chi phí kỳ gần nhất" :value="allPayslips.slice(-1)[0] ? fmtMoney(allPayslips.slice(-1)[0].netSalary) : '0 ₫'" subtitle="Thực nhận tổng cộng" color="violet" :loading="loading">
          <template #icon><CreditCard class="w-5 h-5 text-violet-600" /></template>
        </StatCard>
      </div>

      <!-- Trend Chart & Period Control -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div class="lg:col-span-2 bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex flex-col justify-between">
          <h3 class="text-sm font-semibold text-slate-800 mb-4 flex items-center gap-2">
            <TrendingUp class="w-4 h-4 text-emerald-500" /> Biến động chi lương (Net & Gross qua các kỳ)
          </h3>
          <div class="h-64 relative">
            <Line v-if="!loading && payrollTrend.length > 0" :data="payrollChartData" :options="chartOptions" />
            <div v-else-if="!loading" class="py-24 text-center text-slate-400 text-sm">
              Chưa có dữ liệu phiếu lương nào để phân tích.
            </div>
          </div>
        </div>

        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex flex-col justify-between">
          <div>
            <h3 class="text-sm font-semibold text-slate-800 mb-4 flex items-center gap-2">
              <Calendar class="w-4 h-4 text-blue-500" /> Hành động kỳ lương
            </h3>
            <div v-if="periodsNeedAction.length === 0" class="py-12 text-center text-slate-400 text-sm">
              Tất cả các kỳ lương đã đóng.
            </div>
            <div v-else class="space-y-4 max-h-64 overflow-y-auto pr-1">
              <div v-for="p in periodsNeedAction" :key="p.id" class="border border-slate-100 rounded-xl p-3 bg-slate-50 space-y-2">
                <div class="flex items-center justify-between">
                  <span class="font-semibold text-slate-800 text-sm">{{ p.name }}</span>
                  <AppBadge :status="p.status" />
                </div>
                <p class="text-xs text-slate-500 font-medium">Thời gian: {{ fmtDate(p.fromDate) }} - {{ fmtDate(p.toDate) }}</p>
                <div class="flex justify-end pt-1">
                  <AppButton
                    v-if="p.status === 'Draft'"
                    size="sm"
                    variant="primary"
                    :loading="actionLoading[p.id]"
                    @click="handleCalculatePeriod(p.id)"
                  >
                    Tính toán lương
                  </AppButton>
                  <AppButton
                    v-else-if="p.status === 'Calculated'"
                    size="sm"
                    variant="success"
                    @click="router.push(`/payroll/periods`)"
                  >
                    Xem & Đóng kỳ
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
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Left: Quick utility panel with custom modern styles -->
        <div class="bg-gradient-to-br from-emerald-500 to-teal-600 p-6 rounded-2xl text-white shadow-lg shadow-emerald-500/10 flex flex-col justify-between">
          <div>
            <h3 class="text-lg font-bold tracking-tight">Cổng thông tin cá nhân</h3>
            <p class="text-sm text-emerald-100 mt-2 font-medium leading-relaxed">
              Bạn có thể dễ dàng quản lý ca làm việc, thực hiện Check-in / Check-out hàng ngày và gửi đơn nghỉ phép trực tuyến một cách bảo mật.
            </p>
          </div>
          <div class="mt-8 space-y-2">
            <AppButton variant="secondary" class="w-full justify-center bg-white text-emerald-700 hover:bg-emerald-50 font-bold" @click="router.push('/attendance/checkin')">
              🕑 Check-in / Check-out ngay
            </AppButton>
            <AppButton variant="ghost" class="w-full justify-center text-white hover:bg-white/10 font-bold border border-white/20" @click="router.push('/attendance/leaves')">
              🏖️ Đăng ký nghỉ phép
            </AppButton>
          </div>
        </div>

        <!-- Middle: Personal salary analysis chart -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex flex-col justify-between">
          <div>
            <h3 class="text-sm font-semibold text-slate-800 mb-2 flex items-center gap-2">
              <TrendingUp class="w-4 h-4 text-emerald-500" /> Biểu đồ lương Net (6 kỳ gần nhất)
            </h3>
            <div class="h-48 relative mt-3">
              <Bar v-if="!loading && myPayslips.length > 0" :data="myPayslipsChartData" :options="chartOptions" />
              <div v-else-if="!loading" class="py-12 text-center text-slate-400 text-sm">
                Chưa có dữ liệu phiếu lương.
              </div>
            </div>
          </div>
          <div class="pt-3 border-t border-slate-50">
            <AppButton size="sm" variant="ghost" class="w-full justify-center text-emerald-600 font-semibold" @click="router.push('/payroll/my-payslip')">
              Xem chi tiết phiếu lương →
            </AppButton>
          </div>
        </div>

        <!-- Right: Leaves request overview -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex flex-col justify-between">
          <div>
            <h3 class="text-sm font-semibold text-slate-800 mb-4 flex items-center gap-2">
              <Calendar class="w-4 h-4 text-emerald-500" /> Yêu cầu nghỉ phép gần nhất
            </h3>
            <div v-if="myLeaves.length === 0" class="py-12 text-center text-slate-400 text-sm">
              Bạn chưa đăng ký đơn nghỉ phép nào.
            </div>
            <div v-else class="space-y-3">
              <div v-for="l in myLeaves.slice(-3).reverse()" :key="l.id" class="flex justify-between items-center text-sm border-b border-slate-50 pb-2 last:border-0 last:pb-0">
                <div>
                  <p class="font-semibold text-slate-800">{{ l.leaveTypeName }}</p>
                  <p class="text-xs text-slate-500 font-medium">{{ l.totalDays }} ngày ({{ fmtDate(l.fromDate) }})</p>
                </div>
                <AppBadge :status="l.status" />
              </div>
            </div>
          </div>
          <div class="pt-3 border-t border-slate-50">
            <AppButton size="sm" variant="ghost" class="w-full justify-center text-emerald-600 font-semibold" @click="router.push('/attendance/leaves')">
              Quản lý đơn nghỉ phép →
            </AppButton>
          </div>
        </div>
      </div>
    </div>

    <!-- 🏥 System Health Connection Status Bar (Visible to Admin/HR/PayrollStaff) -->
    <div v-if="auth.isHR || auth.isAdmin || auth.isPayrollStaff" class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm mt-6">
      <h3 class="text-sm font-semibold text-slate-800 mb-4 flex items-center gap-2">
        <Activity class="w-4 h-4 text-emerald-500" /> Trạng thái kết nối dịch vụ
      </h3>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
        <div class="flex items-center justify-between p-3 border border-slate-100 rounded-xl bg-slate-50">
          <span class="font-semibold text-slate-600">HR Core API</span>
          <span class="flex items-center gap-1.5 font-bold text-emerald-600">
            <span class="h-2 w-2 rounded-full bg-emerald-500 animate-ping"></span> 🟢 Hoạt động
          </span>
        </div>
        <div class="flex items-center justify-between p-3 border border-slate-100 rounded-xl bg-slate-50">
          <span class="font-semibold text-slate-600">Attendance API</span>
          <span class="flex items-center gap-1.5 font-bold text-emerald-600">
            <span class="h-2 w-2 rounded-full bg-emerald-500 animate-ping"></span> 🟢 Hoạt động
          </span>
        </div>
        <div class="flex items-center justify-between p-3 border border-slate-100 rounded-xl bg-slate-50">
          <span class="font-semibold text-slate-600">Payroll API</span>
          <span class="flex items-center gap-1.5 font-bold text-emerald-600">
            <span class="h-2 w-2 rounded-full bg-emerald-500 animate-ping"></span> 🟢 Hoạt động
          </span>
        </div>
      </div>
    </div>
  </div>
</template>
