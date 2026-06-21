<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { payrollRuleService } from '../../../services/payrollRule.service'
import { useToastStore } from '../../../stores/toast'
import type { PayrollRule, CreatePayrollRuleDto } from '../../../types/payroll.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import AppModal from '../../../components/ui/AppModal.vue'
import AppInput from '../../../components/ui/AppInput.vue'
import AppConfirm from '../../../components/ui/AppConfirm.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'

const toast = useToastStore()
const rules = ref<PayrollRule[]>([])
const loading = ref(false)
const showForm = ref(false)
const editTarget = ref<PayrollRule | null>(null)
const deleteTarget = ref<PayrollRule | null>(null)
const deleteLoading = ref(false)
const saving = ref(false)
const form = ref({ code: '', name: '', workDayHours: '8', paidLeaveCountsAsWork: true, overtimeRate: '1.5', isActive: true })
const errors = ref<Record<string, string>>({})

const columns = [
  { key: 'code', label: 'Mã' }, { key: 'name', label: 'Tên quy tắc' }, { key: 'hours', label: 'Giờ/ngày' },
  { key: 'ot', label: 'Hệ số OT' }, { key: 'paid', label: 'Phép CL = ngày công' }, { key: 'status', label: 'Trạng thái' }, { key: 'actions', label: '', class: 'text-right' },
]

async function load() {
  loading.value = true
  try { rules.value = await payrollRuleService.getAll() }
  catch { toast.error('Không thể tải quy tắc lương') }
  finally { loading.value = false }
}

function openCreate() { editTarget.value = null; form.value = { code: '', name: '', workDayHours: '8', paidLeaveCountsAsWork: true, overtimeRate: '1.5', isActive: true }; errors.value = {}; showForm.value = true }
function openEdit(r: PayrollRule) { editTarget.value = r; form.value = { code: r.code, name: r.name, workDayHours: String(r.workDayHours), paidLeaveCountsAsWork: r.paidLeaveCountsAsWork, overtimeRate: String(r.overtimeRate), isActive: r.isActive }; errors.value = {}; showForm.value = true }

function validate() {
  errors.value = {}
  if (!form.value.code.trim()) errors.value.code = 'Mã bắt buộc'
  if (!form.value.name.trim()) errors.value.name = 'Tên bắt buộc'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    const dto: CreatePayrollRuleDto = { code: form.value.code, name: form.value.name, workDayHours: Number(form.value.workDayHours), paidLeaveCountsAsWork: form.value.paidLeaveCountsAsWork, overtimeRate: Number(form.value.overtimeRate), isActive: form.value.isActive }
    if (editTarget.value) { await payrollRuleService.update(editTarget.value.id, dto); toast.success('Cập nhật quy tắc thành công') }
    else { await payrollRuleService.create(dto); toast.success('Tạo quy tắc thành công') }
    showForm.value = false; await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Lưu thất bại') }
  finally { saving.value = false }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try { await payrollRuleService.delete(deleteTarget.value.id); toast.success('Đã xóa quy tắc'); deleteTarget.value = null; await load() }
  catch { toast.error('Xóa thất bại — quy tắc đang được sử dụng') }
  finally { deleteLoading.value = false }
}

const { currentPage, perPage, paginatedData, total } = usePagination(rules)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Quy tắc tính lương" :breadcrumbs="[{ label: 'Lương & Báo cáo' }, { label: 'Quy tắc lương' }]">
      <template #actions>
        <AppButton @click="openCreate">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
          Thêm quy tắc
        </AppButton>
      </template>
    </PageHeader>

    <AppTable :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có quy tắc tính lương">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-mono">{{ (row as PayrollRule).code }}</td>
        <td class="px-4 py-3 text-sm font-medium">{{ (row as PayrollRule).name }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as PayrollRule).workDayHours }}h/ngày</td>
        <td class="px-4 py-3 text-sm">x{{ (row as PayrollRule).overtimeRate }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as PayrollRule).paidLeaveCountsAsWork ? '✓ Có' : '✗ Không' }}</td>
        <td class="px-4 py-3"><AppBadge :status="(row as PayrollRule).isActive ? 'Active' : 'Inactive'" /></td>
        <td class="px-4 py-3 text-right">
          <div class="flex justify-end gap-2">
            <AppButton size="sm" variant="secondary" @click="openEdit(row as PayrollRule)">Sửa</AppButton>
            <AppButton size="sm" variant="danger" @click="deleteTarget = row as PayrollRule">Xóa</AppButton>
          </div>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />

    <AppModal v-if="showForm" :title="editTarget ? 'Sửa quy tắc' : 'Thêm quy tắc tính lương'" @close="showForm = false">
      <div class="space-y-4">
        <AppInput id="rule-code" v-model="form.code" label="Mã quy tắc" required :disabled="!!editTarget" :error="errors.code" />
        <AppInput id="rule-name" v-model="form.name" label="Tên quy tắc" required :error="errors.name" />
        <div class="grid grid-cols-2 gap-3">
          <AppInput id="rule-hours" v-model="form.workDayHours" label="Số giờ/ngày công" type="number" />
          <AppInput id="rule-ot" v-model="form.overtimeRate" label="Hệ số tăng ca (OT)" type="number" hint="VD: 1.5 = x1.5 lương" />
        </div>
        <label class="flex items-center gap-2 cursor-pointer">
          <input v-model="form.paidLeaveCountsAsWork" type="checkbox" class="h-4 w-4 accent-emerald-600" />
          <span class="text-sm">Nghỉ phép có lương tính vào ngày công</span>
        </label>
        <label class="flex items-center gap-2 cursor-pointer">
          <input v-model="form.isActive" type="checkbox" class="h-4 w-4 accent-emerald-600" />
          <span class="text-sm">Kích hoạt</span>
        </label>
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="showForm = false">Hủy</AppButton>
        <AppButton :loading="saving" @click="save">{{ editTarget ? 'Cập nhật' : 'Tạo mới' }}</AppButton>
      </template>
    </AppModal>

    <AppConfirm v-if="deleteTarget" title="Xóa quy tắc" :message="`Xóa quy tắc &quot;${deleteTarget.name}&quot;?`" confirm-text="Xóa" :danger="true" :loading="deleteLoading" @confirm="confirmDelete" @cancel="deleteTarget = null" />
  </div>
</template>

