<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { payslipService } from '../../../services/payslip.service'
import { useToastStore } from '../../../stores/toast'
import type { Payslip } from '../../../types/payroll.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppButton from '../../../components/ui/AppButton.vue'

const route = useRoute()
const router = useRouter()
const toast = useToastStore()
const payslip = ref<Payslip | null>(null)
const loading = ref(true)

async function load() {
  try {
    payslip.value = await payslipService.getById(route.params.id as string)
  } catch {
    toast.error('Không tìm thấy phiếu lương')
    router.back()
  } finally {
    loading.value = false
  }
}

function fmtMoney(n: number) {
  return n.toLocaleString('vi-VN') + ' ₫'
}

function formatPeriod(name?: string) {
  if (!name) return 'Kỳ lương không rõ'
  const clean = name.replace(/Luong\s+thang/gi, 'Tháng').replace(/Ky\s+luong/gi, 'Kỳ lương')
  return clean.charAt(0).toUpperCase() + clean.slice(1)
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

const earningItems = computed(() => {
  if (!payslip.value || !payslip.value.items) return []
  return payslip.value.items.filter((item) => item.itemType !== 'Deduction')
})

const deductionItems = computed(() => {
  if (!payslip.value || !payslip.value.items) return []
  return payslip.value.items.filter((item) => item.itemType === 'Deduction')
})

onMounted(load)
</script>

<template>
  <div class="max-w-3xl">
    <PageHeader 
      :title="payslip ? `Chi tiết phiếu lương — ${payslip.fullName}` : 'Phiếu lương'" 
      :breadcrumbs="[
        { label: 'Lương & Báo cáo' }, 
        { label: 'Phiếu lương cá nhân', to: '/payroll/my-payslip' }, 
        { label: payslip?.fullName ?? 'Chi tiết' }
      ]" 
    />

    <div v-if="loading" class="space-y-4">
      <div class="h-20 animate-pulse rounded-2xl bg-slate-100 border border-slate-200" />
      <div class="h-64 animate-pulse rounded-2xl bg-slate-100 border border-slate-200" />
    </div>

    <div v-else-if="payslip" class="rounded-2xl border border-slate-200 bg-white shadow-lg overflow-hidden transition-all duration-300">
      <!-- Premium Receipt Header -->
      <div class="relative bg-gradient-to-r from-emerald-600 to-teal-600 px-8 py-7 text-white">
        <div class="absolute right-6 top-6">
          <span 
            class="px-3 py-1 rounded-full text-xs font-semibold border"
            :class="[
              payslip.status === 'Draft' ? 'border-amber-300/40 bg-amber-500/10 text-amber-200' :
              payslip.status === 'Paid' ? 'border-emerald-300/40 bg-emerald-500/10 text-emerald-200' :
              'border-blue-300/40 bg-blue-500/10 text-blue-200'
            ]"
          >
            {{ getStatusText(payslip.status) }}
          </span>
        </div>
        <div class="text-xs font-semibold uppercase tracking-widest text-emerald-100">Phiếu lương chi tiết</div>
        <div class="text-2xl font-bold mt-1">{{ formatPeriod(payslip.periodName) }}</div>
        <div class="text-sm text-emerald-50 mt-1 opacity-90">
          Nhân viên: <span class="font-semibold">{{ payslip.fullName }}</span> ({{ payslip.employeeCode }})
        </div>
      </div>

      <!-- Detail Grid Info -->
      <div class="border-b border-slate-100 bg-slate-50/50 px-8 py-5">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-x-8 gap-y-3 text-sm">
          <div class="flex justify-between py-1 border-b border-slate-100">
            <span class="text-slate-500">Mã nhân viên:</span>
            <span class="font-medium text-slate-800">{{ payslip.employeeCode }}</span>
          </div>
          <div class="flex justify-between py-1 border-b border-slate-100">
            <span class="text-slate-500">Lương cơ bản (Hợp đồng):</span>
            <span class="font-medium text-slate-800">{{ fmtMoney(payslip.baseSalary) }}</span>
          </div>
          <div class="flex justify-between py-1 border-b border-slate-100">
            <span class="text-slate-500">Ngày công làm việc:</span>
            <span class="font-semibold text-emerald-700">{{ payslip.workedDays?.toFixed(1) }} ngày</span>
          </div>
          <div class="flex justify-between py-1 border-b border-slate-100">
            <span class="text-slate-500">Nghỉ phép hưởng lương:</span>
            <span class="font-semibold text-teal-700">{{ payslip.paidLeaveDays?.toFixed(1) }} ngày</span>
          </div>
        </div>
      </div>

      <div class="p-8 space-y-6">
        <!-- Earnings Section -->
        <div>
          <div class="flex items-center space-x-2 mb-3">
            <span class="p-1 bg-emerald-50 text-emerald-600 rounded">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
            </span>
            <h4 class="text-sm font-bold uppercase tracking-wider text-slate-700">Các khoản thu nhập</h4>
          </div>

          <div class="border border-slate-200/80 rounded-xl overflow-hidden shadow-sm">
            <table class="w-full text-sm">
              <thead>
                <tr class="bg-slate-50 border-b border-slate-200/80 text-slate-500 text-left">
                  <th class="px-4 py-2.5 font-medium">Khoản thu nhập</th>
                  <th class="px-4 py-2.5 font-medium text-right">Số tiền</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100 bg-white">
                <tr v-for="item in earningItems" :key="item.id" class="hover:bg-slate-50/50">
                  <td class="px-4 py-3 text-slate-700 font-medium">{{ item.name }}</td>
                  <td class="px-4 py-3 text-right font-semibold text-emerald-600">+{{ fmtMoney(item.amount) }}</td>
                </tr>
                <tr class="bg-emerald-50/20 font-bold border-t border-emerald-100">
                  <td class="px-4 py-3 text-emerald-900">Tổng thu nhập (Gross)</td>
                  <td class="px-4 py-3 text-right text-emerald-700">{{ fmtMoney(payslip.grossSalary) }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Deductions Section -->
        <div>
          <div class="flex items-center space-x-2 mb-3">
            <span class="p-1 bg-rose-50 text-rose-600 rounded">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 12H4" />
              </svg>
            </span>
            <h4 class="text-sm font-bold uppercase tracking-wider text-slate-700">Các khoản khấu trừ</h4>
          </div>

          <div class="border border-slate-200/80 rounded-xl overflow-hidden shadow-sm">
            <table class="w-full text-sm">
              <thead>
                <tr class="bg-slate-50 border-b border-slate-200/80 text-slate-500 text-left">
                  <th class="px-4 py-2.5 font-medium">Khoản khấu trừ</th>
                  <th class="px-4 py-2.5 font-medium text-right">Số tiền</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100 bg-white">
                <tr v-if="deductionItems.length === 0">
                  <td colspan="2" class="px-4 py-4 text-center text-slate-400 italic">Không có khoản khấu trừ nào trong kỳ này</td>
                </tr>
                <tr v-for="item in deductionItems" :key="item.id" v-else class="hover:bg-slate-50/50">
                  <td class="px-4 py-3 text-slate-700 font-medium">{{ item.name }}</td>
                  <td class="px-4 py-3 text-right font-semibold text-rose-600">-{{ fmtMoney(item.amount) }}</td>
                </tr>
                <tr class="bg-rose-50/20 font-bold border-t border-rose-100">
                  <td class="px-4 py-3 text-rose-900">Tổng các khoản khấu trừ</td>
                  <td class="px-4 py-3 text-right text-rose-700">-{{ fmtMoney(payslip.totalDeduction) }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Net Salary Grand Total Banner -->
        <div class="rounded-2xl bg-gradient-to-br from-emerald-600 to-teal-700 p-6 text-white shadow-md flex flex-col sm:flex-row justify-between items-center space-y-4 sm:space-y-0">
          <div class="space-y-1 text-center sm:text-left">
            <div class="text-xs font-bold uppercase tracking-widest text-emerald-100">Lương thực lĩnh (NET)</div>
            <div class="text-sm text-emerald-50/80">Số tiền thực tế được chuyển khoản vào tài khoản cá nhân</div>
          </div>
          <div class="text-3xl sm:text-4xl font-extrabold tracking-tight">
            {{ fmtMoney(payslip.netSalary) }}
          </div>
        </div>

        <!-- Back Action -->
        <div class="flex justify-between items-center pt-2">
          <AppButton variant="secondary" size="md" @click="router.back()">
            <span class="flex items-center space-x-1.5">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
              </svg>
              <span>Quay lại</span>
            </span>
          </AppButton>
        </div>
      </div>
    </div>
  </div>
</template>
