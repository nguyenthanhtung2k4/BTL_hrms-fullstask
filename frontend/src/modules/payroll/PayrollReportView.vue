<script setup lang="ts">
const modules = [
  'EmployeeProjection',
  'AttendanceProjection',
  'PayrollPeriod',
  'PayrollRule',
  'Allowance/Deduction',
  'Payslip',
  'Report',
  'Inbox/Outbox',
]
</script>

<template>
  <section class="space-y-5">
    <div>
      <h1 class="text-2xl font-semibold text-slate-950">Payroll & Report Service</h1>
      <p class="mt-1 text-sm text-slate-500">Nhóm 9 · HRMS_PayrollReportDb · localhost:5003</p>
    </div>

    <div class="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
      <div
        v-for="moduleName in modules"
        :key="moduleName"
        class="rounded border border-slate-200 bg-white p-4"
      >
        <div class="text-sm font-semibold text-slate-900">{{ moduleName }}</div>
        <div class="mt-3 h-2 rounded bg-slate-100">
          <div class="h-2 w-1/4 rounded bg-violet-600" />
        </div>
        <div class="mt-2 text-xs font-medium text-slate-500">Skeleton ready</div>
      </div>
    </div>
  </section>
</template>

