import { createRouter, createWebHistory } from 'vue-router'
import MainLayout from '../layouts/MainLayout.vue'
import LoginView from '../modules/auth/LoginView.vue'
import DashboardView from '../modules/dashboard/DashboardView.vue'
import HrCoreView from '../modules/hr/HrCoreView.vue'
import AttendanceView from '../modules/attendance/AttendanceView.vue'
import PayrollReportView from '../modules/payroll/PayrollReportView.vue'
import { useAuthStore } from '../stores/auth'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView,
    },
    {
      path: '/',
      component: MainLayout,
      meta: { requiresAuth: true },
      children: [
        { path: '', name: 'dashboard', component: DashboardView },
        { path: 'hr', name: 'hr', component: HrCoreView },
        { path: 'attendance', name: 'attendance', component: AttendanceView },
        { path: 'payroll', name: 'payroll', component: PayrollReportView },
      ],
    },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login' }
  }

  if (to.name === 'login' && auth.isAuthenticated) {
    return { name: 'dashboard' }
  }
})

