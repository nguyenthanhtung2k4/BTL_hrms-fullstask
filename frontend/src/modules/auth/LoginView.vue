<script setup lang="ts">
import { LogIn, Network } from '@lucide/vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'

const router = useRouter()
const auth = useAuthStore()

function login() {
  auth.loginDemo()
  router.push({ name: 'dashboard' })
}
</script>

<template>
  <main class="grid min-h-screen place-items-center bg-slate-100 px-4">
    <section class="w-full max-w-md rounded border border-slate-200 bg-white p-6 shadow-sm">
      <div class="flex items-center gap-3">
        <div class="grid size-11 place-items-center rounded bg-emerald-600 text-white">
          <Network :size="24" />
        </div>
        <div>
          <h1 class="text-xl font-semibold text-slate-950">HRMS Workspace</h1>
          <p class="text-sm text-slate-500">Vue + ASP.NET Core microservices</p>
        </div>
      </div>

      <div class="mt-6 space-y-4">
        <label class="block">
          <span class="text-sm font-medium text-slate-700">Email</span>
          <input
            class="mt-1 h-11 w-full rounded border border-slate-300 px-3 text-sm outline-none focus:border-emerald-600"
            type="email"
            value="admin@hrms.local"
            readonly
          />
        </label>
        <label class="block">
          <span class="text-sm font-medium text-slate-700">Password</span>
          <input
            class="mt-1 h-11 w-full rounded border border-slate-300 px-3 text-sm outline-none focus:border-emerald-600"
            type="password"
            value="demo-password"
            readonly
          />
        </label>
      </div>

      <button
        class="mt-6 inline-flex h-11 w-full items-center justify-center gap-2 rounded bg-emerald-600 px-4 text-sm font-semibold text-white hover:bg-emerald-700"
        type="button"
        @click="login"
      >
        <LogIn :size="18" />
        <span>Đăng nhập demo</span>
      </button>
    </section>
  </main>
</template>

