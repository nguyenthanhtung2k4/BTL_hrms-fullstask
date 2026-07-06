<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { reportService } from '../../../services/report.service'
import { payrollPeriodService } from '../../../services/payrollPeriod.service'
import { useToastStore } from '../../../stores/toast'
import type { PayrollSummaryReport } from '../../../types/payroll.types'
import type { PayrollPeriod } from '../../../types/payroll.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'
import { exportToExcel } from '../../../utils/excel'

const toast = useToastStore()
const reports = ref<PayrollSummaryReport[]>([])
const periods = ref<PayrollPeriod[]>([])
const loading = ref(false)
const filterPeriod = ref('')

const columns = [
  { key: 'dept', label: 'Phòng ban' }, { key: 'count', label: 'Số NV' }, { key: 'workDays', label: 'Tổng ngày công' },
  { key: 'allowances', label: 'Tổng phụ cấp' }, { key: 'deductions', label: 'Tổng khấu trừ' },
  { key: 'gross', label: 'Tổng Gross' }, { key: 'net', label: 'Tổng Net' },
]

async function loadPeriods() {
  periods.value = await payrollPeriodService.getAll()
}

async function loadReport() {
  if (!filterPeriod.value) { toast.warning('Vui lòng chọn kỳ lương'); return }
  loading.value = true
  try { reports.value = await reportService.getSummary({ payrollPeriodId: filterPeriod.value }) }
  catch { toast.error('Không thể tải báo cáo') }
  finally { loading.value = false }
}

const totalNet = () => reports.value.reduce((s, r) => s + r.totalNet, 0)
const totalGross = () => reports.value.reduce((s, r) => s + r.totalGross, 0)
const totalEmp = () => reports.value.reduce((s, r) => s + r.employeeCount, 0)
const totalWorkDays = () => reports.value.reduce((s, r) => s + r.totalWorkDays, 0)
const totalAllowances = () => reports.value.reduce((s, r) => s + r.totalAllowances, 0)
const totalDeductions = () => reports.value.reduce((s, r) => s + r.totalDeductions, 0)

function fmtMoney(n: number) { return n.toLocaleString('vi-VN') + ' ₫' }

function handleExport() {
  if (reports.value.length === 0) {
    toast.warning('Chưa có dữ liệu báo cáo để xuất')
    return
  }
  try {
    const period = periods.value.find(p => p.id === filterPeriod.value)
    const periodName = period ? period.name : 'Ky_Luong'

    const dataToExport = reports.value.map(item => ({
      'Phòng ban': item.departmentName,
      'Số Nhân viên': item.employeeCount,
      'Tổng ngày công': item.totalWorkDays,
      'Tổng phụ cấp (VNĐ)': item.totalAllowances,
      'Tổng khấu trừ (VNĐ)': item.totalDeductions,
      'Tổng Gross (VNĐ)': item.totalGross,
      'Tổng Net (VNĐ)': item.totalNet
    }))

    // Add Grand Total row at the bottom
    dataToExport.push({
      'Phòng ban': 'TỔNG CỘNG',
      'Số Nhân viên': totalEmp(),
      'Tổng ngày công': totalWorkDays(),
      'Tổng phụ cấp (VNĐ)': totalAllowances(),
      'Tổng khấu trừ (VNĐ)': totalDeductions(),
      'Tổng Gross (VNĐ)': totalGross(),
      'Tổng Net (VNĐ)': totalNet()
    })

    const fileName = `Bao_Cao_Tong_Hop_Luong_${periodName.replace(/\s+/g, '_')}`
    exportToExcel(dataToExport, fileName, 'BaoCao')
    toast.success('Đã xuất báo cáo tổng hợp lương ra Excel thành công')
  } catch (err: any) {
    toast.error(err?.message || 'Không thể xuất file Excel')
  }
}

const searchDepartment = ref('')

const filteredReports = ref<PayrollSummaryReport[]>([])

// Update filteredReports whenever reports change
watch(reports, (newVal) => {
  filteredReports.value = newVal
}, { deep: true })

const computedFilteredReports = computed(() => {
  if (!searchDepartment.value) return reports.value
  const q = searchDepartment.value.toLowerCase().trim()
  return reports.value.filter(r => r.departmentName?.toLowerCase().includes(q))
})

const { currentPage, perPage, paginatedData, total } = usePagination(computedFilteredReports)

onMounted(loadPeriods)
</script>

<template>
  <div>
    <PageHeader title="Báo cáo lương" subtitle="Tổng hợp chi phí lương theo phòng ban" :breadcrumbs="[{ label: 'Lương & Báo cáo' }, { label: 'Báo cáo' }]">
      <template #actions>
        <AppButton v-if="reports.length > 0" variant="secondary" @click="handleExport">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
          </svg>
          <span>Xuất Excel</span>
        </AppButton>
      </template>
    </PageHeader>

    <!-- Filter -->
    <div class="mb-6 flex gap-3 items-end flex-wrap">
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium text-slate-700">Chọn kỳ lương</label>
        <select v-model="filterPeriod" class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500 w-64">
          <option value="">-- Chọn kỳ --</option>
          <option v-for="p in periods" :key="p.id" :value="p.id">{{ p.name }}</option>
        </select>
      </div>
      <AppButton :loading="loading" @click="loadReport">Xem báo cáo</AppButton>

      <!-- Department Search Input -->
      <div v-if="reports.length > 0" class="flex flex-col gap-1 ml-auto">
        <label class="text-sm font-medium text-slate-700">Tìm phòng ban</label>
        <input
          v-model="searchDepartment"
          type="text"
          placeholder="Nhập tên phòng ban..."
          class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500 w-64"
        />
      </div>
    </div>

    <!-- Summary cards -->
    <div v-if="reports.length > 0" class="mb-6 grid grid-cols-3 gap-4">
      <div class="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
        <div class="text-xs text-slate-500">Tổng nhân viên</div>
        <div class="mt-1 text-2xl font-bold text-slate-900">{{ totalEmp() }}</div>
      </div>
      <div class="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
        <div class="text-xs text-slate-500">Tổng lương Gross</div>
        <div class="mt-1 text-xl font-bold text-slate-900">{{ fmtMoney(totalGross()) }}</div>
      </div>
      <div class="rounded-xl border border-emerald-200 bg-emerald-50 p-4 shadow-sm">
        <div class="text-xs text-emerald-700">Tổng quỹ lương (Net)</div>
        <div class="mt-1 text-xl font-bold text-emerald-700">{{ fmtMoney(totalNet()) }}</div>
      </div>
    </div>

    <!-- Report table -->
    <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="departmentId" empty-text="Chọn kỳ lương và nhấn Xem báo cáo">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-medium">{{ (row as PayrollSummaryReport).departmentName }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as PayrollSummaryReport).employeeCount }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as PayrollSummaryReport).totalWorkDays }}</td>
        <td class="px-4 py-3 text-sm text-blue-700">{{ fmtMoney((row as PayrollSummaryReport).totalAllowances) }}</td>
        <td class="px-4 py-3 text-sm text-red-700">{{ fmtMoney((row as PayrollSummaryReport).totalDeductions) }}</td>
        <td class="px-4 py-3 text-sm font-medium">{{ fmtMoney((row as PayrollSummaryReport).totalGross) }}</td>
        <td class="px-4 py-3 text-sm font-bold text-emerald-700">{{ fmtMoney((row as PayrollSummaryReport).totalNet) }}</td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />
  </div>
</template>

