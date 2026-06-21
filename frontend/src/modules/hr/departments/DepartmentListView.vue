<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { departmentService } from '../../../services/department.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { Department } from '../../../types/hr.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import AppConfirm from '../../../components/ui/AppConfirm.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'
import DepartmentFormModal from './DepartmentFormModal.vue'

const auth = useAuthStore()
const toast = useToastStore()

const departments = ref<Department[]>([])
const loading = ref(false)
const search = ref('')
const showForm = ref(false)
const editTarget = ref<Department | null>(null)
const deleteTarget = ref<Department | null>(null)
const deleteLoading = ref(false)

const columns = [
  { key: 'code', label: 'Mã' },
  { key: 'name', label: 'Tên phòng ban' },
  { key: 'isActive', label: 'Trạng thái' },
  { key: 'createdAt', label: 'Ngày tạo' },
  { key: 'actions', label: 'Hành động', class: 'text-right' },
]

const filtered = computed(() =>
  departments.value.filter(
    (d) =>
      d.name.toLowerCase().includes(search.value.toLowerCase()) ||
      d.code.toLowerCase().includes(search.value.toLowerCase()),
  ),
)

async function load() {
  loading.value = true
  try {
    departments.value = await departmentService.getAll()
  } catch {
    toast.error('Không thể tải danh sách phòng ban')
  } finally {
    loading.value = false
  }
}

function openCreate() {
  editTarget.value = null
  showForm.value = true
}

function openEdit(dept: Department) {
  editTarget.value = dept
  showForm.value = true
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try {
    await departmentService.delete(deleteTarget.value.id)
    toast.success(`Đã xóa phòng ban "${deleteTarget.value.name}"`)
    deleteTarget.value = null
    await load()
  } catch {
    toast.error('Xóa thất bại — phòng ban có thể đang được sử dụng')
  } finally {
    deleteLoading.value = false
  }
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('vi-VN')
}

const { currentPage, perPage, paginatedData, total } = usePagination(filtered)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader
      title="Phòng ban"
      subtitle="Quản lý các phòng ban trong tổ chức"
      :breadcrumbs="[{ label: 'Nhân sự' }, { label: 'Phòng ban' }]"
    >
      <template #actions>
        <AppButton v-if="auth.isHR" @click="openCreate">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Thêm phòng ban
        </AppButton>
      </template>
    </PageHeader>

    <!-- Search -->
    <div class="mb-4">
      <input
        v-model="search"
        type="text"
        placeholder="Tìm theo tên hoặc mã phòng ban..."
        class="h-9 w-full max-w-sm rounded-lg border border-slate-300 px-3 text-sm outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-400"
      />
    </div>

    <!-- Table -->
    <AppTable :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có phòng ban nào">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-mono text-slate-600">{{ (row as Department).code }}</td>
        <td class="px-4 py-3 text-sm font-medium text-slate-900">{{ (row as Department).name }}</td>
        <td class="px-4 py-3">
          <AppBadge :status="(row as Department).isActive ? 'Active' : 'Inactive'" />
        </td>
        <td class="px-4 py-3 text-sm text-slate-500">{{ formatDate((row as Department).createdAt) }}</td>
        <td class="px-4 py-3 text-right">
          <div class="flex justify-end gap-2">
            <AppButton v-if="auth.isHR" size="sm" variant="secondary" @click="openEdit(row as Department)">Sửa</AppButton>
            <AppButton v-if="auth.isAdmin" size="sm" variant="danger" @click="deleteTarget = row as Department">Xóa</AppButton>
          </div>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />

    <!-- Form modal -->
    <DepartmentFormModal
      v-if="showForm"
      :edit="editTarget"
      @close="showForm = false"
      @saved="load(); showForm = false"
    />

    <!-- Delete confirm -->
    <AppConfirm
      v-if="deleteTarget"
      title="Xóa phòng ban"
      :message="`Bạn có chắc muốn xóa phòng ban &quot;${deleteTarget.name}&quot;?`"
      confirm-text="Xóa"
      :danger="true"
      :loading="deleteLoading"
      @confirm="confirmDelete"
      @cancel="deleteTarget = null"
    />
  </div>
</template>

