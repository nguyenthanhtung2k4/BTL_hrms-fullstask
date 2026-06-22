<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useNotificationStore } from '../../stores/notification'
import { Bell, Check, Eye, CalendarCheck, FileText } from '@lucide/vue'

const notificationStore = useNotificationStore()

onMounted(() => {
  notificationStore.fetchNotifications()
})

const notifications = computed(() => notificationStore.notifications)
const hasUnread = computed(() => notificationStore.unreadCount > 0)

function formatTime(dateStr: string) {
  const date = new Date(dateStr)
  return date.toLocaleString('vi-VN', {
    hour: '2-digit',
    minute: '2-digit',
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}

function getIconForType(type: string) {
  switch (type) {
    case 'ContractExpiry': return FileText
    case 'LeaveApproval': return CalendarCheck
    default: return Bell
  }
}

function getStyleForType(type: string) {
  switch (type) {
    case 'ContractExpiry':
      return {
        bg: 'rgba(239, 68, 68, 0.1)',
        color: '#ef4444'
      }
    case 'LeaveApproval':
      return {
        bg: 'rgba(34, 197, 94, 0.1)',
        color: '#22c55e'
      }
    default:
      return {
        bg: 'rgba(59, 130, 246, 0.1)',
        color: '#3b82f6'
      }
  }
}
</script>

<template>
  <div class="max-w-4xl mx-auto space-y-6">
    <!-- Header -->
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 class="text-2xl font-bold tracking-tight" style="color: var(--text-primary);">
          Thông báo của tôi
        </h1>
        <p class="text-sm" style="color: var(--text-tertiary);">
          Cập nhật phê duyệt đơn từ, kỳ lương và hết hạn hợp đồng
        </p>
      </div>
      <div class="flex items-center gap-2">
        <button
          v-if="hasUnread"
          class="flex items-center gap-2 rounded-lg px-3 py-1.5 text-xs font-semibold shadow-sm transition-all hover:scale-[1.02]"
          style="background: var(--color-primary-light); color: var(--color-primary-text);"
          @click="notificationStore.markAllAsRead"
        >
          <Check :size="14" />
          Đánh dấu tất cả đã đọc
        </button>
      </div>
    </div>

    <!-- Notification List -->
    <div
      v-if="notifications.length > 0"
      class="rounded-2xl border shadow-sm overflow-hidden"
      style="background: var(--bg-surface); border-color: var(--border);"
    >
      <div class="divide-y divide-gray-100 dark:divide-gray-800">
        <div
          v-for="item in notifications"
          :key="item.id"
          class="flex gap-4 p-4 transition-colors duration-150 items-start"
          :style="{
            background: item.isRead ? 'transparent' : 'rgba(var(--color-primary-rgb), 0.03)'
          }"
        >
          <!-- Type Icon -->
          <div
            class="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-xl"
            :style="{
              background: getStyleForType(item.type).bg,
              color: getStyleForType(item.type).color
            }"
          >
            <component :is="getIconForType(item.type)" :size="18" />
          </div>

          <!-- Content -->
          <div class="min-w-0 flex-1">
            <div class="flex items-start justify-between gap-2">
              <h3
                class="text-sm font-semibold truncate"
                :style="{ color: item.isRead ? 'var(--text-secondary)' : 'var(--text-primary)' }"
              >
                {{ item.title }}
              </h3>
              <span class="text-[10px] whitespace-nowrap" style="color: var(--text-tertiary);">
                {{ formatTime(item.createdAt) }}
              </span>
            </div>
            <p
              class="mt-1 text-xs leading-relaxed"
              :style="{ color: item.isRead ? 'var(--text-tertiary)' : 'var(--text-secondary)' }"
            >
              {{ item.content }}
            </p>
          </div>

          <!-- Mark read action button -->
          <div v-if="!item.isRead" class="flex-shrink-0">
            <button
              class="rounded-lg p-1.5 transition-colors hover:bg-gray-100 dark:hover:bg-gray-800"
              style="color: var(--text-tertiary);"
              title="Đánh dấu đã đọc"
              @click="notificationStore.markAsRead(item.id)"
            >
              <Eye :size="15" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div
      v-else
      class="flex flex-col items-center justify-center rounded-2xl border border-dashed p-12 text-center"
      style="border-color: var(--border); background: var(--bg-surface);"
    >
      <div class="rounded-full p-4 mb-4" style="background: var(--bg-subtle);">
        <Bell :size="32" style="color: var(--text-tertiary);" />
      </div>
      <h3 class="text-sm font-semibold" style="color: var(--text-primary);">Không có thông báo nào</h3>
      <p class="mt-1 text-xs" style="color: var(--text-tertiary);">
        Tất cả thông báo của bạn sẽ được hiển thị ở đây.
      </p>
    </div>
  </div>
</template>
