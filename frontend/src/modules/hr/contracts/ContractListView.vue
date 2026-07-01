<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { contractService } from '../../../services/contract.service'
import { employeeService } from '../../../services/employee.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { Contract, Employee, CreateContractDto, UpdateContractDto } from '../../../types/hr.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'
import AppModal from '../../../components/ui/AppModal.vue'
import AppInput from '../../../components/ui/AppInput.vue'
import AppConfirm from '../../../components/ui/AppConfirm.vue'
import { Eye, Download } from '@lucide/vue'
import { getAttachmentUrl } from '../../../services/apiClient'
const auth = useAuthStore()
const toast = useToastStore()

const contracts = ref<Contract[]>([])
const employees = ref<Employee[]>([])
const loading = ref(false)
const showForm = ref(false)
const editTarget = ref<Contract | null>(null)
const deleteTarget = ref<Contract | null>(null)
const deleteLoading = ref(false)
const saving = ref(false)
const filterEmployee = ref('')
const filterStatus = ref('')
const search = ref('')

const selectedFile = ref<File | null>(null)
const filePreviewUrl = ref<string | null>(null)
const existingFileUrl = ref<string | null>(null)

const form = ref({ contractNumber: '', employeeId: '', contractType: 'Chính thức', startDate: '', endDate: '', baseSalary: '', status: 'Active' })
const errors = ref<Record<string, string>>({})

const columns = [
  { key: 'no', label: 'Số HĐ' }, { key: 'employee', label: 'Nhân viên' }, { key: 'type', label: 'Loại HĐ' },
  { key: 'salary', label: 'Lương cơ bản' }, { key: 'start', label: 'Từ ngày' }, { key: 'end', label: 'Đến ngày' },
  { key: 'status', label: 'Trạng thái' }, { key: 'file', label: 'Tài liệu' }, { key: 'actions', label: '', class: 'text-right' },
]

function handleFileChange(event: Event) {
  const target = event.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    selectedFile.value = target.files[0]
    // Tạo preview URL nếu là ảnh
    if (selectedFile.value.type.startsWith('image/')) {
      filePreviewUrl.value = URL.createObjectURL(selectedFile.value)
    } else {
      filePreviewUrl.value = null
    }
  } else {
    selectedFile.value = null
    filePreviewUrl.value = null
  }
}

function removeSelectedFile() {
  // Giải phóng URL preview để tránh memory leak
  if (filePreviewUrl.value) {
    URL.revokeObjectURL(filePreviewUrl.value)
  }
  selectedFile.value = null
  filePreviewUrl.value = null
}

function removeExistingFile() {
  existingFileUrl.value = null
}

function downloadFile(url: string) {
  // Tạo thẻ a ẩn và click để tải
  const link = document.createElement('a')
  link.href = getAttachmentUrl(url)
  link.download = url.split('/').pop() || 'file'
  link.target = '_blank'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

const filtered = computed(() => {
  let list = contracts.value
  if (filterEmployee.value) list = list.filter((c) => c.employeeId === filterEmployee.value)
  if (filterStatus.value) list = list.filter((c) => c.status === filterStatus.value)
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter((c) =>
      c.contractNumber.toLowerCase().includes(q) ||
      c.employeeName?.toLowerCase().includes(q)
    )
  }
  return list
})

async function load() {
  loading.value = true
  try { [contracts.value, employees.value] = await Promise.all([contractService.getAll(), employeeService.getAll()]) }
  catch { toast.error('Không thể tải dữ liệu') }
  finally { loading.value = false }
}

