<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { employeeService } from '../../../services/employee.service'
import { contractService } from '../../../services/contract.service'
import { useToastStore } from '../../../stores/toast'
import type { Employee, Contract } from '../../../types/hr.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import { Building2, Briefcase, User, Calendar, Phone, Users } from '@lucide/vue'

const route = useRoute()
const router = useRouter()
const toast = useToastStore()

const employee = ref<Employee | null>(null)
const contracts = ref<Contract[]>([])
const loading = ref(true)
const activeTab = ref<'info' | 'contracts'>('info')

async function load() {
  try {
    const id = route.params.id as string
    employee.value = await employeeService.getById(id)
    const all = await contractService.getAll()
    contracts.value = all.filter((c) => c.employeeId === id)
  } catch { toast.error('Không tìm thấy nhân viên'); router.push('/hr/employees') }
  finally { loading.value = false }
}

function fmt(d?: string) { return d ? new Date(d).toLocaleDateString('vi-VN') : '—' }
function fmtMoney(n: number) { return n.toLocaleString('vi-VN') + ' ₫' }

function setActiveTab(key: string) { activeTab.value = key as any }

onMounted(load)
</script>

<template>
  <div>
    <PageHeader :title="employee?.fullName ?? '...'" :breadcrumbs="[{ label: 'Nhân sự' }, { label: 'Nhân viên', to: '/hr/employees' }, { label: employee?.fullName ?? '' }]" />

    <div v-if="loading" class="space-y-3">
      <div v-for="n in 4" :key="n" class="h-12 animate-pulse rounded-lg bg-slate-200" />
    </div>

    <template v-else-if="employee">
      <!-- Tabs -->
      <div class="mb-6 border-b border-slate-200">
        <nav class="flex gap-4">
          <button
            v-for="tab in [{ key: 'info', label: 'Thông tin cơ bản' }, { key: 'contracts', label: `Hợp đồng (${contracts.length})` }]"
            :key="tab.key"
            :class="['pb-3 text-sm font-medium border-b-2 transition-colors', activeTab === tab.key ? 'border-emerald-600 text-emerald-700' : 'border-transparent text-slate-500 hover:text-slate-900']"
            @click="setActiveTab(tab.key)"
          >
            {{ tab.label }}
          </button>
        </nav>
      </div>

      <!-- Tab: Info (redesigned) -->
      <div v-if="activeTab === 'info'" class="rounded-xl border border-slate-200 bg-white p-6">
        <div class="flex flex-col lg:flex-row lg:items-start gap-6">
          <!-- Avatar -->
          <div class="flex-shrink-0">
            <div class="h-28 w-28 overflow-hidden rounded-xl bg-slate-100 flex items-center justify-center text-3xl font-bold text-slate-700">
              {{ employee.fullName[0] }}
            </div>
          </div>

          <!-- Main info -->
          <div class="flex-1">
            <div class="flex items-start justify-between">
              <div>
                <div class="text-2xl font-bold text-slate-900">{{ employee.fullName }}</div>
                <div class="mt-1 text-sm text-slate-500">{{ employee.employeeCode }} · {{ employee.email }}</div>
                <div class="mt-3"><AppBadge :status="employee.status" /></div>
              </div>
            </div>

            <!-- Info grid -->
            <div class="mt-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 text-sm">
              <div class="flex items-center gap-3 rounded-lg border border-slate-100 p-3">
                <div class="grid h-10 w-10 place-items-center rounded-lg bg-blue-50 text-blue-600">
                  <Building2 :size="18" />
                </div>
                <div>
                  <div class="text-[12px] text-slate-500">Phòng ban</div>
                  <div class="font-medium text-slate-900">{{ employee.departmentName }}</div>
                </div>
              </div>

              <div class="flex items-center gap-3 rounded-lg border border-slate-100 p-3">
                <div class="grid h-10 w-10 place-items-center rounded-lg bg-blue-50 text-blue-600">
                  <Briefcase :size="18" />
                </div>
                <div>
                  <div class="text-[12px] text-slate-500">Chức vụ</div>
                  <div class="font-medium text-slate-900">{{ employee.positionName }}</div>
                </div>
              </div>

              <div class="flex items-center gap-3 rounded-lg border border-slate-100 p-3">
                <div class="grid h-10 w-10 place-items-center rounded-lg bg-blue-50 text-blue-600">
                  <User :size="18" />
                </div>
                <div>
                  <div class="text-[12px] text-slate-500">Giới tính</div>
                  <div class="font-medium text-slate-900">{{ employee.gender ?? '—' }}</div>
                </div>
              </div>

              <div class="flex items-center gap-3 rounded-lg border border-slate-100 p-3">
                <div class="grid h-10 w-10 place-items-center rounded-lg bg-blue-50 text-blue-600">
                  <Calendar :size="18" />
                </div>
                <div>
                  <div class="text-[12px] text-slate-500">Ngày sinh</div>
                  <div class="font-medium text-slate-900">{{ fmt(employee.dateOfBirth) }}</div>
                </div>
              </div>

              <div class="flex items-center gap-3 rounded-lg border border-slate-100 p-3">
                <div class="grid h-10 w-10 place-items-center rounded-lg bg-blue-50 text-blue-600">
                  <Calendar :size="18" />
                </div>
                <div>
                  <div class="text-[12px] text-slate-500">Ngày vào làm</div>
                  <div class="font-medium text-slate-900">{{ fmt(employee.hireDate) }}</div>
                </div>
              </div>

              <div class="flex items-center gap-3 rounded-lg border border-slate-100 p-3">
                <div class="grid h-10 w-10 place-items-center rounded-lg bg-blue-50 text-blue-600">
                  <Users :size="18" />
                </div>
                <div>
                  <div class="text-[12px] text-slate-500">Quản lý</div>
                  <div class="font-medium text-slate-900">{{ employee.managerName ?? '—' }}</div>
                </div>
              </div>

              <div class="flex items-center gap-3 rounded-lg border border-slate-100 p-3">
                <div class="grid h-10 w-10 place-items-center rounded-lg bg-blue-50 text-blue-600">
                  <Phone :size="18" />
                </div>
                <div>
                  <div class="text-[12px] text-slate-500">Điện thoại</div>
                  <div class="font-medium text-slate-900">{{ employee.phone ?? '—' }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Tab: Contracts -->
      <div v-else-if="activeTab === 'contracts'">
        <div v-if="contracts.length === 0" class="rounded-xl border border-slate-200 bg-white py-12 text-center text-slate-400">Chưa có hợp đồng nào</div>
        <div v-else class="space-y-3">
          <div v-for="c in contracts" :key="c.id" class="rounded-xl border border-slate-200 bg-white p-4">
            <div class="flex items-center justify-between">
              <div>
                <div class="font-semibold text-slate-900">{{ c.contractNumber }}</div>
                <div class="text-xs text-slate-500">{{ c.contractType }} · {{ fmt(c.startDate) }} → {{ c.endDate ? fmt(c.endDate) : 'Không thời hạn' }}</div>
              </div>
              <div class="text-right">
                <div class="text-lg font-bold text-emerald-700">{{ fmtMoney(c.baseSalary) }}</div>
                <AppBadge :status="c.status" />
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>
