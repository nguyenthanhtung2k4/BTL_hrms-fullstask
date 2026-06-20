<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { contractService } from '../../../services/contract.service'
import { employeeService } from '../../../services/employee.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { Contract, Employee, CreateContractDto, UpdateContractDto } from '../../../types/hr.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import AppModal from '../../../components/ui/AppModal.vue'
import AppInput from '../../../components/ui/AppInput.vue'
import AppConfirm from '../../../components/ui/AppConfirm.vue'

const auth = useAuthStore()
const toast = useToastStore()

const contracts = ref<Contract[]>([])
const employees = ref<Employee[]>([])
const loading = ref(false)
const showForm = ref(false)
const editTarget = ref<Contract | null>(null)
const deleteTarget = ref<Contract | null>(null)
const deleteLoading = ref(false)
const saving = ref(false)
const filterEmployee = ref('')
const filterStatus = ref('')

const form = ref({ contractNumber: '', employeeId: '', contractType: 'Chính thức', startDate: '', endDate: '', baseSalary: '', status: 'Active' })
const errors = ref<Record<string, string>>({})

const columns = [
  { key: 'no', label: 'Số HĐ' }, { key: 'employee', label: 'Nhân viên' }, { key: 'type', label: 'Loại HĐ' },
  { key: 'salary', label: 'Lương cơ bản' }, { key: 'start', label: 'Từ ngày' }, { key: 'end', label: 'Đến ngày' },
  { key: 'status', label: 'Trạng thái' }, { key: 'actions', label: '', class: 'text-right' },
]

const filtered = computed(() => {
  let list = contracts.value
  if (filterEmployee.value) list = list.filter((c) => c.employeeId === filterEmployee.value)
  if (filterStatus.value) list = list.filter((c) => c.status === filterStatus.value)
  return list
})

async function load() {
  loading.value = true
  try { [contracts.value, employees.value] = await Promise.all([contractService.getAll(), employeeService.getAll()]) }
  catch { toast.error('Không thể tải dữ liệu') }
  finally { loading.value = false }
}

function openCreate() { editTarget.value = null; form.value = { contractNumber: '', employeeId: '', contractType: 'Chính thức', startDate: new Date().toISOString().split('T')[0], endDate: '', baseSalary: '', status: 'Active' }; errors.value = {}; showForm.value = true }
function openEdit(c: Contract) { editTarget.value = c; form.value = { contractNumber: c.contractNumber, employeeId: c.employeeId, contractType: c.contractType, startDate: c.startDate.split('T')[0], endDate: c.endDate ? c.endDate.split('T')[0] : '', baseSalary: String(c.baseSalary), status: c.status }; errors.value = {}; showForm.value = true }

function validate() {
  errors.value = {}
  if (!form.value.contractNumber.trim()) errors.value.contractNumber = 'Số HĐ bắt buộc'
  if (!form.value.employeeId) errors.value.employeeId = 'Nhân viên bắt buộc'
  if (!form.value.startDate) errors.value.startDate = 'Ngày bắt đầu bắt buộc'
  if (!form.value.baseSalary || isNaN(Number(form.value.baseSalary))) errors.value.baseSalary = 'Lương hợp lệ bắt buộc'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (editTarget.value) {
      const dto: UpdateContractDto = { contractType: form.value.contractType as any, startDate: form.value.startDate, endDate: form.value.endDate || undefined, baseSalary: Number(form.value.baseSalary), status: form.value.status as any }
      await contractService.update(editTarget.value.id, dto)
      toast.success('Cập nhật hợp đồng thành công')
    } else {
      const dto: CreateContractDto = { contractNumber: form.value.contractNumber, employeeId: form.value.employeeId, contractType: form.value.contractType as any, startDate: form.value.startDate, endDate: form.value.endDate || undefined, baseSalary: Number(form.value.baseSalary) }
      await contractService.create(dto)
      toast.success('Tạo hợp đồng thành công')
    }
    showForm.value = false; await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Lưu thất bại') }
  finally { saving.value = false }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try { await contractService.delete(deleteTarget.value.id); toast.success('Đã xóa hợp đồng'); deleteTarget.value = null; await load() }
  catch { toast.error('Xóa thất bại') }
  finally { deleteLoading.value = false }
}

