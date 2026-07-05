<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { payrollRuleService } from '../../../services/payrollRule.service'
import { useToastStore } from '../../../stores/toast'
import { useAuthStore } from '../../../stores/auth'
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
const auth = useAuthStore()
const rules = ref<PayrollRule[]>([])
const loading = ref(false)
const showForm = ref(false)
const editTarget = ref<PayrollRule | null>(null)
const deleteTarget = ref<PayrollRule | null>(null)
const deleteLoading = ref(false)
const saving = ref(false)
const form = ref({
  code: '',
  name: '',
  workDayHours: '8',
  paidLeaveCountsAsWork: true,
  overtimeRate: '1.5',
  isActive: true,
  gracePeriodMinutes: '15',
  lateDeductionRate: '0.05',
  weekendOvertimeRate: '2.0',
  holidayOvertimeRate: '3.0',
  roundingMinutes: '15'
})
const errors = ref<Record<string, string>>({})

const columns = computed(() => {
  const cols: Array<{ key: string; label: string; class?: string }> = [
    { key: 'code', label: 'Mã' },
    { key: 'name', label: 'Tên quy tắc' },
    { key: 'hours', label: 'Giờ/ngày' },
    { key: 'ot', label: 'Hệ số OT' },
    { key: 'paid', label: 'Phép CL = ngày công' },
    { key: 'status', label: 'Trạng thái' }
  ]
  if (auth.isPayrollStaff) {
    cols.push({ key: 'actions', label: '', class: 'text-right' })
  }
  return cols
})

async function load() {
  loading.value = true
  try { rules.value = await payrollRuleService.getAll() }
  catch { toast.error('Không thể tải quy tắc lương') }
  finally { loading.value = false }
}

function openCreate() {
  editTarget.value = null
  form.value = {
    code: '',
    name: '',
    workDayHours: '8',
    paidLeaveCountsAsWork: true,
    overtimeRate: '1.5',
    isActive: true,
    gracePeriodMinutes: '15',
    lateDeductionRate: '0.05',
    weekendOvertimeRate: '2.0',
    holidayOvertimeRate: '3.0',
    roundingMinutes: '15'
  }
  errors.value = {}
  showForm.value = true
}

