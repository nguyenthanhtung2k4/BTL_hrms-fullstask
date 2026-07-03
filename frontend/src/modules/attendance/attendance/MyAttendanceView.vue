<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { attendanceService } from '../../../services/attendance.service'
import { shiftService } from '../../../services/shift.service'
import { workScheduleService } from '../../../services/workSchedule.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { AttendanceRecord, Shift, AttendanceAdjustment } from '../../../types/attendance.types'
import AppButton from '../../../components/ui/AppButton.vue'
import AppTable from '../../../components/ui/AppTable.vue'

const auth = useAuthStore()
const toast = useToastStore()

const todayRecord = ref<AttendanceRecord | null>(null)
const history = ref<AttendanceRecord[]>([])
const shifts = ref<Shift[]>([])
const selectedShiftCode = ref('')
const todaySchedule = ref<any>(null)
const loading = ref(true)
const actionLoading = ref(false)

const activeTab = ref<'history' | 'adjustments'>('history')
const myAdjustments = ref<AttendanceAdjustment[]>([])

// Late/Early Check Reason
const checkReason = ref('')

const historyColumns = [
  { key: 'workDate', label: 'Ngày làm việc' },
  { key: 'shiftName', label: 'Ca làm việc' },
  { key: 'checkInAt', label: 'Giờ Check-in' },
  { key: 'checkOutAt', label: 'Giờ Check-out' },
  { key: 'workedMinutes', label: 'Tổng giờ làm' },
  { key: 'status', label: 'Trạng thái' },
]

const adjustmentColumns = [
  { key: 'workDate', label: 'Ngày giải trình' },
  { key: 'shiftName', label: 'Ca đề xuất' },
  { key: 'proposedCheckIn', label: 'Check-in đề xuất' },
  { key: 'proposedCheckOut', label: 'Check-out đề xuất' },
  { key: 'reason', label: 'Lý do' },
  { key: 'status', label: 'Trạng thái' },
]

// Live running clock
const currentTime = ref(new Date())
let timerId: any = null

