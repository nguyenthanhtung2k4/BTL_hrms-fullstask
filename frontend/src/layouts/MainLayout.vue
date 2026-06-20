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
  <div class="min-h-screen w-full flex bg-[#F8FAFC] text-slate-800 font-sans antialiased">
    <!-- Mobile overlay -->
    <div
      v-if="mobileOpen"
      class="fixed inset-0 z-20 bg-black/40 lg:hidden"
      @click="mobileOpen = false"
    />

    <!-- Sidebar -->
    <aside
      :class="[
        'fixed inset-y-0 left-0 z-30 w-64 flex flex-col border-r border-slate-200 bg-white shadow-[0_12px_32px_rgba(15,23,42,0.06)] transition-transform duration-300 pb-8',
        'lg:translate-x-0',
        mobileOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0',
      ]"
      aria-label="Sidebar"
    >
      <!-- Logo -->
      <div class="flex h-16 flex-shrink-0 items-center gap-3 border-b border-slate-200 px-5">
        <div class="grid h-10 w-10 place-items-center rounded-xl bg-gradient-to-br from-blue-600 to-blue-700 text-white shadow-md">
          <Network :size="20" />
        </div>
        <div>
          <div class="text-base font-extrabold text-slate-900">HRMS</div>
          <div class="text-xs text-slate-500">Enterprise</div>
        </div>
      </div>

      <!-- Nav -->
      <nav class="flex-1 overflow-y-auto px-3 py-4 space-y-4 sidebar-scrollbar">
        <div v-for="group in menuGroups" :key="group.label">
          <div
            v-if="group.label"
            class="mb-2.5 px-3 text-xs font-bold uppercase tracking-widest text-slate-600 opacity-70"
          >
            {{ group.label }}
          </div>
            <div class="space-y-1">
            <RouterLink
              v-for="item in group.items"
              :key="item.to"
              :to="item.to"
              :exact="item.exact"
              class="nav-item group flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-slate-600 transition-all duration-200 transform"
              active-class="bg-blue-100 text-blue-700 font-semibold shadow-sm"
              @click="mobileOpen = false"
            >
              <component :is="item.icon" :size="20" class="flex-shrink-0 text-slate-400 group-hover:text-blue-600 transition-colors" />
              <span>{{ item.label }}</span>
            </RouterLink>
          </div>
        </div>
      </nav>

      <!-- User info & logout -->
      <div class="border-t border-slate-200 p-4">
        <div class="flex items-center gap-2 rounded-xl border border-slate-200 bg-gradient-to-br from-slate-50 to-white p-3 shadow-sm">
          <div class="grid h-9 w-9 flex-shrink-0 place-items-center rounded-full bg-blue-600 text-white shadow-md">
            <ShieldCheck :size="18" />
          </div>
          <div class="min-w-0 flex-1">
            <div class="truncate text-sm font-bold text-slate-900">{{ auth.displayName }}</div>
            <div class="flex flex-wrap gap-1 mt-0.5">
              <span v-for="role in auth.roles" :key="role" class="rounded bg-white px-2 py-0.5 text-xs font-semibold text-blue-700 ring-1 ring-blue-200">{{ role }}</span>
            </div>
          </div>
          <button class="rounded-lg p-1.5 text-slate-500 hover:bg-red-50 hover:text-red-600 transition-all" title="Đăng xuất" @click="logout">
            <LogOut :size="18" />
          </button>
        </div>
      </div>
    </aside>

    <!-- Main -->
    <div class="flex-1 lg:pl-64 flex flex-col min-h-screen w-full">
      <!-- Topbar -->
      <header class="sticky top-0 z-10 h-20 flex items-center border-b border-slate-200 bg-white/95 px-4 sm:px-6 lg:px-8 shadow-[0_1px_0_rgba(15,23,42,0.04)] backdrop-blur">
        <button
          class="mr-4 rounded-lg p-1.5 text-slate-500 hover:bg-slate-100 lg:hidden"
          @click="mobileOpen = true"
        >
          <Menu :size="20" />
        </button>
        <div class="flex-1">
          <div class="text-xs text-slate-500">BTL Fullstack — Đề tài 03</div>
          <div class="text-sm font-semibold text-slate-900">Hệ thống quản lý nhân sự & chấm công</div>
        </div>
      </header>

      <!-- Page content -->
      <main class="flex-1 flex flex-col min-w-0 overflow-y-auto px-4 py-8 sm:px-6 lg:px-8">
        <div class="mx-auto flex w-full max-w-7xl flex-col">
          <RouterView />
        </div>
      </main>
    </div>

    <!-- Toast notifications (global) -->
    <AppToast />
  </div>
</template>

<style>
/* Custom scrollbar for sidebar to blend with blue sidebar */
.sidebar-scrollbar {
  scrollbar-width: thin;
  scrollbar-color: rgba(37,99,235,0.24) transparent;
}
.sidebar-scrollbar::-webkit-scrollbar {
  width: 8px;
}
.sidebar-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.sidebar-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(37,99,235,0.24);
  border-radius: 9999px;
  border: 2px solid transparent;
  background-clip: padding-box;
}
</style>
<style>
/* Hover/active polish for sidebar nav items to match card styling */
.nav-item {
  /* ensure block-level hit area */
  display: flex;
  align-items: center;
}
.nav-item:hover {
  background: rgba(59, 130, 246, 0.06);
  box-shadow: 0 6px 18px rgba(15, 23, 42, 0.06);
  transform: translateY(-1px);
}
.nav-item:focus-visible {
  outline: none;
  box-shadow: 0 0 0 3px rgba(37,99,235,0.14);
}
</style>