function openCreate() {
  editTarget.value = null
  existingFileUrl.value = null
  editTarget.value = null; form.value = { contractNumber: '', employeeId: '', contractType: 'Chính thức', startDate: new Date().toISOString().split('T')[0], endDate: '', baseSalary: '', status: 'Active' }; errors.value = {}; showForm.value = true
}
function openEdit(c: Contract) {
  editTarget.value = c
  existingFileUrl.value = c.attachmentUrl || null // lưu URL file cũ
  form.value = {
    contractNumber: c.contractNumber,
    employeeId: c.employeeId,
    contractType: c.contractType,
    startDate: c.startDate.split('T')[0],
    endDate: c.endDate ? c.endDate.split('T')[0] : '',
    baseSalary: String(c.baseSalary),
    status: c.status
  }
  errors.value = {}
  showForm.value = true
}
function validate() {
  errors.value = {}
  if (!form.value.contractNumber.trim()) errors.value.contractNumber = 'Số HĐ bắt buộc'
  if (!form.value.employeeId) errors.value.employeeId = 'Nhân viên bắt buộc'
  if (!form.value.startDate) errors.value.startDate = 'Ngày bắt đầu bắt buộc'
  if (!form.value.baseSalary || isNaN(Number(form.value.baseSalary))) errors.value.baseSalary = 'Lương hợp lệ bắt buộc'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    let attachmentUrl = existingFileUrl.value
    if (selectedFile.value) {
      const uploadResult = await contractService.uploadAttachment(selectedFile.value);
      attachmentUrl = uploadResult.url;
    }

    if (editTarget.value) {
      const dto: UpdateContractDto = {
        contractType: form.value.contractType as any,
        startDate: form.value.startDate,
        endDate: form.value.endDate || undefined,
        baseSalary: Number(form.value.baseSalary),
        status: form.value.status as any,
        attachmentUrl: attachmentUrl // gửi null nếu đã xóa
      }
      await contractService.update(editTarget.value.id, dto)
      toast.success('Cập nhật hợp đồng thành công')
    } else {
      const dto: CreateContractDto = {
        contractNumber: form.value.contractNumber,
        employeeId: form.value.employeeId,
        contractType: form.value.contractType as any,
        startDate: form.value.startDate,
        endDate: form.value.endDate || undefined,
        baseSalary: Number(form.value.baseSalary),
        attachmentUrl: attachmentUrl
      }
      await contractService.create(dto)
      toast.success('Tạo hợp đồng thành công')
    }
    showForm.value = false
    // Reset file
    selectedFile.value = null
    filePreviewUrl.value = null
    existingFileUrl.value = null
    await load()
  } catch (err: any) {
    toast.error(err?.response?.data?.message ?? 'Lưu thất bại')
  } finally {
    saving.value = false
  }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleteLoading.value = true
  try { await contractService.delete(deleteTarget.value.id); toast.success('Đã xóa hợp đồng'); deleteTarget.value = null; await load() }
  catch { toast.error('Xóa thất bại') }
  finally { deleteLoading.value = false }
}

function fmt(d?: string) { return d ? new Date(d).toLocaleDateString('vi-VN') : '—' }
function fmtMoney(n: number) { return n.toLocaleString('vi-VN') + ' ₫' }

const { currentPage, perPage, paginatedData, total } = usePagination(filtered)

