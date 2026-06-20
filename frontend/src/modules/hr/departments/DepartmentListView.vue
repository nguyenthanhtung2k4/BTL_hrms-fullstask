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

function getDept(row: any): Department {
  return row as Department
}

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

    <!-- Search & Filter -->
    <div class="mb-6 flex flex-col sm:flex-row gap-4 items-start sm:items-center">
      <div class="flex-1 max-w-md">
        <input
          v-model="search"
          type="text"
          placeholder="Tìm theo tên hoặc mã phòng ban..."
          class="w-full px-4 py-2.5 rounded-lg border border-slate-200 bg-white text-sm text-slate-900 placeholder:text-slate-400 focus:border-blue-500 focus:ring-4 focus:ring-blue-100 outline-none transition-all duration-200"
        />
      </div>
    </div>

    <!-- Table -->
    <div class="rounded-lg border border-slate-200 bg-white shadow-sm overflow-hidden">
    <AppTable :columns="columns" :rows="filtered" :loading="loading" row-key="id" empty-text="Chưa có phòng ban nào">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-mono text-slate-600">{{ getDept(row).code }}</td>
        <td class="px-4 py-3 text-sm font-medium text-slate-900">{{ getDept(row).name }}</td>
        <td class="px-4 py-3">
          <AppBadge :status="getDept(row).isActive ? 'Active' : 'Inactive'" />
        </td>
        <td class="px-4 py-3 text-sm text-slate-500">{{ formatDate(getDept(row).createdAt) }}</td>
        <td class="px-4 py-3 text-right">
          <div class="flex justify-end gap-2">
            <AppButton v-if="auth.isHR" size="sm" variant="secondary" @click="openEdit(getDept(row))">Sửa</AppButton>
            <AppButton v-if="auth.isAdmin" size="sm" variant="danger" @click="deleteTarget = getDept(row)">Xóa</AppButton>
          </div>
        </td>
      </template>
    </AppTable>
    </div>

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
