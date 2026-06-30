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
const newTypeName = ref('')
const errors = ref<Record<string, string>>({})

const searchQuery = ref('')
const filterPeriod = ref('')
const filterType = ref('')

function clearFilters() {
  searchQuery.value = ''
  filterPeriod.value = ''
  filterType.value = ''
}

const columns = computed(() => {
  const list = [
    { key: 'period', label: 'Kỳ lương' },
  ]
  if (!auth.isEmployee) {
    list.push({ key: 'employee', label: 'Nhân viên' })
  }
  list.push(
    { key: 'type', label: 'Loại khấu trừ' },
    { key: 'amount', label: 'Số tiền' }
  )
  if (auth.isPayrollStaff) {
    list.push({ key: 'actions', label: '' })
  }
  return list
})

async function load() {
  loading.value = true
  try {
    const params: any = {}
    if (auth.isEmployee && auth.employeeId) {
      params.employeeId = auth.employeeId
    }
    const [dedData, typeData, periodData, empData] = await Promise.all([
      deductionService.getAll(params),
      deductionService.getTypes(),
      payrollPeriodService.getAll(),
      auth.isEmployee ? Promise.resolve([]) : employeeService.getAll(),
    ])

    // ĐÃ FIX: Lọc trùng lặp danh sách khấu trừ cho nhân viên (tránh lặp đúp hiển thị)
    const uniqueDeductions = new Map()
    dedData.forEach((item: any) => {
      const uniqueKey = `${item.payrollPeriodId}_${item.employeeId}_${item.deductionTypeId}`
      if (!uniqueDeductions.has(uniqueKey)) {
        uniqueDeductions.set(uniqueKey, item)
      }
    })
    deductions.value = Array.from(uniqueDeductions.values())

    // ĐÃ FIX: Lọc trùng lặp danh sách Loại khấu trừ (bỏ qua khác biệt chữ hoa/thường và khoảng trắng)
    const uniqueTypes = new Map()
    typeData.forEach((t: any) => {
      const normalizedKey = t.name.trim().toLowerCase()
      if (!uniqueTypes.has(normalizedKey)) {
        uniqueTypes.set(normalizedKey, t)
      }
    })
    types.value = Array.from(uniqueTypes.values())

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
  if (form.value.deductionTypeId === 'NEW_TYPE' && !newTypeName.value.trim()) {
    errors.value.newTypeName = 'Tên loại khấu trừ mới bắt buộc'
  }
  if (!form.value.amount || isNaN(Number(form.value.amount))) errors.value.amount = 'Số tiền hợp lệ bắt buộc'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    let typeId = form.value.deductionTypeId
    if (typeId === 'NEW_TYPE') {
      const createdType = await deductionService.createType(newTypeName.value.trim())
      typeId = createdType.id
      types.value.push(createdType)
    }

    const dto: CreateDeductionDto = {
      payrollPeriodId: form.value.payrollPeriodId,
      employeeId: form.value.employeeId,
      deductionTypeId: typeId,
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

// ĐÃ FIX: Ép kiểu dữ liệu về Number để Format tiền tệ chuẩn xác
function fmtMoney(n: any) {
  return Number(n).toLocaleString('vi-VN') + ' ₫'
}

const filteredDeductions = computed(() => {
  return deductions.value.filter((item) => {
    if (filterPeriod.value && item.payrollPeriodId !== filterPeriod.value) return false
    if (filterType.value && item.deductionTypeId !== filterType.value) return false
    if (searchQuery.value) {
      const q = searchQuery.value.toLowerCase().trim()
      const code = item.employeeCode?.toLowerCase() || ''
      const name = item.employeeName?.toLowerCase() || ''
      if (!code.includes(q) && !name.includes(q)) return false
    }
    return true
  })
})

const { currentPage, perPage, paginatedData, total } = usePagination(filteredDeductions)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Khấu trừ nhân viên" :breadcrumbs="[{ label: 'Lương & Báo cáo' }, { label: 'Khấu trừ' }]">
      <template #actions>
        <div class="flex gap-2">
          <AppButton variant="secondary" @click="handleExport">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
            </svg>
            <span>Xuất Excel</span>
          </AppButton>

          <template v-if="auth.isPayrollStaff">
            <AppButton variant="secondary" @click="showImportModal = true">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
              </svg>
              <span>Nhập Excel</span>
            </AppButton>

            <AppButton @click="form = { payrollPeriodId: '', employeeId: '', deductionTypeId: '', amount: '', notes: '' }; newTypeName = ''; errors = {}; showForm = true">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              <span>Thêm khấu trừ</span>
            </AppButton>
          </template>
        </div>
      </template>
    </PageHeader>

    <div class="mb-6 rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
      <div class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h3 class="text-sm font-semibold text-slate-800">Bộ lọc tìm kiếm</h3>
          <p class="text-xs text-slate-500">Tìm kiếm nhanh thông tin khấu trừ nhân viên</p>
        </div>
        
        <div class="flex flex-wrap gap-3 items-center">
          <div v-if="!auth.isEmployee" class="relative min-w-[200px] flex-1 md:flex-initial">
            <span class="absolute inset-y-0 left-3 flex items-center text-slate-400">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </span>
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Tên hoặc mã NV..."
              class="h-9 w-full rounded-xl border border-slate-200 bg-slate-50/50 pl-9 pr-3 text-sm outline-none transition-all focus:border-emerald-500 focus:bg-white focus:ring-1 focus:ring-emerald-500"
            />
          </div>

          <select
            v-model="filterPeriod"
            class="h-9 rounded-xl border border-slate-200 bg-white px-3 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-1 focus:ring-emerald-500"
          >
            <option value="">Tất cả kỳ lương</option>
            <option v-for="p in periods" :key="p.id" :value="p.id">{{ p.name }}</option>
          </select>

          <select
            v-model="filterType"
            class="h-9 rounded-xl border border-slate-200 bg-white px-3 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-1 focus:ring-emerald-500"
          >
            <option value="">Tất cả loại khấu trừ</option>
            <option v-for="t in types" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>
          
          <AppButton
            v-if="filterPeriod || filterType || searchQuery"
            variant="ghost"
            size="sm"
            class="text-slate-500 hover:text-slate-700"
            @click="clearFilters"
          >
            Xóa bộ lọc
          </AppButton>
        </div>
      </div>
    </div>

    <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có khấu trừ nào">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm">{{ (row as EmployeeDeduction).periodName ?? '—' }}</td>
        <td v-if="!auth.isEmployee" class="px-4 py-3 text-sm font-medium">{{ (row as EmployeeDeduction).employeeName ?? '—' }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ (row as EmployeeDeduction).deductionTypeName ?? '—' }}</td>
        <td class="px-4 py-3 text-sm font-medium text-rose-700">{{ fmtMoney((row as EmployeeDeduction).amount) }}</td>
        <td v-if="auth.isPayrollStaff" class="px-4 py-3 text-right">
          <AppButton size="sm" variant="danger" @click="deleteTarget = row as EmployeeDeduction">Xóa</AppButton>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />

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
            <option value="NEW_TYPE" class="text-emerald-600 font-medium">+ Thêm loại mới...</option>
          </select>
          <p v-if="errors.deductionTypeId" class="text-xs text-red-500">{{ errors.deductionTypeId }}</p>
        </div>

        <div v-if="form.deductionTypeId === 'NEW_TYPE'" class="flex flex-col gap-1 rounded-xl border border-slate-100 bg-slate-50/50 p-3">
          <label class="text-xs font-semibold text-slate-600 uppercase tracking-wider">Tên loại khấu trừ mới <span class="text-red-500">*</span></label>
          <input
            v-model="newTypeName"
            type="text"
            placeholder="Nhập tên loại khấu trừ mới..."
            :class="['h-9 rounded-lg border px-3 text-sm outline-none bg-white transition-all', errors.newTypeName ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']"
          />
          <p v-if="errors.newTypeName" class="text-xs text-red-500">{{ errors.newTypeName }}</p>
        </div>
        <AppInput id="ded-amount" v-model="form.amount" label="Số tiền (₫)" type="number" required :error="errors.amount" placeholder="VD: 100000" />
        <AppInput id="ded-notes" v-model="form.notes" label="Ghi chú" placeholder="Tùy chọn" />
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="showForm = false">Hủy</AppButton>
        <AppButton :loading="saving" @click="save">Thêm khấu trừ</AppButton>
      </template>
    </AppModal>

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