<script setup lang="ts">
import { ref, computed } from 'vue'
import { parseExcelFile, exportToExcel } from '../../../utils/excel'
import type { Shift } from '../../../types/attendance.types'
import type { Employee } from '../../../types/hr.types'
import AppButton from '../../../components/ui/AppButton.vue'
import { useToastStore } from '../../../stores/toast'
import { workScheduleService } from '../../../services/workSchedule.service'
import { X, Upload, Download, AlertTriangle, CheckCircle, FileSpreadsheet } from '@lucide/vue'

const props = defineProps<{
  isOpen: boolean
  employees: Employee[]
  shifts: Shift[]
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'imported'): void
}>()

const toast = useToastStore()
const fileInput = ref<HTMLInputElement | null>(null)
const uploadingFile = ref<File | null>(null)
const parsedRows = ref<any[]>([])
const validating = ref(false)
const errorMessage = ref('')
const importing = ref(false)

const validatedRows = computed(() => {
  if (parsedRows.value.length === 0) return []

  return parsedRows.value.map((row, idx) => {
    // Expected Excel Columns: MaNV, MaCa, NgayLamViec
    const employeeCode = (row['Mã NV'] || row.MaNV || row.manv || '').toString().trim()
    const shiftCode = (row['Mã Ca'] || row.MaCa || row.maca || '').toString().trim()
    const rawDate = row['Ngày Làm Việc'] || row.NgayLamViec || row.ngaylamviec || ''

    let errors: string[] = []
    let workDate = ''

    // 1. Validate and Parse Date
    if (!rawDate) {
      errors.push('Thiếu ngày làm việc')
    } else {
      try {
        // If rawDate is serial number from Excel, format it
        if (typeof rawDate === 'number') {
          const utc_days = Math.floor(rawDate - 25569)
          const utc_value = utc_days * 86400
          const date_info = new Date(utc_value * 1000)
          const y = date_info.getFullYear()
          const m = String(date_info.getMonth() + 1).padStart(2, '0')
          const d = String(date_info.getDate()).padStart(2, '0')
          workDate = `${y}-${m}-${d}`
        } else {
          // Standard string parse: Support YYYY-MM-DD or DD/MM/YYYY
          const str = rawDate.toString().trim()
          if (/^\d{4}-\d{2}-\d{2}$/.test(str)) {
            workDate = str
          } else if (/^\d{1,2}\/\d{1,2}\/\d{4}$/.test(str)) {
            const [d, m, y] = str.split('/')
            workDate = `${y}-${m.padStart(2, '0')}-${d.padStart(2, '0')}`
          } else {
            errors.push(`Ngày làm việc không hợp lệ: "${str}". Định dạng chuẩn: YYYY-MM-DD hoặc DD/MM/YYYY`)
          }
        }
      } catch {
        errors.push(`Không thể phân tích ngày làm việc: "${rawDate}"`)
      }
    }

    // 2. Validate Employee
    const employee = props.employees.find(
      (e) => e.employeeCode?.toLowerCase() === employeeCode.toLowerCase()
    )
    if (!employeeCode) {
      errors.push('Thiếu mã nhân viên')
    } else if (!employee) {
      errors.push(`Không tìm thấy nhân viên với mã: "${employeeCode}"`)
    } else if (employee.status !== 'Active') {
      errors.push(`Nhân viên "${employee.fullName}" (${employeeCode}) đã nghỉ việc (Trạng thái: ${employee.status})`)
    }

    // 3. Validate Shift
    const shift = props.shifts.find(
      (s) => s.code?.toLowerCase() === shiftCode.toLowerCase()
    )
    if (!shiftCode) {
      errors.push('Thiếu mã ca làm việc')
    } else if (!shift) {
      errors.push(`Không tìm thấy ca làm việc với mã: "${shiftCode}"`)
    } else if (!shift.isActive) {
      errors.push(`Ca làm việc "${shift.name}" (${shiftCode}) đã bị tắt kích hoạt`)
    }

    return {
      index: idx + 1,
      employeeCode,
      employeeName: employee?.fullName || 'Chưa rõ',
      employeeId: employee?.id || '',
      shiftCode,
      shiftName: shift?.name || 'Chưa rõ',
      shiftId: shift?.id || '',
      workDate,
      isValid: errors.length === 0,
      errors,
    }
  })
})

const hasErrors = computed(() => {
  return validatedRows.value.some((r) => !r.isValid)
})

function triggerFileSelect() {
  fileInput.value?.click()
}

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
    target.value = '' // Clear input
  }
}

