import { ref, computed } from 'vue'
import { employeeService } from '../services/employee.service'
import { contractService } from '../services/contract.service'
import { leaveService } from '../services/leave.service'
import { payrollPeriodService } from '../services/payrollPeriod.service'
import { payslipService } from '../services/payslip.service'
import { reportService } from '../services/report.service'
import { attendanceService } from '../services/attendance.service'
import { useAuthStore } from '../stores/auth'
import type { Employee, Contract } from '../types/hr.types'
import type { LeaveRequest } from '../types/attendance.types'
import type { PayrollPeriod, Payslip, PayrollSummaryReport } from '../types/payroll.types'

export function useDashboard() {
  const authStore = useAuthStore()
  const loading = ref(true)

  const employees = ref<Employee[]>([])
  const contracts = ref<Contract[]>([])
  const allLeaves = ref<LeaveRequest[]>([])
  const periods = ref<PayrollPeriod[]>([])
  const allPayslips = ref<Payslip[]>([])
  const myPayslips = ref<Payslip[]>([])
  const myLeaves = ref<LeaveRequest[]>([])
  const reportRows = ref<PayrollSummaryReport[]>([])
  const attendanceRecords = ref<any[]>([])

  // ── computed helpers ──────────────────────────────────────────
  const now = new Date()
  const thisMonth = now.getMonth()
  const thisYear = now.getFullYear()
  const in30Days = new Date(now.getTime() + 30 * 864e5)

  // Format today's date to YYYY-MM-DD
  const todayStr = computed(() => {
    const y = now.getFullYear()
    const m = String(now.getMonth() + 1).padStart(2, '0')
    const d = String(now.getDate()).padStart(2, '0')
    return `${y}-${m}-${d}`
  })

  const activeEmployees = computed(() => employees.value.filter(e => e.status === 'Active'))
  const newHires = computed(() => employees.value.filter(e => {
    const d = new Date(e.hireDate)
    return d.getMonth() === thisMonth && d.getFullYear() === thisYear
  }))
  const expiringContracts = computed(() => contracts.value.filter(c => {
    if (!c.endDate || c.status !== 'Active') return false
    const end = new Date(c.endDate)
    return end >= now && end <= in30Days
  }))
  const pendingLeaves = computed(() => allLeaves.value.filter(l => l.status === 'Pending'))

  // Today attendance status
  const todayAttendance = computed(() => {
    const todayRecs = attendanceRecords.value.filter(r => r.date?.startsWith(todayStr.value))
    const checkedInCount = todayRecs.length
    const activeCount = activeEmployees.value.length || 1
    const rate = Math.round((checkedInCount / activeCount) * 100)
    return {
      checkedIn: checkedInCount,
      rate
    }
  })

  // Attendance breakdown by date (last 7 days)
  const attendanceHistory = computed(() => {
    const dates = Array.from({ length: 7 }, (_, i) => {
      const d = new Date()
      d.setDate(now.getDate() - i)
      const y = d.getFullYear()
      const m = String(d.getMonth() + 1).padStart(2, '0')
      const day = String(d.getDate()).padStart(2, '0')
      return `${y}-${m}-${day}`
    }).reverse()

    return dates.map(dateStr => {
      const count = attendanceRecords.value.filter(r => r.date?.startsWith(dateStr)).length
      // Get weekday name
      const dayName = new Date(dateStr).toLocaleDateString('vi-VN', { weekday: 'short' })
      return { date: dayName, count }
    })
  })

  // Payroll trend: group payslips by periodId → sum netSalary
  const payrollTrend = computed(() => {
    const map: Record<string, { name: string; net: number; gross: number; count: number }> = {}
    allPayslips.value.forEach(p => {
      const period = periods.value.find(pr => pr.id === p.payrollPeriodId)
      const key = p.payrollPeriodId
      if (!map[key]) map[key] = { name: period?.name ?? key.slice(0, 8), net: 0, gross: 0, count: 0 }
      map[key].net += p.netSalary
      map[key].gross += p.grossSalary
      map[key].count++
    })
    return Object.values(map).slice(-6)
  })

  // Employee status distribution
  const statusDist = computed(() => {
    const s = { Active: 0, Inactive: 0, OnLeave: 0, Resigned: 0 }
    employees.value.forEach(e => { s[e.status] = (s[e.status] ?? 0) + 1 })
    return s
  })

  // Dept distribution
  const deptDist = computed(() => {
    const map: Record<string, number> = {}
    employees.value.forEach(e => { if (e.departmentName) map[e.departmentName] = (map[e.departmentName] ?? 0) + 1 })
    return Object.entries(map).map(([name, count]) => ({ name, count })).sort((a, b) => b.count - a.count).slice(0, 8)
  })

  // Leave type distribution
  const leaveTypeDist = computed(() => {
    const map: Record<string, number> = {}
    allLeaves.value.forEach(l => { map[l.leaveTypeName] = (map[l.leaveTypeName] ?? 0) + 1 })
    return Object.entries(map).map(([name, count]) => ({ name, count }))
  })

  // Open/calculated periods that need action
  const periodsNeedAction = computed(() =>
    periods.value.filter(p => p.status === 'Draft' || p.status === 'Calculated')
  )

  // ── load functions ────────────────────────────────────────────
  async function load() {
    loading.value = true
    try {
      if (authStore.isHR || authStore.isAdmin) {
        await Promise.allSettled([
          employeeService.getAll().then(r => employees.value = r),
          contractService.getAll().then(r => contracts.value = r),
          leaveService.getAll({}).then(r => allLeaves.value = r),
          payrollPeriodService.getAll().then(r => periods.value = r),
          payslipService.getAll({}).then(r => allPayslips.value = r),
          attendanceService.getAll({}).then(r => attendanceRecords.value = r),
        ])
      } else if (authStore.isPayrollStaff) {
        const ps = await payrollPeriodService.getAll()
        periods.value = ps
        const latestClosed = ps.filter(p => p.status === 'Closed' || p.status === 'Calculated').at(-1)
        await Promise.allSettled([
          payslipService.getAll({}).then(r => allPayslips.value = r),
          latestClosed
            ? reportService.getSummary({ payrollPeriodId: latestClosed.id }).then(r => reportRows.value = r)
            : Promise.resolve(),
        ])
      } else if (authStore.isManager) {
        await Promise.allSettled([
          employeeService.getAll().then(r => employees.value = r),
          leaveService.getAll({ status: 'Pending' }).then(r => allLeaves.value = r),
          attendanceService.getAll({}).then(r => attendanceRecords.value = r),
        ])
      } else {
        // Employee
        const eid = authStore.user?.employeeId
        if (eid) {
          await Promise.allSettled([
            payslipService.getAll({ employeeId: eid }).then(r => myPayslips.value = r),
            leaveService.getAll({ employeeId: eid }).then(r => myLeaves.value = r),
            attendanceService.getAll({ employeeId: eid }).then(r => attendanceRecords.value = r),
          ])
        }
      }
    } finally {
      loading.value = false
    }
  }

  return {
    loading, employees, contracts, allLeaves, pendingLeaves, periods,
    allPayslips, myPayslips, myLeaves, reportRows, attendanceRecords,
    activeEmployees, newHires, expiringContracts, todayAttendance, attendanceHistory,
    payrollTrend, statusDist, deptDist, leaveTypeDist, periodsNeedAction,
    load,
  }
}