function closeModal() {
  showForm.value = false
  selectedFile.value = null
  if (filePreviewUrl.value) URL.revokeObjectURL(filePreviewUrl.value)
  filePreviewUrl.value = null
  existingFileUrl.value = null
}

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Hợp đồng" subtitle="Quản lý hợp đồng lao động"
      :breadcrumbs="[{ label: 'Nhân sự' }, { label: 'Hợp đồng' }]">
      <template #actions>
        <AppButton v-if="auth.isHR" @click="openCreate">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Thêm hợp đồng
        </AppButton>
      </template>
    </PageHeader>

    <div class="mb-4 flex gap-3 flex-wrap">
      <div class="relative">
        <input v-model="search" type="text" placeholder="Tìm theo tên NV, số HĐ..."
          class="h-9 rounded-lg border border-slate-300 bg-white px-3 pl-9 text-sm outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-100 min-w-[240px]" />
        <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none text-slate-400">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </div>
      </div>
      <select v-model="filterEmployee"
        class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500">
        <option value="">Tất cả nhân viên</option>
        <option v-for="e in employees" :key="e.id" :value="e.id">{{ e.fullName }}</option>
      </select>
      <select v-model="filterStatus"
        class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500">
        <option value="">Tất cả trạng thái</option>
        <option value="Active">Hiệu lực</option>
        <option value="Expired">Hết hạn</option>
        <option value="Terminated">Chấm dứt</option>
      </select>
    </div>

    <AppTable :page-size="10" :columns="columns" :rows="paginatedData" :loading="loading" row-key="id"
      empty-text="Chưa có hợp đồng nào">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-mono">{{ (row as Contract).contractNumber }}</td>
        <td class="px-4 py-3 text-sm font-medium">{{ (row as Contract).employeeName }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ (row as Contract).contractType }}</td>
        <td class="px-4 py-3 text-sm font-medium text-emerald-700">{{ fmtMoney((row as Contract).baseSalary) }}</td>
        <td class="px-4 py-3 text-sm text-slate-500">{{ fmt((row as Contract).startDate) }}</td>
        <td class="px-4 py-3 text-sm text-slate-500">{{ fmt((row as Contract).endDate) }}</td>
        <td class="px-4 py-3">
          <AppBadge :status="(row as Contract).status" />
        </td>
        <td class="px-4 py-3">
          <div v-if="(row as Contract).attachmentUrl" class="flex gap-2">
            <a 
              :href="getAttachmentUrl((row as Contract).attachmentUrl!)" 
              target="_blank" 
              class="inline-flex items-center justify-center p-1 text-emerald-600 hover:bg-emerald-50 rounded transition-colors"
              title="Xem trực tuyến"
            >
              <Eye class="h-4 w-4" />
            </a>
            <button 
              type="button" 
              @click="downloadFile((row as Contract).attachmentUrl!)"
              class="inline-flex items-center justify-center p-1 text-slate-500 hover:bg-slate-100 rounded transition-colors"
              title="Tải về"
            >
              <Download class="h-4 w-4" />
            </button>
          </div>
          <span v-else class="text-xs text-slate-400 font-normal italic">—</span>
        </td>
        <td class="px-4 py-3 text-right">
          <div class="flex justify-end gap-2">
            <AppButton v-if="auth.isHR" size="sm" variant="secondary" @click="openEdit(row as Contract)">Sửa</AppButton>
            <AppButton v-if="auth.isAdmin" size="sm" variant="danger" @click="deleteTarget = row as Contract">Xóa
            </AppButton>
          </div>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event"
      @per-page-change="perPage = $event" />

    <AppModal v-if="showForm" :title="editTarget ? 'Sửa hợp đồng' : 'Thêm hợp đồng'" size="lg" @close="closeModal">
      <div class="grid grid-cols-2 gap-4">
        <AppInput id="ct-no" v-model="form.contractNumber" label="Số hợp đồng" required :disabled="!!editTarget"
          :error="errors.contractNumber" />
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Nhân viên <span class="text-red-500">*</span></label>
          <select v-model="form.employeeId" :disabled="!!editTarget"
            :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none', errors.employeeId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn nhân viên --</option>
            <option v-for="e in employees" :key="e.id" :value="e.id">{{ e.fullName }} ({{ e.employeeCode }})</option>
          </select>
          <p v-if="errors.employeeId" class="text-xs text-red-500">{{ errors.employeeId }}</p>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Loại hợp đồng</label>
          <select v-model="form.contractType"
            class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500">
            <option>Chính thức</option>
            <option>Thử việc</option>
            <option>Part-time</option>
          </select>
        </div>
        <AppInput id="ct-salary" v-model="form.baseSalary" label="Lương cơ bản (₫)" type="number" required
          :error="errors.baseSalary" placeholder="VD: 15000000" />
        <AppInput id="ct-start" v-model="form.startDate" label="Từ ngày" type="date" required
          :error="errors.startDate" />
        <AppInput id="ct-end" v-model="form.endDate" label="Đến ngày" type="date" hint="Để trống nếu không thời hạn" />
        <div v-if="editTarget" class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Trạng thái</label>
          <select v-model="form.status"
            class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500">
            <option value="Active">Hiệu lực</option>
            <option value="Expired">Hết hạn</option>
            <option value="Terminated">Chấm dứt</option>
          </select>
        </div>
      </div>
      <div class="col-span-2">
        <label class="text-sm font-medium text-slate-700">File đính kèm</label>
        <input type="file" @change="handleFileChange" accept=".pdf,.jpg,.jpeg,.png,.doc,.docx" class="mt-1 block w-full text-sm text-slate-500
      file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0
      file:text-sm file:font-semibold
      file:bg-emerald-50 file:text-emerald-700
      hover:file:bg-emerald-100" />

        <!-- File mới chọn -->
        <div v-if="selectedFile" class="mt-2 flex items-center gap-2">
          <span class="text-sm text-slate-600">
            Đã chọn: {{ selectedFile.name }} ({{ (selectedFile.size / 1024).toFixed(1) }} KB)
          </span>
          <button type="button" @click="removeSelectedFile" class="text-sm text-red-500 hover:text-red-700">
            ✕ Xóa
          </button>
        </div>

        <!-- Preview ảnh nếu có -->
        <div v-if="filePreviewUrl" class="mt-2">
          <img :src="filePreviewUrl" class="max-h-40 rounded border" alt="Preview" />
        </div>

        <!-- File cũ -->
        <div v-if="editTarget && existingFileUrl" class="mt-2 flex items-center gap-2">
          <a :href="getAttachmentUrl(existingFileUrl)" target="_blank" download class="text-sm text-emerald-600 hover:underline"
            @click.prevent="downloadFile(existingFileUrl)">
            📎 {{ existingFileUrl.split('/').pop() || 'File đã tải lên' }}
          </a>
          <button type="button" @click="removeExistingFile" class="text-sm text-red-500 hover:text-red-700">
            ✕ Xóa
          </button>
        </div>
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="showForm = false">Hủy</AppButton>
        <AppButton :loading="saving" @click="save">{{ editTarget ? 'Cập nhật' : 'Tạo mới' }}</AppButton>
      </template>
    </AppModal>

    <AppConfirm v-if="deleteTarget" title="Xóa hợp đồng"
      :message="`Xóa hợp đồng &quot;${deleteTarget.contractNumber}&quot;?`" confirm-text="Xóa" :danger="true"
      :loading="deleteLoading" @confirm="confirmDelete" @cancel="deleteTarget = null" />
  </div>
</template>
