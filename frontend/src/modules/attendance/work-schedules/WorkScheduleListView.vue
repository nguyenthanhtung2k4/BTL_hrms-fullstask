<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
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
const search = ref('')
const form = ref({ employeeId: '', shiftId: '', startDate: '', endDate: '' })
const errors = ref<Record<string, string>>({})

const columns = [
  { key: 'employee', label: 'Nhân viên' },
  { key: 'shift', label: 'Ca làm việc' },
  { key: 'workDate', label: 'Ngày làm việc' },
  { key: 'status', label: 'Trạng thái' },
  ...(auth.isManager ? [{ key: 'actions', label: '', class: 'text-right' }] : []),
]

const filtered = computed(() => {
  let result = schedules.value
  if (search.value) {
    const q = search.value.toLowerCase()
    result = result.filter(
      (s) =>
        s.employeeName?.toLowerCase().includes(q) ||
        s.shiftName?.toLowerCase().includes(q) ||
        s.workDate?.includes(q)
    )
  }
  // Sắp xếp theo ngày làm việc mới nhất (mới nhất lên trên đầu)
  return [...result].sort((a, b) => new Date(b.workDate).getTime() - new Date(a.workDate).getTime())
})

async function load() {
  loading.value = true
  try {
    const params = auth.isManager ? undefined : { employeeId: auth.employeeId }
    const resSchedules = await workScheduleService.getAll(params || {})
    schedules.value = resSchedules
    
    if (auth.isManager) {
      const [resEmployees, resShifts] = await Promise.all([
        employeeService.getAll(),
        shiftService.getAll()
      ])
      employees.value = resEmployees
      shifts.value = resShifts
    }
  } catch {
    toast.error('Không thể tải dữ liệu lịch làm việc')
  } finally {
    loading.value = false
  }
}

function validate() {
  errors.value = {}
  if (!form.value.employeeId) errors.value.employeeId = 'Nhân viên bắt buộc'
  if (!form.value.shiftId) errors.value.shiftId = 'Ca làm bắt buộc'
  if (!form.value.startDate) errors.value.startDate = 'Từ ngày bắt buộc'
  if (!form.value.endDate) errors.value.endDate = 'Đến ngày bắt buộc'
  if (form.value.startDate && form.value.endDate && form.value.startDate > form.value.endDate) {
    errors.value.endDate = 'Đến ngày phải sau hoặc bằng từ ngày'
  }
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    const start = new Date(form.value.startDate)
    const end = new Date(form.value.endDate)
    const promises = []
    
    // Lặp qua từng ngày trong khoảng thời gian được phân để lưu
    for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
      const y = d.getFullYear()
      const m = String(d.getMonth() + 1).padStart(2, '0')
      const day = String(d.getDate()).padStart(2, '0')
      const workDateStr = `${y}-${m}-${day}`
      promises.push(
        workScheduleService.create({
          employeeId: form.value.employeeId,
          shiftId: form.value.shiftId,
          workDate: workDateStr
        })
      )
    }
    
    await Promise.all(promises)
    toast.success('Phân lịch làm việc thành công')
    showForm.value = false
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message ?? 'Lưu lịch làm việc thất bại')
  } finally {
    saving.value = false
  }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try {
    await workScheduleService.delete(deleteTarget.value.id)
    toast.success('Đã xóa lịch làm việc')
    deleteTarget.value = null
    await load()
  } catch {
    toast.error('Xóa lịch làm việc thất bại')
  } finally {
    deleteLoading.value = false
  }
}

function fmt(d: string) {
  if (!d) return ''
  return new Date(d).toLocaleDateString('vi-VN')
}

const { currentPage, perPage, paginatedData, total } = usePagination(filtered)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Lịch làm việc" subtitle="Phân ca làm việc và theo dõi lịch trình của nhân sự" :breadcrumbs="[{ label: 'Chấm công' }, { label: 'Lịch làm việc' }]">
      <template #actions>
        <AppButton v-if="auth.isManager" @click="showForm = true">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
          Phân lịch
        </AppButton>
      </template>
    </PageHeader>

    <!-- Thanh tìm kiếm & bộ lọc -->
    <div class="mb-4">
      <input
        v-model="search"
        type="text"
        placeholder="Tìm kiếm theo nhân viên, ca làm hoặc ngày..."
        class="h-9 w-full max-w-sm rounded-lg border border-slate-300 px-3 text-sm outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-400"
      />
    </div>

    <!-- Bảng hiển thị -->
    <AppTable :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có lịch làm việc nào">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-medium text-slate-900">{{ (row as WorkSchedule).employeeName }}</td>
        <td class="px-4 py-3 text-sm text-slate-700">{{ (row as WorkSchedule).shiftName }}</td>
        <td class="px-4 py-3 text-sm text-slate-500">{{ fmt((row as WorkSchedule).workDate) }}</td>
        <td class="px-4 py-3 text-sm">
          <span
            class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-100"
          >
            {{ (row as WorkSchedule).status || 'Planned' }}
          </span>
        </td>
        <td v-if="auth.isManager" class="px-4 py-3 text-right">
          <AppButton size="sm" variant="danger" @click="deleteTarget = row as WorkSchedule">Xóa</AppButton>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />

    <!-- Modal Phân Lịch -->
    <AppModal v-if="showForm" title="Phân lịch làm việc" @close="showForm = false">
      <div class="space-y-4">
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Nhân viên <span class="text-red-500">*</span></label>
          <select v-model="form.employeeId" :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none focus:ring-1 focus:ring-emerald-400', errors.employeeId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn nhân viên --</option>
            <option v-for="e in employees" :key="e.id" :value="e.id">{{ e.fullName }}</option>
          </select>
          <p v-if="errors.employeeId" class="text-xs text-red-500">{{ errors.employeeId }}</p>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Ca làm việc <span class="text-red-500">*</span></label>
          <select v-model="form.shiftId" :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none focus:ring-1 focus:ring-emerald-400', errors.shiftId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
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

    <!-- Xác nhận xóa -->
    <AppConfirm v-if="deleteTarget" title="Xóa lịch làm việc" message="Bạn chắc chắn muốn xóa lịch này?" confirm-text="Xóa" :danger="true" :loading="deleteLoading" @confirm="confirmDelete" @cancel="deleteTarget = null" />
  </div>
</template>

