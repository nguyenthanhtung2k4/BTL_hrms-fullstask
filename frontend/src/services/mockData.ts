import { reactive, watch } from 'vue'

// Basic Types
export type UserRole = 'Admin' | 'HR' | 'Manager' | 'Employee' | 'PayrollStaff'

export interface Department {
  id: string
  name: string
  code: string
  managerId?: string
}

export interface Position {
  id: string
  name: string
  code: string
}

export interface Employee {
  id: string
  employeeCode: string
  fullName: string
  email: string
  phone: string
  departmentId: string
  positionId: string
  status: 'Active' | 'Inactive'
  joinedDate: string
}

export interface Contract {
  id: string
  employeeId: string
  contractNumber: string
  startDate: string
  endDate: string
  salaryType: 'Fixed' | 'Hourly'
  baseSalary: number
  status: 'Active' | 'Expired' | 'Terminated'
}

export interface Shift {
  id: string
  name: string
  startTime: string // e.g., "08:00"
  endTime: string // e.g., "17:00"
  lateGraceMinutes: number
  color: string
}

export interface WorkSchedule {
  id: string
  employeeId: string
  date: string // YYYY-MM-DD
  shiftId: string
}

export interface AttendanceRecord {
  id: string
  employeeId: string
  workDate: string // YYYY-MM-DD
  shiftId: string
  checkInAt: string | null // ISO Time or HH:mm:ss
  checkOutAt: string | null
  checkInIP?: string
  checkOutIP?: string
  workedMinutes: number
  status: 'Completed' | 'Incomplete' | 'Late' | 'EarlyLeave' | 'Absent' | 'OnLeave'
}

export interface LeaveRequest {
  id: string
  employeeId: string
  leaveType: 'Annual' | 'Sick' | 'Maternity' | 'Unpaid'
  fromDate: string
  toDate: string
  reason: string
  status: 'Pending' | 'Approved' | 'Rejected' | 'Cancelled'
  approvedBy: string | null
  feedback: string | null
}

export interface OTRequest {
  id: string
  employeeId: string
  date: string // YYYY-MM-DD
  requestedMinutes: number
  reason: string
  status: 'Pending' | 'Approved' | 'Rejected'
  approvedBy: string | null
}

export interface PayrollPeriod {
  id: string
  name: string // e.g., "Tháng 06/2026"
  startDate: string
  endDate: string
  isClosed: boolean
  closedAt: string | null
  closedBy: string | null
}

export interface AllowanceDeduction {
  id: string
  employeeId: string
  type: 'Allowance' | 'Deduction'
  title: string
  amount: number
  isRecurring: boolean
}

export interface Payslip {
  id: string
  periodId: string
  employeeId: string
  baseSalary: number
  workDays: number
  actualWorkDays: number
  otHours: number
  otAmount: number
  allowanceAmount: number
  deductionAmount: number
  grossSalary: number
  netSalary: number
  calculatedAt: string
  status: 'Draft' | 'Paid'
}

export interface AuditLog {
  id: string
  timestamp: string
  userFullName: string
  action: string
  details: string
  service: 'HR Core' | 'Attendance' | 'Payroll & Report'
}

export interface RabbitMQEvent {
  id: string
  eventName: string
  occurredAt: string
  sourceService: 'hr-core' | 'attendance' | 'payroll-report'
  payload: string // JSON representation
}

// Initial Seed Data
const defaultDepartments: Department[] = [
  { id: 'dept-001', name: 'Phòng Phát triển Phần mềm', code: 'DEV' },
  { id: 'dept-002', name: 'Phòng Nhân sự', code: 'HR' },
  { id: 'dept-003', name: 'Phòng Tài chính Kế toán', code: 'ACC' },
  { id: 'dept-004', name: 'Phòng Kinh doanh', code: 'SALES' },
]

const defaultPositions: Position[] = [
  { id: 'pos-001', name: 'Giám đốc công nghệ', code: 'CTO' },
  { id: 'pos-002', name: 'Trưởng phòng', code: 'MGR' },
  { id: 'pos-003', name: 'Lập trình viên Senior', code: 'SR_DEV' },
  { id: 'pos-004', name: 'Lập trình viên Junior', code: 'JR_DEV' },
  { id: 'pos-005', name: 'Chuyên viên Nhân sự', code: 'HR_SPEC' },
  { id: 'pos-006', name: 'Chuyên viên Kế toán', code: 'ACC_SPEC' },
]

