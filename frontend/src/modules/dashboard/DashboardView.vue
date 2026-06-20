<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { employeeService } from '../../services/employee.service'
import { departmentService } from '../../services/department.service'
import { payrollPeriodService } from '../../services/payrollPeriod.service'
import { leaveService } from '../../services/leave.service'
import { useAuthStore } from '../../stores/auth'
import type { Employee } from '../../types/hr.types'
import type { PayrollPeriod } from '../../types/payroll.types'
import type { LeaveRequest } from '../../types/attendance.types'
import StatCard from '../../components/layout/StatCard.vue'
import AppBadge from '../../components/ui/AppBadge.vue'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()

const loading = ref(true)
const employees = ref<Employee[]>([])
const departments = ref<{ id: string; name: string }[]>([])
const periods = ref<PayrollPeriod[]>([])
const pendingLeaves = ref<LeaveRequest[]>([])

const totalEmployees = computed(() => employees.value.length)
const activeEmployees = computed(() => employees.value.filter((e) => e.status === 'Active').length)
const totalDepts = computed(() => departments.value.length)
const openPeriods = computed(() => periods.value.filter((p) => p.status !== 'Closed').length)
const greeting = computed(() => {
  const h = new Date().getHours()
  if (h < 12) return 'Chào buổi sáng'
  if (h < 18) return 'Chào buổi chiều'
  return 'Chào buổi tối'
})

const deptStats = computed(() => {
  const map: Record<string, number> = {}
  employees.value.forEach((e) => {
    if (e.departmentName) map[e.departmentName] = (map[e.departmentName] ?? 0) + 1
  })
  return Object.entries(map)
    .map(([name, count]) => ({ name, count }))
    .sort((a, b) => b.count - a.count)
    .slice(0, 5)
})

const deptBarClass = (name: string) => {
  const palette = [
    'bg-blue-600',
    'bg-emerald-500',
    'bg-violet-500',
    'bg-amber-500',
    'bg-cyan-500',
  ]
  let hash = 0
  for (let i = 0; i < name.length; i += 1) {
    hash = (hash * 31 + name.charCodeAt(i)) % palette.length
  }
  return palette[hash]
}

