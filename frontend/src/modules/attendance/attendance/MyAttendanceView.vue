<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { attendanceService } from '../../../services/attendance.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { AttendanceRecord } from '../../../types/attendance.types'
import AppButton from '../../../components/ui/AppButton.vue'

const auth = useAuthStore()
const toast = useToastStore()

const todayRecord = ref<AttendanceRecord | null>(null)
const history = ref<AttendanceRecord[]>([])
const loading = ref(true)
const actionLoading = ref(false)

const today = new Date()
const todayStr = today.toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })

const employeeId = computed(() => auth.employeeId ?? '')

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

async function load() {
  if (!employeeId.value) { loading.value = false; return }
  try {
    const myRecords = await attendanceService.getMyToday()
    todayRecord.value = Array.isArray(myRecords) && myRecords.length > 0 ? myRecords[myRecords.length - 1] : null
    history.value = Array.isArray(myRecords) ? myRecords.slice(0, 30) : []
  } catch { /* có thể chưa có record hôm nay */ }
  finally { loading.value = false }
}

async function doCheckIn() {
  if (!employeeId.value) { toast.error('Không tìm thấy thông tin nhân viên'); return }
  actionLoading.value = true
  try {
    await attendanceService.checkIn()
    toast.success('Check-in thành công!')
    await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Check-in thất bại') }
  finally { actionLoading.value = false }
}

async function doCheckOut() {
  if (!employeeId.value) return
  actionLoading.value = true
  try {
    await attendanceService.checkOut()
    toast.success('Check-out thành công!')
    await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Check-out thất bại') }
  finally { actionLoading.value = false }
}

function fmtTime(d?: string) { return d ? new Date(d).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '—' }
function fmtDate(d: string) { return new Date(d).toLocaleDateString('vi-VN') }
function fmtMinutes(m: number) { return m >= 60 ? `${Math.floor(m / 60)}h ${m % 60}m` : `${m}m` }

onMounted(load)
</script>

<template>
  <div class="max-w-2xl">
    <div class="mb-6">
      <h1 class="text-xl font-bold text-slate-900">Check-in / Check-out</h1>
      <p class="text-sm text-slate-500 mt-0.5 capitalize">{{ todayStr }}</p>
    </div>

    <!-- Status Card -->
    <div class="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm mb-6">
      <div v-if="loading" class="space-y-3">
        <div class="h-6 w-40 animate-pulse rounded bg-slate-200" />
        <div class="h-12 w-48 animate-pulse rounded bg-slate-200" />
      </div>

      <div v-else-if="!employeeId" class="text-center py-4 text-slate-500">
        <p>Bạn chưa được liên kết với hồ sơ nhân viên.</p>
        <p class="text-xs mt-1">Liên hệ HR để cập nhật.</p>
      </div>

      <div v-else>
        <!-- Check-in time -->
        <div class="flex items-center gap-6 mb-6">
          <div class="text-center">
            <div class="text-xs text-slate-500 mb-1">Giờ vào</div>
            <div class="text-2xl font-bold" :class="todayRecord?.checkInAt ? 'text-emerald-600' : 'text-slate-300'">
              {{ fmtTime(todayRecord?.checkInAt) }}
            </div>
          </div>
          <div class="text-slate-300 text-xl">→</div>
          <div class="text-center">
            <div class="text-xs text-slate-500 mb-1">Giờ ra</div>
            <div class="text-2xl font-bold" :class="todayRecord?.checkOutAt ? 'text-blue-600' : 'text-slate-300'">
              {{ fmtTime(todayRecord?.checkOutAt) }}
            </div>
          </div>
          <div v-if="checkStatus === 'checked_out'" class="ml-auto text-center">
            <div class="text-xs text-slate-500 mb-1">Tổng giờ làm</div>
            <div class="text-xl font-bold text-emerald-700">{{ fmtMinutes(workMinutes) }}</div>
          </div>
        </div>

        <!-- Status + action buttons -->
        <div class="flex items-center justify-between">
          <div>
            <span v-if="checkStatus === 'not_checked_in'" class="inline-flex items-center gap-1.5 rounded-full bg-slate-100 px-3 py-1 text-sm text-slate-600">
              <span class="h-2 w-2 rounded-full bg-slate-400" /> Chưa vào
            </span>
            <span v-else-if="checkStatus === 'checked_in'" class="inline-flex items-center gap-1.5 rounded-full bg-emerald-100 px-3 py-1 text-sm text-emerald-700 font-medium">
              <span class="h-2 w-2 rounded-full bg-emerald-500 animate-pulse" /> Đang làm việc
            </span>
            <span v-else class="inline-flex items-center gap-1.5 rounded-full bg-blue-100 px-3 py-1 text-sm text-blue-700 font-medium">
              <span class="h-2 w-2 rounded-full bg-blue-500" /> Hoàn thành hôm nay ✓
            </span>
          </div>

          <div class="flex gap-3">
            <AppButton
              v-if="checkStatus === 'not_checked_in'"
              size="lg"
              :loading="actionLoading"
              @click="doCheckIn"
            >
              <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1" /></svg>
              CHECK-IN
            </AppButton>
            <AppButton
              v-if="checkStatus === 'checked_in'"
              size="lg"
              variant="secondary"
              :loading="actionLoading"
              @click="doCheckOut"
            >
              <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" /></svg>
              CHECK-OUT
            </AppButton>
          </div>
        </div>
      </div>
    </div>

    <!-- History -->
    <div>
      <h2 class="text-base font-semibold text-slate-800 mb-3">Lịch sử chấm công gần đây</h2>
      <div class="rounded-xl border border-slate-200 bg-white overflow-hidden">
        <table class="w-full text-sm">
          <thead class="bg-slate-50 text-xs uppercase text-slate-500">
            <tr>
              <th class="px-4 py-3 text-left">Ngày</th>
              <th class="px-4 py-3 text-left">Giờ vào</th>
              <th class="px-4 py-3 text-left">Giờ ra</th>
              <th class="px-4 py-3 text-left">Tổng giờ</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr v-if="history.length === 0">
              <td colspan="4" class="px-4 py-8 text-center text-slate-400">Chưa có lịch sử</td>
            </tr>
            <tr v-for="r in history" :key="r.id" class="hover:bg-slate-50">
              <td class="px-4 py-3">{{ fmtDate(r.workDate) }}</td>
              <td class="px-4 py-3 text-emerald-700">{{ fmtTime(r.checkInAt) }}</td>
              <td class="px-4 py-3 text-blue-700">{{ fmtTime(r.checkOutAt) }}</td>
              <td class="px-4 py-3 font-medium">{{ r.workedMinutes > 0 ? fmtMinutes(r.workedMinutes) : '—' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