const defaultEmployees: Employee[] = [
  { id: 'emp-001', employeeCode: 'HRMS-001', fullName: 'Nguyễn Văn A', email: 'employee@hrms.local', phone: '0987654321', departmentId: 'dept-001', positionId: 'pos-004', status: 'Active', joinedDate: '2024-01-15' },
  { id: 'emp-002', employeeCode: 'HRMS-002', fullName: 'Trần Thị B', email: 'manager@hrms.local', phone: '0912345678', departmentId: 'dept-001', positionId: 'pos-002', status: 'Active', joinedDate: '2023-05-10' },
  { id: 'emp-003', employeeCode: 'HRMS-003', fullName: 'Lê Văn C', email: 'hr@hrms.local', phone: '0909090909', departmentId: 'dept-002', positionId: 'pos-005', status: 'Active', joinedDate: '2024-03-01' },
  { id: 'emp-004', employeeCode: 'HRMS-004', fullName: 'Phạm Minh D', email: 'payroll@hrms.local', phone: '0977889900', departmentId: 'dept-003', positionId: 'pos-006', status: 'Active', joinedDate: '2023-11-20' },
  { id: 'emp-005', employeeCode: 'HRMS-005', fullName: 'Hoàng Văn E', email: 'hoangve@hrms.local', phone: '0966554433', departmentId: 'dept-001', positionId: 'pos-003', status: 'Active', joinedDate: '2022-08-15' },
  { id: 'emp-006', employeeCode: 'HRMS-006', fullName: 'Đỗ Thị F', email: 'dothif@hrms.local', phone: '0944332211', departmentId: 'dept-004', positionId: 'pos-004', status: 'Inactive', joinedDate: '2024-02-10' },
]

const defaultContracts: Contract[] = [
  { id: 'con-001', employeeId: 'emp-001', contractNumber: 'LĐ-2024/001', startDate: '2024-01-15', endDate: '2025-01-14', salaryType: 'Fixed', baseSalary: 15000000, status: 'Active' },
  { id: 'con-002', employeeId: 'emp-002', contractNumber: 'LĐ-2023/002', startDate: '2023-05-10', endDate: '2026-05-09', salaryType: 'Fixed', baseSalary: 25000000, status: 'Active' },
  { id: 'con-003', employeeId: 'emp-003', contractNumber: 'LĐ-2024/003', startDate: '2024-03-01', endDate: '2025-02-28', salaryType: 'Fixed', baseSalary: 12000000, status: 'Active' },
  { id: 'con-004', employeeId: 'emp-004', contractNumber: 'LĐ-2023/004', startDate: '2023-11-20', endDate: '2025-11-19', salaryType: 'Fixed', baseSalary: 14000000, status: 'Active' },
  { id: 'con-005', employeeId: 'emp-005', contractNumber: 'LĐ-2022/005', startDate: '2022-08-15', endDate: '2025-08-14', salaryType: 'Fixed', baseSalary: 22000000, status: 'Active' },
]

const defaultShifts: Shift[] = [
  { id: 'shift-hc', name: 'Ca Hành chính (8:00 - 17:00)', startTime: '08:00', endTime: '17:00', lateGraceMinutes: 15, color: '#059669' },
  { id: 'shift-s', name: 'Ca Sáng (6:00 - 12:00)', startTime: '06:00', endTime: '12:00', lateGraceMinutes: 10, color: '#2563eb' },
  { id: 'shift-c', name: 'Ca Chiều (12:00 - 18:00)', startTime: '12:00', endTime: '18:00', lateGraceMinutes: 10, color: '#d97706' },
]

const defaultPeriods: PayrollPeriod[] = [
  { id: 'per-01', name: 'Tháng 05/2026', startDate: '2026-05-01', endDate: '2026-05-31', isClosed: true, closedAt: '2026-05-31T17:30:00Z', closedBy: 'Phạm Minh D' },
  { id: 'per-02', name: 'Tháng 06/2026', startDate: '2026-06-01', endDate: '2026-06-30', isClosed: false, closedAt: null, closedBy: null },
]

