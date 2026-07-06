import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'

export type LocaleCode = 'vi' | 'en'

const STORAGE_KEY = 'hrms-locale'
const currentLocale = ref<LocaleCode>('vi')

export function useLocale() {
  const { locale } = useI18n({ useScope: 'global' })

  function initLocale() {
    const saved = localStorage.getItem(STORAGE_KEY) as LocaleCode | null
    currentLocale.value = saved ?? 'vi'
    locale.value = currentLocale.value
  }

  function setLocale(lang: LocaleCode) {
    currentLocale.value = lang
    locale.value = lang
    localStorage.setItem(STORAGE_KEY, lang)
    document.documentElement.setAttribute('lang', lang)
  }

  function toggleLocale() {
    setLocale(currentLocale.value === 'vi' ? 'en' : 'vi')
  }

  onMounted(() => {
    initLocale()
  })

  return { currentLocale, initLocale, setLocale, toggleLocale }
}
