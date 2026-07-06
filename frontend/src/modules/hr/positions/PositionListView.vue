<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { positionService } from '../../../services/position.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { Position } from '../../../types/hr.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import AppConfirm from '../../../components/ui/AppConfirm.vue'
import AppModal from '../../../components/ui/AppModal.vue'
import AppInput from '../../../components/ui/AppInput.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'

const auth = useAuthStore()
const toast = useToastStore()
const positions = ref<Position[]>([])
const loading = ref(false)
const search = ref('')
const showForm = ref(false)
const editTarget = ref<Position | null>(null)
const deleteTarget = ref<Position | null>(null)
const deleteLoading = ref(false)
const saving = ref(false)
const form = ref({ code: '', name: '', description: '', isActive: true })
const errors = ref<Record<string, string>>({})

const columns = [
  { key: 'code', label: 'Mã' },
  { key: 'name', label: 'Tên chức vụ' },
  { key: 'isActive', label: 'Trạng thái' },
  { key: 'createdAt', label: 'Ngày tạo' },
  { key: 'actions', label: '', class: 'text-right' },
]

const filtered = computed(() =>
  positions.value.filter(
    (p) =>
      p.name.toLowerCase().includes(search.value.toLowerCase()) ||
      p.code.toLowerCase().includes(search.value.toLowerCase()),
  ),
)

function asPosition(row: unknown) {
  return row as Position
}

async function load() {
  loading.value = true
  try { positions.value = await positionService.getAll() }
  catch { toast.error('Không thể tải chức vụ') }
  finally { loading.value = false }
}

function openCreate() { editTarget.value = null; form.value = { code: '', name: '', description: '', isActive: true }; errors.value = {}; showForm.value = true }
function openEdit(p: Position) { editTarget.value = p; form.value = { code: p.code, name: p.name, description: p.description ?? '', isActive: p.isActive }; errors.value = {}; showForm.value = true }

function validate() {
  errors.value = {}
  if (!form.value.code.trim()) errors.value.code = 'Mã không được để trống'
  if (!form.value.name.trim()) errors.value.name = 'Tên không được để trống'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (editTarget.value) {
      await positionService.update(editTarget.value.id, { name: form.value.name, description: form.value.description, isActive: form.value.isActive })
      toast.success('Cập nhật chức vụ thành công')
    } else {
      await positionService.create({ code: form.value.code, name: form.value.name, description: form.value.description })
      toast.success('Tạo chức vụ thành công')
    }
    showForm.value = false; await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Lưu thất bại') }
  finally { saving.value = false }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try {
    await positionService.delete(deleteTarget.value.id)
    toast.success('Đã xóa chức vụ')
    deleteTarget.value = null; await load()
  } catch { toast.error('Xóa thất bại') }
  finally { deleteLoading.value = false }
}

const { currentPage, perPage, paginatedData, total } = usePagination(filtered)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Chức vụ" subtitle="Quản lý các chức vụ" :breadcrumbs="[{ label: 'Nhân sự' }, { label: 'Chức vụ' }]">
      <template #actions>
        <AppButton v-if="auth.isHR" @click="openCreate">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
          Thêm chức vụ
        </AppButton>
      </template>
    </PageHeader>

    <div class="mb-6 flex flex-col sm:flex-row gap-4 items-start sm:items-center">
      <div class="flex-1 max-w-md">
        <input v-model="search" type="text" placeholder="Tìm theo tên hoặc mã chức vụ..." class="w-full px-4 py-2.5 rounded-lg border border-slate-200 bg-white text-sm text-slate-900 placeholder:text-slate-400 focus:border-blue-500 focus:ring-4 focus:ring-blue-100 outline-none transition-all duration-200" />
      </div>
    </div>

    <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có chức vụ nào">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-mono text-slate-600">{{ asPosition(row).code }}</td>
        <td class="px-4 py-3 text-sm font-medium">{{ asPosition(row).name }}</td>
        <td class="px-4 py-3"><AppBadge :status="asPosition(row).isActive ? 'Active' : 'Inactive'" /></td>
        <td class="px-4 py-3 text-sm text-slate-500">{{ new Date(asPosition(row).createdAt).toLocaleDateString('vi-VN') }}</td>
        <td class="px-4 py-3 text-right">
          <div class="flex justify-end gap-2">
            <AppButton v-if="auth.isHR" size="sm" variant="secondary" @click="openEdit(asPosition(row))">Sửa</AppButton>
            <AppButton v-if="auth.isAdmin" size="sm" variant="danger" @click="deleteTarget = asPosition(row)">Xóa</AppButton>
          </div>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />

    <!-- Form Modal -->
    <AppModal v-if="showForm" :title="editTarget ? 'Sửa chức vụ' : 'Thêm chức vụ'" @close="showForm = false">
      <div class="space-y-5">
        <AppInput id="pos-code" v-model="form.code" label="Mã" required :disabled="!!editTarget" :error="errors.code" />
        <AppInput id="pos-name" v-model="form.name" label="Tên chức vụ" required :error="errors.name" />
        <div class="flex flex-col space-y-1.5">
          <label class="text-sm font-medium text-slate-700">Mô tả</label>
          <textarea v-model="form.description" rows="3" class="min-h-24 w-full rounded-lg border border-slate-200 bg-white px-3.5 py-2.5 text-sm text-slate-900 outline-none transition-all duration-200 placeholder:text-slate-400 focus:border-blue-500 focus:ring-4 focus:ring-blue-100" />
        </div>
        <label v-if="editTarget" class="flex items-center gap-2 cursor-pointer pt-1">
          <input v-model="form.isActive" type="checkbox" class="h-4 w-4 accent-blue-600" />
          <span class="text-sm">Kích hoạt</span>
        </label>
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="showForm = false">Hủy</AppButton>
        <AppButton :loading="saving" @click="save">{{ editTarget ? 'Cập nhật' : 'Tạo mới' }}</AppButton>
      </template>
    </AppModal>

    <AppConfirm v-if="deleteTarget" title="Xóa chức vụ" :message="`Xóa chức vụ &quot;${deleteTarget.name}&quot;?`" confirm-text="Xóa" :danger="true" :loading="deleteLoading" @confirm="confirmDelete" @cancel="deleteTarget = null" />
  </div>
</template>

