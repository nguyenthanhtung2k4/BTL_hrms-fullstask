<script setup lang="ts">
import { ref, computed, reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Search,
  Plus,
  Edit,
  Eye,
  EyeOff,
  UserPlus,
  FileText,
  Layers,
  ShieldCheck,
  CheckCircle,
  XCircle,
  X,
  FileSpreadsheet,
  Briefcase
} from '@lucide/vue'
import { mockDB, hrService, type Employee } from '../../services/mockData'

const route = useRoute()
const router = useRouter()

// Sync active tab with router query param ?tab=
const activeTab = computed(() => {
  const queryTab = route.query.tab as string
  if (['employees', 'org', 'contracts', 'roles'].includes(queryTab)) {
    return queryTab
  }
  return 'employees'
})

function setTab(tabName: 'employees' | 'org' | 'contracts' | 'roles') {
  router.push({ path: '/hr', query: { tab: tabName } })
}

// Secure Data Toggles (PII Masking)
const revealedPII = ref<Record<string, boolean>>({})

function togglePII(id: string) {
  revealedPII.value[id] = !revealedPII.value[id]
}

function formatEmail(email: string, id: string) {
  if (revealedPII.value[id]) return email
  const [user, domain] = email.split('@')
  return `${user.slice(0, 2)}***@${domain}`
}

function formatPhone(phone: string, id: string) {
  if (revealedPII.value[id]) return phone
  return `${phone.slice(0, 3)}***${phone.slice(-3)}`
}

function formatVND(amount: number) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)
}

// Search & Filter state
const employeeSearch = ref('')
const selectedDeptFilter = ref('')
const selectedStatusFilter = ref('')

const filteredEmployees = computed(() => {
  return mockDB.employees.filter(e => {
    const matchesSearch = e.fullName.toLowerCase().includes(employeeSearch.value.toLowerCase()) || 
                          e.employeeCode.toLowerCase().includes(employeeSearch.value.toLowerCase())
    const matchesDept = !selectedDeptFilter.value || e.departmentId === selectedDeptFilter.value
    const matchesStatus = !selectedStatusFilter.value || e.status === selectedStatusFilter.value
    return matchesSearch && matchesDept && matchesStatus
  })
})

// Dialog States
const isEmpModalOpen = ref(false)
const editingEmpId = ref<string | null>(null)
const empForm = reactive({
  employeeCode: '',
  fullName: '',
  email: '',
  phone: '',
  departmentId: '',
  positionId: '',
  status: 'Active' as 'Active' | 'Inactive',
  joinedDate: ''
})

const isContractModalOpen = ref(false)
const contractForm = reactive({
  employeeId: '',
  contractNumber: '',
  startDate: '',
  endDate: '',
  salaryType: 'Fixed' as 'Fixed' | 'Hourly',
  baseSalary: 0
})

const activeDetailDrawerId = ref<string | null>(null)
const selectedEmployeeDetails = computed(() => {
  return mockDB.employees.find(e => e.id === activeDetailDrawerId.value)
})
const selectedEmployeeContract = computed(() => {
  if (!activeDetailDrawerId.value) return null
  return mockDB.contracts.find(c => c.employeeId === activeDetailDrawerId.value && c.status === 'Active')
})

// Org Add forms
const newDeptName = ref('')
const newDeptCode = ref('')
const newPosName = ref('')
const newPosCode = ref('')

// Roles management simulation state
const roleMatrix = reactive<Record<string, string[]>>({
  'emp-001': ['Employee'],
  'emp-002': ['Employee', 'Manager'],
  'emp-003': ['Employee', 'HR'],
  'emp-004': ['Employee', 'PayrollStaff'],
  'emp-admin': ['Admin', 'HR', 'Manager', 'Employee', 'PayrollStaff']
})

function toggleRoleInMatrix(empId: string, roleName: any) {
  if (!roleMatrix[empId]) {
    roleMatrix[empId] = []
  }
  const index = roleMatrix[empId].indexOf(roleName)
  if (index === -1) {
    roleMatrix[empId].push(roleName)
  } else {
    roleMatrix[empId].splice(index, 1)
  }
}

// Action triggers
function openAddEmpModal() {
  editingEmpId.value = null
  empForm.employeeCode = `HRMS-${Math.floor(100 + Math.random() * 900)}`
  empForm.fullName = ''
  empForm.email = ''
  empForm.phone = ''
  empForm.departmentId = mockDB.departments[0]?.id || ''
  empForm.positionId = mockDB.positions[0]?.id || ''
  empForm.status = 'Active'
  empForm.joinedDate = new Date().toISOString().split('T')[0]
  isEmpModalOpen.value = true
}

function openEditEmpModal(emp: Employee) {
  editingEmpId.value = emp.id
  empForm.employeeCode = emp.employeeCode
  empForm.fullName = emp.fullName
  empForm.email = emp.email
  empForm.phone = emp.phone
  empForm.departmentId = emp.departmentId
  empForm.positionId = emp.positionId
  empForm.status = emp.status
  empForm.joinedDate = emp.joinedDate
  isEmpModalOpen.value = true
}