// Generating historical attendance for emp-001 (Nguyễn Văn A) and emp-002 (Trần Thị B) in May and early June 2026
const generateHistoricalAttendance = (): AttendanceRecord[] => {
  const records: AttendanceRecord[] = []
  const dates = [
    '2026-06-01', '2026-06-02', '2026-06-03', '2026-06-04', '2026-06-05',
    '2026-06-08', '2026-06-09', '2026-06-10', '2026-06-11', '2026-06-12',
    '2026-06-15'
  ]

  dates.forEach((date, index) => {
    // emp-001: Nguyễn Văn A (Usually check in on time, sometimes 5-10m late)
    const delay = index === 3 ? 20 : (index % 5 === 0 ? 5 : 0)
    const checkInTime = delay > 0 ? `08:${delay.toString().padStart(2, '0')}:12` : '07:55:45'
    const status = delay > 15 ? 'Late' : 'Completed'

    records.push({
      id: `att-001-${index}`,
      employeeId: 'emp-001',
      workDate: date,
      shiftId: 'shift-hc',
      checkInAt: `2026-06-${date.slice(-2)}T${checkInTime}Z`,
      checkOutAt: `2026-06-${date.slice(-2)}T17:05:00Z`,
      checkInIP: '192.168.1.102',
      checkOutIP: '192.168.1.102',
      workedMinutes: 480 - delay,
      status: status
    })

    // emp-002: Trần Thị B (Manager)
    records.push({
      id: `att-002-${index}`,
      employeeId: 'emp-002',
      workDate: date,
      shiftId: 'shift-hc',
      checkInAt: `2026-06-${date.slice(-2)}T07:50:30Z`,
      checkOutAt: `2026-06-${date.slice(-2)}T17:30:00Z`,
      checkInIP: '192.168.1.20',
      checkOutIP: '192.168.1.20',
      workedMinutes: 520,
      status: 'Completed'
    })
  })

  return records
}

const defaultSchedules = (): WorkSchedule[] => {
  const schedules: WorkSchedule[] = []
  const employees = ['emp-001', 'emp-002', 'emp-003', 'emp-004', 'emp-005']
  const dates = [
    '2026-06-01', '2026-06-02', '2026-06-03', '2026-06-04', '2026-06-05',
    '2026-06-08', '2026-06-09', '2026-06-10', '2026-06-11', '2026-06-12',
    '2026-06-15', '2026-06-16', '2026-06-17', '2026-06-18', '2026-06-19',
    '2026-06-22', '2026-06-23', '2026-06-24', '2026-06-25', '2026-06-26',
    '2026-06-29', '2026-06-30'
  ]

  employees.forEach(empId => {
    dates.forEach((date, index) => {
      schedules.push({
        id: `sch-${empId}-${index}`,
        employeeId: empId,
        date: date,
        shiftId: 'shift-hc'
      })
    })
  })

  return schedules
}

const defaultLeaveRequests: LeaveRequest[] = [
  { id: 'leave-001', employeeId: 'emp-001', leaveType: 'Annual', fromDate: '2026-06-18', toDate: '2026-06-18', reason: 'Có việc gia đình ở quê', status: 'Pending', approvedBy: null, feedback: null },
  { id: 'leave-002', employeeId: 'emp-005', leaveType: 'Sick', fromDate: '2026-06-10', toDate: '2026-06-11', reason: 'Sốt xuất huyết nằm viện', status: 'Approved', approvedBy: 'emp-002', feedback: 'Nghỉ ngơi giữ sức khỏe em nhé' }
]

const defaultAllowancesDeductions: AllowanceDeduction[] = [
  { id: 'ad-001', employeeId: 'emp-001', type: 'Allowance', title: 'Phụ cấp ăn trưa', amount: 730000, isRecurring: true },
  { id: 'ad-002', employeeId: 'emp-001', type: 'Allowance', title: 'Phụ cấp xăng xe', amount: 300000, isRecurring: true },
  { id: 'ad-003', employeeId: 'emp-001', type: 'Deduction', title: 'Khấu trừ bảo hiểm xã hội (10.5%)', amount: 1575000, isRecurring: true },
  { id: 'ad-004', employeeId: 'emp-002', type: 'Allowance', title: 'Phụ cấp quản lý', amount: 3000000, isRecurring: true },
]

const defaultEvents: RabbitMQEvent[] = [
  { id: 'evt-001', eventName: 'EmployeeCreated', occurredAt: '2026-06-15T09:00:00Z', sourceService: 'hr-core', payload: JSON.stringify({ employeeId: 'emp-005', fullName: 'Hoàng Văn E', departmentId: 'dept-001' }) },
  { id: 'evt-002', eventName: 'LeaveApproved', occurredAt: '2026-06-10T08:30:00Z', sourceService: 'attendance', payload: JSON.stringify({ leaveRequestId: 'leave-002', employeeId: 'emp-005', fromDate: '2026-06-10', toDate: '2026-06-11', paid: true }) },
  { id: 'evt-003', eventName: 'PayrollClosed', occurredAt: '2026-05-31T17:30:00Z', sourceService: 'payroll-report', payload: JSON.stringify({ periodId: 'per-01', closedBy: 'Phạm Minh D' }) },
]

// Load or Setup reactive state
const localStorageKey = 'hrms_mock_db_v2'
const loadedData = localStorage.getItem(localStorageKey)

