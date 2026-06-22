<script setup lang="ts">
import { ref, onMounted } from 'vue'
import {
  LogIn,
  Network,
  Key,
  Mail,
  ShieldCheck
} from '@lucide/vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { writeAuditLog } from '../../services/mockData'
import gsap from 'gsap'

const router = useRouter()
const auth = useAuthStore()

// State for custom inputs
const email = ref('admin@hrms.local')
const password = ref('•••••••••••••')

function handleLogin() {
  const mailVal = email.value.toLowerCase().trim()
  let roleSelected: 'Admin' | 'HR' | 'Manager' | 'Employee' | 'PayrollStaff' = 'Admin'
  
  if (mailVal.includes('hr')) roleSelected = 'HR'
  else if (mailVal.includes('manager')) roleSelected = 'Manager'
  else if (mailVal.includes('employee')) roleSelected = 'Employee'
  else if (mailVal.includes('payroll')) roleSelected = 'PayrollStaff'

  auth.loginAs(roleSelected)
  localStorage.setItem('active_user_name', auth.displayName)
  writeAuditLog('HR Core', 'LOGIN', `Đăng nhập thành công (${auth.displayName})`)
  router.push({ name: 'dashboard' })
}

onMounted(() => {
  // GSAP: Floating orbs animation
  gsap.to('.orb-1', { x: 60, y: -40, duration: 8, repeat: -1, yoyo: true, ease: 'sine.inOut' })
  gsap.to('.orb-2', { x: -50, y: 50, duration: 10, repeat: -1, yoyo: true, ease: 'sine.inOut' })
  gsap.to('.orb-3', { x: 30, y: 30, duration: 12, repeat: -1, yoyo: true, ease: 'sine.inOut' })

  // GSAP: Login card entrance
  const tl = gsap.timeline({ defaults: { ease: 'power3.out' }})
  tl.from('.login-card', { y: 40, opacity: 0, duration: 0.8 })
    .from('.login-logo', { scale: 0, rotation: -180, duration: 0.6, ease: 'back.out(1.7)' }, '-=0.4')
    .from('.login-title', { y: 20, opacity: 0, duration: 0.5 }, '-=0.3')
    .from('.login-field', { y: 20, opacity: 0, duration: 0.4, stagger: 0.1 }, '-=0.2')
    .from('.login-btn', { y: 15, opacity: 0, duration: 0.4 }, '-=0.1')
    .from('.login-footer', { y: 15, opacity: 0, duration: 0.4 }, '-=0.1')
})
</script>

