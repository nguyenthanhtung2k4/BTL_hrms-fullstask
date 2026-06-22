<script setup lang="ts">
/**
 * ProfileView.vue — Hồ sơ cá nhân của người dùng đang đăng nhập
 * Quyền: tất cả roles (xem thông tin cá nhân + đổi mật khẩu)
 */
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '../../stores/auth'
import { useToastStore } from '../../stores/toast'
import { employeeService } from '../../services/employee.service'
import PageHeader from '../../components/layout/PageHeader.vue'
import AppButton from '../../components/ui/AppButton.vue'
import AppInput from '../../components/ui/AppInput.vue'
import type { Employee } from '../../types/hr.types'
import { useFormGuard } from '../../composables/useFormGuard'

const { t } = useI18n()
const auth = useAuthStore()
const toast = useToastStore()

// ─── Employee data ─────────────────────────────────────────────────────────────
const employee = ref<Employee | null>(null)
const loadingEmployee = ref(false)

async function loadEmployee() {
  if (!auth.employeeId) return
  loadingEmployee.value = true
  try {
    employee.value = await employeeService.getById(auth.employeeId)
  } catch {
    // silently fail if no employee linked
  } finally {
    loadingEmployee.value = false
  }
}

// ─── Change Password ──────────────────────────────────────────────────────────
const pwForm = ref({ currentPassword: '', newPassword: '', confirmPassword: '' })
const pwErrors = ref<Record<string, string>>({})
const pwLoading = ref(false)

const isDirty = computed(() => {
  return !!(pwForm.value.currentPassword || pwForm.value.newPassword || pwForm.value.confirmPassword)
})
useFormGuard(isDirty)

function validatePw(): boolean {
  pwErrors.value = {}
  if (!pwForm.value.currentPassword) { pwErrors.value.currentPassword = t('validation.required'); return false }
  if (!pwForm.value.newPassword || pwForm.value.newPassword.length < 6) {
    pwErrors.value.newPassword = t('profile.passwordMinLength'); return false
  }
  if (pwForm.value.newPassword !== pwForm.value.confirmPassword) {
    pwErrors.value.confirmPassword = t('profile.passwordMismatch'); return false
  }
  return true
}

async function submitChangePassword() {
  if (!validatePw()) return
  pwLoading.value = true
  try {
    await auth.changePassword({
      currentPassword: pwForm.value.currentPassword,
      newPassword: pwForm.value.newPassword,
      confirmPassword: pwForm.value.confirmPassword,
    })
    toast.success(t('profile.changePasswordSuccess'))
    pwForm.value = { currentPassword: '', newPassword: '', confirmPassword: '' }
    // auth.changePassword gọi logout() → redirect về login tự động
  } catch (e: any) {
    const msg = e?.response?.data?.message ?? t('toast.saveFailed')
    toast.error(msg)
  } finally {
    pwLoading.value = false
  }
}

// ─── Avatar initials ──────────────────────────────────────────────────────────
const initials = computed(() => {
  const name = auth.displayName
  if (!name) return 'U'
  const parts = name.split(' ')
  if (parts.length >= 2) return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  return name.slice(0, 2).toUpperCase()
})

// ─── Role display ─────────────────────────────────────────────────────────────
function getRoleColor(role: string): string {
  const map: Record<string, string> = {
    Admin: 'background: var(--color-danger-light); color: var(--color-danger);',
    HR: 'background: var(--color-info-light); color: var(--color-info);',
    PayrollStaff: 'background: var(--color-success-light); color: var(--color-success);',
    Manager: 'background: var(--color-warning-light); color: hsl(36,70%,30%);',
  }
  return map[role] ?? 'background: var(--bg-muted); color: var(--text-secondary);'
}

function formatDate(dateStr: string | null | undefined): string {
  if (!dateStr) return '—'
  return new Date(dateStr).toLocaleDateString('vi-VN')
}

function formatCurrency(amount: number | null | undefined): string {
  if (!amount) return '—'
  return new Intl.NumberFormat('vi-VN', { style: 'decimal' }).format(amount) + ' đ'
}

onMounted(loadEmployee)
</script>