interface MockDB {
  departments: Department[]
  positions: Position[]
  employees: Employee[]
  contracts: Contract[]
  shifts: Shift[]
  schedules: WorkSchedule[]
  attendanceRecords: AttendanceRecord[]
  leaveRequests: LeaveRequest[]
  otRequests: OTRequest[]
  periods: PayrollPeriod[]
  adjustments: AllowanceDeduction[]
  payslips: Payslip[]
  events: RabbitMQEvent[]
  auditLogs: AuditLog[]
  systemMode: 'Mock' | 'Live'
}

export const mockDB = reactive<MockDB>(
  loadedData
    ? JSON.parse(loadedData)
    : {
        departments: defaultDepartments,
        positions: defaultPositions,
        employees: defaultEmployees,
        contracts: defaultContracts,
        shifts: defaultShifts,
        schedules: defaultSchedules(),
        attendanceRecords: generateHistoricalAttendance(),
        leaveRequests: defaultLeaveRequests,
        otRequests: [],
        periods: defaultPeriods,
        adjustments: defaultAllowancesDeductions,
        payslips: [],
        events: defaultEvents,
        auditLogs: [
          { id: 'log-001', timestamp: '2026-06-15T09:00:00Z', userFullName: 'Lê Văn C', action: 'CREATE_EMPLOYEE', details: 'Đã thêm nhân viên Hoàng Văn E (HRMS-005)', service: 'HR Core' },
          { id: 'log-002', timestamp: '2026-06-10T08:30:00Z', userFullName: 'Trần Thị B', action: 'APPROVE_LEAVE', details: 'Duyệt đơn nghỉ bệnh của Hoàng Văn E', service: 'Attendance' }
        ],
        systemMode: 'Mock'
      }
)

// Watch changes to save automatically
watch(
  mockDB,
  (newVal) => {
    localStorage.setItem(localStorageKey, JSON.stringify(newVal))
  },
  { deep: true }
)

// Helper: Emit Simulated RabbitMQ Event
export function emitEvent(eventName: string, source: 'hr-core' | 'attendance' | 'payroll-report', payload: any) {
  const newEvent: RabbitMQEvent = {
    id: `evt-${Date.now()}`,
    eventName,
    occurredAt: new Date().toISOString(),
    sourceService: source,
    payload: JSON.stringify(payload)
  }
  mockDB.events.unshift(newEvent)
  if (mockDB.events.length > 50) {
    mockDB.events.pop()
  }

  // Auto add audit log too
  writeAuditLog(
    source === 'hr-core' ? 'HR Core' : source === 'attendance' ? 'Attendance' : 'Payroll & Report',
    `EVENT_EMITTED: ${eventName}`,
    `Phát sự kiện ${eventName} lên RabbitMQ: ${JSON.stringify(payload).slice(0, 100)}...`
  )
}

// Helper: Write Audit Log
export function writeAuditLog(service: 'HR Core' | 'Attendance' | 'Payroll & Report', action: string, details: string) {
  const activeUser = localStorage.getItem('active_user_name') || 'Hệ thống'
  const log: AuditLog = {
    id: `log-${Date.now()}-${Math.random().toString(36).substring(2, 6)}`,
    timestamp: new Date().toISOString(),
    userFullName: activeUser,
    action,
    details,
    service
  }
  mockDB.auditLogs.unshift(log)
  if (mockDB.auditLogs.length > 100) {
    mockDB.auditLogs.pop()
  }
}

// --- CRUD Actions ---

