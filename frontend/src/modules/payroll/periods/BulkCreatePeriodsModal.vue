<script setup lang="ts">
/**
 * BulkCreatePeriodsModal
 * Cho phép tạo nhiều kỳ lương liên tiếp (3 / 6 / 12 tháng) chỉ với vài cú click.
 * Luồng: Chọn tháng bắt đầu → chọn số tháng → preview danh sách → xác nhận tạo.
 */
import { ref, computed, watch } from 'vue'
import AppModal from '../../../components/ui/AppModal.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import type { PayrollRule, CreatePayrollPeriodDto } from '../../../types/payroll.types'

// ── Props / Emits ────────────────────────────────────────
const props = defineProps<{
  rules: PayrollRule[]
  existingCodes: string[] // để phát hiện trùng
}>()

const emit = defineEmits<{
  close: []
  created: [count: number]
  confirm: []
}>()

// ── Form state ────────────────────────────────────────────
const startMonth = ref('')       // "YYYY-MM"
const monthCount = ref(3)        // 3 | 6 | 12
const payrollRuleId = ref(props.rules.find(r => r.isActive)?.id ?? '')
const nameTemplate = ref('Lương tháng {M}/{Y}')  // template với {M} và {Y}

// ── Preview generation ────────────────────────────────────
interface PeriodPreview extends CreatePayrollPeriodDto {
  isDuplicate: boolean
  selected: boolean
}

const previews = computed<PeriodPreview[]>(() => {
  if (!startMonth.value || !payrollRuleId.value) return []

  const [baseYear, baseMonth] = startMonth.value.split('-').map(Number)
  const result: PeriodPreview[] = []

  for (let i = 0; i < monthCount.value; i++) {
    const date = new Date(baseYear, baseMonth - 1 + i, 1)
    const y = date.getFullYear()
    const m = date.getMonth() + 1
    const mm = String(m).padStart(2, '0')

    const code = `KY-${mm}-${y}`
    const name = nameTemplate.value
      .replace('{M}', String(m))
      .replace('{MM}', mm)
      .replace('{Y}', String(y))

    const fromDate = new Date(y, m - 1, 1).toISOString().split('T')[0]
    const toDate = new Date(y, m, 0).toISOString().split('T')[0]

    result.push({
      code,
      name,
      fromDate,
      toDate,
      payrollRuleId: payrollRuleId.value,
      isDuplicate: props.existingCodes.includes(code),
      selected: !props.existingCodes.includes(code), // mặc định bỏ chọn nếu trùng
    })
  }
  return result
})

// Cho phép toggle từng hàng preview
const previewList = ref<PeriodPreview[]>([])
watch(previews, (val) => { previewList.value = val.map(p => ({ ...p })) }, { immediate: true })

const selectedPreviews = computed(() => previewList.value.filter(p => p.selected && !p.isDuplicate))
const allSelected = computed(() => previewList.value.filter(p => !p.isDuplicate).every(p => p.selected))
const someSelected = computed(() => previewList.value.some(p => p.selected))

function toggleAll() {
  const next = !allSelected.value
  previewList.value.forEach(p => { if (!p.isDuplicate) p.selected = next })
}

// ── Validation ────────────────────────────────────────────
const formError = computed(() => {
  if (!startMonth.value) return 'Vui lòng chọn tháng bắt đầu'
  if (!payrollRuleId.value) return 'Vui lòng chọn quy tắc lương'
  if (!nameTemplate.value.trim()) return 'Vui lòng nhập mẫu tên'
  return ''
})

// ── Progress tracking ─────────────────────────────────────
type JobStatus = 'idle' | 'running' | 'done'
const jobStatus = ref<JobStatus>('idle')
const jobProgress = ref(0)   // 0-100
const jobDone = ref(0)
const jobFailed = ref<string[]>([])

// emitted by parent

async function startCreate(createFn: (dto: CreatePayrollPeriodDto) => Promise<any>) {
  const list = selectedPreviews.value
  if (!list.length) return

  jobStatus.value = 'running'
  jobProgress.value = 0
  jobDone.value = 0
  jobFailed.value = []

  for (let i = 0; i < list.length; i++) {
    const { isDuplicate, selected, ...dto } = list[i] as any
    try {
      await createFn(dto)
      jobDone.value++
    } catch {
      jobFailed.value.push(list[i].name)
    }
    jobProgress.value = Math.round(((i + 1) / list.length) * 100)
  }

  jobStatus.value = 'done'
  if (jobDone.value > 0) emit('created', jobDone.value)
}

function fmt(d: string) {
  return new Date(d).toLocaleDateString('vi-VN')
}

defineExpose({ startCreate })
</script>