function downloadTemplate() {
  const sampleEmployee = props.employees.find(e => e.status === 'Active')?.employeeCode || 'NV001'
  const sampleShift = props.shifts.find(s => s.isActive)?.code || 'HC'

  // Get current date formatted as YYYY-MM-DD
  const today = new Date()
  const y = today.getFullYear()
  const m = String(today.getMonth() + 1).padStart(2, '0')
  const d = String(today.getDate()).padStart(2, '0')
  const sampleDate = `${y}-${m}-${d}`

  const headers = [
    {
      'Mã NV': sampleEmployee,
      'Mã Ca': sampleShift,
      'Ngày Làm Việc': sampleDate
    },
    {
      'Mã NV': sampleEmployee,
      'Mã Ca': sampleShift,
      'Ngày Làm Việc': new Date(today.getTime() + 86400000).toISOString().split('T')[0] // Tomorrow
    }
  ]

  exportToExcel(headers, 'Mau_Nhap_Lich_Lam_Viec', 'Template')
  toast.success('Đã tải xuống file mẫu nhập lịch làm việc!')
}

async function submitImport() {
  if (hasErrors.value || validatedRows.value.length === 0) return
  importing.value = true
  try {
    const promises = validatedRows.value
      .filter((r) => r.isValid)
      .map((r) =>
        workScheduleService.create({
          employeeId: r.employeeId,
          shiftId: r.shiftId,
          workDate: r.workDate
        })
      )

    await Promise.all(promises)
    toast.success(`Nhập lịch thành công cho ${promises.length} ngày công!`)
    emit('imported')
    emit('close')
    reset()
  } catch (err: any) {
    toast.error(err?.response?.data?.message ?? 'Đã xảy ra lỗi khi tạo lịch từ Excel')
  } finally {
    importing.value = false
  }
}