const todayStr = computed(() => {
  return currentTime.value.toLocaleDateString('vi-VN', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
})

const timeStr = computed(() => {
  return currentTime.value.toLocaleTimeString('vi-VN', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
})

const employeeId = computed(() => {
  const id = auth.employeeId
  if (!id || id === '00000000-0000-0000-0000-000000000000') return ''
  return id
})

const checkStatus = computed(() => {
  if (!todayRecord.value) return 'not_checked_in'
  if (todayRecord.value.checkInAt && !todayRecord.value.checkOutAt) return 'checked_in'
  if (todayRecord.value.checkOutAt) return 'checked_out'
  return 'not_checked_in'
})

const workMinutes = computed(() => {
  if (!todayRecord.value?.checkInAt || !todayRecord.value?.checkOutAt) return 0
  return todayRecord.value.workedMinutes
})

// Late/Early detection
const selectedShift = computed(() => {
  return shifts.value.find((s) => s.code === selectedShiftCode.value)
})

const isLate = computed(() => {
  if (checkStatus.value !== 'not_checked_in' || !selectedShift.value) return false
  const [hour, minute] = selectedShift.value.startTime.split(':').map(Number)
  const shiftStart = new Date()
  shiftStart.setHours(hour, minute, 0, 0)
  // 15 mins buffer
  shiftStart.setMinutes(shiftStart.getMinutes() + 15)
  return currentTime.value > shiftStart
})

const isEarly = computed(() => {
  if (checkStatus.value !== 'checked_in' || !todayRecord.value) return false
  const activeShift = shifts.value.find((s) => s.name === todayRecord.value?.shiftName)
  if (!activeShift) return false
  const [hour, minute] = activeShift.endTime.split(':').map(Number)
  const shiftEnd = new Date()
  shiftEnd.setHours(hour, minute, 0, 0)
  return currentTime.value < shiftEnd
})

// Adjustment Modal Form State
const showAdjustmentModal = ref(false)
const adjDate = ref(new Date().toISOString().split('T')[0])
const adjShiftId = ref('')
const adjCheckInTime = ref('')
const adjCheckOutTime = ref('')
const adjReason = ref('')
const adjSubmitting = ref(false)

async function load() {
  if (!employeeId.value) {
    loading.value = false
    return
  }
  try {
    const localTodayStr = (() => {
      const d = new Date()
      const y = d.getFullYear()
      const m = String(d.getMonth() + 1).padStart(2, '0')
      const day = String(d.getDate()).padStart(2, '0')
      return `${y}-${m}-${day}`
    })()

    const [myRecords, allShifts, adjs] = await Promise.all([
      attendanceService.getMyToday(),
      shiftService.getAll(),
      attendanceService.getMyAdjustments(),
    ])

    todayRecord.value = Array.isArray(myRecords) && myRecords.length > 0 ? myRecords[myRecords.length - 1] : null
    history.value = Array.isArray(myRecords) ? myRecords.slice(0, 10) : []
    shifts.value = allShifts.filter((s) => s.isActive)
    myAdjustments.value = adjs

    // Load schedule for today to auto-select shift
    try {
      const userSchedules = await workScheduleService.getAll({ employeeId: employeeId.value })
      const schedToday = userSchedules.find((s) => s.workDate === localTodayStr)
      if (schedToday) {
        todaySchedule.value = schedToday
        const matchedShift = allShifts.find((s) => s.id === schedToday.shiftId)
        if (matchedShift) {
          selectedShiftCode.value = matchedShift.code
        }
      }
    } catch {
      // Ignored if no schedules endpoint access
    }

    // Default shift if not selected
    if (!selectedShiftCode.value && shifts.value.length > 0) {
      selectedShiftCode.value = shifts.value[0].code
    }
  } catch {
    toast.error('Không thể tải thông tin ca làm việc')
  } finally {
    loading.value = false
  }
}

async function doCheckIn() {
  if (!employeeId.value) {
    toast.error('Không tìm thấy thông tin nhân viên')
    return
  }
  if (!selectedShiftCode.value) {
    toast.error('Vui lòng chọn ca làm việc để Check-in')
    return
  }
  if (isLate.value && !checkReason.value.trim()) {
    toast.error('Vui lòng nhập lý do đi muộn để Check-in!')
    return
  }

  actionLoading.value = true
  try {
    await attendanceService.checkIn(selectedShiftCode.value, checkReason.value.trim() || undefined)
    toast.success('Check-in thành công!')
    checkReason.value = ''
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message ?? 'Check-in thất bại')
  } finally {
    actionLoading.value = false
  }
}

async function doCheckOut() {
  if (!employeeId.value) return
  if (isEarly.value && !checkReason.value.trim()) {
    toast.error('Vui lòng nhập lý do về sớm để Check-out!')
    return
  }

  actionLoading.value = true
  try {
    await attendanceService.checkOut(checkReason.value.trim() || undefined)
    toast.success('Check-out thành công!')
    checkReason.value = ''
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message ?? 'Check-out thất bại')
  } finally {
    actionLoading.value = false
  }
}

function openAdjustmentModal() {
  adjDate.value = new Date().toISOString().split('T')[0]
  adjShiftId.value = shifts.value[0]?.id || ''
  adjCheckInTime.value = ''
  adjCheckOutTime.value = ''
  adjReason.value = ''
  showAdjustmentModal.value = true
}

async function submitAdjustment() {
  if (!adjShiftId.value) {
    toast.error('Vui lòng chọn ca làm việc')
    return
  }
  if (!adjReason.value.trim()) {
    toast.error('Vui lòng nhập lý do giải trình')
    return
  }
  if (!adjCheckInTime.value && !adjCheckOutTime.value) {
    toast.error('Vui lòng nhập ít nhất giờ vào hoặc giờ ra đề xuất')
    return
  }

  adjSubmitting.value = true
  try {
    const proposedIn = adjCheckInTime.value ? `${adjDate.value}T${adjCheckInTime.value}:00` : undefined
    const proposedOut = adjCheckOutTime.value ? `${adjDate.value}T${adjCheckOutTime.value}:00` : undefined

    await attendanceService.createAdjustment({
      workDate: adjDate.value,
      shiftId: adjShiftId.value,
      proposedCheckIn: proposedIn,
      proposedCheckOut: proposedOut,
      reason: adjReason.value.trim(),
    })

    toast.success('Gửi đơn giải trình chấm công thành công!')
    showAdjustmentModal.value = false
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message ?? 'Gửi đơn giải trình thất bại')
  } finally {
    adjSubmitting.value = false
  }
}

