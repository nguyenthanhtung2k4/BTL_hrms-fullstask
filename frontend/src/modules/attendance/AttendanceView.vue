<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Clock,
  Calendar,
  FileText,
  CheckSquare,
  Settings,
  MapPin,
  Server,
  Play,
  Square,
  Check,
  X
} from '@lucide/vue'
import { mockDB, attendanceService } from '../../services/mockData'
import { useAuthStore } from '../../stores/auth'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

// Sync active tab with router query param ?tab=
const activeTab = computed(() => {
  const queryTab = route.query.tab as string
  if (['checkin', 'schedule', 'requests', 'approval', 'shifts'].includes(queryTab)) {
    return queryTab
  }
  return 'checkin'
})

function setTab(tabName: 'checkin' | 'schedule' | 'requests' | 'approval' | 'shifts') {
  router.push({ path: '/attendance', query: { tab: tabName } })
}

// Current Digital Clock
const currentTime = ref(new Date())
let timerInterval: any = null

onMounted(() => {
  timerInterval = setInterval(() => {
    currentTime.value = new Date()
  }, 1000)
})

onUnmounted(() => {
  if (timerInterval) clearInterval(timerInterval)
})

const timeString = computed(() => {
  return currentTime.value.toTimeString().split(' ')[0]
})

const dateString = computed(() => {
  const options: Intl.DateTimeFormatOptions = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }
  return currentTime.value.toLocaleDateString('vi-VN', options)
})

const todayDateStr = computed(() => currentTime.value.toISOString().split('T')[0])

const todayRecord = computed(() => {
  const empId = auth.user?.employeeId || 'emp-001'
  return mockDB.attendanceRecords.find(r => r.employeeId === empId && r.workDate === todayDateStr.value)
})

const activeShift = computed(() => {
  const empId = auth.user?.employeeId || 'emp-001'
  const sched = mockDB.schedules.find(s => s.employeeId === empId && s.date === todayDateStr.value)
  const shiftId = sched?.shiftId || 'shift-hc'
  return mockDB.shifts.find(s => s.id === shiftId)
})

// Success status message
const messageText = ref('')
const messageType = ref<'success' | 'error'>('success')

function showMessage(text: string, type: 'success' | 'error' = 'success') {
  messageText.value = text
  messageType.value = type
  setTimeout(() => {
    messageText.value = ''
  }, 4000)
}

// Perform Check-in
function doCheckIn() {
  try {
    const empId = auth.user?.employeeId || 'emp-001'
    const shiftId = activeShift.value?.id || 'shift-hc'
    attendanceService.checkIn(empId, shiftId)
    showMessage('Chấm công CHECK-IN thành công!')
  } catch (error: any) {
    showMessage(error.message, 'error')
  }
}

// Perform Check-out
function doCheckOut() {
  try {
    const empId = auth.user?.employeeId || 'emp-001'
    const shiftId = activeShift.value?.id || 'shift-hc'
    attendanceService.checkOut(empId, shiftId)
    showMessage('Chấm công CHECK-OUT thành công!')
  } catch (error: any) {
    showMessage(error.message, 'error')
  }
}

// Personal history
const personalHistory = computed(() => {
  const empId = auth.user?.employeeId || 'emp-001'
  return mockDB.attendanceRecords
    .filter(r => r.employeeId === empId)
    .sort((a, b) => b.workDate.localeCompare(a.workDate))
})

// Leave request Form
const leaveForm = ref({
  leaveType: 'Annual' as 'Annual' | 'Sick' | 'Maternity' | 'Unpaid',
  fromDate: new Date().toISOString().split('T')[0],
  toDate: new Date().toISOString().split('T')[0],
  reason: ''
})

function submitLeave() {
  if (!leaveForm.value.reason) {
    showMessage('Vui lòng nhập lý do xin nghỉ!', 'error')
    return
  }
  const empId = auth.user?.employeeId || 'emp-001'
  attendanceService.createLeave({
    employeeId: empId,
    leaveType: leaveForm.value.leaveType,
    fromDate: leaveForm.value.fromDate,
    toDate: leaveForm.value.toDate,
    reason: leaveForm.value.reason
  })
  leaveForm.value.reason = ''
  showMessage('Gửi đơn xin nghỉ phép thành công!')
}

// OT request Form
const otForm = ref({
  date: new Date().toISOString().split('T')[0],
  minutes: 60,
  reason: ''
})

