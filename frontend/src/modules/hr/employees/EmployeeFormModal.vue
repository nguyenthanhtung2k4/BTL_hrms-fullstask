<script setup lang="ts">
import { ref, watch } from 'vue'
import { employeeService } from '../../../services/employee.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { Employee, CreateEmployeeDto, UpdateEmployeeDto, Department, Position } from '../../../types/hr.types'
import AppModal from '../../../components/ui/AppModal.vue'
import AppInput from '../../../components/ui/AppInput.vue'
import AppButton from '../../../components/ui/AppButton.vue'

const props = defineProps<{
  edit: Employee | null
  departments: Department[]
  positions: Position[]
  employees: Employee[]
}>()
const emit = defineEmits<{ close: []; saved: [] }>()

const auth = useAuthStore()
const toast = useToastStore()
const saving = ref(false)

const form = ref({
  employeeCode: '', fullName: '', email: '', phone: '', gender: '',
  dateOfBirth: '', hireDate: '', departmentId: '', positionId: '', managerEmployeeId: '', status: 'Active',
})
const errors = ref<Record<string, string>>({})

watch(() => props.edit, (e) => {
  if (e) {
    form.value = {
      employeeCode: e.employeeCode, fullName: e.fullName, email: e.email, phone: e.phone ?? '',
      gender: e.gender ?? '', dateOfBirth: e.dateOfBirth ? e.dateOfBirth.split('T')[0] : '',
      hireDate: e.hireDate ? e.hireDate.split('T')[0] : '', departmentId: e.departmentId,
      positionId: e.positionId, managerEmployeeId: e.managerEmployeeId ?? '', status: e.status,
    }
  } else {
    form.value = { employeeCode: '', fullName: '', email: '', phone: '', gender: '', dateOfBirth: '', hireDate: new Date().toISOString().split('T')[0], departmentId: '', positionId: '', managerEmployeeId: '', status: 'Active' }
  }
  errors.value = {}
}, { immediate: true })

function validate() {
  errors.value = {}
  if (!form.value.employeeCode.trim()) errors.value.employeeCode = 'Mã NV bắt buộc'
  if (!form.value.fullName.trim()) errors.value.fullName = 'Họ tên bắt buộc'
  if (!form.value.email.trim()) errors.value.email = 'Email bắt buộc'
  if (!form.value.departmentId) errors.value.departmentId = 'Phòng ban bắt buộc'
  if (!form.value.positionId) errors.value.positionId = 'Chức vụ bắt buộc'
  if (!form.value.hireDate) errors.value.hireDate = 'Ngày vào làm bắt buộc'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (props.edit) {
      const dto: UpdateEmployeeDto = { fullName: form.value.fullName, phone: form.value.phone || undefined, gender: form.value.gender || undefined, dateOfBirth: form.value.dateOfBirth || undefined, hireDate: form.value.hireDate, departmentId: form.value.departmentId, positionId: form.value.positionId, managerEmployeeId: form.value.managerEmployeeId || undefined, status: form.value.status as any }
      await employeeService.update(props.edit.id, dto)
      toast.success('Cập nhật nhân viên thành công')
    } else {
      const dto: CreateEmployeeDto = { employeeCode: form.value.employeeCode, fullName: form.value.fullName, email: form.value.email, phone: form.value.phone || undefined, gender: form.value.gender || undefined, dateOfBirth: form.value.dateOfBirth || undefined, hireDate: form.value.hireDate, departmentId: form.value.departmentId, positionId: form.value.positionId, managerEmployeeId: form.value.managerEmployeeId || undefined }
      await employeeService.create(dto)
      toast.success('Tạo nhân viên thành công')
    }
    emit('saved')
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Lưu thất bại') }
  finally { saving.value = false }
}
</script>

<template>
  <AppModal :title="edit ? 'Sửa nhân viên' : 'Thêm nhân viên'" size="lg" @close="emit('close')">
    <div class="grid grid-cols-2 gap-4">
      <AppInput id="emp-code" v-model="form.employeeCode" label="Mã nhân viên" required :disabled="!!edit" :error="errors.employeeCode" placeholder="VD: NV001" />
      <AppInput id="emp-name" v-model="form.fullName" label="Họ tên" required :error="errors.fullName" />
      <AppInput id="emp-email" v-model="form.email" label="Email" type="email" required :disabled="!!edit" :error="errors.email" />
      <AppInput id="emp-phone" v-model="form.phone" label="Số điện thoại" type="tel" />
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium text-slate-700">Giới tính</label>
        <select v-model="form.gender" class="h-9 rounded-lg border border-slate-300 px-3 text-sm outline-none focus:border-emerald-500 bg-white">
          <option value="">-- Chọn --</option>
          <option value="Nam">Nam</option>
          <option value="Nữ">Nữ</option>
          <option value="Khác">Khác</option>
        </select>
      </div>
      <AppInput id="emp-dob" v-model="form.dateOfBirth" label="Ngày sinh" type="date" />
      <AppInput id="emp-hire" v-model="form.hireDate" label="Ngày vào làm" type="date" required :error="errors.hireDate" />
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium text-slate-700">Phòng ban <span class="text-red-500">*</span></label>
        <select v-model="form.departmentId" :class="['h-9 rounded-lg border px-3 text-sm outline-none bg-white', errors.departmentId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
          <option value="">-- Chọn phòng ban --</option>
          <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
        </select>
        <p v-if="errors.departmentId" class="text-xs text-red-500">{{ errors.departmentId }}</p>
      </div>
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium text-slate-700">Chức vụ <span class="text-red-500">*</span></label>
        <select v-model="form.positionId" :class="['h-9 rounded-lg border px-3 text-sm outline-none bg-white', errors.positionId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
          <option value="">-- Chọn chức vụ --</option>
          <option v-for="p in positions" :key="p.id" :value="p.id">{{ p.name }}</option>
        </select>
        <p v-if="errors.positionId" class="text-xs text-red-500">{{ errors.positionId }}</p>
      </div>
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium text-slate-700">Quản lý trực tiếp</label>
        <select v-model="form.managerEmployeeId" class="h-9 rounded-lg border border-slate-300 px-3 text-sm outline-none bg-white focus:border-emerald-500">
          <option value="">-- Không có --</option>
          <option v-for="e in employees.filter(e2 => !edit || e2.id !== edit.id)" :key="e.id" :value="e.id">{{ e.fullName }} ({{ e.employeeCode }})</option>
        </select>
      </div>
      <div v-if="edit" class="flex flex-col gap-1">
        <label class="text-sm font-medium text-slate-700">Trạng thái</label>
        <select v-model="form.status" class="h-9 rounded-lg border border-slate-300 px-3 text-sm outline-none bg-white focus:border-emerald-500">
          <option value="Active">Đang làm</option>
          <option value="Inactive">Ngưng</option>
          <option value="OnLeave">Nghỉ phép</option>
          <option value="Resigned">Đã nghỉ</option>
        </select>
      </div>
    </div>
    <template #footer>
      <AppButton variant="secondary" @click="emit('close')">Hủy</AppButton>
      <AppButton :loading="saving" @click="save">{{ edit ? 'Cập nhật' : 'Tạo mới' }}</AppButton>
    </template>
  </AppModal>
</template>
