<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Calculator,
  FileText,
  Trash2,
  BarChart3,
  Lock,
  Printer,
  Coins,
  TrendingUp,
  X
} from '@lucide/vue'
import { mockDB, payrollService } from '../../services/mockData'
import { useAuthStore } from '../../stores/auth'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

// Sync active tab with router query param ?tab=
const activeTab = computed(() => {
  const queryTab = route.query.tab as string
  if (['calc', 'adjustments', 'reports'].includes(queryTab)) {
    return queryTab
  }
  return 'calc'
})

function setTab(tabName: 'calc' | 'adjustments' | 'reports') {
  router.push({ path: '/payroll', query: { tab: tabName } })
}

// Recalculating state
const selectedPeriodId = ref('per-02')
const isCalculating = ref(false)
const calcProgress = ref(0)
const calcStepMessage = ref('')

function runCalculation() {
  const period = mockDB.periods.find(p => p.id === selectedPeriodId.value)
  if (!period) return
  if (period.isClosed) {
    alert('Kỳ lương này đã khóa, không thể tính lại!')
    return
  }

  isCalculating.value = true
  calcProgress.value = 5
  calcStepMessage.value = 'Đang đồng bộ dữ liệu EmployeeProjection...'

  setTimeout(() => {
    calcProgress.value = 35
    calcStepMessage.value = 'Đang tổng hợp dữ liệu chấm công từ Attendance Projections...'
  }, 400)

  setTimeout(() => {
    calcProgress.value = 65
    calcStepMessage.value = 'Đang đối chiếu quy tắc lương (Base Rate & OT hours)...'
  }, 850)

  setTimeout(() => {
    calcProgress.value = 90
    calcStepMessage.value = 'Áp dụng các cấu phần Thưởng, Phạt & Khấu trừ BHXH...'
  }, 1200)

  setTimeout(() => {
    calcProgress.value = 100
    calcStepMessage.value = 'Hoàn tất tính toán bảng lương!'
    
    payrollService.calculatePeriod(selectedPeriodId.value)
    isCalculating.value = false
  }, 1600)
}

function lockPeriod() {
  if (confirm('Khóa kỳ lương này? Sau khi khóa sẽ không thể thay đổi dữ liệu.')) {
    payrollService.closePeriod(selectedPeriodId.value)
  }
}

// Adjustments
const adjForm = ref({
  employeeId: 'emp-001',
  type: 'Allowance' as 'Allowance' | 'Deduction',
  title: '',
  amount: 500000,
  isRecurring: true
})

function addAdjustment() {
  if (!adjForm.value.title || adjForm.value.amount <= 0) return
  payrollService.addAdjustment({ ...adjForm.value })
  adjForm.value.title = ''
  adjForm.value.amount = 500000
}

// Detailed payslip modal preview
const activePayslipModalId = ref<string | null>(null)
const selectedPayslip = computed(() => {
  return mockDB.payslips.find(p => p.id === activePayslipModalId.value)
})

const currentPeriod = computed(() => {
  return mockDB.periods.find(p => p.id === selectedPeriodId.value)
})

const payslipsInPeriod = computed(() => {
  return mockDB.payslips.filter(p => p.periodId === selectedPeriodId.value)
})

// Employee-only payslips filter
const employeePayslips = computed(() => {
  const empId = auth.user?.employeeId || 'emp-001'
  return mockDB.payslips.filter(p => p.employeeId === empId)
})

function formatVND(amount: number) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)
}

function printPayslip() {
  window.print()
}

// Statistics calculations
const totalPayrollCost = computed(() => {
  return payslipsInPeriod.value.reduce((sum, curr) => sum + curr.netSalary, 0)
})

const averageSalary = computed(() => {
  if (payslipsInPeriod.value.length === 0) return 0
  return Math.round(totalPayrollCost.value / payslipsInPeriod.value.length)
})

const departmentStats = computed(() => {
  return mockDB.departments.map(dept => {
    const empIds = mockDB.employees.filter(e => e.departmentId === dept.id).map(e => e.id)
    const deptPayslips = payslipsInPeriod.value.filter(p => empIds.includes(p.employeeId))
    const totalCost = deptPayslips.reduce((sum, curr) => sum + curr.netSalary, 0)
    return {
      deptName: dept.name,
      totalCost,
      count: deptPayslips.length
    }
  })
})
</script>