<template>
  <AppModal title="Tạo kỳ lương hàng loạt" size="lg" @close="$emit('close')">

    <!-- ═══════════════════════════════════════════════════
         STEP 1 — Config form
    ════════════════════════════════════════════════════ -->
    <div v-if="jobStatus === 'idle'" class="space-y-5">

      <!-- Header hint -->
      <div class="flex items-start gap-3 rounded-xl bg-emerald-50 border border-emerald-200 p-3.5">
        <svg class="h-5 w-5 text-emerald-500 mt-0.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <p class="text-sm text-emerald-700">
          Chọn tháng bắt đầu và số tháng cần tạo. Hệ thống sẽ tự sinh mã, tên và ngày cho từng kỳ lương.
        </p>
      </div>

      <!-- Row 1: start month + count -->
      <div class="grid grid-cols-2 gap-4">
        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium text-slate-700">Tháng bắt đầu <span class="text-red-500">*</span></label>
          <input
            v-model="startMonth"
            type="month"
            class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 transition"
          />
        </div>

        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium text-slate-700">Số tháng tạo</label>
          <div class="flex gap-2">
            <button
              v-for="n in [3, 6, 12]" :key="n"
              :class="[
                'flex-1 h-9 rounded-lg border text-sm font-medium transition',
                monthCount === n
                  ? 'bg-emerald-500 border-emerald-500 text-white shadow-sm'
                  : 'bg-white border-slate-300 text-slate-600 hover:border-emerald-400 hover:text-emerald-600'
              ]"
              @click="monthCount = n"
            >{{ n }} tháng</button>
          </div>
        </div>
      </div>

      <!-- Row 2: rule + name template -->
      <div class="grid grid-cols-2 gap-4">
        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium text-slate-700">Quy tắc lương <span class="text-red-500">*</span></label>
          <select
            v-model="payrollRuleId"
            class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 transition"
          >
            <option value="">-- Chọn quy tắc --</option>
            <option v-for="r in rules.filter(r => r.isActive)" :key="r.id" :value="r.id">{{ r.name }}</option>
          </select>
        </div>

        <div class="flex flex-col gap-1.5">
          <label class="text-sm font-medium text-slate-700">
            Mẫu tên kỳ
            <span class="ml-1 text-xs text-slate-400 font-normal">{M} = tháng, {Y} = năm</span>
          </label>
          <input
            v-model="nameTemplate"
            type="text"
            placeholder="VD: Lương tháng {M}/{Y}"
            class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 transition"
          />
        </div>
      </div>

      <!-- Error hint -->
      <p v-if="formError && startMonth" class="text-sm text-red-500">{{ formError }}</p>

      <!-- ── Preview table ── -->
      <div v-if="previewList.length" class="rounded-xl border border-slate-200 overflow-hidden">
        <!-- Table header -->
        <div class="flex items-center gap-3 px-4 py-2.5 bg-slate-50 border-b border-slate-200">
          <input
            type="checkbox"
            :checked="allSelected"
            class="h-4 w-4 rounded border-slate-300 text-emerald-600 cursor-pointer"
            @change="toggleAll"
          />
          <span class="text-xs font-semibold text-slate-500 uppercase tracking-wider">
            Xem trước — {{ selectedPreviews.length }}/{{ previewList.length }} kỳ được chọn
          </span>
          <span
            v-if="previewList.some(p => p.isDuplicate)"
            class="ml-auto flex items-center gap-1 text-xs text-amber-600 bg-amber-50 border border-amber-200 px-2 py-0.5 rounded-full"
          >
            <svg class="h-3 w-3" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M8.485 2.495c.673-1.167 2.357-1.167 3.03 0l6.28 10.875c.673 1.167-.17 2.625-1.516 2.625H3.72c-1.347 0-2.189-1.458-1.515-2.625L8.485 2.495zM10 5a.75.75 0 01.75.75v3.5a.75.75 0 01-1.5 0v-3.5A.75.75 0 0110 5zm0 9a1 1 0 100-2 1 1 0 000 2z" clip-rule="evenodd" /></svg>
            {{ previewList.filter(p => p.isDuplicate).length }} kỳ đã tồn tại
          </span>
        </div>

        <!-- Rows -->
        <div class="divide-y divide-slate-100 max-h-60 overflow-y-auto">
          <div
            v-for="(p, idx) in previewList" :key="idx"
            :class="[
              'flex items-center gap-3 px-4 py-2.5 text-sm transition',
              p.isDuplicate ? 'bg-amber-50/60' : p.selected ? 'bg-white' : 'bg-slate-50/60 opacity-60'
            ]"
          >
            <!-- Checkbox -->
            <input
              v-if="!p.isDuplicate"
              type="checkbox"
              v-model="p.selected"
              class="h-4 w-4 rounded border-slate-300 text-emerald-600 cursor-pointer shrink-0"
            />
            <div v-else class="h-4 w-4 shrink-0 flex items-center justify-center">
              <svg class="h-4 w-4 text-amber-400" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M8.485 2.495c.673-1.167 2.357-1.167 3.03 0l6.28 10.875c.673 1.167-.17 2.625-1.516 2.625H3.72c-1.347 0-2.189-1.458-1.515-2.625L8.485 2.495z" clip-rule="evenodd" />
              </svg>
            </div>

            <!-- Code badge -->
            <span class="font-mono text-xs bg-slate-100 text-slate-600 px-1.5 py-0.5 rounded shrink-0">{{ p.code }}</span>

            <!-- Name -->
            <span class="flex-1 font-medium text-slate-700 truncate">{{ p.name }}</span>

            <!-- Date range -->
            <span class="text-xs text-slate-400 shrink-0">{{ fmt(p.fromDate) }} – {{ fmt(p.toDate) }}</span>

            <!-- Duplicate badge -->
            <span v-if="p.isDuplicate" class="shrink-0 text-xs text-amber-600 font-medium">Đã tồn tại</span>
          </div>
        </div>
      </div>

      <!-- Empty state when no month chosen -->
      <div v-else class="flex flex-col items-center gap-2 py-8 text-slate-400">
        <svg class="h-10 w-10 opacity-40" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
        <span class="text-sm">Chọn tháng bắt đầu để xem trước danh sách kỳ lương</span>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════
         STEP 2 — Running / Progress
    ════════════════════════════════════════════════════ -->
    <div v-else-if="jobStatus === 'running'" class="py-6 space-y-5">
      <div class="flex flex-col items-center gap-3">
        <div class="relative h-16 w-16">
          <svg class="h-16 w-16 -rotate-90" viewBox="0 0 64 64">
            <circle cx="32" cy="32" r="28" fill="none" stroke="#e2e8f0" stroke-width="6" />
            <circle
              cx="32" cy="32" r="28" fill="none"
              stroke="#10b981" stroke-width="6"
              stroke-linecap="round"
              :stroke-dasharray="`${jobProgress * 1.759} 175.9`"
              style="transition: stroke-dasharray 0.3s ease"
            />
          </svg>
          <span class="absolute inset-0 flex items-center justify-center text-sm font-bold text-emerald-600">
            {{ jobProgress }}%
          </span>
        </div>
        <p class="text-sm text-slate-600">Đang tạo kỳ lương... <strong class="text-slate-800">{{ jobDone }}/{{ selectedPreviews.length }}</strong></p>
      </div>

      <!-- Mini progress bar -->
      <div class="h-2 rounded-full bg-slate-100 overflow-hidden">
        <div
          class="h-full bg-emerald-500 rounded-full transition-all duration-300"
          :style="{ width: jobProgress + '%' }"
        />
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════
         STEP 3 — Done
    ════════════════════════════════════════════════════ -->
    <div v-else class="py-6 space-y-4">
      <div class="flex flex-col items-center gap-3 text-center">
        <div class="h-14 w-14 rounded-full bg-emerald-100 flex items-center justify-center">
          <svg class="h-7 w-7 text-emerald-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
          </svg>
        </div>
        <div>
          <p class="font-semibold text-slate-800">Hoàn tất!</p>
          <p class="text-sm text-slate-500 mt-0.5">
            Đã tạo thành công <strong class="text-emerald-600">{{ jobDone }}</strong> kỳ lương.
            <span v-if="jobFailed.length" class="text-red-500"> {{ jobFailed.length }} kỳ thất bại.</span>
          </p>
        </div>
      </div>

      <!-- Failed list -->
      <div v-if="jobFailed.length" class="rounded-lg border border-red-200 bg-red-50 p-3 space-y-1">
        <p class="text-xs font-semibold text-red-600 mb-1">Kỳ không tạo được:</p>
        <p v-for="name in jobFailed" :key="name" class="text-xs text-red-500">• {{ name }}</p>
      </div>
    </div>

    <!-- ── Footer ── -->
    <template #footer>
      <template v-if="jobStatus === 'idle'">
        <AppButton variant="secondary" @click="$emit('close')">Hủy</AppButton>
        <AppButton
          :disabled="!!formError || !selectedPreviews.length"
          @click="$emit('confirm')"
        >
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Tạo {{ selectedPreviews.length }} kỳ lương
        </AppButton>
      </template>

      <template v-else-if="jobStatus === 'running'">
        <AppButton variant="secondary" disabled>Đang xử lý…</AppButton>
      </template>

      <template v-else>
        <AppButton @click="$emit('close')">Đóng</AppButton>
      </template>
    </template>
  </AppModal>
</template>

<style scoped>
/* Smooth row hover in preview */
.divide-y > div:hover {
  background-color: var(--bg-subtle, #f8fafc) !important;
}
</style>
