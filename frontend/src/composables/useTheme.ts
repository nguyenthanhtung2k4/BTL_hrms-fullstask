import { ref, watch } from 'vue'

export type ThemeMode = 'light' | 'dark' | 'system'

const STORAGE_KEY = 'hrms-theme'
const themeMode = ref<ThemeMode>('system')

function getSystemTheme(): 'light' | 'dark' {
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

function applyTheme(mode: ThemeMode) {
  const resolved = mode === 'system' ? getSystemTheme() : mode
  document.documentElement.setAttribute('data-theme', resolved)
  if (resolved === 'dark') {
    document.documentElement.classList.add('dark')
  } else {
    document.documentElement.classList.remove('dark')
  }
}

// Watch system preference changes
const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
mediaQuery.addEventListener('change', () => {
  if (themeMode.value === 'system') applyTheme('system')
})

watch(themeMode, (mode) => {
  localStorage.setItem(STORAGE_KEY, mode)
  applyTheme(mode)
})

export function useTheme() {
  function initTheme() {
    const saved = localStorage.getItem(STORAGE_KEY) as ThemeMode | null
    themeMode.value = saved ?? 'system'
    applyTheme(themeMode.value)
  }

  function cycleTheme() {
    // light → dark → system → light
    const cycle: ThemeMode[] = ['light', 'dark', 'system']
    const idx = cycle.indexOf(themeMode.value)
    themeMode.value = cycle[(idx + 1) % cycle.length]
  }

  function setTheme(mode: ThemeMode) {
    themeMode.value = mode
  }

  function isDark() {
    if (themeMode.value === 'system') return getSystemTheme() === 'dark'
    return themeMode.value === 'dark'
  }

  return { themeMode, initTheme, cycleTheme, setTheme, isDark }
}
