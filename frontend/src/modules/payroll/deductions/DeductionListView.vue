<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { deductionService } from '../../../services/deduction.service'
import { payrollPeriodService } from '../../../services/payrollPeriod.service'
import { employeeService } from '../../../services/employee.service'
import { useToastStore } from '../../../stores/toast'
import { useAuthStore } from '../../../stores/auth'
import { exportToExcel } from '../../../utils/excel'
import type { EmployeeDeduction, DeductionType, CreateDeductionDto } from '../../../types/payroll.types'
import type { Employee } from '../../../types/hr.types'
import type { PayrollPeriod } from '../../../types/payroll.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppModal from '../../../components/ui/AppModal.vue'
import AppInput from '../../../components/ui/AppInput.vue'
import AppConfirm from '../../../components/ui/AppConfirm.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import ExcelImportModal from '../../../components/ui/ExcelImportModal.vue'
import { usePagination } from '../../../composables/usePagination'

const toast = useToastStore()
const auth = useAuthStore()

const deductions = ref<EmployeeDeduction[]>([])
const types = ref<DeductionType[]>([])
const periods = ref<PayrollPeriod[]>([])
const employees = ref<Employee[]>([])
const loading = ref(false)
const showForm = ref(false)
const showImportModal = ref(false)
const deleteTarget = ref<EmployeeDeduction | null>(null)
const deleteLoading = ref(false)
const saving = ref(false)
const form = ref({ payrollPeriodId: '', employeeId: '', deductionTypeId: '', amount: '', notes: '' })
const errors = ref<Record<string, string>>({})

// Dynamic columns based on role permissions
const columns = computed(() => {
  const list = [
    { key: 'period', label: 'Kỳ lương' },
    { key: 'employee', label: 'Nhân viên' },
    { key: 'type', label: 'Loại khấu trừ' },
    { key: 'amount', label: 'Số tiền' },
  ]
  if (auth.isPayrollStaff) {
    list.push({ key: 'actions', label: '' })
  }
  return list
})

async function load() {
  loading.value = true
  try {
    const [dedData, typeData, periodData, empData] = await Promise.all([
      deductionService.getAll(),
      deductionService.getTypes(),
      payrollPeriodService.getAll(),
      employeeService.getAll(),
    ])
    deductions.value = dedData
    types.value = typeData
    periods.value = periodData
    employees.value = empData
  } catch {
    toast.error('Không thể tải dữ liệu')
  } finally {
    loading.value = false
  }
}

function validate() {
  errors.value = {}
  if (!form.value.payrollPeriodId) errors.value.payrollPeriodId = 'Kỳ lương bắt buộc'
  if (!form.value.employeeId) errors.value.employeeId = 'Nhân viên bắt buộc'
  if (!form.value.deductionTypeId) errors.value.deductionTypeId = 'Loại khấu trừ bắt buộc'
  if (!form.value.amount || isNaN(Number(form.value.amount))) errors.value.amount = 'Số tiền hợp lệ bắt buộc'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    const dto: CreateDeductionDto = {
      payrollPeriodId: form.value.payrollPeriodId,
      employeeId: form.value.employeeId,
      deductionTypeId: form.value.deductionTypeId,
      amount: Number(form.value.amount),
      notes: form.value.notes || undefined
    }
    await deductionService.create(dto)
    toast.success('Đã thêm khấu trừ')
    showForm.value = false
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message ?? 'Lưu thất bại')
  } finally {
    saving.value = false
  }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try {
    await deductionService.delete(deleteTarget.value.id)
    toast.success('Đã xóa khấu trừ')
    deleteTarget.value = null
    await load()
  } catch {
    toast.error('Xóa thất bại')
  } finally {
    deleteLoading.value = false
  }
}

// Export deductions list to Excel
function handleExport() {
  try {
    const dataToExport = deductions.value.map((item) => {
      const emp = employees.value.find((e) => e.fullName === item.employeeName)
      return {
        'Kỳ lương': item.periodName || '',
        'Mã Nhân viên': emp?.employeeCode || '',
        'Họ tên Nhân viên': item.employeeName || '',
        'Loại khấu trừ': item.deductionTypeName || '',
        'Số tiền (VNĐ)': item.amount,
        'Ghi chú': item.notes || '',
        'Ngày tạo': item.createdAt ? new Date(item.createdAt).toLocaleDateString('vi-VN') : '',
      }
    })
    exportToExcel(dataToExport, 'Danh_Sach_Khau_Tru_Nhan_Vien', 'KhauTru')
    toast.success('Đã xuất dữ liệu Excel thành công')
  } catch (err: any) {
    toast.error(err?.message || 'Không thể xuất file Excel')
  }
}

