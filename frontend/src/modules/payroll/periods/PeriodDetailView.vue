<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { payrollPeriodService } from '../../../services/payrollPeriod.service'
import { payslipService } from '../../../services/payslip.service'
import { useToastStore } from '../../../stores/toast'
import type { PayrollPeriod, Payslip } from '../../../types/payroll.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import AppConfirm from '../../../components/ui/AppConfirm.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'

const route = useRoute()
const router = useRouter()
const toast = useToastStore()

const period = ref<PayrollPeriod | null>(null)
const payslips = ref<Payslip[]>([])
const loading = ref(true)
const calculating = ref(false)
const closing = ref(false)
const showCloseConfirm = ref(false)

const columns = [
  { key: 'employee', label: 'Nhân viên' }, { key: 'code', label: 'Mã NV' },
  { key: 'workDays', label: 'Ngày công' }, { key: 'paidLeave', label: 'Phép CL' },
  { key: 'gross', label: 'Gross' }, { key: 'deductions', label: 'Khấu trừ' },
  { key: 'net', label: 'Net lương' }, { key: 'actions', label: '' },
]

async function load() {
  const id = route.params.id as string
  loading.value = true
  try {
    [period.value, payslips.value] = await Promise.all([
      payrollPeriodService.getById(id),
      payslipService.getAll({ periodId: id }),
    ])
  } catch { toast.error('Không tìm thấy kỳ lương'); router.push('/payroll/periods') }
  finally { loading.value = false }
}

async function calculate() {
  if (!period.value) return
  calculating.value = true
  try {
    await payrollPeriodService.calculate(period.value.id)
    toast.success('Tính lương thành công! Đang làm mới...')
    await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Tính lương thất bại') }
  finally { calculating.value = false }
}

async function closePeriod() {
  if (!period.value) return
  closing.value = true
  try {
    await payrollPeriodService.close(period.value.id)
    toast.success('Đã đóng kỳ lương 🔒')
    showCloseConfirm.value = false
    await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Đóng kỳ thất bại') }
  finally { closing.value = false }
}

function fmt(d: string) { return new Date(d).toLocaleDateString('vi-VN') }
function fmtMoney(n: number) { return n.toLocaleString('vi-VN') + ' ₫' }

const { currentPage, perPage, paginatedData, total } = usePagination(payslips)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader
      :title="period?.name ?? '...'"
      :breadcrumbs="[{ label: 'Lương & Báo cáo' }, { label: 'Kỳ lương', to: '/payroll/periods' }, { label: period?.name ?? '' }]"
    >
      <template #actions>
        <template v-if="period && period.status !== 'Closed'">
          <AppButton variant="secondary" :loading="calculating" @click="calculate">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 11h.01M12 11h.01M15 11h.01M4 19h16a2 2 0 002-2V7a2 2 0 00-2-2H4a2 2 0 00-2 2v10a2 2 0 002 2z" /></svg>
            Tính lương
          </AppButton>
          <AppButton v-if="period.status === 'Calculated'" variant="success" @click="showCloseConfirm = true">
            🔒 Đóng kỳ lương
          </AppButton>
        </template>
      </template>
    </PageHeader>

    <!-- Period info -->
    <div v-if="period" class="mb-6 rounded-xl border border-slate-200 bg-white p-4">
      <div class="flex flex-wrap gap-6 text-sm">
        <div><span class="text-slate-500">Từ ngày:</span> <span class="ml-1 font-medium">{{ fmt(period.fromDate) }}</span></div>
        <div><span class="text-slate-500">Đến ngày:</span> <span class="ml-1 font-medium">{{ fmt(period.toDate) }}</span></div>
        <div><span class="text-slate-500">Quy tắc:</span> <span class="ml-1 font-medium">{{ period.payrollRuleName ?? '—' }}</span></div>
        <div><span class="text-slate-500">Trạng thái:</span> <span class="ml-1"><AppBadge :status="period.status" /></span></div>
        <div><span class="text-slate-500">Số phiếu:</span> <span class="ml-1 font-bold text-emerald-700">{{ payslips.length }}</span></div>
        <div v-if="payslips.length > 0"><span class="text-slate-500">Tổng Net:</span> <span class="ml-1 font-bold text-emerald-700">{{ fmtMoney(payslips.reduce((s, p) => s + p.netSalary, 0)) }}</span></div>
      </div>
    </div>

    <!-- Payslips table -->
    <AppTable :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có phiếu lương — hãy nhấn Tính lương">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-medium">{{ (row as Payslip).fullName }}</td>
        <td class="px-4 py-3 text-sm text-slate-500">{{ (row as Payslip).employeeCode }}</td>
        <td class="px-4 py-3 text-sm font-semibold text-emerald-700">{{ (row as Payslip).workedDays.toFixed(1) }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as Payslip).paidLeaveDays > 0 ? (row as Payslip).paidLeaveDays : '—' }}</td>
        <td class="px-4 py-3 text-sm font-medium">{{ fmtMoney((row as Payslip).grossSalary) }}</td>
        <td class="px-4 py-3 text-sm text-red-600">{{ (row as Payslip).totalDeduction > 0 ? fmtMoney((row as Payslip).totalDeduction) : '—' }}</td>
        <td class="px-4 py-3 text-sm font-bold text-emerald-700">{{ fmtMoney((row as Payslip).netSalary) }}</td>
        <td class="px-4 py-3">
          <AppButton size="sm" variant="ghost" @click="router.push(`/payroll/payslips/${(row as Payslip).id}`)">Xem</AppButton>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />

    <AppConfirm
      v-if="showCloseConfirm"
      title="Đóng kỳ lương"
      message="Sau khi đóng, kỳ lương sẽ không thể chỉnh sửa. Bạn có chắc chắn?"
      confirm-text="Đóng kỳ"
      :loading="closing"
      @confirm="closePeriod"
      @cancel="showCloseConfirm = false"
    />
  </div>
</template>

