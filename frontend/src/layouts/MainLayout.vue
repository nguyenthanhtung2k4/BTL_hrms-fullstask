<script setup lang="ts">
import {
  LayoutDashboard,
  Users,
  Building2,
  Briefcase,
  FileText,
  CalendarCheck,
  Clock,
  Calendar,
  ClipboardList,
  UmbrellaOff,
  BadgeDollarSign,
  ScrollText,
  Settings2,
  PiggyBank,
  Wallet,
  BarChart3,
  LogOut,
  Menu,
  Network,
  ShieldCheck,
} from '@lucide/vue'
import { ref, computed } from 'vue'
import { RouterLink, RouterView, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useToastStore } from '../stores/toast'
import AppToast from '../components/ui/AppToast.vue'

const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()
const mobileOpen = ref(false)

// ── Menu groups (hiển thị theo role)
const menuGroups = computed(() => {
  const groups = []

  // Dashboard — all
  groups.push({
    label: '',
    items: [
      { to: '/', name: 'dashboard', label: 'Dashboard', icon: LayoutDashboard, exact: true },
    ],
  })

  // HR Core — Admin, HR, Manager
  if (auth.isManager) {
    const hrItems: any[] = []
    if (auth.isHR) {
      hrItems.push(
        { to: '/hr/departments', name: 'hr-departments', label: 'Phòng ban', icon: Building2 },
        { to: '/hr/positions', name: 'hr-positions', label: 'Chức vụ', icon: Briefcase },
      )
    }
    hrItems.push({ to: '/hr/employees', name: 'hr-employees', label: 'Nhân viên', icon: Users })
    if (auth.isHR) {
      hrItems.push({ to: '/hr/contracts', name: 'hr-contracts', label: 'Hợp đồng', icon: FileText })
    }
    groups.push({ label: 'Nhân sự', items: hrItems })
  }

  // Attendance — all logged in
  const attItems: any[] = []
  if (auth.isHR) {
    attItems.push({ to: '/attendance/shifts', name: 'attendance-shifts', label: 'Ca làm việc', icon: Clock })
  }
  if (auth.isManager) {
    attItems.push({ to: '/attendance/work-schedules', name: 'attendance-work-schedules', label: 'Lịch làm việc', icon: Calendar })
    attItems.push({ to: '/attendance/records', name: 'attendance-records', label: 'Chấm công (tất cả)', icon: ClipboardList })
  }
  attItems.push({ to: '/attendance/checkin', name: 'attendance-checkin', label: 'Check-in / Check-out', icon: CalendarCheck })
  attItems.push({ to: '/attendance/leaves', name: 'attendance-leaves', label: 'Nghỉ phép', icon: UmbrellaOff })
  attItems.push({ to: '/attendance/timesheets', name: 'attendance-timesheets', label: 'Bảng công', icon: ScrollText })
  groups.push({ label: 'Chấm công', items: attItems })

  // Payroll — Admin/PayrollStaff & Employee
  const payItems: any[] = []
  if (auth.isPayrollStaff) {
    payItems.push(
      { to: '/payroll/periods', name: 'payroll-periods', label: 'Kỳ lương', icon: BadgeDollarSign },
      { to: '/payroll/rules', name: 'payroll-rules', label: 'Quy tắc lương', icon: Settings2 },
      { to: '/payroll/allowances', name: 'payroll-allowances', label: 'Phụ cấp', icon: PiggyBank },
      { to: '/payroll/deductions', name: 'payroll-deductions', label: 'Khấu trừ', icon: Wallet },
      { to: '/payroll/payslips', name: 'payroll-payslips', label: 'Phiếu lương (tất cả)', icon: ScrollText },
    )
  }
  payItems.push({ to: '/payroll/my-payslip', name: 'payroll-my-payslip', label: 'Phiếu lương của tôi', icon: BadgeDollarSign })
  if (auth.hasAnyRole(['Admin', 'HR', 'PayrollStaff', 'Manager'])) {
    payItems.push({ to: '/payroll/reports', name: 'payroll-reports', label: 'Báo cáo', icon: BarChart3 })
  }
  groups.push({ label: 'Lương & Báo cáo', items: payItems })

  return groups
})

