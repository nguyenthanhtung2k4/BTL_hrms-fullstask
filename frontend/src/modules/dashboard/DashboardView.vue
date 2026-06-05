<script setup lang="ts">
import { computed } from 'vue'
import { Activity, Database, GitPullRequest, Network } from '@lucide/vue'

const serviceRows = [
  { name: 'HR Core', database: 'HRMS_HrCoreDb', owner: 'Nhóm 7', port: '5001' },
  { name: 'Attendance', database: 'HRMS_AttendanceDb', owner: 'Nhóm 8', port: '5002' },
  { name: 'Payroll & Report', database: 'HRMS_PayrollReportDb', owner: 'Nhóm 9', port: '5003' },
]

const totalServices = computed(() => serviceRows.length)
</script>

<template>
  <section class="space-y-6">
    <div class="grid gap-4 md:grid-cols-4">
      <div class="rounded border border-slate-200 bg-white p-4">
        <Network class="text-emerald-600" :size="22" />
        <div class="mt-3 text-2xl font-semibold">{{ totalServices }}</div>
        <div class="text-sm text-slate-500">Backend services</div>
      </div>
      <div class="rounded border border-slate-200 bg-white p-4">
        <Database class="text-blue-600" :size="22" />
        <div class="mt-3 text-2xl font-semibold">3</div>
        <div class="text-sm text-slate-500">SQL databases</div>
      </div>
      <div class="rounded border border-slate-200 bg-white p-4">
        <Activity class="text-cyan-600" :size="22" />
        <div class="mt-3 text-2xl font-semibold">RabbitMQ</div>
        <div class="text-sm text-slate-500">Event bus</div>
      </div>
      <div class="rounded border border-slate-200 bg-white p-4">
        <GitPullRequest class="text-violet-600" :size="22" />
        <div class="mt-3 text-2xl font-semibold">PR</div>
        <div class="text-sm text-slate-500">Review flow</div>
      </div>
    </div>

    <section class="rounded border border-slate-200 bg-white">
      <div class="border-b border-slate-200 px-5 py-4">
        <h2 class="text-base font-semibold">Service map</h2>
      </div>
      <div class="overflow-x-auto">
        <table class="w-full min-w-[720px] text-left text-sm">
          <thead class="bg-slate-50 text-slate-500">
            <tr>
              <th class="px-5 py-3 font-medium">Service</th>
              <th class="px-5 py-3 font-medium">Database</th>
              <th class="px-5 py-3 font-medium">Owner</th>
              <th class="px-5 py-3 font-medium">Port</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr v-for="service in serviceRows" :key="service.name">
              <td class="px-5 py-3 font-medium text-slate-900">{{ service.name }}</td>
              <td class="px-5 py-3 text-slate-600">{{ service.database }}</td>
              <td class="px-5 py-3 text-slate-600">{{ service.owner }}</td>
              <td class="px-5 py-3 text-slate-600">localhost:{{ service.port }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </section>
</template>