function openEdit(r: PayrollRule) {
  editTarget.value = r
  form.value = {
    code: r.code,
    name: r.name,
    workDayHours: String(r.workDayHours),
    paidLeaveCountsAsWork: r.paidLeaveCountsAsWork,
    overtimeRate: String(r.overtimeRate),
    isActive: r.isActive,
    gracePeriodMinutes: String(r.gracePeriodMinutes ?? 15),
    lateDeductionRate: String(r.lateDeductionRate ?? 0.05),
    weekendOvertimeRate: String(r.weekendOvertimeRate ?? 2.0),
    holidayOvertimeRate: String(r.holidayOvertimeRate ?? 3.0),
    roundingMinutes: String(r.roundingMinutes ?? 15)
  }
  errors.value = {}
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
    const dto: CreatePayrollRuleDto = {
      code: form.value.code,
      name: form.value.name,
      workDayHours: Number(form.value.workDayHours),
      paidLeaveCountsAsWork: form.value.paidLeaveCountsAsWork,
      overtimeRate: Number(form.value.overtimeRate),
      isActive: form.value.isActive,
      gracePeriodMinutes: Number(form.value.gracePeriodMinutes),
      lateDeductionRate: Number(form.value.lateDeductionRate),
      weekendOvertimeRate: Number(form.value.weekendOvertimeRate),
      holidayOvertimeRate: Number(form.value.holidayOvertimeRate),
      roundingMinutes: Number(form.value.roundingMinutes)
    }
    if (editTarget.value) {
      await payrollRuleService.update(editTarget.value.id, dto)
      toast.success('Cập nhật quy tắc thành công')
    } else {
      await payrollRuleService.create(dto)
      toast.success('Tạo quy tắc thành công')
    }
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
      <template #actions v-if="auth.isPayrollStaff">
        <AppButton @click="openCreate">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
          Thêm quy tắc
        </AppButton>
      </template>
    </PageHeader>

    <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có quy tắc tính lương">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-mono">{{ (row as PayrollRule).code }}</td>
        <td class="px-4 py-3 text-sm font-medium">{{ (row as PayrollRule).name }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as PayrollRule).workDayHours }}h/ngày</td>
        <td class="px-4 py-3 text-sm">
          <div class="flex flex-wrap gap-2 text-[11px]">
            <span class="inline-flex items-center px-1.5 py-0.5 rounded-md bg-slate-100 text-slate-700 font-medium">Thường: x{{ (row as PayrollRule).overtimeRate }}</span>
            <span class="inline-flex items-center px-1.5 py-0.5 rounded-md bg-emerald-50 text-emerald-700 font-medium border border-emerald-200">Cuối tuần: x{{ (row as PayrollRule).weekendOvertimeRate ?? '2.0' }}</span>
            <span class="inline-flex items-center px-1.5 py-0.5 rounded-md bg-amber-50 text-amber-700 font-medium border border-amber-200">Ngày lễ: x{{ (row as PayrollRule).holidayOvertimeRate ?? '3.0' }}</span>
          </div>
        </td>
        <td class="px-4 py-3 text-sm">{{ (row as PayrollRule).paidLeaveCountsAsWork ? '✓ Có' : '✗ Không' }}</td>
        <td class="px-4 py-3"><AppBadge :status="(row as PayrollRule).isActive ? 'Active' : 'Inactive'" /></td>
        <td v-if="auth.isPayrollStaff" class="px-4 py-3 text-right">
          <div class="flex justify-end gap-2">
            <AppButton size="sm" variant="secondary" @click="openEdit(row as PayrollRule)">Sửa</AppButton>
            <AppButton size="sm" variant="danger" @click="deleteTarget = row as PayrollRule">Xóa</AppButton>
          </div>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />

    <AppModal v-if="showForm" :title="editTarget ? 'Sửa quy tắc' : 'Thêm quy tắc tính lương'" @close="showForm = false">
      <div class="space-y-5 max-h-[70vh] overflow-y-auto pr-1">
        <!-- 1. Thông tin chung -->
        <div class="space-y-3">
          <h4 class="text-[11px] font-bold uppercase tracking-wider text-slate-400 border-b border-slate-100 pb-1">1. Thông tin chung</h4>
          <div class="grid grid-cols-2 gap-3">
            <AppInput id="rule-code" v-model="form.code" label="Mã quy tắc" required :disabled="!!editTarget" :error="errors.code" placeholder="VD: RULE_STANDARD" />
            <AppInput id="rule-name" v-model="form.name" label="Tên quy tắc" required :error="errors.name" placeholder="VD: Quy tắc tính lương tiêu chuẩn" />
          </div>
          <div class="grid grid-cols-2 gap-3">
            <AppInput id="rule-hours" v-model="form.workDayHours" label="Số giờ/ngày công" type="number" min="1" max="24" />
            <AppInput id="rule-round" v-model="form.roundingMinutes" label="Làm tròn công (phút)" type="number" min="0" max="60" placeholder="VD: 15" />
          </div>
        </div>

        <!-- 2. Hệ số tăng ca (OT) -->
        <div class="space-y-3">
          <h4 class="text-[11px] font-bold uppercase tracking-wider text-slate-400 border-b border-slate-100 pb-1">2. Hệ số tăng ca (OT)</h4>
          <div class="grid grid-cols-3 gap-3">
            <AppInput id="rule-ot" v-model="form.overtimeRate" label="OT Ngày thường" type="number" step="0.1" placeholder="1.5" />
            <AppInput id="rule-ot-weekend" v-model="form.weekendOvertimeRate" label="OT Cuối tuần" type="number" step="0.1" placeholder="2.0" />
            <AppInput id="rule-ot-holiday" v-model="form.holidayOvertimeRate" label="OT Ngày lễ" type="number" step="0.1" placeholder="3.0" />
          </div>
        </div>

        <!-- 3. Quản lý đi muộn / Về sớm -->
        <div class="space-y-3">
          <h4 class="text-[11px] font-bold uppercase tracking-wider text-slate-400 border-b border-slate-100 pb-1">3. Quản lý đi muộn / Về sớm</h4>
          <div class="grid grid-cols-2 gap-3">
            <AppInput id="rule-grace" v-model="form.gracePeriodMinutes" label="Đi muộn cho phép (phút)" type="number" min="0" placeholder="15" />
            <AppInput id="rule-deduct" v-model="form.lateDeductionRate" label="Hệ số phạt đi muộn" type="number" step="0.01" placeholder="0.05" />
          </div>
        </div>

        <!-- 4. Cấu hình khác -->
        <div class="space-y-3">
          <h4 class="text-[11px] font-bold uppercase tracking-wider text-slate-400 border-b border-slate-100 pb-1">4. Cấu hình khác</h4>
          <div class="flex flex-col sm:flex-row gap-4 pt-1">
            <label class="flex items-center gap-2 cursor-pointer select-none">
              <input v-model="form.paidLeaveCountsAsWork" type="checkbox" class="h-4 w-4 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500 accent-emerald-600" />
              <span class="text-sm text-slate-700">Nghỉ phép tính vào ngày công</span>
            </label>
            <label class="flex items-center gap-2 cursor-pointer select-none">
              <input v-model="form.isActive" type="checkbox" class="h-4 w-4 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500 accent-emerald-600" />
              <span class="text-sm text-slate-700">Kích hoạt quy tắc</span>
            </label>
          </div>
        </div>
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="showForm = false">Hủy</AppButton>
        <AppButton :loading="saving" @click="save">{{ editTarget ? 'Cập nhật' : 'Tạo mới' }}</AppButton>
      </template>
    </AppModal>

    <AppConfirm v-if="deleteTarget" title="Xóa quy tắc" :message="`Xóa quy tắc &quot;${deleteTarget.name}&quot;?`" confirm-text="Xóa" :danger="true" :loading="deleteLoading" @confirm="confirmDelete" @cancel="deleteTarget = null" />
  </div>
</template>

