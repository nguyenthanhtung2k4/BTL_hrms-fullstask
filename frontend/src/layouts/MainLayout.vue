<script setup lang="ts">
import {
  LayoutDashboard, Users, Building2, Briefcase, FileText,
  CalendarCheck, Clock, Calendar, ClipboardList, UmbrellaOff,
  BadgeDollarSign, ScrollText, Settings2, PiggyBank, Wallet,
  BarChart3, LogOut, Menu, Network,
  Sun, Moon, Monitor, ChevronDown, ShieldCheck, UserCircle, Bell
} from '@lucide/vue'
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { RouterLink, RouterView, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '../stores/auth'
import { useToastStore } from '../stores/toast'
import { useTheme, type ThemeMode } from '../composables/useTheme'
import { useLocale, type LocaleCode } from '../composables/useLocale'
import AppToast from '../components/ui/AppToast.vue'

import { useNotificationStore } from '../stores/notification'

const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()
const notificationStore = useNotificationStore()
const { t } = useI18n()
const { themeMode, setTheme } = useTheme()
const { currentLocale, setLocale } = useLocale()

const mobileOpen = ref(false)
const langDropdownOpen = ref(false)
const themeDropdownOpen = ref(false)



// ── Notifications polling
onMounted(() => {
  if (auth.isAuthenticated) {
    notificationStore.startPolling()
  }
})

onUnmounted(() => {
  notificationStore.stopPolling()
})

// Close dropdowns when clicking outside
function closeDropdowns() {
  langDropdownOpen.value = false
  themeDropdownOpen.value = false
}

// ── Menu groups (hiển thị theo role)
const menuGroups = computed(() => {
  const groups = []

  // Dashboard — all
  groups.push({
    label: '',
    items: [
      { to: '/', name: 'dashboard', label: t('nav.dashboard'), icon: LayoutDashboard, exact: true },
    ],
  })

  // HR Core — Admin, HR, Manager
  const hrItems: any[] = []
  hrItems.push({ to: '/hr/departments', name: 'hr-departments', label: t('nav.departments'), icon: Building2 })

  if (auth.isHR) {
    hrItems.push({ to: '/hr/positions', name: 'hr-positions', label: t('nav.positions'), icon: Briefcase })
  }
  if (auth.isManager) {
    hrItems.push({ to: '/hr/employees', name: 'hr-employees', label: t('nav.employees'), icon: Users })
  }
  if (auth.isHR) {
    hrItems.push({ to: '/hr/contracts', name: 'hr-contracts', label: t('nav.contracts'), icon: FileText })
  }
  groups.push({ label: t('nav.hr'), items: hrItems })

  // Attendance — all logged in
  const attItems: any[] = []
  if (auth.isHR) {
    attItems.push({ to: '/attendance/shifts', name: 'attendance-shifts', label: t('nav.shifts'), icon: Clock })
  }
  attItems.push({
    to: '/attendance/work-schedules',
    name: 'attendance-work-schedules',
    label: auth.isManager ? t('nav.workSchedules') : t('nav.myWorkSchedule'),
    icon: Calendar,
  })
  if (auth.isManager) {
    attItems.push({ to: '/attendance/records', name: 'attendance-records', label: t('nav.attendanceRecords'), icon: ClipboardList })
  }
  attItems.push({ to: '/attendance/checkin', name: 'attendance-checkin', label: t('nav.checkin'), icon: CalendarCheck })
  attItems.push({ to: '/attendance/my-attendance', name: 'attendance-my-attendance', label: t('nav.myAttendance'), icon: ClipboardList })
  attItems.push({
    to: '/attendance/leaves',
    name: 'attendance-leaves',
    label: auth.isManager ? t('nav.leaves') : t('nav.myLeaves'),
    icon: UmbrellaOff,
  })
  attItems.push({
    to: '/attendance/timesheets',
    name: 'attendance-timesheets',
    label: auth.isManager ? t('nav.timesheets') : t('nav.myTimesheets'),
    icon: ScrollText,
  })
  groups.push({ label: t('nav.attendance'), items: attItems })

  // Payroll — Admin/PayrollStaff & Employee
  const payItems: any[] = []
  if (auth.isPayrollStaff) {
    payItems.push(
      { to: '/payroll/periods', name: 'payroll-periods', label: t('nav.periods'), icon: BadgeDollarSign },
      { to: '/payroll/rules', name: 'payroll-rules', label: t('nav.rules'), icon: Settings2 },
    )
  }
  if (auth.hasAnyRole(['Admin', 'PayrollStaff', 'HR', 'Manager'])) {
    payItems.push(
      { to: '/payroll/allowances', name: 'payroll-allowances', label: t('nav.allowances'), icon: PiggyBank },
      { to: '/payroll/deductions', name: 'payroll-deductions', label: t('nav.deductions'), icon: Wallet },
    )
  }
  if (auth.isPayrollStaff) {
    payItems.push({ to: '/payroll/payslips', name: 'payroll-payslips', label: t('nav.allPayslips'), icon: ScrollText })
  }
  payItems.push({ to: '/payroll/my-payslip', name: 'payroll-my-payslip', label: t('nav.myPayslip'), icon: BadgeDollarSign })
  if (auth.hasAnyRole(['Admin', 'HR', 'PayrollStaff', 'Manager'])) {
    payItems.push({ to: '/payroll/reports', name: 'payroll-reports', label: t('nav.reports'), icon: BarChart3 })
  }
  groups.push({ label: t('nav.payroll'), items: payItems })

  // Admin / HR — Quản trị tài khoản
  if (auth.isAdmin || auth.isHR) {
    groups.push({
      label: t('user.title'),
      items: [
        { to: '/admin/users', name: 'admin-users', label: t('nav.userManagement'), icon: ShieldCheck },
      ],
    })
  }

  // Profile — tất cả (nằm cuối menu)
  groups.push({
    label: '',
    items: [
      { to: '/profile', name: 'profile', label: t('nav.profile'), icon: UserCircle },
    ],
  })

  return groups
})

// Theme helpers
const themeOptions: { mode: ThemeMode; label: string; icon: any }[] = [
  { mode: 'light', label: t('theme.light'), icon: Sun },
  { mode: 'dark', label: t('theme.dark'), icon: Moon },
  { mode: 'system', label: t('theme.system'), icon: Monitor },
]

const currentThemeIcon = computed(() => {
  if (themeMode.value === 'dark') return Moon
  if (themeMode.value === 'light') return Sun
  return Monitor
})

// Language helpers
const langOptions: { code: LocaleCode; label: string; flag: string }[] = [
  { code: 'vi', label: 'Tiếng Việt', flag: '🇻🇳' },
  { code: 'en', label: 'English', flag: '🇬🇧' },
]

const currentLangFlag = computed(() => {
  return langOptions.find(l => l.code === currentLocale.value)?.flag ?? '🇻🇳'
})

async function logout() {
  await auth.logout()
  toast.success(t('auth.loggedOut'))
  router.push({ name: 'login' })
}
</script>

<template>
  <div class="min-h-screen transition-colors duration-200" style="background-color: var(--bg-page); color: var(--text-primary);">
    <!-- Mobile overlay -->
    <div v-if="mobileOpen" class="fixed inset-0 z-20 bg-black/50 lg:hidden" @click="mobileOpen = false" />
    <!-- Global click-away for dropdowns -->
    <div v-if="langDropdownOpen || themeDropdownOpen" class="fixed inset-0 z-30" @click="closeDropdowns" />

    <!-- Sidebar -->
    <aside
      :class="[
        'fixed inset-y-0 left-0 z-40 w-64 flex flex-col transition-transform duration-300',
        'lg:translate-x-0',
        mobileOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0',
      ]"
      style="background-color: var(--sidebar-bg); border-right: 1px solid var(--sidebar-border);"
    >
      <!-- Logo -->
      <div class="flex h-16 flex-shrink-0 items-center gap-3 px-5" style="border-bottom: 1px solid var(--sidebar-border);">
        <div class="grid h-9 w-9 place-items-center rounded-xl shadow-sm" style="background: var(--color-primary); color: var(--text-inverse);">
          <Network :size="18" />
        </div>
        <div>
          <div class="text-sm font-bold" style="color: var(--text-primary);">HRMS</div>
          <div class="text-xs" style="color: var(--text-secondary);">Microservices</div>
        </div>
      </div>

      <!-- Nav -->
      <nav class="flex-1 overflow-y-auto px-3 py-4 space-y-5">
        <div v-for="group in menuGroups" :key="group.label">
          <div
            v-if="group.label"
            class="mb-1.5 px-2 text-[10px] font-semibold uppercase tracking-widest"
            style="color: var(--text-tertiary);"
          >
            {{ group.label }}
          </div>
          <div class="space-y-0.5">
            <RouterLink
              v-for="item in group.items"
              :key="item.to"
              :to="item.to"
              :exact="item.exact"
              class="flex h-9 items-center gap-2.5 rounded-lg px-3 text-sm font-medium transition-all duration-150"
              style="color: var(--text-secondary);"
              active-class="sidebar-active"
              @click="mobileOpen = false"
            >
              <component :is="item.icon" :size="16" class="flex-shrink-0" />
              <span>{{ item.label }}</span>
            </RouterLink>
          </div>
        </div>
      </nav>

      <!-- User info & logout -->
      <div class="p-3" style="border-top: 1px solid var(--sidebar-border);">
        <div class="flex items-center gap-2 rounded-xl p-2.5" style="background: var(--bg-subtle);">
          <div class="grid h-8 w-8 flex-shrink-0 place-items-center rounded-full text-sm font-bold" style="background: var(--color-primary-light); color: var(--color-primary-text);">
            {{ auth.displayName?.charAt(0)?.toUpperCase() ?? 'U' }}
          </div>
          <div class="min-w-0 flex-1">
            <div class="truncate text-xs font-semibold" style="color: var(--text-primary);">{{ auth.displayName }}</div>
            <div class="flex flex-wrap gap-1 mt-0.5">
              <span
                v-for="role in auth.roles"
                :key="role"
                class="rounded-full px-1.5 py-px text-[10px] font-medium"
                style="background: var(--color-primary-light); color: var(--color-primary-text);"
              >
                {{ role }}
              </span>
            </div>
          </div>
          <button
            class="rounded-lg p-1.5 transition-colors"
            :title="t('auth.logout')"
            style="color: var(--text-tertiary);"
            @click="logout"
          >
            <LogOut :size="15" />
          </button>
        </div>
      </div>
    </aside>

    <!-- Main -->
    <div class="lg:pl-64 flex flex-col min-h-screen">
      <!-- Topbar -->
      <header
        class="sticky top-0 z-30 flex h-14 items-center gap-3 px-4 backdrop-blur-sm lg:px-6"
        style="border-bottom: 1px solid var(--border); background-color: color-mix(in srgb, var(--bg-surface) 95%, transparent);"
      >
        <!-- Mobile menu button -->
        <button
          class="rounded-lg p-1.5 transition-colors lg:hidden"
          style="color: var(--text-secondary);"
          @click="mobileOpen = true"
        >
          <Menu :size="20" />
        </button>

        <!-- Title -->
        <div class="flex-1 min-w-0">
          <div class="text-sm font-semibold truncate" style="color: var(--text-primary);">
            Quản lý Nhân sự & Chấm công
          </div>
          <div class="text-xs hidden sm:block" style="color: var(--text-tertiary);">BTL Fullstack — Đề tài 03</div>
        </div>

        <!-- Right controls -->
        <div class="flex items-center gap-1.5">

          <!-- Notification Bell -->
          <RouterLink
            to="/notifications"
            class="relative flex items-center justify-center h-9 w-9 rounded-lg transition-colors hover:bg-gray-100 dark:hover:bg-gray-800"
            style="color: var(--text-secondary);"
            title="Thông báo"
          >
            <Bell :size="18" />
            <span
              v-if="notificationStore.unreadCount > 0"
              class="absolute top-1 right-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white shadow-sm"
            >
              {{ notificationStore.unreadCount }}
            </span>
          </RouterLink>

          <!-- Language Switcher -->
          <div class="relative">
            <button
              class="flex items-center gap-1.5 h-9 rounded-lg px-2.5 text-sm font-medium transition-colors"
              :title="t('language.switch')"
              style="color: var(--text-secondary);"
              @click.stop="langDropdownOpen = !langDropdownOpen; themeDropdownOpen = false"
            >
              <span class="text-base leading-none">{{ currentLangFlag }}</span>
              <span class="text-xs font-semibold hidden sm:block">{{ currentLocale.toUpperCase() }}</span>
              <ChevronDown :size="12" class="opacity-50" />
            </button>

            <!-- Language Dropdown -->
            <div
              v-if="langDropdownOpen"
              class="absolute right-0 top-full mt-1.5 z-50 w-44 rounded-xl shadow-lg overflow-hidden"
              style="background: var(--bg-surface); border: 1px solid var(--border); box-shadow: var(--shadow-lg);"
            >
              <button
                v-for="lang in langOptions"
                :key="lang.code"
                class="w-full flex items-center gap-2.5 px-3 py-2.5 text-sm text-left transition-colors"
                :style="{
                  background: currentLocale === lang.code ? 'var(--color-primary-light)' : 'transparent',
                  color: currentLocale === lang.code ? 'var(--color-primary-text)' : 'var(--text-primary)',
                  fontWeight: currentLocale === lang.code ? '600' : '400'
                }"
                @click="setLocale(lang.code); langDropdownOpen = false"
              >
                <span class="text-base">{{ lang.flag }}</span>
                <span>{{ lang.label }}</span>
                <span v-if="currentLocale === lang.code" class="ml-auto text-xs">✓</span>
              </button>
            </div>
          </div>

          <!-- Theme Toggle -->
          <div class="relative">
            <button
              class="flex items-center justify-center h-9 w-9 rounded-lg transition-colors"
              :title="t('theme.toggle')"
              style="color: var(--text-secondary); background: transparent;"
              @click.stop="themeDropdownOpen = !themeDropdownOpen; langDropdownOpen = false"
            >
              <component :is="currentThemeIcon" :size="18" />
            </button>

            <!-- Theme Dropdown -->
            <div
              v-if="themeDropdownOpen"
              class="absolute right-0 top-full mt-1.5 z-50 w-48 rounded-xl shadow-lg overflow-hidden"
              style="background: var(--bg-surface); border: 1px solid var(--border); box-shadow: var(--shadow-lg);"
            >
              <button
                v-for="opt in themeOptions"
                :key="opt.mode"
                class="w-full flex items-center gap-2.5 px-3 py-2.5 text-sm text-left transition-colors"
                :style="{
                  background: themeMode === opt.mode ? 'var(--color-primary-light)' : 'transparent',
                  color: themeMode === opt.mode ? 'var(--color-primary-text)' : 'var(--text-primary)',
                  fontWeight: themeMode === opt.mode ? '600' : '400'
                }"
                @click="setTheme(opt.mode); themeDropdownOpen = false"
              >
                <component :is="opt.icon" :size="16" />
                <span>{{ opt.label }}</span>
                <span v-if="themeMode === opt.mode" class="ml-auto text-xs">✓</span>
              </button>
            </div>
          </div>
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

<style scoped>
/* Active sidebar link */
:deep(.sidebar-active) {
  background-color: var(--sidebar-active) !important;
  color: var(--sidebar-active-text) !important;
  font-weight: 600;
}
:deep(.router-link-active:not(.sidebar-active)):hover {
  background-color: var(--sidebar-hover);
}
nav a:hover {
  background-color: var(--sidebar-hover);
  color: var(--sidebar-active-text);
}
</style>
