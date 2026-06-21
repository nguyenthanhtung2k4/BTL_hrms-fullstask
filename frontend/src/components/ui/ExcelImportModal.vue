<script setup lang="ts">
import { ref, computed } from 'vue'
import { parseExcelFile, exportToExcel } from '../../utils/excel'
import type { PayrollPeriod } from '../../types/payroll.types'
import type { Employee } from '../../types/hr.types'
import AppButton from './AppButton.vue'

const props = defineProps<{
  isOpen: boolean
  title: string
  type: 'allowance' | 'deduction'
  periods: PayrollPeriod[]
  employees: Employee[]
  types: any[] // AllowanceType[] or DeductionType[]
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'import', data: any[]): void
}>()

const fileInput = ref<HTMLInputElement | null>(null)
const uploadingFile = ref<File | null>(null)
const parsedRows = ref<any[]>([])
const validating = ref(false)
const errorMessage = ref('')

// Computed list of validated rows with mapping details
const validatedRows = computed(() => {
  if (parsedRows.value.length === 0) return []

  return parsedRows.value.map((row, idx) => {
    // Expected Excel Columns: MaKyLuong, MaNhanVien, MaLoai, SoTien, GhiChu
    // Allow fallback if user writes slightly different casing
    const periodCode = (row.MaKyLuong || row.maKyLuong || '').toString().trim()
    const employeeCode = (row.MaNhanVien || row.maNhanVien || row.MaNV || row.maNV || '').toString().trim()
    const typeCode = (row.MaLoai || row.maLoai || row.MaLoaiPhuCap || row.MaLoaiKhauTru || '').toString().trim()
    const amount = Number(row.SoTien || row.soTien || row.Amount || row.amount || 0)
    const notes = (row.GhiChu || row.ghiChu || row.Notes || row.notes || '').toString().trim()

    let errors: string[] = []

    // 1. Resolve Period
    const period = props.periods.find(
      (p) => p.code.toLowerCase() === periodCode.toLowerCase() || p.name.toLowerCase() === periodCode.toLowerCase()
    )
    if (!periodCode) {
      errors.push('Thiếu mã kỳ lương')
    } else if (!period) {
      errors.push(`Không tìm thấy kỳ lương: "${periodCode}"`)
    } else if (period.status === 'Closed') {
      errors.push(`Kỳ lương "${period.name}" đã đóng, không thể nhập`)
    }

    // 2. Resolve Employee
    const employee = props.employees.find(
      (e) => e.employeeCode.toLowerCase() === employeeCode.toLowerCase()
    )
    if (!employeeCode) {
      errors.push('Thiếu mã nhân viên')
    } else if (!employee) {
      errors.push(`Không tìm thấy mã NV: "${employeeCode}"`)
    }

    // 3. Resolve Type
    const itemType = props.types.find(
      (t) => t.code.toLowerCase() === typeCode.toLowerCase() || t.name.toLowerCase() === typeCode.toLowerCase()
    )
    if (!typeCode) {
      errors.push(`Thiếu mã loại ${props.type === 'allowance' ? 'phụ cấp' : 'khấu trừ'}`)
    } else if (!itemType) {
      errors.push(`Không tìm thấy loại: "${typeCode}"`)
    }

    // 4. Validate Amount
    if (isNaN(amount) || amount <= 0) {
      errors.push('Số tiền phải là số lớn hơn 0')
    }

    return {
      index: idx + 1,
      periodCode,
      periodName: period?.name || 'Chưa rõ',
      payrollPeriodId: period?.id || '',
      employeeCode,
      employeeName: employee?.fullName || 'Chưa rõ',
      employeeId: employee?.id || '',
      typeCode,
      typeName: itemType?.name || 'Chưa rõ',
      typeId: itemType?.id || '',
      amount,
      notes,
      isValid: errors.length === 0,
      errors,
    }
  })
})

const hasErrors = computed(() => {
  return validatedRows.value.some((r) => !r.isValid)
})

// Trigger file open dialog
function triggerFileSelect() {
  fileInput.value?.click()
}

// Read Excel file
async function handleFileUpload(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  uploadingFile.value = file
  validating.value = true
  errorMessage.value = ''
  parsedRows.value = []

  try {
    const data = await parseExcelFile(file)
    if (data.length === 0) {
      errorMessage.value = 'File Excel rỗng hoặc không đúng định dạng.'
    } else {
      parsedRows.value = data
    }
  } catch (err) {
    console.error(err)
    errorMessage.value = 'Đã có lỗi xảy ra khi đọc file Excel.'
  } finally {
    validating.value = false
  }
}

