<script setup lang="ts">
import { ref, watch } from 'vue'
import { employeeService } from '../../../services/employee.service'
import { userService } from '../../../services/user.service'
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

const toast = useToastStore()
const saving = ref(false)

const form = ref({
  employeeCode: '', fullName: '', email: '', phone: '', gender: '',
  dateOfBirth: '', hireDate: '', departmentId: '', positionId: '', managerEmployeeId: '', status: 'Active',
})

const createAccount = ref(true)
const accountEmail = ref('')
const accountPassword = ref('')
const accountRoles = ref<string[]>(['Employee'])
const autoPassword = ref(true)

const roleOptions = [
  { value: 'Admin', label: 'Quản trị viên (Admin)' },
  { value: 'HR', label: 'Quản lý Nhân sự (HR)' },
  { value: 'Manager', label: 'Quản lý bộ phận (Manager)' },
  { value: 'PayrollStaff', label: 'Nhân viên tính lương (PayrollStaff)' },
  { value: 'Employee', label: 'Nhân viên thường (Employee)' },
]

function generateRandomPassword() {
  const lower = 'abcdefghijklmnopqrstuvwxyz'
  const upper = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'
  const digits = '0123456789'
  const special = '!@#$%^&*'
  
  let pass = ''
  for (let i = 0; i < 3; i++) pass += lower.charAt(Math.floor(Math.random() * lower.length))
  for (let i = 0; i < 3; i++) pass += upper.charAt(Math.floor(Math.random() * upper.length))
  for (let i = 0; i < 2; i++) pass += digits.charAt(Math.floor(Math.random() * digits.length))
  for (let i = 0; i < 2; i++) pass += special.charAt(Math.floor(Math.random() * special.length))
  
  return pass.split('').sort(() => 0.5 - Math.random()).join('')
}

function toggleAutoPassword() {
  if (autoPassword.value) {
    accountPassword.value = generateRandomPassword()
  } else {
    accountPassword.value = ''
  }
}

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
    createAccount.value = true
    accountEmail.value = ''
    accountPassword.value = generateRandomPassword()
    accountRoles.value = ['Employee']
    autoPassword.value = true
  }
  errors.value = {}
}, { immediate: true })

watch(() => form.value.email, (newVal) => {
  if (!props.edit) {
    accountEmail.value = newVal
  }
})