// HR Core Services
export const hrService = {
  // Employee
  addEmployee(emp: Omit<Employee, 'id'>) {
    const id = `emp-${Date.now()}`
    const newEmp = { ...emp, id }
    mockDB.employees.push(newEmp)
    writeAuditLog('HR Core', 'CREATE_EMPLOYEE', `Thêm nhân viên ${newEmp.fullName} (${newEmp.employeeCode})`)
    
    // Publish Event EmployeeCreated
    emitEvent('EmployeeCreated', 'hr-core', {
      employeeId: newEmp.id,
      employeeCode: newEmp.employeeCode,
      fullName: newEmp.fullName,
      departmentId: newEmp.departmentId,
      positionId: newEmp.positionId,
      status: newEmp.status
    })
    return newEmp
  },
  updateEmployee(id: string, updates: Partial<Employee>) {
    const index = mockDB.employees.findIndex(e => e.id === id)
    if (index !== -1) {
      mockDB.employees[index] = { ...mockDB.employees[index], ...updates }
      const emp = mockDB.employees[index]
      writeAuditLog('HR Core', 'UPDATE_EMPLOYEE', `Cập nhật thông tin nhân viên ${emp.fullName}`)
      
      emitEvent('EmployeeUpdated', 'hr-core', {
        employeeId: emp.id,
        employeeCode: emp.employeeCode,
        fullName: emp.fullName,
        departmentId: emp.departmentId,
        positionId: emp.positionId,
        status: emp.status
      })
    }
  },
  toggleEmployeeStatus(id: string) {
    const emp = mockDB.employees.find(e => e.id === id)
    if (emp) {
      emp.status = emp.status === 'Active' ? 'Inactive' : 'Active'
      writeAuditLog('HR Core', 'CHANGE_EMPLOYEE_STATUS', `Đổi trạng thái nhân viên ${emp.fullName} sang ${emp.status}`)
      
      emitEvent('EmployeeStatusChanged', 'hr-core', {
        employeeId: emp.id,
        status: emp.status
      })
    }
  },

  // Department
  addDepartment(dept: Omit<Department, 'id'>) {
    const newDept = { ...dept, id: `dept-${Date.now()}` }
    mockDB.departments.push(newDept)
    writeAuditLog('HR Core', 'CREATE_DEPARTMENT', `Thêm phòng ban ${newDept.name} (${newDept.code})`)
  },
  updateDepartment(id: string, name: string, code: string) {
    const dept = mockDB.departments.find(d => d.id === id)
    if (dept) {
      dept.name = name
      dept.code = code
      writeAuditLog('HR Core', 'UPDATE_DEPARTMENT', `Cập nhật phòng ban ${name}`)
    }
  },

  // Position
  addPosition(pos: Omit<Position, 'id'>) {
    const newPos = { ...pos, id: `pos-${Date.now()}` }
    mockDB.positions.push(newPos)
    writeAuditLog('HR Core', 'CREATE_POSITION', `Thêm chức vụ ${newPos.name}`)
  },
  updatePosition(id: string, name: string, code: string) {
    const pos = mockDB.positions.find(p => p.id === id)
    if (pos) {
      pos.name = name
      pos.code = code
      writeAuditLog('HR Core', 'UPDATE_POSITION', `Cập nhật chức vụ ${name}`)
    }
  },

  // Contract
  addContract(con: Omit<Contract, 'id' | 'status'>) {
    const newCon: Contract = { ...con, id: `con-${Date.now()}`, status: 'Active' }
    mockDB.contracts.push(newCon)
    const emp = mockDB.employees.find(e => e.id === con.employeeId)
    writeAuditLog('HR Core', 'CREATE_CONTRACT', `Tạo hợp đồng ${newCon.contractNumber} cho nhân viên ${emp?.fullName ?? ''}`)
  },
  terminateContract(id: string) {
    const con = mockDB.contracts.find(c => c.id === id)
    if (con) {
      con.status = 'Terminated'
      const emp = mockDB.employees.find(e => e.id === con.employeeId)
      writeAuditLog('HR Core', 'TERMINATE_CONTRACT', `Chấm dứt hợp đồng ${con.contractNumber} của nhân viên ${emp?.fullName ?? ''}`)
    }
  }
}