// Bulk import deductions from Excel
async function handleImportSave(validatedRows: any[]) {
  saving.value = true
  let successCount = 0
  let failCount = 0

  for (const row of validatedRows) {
    try {
      const dto: CreateDeductionDto = {
        payrollPeriodId: row.payrollPeriodId,
        employeeId: row.employeeId,
        deductionTypeId: row.typeId,
        amount: row.amount,
        notes: row.notes || undefined,
      }
      await deductionService.create(dto)
      successCount++
    } catch {
      failCount++
    }
  }

  saving.value = false
  showImportModal.value = false
  if (failCount === 0) {
    toast.success(`Đã nhập thành công toàn bộ ${successCount} dòng khấu trừ từ Excel.`)
  } else {
    toast.warning(`Đã nhập thành công ${successCount} dòng. Thất bại ${failCount} dòng.`)
  }
  await load()
}

function fmtMoney(n: number) {
  return n.toLocaleString('vi-VN') + ' ₫'
}

const { currentPage, perPage, paginatedData, total } = usePagination(deductions)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Khấu trừ nhân viên" :breadcrumbs="[{ label: 'Lương & Báo cáo' }, { label: 'Khấu trừ' }]">
      <template #actions>
        <div class="flex gap-2">
          <!-- Export Button - for all roles -->
          <AppButton variant="secondary" @click="handleExport">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
            </svg>
            <span>Xuất Excel</span>
          </AppButton>

          <!-- Import and Add Buttons - strictly Admin / PayrollStaff -->
          <template v-if="auth.isPayrollStaff">
            <AppButton variant="secondary" @click="showImportModal = true">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
              </svg>
              <span>Nhập Excel</span>
            </AppButton>

            <AppButton @click="form = { payrollPeriodId: '', employeeId: '', deductionTypeId: '', amount: '', notes: '' }; errors = {}; showForm = true">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              <span>Thêm khấu trừ</span>
            </AppButton>
          </template>
        </div>
      </template>
    </PageHeader>

    <AppTable :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có khấu trừ nào">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm">{{ (row as EmployeeDeduction).periodName ?? '—' }}</td>
        <td class="px-4 py-3 text-sm font-medium">{{ (row as EmployeeDeduction).employeeName ?? '—' }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ (row as EmployeeDeduction).deductionTypeName ?? '—' }}</td>
        <td class="px-4 py-3 text-sm font-medium text-rose-700">{{ fmtMoney((row as EmployeeDeduction).amount) }}</td>
        <td v-if="auth.isPayrollStaff" class="px-4 py-3 text-right">
          <AppButton size="sm" variant="danger" @click="deleteTarget = row as EmployeeDeduction">Xóa</AppButton>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />

    <!-- Form modal -->
    <AppModal v-if="showForm" title="Thêm khấu trừ" @close="showForm = false">
      <div class="space-y-4">
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Kỳ lương <span class="text-red-500">*</span></label>
          <select v-model="form.payrollPeriodId" :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none', errors.payrollPeriodId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn kỳ lương --</option>
            <option v-for="p in periods.filter(p => p.status !== 'Closed')" :key="p.id" :value="p.id">{{ p.name }}</option>
          </select>
          <p v-if="errors.payrollPeriodId" class="text-xs text-red-500">{{ errors.payrollPeriodId }}</p>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Nhân viên <span class="text-red-500">*</span></label>
          <select v-model="form.employeeId" :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none', errors.employeeId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn nhân viên --</option>
            <option v-for="e in employees" :key="e.id" :value="e.id">{{ e.fullName }}</option>
          </select>
          <p v-if="errors.employeeId" class="text-xs text-red-500">{{ errors.employeeId }}</p>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Loại khấu trừ <span class="text-red-500">*</span></label>
          <select v-model="form.deductionTypeId" :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none', errors.deductionTypeId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn loại --</option>
            <option v-for="t in types" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>
          <p v-if="errors.deductionTypeId" class="text-xs text-red-500">{{ errors.deductionTypeId }}</p>
        </div>
        <AppInput id="ded-amount" v-model="form.amount" label="Số tiền (₫)" type="number" required :error="errors.amount" placeholder="VD: 100000" />
        <AppInput id="ded-notes" v-model="form.notes" label="Ghi chú" placeholder="Tùy chọn" />
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="showForm = false">Hủy</AppButton>
        <AppButton :loading="saving" @click="save">Thêm khấu trừ</AppButton>
      </template>
    </AppModal>

    <!-- Excel Import Modal -->
    <ExcelImportModal
      v-if="showImportModal"
      :is-open="showImportModal"
      title="Nhập khấu trừ từ Excel"
      type="deduction"
      :periods="periods"
      :employees="employees"
      :types="types"
      @close="showImportModal = false"
      @import="handleImportSave"
    />

    <AppConfirm v-if="deleteTarget" title="Xóa khấu trừ" message="Bạn chắc chắn muốn xóa khấu trừ này?" confirm-text="Xóa" :danger="true" :loading="deleteLoading" @confirm="confirmDelete" @cancel="deleteTarget = null" />
  </div>
</template>
