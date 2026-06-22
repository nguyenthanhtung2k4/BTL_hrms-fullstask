<script setup lang="ts">
import { computed, onMounted, nextTick } from 'vue'
import {
  Network,
  Users,
  CalendarCheck,
  BadgeDollarSign,
  History,
  FileCheck,
  Zap,
  Layers,
  ArrowUpRight,
  TrendingUp,
  UserPlus
} from '@lucide/vue'
import { mockDB, emitEvent } from '../../services/mockData'
import gsap from 'gsap'

const serviceRows = [
  { name: 'HR Core Service', database: 'HRMS_HrCoreDb', owner: 'Nhóm 7', port: '5001', color: 'text-blue-600 bg-blue-50 dark:bg-blue-950/20 border-blue-200 dark:border-blue-800' },
  { name: 'Attendance Service', database: 'HRMS_AttendanceDb', owner: 'Nhóm 8', port: '5002', color: 'text-sky-600 bg-sky-50 dark:bg-sky-950/20 border-sky-200 dark:border-sky-850' },
  { name: 'Payroll & Report Service', database: 'HRMS_PayrollReportDb', owner: 'Nhóm 9', port: '5003', color: 'text-indigo-600 bg-indigo-50 dark:bg-indigo-950/20 border-indigo-200 dark:border-indigo-850' },
]

// Dynamic metrics
const totalEmployees = computed(() => mockDB.employees.length)
const activeEmployees = computed(() => mockDB.employees.filter(e => e.status === 'Active').length)
const newHires = computed(() => mockDB.employees.filter(e => new Date(e.joinedDate).getFullYear() >= 2024).length)
const todayDateStr = new Date().toISOString().split('T')[0]
const todayCheckIns = computed(() => {
  return mockDB.attendanceRecords.filter(r => r.workDate === todayDateStr && r.checkInAt).length
})
const attendanceRate = computed(() => {
  if (activeEmployees.value === 0) return '0%'
  const rate = (todayCheckIns.value / activeEmployees.value) * 100
  return todayCheckIns.value > 0 ? `${Math.round(rate)}%` : '95%'
})
const pendingLeaves = computed(() => {
  return mockDB.leaveRequests.filter(l => l.status === 'Pending').length
})
const totalDepartments = computed(() => mockDB.departments.length)
const totalPayrollCost = computed(() => {
  const slips = mockDB.payslips.filter(p => p.periodId === 'per-02')
  const total = slips.reduce((sum, curr) => sum + curr.netSalary, 0)
  return total > 0 ? total : 84000000 
})

const formatVND = (amount: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)
}

function triggerSystemPing() {
  emitEvent('SystemPing', 'hr-core', {
    pingedAt: new Date().toISOString(),
    status: 'Healthy',
    activeServices: ['hr-core', 'attendance', 'payroll-report']
  })
}

// Chart data
const hrChartData = computed(() => {
  return mockDB.departments.map(d => {
    const count = mockDB.employees.filter(e => e.departmentId === d.id && e.status === 'Active').length
    const pct = totalEmployees.value > 0 ? (count / totalEmployees.value) * 100 : 0
    return { name: d.name, count, pct }
  })
})

const attendanceChartData = [
  { day: 'T2', rate: 94 },
  { day: 'T3', rate: 96 },
  { day: 'T4', rate: 91 },
  { day: 'T5', rate: 95 },
  { day: 'T6', rate: 92 },
]

const salaryChartData = computed(() => {
  return mockDB.departments.map(d => {
    const empIds = mockDB.employees.filter(e => e.departmentId === d.id).map(e => e.id)
    const slips = mockDB.payslips.filter(p => empIds.includes(p.employeeId) && p.periodId === 'per-02')
    const cost = slips.reduce((sum, curr) => sum + curr.netSalary, 0)
    const mockCosts: Record<string, number> = {
      'dept-001': 45000000,
      'dept-002': 18000000,
      'dept-003': 15000000,
      'dept-004': 10000000
    }
    const finalCost = cost > 0 ? cost : (mockCosts[d.id] || 8000000)
    return { name: d.name.replace('Phòng ', ''), cost: finalCost }
  })
})

