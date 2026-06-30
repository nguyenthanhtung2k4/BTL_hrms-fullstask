<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { payslipService } from '../../../services/payslip.service'
import { payrollPeriodService } from '../../../services/payrollPeriod.service'
import { useToastStore } from '../../../stores/toast'
import type { Payslip } from '../../../types/payroll.types'
import type { PayrollPeriod } from '../../../types/payroll.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import { useRouter } from 'vue-router'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'
import { exportToExcel } from '../../../utils/excel'

const toast = useToastStore()
const router = useRouter()
const payslips = ref<Payslip[]>([])
const periods = ref<PayrollPeriod[]>([])
const loading = ref(false)

const filterPeriod = ref('')
const searchQuery = ref('') // Biến mới: Chứa từ khóa tìm kiếm Tên/Mã NV

// Bổ sung thêm cột Trạng thái vào bảng
const columns = [
  { key: 'period', label: 'Kỳ lương' },
  { key: 'employee', label: 'Nhân viên' },
  { key: 'code', label: 'Mã NV' },
  { key: 'work', label: 'Ngày công' },
  { key: 'gross', label: 'Gross' },
  { key: 'net', label: 'Net lương' },
  { key: 'status', label: 'Trạng thái' }, // <--- Đã thêm cột này
  { key: 'actions', label: '' },
]

async function load() {
  loading.value = true
  try {
    // Không cần load danh sách employees nữa để tối ưu hiệu năng
    [payslips.value, periods.value] = await Promise.all([
      payslipService.getAll({ periodId: filterPeriod.value || undefined }),
      payrollPeriodService.getAll()
    ])
  } catch { 
    toast.error('Không thể tải dữ liệu') 
  }
  finally { 
    loading.value = false 
  }
}

// Logic lọc dữ liệu Real-time trên Frontend khi gõ từ khóa
const filteredPayslips = computed(() => {
  if (!searchQuery.value) return payslips.value;
  
  const query = searchQuery.value.toLowerCase();
  return payslips.value.filter(p => 
    (p.fullName && p.fullName.toLowerCase().includes(query)) ||
    (p.employeeCode && p.employeeCode.toLowerCase().includes(query))
  );
})

function fmtMoney(n: number) { return n.toLocaleString('vi-VN') + ' ₫' }
function formatPeriod(name?: string) {
  if (!name) return 'Kỳ lương không rõ'
  const clean = name.replace(/Luong\s+thang/gi, 'Tháng').replace(/Ky\s+luong/gi, 'Kỳ lương')
  return clean.charAt(0).toUpperCase() + clean.slice(1)
}

function handleExport() {
  try {
    // Xuất ra Excel dựa trên danh sách đã lọc thay vì toàn bộ
    const dataToExport = filteredPayslips.value.map(item => ({
      'Kỳ lương': formatPeriod(item.periodName) || '',
      'Mã Nhân viên': item.employeeCode || '',
      'Họ tên Nhân viên': item.fullName || '',
      'Lương cơ bản (VNĐ)': item.baseSalary,
      'Ngày công': item.workedDays,
      'Phép hưởng lương': item.paidLeaveDays,
      'Thu nhập Gross (VNĐ)': item.grossSalary,
      'Khấu trừ (VNĐ)': item.totalDeduction,
      'Thực lĩnh Net (VNĐ)': item.netSalary,
      'Trạng thái': item.status === 'Draft' ? 'Chưa chi trả (Nháp)' : item.status === 'Paid' ? 'Đã chi trả' : 'Đã chốt'
    }))
    exportToExcel(dataToExport, 'Danh_Sach_Phieu_Luong_Nhan_Vien', 'PhieuLuong')
    toast.success('Đã xuất báo cáo phiếu lương ra Excel thành công')
  } catch (err: any) {
    toast.error(err?.message || 'Không thể xuất file Excel')
  }
}

// Đẩy mảng đã lọc qua hàm phân trang
const { currentPage, perPage, paginatedData, total } = usePagination(filteredPayslips)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Phiếu lương" subtitle="Toàn bộ phiếu lương trong hệ thống" :breadcrumbs="[{ label: 'Lương & Báo cáo' }, { label: 'Phiếu lương' }]">
      <template #actions>
        <AppButton variant="secondary" @click="handleExport">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
          </svg>
          <span>Xuất Excel</span>
        </AppButton>
      </template>
    </PageHeader>

    <div class="mb-4 flex gap-3 flex-wrap">
      <select v-model="filterPeriod" class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500" @change="load">
        <option value="">Tất cả kỳ lương</option>
        <option v-for="p in periods" :key="p.id" :value="p.id">{{ p.name }}</option>
      </select>
      
      <!-- Đã thay đổi: Chuyển Dropdown thành Input tìm kiếm -->
      <div class="relative">
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="Tìm theo Mã NV hoặc Tên..." 
          class="h-9 w-64 rounded-lg border border-slate-300 px-3 py-2 text-sm bg-white outline-none focus:border-emerald-500"
        />
        <svg v-if="!searchQuery" class="absolute right-3 top-2.5 h-4 w-4 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
      </div>

      <button class="h-9 rounded-lg bg-emerald-600 px-4 text-sm font-medium text-white hover:bg-emerald-700" @click="load">Tải lại</button>
    </div>

    <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Chưa có phiếu lương nào">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm text-slate-500">{{ formatPeriod((row as Payslip).periodName) }}</td>
        <td class="px-4 py-3 text-sm font-medium">{{ (row as Payslip).fullName }}</td>
        <td class="px-4 py-3 text-sm text-slate-500">{{ (row as Payslip).employeeCode }}</td>
        <td class="px-4 py-3 text-sm font-semibold text-emerald-700">{{ (row as Payslip).workedDays?.toFixed(1) }}</td>
        <td class="px-4 py-3 text-sm">{{ fmtMoney((row as Payslip).grossSalary) }}</td>
        <td class="px-4 py-3 text-sm font-bold text-emerald-700">{{ fmtMoney((row as Payslip).netSalary) }}</td>
        
        <!-- Đã thêm: Hiển thị Badge trạng thái -->
        <td class="px-4 py-3">
          <span :class="[
            'inline-flex items-center rounded-full px-2 py-1 text-[11px] font-medium',
            (row as Payslip).status === 'Draft' ? 'bg-amber-100 text-amber-700' :
            (row as Payslip).status === 'Paid' ? 'bg-emerald-100 text-emerald-700' : 'bg-blue-100 text-blue-700'
          ]">
            {{ (row as Payslip).status === 'Draft' ? 'Nháp (Chưa chi)' : (row as Payslip).status === 'Paid' ? 'Đã chi trả' : 'Đã chốt' }}
          </span>
        </td>

        <td class="px-4 py-3">
          <AppButton size="sm" variant="ghost" @click="router.push(`/payroll/payslips/${(row as Payslip).id}`)">Chi tiết</AppButton>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />
  </div>
</template>