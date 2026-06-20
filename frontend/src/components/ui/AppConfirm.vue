<script setup lang="ts">
defineProps<{
  title: string
  message?: string
  confirmText?: string
  cancelText?: string
  danger?: boolean
  loading?: boolean
}>()

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()
</script>

<template>
  <Teleport to="body">
    <div
      class="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4 backdrop-blur-sm"
      @mousedown.self="emit('cancel')"
    >
      <div class="w-full max-w-sm rounded-2xl bg-white p-6 shadow-[0_24px_60px_rgba(15,23,42,0.18)]">
        <div class="flex items-start gap-3">
          <div
            :class="[
              'flex h-11 w-11 flex-shrink-0 items-center justify-center rounded-full',
              danger ? 'bg-red-100' : 'bg-amber-100',
            ]"
          >
            <svg
              :class="['h-5 w-5', danger ? 'text-red-600' : 'text-amber-600']"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 9v4m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"
              />
            </svg>
          </div>
          <div>
            <h3 class="text-lg font-semibold text-slate-900">{{ title }}</h3>
            <p v-if="message" class="mt-1 text-sm text-slate-600">{{ message }}</p>
          </div>
        </div>

        <div class="mt-5 flex justify-end gap-3">
          <button
            class="rounded-lg px-4 py-2 text-sm font-medium text-slate-600 transition-colors hover:bg-slate-100 hover:text-slate-900"
            type="button"
            :disabled="loading"
            @click="emit('cancel')"
          >
            {{ cancelText ?? 'Hủy' }}
          </button>
          <button
            :class="[
              'inline-flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-medium text-white transition-colors',
              danger
                ? 'bg-red-600 hover:bg-red-700 disabled:bg-red-300'
                : 'bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300',
            ]"
            type="button"
            :disabled="loading"
            @click="emit('confirm')"
          >
            <svg
              v-if="loading"
              class="h-4 w-4 animate-spin"
              fill="none"
              viewBox="0 0 24 24"
            >
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
            {{ confirmText ?? 'Xác nhận' }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
