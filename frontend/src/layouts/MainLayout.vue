<script setup lang="ts">
import {
  BadgeDollarSign,
  CalendarCheck,
  LayoutDashboard,
  LogOut,
  Network,
  ShieldCheck,
  Users,
} from '@lucide/vue'
import { RouterLink, RouterView, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const auth = useAuthStore()

const navItems = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/hr', label: 'HR Core', icon: Users },
  { to: '/attendance', label: 'Attendance', icon: CalendarCheck },
  { to: '/payroll', label: 'Payroll & Report', icon: BadgeDollarSign },
]

function logout() {
  auth.logout()
  router.push({ name: 'login' })
}
</script>

<template>
  <div class="min-h-screen bg-slate-100 text-slate-900">
    <aside
      class="fixed inset-y-0 left-0 hidden w-72 border-r border-slate-200 bg-white px-4 py-5 lg:block"
    >
      <div class="flex h-12 items-center gap-3 px-2">
        <div class="grid size-10 place-items-center rounded bg-emerald-600 text-white">
          <Network :size="22" />
        </div>
        <div>
          <div class="text-sm font-semibold uppercase tracking-wide text-slate-500">HRMS</div>
          <div class="text-base font-semibold">Microservices</div>
        </div>
      </div>

      <nav class="mt-8 space-y-1">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="flex h-11 items-center gap-3 rounded px-3 text-sm font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-950"
          active-class="bg-emerald-50 text-emerald-700"
        >
          <component :is="item.icon" :size="19" />
          <span>{{ item.label }}</span>
        </RouterLink>
      </nav>

      <div class="absolute inset-x-4 bottom-5 rounded border border-slate-200 bg-slate-50 p-4">
        <div class="flex items-center gap-2 text-sm font-semibold text-slate-700">
          <ShieldCheck :size="18" class="text-emerald-600" />
          <span>{{ auth.displayName }}</span>
        </div>
        <div class="mt-2 flex flex-wrap gap-1">
          <span
            v-for="role in auth.roles"
            :key="role"
            class="rounded bg-white px-2 py-1 text-xs font-medium text-slate-600"
          >
            {{ role }}
          </span>
        </div>
      </div>
    </aside>

    <div class="lg:pl-72">
      <header
        class="sticky top-0 z-10 flex h-16 items-center justify-between border-b border-slate-200 bg-white/95 px-4 backdrop-blur lg:px-8"
      >
        <div>
          <div class="text-sm font-medium text-slate-500">BTL Fullstack</div>
          <div class="text-lg font-semibold">Quản lý nhân sự và chấm công</div>
        </div>
        <button
          class="inline-flex h-10 items-center gap-2 rounded border border-slate-200 bg-white px-3 text-sm font-medium text-slate-700 hover:bg-slate-50"
          type="button"
          @click="logout"
        >
          <LogOut :size="18" />
          <span>Đăng xuất</span>
        </button>
      </header>

      <main class="px-4 py-6 lg:px-8">
        <RouterView />
      </main>
    </div>
  </div>
</template>

