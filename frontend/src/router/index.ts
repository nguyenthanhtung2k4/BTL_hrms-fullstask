import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

// ── Layouts
import MainLayout from '../layouts/MainLayout.vue'

// ── Auth
import LoginView from '../modules/auth/LoginView.vue'

// ── Dashboard
const DashboardView = () => import('../modules/dashboard/DashboardView.vue')

// ── HR Module
const DepartmentListView = () => import('../modules/hr/departments/DepartmentListView.vue')
const PositionListView   = () => import('../modules/hr/positions/PositionListView.vue')
const EmployeeListView   = () => import('../modules/hr/employees/EmployeeListView.vue')
const EmployeeDetailView = () => import('../modules/hr/employees/EmployeeDetailView.vue')
const ContractListView   = () => import('../modules/hr/contracts/ContractListView.vue')

// ── Attendance Module
const ShiftListView        = () => import('../modules/attendance/shifts/ShiftListView.vue')
const WorkScheduleListView = () => import('../modules/attendance/work-schedules/WorkScheduleListView.vue')
const MyAttendanceView     = () => import('../modules/attendance/attendance/MyAttendanceView.vue')
const MyAttendanceListView = () => import('../modules/attendance/attendance/MyAttendanceListView.vue')
const AttendanceListView   = () => import('../modules/attendance/attendance/AttendanceListView.vue')
const LeaveListView        = () => import('../modules/attendance/leaves/LeaveListView.vue')
const TimesheetView        = () => import('../modules/attendance/timesheets/TimesheetView.vue')

// ── Payroll Module
const PeriodListView   = () => import('../modules/payroll/periods/PeriodListView.vue')
const PeriodDetailView = () => import('../modules/payroll/periods/PeriodDetailView.vue')
const RuleListView     = () => import('../modules/payroll/rules/RuleListView.vue')
const AllowanceListView = () => import('../modules/payroll/allowances/AllowanceListView.vue')
const DeductionListView = () => import('../modules/payroll/deductions/DeductionListView.vue')
const PayslipListView  = () => import('../modules/payroll/payslips/PayslipListView.vue')
const PayslipDetailView = () => import('../modules/payroll/payslips/PayslipDetailView.vue')
const MyPayslipView    = () => import('../modules/payroll/payslips/MyPayslipView.vue')
const ReportView       = () => import('../modules/payroll/reports/ReportView.vue')

// ── Admin Module
const UserManagementView = () => import('../modules/admin/UserManagementView.vue')

// ── Profile
const ProfileView = () => import('../modules/profile/ProfileView.vue')

// ── Notifications
const NotificationListView = () => import('../modules/notifications/NotificationListView.vue')

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    // Public
    { path: '/', name: 'login', component: LoginView },

    // Protected
    {
      path: '/',
      component: MainLayout,
      meta: { requiresAuth: true },
      children: [
        // Dashboard
        { path: 'dashboard', name: 'dashboard', component: DashboardView },

        // Notifications
        { path: 'notifications', name: 'notifications', component: NotificationListView },

        // ── HR Core
        {
          path: 'hr/departments',
          name: 'hr-departments',
          component: DepartmentListView,
          meta: { roles: ['Admin', 'HR', 'Manager', 'PayrollStaff', 'Employee'] },
        },
        {
          path: 'hr/positions',
          name: 'hr-positions',
          component: PositionListView,
          meta: { roles: ['Admin', 'HR', 'Manager', 'PayrollStaff', 'Employee'] },
        },
        {
          path: 'hr/employees',
          name: 'hr-employees',
          component: EmployeeListView,
          meta: { roles: ['Admin', 'HR', 'Manager', 'PayrollStaff'] },
        },
        {
          path: 'hr/employees/:id',
          name: 'hr-employee-detail',
          component: EmployeeDetailView,
          meta: { roles: ['Admin', 'HR', 'Manager', 'PayrollStaff'] },
        },
        {
          path: 'hr/contracts',
          name: 'hr-contracts',
          component: ContractListView,
          meta: { roles: ['Admin', 'HR', 'PayrollStaff'] },
        },

        // ── Attendance
        {
          path: 'attendance/shifts',
          name: 'attendance-shifts',
          component: ShiftListView,
          meta: { roles: ['Admin', 'HR', 'Manager', 'PayrollStaff'] },
        },
        {
          path: 'attendance/work-schedules',
          name: 'attendance-work-schedules',
          component: WorkScheduleListView,
          meta: { roles: ['Admin', 'HR', 'Manager', 'PayrollStaff', 'Employee'] },
        },
        {
          path: 'attendance/checkin',
          name: 'attendance-checkin',
          component: MyAttendanceView,
          // Tất cả roles được phép
        },
        {
          path: 'attendance/my-attendance',
          name: 'attendance-my-attendance',
          component: MyAttendanceListView,
          // Tất cả roles được phép
        },
        {
          path: 'attendance/records',
          name: 'attendance-records',
          component: AttendanceListView,
          meta: { roles: ['Admin', 'HR', 'Manager', 'PayrollStaff'] },
        },
        {
          path: 'attendance/leaves',
          name: 'attendance-leaves',
          component: LeaveListView,
          // Tất cả roles được phép — hiển thị khác nhau trong component
        },
        {
          path: 'attendance/timesheets',
          name: 'attendance-timesheets',
          component: TimesheetView,
        },

        // ── Payroll
        {
          path: 'payroll/periods',
          name: 'payroll-periods',
          component: PeriodListView,
          meta: { roles: ['Admin', 'PayrollStaff'] },
        },
        {
          path: 'payroll/periods/:id',
          name: 'payroll-period-detail',
          component: PeriodDetailView,
          meta: { roles: ['Admin', 'PayrollStaff'] },
        },
        {
          path: 'payroll/rules',
          name: 'payroll-rules',
          component: RuleListView,
          meta: { roles: ['Admin', 'PayrollStaff'] },
        },
        {
          path: 'payroll/allowances',
          name: 'payroll-allowances',
          component: AllowanceListView,
          meta: { roles: ['Admin', 'PayrollStaff', 'HR'] },
        },
        {
          path: 'payroll/deductions',
          name: 'payroll-deductions',
          component: DeductionListView,
          meta: { roles: ['Admin', 'PayrollStaff', 'HR'] },
        },
        {
          path: 'payroll/payslips',
          name: 'payroll-payslips',
          component: PayslipListView,
          meta: { roles: ['Admin', 'HR', 'Manager', 'PayrollStaff'] },
        },
        {
          path: 'payroll/payslips/:id',
          name: 'payroll-payslip-detail',
          component: PayslipDetailView,
        },
        {
          path: 'payroll/my-payslip',
          name: 'payroll-my-payslip',
          component: MyPayslipView,
        },
        {
          path: 'payroll/reports',
          name: 'payroll-reports',
          component: ReportView,
          meta: { roles: ['Admin', 'HR', 'PayrollStaff', 'Manager'] },
        },

        // ── Admin / Quản trị tài khoản
        {
          path: 'admin/users',
          name: 'admin-users',
          component: UserManagementView,
          meta: { roles: ['Admin'] },
        },

        // ── Profile (tất cả roles)
        {
          path: 'profile',
          name: 'profile',
          component: ProfileView,
        },
      ],
    },

    // Catch-all
    { path: '/:pathMatch(.*)*', redirect: '/' },
  ],
})