watch(createAccount, (val) => {
  if (val && autoPassword.value && !accountPassword.value) {
    accountPassword.value = generateRandomPassword()
  }
})

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

  if (!props.edit && createAccount.value) {
    if (!accountEmail.value.trim()) {
      toast.error('Email đăng nhập tài khoản không được để trống')
      return
    }
    if (!accountPassword.value || accountPassword.value.length < 8) {
      toast.error('Mật khẩu tài khoản phải từ 8 ký tự')
      return
    }
    if (accountRoles.value.length === 0) {
      toast.error('Chọn ít nhất một vai trò cho tài khoản')
      return
    }
  }

  saving.value = true
  try {
    if (props.edit) {
      const dto: UpdateEmployeeDto = { fullName: form.value.fullName, phone: form.value.phone || undefined, gender: form.value.gender || undefined, dateOfBirth: form.value.dateOfBirth || undefined, hireDate: form.value.hireDate, departmentId: form.value.departmentId, positionId: form.value.positionId, managerEmployeeId: form.value.managerEmployeeId || undefined, status: form.value.status as any }
      await employeeService.update(props.edit.id, dto)
      toast.success('Cập nhật nhân viên thành công')
    } else {
      const dto: CreateEmployeeDto = { employeeCode: form.value.employeeCode, fullName: form.value.fullName, email: form.value.email, phone: form.value.phone || undefined, gender: form.value.gender || undefined, dateOfBirth: form.value.dateOfBirth || undefined, hireDate: form.value.hireDate, departmentId: form.value.departmentId, positionId: form.value.positionId, managerEmployeeId: form.value.managerEmployeeId || undefined }
      const newEmp = await employeeService.create(dto)
      toast.success('Tạo nhân viên thành công')

      if (createAccount.value) {
        try {
          await userService.create({
            employeeId: newEmp.id,
            email: accountEmail.value,
            password: accountPassword.value,
            roles: accountRoles.value,
          })
          toast.success(`Đã cấp tài khoản truy cập cho ${form.value.fullName}`)
        } catch (err: any) {
          toast.error(`Nhân viên đã tạo nhưng lỗi khi cấp tài khoản: ${err?.response?.data?.message ?? 'Lỗi không xác định'}`)
        }
      }
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

      <!-- Option to create system login account (Only when creating new employee) -->
      <div v-if="!edit" class="col-span-2 border-t border-slate-200 pt-4 mt-2 space-y-4">
        <label class="flex items-center gap-2.5 cursor-pointer">
          <input
            v-model="createAccount"
            type="checkbox"
            class="rounded border-slate-300 text-emerald-600 focus:ring-emerald-500"
          />
          <span class="text-sm font-semibold text-slate-800">Cấp tài khoản đăng nhập hệ thống ngay lập tức</span>
        </label>

        <div v-if="createAccount" class="grid grid-cols-2 gap-4 bg-slate-50 p-4 rounded-xl border border-slate-100">
          <!-- Account Email -->
          <div class="flex flex-col gap-1">
            <label class="text-xs font-medium text-slate-600">Email đăng nhập</label>
            <input
              v-model="accountEmail"
              type="email"
              placeholder="email@hrms.com"
              class="h-9 w-full rounded-lg border border-slate-300 px-3 text-xs outline-none bg-white focus:border-emerald-500"
            />
          </div>

          <!-- Account Roles -->
          <div class="flex flex-col gap-1">
            <label class="text-xs font-medium text-slate-600 mb-1">Vai trò quyền hạn</label>
            <div class="flex flex-wrap gap-2">
              <label
                v-for="role in roleOptions"
                :key="role.value"
                class="flex items-center gap-1.5 px-2 py-1 rounded bg-white border border-slate-200 text-[11px] font-medium text-slate-700 cursor-pointer hover:bg-slate-50"
                :class="{ 'border-emerald-500 bg-emerald-50/10': accountRoles.includes(role.value) }"
              >
                <input
                  v-model="accountRoles"
                  type="checkbox"
                  :value="role.value"
                  class="rounded border-slate-300 text-emerald-600 focus:ring-emerald-500 scale-90"
                />
                {{ role.label }}
              </label>
            </div>
          </div>

          <!-- Password Setup -->
          <div class="col-span-2 space-y-1.5 mt-1">
            <div class="flex items-center justify-between">
              <label class="text-xs font-medium text-slate-600">Mật khẩu</label>
              <label class="flex items-center gap-1.5 text-[10px] text-slate-500 cursor-pointer">
                <input
                  v-model="autoPassword"
                  type="checkbox"
                  class="rounded border-slate-300 text-emerald-600 focus:ring-emerald-500 scale-90"
                  @change="toggleAutoPassword"
                />
                Tự động tạo mật khẩu mạnh
              </label>
            </div>
            <input
              v-model="accountPassword"
              :type="autoPassword ? 'text' : 'password'"
              :readonly="autoPassword"
              placeholder="Nhập mật khẩu (tối thiểu 6 ký tự)"
              class="h-9 w-full rounded-lg border border-slate-300 px-3 text-xs outline-none bg-white focus:border-emerald-500"
              :class="{ 'bg-slate-100 border-slate-200 font-mono text-slate-600': autoPassword }"
            />
          </div>
        </div>
      </div>
    </div>
    <template #footer>
      <AppButton variant="secondary" @click="emit('close')">Hủy</AppButton>
      <AppButton :loading="saving" @click="save">{{ edit ? 'Cập nhật' : 'Tạo mới' }}</AppButton>
    </template>
  </AppModal>
</template>
