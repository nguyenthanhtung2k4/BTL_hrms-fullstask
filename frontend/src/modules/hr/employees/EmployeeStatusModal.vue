<script setup lang="ts">
import { ref } from 'vue'
import { employeeService } from '../../../services/employee.service'
import { extractError } from '../../../services/apiClient'
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
  } catch (err: any) { toast.error(extractError(err, 'Cập nhật thất bại')) }
  finally { saving.value = false }
}
</script>

<template>
  <AppModal title="Đổi trạng thái nhân viên" size="sm" @close="emit('close')">
    <div class="space-y-4">
      <div class="rounded-lg bg-slate-50 p-3">
        <div class="text-sm font-semibold text-slate-800">{{ employee.fullName }}</div>
        <div class="text-xs text-slate-500 mt-0.5">{{ employee.employeeCode }} · {{ employee.departmentName }}</div>
        <div class="mt-2 flex items-center gap-2">
          <span class="text-xs text-slate-500">Hiện tại:</span>
          <AppBadge :status="employee.status" />
        </div>
      </div>

      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium text-slate-700">Trạng thái mới <span class="text-red-500">*</span></label>
        <select v-model="newStatus" class="h-9 rounded-lg border border-slate-300 px-3 text-sm outline-none bg-white focus:border-emerald-500">
          <option v-for="opt in statusOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
        </select>
      </div>

      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium text-slate-700">Lý do</label>
        <textarea v-model="reason" rows="3" placeholder="Nhập lý do đổi trạng thái..." class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm outline-none focus:border-emerald-500" />
      </div>
    </div>
    <template #footer>
      <AppButton variant="secondary" @click="emit('close')">Hủy</AppButton>
      <AppButton :loading="saving" :disabled="newStatus === employee.status" @click="save">Xác nhận</AppButton>
    </template>
  </AppModal>
</template>
