<script setup lang="ts">
import {
  BadgeDollarSign,
  CalendarCheck,
  LayoutDashboard,
  LogOut,
  Network,
  Users,
  Radio,
  Menu,
  X,
  Bell,
  Sun,
  Moon,
  ChevronDown,
  ChevronUp,
  UserCheck
} from '@lucide/vue'
import { RouterLink, RouterView, useRouter, useRoute } from 'vue-router'
import { ref, onMounted, computed, nextTick } from 'vue'
import { useAuthStore, type UserRole } from '../stores/auth'
import { mockDB, writeAuditLog } from '../services/mockData'
import gsap from 'gsap'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()

const isMobileMenuOpen = ref(false)
const isNotificationOpen = ref(false)
const isProfileOpen = ref(false)

// Dark mode state
const isDark = ref(false)

// Collapsible sidebar menu sections
const isHrOpen = ref(true)
const isAttendanceOpen = ref(true)
const isPayrollOpen = ref(true)

// Mock Notifications
const notifications = ref([
  { id: 1, text: 'Nguyễn Văn A đã check-in ca Hành chính hôm nay.', time: 'Vừa xong', unread: true },
  { id: 2, text: 'Đơn xin nghỉ phép mới của Hoàng Văn E đang chờ duyệt.', time: '10 phút trước', unread: true },
  { id: 3, text: 'Kỳ lương Tháng 05/2026 đã được khóa sổ bởi kế toán.', time: '2 giờ trước', unread: false },
])

const unreadCount = computed(() => notifications.value.filter(n => n.unread).length)

function clearNotifications() {
  notifications.value.forEach(n => n.unread = false)
}

function toggleDarkMode() {
  isDark.value = !isDark.value
  if (isDark.value) {
    document.documentElement.classList.add('dark')
    localStorage.setItem('theme', 'dark')
  } else {
    document.documentElement.classList.remove('dark')
    localStorage.setItem('theme', 'light')
  }
  writeAuditLog('HR Core', 'TOGGLE_DARK_MODE', `Đổi giao diện sang Chế độ ${isDark.value ? 'Tối' : 'Sáng'}`)
}

const rolesList: { value: UserRole; label: string; desc: string }[] = [
  { value: 'Admin', label: 'Quản trị viên (Admin)', desc: 'Toàn quyền hệ thống' },
  { value: 'HR', label: 'Nhân sự (HR Core)', desc: 'Quản lý nhân viên, hợp đồng, lịch làm' },
  { value: 'Manager', label: 'Trưởng phòng (Manager)', desc: 'Duyệt công, duyệt nghỉ phép' },
  { value: 'Employee', label: 'Nhân viên (Employee)', desc: 'Checkin/out, gửi đơn phép cá nhân' },
  { value: 'PayrollStaff', label: 'Kế toán (Payroll)', desc: 'Tính lương, thưởng phạt, báo cáo' },
]

function handleRoleChange(newRole: UserRole) {
  auth.switchRole(newRole)
  localStorage.setItem('active_user_name', auth.displayName)
  writeAuditLog('HR Core', 'SWITCH_ROLE', `Chuyển vai trò giả lập sang ${newRole}`)
  isProfileOpen.value = false
}

function toggleMode() {
  mockDB.systemMode = mockDB.systemMode === 'Mock' ? 'Live' : 'Mock'
}

function logout() {
  auth.logout()
  router.push({ name: 'login' })
}

