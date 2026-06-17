<script setup lang="ts">
import { onMounted } from 'vue'
import { useAuthStore } from './stores/auth'
import { RouterView } from 'vue-router'

const auth = useAuthStore()

// Khi app load, tự động gọi /auth/me nếu có token
onMounted(async () => {
  if (auth.token && !auth.user) {
    await auth.fetchMe()
  }
})
</script>

<template>
  <RouterView />
</template>
