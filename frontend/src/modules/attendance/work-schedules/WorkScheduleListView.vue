<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { workScheduleService } from '../../../services/workSchedule.service'
import { employeeService } from '../../../services/employee.service'
import { shiftService } from '../../../services/shift.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { WorkSchedule, Shift } from '../../../types/attendance.types'
import type { Employee } from '../../../types/hr.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'
import AppModal from '../../../components/ui/AppModal.vue'
import AppInput from '../../../components/ui/AppInput.vue'
import AppConfirm from '../../../components/ui/AppConfirm.vue'

const auth = useAuthStore()
const toast = useToastStore()
const schedules = ref<WorkSchedule[]>([])
const employees = ref<Employee[]>([])
const shifts = ref<Shift[]>([])
const loading = ref(false)
const showForm = ref(false)
const deleteTarget = ref<WorkSchedule | null>(null)
const deleteLoading = ref(false)
const saving = ref(false)
const form = ref({ employeeId: '', shiftId: '', startDate: '', endDate: '' })
const errors = ref<Record<string, string>>({})

const columns = [
  { key: 'employee', label: 'Nhân viên' }, { key: 'shift', label: 'Ca làm việc' },
  { key: 'start', label: 'Từ ngày' }, { key: 'end', label: 'Đến ngày' }, { key: 'actions', label: '', class: 'text-right' },
]

async function load() {
  loading.value = true
  try { [schedules.value, employees.value, shifts.value] = await Promise.all([workScheduleService.getAll(), employeeService.getAll(), shiftService.getAll()]) }
  catch { toast.error('Không thể tải dữ liệu') }
  finally { loading.value = false }
}

function validate() {
  errors.value = {}
  if (!form.value.employeeId) errors.value.employeeId = 'Nhân viên bắt buộc'
  if (!form.value.shiftId) errors.value.shiftId = 'Ca làm bắt buộc'
  if (!form.value.startDate) errors.value.startDate = 'Từ ngày bắt buộc'
  if (!form.value.endDate) errors.value.endDate = 'Đến ngày bắt buộc'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    await workScheduleService.create({ employeeId: form.value.employeeId, shiftId: form.value.shiftId, startDate: form.value.startDate, endDate: form.value.endDate })
    toast.success('Phân lịch thành công'); showForm.value = false; await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Lưu thất bại') }
  finally { saving.value = false }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try { await workScheduleService.delete(deleteTarget.value.id); toast.success('Đã xóa lịch làm việc'); deleteTarget.value = null; await load() }
  catch { toast.error('Xóa thất bại') }
  finally { deleteLoading.value = false }
}

function fmt(d: string) { return new Date(d).toLocaleDateString('vi-VN') }
const { currentPage, perPage, paginatedData, total } = usePagination(schedules)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Lịch làm việc" subtitle="Phân ca làm việc cho nhân viên" :breadcrumbs="[{ label: 'Chấm công' }, { label: 'Lịch làm việc' }]">
      <template #actions>
        <AppButton v-if="auth.isManager" @click="showForm = true">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
          Phân lịch
        </AppButton>
      </template>
    </PageHeader>

    <AppTable :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có lịch làm việc">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-medium">{{ (row as WorkSchedule).employeeName }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as WorkSchedule).shiftName }}</td>
        <td class="px-4 py-3 text-sm text-slate-500">{{ fmt((row as WorkSchedule).startDate) }}</td>
        <td class="px-4 py-3 text-sm text-slate-500">{{ fmt((row as WorkSchedule).endDate) }}</td>
        <td class="px-4 py-3 text-right">
          <AppButton v-if="auth.isManager" size="sm" variant="danger" @click="deleteTarget = row as WorkSchedule">Xóa</AppButton>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />

    <AppModal v-if="showForm" title="Phân lịch làm việc" @close="showForm = false">
      <div class="space-y-4">
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Nhân viên <span class="text-red-500">*</span></label>
          <select v-model="form.employeeId" :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none', errors.employeeId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn nhân viên --</option>
            <option v-for="e in employees" :key="e.id" :value="e.id">{{ e.fullName }}</option>
          </select>
          <p v-if="errors.employeeId" class="text-xs text-red-500">{{ errors.employeeId }}</p>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Ca làm việc <span class="text-red-500">*</span></label>
          <select v-model="form.shiftId" :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none', errors.shiftId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn ca --</option>
            <option v-for="s in shifts.filter(s => s.isActive)" :key="s.id" :value="s.id">{{ s.name }} ({{ s.startTime }}-{{ s.endTime }})</option>
          </select>
          <p v-if="errors.shiftId" class="text-xs text-red-500">{{ errors.shiftId }}</p>
        </div>
        <div class="grid grid-cols-2 gap-3">
          <AppInput id="ws-start" v-model="form.startDate" label="Từ ngày" type="date" required :error="errors.startDate" />
          <AppInput id="ws-end" v-model="form.endDate" label="Đến ngày" type="date" required :error="errors.endDate" />
        </div>
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="showForm = false">Hủy</AppButton>
        <AppButton :loading="saving" @click="save">Phân lịch</AppButton>
      </template>
    </AppModal>

    <AppConfirm v-if="deleteTarget" title="Xóa lịch làm việc" message="Bạn chắc chắn muốn xóa lịch này?" confirm-text="Xóa" :danger="true" :loading="deleteLoading" @confirm="confirmDelete" @cancel="deleteTarget = null" />
  </div>
</template>

