<script setup lang="ts">
import { ref, computed } from 'vue'
import { parseExcelFile, exportToExcel } from '../../../utils/excel'
import type { Department, Position } from '../../../types/hr.types'
import AppButton from '../../../components/ui/AppButton.vue'

const props = defineProps<{
  isOpen: boolean
  title: string
  departments: Department[]
  positions: Position[]
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
    const employeeCode = (row['Mã NV'] || row['maNV'] || row['Mã nhân viên'] || row['maNhanVien'] || '').toString().trim()
    const fullName = (row['Họ tên'] || row['hoTen'] || row['Tên'] || row['ten'] || row['FullName'] || '').toString().trim()
    const email = (row['Email'] || row['email'] || '').toString().trim()
    const phone = (row['SĐT'] || row['sdt'] || row['Phone'] || row['phone'] || '').toString().trim()
    const deptName = (row['Phòng ban'] || row['phongBan'] || row['Department'] || row['department'] || '').toString().trim()
    const posName = (row['Chức vụ'] || row['chucVu'] || row['Position'] || row['position'] || '').toString().trim()
    
    const rawHireDate = row['Ngày vào'] || row['ngayVao'] || row['HireDate'] || row['hireDate']
    const rawBirthDate = row['Ngày sinh'] || row['ngaySinh'] || row['DateOfBirth'] || row['dateOfBirth']
    const rawGender = (row['Giới tính'] || row['gioiTinh'] || row['Gender'] || row['gender'] || '').toString().trim()

    let errors: string[] = []

    // 1. Validate employee code, full name, email
    if (!employeeCode) {
      errors.push('Thiếu mã nhân viên')
    }
    if (!fullName) {
      errors.push('Thiếu họ và tên')
    }
    if (!email) {
      errors.push('Thiếu email')
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      errors.push('Email không đúng định dạng')
    }

    // 2. Resolve Department
    const dept = props.departments.find(
      (d) => d.name.toLowerCase() === deptName.toLowerCase()
    )
    if (!deptName) {
      errors.push('Thiếu phòng ban')
    } else if (!dept) {
      errors.push(`Không tìm thấy phòng ban: "${deptName}"`)
    }

    // 3. Resolve Position
    const pos = props.positions.find(
      (p) => p.name.toLowerCase() === posName.toLowerCase()
    )
    if (!posName) {
      errors.push('Thiếu chức vụ')
    } else if (!pos) {
      errors.push(`Không tìm thấy chức vụ: "${posName}"`)
    }

    // 4. Resolve Dates
    let hireDate = new Date().toISOString().split('T')[0]
    if (rawHireDate) {
      const d = new Date(rawHireDate)
      if (isNaN(d.getTime())) {
        errors.push(`Ngày vào không hợp lệ: "${rawHireDate}"`)
      } else {
        hireDate = d.toISOString().split('T')[0]
      }
    }

    let dateOfBirth: string | undefined = undefined
    if (rawBirthDate) {
      const d = new Date(rawBirthDate)
      if (isNaN(d.getTime())) {
        errors.push(`Ngày sinh không hợp lệ: "${rawBirthDate}"`)
      } else {
        dateOfBirth = d.toISOString().split('T')[0]
      }
    }

    const gender = rawGender === 'Nữ' ? 'Female' : (rawGender === 'Nam' ? 'Male' : 'Other')

    return {
      index: idx + 1,
      employeeCode,
      fullName,
      email,
      phone,
      departmentName: dept?.name || deptName || 'Chưa rõ',
      departmentId: dept?.id || '',
      positionName: pos?.name || posName || 'Chưa rõ',
      positionId: pos?.id || '',
      hireDate,
      dateOfBirth,
      gender,
      genderLabel: rawGender || 'Chưa rõ',
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
  const templateData = [
    {
      'Mã NV': 'NV001',
      'Họ tên': 'Nguyễn Văn A',
      'Email': 'nguyenvana@example.com',
      'SĐT': '0912345678',
      'Phòng ban': props.departments[0]?.name || 'Ban Giam Doc',
      'Chức vụ': props.positions[0]?.name || 'Chuyen Vien NS',
      'Ngày vào': '2026-07-01',
      'Ngày sinh': '1995-05-15',
      'Giới tính': 'Nam'
    },
    {
      'Mã NV': 'NV002',
      'Họ tên': 'Trần Thị B',
      'Email': 'tranthib@example.com',
      'SĐT': '0987654321',
      'Phòng ban': props.departments[0]?.name || 'Ban Giam Doc',
      'Chức vụ': props.positions[0]?.name || 'Chuyen Vien NS',
      'Ngày vào': '2026-07-02',
      'Ngày sinh': '1998-09-20',
      'Giới tính': 'Nữ'
    }
  ]

  const fileName = 'Mau_Nhap_Nhan_Vien'
  exportToExcel(templateData, fileName, 'Template')
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
  <Teleport to="body">
    <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 overflow-y-auto bg-black/50 backdrop-blur-sm">
    <div class="relative w-full max-w-5xl bg-white rounded-2xl shadow-xl overflow-hidden flex flex-col max-h-[85vh]">
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
                    <th class="px-4 py-3">Mã NV</th>
                    <th class="px-4 py-3">Họ tên / Email</th>
                    <th class="px-4 py-3">Phòng ban</th>
                    <th class="px-4 py-3">Chức vụ</th>
                    <th class="px-4 py-3">Thông tin khác</th>
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
                    <td class="px-4 py-3 font-mono text-slate-700 font-semibold">{{ row.employeeCode || '—' }}</td>
                    <td class="px-4 py-3">
                      <div class="font-medium text-slate-800">{{ row.fullName || '—' }}</div>
                      <div class="text-[10px] text-slate-500">{{ row.email || '—' }}</div>
                    </td>
                    <td class="px-4 py-3 text-slate-700">{{ row.departmentName }}</td>
                    <td class="px-4 py-3 text-slate-700">{{ row.positionName }}</td>
                    <td class="px-4 py-3 text-xs text-slate-500 space-y-0.5">
                      <div>SĐT: {{ row.phone || '—' }}</div>
                      <div>Vào: {{ row.hireDate }}</div>
                      <div>Phái: {{ row.genderLabel }}</div>
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
  </Teleport>
</template>
