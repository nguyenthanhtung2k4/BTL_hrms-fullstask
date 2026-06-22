<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { attendanceService } from '../../../services/attendance.service'
import { useToastStore } from '../../../stores/toast'
import type { AttendanceRecord } from '../../../types/attendance.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'

const toast = useToastStore()
const records = ref<AttendanceRecord[]>([])
const loading = ref(false)

const filterMonth = ref(new Date().getMonth() + 1)
const filterYear = new Date().getFullYear()
const filterStatus = ref('')
const search = ref('')

const columns = [
  { key: 'workDate', label: 'Ngày làm việc' },
  { key: 'shiftName', label: 'Ca làm việc' },
  { key: 'checkInAt', label: 'Giờ check-in' },
  { key: 'checkOutAt', label: 'Giờ check-out' },
  { key: 'workedMinutes', label: 'Tổng giờ làm' },
  { key: 'status', label: 'Trạng thái' },
]

const months = Array.from({ length: 12 }, (_, i) => ({ value: i + 1, label: `Tháng ${i + 1}` }))

async function load() {
  loading.value = true
  try {
    const year = filterYear
    const month = filterMonth.value
    const fromDate = `${year}-${String(month).padStart(2, '0')}-01`
    const toDate = `${year}-${String(month).padStart(2, '0')}-${new Date(year, month, 0).getDate()}`
    
    records.value = await attendanceService.getMyRecords({ fromDate, toDate })
  } catch {
    toast.error('Không thể tải lịch sử chấm công')
  } finally {
    loading.value = false
  }
}

function fmtTime(d?: string) {
  return d ? new Date(d).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '—'
}

function fmtDate(d: string) {
  return new Date(d).toLocaleDateString('vi-VN')
}

function fmtMin(m: number) {
  return m >= 60 ? `${Math.floor(m / 60)}h ${m % 60}m` : `${m}m`
}

const filteredRecords = computed(() => {
  let result = records.value
  
  if (filterStatus.value) {
    result = result.filter((r) => r.status === filterStatus.value)
  }
  
  if (search.value) {
    const q = search.value.toLowerCase()
    result = result.filter(
      (r) =>
        r.shiftName?.toLowerCase().includes(q) ||
        fmtDate(r.workDate).includes(q)
    )
  }
  
  return [...result].sort((a, b) => new Date(b.workDate).getTime() - new Date(a.workDate).getTime())
})

const { currentPage, perPage, paginatedData, total } = usePagination(filteredRecords)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader
      title="Chấm công của tôi"
      subtitle="Theo dõi lịch sử đi làm và thời gian chấm công cá nhân"
      :breadcrumbs="[{ label: 'Chấm công' }, { label: 'Chấm công của tôi' }]"
    />

    <!-- Thanh tìm kiếm & bộ lọc -->
    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4 mb-6 bg-slate-50 p-4 rounded-2xl border border-slate-150 shadow-sm">
      <!-- Tìm kiếm -->
      <div class="flex flex-col md:col-span-2">
        <label class="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-1.5">Tìm kiếm</label>
        <div class="relative">
          <input
            v-model="search"
            type="text"
            placeholder="Tìm kiếm theo ca làm hoặc ngày..."
            class="h-10 w-full rounded-xl border border-slate-300 bg-white px-3.5 pl-10 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100"
          />
          <div class="absolute inset-y-0 left-0 flex items-center pl-3.5 pointer-events-none text-slate-400">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" /></svg>
          </div>
        </div>
      </div>

      <!-- Trạng thái -->
      <div class="flex flex-col">
        <label class="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-1.5">Trạng thái</label>
        <div class="relative">
          <select
            v-model="filterStatus"
            class="h-10 w-full rounded-xl border border-slate-300 bg-white px-3 pr-10 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 appearance-none"
          >
            <option value="">Tất cả trạng thái</option>
            <option value="Completed">Hoàn thành</option>
            <option value="CheckedIn">Đang làm</option>
            <option value="Absent">Vắng mặt</option>
          </select>
          <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3.5 text-slate-400">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
          </div>
        </div>
      </div>

      <!-- Tháng -->
      <div class="flex flex-col">
        <label class="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-1.5">Tháng</label>
        <div class="relative">
          <select
            v-model="filterMonth"
            @change="load"
            class="h-10 w-full rounded-xl border border-slate-300 bg-white px-3 pr-10 text-sm outline-none transition-all focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 appearance-none"
          >
            <option v-for="m in months" :key="m.value" :value="m.value">{{ m.label }}</option>
          </select>
          <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3.5 text-slate-400">
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
          </div>
        </div>
      </div>
    </div>

    <!-- Bảng dữ liệu -->
    <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Không tìm thấy lịch sử chấm công phù hợp">
      <template #default="{ row }">
        <td class="px-5 py-4 text-sm font-semibold text-slate-900">{{ fmtDate((row as AttendanceRecord).workDate) }}</td>
        <td class="px-5 py-4 text-sm text-slate-700 font-medium">{{ (row as AttendanceRecord).shiftName ?? '—' }}</td>
        <td class="px-5 py-4 text-sm text-emerald-700 font-mono font-semibold">{{ fmtTime((row as AttendanceRecord).checkInAt) }}</td>
        <td class="px-5 py-4 text-sm text-blue-700 font-mono font-semibold">{{ fmtTime((row as AttendanceRecord).checkOutAt) }}</td>
        <td class="px-5 py-4 text-sm font-semibold text-slate-800">{{ (row as AttendanceRecord).workedMinutes > 0 ? fmtMin((row as AttendanceRecord).workedMinutes) : '—' }}</td>
        <td class="px-5 py-4">
          <span
            class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold border"
            :class="[
              (row as AttendanceRecord).status === 'Completed' ? 'bg-emerald-50 text-emerald-700 border-emerald-100' :
              (row as AttendanceRecord).status === 'CheckedIn' ? 'bg-amber-50 text-amber-700 border-amber-100' :
              'bg-slate-50 text-slate-600 border-slate-100'
            ]"
          >
            {{ (row as AttendanceRecord).status === 'Completed' ? 'Hoàn thành' : (row as AttendanceRecord).status === 'CheckedIn' ? 'Đang làm' : (row as AttendanceRecord).status }}
          </span>
        </td>
      </template>
    </AppTable>

    <!-- Phân trang -->
    <AppPagination
      :total="total"
      :current="currentPage"
      :per-page="perPage"
      @change="currentPage = $event"
      @per-page-change="perPage = $event"
    />
  </div>
</template>
