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
const rules = ref<any[]>([]) // Dùng any tạm thời để tránh lỗi Type nếu chưa update interface
const loading = ref(false)
const showForm = ref(false)
const editTarget = ref<any | null>(null)
const deleteTarget = ref<any | null>(null)
const deleteLoading = ref(false)
const saving = ref(false)

// ĐÃ FIX: Thêm đầy đủ 5 trường mới theo yêu cầu QA
const form = ref({ 
  code: '', 
  name: '', 
  workDayHours: '8', 
  paidLeaveCountsAsWork: true, 
  isActive: true,
  standardWorkingDays: '22',
  latePenaltyRule: '50000',
  otMultiplierWeekday: '1.5',
  otMultiplierWeekend: '2.0',
  otMultiplierHoliday: '3.0'
})
const errors = ref<Record<string, string>>({})

// ĐÃ FIX: Cập nhật cột hiển thị
const columns = [
  { key: 'code', label: 'Mã' }, 
  { key: 'name', label: 'Tên quy tắc' }, 
  { key: 'stdDays', label: 'Công chuẩn' },
  { key: 'ot', label: 'Hệ số OT (Thường/Nghỉ/Lễ)' }, 
  { key: 'penalty', label: 'Phạt đi muộn' }, 
  { key: 'status', label: 'Trạng thái' }, 
  { key: 'actions', label: '', class: 'text-right' },
]

async function load() {
  loading.value = true
  try { rules.value = await payrollRuleService.getAll() }
  catch { toast.error('Không thể tải quy tắc lương') }
  finally { loading.value = false }
}

function openCreate() { 
  editTarget.value = null; 
  form.value = { 
    code: '', name: '', workDayHours: '8', paidLeaveCountsAsWork: true, isActive: true,
    standardWorkingDays: '22', latePenaltyRule: '50000', 
    otMultiplierWeekday: '1.5', otMultiplierWeekend: '2.0', otMultiplierHoliday: '3.0'
  }; 
  errors.value = {}; 
  showForm.value = true 
}

function openEdit(r: any) { 
  editTarget.value = r; 
  form.value = { 
    code: r.code, name: r.name, 
    workDayHours: String(r.workDayHours), 
    paidLeaveCountsAsWork: r.paidLeaveCountsAsWork, 
    isActive: r.isActive,
    standardWorkingDays: String(r.standardWorkingDays || 22),
    latePenaltyRule: String(r.latePenaltyRule || 50000),
    otMultiplierWeekday: String(r.otMultiplierWeekday || 1.5),
    otMultiplierWeekend: String(r.otMultiplierWeekend || 2.0),
    otMultiplierHoliday: String(r.otMultiplierHoliday || 3.0)
  }; 
  errors.value = {}; 
  showForm.value = true 
}

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
    // ĐÃ FIX: Map dữ liệu DTO chuẩn xác
    const dto: any = { 
      code: form.value.code, 
      name: form.value.name, 
      workDayHours: Number(form.value.workDayHours), 
      paidLeaveCountsAsWork: form.value.paidLeaveCountsAsWork, 
      isActive: form.value.isActive,
      standardWorkingDays: Number(form.value.standardWorkingDays),
      latePenaltyRule: Number(form.value.latePenaltyRule),
      otMultiplierWeekday: Number(form.value.otMultiplierWeekday),
      otMultiplierWeekend: Number(form.value.otMultiplierWeekend),
      otMultiplierHoliday: Number(form.value.otMultiplierHoliday)
    }
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

    <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có quy tắc tính lương">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-mono">{{ row.code }}</td>
        <td class="px-4 py-3 text-sm font-medium">{{ row.name }}</td>
        <td class="px-4 py-3 text-sm">{{ row.standardWorkingDays || 22 }} ngày</td>
        <td class="px-4 py-3 text-sm">
          Thường: x{{ row.otMultiplierWeekday }} <br/>
          Nghỉ: x{{ row.otMultiplierWeekend }} <br/>
          Lễ: x{{ row.otMultiplierHoliday }}
        </td>
        <td class="px-4 py-3 text-sm text-red-600 font-medium">-{{ (row.latePenaltyRule || 0).toLocaleString() }} ₫</td>
        <td class="px-4 py-3"><AppBadge :status="row.isActive ? 'Active' : 'Inactive'" /></td>
        <td class="px-4 py-3 text-right">
          <div class="flex justify-end gap-2">
            <AppButton size="sm" variant="secondary" @click="openEdit(row)">Sửa</AppButton>
            <AppButton size="sm" variant="danger" @click="deleteTarget = row">Xóa</AppButton>
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
          <AppInput id="rule-std-days" v-model="form.standardWorkingDays" label="Ngày công chuẩn/tháng" type="number" />
          <AppInput id="rule-penalty" v-model="form.latePenaltyRule" label="Mức phạt đi muộn (VNĐ)" type="number" />
        </div>

        <div class="grid grid-cols-3 gap-3 border-t pt-3 mt-3">
          <AppInput id="rule-ot-1" v-model="form.otMultiplierWeekday" label="Hệ số OT Ngày thường" type="number" step="0.1" />
          <AppInput id="rule-ot-2" v-model="form.otMultiplierWeekend" label="Hệ số OT Ngày nghỉ" type="number" step="0.1" />
          <AppInput id="rule-ot-3" v-model="form.otMultiplierHoliday" label="Hệ số OT Ngày lễ" type="number" step="0.1" />
        </div>

        <label class="flex items-center gap-2 cursor-pointer mt-4">
          <input v-model="form.paidLeaveCountsAsWork" type="checkbox" class="h-4 w-4 accent-emerald-600" />
          <span class="text-sm">Nghỉ phép có lương tính vào ngày công</span>
        </label>
        <label class="flex items-center gap-2 cursor-pointer">
          <input v-model="form.isActive" type="checkbox" class="h-4 w-4 accent-emerald-600" />
          <span class="text-sm">Kích hoạt quy tắc này</span>
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