<template>
  <div>
    <PageHeader
      :title="t('profile.title')"
      :subtitle="t('profile.myInfo')"
      :breadcrumbs="[{ label: t('nav.profile') }]"
    />

    <div class="grid gap-6 lg:grid-cols-3">
      <!-- ── Left: Account Info Card ────────────────────────────────────────── -->
      <div class="lg:col-span-1">
        <!-- Avatar + Name -->
        <div class="profile-card mb-6 text-center">
          <div class="profile-avatar">{{ initials }}</div>
          <h2 class="mt-3 text-lg font-bold" style="color: var(--text-primary);">{{ auth.displayName }}</h2>
          <p class="text-sm" style="color: var(--text-secondary);">{{ auth.user?.email }}</p>

          <!-- Roles -->
          <div class="mt-3 flex flex-wrap justify-center gap-1.5">
            <span
              v-for="role in auth.roles"
              :key="role"
              class="rounded-full px-2.5 py-0.5 text-xs font-semibold"
              :style="getRoleColor(role)"
            >
              {{ role }}
            </span>
          </div>
        </div>

        <!-- Account Info -->
        <div class="profile-card">
          <h3 class="profile-section-title">{{ t('profile.accountInfo') }}</h3>
          <dl class="space-y-3 mt-3">
            <div class="profile-dl-row">
              <dt>Email</dt>
              <dd>{{ auth.user?.email }}</dd>
            </div>
            <div class="profile-dl-row">
              <dt>{{ t('user.status') }}</dt>
              <dd>
                <span class="badge badge--success">{{ t('user.active') }}</span>
              </dd>
            </div>
            <div class="profile-dl-row">
              <dt>{{ t('profile.rolesLabel') }}</dt>
              <dd>{{ auth.roles.join(', ') }}</dd>
            </div>
          </dl>
        </div>
      </div>

      <!-- ── Right: Employee Info + Change Password ─────────────────────────── -->
      <div class="lg:col-span-2 space-y-6">

        <!-- Employee Info Card -->
        <div class="profile-card">
          <h3 class="profile-section-title">{{ t('profile.employeeInfo') }}</h3>

          <div v-if="loadingEmployee" class="py-6 text-center text-sm" style="color: var(--text-tertiary);">
            {{ t('common.loading') }}
          </div>

          <div v-else-if="!employee" class="py-6 text-center text-sm" style="color: var(--text-tertiary);">
            {{ t('user.noEmployee') }}
          </div>

          <div v-else class="mt-4 grid gap-4 sm:grid-cols-2">
            <div class="profile-info-field">
              <span class="profile-info-label">{{ t('employee.code') }}</span>
              <span class="profile-info-value font-mono">{{ employee.employeeCode }}</span>
            </div>
            <div class="profile-info-field">
              <span class="profile-info-label">{{ t('employee.fullName') }}</span>
              <span class="profile-info-value">{{ employee.fullName }}</span>
            </div>
            <div class="profile-info-field">
              <span class="profile-info-label">{{ t('employee.email') }}</span>
              <span class="profile-info-value">{{ employee.email }}</span>
            </div>
            <div class="profile-info-field">
              <span class="profile-info-label">{{ t('employee.phone') }}</span>
              <span class="profile-info-value">{{ employee.phoneNumber || '—' }}</span>
            </div>
            <div class="profile-info-field">
              <span class="profile-info-label">{{ t('employee.department') }}</span>
              <span class="profile-info-value">{{ employee.departmentName || '—' }}</span>
            </div>
            <div class="profile-info-field">
              <span class="profile-info-label">{{ t('employee.position') }}</span>
              <span class="profile-info-value">{{ employee.positionName || '—' }}</span>
            </div>
            <div class="profile-info-field">
              <span class="profile-info-label">{{ t('employee.hireDate') }}</span>
              <span class="profile-info-value">{{ formatDate(employee.hireDate) }}</span>
            </div>
            <div class="profile-info-field">
              <span class="profile-info-label">{{ t('employee.baseSalary') }}</span>
              <span class="profile-info-value font-mono">{{ formatCurrency(employee.baseSalary) }}</span>
            </div>
            <div class="profile-info-field sm:col-span-2">
              <span class="profile-info-label">{{ t('employee.address') }}</span>
              <span class="profile-info-value">{{ employee.address || '—' }}</span>
            </div>
          </div>
        </div>

        <!-- Change Password Card -->
        <div class="profile-card">
          <h3 class="profile-section-title">{{ t('profile.changePassword') }}</h3>
          <p class="mt-1 text-sm" style="color: var(--text-secondary);">
            Sau khi đổi mật khẩu thành công, bạn sẽ được đăng xuất tự động.
          </p>

          <form class="mt-4 space-y-4 max-w-sm" @submit.prevent="submitChangePassword">
            <AppInput
              v-model="pwForm.currentPassword"
              :label="t('profile.currentPassword')"
              type="password"
              placeholder="••••••••"
              required
              :error="pwErrors.currentPassword"
            />
            <AppInput
              v-model="pwForm.newPassword"
              :label="t('profile.newPassword')"
              type="password"
              placeholder="••••••••"
              required
              :error="pwErrors.newPassword"
            />
            <AppInput
              v-model="pwForm.confirmPassword"
              :label="t('profile.confirmPassword')"
              type="password"
              placeholder="••••••••"
              required
              :error="pwErrors.confirmPassword"
            />
            <AppButton type="submit" variant="primary" :loading="pwLoading">
              {{ t('profile.changePasswordBtn') }}
            </AppButton>
          </form>
        </div>

      </div>
    </div>
  </div>
</template>

<style scoped>
.profile-card {
  border-radius: var(--radius-lg);
  border: 1px solid var(--border);
  background-color: var(--bg-surface);
  padding: 1.25rem 1.5rem;
  box-shadow: var(--shadow-sm);
}

.profile-avatar {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 5rem;
  height: 5rem;
  border-radius: 50%;
  background-color: var(--color-primary);
  color: white;
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0 auto;
  box-shadow: 0 4px 12px color-mix(in srgb, var(--color-primary) 30%, transparent);
}

.profile-section-title {
  font-size: 0.9375rem;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
  padding-bottom: 0.625rem;
  border-bottom: 1px solid var(--border);
}

.profile-dl-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.8125rem;
}
.profile-dl-row dt {
  color: var(--text-tertiary);
  flex-shrink: 0;
}
.profile-dl-row dd {
  color: var(--text-primary);
  font-weight: 500;
  text-align: right;
}

.profile-info-field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.profile-info-label {
  font-size: 0.6875rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-tertiary);
}

.profile-info-value {
  font-size: 0.9375rem;
  color: var(--text-primary);
  font-weight: 500;
}

/* Badge reuse */
.badge {
  display: inline-flex;
  align-items: center;
  border-radius: var(--radius-full);
  padding: 0.125rem 0.625rem;
  font-size: 0.6875rem;
  font-weight: 600;
}
.badge--success {
  background: var(--color-success-light);
  color: var(--color-success);
}
</style>