function submitOT() {
  if (!otForm.value.reason) {
    showMessage('Vui lòng nhập lý do tăng ca!', 'error')
    return
  }
  const empId = auth.user?.employeeId || 'emp-001'
  attendanceService.createOT(
    empId,
    otForm.value.date,
    otForm.value.minutes,
    otForm.value.reason
  )
  otForm.value.reason = ''
  showMessage('Đăng ký làm thêm giờ thành công!')
}

// Requests lists
const personalLeaves = computed(() => {
  const empId = auth.user?.employeeId || 'emp-001'
  return mockDB.leaveRequests.filter(l => l.employeeId === empId)
})

const personalOTs = computed(() => {
  const empId = auth.user?.employeeId || 'emp-001'
  return mockDB.otRequests.filter(o => o.employeeId === empId)
})

// Approvals lists
const pendingLeaves = computed(() => {
  return mockDB.leaveRequests.filter(l => l.status === 'Pending')
})

const pendingOTs = computed(() => {
  return mockDB.otRequests.filter(o => o.status === 'Pending')
})

const approvalFeedback = ref<Record<string, string>>({})

function approveLeave(id: string) {
  const managerId = auth.user?.employeeId || 'emp-002'
  const fb = approvalFeedback.value[id] || 'Đã duyệt phép'
  attendanceService.approveLeave(id, managerId, fb)
  showMessage('Đã phê duyệt đơn nghỉ phép của nhân viên!')
}

function rejectLeave(id: string) {
  const managerId = auth.user?.employeeId || 'emp-002'
  const fb = approvalFeedback.value[id] || 'Không duyệt phép'
  attendanceService.rejectLeave(id, managerId, fb)
  showMessage('Từ chối duyệt phép thành công!', 'error')
}

function approveOT(id: string) {
  const managerId = auth.user?.employeeId || 'emp-002'
  attendanceService.approveOT(id, managerId)
  showMessage('Đã duyệt tăng ca cho nhân viên!')
}

function rejectOT(id: string) {
  const managerId = auth.user?.employeeId || 'emp-002'
  attendanceService.rejectOT(id, managerId)
  showMessage('Từ chối duyệt tăng ca thành công!', 'error')
}

// Shifts Config
const shiftForm = ref({
  name: '',
  startTime: '08:00',
  endTime: '17:00',
  lateGraceMinutes: 15,
  color: '#2563eb'
})

function saveShift() {
  if (!shiftForm.value.name) return
  attendanceService.addShift({ ...shiftForm.value })
  shiftForm.value.name = ''
  showMessage('Tạo ca làm việc mới thành công!')
}

// Schedulingboard Matrix state
const selectedScheduleDate = ref(new Date().toISOString().split('T')[0])
const selectedScheduleShift = ref('shift-hc')

function assignShiftToEmployee(empId: string) {
  attendanceService.assignSchedule(empId, selectedScheduleDate.value, selectedScheduleShift.value)
  showMessage('Cập nhật lịch làm việc thành công!')
}

function getShiftNameForEmpDate(empId: string, date: string) {
  const sched = mockDB.schedules.find(s => s.employeeId === empId && s.date === date)
  if (!sched) return 'Nghỉ'
  const shift = mockDB.shifts.find(s => s.id === sched.shiftId)
  return shift ? shift.name.split(' ')[0] : 'Nghỉ'
}
</script>

