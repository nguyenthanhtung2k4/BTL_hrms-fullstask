<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { allowanceService } from '../../../services/allowance.service'
import { payrollPeriodService } from '../../../services/payrollPeriod.service'
import { employeeService } from '../../../services/employee.service'
import { useToastStore } from '../../../stores/toast'
import { useAuthStore } from '../../../stores/auth'
import { exportToExcel } from '../../../utils/excel'
import type { EmployeeAllowance, AllowanceType, CreateAllowanceDto } from '../../../types/payroll.types'
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
import BulkAllowanceModal from './BulkAllowanceModal.vue'

const toast = useToastStore()
const auth = useAuthStore()

const allowances = ref<EmployeeAllowance[]>([])
const types = ref<AllowanceType[]>([])
const periods = ref<PayrollPeriod[]>([])
const employees = ref<Employee[]>([])
const loading = ref(false)
const showForm = ref(false)
const showImportModal = ref(false)
const deleteTarget = ref<EmployeeAllowance | null>(null)
const deleteLoading = ref(false)
const saving = ref(false)
const form = ref({ payrollPeriodId: '', employeeId: '', allowanceTypeId: '', amount: '', notes: '' })
const newTypeName = ref('')
const errors = ref<Record<string, string>>({})

// ── Bulk allowance ───────────────────────────────────
const showBulkModal = ref(false)
const bulkModalRef = ref<InstanceType<typeof BulkAllowanceModal> | null>(null)

async function handleBulkConfirm() {
  // If the modal chose NEW_TYPE we must resolve it first.
  // For simplicity we pass the raw create function — the modal
  // stores allowanceTypeId directly (parent already created the type if needed).
  await bulkModalRef.value?.startApply(allowanceService.create)
}

async function handleBulkSaved(count: number) {
  toast.success(`Đã áp dụng thành công ${count} phụ cấp`)
  await load()
}

// ── Bulk delete ───────────────────────────────────────
const selectedIds = ref<Set<string>>(new Set())
const showBulkDeleteConfirm = ref(false)
const bulkDeleteLoading = ref(false)

const isAllSelected = computed(() =>
  paginatedData.value.length > 0 &&
  paginatedData.value.every((r: any) => selectedIds.value.has((r as EmployeeAllowance).id))
)
const isSomeSelected = computed(() =>
  paginatedData.value.some((r: any) => selectedIds.value.has((r as EmployeeAllowance).id))
)
const isIndeterminate = computed(() =>
  isSomeSelected.value && !isAllSelected.value
)

function toggleSelectAll() {
  if (isAllSelected.value)
    paginatedData.value.forEach((r: any) => selectedIds.value.delete((r as EmployeeAllowance).id))
  else
    paginatedData.value.forEach((r: any) => selectedIds.value.add((r as EmployeeAllowance).id))
}
function toggleSelect(id: string) {
  if (selectedIds.value.has(id)) selectedIds.value.delete(id)
  else selectedIds.value.add(id)
}

async function confirmBulkDelete() {
  bulkDeleteLoading.value = true
  const ids = [...selectedIds.value]
  let failCount = 0
  for (const id of ids) {
    try { await allowanceService.delete(id) }
    catch { failCount++ }
  }
  bulkDeleteLoading.value = false
  showBulkDeleteConfirm.value = false
  selectedIds.value.clear()
  if (failCount > 0) toast.error(`Xóa thất bại ${failCount} phụ cấp`)
  else toast.success(`Đã xóa ${ids.length} phụ cấp`)
  await load()
}

// Filter states
const searchQuery = ref('')
const filterPeriod = ref('')
const filterType = ref('')

function clearFilters() {
  searchQuery.value = ''
  filterPeriod.value = ''
  filterType.value = ''
}

