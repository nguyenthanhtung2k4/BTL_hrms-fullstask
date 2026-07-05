<script setup lang="ts">
/**
 * UserManagementView.vue — Admin quản lý tài khoản người dùng
 * Quyền: chỉ Admin
 * Chức năng: xem danh sách, tạo, cập nhật role, reset mật khẩu, khóa/mở tài khoản
 */
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useToastStore } from '../../stores/toast'
import { useAuthStore } from '../../stores/auth'
import { userService } from '../../services/user.service'
import { employeeService } from '../../services/employee.service'
import { extractError } from '../../services/apiClient'
import type { UserAccount } from '../../types/user.types'
import type { Employee } from '../../types/hr.types'
import PageHeader from '../../components/layout/PageHeader.vue'
import AppTable from '../../components/ui/AppTable.vue'
import AppBadge from '../../components/ui/AppBadge.vue'
import AppButton from '../../components/ui/AppButton.vue'
import AppModal from '../../components/ui/AppModal.vue'
import AppInput from '../../components/ui/AppInput.vue'
import AppConfirm from '../../components/ui/AppConfirm.vue'

const { t } = useI18n({ useScope: 'global' })
const toast = useToastStore()
const auth = useAuthStore()

// ─── Data ─────────────────────────────────────────────────────────────────────
const users = ref<UserAccount[]>([])
const employees = ref<Employee[]>([])
const loading = ref(false)
const search = ref('')

// ─── Columns ──────────────────────────────────────────────────────────────────
const columns = computed(() => [
  { key: 'email', label: t('user.email') },
  { key: 'employee', label: t('user.employee') },
  { key: 'roles', label: t('user.roles') },
  { key: 'status', label: t('user.status') },
  { key: 'lastLogin', label: t('user.lastLogin') },
  { key: 'actions', label: t('common.actions') },
])

const filteredUsers = computed(() => {
  if (!search.value.trim()) return users.value
  const q = search.value.toLowerCase()
  return users.value.filter(
    (u) => u.email.toLowerCase().includes(q)
  )
})

// ─── Available Roles ──────────────────────────────────────────────────────────
const AVAILABLE_ROLES = ['Admin', 'HR', 'Manager', 'Employee', 'PayrollStaff']

// ─── Employee lookup ──────────────────────────────────────────────────────────
function getEmployee(empId: string | null): Employee | null {
  if (!empId) return null
  return employees.value.find((e) => e.id === empId) ?? null
}

function getEmployeeName(empId: string | null): string {
  const emp = getEmployee(empId)
  if (!emp) return t('user.noEmployee')
  return `${emp.fullName} (${emp.employeeCode})`
}

// ─── Employees without account ────────────────────────────────────────────────
const employeesWithoutAccount = computed(() => {
  const accountedIds = new Set(users.value.map((u) => u.employeeId).filter(Boolean))
  return employees.value.filter((e) => !accountedIds.has(e.id))
})

// ─── Create Modal ─────────────────────────────────────────────────────────────
const showCreateModal = ref(false)
const createForm = ref({ employeeId: '', email: '', password: '', roles: [] as string[] })
const createErrors = ref<Record<string, string>>({})
const createLoading = ref(false)

function openCreateModal() {
  createForm.value = { employeeId: '', email: '', password: '', roles: ['Employee'] }
  createErrors.value = {}
  showCreateModal.value = true
}

function toggleRole(role: string) {
  const idx = createForm.value.roles.indexOf(role)
  if (idx >= 0) createForm.value.roles.splice(idx, 1)
  else createForm.value.roles.push(role)
}

async function submitCreate() {
  createErrors.value = {}
  if (!createForm.value.employeeId) { createErrors.value.employeeId = t('validation.required'); return }
  if (!createForm.value.password || createForm.value.password.length < 8) {
    createErrors.value.password = 'Mật khẩu phải chứa ít nhất 8 ký tự'; return
  }
  if (createForm.value.roles.length === 0) { createErrors.value.roles = t('validation.required'); return }

  // Auto-fill email from employee if not set
  if (!createForm.value.email) {
    const emp = getEmployee(createForm.value.employeeId)
    if (emp?.email) createForm.value.email = emp.email
  }
  if (!createForm.value.email) { createErrors.value.email = t('validation.required'); return }

  createLoading.value = true
  try {
    await userService.create({
      employeeId: createForm.value.employeeId,
      email: createForm.value.email,
      password: createForm.value.password,
      roles: createForm.value.roles,
    })
    toast.success(t('user.createSuccess'))
    showCreateModal.value = false
    await loadData()
  } catch (e: any) {
    toast.error(extractError(e, t('toast.saveFailed')))
  } finally {
    createLoading.value = false
  }
}

