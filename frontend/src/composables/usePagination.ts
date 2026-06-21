import { ref, computed, watch, type Ref } from 'vue'

export function usePagination<T>(data: Ref<T[]>, defaultPerPage = 10) {
  const currentPage = ref(1)
  const perPage = ref(defaultPerPage)

  // Reset to page 1 when data or perPage changes
  watch(
    () => [data.value, perPage.value],
    () => {
      currentPage.value = 1
    },
    { deep: true }
  )

  const paginatedData = computed(() => {
    const start = (currentPage.value - 1) * perPage.value
    return data.value.slice(start, start + perPage.value)
  })

  return {
    currentPage,
    perPage,
    paginatedData,
    total: computed(() => data.value.length),
  }
}