// ── Navigation Guard
router.beforeEach(async (to) => {
  const auth = useAuthStore()

  // Nếu có token nhưng chưa load user (ví dụ F5) → gọi /me
  if (auth.token && !auth.user && !auth.initialized) {
    await auth.fetchMe()
  }

  // Yêu cầu đăng nhập
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login' }
  }

  // Đang ở login nhưng đã xác thực → về dashboard
  if (to.name === 'login' && auth.isAuthenticated) {
    return { name: 'dashboard' }
  }

  // Ngăn Admin truy cập các chức năng check-in/check-out và chấm công cá nhân
  if (auth.isAuthenticated && auth.isAdmin && (to.name === 'attendance-checkin' || to.name === 'attendance-my-attendance')) {
    return { name: 'dashboard' }
  }

  // Kiểm tra role
  const requiredRoles = to.meta.roles as string[] | undefined
  if (requiredRoles && auth.isAuthenticated) {
    const hasAccess = requiredRoles.some((r) => auth.roles.includes(r))
    if (!hasAccess) {
      return { name: 'dashboard' } // redirect về dashboard thay vì 403
    }
  }
})

// ── Danh sách tiêu đề trang động
const titleMap: Record<string, string> = {
  'login': 'Đăng nhập',
  'dashboard': 'Tổng quan',
  'notifications': 'Thông báo',
  'hr-departments': 'Quản lý Phòng ban',
  'hr-positions': 'Quản lý Chức vụ',
  'hr-employees': 'Danh sách Nhân viên',
  'hr-employee-detail': 'Chi tiết Nhân viên',
  'hr-contracts': 'Quản lý Hợp đồng',
  'attendance-shifts': 'Quản lý Ca làm việc',
  'attendance-work-schedules': 'Lịch làm việc',
  'attendance-checkin': 'Check-In / Check-Out',
  'attendance-my-attendance': 'Chấm công của tôi',
  'attendance-records': 'Quản lý Chấm công',
  'attendance-leaves': 'Đăng ký Nghỉ phép',
  'attendance-timesheets': 'Bảng công tổng hợp',
  'payroll-periods': 'Chu kỳ Lương',
  'payroll-period-detail': 'Chi tiết Chu kỳ Lương',
  'payroll-rules': 'Quy tắc tính lương',
  'payroll-allowances': 'Quản lý Phụ cấp',
  'payroll-deductions': 'Quản lý Khấu trừ',
  'payroll-payslips': 'Danh sách Phiếu lương',
  'payroll-payslip-detail': 'Chi tiết Phiếu lương',
  'payroll-my-payslip': 'Phiếu lương của tôi',
  'payroll-reports': 'Báo cáo lương',
  'admin-users': 'Quản lý tài khoản',
  'profile': 'Hồ sơ cá nhân'
}

// ── Cập nhật Document Title tự động sau khi chuyển trang
router.afterEach((to) => {
  const baseTitle = 'Chấm Công Số'
  const pageTitle = to.name && titleMap[to.name as string] ? titleMap[to.name as string] : ''
  document.title = pageTitle ? `${pageTitle} | ${baseTitle}` : baseTitle
})