// Attendance Services
export const attendanceService = {
  // Check-in / Check-out
  checkIn(employeeId: string, shiftId: string) {
    const today = new Date().toISOString().split('T')[0]
    
    // Rule: Check-in validation
    const employee = mockDB.employees.find(e => e.id === employeeId)
    if (!employee || employee.status === 'Inactive') {
      throw new Error('Nhân viên không hoạt động hoặc không tồn tại. Không thể chấm công!')
    }

    const existing = mockDB.attendanceRecords.find(r => r.employeeId === employeeId && r.workDate === today && r.shiftId === shiftId)
    if (existing) {
      throw new Error('Bạn đã check-in cho ca làm việc này hôm nay!')
    }

    const shift = mockDB.shifts.find(s => s.id === shiftId)
    const now = new Date()
    
    // Status check (Late?)
    let status: AttendanceRecord['status'] = 'Completed'
    if (shift) {
      const [sh, sm] = shift.startTime.split(':').map(Number)
      const shiftStartMinutes = sh * 60 + sm
      const checkInMinutes = now.getHours() * 60 + now.getMinutes()
      if (checkInMinutes > shiftStartMinutes + shift.lateGraceMinutes) {
        status = 'Late'
      }
    }

    const record: AttendanceRecord = {
      id: `att-${Date.now()}`,
      employeeId,
      workDate: today,
      shiftId,
      checkInAt: now.toISOString(),
      checkOutAt: null,
      checkInIP: '192.168.1.100', // Mock IP
      workedMinutes: 0,
      status
    }

    mockDB.attendanceRecords.push(record)
    writeAuditLog('Attendance', 'CHECK_IN', `Nhân viên ${employee.fullName} check-in ca ${shift?.name ?? ''}`)
    return record
  },

  checkOut(employeeId: string, shiftId: string) {
    const today = new Date().toISOString().split('T')[0]
    const record = mockDB.attendanceRecords.find(r => r.employeeId === employeeId && r.workDate === today && r.shiftId === shiftId)
    
    if (!record) {
      throw new Error('Không tìm thấy bản ghi check-in. Bạn phải check-in trước!')
    }
    if (record.checkOutAt) {
      throw new Error('Bạn đã check-out ca làm việc này hôm nay!')
    }

    const shift = mockDB.shifts.find(s => s.id === shiftId)
    const now = new Date()
    record.checkOutAt = now.toISOString()
    record.checkOutIP = '192.168.1.100'

    // Calculate worked minutes
    const checkInTime = new Date(record.checkInAt!)
    const workedMs = now.getTime() - checkInTime.getTime()
    record.workedMinutes = Math.round(workedMs / 60000)

    // Adjust status if early leave
    if (shift) {
      const [eh, em] = shift.endTime.split(':').map(Number)
      const shiftEndMinutes = eh * 60 + em
      const checkOutMinutes = now.getHours() * 60 + now.getMinutes()
      if (checkOutMinutes < shiftEndMinutes) {
        record.status = record.status === 'Late' ? 'Late' : 'EarlyLeave'
      }
    }

    const employee = mockDB.employees.find(e => e.id === employeeId)
    writeAuditLog('Attendance', 'CHECK_OUT', `Nhân viên ${employee?.fullName ?? ''} check-out ca ${shift?.name ?? ''}`)

    // Emit event AttendanceRecorded
    emitEvent('AttendanceRecorded', 'attendance', {
      attendanceRecordId: record.id,
      employeeId: record.employeeId,
      workDate: record.workDate,
      shiftId: record.shiftId,
      checkInAt: record.checkInAt,
      checkOutAt: record.checkOutAt,
      workedMinutes: record.workedMinutes,
      status: record.status
    })

    return record
  },

  // Shifts
  addShift(shift: Omit<Shift, 'id'>) {
    const newShift = { ...shift, id: `shift-${Date.now()}` }
    mockDB.shifts.push(newShift)
    writeAuditLog('Attendance', 'CREATE_SHIFT', `Tạo ca làm việc ${newShift.name}`)
  },
  
  // Schedules
  assignSchedule(empId: string, date: string, shiftId: string) {
    // Delete existing schedule for that date
    mockDB.schedules = mockDB.schedules.filter(s => !(s.employeeId === empId && s.date === date))
    if (shiftId) {
      const newSched = { id: `sch-${Date.now()}`, employeeId: empId, date, shiftId }
      mockDB.schedules.push(newSched)
    }
  },

  // Leave Requests
  createLeave(req: Omit<LeaveRequest, 'id' | 'status' | 'approvedBy' | 'feedback'>) {
    const newReq: LeaveRequest = {
      ...req,
      id: `leave-${Date.now()}`,
      status: 'Pending',
      approvedBy: null,
      feedback: null
    }
    mockDB.leaveRequests.push(newReq)
    const emp = mockDB.employees.find(e => e.id === req.employeeId)
    writeAuditLog('Attendance', 'CREATE_LEAVE_REQUEST', `Nhân viên ${emp?.fullName ?? ''} gửi đơn nghỉ từ ngày ${req.fromDate} đến ${req.toDate}`)
    return newReq
  },

  approveLeave(id: string, managerId: string, feedback: string) {
    const leave = mockDB.leaveRequests.find(l => l.id === id)
    if (leave && leave.status === 'Pending') {
      leave.status = 'Approved'
      leave.approvedBy = managerId
      leave.feedback = feedback

      const emp = mockDB.employees.find(e => e.id === leave.employeeId)
      writeAuditLog('Attendance', 'APPROVE_LEAVE', `Phê duyệt đơn nghỉ phép của ${emp?.fullName ?? ''}`)

      // Emit LeaveApproved event
      emitEvent('LeaveApproved', 'attendance', {
        leaveRequestId: leave.id,
        employeeId: leave.employeeId,
        fromDate: leave.fromDate,
        toDate: leave.toDate,
        leaveType: leave.leaveType,
        paid: leave.leaveType !== 'Unpaid',
        approvedBy: managerId
      })
      
      // Auto add attendance records as 'OnLeave' for leave dates
      const start = new Date(leave.fromDate)
      const end = new Date(leave.toDate)
      for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
        const dateStr = d.toISOString().split('T')[0]
        // Check if there is a schedule assigned to find shiftId
        const sched = mockDB.schedules.find(s => s.employeeId === leave.employeeId && s.date === dateStr)
        const shiftId = sched?.shiftId || 'shift-hc'
        
        // Remove existing record for that day
        mockDB.attendanceRecords = mockDB.attendanceRecords.filter(r => !(r.employeeId === leave.employeeId && r.workDate === dateStr))
        
        mockDB.attendanceRecords.push({
          id: `att-leave-${Date.now()}-${Math.random().toString(36).substring(2,5)}`,
          employeeId: leave.employeeId,
          workDate: dateStr,
          shiftId,
          checkInAt: null,
          checkOutAt: null,
          workedMinutes: 0,
          status: 'OnLeave'
        })
      }
    }
  },

  rejectLeave(id: string, managerId: string, feedback: string) {
    const leave = mockDB.leaveRequests.find(l => l.id === id)
    if (leave && leave.status === 'Pending') {
      leave.status = 'Rejected'
      leave.approvedBy = managerId
      leave.feedback = feedback

      const emp = mockDB.employees.find(e => e.id === leave.employeeId)
      writeAuditLog('Attendance', 'REJECT_LEAVE', `Từ chối đơn nghỉ phép của ${emp?.fullName ?? ''}`)
    }
  },

  cancelLeave(id: string) {
    const leave = mockDB.leaveRequests.find(l => l.id === id)
    if (leave && leave.status === 'Pending') {
      leave.status = 'Cancelled'
      writeAuditLog('Attendance', 'CANCEL_LEAVE', `Hủy đơn nghỉ phép #${leave.id}`)
    }
  },

  // OT requests
  createOT(employeeId: string, date: string, minutes: number, reason: string) {
    const req: OTRequest = {
      id: `ot-${Date.now()}`,
      employeeId,
      date,
      requestedMinutes: minutes,
      reason,
      status: 'Pending',
      approvedBy: null
    }
    mockDB.otRequests.push(req)
    const emp = mockDB.employees.find(e => e.id === employeeId)
    writeAuditLog('Attendance', 'CREATE_OT_REQUEST', `Nhân viên ${emp?.fullName} đăng ký tăng ca ${minutes} phút ngày ${date}`)
  },

  approveOT(id: string, managerId: string) {
    const ot = mockDB.otRequests.find(o => o.id === id)
    if (ot && ot.status === 'Pending') {
      ot.status = 'Approved'
      ot.approvedBy = managerId
      const emp = mockDB.employees.find(e => e.id === ot.employeeId)
      writeAuditLog('Attendance', 'APPROVE_OT', `Duyệt tăng ca ${ot.requestedMinutes} phút ngày ${ot.date} cho ${emp?.fullName}`)
    }
  },

  rejectOT(id: string, managerId: string) {
    const ot = mockDB.otRequests.find(o => o.id === id)
    if (ot && ot.status === 'Pending') {
      ot.status = 'Rejected'
      ot.approvedBy = managerId
      const emp = mockDB.employees.find(e => e.id === ot.employeeId)
      writeAuditLog('Attendance', 'REJECT_OT', `Từ chối tăng ca của ${emp?.fullName}`)
    }
  }
}

