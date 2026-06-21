<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '../../stores/auth'
import { useToastStore } from '../../stores/toast'

const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()
const { t } = useI18n()

const email = ref('')
const password = ref('')
const errors = ref<{ email?: string; password?: string; general?: string }>({})

async function handleLogin() {
  errors.value = {}
  if (!email.value)    { errors.value.email    = t('validation.required'); return }
  if (!password.value) { errors.value.password = t('validation.required'); return }

  try {
    await auth.login(email.value, password.value)
    toast.success(`${t('dashboard.greeting_morning')}, ${auth.displayName}!`)
    if (auth.isHR || auth.isAdmin) router.push('/')
    else if (auth.isPayrollStaff)  router.push('/payroll/periods')
    else                           router.push('/attendance/checkin')
  } catch (err: any) {
    errors.value.general = err?.response?.data?.message ?? t('auth.loginError')
  }
}
</script>

<template>
  <main class="login-page">
    <div class="login-container">
      <!-- Card -->
      <div class="login-card">
        <!-- Logo + Title -->
        <div class="login-logo-row">
          <div class="login-logo-icon">
            <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round"
                d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
          </div>
          <div>
            <h1 class="login-title">HRMS Workspace</h1>
            <p class="login-subtitle">Hệ thống Quản lý Nhân sự &amp; Chấm công</p>
          </div>
        </div>

        <!-- General error -->
        <div v-if="errors.general" class="login-error">
          <svg class="h-4 w-4 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          {{ errors.general }}
        </div>

        <!-- Form -->
        <form class="login-form" @submit.prevent="handleLogin">
          <!-- Email -->
          <div class="login-field">
            <label class="login-label" for="login-email">
              {{ t('auth.email') }} <span class="login-required">*</span>
            </label>
            <input
              id="login-email"
              v-model="email"
              type="email"
              :placeholder="t('auth.emailPlaceholder')"
              autocomplete="email"
              :class="['login-input', errors.email ? 'login-input--error' : '']"
            />
            <p v-if="errors.email" class="login-field-error">{{ errors.email }}</p>
          </div>

          <!-- Password -->
          <div class="login-field">
            <label class="login-label" for="login-password">
              {{ t('auth.password') }} <span class="login-required">*</span>
            </label>
            <input
              id="login-password"
              v-model="password"
              type="password"
              :placeholder="t('auth.passwordPlaceholder')"
              autocomplete="current-password"
              :class="['login-input', errors.password ? 'login-input--error' : '']"
            />
            <p v-if="errors.password" class="login-field-error">{{ errors.password }}</p>
          </div>

          <!-- Submit -->
          <button type="submit" :disabled="auth.loading" class="login-submit">
            <svg v-if="auth.loading" class="h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
            <svg v-else class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1" />
            </svg>
            {{ auth.loading ? t('auth.loginLoading') : t('auth.loginButton') }}
          </button>
        </form>

        <!-- Demo hint -->
        <div class="login-demo">
          <span class="login-demo__label">Tài khoản demo:</span><br>
          Email: <code class="login-demo__code">admin@hrms.com</code> &nbsp;|&nbsp;
          Mật khẩu: <code class="login-demo__code">admin123</code>
        </div>
      </div>

      <p class="login-footer">BTL Fullstack — Đề tài 03: HRMS Microservices</p>
    </div>
  </main>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  background-color: var(--bg-page);
  background-image:
    radial-gradient(ellipse at 10% 20%, color-mix(in srgb, var(--color-primary) 8%, transparent) 0%, transparent 60%),
    radial-gradient(ellipse at 90% 80%, color-mix(in srgb, var(--color-primary) 5%, transparent) 0%, transparent 60%);
  transition: background-color var(--transition-base);
}

.login-container {
  width: 100%;
  max-width: 26rem;
}

.login-card {
  border-radius: var(--radius-xl);
  border: 1px solid var(--border);
  background-color: var(--bg-surface);
  box-shadow: var(--shadow-xl);
  padding: 2rem;
  backdrop-filter: blur(8px);
}

.login-logo-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1.75rem;
}

.login-logo-icon {
  display: grid;
  place-items: center;
  width: 3rem;
  height: 3rem;
  border-radius: var(--radius-md);
  background-color: var(--color-primary);
  color: white;
  flex-shrink: 0;
  box-shadow: 0 4px 12px color-mix(in srgb, var(--color-primary) 30%, transparent);
}

.login-title {
  font-size: 1.125rem;
  font-weight: 700;
  color: var(--text-primary);
  margin: 0;
}

.login-subtitle {
  font-size: 0.75rem;
  color: var(--text-secondary);
  margin: 0.125rem 0 0;
}

/* Error alert */
.login-error {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  border-radius: var(--radius-sm);
  border: 1px solid var(--color-danger);
  background-color: var(--color-danger-light);
  color: var(--color-danger);
  padding: 0.625rem 0.875rem;
  font-size: 0.875rem;
  margin-bottom: 1rem;
}

/* Form */
.login-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.login-field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.login-label {
  font-size: 0.8125rem;
  font-weight: 500;
  color: var(--text-primary);
}

.login-required {
  color: var(--color-danger);
  margin-left: 2px;
}

.login-input {
  height: 2.75rem;
  width: 100%;
  border-radius: var(--radius-sm);
  border: 1px solid var(--border-strong);
  background-color: var(--bg-surface);
  color: var(--text-primary);
  padding: 0 1rem;
  font-size: 0.9375rem;
  outline: none;
  transition: border-color var(--transition-fast), box-shadow var(--transition-fast),
    background-color var(--transition-base);
}
.login-input::placeholder { color: var(--text-tertiary); }
.login-input:focus {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-primary) 15%, transparent);
}
.login-input--error {
  border-color: var(--color-danger);
  background-color: var(--color-danger-light);
}

.login-field-error {
  font-size: 0.75rem;
  color: var(--color-danger);
  margin: 0;
}

.login-submit {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  height: 2.75rem;
  width: 100%;
  border-radius: var(--radius-sm);
  background-color: var(--color-primary);
  color: white;
  font-size: 0.9375rem;
  font-weight: 600;
  border: none;
  cursor: pointer;
  margin-top: 0.25rem;
  transition: background-color var(--transition-fast), opacity var(--transition-fast);
  box-shadow: 0 2px 8px color-mix(in srgb, var(--color-primary) 35%, transparent);
}
.login-submit:hover:not(:disabled) {
  background-color: var(--color-primary-hover);
}
.login-submit:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Demo hint */
.login-demo {
  margin-top: 1.25rem;
  border-radius: var(--radius-sm);
  border: 1px solid var(--border);
  background-color: var(--bg-subtle);
  padding: 0.75rem;
  font-size: 0.75rem;
  color: var(--text-secondary);
}

.login-demo__label {
  font-weight: 600;
  color: var(--text-primary);
}

.login-demo__code {
  font-family: 'JetBrains Mono', monospace;
  color: var(--color-primary-text);
  background-color: var(--color-primary-light);
  padding: 0.125rem 0.375rem;
  border-radius: 4px;
  font-size: 0.75rem;
}

/* Footer */
.login-footer {
  text-align: center;
  margin-top: 1rem;
  font-size: 0.75rem;
  color: var(--text-tertiary);
}
</style>
