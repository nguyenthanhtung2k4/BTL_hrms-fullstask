<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { userService } from '../../../services/user.service'
import { useToastStore } from '../../../stores/toast'
import type { Employee } from '../../../types/hr.types'
import type { UserAccount } from '../../../types/user.types'
import AppModal from '../../../components/ui/AppModal.vue'
import AppButton from '../../../components/ui/AppButton.vue'

const props = defineProps<{ employee: Employee; userAccount: UserAccount }>()
const emit = defineEmits<{ close: []; saved: [] }>()

const toast = useToastStore()
const saving = ref(false)
const password = ref('')
const autoGeneratePassword = ref(true)

function generateRandomPassword() {
  const chars = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*'
  let pass = ''
  for (let i = 0; i < 10; i++) {
    pass += chars.charAt(Math.floor(Math.random() * chars.length))
  }
  return pass
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
  if (!password.value || password.value.length < 6) {
    toast.error('Mật khẩu phải có ít nhất 6 ký tự')
    return
  }

  saving.value = true
  try {
    await userService.resetPassword(props.userAccount.id, password.value)
    toast.success(`Đã đặt lại mật khẩu cho tài khoản ${props.userAccount.email} thành công!`)
    emit('saved')
  } catch (err: any) {
    toast.error(err?.response?.data?.message ?? 'Đặt lại mật khẩu thất bại')
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <AppModal title="Đặt lại mật khẩu tài khoản" size="sm" @close="emit('close')">
    <div class="space-y-4">
      <!-- Target Account Summary -->
      <div class="rounded-lg bg-slate-50 p-3 border border-slate-100">
        <div class="text-xs text-slate-500">Đặt lại mật khẩu cho:</div>
        <div class="text-sm font-semibold text-slate-800 mt-0.5">{{ employee.fullName }}</div>
        <div class="text-xs text-slate-600 mt-1 font-mono bg-white inline-block px-1.5 py-0.5 rounded border">
          {{ userAccount.email }}
        </div>
      </div>

      <!-- Password Input -->
      <div class="space-y-2">
        <div class="flex items-center justify-between">
          <label class="text-sm font-medium text-slate-700">Mật khẩu mới <span class="text-red-500">*</span></label>
          <label class="flex items-center gap-1.5 text-xs text-slate-600 cursor-pointer">
            <input
              v-model="autoGeneratePassword"
              type="checkbox"
              class="rounded border-slate-300 text-emerald-600 focus:ring-emerald-500"
              @change="toggleAutoPassword"
            />
            Tự động sinh
          </label>
        </div>
        
        <input
          v-model="password"
          :type="autoGeneratePassword ? 'text' : 'password'"
          :readonly="autoGeneratePassword"
          placeholder="Nhập mật khẩu mới (tối thiểu 6 ký tự)"
          class="h-10 w-full rounded-lg border border-slate-300 px-3 text-sm outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-400 bg-white"
          :class="{ 'bg-slate-50 border-slate-200 font-mono text-slate-700': autoGeneratePassword }"
        />
        <p v-if="autoGeneratePassword" class="text-[11px] text-slate-500">
          Hãy sao chép mật khẩu này và gửi riêng cho nhân viên.
        </p>
      </div>
    </div>
    <template #footer>
      <AppButton variant="secondary" @click="emit('close')">Hủy</AppButton>
      <AppButton :loading="saving" @click="save">Xác nhận đặt lại</AppButton>
    </template>
  </AppModal>
</template>