function saveEmployee() {
  if (!empForm.fullName || !empForm.email) return
  if (editingEmpId.value) {
    hrService.updateEmployee(editingEmpId.value, { ...empForm })
  } else {
    hrService.addEmployee({ ...empForm })
  }
  isEmpModalOpen.value = false
}

function deleteEmployee(id: string) {
  hrService.toggleEmployeeStatus(id)
}

function openAddContractModal(empId: string = '') {
  contractForm.employeeId = empId || (mockDB.employees[0]?.id || '')
  contractForm.contractNumber = `HĐ-${new Date().getFullYear()}/${Math.floor(100 + Math.random() * 900)}`
  contractForm.startDate = new Date().toISOString().split('T')[0]
  const oneYearLater = new Date()
  oneYearLater.setFullYear(oneYearLater.getFullYear() + 1)
  contractForm.endDate = oneYearLater.toISOString().split('T')[0]
  contractForm.salaryType = 'Fixed'
  contractForm.baseSalary = 12000000
  isContractModalOpen.value = true
}

function saveContract() {
  if (!contractForm.contractNumber || contractForm.baseSalary <= 0) return
  hrService.addContract({ ...contractForm })
  isContractModalOpen.value = false
}

function saveDepartment() {
  if (!newDeptName.value || !newDeptCode.value) return
  hrService.addDepartment({ name: newDeptName.value, code: newDeptCode.value })
  newDeptName.value = ''
  newDeptCode.value = ''
}

function savePosition() {
  if (!newPosName.value || !newPosCode.value) return
  hrService.addPosition({ name: newPosName.value, code: newPosCode.value })
  newPosName.value = ''
  newPosCode.value = ''
}
</script>

