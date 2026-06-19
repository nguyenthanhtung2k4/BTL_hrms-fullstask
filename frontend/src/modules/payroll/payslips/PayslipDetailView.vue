<script setup lang="ts">
import { ref, onMounted } from 'vue'
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
  try { payslip.value = await payslipService.getById(route.params.id as string) }
  catch { toast.error('Không tìm thấy phiếu lương'); router.back() }
  finally { loading.value = false }
}

function fmtMoney(n: number) { return n.toLocaleString('vi-VN') + ' ₫' }

onMounted(load)
</script>

<template>
  <div class="max-w-2xl">
    <PageHeader :title="payslip ? `Phiếu lương — ${payslip.fullName}` : '...'" :breadcrumbs="[{ label: 'Lương & Báo cáo' }, { label: 'Phiếu lương', to: '/payroll/payslips' }, { label: payslip?.fullName ?? '' }]" />

    <div v-if="loading" class="space-y-3">
      <div v-for="n in 6" :key="n" class="h-10 animate-pulse rounded-lg bg-slate-200" />
    </div>

    <div v-else-if="payslip" class="rounded-2xl border border-slate-200 bg-white shadow-sm overflow-hidden">
      <!-- Header -->
      <div class="bg-emerald-600 px-6 py-5 text-white">
        <div class="text-lg font-bold">PHIẾU LƯƠNG</div>
        <div class="text-sm opacity-80 mt-0.5">{{ payslip.employeeCode }} — {{ payslip.fullName }}</div>
      </div>

      <!-- Employee info -->
      <div class="border-b border-slate-200 px-6 py-4">
        <div class="grid grid-cols-2 gap-2 text-sm">
          <div><span class="text-slate-500">Họ tên:</span> <span class="ml-1 font-semibold">{{ payslip.fullName }}</span></div>
          <div><span class="text-slate-500">Mã NV:</span> <span class="ml-1">{{ payslip.employeeCode }}</span></div>
          <div><span class="text-slate-500">Ngày công:</span> <span class="ml-1 font-semibold">{{ payslip.workedDays?.toFixed(1) }} ngày</span></div>
          <div><span class="text-slate-500">Phép có lương:</span> <span class="ml-1">{{ payslip.paidLeaveDays }} ngày</span></div>
        </div>
      </div>

      <!-- Earning rows -->
      <div class="px-6 py-4 space-y-1">
        <div class="text-xs font-semibold uppercase tracking-wide text-slate-400 mb-2">Thu nhập</div>
        <div class="flex justify-between text-sm"><span>Lương cơ bản</span><span class="font-medium">{{ fmtMoney(payslip.baseSalary) }}</span></div>
        <template v-for="item in payslip.items" :key="item.id">
          <div class="flex justify-between text-sm text-blue-700"><span>+ {{ item.name }}</span><span>{{ fmtMoney(item.amount) }}</span></div>
        </template>
        <div class="flex justify-between text-sm font-semibold border-t border-slate-100 pt-2 mt-1">
          <span>Tổng thu nhập (Gross)</span><span>{{ fmtMoney(payslip.grossSalary) }}</span>
        </div>
      </div>

      <!-- Deduction rows -->
      <div class="border-t border-slate-100 px-6 py-4 space-y-1">
        <div class="text-xs font-semibold uppercase tracking-wide text-slate-400 mb-2">Khấu trừ</div>
        <div v-if="payslip.totalDeduction === 0" class="text-sm text-slate-400">Không có khấu trừ</div>
        <div v-else class="flex justify-between text-sm font-semibold border-t border-slate-100 pt-2">
          <span>Tổng khấu trừ</span><span class="text-red-700">{{ fmtMoney(payslip.totalDeduction) }}</span>
        </div>
      </div>

      <!-- Net salary -->
      <div class="border-t-2 border-emerald-200 bg-emerald-50 px-6 py-4 flex justify-between items-center">
        <span class="text-base font-bold text-slate-900">LƯƠNG THỰC LĨNH (NET)</span>
        <span class="text-2xl font-bold text-emerald-700">{{ fmtMoney(payslip.netSalary) }}</span>
      </div>

      <div class="px-6 py-3">
        <AppButton variant="secondary" size="sm" @click="router.back()">← Quay lại</AppButton>
      </div>
    </div>
  </div>
</template>
