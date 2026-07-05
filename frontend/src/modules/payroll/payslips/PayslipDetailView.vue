<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { payslipService } from '../../../services/payslip.service'
import { useToastStore } from '../../../stores/toast'
import { useAuthStore } from '../../../stores/auth'
import { exportPayslipPdf } from '../../../services/pdf.service'
import type { Payslip } from '../../../types/payroll.types'
import PageHeader from '../../../components/layout/PageHeader.vue'
import AppButton from '../../../components/ui/AppButton.vue'
import AppBadge from '../../../components/ui/AppBadge.vue'

const route = useRoute()
const router = useRouter()
const toast = useToastStore()
const auth = useAuthStore()
const { t } = useI18n()

const payslip = ref<Payslip | null>(null)
const loading = ref(true)
const pdfLoading = ref(false)

const isMyPayslip = computed(() => {
  return payslip.value?.employeeId === auth.employeeId
})

const breadcrumbs = computed(() => {
  const crumbs: { label: string; to?: string }[] = [{ label: t('nav.payroll') }]
  if (isMyPayslip.value) {
    crumbs.push({ label: t('nav.myPayslip'), to: '/payroll/my-payslip' })
  } else {
    crumbs.push({ label: t('nav.allPayslips'), to: '/payroll/payslips' })
  }
  crumbs.push({ label: payslip.value?.fullName ?? t('common.detail') })
  return crumbs
})

async function load() {
  try {
    payslip.value = await payslipService.getById(route.params.id as string)
  } catch {
    toast.error('Không tìm thấy phiếu lương')
    router.back()
  } finally {
    loading.value = false
  }
}

async function downloadPdf() {
  if (!payslip.value) return
  pdfLoading.value = true
  try {
    exportPayslipPdf(payslip.value)
    toast.success('Đã xuất PDF phiếu lương thành công')
  } catch {
    toast.error('Không thể xuất PDF')
  } finally {
    pdfLoading.value = false
  }
}

function fmtMoney(n: number) {
  return n.toLocaleString('vi-VN') + ' ₫'
}

function formatPeriod(name?: string) {
  if (!name) return 'Kỳ lương không rõ'
  return name.replace(/Luong\s+thang/gi, 'Tháng').replace(/Ky\s+luong/gi, 'Kỳ lương')
}

const earningItems = computed(() =>
  payslip.value?.items?.filter((i) => i.itemType !== 'Deduction') ?? []
)
const deductionItems = computed(() =>
  payslip.value?.items?.filter((i) => i.itemType === 'Deduction') ?? []
)

onMounted(load)
</script>

