<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { payrollPeriodService } from '../../../services/payrollPeriod.service'
import { payrollRuleService } from '../../../services/payrollRule.service'
import { useToastStore } from '../../../stores/toast'
import { useAuthStore } from '../../../stores/auth'
import type { PayrollPeriod, PayrollRule, CreatePayrollPeriodDto } from '../../../types/payroll.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import AppModal from '../../../components/ui/AppModal.vue'
import AppInput from '../../../components/ui/AppInput.vue'
import AppConfirm from '../../../components/ui/AppConfirm.vue'
import { useRouter } from 'vue-router'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'
import BulkCreatePeriodsModal from './BulkCreatePeriodsModal.vue'

const toast = useToastStore()
const router = useRouter()
const auth = useAuthStore()

const periods = ref<PayrollPeriod[]>([])
const rules = ref<PayrollRule[]>([])
const loading = ref(false)
const showForm = ref(false)
const editTarget = ref<PayrollPeriod | null>(null)
const deleteTarget = ref<PayrollPeriod | null>(null)
const deleteLoading = ref(false)
const saving = ref(false)
const form = ref({ code: '', name: '', fromDate: '', toDate: '', payrollRuleId: '' })
const errors = ref<Record<string, string>>({})

// ── Bulk Create ───────────────────────────────────────────
const showBulkCreate = ref(false)
const bulkModalRef = ref<InstanceType<typeof BulkCreatePeriodsModal> | null>(null)

const existingCodes = computed(() => periods.value.map(p => p.code))

async function handleBulkConfirm() {
  await bulkModalRef.value?.startCreate(payrollPeriodService.create)
}

async function handleBulkCreated(count: number) {
  toast.success(`Đã tạo thành công ${count} kỳ lương`)
  await load()
}

// ── Search ────────────────────────────────────────────────
const searchQuery = ref('')

const filteredPeriods = computed(() => {
  let list = periods.value
  
  if (!auth.isPayrollStaff) {
    list = list.filter(p => p.status === 'Calculated' || p.status === 'Closed')
  }

  const q = searchQuery.value.trim().toLowerCase()
  if (!q) return list
  return list.filter(p =>
    p.name.toLowerCase().includes(q) ||
    p.code.toLowerCase().includes(q)
  )
})

// ── Multi-select ──────────────────────────────────────────
const selectedIds = ref<Set<string>>(new Set())
const showBulkDeleteConfirm = ref(false)
const bulkDeleteLoading = ref(false)

const isAllSelected = computed(() => {
  const deletable = paginatedData.value.filter((r: PayrollPeriod) => (r as PayrollPeriod).status !== 'Closed')
  return deletable.length > 0 && deletable.every((r: PayrollPeriod) => selectedIds.value.has((r as PayrollPeriod).id))
})

const isIndeterminate = computed(() => {
  const deletable = paginatedData.value.filter((r: PayrollPeriod) => (r as PayrollPeriod).status !== 'Closed')
  const checked = deletable.filter((r: PayrollPeriod) => selectedIds.value.has((r as PayrollPeriod).id))
  return checked.length > 0 && checked.length < deletable.length
})

function toggleSelectAll() {
  const deletable = paginatedData.value.filter((r: PayrollPeriod) => (r as PayrollPeriod).status !== 'Closed')
  if (isAllSelected.value) {
    deletable.forEach((r: PayrollPeriod) => selectedIds.value.delete((r as PayrollPeriod).id))
  } else {
    deletable.forEach((r: PayrollPeriod) => selectedIds.value.add((r as PayrollPeriod).id))
  }
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
    try { await payrollPeriodService.delete(id) }
    catch { failCount++ }
  }
  bulkDeleteLoading.value = false
  showBulkDeleteConfirm.value = false
  selectedIds.value.clear()
  if (failCount > 0) toast.error(`Xóa thất bại ${failCount} kỳ (đã có phiếu lương)`)
  else toast.success(`Đã xóa ${ids.length} kỳ lương`)
  await load()
}

// ── Table columns ─────────────────────────────────────────
const columns = computed(() => {
  const cols = [
    { key: 'name', label: 'Tên kỳ lương' },
    { key: 'from', label: 'Từ ngày' },
    { key: 'to', label: 'Đến ngày' },
    { key: 'rule', label: 'Quy tắc' },
    { key: 'status', label: 'Trạng thái' },
    { key: 'actions', label: '', class: 'text-right' }
  ]
  if (auth.isPayrollStaff) {
    cols.unshift({ key: 'select', label: '', class: 'w-10' })
  }
  return cols
})

async function load() {
  loading.value = true
  try { [periods.value, rules.value] = await Promise.all([payrollPeriodService.getAll(), payrollRuleService.getAll()]) }
  catch { toast.error('Không thể tải dữ liệu') }
  finally { loading.value = false }
}

