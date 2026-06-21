<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { payslipService } from '../../../services/payslip.service'
import { payrollPeriodService } from '../../../services/payrollPeriod.service'
import { employeeService } from '../../../services/employee.service'
import { useToastStore } from '../../../stores/toast'
import type { Payslip } from '../../../types/payroll.types'
import type { PayrollPeriod } from '../../../types/payroll.types'
import type { Employee } from '../../../types/hr.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import { useRouter } from 'vue-router'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'
import { exportToExcel } from '../../../utils/excel'

const toast = useToastStore()
const router = useRouter()
const payslips = ref<Payslip[]>([])
const periods = ref<PayrollPeriod[]>([])
const employees = ref<Employee[]>([])
const loading = ref(false)
const filterPeriod = ref('')
const filterEmployee = ref('')

const columns = [
  { key: 'period', label: 'Kỳ lương' },
  { key: 'employee', label: 'Nhân viên' },
  { key: 'code', label: 'Mã NV' },
  { key: 'work', label: 'Ngày công' },
  { key: 'gross', label: 'Gross' },
  { key: 'net', label: 'Net lương' },
  { key: 'actions', label: '' },
]

async function load() {
  loading.value = true
  try {
    [payslips.value, periods.value, employees.value] = await Promise.all([
      payslipService.getAll({ periodId: filterPeriod.value || undefined, employeeId: filterEmployee.value || undefined }),
      payrollPeriodService.getAll(), employeeService.getAll(),
    ])
  } catch { toast.error('Không thể tải dữ liệu') }
  finally { loading.value = false }
}

function fmtMoney(n: number) { return n.toLocaleString('vi-VN') + ' ₫' }
function formatPeriod(name?: string) {
  if (!name) return 'Kỳ lương không rõ'
  const clean = name.replace(/Luong\s+thang/gi, 'Tháng').replace(/Ky\s+luong/gi, 'Kỳ lương')
  return clean.charAt(0).toUpperCase() + clean.slice(1)
}

function handleExport() {
  try {
    const dataToExport = payslips.value.map(item => ({
      'Kỳ lương': formatPeriod(item.periodName) || '',
      'Mã Nhân viên': item.employeeCode || '',
      'Họ tên Nhân viên': item.fullName || '',
      'Lương cơ bản (VNĐ)': item.baseSalary,
      'Ngày công': item.workedDays,
      'Phép hưởng lương': item.paidLeaveDays,
      'Thu nhập Gross (VNĐ)': item.grossSalary,
      'Khấu trừ (VNĐ)': item.totalDeduction,
      'Thực lĩnh Net (VNĐ)': item.netSalary,
      'Trạng thái': item.status === 'Draft' ? 'Bản nháp' : item.status === 'Paid' ? 'Đã chi trả' : 'Đã chốt'
    }))
    exportToExcel(dataToExport, 'Danh_Sach_Phieu_Luong_Nhan_Vien', 'PhieuLuong')
    toast.success('Đã xuất báo cáo phiếu lương ra Excel thành công')
  } catch (err: any) {
    toast.error(err?.message || 'Không thể xuất file Excel')
  }
}

const { currentPage, perPage, paginatedData, total } = usePagination(payslips)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Phiếu lương" subtitle="Toàn bộ phiếu lương trong hệ thống" :breadcrumbs="[{ label: 'Lương & Báo cáo' }, { label: 'Phiếu lương' }]">
      <template #actions>
        <AppButton variant="secondary" @click="handleExport">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
          </svg>
          <span>Xuất Excel</span>
        </AppButton>
      </template>
    </PageHeader>

    <div class="mb-4 flex gap-3 flex-wrap">
      <select v-model="filterPeriod" class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500">
        <option value="">Tất cả kỳ lương</option>
        <option v-for="p in periods" :key="p.id" :value="p.id">{{ p.name }}</option>
      </select>
      <select v-model="filterEmployee" class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500">
        <option value="">Tất cả nhân viên</option>
        <option v-for="e in employees" :key="e.id" :value="e.id">{{ e.fullName }}</option>
      </select>
      <button class="h-9 rounded-lg bg-emerald-600 px-4 text-sm font-medium text-white hover:bg-emerald-700" @click="load">Tìm</button>
    </div>

    <AppTable :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có phiếu lương nào">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm text-slate-500">{{ formatPeriod((row as Payslip).periodName) }}</td>
        <td class="px-4 py-3 text-sm font-medium">{{ (row as Payslip).fullName }}</td>
        <td class="px-4 py-3 text-sm text-slate-500">{{ (row as Payslip).employeeCode }}</td>
        <td class="px-4 py-3 text-sm font-semibold text-emerald-700">{{ (row as Payslip).workedDays?.toFixed(1) }}</td>
        <td class="px-4 py-3 text-sm">{{ fmtMoney((row as Payslip).grossSalary) }}</td>
        <td class="px-4 py-3 text-sm font-bold text-emerald-700">{{ fmtMoney((row as Payslip).netSalary) }}</td>
        <td class="px-4 py-3">
          <AppButton size="sm" variant="ghost" @click="router.push(`/payroll/payslips/${(row as Payslip).id}`)">Chi tiết</AppButton>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />
  </div>
</template>

