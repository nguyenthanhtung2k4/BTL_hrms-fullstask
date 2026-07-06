<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { payslipService } from '../../../services/payslip.service'
import { useToastStore } from '../../../stores/toast'
import type { Payslip } from '../../../types/payroll.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import { useRouter } from 'vue-router'

const toast = useToastStore()
const router = useRouter()
const payslips = ref<Payslip[]>([])
const loading = ref(false)
const searchQuery = ref('')
const layoutMode = ref<'grid' | 'list'>('grid')

const columns = [
  { key: 'periodName', label: 'Kỳ lương' },
  { key: 'netSalary', label: 'Lương thực lĩnh (NET)' },
  { key: 'workedDays', label: 'Ngày công làm việc' },
  { key: 'grossSalary', label: 'Tổng thu nhập (Gross)' },
  { key: 'totalDeduction', label: 'Khấu trừ' },
  { key: 'status', label: 'Trạng thái' },
  { key: 'actions', label: 'Thao tác', class: 'text-right' }
]

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

const filteredPayslips = computed(() => {
  const sorted = [...payslips.value].sort((a, b) => {
    const codeA = a.periodCode || ''
    const codeB = b.periodCode || ''
    return codeB.localeCompare(codeA)
  })
  
  if (!searchQuery.value.trim()) return sorted
  
  const q = searchQuery.value.toLowerCase()
  return sorted.filter(p => {
    const periodName = (p.periodName || '').toLowerCase()
    const periodCode = (p.periodCode || '').toLowerCase()
    return periodName.includes(q) || periodCode.includes(q)
  })
})

function fmtMoney(n: number) {
  return n.toLocaleString('vi-VN') + ' ₫'
}

function formatPeriod(name?: string) {
  if (!name) return 'Kỳ lương không rõ'
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

    <!-- Search and Layout Toggle Bar -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 bg-white border border-slate-200/80 p-4 rounded-2xl shadow-sm">
      <div class="relative flex-1 max-w-md">
        <span class="absolute inset-y-0 left-0 flex items-center pl-3 text-slate-400 pointer-events-none">
          <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </span>
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="Tìm kiếm kỳ lương (ví dụ: T6/2026)..." 
          class="w-full pl-10 pr-4 py-2 border border-slate-200 rounded-xl text-sm focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 focus:outline-none transition-all duration-300"
        />
      </div>

      <div class="flex items-center space-x-2 border border-slate-200 p-1 rounded-xl bg-slate-50">
        <button 
          @click="layoutMode = 'grid'" 
          :class="[
            'px-3 py-1.5 rounded-lg text-xs font-semibold flex items-center space-x-1.5 transition-all duration-300',
            layoutMode === 'grid' ? 'bg-white text-emerald-600 shadow-sm' : 'text-slate-500 hover:text-slate-800'
          ]"
        >
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
          </svg>
          <span>Dạng lưới</span>
        </button>
        <button 
          @click="layoutMode = 'list'" 
          :class="[
            'px-3 py-1.5 rounded-lg text-xs font-semibold flex items-center space-x-1.5 transition-all duration-300',
            layoutMode === 'list' ? 'bg-white text-emerald-600 shadow-sm' : 'text-slate-500 hover:text-slate-800'
          ]"
        >
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
          </svg>
          <span>Dạng danh sách</span>
        </button>
      </div>
    </div>

    <!-- Loading Skeleton -->
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

    <!-- Empty State -->
    <div v-else-if="filteredPayslips.length === 0" class="flex flex-col items-center justify-center py-20 bg-white border border-slate-200/80 rounded-2xl shadow-sm text-center">
      <div class="p-4 bg-slate-50 rounded-full text-slate-300">
        <svg class="h-16 w-16" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
      </div>
      <h3 class="mt-4 text-lg font-semibold text-slate-800">Không tìm thấy phiếu lương nào</h3>
      <p class="mt-2 text-sm text-slate-500 max-w-sm">Hãy thử thay đổi từ khóa tìm kiếm hoặc kiểm tra lại sau.</p>
    </div>

    <!-- Grid Layout Mode -->
    <div v-else-if="layoutMode === 'grid'" class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
      <div
        v-for="p in filteredPayslips"
        :key="p.id"
        class="group relative overflow-hidden rounded-2xl border border-slate-200 bg-white p-6 shadow-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 cursor-pointer flex flex-col justify-between"
        @click="router.push(`/payroll/payslips/${p.id}`)"
      >
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

          <!-- Net Salary -->
          <div class="space-y-1">
            <div class="text-xs font-medium uppercase tracking-wider text-slate-400">Lương thực lĩnh (NET)</div>
            <div class="text-3xl font-extrabold text-slate-900 group-hover:text-emerald-600 transition-colors duration-300">
              {{ fmtMoney(p.netSalary) }}
            </div>
          </div>

          <!-- Stats -->
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

          <!-- Action -->
          <div class="flex items-center justify-between text-sm font-semibold text-emerald-600 group-hover:text-emerald-700">
            <span>Chi tiết lương</span>
            <svg class="h-4 w-4 transform group-hover:translate-x-1 transition-transform duration-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14 5l7 7m0 0l-7 7m7-7H3" />
            </svg>
          </div>
        </div>
      </div>
    </div>

    <!-- List (Table) Layout Mode -->
    <div v-else-if="layoutMode === 'list'">
      <AppTable :columns="columns" :rows="filteredPayslips" :pageSize="10">
        <template #default="{ row }">
          <td class="app-table__td font-semibold text-slate-800">
            {{ formatPeriod((row as Payslip).periodName) }}
          </td>
          <td class="app-table__td font-bold text-emerald-700">
            {{ fmtMoney((row as Payslip).netSalary) }}
          </td>
          <td class="app-table__td text-slate-600">
            {{ (row as Payslip).workedDays?.toFixed(1) }} ngày 
            <span v-if="(row as Payslip).paidLeaveDays > 0" class="text-emerald-600 font-medium">
              (+{{ (row as Payslip).paidLeaveDays.toFixed(1) }} phép)
            </span>
          </td>
          <td class="app-table__td text-slate-700 font-medium">
            {{ fmtMoney((row as Payslip).grossSalary) }}
          </td>
          <td class="app-table__td font-semibold text-rose-600">
            -{{ fmtMoney((row as Payslip).totalDeduction) }}
          </td>
          <td class="app-table__td">
            <span 
              class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold border"
              :class="getStatusClass((row as Payslip).status)"
            >
              {{ getStatusText((row as Payslip).status) }}
            </span>
          </td>
          <td class="app-table__td text-right">
            <button 
              @click="router.push(`/payroll/payslips/${(row as Payslip).id}`)"
              class="text-xs font-bold text-emerald-600 hover:text-emerald-700 flex items-center justify-end space-x-1 ml-auto group"
            >
              <span>Xem chi tiết</span>
              <svg class="h-3.5 w-3.5 transform group-hover:translate-x-0.5 transition-transform duration-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </button>
          </td>
        </template>
      </AppTable>
    </div>
  </div>
</template>