<template>
  <div class="space-y-6 animate-fadeIn">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between border-b border-slate-200 dark:border-slate-800 pb-4">
      <div>
        <h1 class="text-xl font-bold text-slate-900 dark:text-slate-50">💰 Payroll & Report Service (Quản lý Lương)</h1>
        <p class="text-xs text-slate-500 dark:text-slate-400 mt-0.5">Nhóm 9 · Schema: HRMS_PayrollReportDb · Thiết lập quy tắc, tính toán lương và xuất biểu đồ.</p>
      </div>
      <div class="flex items-center gap-2 text-xs font-semibold text-slate-500 dark:text-slate-400">
        <span>Cơ sở dữ liệu:</span>
        <span class="px-2 py-0.5 bg-blue-50 dark:bg-blue-950/20 text-blue-600 dark:text-blue-400 rounded-md border border-blue-150 dark:border-blue-900/40 font-mono">
          HRMS_PayrollReportDb
        </span>
      </div>
    </div>

    <!-- 1. MODE: EMPLOYEE VIEW ONLY -->
    <div v-if="auth.activeRole === 'Employee'" class="space-y-4">
      <div class="bg-white dark:bg-slate-900 p-5 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-2xs space-y-4">
        <div>
          <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Phiếu lương của bạn</h2>
          <p class="text-[11px] text-slate-455 mt-0.5">Tra cứu chi tiết phiếu chi lương trong các kỳ gần đây</p>
        </div>

        <div class="overflow-x-auto rounded-xl border border-slate-200 dark:border-slate-800">
          <table class="w-full text-left text-xs">
            <thead class="bg-slate-50 dark:bg-slate-850 text-slate-550 uppercase font-bold text-[10px] tracking-wider border-b border-slate-150 dark:border-slate-800">
              <tr>
                <th class="px-5 py-3.5">Kỳ lương</th>
                <th class="px-5 py-3.5">Lương cơ bản</th>
                <th class="px-5 py-3.5">Công thực tế</th>
                <th class="px-5 py-3.5">Tiền OT</th>
                <th class="px-5 py-3.5">Khoản thưởng / phạt</th>
                <th class="px-5 py-3.5">Thực nhận (Net)</th>
                <th class="px-5 py-3.5 text-right">Chi tiết</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 dark:divide-slate-800 font-semibold text-slate-700 dark:text-slate-300">
              <tr v-for="slip in employeePayslips" :key="slip.id" class="hover:bg-slate-50/30">
                <td class="px-5 py-4 text-slate-900 dark:text-slate-100 font-bold">
                  {{ mockDB.periods.find(p => p.id === slip.periodId)?.name || 'Kỳ lương' }}
                </td>
                <td class="px-5 py-4 font-mono">{{ formatVND(slip.baseSalary) }}</td>
                <td class="px-5 py-4 font-mono">{{ slip.actualWorkDays }} / {{ slip.workDays }} ngày</td>
                <td class="px-5 py-4 font-mono text-blue-600 dark:text-blue-400">+{{ formatVND(slip.otAmount) }}</td>
                <td class="px-5 py-4 font-mono text-red-600 dark:text-red-400">-{{ formatVND(slip.deductionAmount) }}</td>
                <td class="px-5 py-4 font-mono font-bold text-slate-950 dark:text-slate-50">{{ formatVND(slip.netSalary) }}</td>
                <td class="px-5 py-4 text-right">
                  <button 
                    @click="activePayslipModalId = slip.id"
                    class="inline-flex items-center gap-1 text-blue-600 dark:text-blue-400 hover:underline cursor-pointer"
                  >
                    <FileText :size="12" /> Xem phiếu
                  </button>
                </td>
              </tr>
              <tr v-if="employeePayslips.length === 0">
                <td colspan="7" class="text-center py-8 text-slate-400 italic">Chưa có dữ liệu phiếu lương nào cho nhân viên này.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- 2. MODE: STAFF / ADMIN VIEW -->
    <div v-else class="space-y-6">
      <!-- Tabs switcher -->
      <div class="flex border-b border-slate-200 dark:border-slate-800 overflow-x-auto no-print">
        <button 
          @click="setTab('calc')"
          class="flex items-center gap-2 px-5 py-3.5 border-b-2 text-sm font-semibold transition-all cursor-pointer whitespace-nowrap"
          :class="activeTab === 'calc' ? 'border-blue-600 text-blue-600 dark:text-blue-400 font-bold' : 'border-transparent text-slate-500 hover:text-slate-900'"
        >
          <Calculator :size="16" />
          <span>Tính & Bảng Lương</span>
        </button>

        <button 
          @click="setTab('adjustments')"
          class="flex items-center gap-2 px-5 py-3.5 border-b-2 text-sm font-semibold transition-all cursor-pointer whitespace-nowrap"
          :class="activeTab === 'adjustments' ? 'border-blue-600 text-blue-600 dark:text-blue-400 font-bold' : 'border-transparent text-slate-500 hover:text-slate-900'"
        >
          <Coins :size="16" />
          <span>Thưởng & Khấu trừ</span>
        </button>

        <button 
          @click="setTab('reports')"
          class="flex items-center gap-2 px-5 py-3.5 border-b-2 text-sm font-semibold transition-all cursor-pointer whitespace-nowrap"
          :class="activeTab === 'reports' ? 'border-blue-600 text-blue-600 dark:text-blue-400 font-bold' : 'border-transparent text-slate-500 hover:text-slate-900'"
        >
          <BarChart3 :size="16" />
          <span>Báo cáo & Thống kê</span>
        </button>
      </div>

      <!-- 2.1 TAB: CALCULATE & PAYSLIPS -->
      <div v-if="activeTab === 'calc'" class="space-y-4">
        <!-- Controls -->
        <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-4 bg-white dark:bg-slate-900 p-4 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-2xs no-print">
          <div class="flex flex-wrap items-center gap-3">
            <div class="flex items-center gap-2 text-xs font-semibold text-slate-600 dark:text-slate-400">
              <span>Chọn Kỳ tính lương:</span>
              <select v-model="selectedPeriodId" class="border border-slate-200 dark:border-slate-800 rounded-lg px-2.5 py-1.5 bg-white dark:bg-slate-900 text-slate-700 dark:text-slate-300">
                <option v-for="p in mockDB.periods" :key="p.id" :value="p.id">{{ p.name }}</option>
              </select>
            </div>

            <div class="text-xs text-slate-500 dark:text-slate-400 font-medium">
              Từ {{ currentPeriod?.startDate }} đến {{ currentPeriod?.endDate }} | Trạng thái: 
              <span class="font-bold uppercase" :class="currentPeriod?.isClosed ? 'text-red-600' : 'text-blue-600 dark:text-blue-400'">
                {{ currentPeriod?.isClosed ? 'Đã khóa' : 'Đang mở' }}
              </span>
            </div>
          </div>

          <div class="flex gap-2">
            <button 
              @click="runCalculation"
              :disabled="currentPeriod?.isClosed || isCalculating"
              class="inline-flex items-center justify-center gap-1.5 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-xs font-bold shadow-sm cursor-pointer disabled:opacity-50"
            >
              <Calculator :size="14" />
              <span>Chạy Tính Lương</span>
            </button>

            <button 
              v-if="!currentPeriod?.isClosed"
              @click="lockPeriod"
              class="inline-flex items-center justify-center gap-1.5 px-4 py-2 border border-red-200 text-red-650 hover:bg-red-50 dark:hover:bg-red-950/20 rounded-xl text-xs font-bold cursor-pointer animate-pulse"
            >
              <Lock :size="14" />
              <span>Khóa kỳ lương</span>
            </button>
          </div>
        </div>

        <!-- Calculation progress bar -->
        <div v-if="isCalculating" class="bg-white dark:bg-slate-900 p-4 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-2xs space-y-2">
          <div class="flex justify-between text-xs font-semibold text-slate-600 dark:text-slate-400">
            <span>{{ calcStepMessage }}</span>
            <span class="font-mono">{{ calcProgress }}%</span>
          </div>
          <div class="w-full bg-slate-100 dark:bg-slate-800 h-2 rounded-full overflow-hidden">
            <div class="bg-blue-600 dark:bg-blue-500 h-2 transition-all duration-300" :style="{ width: `${calcProgress}%` }"></div>
          </div>
        </div>

        <!-- Payslips Table -->
        <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl overflow-hidden shadow-2xs">
          <div class="overflow-x-auto">
            <table class="w-full text-left text-xs">
              <thead class="bg-slate-50 dark:bg-slate-850 text-slate-500 uppercase font-bold text-[10px] tracking-wider border-b border-slate-155 dark:border-slate-800">
                <tr>
                  <th class="px-5 py-3.5">Mã NV</th>
                  <th class="px-5 py-3.5">Họ và Tên</th>
                  <th class="px-5 py-3.5">Lương hợp đồng</th>
                  <th class="px-5 py-3.5">Công định mức</th>
                  <th class="px-5 py-3.5">Tiền OT</th>
                  <th class="px-5 py-3.5">Phụ cấp/Thưởng</th>
                  <th class="px-5 py-3.5">Khấu trừ</th>
                  <th class="px-5 py-3.5">Thực nhận (Net)</th>
                  <th class="px-5 py-3.5 text-right no-print">Thao tác</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100 dark:divide-slate-800 font-semibold text-slate-700 dark:text-slate-300">
                <tr v-for="slip in payslipsInPeriod" :key="slip.id" class="hover:bg-slate-50/30 dark:hover:bg-slate-850/10">
                  <td class="px-5 py-4 font-mono text-slate-400">
                    {{ mockDB.employees.find(e => e.id === slip.employeeId)?.employeeCode }}
                  </td>
                  <td class="px-5 py-4 text-slate-950 dark:text-slate-50 font-bold">
                    {{ mockDB.employees.find(e => e.id === slip.employeeId)?.fullName }}
                  </td>
                  <td class="px-5 py-4 font-mono">{{ formatVND(slip.baseSalary) }}</td>
                  <td class="px-5 py-4 font-mono text-slate-500">{{ slip.actualWorkDays }} / {{ slip.workDays }} ngày</td>
                  <td class="px-5 py-4 font-mono text-blue-600 dark:text-blue-400">+{{ formatVND(slip.otAmount) }}</td>
                  <td class="px-5 py-4 font-mono text-blue-600 dark:text-blue-400">+{{ formatVND(slip.allowanceAmount) }}</td>
                  <td class="px-5 py-4 font-mono text-red-600 dark:text-red-400">-{{ formatVND(slip.deductionAmount) }}</td>
                  <td class="px-5 py-4 font-mono font-bold text-blue-650 dark:text-blue-400 bg-blue-50/10 dark:bg-blue-950/10">{{ formatVND(slip.netSalary) }}</td>
                  <td class="px-5 py-4 text-right no-print">
                    <button 
                      @click="activePayslipModalId = slip.id"
                      class="text-blue-600 dark:text-blue-450 hover:underline inline-flex items-center gap-1 cursor-pointer"
                    >
                      <FileText :size="12" /> Xem phiếu
                    </button>
                  </td>
                </tr>

                <tr v-if="payslipsInPeriod.length === 0">
                  <td colspan="9" class="text-center py-12 text-slate-400 italic font-semibold">
                    Chưa có bảng lương nào được tính trong kỳ này. Hãy chạy tính lương ở trên!
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- 2.2 TAB: ALLOWANCES & DEDUCTIONS -->
      <div v-if="activeTab === 'adjustments'" class="grid gap-6 md:grid-cols-2">
        <!-- Add adjustment form -->
        <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs space-y-4">
          <div class="border-b border-slate-100 dark:border-slate-800 pb-2.5">
            <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Thêm khoản lương bổ sung</h2>
          </div>

          <div class="space-y-4 text-xs font-semibold text-slate-800 dark:text-slate-200">
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase">Chọn Nhân viên</label>
              <select v-model="adjForm.employeeId" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg bg-white dark:bg-slate-900">
                <option v-for="e in mockDB.employees.filter(emp => emp.status === 'Active')" :key="e.id" :value="e.id">
                  [{{ e.employeeCode }}] {{ e.fullName }}
                </option>
              </select>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div class="space-y-1">
                <label class="text-[10px] font-bold text-slate-400 uppercase">Phân loại</label>
                <select v-model="adjForm.type" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg bg-white dark:bg-slate-900">
                  <option value="Allowance">Phụ cấp / Thưởng (+)</option>
                  <option value="Deduction">Khấu trừ / Phạt (-)</option>
                </select>
              </div>
              <div class="space-y-1">
                <label class="text-[10px] font-bold text-slate-400 uppercase">Số tiền (VND)</label>
                <input type="number" v-model="adjForm.amount" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 font-mono font-bold" />
              </div>
            </div>

            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase">Nội dung chi tiết</label>
              <input v-model="adjForm.title" type="text" placeholder="e.g. Trách nhiệm dự án..." class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 outline-none focus:border-blue-500" />
            </div>

            <button @click="addAdjustment" class="w-full py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-lg text-xs shadow-sm cursor-pointer">Thêm cấu phần</button>
          </div>
        </div>

        <!-- Adjustment lists -->
        <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs space-y-4">
          <div>
            <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Các khoản phụ cộng / trừ thực tế</h2>
          </div>

          <div class="space-y-2.5 max-h-[340px] overflow-y-auto pr-1">
            <div v-for="a in mockDB.adjustments" :key="a.id" class="p-3.5 border border-slate-100 dark:border-slate-800 rounded-xl hover:bg-slate-50 dark:hover:bg-slate-850/30 flex items-center justify-between text-xs">
              <div>
                <div class="font-bold text-slate-900 dark:text-slate-100">
                  {{ mockDB.employees.find(e => e.id === a.employeeId)?.fullName }}
                </div>
                <div class="text-[10px] font-semibold text-slate-500 mt-1 flex items-center gap-1.5">
                  <span class="inline-block px-1.5 py-0.2 rounded text-[9px]"
                    :class="a.type === 'Allowance' ? 'bg-blue-50 dark:bg-blue-950/20 text-blue-700' : 'bg-red-50 dark:bg-red-950/20 text-red-700'"
                  >
                    {{ a.type === 'Allowance' ? 'Cộng' : 'Phạt' }}
                  </span>
                  <span>{{ a.title }}</span>
                </div>
              </div>

              <div class="flex items-center gap-3 font-mono">
                <span class="font-bold" :class="a.type === 'Allowance' ? 'text-blue-600 dark:text-blue-455' : 'text-red-600 dark:text-red-455'">
                  {{ a.type === 'Allowance' ? '+' : '-' }}{{ formatVND(a.amount) }}
                </span>
                <button @click="payrollService.removeAdjustment(a.id)" class="p-1 rounded text-slate-400 hover:text-red-500 hover:bg-slate-50 dark:hover:bg-slate-800 cursor-pointer">
                  <Trash2 :size="14" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 2.3 TAB: REPORT VISUAL COST CHART -->
      <div v-if="activeTab === 'reports'" class="space-y-6">
        <div class="grid gap-4 md:grid-cols-3">
          <!-- Widget 1: Quỹ lương -->
          <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs flex items-center gap-4">
            <div class="size-11 rounded-full bg-blue-50 dark:bg-blue-950/20 text-blue-600 dark:text-blue-400 flex items-center justify-center">
              <Coins :size="20" />
            </div>
            <div>
              <div class="text-base font-black font-mono text-slate-900 dark:text-slate-50">{{ formatVND(totalPayrollCost) }}</div>
              <div class="text-[10px] font-bold text-slate-400 uppercase tracking-wide">Tổng quỹ chi lương net</div>
            </div>
          </div>

          <!-- Widget 2: Lương trung bình -->
          <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs flex items-center gap-4">
            <div class="size-11 rounded-full bg-indigo-50 dark:bg-indigo-950/20 text-indigo-600 dark:text-indigo-400 flex items-center justify-center">
              <TrendingUp :size="20" />
            </div>
            <div>
              <div class="text-base font-black font-mono text-slate-900 dark:text-slate-50">{{ formatVND(averageSalary) }}</div>
              <div class="text-[10px] font-bold text-slate-400 uppercase tracking-wide">Lương Net bình quân</div>
            </div>
          </div>

          <!-- Widget 3: Số phiếu tạo -->
          <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs flex items-center gap-4">
            <div class="size-11 rounded-full bg-sky-50 dark:bg-sky-950/20 text-sky-600 dark:text-sky-400 flex items-center justify-center">
              <FileText :size="20" />
            </div>
            <div>
              <div class="text-base font-black font-mono text-slate-900 dark:text-slate-50">{{ payslipsInPeriod.length }} phiếu</div>
              <div class="text-[10px] font-bold text-slate-400 uppercase tracking-wide">Số lượng phiếu đã tạo</div>
            </div>
          </div>
        </div>

        <!-- Bar chart CSS -->
        <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-6 shadow-2xs space-y-6">
          <div>
            <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Cơ cấu ngân sách lương theo phòng ban</h2>
          </div>

          <div class="space-y-4">
            <div v-for="stat in departmentStats" :key="stat.deptName" class="space-y-1.5 text-xs font-semibold">
              <div class="flex justify-between text-slate-700 dark:text-slate-350">
                <span>{{ stat.deptName }} ({{ stat.count }} nhân viên)</span>
                <span class="font-mono font-bold text-slate-900 dark:text-slate-100">{{ formatVND(stat.totalCost) }}</span>
              </div>
              <div class="w-full bg-slate-100 dark:bg-slate-800 h-3.5 rounded-full overflow-hidden flex shadow-inner">
                <div class="bg-gradient-to-r from-blue-600 to-indigo-500 h-3.5 rounded-full" :style="{ width: `${totalPayrollCost > 0 ? (stat.totalCost / totalPayrollCost) * 100 : 0}%` }"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- MODAL: PAYSLIP DETAIL MODAL (PRINT FRIENDLY) -->
    <div v-if="activePayslipModalId" class="fixed inset-0 bg-slate-950/40 backdrop-blur-xs flex items-center justify-center z-40 p-4">
      <div class="bg-white dark:bg-slate-900 rounded-2xl border border-slate-200 dark:border-slate-850 shadow-xl max-w-lg w-full overflow-hidden animate-scaleIn">
        <!-- Header -->
        <div class="bg-slate-50 dark:bg-slate-850 px-5 py-4 border-b border-slate-200 dark:border-slate-850 flex items-center justify-between no-print">
          <span class="text-xs font-black text-slate-900 dark:text-slate-100 uppercase tracking-wider">Biên nhận Phiếu lương chi tiết</span>
          <div class="flex gap-2">
            <button @click="printPayslip" class="px-3 py-1.5 bg-white dark:bg-slate-900 border border-slate-250 dark:border-slate-800 hover:bg-slate-50 rounded-lg text-[10.5px] font-bold text-slate-700 dark:text-slate-300 flex items-center gap-1.5 cursor-pointer shadow-2xs">
              <Printer :size="12" /> In hóa đơn
            </button>
            <button @click="activePayslipModalId = null" class="p-1 rounded text-slate-400 hover:text-slate-600 dark:hover:text-slate-200">
              <X :size="18" />
            </button>
          </div>
        </div>

        <!-- Receipt Area -->
        <div id="payslip-print-area" class="p-6 space-y-6 text-slate-800 dark:text-slate-200">
          <div class="text-center border-b border-dashed border-slate-200 dark:border-slate-800 pb-5">
            <div class="text-[9px] font-extrabold text-slate-400 dark:text-slate-500 uppercase tracking-widest">BIÊN LAI THANH TOÁN LƯƠNG THÁNG</div>
            <h2 class="text-lg font-black text-slate-900 dark:text-slate-100 mt-1 uppercase">
              {{ mockDB.periods.find(p => p.id === selectedPayslip?.periodId)?.name }}
            </h2>
            <div class="text-[9px] text-slate-400 font-mono mt-1">ID: {{ selectedPayslip?.id }}</div>
          </div>

          <!-- Employee meta details -->
          <div class="grid grid-cols-2 gap-4 text-xs font-semibold border-b border-slate-100 dark:border-slate-800 pb-4">
            <div>
              <span class="text-[9px] text-slate-455 uppercase block mb-1">Nhân viên nhận:</span>
              <strong class="text-slate-950 dark:text-slate-105 text-sm font-black">
                {{ mockDB.employees.find(e => e.id === selectedPayslip?.employeeId)?.fullName }}
              </strong>
              <div class="font-mono text-[10px] text-slate-400 mt-0.5">
                Mã: {{ mockDB.employees.find(e => e.id === selectedPayslip?.employeeId)?.employeeCode }}
              </div>
            </div>
            <div>
              <span class="text-[9px] text-slate-455 uppercase block mb-1">Phòng ban & chức danh:</span>
              <span class="text-slate-800 dark:text-slate-200 block">
                {{ mockDB.positions.find(p => p.id === mockDB.employees.find(e => e.id === selectedPayslip?.employeeId)?.positionId)?.name }}
              </span>
              <span class="text-[10px] text-slate-450 block mt-0.5">
                {{ mockDB.departments.find(d => d.id === mockDB.employees.find(e => e.id === selectedPayslip?.employeeId)?.departmentId)?.name }}
              </span>
            </div>
          </div>

          <!-- Breakdowns table list -->
          <div class="space-y-2.5 text-xs">
            <div class="flex justify-between font-bold text-slate-400 uppercase text-[9px] border-b border-slate-100 dark:border-slate-800 pb-1.5">
              <span>Hạng mục thanh toán</span>
              <span>Định mức thực tế</span>
              <span class="text-right">Mức chi (VND)</span>
            </div>

            <div class="flex justify-between font-medium">
              <span>Lương cơ bản</span>
              <span class="text-slate-400 font-normal">Đầy đủ</span>
              <span class="font-mono text-slate-800 dark:text-slate-200">{{ formatVND(selectedPayslip?.baseSalary ?? 0) }}</span>
            </div>

            <div class="flex justify-between font-medium">
              <span>Lương thực tế theo ngày công</span>
              <span class="text-slate-500 font-mono">{{ selectedPayslip?.actualWorkDays }} / {{ selectedPayslip?.workDays }} ngày</span>
              <span class="font-mono text-slate-800 dark:text-slate-200">
                {{ formatVND(Math.round(((selectedPayslip?.baseSalary ?? 0) / (selectedPayslip?.workDays ?? 22)) * (selectedPayslip?.actualWorkDays ?? 0))) }}
              </span>
            </div>

            <div v-if="selectedPayslip && selectedPayslip.otHours > 0" class="flex justify-between font-medium">
              <span>Làm thêm giờ (OT 150%)</span>
              <span class="text-slate-550 font-mono">{{ selectedPayslip.otHours }} giờ</span>
              <span class="font-mono text-blue-600 dark:text-blue-400">+{{ formatVND(selectedPayslip.otAmount) }}</span>
            </div>

            <div v-if="selectedPayslip && selectedPayslip.allowanceAmount > 0" class="flex justify-between font-medium">
              <span>Các khoản phụ cấp / thưởng</span>
              <span class="text-slate-400 font-normal">Thưởng thêm</span>
              <span class="font-mono text-blue-600 dark:text-blue-400">+{{ formatVND(selectedPayslip.allowanceAmount) }}</span>
            </div>

            <div v-if="selectedPayslip && selectedPayslip.deductionAmount > 0" class="flex justify-between font-medium">
              <span>Khấu trừ (Bảo hiểm, Kỷ luật...)</span>
              <span class="text-slate-400 font-normal">Khấu trừ (-)</span>
              <span class="font-mono text-red-650 dark:text-red-400">-{{ formatVND(selectedPayslip.deductionAmount) }}</span>
            </div>
          </div>

          <!-- Total net receipt box -->
          <div class="bg-blue-50/50 dark:bg-blue-950/20 p-4 border border-blue-150 dark:border-blue-800 rounded-xl flex items-center justify-between text-slate-900 dark:text-slate-100 border-t-2 border-blue-600">
            <div class="font-bold text-xs uppercase text-blue-800 dark:text-blue-400">TỔNG LƯƠNG NET THỰC NHẬN</div>
            <div class="text-lg font-black font-mono text-blue-755 dark:text-blue-400 tracking-tight">
              {{ formatVND(selectedPayslip?.netSalary ?? 0) }}
            </div>
          </div>

          <!-- Sign panel -->
          <div class="flex justify-between text-[10px] text-slate-400 font-semibold pt-4">
            <div class="text-center w-24">
              <div>Người nhận phiếu</div>
              <div class="h-10"></div>
              <div class="text-slate-500 dark:text-slate-400">
                {{ mockDB.employees.find(e => e.id === selectedPayslip?.employeeId)?.fullName }}
              </div>
            </div>
            
            <div class="text-center w-28">
              <div>Kế toán lập biểu</div>
              <div class="h-10"></div>
              <div class="text-slate-500 dark:text-slate-400">
                {{ currentPeriod?.closedBy || 'Kế toán trưởng' }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
