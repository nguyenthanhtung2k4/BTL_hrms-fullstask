<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { leaveService } from '../../../services/leave.service'
import { useAuthStore } from '../../../stores/auth'
import { useToastStore } from '../../../stores/toast'
import type { LeaveRequest, LeaveType, CreateLeaveRequestDto } from '../../../types/attendance.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppTable from '../../../components/ui/AppTable.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'
import AppPagination from '../../../components/ui/AppPagination.vue'
import { usePagination } from '../../../composables/usePagination'
import AppModal from '../../../components/ui/AppModal.vue'
import AppInput from '../../../components/ui/AppInput.vue'
import AppConfirm from '../../../components/ui/AppConfirm.vue'

const auth = useAuthStore()
const toast = useToastStore()

const leaves = ref<LeaveRequest[]>([])
const leaveTypes = ref<LeaveType[]>([])
const loading = ref(false)
const showCreateForm = ref(false)
const approveTarget = ref<LeaveRequest | null>(null)
const rejectTarget = ref<LeaveRequest | null>(null)
const actionLoading = ref(false)
const saving = ref(false)
const filterStatus = ref('')

const form = ref({ leaveTypeId: '', fromDate: '', toDate: '', reason: '' })
const errors = ref<Record<string, string>>({})

const columns = [
  { key: 'employee', label: 'Nhân viên' }, { key: 'type', label: 'Loại nghỉ' },
  { key: 'from', label: 'Từ ngày' }, { key: 'to', label: 'Đến ngày' }, { key: 'days', label: 'Số ngày' },
  { key: 'reason', label: 'Lý do' }, { key: 'status', label: 'Trạng thái' }, { key: 'actions', label: '', class: 'text-right' },
]

const filtered = computed(() => filterStatus.value ? leaves.value.filter(l => l.status === filterStatus.value) : leaves.value)

const totalDays = computed(() => {
  if (!form.value.fromDate || !form.value.toDate) return 0
  const diff = new Date(form.value.toDate).getTime() - new Date(form.value.fromDate).getTime()
  return Math.max(0, Math.floor(diff / 86400000) + 1)
})

async function load() {
  loading.value = true
  try {
    [leaves.value, leaveTypes.value] = await Promise.all([
      leaveService.getAll(auth.isManager ? {} : { employeeId: auth.employeeId }),
      leaveService.getTypes(),
    ])
  } catch { toast.error('Không thể tải dữ liệu') }
  finally { loading.value = false }
}

function validate() {
  errors.value = {}
  if (!form.value.leaveTypeId) errors.value.leaveTypeId = 'Loại nghỉ bắt buộc'
  if (!form.value.fromDate) errors.value.fromDate = 'Từ ngày bắt buộc'
  if (!form.value.toDate) errors.value.toDate = 'Đến ngày bắt buộc'
  if (!form.value.reason.trim()) errors.value.reason = 'Lý do bắt buộc'
  return Object.keys(errors.value).length === 0
}