function reset() {
  uploadingFile.value = null
  parsedRows.value = []
  errorMessage.value = ''
}
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 overflow-y-auto bg-black/50 backdrop-blur-sm">
    <div class="relative w-full max-w-4xl bg-white rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[85vh] border border-slate-150 animate-fadein">
      <!-- Header -->
      <div class="flex items-center justify-between px-6 py-5 border-b border-slate-100 bg-slate-50">
        <div>
          <h3 class="text-lg font-bold text-slate-800 flex items-center gap-2">
            <FileSpreadsheet class="h-5 w-5 text-emerald-600" />
            Nhập lịch làm việc từ Excel
          </h3>
          <p class="text-xs text-slate-500 mt-0.5">Tải lên bảng phân ca nhân viên hàng loạt để tiết kiệm thời gian</p>
        </div>
        <button @click="emit('close')" class="p-1.5 rounded-xl hover:bg-slate-150 text-slate-400 hover:text-slate-600 transition-all">
          <X class="h-5 w-5" />
        </button>
      </div>

      <!-- Content -->
      <div class="flex-1 overflow-y-auto p-6 space-y-6">
        <!-- Step 1: Upload File -->
        <div v-if="parsedRows.length === 0" class="space-y-4">
          <div 
            @click="triggerFileSelect"
            class="flex flex-col items-center justify-center border-2 border-dashed border-slate-200 rounded-2xl p-12 bg-slate-50 hover:bg-emerald-50/20 hover:border-emerald-500 transition-all duration-300 cursor-pointer group text-center"
          >
            <div class="p-4 bg-white rounded-2xl shadow-sm text-slate-400 group-hover:text-emerald-600 group-hover:shadow-md transition-all duration-300 mb-4 border border-slate-100">
              <Upload class="h-8 w-8" />
            </div>
            <span class="font-bold text-slate-700 text-base group-hover:text-emerald-700 transition-colors">Kéo thả file Excel (.xlsx) hoặc click để tải lên</span>
            <span class="text-xs text-slate-400 mt-1.5 font-medium">Định dạng file yêu cầu các cột: Mã NV, Mã Ca, Ngày Làm Việc</span>
            <input 
              ref="fileInput" 
              type="file" 
              accept=".xlsx, .xls" 
              class="hidden" 
              @change="handleFileUpload" 
            />
          </div>

          <div class="flex justify-between items-center bg-emerald-50/40 rounded-2xl p-4 border border-emerald-100">
            <div class="space-y-1">
              <h5 class="text-sm font-bold text-emerald-800">Tải file biểu mẫu</h5>
              <p class="text-xs text-emerald-600">Download file Excel mẫu để điền thông tin đúng định dạng dữ liệu hệ thống.</p>
            </div>
            <AppButton size="sm" variant="secondary" @click="downloadTemplate" class="flex items-center gap-1.5">
              <Download class="h-4 w-4" />
              <span>Tải file mẫu</span>
            </AppButton>
          </div>

          <p v-if="errorMessage" class="text-sm font-semibold text-rose-600 text-center bg-rose-50 p-3 rounded-xl border border-rose-100">{{ errorMessage }}</p>
        </div>

        <!-- Step 2: Validation Preview -->
        <div v-else class="space-y-4">
          <div class="flex items-center justify-between bg-slate-50 px-4 py-3 rounded-2xl border border-slate-150 shadow-sm text-sm">
            <div class="text-slate-600">
              File đang xử lý: <span class="font-bold text-slate-800">{{ uploadingFile?.name }}</span> 
              <span class="text-slate-400 mx-1.5">|</span>
              Tổng cộng: <span class="font-bold text-slate-800">{{ validatedRows.length }} dòng</span>
            </div>
            <AppButton size="sm" variant="secondary" @click="reset">Chọn file khác</AppButton>
          </div>

          <!-- Alert state -->
          <div 
            v-if="hasErrors" 
            class="p-4 bg-rose-50 border border-rose-200 text-rose-800 rounded-2xl flex items-start gap-3 text-sm"
          >
            <AlertTriangle class="h-5 w-5 text-rose-600 shrink-0 mt-0.5" />
            <div>
              <div class="font-bold">Hồ sơ chứa dòng lỗi dữ liệu!</div>
              <div class="mt-1 text-xs text-rose-600/90 font-medium">Bảng phân lịch của bạn chứa dữ liệu không hợp lệ. Vui lòng sửa lại các dòng màu đỏ trước khi xác nhận nhập.</div>
            </div>
          </div>
          <div 
            v-else 
            class="p-4 bg-emerald-50 border border-emerald-200 text-emerald-800 rounded-2xl flex items-start gap-3 text-sm"
          >
            <CheckCircle class="h-5 w-5 text-emerald-600 shrink-0 mt-0.5" />
            <div>
              <div class="font-bold">Kiểm tra thành công!</div>
              <div class="mt-1 text-xs text-emerald-600/90 font-medium">Tất cả dữ liệu đều chính xác. Click "Xác nhận nhập" để lưu lịch làm việc vào hệ thống.</div>
            </div>
          </div>

          <!-- Table preview -->
          <div class="border border-slate-150 rounded-2xl overflow-hidden shadow-sm bg-white">
            <div class="overflow-x-auto max-h-[45vh]">
              <table class="w-full text-sm text-left border-collapse">
                <thead class="bg-slate-50 border-b border-slate-150 text-slate-500 font-semibold uppercase tracking-wider text-[10px]">
                  <tr>
                    <th class="px-4 py-3 text-center w-12 border-r border-slate-150">STT</th>
                    <th class="px-4 py-3 border-r border-slate-150">Nhân viên</th>
                    <th class="px-4 py-3 border-r border-slate-150">Ca làm việc</th>
                    <th class="px-4 py-3 border-r border-slate-150">Ngày làm việc</th>
                    <th class="px-4 py-3">Kết quả kiểm tra</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-100 bg-white">
                  <tr 
                    v-for="row in validatedRows" 
                    :key="row.index" 
                    :class="[!row.isValid ? 'bg-rose-50/20' : 'hover:bg-slate-50/30']"
                  >
                    <td class="px-4 py-3 text-slate-400 text-center font-semibold border-r border-slate-100">{{ row.index }}</td>
                    <td class="px-4 py-3 border-r border-slate-100">
                      <div class="font-bold text-slate-800 text-xs">{{ row.employeeName }}</div>
                      <div class="text-[10px] text-slate-400 font-semibold">Mã NV: {{ row.employeeCode }}</div>
                    </td>
                    <td class="px-4 py-3 border-r border-slate-100">
                      <div class="font-bold text-slate-800 text-xs">{{ row.shiftName }}</div>
                      <div class="text-[10px] text-slate-400 font-semibold">Mã Ca: {{ row.shiftCode }}</div>
                    </td>
                    <td class="px-4 py-3 border-r border-slate-100 font-medium text-slate-700 text-xs">
                      {{ row.workDate ? new Date(row.workDate).toLocaleDateString('vi-VN') : '—' }}
                    </td>
                    <td class="px-4 py-3">
                      <div v-if="row.isValid" class="flex items-center gap-1 text-emerald-600 text-xs font-bold">
                        <CheckCircle class="h-4 w-4" />
                        <span>Hợp lệ</span>
                      </div>
                      <div v-else class="space-y-1">
                        <div v-for="err in row.errors" :key="err" class="text-xs font-semibold text-rose-600 flex items-start gap-1">
                          <span class="text-rose-400 shrink-0">•</span>
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
      <div class="flex items-center justify-end px-6 py-4 border-t border-slate-100 bg-slate-50 gap-3">
        <AppButton variant="secondary" @click="emit('close')">Đóng</AppButton>
        <AppButton 
          v-if="parsedRows.length > 0"
          :disabled="hasErrors || validatedRows.length === 0 || importing"
          :loading="importing"
          variant="primary"
          @click="submitImport"
        >
          Xác nhận nhập ({{ validatedRows.filter(r => r.isValid).length }} dòng)
        </AppButton>
      </div>
    </div>
  </div>
</template>

<style scoped>
@keyframes fadein {
  from { opacity: 0; transform: scale(0.98); }
  to   { opacity: 1; transform: scale(1); }
}
.animate-fadein { animation: fadein 0.15s ease-out; }
</style>
