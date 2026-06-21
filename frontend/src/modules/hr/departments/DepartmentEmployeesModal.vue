<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { employeeService } from '../../../services/employee.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { Employee } from '../../../types/hr.types'
import AppModal from '../../../components/ui/AppModal.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'

const props = defineProps<{
  departmentId: string
  departmentName: string
}>()
const emit = defineEmits<{ close: [] }>()

const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()

const employees = ref<Employee[]>([])
const loading = ref(false)

async function load() {
  loading.value = true
  try {
    const all = await employeeService.getAll()
    employees.value = all.filter(e => e.departmentId === props.departmentId)
  } catch {
    toast.error('Không thể tải danh sách nhân viên của phòng ban')
  } finally {
    loading.value = false
  }
}

function viewDetail(empId: string) {
  emit('close')
  router.push(`/hr/employees/${empId}`)
}

onMounted(load)
</script>

<template>
  <AppModal :title="`Thành viên phòng ban: ${departmentName}`" size="lg" @close="emit('close')">
    <div v-if="loading" class="py-12 flex justify-center items-center">
      <div class="h-8 w-8 animate-spin rounded-full border-4 border-emerald-500 border-t-transparent" />
    </div>

    <div v-else-if="employees.length === 0" class="py-12 text-center text-slate-400 text-sm">
      Phòng ban này hiện chưa có nhân viên nào.
    </div>

    <div v-else class="overflow-x-auto">
      <table class="w-full text-left border-collapse text-sm">
        <thead>
          <tr class="border-b border-slate-200 bg-slate-50 text-[11px] font-semibold text-slate-500 uppercase tracking-wider">
            <th class="px-4 py-3">Mã NV</th>
            <th class="px-4 py-3">Họ và tên</th>
            <th class="px-4 py-3">Chức vụ</th>
            <th class="px-4 py-3">Thông tin liên hệ</th>
            <th class="px-4 py-3">Trạng thái</th>
            <th v-if="auth.isManager || auth.isHR || auth.isAdmin" class="px-4 py-3 text-right">Thao tác</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-slate-100">
          <tr v-for="emp in employees" :key="emp.id" class="hover:bg-slate-50/50 transition-colors">
            <td class="px-4 py-3 text-xs font-mono text-slate-600">{{ emp.employeeCode }}</td>
            <td class="px-4 py-3 font-medium text-slate-900">{{ emp.fullName }}</td>
            <td class="px-4 py-3 text-slate-600">{{ emp.positionName }}</td>
            <td class="px-4 py-3 text-xs space-y-0.5">
              <div class="text-slate-700">{{ emp.email }}</div>
              <div v-if="emp.phone" class="text-slate-400 font-mono">{{ emp.phone }}</div>
            </td>
            <td class="px-4 py-3">
              <AppBadge :status="emp.status" />
            </td>
            <td v-if="auth.isManager || auth.isHR || auth.isAdmin" class="px-4 py-3 text-right">
              <AppButton size="sm" variant="secondary" @click="viewDetail(emp.id)">
                Chi tiết
              </AppButton>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <template #footer>
      <AppButton variant="secondary" @click="emit('close')">Đóng</AppButton>
    </template>
  </AppModal>
</template>