async function load() {
  try {
    const promises: Promise<any>[] = []

    if (auth.isManager) {
      promises.push(
        employeeService.getAll().then((r) => (employees.value = r)),
        departmentService.getAll().then((r) => (departments.value = r)),
      )
    }
    if (auth.isPayrollStaff) {
      promises.push(payrollPeriodService.getAll().then((r) => (periods.value = r)))
    }
    if (auth.isManager) {
      promises.push(leaveService.getAll({ status: 'Pending' }).then((r) => (pendingLeaves.value = r)))
    }

    await Promise.all(promises)
  } catch {
    // silent
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="mx-auto w-full max-w-7xl space-y-6">
    <!-- Greeting -->
    <div class="mb-6">
      <h1 class="text-2xl font-bold text-slate-900">{{ greeting }}, {{ auth.displayName }} 👋</h1>
      <p class="mt-0.5 text-sm text-slate-500">
        {{ new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) }}
      </p>
    </div>

    <!-- Stat cards (chỉ cho Manager) -->
    <div v-if="auth.isManager" class="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
      <StatCard class="w-full" title="Tổng nhân viên" :value="totalEmployees" subtitle="Trong hệ thống" color="emerald" :loading="loading">
        <template #icon>
          <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" /></svg>
        </template>
      </StatCard>
      <StatCard class="w-full" title="Đang làm việc" :value="activeEmployees" subtitle="Trạng thái Active" color="blue" :loading="loading">
        <template #icon>
          <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
        </template>
      </StatCard>
      <StatCard class="w-full" title="Phòng ban" :value="totalDepts" subtitle="Đang hoạt động" color="violet" :loading="loading">
        <template #icon>
          <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" /></svg>
        </template>
      </StatCard>
      <StatCard class="w-full" title="Chờ duyệt nghỉ" :value="pendingLeaves.length" subtitle="Đơn nghỉ phép" color="amber" :loading="loading">
        <template #icon>
          <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
        </template>
      </StatCard>
    </div>

    <!-- Payroll stat (PayrollStaff) -->
    <div v-if="auth.isPayrollStaff && !auth.isManager" class="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-3">
      <StatCard title="Kỳ lương đang mở" :value="openPeriods" subtitle="Cần xử lý" color="amber" :loading="loading" />
    </div>

    <!-- Employee welcome card -->
    <div v-if="auth.isEmployee && !auth.isManager && !auth.isPayrollStaff" class="mb-6">
      <div class="rounded-2xl border border-emerald-200 bg-emerald-50 p-6">
        <h2 class="text-base font-semibold text-emerald-900">Chào mừng đến HRMS! 🏢</h2>
        <p class="mt-1 text-sm text-emerald-700">Bạn có thể sử dụng các chức năng bên trái để check-in, xem nghỉ phép và phiếu lương.</p>
        <div class="mt-4 flex gap-3">
          <button class="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-700" @click="router.push('/attendance/checkin')">Check-in ngay →</button>
          <button class="rounded-lg border border-emerald-300 bg-white px-4 py-2 text-sm font-medium text-emerald-700 hover:bg-emerald-50" @click="router.push('/payroll/my-payslip')">Phiếu lương của tôi</button>
        </div>
      </div>
    </div>

    <!-- Bottom section for managers -->
    <div v-if="auth.isManager" class="grid grid-cols-1 items-stretch gap-6 lg:grid-cols-5">
      <!-- Department breakdown -->
      <div class="lg:col-span-3 rounded-2xl border border-slate-200 bg-white p-6 shadow-sm flex flex-col">
        <div class="mb-4 flex items-center justify-between">
          <h2 class="text-sm font-semibold text-slate-900">Nhân viên theo phòng ban</h2>
          <button class="text-xs text-emerald-600 hover:underline" @click="router.push('/hr/employees')">Xem tất cả →</button>
        </div>
        <div class="flex-1">
          <div v-if="loading" class="space-y-2">
            <div v-for="n in 5" :key="n" class="h-6 animate-pulse rounded bg-slate-200" />
          </div>
          <div v-else class="space-y-3">
            <div
              v-for="dept in deptStats"
              :key="dept.name"
              class="space-y-2 w-full"
            >
              <div class="flex items-center justify-between gap-3 text-sm font-semibold text-slate-600">
                <span class="truncate">{{ dept.name }}</span>
                <span class="text-slate-800">{{ dept.count }}</span>
              </div>
              <div class="h-2 w-full overflow-hidden rounded-full bg-slate-100 border border-slate-200">
                <div
                  :class="['h-full rounded-full transition-all duration-500 ease-out shadow-inner', deptBarClass(dept.name)]"
                  :style="{ width: `${(dept.count / totalEmployees) * 100}%` }"
                />
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Pending leave requests -->
      <div class="lg:col-span-2 rounded-2xl border border-slate-200 bg-white p-6 shadow-sm flex flex-col">
        <div class="mb-4 flex items-center justify-between">
          <h2 class="text-sm font-semibold text-slate-900">Đơn nghỉ phép chờ duyệt</h2>
          <button class="text-xs text-emerald-600 hover:underline" @click="router.push('/attendance/leaves')">Xử lý →</button>
        </div>
        <div class="flex-1">
          <div v-if="loading" class="space-y-2">
            <div v-for="n in 3" :key="n" class="h-12 animate-pulse rounded bg-slate-200" />
          </div>
          <div v-else-if="pendingLeaves.length === 0" class="py-8 text-center text-sm text-slate-400">
            ✓ Không có đơn nào chờ duyệt
          </div>
          <div v-else class="space-y-3">
            <div
              v-for="lv in pendingLeaves.slice(0, 5)"
              :key="lv.id"
              class="flex items-center justify-between rounded-xl border border-slate-200 bg-white px-3 py-3 shadow-sm"
            >
              <div class="flex min-w-0 items-center gap-3">
                <div class="grid h-10 w-10 shrink-0 place-items-center rounded-full bg-blue-50 text-sm font-semibold text-blue-700 ring-1 ring-blue-100">
                  {{ lv.employeeName.split(' ').map((part) => part[0]).slice(0, 2).join('').toUpperCase() }}
                </div>
                <div class="min-w-0">
                  <div class="truncate text-sm font-medium text-slate-900">{{ lv.employeeName }}</div>
                  <div class="text-xs text-slate-500">{{ lv.leaveTypeName }} · {{ lv.totalDays }} ngày</div>
                </div>
              </div>
              <AppBadge status="Pending" />
            </div>
            <div v-if="pendingLeaves.length > 5" class="text-center text-xs text-slate-400">
              +{{ pendingLeaves.length - 5 }} đơn khác
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