onMounted(() => {
  nextTick(() => {
    // GSAP: Page header entrance
    gsap.from('.dashboard-header', { y: -20, opacity: 0, duration: 0.6, ease: 'power3.out' })

    // GSAP: Stats cards stagger entrance
    gsap.from('.stat-card-item', {
      y: 30, opacity: 0, duration: 0.5, stagger: 0.08, ease: 'power3.out', delay: 0.2
    })

    // GSAP: Chart sections stagger
    gsap.from('.chart-section', {
      y: 40, opacity: 0, duration: 0.6, stagger: 0.12, ease: 'power3.out', delay: 0.6
    })

    // GSAP: Bottom sections
    gsap.from('.bottom-section', {
      y: 40, opacity: 0, duration: 0.6, stagger: 0.15, ease: 'power3.out', delay: 0.9
    })

    // GSAP: Animate chart bars from 0 width
    gsap.from('.chart-bar-animated', {
      width: 0, duration: 1, stagger: 0.1, ease: 'power2.out', delay: 1
    })

    // GSAP: Animate attendance bars from 0 height
    gsap.from('.attendance-bar', {
      height: 0, duration: 0.8, stagger: 0.1, ease: 'power2.out', delay: 1
    })
  })
})
</script>

<template>
  <div class="space-y-8">
    
    <!-- Title & Navigation Bar -->
    <div class="dashboard-header flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-black text-slate-900 dark:text-slate-50 tracking-tight">Tổng quan dự án</h1>
        <p class="text-xs text-slate-500 dark:text-slate-400 mt-1">
          Báo cáo thống kê tổng hợp và tình trạng kết nối các phân hệ microservices.
        </p>
      </div>
      <button 
        @click="triggerSystemPing"
        class="btn-primary"
      >
        <Zap :size="14" />
        <span>Phát tin hiệu Ping (System Event)</span>
      </button>
    </div>

    <!-- Quick Stats Grid (6 items) -->
    <div class="grid gap-5 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
      <!-- 1. Total staff -->
      <div class="stat-card stat-card-item p-5 flex flex-col justify-between">
        <div class="flex justify-between items-start">
          <span class="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-wider">Tổng nhân viên</span>
          <div class="icon-gradient brand size-9">
            <Users :size="16" />
          </div>
        </div>
        <div class="mt-4">
          <div class="text-2xl font-extrabold text-slate-900 dark:text-slate-50">{{ totalEmployees }}</div>
          <div class="text-[10px] text-slate-400 font-semibold mt-1 flex items-center gap-0.5">
            <span class="text-emerald-500 font-bold flex items-center"><ArrowUpRight :size="10" /> +15%</span>
            <span>tháng này</span>
          </div>
        </div>
      </div>

      <!-- 2. New hires -->
      <div class="stat-card stat-card-item p-5 flex flex-col justify-between">
        <div class="flex justify-between items-start">
          <span class="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-wider">Nhân viên mới</span>
          <div class="icon-gradient violet size-9">
            <UserPlus :size="16" />
          </div>
        </div>
        <div class="mt-4">
          <div class="text-2xl font-extrabold text-slate-900 dark:text-slate-50">{{ newHires }}</div>
          <span class="text-[10px] text-slate-400 dark:text-slate-500 font-semibold mt-1 block">Trong quý hiện tại</span>
        </div>
      </div>

      <!-- 3. Attendance Rate -->
      <div class="stat-card stat-card-item p-5 flex flex-col justify-between">
        <div class="flex justify-between items-start">
          <span class="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-wider">Tỷ lệ đi làm</span>
          <div class="icon-gradient emerald size-9">
            <CalendarCheck :size="16" />
          </div>
        </div>
        <div class="mt-4">
          <div class="text-2xl font-extrabold text-slate-900 dark:text-slate-50">{{ attendanceRate }}</div>
          <span class="text-[10px] text-slate-400 dark:text-slate-500 font-semibold mt-1 block">Hôm nay: {{ todayCheckIns }} check-in</span>
        </div>
      </div>

      <!-- 4. Leave request pending -->
      <div class="stat-card stat-card-item p-5 flex flex-col justify-between">
        <div class="flex justify-between items-start">
          <span class="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-wider">Đơn nghỉ phép</span>
          <div class="icon-gradient amber size-9 relative">
            <span v-if="pendingLeaves > 0" class="absolute top-1 right-1 size-1.5 bg-amber-500 rounded-full animate-ping"></span>
            <FileCheck :size="16" />
          </div>
        </div>
        <div class="mt-4">
          <div class="text-2xl font-extrabold text-slate-900 dark:text-slate-50">{{ pendingLeaves }}</div>
          <span class="text-[10px] text-slate-400 dark:text-slate-500 font-semibold mt-1 block">Đơn chờ phê duyệt</span>
        </div>
      </div>

      <!-- 5. Total payroll net cost -->
      <div class="stat-card stat-card-item p-5 flex flex-col justify-between">
        <div class="flex justify-between items-start">
          <span class="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-wider">Tổng quỹ lương</span>
          <div class="icon-gradient rose size-9">
            <BadgeDollarSign :size="16" />
          </div>
        </div>
        <div class="mt-4">
          <div class="text-lg font-black text-slate-900 dark:text-slate-50 truncate leading-none mt-1">{{ formatVND(totalPayrollCost) }}</div>
          <span class="text-[9px] text-slate-400 dark:text-slate-500 font-semibold mt-1.5 block">Chi trả dự kiến</span>
        </div>
      </div>

      <!-- 6. Total departments -->
      <div class="stat-card stat-card-item p-5 flex flex-col justify-between">
        <div class="flex justify-between items-start">
          <span class="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-wider">Phòng ban</span>
          <div class="icon-gradient sky size-9">
            <Layers :size="16" />
          </div>
        </div>
        <div class="mt-4">
          <div class="text-2xl font-extrabold text-slate-900 dark:text-slate-50">{{ totalDepartments }}</div>
          <span class="text-[10px] text-slate-400 dark:text-slate-500 font-semibold mt-1 block">Phòng ban hoạt động</span>
        </div>
      </div>
    </div>

    <!-- Symmetrical Charts Section (3 columns) -->
    <div class="grid gap-6 md:grid-cols-3">
      <!-- 1. Biểu đồ nhân sự -->
      <div class="chart-section premium-card p-5 space-y-4">
        <div>
          <h2 class="section-title">Biểu đồ nhân sự</h2>
          <p class="section-subtitle">Tỷ lệ phân bổ nhân sự theo phòng ban</p>
        </div>
        
        <div class="space-y-3 pt-2">
          <div v-for="item in hrChartData" :key="item.name" class="space-y-1.5">
            <div class="flex justify-between text-[11px] font-semibold text-slate-700 dark:text-slate-350">
              <span class="truncate w-36">{{ item.name.replace('Phòng ', '') }}</span>
              <span>{{ item.count }} nhân sự</span>
            </div>
            <div class="chart-bar-track">
              <div class="chart-bar-fill chart-bar-animated" :style="{ width: `${item.pct}%` }"></div>
            </div>
          </div>
        </div>
      </div>

      <!-- 2. Biểu đồ chấm công -->
      <div class="chart-section premium-card p-5 space-y-4">
        <div>
          <h2 class="section-title">Biểu đồ chấm công</h2>
          <p class="section-subtitle">Tỷ lệ đi làm đúng giờ trong tuần qua (%)</p>
        </div>
        
        <div class="flex items-end justify-between gap-2 h-36 pt-4 px-2">
          <div 
            v-for="day in attendanceChartData" 
            :key="day.day" 
            class="flex-1 flex flex-col items-center gap-2"
          >
            <div class="w-full rounded-t-lg relative flex items-end justify-center h-24" style="background: var(--color-surface-alt);">
              <div 
                class="attendance-bar w-8/12 rounded-t-md relative group"
                style="background: var(--gradient-brand);"
                :style="{ height: `${day.rate}%` }"
              >
                <div class="absolute -top-6 left-1/2 -translate-x-1/2 bg-slate-900 text-white text-[9px] px-1.5 py-0.5 rounded opacity-0 group-hover:opacity-100 transition-opacity font-mono">
                  {{ day.rate }}%
                </div>
              </div>
            </div>
            <span class="text-[10px] font-bold text-slate-400 dark:text-slate-500">{{ day.day }}</span>
          </div>
        </div>
      </div>

      <!-- 3. Biểu đồ lương -->
      <div class="chart-section premium-card p-5 space-y-4">
        <div>
          <h2 class="section-title">Biểu đồ lương</h2>
          <p class="section-subtitle">Tổng ngân sách chi trả Net của phòng ban</p>
        </div>
        
        <div class="space-y-3 pt-2">
          <div v-for="item in salaryChartData" :key="item.name" class="space-y-1.5">
            <div class="flex justify-between text-[11px] font-semibold text-slate-700 dark:text-slate-350">
              <span class="truncate">{{ item.name }}</span>
              <span class="font-mono font-bold">{{ formatVND(item.cost).replace(' ₫', '') }}</span>
            </div>
            <div class="chart-bar-track">
              <div class="chart-bar-fill accent chart-bar-animated" :style="{ width: `${(item.cost / 50000000) * 100}%` }"></div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Bottom: Microservices Map & Recent Logs Summary -->
    <div class="grid gap-6 lg:grid-cols-12">
      <!-- Service map details (7 cols) -->
      <section class="bottom-section lg:col-span-7 premium-card overflow-hidden">
        <div class="px-6 py-4 flex items-center justify-between" style="border-bottom: 1px solid var(--color-border); background: var(--color-surface-alt);">
          <div class="flex items-center gap-2">
            <Network :size="15" class="text-indigo-600 dark:text-indigo-400" />
            <h2 class="section-title">Kiến trúc kết nối Microservices</h2>
          </div>
          <span class="inline-flex items-center gap-1.5 text-[10px] font-bold text-indigo-600 dark:text-indigo-400">
            <span class="h-1.5 w-1.5 rounded-full bg-indigo-500 animate-pulse"></span>
            Synced
          </span>
        </div>
        
        <div class="p-5">
          <table class="premium-table">
            <thead>
              <tr>
                <th>Microservice</th>
                <th>SQL Database Schema</th>
                <th class="text-right">Cổng Localhost</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="service in serviceRows" :key="service.name">
                <td class="font-bold" style="color: var(--color-text-primary);">{{ service.name }}</td>
                <td class="font-mono text-[10.5px]" style="color: var(--color-text-muted);">{{ service.database }}</td>
                <td class="font-mono text-[10.5px] text-right">localhost:{{ service.port }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- System Audit logs summary (5 cols) -->
      <section class="bottom-section lg:col-span-5 premium-card overflow-hidden">
        <div class="px-6 py-4 flex items-center gap-2" style="border-bottom: 1px solid var(--color-border); background: var(--color-surface-alt);">
          <History :size="15" class="text-slate-500" />
          <h2 class="section-title">Nhật ký hoạt động hệ thống</h2>
        </div>
        <div class="p-5">
          <div class="flow-root">
            <ul class="-mb-8">
              <li v-for="(log, logIdx) in mockDB.auditLogs.slice(0, 4)" :key="log.id">
                <div class="relative pb-8">
                  <span v-if="logIdx !== mockDB.auditLogs.slice(0, 4).length - 1" class="absolute top-4 left-4 -ml-px h-full w-0.5 bg-gradient-to-b from-indigo-200 to-transparent dark:from-indigo-900" aria-hidden="true"></span>
                  <div class="relative flex space-x-3">
                    <div>
                      <span class="h-8 w-8 rounded-full flex items-center justify-center ring-4 ring-white dark:ring-slate-900"
                        :class="log.service === 'HR Core' ? 'bg-indigo-50 dark:bg-indigo-950/30 text-indigo-600' : log.service === 'Attendance' ? 'bg-sky-50 dark:bg-sky-950/30 text-sky-600' : 'bg-violet-50 dark:bg-violet-950/30 text-violet-600'"
                      >
                        <Network :size="13" />
                      </span>
                    </div>
                    <div class="flex-1 min-w-0 pt-1.5 flex justify-between space-x-4">
                      <div>
                        <p class="text-xs text-slate-800 dark:text-slate-200 font-bold">
                          {{ log.details }}
                          <span class="text-slate-400 dark:text-slate-500 font-normal">bởi {{ log.userFullName }}</span>
                        </p>
                      </div>
                      <div class="text-right text-[9px] whitespace-nowrap text-slate-400 font-mono">
                        {{ new Date(log.timestamp).toLocaleTimeString() }}
                      </div>
                    </div>
                  </div>
                </div>
              </li>
            </ul>
          </div>
        </div>
      </section>
    </div>

  </div>
</template>
