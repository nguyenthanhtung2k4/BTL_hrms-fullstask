<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { userService } from '../../../services/user.service'
import { useToastStore } from '../../../stores/toast'
import type { Employee } from '../../../types/hr.types'
import AppModal from '../../../components/ui/AppModal.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import { KeyRound, Mail, ShieldAlert } from '@lucide/vue'

const props = defineProps<{ employee: Employee }>()
const emit = defineEmits<{ close: []; saved: [] }>()

const toast = useToastStore()
const saving = ref(false)
const email = ref(props.employee.email || '')
const password = ref('')
const selectedRoles = ref<string[]>(['Employee'])
const autoGeneratePassword = ref(true)

const roleOptions = [
  { value: 'Admin', label: 'Quản trị viên (Admin)' },
  { value: 'HR', label: 'Quản lý Nhân sự (HR)' },
  { value: 'Manager', label: 'Quản lý bộ phận (Manager)' },
  { value: 'PayrollStaff', label: 'Nhân viên tính lương (PayrollStaff)' },
  { value: 'Employee', label: 'Nhân viên thường (Employee)' },
]

import { extractError } from '../../../services/apiClient'

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

onMounted(() => {
  if (autoGeneratePassword.value) {
    password.value = generateRandomPassword()
  }
})

function toggleAutoPassword() {
  if (autoGeneratePassword.value) {
    password.value = generateRandomPassword()
  } else {
    password.value = ''
  }
}

async function save() {
  if (!email.value) {
    toast.error('Email không được để trống')
    return
  }
  if (!password.value || password.value.length < 8) {
    toast.error('Mật khẩu phải chứa ít nhất 8 ký tự')
    return
  }
  if (selectedRoles.value.length === 0) {
    toast.error('Vui lòng chọn ít nhất một vai trò')
    return
  }

  saving.value = true
  try {
    await userService.create({
      employeeId: props.employee.id,
      email: email.value,
      password: password.value,
      roles: selectedRoles.value,
    })
    toast.success(`Đã cấp tài khoản cho nhân viên ${props.employee.fullName} thành công!`)
    emit('saved')
  } catch (err: any) {
    toast.error(extractError(err, 'Cấp tài khoản thất bại'))
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <AppModal title="Cấp tài khoản truy cập hệ thống" size="md" @close="emit('close')">
    <div class="space-y-5">
      <!-- Employee Profile Summary -->
      <div class="flex items-start gap-3 rounded-xl bg-slate-50 p-4 border border-slate-100">
        <div class="h-10 w-10 flex-shrink-0 grid place-items-center rounded-lg bg-emerald-100 text-emerald-700">
          <KeyRound class="h-5 w-5" />
        </div>
        <div>
          <h4 class="text-sm font-semibold text-slate-800">{{ employee.fullName }}</h4>
          <p class="text-xs text-slate-500 mt-0.5">Mã NV: {{ employee.employeeCode }} · Chức vụ: {{ employee.positionName }} · Phòng: {{ employee.departmentName }}</p>
        </div>
      </div>

      <!-- Login Email -->
      <div class="flex flex-col gap-1.5">
        <label class="text-sm font-medium text-slate-700 flex items-center gap-1">
          <Mail class="h-4 w-4 text-slate-400" />
          Email Đăng nhập <span class="text-red-500">*</span>
        </label>
        <input
          v-model="email"
          type="email"
          placeholder="email@hrms.com"
          class="h-10 w-full rounded-lg border border-slate-300 px-3.5 text-sm outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-400"
        />
      </div>

      <!-- Password Setup -->
      <div class="space-y-2">
        <div class="flex items-center justify-between">
          <label class="text-sm font-medium text-slate-700">Mật khẩu khởi tạo <span class="text-red-500">*</span></label>
          <label class="flex items-center gap-1.5 text-xs text-slate-600 cursor-pointer">
            <input
              v-model="autoGeneratePassword"
              type="checkbox"
              class="rounded border-slate-300 text-emerald-600 focus:ring-emerald-500"
              @change="toggleAutoPassword"
            />
            Tự động tạo mật khẩu mạnh
          </label>
        </div>
        
        <input
          v-model="password"
          :type="autoGeneratePassword ? 'text' : 'password'"
          :readonly="autoGeneratePassword"
          placeholder="Nhập mật khẩu (tối thiểu 8 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt)"
          class="h-10 w-full rounded-lg border border-slate-300 px-3.5 text-sm outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-400 bg-white"
          :class="{ 'bg-slate-50 border-slate-200 font-mono text-slate-700': autoGeneratePassword }"
        />
        <p v-if="autoGeneratePassword" class="text-[11px] text-slate-500">
          Hãy sao chép lại mật khẩu này để gửi cho nhân viên đăng nhập lần đầu.
        </p>
      </div>

      <!-- Role Selection -->
      <div class="space-y-2">
        <label class="text-sm font-medium text-slate-700">Phân quyền vai trò <span class="text-red-500">*</span></label>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
          <label
            v-for="role in roleOptions"
            :key="role.value"
            class="flex items-start gap-2.5 rounded-lg border border-slate-200 p-3 hover:bg-slate-50/50 cursor-pointer transition-colors"
            :class="{ 'border-emerald-500 bg-emerald-50/30': selectedRoles.includes(role.value) }"
          >
            <input
              v-model="selectedRoles"
              type="checkbox"
              :value="role.value"
              class="mt-0.5 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500"
            />
            <span class="text-xs font-medium text-slate-700 leading-tight">{{ role.label }}</span>
          </label>
        </div>
      </div>
      
      <!-- Caution banner -->
      <div class="flex gap-2 p-3 rounded-lg border border-amber-200 bg-amber-50 text-[11px] text-amber-800">
        <ShieldAlert class="h-4 w-4 flex-shrink-0 mt-0.5" />
        <div>
          Tài khoản sau khi được cấp sẽ có quyền truy cập tương ứng với vai trò đã gán. Bạn có thể khóa tài khoản hoặc chỉnh sửa phân quyền này bất kỳ lúc nào.
        </div>
      </div>
    </div>
    <template #footer>
      <AppButton variant="secondary" @click="emit('close')">Hủy</AppButton>
      <AppButton :loading="saving" @click="save">Cấp tài khoản</AppButton>
    </template>
  </AppModal>
</template>
