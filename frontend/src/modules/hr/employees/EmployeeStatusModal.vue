<script setup lang="ts">
import { ref } from 'vue'
import { employeeService } from '../../../services/employee.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { Employee } from '../../../types/hr.types'
import AppModal from '../../../components/ui/AppModal.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'

const props = defineProps<{ employee: Employee }>()
const emit = defineEmits<{ close: []; saved: [] }>()

const auth = useAuthStore()
const toast = useToastStore()
const saving = ref(false)
const newStatus = ref(props.employee.status)
const reason = ref('')

const statusOptions = [
  { value: 'Active', label: 'Đang làm' },
  { value: 'Inactive', label: 'Ngưng' },
  { value: 'OnLeave', label: 'Nghỉ phép' },
  { value: 'Resigned', label: 'Đã nghỉ' },
]

async function save() {
  if (newStatus.value === props.employee.status) { emit('close'); return }
  saving.value = true
  try {
    await employeeService.changeStatus(props.employee.id, {
      newStatus: newStatus.value as any,
      reason: reason.value || undefined,
      changedByUserId: auth.userId ?? '',
    })
    toast.success(`Đã đổi trạng thái sang "${newStatus.value}"`)
    emit('saved')
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Cập nhật thất bại') }
  finally { saving.value = false }
}
</script>

<template>
  <AppModal title="Đổi trạng thái nhân viên" size="sm" @close="emit('close')">
    <div class="space-y-5">
      <div class="rounded-2xl border border-slate-200 bg-slate-50 p-4">
        <div class="text-sm font-semibold text-slate-900">{{ employee.fullName }}</div>
        <div class="mt-0.5 text-xs text-slate-500">{{ employee.employeeCode }} · {{ employee.departmentName }}</div>
        <div class="mt-2 flex items-center gap-2">
          <span class="text-xs text-slate-500">Hiện tại:</span>
          <AppBadge :status="employee.status" />
        </div>
      </div>

      <div class="flex flex-col space-y-1.5">
        <label class="text-sm font-medium text-slate-700">Trạng thái mới <span class="ml-1 text-red-500">*</span></label>
        <select v-model="newStatus" class="h-10 rounded-lg border border-slate-200 bg-white px-3.5 text-sm outline-none transition-all duration-200 focus:border-blue-500 focus:ring-4 focus:ring-blue-100">
          <option v-for="opt in statusOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
        </select>
      </div>

      <div class="flex flex-col space-y-1.5">
        <label class="text-sm font-medium text-slate-700">Lý do</label>
        <textarea v-model="reason" rows="3" placeholder="Nhập lý do đổi trạng thái..." class="min-h-24 w-full rounded-lg border border-slate-200 bg-white px-3.5 py-2.5 text-sm text-slate-900 outline-none transition-all duration-200 placeholder:text-slate-400 focus:border-blue-500 focus:ring-4 focus:ring-blue-100" />
      </div>
    </div>
    <template #footer>
      <AppButton variant="secondary" @click="emit('close')">Hủy</AppButton>
      <AppButton :loading="saving" :disabled="newStatus === employee.status" @click="save">Xác nhận</AppButton>
    </template>
  </AppModal>
</template>
