<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { payslipService } from '../../../services/payslip.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { Payslip } from '../../../types/payroll.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const toast = useToastStore()
const router = useRouter()
const payslips = ref<Payslip[]>([])
const loading = ref(false)

async function load() {
  loading.value = true
  try { payslips.value = await payslipService.getMyPayslips() }
  catch { toast.error('Không thể tải phiếu lương') }
  finally { loading.value = false }
}

function fmtMoney(n: number) { return n.toLocaleString('vi-VN') + ' ₫' }

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Phiếu lương của tôi" subtitle="Lịch sử phiếu lương cá nhân" :breadcrumbs="[{ label: 'Lương & Báo cáo' }, { label: 'Phiếu lương của tôi' }]" />

    <div v-if="loading" class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <div v-for="n in 3" :key="n" class="h-32 animate-pulse rounded-xl bg-slate-200" />
    </div>

    <div v-else-if="payslips.length === 0" class="py-16 text-center text-slate-400">
      <svg class="mx-auto h-12 w-12 text-slate-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
      </svg>
      <p class="mt-3">Chưa có phiếu lương nào</p>
    </div>

    <div v-else class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <div
        v-for="p in payslips"
        :key="p.id"
        class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm hover:shadow-md transition-shadow cursor-pointer"
        @click="router.push(`/payroll/payslips/${p.id}`)"
      >
        <div class="text-sm font-semibold text-slate-700">{{ p.fullName }} ({{ p.employeeCode }})</div>
        <div class="mt-3 text-3xl font-bold text-emerald-700">{{ fmtMoney(p.netSalary) }}</div>
        <div class="mt-2 text-xs text-slate-500">Ngày công: {{ p.workedDays?.toFixed(1) }} ngày</div>
        <div class="mt-3 flex justify-between text-xs text-slate-400">
          <span>Gross: {{ fmtMoney(p.grossSalary) }}</span>
          <span class="text-red-400">-{{ fmtMoney(p.totalDeduction) }}</span>
        </div>
        <div class="mt-2 text-xs text-emerald-600 font-medium">Xem chi tiết →</div>
      </div>
    </div>
  </div>
</template>
