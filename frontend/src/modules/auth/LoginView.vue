<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { useToastStore } from '../../stores/toast'

const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()

const email = ref('')
const password = ref('')
const errors = ref<{ email?: string; password?: string; general?: string }>({})

async function handleLogin() {
  // Reset errors
  errors.value = {}

  // Validate
  if (!email.value) {
    errors.value.email = 'Email không được để trống'
    return
  }
  if (!password.value) {
    errors.value.password = 'Mật khẩu không được để trống'
    return
  }

  try {
    await auth.login(email.value, password.value)
    toast.success(`Chào mừng, ${auth.displayName}!`)
    // Redirect theo role
    if (auth.isHR || auth.isAdmin) {
      router.push('/')
    } else if (auth.isPayrollStaff) {
      router.push('/payroll/periods')
    } else {
      router.push('/attendance/checkin')
    }
  } catch (err: any) {
    const msg = err?.response?.data?.message ?? 'Email hoặc mật khẩu không đúng'
    errors.value.general = msg
  }
}
</script>

<template>
  <main class="min-h-screen bg-gradient-to-br from-slate-100 via-white to-emerald-50 flex items-center justify-center px-4">
    <div class="w-full max-w-md">
      <!-- Card -->
      <div class="rounded-2xl border border-slate-200 bg-white/90 p-8 shadow-xl backdrop-blur">
        <!-- Logo -->
        <div class="flex items-center gap-3 mb-8">
          <div class="grid h-12 w-12 place-items-center rounded-xl bg-emerald-600 text-white shadow-lg shadow-emerald-200">
            <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round"
                d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
          </div>
          <div>
            <h1 class="text-xl font-bold text-slate-900">HRMS Workspace</h1>
            <p class="text-xs text-slate-500">Hệ thống Quản lý Nhân sự & Chấm công</p>
          </div>
        </div>

        <!-- Error chung -->
        <div
          v-if="errors.general"
          class="mb-4 flex items-center gap-2 rounded-lg bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700"
        >
          <svg class="h-4 w-4 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          {{ errors.general }}
        </div>

        <!-- Form -->
        <form class="space-y-4" @submit.prevent="handleLogin">
          <!-- Email -->
          <div class="flex flex-col gap-1">
            <label class="text-sm font-medium text-slate-700" for="login-email">
              Email <span class="text-red-500">*</span>
            </label>
            <input
              id="login-email"
              v-model="email"
              type="email"
              placeholder="admin@hrms.com"
              autocomplete="email"
              :class="[
                'h-11 w-full rounded-lg border px-4 text-sm outline-none transition-colors',
                errors.email
                  ? 'border-red-400 bg-red-50 focus:border-red-500 focus:ring-1 focus:ring-red-400'
                  : 'border-slate-300 bg-white focus:border-emerald-500 focus:ring-1 focus:ring-emerald-400',
              ]"
            />
            <p v-if="errors.email" class="text-xs text-red-500">{{ errors.email }}</p>
          </div>

          <!-- Password -->
          <div class="flex flex-col gap-1">
            <label class="text-sm font-medium text-slate-700" for="login-password">
              Mật khẩu <span class="text-red-500">*</span>
            </label>
            <input
              id="login-password"
              v-model="password"
              type="password"
              placeholder="••••••••"
              autocomplete="current-password"
              :class="[
                'h-11 w-full rounded-lg border px-4 text-sm outline-none transition-colors',
                errors.password
                  ? 'border-red-400 bg-red-50 focus:border-red-500 focus:ring-1 focus:ring-red-400'
                  : 'border-slate-300 bg-white focus:border-emerald-500 focus:ring-1 focus:ring-emerald-400',
              ]"
            />
            <p v-if="errors.password" class="text-xs text-red-500">{{ errors.password }}</p>
          </div>

          <!-- Submit -->
          <button
            type="submit"
            :disabled="auth.loading"
            class="mt-2 inline-flex h-11 w-full items-center justify-center gap-2 rounded-lg bg-emerald-600 px-4 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-1 disabled:bg-emerald-300"
          >
            <svg
              v-if="auth.loading"
              class="h-4 w-4 animate-spin"
              fill="none"
              viewBox="0 0 24 24"
            >
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
            <svg
              v-else
              class="h-4 w-4"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1" />
            </svg>
            {{ auth.loading ? 'Đang đăng nhập...' : 'Đăng nhập' }}
          </button>
        </form>

        <!-- Hint tài khoản demo -->
        <div class="mt-5 rounded-lg bg-slate-50 border border-slate-200 p-3 text-xs text-slate-500">
          <span class="font-medium text-slate-700">Tài khoản demo:</span><br>
          Email: <code class="text-emerald-700">admin@hrms.com</code> &nbsp;|&nbsp;
          Mật khẩu: <code class="text-emerald-700">admin123</code>
        </div>
      </div>

      <p class="mt-4 text-center text-xs text-slate-400">
        BTL Fullstack — Đề tài 03: HRMS Microservices
      </p>
    </div>
  </main>
</template>
