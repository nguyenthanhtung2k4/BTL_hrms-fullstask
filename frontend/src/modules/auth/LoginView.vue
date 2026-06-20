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
  <div class="min-h-screen w-full flex items-center justify-center bg-[#0f172a] p-4 md:p-8 select-none">
    <div class="flex w-full max-w-5xl bg-[#1e293b] rounded-2xl overflow-hidden shadow-[0_25px_50px_-12px_rgba(0,0,0,0.6)] min-h-[600px]">
      <div class="hidden md:flex md:w-1/2 bg-[#0f172a] p-12 flex-col justify-between border-r border-slate-800">
        <div>
          <span class="inline-block px-3 py-1 text-xs font-semibold tracking-wider text-blue-300 bg-blue-900/50 rounded-full uppercase">
            HRMS Enterprise
          </span>

          <h1 class="mt-6 text-3xl font-extrabold text-white tracking-tight leading-tight">
            Quản lý nhân sự doanh nghiệp chuẩn mực
          </h1>
          <p class="mt-3 text-sm text-slate-400 leading-relaxed">
            Nền tảng HRMS toàn diện dành cho doanh nghiệp: chấm công, quản lý nhân viên, lương thưởng và báo cáo chuyên sâu.
          </p>

          <div class="mt-8 space-y-4">
            <div class="flex items-start gap-3">
              <span class="flex items-center justify-center w-6 h-6 text-xs font-bold text-blue-400 bg-blue-950/50 border border-blue-800/50 rounded-full mt-0.5">01</span>
              <div>
                <h4 class="text-sm font-semibold text-slate-200">Bảo mật doanh nghiệp</h4>
                <p class="text-xs text-slate-400">Hệ thống phân quyền rõ ràng và bảo vệ dữ liệu nhân sự toàn diện.</p>
              </div>
            </div>
            <div class="flex items-start gap-3">
              <span class="flex items-center justify-center w-6 h-6 text-xs font-bold text-blue-400 bg-blue-950/50 border border-blue-800/50 rounded-full mt-0.5">02</span>
              <div>
                <h4 class="text-sm font-semibold text-slate-200">Quy trình vận hành</h4>
                <p class="text-xs text-slate-400">Tích hợp chấm công, duyệt phép và báo cáo lương trong cùng một luồng.</p>
              </div>
            </div>
            <div class="flex items-start gap-3">
              <span class="flex items-center justify-center w-6 h-6 text-xs font-bold text-blue-400 bg-blue-950/50 border border-blue-800/50 rounded-full mt-0.5">03</span>
              <div>
                <h4 class="text-sm font-semibold text-slate-200">Giao diện chuyên nghiệp</h4>
                <p class="text-xs text-slate-400">Thiết kế tinh gọn, dễ dùng cho cả nhân viên và cấp quản lý.</p>
              </div>
            </div>
          </div>
        </div>

        <div class="pt-6 border-t border-slate-800/60">
          <h5 class="text-xs font-bold text-slate-300 uppercase tracking-wide">Triển khai doanh nghiệp</h5>
          <p class="mt-1 text-xs text-slate-400 leading-relaxed">
            Giao diện tối ưu cho doanh nghiệp vừa và lớn, tập trung vào trải nghiệm người dùng và khả năng mở rộng.
          </p>
        </div>
      </div>

      <div class="w-full md:w-1/2 bg-white p-8 md:p-12 flex flex-col justify-center">
        <div class="mb-6">
          <div class="flex items-center gap-2 mb-2">
            <div class="flex items-center justify-center w-5 h-5 rounded bg-blue-100 text-blue-600">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2.5" stroke="currentColor" class="w-3.5 h-3.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 6a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0ZM4.501 20.118a7.5 7.5 0 0 1 14.998 0A17.933 17.933 0 0 1 12 21.75c-2.676 0-5.216-.584-7.499-1.632Z" />
              </svg>
            </div>
            <span class="text-xs font-bold tracking-wider text-gray-400 uppercase">Hệ thống quản lý nhân sự</span>
          </div>

          <h2 class="text-2xl md:text-3xl font-extrabold text-gray-900 tracking-tight">
            Đăng nhập hệ thống
          </h2>
          <p class="mt-1.5 text-xs md:text-sm text-gray-500">
            Truy cập nhanh các chức năng nhân sự và báo cáo theo vai trò của bạn.
          </p>
        </div>

        <div v-if="errors.general" class="p-3 mb-4 text-xs font-medium text-red-600 bg-red-50 border border-red-100 rounded-lg flex items-center gap-1.5">
          <span>⚠️</span> {{ errors.general }}
        </div>

        <form @submit.prevent="handleLogin" class="space-y-4">
          <div>
            <label class="block text-xs font-bold text-gray-700 uppercase tracking-wider mb-1" for="login-email">E-mail</label>
            <input
              id="login-email"
              type="email"
              v-model="email"
              class="w-full px-4 py-2.5 text-sm text-gray-900 border border-slate-200 rounded-lg bg-white placeholder:text-slate-400 focus:border-blue-500 focus:ring-4 focus:ring-blue-100 outline-none transition-all duration-200"
              placeholder="admin@hrms.com"
            />
            <p v-if="errors.email" class="mt-1 text-xs text-red-600">{{ errors.email }}</p>
          </div>

          <div>
            <label class="block text-xs font-bold text-gray-700 uppercase tracking-wider mb-1" for="login-password">Mật khẩu</label>
            <input
              id="login-password"
              type="password"
              v-model="password"
              class="w-full px-4 py-2.5 text-sm text-gray-900 border border-slate-200 rounded-lg bg-white placeholder:text-slate-400 focus:border-blue-500 focus:ring-4 focus:ring-blue-100 outline-none transition-all duration-200"
              placeholder="••••••••"
            />
            <p v-if="errors.password" class="mt-1 text-xs text-red-600">{{ errors.password }}</p>
          </div>

          <button
            type="submit"
            :disabled="auth.loading"
            class="w-full py-2.5 mt-2 text-sm font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-lg shadow-sm transition duration-150 active:scale-[0.99] disabled:opacity-70 disabled:cursor-not-allowed"
          >
            Đăng nhập
          </button>
        </form>

        <div class="mt-6 p-4 bg-blue-50 border border-blue-100 rounded-lg text-xs text-gray-500">
          <span class="font-bold text-gray-700 block mb-1">Tài khoản demo</span>
          <p class="flex justify-between">Email: <span class="text-blue-700 font-semibold selection:bg-blue-100">admin@hrms.com</span></p>
          <p class="flex justify-between mt-0.5">Mật khẩu: <span class="text-blue-700 font-semibold selection:bg-blue-100">admin123</span></p>
        </div>

        <p class="mt-6 text-[11px] text-center text-gray-400">
          BTL Fullstack — Đề tài 03: HRMS Microservices
        </p>
      </div>
    </div>
  </div>
</template>