<template>
  <main class="relative grid min-h-screen place-items-center px-4 overflow-hidden font-sans" style="background: #050816;">
    <!-- Animated floating orbs -->
    <div class="orb-1 login-orb" style="width: 500px; height: 500px; top: -15%; left: -10%; background: radial-gradient(circle, rgba(99,102,241,0.2), transparent 70%);"></div>
    <div class="orb-2 login-orb" style="width: 400px; height: 400px; bottom: -10%; right: -5%; background: radial-gradient(circle, rgba(139,92,246,0.2), transparent 70%);"></div>
    <div class="orb-3 login-orb" style="width: 300px; height: 300px; top: 40%; left: 50%; background: radial-gradient(circle, rgba(59,130,246,0.12), transparent 70%);"></div>

    <!-- Grid pattern overlay -->
    <div class="absolute inset-0 pointer-events-none opacity-[0.03]" style="background-image: linear-gradient(rgba(255,255,255,0.1) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.1) 1px, transparent 1px); background-size: 60px 60px;"></div>

    <section class="login-card w-full max-w-md rounded-3xl p-7 md:p-9 space-y-7 z-10" style="background: rgba(15,23,42,0.5); backdrop-filter: blur(24px); border: 1px solid rgba(99,102,241,0.15); box-shadow: 0 25px 50px -12px rgba(0,0,0,0.5), 0 0 60px rgba(99,102,241,0.08);">
      <!-- Title & Branding -->
      <div class="login-title text-center space-y-3">
        <div class="login-logo inline-flex size-16 place-items-center justify-center rounded-2xl text-white font-bold shadow-2xl mb-2" style="background: var(--gradient-brand); box-shadow: 0 0 40px rgba(99,102,241,0.3);">
          <Network :size="30" class="stroke-[2.5]" />
        </div>
        <div>
          <h1 class="text-2xl font-black bg-gradient-to-r from-white to-slate-300 bg-clip-text text-transparent">HRMS Workspace</h1>
          <p class="text-xs text-slate-400 font-semibold mt-1.5">Hệ thống Quản lý Nhân sự & Chấm công Microservices</p>
        </div>
      </div>

      <!-- Login Form inputs -->
      <div class="space-y-4 pt-1">
        <div class="login-field space-y-1.5">
          <label class="text-[10px] font-bold text-slate-500 uppercase tracking-wide flex items-center gap-1">
            <Mail :size="11" />
            <span>Địa chỉ Email</span>
          </label>
          <input 
            v-model="email" 
            type="email" 
            placeholder="admin@hrms.local"
            class="w-full h-12 px-4 rounded-xl text-slate-100 outline-none font-semibold transition-all"
            style="background: rgba(15,23,42,0.7); border: 1px solid rgba(99,102,241,0.2);"
            @focus="($event.target as HTMLInputElement).style.borderColor = 'rgba(99,102,241,0.5)'; ($event.target as HTMLInputElement).style.boxShadow = '0 0 0 3px rgba(99,102,241,0.15)'"
            @blur="($event.target as HTMLInputElement).style.borderColor = 'rgba(99,102,241,0.2)'; ($event.target as HTMLInputElement).style.boxShadow = 'none'"
          />
        </div>

        <div class="login-field space-y-1.5">
          <label class="text-[10px] font-bold text-slate-500 uppercase tracking-wide flex items-center gap-1">
            <Key :size="11" />
            <span>Mật khẩu bảo mật</span>
          </label>
          <input 
            v-model="password" 
            type="password" 
            class="w-full h-12 px-4 rounded-xl text-slate-400 outline-none transition-all"
            style="background: rgba(15,23,42,0.7); border: 1px solid rgba(99,102,241,0.2);"
            @focus="($event.target as HTMLInputElement).style.borderColor = 'rgba(99,102,241,0.5)'; ($event.target as HTMLInputElement).style.boxShadow = '0 0 0 3px rgba(99,102,241,0.15)'"
            @blur="($event.target as HTMLInputElement).style.borderColor = 'rgba(99,102,241,0.2)'; ($event.target as HTMLInputElement).style.boxShadow = 'none'"
          />
        </div>
      </div>

      <!-- Submit button -->
      <button 
        @click="handleLogin"
        class="login-btn w-full h-12 inline-flex items-center justify-center gap-2 rounded-xl text-white text-sm font-bold cursor-pointer transition-all active:scale-[0.98] relative overflow-hidden"
        type="button"
        style="background: var(--gradient-brand); box-shadow: 0 4px 20px rgba(99,102,241,0.35);"
        @mouseenter="($event.target as HTMLElement).style.boxShadow = '0 6px 30px rgba(99,102,241,0.5)'; ($event.target as HTMLElement).style.transform = 'translateY(-1px)'"
        @mouseleave="($event.target as HTMLElement).style.boxShadow = '0 4px 20px rgba(99,102,241,0.35)'; ($event.target as HTMLElement).style.transform = 'none'"
      >
        <LogIn :size="16" />
        <span>Tiến hành Đăng nhập</span>
      </button>

      <!-- Account Notes at bottom -->
      <div class="login-footer mt-6 pt-5 text-center text-[11px] text-slate-500 leading-relaxed space-y-2" style="border-top: 1px solid rgba(99,102,241,0.1);">
        <div class="flex items-center justify-center gap-1 text-indigo-400 font-bold uppercase tracking-wider text-[9px]">
          <ShieldCheck :size="12" />
          <span>Tài khoản trải nghiệm</span>
        </div>
        <p>
          Email: <span class="font-mono text-indigo-400 font-bold">admin@hrms.local</span><br/>
          Mật khẩu: <span class="font-mono text-indigo-400 font-bold">bất kỳ</span>
        </p>
        <p class="text-[10px] italic text-slate-600 pt-2" style="border-top: 1px solid rgba(99,102,241,0.06);">
          Sau khi đăng nhập, có thể chuyển nhanh quyền truy cập (HR, Manager, Employee, Kế toán) trực tiếp tại Avatar ở Topbar.
        </p>
      </div>
    </section>
  </main>
</template>