// ─── Update Roles Modal ───────────────────────────────────────────────────────
const showRolesModal = ref(false)
const rolesTarget = ref<UserAccount | null>(null)
const editRoles = ref<string[]>([])
const rolesLoading = ref(false)

function openRolesModal(user: UserAccount) {
  rolesTarget.value = user
  editRoles.value = [...user.roles]
  showRolesModal.value = true
}

function toggleEditRole(role: string) {
  const idx = editRoles.value.indexOf(role)
  if (idx >= 0) editRoles.value.splice(idx, 1)
  else editRoles.value.push(role)
}

async function submitUpdateRoles() {
  if (!rolesTarget.value) return
  if (editRoles.value.length === 0) { toast.error(t('validation.required')); return }
  rolesLoading.value = true
  try {
    await userService.updateRoles(rolesTarget.value.id, editRoles.value)
    toast.success(t('user.updateSuccess'))
    showRolesModal.value = false
    await loadData()
  } catch {
    toast.error(t('toast.saveFailed'))
  } finally {
    rolesLoading.value = false
  }
}

// ─── Reset Password Modal ─────────────────────────────────────────────────────
const showResetModal = ref(false)
const resetTarget = ref<UserAccount | null>(null)
const newPassword = ref('')
const resetLoading = ref(false)

function openResetModal(user: UserAccount) {
  resetTarget.value = user
  newPassword.value = ''
  showResetModal.value = true
}

async function submitReset() {
  if (!resetTarget.value) return
  if (!newPassword.value || newPassword.value.length < 8) {
    toast.error('Mật khẩu phải chứa ít nhất 8 ký tự')
    return
  }
  resetLoading.value = true
  try {
    await userService.resetPassword(resetTarget.value.id, newPassword.value)
    toast.success(t('user.resetSuccess'))
    showResetModal.value = false
  } catch (e: any) {
    toast.error(extractError(e, t('toast.saveFailed')))
  } finally {
    resetLoading.value = false
  }
}

// ─── Toggle Status Confirm ────────────────────────────────────────────────────
const showStatusConfirm = ref(false)
const statusTarget = ref<UserAccount | null>(null)
const statusLoading = ref(false)

function openStatusConfirm(user: UserAccount) {
  statusTarget.value = user
  showStatusConfirm.value = true
}

async function confirmToggleStatus() {
  if (!statusTarget.value) return
  statusLoading.value = true
  try {
    await userService.changeStatus(statusTarget.value.id, !statusTarget.value.isActive)
    toast.success(statusTarget.value.isActive ? t('user.deactivateSuccess') : t('user.activateSuccess'))
    showStatusConfirm.value = false
    await loadData()
  } catch {
    toast.error(t('toast.saveFailed'))
  } finally {
    statusLoading.value = false
  }
}

// ─── Load Data ────────────────────────────────────────────────────────────────
async function loadData() {
  loading.value = true
  try {
    const [usersData, empData] = await Promise.all([
      userService.getAll(),
      employeeService.getAll(),
    ])
    users.value = usersData
    employees.value = empData
  } catch {
    toast.error(t('toast.loadFailed'))
  } finally {
    loading.value = false
  }
}

// ─── Helpers ──────────────────────────────────────────────────────────────────
function formatDate(dateStr: string | null): string {
  if (!dateStr) return '—'
  return new Date(dateStr).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' })
}

function onEmployeeSelect(e: Event) {
  const empId = (e.target as HTMLSelectElement).value
  createForm.value.employeeId = empId
  const emp = getEmployee(empId)
  if (emp?.email) createForm.value.email = emp.email
}

onMounted(loadData)
</script>