// Compute Breadcrumbs
const breadcrumbs = computed(() => {
  const list = [{ label: 'Trang chủ', to: '/' }]
  if (route.path.startsWith('/hr')) {
    list.push({ label: 'HR Core Service', to: '/hr' })
    const tab = route.query.tab
    if (tab === 'org') list.push({ label: 'Phòng ban & Chức vụ', to: '/hr?tab=org' })
    else if (tab === 'contracts') list.push({ label: 'Hợp đồng lao động', to: '/hr?tab=contracts' })
    else if (tab === 'roles') list.push({ label: 'Người dùng & Phân quyền', to: '/hr?tab=roles' })
    else list.push({ label: 'Nhân viên & Hồ sơ', to: '/hr?tab=employees' })
  } else if (route.path.startsWith('/attendance')) {
    list.push({ label: 'Attendance Service', to: '/attendance' })
    const tab = route.query.tab
    if (tab === 'schedule') list.push({ label: 'Lịch & Bảng công tháng', to: '/attendance?tab=schedule' })
    else if (tab === 'requests') list.push({ label: 'Đơn nghỉ phép & OT', to: '/attendance?tab=requests' })
    else if (tab === 'approval') list.push({ label: 'Phê duyệt yêu cầu', to: '/attendance?tab=approval' })
    else if (tab === 'shifts') list.push({ label: 'Quản lý ca làm', to: '/attendance?tab=shifts' })
    else list.push({ label: 'Chấm công hôm nay', to: '/attendance?tab=checkin' })
  } else if (route.path.startsWith('/payroll')) {
    list.push({ label: 'Payroll & Report Service', to: '/payroll' })
    const tab = route.query.tab
    if (tab === 'adjustments') list.push({ label: 'Thưởng & Khấu trừ', to: '/payroll?tab=adjustments' })
    else if (tab === 'reports') list.push({ label: 'Báo cáo thống kê', to: '/payroll?tab=reports' })
    else list.push({ label: 'Tính & Bảng lương', to: '/payroll?tab=calc' })
  }
  return list
})

onMounted(() => {
  const savedTheme = localStorage.getItem('theme')
  if (savedTheme === 'dark') {
    isDark.value = true
    document.documentElement.classList.add('dark')
  } else {
    isDark.value = false
    document.documentElement.classList.remove('dark')
  }

  // GSAP: Sidebar entrance
  nextTick(() => {
    gsap.from('.sidebar-logo', { y: -20, opacity: 0, duration: 0.6, ease: 'power3.out' })
    gsap.from('.sidebar-nav-item', {
      x: -30, opacity: 0, duration: 0.5, stagger: 0.06, ease: 'power3.out', delay: 0.2
    })
  })
})
</script>

