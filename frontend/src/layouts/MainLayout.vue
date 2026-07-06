<script setup lang="ts">
import {
  LayoutDashboard, Users, Building2, Briefcase, FileText,
  CalendarCheck, Clock, Calendar, ClipboardList, UmbrellaOff,
  BadgeDollarSign, ScrollText, Settings2, PiggyBank, Wallet,
  BarChart3, LogOut, Menu,
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
import gsap from 'gsap'

const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()
const notificationStore = useNotificationStore()
const { t } = useI18n()
const { themeMode, setTheme } = useTheme()
const { currentLocale, setLocale } = useLocale()

const isSidebarCollapsed = ref(localStorage.getItem('sidebar_collapsed') === 'true')
const mobileOpen = ref(false)
const langDropdownOpen = ref(false)
const themeDropdownOpen = ref(false)

function handleMenuClick() {
  if (window.innerWidth < 1024) {
    mobileOpen.value = !mobileOpen.value
  } else {
    isSidebarCollapsed.value = !isSidebarCollapsed.value
    localStorage.setItem('sidebar_collapsed', String(isSidebarCollapsed.value))
  }
}

// GSAP Page Transition Callbacks
function onBeforeEnter(el: any) {
  gsap.set(el, {
    opacity: 0,
    y: 12,
    scale: 0.99
  })
}

function onEnter(el: any, done: () => void) {
  gsap.to(el, {
    opacity: 1,
    y: 0,
    scale: 1,
    duration: 0.4,
    ease: 'power2.out',
    onComplete: done
  })
}

function onLeave(el: any, done: () => void) {
  gsap.to(el, {
    opacity: 0,
    y: -12,
    scale: 0.99,
    duration: 0.25,
    ease: 'power2.in',
    onComplete: done
  })
}



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

  // HR Core — Admin, HR, Manager, PayrollStaff
  const hrItems: any[] = []
  hrItems.push({ to: '/hr/departments', name: 'hr-departments', label: t('nav.departments'), icon: Building2 })

  if (auth.isHR) {
    hrItems.push({ to: '/hr/positions', name: 'hr-positions', label: t('nav.positions'), icon: Briefcase })
  }
  if (auth.isManager || auth.isPayrollStaff) {
    hrItems.push({ to: '/hr/employees', name: 'hr-employees', label: t('nav.employees'), icon: Users })
  }
  if (auth.isHR || auth.isPayrollStaff) {
    hrItems.push({ to: '/hr/contracts', name: 'hr-contracts', label: t('nav.contracts'), icon: FileText })
  }
  groups.push({ label: t('nav.hr'), items: hrItems })

  // Attendance — all logged in
  const attItems: any[] = []
  if (auth.isHR || auth.isManager || auth.isPayrollStaff) {
    attItems.push({ to: '/attendance/shifts', name: 'attendance-shifts', label: t('nav.shifts'), icon: Clock })
  }
  attItems.push({
    to: '/attendance/work-schedules',
    name: 'attendance-work-schedules',
    label: auth.isManager ? t('nav.workSchedules') : t('nav.myWorkSchedule'),
    icon: Calendar,
  })
  if (auth.isManager || auth.isPayrollStaff) {
    attItems.push({ to: '/attendance/records', name: 'attendance-records', label: t('nav.attendanceRecords'), icon: ClipboardList })
  }
  if (!auth.isAdmin) {
    attItems.push({ to: '/attendance/checkin', name: 'attendance-checkin', label: t('nav.checkin'), icon: CalendarCheck })
    attItems.push({ to: '/attendance/my-attendance', name: 'attendance-my-attendance', label: t('nav.myAttendance'), icon: ClipboardList })
  }
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
  if (auth.hasAnyRole(['Admin', 'PayrollStaff', 'HR'])) {
    payItems.push(
      { to: '/payroll/allowances', name: 'payroll-allowances', label: t('nav.allowances'), icon: PiggyBank },
      { to: '/payroll/deductions', name: 'payroll-deductions', label: t('nav.deductions'), icon: Wallet },
    )
  }
  if (auth.isPayrollStaff || auth.isHR || auth.isManager) {
    payItems.push({ to: '/payroll/payslips', name: 'payroll-payslips', label: t('nav.allPayslips'), icon: ScrollText })
  }
  if (!auth.isAdmin) {
    payItems.push({ to: '/payroll/my-payslip', name: 'payroll-my-payslip', label: t('nav.myPayslip'), icon: BadgeDollarSign })
  }
  if (auth.hasAnyRole(['Admin', 'HR', 'PayrollStaff', 'Manager'])) {
    payItems.push({ to: '/payroll/reports', name: 'payroll-reports', label: t('nav.reports'), icon: BarChart3 })
  }
  groups.push({ label: t('nav.payroll'), items: payItems })

  // Admin — Quản trị tài khoản
  if (auth.isAdmin) {
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
        isSidebarCollapsed ? 'lg:-translate-x-full' : 'lg:translate-x-0',
        mobileOpen ? 'translate-x-0' : '-translate-x-full',
      ]"
      style="background-color: var(--sidebar-bg); border-right: 1px solid var(--sidebar-border);"
    >
      <!-- Logo -->
      <div class="flex h-16 flex-shrink-0 items-center gap-3 px-5" style="border-bottom: 1px solid var(--sidebar-border);">
        <div class="grid h-9 w-9 place-items-center rounded-xl shadow-sm" style="background: var(--color-primary); color: var(--text-inverse);">
          <svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <circle cx="11" cy="11" r="7" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
            <path d="M11 7V11H14" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            <circle cx="18" cy="18" r="4.5" fill="var(--color-primary)" stroke="currentColor" stroke-width="1.5"/>
            <path d="M16.5 18L17.5 19L19.5 17" stroke="currentColor" stroke-width="1.2" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </div>
        <div>
          <div class="text-sm font-bold" style="color: var(--text-primary);">Chấm Công Số</div>
          <div class="text-xs" style="color: var(--text-secondary);">Cổng thông tin nội bộ</div>
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
            <div class="space-y-1">
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
          <div class="grid h-8 w-8 flex-shrink-0 place-items-center rounded-full text-sm font-bold overflow-hidden" style="background: var(--color-primary-light); color: var(--color-primary-text);">
            <img v-if="auth.avatarUrl" :src="auth.avatarUrl" alt="Avatar" class="h-full w-full object-cover" />
            <span v-else>{{ auth.displayName?.charAt(0)?.toUpperCase() ?? 'U' }}</span>
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
    <div
      :class="[
        'flex flex-col min-h-screen transition-all duration-300',
        isSidebarCollapsed ? 'lg:pl-0' : 'lg:pl-64'
      ]"
    >
      <!-- Topbar -->
      <header
        class="sticky top-0 z-30 flex h-14 items-center gap-3 px-4 backdrop-blur-sm lg:px-6"
        style="border-bottom: 1px solid var(--border); background-color: color-mix(in srgb, var(--bg-surface) 95%, transparent);"
      >
        <!-- Toggle menu button -->
        <button
          class="toggle-sidebar-btn"
          :title="isSidebarCollapsed ? 'Mở rộng menu' : 'Thu gọn menu'"
          @click="handleMenuClick"
        >
          <Menu :size="18" />
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
              <span class="flex items-center">
                <template v-if="currentLocale === 'vi'">
                  <svg class="w-5 h-3.5 rounded-sm object-cover shadow-sm border border-black/10" viewBox="0 0 30 20" xmlns="http://www.w3.org/2000/svg">
                    <rect width="30" height="20" fill="#da251d"/>
                    <polygon points="15,4 16.18,7.62 20,7.62 16.91,9.88 18.09,13.5 15,11.25 11.91,13.5 13.09,9.88 10,7.62 13.82,7.62" fill="#ffff00"/>
                  </svg>
                </template>
                <template v-else-if="currentLocale === 'en'">
                  <svg class="w-5 h-3.5 rounded-sm object-cover shadow-sm border border-black/10" viewBox="0 0 20 14" xmlns="http://www.w3.org/2000/svg">
                    <rect width="20" height="14" fill="#bb133e"/>
                    <path d="M0,1h20M0,3h20M0,5h20M0,7h20M0,9h20M0,11h20M0,13h20" stroke="#fff" stroke-width="1"/>
                    <rect width="8" height="8" fill="#002147"/>
                    <polygon points="1.5,1.8 1.8,2.8 2.8,2.8 2.0,3.4 2.3,4.4 1.5,3.8 0.7,4.4 1.0,3.4 0.2,2.8 1.2,2.8" fill="#fff"/>
                    <polygon points="4.0,1.8 4.3,2.8 5.3,2.8 4.5,3.4 4.8,4.4 4.0,3.8 3.2,4.4 3.5,3.4 2.7,2.8 3.7,2.8" fill="#fff"/>
                    <polygon points="6.5,1.8 6.8,2.8 7.8,2.8 7.0,3.4 7.3,4.4 6.5,3.8 5.7,4.4 6.0,3.4 5.2,2.8 6.2,2.8" fill="#fff"/>
                    <polygon points="1.5,4.3 1.8,5.3 2.8,5.3 2.0,5.9 2.3,6.9 1.5,6.3 0.7,6.9 1.0,5.9 0.2,5.3 1.2,5.3" fill="#fff"/>
                    <polygon points="4.0,4.3 4.3,5.3 5.3,5.3 4.5,5.9 4.8,6.9 4.0,6.3 3.2,6.9 3.5,5.9 2.7,5.3 3.7,5.3" fill="#fff"/>
                    <polygon points="6.5,4.3 6.8,5.3 7.8,5.3 7.0,5.9 7.3,6.9 6.5,6.3 5.7,6.9 6.0,5.9 5.2,5.3 6.2,5.3" fill="#fff"/>
                  </svg>
                </template>
              </span>
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
                <span class="flex items-center">
                  <template v-if="lang.code === 'vi'">
                    <svg class="w-5 h-3.5 rounded-sm object-cover shadow-sm border border-black/10" viewBox="0 0 30 20" xmlns="http://www.w3.org/2000/svg">
                      <rect width="30" height="20" fill="#da251d"/>
                      <polygon points="15,4 16.18,7.62 20,7.62 16.91,9.88 18.09,13.5 15,11.25 11.91,13.5 13.09,9.88 10,7.62 13.82,7.62" fill="#ffff00"/>
                    </svg>
                  </template>
                  <template v-else-if="lang.code === 'en'">
                    <svg class="w-5 h-3.5 rounded-sm object-cover shadow-sm border border-black/10" viewBox="0 0 20 14" xmlns="http://www.w3.org/2000/svg">
                      <rect width="20" height="14" fill="#bb133e"/>
                      <path d="M0,1h20M0,3h20M0,5h20M0,7h20M0,9h20M0,11h20M0,13h20" stroke="#fff" stroke-width="1"/>
                      <rect width="8" height="8" fill="#002147"/>
                      <polygon points="1.5,1.8 1.8,2.8 2.8,2.8 2.0,3.4 2.3,4.4 1.5,3.8 0.7,4.4 1.0,3.4 0.2,2.8 1.2,2.8" fill="#fff"/>
                      <polygon points="4.0,1.8 4.3,2.8 5.3,2.8 4.5,3.4 4.8,4.4 4.0,3.8 3.2,4.4 3.5,3.4 2.7,2.8 3.7,2.8" fill="#fff"/>
                      <polygon points="6.5,1.8 6.8,2.8 7.8,2.8 7.0,3.4 7.3,4.4 6.5,3.8 5.7,4.4 6.0,3.4 5.2,2.8 6.2,2.8" fill="#fff"/>
                      <polygon points="1.5,4.3 1.8,5.3 2.8,5.3 2.0,5.9 2.3,6.9 1.5,6.3 0.7,6.9 1.0,5.9 0.2,5.3 1.2,5.3" fill="#fff"/>
                      <polygon points="4.0,4.3 4.3,5.3 5.3,5.3 4.5,5.9 4.8,6.9 4.0,6.3 3.2,6.9 3.5,5.9 2.7,5.3 3.7,5.3" fill="#fff"/>
                      <polygon points="6.5,4.3 6.8,5.3 7.8,5.3 7.0,5.9 7.3,6.9 6.5,6.3 5.7,6.9 6.0,5.9 5.2,5.3 6.2,5.3" fill="#fff"/>
                    </svg>
                  </template>
                </span>
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
        <router-view v-slot="{ Component }">
          <transition
            :css="false"
            @before-enter="onBeforeEnter"
            @enter="onEnter"
            @leave="onLeave"
            mode="out-in"
          >
            <component :is="Component" />
          </transition>
        </router-view>
      </main>
    </div>

    <!-- Toast notifications (global) -->
    <AppToast />
  </div>
</template>

<style scoped>
.toggle-sidebar-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2.25rem;
  height: 2.25rem;
  border-radius: var(--radius-md);
  color: var(--text-secondary);
  background-color: transparent;
  transition: all var(--transition-fast);
  cursor: pointer;
  border: none;
  outline: none;
}
.toggle-sidebar-btn:hover {
  background-color: var(--bg-subtle);
  color: var(--text-primary);
}

/* Active sidebar link */
:deep(.sidebar-active) {
  background-color: var(--sidebar-active) !important;
  color: var(--sidebar-active-text) !important;
  font-weight: 600;
}
:deep(.router-link-active:not(.sidebar-active)):hover {
  background-color: var(--sidebar-hover);
}
nav a {
  transition: all var(--transition-fast);
}
nav a:hover {
  background-color: var(--sidebar-hover);
  color: var(--sidebar-active-text);
  transform: translateX(4px);
}
</style>
