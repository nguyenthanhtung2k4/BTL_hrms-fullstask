import { createI18n } from 'vue-i18n'
import vi from './vi.json'
import en from './en.json'

const STORAGE_KEY = 'hrms-locale'

function getInitialLocale(): 'vi' | 'en' {
  const saved = localStorage.getItem(STORAGE_KEY)
  if (saved === 'vi' || saved === 'en') return saved
  // Detect browser language
  const browserLang = navigator.language.toLowerCase()
  if (browserLang.startsWith('vi')) return 'vi'
  return 'vi' // default to Vietnamese
}

export const i18n = createI18n({
  legacy: false,           // use Composition API mode
  globalInjection: true,  // allow $t() in templates
  locale: getInitialLocale(),
  fallbackLocale: 'vi',
  messages: { vi, en },
  numberFormats: {
    vi: {
      currency: {
        style: 'decimal',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0,
      },
    },
    en: {
      currency: {
        style: 'decimal',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0,
      },
    },
  },
  datetimeFormats: {
    vi: {
      short: { day: '2-digit', month: '2-digit', year: 'numeric' },
      long:  { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' },
    },
    en: {
      short: { month: '2-digit', day: '2-digit', year: 'numeric' },
      long:  { month: '2-digit', day: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' },
    },
  },
})