async function submitLeave() {
  if (!validate()) return
  if (!auth.employeeId) { toast.error('Bạn chưa được liên kết nhân viên'); return }
  saving.value = true
  try {
    const dto: CreateLeaveRequestDto = { leaveTypeId: form.value.leaveTypeId, fromDate: form.value.fromDate, toDate: form.value.toDate, reason: form.value.reason }
    await leaveService.create(dto)
    toast.success('Gửi đơn nghỉ phép thành công')
    showCreateForm.value = false
    form.value = { leaveTypeId: '', fromDate: '', toDate: '', reason: '' }
    await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Gửi thất bại') }
  finally { saving.value = false }
}

async function doApprove() {
  if (!approveTarget.value || !auth.employeeId) return
  actionLoading.value = true
  try {
    await leaveService.approve(approveTarget.value.id)
    toast.success('Đã duyệt đơn nghỉ phép')
    approveTarget.value = null; await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Duyệt thất bại') }
  finally { actionLoading.value = false }
}

async function doReject() {
  if (!rejectTarget.value || !auth.employeeId) return
  actionLoading.value = true
  try {
    await leaveService.reject(rejectTarget.value.id)
    toast.success('Đã từ chối đơn nghỉ phép')
    rejectTarget.value = null; await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Từ chối thất bại') }
  finally { actionLoading.value = false }
}

async function cancelLeave(l: LeaveRequest) {
  if (!auth.employeeId) return
  try {
    await leaveService.cancel(l.id)
    toast.success('Đã hủy đơn nghỉ phép')
    await load()
  } catch (err: any) { toast.error(err?.response?.data?.message ?? 'Hủy thất bại') }
}

function fmt(d: string) { return new Date(d).toLocaleDateString('vi-VN') }

const { currentPage, perPage, paginatedData, total } = usePagination(filtered)

onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Nghỉ phép" subtitle="Quản lý đơn xin nghỉ phép" :breadcrumbs="[{ label: 'Chấm công' }, { label: 'Nghỉ phép' }]">
      <template #actions>
        <AppButton @click="showCreateForm = true; form = { leaveTypeId: '', fromDate: '', toDate: '', reason: '' }; errors = {}">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
          Tạo đơn nghỉ phép
        </AppButton>
      </template>
    </PageHeader>

    <!-- Filter -->
    <div class="mb-4 flex gap-3">
      <select v-model="filterStatus" class="h-9 rounded-lg border border-slate-300 px-3 text-sm bg-white outline-none focus:border-emerald-500">
        <option value="">Tất cả trạng thái</option>
        <option value="Pending">Chờ duyệt</option>
        <option value="Approved">Đã duyệt</option>
        <option value="Rejected">Từ chối</option>
        <option value="Cancelled">Đã hủy</option>
      </select>
    </div>

    <AppTable :columns="columns" :rows="paginatedData" :loading="loading" row-key="id" empty-text="Không có đơn nghỉ phép nào">
      <template #default="{ row }">
        <td class="px-4 py-3 text-sm font-medium">{{ (row as LeaveRequest).employeeName }}</td>
        <td class="px-4 py-3 text-sm">{{ (row as LeaveRequest).leaveTypeName }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ fmt((row as LeaveRequest).fromDate) }}</td>
        <td class="px-4 py-3 text-sm text-slate-600">{{ fmt((row as LeaveRequest).toDate) }}</td>
        <td class="px-4 py-3 text-sm font-medium">{{ (row as LeaveRequest).totalDays }} ngày</td>
        <td class="px-4 py-3 text-sm text-slate-600 max-w-xs truncate">{{ (row as LeaveRequest).reason }}</td>
        <td class="px-4 py-3"><AppBadge :status="(row as LeaveRequest).status" /></td>
        <td class="px-4 py-3 text-right">
          <div class="flex justify-end gap-1.5">
            <!-- Manager/HR duyệt -->
            <template v-if="auth.isManager && (row as LeaveRequest).status === 'Pending'">
              <AppButton size="sm" variant="success" @click="approveTarget = row as LeaveRequest">Duyệt</AppButton>
              <AppButton size="sm" variant="danger" @click="rejectTarget = row as LeaveRequest">Từ chối</AppButton>
            </template>
            <!-- Employee hủy đơn của mình -->
            <AppButton
              v-if="(row as LeaveRequest).employeeId === auth.employeeId && (row as LeaveRequest).status === 'Pending'"
              size="sm"
              variant="ghost"
              @click="cancelLeave(row as LeaveRequest)"
            >Hủy</AppButton>
          </div>
        </td>
      </template>
    </AppTable>
    <AppPagination :total="total" :current="currentPage" :per-page="perPage" @change="currentPage = $event" @per-page-change="perPage = $event" />

    <!-- Create form modal -->
    <AppModal v-if="showCreateForm" title="Tạo đơn xin nghỉ phép" @close="showCreateForm = false">
      <div class="space-y-4">
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Loại nghỉ <span class="text-red-500">*</span></label>
          <select v-model="form.leaveTypeId" :class="['h-9 rounded-lg border px-3 text-sm bg-white outline-none', errors.leaveTypeId ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']">
            <option value="">-- Chọn loại nghỉ --</option>
            <option v-for="t in leaveTypes" :key="t.id" :value="t.id">{{ t.name }} {{ t.isPaid ? '(có lương)' : '(không lương)' }}</option>
          </select>
          <p v-if="errors.leaveTypeId" class="text-xs text-red-500">{{ errors.leaveTypeId }}</p>
        </div>
        <div class="grid grid-cols-2 gap-3">
          <AppInput id="lv-from" v-model="form.fromDate" label="Từ ngày" type="date" required :error="errors.fromDate" />
          <AppInput id="lv-to" v-model="form.toDate" label="Đến ngày" type="date" required :error="errors.toDate" />
        </div>
        <div v-if="totalDays > 0" class="rounded-lg bg-emerald-50 border border-emerald-200 px-3 py-2 text-sm text-emerald-700">
          Tổng: <strong>{{ totalDays }} ngày</strong>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700">Lý do <span class="text-red-500">*</span></label>
          <textarea v-model="form.reason" rows="3" :class="['w-full rounded-lg border px-3 py-2 text-sm outline-none', errors.reason ? 'border-red-400' : 'border-slate-300 focus:border-emerald-500']" placeholder="Nhập lý do xin nghỉ..." />
          <p v-if="errors.reason" class="text-xs text-red-500">{{ errors.reason }}</p>
        </div>
      </div>
      <template #footer>
        <AppButton variant="secondary" @click="showCreateForm = false">Hủy</AppButton>
        <AppButton :loading="saving" @click="submitLeave">Gửi đơn</AppButton>
      </template>
    </AppModal>

    <!-- Approve confirm -->
    <AppConfirm
      v-if="approveTarget"
      title="Duyệt đơn nghỉ phép"
      :message="`Duyệt đơn của &quot;${approveTarget.employeeName}&quot; (${approveTarget.totalDays} ngày)?`"
      confirm-text="Duyệt"
      :loading="actionLoading"
      @confirm="doApprove"
      @cancel="approveTarget = null"
    />

    <!-- Reject confirm -->
    <AppConfirm
      v-if="rejectTarget"
      title="Từ chối đơn nghỉ phép"
      :message="`Từ chối đơn của &quot;${rejectTarget.employeeName}&quot;?`"
      confirm-text="Từ chối"
      :danger="true"
      :loading="actionLoading"
      @confirm="doReject"
      @cancel="rejectTarget = null"
    />
  </div>
</template>