function ruleName(id: string) { return rules.value.find(r => r.id === id)?.name ?? '—' }

function openCreate() {
  editTarget.value = null
  const now = new Date()
  const mm = String(now.getMonth() + 1).padStart(2, '0')
  const yy = now.getFullYear()
  form.value = {
    code: `KY-${mm}-${yy}`,
    name: `Lương tháng ${now.getMonth() + 1}/${yy}`,
    fromDate: new Date(yy, now.getMonth(), 1).toISOString().split('T')[0],
    toDate: new Date(yy, now.getMonth() + 1, 0).toISOString().split('T')[0],
    payrollRuleId: rules.value[0]?.id ?? ''
  }
  errors.value = {}; showForm.value = true
}

function editPeriod(row: PayrollPeriod) {
  editTarget.value = row
  form.value = {
    code: row.code,
    name: row.name,
    fromDate: row.fromDate.split('T')[0],
    toDate: row.toDate.split('T')[0],
    payrollRuleId: row.payrollRuleId
  }
  errors.value = {}
  showForm.value = true
}

function validate() {
  errors.value = {}
  if (!form.value.code.trim()) errors.value.code = 'Mã kỳ bắt buộc'
  if (!form.value.name.trim()) errors.value.name = 'Tên bắt buộc'
  if (!form.value.fromDate) errors.value.fromDate = 'Từ ngày bắt buộc'
  if (!form.value.toDate) errors.value.toDate = 'Đến ngày bắt buộc'
  if (!form.value.payrollRuleId) errors.value.payrollRuleId = 'Quy tắc bắt buộc'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (editTarget.value) {
      await payrollPeriodService.update(editTarget.value.id, form.value as any)
      toast.success('Cập nhật kỳ lương thành công')
    } else {
      await payrollPeriodService.create(form.value as CreatePayrollPeriodDto)
      toast.success('Tạo kỳ lương thành công')
    }
    showForm.value = false; await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Lưu thất bại') }
  finally { saving.value = false }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try { await payrollPeriodService.delete(deleteTarget.value.id); toast.success('Đã xóa kỳ lương'); deleteTarget.value = null; await load() }
  catch { toast.error('Xóa thất bại — kỳ đã có phiếu lương') }
  finally { deleteLoading.value = false }
}

function fmt(d: string) { return new Date(d).toLocaleDateString('vi-VN') }

