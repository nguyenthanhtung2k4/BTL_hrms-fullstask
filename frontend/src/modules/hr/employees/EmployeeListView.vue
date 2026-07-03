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
import * as XLSX from 'xlsx'
import type { CreateEmployeeDto } from '../../../types/hr.types'


const auth = useAuthStore()
const toast = useToastStore()
const router = useRouter()

const selectedIds = ref<string[]>([])
const fileInput = ref<HTMLInputElement | null>(null)
const importLoading = ref(false)

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
  { key: 'select', label: '' },
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
  
  // Prioritize active employees ('Active' status) on top
  return [...list].sort((a, b) => {
    if (a.status === 'Active' && b.status !== 'Active') return -1
    if (a.status !== 'Active' && b.status === 'Active') return 1
    return 0
  })
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

function toggleSelect(id: string) {
  const idx = selectedIds.value.indexOf(id)
  if (idx > -1) selectedIds.value.splice(idx, 1)
  else selectedIds.value.push(id)
}

const isAllSelected = computed(() => {
  return paginatedData.value.length > 0 && paginatedData.value.every(e => selectedIds.value.includes(e.id))
})

const isIndeterminate = computed(() => {
  const checkedCount = paginatedData.value.filter(e => selectedIds.value.includes(e.id)).length
  return checkedCount > 0 && checkedCount < paginatedData.value.length
})

function selectAll() {
  if (isAllSelected.value) {
    selectedIds.value = selectedIds.value.filter(id => !paginatedData.value.some(e => e.id === id))
  } else {
    const idsToAdd = paginatedData.value.map(e => e.id).filter(id => !selectedIds.value.includes(id))
    selectedIds.value.push(...idsToAdd)
  }
}

async function confirmBulkDelete() {
  if (!selectedIds.value.length) return
  if (!confirm(`Bạn có chắc chắn muốn xóa ${selectedIds.value.length} nhân viên đã chọn?`)) return
  
  deleteLoading.value = true
  try {
    await employeeService.deleteMultiple(selectedIds.value)
    toast.success('Đã xóa các nhân viên được chọn')
    selectedIds.value = []
    await load()
  } catch {
    toast.error('Không thể xóa — có lỗi xảy ra hoặc dữ liệu đang được liên kết')
  } finally {
    deleteLoading.value = false
  }
}

