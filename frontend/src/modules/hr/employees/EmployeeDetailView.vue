<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { employeeService } from '../../../services/employee.service'
import { contractService } from '../../../services/contract.service'
import { userService } from '../../../services/user.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { Employee, Contract } from '../../../types/hr.types'
import type { UserAccount } from '../../../types/user.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import GrantAccountModal from './GrantAccountModal.vue'
import ResetPasswordModal from './ResetPasswordModal.vue'
import EditRolesModal from './EditRolesModal.vue'
import { KeyRound, Shield, UserPlus, Lock, Unlock, RefreshCw } from '@lucide/vue'

const route = useRoute()
const router = useRouter()
const toast = useToastStore()
const auth = useAuthStore()

const employee = ref<Employee | null>(null)
const contracts = ref<Contract[]>([])
const loading = ref(true)
const activeTab = ref<'info' | 'contracts' | 'account'>('info')

// Account States
const userAccount = ref<UserAccount | null>(null)
const loadingAccount = ref(false)
const showGrantModal = ref(false)
const showResetModal = ref(false)
const showEditRolesModal = ref(false)
const changingStatus = ref(false)

async function load() {
  try {
    const id = route.params.id as string
    employee.value = await employeeService.getById(id)
    const all = await contractService.getAll()
    contracts.value = all.filter((c) => c.employeeId === id)
  } catch {
    toast.error('Không tìm thấy nhân viên')
    router.push('/hr/employees')
  } finally {
    loading.value = false
  }
}

async function loadAccount() {
  if (!employee.value) return
  loadingAccount.value = true
  try {
    userAccount.value = await userService.getByEmployeeId(employee.value.id)
  } catch (err: any) {
    userAccount.value = null
  } finally {
    loadingAccount.value = false
  }
}

async function selectTab(tab: 'info' | 'contracts' | 'account') {
  activeTab.value = tab
  if (tab === 'account') {
    await loadAccount()
  }
}

async function toggleStatus() {
  if (!userAccount.value) return
  changingStatus.value = true
  const newStatus = !userAccount.value.isActive
  try {
    await userService.changeStatus(userAccount.value.id, newStatus)
    toast.success(newStatus ? 'Đã kích hoạt tài khoản' : 'Đã tạm khóa tài khoản')
    await loadAccount()
  } catch (err: any) {
    toast.error(err?.response?.data?.message ?? 'Thao tác thất bại')
  } finally {
    changingStatus.value = false
  }
}

function handleAccountCreated() {
  showGrantModal.value = false
  loadAccount()
}

function handleRolesUpdated() {
  showEditRolesModal.value = false
  loadAccount()
}

function handlePasswordReset() {
  showResetModal.value = false
}

function fmt(d?: string) { return d ? new Date(d).toLocaleDateString('vi-VN') : '—' }
function fmtDateTime(d?: string | null) {
  return d ? new Date(d).toLocaleString('vi-VN') : 'Chưa từng đăng nhập'
}
function fmtMoney(n: number) { return n.toLocaleString('vi-VN') + ' ₫' }

const roleLabels: Record<string, string> = {
  Admin: 'Quản trị viên',
  HR: 'Quản lý Nhân sự',
  Manager: 'Quản lý bộ phận',
  PayrollStaff: 'Kế toán lương',
  Employee: 'Nhân viên',
}

onMounted(load)
</script>

