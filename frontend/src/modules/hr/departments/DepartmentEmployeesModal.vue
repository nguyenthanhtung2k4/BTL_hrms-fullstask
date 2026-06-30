<script setup lang="ts">
import { useRouter } from 'vue-router'
import { employeeService } from '../../../services/employee.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { Employee } from '../../../types/hr.types'
import AppModal from '../../../components/ui/AppModal.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import { ref, computed, onMounted } from 'vue'

const search = ref('')

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

const sortedEmployees = computed(() => {
  const positionOrder = (positionName: string): number => {
    const name = positionName?.toLowerCase() ?? ''
    if (name.includes('admin')) return 0
    if (name.includes('manager')) return 1
    if (name.includes('hr')) return 2
    if (name.includes('payroll')) return 3
    return 4
  }

  const filtered = search.value
    ? employees.value.filter(e =>
      e.fullName.toLowerCase().includes(search.value.toLowerCase()) ||
      e.employeeCode.toLowerCase().includes(search.value.toLowerCase()) ||
      e.positionName?.toLowerCase().includes(search.value.toLowerCase())
    )
    : employees.value

  return [...filtered].sort((a, b) =>
    positionOrder(a.positionName) - positionOrder(b.positionName)
  )
})

onMounted(load)
</script>

<template>
  <AppModal :title="`Thành viên phòng ban: ${departmentName}`" size="lg" @close="emit('close')">
    <!-- Search -->
    <div class="mb-4 relative">
      <input v-model="search" type="text" placeholder="Tìm theo tên, mã NV, chức vụ..."
        class="h-9 w-full rounded-lg border border-slate-300 bg-white px-3 pl-9 text-sm outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100" />
      <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none text-slate-400">
        <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
      </div>
    </div>
    <div v-if="loading" class="py-12 flex justify-center items-center">
      <div class="h-8 w-8 animate-spin rounded-full border-4 border-emerald-500 border-t-transparent" />
    </div>

    <div v-else-if="employees.length === 0" class="py-12 text-center text-slate-400 text-sm">
      Phòng ban này hiện chưa có nhân viên nào.
    </div>

    <div v-else class="overflow-x-auto">
      <table class="w-full text-left border-collapse text-sm">
        <thead>
          <tr
            class="border-b border-slate-200 bg-slate-50 text-[11px] font-semibold text-slate-500 uppercase tracking-wider">
            <th class="px-4 py-3">Mã NV</th>
            <th class="px-4 py-3">Họ và tên</th>
            <th class="px-4 py-3">Chức vụ</th>
            <th class="px-4 py-3">Thông tin liên hệ</th>
            <th class="px-4 py-3">Trạng thái</th>
            <th v-if="auth.isManager || auth.isHR || auth.isAdmin" class="px-4 py-3 text-right">Thao tác</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-slate-100">
          <tr v-for="emp in sortedEmployees" :key="emp.id" class="hover:bg-slate-50/50 transition-colors">
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