// Payroll & Report Services
export const payrollService = {
  // Allowances & Deductions
  addAdjustment(adj: Omit<AllowanceDeduction, 'id'>) {
    const newAdj = { ...adj, id: `ad-${Date.now()}` }
    mockDB.adjustments.push(newAdj)
    const emp = mockDB.employees.find(e => e.id === adj.employeeId)
    writeAuditLog('Payroll & Report', 'CREATE_ADJUSTMENT', `Thêm cấu phần ${adj.type === 'Allowance' ? 'Thưởng' : 'Khấu trừ'} (${adj.title}) cho ${emp?.fullName ?? ''}`)
  },
  removeAdjustment(id: string) {
    const adj = mockDB.adjustments.find(a => a.id === id)
    if (adj) {
      mockDB.adjustments = mockDB.adjustments.filter(a => a.id !== id)
      const emp = mockDB.employees.find(e => e.id === adj.employeeId)
      writeAuditLog('Payroll & Report', 'REMOVE_ADJUSTMENT', `Xóa cấu phần ${adj.title} của ${emp?.fullName ?? ''}`)
    }
  },

  // Calculate Period Payroll
  calculatePeriod(periodId: string) {
    const period = mockDB.periods.find(p => p.id === periodId)
    if (!period) return
    if (period.isClosed) {
      throw new Error('Kỳ lương đã khóa. Không thể tính toán lại!')
    }

    // Filter payslips for this period
    mockDB.payslips = mockDB.payslips.filter(p => p.periodId !== periodId)

    // Calculate for all Active employees
    const activeEmployees = mockDB.employees.filter(e => e.status === 'Active')
    
    activeEmployees.forEach(emp => {
      // Find Contract
      const contract = mockDB.contracts.find(c => c.employeeId === emp.id && c.status === 'Active')
      const baseSalary = contract ? contract.baseSalary : 8000000 // Default minimal

      // Count attendance in this period range
      const start = new Date(period.startDate)
      const end = new Date(period.endDate)
      
      // Calculate Scheduled work days (weekdays: Mon to Fri)
      let workDays = 0
      for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
        const day = d.getDay()
        if (day !== 0 && day !== 6) { // Exclude Sat, Sun
          workDays++
        }
      }

      // Count actual work days from records
      const records = mockDB.attendanceRecords.filter(r => 
        r.employeeId === emp.id && 
        new Date(r.workDate) >= start && 
        new Date(r.workDate) <= end
      )

      let actualWorkDays = 0
      records.forEach(r => {
        if (r.status === 'Completed' || r.status === 'Late' || r.status === 'EarlyLeave') {
          actualWorkDays++
        } else if (r.status === 'OnLeave') {
          // Check if paid leave
          const leaves = mockDB.leaveRequests.filter(l => 
            l.employeeId === emp.id && 
            l.status === 'Approved' &&
            new Date(l.fromDate) <= new Date(r.workDate) &&
            new Date(l.toDate) >= new Date(r.workDate)
          )
          const isPaid = leaves.some(l => l.leaveType !== 'Unpaid')
          if (isPaid) {
            actualWorkDays++ // Paid leaves count as actual work day
          }
        }
      })

      // OT Calculation
      const otRecords = mockDB.otRequests.filter(o => 
        o.employeeId === emp.id && 
        o.status === 'Approved' && 
        new Date(o.date) >= start && 
        new Date(o.date) <= end
      )
      const totalOtMinutes = otRecords.reduce((sum, curr) => sum + curr.requestedMinutes, 0)
      const otHours = parseFloat((totalOtMinutes / 60).toFixed(1))
      
      // Base Hourly Rate (Base / 22 days / 8 hours)
      const hourlyRate = (baseSalary / 22) / 8
      const otAmount = Math.round(otHours * hourlyRate * 1.5) // 150% rate

      // Adjustments (Allowances & Deductions)
      const empAdjs = mockDB.adjustments.filter(a => a.employeeId === emp.id)
      const allowanceAmount = empAdjs.filter(a => a.type === 'Allowance').reduce((sum, curr) => sum + curr.amount, 0)
      const deductionAmount = empAdjs.filter(a => a.type === 'Deduction').reduce((sum, curr) => sum + curr.amount, 0)

      // Gross & Net Calculations
      const workedSalary = (baseSalary / workDays) * actualWorkDays
      const grossSalary = Math.round(workedSalary + otAmount + allowanceAmount)
      const netSalary = Math.round(grossSalary - deductionAmount)

      mockDB.payslips.push({
        id: `pay-${periodId}-${emp.id}`,
        periodId,
        employeeId: emp.id,
        baseSalary,
        workDays,
        actualWorkDays,
        otHours,
        otAmount,
        allowanceAmount,
        deductionAmount,
        grossSalary,
        netSalary,
        calculatedAt: new Date().toISOString(),
        status: 'Draft'
      })
    })

    writeAuditLog('Payroll & Report', 'CALCULATE_PAYROLL', `Tính lương kỳ ${period.name} cho ${activeEmployees.length} nhân viên`)
  },

  closePeriod(periodId: string) {
    const period = mockDB.periods.find(p => p.id === periodId)
    if (period && !period.isClosed) {
      period.isClosed = true
      period.closedAt = new Date().toISOString()
      const user = localStorage.getItem('active_user_name') || 'Phạm Minh D'
      period.closedBy = user

      // Update payslips of this period to Sent
      mockDB.payslips.filter(p => p.periodId === periodId).forEach(p => p.status = 'Paid')

      writeAuditLog('Payroll & Report', 'CLOSE_PAYROLL_PERIOD', `Khóa kỳ lương ${period.name}`)

      emitEvent('PayrollClosed', 'payroll-report', {
        periodId: period.id,
        periodName: period.name,
        closedBy: user
      })
    }
  }
}