function fmt(d?: string) { return d ? new Date(d).toLocaleDateString('vi-VN') : '—' }
function fmtMoney(n: number) { return n.toLocaleString('vi-VN') + ' ₫' }

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Hợp đồng" subtitle="Quản lý hợp đồng lao động" :breadcrumbs="[{ label: 'Nhân sự' }, { label: 'Hợp đồng' }]">
      <template #actions>
        <AppButton v-if="auth.isHR" @click="openCreate">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
          Thêm hợp đồng
        </AppButton>
      </template>
    </PageHeader>

    <div class="mb-6 flex gap-4 items-center">
      <div class="flex gap-3">
        <select v-model="filterEmployee" class="px-4 py-2.5 rounded-lg border border-slate-200 bg-white text-sm text-slate-700 outline-none focus:border-blue-500 focus:ring-4 focus:ring-blue-100 transition-all duration-200">
          <option value="">Tất cả nhân viên</option>
          <option v-for="e in employees" :key="e.id" :value="e.id">{{ e.fullName }}</option>
        </select>
        <select v-model="filterStatus" class="px-4 py-2.5 rounded-lg border border-slate-200 bg-white text-sm text-slate-700 outline-none focus:border-blue-500 focus:ring-4 focus:ring-blue-100 transition-all duration-200">
          <option value="">Tất cả trạng thái</option>
          <option value="Active">Hiệu lực</option>
          <option value="Expired">Hết hạn</option>
          <option value="Terminated">Chấm dứt</option>
        </select>
      </div>
    </div>

    <div class="rounded-lg border border-slate-200 bg-white shadow-sm overflow-hidden">
    <AppTable :columns="columns" :rows="filtered" :loading="loading" row-key="id" empty-text="Chưa có hợp đồng nào">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-mono">{{ (row as Contract).contractNumber }}</td>
        <td class="px-4 py-3 text-sm font-medium">{{ (row as Contract).employeeName }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ (row as Contract).contractType }}</td>
        <td class="px-4 py-3 text-sm font-medium text-emerald-700">{{ fmtMoney((row as Contract).baseSalary) }}</td>
        <td class="px-4 py-3 text-sm text-slate-500">{{ fmt((row as Contract).startDate) }}</td>
        <td class="px-4 py-3 text-sm text-slate-500">{{ fmt((row as Contract).endDate) }}</td>
        <td class="px-4 py-3"><AppBadge :status="(row as Contract).status" /></td>
        <td class="px-4 py-3 text-right">
          <div class="flex justify-end gap-2">
            <AppButton v-if="auth.isHR" size="sm" variant="secondary" @click="openEdit(row as Contract)">Sửa</AppButton>
            <AppButton v-if="auth.isAdmin" size="sm" variant="danger" @click="deleteTarget = row as Contract">Xóa</AppButton>
          </div>
        </td>
      </template>
    </AppTable>
    </div>

    <AppModal v-if="showForm" :title="editTarget ? 'Sửa hợp đồng' : 'Thêm hợp đồng'" size="lg" @close="showForm = false">
      <div class="grid grid-cols-2 gap-4">
        <AppInput id="ct-no" v-model="form.contractNumber" label="Số hợp đồng" required :disabled="!!editTarget" :error="errors.contractNumber" />
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Nhân viên <span class="text-red-500">*</span></label>
          <select v-model="form.employeeId" :disabled="!!editTarget" :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none', errors.employeeId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn nhân viên --</option>
            <option v-for="e in employees" :key="e.id" :value="e.id">{{ e.fullName }} ({{ e.employeeCode }})</option>
          </select>
          <p v-if="errors.employeeId" class="text-xs text-red-500">{{ errors.employeeId }}</p>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Loại hợp đồng</label>
          <select v-model="form.contractType" class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500">
            <option>Chính thức</option>
            <option>Thử việc</option>
            <option>Part-time</option>
          </select>
        </div>
        <AppInput id="ct-salary" v-model="form.baseSalary" label="Lương cơ bản (₫)" type="number" required :error="errors.baseSalary" placeholder="VD: 15000000" />
        <AppInput id="ct-start" v-model="form.startDate" label="Từ ngày" type="date" required :error="errors.startDate" />
        <AppInput id="ct-end" v-model="form.endDate" label="Đến ngày" type="date" hint="Để trống nếu không thời hạn" />
        <div v-if="editTarget" class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Trạng thái</label>
          <select v-model="form.status" class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500">
            <option value="Active">Hiệu lực</option>
            <option value="Expired">Hết hạn</option>
            <option value="Terminated">Chấm dứt</option>
          </select>
        </div>
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="showForm = false">Hủy</AppButton>
        <AppButton :loading="saving" @click="save">{{ editTarget ? 'Cập nhật' : 'Tạo mới' }}</AppButton>
      </template>
    </AppModal>

    <AppConfirm v-if="deleteTarget" title="Xóa hợp đồng" :message="`Xóa hợp đồng &quot;${deleteTarget.contractNumber}&quot;?`" confirm-text="Xóa" :danger="true" :loading="deleteLoading" @confirm="confirmDelete" @cancel="deleteTarget = null" />
  </div>
</template>