<template>
  <div class="min-h-screen text-slate-900 dark:text-slate-100 font-sans antialiased flex flex-col transition-colors duration-300" style="background: var(--color-bg); background-image: var(--gradient-mesh); background-attachment: fixed;">
    <!-- Top System Switch Mode Header -->
    <div 
      class="text-white text-xs font-semibold py-1.5 px-6 flex items-center justify-between transition-all duration-300 border-b border-white/5 no-print"
      :class="mockDB.systemMode === 'Mock' ? 'bg-gradient-to-r from-indigo-600/90 to-violet-600/90 backdrop-blur' : 'bg-gradient-to-r from-emerald-600/90 to-teal-600/90 backdrop-blur'"
    >
      <div class="flex items-center gap-2">
        <Radio :size="14" class="animate-pulse" />
        <span v-if="mockDB.systemMode === 'Mock'">Chế độ giả lập (Mock Mode) — Dữ liệu lưu cục bộ LocalStorage & RabbitMQ Terminal Logs</span>
        <span v-else>Kết nối trực tiếp API Gateway (Live Gateway) — localhost:5000</span>
      </div>
      <button 
        @click="toggleMode"
        class="bg-white/15 hover:bg-white/25 text-white px-3 py-0.5 rounded-md text-[10px] uppercase font-bold tracking-wider transition-all cursor-pointer backdrop-blur-sm border border-white/10"
      >
        Đổi chế độ kết nối
      </button>
    </div>

    <!-- Layout container -->
    <div class="flex flex-1 relative">
      <!-- Desktop Left Sidebar -->
      <aside
        class="fixed inset-y-0 left-0 top-8 hidden w-72 glass-sidebar px-5 py-6 lg:flex flex-col justify-between z-20 transition-colors duration-300"
      >
        <div class="space-y-5 overflow-y-auto pr-1">
          <!-- Logo Brand -->
          <div class="sidebar-logo flex h-14 items-center gap-3 px-2">
            <div class="grid size-11 place-items-center rounded-2xl text-white shadow-lg" style="background: var(--gradient-brand); box-shadow: var(--shadow-glow-brand);">
              <Network :size="22" class="stroke-[2.5]" />
            </div>
            <div>
              <div class="text-[10px] font-bold uppercase tracking-widest text-slate-400 dark:text-slate-500">Phần mềm</div>
              <div class="text-base font-black bg-gradient-to-r from-indigo-600 to-violet-600 dark:from-indigo-400 dark:to-violet-400 bg-clip-text text-transparent">HRM & Attendance</div>
            </div>
          </div>

          <!-- Sidebar Multi-level menu -->
          <nav class="space-y-3">
            <!-- 0. General overview -->
            <div class="sidebar-nav-item">
              <RouterLink
                to="/"
                class="sidebar-link"
                active-class="active"
              >
                <LayoutDashboard :size="17" />
                <span>Tổng quan (Dashboard)</span>
              </RouterLink>
            </div>

            <!-- 1. HR Core Service Section -->
            <div class="sidebar-nav-item" v-if="['Admin', 'HR'].includes(auth.activeRole)">
              <button 
                @click="isHrOpen = !isHrOpen"
                class="flex w-full h-9 items-center justify-between rounded-lg px-3.5 text-[10px] font-extrabold uppercase tracking-wider text-slate-400 dark:text-slate-500 hover:bg-indigo-500/5 dark:hover:bg-indigo-500/10 transition-colors cursor-pointer"
              >
                <span class="flex items-center gap-2">
                  <Users :size="14" />
                  <span>HR Core Service</span>
                </span>
                <ChevronUp v-if="isHrOpen" :size="13" />
                <ChevronDown v-else :size="13" />
              </button>
              
              <div v-show="isHrOpen" class="pl-4 border-l-2 border-indigo-500/15 dark:border-indigo-400/15 ml-5 space-y-0.5 mt-1">
                <RouterLink to="/hr?tab=employees" class="sidebar-sub-link">Nhân viên & Hồ sơ</RouterLink>
                <RouterLink to="/hr?tab=org" class="sidebar-sub-link">Phòng ban & Chức vụ</RouterLink>
                <RouterLink to="/hr?tab=contracts" class="sidebar-sub-link">Hợp đồng lao động</RouterLink>
                <RouterLink to="/hr?tab=roles" class="sidebar-sub-link">Người dùng & Phân quyền</RouterLink>
              </div>
            </div>

            <!-- 2. Attendance Service Section -->
            <div class="sidebar-nav-item" v-if="['Admin', 'HR', 'Manager', 'Employee'].includes(auth.activeRole)">
              <button 
                @click="isAttendanceOpen = !isAttendanceOpen"
                class="flex w-full h-9 items-center justify-between rounded-lg px-3.5 text-[10px] font-extrabold uppercase tracking-wider text-slate-400 dark:text-slate-500 hover:bg-indigo-500/5 dark:hover:bg-indigo-500/10 transition-colors cursor-pointer"
              >
                <span class="flex items-center gap-2">
                  <CalendarCheck :size="14" />
                  <span>Chấm công</span>
                </span>
                <ChevronUp v-if="isAttendanceOpen" :size="13" />
                <ChevronDown v-else :size="13" />
              </button>

              <div v-show="isAttendanceOpen" class="pl-4 border-l-2 border-indigo-500/15 dark:border-indigo-400/15 ml-5 space-y-0.5 mt-1">
                <RouterLink to="/attendance?tab=checkin" class="sidebar-sub-link">Chấm công & Lịch sử</RouterLink>
                <RouterLink to="/attendance?tab=schedule" class="sidebar-sub-link">Bảng công tháng</RouterLink>
                <RouterLink to="/attendance?tab=requests" class="sidebar-sub-link">Đăng ký Nghỉ / OT</RouterLink>
                <RouterLink v-if="['Admin', 'Manager', 'HR'].includes(auth.activeRole)" to="/attendance?tab=approval" class="sidebar-sub-link">Duyệt Nghỉ / OT</RouterLink>
                <RouterLink v-if="['Admin', 'HR'].includes(auth.activeRole)" to="/attendance?tab=shifts" class="sidebar-sub-link">Quản lý ca làm việc</RouterLink>
              </div>
            </div>

            <!-- 3. Payroll & Report Service Section -->
            <div class="sidebar-nav-item" v-if="['Admin', 'PayrollStaff', 'Employee'].includes(auth.activeRole)">
              <button 
                @click="isPayrollOpen = !isPayrollOpen"
                class="flex w-full h-9 items-center justify-between rounded-lg px-3.5 text-[10px] font-extrabold uppercase tracking-wider text-slate-400 dark:text-slate-500 hover:bg-indigo-500/5 dark:hover:bg-indigo-500/10 transition-colors cursor-pointer"
              >
                <span class="flex items-center gap-2">
                  <BadgeDollarSign :size="14" />
                  <span>Lương & Báo cáo</span>
                </span>
                <ChevronUp v-if="isPayrollOpen" :size="13" />
                <ChevronDown v-else :size="13" />
              </button>

              <div v-show="isPayrollOpen" class="pl-4 border-l-2 border-indigo-500/15 dark:border-indigo-400/15 ml-5 space-y-0.5 mt-1">
                <template v-if="auth.activeRole !== 'Employee'">
                  <RouterLink to="/payroll?tab=calc" class="sidebar-sub-link">Tính & Bảng lương</RouterLink>
                  <RouterLink to="/payroll?tab=adjustments" class="sidebar-sub-link">Thưởng & Khấu trừ</RouterLink>
                  <RouterLink to="/payroll?tab=reports" class="sidebar-sub-link">Báo cáo & Thống kê</RouterLink>
                </template>
                <template v-else>
                  <RouterLink to="/payroll?tab=calc" class="sidebar-sub-link">Phiếu lương cá nhân</RouterLink>
                </template>
              </div>
            </div>
          </nav>
        </div>

        <!-- Sidebar Footer & Account Switch Mode -->
        <div class="space-y-4 pt-4 border-t border-indigo-500/10 dark:border-indigo-400/10 no-print">
          <!-- Role selector warning -->
          <div class="text-[10px] text-slate-400 dark:text-slate-500 leading-relaxed p-3 rounded-xl" style="background: var(--color-surface-alt); border: 1px solid var(--color-border);">
            <strong>Lưu ý:</strong> Chuyển nhanh vai trò đăng nhập ở Avatar góc Topbar để test phân quyền.
          </div>
        </div>
      </aside>

      <!-- Mobile Top Header -->
      <header class="lg:hidden fixed top-8 inset-x-0 h-16 glass-topbar px-4 flex items-center justify-between z-30 transition-colors">
        <div class="flex items-center gap-3">
          <button @click="isMobileMenuOpen = !isMobileMenuOpen" class="p-1.5 rounded-lg hover:bg-indigo-500/10 text-slate-700 dark:text-slate-300 cursor-pointer">
            <Menu v-if="!isMobileMenuOpen" :size="22" />
            <X v-else :size="22" />
          </button>
          <div class="flex items-center gap-2.5">
            <div class="size-8 rounded-lg text-white grid place-items-center font-bold" style="background: var(--gradient-brand);">
              <Network :size="16" />
            </div>
            <span class="text-sm font-black bg-gradient-to-r from-indigo-600 to-violet-600 dark:from-indigo-400 dark:to-violet-400 bg-clip-text text-transparent">HRM</span>
          </div>
        </div>
        <button 
          @click="logout" 
          class="size-8 rounded-lg border border-slate-200 dark:border-slate-800 flex items-center justify-center text-slate-500 hover:bg-slate-50 dark:hover:bg-slate-800 cursor-pointer"
        >
          <LogOut :size="16" />
        </button>
      </header>

      <!-- Mobile overlay -->
      <div v-if="isMobileMenuOpen" class="lg:hidden fixed inset-0 top-[calc(2rem+4rem)] bg-slate-900/30 backdrop-blur-sm z-10" @click="isMobileMenuOpen = false"></div>
      
      <!-- Mobile Drawer Sidebar -->
      <aside 
        class="lg:hidden fixed inset-y-0 left-0 top-[calc(2rem+4rem)] w-72 glass-sidebar px-4 py-5 flex flex-col justify-between z-20 transition-transform duration-300 shadow-xl"
        :class="isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full'"
      >
        <nav class="space-y-3">
          <RouterLink to="/" @click="isMobileMenuOpen = false" class="sidebar-link" active-class="active">
            <LayoutDashboard :size="16" />
            <span>Tổng quan</span>
          </RouterLink>
          <RouterLink to="/hr?tab=employees" @click="isMobileMenuOpen = false" class="sidebar-link" active-class="active">
            <Users :size="16" />
            <span>HR Core Service</span>
          </RouterLink>
          <RouterLink to="/attendance?tab=checkin" @click="isMobileMenuOpen = false" class="sidebar-link" active-class="active">
            <CalendarCheck :size="16" />
            <span>Attendance Service</span>
          </RouterLink>
          <RouterLink to="/payroll?tab=calc" @click="isMobileMenuOpen = false" class="sidebar-link" active-class="active">
            <BadgeDollarSign :size="16" />
            <span>Lương & Báo cáo</span>
          </RouterLink>
        </nav>
      </aside>

      <!-- Main container (Topbar + Viewport) -->
      <div class="flex-1 lg:pl-72 pt-8 lg:pt-0">
        
        <!-- Top Navbar -->
        <header
          class="sticky top-11 lg:top-8 z-10 flex h-16 items-center justify-between glass-topbar px-6 lg:px-8 no-print"
        >
          <!-- Left: Breadcrumbs -->
          <div class="flex items-center gap-2 text-xs font-semibold text-slate-500 dark:text-slate-400">
            <template v-for="(crumb, idx) in breadcrumbs" :key="crumb.to">
              <span v-if="idx > 0" class="text-indigo-300 dark:text-indigo-700">/</span>
              <RouterLink 
                :to="crumb.to" 
                class="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors"
                :class="idx === breadcrumbs.length - 1 ? 'text-slate-900 dark:text-slate-100 font-bold' : ''"
              >
                {{ crumb.label }}
              </RouterLink>
            </template>
          </div>

          <!-- Right: Widgets (Dark Mode, Notifications, Profile) -->
          <div class="flex items-center gap-2">
            <!-- 1. Dark Mode switch -->
            <button 
              @click="toggleDarkMode" 
              class="p-2 rounded-xl text-slate-500 hover:text-indigo-600 dark:text-slate-400 dark:hover:text-indigo-400 hover:bg-indigo-500/8 transition-all cursor-pointer"
              title="Chuyển chế độ Sáng/Tối"
            >
              <Sun v-if="isDark" :size="18" />
              <Moon v-else :size="18" />
            </button>

            <!-- 2. Notification Center -->
            <div class="relative">
              <button 
                @click="isNotificationOpen = !isNotificationOpen; isProfileOpen = false" 
                class="p-2 rounded-xl text-slate-500 hover:text-indigo-600 dark:text-slate-400 dark:hover:text-indigo-400 hover:bg-indigo-500/8 transition-all relative cursor-pointer"
              >
                <Bell :size="18" />
                <span v-if="unreadCount > 0" class="absolute top-1 right-1.5 size-4 rounded-full text-[9px] font-bold grid place-items-center text-white" style="background: var(--gradient-warm);">
                  {{ unreadCount }}
                </span>
              </button>

              <!-- Notifications dropdown -->
              <div 
                v-if="isNotificationOpen" 
                class="absolute right-0 mt-2.5 w-80 dropdown-premium z-30 text-xs"
              >
                <div class="px-4 py-3 border-b flex justify-between items-center" style="border-color: var(--color-border); background: var(--color-surface-alt);">
                  <span class="font-bold text-slate-800 dark:text-slate-200">Thông báo mới</span>
                  <button @click="clearNotifications" class="text-[10px] font-bold text-indigo-600 dark:text-indigo-400 hover:underline cursor-pointer">Đánh dấu đọc</button>
                </div>
                <div class="divide-y max-h-[260px] overflow-y-auto" style="border-color: var(--color-border-subtle);">
                  <div 
                    v-for="notif in notifications" 
                    :key="notif.id"
                    class="p-3.5 hover:bg-indigo-500/4 dark:hover:bg-indigo-500/8 flex items-start gap-2.5 transition-colors"
                    :class="notif.unread ? 'bg-indigo-500/3' : ''"
                  >
                    <div class="size-2 bg-indigo-500 rounded-full mt-1.5 flex-shrink-0" :class="notif.unread ? '' : 'opacity-0'"></div>
                    <div class="space-y-0.5">
                      <p class="text-slate-700 dark:text-slate-300 font-semibold leading-normal">{{ notif.text }}</p>
                      <span class="text-[9px] text-slate-400 block">{{ notif.time }}</span>
                    </div>
                  </div>
                </div>
                <div class="p-2 border-t text-center" style="border-color: var(--color-border); background: var(--color-surface-alt);">
                  <span class="text-[10px] text-slate-400 font-semibold">Tất cả thông báo</span>
                </div>
              </div>
            </div>

            <!-- Vertical Divider -->
            <span class="h-5 w-px bg-slate-200 dark:bg-slate-800"></span>

            <!-- 3. Profile Dropdown & Role Swapper -->
            <div class="relative">
              <button 
                @click="isProfileOpen = !isProfileOpen; isNotificationOpen = false" 
                class="flex items-center gap-2.5 p-1.5 rounded-xl hover:bg-indigo-500/6 dark:hover:bg-indigo-500/10 text-left transition-all cursor-pointer"
              >
                <div class="size-9 rounded-xl flex items-center justify-center font-bold text-white text-xs shadow-md" style="background: var(--gradient-brand); box-shadow: var(--shadow-glow-brand);">
                  {{ auth.displayName.charAt(0) }}
                </div>
                <div class="hidden md:block">
                  <div class="text-xs font-bold text-slate-900 dark:text-slate-100 leading-none">{{ auth.displayName }}</div>
                  <div class="text-[9px] text-indigo-500 dark:text-indigo-400 font-mono font-semibold mt-0.5">{{ auth.activeRole }}</div>
                </div>
                <ChevronDown :size="14" class="text-slate-400" />
              </button>

              <!-- Profile dropmenu details -->
              <div 
                v-if="isProfileOpen" 
                class="absolute right-0 mt-2.5 w-64 dropdown-premium z-30 text-xs"
              >
                <!-- Role selector dropdown inside profile -->
                <div class="p-4 border-b space-y-2" style="border-color: var(--color-border); background: var(--color-surface-alt);">
                  <span class="text-[9px] font-bold text-slate-400 uppercase tracking-wider block">Giả lập Vai trò</span>
                  <div class="space-y-1">
                    <button 
                      v-for="r in rolesList" 
                      :key="r.value"
                      @click="handleRoleChange(r.value)"
                      class="w-full text-left px-2.5 py-1.5 rounded-lg text-[11px] font-semibold flex items-center justify-between transition-all cursor-pointer"
                      :class="auth.activeRole === r.value 
                        ? 'bg-indigo-500/10 dark:bg-indigo-500/15 text-indigo-600 dark:text-indigo-400 font-bold shadow-sm' 
                        : 'text-slate-600 dark:text-slate-400 hover:bg-indigo-500/5'"
                    >
                      <span>{{ r.label }}</span>
                      <UserCheck v-if="auth.activeRole === r.value" :size="12" />
                    </button>
                  </div>
                </div>

                <div class="p-2">
                  <button 
                    @click="logout"
                    class="w-full flex items-center gap-2 px-3 py-2.5 rounded-xl hover:bg-red-500/8 dark:hover:bg-red-500/12 text-red-600 dark:text-red-400 font-bold text-left cursor-pointer transition-colors"
                  >
                    <LogOut :size="14" />
                    <span>Đăng xuất</span>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </header>

        <!-- Main Workspace -->
        <main class="px-6 py-8 lg:px-8 max-w-7xl mx-auto w-full">
          <RouterView />
        </main>
      </div>
    </div>
  </div>
</template>