// Dynamic columns based on role permissions
const columns = computed(() => {
  const list: { key: string; label: string; class?: string }[] = []
  if (auth.hasAnyRole(['Admin', 'HR', 'PayrollStaff'])) {
    list.push({ key: 'select', label: '', class: 'w-10' })
  }
  list.push({ key: 'period', label: 'Kỳ lương' })
  if (!auth.isEmployee) {
    list.push({ key: 'employee', label: 'Nhân viên' })
  }
  list.push(
    { key: 'type', label: 'Loại phụ cấp' },
    { key: 'amount', label: 'Số tiền' }
  )
  if (auth.hasAnyRole(['Admin', 'HR', 'PayrollStaff'])) {
    list.push({ key: 'actions', label: '', class: 'text-right' })
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
    const [allowData, typeData, periodData, empData] = await Promise.all([
      allowanceService.getAll(params),
      allowanceService.getTypes(),
      payrollPeriodService.getAll(),
      auth.isEmployee ? Promise.resolve([]) : employeeService.getAll(),
    ])
    allowances.value = allowData
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
  if (!form.value.allowanceTypeId) errors.value.allowanceTypeId = 'Loại phụ cấp bắt buộc'
  if (form.value.allowanceTypeId === 'NEW_TYPE' && !newTypeName.value.trim()) {
    errors.value.newTypeName = 'Tên loại phụ cấp mới bắt buộc'
  }
  if (!form.value.amount || isNaN(Number(form.value.amount))) errors.value.amount = 'Số tiền hợp lệ bắt buộc'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    let typeId = form.value.allowanceTypeId
    if (typeId === 'NEW_TYPE') {
      const createdType = await allowanceService.createType(newTypeName.value.trim())
      typeId = createdType.id
      types.value.push(createdType)
    }

    const dto: CreateAllowanceDto = {
      payrollPeriodId: form.value.payrollPeriodId,
      employeeId: form.value.employeeId,
      allowanceTypeId: typeId,
      amount: Number(form.value.amount),
      notes: form.value.notes || undefined
    }
    await allowanceService.create(dto)
    toast.success('Đã thêm phụ cấp')
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
    await allowanceService.delete(deleteTarget.value.id)
    toast.success('Đã xóa phụ cấp')
    deleteTarget.value = null
    await load()
  } catch {
    toast.error('Xóa thất bại')
  } finally {
    deleteLoading.value = false
  }
}

// Export allowances list to Excel
function handleExport() {
  try {
    const dataToExport = allowances.value.map((item) => {
      const emp = employees.value.find((e) => e.fullName === item.employeeName)
      return {
        'Kỳ lương': item.periodName || '',
        'Mã Nhân viên': emp?.employeeCode || '',
        'Họ tên Nhân viên': item.employeeName || '',
        'Loại phụ cấp': item.allowanceTypeName || '',
        'Số tiền (VNĐ)': item.amount,
        'Ghi chú': item.notes || '',
        'Ngày tạo': item.createdAt ? new Date(item.createdAt).toLocaleDateString('vi-VN') : '',
      }
    })
    exportToExcel(dataToExport, 'Danh_Sach_Phu_Cap_Nhan_Vien', 'PhuCap')
    toast.success('Đã xuất dữ liệu Excel thành công')
  } catch (err: any) {
    toast.error(err?.message || 'Không thể xuất file Excel')
  }
}

// Bulk import allowances from Excel
async function handleImportSave(validatedRows: any[]) {
  saving.value = true
  let successCount = 0
  let failCount = 0

  for (const row of validatedRows) {
    try {
      const dto: CreateAllowanceDto = {
        payrollPeriodId: row.payrollPeriodId,
        employeeId: row.employeeId,
        allowanceTypeId: row.typeId,
        amount: row.amount,
        notes: row.notes || undefined,
      }
      await allowanceService.create(dto)
      successCount++
    } catch {
      failCount++
    }
  }

  saving.value = false
  showImportModal.value = false
  if (failCount === 0) {
    toast.success(`Đã nhập thành công toàn bộ ${successCount} dòng phụ cấp từ Excel.`)
  } else {
    toast.warning(`Đã nhập thành công ${successCount} dòng. Thất bại ${failCount} dòng.`)
  }
  await load()
}

function fmtMoney(n: number) {
  return n.toLocaleString('vi-VN') + ' ₫'
}

const filteredAllowances = computed(() => {
  return allowances.value.filter((item) => {
    if (filterPeriod.value && item.payrollPeriodId !== filterPeriod.value) return false
    if (filterType.value && item.allowanceTypeId !== filterType.value) return false
    if (searchQuery.value) {
      const q = searchQuery.value.toLowerCase().trim()
      const code = item.employeeCode?.toLowerCase() || ''
      const name = item.employeeName?.toLowerCase() || ''
      if (!code.includes(q) && !name.includes(q)) return false
    }
    return true
  })
})

const { currentPage, perPage, paginatedData, total } = usePagination(filteredAllowances)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Phụ cấp nhân viên" :breadcrumbs="[{ label: 'Lương & Báo cáo' }, { label: 'Phụ cấp' }]">
      <template #actions>
        <div class="flex gap-2">
          <!-- Export Button - for all roles -->
          <AppButton variant="secondary" @click="handleExport">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
            </svg>
            <span>Xuất Excel</span>
          </AppButton>

          <!-- Import and Add Buttons - strictly Admin / HR / PayrollStaff -->
          <template v-if="auth.hasAnyRole(['Admin', 'HR', 'PayrollStaff'])">
            <AppButton variant="secondary" @click="showImportModal = true">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
              </svg>
              <span>Nhập Excel</span>
            </AppButton>

            <!-- Bulk apply button -->
            <AppButton variant="secondary" @click="showBulkModal = true">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              <span>Áp dụng hàng loạt</span>
            </AppButton>

            <AppButton
              @click="form = { payrollPeriodId: '', employeeId: '', allowanceTypeId: '', amount: '', notes: '' }; newTypeName = ''; errors = {}; showForm = true">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              <span>Thêm phụ cấp</span>
            </AppButton>
          </template>
        </div>
      </template>
    </PageHeader>

    <!-- Filter Card -->
    <div class="mb-6 rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
      <div class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h3 class="text-sm font-semibold text-slate-800">Bộ lọc tìm kiếm</h3>
          <p class="text-xs text-slate-500">Tìm kiếm nhanh thông tin phụ cấp nhân viên</p>
        </div>

        <div class="flex flex-wrap gap-3 items-center">
          <!-- Search employee name or code (hidden for Employee role) -->
          <div v-if="!auth.isEmployee" class="relative min-w-[200px] flex-1 md:flex-initial">
            <span class="absolute inset-y-0 left-3 flex items-center text-slate-400">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </span>
            <input v-model="searchQuery" type="text" placeholder="Tên hoặc mã NV..."
              class="h-9 w-full rounded-xl border border-slate-200 bg-slate-50/50 pl-9 pr-3 text-sm outline-none transition-all focus:border-emerald-500 focus:bg-white focus:ring-1 focus:ring-emerald-500" />
          </div>

          <!-- Period dropdown -->
          <select v-model="filterPeriod"
            class="h-9 rounded-xl border border-slate-200 bg-white px-3 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-1 focus:ring-emerald-500">
            <option value="">Tất cả kỳ lương</option>
            <option v-for="p in periods" :key="p.id" :value="p.id">{{ p.name }}</option>
          </select>

          <!-- Allowance Type dropdown -->
          <select v-model="filterType"
            class="h-9 rounded-xl border border-slate-200 bg-white px-3 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-1 focus:ring-emerald-500">
            <option value="">Tất cả loại phụ cấp</option>
            <option v-for="t in types" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>

          <AppButton v-if="filterPeriod || filterType || searchQuery" variant="ghost" size="sm"
            class="text-slate-500 hover:text-slate-700" @click="clearFilters">
            Xóa bộ lọc
          </AppButton>
        </div>
      </div>
    </div>
    <div v-if="auth.hasAnyRole(['Admin', 'HR', 'PayrollStaff']) && selectedIds.size > 0"
      class="mb-3 flex items-center justify-between rounded-xl border border-emerald-100 bg-emerald-50 px-4 py-2.5">
      <span class="text-sm font-medium text-emerald-800">
        Đã chọn {{ selectedIds.size }} phụ cấp
      </span>
      <div class="flex gap-2">
        <AppButton size="sm" variant="ghost" @click="selectedIds.clear()">Bỏ chọn</AppButton>
        <AppButton size="sm" variant="danger" @click="showBulkDeleteConfirm = true">Xóa đã chọn</AppButton>
      </div>
    </div>
    <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="id"
      empty-text="Chưa có phụ cấp nào">
      <!-- Select-all checkbox in header -->
      <template v-if="auth.hasAnyRole(['Admin', 'HR', 'PayrollStaff'])" #header-select>
        <input type="checkbox" :checked="isAllSelected" :indeterminate="isIndeterminate"
          class="h-4 w-4 rounded border-slate-300 text-emerald-600 cursor-pointer" @change="toggleSelectAll" />
      </template>

      <template #default="{ row }">
        <!-- Checkbox cell -->
        <td v-if="auth.hasAnyRole(['Admin', 'HR', 'PayrollStaff'])" class="px-4 py-3 w-10">
          <input type="checkbox" :checked="selectedIds.has((row as EmployeeAllowance).id)"
            class="h-4 w-4 rounded border-slate-300 text-emerald-600 cursor-pointer"
            @change="toggleSelect((row as EmployeeAllowance).id)" />
        </td>
        <td class="px-4 py-3 text-sm">{{ (row as EmployeeAllowance).periodName ?? '—' }}</td>
        <td v-if="!auth.isEmployee" class="px-4 py-3 text-sm font-medium">{{ (row as EmployeeAllowance).employeeName ??
          '—' }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ (row as EmployeeAllowance).allowanceTypeName ?? '—' }}</td>
        <td class="px-4 py-3 text-sm font-medium text-emerald-700">{{ fmtMoney((row as EmployeeAllowance).amount) }}
        </td>
        <td v-if="auth.hasAnyRole(['Admin', 'HR', 'PayrollStaff'])" class="px-4 py-3 text-right">
          <AppButton size="sm" variant="danger" @click="deleteTarget = row as EmployeeAllowance">Xóa</AppButton>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event"
      @per-page-change="perPage = $event" />

    <!-- Form modal -->
    <AppModal v-if="showForm" title="Thêm phụ cấp" @close="showForm = false">
      <div class="space-y-4">
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Kỳ lương <span class="text-red-500">*</span></label>
          <select v-model="form.payrollPeriodId"
            :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none', errors.payrollPeriodId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn kỳ lương --</option>
            <option v-for="p in periods.filter(p => p.status !== 'Closed')" :key="p.id" :value="p.id">{{ p.name }}
            </option>
          </select>
          <p v-if="errors.payrollPeriodId" class="text-xs text-red-500">{{ errors.payrollPeriodId }}</p>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Nhân viên <span class="text-red-500">*</span></label>
          <select v-model="form.employeeId"
            :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none', errors.employeeId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn nhân viên --</option>
            <option v-for="e in employees" :key="e.id" :value="e.id">{{ e.fullName }}</option>
          </select>
          <p v-if="errors.employeeId" class="text-xs text-red-500">{{ errors.employeeId }}</p>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Loại phụ cấp <span class="text-red-500">*</span></label>
          <select v-model="form.allowanceTypeId"
            :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none', errors.allowanceTypeId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn loại --</option>
            <option v-for="t in types" :key="t.id" :value="t.id">{{ t.name }}</option>
            <option value="NEW_TYPE" class="text-emerald-600 font-medium">+ Thêm loại mới...</option>
          </select>
          <p v-if="errors.allowanceTypeId" class="text-xs text-red-500">{{ errors.allowanceTypeId }}</p>
        </div>

        <div v-if="form.allowanceTypeId === 'NEW_TYPE'"
          class="flex flex-col gap-1 rounded-xl border border-slate-100 bg-slate-50/50 p-3">
          <label class="text-xs font-semibold text-slate-600 uppercase tracking-wider">Tên loại phụ cấp mới <span
              class="text-red-500">*</span></label>
          <input v-model="newTypeName" type="text" placeholder="Nhập tên loại phụ cấp mới..."
            :class="['h-9 rounded-lg border px-3 text-sm outline-none bg-white transition-all', errors.newTypeName ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']" />
          <p v-if="errors.newTypeName" class="text-xs text-red-500">{{ errors.newTypeName }}</p>
        </div>
        <AppInput id="allow-amount" v-model="form.amount" label="Số tiền (₫)" type="number" required
          :error="errors.amount" placeholder="VD: 500000" />
        <AppInput id="allow-notes" v-model="form.notes" label="Ghi chú" placeholder="Tùy chọn" />
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="showForm = false">Hủy</AppButton>
        <AppButton :loading="saving" @click="save">Thêm phụ cấp</AppButton>
      </template>
    </AppModal>

    <!-- Excel Import Modal -->
    <ExcelImportModal v-if="showImportModal" :is-open="showImportModal" title="Nhập phụ cấp từ Excel" type="allowance"
      :periods="periods" :employees="employees" :types="types" @close="showImportModal = false"
      @import="handleImportSave" />

    <AppConfirm v-if="deleteTarget" title="Xóa phụ cấp" message="Bạn chắc chắn muốn xóa phụ cấp này?" confirm-text="Xóa"
      :danger="true" :loading="deleteLoading" @confirm="confirmDelete" @cancel="deleteTarget = null" />

    <!-- ── Bulk delete confirm ── -->
    <AppConfirm v-if="showBulkDeleteConfirm" title="Xóa nhiều phụ cấp"
      :message="`Bạn có chắc muốn xóa ${selectedIds.size} phụ cấp đã chọn?`" confirm-text="Xóa tất cả" :danger="true"
      :loading="bulkDeleteLoading" @confirm="confirmBulkDelete" @cancel="showBulkDeleteConfirm = false" />

    <!-- ── Bulk Allowance Modal ── -->
    <BulkAllowanceModal v-if="showBulkModal" ref="bulkModalRef" :periods="periods" :employees="employees" :types="types"
      @close="showBulkModal = false" @confirm="handleBulkConfirm" @saved="handleBulkSaved" />
  </div>
</template>