<template>
  <div class="space-y-6 animate-fadeIn">
    <!-- Header of the module -->
    <div class="flex flex-col md:flex-row md:items-center justify-between border-b border-slate-200 dark:border-slate-800 pb-4">
      <div>
        <h1 class="text-xl font-bold text-slate-900 dark:text-slate-50">👥 HR Core Service (Quản lý Nhân sự)</h1>
        <p class="text-xs text-slate-500 dark:text-slate-400 mt-0.5">Phân hệ quản trị thông tin nhân sự, phòng ban, chức vụ và hợp đồng lao động.</p>
      </div>
      <div class="flex items-center gap-2 text-xs font-semibold text-slate-500 dark:text-slate-400">
        <span>Cơ sở dữ liệu:</span>
        <span class="px-2 py-0.5 bg-blue-50 dark:bg-blue-950/20 text-blue-600 dark:text-blue-400 rounded-md border border-blue-150 dark:border-blue-900/40 font-mono">
          HRMS_HrCoreDb
        </span>
      </div>
    </div>

    <!-- Tabs switcher -->
    <div class="flex border-b border-slate-200 dark:border-slate-800 overflow-x-auto no-print">
      <button 
        v-for="tab in [
          { id: 'employees', label: 'Nhân viên & Hồ sơ', icon: UserPlus },
          { id: 'org', label: 'Phòng ban & Chức vụ', icon: Layers },
          { id: 'contracts', label: 'Hợp đồng lao động', icon: FileSpreadsheet },
          { id: 'roles', label: 'Người dùng & Phân quyền', icon: ShieldCheck },
        ]"
        :key="tab.id"
        @click="setTab(tab.id as any)"
        class="flex items-center gap-2 px-5 py-3.5 border-b-2 text-sm font-semibold transition-all cursor-pointer whitespace-nowrap"
        :class="activeTab === tab.id 
          ? 'border-blue-600 text-blue-600 dark:text-blue-400 font-bold' 
          : 'border-transparent text-slate-500 hover:text-slate-900 dark:hover:text-slate-350'"
      >
        <component :is="tab.icon" :size="16" />
        <span>{{ tab.label }}</span>
      </button>
    </div>

    <!-- 1. TAB: EMPLOYEES -->
    <div v-if="activeTab === 'employees'" class="space-y-4">
      <!-- Controls -->
      <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3 bg-white dark:bg-slate-900 p-4 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-2xs no-print">
        <div class="flex flex-1 flex-wrap items-center gap-3">
          <div class="relative flex-1 min-w-[200px] max-w-sm">
            <Search class="absolute left-3 top-2.5 text-slate-400" :size="16" />
            <input 
              v-model="employeeSearch"
              type="text" 
              placeholder="Tìm kiếm mã hoặc tên..." 
              class="w-full pl-9 pr-4 py-2 border border-slate-200 dark:border-slate-800 rounded-xl text-xs outline-none focus:border-blue-500 bg-slate-50/50 dark:bg-slate-950/50 text-slate-900 dark:text-slate-100"
            />
          </div>
          
          <select v-model="selectedDeptFilter" class="border border-slate-200 dark:border-slate-800 rounded-xl px-3 py-2 text-xs text-slate-600 dark:text-slate-350 bg-white dark:bg-slate-900">
            <option value="">Tất cả phòng ban</option>
            <option v-for="d in mockDB.departments" :key="d.id" :value="d.id">{{ d.name }}</option>
          </select>

          <select v-model="selectedStatusFilter" class="border border-slate-200 dark:border-slate-800 rounded-xl px-3 py-2 text-xs text-slate-600 dark:text-slate-350 bg-white dark:bg-slate-900">
            <option value="">Tất cả trạng thái</option>
            <option value="Active">Hoạt động (Active)</option>
            <option value="Inactive">Ngừng việc (Inactive)</option>
          </select>
        </div>

        <button 
          @click="openAddEmpModal"
          class="inline-flex items-center justify-center gap-2 px-4 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-xs font-bold shadow-sm cursor-pointer active:scale-98"
        >
          <Plus :size="14" />
          <span>Thêm Nhân Viên</span>
        </button>
      </div>

      <!-- Table -->
      <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl overflow-hidden shadow-2xs">
        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs">
            <thead class="bg-slate-50 dark:bg-slate-850 text-slate-500 uppercase font-bold text-[10px] tracking-wider border-b border-slate-155 dark:border-slate-800">
              <tr>
                <th class="px-5 py-3.5">Mã NV</th>
                <th class="px-5 py-3.5">Họ và Tên</th>
                <th class="px-5 py-3.5">Liên hệ (Ẩn/Hiện)</th>
                <th class="px-5 py-3.5">Phòng ban</th>
                <th class="px-5 py-3.5">Chức vụ</th>
                <th class="px-5 py-3.5 text-center">Trạng thái</th>
                <th class="px-5 py-3.5 text-right no-print">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 dark:divide-slate-800 font-semibold text-slate-700 dark:text-slate-300">
              <tr v-for="emp in filteredEmployees" :key="emp.id" class="hover:bg-slate-50/30 dark:hover:bg-slate-850/10">
                <td class="px-5 py-4 font-mono font-bold text-slate-900 dark:text-slate-100">{{ emp.employeeCode }}</td>
                <td class="px-5 py-4">
                  <div class="font-extrabold text-slate-900 dark:text-slate-100 text-sm">{{ emp.fullName }}</div>
                  <div class="text-[10px] text-slate-400 font-medium">Ngày vào: {{ emp.joinedDate }}</div>
                </td>
                <td class="px-5 py-4">
                  <div class="flex items-center gap-2">
                    <div>
                      <div>{{ formatEmail(emp.email, emp.id) }}</div>
                      <div class="text-[10px] text-slate-400 font-medium">{{ formatPhone(emp.phone, emp.id) }}</div>
                    </div>
                    <button 
                      @click="togglePII(emp.id)"
                      class="text-slate-450 hover:text-slate-700 dark:hover:text-slate-100 p-1 rounded-full hover:bg-slate-100 dark:hover:bg-slate-800"
                    >
                      <Eye v-if="!revealedPII[emp.id]" :size="13" />
                      <EyeOff v-else :size="13" />
                    </button>
                  </div>
                </td>
                <td class="px-5 py-4">
                  {{ mockDB.departments.find(d => d.id === emp.departmentId)?.name || 'Chưa phân bổ' }}
                </td>
                <td class="px-5 py-4">
                  {{ mockDB.positions.find(p => p.id === emp.positionId)?.name || 'Chưa phân bổ' }}
                </td>
                <td class="px-5 py-4 text-center">
                  <span 
                    class="inline-flex px-2 py-0.5 rounded-full text-[10px] font-bold border"
                    :class="emp.status === 'Active' 
                      ? 'bg-blue-50 dark:bg-blue-950/20 border-blue-200 dark:border-blue-800 text-blue-700 dark:text-blue-400' 
                      : 'bg-slate-50 dark:bg-slate-800 border-slate-200 dark:border-slate-700 text-slate-500 dark:text-slate-400'"
                  >
                    {{ emp.status }}
                  </span>
                </td>
                <td class="px-5 py-4 text-right no-print">
                  <div class="flex items-center justify-end gap-1.5">
                    <button 
                      @click="activeDetailDrawerId = emp.id"
                      title="Chi tiết lý lịch"
                      class="p-1.5 border border-slate-200 dark:border-slate-800 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-800 text-slate-600 dark:text-slate-400 cursor-pointer"
                    >
                      <FileText :size="14" />
                    </button>
                    <button 
                      @click="openEditEmpModal(emp)"
                      title="Chỉnh sửa"
                      class="p-1.5 border border-slate-200 dark:border-slate-800 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-800 text-slate-600 dark:text-slate-400 cursor-pointer"
                    >
                      <Edit :size="14" />
                    </button>
                    <button 
                      @click="deleteEmployee(emp.id)"
                      class="p-1.5 border rounded-lg cursor-pointer"
                      :class="emp.status === 'Active' 
                        ? 'border-red-100 dark:border-red-900/30 text-red-650 hover:bg-red-50 dark:hover:bg-red-950/20' 
                        : 'border-blue-100 dark:border-blue-900/30 text-blue-650 hover:bg-blue-50 dark:hover:bg-blue-950/20'"
                    >
                      <XCircle v-if="emp.status === 'Active'" :size="14" />
                      <CheckCircle v-else :size="14" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- 2. TAB: DEPARTMENTS & POSITIONS -->
    <div v-if="activeTab === 'org'" class="grid gap-6 md:grid-cols-2">
      <!-- Departments list -->
      <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs space-y-4">
        <div class="flex items-center gap-2 border-b border-slate-100 dark:border-slate-800 pb-3">
          <Layers class="text-blue-600 dark:text-blue-400" :size="18" />
          <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Phòng ban công ty</h2>
        </div>

        <!-- Add Form -->
        <div class="flex gap-2 items-end bg-slate-50 dark:bg-slate-950 p-3.5 rounded-xl border border-slate-250 dark:border-slate-850 no-print">
          <div class="flex-1 space-y-1">
            <label class="text-[10px] font-bold text-slate-400 uppercase">Tên phòng ban</label>
            <input 
              v-model="newDeptName"
              type="text" 
              placeholder="e.g. Phòng Marketing" 
              class="w-full px-2.5 py-1.5 border border-slate-200 dark:border-slate-800 rounded bg-white dark:bg-slate-900 text-xs text-slate-900 dark:text-slate-100 outline-none focus:border-blue-500"
            />
          </div>
          <div class="w-24 space-y-1">
            <label class="text-[10px] font-bold text-slate-400 uppercase">Mã viết tắt</label>
            <input 
              v-model="newDeptCode"
              type="text" 
              placeholder="e.g. MKT" 
              class="w-full px-2.5 py-1.5 border border-slate-200 dark:border-slate-800 rounded bg-white dark:bg-slate-900 text-xs text-slate-900 dark:text-slate-100 outline-none focus:border-blue-500"
            />
          </div>
          <button 
            @click="saveDepartment"
            class="px-4 py-1.5 bg-blue-600 hover:bg-blue-700 text-white rounded text-xs font-bold shadow-sm cursor-pointer"
          >
            Thêm
          </button>
        </div>

        <!-- Lists -->
        <div class="space-y-2 max-h-[300px] overflow-y-auto pr-1">
          <div 
            v-for="dept in mockDB.departments" 
            :key="dept.id"
            class="flex items-center justify-between p-3.5 border border-slate-100 dark:border-slate-800 rounded-xl hover:bg-slate-50 dark:hover:bg-slate-850/30"
          >
            <div>
              <div class="text-xs font-bold text-slate-900 dark:text-slate-100">{{ dept.name }}</div>
              <div class="text-[10px] text-slate-400 font-mono">Mã: {{ dept.code }} | ID: {{ dept.id }}</div>
            </div>
            <span class="text-xs font-bold bg-blue-50 dark:bg-blue-950 text-blue-750 dark:text-blue-400 px-2.5 py-0.5 rounded-full">
              {{ mockDB.employees.filter(e => e.departmentId === dept.id).length }} nhân viên
            </span>
          </div>
        </div>
      </div>

      <!-- Positions list -->
      <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-2xs space-y-4">
        <div class="flex items-center gap-2 border-b border-slate-100 dark:border-slate-800 pb-3">
          <Briefcase class="text-indigo-600 dark:text-indigo-400" :size="18" />
          <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Chức vụ & Danh xưng</h2>
        </div>

        <!-- Add Form -->
        <div class="flex gap-2 items-end bg-slate-50 dark:bg-slate-950 p-3.5 rounded-xl border border-slate-250 dark:border-slate-850 no-print">
          <div class="flex-1 space-y-1">
            <label class="text-[10px] font-bold text-slate-400 uppercase">Tên chức vụ</label>
            <input 
              v-model="newPosName"
              type="text" 
              placeholder="e.g. Lập trình viên chính" 
              class="w-full px-2.5 py-1.5 border border-slate-200 dark:border-slate-800 rounded bg-white dark:bg-slate-900 text-xs text-slate-900 dark:text-slate-100 outline-none focus:border-blue-500"
            />
          </div>
          <div class="w-24 space-y-1">
            <label class="text-[10px] font-bold text-slate-400 uppercase">Mã viết tắt</label>
            <input 
              v-model="newPosCode"
              type="text" 
              placeholder="e.g. LEAD_DEV" 
              class="w-full px-2.5 py-1.5 border border-slate-200 dark:border-slate-800 rounded bg-white dark:bg-slate-900 text-xs text-slate-900 dark:text-slate-100 outline-none focus:border-blue-500"
            />
          </div>
          <button 
            @click="savePosition"
            class="px-4 py-1.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded text-xs font-bold shadow-sm cursor-pointer"
          >
            Thêm
          </button>
        </div>

        <!-- Lists -->
        <div class="space-y-2 max-h-[300px] overflow-y-auto pr-1">
          <div 
            v-for="pos in mockDB.positions" 
            :key="pos.id"
            class="flex items-center justify-between p-3.5 border border-slate-100 dark:border-slate-800 rounded-xl hover:bg-slate-50 dark:hover:bg-slate-850/30"
          >
            <div>
              <div class="text-xs font-bold text-slate-900 dark:text-slate-100">{{ pos.name }}</div>
              <div class="text-[10px] text-slate-400 font-mono">Mã: {{ pos.code }} | ID: {{ pos.id }}</div>
            </div>
            <span class="text-xs font-bold bg-indigo-50 dark:bg-indigo-950 text-indigo-755 dark:text-indigo-400 px-2.5 py-0.5 rounded-full">
              {{ mockDB.employees.filter(e => e.positionId === pos.id).length }} nhân sự
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- 3. TAB: LABOR CONTRACTS -->
    <div v-if="activeTab === 'contracts'" class="space-y-4">
      <div class="flex items-center justify-between bg-white dark:bg-slate-900 p-4 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-2xs no-print">
        <div>
          <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Hợp đồng lao động</h2>
          <p class="text-[11px] text-slate-400 mt-0.5">Theo dõi lịch sử, thời hạn và phân loại mức lương cơ bản</p>
        </div>
        <button 
          @click="openAddContractModal()"
          class="inline-flex items-center gap-2 px-4 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-xs font-bold shadow-sm cursor-pointer"
        >
          <Plus :size="14" />
          <span>Tạo Hợp đồng</span>
        </button>
      </div>

      <!-- Table -->
      <div class="bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl overflow-hidden shadow-2xs">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-50 dark:bg-slate-850 text-slate-500 uppercase font-bold text-[10px] tracking-wider border-b border-slate-155 dark:border-slate-800">
            <tr>
              <th class="px-5 py-3.5">Mã Hợp đồng</th>
              <th class="px-5 py-3.5">Nhân viên</th>
              <th class="px-5 py-3.5">Từ ngày</th>
              <th class="px-5 py-3.5">Đến ngày</th>
              <th class="px-5 py-3.5">Cơ chế lương</th>
              <th class="px-5 py-3.5">Lương cơ bản</th>
              <th class="px-5 py-3.5 text-center">Trạng thái</th>
              <th class="px-5 py-3.5 text-right no-print">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 dark:divide-slate-800 font-semibold text-slate-700 dark:text-slate-300">
            <tr v-for="con in mockDB.contracts" :key="con.id" class="hover:bg-slate-50/30">
              <td class="px-5 py-4 font-mono font-bold text-slate-900 dark:text-slate-100">{{ con.contractNumber }}</td>
              <td class="px-5 py-4">
                <div class="font-bold text-slate-955 dark:text-slate-200">
                  {{ mockDB.employees.find(e => e.id === con.employeeId)?.fullName || 'Chưa rõ' }}
                </div>
              </td>
              <td class="px-5 py-4 font-mono">{{ con.startDate }}</td>
              <td class="px-5 py-4 font-mono">{{ con.endDate }}</td>
              <td class="px-5 py-4 text-slate-550">{{ con.salaryType === 'Fixed' ? 'Lương cố định' : 'Lương giờ' }}</td>
              <td class="px-5 py-4 font-mono text-slate-900 dark:text-slate-100 font-bold">{{ formatVND(con.baseSalary) }}</td>
              <td class="px-5 py-4 text-center">
                <span 
                  class="inline-flex px-2 py-0.5 rounded-full text-[10px] font-bold border"
                  :class="con.status === 'Active' 
                    ? 'bg-blue-50 dark:bg-blue-950/20 border-blue-200 dark:border-blue-800 text-blue-700 dark:text-blue-400' 
                    : 'bg-red-50 dark:bg-red-950/20 border-red-200 dark:border-red-800 text-red-700 dark:text-red-400'"
                >
                  {{ con.status }}
                </span>
              </td>
              <td class="px-5 py-4 text-right no-print">
                <button 
                  v-if="con.status === 'Active'"
                  @click="hrService.terminateContract(con.id)"
                  class="text-xs font-bold text-red-600 dark:text-red-400 hover:underline cursor-pointer"
                >
                  Chấm dứt HĐ
                </button>
                <span v-else class="text-slate-400">-</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- 4. TAB: USERS & ROLES MATRIX -->
    <div v-if="activeTab === 'roles'" class="space-y-4">
      <div class="bg-white dark:bg-slate-900 p-5 rounded-2xl border border-slate-200 dark:border-slate-800 shadow-2xs space-y-3">
        <h2 class="text-sm font-bold text-slate-900 dark:text-slate-100 uppercase tracking-wider">Người dùng & Phân quyền</h2>
        <p class="text-xs text-slate-550">
          Chỉnh sửa vai trò quyền hạn giả lập của nhân viên để kiểm nghiệm cơ chế Route Guard và hiển thị menu Sidebar có điều kiện.
        </p>

        <div class="overflow-x-auto rounded-xl border border-slate-200 dark:border-slate-800 mt-4">
          <table class="w-full text-left text-xs">
            <thead class="bg-slate-50 dark:bg-slate-850 text-slate-500 uppercase font-bold text-[10px] tracking-wider border-b border-slate-155 dark:border-slate-800">
              <tr>
                <th class="px-5 py-3.5">Tài khoản Email</th>
                <th class="px-5 py-3.5">Họ tên nhân sự</th>
                <th class="px-5 py-3.5 text-center">Admin</th>
                <th class="px-5 py-3.5 text-center">HR</th>
                <th class="px-5 py-3.5 text-center">Manager</th>
                <th class="px-5 py-3.5 text-center">Employee</th>
                <th class="px-5 py-3.5 text-center">PayrollStaff</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 dark:divide-slate-800 font-semibold text-slate-700 dark:text-slate-350">
              <tr v-for="user in [
                { id: 'emp-001', name: 'Nguyễn Văn A', email: 'employee@hrms.local' },
                { id: 'emp-002', name: 'Trần Thị B', email: 'manager@hrms.local' },
                { id: 'emp-003', name: 'Lê Văn C', email: 'hr@hrms.local' },
                { id: 'emp-004', name: 'Phạm Minh D', email: 'payroll@hrms.local' },
                { id: 'emp-admin', name: 'Admin Demo', email: 'admin@hrms.local' },
              ]" :key="user.id">
                <td class="px-5 py-4 font-mono text-slate-900 dark:text-slate-100">{{ user.email }}</td>
                <td class="px-5 py-4">{{ user.name }}</td>
                <td v-for="role in ['Admin', 'HR', 'Manager', 'Employee', 'PayrollStaff']" :key="role" class="px-5 py-4 text-center">
                  <input 
                    type="checkbox"
                    :checked="roleMatrix[user.id]?.includes(role as any)"
                    @change="toggleRoleInMatrix(user.id, role)"
                    class="accent-blue-600 size-4 cursor-pointer"
                  />
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- DIALOG: ADD/EDIT EMPLOYEE -->
    <div v-if="isEmpModalOpen" class="fixed inset-0 bg-slate-950/40 backdrop-blur-xs flex items-center justify-center z-40 p-4">
      <div class="bg-white dark:bg-slate-900 rounded-2xl border border-slate-200 dark:border-slate-850 shadow-xl max-w-md w-full overflow-hidden animate-scaleIn">
        <div class="bg-slate-50 dark:bg-slate-850 px-5 py-4 border-b border-slate-200 dark:border-slate-800 flex items-center justify-between">
          <h3 class="text-xs font-black text-slate-900 dark:text-slate-100 uppercase tracking-wider">
            {{ editingEmpId ? 'Chỉnh sửa Hồ sơ' : 'Thêm Nhân viên mới' }}
          </h3>
          <button @click="isEmpModalOpen = false" class="text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 cursor-pointer">
            <X :size="18" />
          </button>
        </div>
        <div class="p-5 space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase">Mã nhân viên</label>
              <input 
                v-model="empForm.employeeCode" 
                type="text" 
                class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg bg-slate-100 dark:bg-slate-950 font-mono font-bold text-xs text-slate-900 dark:text-slate-100" 
                readonly 
              />
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-450 dark:text-slate-400 uppercase">Trạng thái</label>
              <select v-model="empForm.status" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900">
                <option value="Active">Active (Hoạt động)</option>
                <option value="Inactive">Inactive (Nghỉ việc)</option>
              </select>
            </div>
          </div>

          <div class="space-y-1">
            <label class="text-[10px] font-bold text-slate-450 dark:text-slate-450 uppercase">Họ và Tên</label>
            <input 
              v-model="empForm.fullName" 
              type="text" 
              placeholder="e.g. Nguyễn Văn A"
              class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 outline-none focus:border-blue-500" 
            />
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-450 dark:text-slate-400 uppercase">Email</label>
              <input 
                v-model="empForm.email" 
                type="email" 
                placeholder="email@hrms.local"
                class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 outline-none focus:border-blue-500" 
              />
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-450 dark:text-slate-450 uppercase">Số điện thoại</label>
              <input 
                v-model="empForm.phone" 
                type="text" 
                placeholder="09..."
                class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 outline-none focus:border-blue-500" 
              />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-455 dark:text-slate-400 uppercase">Phòng ban</label>
              <select v-model="empForm.departmentId" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900">
                <option v-for="d in mockDB.departments" :key="d.id" :value="d.id">{{ d.name }}</option>
              </select>
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-455 dark:text-slate-400 uppercase">Chức danh</label>
              <select v-model="empForm.positionId" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900">
                <option v-for="p in mockDB.positions" :key="p.id" :value="p.id">{{ p.name }}</option>
              </select>
            </div>
          </div>

          <div class="space-y-1">
            <label class="text-[10px] font-bold text-slate-450 dark:text-slate-400 uppercase font-semibold text-slate-700">Ngày gia nhập</label>
            <input 
              v-model="empForm.joinedDate" 
              type="date" 
              class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 outline-none focus:border-blue-500" 
            />
          </div>
        </div>
        <div class="bg-slate-50 dark:bg-slate-850 px-5 py-3.5 border-t border-slate-200 dark:border-slate-800 flex justify-end gap-2">
          <button @click="isEmpModalOpen = false" class="px-4 py-2 border border-slate-200 dark:border-slate-800 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg text-xs font-bold text-slate-600 dark:text-slate-300 cursor-pointer">
            Hủy
          </button>
          <button @click="saveEmployee" class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-xs font-bold shadow-sm cursor-pointer">
            Lưu hồ sơ
          </button>
        </div>
      </div>
    </div>

    <!-- DIALOG: CREATE CONTRACT -->
    <div v-if="isContractModalOpen" class="fixed inset-0 bg-slate-950/40 backdrop-blur-xs flex items-center justify-center z-40 p-4">
      <div class="bg-white dark:bg-slate-900 rounded-2xl border border-slate-200 dark:border-slate-850 shadow-xl max-w-md w-full overflow-hidden animate-scaleIn">
        <div class="bg-slate-50 dark:bg-slate-850 px-5 py-4 border-b border-slate-200 dark:border-slate-800 flex items-center justify-between">
          <h3 class="text-xs font-black text-slate-900 dark:text-slate-100 uppercase tracking-wider">Tạo hợp đồng mới</h3>
          <button @click="isContractModalOpen = false" class="text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 cursor-pointer">
            <X :size="18" />
          </button>
        </div>
        <div class="p-5 space-y-4 text-slate-800 dark:text-slate-200">
          <div class="space-y-1">
            <label class="text-[10px] font-bold text-slate-400 uppercase">Nhân viên áp dụng</label>
            <select v-model="contractForm.employeeId" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900">
              <option v-for="e in mockDB.employees.filter(emp => emp.status === 'Active')" :key="e.id" :value="e.id">
                [{{ e.employeeCode }}] {{ e.fullName }}
              </option>
            </select>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-405 dark:text-slate-400 uppercase">Số hợp đồng</label>
              <input 
                v-model="contractForm.contractNumber" 
                type="text" 
                class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 font-mono font-bold" 
              />
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-405 dark:text-slate-400 uppercase">Cơ chế lương</label>
              <select v-model="contractForm.salaryType" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900">
                <option value="Fixed">Lương cố định (Fixed)</option>
                <option value="Hourly">Lương theo giờ (Hourly)</option>
              </select>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase">Ngày bắt đầu</label>
              <input v-model="contractForm.startDate" type="date" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100" />
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase">Ngày kết thúc</label>
              <input v-model="contractForm.endDate" type="date" class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100" />
            </div>
          </div>

          <div class="space-y-1">
            <label class="text-[10px] font-bold text-slate-400 uppercase">Mức lương cơ bản (VND)</label>
            <input 
              v-model="contractForm.baseSalary" 
              type="number" 
              class="w-full px-3 py-2 border border-slate-200 dark:border-slate-800 rounded-lg text-xs bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 font-mono font-bold" 
            />
          </div>
        </div>
        <div class="bg-slate-50 dark:bg-slate-850 px-5 py-3.5 border-t border-slate-200 dark:border-slate-800 flex justify-end gap-2">
          <button @click="isContractModalOpen = false" class="px-4 py-2 border border-slate-200 dark:border-slate-800 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg text-xs font-bold text-slate-600 dark:text-slate-300 cursor-pointer">
            Hủy
          </button>
          <button @click="saveContract" class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-xs font-bold shadow-sm cursor-pointer">
            Tạo hợp đồng
          </button>
        </div>
      </div>
    </div>

    <!-- DRAWER: DETAILS -->
    <div 
      v-if="activeDetailDrawerId" 
      class="fixed inset-0 bg-slate-950/40 backdrop-blur-xs z-35 flex justify-end"
      @click="activeDetailDrawerId = null"
    >
      <div 
        class="bg-white dark:bg-slate-900 w-full max-w-md h-full shadow-2xl flex flex-col justify-between border-l border-slate-200 dark:border-slate-800 animate-slideOver"
        @click.stop
      >
        <!-- Header -->
        <div class="px-6 py-5 border-b border-slate-150 dark:border-slate-800 flex items-center justify-between bg-slate-50 dark:bg-slate-850">
          <div>
            <span class="text-[9px] font-bold text-slate-400 uppercase tracking-widest block font-mono">Hồ sơ chi tiết</span>
            <h3 class="text-base font-black text-slate-900 dark:text-slate-100">{{ selectedEmployeeDetails?.fullName }}</h3>
          </div>
          <button @click="activeDetailDrawerId = null" class="p-1 rounded-full hover:bg-slate-200 dark:hover:bg-slate-800 text-slate-400 cursor-pointer">
            <X :size="20" />
          </button>
        </div>

        <!-- Body -->
        <div class="flex-1 overflow-y-auto p-6 space-y-6 text-slate-800 dark:text-slate-250">
          <div class="space-y-4">
            <div class="flex items-center gap-4">
              <div class="size-16 rounded-full bg-blue-50 dark:bg-blue-950/20 border border-blue-100 dark:border-blue-900/50 flex items-center justify-center font-black text-blue-650 dark:text-blue-400 text-2xl">
                {{ selectedEmployeeDetails?.fullName.charAt(0) }}
              </div>
              <div>
                <div class="text-xs font-bold text-slate-400 font-mono">MÃ NV: {{ selectedEmployeeDetails?.employeeCode }}</div>
                <span class="text-[10px] font-bold px-2 py-0.5 rounded border inline-block mt-1.5"
                  :class="selectedEmployeeDetails?.status === 'Active' 
                    ? 'bg-blue-50 border-blue-200 text-blue-700' 
                    : 'bg-slate-150 border-slate-200 text-slate-650'"
                >
                  {{ selectedEmployeeDetails?.status }}
                </span>
              </div>
            </div>

            <!-- List data -->
            <div class="grid grid-cols-2 gap-4 text-xs font-semibold border-t border-b border-slate-100 dark:border-slate-800 py-4">
              <div>
                <span class="text-[9px] text-slate-400 font-bold uppercase block mb-1">Email liên hệ</span>
                <span class="text-slate-900 dark:text-slate-100">{{ selectedEmployeeDetails?.email }}</span>
              </div>
              <div>
                <span class="text-[9px] text-slate-400 font-bold uppercase block mb-1">Số điện thoại</span>
                <span class="text-slate-900 dark:text-slate-100">{{ selectedEmployeeDetails?.phone }}</span>
              </div>
              <div>
                <span class="text-[9px] text-slate-400 font-bold uppercase block mb-1">Phòng ban làm việc</span>
                <span class="text-slate-900 dark:text-slate-100">
                  {{ mockDB.departments.find(d => d.id === selectedEmployeeDetails?.departmentId)?.name || 'Chưa rõ' }}
                </span>
              </div>
              <div>
                <span class="text-[9px] text-slate-400 font-bold uppercase block mb-1">Chức danh chức vụ</span>
                <span class="text-slate-900 dark:text-slate-100">
                  {{ mockDB.positions.find(p => p.id === selectedEmployeeDetails?.positionId)?.name || 'Chưa rõ' }}
                </span>
              </div>
              <div class="col-span-2">
                <span class="text-[9px] text-slate-400 font-bold uppercase block mb-1">Ngày gia nhập công ty</span>
                <span class="text-slate-900 dark:text-slate-100">{{ selectedEmployeeDetails?.joinedDate }}</span>
              </div>
            </div>
          </div>

          <!-- Active Contract -->
          <div class="space-y-3">
            <h4 class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Hợp đồng active hiện tại</h4>
            
            <div v-if="selectedEmployeeContract" class="border border-slate-200 dark:border-slate-800 rounded-xl p-4 bg-slate-50 dark:bg-slate-950 space-y-2">
              <div class="flex items-center justify-between">
                <span class="text-xs font-bold text-slate-850 dark:text-slate-100 font-mono">{{ selectedEmployeeContract.contractNumber }}</span>
                <span class="text-[9px] font-bold px-1.5 py-0.5 bg-blue-50 dark:bg-blue-950 text-blue-700 dark:text-blue-400 rounded border border-blue-100 dark:border-blue-900/40">
                  Active
                </span>
              </div>
              <div class="grid grid-cols-2 gap-2 text-[11px] text-slate-500 dark:text-slate-400 font-medium">
                <div>Hiệu lực từ: {{ selectedEmployeeContract.startDate }}</div>
                <div>Đến ngày: {{ selectedEmployeeContract.endDate }}</div>
                <div class="col-span-2 text-slate-900 dark:text-slate-100 font-semibold border-t border-slate-200/50 dark:border-slate-800 pt-2 mt-1 flex justify-between items-center">
                  <span>Lương cơ bản hợp đồng:</span>
                  <span class="font-mono text-blue-650 dark:text-blue-400 font-bold text-sm">{{ formatVND(selectedEmployeeContract.baseSalary) }}</span>
                </div>
              </div>
            </div>
            
            <div v-else class="text-center py-4 bg-slate-50 dark:bg-slate-950 border border-dashed border-slate-250 dark:border-slate-800 rounded-xl text-slate-400 text-xs font-semibold">
              Chưa gán hợp đồng lao động nào.<br/>
              <button 
                @click="openAddContractModal(selectedEmployeeDetails?.id)"
                class="text-blue-600 dark:text-blue-450 hover:underline mt-1.5 font-bold inline-block cursor-pointer"
              >
                + Gán Hợp đồng mới
              </button>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="p-6 border-t border-slate-100 dark:border-slate-800 bg-slate-50 dark:bg-slate-850">
          <button @click="activeDetailDrawerId = null" class="w-full py-2.5 bg-slate-200 dark:bg-slate-800 hover:bg-slate-300 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-200 rounded-lg text-xs font-bold transition-all cursor-pointer">
            Đóng hồ sơ
          </button>
        </div>
      </div>
    </div>

  </div>
</template>
