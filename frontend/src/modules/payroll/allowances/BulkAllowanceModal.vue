<script setup lang="ts">
/**
 * BulkAllowanceModal
 * Áp dụng cùng 1 loại phụ cấp + số tiền cho nhiều nhân viên cùng lúc.
 * Luồng: Chọn kỳ → chọn loại + số tiền → tìm & chọn nhân viên → preview → tạo hàng loạt
 */
import { ref, computed } from 'vue'
import AppModal from '../../../components/ui/AppModal.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import type { AllowanceType, CreateAllowanceDto, PayrollPeriod } from '../../../types/payroll.types'
import type { Employee } from '../../../types/hr.types'

// ── Props / Emits ─────────────────────────────────────────
const props = defineProps<{
  periods: PayrollPeriod[]
  employees: Employee[]
  types: AllowanceType[]
}>()

const emit = defineEmits<{
  close: []
  saved: [count: number]
  confirm: []
}>()

// ── Step management ───────────────────────────────────────
// step: 'config' → 'select' → 'preview' → 'progress' → 'done'
type Step = 'config' | 'select' | 'preview' | 'progress' | 'done'
const step = ref<Step>('config')

// ── Step 1 — Config ───────────────────────────────────────
const periodId       = ref('')
const allowanceTypeId = ref('')
const newTypeName    = ref('')
const amount         = ref('')
const notes          = ref('')
const configErrors   = ref<Record<string, string>>({})

const activeTypes = computed(() => props.types.filter(t => t.isActive !== false))

function validateConfig() {
  configErrors.value = {}
  if (!periodId.value)        configErrors.value.period  = 'Chọn kỳ lương'
  if (!allowanceTypeId.value) configErrors.value.type    = 'Chọn loại phụ cấp'
  if (allowanceTypeId.value === 'NEW_TYPE' && !newTypeName.value.trim())
                               configErrors.value.newType = 'Nhập tên loại mới'
  if (!amount.value || isNaN(Number(amount.value)) || Number(amount.value) <= 0)
                               configErrors.value.amount  = 'Nhập số tiền hợp lệ'
  return Object.keys(configErrors.value).length === 0
}

function goToSelect() {
  if (validateConfig()) step.value = 'select'
}

// ── Step 2 — Employee picker ──────────────────────────────
const empSearch    = ref('')
const deptFilter   = ref('')
const selectedIds  = ref<Set<string>>(new Set())

/** Unique departments from active employees */
const departments = computed(() => {
  const map = new Map<string, string>()
  props.employees
    .filter(e => e.status === 'Active')
    .forEach(e => { if (e.departmentId) map.set(e.departmentId, e.departmentName) })
  return [...map.entries()].map(([id, name]) => ({ id, name }))
})

const activeEmployees = computed(() =>
  props.employees.filter(e => e.status === 'Active')
)

const filteredEmployees = computed(() => {
  let list = activeEmployees.value
  if (deptFilter.value) list = list.filter(e => e.departmentId === deptFilter.value)
  if (empSearch.value.trim()) {
    const q = empSearch.value.trim().toLowerCase()
    list = list.filter(e =>
      e.fullName.toLowerCase().includes(q) ||
      e.employeeCode.toLowerCase().includes(q)
    )
  }
  return list
})

const isAllFiltered = computed(() =>
  filteredEmployees.value.length > 0 &&
  filteredEmployees.value.every(e => selectedIds.value.has(e.id))
)
const isIndeterminate = computed(() => {
  const c = filteredEmployees.value.filter(e => selectedIds.value.has(e.id)).length
  return c > 0 && c < filteredEmployees.value.length
})

function toggleAll() {
  if (isAllFiltered.value) filteredEmployees.value.forEach(e => selectedIds.value.delete(e.id))
  else                     filteredEmployees.value.forEach(e => selectedIds.value.add(e.id))
}
function toggleEmp(id: string) {
  if (selectedIds.value.has(id)) selectedIds.value.delete(id)
  else                           selectedIds.value.add(id)
}

// Sorted selected employees for preview
const selectedEmployees = computed(() =>
  activeEmployees.value.filter(e => selectedIds.value.has(e.id))
)

function goToPreview() {
  if (selectedIds.value.size === 0) return
  step.value = 'preview'
}

// ── Step 3 — Progress / Done ──────────────────────────────
const jobDone   = ref(0)
const jobFailed = ref<string[]>([])
const jobProgress = computed(() =>
  selectedEmployees.value.length === 0 ? 0
    : Math.round(((jobDone.value + jobFailed.value.length) / selectedEmployees.value.length) * 100)
)