async function handleFileUpload(event: Event) {
  const target = event.target as HTMLInputElement
  if (!target.files || target.files.length === 0) return
  const file = target.files[0]
  
  importLoading.value = true
  try {
    const data = await file.arrayBuffer()
    const workbook = XLSX.read(data)
    const worksheet = workbook.Sheets[workbook.SheetNames[0]]
    const rows = XLSX.utils.sheet_to_json<any>(worksheet)
    
    let successCount = 0
    let errorCount = 0
    
    for (const row of rows) {
      try {
        const empCode = row['Mã NV']?.toString() || ''
        const fullName = row['Họ tên']?.toString() || row['Tên']?.toString() || ''
        const email = row['Email']?.toString() || ''
        
        if (!empCode || !fullName || !email) {
          throw new Error('Thiếu thông tin bắt buộc: Mã NV, Họ tên, hoặc Email.')
        }

        const deptName = row['Phòng ban']?.toString() || ''
        const posName = row['Chức vụ']?.toString() || ''
        
        const dept = departments.value.find(d => d.name === deptName)
        const pos = positions.value.find(p => p.name === posName)
        
        if (!dept) {
          throw new Error(`Không tìm thấy phòng ban '${deptName}' trong hệ thống.`)
        }
        if (!pos) {
          throw new Error(`Không tìm thấy chức vụ '${posName}' trong hệ thống.`)
        }

        const dto: CreateEmployeeDto = {
          employeeCode: empCode,
          fullName: fullName,
          email: email,
          phone: row['SĐT']?.toString() || '',
          hireDate: row['Ngày vào'] ? new Date(row['Ngày vào']).toISOString().split('T')[0] : new Date().toISOString().split('T')[0],
          departmentId: dept.id,
          positionId: pos.id,
          dateOfBirth: row['Ngày sinh'] ? new Date(row['Ngày sinh']).toISOString().split('T')[0] : undefined,
          gender: row['Giới tính'] === 'Nữ' ? 'Female' : (row['Giới tính'] === 'Nam' ? 'Male' : 'Other')
        }
        await employeeService.create(dto)
        successCount++
      } catch (err: any) {
        console.error('Lỗi khi import dòng:', row, err)
        errorCount++
      }
    }
    
    if (successCount > 0) {
      toast.success(`Đã thêm thành công ${successCount} nhân viên`)
      await load()
    }
    if (errorCount > 0) {
      toast.error(`Có ${errorCount} nhân viên không thể thêm (Vui lòng kiểm tra lại dữ liệu)`)
    }
  } catch (err) {
    console.error(err)
    toast.error('Lỗi khi đọc file Excel')
  } finally {
    importLoading.value = false
    if (fileInput.value) fileInput.value.value = ''
  }
}

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
        <input type="file" ref="fileInput" accept=".xlsx, .xls" class="hidden" @change="handleFileUpload" />
        <AppButton v-if="auth.isHR || auth.isAdmin" variant="secondary" @click="fileInput?.click()" class="mr-2" :loading="importLoading">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" /></svg>
          Nhập Excel
        </AppButton>
        <AppButton v-if="auth.isHR || auth.isAdmin" @click="openCreate">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
          Thêm nhân viên
        </AppButton>
      </template>
    </PageHeader>

    <!-- Filters -->
    <div class="mb-4 flex flex-wrap items-center gap-3">
      <input v-model="search" type="text" placeholder="Tìm theo tên, mã NV, email..." class="h-9 w-full max-w-xs rounded-lg border border-slate-300 px-3 text-sm outline-none focus:border-emerald-500 bg-white text-slate-700" />
      <select v-model="filterDept" class="h-9 rounded-lg border border-slate-300 px-3 text-sm outline-none focus:border-emerald-500 bg-white text-slate-700">
        <option value="">Tất cả phòng ban</option>
        <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
      </select>
      <select v-model="filterStatus" class="h-9 rounded-lg border border-slate-300 px-3 text-sm outline-none focus:border-emerald-500 bg-white text-slate-700">
        <option value="">Tất cả trạng thái</option>
        <option value="Active">Đang làm</option>
        <option value="Inactive">Ngưng</option>
        <option value="OnLeave">Nghỉ phép</option>
        <option value="Resigned">Đã nghỉ</option>
      </select>
      <AppButton variant="ghost" size="sm" @click="search = ''; filterDept = ''; filterStatus = ''">Reset</AppButton>

      <Transition name="fade-slide">
        <div v-if="selectedIds.length > 0 && (auth.isHR || auth.isAdmin)" class="flex items-center gap-2">
          <AppButton variant="danger" size="sm" @click="confirmBulkDelete">
            <svg class="h-4 w-4 mr-1.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6M9 7h6m-7 0a1 1 0 011-1h4a1 1 0 011 1m-7 0H5m14 0h-2" /></svg>
            Xóa đã chọn
          </AppButton>
          <button class="text-sm text-slate-500 hover:text-slate-800 underline" @click="selectedIds = []">Bỏ chọn</button>
        </div>
      </Transition>
    </div>

    <!-- Table -->
    <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có nhân viên nào">
      <template #header-select>
        <input 
          type="checkbox" 
          :checked="isAllSelected" 
          :indeterminate="isIndeterminate"
          @change="selectAll" 
          class="rounded border-slate-300 text-emerald-600 focus:ring-emerald-500 cursor-pointer" 
        />
      </template>

      <template #default="{ row }">
        <td class="px-4 py-3 text-center">
          <input type="checkbox" :checked="selectedIds.includes((row as Employee).id)" @change="toggleSelect((row as Employee).id)" class="rounded border-slate-300 text-emerald-600 focus:ring-emerald-500 cursor-pointer" />
        </td>
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

<style scoped>
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.fade-slide-enter-from,
.fade-slide-leave-to {
  opacity: 0;
  transform: translateX(-8px);
}
</style>