const { currentPage, perPage, paginatedData, total } = usePagination(filteredPeriods)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Kỳ lương" subtitle="Quản lý các kỳ tính lương" :breadcrumbs="[{ label: 'Lương & Báo cáo' }, { label: 'Kỳ lương' }]">
      <template #actions v-if="auth.isPayrollStaff">
        <AppButton variant="secondary" @click="showBulkCreate = true">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
          Tạo hàng loạt
        </AppButton>
        <AppButton @click="openCreate">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
          Tạo kỳ lương
        </AppButton>
      </template>
    </PageHeader>

    <!-- ── Toolbar: Search + Bulk actions ── -->
    <div class="mb-4 flex flex-wrap items-center gap-3">
      <!-- Search -->
      <div class="relative flex-1 min-w-[220px] max-w-sm">
        <svg class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-4.35-4.35M17 11A6 6 0 1 1 5 11a6 6 0 0 1 12 0z" />
        </svg>
        <input
          id="period-search"
          v-model="searchQuery"
          type="text"
          placeholder="Tìm theo tên hoặc mã kỳ lương…"
          class="w-full h-9 pl-9 pr-3 rounded-lg border border-slate-300 bg-white text-sm outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 transition"
        />
        <button v-if="searchQuery" @click="searchQuery = ''" class="absolute right-2 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
        </button>
      </div>

      <!-- Bulk delete button (visible when items are selected) -->
      <Transition name="fade-slide">
        <div v-if="selectedIds.size > 0" class="flex items-center gap-2">
          <span class="text-sm text-slate-500">Đã chọn <strong class="text-slate-700">{{ selectedIds.size }}</strong> kỳ</span>
          <AppButton variant="danger" size="sm" @click="showBulkDeleteConfirm = true">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6M9 7h6m-7 0a1 1 0 011-1h4a1 1 0 011 1m-7 0H5m14 0h-2" /></svg>
            Xóa {{ selectedIds.size }} kỳ đã chọn
          </AppButton>
          <button class="text-xs text-slate-400 hover:text-slate-600 underline" @click="selectedIds.clear()">Bỏ chọn</button>
        </div>
      </Transition>

      <!-- Result count -->
      <span v-if="searchQuery" class="ml-auto text-sm text-slate-400">{{ total }} kết quả</span>
    </div>

    <!-- ── Table ── -->
    <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có kỳ lương nào">
      <!-- Custom header for checkbox column -->
      <template #header-select v-if="auth.isPayrollStaff">
        <input
          type="checkbox"
          :checked="isAllSelected"
          :indeterminate="isIndeterminate"
          class="h-4 w-4 rounded border-slate-300 text-emerald-600 cursor-pointer"
          @change="toggleSelectAll"
        />
      </template>

      <template #default="{ row }">
        <!-- Checkbox -->
        <td v-if="auth.isPayrollStaff" class="px-4 py-3 w-10">
          <input
            v-if="(row as PayrollPeriod).status !== 'Closed'"
            type="checkbox"
            :checked="selectedIds.has((row as PayrollPeriod).id)"
            class="h-4 w-4 rounded border-slate-300 text-emerald-600 cursor-pointer"
            @change="toggleSelect((row as PayrollPeriod).id)"
          />
        </td>
        <td class="px-4 py-3 text-sm font-medium">{{ (row as PayrollPeriod).name }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ fmt((row as PayrollPeriod).fromDate) }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ fmt((row as PayrollPeriod).toDate) }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ ruleName((row as PayrollPeriod).payrollRuleId) }}</td>
        <td class="px-4 py-3"><AppBadge :status="(row as PayrollPeriod).status" /></td>
        <td class="px-4 py-3 text-right">
          <div class="flex justify-end gap-1.5">
            <AppButton size="sm" variant="ghost" @click="router.push(`/payroll/periods/${(row as PayrollPeriod).id}`)">Chi tiết</AppButton>
            <template v-if="auth.isPayrollStaff && (row as PayrollPeriod).status !== 'Closed'">
              <AppButton size="sm" variant="secondary" @click="editPeriod(row as PayrollPeriod)">Sửa</AppButton>
              <AppButton size="sm" variant="danger" @click="deleteTarget = row as PayrollPeriod">Xóa</AppButton>
            </template>
          </div>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />

    <!-- ── Create / Edit Modal ── -->
    <AppModal v-if="showForm" :title="editTarget ? 'Sửa kỳ lương' : 'Tạo kỳ lương'" @close="showForm = false">
      <div class="space-y-4">
        <div class="grid grid-cols-2 gap-3">
          <AppInput id="pp-code" v-model="form.code" label="Mã kỳ lương" required :disabled="!!editTarget" :error="errors.code" placeholder="VD: KY-06-2026" />
          <AppInput id="pp-name" v-model="form.name" label="Tên kỳ lương" required :error="errors.name" placeholder="VD: Lương tháng 06/2026" />
        </div>
        <div class="grid grid-cols-2 gap-3">
          <AppInput id="pp-from" v-model="form.fromDate" label="Từ ngày" type="date" required :error="errors.fromDate" />
          <AppInput id="pp-to" v-model="form.toDate" label="Đến ngày" type="date" required :error="errors.toDate" />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Quy tắc lương <span class="text-red-500">*</span></label>
          <select v-model="form.payrollRuleId" :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none', errors.payrollRuleId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn quy tắc --</option>
            <option v-for="r in rules.filter(r => r.isActive)" :key="r.id" :value="r.id">{{ r.name }}</option>
          </select>
          <p v-if="errors.payrollRuleId" class="text-xs text-red-500">{{ errors.payrollRuleId }}</p>
        </div>
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="showForm = false">Hủy</AppButton>
        <AppButton :loading="saving" @click="save">{{ editTarget ? 'Cập nhật' : 'Tạo mới' }}</AppButton>
      </template>
    </AppModal>

    <!-- ── Single delete confirm ── -->
    <AppConfirm v-if="deleteTarget" title="Xóa kỳ lương" :message="`Xóa kỳ &quot;${deleteTarget.name}&quot;?`" confirm-text="Xóa" :danger="true" :loading="deleteLoading" @confirm="confirmDelete" @cancel="deleteTarget = null" />

    <!-- ── Bulk Create Modal ── -->
    <BulkCreatePeriodsModal
      v-if="showBulkCreate"
      ref="bulkModalRef"
      :rules="rules"
      :existing-codes="existingCodes"
      @close="showBulkCreate = false"
      @confirm="handleBulkConfirm"
      @created="handleBulkCreated"
    />

    <!-- ── Bulk delete confirm ── -->
    <AppConfirm
      v-if="showBulkDeleteConfirm"
      title="Xóa nhiều kỳ lương"
      :message="`Bạn có chắc muốn xóa ${selectedIds.size} kỳ lương đã chọn? Các kỳ đã có phiếu lương sẽ không thể xóa.`"
      confirm-text="Xóa tất cả"
      :danger="true"
      :loading="bulkDeleteLoading"
      @confirm="confirmBulkDelete"
      @cancel="showBulkDeleteConfirm = false"
    />
  </div>
</template>

<style scoped>
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.fade-slide-enter-from,
.fade-slide-leave-to {
  opacity: 0;
  transform: translateX(-8px);
}
</style>