<template>
  <div>
    <PageHeader :title="employee?.fullName ?? '...'" :breadcrumbs="[{ label: 'Nhân sự' }, { label: 'Nhân viên', to: '/hr/employees' }, { label: employee?.fullName ?? '' }]" />

    <div v-if="loading" class="space-y-3">
      <div v-for="n in 4" :key="n" class="h-12 animate-pulse rounded-lg bg-slate-200" />
    </div>

    <template v-else-if="employee">
      <!-- Tabs -->
      <div class="mb-6 border-b border-slate-200">
        <nav class="flex gap-4">
          <button
            v-for="tab in [
              { key: 'info', label: 'Thông tin cơ bản' },
              { key: 'contracts', label: `Hợp đồng (${contracts.length})` },
              ...(auth.hasAnyRole(['Admin', 'HR']) ? [{ key: 'account', label: 'Tài khoản hệ thống' }] : [])
            ]"
            :key="tab.key"
            :class="['pb-3 text-sm font-medium border-b-2 transition-colors', activeTab === tab.key ? 'border-emerald-600 text-emerald-700' : 'border-transparent text-slate-500 hover:text-slate-900']"
            @click="selectTab(tab.key as any)"
          >
            {{ tab.label }}
          </button>
        </nav>
      </div>

      <!-- Tab: Info -->
      <div v-if="activeTab === 'info'" class="rounded-xl border border-slate-200 bg-white p-6">
        <div class="flex items-start justify-between">
          <div class="flex items-center gap-4">
            <div class="grid h-16 w-16 place-items-center rounded-full bg-emerald-100 text-2xl font-bold text-emerald-700">
              {{ employee.fullName[0] }}
            </div>
            <div>
              <div class="text-xl font-bold text-slate-900">{{ employee.fullName }}</div>
              <div class="text-sm text-slate-500">{{ employee.employeeCode }} · {{ employee.email }}</div>
              <div class="mt-1"><AppBadge :status="employee.status" /></div>
            </div>
          </div>
        </div>

        <div class="mt-6 grid grid-cols-2 gap-x-8 gap-y-4 text-sm">
          <div><span class="text-slate-500">Phòng ban:</span> <span class="ml-2 font-medium">{{ employee.departmentName }}</span></div>
          <div><span class="text-slate-500">Chức vụ:</span> <span class="ml-2 font-medium">{{ employee.positionName }}</span></div>
          <div><span class="text-slate-500">Quản lý:</span> <span class="ml-2 font-medium">{{ employee.managerName ?? '—' }}</span></div>
          <div><span class="text-slate-500">SĐT:</span> <span class="ml-2 font-medium">{{ employee.phone ?? '—' }}</span></div>
          <div><span class="text-slate-500">Giới tính:</span> <span class="ml-2 font-medium">{{ employee.gender ?? '—' }}</span></div>
          <div><span class="text-slate-500">Ngày sinh:</span> <span class="ml-2 font-medium">{{ fmt(employee.dateOfBirth) }}</span></div>
          <div><span class="text-slate-500">Ngày vào làm:</span> <span class="ml-2 font-medium">{{ fmt(employee.hireDate) }}</span></div>
        </div>
      </div>

      <!-- Tab: Contracts -->
      <div v-else-if="activeTab === 'contracts'">
        <div v-if="contracts.length === 0" class="rounded-xl border border-slate-200 bg-white py-12 text-center text-slate-400">Chưa có hợp đồng nào</div>
        <div v-else class="space-y-3">
          <div v-for="c in contracts" :key="c.id" class="rounded-xl border border-slate-200 bg-white p-4">
            <div class="flex items-center justify-between">
              <div>
                <div class="font-semibold text-slate-900">{{ c.contractNumber }}</div>
                <div class="text-xs text-slate-500">{{ c.contractType }} · {{ fmt(c.startDate) }} → {{ c.endDate ? fmt(c.endDate) : 'Không thời hạn' }}</div>
              </div>
              <div class="text-right">
                <div class="text-lg font-bold text-emerald-700">{{ fmtMoney(c.baseSalary) }}</div>
                <AppBadge :status="c.status" />
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Tab: System Account -->
      <div v-else-if="activeTab === 'account'">
        <!-- Loading state -->
        <div v-if="loadingAccount" class="flex flex-col items-center justify-center py-12 space-y-3">
          <RefreshCw class="h-8 w-8 animate-spin text-emerald-600" />
          <span class="text-sm text-slate-500">Đang tải thông tin tài khoản...</span>
        </div>

        <template v-else>
          <!-- Case 1: Account NOT found -->
          <div v-if="!userAccount" class="rounded-xl border border-dashed border-slate-300 bg-white p-8 text-center max-w-lg mx-auto">
            <div class="h-12 w-12 rounded-full bg-slate-100 flex items-center justify-center mx-auto text-slate-400 mb-4">
              <KeyRound class="h-6 w-6" />
            </div>
            <h3 class="text-base font-semibold text-slate-800">Chưa có tài khoản truy cập</h3>
            <p class="text-xs text-slate-500 mt-1 max-w-sm mx-auto">
              Nhân sự này chưa được cấp tài khoản để đăng nhập vào cổng thông tin nội bộ HRMS.
            </p>
            <div class="mt-5" v-if="auth.isAdmin || auth.isHR">
              <AppButton @click="showGrantModal = true">
                <UserPlus class="h-4 w-4 mr-2" />
                Cấp tài khoản đăng nhập
              </AppButton>
            </div>
          </div>

          <!-- Case 2: Account exists -->
          <div v-else class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <!-- Account Info Card -->
            <div class="md:col-span-2 rounded-xl border border-slate-200 bg-white p-6 space-y-6">
              <div class="flex items-center justify-between border-b border-slate-100 pb-4">
                <h3 class="text-base font-bold text-slate-900">Chi tiết Tài khoản</h3>
                <span
                  class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold"
                  :class="userAccount.isActive ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' : 'bg-red-50 text-red-700 border border-red-200'"
                >
                  <span class="h-1.5 w-1.5 rounded-full" :class="userAccount.isActive ? 'bg-emerald-600' : 'bg-red-600'" />
                  {{ userAccount.isActive ? 'Đang hoạt động' : 'Đang bị khóa' }}
                </span>
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-6 text-sm">
                <div>
                  <span class="text-slate-500 block mb-1">Email đăng nhập</span>
                  <span class="font-mono font-semibold text-slate-800">{{ userAccount.email }}</span>
                </div>
                <div>
                  <span class="text-slate-500 block mb-1">Đăng nhập lần cuối</span>
                  <span class="font-medium text-slate-800">{{ fmtDateTime(userAccount.lastLoginAt) }}</span>
                </div>
                <div class="sm:col-span-2">
                  <span class="text-slate-500 block mb-1">Vai trò quyền hạn</span>
                  <div class="flex flex-wrap gap-2 mt-1.5">
                    <span
                      v-for="role in userAccount.roles"
                      :key="role"
                      class="inline-flex items-center gap-1 px-3 py-1 rounded-lg bg-slate-100 border border-slate-200 text-xs font-medium text-slate-700"
                    >
                      <Shield class="h-3 w-3 text-emerald-600" />
                      {{ roleLabels[role] || role }}
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Administrative Actions Widget -->
            <div class="rounded-xl border border-slate-200 bg-white p-6 space-y-4 h-fit" v-if="auth.isAdmin || auth.isHR">
              <h3 class="text-sm font-bold text-slate-950 uppercase tracking-wider mb-2">Thao tác quản trị</h3>

              <button
                v-if="auth.isAdmin"
                @click="showEditRolesModal = true"
                class="w-full inline-flex items-center justify-center gap-2 h-10 rounded-lg border border-slate-200 bg-white px-4 text-xs font-semibold text-slate-700 shadow-sm transition-all hover:bg-slate-50"
              >
                Thay đổi quyền hạn
              </button>

              <button
                @click="showResetModal = true"
                class="w-full inline-flex items-center justify-center gap-2 h-10 rounded-lg border border-slate-200 bg-white px-4 text-xs font-semibold text-slate-700 shadow-sm transition-all hover:bg-slate-50"
              >
                Đặt lại mật khẩu
              </button>

              <button
                :disabled="changingStatus"
                @click="toggleStatus"
                class="w-full inline-flex items-center justify-center gap-2 h-10 rounded-lg px-4 text-xs font-semibold shadow-sm transition-all"
                :class="userAccount.isActive
                  ? 'border border-red-200 bg-red-50 text-red-700 hover:bg-red-100/70'
                  : 'border border-emerald-200 bg-emerald-50 text-emerald-700 hover:bg-emerald-100/70'"
              >
                <Lock v-if="userAccount.isActive" class="h-3.5 w-3.5" />
                <Unlock v-else class="h-3.5 w-3.5" />
                {{ userAccount.isActive ? 'Khóa tài khoản' : 'Mở khóa tài khoản' }}
              </button>
            </div>
          </div>
        </template>
      </div>
    </template>

    <!-- Modals -->
    <GrantAccountModal
      v-if="showGrantModal && employee"
      :employee="employee"
      @close="showGrantModal = false"
      @saved="handleAccountCreated"
    />

    <ResetPasswordModal
      v-if="showResetModal && employee && userAccount"
      :employee="employee"
      :user-account="userAccount"
      @close="showResetModal = false"
      @saved="handlePasswordReset"
    />

    <EditRolesModal
      v-if="showEditRolesModal && employee && userAccount"
      :employee="employee"
      :user-account="userAccount"
      @close="showEditRolesModal = false"
      @saved="handleRolesUpdated"
    />
  </div>
</template>

