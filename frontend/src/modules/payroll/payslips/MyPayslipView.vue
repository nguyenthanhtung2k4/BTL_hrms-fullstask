<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { payslipService } from '../../../services/payslip.service'
import { useToastStore } from '../../../stores/toast'
import type { Payslip } from '../../../types/payroll.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import { useRouter } from 'vue-router'

const toast = useToastStore()
const router = useRouter()
const payslips = ref<Payslip[]>([])
const loading = ref(false)

async function load() {
  loading.value = true
  try {
    const raw = await payslipService.getMyPayslips()
    payslips.value = raw.filter(p => p.status !== 'Draft')
  } catch {
    toast.error('Không thể tải phiếu lương')
  } finally {
    loading.value = false
  }
}

function fmtMoney(n: number) {
  return n.toLocaleString('vi-VN') + ' ₫'
}

function formatPeriod(name?: string) {
  if (!name) return 'Kỳ lương không rõ'
  // Prettify e.g. "Luong thang 06/2026" -> "Tháng 06/2026"
  const clean = name.replace(/Luong\s+thang/gi, 'Tháng').replace(/Ky\s+luong/gi, 'Kỳ lương')
  return clean.charAt(0).toUpperCase() + clean.slice(1)
}

function getStatusClass(status: string) {
  switch (status) {
    case 'Draft':
      return 'bg-amber-50 text-amber-700 border-amber-200'
    case 'Paid':
      return 'bg-emerald-50 text-emerald-700 border-emerald-200'
    case 'Closed':
      return 'bg-blue-50 text-blue-700 border-blue-200'
    default:
      return 'bg-slate-50 text-slate-700 border-slate-200'
  }
}

function getStatusText(status: string) {
  switch (status) {
    case 'Draft':
      return 'Bản nháp'
    case 'Paid':
      return 'Đã chi trả'
    case 'Closed':
      return 'Đã chốt'
    default:
      return status
  }
}

onMounted(load)
</script>

<template>
  <div class="space-y-6">
    <PageHeader 
      title="Phiếu lương của tôi" 
      subtitle="Xem chi tiết thu nhập, ngày công và các khoản khấu trừ theo từng kỳ lương" 
      :breadcrumbs="[{ label: 'Lương & Báo cáo' }, { label: 'Phiếu lương của tôi' }]" 
    />

    <div v-if="loading" class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
      <div v-for="n in 3" :key="n" class="h-64 animate-pulse rounded-2xl bg-slate-100 border border-slate-200 p-6 flex flex-col justify-between">
        <div class="space-y-3">
          <div class="h-5 w-1/2 rounded bg-slate-200" />
          <div class="h-8 w-2/3 rounded bg-slate-200 mt-4" />
        </div>
        <div class="space-y-2">
          <div class="h-4 w-full rounded bg-slate-200" />
          <div class="h-4 w-full rounded bg-slate-200" />
        </div>
        <div class="h-8 w-1/3 rounded bg-slate-200" />
      </div>
    </div>

    <div v-else-if="payslips.length === 0" class="flex flex-col items-center justify-center py-20 bg-white border border-slate-200/80 rounded-2xl shadow-sm text-center">
      <div class="p-4 bg-slate-50 rounded-full text-slate-300">
        <svg class="h-16 w-16" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
      </div>
      <h3 class="mt-4 text-lg font-semibold text-slate-800">Chưa có phiếu lương nào</h3>
      <p class="mt-2 text-sm text-slate-500 max-w-sm">Phiếu lương của bạn sẽ xuất hiện ở đây sau khi kỳ lương hiện tại được tính và phê duyệt.</p>
    </div>

    <div v-else class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
      <div
        v-for="p in payslips"
        :key="p.id"
        class="group relative overflow-hidden rounded-2xl border border-slate-200 bg-white p-6 shadow-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 cursor-pointer flex flex-col justify-between"
        @click="router.push(`/payroll/payslips/${p.id}`)"
      >
        <!-- Decorative subtle background glow -->
        <div class="absolute top-0 right-0 -z-10 h-32 w-32 rounded-full bg-emerald-500/5 blur-3xl group-hover:bg-emerald-500/10 transition-all duration-300" />

        <div>
          <!-- Header -->
          <div class="flex items-center justify-between">
            <div class="flex items-center space-x-2">
              <div class="p-2 bg-slate-50 rounded-lg text-slate-600 group-hover:bg-emerald-50 group-hover:text-emerald-600 transition-colors duration-300">
                <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>
              <span class="font-bold text-slate-800 text-base leading-tight group-hover:text-emerald-700 transition-colors duration-300">
                {{ formatPeriod(p.periodName) }}
              </span>
            </div>
            
            <span 
              class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold border"
              :class="getStatusClass(p.status)"
            >
              {{ getStatusText(p.status) }}
            </span>
          </div>

          <!-- Divider -->
          <div class="my-4 border-b border-slate-100" />

          <!-- Net Salary (Main info) -->
          <div class="space-y-1">
            <div class="text-xs font-medium uppercase tracking-wider text-slate-400">Lương thực lĩnh (NET)</div>
            <div class="text-3xl font-extrabold text-slate-900 group-hover:text-emerald-600 transition-colors duration-300">
              {{ fmtMoney(p.netSalary) }}
            </div>
          </div>

          <!-- Stats list -->
          <div class="mt-5 space-y-2.5">
            <div class="flex justify-between items-center text-xs">
              <span class="text-slate-500">Ngày công làm việc:</span>
              <span class="font-semibold text-slate-700">
                {{ p.workedDays?.toFixed(1) }} ngày {{ p.paidLeaveDays > 0 ? `(+${p.paidLeaveDays.toFixed(1)} phép)` : '' }}
              </span>
            </div>
            <div class="flex justify-between items-center text-xs">
              <span class="text-slate-500">Tổng thu nhập (Gross):</span>
              <span class="font-medium text-slate-700">{{ fmtMoney(p.grossSalary) }}</span>
            </div>
            <div class="flex justify-between items-center text-xs">
              <span class="text-slate-500">Các khoản khấu trừ:</span>
              <span class="font-semibold text-rose-600">-{{ fmtMoney(p.totalDeduction) }}</span>
            </div>
          </div>
        </div>

        <div>
          <!-- Divider -->
          <div class="my-4 border-b border-slate-100" />

          <!-- Action link -->
          <div class="flex items-center justify-between text-sm font-semibold text-emerald-600 group-hover:text-emerald-700">
            <span>Chi tiết lương</span>
            <svg class="h-4 w-4 transform group-hover:translate-x-1 transition-transform duration-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14 5l7 7m0 0l-7 7m7-7H3" />
            </svg>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