async function logout() {
  auth.logout()
  toast.success('Đã đăng xuất')
  router.push({ name: 'login' })
}
</script>

<template>
  <div class="min-h-screen bg-slate-50 text-slate-900">
    <!-- Mobile overlay -->
    <div
      v-if="mobileOpen"
      class="fixed inset-0 z-20 bg-black/40 lg:hidden"
      @click="mobileOpen = false"
    />

    <!-- Sidebar -->
    <aside
      :class="[
        'fixed inset-y-0 left-0 z-30 w-64 flex flex-col bg-white border-r border-slate-200 transition-transform duration-300',
        'lg:translate-x-0',
        mobileOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0',
      ]"
    >
      <!-- Logo -->
      <div class="flex h-16 flex-shrink-0 items-center gap-3 border-b border-slate-200 px-5">
        <div class="grid h-9 w-9 place-items-center rounded-lg bg-emerald-600 text-white shadow-sm">
          <Network :size="18" />
        </div>
        <div>
          <div class="text-sm font-bold text-slate-900">HRMS</div>
          <div class="text-xs text-slate-500">Microservices</div>
        </div>
      </div>

      <!-- Nav -->
      <nav class="flex-1 overflow-y-auto px-3 py-4 space-y-4">
        <div v-for="group in menuGroups" :key="group.label">
          <div
            v-if="group.label"
            class="mb-1.5 px-2 text-[10px] font-semibold uppercase tracking-widest text-slate-400"
          >
            {{ group.label }}
          </div>
          <div class="space-y-0.5">
            <RouterLink
              v-for="item in group.items"
              :key="item.to"
              :to="item.to"
              :exact="item.exact"
              class="flex h-9 items-center gap-3 rounded-lg px-3 text-sm font-medium text-slate-600 transition-colors hover:bg-emerald-50 hover:text-emerald-700"
              active-class="bg-emerald-50 text-emerald-700 font-semibold"
              @click="mobileOpen = false"
            >
              <component :is="item.icon" :size="17" class="flex-shrink-0" />
              <span>{{ item.label }}</span>
            </RouterLink>
          </div>
        </div>
      </nav>

      <!-- User info & logout -->
      <div class="border-t border-slate-200 p-3">
        <div class="flex items-center gap-2 rounded-lg bg-slate-50 p-3">
          <div class="grid h-8 w-8 flex-shrink-0 place-items-center rounded-full bg-emerald-100 text-emerald-700">
            <ShieldCheck :size="16" />
          </div>
          <div class="min-w-0 flex-1">
            <div class="truncate text-xs font-semibold text-slate-800">{{ auth.displayName }}</div>
            <div class="flex flex-wrap gap-1 mt-0.5">
              <span
                v-for="role in auth.roles"
                :key="role"
                class="rounded bg-emerald-100 px-1.5 text-[10px] font-medium text-emerald-700"
              >
                {{ role }}
              </span>
            </div>
          </div>
          <button
            class="rounded p-1.5 text-slate-400 hover:bg-white hover:text-red-500 transition-colors"
            title="Đăng xuất"
            @click="logout"
          >
            <LogOut :size="16" />
          </button>
        </div>
      </div>
    </aside>

    <!-- Main -->
    <div class="lg:pl-64 flex flex-col min-h-screen">
      <!-- Topbar -->
      <header class="sticky top-0 z-10 flex h-16 items-center border-b border-slate-200 bg-white/95 px-4 backdrop-blur lg:px-6">
        <button
          class="mr-4 rounded-lg p-1.5 text-slate-500 hover:bg-slate-100 lg:hidden"
          @click="mobileOpen = true"
        >
          <Menu :size="20" />
        </button>
        <div class="flex-1">
          <div class="text-xs text-slate-500">BTL Fullstack — Đề tài 03</div>
          <div class="text-sm font-semibold text-slate-900">Quản lý Nhân sự & Chấm công</div>
        </div>
      </header>

      <!-- Page content -->
      <main class="flex-1 px-4 py-6 lg:px-8">
        <RouterView />
      </main>
    </div>

    <!-- Toast notifications (global) -->
    <AppToast />
  </div>
</template>