// Download Sample Template
function downloadTemplate() {
  const samplePeriod = props.periods[0]?.code || 'JUN-2026'
  const sampleEmployee = props.employees[0]?.employeeCode || 'NV001'
  const sampleType = props.types[0]?.code || 'SAMPLE_CODE'

  const headers = {
    MaKyLuong: samplePeriod,
    MaNhanVien: sampleEmployee,
    MaLoai: sampleType,
    SoTien: 500000,
    GhiChu: 'Nhập mẫu dữ liệu'
  }

  const fileName = `Mau_Nhap_${props.type === 'allowance' ? 'Phu_Cap' : 'Khau_Tru'}`
  exportToExcel([headers], fileName, 'Template')
}

// Submit Import
function submitImport() {
  if (hasErrors.value || validatedRows.value.length === 0) return
  emit('import', validatedRows.value)
}

function reset() {
  uploadingFile.value = null
  parsedRows.value = []
  errorMessage.value = ''
}
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 overflow-y-auto bg-black/50 backdrop-blur-sm">
    <div class="relative w-full max-w-4xl bg-white rounded-2xl shadow-xl overflow-hidden flex flex-col max-h-[85vh]">
      <!-- Header -->
      <div class="flex items-center justify-between px-6 py-4 border-b border-slate-100 bg-slate-50">
        <div>
          <h3 class="text-lg font-bold text-slate-800">{{ title }}</h3>
          <p class="text-xs text-slate-500 mt-0.5">Hỗ trợ định dạng file .xlsx và .xls</p>
        </div>
        <button @click="emit('close')" class="text-slate-400 hover:text-slate-600 transition-colors">
          <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <!-- Content -->
      <div class="flex-1 overflow-y-auto p-6 space-y-6">
        <!-- Step 1: Upload File -->
        <div v-if="parsedRows.length === 0" class="space-y-4">
          <div 
            @click="triggerFileSelect"
            class="flex flex-col items-center justify-center border-2 border-dashed border-slate-300 rounded-2xl p-10 bg-slate-50 hover:bg-emerald-50/30 hover:border-emerald-500 transition-all duration-300 cursor-pointer group text-center"
          >
            <div class="p-4 bg-white rounded-full shadow-sm text-slate-400 group-hover:text-emerald-600 group-hover:shadow-md transition-all duration-300 mb-4">
              <svg class="h-10 w-10" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 13h6m-3-3v6m-9 1V4a2 2 0 012-2h6l2 2h6a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2z" />
              </svg>
            </div>
            <span class="font-bold text-slate-700 text-base group-hover:text-emerald-700 transition-colors">Kéo thả file Excel hoặc click để tải lên</span>
            <span class="text-xs text-slate-500 mt-2">Dung lượng tối đa: 10MB</span>
            <input 
              ref="fileInput" 
              type="file" 
              accept=".xlsx, .xls" 
              class="hidden" 
              @change="handleFileUpload" 
            />
          </div>

          <div class="flex justify-between items-center bg-emerald-50/50 rounded-xl p-4 border border-emerald-100">
            <div class="space-y-1">
              <h5 class="text-sm font-bold text-emerald-800">Bạn chưa có file mẫu?</h5>
              <p class="text-xs text-emerald-600">Tải xuống file Excel mẫu có chứa sẵn cấu trúc cột và dữ liệu nháp chuẩn.</p>
            </div>
            <AppButton size="sm" variant="secondary" @click="downloadTemplate">
              <span class="flex items-center space-x-1">
                <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                </svg>
                <span>Tải file mẫu</span>
              </span>
            </AppButton>
          </div>

          <p v-if="errorMessage" class="text-sm font-semibold text-rose-600 text-center">{{ errorMessage }}</p>
        </div>

        <!-- Step 2: Validation Preview -->
        <div v-else class="space-y-4">
          <div class="flex items-center justify-between bg-slate-50 p-3 rounded-lg border border-slate-100">
            <div class="text-sm text-slate-600">
              File: <span class="font-bold text-slate-800">{{ uploadingFile?.name }}</span> 
              ({{ validatedRows.length }} dòng dữ liệu)
            </div>
            <AppButton size="sm" variant="secondary" @click="reset">Chọn file khác</AppButton>
          </div>

          <!-- Alert State -->
          <div 
            v-if="hasErrors" 
            class="p-4 bg-rose-50 border border-rose-200 text-rose-800 rounded-xl flex items-start space-x-3 text-sm"
          >
            <svg class="h-5 w-5 text-rose-600 mt-0.5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
            <div>
              <div class="font-bold">Không thể nhập dữ liệu!</div>
              <div class="mt-1">Vui lòng kiểm tra và sửa lại các lỗi được đánh dấu màu đỏ trong bảng bên dưới.</div>
            </div>
          </div>
          <div 
            v-else 
            class="p-4 bg-emerald-50 border border-emerald-200 text-emerald-800 rounded-xl flex items-start space-x-3 text-sm"
          >
            <svg class="h-5 w-5 text-emerald-600 mt-0.5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
              <div class="font-bold">Kiểm tra hợp lệ thành công!</div>
              <div class="mt-1">Tất cả các dòng dữ liệu đều hợp lệ. Nhấp "Xác nhận nhập" để lưu vào cơ sở dữ liệu.</div>
            </div>
          </div>

          <!-- Data Grid Validation -->
          <div class="border border-slate-200 rounded-xl overflow-hidden">
            <div class="overflow-x-auto">
              <table class="w-full text-sm text-left">
                <thead class="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider text-[11px]">
                  <tr>
                    <th class="px-4 py-3 text-center w-12">STT</th>
                    <th class="px-4 py-3">Kỳ lương</th>
                    <th class="px-4 py-3">Nhân viên</th>
                    <th class="px-4 py-3">Phân loại</th>
                    <th class="px-4 py-3 text-right">Số tiền</th>
                    <th class="px-4 py-3">Ghi chú</th>
                    <th class="px-4 py-3">Trạng thái</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-100 bg-white">
                  <tr 
                    v-for="row in validatedRows" 
                    :key="row.index" 
                    :class="[!row.isValid ? 'bg-rose-50/30' : 'hover:bg-slate-50/50']"
                  >
                    <td class="px-4 py-3 text-slate-400 text-center font-medium">{{ row.index }}</td>
                    <td class="px-4 py-3">
                      <div class="font-medium text-slate-800">{{ row.periodName }}</div>
                      <div class="text-[10px] text-slate-500">Mã: {{ row.periodCode }}</div>
                    </td>
                    <td class="px-4 py-3">
                      <div class="font-medium text-slate-800">{{ row.employeeName }}</div>
                      <div class="text-[10px] text-slate-500">Mã: {{ row.employeeCode }}</div>
                    </td>
                    <td class="px-4 py-3">
                      <div class="font-medium text-slate-800">{{ row.typeName }}</div>
                      <div class="text-[10px] text-slate-500">Mã: {{ row.typeCode }}</div>
                    </td>
                    <td class="px-4 py-3 text-right font-semibold text-slate-800">
                      {{ row.amount.toLocaleString('vi-VN') }} ₫
                    </td>
                    <td class="px-4 py-3 text-slate-500 text-xs italic max-w-[150px] truncate" :title="row.notes">
                      {{ row.notes || '—' }}
                    </td>
                    <td class="px-4 py-3">
                      <div v-if="row.isValid" class="flex items-center space-x-1 text-emerald-600 text-xs font-semibold">
                        <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 13l4 4L19 7" />
                        </svg>
                        <span>Hợp lệ</span>
                      </div>
                      <div v-else class="space-y-1 max-w-[200px]">
                        <div v-for="err in row.errors" :key="err" class="text-xs font-semibold text-rose-600 flex items-start space-x-1">
                          <span class="text-rose-500 mr-0.5">•</span>
                          <span>{{ err }}</span>
                        </div>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="flex items-center justify-end px-6 py-4 border-t border-slate-100 bg-slate-50 space-x-3">
        <AppButton variant="secondary" @click="emit('close')">Đóng</AppButton>
        <AppButton 
          v-if="parsedRows.length > 0"
          :disabled="hasErrors || validatedRows.length === 0"
          variant="primary"
          @click="submitImport"
        >
          Xác nhận nhập ({{ validatedRows.filter(r => r.isValid).length }} dòng)
        </AppButton>
      </div>
    </div>
  </div>
</template>