<template>
  <div class="space-y-6 animate-fadeIn">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between border-b border-slate-200 dark:border-slate-800 pb-4">
      <div>
        <h1 class="text-xl font-bold text-slate-900 dark:text-slate-50">🕒 Attendance Service (Chấm công & Nghỉ phép)</h1>
        <p class="text-xs text-slate-500 dark:text-slate-400 mt-0.5">Nhóm 8 · Schema: HRMS_AttendanceDb · Quản lý lịch làm việc và đơn nghỉ phép.</p>
      </div>
      <div class="flex items-center gap-2 text-xs font-semibold text-slate-500 dark:text-slate-400">
        <span>Cơ sở dữ liệu:</span>
        <span class="px-2 py-0.5 bg-blue-50 dark:bg-blue-950/20 text-blue-600 dark:text-blue-400 rounded-md border border-blue-150 dark:border-blue-900/40 font-mono">
          HRMS_AttendanceDb
        </span>
      </div>
    </div>

    <!-- Alert Success/Error overlay -->
    <div 
      v-if="messageText" 
      class="fixed top-12 right-6 z-50 px-4 py-3 rounded-lg shadow-lg text-xs font-bold flex items-center gap-2 border animate-scaleIn transition-all"
      :class="messageType === 'success' 
        ? 'bg-blue-50 dark:bg-blue-950/20 border-blue-250 dark:border-blue-800 text-blue-750 dark:text-blue-400' 
        : 'bg-red-50 dark:bg-red-950/20 border-red-250 dark:border-red-800 text-red-750 dark:text-red-400'"
    >
      <Check v-if="messageType === 'success'" :size="16" />
      <X v-else :size="16" />
      <span>{{ messageText }}</span>
    </div>

    <!-- Tabs switcher based on Role -->
    <div class="flex border-b border-slate-200 dark:border-slate-800 overflow-x-auto no-print">
      <button 
        v-if="['Admin', 'HR', 'Manager', 'Employee'].includes(auth.activeRole)"
        @click="setTab('checkin')"
        class="flex items-center gap-2 px-5 py-3.5 border-b-2 text-sm font-semibold transition-all cursor-pointer whitespace-nowrap"
        :class="activeTab === 'checkin' ? 'border-blue-600 text-blue-600 dark:text-blue-400 font-bold' : 'border-transparent text-slate-500 hover:text-slate-900'"
      >
        <Clock :size="16" />
        <span>Chấm công cá nhân</span>
      </button>

      <button 
        v-if="['Admin', 'Employee'].includes(auth.activeRole)"
        @click="setTab('requests')"
        class="flex items-center gap-2 px-5 py-3.5 border-b-2 text-sm font-semibold transition-all cursor-pointer whitespace-nowrap"
        :class="activeTab === 'requests' ? 'border-blue-600 text-blue-600 dark:text-blue-400 font-bold' : 'border-transparent text-slate-500 hover:text-slate-900'"
      >
        <FileText :size="16" />
        <span>Đăng ký Nghỉ / OT</span>
      </button>

      <button 
        v-if="['Admin', 'Manager', 'HR'].includes(auth.activeRole)"
        @click="setTab('approval')"
        class="flex items-center gap-2 px-5 py-3.5 border-b-2 text-sm font-semibold transition-all cursor-pointer whitespace-nowrap"
        :class="activeTab === 'approval' ? 'border-blue-600 text-blue-600 dark:text-blue-400 font-bold' : 'border-transparent text-slate-500 hover:text-slate-900'"
      >
        <CheckSquare :size="16" class="relative">
          <span v-if="pendingLeaves.length + pendingOTs.length > 0" class="absolute -top-1 -right-1 size-1.5 bg-blue-500 rounded-full animate-ping"></span>
        </CheckSquare>
        <span>Duyệt yêu cầu ({{ pendingLeaves.length + pendingOTs.length }})</span>
      </button>

      <button 
        v-if="['Admin', 'HR', 'Manager'].includes(auth.activeRole)"
        @click="setTab('schedule')"
        class="flex items-center gap-2 px-5 py-3.5 border-b-2 text-sm font-semibold transition-all cursor-pointer whitespace-nowrap"
        :class="activeTab === 'schedule' ? 'border-blue-600 text-blue-600 dark:text-blue-400 font-bold' : 'border-transparent text-slate-500 hover:text-slate-900'"
      >
        <Calendar :size="16" />
        <span>Phân lịch & Bảng công</span>
      </button>

      <button 
        v-if="['Admin', 'HR'].includes(auth.activeRole)"
        @click="setTab('shifts')"
        class="flex items-center gap-2 px-5 py-3.5 border-b-2 text-sm font-semibold transition-all cursor-pointer whitespace-nowrap"
        :class="activeTab === 'shifts' ? 'border-blue-600 text-blue-600 dark:text-blue-400 font-bold' : 'border-transparent text-slate-500 hover:text-slate-900'"
      >
        <Settings :size="16" />
        <span>Quản lý Ca làm việc</span>
      </button>
    </div>

    <!-- 1. VIEW TAB: CHẤM CÔNG CÁ NHÂN -->
    <div v-if="activeTab === 'checkin'" class="grid gap-6 md:grid-cols-12">
      <!-- Checkin Console -->
      <div class="md:col-span-5 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-6 shadow-2xs space-y-6 flex flex-col justify-between">
        <div class="text-center space-y-2">
          <span class="inline-block px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-slate-100 dark:bg-slate-800 text-slate-500 font-mono">
            {{ dateString }}
          </span>
          <div class="text-4xl font-extrabold text-slate-900 dark:text-slate-50 font-mono tracking-wider tabular-nums py-2.5 bg-slate-50 dark:bg-slate-950 rounded-2xl border border-slate-100 dark:border-slate-850 shadow-inner">
            {{ timeString }}
          </div>
        </div>

        <!-- Mocks GPS & IP -->
        <div class="bg-slate-50 dark:bg-slate-950 border border-slate-150 dark:border-slate-850 p-4 rounded-2xl space-y-2.5 text-xs font-semibold text-slate-655 dark:text-slate-400">
          <div class="flex items-center gap-2">
            <MapPin :size="14" class="text-red-500" />
            <span>Địa điểm GPS:</span>
            <span class="text-slate-900 dark:text-slate-100 font-bold ml-auto">Keangnam Landmark, Hà Nội</span>
          </div>
          <div class="flex items-center gap-2">
            <Server :size="14" class="text-blue-500" />
            <span>Địa chỉ IP mạng:</span>
            <span class="text-slate-900 dark:text-slate-100 font-bold font-mono ml-auto">192.168.1.100</span>
          </div>
          <div class="flex items-center gap-2 border-t border-slate-200/50 dark:border-slate-800 pt-2.5 mt-1">
            <Clock :size="14" class="text-blue-500" />
            <span>Ca làm được gán hôm nay:</span>
          </div>
          <div class="bg-white dark:bg-slate-900 border border-slate-250 dark:border-slate-800 p-2.5 rounded-lg text-slate-800 dark:text-slate-105 font-extrabold text-center">
            {{ activeShift?.name || 'Ca Hành chính (8:00 - 17:00)' }}
          </div>
        </div>

        <!-- Checkin Buttons -->
        <div class="space-y-3">
          <div class="flex gap-4">
            <button 
              @click="doCheckIn"
              :disabled="Boolean(todayRecord?.checkInAt)"
              class="flex-1 inline-flex h-12 items-center justify-center gap-2 rounded-xl text-xs font-bold transition-all shadow-md shadow-blue-100 dark:shadow-none cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed text-white"
              :class="todayRecord?.checkInAt ? 'bg-slate-200 dark:bg-slate-800 text-slate-400 dark:text-slate-600 border border-slate-200 dark:border-slate-800 shadow-none' : 'bg-blue-600 hover:bg-blue-755'"
            >
              <Play :size="14" class="fill-white" />
              <span>CHECK-IN</span>
            </button>

            <button 
              @click="doCheckOut"
              :disabled="!todayRecord?.checkInAt || Boolean(todayRecord?.checkOutAt)"
              class="flex-1 inline-flex h-12 items-center justify-center gap-2 rounded-xl text-xs font-bold transition-all shadow-md shadow-indigo-100 dark:shadow-none cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed text-white"
              :class="(!todayRecord?.checkInAt || todayRecord?.checkOutAt) ? 'bg-slate-200 dark:bg-slate-800 text-slate-400 dark:text-slate-600 border border-slate-200 dark:border-slate-800 shadow-none' : 'bg-indigo-600 hover:bg-indigo-755'"
            >
              <Square :size="14" class="fill-white" />
              <span>CHECK-OUT</span>
            </button>
          </div>
          
          <div v-if="todayRecord" class="text-center">
            <span v-if="todayRecord.checkInAt && !todayRecord.checkOutAt" class="text-xs text-amber-600 dark:text-amber-400 font-bold flex items-center justify-center gap-1.5">
              <span class="h-1.5 w-1.5 rounded-full bg-amber-500 animate-pulse"></span>
              Đang trong giờ làm (Vào lúc {{ new Date(todayRecord.checkInAt).toLocaleTimeString() }})
            </span>
            <span v-if="todayRecord.checkOutAt" class="text-xs text-blue-600 dark:text-blue-400 font-bold flex items-center justify-center gap-1.5">
              <Check :size="14" class="stroke-[2.5]" />
              Đã check-out (Lúc {{ new Date(todayRecord.checkOutAt).toLocaleTimeString() }})
            </span>
          </div>
        </div>
      </div>

      <!-- History -->
      <div class="md:col-span-7 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs space-y-4">
        <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Nhật ký đi làm gần đây</h2>
        
        <div class="overflow-y-auto max-h-[360px] pr-1 space-y-2.5">
          <div 
            v-for="h in personalHistory" 
            :key="h.id"
            class="flex items-center justify-between p-3.5 border border-slate-100 dark:border-slate-800 rounded-xl hover:bg-slate-50/50 dark:hover:bg-slate-850/30 text-xs"
          >
            <div>
              <div class="font-extrabold text-slate-950 dark:text-slate-100">{{ h.workDate }}</div>
              <div class="text-[10px] text-slate-400 mt-0.5">
                {{ mockDB.shifts.find(s => s.id === h.shiftId)?.name.split(' ')[0] || 'Ca làm' }}
              </div>
            </div>
            
            <div class="flex items-center gap-6">
              <div class="text-right font-mono text-[11px] text-slate-655 dark:text-slate-400 font-medium leading-relaxed">
                <div>Vào: {{ h.checkInAt ? new Date(h.checkInAt).toLocaleTimeString() : '--:--' }}</div>
                <div>Ra: {{ h.checkOutAt ? new Date(h.checkOutAt).toLocaleTimeString() : '--:--' }}</div>
              </div>

              <span 
                class="inline-block px-2.5 py-0.5 rounded text-[10px] font-bold border w-24 text-center"
                :class="h.status === 'Completed' ? 'bg-blue-50 dark:bg-blue-950/20 border-blue-200 dark:border-blue-800 text-blue-700 dark:text-blue-400' :
                        h.status === 'Late' ? 'bg-amber-50 dark:bg-amber-950/20 border-amber-200 dark:border-amber-800 text-amber-700 dark:text-amber-400' :
                        h.status === 'OnLeave' ? 'bg-indigo-50 dark:bg-indigo-950/20 border-indigo-200 dark:border-indigo-800 text-indigo-700 dark:text-indigo-400' :
                        'bg-red-50 dark:bg-red-950/20 border-red-200 dark:border-red-800 text-red-700 dark:text-red-400'"
              >
                {{ h.status === 'Completed' ? 'Đúng giờ' : 
                   h.status === 'Late' ? 'Trễ ca' : 
                   h.status === 'OnLeave' ? 'Nghỉ phép' :
                   h.status === 'EarlyLeave' ? 'Về sớm' : 'Vắng mặt' }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 2. VIEW TAB: REQUEST LEAVE / OT -->
    <div v-if="activeTab === 'requests'" class="grid gap-6 md:grid-cols-2">
      <div class="space-y-6">
        <!-- Leave form -->
        <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs space-y-4">
          <div class="border-b border-slate-100 dark:border-slate-800 pb-2.5">
            <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Tạo đơn xin phép</h2>
          </div>
          
          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase">Phân loại phép</label>
              <select v-model="leaveForm.leaveType" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900">
                <option value="Annual">Phép năm (Có lương)</option>
                <option value="Sick">Phép ốm (Có lương)</option>
                <option value="Maternity">Thai sản (Có lương)</option>
                <option value="Unpaid">Nghỉ không lương</option>
              </select>
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase">Thời hạn phép</label>
              <div class="flex items-center gap-1 text-xs">
                <input type="date" v-model="leaveForm.fromDate" class="w-full px-2 py-1.5 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100" />
                <span>-</span>
                <input type="date" v-model="leaveForm.toDate" class="w-full px-2 py-1.5 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100" />
              </div>
            </div>
          </div>

          <div class="space-y-1">
            <label class="text-[10px] font-bold text-slate-400 uppercase">Lý do nghỉ phép</label>
            <textarea 
              v-model="leaveForm.reason" 
              rows="2" 
              class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 outline-none focus:border-blue-500"
            ></textarea>
          </div>

          <button @click="submitLeave" class="w-full py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-lg text-xs shadow-sm cursor-pointer">
            Nộp đơn xin phép
          </button>
        </div>

        <!-- OT request form -->
        <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs space-y-4">
          <div class="border-b border-slate-100 dark:border-slate-800 pb-2.5">
            <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Đăng ký làm thêm giờ (OT)</h2>
          </div>
          
          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase">Ngày làm việc</label>
              <input type="date" v-model="otForm.date" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100" />
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase">Định lượng tăng ca</label>
              <select v-model="otForm.minutes" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900">
                <option :value="60">1 Giờ (60 phút)</option>
                <option :value="120">2 Giờ (120 phút)</option>
                <option :value="180">3 Giờ (180 phút)</option>
                <option :value="240">4 Giờ (240 phút)</option>
              </select>
            </div>
          </div>

          <div class="space-y-1">
            <label class="text-[10px] font-bold text-slate-400 uppercase">Nhiệm vụ chi tiết</label>
            <textarea 
              v-model="otForm.reason" 
              rows="2" 
              class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 outline-none focus:border-blue-500"
            ></textarea>
          </div>

          <button @click="submitOT" class="w-full py-2 bg-indigo-600 hover:bg-indigo-700 text-white font-bold rounded-lg text-xs shadow-sm cursor-pointer">
            Nộp đơn đăng ký tăng ca
          </button>
        </div>
      </div>

      <!-- Personal History list -->
      <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs space-y-6">
        <div>
          <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Đơn yêu cầu cá nhân</h2>
          <p class="text-[11px] text-slate-400 mt-0.5">Theo dõi lịch trình phê duyệt</p>
        </div>

        <div class="space-y-5">
          <div>
            <span class="text-[10px] font-bold text-slate-400 uppercase block mb-2">Đơn xin nghỉ</span>
            <div class="space-y-2 max-h-[190px] overflow-y-auto pr-1">
              <div v-for="l in personalLeaves" :key="l.id" class="p-3.5 border border-slate-100 dark:border-slate-800 rounded-xl hover:bg-slate-50 dark:hover:bg-slate-850/30 text-xs">
                <div class="flex items-center justify-between font-bold text-slate-850 dark:text-slate-200">
                  <span>{{ l.leaveType === 'Annual' ? 'Phép năm' : 'Phép bệnh' }}</span>
                  <span class="px-2 py-0.5 rounded text-[9px] border"
                    :class="l.status === 'Approved' ? 'bg-blue-50 border-blue-200 text-blue-700' : 'bg-amber-50 border-amber-200 text-amber-700'"
                  >
                    {{ l.status }}
                  </span>
                </div>
                <div class="text-[10px] text-slate-400 mt-1">Từ {{ l.fromDate }} đến {{ l.toDate }}</div>
                <div class="text-[11px] bg-slate-50 dark:bg-slate-950 p-2 border border-slate-100 dark:border-slate-850 rounded-lg text-slate-500 italic mt-2">
                  "{{ l.reason }}"
                </div>
              </div>
            </div>
          </div>

          <div class="border-t border-slate-100 dark:border-slate-800 pt-4">
            <span class="text-[10px] font-bold text-slate-400 uppercase block mb-2">Yêu cầu Tăng ca (OT)</span>
            <div class="space-y-2 max-h-[190px] overflow-y-auto pr-1">
              <div v-for="o in personalOTs" :key="o.id" class="p-3.5 border border-slate-100 dark:border-slate-800 rounded-xl hover:bg-slate-50 dark:hover:bg-slate-850/30 text-xs">
                <div class="flex items-center justify-between font-bold text-slate-850 dark:text-slate-200">
                  <span>Ngày {{ o.date }} ({{ o.requestedMinutes }}m)</span>
                  <span class="px-2 py-0.5 rounded text-[9px] border"
                    :class="o.status === 'Approved' ? 'bg-blue-50 border-blue-200 text-blue-700' : 'bg-amber-50 border-amber-200 text-amber-700'"
                  >
                    {{ o.status }}
                  </span>
                </div>
                <div class="text-[11px] bg-slate-50 dark:bg-slate-950 p-2 border border-slate-100 dark:border-slate-850 rounded-lg text-slate-500 italic mt-2">
                  "{{ o.reason }}"
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 3. VIEW TAB: LEAVE APPROVAL (Manager) -->
    <div v-if="activeTab === 'approval'" class="space-y-6">
      <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs space-y-4">
        <div>
          <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Đơn xin nghỉ chờ duyệt</h2>
        </div>

        <div class="space-y-3 max-h-[300px] overflow-y-auto pr-1">
          <div 
            v-for="l in pendingLeaves" 
            :key="l.id"
            class="p-4 border border-slate-100 dark:border-slate-800 rounded-xl bg-slate-50/20 text-xs space-y-3"
          >
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2">
                <div class="size-8 rounded-full bg-slate-100 dark:bg-slate-800 flex items-center justify-center font-bold text-slate-600 dark:text-slate-300">
                  {{ mockDB.employees.find(e => e.id === l.employeeId)?.fullName.charAt(0) }}
                </div>
                <div>
                  <div class="font-bold text-slate-900 dark:text-slate-100">
                    {{ mockDB.employees.find(e => e.id === l.employeeId)?.fullName }}
                  </div>
                  <div class="text-[9px] text-slate-450">
                    Phòng: {{ mockDB.departments.find(d => d.id === mockDB.employees.find(e => e.id === l.employeeId)?.departmentId)?.name }}
                  </div>
                </div>
              </div>
              <span class="px-2 py-0.5 bg-blue-50 dark:bg-blue-950 text-blue-700 dark:text-blue-400 border border-blue-200 dark:border-blue-800 text-[10px] font-bold rounded">
                {{ l.leaveType }}
              </span>
            </div>

            <div class="font-semibold text-slate-655 dark:text-slate-400">
              Thời gian nghỉ: từ <strong class="text-slate-900 dark:text-slate-100 font-bold">{{ l.fromDate }}</strong> đến <strong class="text-slate-900 dark:text-slate-100 font-bold">{{ l.toDate }}</strong>
            </div>

            <div class="bg-white dark:bg-slate-950 p-3 border border-slate-200 dark:border-slate-850 rounded text-slate-500 italic">
              "{{ l.reason }}"
            </div>

            <div class="flex gap-2 items-center">
              <input 
                v-model="approvalFeedback[l.id]" 
                type="text" 
                placeholder="Phản hồi..." 
                class="flex-1 px-3 py-1.5 border border-slate-200 dark:border-slate-800 rounded-lg outline-none bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100"
              />
              <button @click="rejectLeave(l.id)" class="px-3 py-1.5 border border-red-200 text-red-650 font-bold rounded-lg hover:bg-red-50 text-[11px] cursor-pointer">Từ chối</button>
              <button @click="approveLeave(l.id)" class="px-3 py-1.5 bg-blue-600 text-white font-bold rounded-lg hover:bg-blue-700 text-[11px] shadow-sm cursor-pointer">Phê duyệt</button>
            </div>
          </div>

          <div v-if="pendingLeaves.length === 0" class="text-center py-6 text-slate-400 italic">Chưa có đơn nghỉ phép nào chờ duyệt.</div>
        </div>
      </div>

      <!-- OT Approval -->
      <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs space-y-4">
        <div>
          <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Yêu cầu tăng ca chờ duyệt</h2>
        </div>

        <div class="space-y-3 max-h-[300px] overflow-y-auto pr-1">
          <div 
            v-for="o in pendingOTs" 
            :key="o.id"
            class="p-4 border border-slate-100 dark:border-slate-800 rounded-xl bg-slate-50/20 text-xs flex items-center justify-between"
          >
            <div>
              <div class="font-bold text-slate-900 dark:text-slate-100">
                {{ mockDB.employees.find(e => e.id === o.employeeId)?.fullName }}
              </div>
              <div class="text-[10px] text-slate-450 mt-1">Ngày {{ o.date }} (Tăng ca {{ o.requestedMinutes }}m)</div>
              <div class="text-[10px] text-slate-400 italic mt-1">Lý do: "{{ o.reason }}"</div>
            </div>

            <div class="flex gap-2">
              <button @click="rejectOT(o.id)" class="size-8 rounded-lg border border-red-200 text-red-650 flex items-center justify-center hover:bg-red-50 cursor-pointer">
                <X :size="14" />
              </button>
              <button @click="approveOT(o.id)" class="size-8 rounded-lg bg-blue-600 text-white flex items-center justify-center hover:bg-blue-700 shadow-sm cursor-pointer">
                <Check :size="14" />
              </button>
            </div>
          </div>

          <div v-if="pendingOTs.length === 0" class="text-center py-6 text-slate-400 italic">Chưa có đăng ký tăng ca nào chờ duyệt.</div>
        </div>
      </div>
    </div>

    <!-- 4. VIEW TAB: SCHEDULING & MONTHLY TIMESHEET -->
    <div v-if="activeTab === 'schedule'" class="space-y-4">
      <div class="bg-white dark:bg-slate-900 p-5 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-2xs space-y-4">
        <div>
          <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Phân lịch ca làm & Bảng công tháng</h2>
        </div>

        <div class="flex flex-wrap gap-4 items-end bg-slate-50 dark:bg-slate-950 p-4 rounded-xl border border-slate-150 dark:border-slate-850 text-xs no-print">
          <div class="space-y-1">
            <span class="text-[10px] font-bold text-slate-400 uppercase">Ngày gán</span>
            <input type="date" v-model="selectedScheduleDate" class="px-3 py-1.5 border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 rounded-lg outline-none" />
          </div>
          <div class="space-y-1">
            <span class="text-[10px] font-bold text-slate-400 uppercase">Chọn Ca gán</span>
            <select v-model="selectedScheduleShift" class="px-3 py-1.5 border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 rounded-lg outline-none">
              <option v-for="s in mockDB.shifts" :key="s.id" :value="s.id">{{ s.name }}</option>
              <option value="">Nghỉ</option>
            </select>
          </div>
        </div>

        <!-- Schedule Assign Grid -->
        <div class="overflow-x-auto rounded-xl border border-slate-200 dark:border-slate-800">
          <table class="w-full text-left text-xs">
            <thead class="bg-slate-50 dark:bg-slate-850 text-slate-500 uppercase font-bold text-[10px] tracking-wider border-b border-slate-155 dark:border-slate-800">
              <tr>
                <th class="px-5 py-3">Mã NV</th>
                <th class="px-5 py-3">Nhân viên</th>
                <th class="px-5 py-3">Phòng ban</th>
                <th class="px-5 py-3 text-center">Ca hiện tại (Ngày {{ selectedScheduleDate }})</th>
                <th class="px-5 py-3 text-right no-print">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 dark:divide-slate-800 font-semibold text-slate-700 dark:text-slate-300">
              <tr v-for="emp in mockDB.employees.filter(e => e.status === 'Active')" :key="emp.id" class="hover:bg-slate-50/20">
                <td class="px-5 py-3 font-mono font-bold">{{ emp.employeeCode }}</td>
                <td class="px-5 py-3 font-bold text-slate-900 dark:text-slate-100">{{ emp.fullName }}</td>
                <td class="px-5 py-3 text-slate-500">{{ mockDB.departments.find(d => d.id === emp.departmentId)?.name }}</td>
                <td class="px-5 py-3 text-center">
                  <span class="inline-block px-2 py-0.5 rounded text-[10px] font-bold border bg-slate-50 dark:bg-slate-950 border-slate-200 dark:border-slate-800 text-slate-700 dark:text-slate-350">
                    {{ getShiftNameForEmpDate(emp.id, selectedScheduleDate) }}
                  </span>
                </td>
                <td class="px-5 py-3 text-right no-print">
                  <button @click="assignShiftToEmployee(emp.id)" class="px-3 py-1 bg-blue-600 hover:bg-blue-700 text-white rounded text-[11px] font-bold shadow-2xs cursor-pointer">Gán ca</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- 5. VIEW TAB: SHIFTS CONFIG -->
    <div v-if="activeTab === 'shifts'" class="grid gap-6 md:grid-cols-2">
      <!-- Add shift form -->
      <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs space-y-4">
        <div class="border-b border-slate-100 dark:border-slate-800 pb-2.5">
          <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Tạo ca làm việc</h2>
        </div>

        <div class="space-y-4 text-xs font-semibold text-slate-800 dark:text-slate-200">
          <div class="space-y-1">
            <label class="text-[10px] font-bold text-slate-400 uppercase">Tên ca làm việc</label>
            <input v-model="shiftForm.name" type="text" placeholder="e.g. Ca Đêm (22:00 - 6:00)" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 outline-none focus:border-blue-500" />
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase">Giờ Check-in</label>
              <input v-model="shiftForm.startTime" type="time" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100" />
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase">Giờ Check-out</label>
              <input v-model="shiftForm.endTime" type="time" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100" />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase">Số phút trễ cho phép (Grace)</label>
              <input v-model="shiftForm.lateGraceMinutes" type="number" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 font-mono font-bold" />
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase">Màu hiển thị</label>
              <input v-model="shiftForm.color" type="color" class="w-full h-8 p-1 border border-slate-200 dark:border-slate-800 rounded-lg bg-white dark:bg-slate-900 cursor-pointer" />
            </div>
          </div>

          <button @click="saveShift" class="w-full py-2 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-lg text-xs shadow-sm cursor-pointer">Tạo ca làm</button>
        </div>
      </div>

      <!-- List shifts -->
      <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs space-y-4">
        <div>
          <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Danh sách ca hoạt động</h2>
        </div>

        <div class="space-y-2 max-h-[300px] overflow-y-auto pr-1">
          <div v-for="s in mockDB.shifts" :key="s.id" class="p-3 border border-slate-100 dark:border-slate-800 rounded-xl hover:bg-slate-50 dark:hover:bg-slate-850/30 flex items-center justify-between text-xs">
            <div class="flex items-center gap-2">
              <span class="size-3.5 rounded-full" :style="{ backgroundColor: s.color }"></span>
              <div>
                <div class="font-bold text-slate-900 dark:text-slate-100">{{ s.name }}</div>
                <div class="text-[10px] text-slate-400 font-mono mt-0.5">Giờ: {{ s.startTime }} - {{ s.endTime }}</div>
              </div>
            </div>
            <span class="bg-slate-100 dark:bg-slate-800 text-[10px] font-semibold text-slate-600 dark:text-slate-350 px-2 py-0.5 rounded font-mono">Grace: {{ s.lateGraceMinutes }}m</span>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>
