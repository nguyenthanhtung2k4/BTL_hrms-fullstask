<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { payrollPeriodService } from '../../../services/payrollPeriod.service'
import { payrollRuleService } from '../../../services/payrollRule.service'
import { useToastStore } from '../../../stores/toast'
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

const toast = useToastStore()
const router = useRouter()

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

const columns = [
  { key: 'name', label: 'Tên kỳ lương' }, { key: 'from', label: 'Từ ngày' }, { key: 'to', label: 'Đến ngày' },
  { key: 'rule', label: 'Quy tắc' }, { key: 'status', label: 'Trạng thái' }, { key: 'actions', label: '', class: 'text-right' },
]

async function load() {
  loading.value = true
  try { [periods.value, rules.value] = await Promise.all([payrollPeriodService.getAll(), payrollRuleService.getAll()]) }
  catch { toast.error('Không thể tải dữ liệu') }
  finally { loading.value = false }
}

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

const { currentPage, perPage, paginatedData, total } = usePagination(periods)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Kỳ lương" subtitle="Quản lý các kỳ tính lương" :breadcrumbs="[{ label: 'Lương & Báo cáo' }, { label: 'Kỳ lương' }]">
      <template #actions>
        <AppButton @click="openCreate">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
          Tạo kỳ lương
        </AppButton>
      </template>
    </PageHeader>

    <AppTable :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có kỳ lương nào">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-medium">{{ (row as PayrollPeriod).name }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ fmt((row as PayrollPeriod).fromDate) }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ fmt((row as PayrollPeriod).toDate) }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ (row as PayrollPeriod).payrollRuleName ?? '—' }}</td>
        <td class="px-4 py-3"><AppBadge :status="(row as PayrollPeriod).status" /></td>
        <td class="px-4 py-3 text-right">
          <div class="flex justify-end gap-1.5">
            <AppButton size="sm" variant="ghost" @click="router.push(`/payroll/periods/${(row as PayrollPeriod).id}`)">Chi tiết</AppButton>
            <template v-if="(row as PayrollPeriod).status !== 'Closed'">
              <AppButton size="sm" variant="secondary" @click="editPeriod(row as PayrollPeriod)">Sửa</AppButton>
              <AppButton size="sm" variant="danger" @click="deleteTarget = row as PayrollPeriod">Xóa</AppButton>
            </template>
          </div>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />

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

    <AppConfirm v-if="deleteTarget" title="Xóa kỳ lương" :message="`Xóa kỳ &quot;${deleteTarget.name}&quot;?`" confirm-text="Xóa" :danger="true" :loading="deleteLoading" @confirm="confirmDelete" @cancel="deleteTarget = null" />
  </div>
</template>