async function startApply(createFn: (dto: CreateAllowanceDto) => Promise<any>) {
  step.value = 'progress'
  jobDone.value = 0
  jobFailed.value = []

  for (const emp of selectedEmployees.value) {
    const dto: CreateAllowanceDto = {
      payrollPeriodId: periodId.value,
      employeeId: emp.id,
      allowanceTypeId: allowanceTypeId.value === 'NEW_TYPE'
        ? '__NEW__' // parent resolves actual id
        : allowanceTypeId.value,
      amount: Number(amount.value),
      notes: notes.value || undefined,
    }
    try {
      await createFn(dto)
      jobDone.value++
    } catch {
      jobFailed.value.push(emp.fullName)
    }
  }

  step.value = 'done'
  if (jobDone.value > 0) emit('saved', jobDone.value)
}

// Helper
function fmtMoney(n: number | string) {
  return Number(n).toLocaleString('vi-VN') + ' ₫'
}

function periodName(id: string) {
  return props.periods.find(p => p.id === id)?.name ?? id
}
function typeName(id: string) {
  if (id === 'NEW_TYPE') return newTypeName.value || '(mới)'
  return props.types.find(t => t.id === id)?.name ?? id
}

defineExpose({ startApply })
</script>

<template>
  <AppModal title="Áp dụng phụ cấp hàng loạt" size="lg" @close="$emit('close')">

    <!-- Stepper header -->
      <div class="mb-6 flex items-center gap-0">
        <template v-for="(s, i) in [
          { key: 'config',  label: 'Cấu hình' },
          { key: 'select',  label: 'Chọn NV'  },
          { key: 'preview', label: 'Xem trước' },
        ]" :key="s.key">
          <!-- Step bubble -->
          <div class="flex flex-col items-center gap-1">
            <div :class="[
              'h-8 w-8 rounded-full flex items-center justify-center text-sm font-bold transition-all duration-300',
              step === s.key
                ? 'bg-emerald-500 text-white shadow-md shadow-emerald-200'
                : (i < ['config','select','preview','progress','done'].indexOf(step))
                  ? 'bg-emerald-100 text-emerald-600'
                  : 'bg-slate-100 text-slate-400',
            ]">
              <svg v-if="i < ['config','select','preview','progress','done'].indexOf(step)"
                class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 13l4 4L19 7" />
              </svg>
              <span v-else>{{ i + 1 }}</span>
            </div>
            <span :class="[
              'text-xs font-medium',
              step === s.key ? 'text-emerald-600' : 'text-slate-400'
            ]">{{ s.label }}</span>
          </div>
          <!-- Connector line -->
          <div v-if="i < 2" :class="[
            'flex-1 h-0.5 mb-5 mx-1 rounded transition-all duration-500',
            i < ['config','select','preview','progress','done'].indexOf(step)
              ? 'bg-emerald-400' : 'bg-slate-200'
          ]" />
        </template>
      </div>

      <!-- ════════════════════════════════
           STEP 1 — Config
      ════════════════════════════════ -->
      <div v-if="step === 'config'" class="space-y-4 animate-fadein">
        <div class="rounded-xl bg-blue-50 border border-blue-200 p-3.5 flex gap-3 items-start">
          <svg class="h-5 w-5 text-blue-500 mt-0.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
          <p class="text-sm text-blue-700">
            Thiết lập <strong>kỳ lương, loại phụ cấp và số tiền</strong> — sau đó chọn danh sách nhân viên được áp dụng.
          </p>
        </div>

        <!-- Kỳ lương -->
        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium text-slate-700">Kỳ lương <span class="text-red-500">*</span></label>
          <select v-model="periodId"
            :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none transition',
              configErrors.period ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100']">
            <option value="">-- Chọn kỳ lương --</option>
            <option v-for="p in periods.filter(p => p.status !== 'Closed')" :key="p.id" :value="p.id">
              {{ p.name }}
            </option>
          </select>
          <p v-if="configErrors.period" class="text-xs text-red-500">{{ configErrors.period }}</p>
        </div>

        <!-- Loại phụ cấp + Số tiền side by side -->
        <div class="grid grid-cols-2 gap-3">
          <div class="flex flex-col gap-1.5">
            <label class="text-sm font-medium text-slate-700">Loại phụ cấp <span class="text-red-500">*</span></label>
            <select v-model="allowanceTypeId"
              :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none transition',
                configErrors.type ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100']">
              <option value="">-- Chọn loại --</option>
              <option v-for="t in activeTypes" :key="t.id" :value="t.id">{{ t.name }}</option>
              <option value="NEW_TYPE" class="text-emerald-600 font-medium">+ Thêm loại mới…</option>
            </select>
            <p v-if="configErrors.type" class="text-xs text-red-500">{{ configErrors.type }}</p>
          </div>

          <div class="flex flex-col gap-1.5">
            <label class="text-sm font-medium text-slate-700">Số tiền (₫) <span class="text-red-500">*</span></label>
            <input v-model="amount" type="number" min="0" placeholder="VD: 500000"
              :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none transition',
                configErrors.amount ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100']" />
            <p v-if="configErrors.amount" class="text-xs text-red-500">{{ configErrors.amount }}</p>
          </div>
        </div>

        <!-- New type name input -->
        <div v-if="allowanceTypeId === 'NEW_TYPE'"
          class="rounded-xl border border-slate-100 bg-slate-50 p-3 flex flex-col gap-1.5">
          <label class="text-xs font-semibold text-slate-600 uppercase tracking-wider">
            Tên loại phụ cấp mới <span class="text-red-500">*</span>
          </label>
          <input v-model="newTypeName" type="text" placeholder="VD: Phụ cấp dự án, Phụ cấp thâm niên…"
            :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none transition',
              configErrors.newType ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']" />
          <p v-if="configErrors.newType" class="text-xs text-red-500">{{ configErrors.newType }}</p>
        </div>

        <!-- Ghi chú -->
        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium text-slate-700">Ghi chú <span class="text-xs text-slate-400">(tuỳ chọn)</span></label>
          <input v-model="notes" type="text" placeholder="VD: Thưởng quý 2, Phụ cấp dự án ABC…"
            class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 transition" />
        </div>
      </div>

      <!-- ════════════════════════════════
           STEP 2 — Employee picker
      ════════════════════════════════ -->
      <div v-else-if="step === 'select'" class="space-y-3 animate-fadein">
        <!-- Config summary chip -->
        <div class="flex flex-wrap gap-2 text-xs">
          <span class="inline-flex items-center gap-1.5 bg-slate-100 text-slate-600 px-2.5 py-1 rounded-full">
            <svg class="h-3.5 w-3.5 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
            {{ periodName(periodId) }}
          </span>
          <span class="inline-flex items-center gap-1.5 bg-emerald-50 text-emerald-700 border border-emerald-200 px-2.5 py-1 rounded-full font-medium">
            {{ typeName(allowanceTypeId) }} · {{ fmtMoney(amount) }}
          </span>
          <button class="text-slate-400 hover:text-slate-600 underline ml-auto" @click="step = 'config'">Sửa</button>
        </div>

        <!-- Search + Dept filter -->
        <div class="flex gap-2">
          <div class="relative flex-1">
            <svg class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none"
              fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M21 21l-4.35-4.35M17 11A6 6 0 1 1 5 11a6 6 0 0 1 12 0z" />
            </svg>
            <input v-model="empSearch" type="text" placeholder="Tìm tên hoặc mã nhân viên…"
              class="w-full h-9 pl-9 pr-3 rounded-lg border border-slate-300 bg-white text-sm outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 transition" />
          </div>
          <select v-model="deptFilter"
            class="h-9 rounded-lg border border-slate-300 bg-white px-3 text-sm outline-none focus:border-emerald-500 transition">
            <option value="">Tất cả phòng ban</option>
            <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
          </select>
        </div>

        <!-- Select all bar -->
        <div class="flex items-center gap-3 px-3 py-2 rounded-lg bg-slate-50 border border-slate-200">
          <input type="checkbox" :checked="isAllFiltered" :indeterminate="isIndeterminate"
            class="h-4 w-4 rounded border-slate-300 text-emerald-600 cursor-pointer"
            @change="toggleAll" />
          <span class="text-sm text-slate-600">
            Chọn tất cả
            <span class="text-slate-400">({{ filteredEmployees.length }} nhân viên)</span>
          </span>
          <span v-if="selectedIds.size > 0"
            class="ml-auto text-xs font-semibold text-emerald-600 bg-emerald-50 border border-emerald-200 px-2 py-0.5 rounded-full">
            ✓ {{ selectedIds.size }} đã chọn
          </span>
        </div>

        <!-- Employee list -->
        <div class="max-h-64 overflow-y-auto rounded-xl border border-slate-200 divide-y divide-slate-100">
          <div v-if="filteredEmployees.length === 0"
            class="flex flex-col items-center gap-2 py-10 text-slate-400 text-sm">
            <svg class="h-8 w-8 opacity-40" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            Không tìm thấy nhân viên
          </div>
          <label v-for="emp in filteredEmployees" :key="emp.id"
            :class="[
              'flex items-center gap-3 px-4 py-2.5 cursor-pointer transition',
              selectedIds.has(emp.id) ? 'bg-emerald-50/60' : 'bg-white hover:bg-slate-50'
            ]">
            <input type="checkbox" :checked="selectedIds.has(emp.id)"
              class="h-4 w-4 rounded border-slate-300 text-emerald-600 cursor-pointer shrink-0"
              @change="toggleEmp(emp.id)" />
            <!-- Avatar -->
            <div class="h-7 w-7 rounded-full bg-gradient-to-br from-emerald-400 to-teal-500 flex items-center justify-center text-white text-xs font-bold shrink-0">
              {{ emp.fullName.charAt(0).toUpperCase() }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-slate-800 truncate">{{ emp.fullName }}</p>
              <p class="text-xs text-slate-400 truncate">{{ emp.employeeCode }} · {{ emp.departmentName }}</p>
            </div>
            <span v-if="selectedIds.has(emp.id)" class="shrink-0 text-emerald-500">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 13l4 4L19 7" />
              </svg>
            </span>
          </label>
        </div>
      </div>

      <!-- ════════════════════════════════
           STEP 3 — Preview
      ════════════════════════════════ -->
      <div v-else-if="step === 'preview'" class="space-y-4 animate-fadein">
        <!-- Summary cards -->
        <div class="grid grid-cols-3 gap-3">
          <div class="rounded-xl bg-slate-50 border border-slate-200 p-3 text-center">
            <p class="text-2xl font-bold text-slate-800">{{ selectedEmployees.length }}</p>
            <p class="text-xs text-slate-500 mt-0.5">Nhân viên</p>
          </div>
          <div class="rounded-xl bg-emerald-50 border border-emerald-200 p-3 text-center">
            <p class="text-lg font-bold text-emerald-700 truncate">{{ fmtMoney(amount) }}</p>
            <p class="text-xs text-emerald-600 mt-0.5">Mỗi người</p>
          </div>
          <div class="rounded-xl bg-blue-50 border border-blue-200 p-3 text-center">
            <p class="text-lg font-bold text-blue-700 truncate">
              {{ fmtMoney(Number(amount) * selectedEmployees.length) }}
            </p>
            <p class="text-xs text-blue-600 mt-0.5">Tổng cộng</p>
          </div>
        </div>

        <!-- Detail: loại + kỳ -->
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-3 flex flex-wrap gap-x-6 gap-y-2 text-sm">
          <div>
            <span class="text-slate-400 text-xs">Kỳ lương:</span>
            <span class="ml-1.5 font-medium text-slate-700">{{ periodName(periodId) }}</span>
          </div>
          <div>
            <span class="text-slate-400 text-xs">Loại phụ cấp:</span>
            <span class="ml-1.5 font-medium text-slate-700">{{ typeName(allowanceTypeId) }}</span>
          </div>
          <div v-if="notes">
            <span class="text-slate-400 text-xs">Ghi chú:</span>
            <span class="ml-1.5 font-medium text-slate-700">{{ notes }}</span>
          </div>
        </div>

        <!-- Employee preview list -->
        <div class="max-h-52 overflow-y-auto rounded-xl border border-slate-200 divide-y divide-slate-100">
          <div v-for="emp in selectedEmployees" :key="emp.id"
            class="flex items-center gap-3 px-4 py-2 bg-white">
            <div class="h-6 w-6 rounded-full bg-gradient-to-br from-emerald-400 to-teal-500 flex items-center justify-center text-white text-xs font-bold shrink-0">
              {{ emp.fullName.charAt(0).toUpperCase() }}
            </div>
            <div class="flex-1 min-w-0">
              <span class="text-sm font-medium text-slate-700 truncate">{{ emp.fullName }}</span>
              <span class="ml-2 text-xs text-slate-400">{{ emp.departmentName }}</span>
            </div>
            <span class="text-sm font-semibold text-emerald-600 shrink-0">{{ fmtMoney(amount) }}</span>
          </div>
        </div>
      </div>

      <!-- ════════════════════════════════
           STEP 4 — Progress
      ════════════════════════════════ -->
      <div v-else-if="step === 'progress'" class="py-6 space-y-5 animate-fadein">
        <div class="flex flex-col items-center gap-4">
          <!-- Circular progress -->
          <div class="relative h-20 w-20">
            <svg class="h-20 w-20 -rotate-90" viewBox="0 0 80 80">
              <circle cx="40" cy="40" r="34" fill="none" stroke="#e2e8f0" stroke-width="7" />
              <circle cx="40" cy="40" r="34" fill="none" stroke="#10b981" stroke-width="7"
                stroke-linecap="round"
                :stroke-dasharray="`${jobProgress * 2.138} 213.8`"
                style="transition: stroke-dasharray 0.4s ease" />
            </svg>
            <span class="absolute inset-0 flex items-center justify-center text-base font-bold text-emerald-600">
              {{ jobProgress }}%
            </span>
          </div>
          <div class="text-center">
            <p class="text-sm font-semibold text-slate-700">Đang áp dụng phụ cấp…</p>
            <p class="text-xs text-slate-400 mt-0.5">
              {{ jobDone + jobFailed.length }}/{{ selectedEmployees.length }} nhân viên
            </p>
          </div>
        </div>
        <!-- Linear bar -->
        <div class="h-2 rounded-full bg-slate-100 overflow-hidden">
          <div class="h-full bg-emerald-500 rounded-full transition-all duration-400"
            :style="{ width: jobProgress + '%' }" />
        </div>
      </div>

      <!-- ════════════════════════════════
           STEP 5 — Done
      ════════════════════════════════ -->
      <div v-else-if="step === 'done'" class="py-6 space-y-4 animate-fadein">
        <div class="flex flex-col items-center gap-3 text-center">
          <div :class="[
            'h-16 w-16 rounded-full flex items-center justify-center',
            jobFailed.length === 0 ? 'bg-emerald-100' : 'bg-amber-100'
          ]">
            <svg v-if="jobFailed.length === 0" class="h-8 w-8 text-emerald-600"
              fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
            </svg>
            <svg v-else class="h-8 w-8 text-amber-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M12 9v2m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" />
            </svg>
          </div>
          <div>
            <p class="font-semibold text-slate-800 text-base">
              {{ jobFailed.length === 0 ? 'Hoàn tất!' : 'Hoàn tất với một số lỗi' }}
            </p>
            <p class="text-sm text-slate-500 mt-1">
              Đã áp dụng thành công
              <strong class="text-emerald-600">{{ jobDone }}</strong> phụ cấp.
              <span v-if="jobFailed.length" class="text-red-500">
                {{ jobFailed.length }} thất bại.
              </span>
            </p>
          </div>
        </div>

        <!-- Failed list -->
        <div v-if="jobFailed.length" class="rounded-xl border border-red-200 bg-red-50 p-3 space-y-1">
          <p class="text-xs font-semibold text-red-600 mb-1.5">Nhân viên không áp dụng được:</p>
          <p v-for="name in jobFailed" :key="name" class="text-xs text-red-500">• {{ name }}</p>
        </div>
      </div>

    <!-- ══════════════════════════════════════════════════
         FOOTER ACTIONS
    ═══════════════════════════════════════════════════ -->
    <template #footer>
      <!-- Step: config -->
      <template v-if="step === 'config'">
        <AppButton variant="secondary" @click="$emit('close')">Hủy</AppButton>
        <AppButton @click="goToSelect">
          Tiếp theo
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
          </svg>
        </AppButton>
      </template>

      <!-- Step: select -->
      <template v-else-if="step === 'select'">
        <AppButton variant="secondary" @click="step = 'config'">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
          Quay lại
        </AppButton>
        <AppButton :disabled="selectedIds.size === 0" @click="goToPreview">
          Xem trước ({{ selectedIds.size }} NV)
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
          </svg>
        </AppButton>
      </template>

      <!-- Step: preview -->
      <template v-else-if="step === 'preview'">
        <AppButton variant="secondary" @click="step = 'select'">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
          Quay lại
        </AppButton>
        <AppButton @click="$emit('confirm')">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          Áp dụng cho {{ selectedEmployees.length }} nhân viên
        </AppButton>
      </template>

      <!-- Step: progress -->
      <template v-else-if="step === 'progress'">
        <AppButton variant="secondary" disabled>Đang xử lý…</AppButton>
      </template>

      <!-- Step: done -->
      <template v-else>
        <AppButton @click="$emit('close')">Đóng</AppButton>
      </template>
    </template>
  </AppModal>
</template>

<style scoped>
@keyframes fadein {
  from { opacity: 0; transform: translateY(6px); }
  to   { opacity: 1; transform: translateY(0); }
}
.animate-fadein { animation: fadein 0.2s ease-out; }
</style>
