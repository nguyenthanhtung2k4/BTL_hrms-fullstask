<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { employeeService } from '../../../services/employee.service'
import { departmentService } from '../../../services/department.service'
import { positionService } from '../../../services/position.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { Employee, Department, Position } from '../../../types/hr.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import AppConfirm from '../../../components/ui/AppConfirm.vue'
import EmployeeFormModal from './EmployeeFormModal.vue'
import EmployeeStatusModal from './EmployeeStatusModal.vue'
import { useRouter } from 'vue-router'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'


const auth = useAuthStore()
const toast = useToastStore()
const router = useRouter()

const employees = ref<Employee[]>([])
const departments = ref<Department[]>([])
const positions = ref<Position[]>([])
const loading = ref(false)

const search = ref('')
const filterDept = ref('')
const filterStatus = ref('')

const showForm = ref(false)
const editTarget = ref<Employee | null>(null)
const deleteTarget = ref<Employee | null>(null)
const deleteLoading = ref(false)
const statusTarget = ref<Employee | null>(null)

const columns = [
  { key: 'code', label: 'Mã NV' },
  { key: 'name', label: 'Họ tên' },
  { key: 'dept', label: 'Phòng ban' },
  { key: 'position', label: 'Chức vụ' },
  { key: 'hireDate', label: 'Ngày vào' },
  { key: 'status', label: 'Trạng thái' },
  { key: 'actions', label: '', class: 'text-right' },
]

const filtered = computed(() => {
  let list = employees.value
  if (search.value) {
    const s = search.value.toLowerCase()
    list = list.filter(
      (e) => e.fullName.toLowerCase().includes(s) || e.employeeCode.toLowerCase().includes(s) || e.email.toLowerCase().includes(s),
    )
  }
  if (filterDept.value) list = list.filter((e) => e.departmentId === filterDept.value)
  if (filterStatus.value) list = list.filter((e) => e.status === filterStatus.value)
  return list
})

const { currentPage, perPage, paginatedData, total } = usePagination(filtered)

async function load() {
  loading.value = true
  try {
    ;[employees.value, departments.value, positions.value] = await Promise.all([
      employeeService.getAll(),
      departmentService.getAll(),
      positionService.getAll(),
    ])
  } catch { toast.error('Không thể tải dữ liệu') }
  finally { loading.value = false }
}

function formatSeniority(hireDateStr: string) {
  if (!hireDateStr) return ''
  const hireDate = new Date(hireDateStr)
  if (isNaN(hireDate.getTime())) return ''
  const now = new Date()
  let years = now.getFullYear() - hireDate.getFullYear()
  let months = now.getMonth() - hireDate.getMonth()
  if (months < 0) {
    years--
    months += 12
  }
  const dateStr = hireDate.toLocaleDateString('vi-VN')
  if (years === 0 && months === 0) return `${dateStr} (Mới vào)`
  const parts: string[] = []
  if (years > 0) parts.push(`${years} năm`)
  if (months > 0) parts.push(`${months} tháng`)
  return `${dateStr} (${parts.join(' ')})`
}
function openCreate() { editTarget.value = null; showForm.value = true }
function openEdit(e: Employee) { editTarget.value = e; showForm.value = true }

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try {
    await employeeService.delete(deleteTarget.value.id)
    toast.success('Đã xóa nhân viên')
    deleteTarget.value = null; await load()
  } catch { toast.error('Không thể xóa — nhân viên có dữ liệu liên quan') }
  finally { deleteLoading.value = false }
}

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Nhân viên" subtitle="Quản lý hồ sơ nhân viên" :breadcrumbs="[{ label: 'Nhân sự' }, { label: 'Nhân viên' }]">
      <template #actions>
        <AppButton v-if="auth.isHR || auth.isAdmin" @click="openCreate">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
          Thêm nhân viên
        </AppButton>
      </template>
    </PageHeader>

    <!-- Filters -->
    <div class="mb-4 flex flex-wrap gap-3">
      <input v-model="search" type="text" placeholder="Tìm theo tên, mã NV, email..." class="h-9 w-full max-w-xs rounded-lg border border-slate-300 px-3 text-sm outline-none focus:border-emerald-500" />
      <select v-model="filterDept" class="h-9 rounded-lg border border-slate-300 px-3 text-sm outline-none focus:border-emerald-500 bg-white">
        <option value="">Tất cả phòng ban</option>
        <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
      </select>
      <select v-model="filterStatus" class="h-9 rounded-lg border border-slate-300 px-3 text-sm outline-none focus:border-emerald-500 bg-white">
        <option value="">Tất cả trạng thái</option>
        <option value="Active">Đang làm</option>
        <option value="Inactive">Ngưng</option>
        <option value="OnLeave">Nghỉ phép</option>
        <option value="Resigned">Đã nghỉ</option>
      </select>
      <AppButton variant="ghost" size="sm" @click="search = ''; filterDept = ''; filterStatus = ''">Reset</AppButton>
    </div>

    <!-- Table -->
    <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có nhân viên nào">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-mono text-slate-600">{{ (row as Employee).employeeCode }}</td>
        <td class="px-4 py-3">
          <div class="text-sm font-medium text-slate-900">{{ (row as Employee).fullName }}</div>
          <div class="text-xs text-slate-500">{{ (row as Employee).email }}</div>
        </td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ (row as Employee).departmentName }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ (row as Employee).positionName }}</td>
        <td class="px-4 py-3 text-sm text-slate-500">{{ formatSeniority((row as Employee).hireDate) }}</td>
        <td class="px-4 py-3"><AppBadge :status="(row as Employee).status" /></td>
        <td class="px-4 py-3 text-right">
          <div class="flex justify-end gap-2">
            <AppButton size="sm" variant="ghost" @click="router.push(`/hr/employees/${(row as Employee).id}`)">Xem</AppButton>
            <AppButton v-if="auth.isHR || auth.isAdmin" size="sm" variant="secondary" @click="openEdit(row as Employee)">Sửa</AppButton>
            <AppButton v-if="auth.isHR || auth.isAdmin" size="sm" variant="ghost" @click="statusTarget = row as Employee">Trạng thái</AppButton>
            <AppButton v-if="auth.isAdmin" size="sm" variant="danger" @click="deleteTarget = row as Employee">Xóa</AppButton>
          </div>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />

    <EmployeeFormModal v-if="showForm" :edit="editTarget" :departments="departments" :positions="positions" :employees="employees" @close="showForm = false" @saved="load(); showForm = false" />
    <EmployeeStatusModal v-if="statusTarget" :employee="statusTarget" @close="statusTarget = null" @saved="load(); statusTarget = null" />
    <AppConfirm v-if="deleteTarget" title="Xóa nhân viên" :message="`Xóa nhân viên &quot;${deleteTarget.fullName}&quot;?`" confirm-text="Xóa" :danger="true" :loading="deleteLoading" @confirm="confirmDelete" @cancel="deleteTarget = null" />
  </div>
</template>

