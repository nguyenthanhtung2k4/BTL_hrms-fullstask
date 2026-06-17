// Attendance Types

export interface Shift {
  id: string
  code: string
  name: string
  startTime: string   // "08:00"
  endTime: string     // "17:00"
  breakMinutes: number
  isActive: boolean
  createdAt: string
}

export interface CreateShiftDto {
  code: string
  name: string
  startTime: string
  endTime: string
  breakMinutes?: number
  isActive?: boolean
}

export interface UpdateShiftDto {
  name: string
  startTime: string
  endTime: string
  breakMinutes?: number
  isActive: boolean
}

// ---

export interface WorkSchedule {
  id: string
  employeeId: string
  employeeName: string
  shiftId: string
  shiftName: string
  startDate: string
  endDate: string
  isActive: boolean
  createdAt: string
}

export interface CreateWorkScheduleDto {
  employeeId: string
  shiftId: string
  startDate: string
  endDate: string
}

// ---

export interface AttendanceRecord {
  id: string
  employeeId: string
  employeeName: string
  workScheduleId?: string
  shiftName?: string
  checkInTime?: string
  checkOutTime?: string
  totalMinutesWorked: number
  notes?: string
  date: string
  createdAt: string
}

// ---

export type LeaveStatus = 'Pending' | 'Approved' | 'Rejected' | 'Cancelled'

export interface LeaveType {
  id: string
  code: string
  name: string
  isPaid: boolean
  isActive: boolean
}

export interface LeaveRequest {
  id: string
  employeeId: string
  employeeName: string
  leaveTypeId: string
  leaveTypeName: string
  isPaid: boolean
  fromDate: string
  toDate: string
  totalDays: number
  reason: string
  status: LeaveStatus
  approvedByEmployeeId?: string
  approvedByName?: string
  approvedAt?: string
  createdAt: string
  updatedAt?: string
}

export interface CreateLeaveRequestDto {
  leaveTypeId: string
  fromDate: string
  toDate: string
  reason: string
}

// ---

export interface Timesheet {
  id: string
  employeeId: string
  employeeName: string
  departmentName: string
  month: number
  year: number
  totalWorkDays: number
  totalPaidLeaveDays: number
  totalUnpaidLeaveDays: number
  totalAbsentDays: number
  totalOvertimeMinutes: number
}