<template>
  <div class="payslip-detail">
    <PageHeader
      :title="payslip ? `Chi tiết phiếu lương — ${payslip.fullName}` : t('payroll.payslips')"
      :breadcrumbs="breadcrumbs"
    >
      <template #actions>
        <AppButton variant="secondary" size="sm" @click="router.back()">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
          </svg>
          {{ t('common.back') }}
        </AppButton>
        <AppButton v-if="payslip" variant="primary" size="sm" :loading="pdfLoading" @click="downloadPdf">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          {{ t('common.download') }} PDF
        </AppButton>
      </template>
    </PageHeader>

    <!-- Loading skeleton -->
    <div v-if="loading" class="space-y-4">
      <div class="skeleton-box h-24 rounded-xl" />
      <div class="skeleton-box h-72 rounded-xl" />
    </div>

    <!-- Payslip card -->
    <div v-else-if="payslip" class="payslip-card">
      <!-- ── Header Banner ─────────────────────────────────────────────────── -->
      <div class="payslip-header">
        <div class="payslip-header__badge">
          <AppBadge :status="payslip.status" />
        </div>
        <div class="payslip-header__label">{{ t('payroll.payslips') }}</div>
        <div class="payslip-header__period">{{ formatPeriod(payslip.periodName) }}</div>
        <div class="payslip-header__employee">
          {{ t('employee.fullName') }}: <strong>{{ payslip.fullName }}</strong>
          ({{ payslip.employeeCode }})
        </div>
      </div>

      <!-- ── Info Grid ──────────────────────────────────────────────────────── -->
      <div class="payslip-info-grid">
        <div class="payslip-info-row">
          <span class="payslip-info-label">{{ t('employee.code') }}</span>
          <span class="payslip-info-val font-mono">{{ payslip.employeeCode }}</span>
        </div>
        <div class="payslip-info-row">
          <span class="payslip-info-label">{{ t('payroll.baseSalary') }}</span>
          <span class="payslip-info-val font-mono">{{ fmtMoney(payslip.baseSalary) }}</span>
        </div>
        <div class="payslip-info-row">
          <span class="payslip-info-label">{{ t('payroll.workedDays') }}</span>
          <span class="payslip-info-val font-semibold" style="color: var(--color-success);">
            {{ payslip.workedDays?.toFixed(1) }} ngày
          </span>
        </div>
        <div class="payslip-info-row">
          <span class="payslip-info-label">{{ t('payroll.paidLeaveDays') }}</span>
          <span class="payslip-info-val">{{ payslip.paidLeaveDays?.toFixed(1) }} ngày</span>
        </div>
      </div>

      <div class="payslip-body">
        <!-- ── Earnings ───────────────────────────────────────────────────── -->
        <div class="payslip-section">
          <div class="payslip-section__header">
            <span class="payslip-section__icon payslip-section__icon--earning">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
            </span>
            <h4>Các khoản thu nhập</h4>
          </div>
          <table class="payslip-table">
            <thead>
              <tr>
                <th>Khoản thu nhập</th>
                <th class="text-right">Số tiền</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in earningItems" :key="item.id">
                <td>{{ item.name }}</td>
                <td class="text-right font-semibold" style="color: var(--color-success);">
                  +{{ fmtMoney(item.amount) }}
                </td>
              </tr>
              <tr class="payslip-table__total payslip-table__total--earning">
                <td class="font-bold">Tổng thu nhập (Gross)</td>
                <td class="text-right font-bold">{{ fmtMoney(payslip.grossSalary) }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- ── Deductions ─────────────────────────────────────────────────── -->
        <div class="payslip-section">
          <div class="payslip-section__header">
            <span class="payslip-section__icon payslip-section__icon--deduction">
              <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 12H4" />
              </svg>
            </span>
            <h4>Các khoản khấu trừ</h4>
          </div>
          <table class="payslip-table">
            <thead>
              <tr>
                <th>Khoản khấu trừ</th>
                <th class="text-right">Số tiền</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="deductionItems.length === 0">
                <td colspan="2" class="text-center italic" style="color: var(--text-tertiary);">
                  Không có khoản khấu trừ trong kỳ này
                </td>
              </tr>
              <tr v-for="item in deductionItems" :key="item.id">
                <td>{{ item.name }}</td>
                <td class="text-right font-semibold" style="color: var(--color-danger);">
                  -{{ fmtMoney(item.amount) }}
                </td>
              </tr>
              <tr class="payslip-table__total payslip-table__total--deduction">
                <td class="font-bold">Tổng các khoản khấu trừ</td>
                <td class="text-right font-bold" style="color: var(--color-danger);">
                  -{{ fmtMoney(payslip.totalDeduction) }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- ── Net Salary Banner ──────────────────────────────────────────── -->
        <div class="payslip-net">
          <div>
            <div class="payslip-net__label">Lương thực lĩnh (NET)</div>
            <div class="payslip-net__sub">Số tiền thực tế chuyển khoản vào tài khoản cá nhân</div>
          </div>
          <div class="payslip-net__amount">{{ fmtMoney(payslip.netSalary) }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.payslip-detail { max-width: 52rem; }

.skeleton-box {
  background-color: var(--bg-muted);
  animation: pulse 1.5s ease-in-out infinite;
}
@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.payslip-card {
  border-radius: var(--radius-xl);
  border: 1px solid var(--border);
  background-color: var(--bg-surface);
  box-shadow: var(--shadow-lg);
  overflow: hidden;
}

/* Header */
.payslip-header {
  position: relative;
  background: linear-gradient(135deg, hsl(160, 84%, 30%) 0%, hsl(185, 84%, 30%) 100%);
  padding: 1.75rem 2rem;
  color: white;
}
.payslip-header__badge {
  position: absolute;
  top: 1rem;
  right: 1.25rem;
}
.payslip-header__label {
  font-size: 0.6875rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.12em;
  opacity: 0.8;
}
.payslip-header__period {
  font-size: 1.375rem;
  font-weight: 700;
  margin-top: 0.25rem;
}
.payslip-header__employee {
  font-size: 0.875rem;
  opacity: 0.9;
  margin-top: 0.25rem;
}

/* Info grid */
.payslip-info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0;
  border-bottom: 1px solid var(--border);
  background-color: var(--bg-subtle);
}
.payslip-info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.875rem 1.5rem;
  border-bottom: 1px solid var(--border);
  font-size: 0.8125rem;
}
.payslip-info-label { color: var(--text-tertiary); }
.payslip-info-val { color: var(--text-primary); }

/* Body */
.payslip-body { padding: 1.5rem 2rem; display: flex; flex-direction: column; gap: 1.5rem; }

/* Section */
.payslip-section__header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}
.payslip-section__header h4 {
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: var(--text-secondary);
}
.payslip-section__icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 1.5rem;
  height: 1.5rem;
  border-radius: 0.375rem;
}
.payslip-section__icon--earning {
  background: var(--color-success-light);
  color: var(--color-success);
}
.payslip-section__icon--deduction {
  background: var(--color-danger-light);
  color: var(--color-danger);
}

/* Table */
.payslip-table {
  width: 100%;
  font-size: 0.875rem;
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  overflow: hidden;
  border-collapse: collapse;
}
.payslip-table thead tr {
  background-color: var(--bg-subtle);
  border-bottom: 1px solid var(--border);
}
.payslip-table th {
  padding: 0.625rem 1rem;
  font-weight: 500;
  color: var(--text-tertiary);
  font-size: 0.8125rem;
  text-align: left;
}
.payslip-table td {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid var(--border);
  color: var(--text-primary);
}
.payslip-table tbody tr:last-child td { border-bottom: none; }
.payslip-table tbody tr:hover td { background-color: var(--bg-subtle); }

.payslip-table__total td {
  background-color: var(--bg-subtle) !important;
  border-top: 2px solid var(--border-strong) !important;
}
.payslip-table__total--earning td { color: var(--color-success) !important; }

/* Net salary */
.payslip-net {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
  border-radius: var(--radius-lg);
  background: linear-gradient(135deg, hsl(160, 84%, 30%) 0%, hsl(185, 84%, 30%) 100%);
  padding: 1.25rem 1.75rem;
  color: white;
}
.payslip-net__label {
  font-size: 0.6875rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  opacity: 0.8;
}
.payslip-net__sub { font-size: 0.75rem; opacity: 0.7; margin-top: 0.25rem; }
.payslip-net__amount { font-size: 2rem; font-weight: 800; white-space: nowrap; }
</style>
