<script setup lang="ts">
import { ref } from 'vue'
import { userService } from '../../../services/user.service'
import { extractError } from '../../../services/apiClient'
import { useToastStore } from '../../../stores/toast'
import type { Employee } from '../../../types/hr.types'
import type { UserAccount } from '../../../types/user.types'
import AppModal from '../../../components/ui/AppModal.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import { Shield } from '@lucide/vue'

const props = defineProps<{ employee: Employee; userAccount: UserAccount }>()
const emit = defineEmits<{ close: []; saved: [] }>()

const toast = useToastStore()
const saving = ref(false)
const selectedRoles = ref<string[]>([...props.userAccount.roles])

const roleOptions = [
  { value: 'Admin', label: 'Quản trị viên (Admin)' },
  { value: 'HR', label: 'Quản lý Nhân sự (HR)' },
  { value: 'Manager', label: 'Quản lý bộ phận (Manager)' },
  { value: 'PayrollStaff', label: 'Nhân viên tính lương (PayrollStaff)' },
  { value: 'Employee', label: 'Nhân viên thường (Employee)' },
]

async function save() {
  if (selectedRoles.value.length === 0) {
    toast.error('Vui lòng chọn ít nhất một vai trò')
    return
  }

  saving.value = true
  try {
    await userService.updateRoles(props.userAccount.id, selectedRoles.value)
    toast.success(`Đã cập nhật vai trò cho tài khoản ${props.userAccount.email} thành công!`)
    emit('saved')
  } catch (err: any) {
    toast.error(extractError(err, 'Cập nhật vai trò thất bại'))
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <AppModal title="Thay đổi vai trò người dùng" size="sm" @close="emit('close')">
    <div class="space-y-4">
      <!-- Target Account Summary -->
      <div class="rounded-lg bg-slate-50 p-3 border border-slate-100">
        <div class="text-sm font-semibold text-slate-800">{{ employee.fullName }}</div>
        <div class="text-xs text-slate-500 font-mono mt-0.5">{{ userAccount.email }}</div>
      </div>

      <!-- Role Checkbox List -->
      <div class="space-y-2">
        <label class="text-sm font-medium text-slate-700 flex items-center gap-1.5">
          <Shield class="h-4 w-4 text-emerald-600" />
          Chọn vai trò
        </label>
        
        <div class="space-y-2">
          <label
            v-for="role in roleOptions"
            :key="role.value"
            class="flex items-center gap-2.5 rounded-lg border border-slate-200 p-2.5 hover:bg-slate-50 cursor-pointer transition-colors"
            :class="{ 'border-emerald-500 bg-emerald-50/10': selectedRoles.includes(role.value) }"
          >
            <input
              v-model="selectedRoles"
              type="checkbox"
              :value="role.value"
              class="rounded border-slate-300 text-emerald-600 focus:ring-emerald-500"
            />
            <span class="text-xs font-medium text-slate-700">{{ role.label }}</span>
          </label>
        </div>
      </div>
    </div>
    <template #footer>
      <AppButton variant="secondary" @click="emit('close')">Hủy</AppButton>
      <AppButton :loading="saving" @click="save">Cập nhật vai trò</AppButton>
    </template>
  </AppModal>
</template>
