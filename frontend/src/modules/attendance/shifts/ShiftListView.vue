<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { shiftService } from '../../../services/shift.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { Shift } from '../../../types/attendance.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppModal from '../../../components/ui/AppModal.vue'
import AppInput from '../../../components/ui/AppInput.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import AppConfirm from '../../../components/ui/AppConfirm.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'

const auth = useAuthStore()
const toast = useToastStore()
const shifts = ref<Shift[]>([])
const loading = ref(false)
const showForm = ref(false)
const editTarget = ref<Shift | null>(null)
const deleteTarget = ref<Shift | null>(null)
const deleteLoading = ref(false)
const saving = ref(false)
const form = ref({ code: '', name: '', startTime: '08:00', endTime: '17:00', breakMinutes: '60', isActive: true })
const errors = ref<Record<string, string>>({})

const columns = [
  { key: 'code', label: 'Mã' }, { key: 'name', label: 'Tên ca' },
  { key: 'start', label: 'Giờ bắt đầu' }, { key: 'end', label: 'Giờ kết thúc' },
  { key: 'break', label: 'Giờ nghỉ (phút)' }, { key: 'status', label: 'Trạng thái' },
  { key: 'actions', label: '', class: 'text-right' },
]

async function load() {
  loading.value = true
  try { shifts.value = await shiftService.getAll() }
  catch { toast.error('Không thể tải ca làm việc') }
  finally { loading.value = false }
}

function openCreate() { editTarget.value = null; form.value = { code: '', name: '', startTime: '08:00', endTime: '17:00', breakMinutes: '60', isActive: true }; errors.value = {}; showForm.value = true }
function openEdit(s: Shift) { editTarget.value = s; form.value = { code: s.code, name: s.name, startTime: s.startTime, endTime: s.endTime, breakMinutes: String(s.breakMinutes), isActive: s.isActive }; errors.value = {}; showForm.value = true }

function validate() {
  errors.value = {}
  if (!form.value.code.trim()) errors.value.code = 'Mã bắt buộc'
  if (!form.value.name.trim()) errors.value.name = 'Tên ca bắt buộc'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (editTarget.value) {
      await shiftService.update(editTarget.value.id, { name: form.value.name, startTime: form.value.startTime, endTime: form.value.endTime, breakMinutes: Number(form.value.breakMinutes), isActive: form.value.isActive })
      toast.success('Cập nhật ca làm việc thành công')
    } else {
      await shiftService.create({ code: form.value.code, name: form.value.name, startTime: form.value.startTime, endTime: form.value.endTime, breakMinutes: Number(form.value.breakMinutes) })
      toast.success('Tạo ca làm việc thành công')
    }
    showForm.value = false; await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Lưu thất bại') }
  finally { saving.value = false }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try { await shiftService.delete(deleteTarget.value.id); toast.success('Đã xóa ca làm việc'); deleteTarget.value = null; await load() }
  catch { toast.error('Xóa thất bại') }
  finally { deleteLoading.value = false }
}

const { currentPage, perPage, paginatedData, total } = usePagination(shifts)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Ca làm việc" subtitle="Quản lý các ca làm việc" :breadcrumbs="[{ label: 'Chấm công' }, { label: 'Ca làm việc' }]">
      <template #actions>
        <AppButton v-if="auth.isHR" @click="openCreate">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
          Thêm ca
        </AppButton>
      </template>
    </PageHeader>

    <AppTable :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có ca làm việc nào">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-mono">{{ (row as Shift).code }}</td>
        <td class="px-4 py-3 text-sm font-medium">{{ (row as Shift).name }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as Shift).startTime }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as Shift).endTime }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as Shift).breakMinutes }} phút</td>
        <td class="px-4 py-3"><AppBadge :status="(row as Shift).isActive ? 'Active' : 'Inactive'" /></td>
        <td class="px-4 py-3 text-right">
          <div class="flex justify-end gap-2">
            <AppButton v-if="auth.isHR" size="sm" variant="secondary" @click="openEdit(row as Shift)">Sửa</AppButton>
            <AppButton v-if="auth.isAdmin" size="sm" variant="danger" @click="deleteTarget = row as Shift">Xóa</AppButton>
          </div>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />

    <AppModal v-if="showForm" :title="editTarget ? 'Sửa ca làm việc' : 'Thêm ca làm việc'" @close="showForm = false">
      <div class="space-y-4">
        <AppInput id="sh-code" v-model="form.code" label="Mã ca" required :disabled="!!editTarget" :error="errors.code" placeholder="VD: MORNING" />
        <AppInput id="sh-name" v-model="form.name" label="Tên ca" required :error="errors.name" placeholder="VD: Ca sáng" />
        <div class="grid grid-cols-2 gap-3">
          <AppInput id="sh-start" v-model="form.startTime" label="Giờ bắt đầu" type="time" required />
          <AppInput id="sh-end" v-model="form.endTime" label="Giờ kết thúc" type="time" required />
        </div>
        <AppInput id="sh-break" v-model="form.breakMinutes" label="Thời gian nghỉ (phút)" type="number" />
        <label v-if="editTarget" class="flex items-center gap-2 cursor-pointer">
          <input v-model="form.isActive" type="checkbox" class="h-4 w-4 accent-emerald-600" />
          <span class="text-sm">Kích hoạt</span>
        </label>
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="showForm = false">Hủy</AppButton>
        <AppButton :loading="saving" @click="save">{{ editTarget ? 'Cập nhật' : 'Tạo mới' }}</AppButton>
      </template>
    </AppModal>

    <AppConfirm v-if="deleteTarget" title="Xóa ca làm việc" :message="`Xóa ca &quot;${deleteTarget.name}&quot;?`" confirm-text="Xóa" :danger="true" :loading="deleteLoading" @confirm="confirmDelete" @cancel="deleteTarget = null" />
  </div>
</template>