<template>
  <div>
    <PageHeader
      :title="t('user.title')"
      :subtitle="t('user.list')"
      :breadcrumbs="[{ label: t('nav.hr') }, { label: t('user.title') }]"
    >
      <template #actions>
        <AppButton variant="primary" @click="openCreateModal">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          {{ t('user.create') }}
        </AppButton>
      </template>
    </PageHeader>

    <!-- Search -->
    <div class="mb-4 flex gap-3 items-center">
      <div class="relative flex-1 max-w-xs">
        <svg class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4" style="color: var(--text-tertiary);" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-4.35-4.35M17 11A6 6 0 105 11a6 6 0 0012 0z" />
        </svg>
        <input
          v-model="search"
          type="text"
          :placeholder="t('common.search') + ' email...'"
          class="h-9 w-full rounded-lg border pl-9 pr-3 text-sm outline-none transition-colors"
          style="border-color: var(--border-strong); background: var(--bg-surface); color: var(--text-primary);"
        />
      </div>
      <span class="text-sm" style="color: var(--text-tertiary);">
        {{ filteredUsers.length }} / {{ users.length }} {{ t('common.results') }}
      </span>
    </div>

    <!-- Table -->
    <AppTable :page-size="10" :loading="loading" :columns="columns" :rows="filteredUsers" row-key="id">
      <template #default="{ row }">
        <!-- Email -->
        <td class="app-table__td">
          <div class="font-medium" style="color: var(--text-primary);">{{ row.email }}</div>
        </td>
        <!-- Employee -->
        <td class="app-table__td">
          <div class="text-sm" style="color: var(--text-secondary);">
            {{ getEmployeeName(row.employeeId) }}
          </div>
        </td>
        <!-- Roles -->
        <td class="app-table__td">
          <div class="flex flex-wrap gap-1">
            <span
              v-for="role in row.roles"
              :key="role"
              class="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-semibold"
              :style="role === 'Admin'
                ? 'background: var(--color-danger-light); color: var(--color-danger);'
                : role === 'HR'
                  ? 'background: var(--color-info-light); color: var(--color-info);'
                  : role === 'PayrollStaff'
                    ? 'background: var(--color-success-light); color: var(--color-success);'
                    : role === 'Manager'
                      ? 'background: var(--color-warning-light); color: hsl(36,70%,30%);'
                      : 'background: var(--bg-muted); color: var(--text-secondary);'"
            >
              {{ role }}
            </span>
          </div>
        </td>
        <!-- Status -->
        <td class="app-table__td">
          <AppBadge :status="row.isActive ? 'Active' : 'Inactive'" />
        </td>
        <!-- Last Login -->
        <td class="app-table__td">
          <span class="text-sm font-mono" style="color: var(--text-secondary);">
            {{ formatDate(row.lastLoginAt) }}
          </span>
        </td>
        <!-- Actions -->
        <td class="app-table__td">
          <div class="flex items-center gap-1.5 flex-wrap">
            <AppButton v-if="auth.isAdmin" variant="ghost" size="xs" :title="t('user.edit')" @click="openRolesModal(row)">
              <svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
              {{ t('user.roles') }}
            </AppButton>
            <AppButton variant="ghost" size="xs" @click="openResetModal(row)">
              <svg class="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z" />
              </svg>
              {{ t('user.resetPassword') }}
            </AppButton>
            <AppButton
              :variant="row.isActive ? 'danger' : 'success'"
              size="xs"
              @click="openStatusConfirm(row)"
            >
              {{ row.isActive ? t('user.inactive') : t('user.active') }}
            </AppButton>
          </div>
        </td>
      </template>
    </AppTable>

    <!-- ── Create Modal ────────────────────────────────────────────────────── -->
    <AppModal v-if="showCreateModal" :title="t('user.create')" @close="showCreateModal = false">
      <form class="space-y-4" @submit.prevent="submitCreate">
        <!-- Employee picker -->
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium" style="color: var(--text-primary);">
            {{ t('user.employee') }} <span style="color: var(--color-danger);">*</span>
          </label>
          <select
            class="h-9 w-full rounded-lg border px-3 text-sm outline-none"
            style="border-color: var(--border-strong); background: var(--bg-surface); color: var(--text-primary);"
            :value="createForm.employeeId"
            @change="onEmployeeSelect"
          >
            <option value="">— {{ t('user.selectEmployee') }} —</option>
            <option v-for="emp in employeesWithoutAccount" :key="emp.id" :value="emp.id">
              {{ emp.fullName }} ({{ emp.employeeCode }})
            </option>
          </select>
          <p v-if="createErrors.employeeId" class="text-xs" style="color: var(--color-danger);">{{ createErrors.employeeId }}</p>
        </div>

        <!-- Email -->
        <AppInput
          v-model="createForm.email"
          :label="t('user.email')"
          type="email"
          placeholder="user@company.com"
          required
          :error="createErrors.email"
        />

        <!-- Password -->
        <AppInput
          v-model="createForm.password"
          :label="t('user.password')"
          type="password"
          placeholder="••••••••"
          required
          :error="createErrors.password"
        />

        <!-- Roles -->
        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium" style="color: var(--text-primary);">
            {{ t('user.selectRoles') }} <span style="color: var(--color-danger);">*</span>
          </label>
          <div class="flex flex-wrap gap-2">
            <button
              v-for="role in AVAILABLE_ROLES"
              :key="role"
              type="button"
              :class="[
                'rounded-full px-3 py-1 text-xs font-semibold border transition-colors',
                createForm.roles.includes(role)
                  ? 'border-[var(--color-primary)] bg-[var(--color-primary-light)] text-[var(--color-primary-text)]'
                  : 'border-[var(--border-strong)] text-[var(--text-secondary)]',
              ]"
              @click="toggleRole(role)"
            >
              {{ role }}
            </button>
          </div>
          <p v-if="createErrors.roles" class="text-xs" style="color: var(--color-danger);">{{ createErrors.roles }}</p>
        </div>
      </form>
      <template #footer>
        <AppButton variant="secondary" @click="showCreateModal = false">{{ t('common.cancel') }}</AppButton>
        <AppButton variant="primary" :loading="createLoading" @click="submitCreate">{{ t('common.save') }}</AppButton>
      </template>
    </AppModal>

    <!-- ── Update Roles Modal ──────────────────────────────────────────────── -->
    <AppModal v-if="showRolesModal" :title="t('user.edit') + ' — ' + rolesTarget?.email" @close="showRolesModal = false">
      <div class="space-y-3">
        <p class="text-sm" style="color: var(--text-secondary);">{{ t('user.availableRoles') }}:</p>
        <div class="flex flex-wrap gap-2">
          <button
            v-for="role in AVAILABLE_ROLES"
            :key="role"
            type="button"
            :class="[
              'rounded-full px-3 py-1 text-xs font-semibold border transition-colors',
              editRoles.includes(role)
                ? 'border-[var(--color-primary)] bg-[var(--color-primary-light)] text-[var(--color-primary-text)]'
                : 'border-[var(--border-strong)] text-[var(--text-secondary)]',
            ]"
            @click="toggleEditRole(role)"
          >
            {{ role }}
          </button>
        </div>
        <p v-if="editRoles.length === 0" class="text-xs" style="color: var(--color-danger);">{{ t('validation.required') }}</p>
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="showRolesModal = false">{{ t('common.cancel') }}</AppButton>
        <AppButton variant="primary" :loading="rolesLoading" @click="submitUpdateRoles">{{ t('common.save') }}</AppButton>
      </template>
    </AppModal>

    <!-- ── Reset Password Modal ────────────────────────────────────────────── -->
    <AppModal v-if="showResetModal" :title="t('user.resetPasswordTitle')" @close="showResetModal = false">
      <p class="text-sm mb-4" style="color: var(--text-secondary);">
        {{ t('user.resetPasswordConfirm') }} <strong>{{ resetTarget?.email }}</strong>
      </p>
      <AppInput
        v-model="newPassword"
        :label="t('user.newPassword')"
        type="password"
        placeholder="••••••••"
        required
      />
      <template #footer>
        <AppButton variant="secondary" @click="showResetModal = false">{{ t('common.cancel') }}</AppButton>
        <AppButton variant="primary" :loading="resetLoading" @click="submitReset">{{ t('common.confirm') }}</AppButton>
      </template>
    </AppModal>

    <!-- ── Toggle Status Confirm ───────────────────────────────────────────── -->
    <AppConfirm
      v-if="showStatusConfirm"
      :title="statusTarget?.isActive ? t('user.deactivateConfirm') : t('user.activateConfirm')"
      :message="statusTarget?.email"
      :danger="statusTarget?.isActive"
      :loading="statusLoading"
      @confirm="confirmToggleStatus"
      @cancel="showStatusConfirm = false"
    />
  </div>
</template>

<style scoped>
.app-table__td {
  padding: 0.875rem 1.25rem;
  color: var(--text-primary);
  border-bottom: 1px solid var(--border);
}
</style>