function fmtTime(d?: string) {
  return d ? new Date(d).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '—'
}

function fmtDate(d: string) {
  return new Date(d).toLocaleDateString('vi-VN')
}

function fmtMinutes(m: number) {
  return m >= 60 ? `${Math.floor(m / 60)}h ${m % 60}m` : `${m}m`
}

const sortedHistory = computed(() => {
  return [...history.value].sort((a, b) => new Date(b.workDate).getTime() - new Date(a.workDate).getTime())
})

const sortedAdjustments = computed(() => {
  return [...myAdjustments.value].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
})

onMounted(() => {
  load()
  timerId = setInterval(() => {
    currentTime.value = new Date()
  }, 1000)
})

onUnmounted(() => {
  if (timerId) clearInterval(timerId)
})
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-6">
    <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-slate-900 tracking-tight">Cổng chấm công</h1>
        <p class="text-sm text-slate-500 mt-1 capitalize">{{ todayStr }}</p>
      </div>
      <!-- Clock component -->
      <div class="bg-gradient-to-r from-emerald-600 to-teal-600 text-white px-5 py-2.5 rounded-2xl shadow-md flex items-center gap-3 self-start md:self-auto">
        <svg class="h-5 w-5 animate-pulse" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <span class="font-mono text-xl font-bold tracking-widest">{{ timeStr }}</span>
      </div>
    </div>

    <!-- Status & Checkin control card -->
    <div class="relative overflow-hidden rounded-3xl border border-slate-200 bg-white p-6 md:p-8 shadow-sm">
      <div v-if="loading" class="space-y-6 py-4">
        <div class="flex gap-6">
          <div class="h-10 w-24 animate-pulse rounded bg-slate-200" />
          <div class="h-10 w-24 animate-pulse rounded bg-slate-200" />
        </div>
        <div class="h-12 w-full animate-pulse rounded bg-slate-200" />
      </div>

      <div v-else-if="auth.isAdmin" class="text-center py-12 max-w-md mx-auto animate-fade-in">
        <div class="mx-auto w-16 h-16 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center mb-5 border border-emerald-100 shadow-sm">
          <svg class="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
          </svg>
        </div>
        <h3 class="text-lg font-bold text-slate-900 tracking-tight">Tài khoản Quản trị viên (Admin)</h3>
        <p class="text-sm text-slate-500 mt-2 leading-relaxed">
          Tài khoản Admin hệ thống có đặc quyền quản trị tối cao và không tham gia chấm công đi làm trực tiếp. 
          Bạn có thể truy cập trang Quản lý chấm công để xem hoặc đối soát lịch sử đi làm của nhân viên.
        </p>
      </div>

      <div v-else-if="!employeeId" class="text-center py-10 max-w-md mx-auto">
        <div class="mx-auto w-12 h-12 rounded-full bg-amber-50 flex items-center justify-center text-amber-500 mb-4">
          <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
        </div>
        <h3 class="text-base font-semibold text-slate-900">Tài khoản chưa được liên kết</h3>
        <p class="text-sm text-slate-500 mt-2">
          Hồ sơ của bạn hiện tại chưa được liên kết với mã nhân viên trên hệ thống. 
          Vui lòng liên hệ bộ phận nhân sự (HR) để được hỗ trợ đồng bộ thông tin tài khoản.
        </p>
      </div>

      <div v-else class="space-y-6">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6 items-center">
          
          <!-- Column 1: Selector (Only show if not checked in) -->
          <div class="md:col-span-2 space-y-4">
            <div v-if="checkStatus === 'not_checked_in'">
              <label class="text-xs font-semibold text-slate-500 uppercase tracking-wider block mb-2">Chọn ca chấm công</label>
              <div class="relative">
                <select
                  v-model="selectedShiftCode"
                  class="h-11 w-full rounded-xl border border-slate-300 bg-slate-50 px-4 text-sm font-medium text-slate-800 outline-none transition-all focus:border-emerald-500 focus:bg-white focus:ring-2 focus:ring-emerald-100 appearance-none"
                >
                  <option v-for="s in shifts" :key="s.id" :value="s.code">
                    {{ s.name }} ({{ s.startTime }} - {{ s.endTime }})
                  </option>
                </select>
                <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-4 text-slate-400">
                  <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
                </div>
              </div>
              <p v-if="todaySchedule" class="text-xs text-emerald-600 font-medium mt-2 flex items-center gap-1">
                <span class="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-ping" />
                Lịch biểu hôm nay của bạn: {{ todaySchedule.shiftName }} (Tự động đề xuất)
              </p>
              <p v-else class="text-xs text-slate-400 mt-2">
                Bạn không có lịch biểu phân ca hôm nay. Vui lòng chọn ca làm việc phù hợp để chấm công.
              </p>
            </div>
            
            <div v-else class="bg-slate-50 rounded-2xl p-4 border border-slate-100">
              <span class="text-xs font-semibold text-slate-400 uppercase tracking-wider block mb-1">Ca đang chấm công</span>
              <span class="text-sm font-bold text-slate-800">
                {{ todayRecord?.shiftName || '—' }} ({{ fmtTime(todayRecord?.checkInAt) }} - {{ fmtTime(todayRecord?.checkOutAt) }})
              </span>
            </div>
          </div>

          <!-- Column 2: Status Indicator -->
          <div class="flex justify-start md:justify-end">
            <span v-if="checkStatus === 'not_checked_in'" class="inline-flex items-center gap-2 rounded-full bg-slate-100 px-4 py-1.5 text-sm text-slate-600 font-medium border border-slate-200">
              <span class="h-2 w-2 rounded-full bg-slate-400" /> Chưa check-in
            </span>
            <span v-else-if="checkStatus === 'checked_in'" class="inline-flex items-center gap-2 rounded-full bg-emerald-50 px-4 py-1.5 text-sm text-emerald-700 font-semibold border border-emerald-100">
              <span class="h-2 w-2 rounded-full bg-emerald-500 animate-pulse" /> Đang trong giờ làm
            </span>
            <span v-else class="inline-flex items-center gap-2 rounded-full bg-blue-50 px-4 py-1.5 text-sm text-blue-700 font-semibold border border-blue-100">
              <span class="h-2 w-2 rounded-full bg-blue-500" /> Đã hoàn thành hôm nay
            </span>
          </div>
        </div>

        <!-- Late/Early checkout Warnings and Inputs -->
        <div v-if="isLate" class="p-4 bg-amber-50 border border-amber-200 rounded-2xl space-y-2">
          <label class="text-xs font-bold text-amber-800 block">Bạn đang Check-in muộn hơn thời gian quy định. Vui lòng điền lý do:</label>
          <textarea v-model="checkReason" placeholder="Ví dụ: Kẹt xe, hỏng xe, sự cố gia đình..." class="w-full text-sm p-3 rounded-xl border border-slate-200 outline-none focus:border-amber-500 focus:ring-2 focus:ring-amber-100" rows="2"></textarea>
        </div>

        <div v-if="isEarly" class="p-4 bg-amber-50 border border-amber-200 rounded-2xl space-y-2">
          <label class="text-xs font-bold text-amber-800 block">Bạn đang Check-out sớm trước giờ tan làm quy định. Vui lòng điền lý do:</label>
          <textarea v-model="checkReason" placeholder="Ví dụ: Về sớm có việc đột xuất, đi công tác khách hàng..." class="w-full text-sm p-3 rounded-xl border border-slate-200 outline-none focus:border-amber-500 focus:ring-2 focus:ring-amber-100" rows="2"></textarea>
        </div>

        <hr class="border-slate-100" />

        <!-- Checkin Details and Actions -->
        <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-6">
          <div class="grid grid-cols-3 gap-6 sm:gap-12">
            <div>
              <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-1">Giờ vào</div>
              <div class="text-xl font-bold text-slate-800 font-mono">
                {{ fmtTime(todayRecord?.checkInAt) }}
              </div>
            </div>
            <div>
              <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-1">Giờ ra</div>
              <div class="text-xl font-bold text-slate-800 font-mono">
                {{ fmtTime(todayRecord?.checkOutAt) }}
              </div>
            </div>
            <div>
              <div class="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-1">Tổng giờ</div>
              <div class="text-xl font-bold text-emerald-700 font-mono">
                {{ checkStatus === 'checked_out' ? fmtMinutes(workMinutes) : '—' }}
              </div>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex items-stretch sm:items-center">
            <AppButton
              v-if="checkStatus === 'not_checked_in'"
              size="lg"
              class="w-full sm:w-auto px-8 bg-gradient-to-r from-emerald-600 to-teal-600 text-white font-semibold shadow-lg hover:shadow-emerald-600/10 hover:from-emerald-550 hover:to-teal-550 transition-all rounded-xl"
              :loading="actionLoading"
              @click="doCheckIn"
            >
              <svg class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1" /></svg>
              CHECK-IN VÀO CA
            </AppButton>
            <AppButton
              v-if="checkStatus === 'checked_in'"
              size="lg"
              class="w-full sm:w-auto px-8 bg-gradient-to-r from-blue-600 to-indigo-600 text-white hover:from-blue-750 hover:to-indigo-750 border-0 font-semibold shadow-lg hover:shadow-blue-650/10 transition-all rounded-xl"
              :loading="actionLoading"
              @click="doCheckOut"
            >
              <svg class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" /></svg>
              CHECK-OUT RA CA
            </AppButton>
          </div>
        </div>
      </div>
    </div>

    <!-- Tab bar & list -->
    <div class="space-y-4">
      <div class="flex items-center justify-between border-b border-slate-200">
        <div class="flex gap-6">
          <button
            @click="activeTab = 'history'"
            :class="['pb-3 text-sm font-bold border-b-2 transition-all outline-none', activeTab === 'history' ? 'border-emerald-600 text-emerald-600' : 'border-transparent text-slate-500 hover:text-slate-800']"
          >
            Lịch sử chấm công gần đây
          </button>
          <button
            @click="activeTab = 'adjustments'"
            :class="['pb-3 text-sm font-bold border-b-2 transition-all outline-none', activeTab === 'adjustments' ? 'border-emerald-600 text-emerald-600' : 'border-transparent text-slate-500 hover:text-slate-800']"
          >
            Đơn giải trình / Sửa công ({{ myAdjustments.length }})
          </button>
        </div>
        <AppButton size="sm" variant="secondary" @click="openAdjustmentModal">
          <svg class="h-4 w-4 mr-1.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>
          Tạo đơn giải trình
        </AppButton>
      </div>

      <!-- History Table -->
      <div v-if="activeTab === 'history'">
        <AppTable :page-size="10"
          :columns="historyColumns"
          :rows="sortedHistory"
          :loading="loading"
          row-key="id"
          empty-text="Chưa ghi nhận lịch sử chấm công nào trong tháng này."
        >
          <template #default="{ row: r }">
            <td class="px-5 py-4 font-medium text-slate-900">{{ fmtDate(r.workDate) }}</td>
            <td class="px-5 py-4 text-slate-700 font-medium">{{ r.shiftName || '—' }}</td>
            <td class="px-5 py-4">
              <div>
                <span class="text-emerald-700 font-mono font-semibold">{{ fmtTime(r.checkInAt) }}</span>
                <p v-if="r.checkInReason" class="text-[11px] text-slate-400 italic mt-0.5" :title="r.checkInReason">Lý do: {{ r.checkInReason }}</p>
              </div>
            </td>
            <td class="px-5 py-4">
              <div>
                <span class="text-blue-700 font-mono font-semibold">{{ fmtTime(r.checkOutAt) }}</span>
                <p v-if="r.checkOutReason" class="text-[11px] text-slate-400 italic mt-0.5" :title="r.checkOutReason">Lý do: {{ r.checkOutReason }}</p>
              </div>
            </td>
            <td class="px-5 py-4 text-slate-800 font-semibold">
              {{ r.workedMinutes > 0 ? fmtMinutes(r.workedMinutes) : '—' }}
            </td>
            <td class="px-5 py-4">
              <span
                class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold border"
                :class="[
                  r.status === 'Completed' ? 'bg-emerald-50 text-emerald-700 border-emerald-100' :
                  r.status === 'CheckedIn' ? 'bg-amber-50 text-amber-700 border-amber-100' :
                  'bg-slate-50 text-slate-600 border-slate-100'
                ]"
              >
                {{ r.status === 'Completed' ? 'Hoàn thành' : r.status === 'CheckedIn' ? 'Đang làm' : r.status }}
              </span>
            </td>
          </template>
        </AppTable>
      </div>

      <!-- Adjustments Table -->
      <div v-else>
        <AppTable :page-size="10"
          :columns="adjustmentColumns"
          :rows="sortedAdjustments"
          :loading="loading"
          row-key="id"
          empty-text="Bạn chưa gửi đơn giải trình chấm công nào."
        >
          <template #default="{ row: a }">
            <td class="px-5 py-4 font-medium text-slate-900">{{ fmtDate(a.workDate) }}</td>
            <td class="px-5 py-4 text-slate-700 font-medium">{{ a.shiftName }}</td>
            <td class="px-5 py-4 font-mono font-semibold text-slate-700">{{ a.proposedCheckIn ? fmtTime(a.proposedCheckIn) : '—' }}</td>
            <td class="px-5 py-4 font-mono font-semibold text-slate-700">{{ a.proposedCheckOut ? fmtTime(a.proposedCheckOut) : '—' }}</td>
            <td class="px-5 py-4 text-sm text-slate-600 max-w-[200px] truncate" :title="a.reason">{{ a.reason }}</td>
            <td class="px-5 py-4">
              <span
                class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold border"
                :class="[
                  a.status === 'Approved' ? 'bg-emerald-50 text-emerald-700 border-emerald-100' :
                  a.status === 'Pending' ? 'bg-amber-50 text-amber-700 border-amber-100' :
                  'bg-rose-50 text-rose-700 border-rose-100'
                ]"
              >
                {{ a.status === 'Approved' ? 'Đã duyệt' : a.status === 'Pending' ? 'Chờ duyệt' : 'Từ chối' }}
              </span>
              <p v-if="a.handledByName" class="text-[10px] text-slate-400 mt-1">Duyệt bởi: {{ a.handledByName }}</p>
            </td>
          </template>
        </AppTable>
      </div>
    </div>

    <!-- Attendance Correction Modal -->
    <div v-if="showAdjustmentModal" class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4">
      <div class="w-full max-w-lg bg-white rounded-3xl shadow-xl overflow-hidden border border-slate-100 animate-scale-in">
        <div class="px-6 py-5 border-b border-slate-100 flex items-center justify-between">
          <h3 class="text-lg font-bold text-slate-900">Đơn giải trình chấm công</h3>
          <button @click="showAdjustmentModal = false" class="text-slate-400 hover:text-slate-600 transition-colors">
            <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
          </button>
        </div>
        
        <form @submit.prevent="submitAdjustment" class="p-6 space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="flex flex-col">
              <label class="text-xs font-bold text-slate-500 uppercase tracking-wider mb-1.5">Ngày giải trình</label>
              <input type="date" v-model="adjDate" class="h-10 rounded-xl border border-slate-300 px-3 text-sm focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 outline-none" required />
            </div>
            
            <div class="flex flex-col">
              <label class="text-xs font-bold text-slate-500 uppercase tracking-wider mb-1.5">Ca làm việc</label>
              <select v-model="adjShiftId" class="h-10 rounded-xl border border-slate-300 px-3 text-sm focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 outline-none" required>
                <option v-for="s in shifts" :key="s.id" :value="s.id">{{ s.name }}</option>
              </select>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="flex flex-col">
              <label class="text-xs font-bold text-slate-500 uppercase tracking-wider mb-1.5">Giờ Check-in đề xuất</label>
              <input type="time" v-model="adjCheckInTime" class="h-10 rounded-xl border border-slate-300 px-3 text-sm focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 outline-none" />
            </div>

            <div class="flex flex-col">
              <label class="text-xs font-bold text-slate-500 uppercase tracking-wider mb-1.5">Giờ Check-out đề xuất</label>
              <input type="time" v-model="adjCheckOutTime" class="h-10 rounded-xl border border-slate-300 px-3 text-sm focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 outline-none" />
            </div>
          </div>

          <div class="flex flex-col">
            <label class="text-xs font-bold text-slate-500 uppercase tracking-wider mb-1.5">Lý do giải trình</label>
            <textarea v-model="adjReason" placeholder="Ví dụ: Quên check-in buổi sáng do vội họp, đi công tác ngoài..." class="w-full text-sm p-3 rounded-xl border border-slate-300 focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 outline-none" rows="3" required></textarea>
          </div>

          <div class="flex justify-end gap-3 pt-4 border-t border-slate-100">
            <AppButton type="button" variant="secondary" @click="showAdjustmentModal = false">Hủy</AppButton>
            <AppButton type="submit" variant="primary" :loading="adjSubmitting">Gửi đơn</AppButton>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
