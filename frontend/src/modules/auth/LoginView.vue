<script setup lang="ts">
import { ref, onMounted, watch, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '../../stores/auth'
import { useToastStore } from '../../stores/toast'
import { useTheme } from '../../composables/useTheme'
import { extractError } from '../../services/apiClient'
import { 
  Mail, Lock, LogIn, AlertCircle, X, ArrowRight, 
  Users, Calendar, CreditCard, Layers, Briefcase, BarChart2,
  TrendingUp, Award, Shield, Sun, Moon
} from '@lucide/vue'
import gsap from 'gsap'
import { ScrollTrigger } from 'gsap/ScrollTrigger'

gsap.registerPlugin(ScrollTrigger)

import { useLocale } from '../../composables/useLocale'

const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()
const { t } = useI18n({ useScope: 'global' })
const { cycleTheme, isDark } = useTheme()
const { currentLocale, setLocale } = useLocale()

const langDropdownOpen = ref(false)
const langOptions = [
  { code: 'vi', label: 'Tiếng Việt' },
  { code: 'en', label: 'English' }
]

const email = ref('')
const password = ref('')
const errors = ref<{ email?: string; password?: string; general?: string }>({})
const showLoginModal = ref(false)

// Interactive Showcase State
const activeShowcase = ref(0)
const showcaseItems = [
  { title: "Tài chính & Doanh thu", desc: "Theo dõi dòng tiền, chi phí nhân sự và báo cáo doanh thu trực quan.", icon: TrendingUp },
  { title: "Nhân sự & Hợp đồng", desc: "Xem danh sách hồ sơ nhân viên, hợp đồng và trạng thái làm việc thời gian thực.", icon: Users },
  { title: "Phân tích cơ cấu", desc: "Báo cáo sơ đồ tổ chức, cơ cấu nhân viên giữa các phòng ban tự động.", icon: BarChart2 }
]

const showcaseEmps = [
  { name: "Nguyễn Thị Lan Anh", initials: "LA", color: "#3b82f6", role: "Trưởng phòng Nhân sự", status: "Online", statusClass: "online" },
  { name: "Trần Minh Hoàng", initials: "MH", color: "#10b981", role: "Kỹ sư Phần mềm", status: "Online", statusClass: "online" },
  { name: "Phạm Văn Đức", initials: "VĐ", color: "#f59e0b", role: "Kế toán viên", status: "Offline", statusClass: "offline" }
]

async function handleLogin() {
  errors.value = {}
  if (!email.value)    { errors.value.email    = t('validation.required'); return }
  if (!password.value) { errors.value.password = t('validation.required'); return }

  try {
    await auth.login(email.value, password.value)
    toast.success(`${t('dashboard.greeting_morning')}, ${auth.displayName}!`)
    if (auth.isHR || auth.isAdmin) router.push('/dashboard')
    else if (auth.isPayrollStaff)  router.push('/payroll/periods')
    else                           router.push('/attendance/checkin')
  } catch (err: any) {
    errors.value.general = extractError(err, t('auth.loginError'))
  }
}

// Watch for errors to trigger shake animation
watch(errors, (newVal) => {
  if (Object.keys(newVal).length > 0) {
    gsap.timeline()
      .to('.login-modal-card', { x: -6, duration: 0.08 })
      .to('.login-modal-card', { x: 6, duration: 0.08 })
      .to('.login-modal-card', { x: -4, duration: 0.08 })
      .to('.login-modal-card', { x: 4, duration: 0.08 })
      .to('.login-modal-card', { x: 0, duration: 0.08 })
  }
}, { deep: true })

function openModal() {
  showLoginModal.value = true
  nextTick(() => {
    gsap.fromTo('.login-modal-overlay',
      { opacity: 0 },
      { opacity: 1, duration: 0.4, ease: 'power2.out' }
    )
    gsap.fromTo('.login-modal-card',
      { scale: 0.9, opacity: 0, y: 30 },
      { scale: 1, opacity: 1, y: 0, duration: 0.5, ease: 'back.out(1.2)' }
    )
    gsap.fromTo('.login-animate-item',
      { opacity: 0, y: 15 },
      { opacity: 1, y: 0, duration: 0.6, stagger: 0.08, ease: 'power2.out', delay: 0.1 }
    )
  })
}

function closeModal() {
  gsap.timeline({
    onComplete: () => {
      showLoginModal.value = false
    }
  })
  .to('.login-modal-card', { scale: 0.9, opacity: 0, y: 20, duration: 0.3, ease: 'power2.in' })
  .to('.login-modal-overlay', { opacity: 0, duration: 0.2 }, '-=0.2')
}

function handleContact() {
  toast.info('Vui lòng liên hệ Phòng Hành chính - Nhân sự (Hotline: ext 102) hoặc gửi email đến hr@company.com để yêu cầu cấp tài khoản mới.')
}

function scrollToSection(selector: string) {
  const element = document.querySelector(selector)
  if (element) {
    element.scrollIntoView({ behavior: 'smooth' })
  }
}

onMounted(() => {
  // 1. Entrance animation for hero section elements
  gsap.fromTo('.landing-fade-in', 
    { opacity: 0, y: 25 },
    { opacity: 1, y: 0, duration: 0.8, stagger: 0.08, ease: 'power2.out' }
  )

  // 2. Mockup card hovering parallax drift
  gsap.fromTo('.landing-mockup-wrapper',
    { y: 0 },
    { y: -15, duration: 6, repeat: -1, yoyo: true, ease: 'sine.inOut' }
  )

  // 3. Floating animation for stats/mockup sub-elements
  gsap.fromTo('.landing-badge-wrapper-1',
    { y: 0 }, { y: -8, duration: 3.5, repeat: -1, yoyo: true, ease: 'sine.inOut', delay: 0.5 }
  )
  gsap.fromTo('.landing-badge-wrapper-2',
    { y: 0 }, { y: 8, duration: 4, repeat: -1, yoyo: true, ease: 'sine.inOut', delay: 1 }
  )

  // 4. ScrollTrigger: Parallax zoom & rise mockup on scroll
  gsap.to('.landing-mockup-wrapper', {
    scale: 1.06,
    yPercent: -10,
    scrollTrigger: {
      trigger: '.landing-hero-container',
      start: 'top top',
      end: 'bottom top',
      scrub: 1.5
    }
  })

  // ScrollTrigger: Parallax float badges on scroll
  gsap.to('.landing-badge-wrapper-1', {
    yPercent: -45,
    scrollTrigger: {
      trigger: '.landing-hero-container',
      start: 'top top',
      end: 'bottom top',
      scrub: 1.5
    }
  })
  gsap.to('.landing-badge-wrapper-2', {
    yPercent: 45,
    scrollTrigger: {
      trigger: '.landing-hero-container',
      start: 'top top',
      end: 'bottom top',
      scrub: 1.5
    }
  })

  // 5. ScrollTrigger: Stagger entry for feature cards
  gsap.fromTo('.features-header',
    { opacity: 0, y: 35 },
    {
      opacity: 1,
      y: 0,
      duration: 0.8,
      ease: 'power3.out',
      scrollTrigger: {
        trigger: '.landing-features-section',
        start: 'top 85%'
      }
    }
  )

  gsap.fromTo('.feature-card', 
    { opacity: 0, y: 30 },
    {
      opacity: 1,
      y: 0,
      duration: 0.6,
      stagger: 0.1,
      ease: 'power2.out',
      scrollTrigger: {
        trigger: '.landing-features-section',
        start: 'top 80%',
      }
    }
  )

  // 6. ScrollTrigger: Showcase Container Reveal
  gsap.fromTo('.showcase-container',
    { opacity: 0, y: 40 },
    {
      opacity: 1,
      y: 0,
      duration: 1,
      ease: 'power3.out',
      scrollTrigger: {
        trigger: '.landing-showcase-section',
        start: 'top 85%'
      }
    }
  )

  // 7. ScrollTrigger: Stats Cards reveal
  gsap.fromTo('.stat-card',
    { opacity: 0, scale: 0.9, y: 30 },
    {
      opacity: 1,
      scale: 1,
      y: 0,
      duration: 0.7,
      stagger: 0.1,
      ease: 'back.out(1.1)',
      scrollTrigger: {
        trigger: '.landing-stats-section',
        start: 'top 85%'
      }
    }
  )

  // 8. Count-up animation for stats KPI counters
  const statsElements = document.querySelectorAll('.stat-number-val')
  statsElements.forEach(el => {
    const target = parseFloat(el.getAttribute('data-target') || '0')
    const isFloat = el.getAttribute('data-float') === 'true'
    const progress = { value: 0 }
    gsap.to(progress, {
      value: target,
      duration: 2,
      ease: 'power2.out',
      scrollTrigger: {
        trigger: el,
        start: 'top 90%',
      },
      onUpdate: () => {
        if (isFloat) {
          el.textContent = progress.value.toFixed(1)
        } else {
          el.textContent = Math.floor(progress.value).toLocaleString()
        }
      }
    })
  })

  // 9. ScrollTrigger: Footer reveal
  gsap.fromTo('.landing-footer-detailed',
    { opacity: 0, y: 20 },
    {
      opacity: 1,
      y: 0,
      duration: 0.8,
      ease: 'power3.out',
      scrollTrigger: {
        trigger: '.landing-footer-detailed',
        start: 'top 90%'
      }
    }
  )

  // 10. ScrollTrigger: Sticky Scroll for Apple-style Showcase (Desktop only)
  const mm = gsap.matchMedia()
  mm.add('(min-width: 1024px)', () => {
    ScrollTrigger.create({
      trigger: '.landing-showcase-section',
      start: 'top 5%',
      end: '+=1200',
      pin: true,
      pinSpacing: true,
      scrub: 1,
      onUpdate: (self) => {
        const idx = Math.min(2, Math.floor(self.progress * 3))
        if (activeShowcase.value !== idx) {
          activeShowcase.value = idx
        }
      }
    })
  })
})
</script>

<template>
  <main class="login-page">
    <!-- Optimized Background Layer Container (Limits compositing to viewport) -->
    <div class="landing-bg-container">
      <!-- Ambient background blobs -->
      <div class="login-blob login-blob-1"></div>
      <div class="login-blob login-blob-2"></div>
      <div class="login-blob login-blob-3"></div>

      <!-- Frosted glass background blur filter layer -->
      <div class="login-bg-blur-overlay"></div>

      <!-- Premium tech grid background pattern -->
      <div class="login-bg-grid"></div>
    </div>

    <!-- STICKY HEADER BAR -->
    <header class="landing-header">
      <div class="landing-header-left" @click="scrollToSection('.login-page')" style="cursor: pointer;">
        <div class="landing-logo">
          <svg class="h-5.5 w-5.5" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <circle cx="11" cy="11" r="7" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
            <path d="M11 7V11H14" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            <circle cx="18" cy="18" r="4.5" fill="currentColor" stroke="#ffffff" stroke-width="1.5"/>
            <path d="M16.5 18L17.5 19L19.5 17" stroke="#ffffff" stroke-width="1.2" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </div>
        <div class="landing-logo-text">
          <span class="landing-brand-name">{{ t('auth.brandName') }}</span>
          <span class="landing-brand-subname">{{ t('auth.subnamePortal') }}</span>
        </div>
      </div>

      <nav class="landing-nav-menu">
        <a href="#" @click.prevent="scrollToSection('.login-page')" class="landing-nav-link">{{ t('auth.home') }}</a>
        <a href="#" @click.prevent="scrollToSection('.landing-features-section')" class="landing-nav-link">{{ t('auth.features') }}</a>
        <a href="#" @click.prevent="scrollToSection('.landing-showcase-section')" class="landing-nav-link">{{ t('auth.experience') }}</a>
        <a href="#" @click.prevent="scrollToSection('.landing-stats-section')" class="landing-nav-link">{{ t('auth.stats') }}</a>
      </nav>

      <div class="landing-header-right">
        <!-- Language Switcher -->
        <div class="relative">
          <button
            class="flex items-center gap-1.5 h-9 rounded-lg px-2.5 text-sm font-medium transition-colors border border-[var(--border-strong)] bg-[var(--bg-surface)] hover:bg-[var(--bg-subtle)]"
            :title="t('language.switch')"
            style="color: var(--text-secondary);"
            @click.stop="langDropdownOpen = !langDropdownOpen"
          >
            <span class="flex items-center">
              <template v-if="currentLocale === 'vi'">
                <svg class="w-5 h-3.5 rounded-sm object-cover shadow-sm border border-black/10" viewBox="0 0 30 20" xmlns="http://www.w3.org/2000/svg">
                  <rect width="30" height="20" fill="#da251d"/>
                  <polygon points="15,4 16.18,7.62 20,7.62 16.91,9.88 18.09,13.5 15,11.25 11.91,13.5 13.09,9.88 10,7.62 13.82,7.62" fill="#ffff00"/>
                </svg>
              </template>
              <template v-else-if="currentLocale === 'en'">
                <svg class="w-5 h-3.5 rounded-sm object-cover shadow-sm border border-black/10" viewBox="0 0 20 14" xmlns="http://www.w3.org/2000/svg">
                  <rect width="20" height="14" fill="#bb133e"/>
                  <path d="M0,1h20M0,3h20M0,5h20M0,7h20M0,9h20M0,11h20M0,13h20" stroke="#fff" stroke-width="1"/>
                  <rect width="8" height="8" fill="#002147"/>
                  <polygon points="1.5,1.8 1.8,2.8 2.8,2.8 2.0,3.4 2.3,4.4 1.5,3.8 0.7,4.4 1.0,3.4 0.2,2.8 1.2,2.8" fill="#fff"/>
                  <polygon points="4.0,1.8 4.3,2.8 5.3,2.8 4.5,3.4 4.8,4.4 4.0,3.8 3.2,4.4 3.5,3.4 2.7,2.8 3.7,2.8" fill="#fff"/>
                  <polygon points="6.5,1.8 6.8,2.8 7.8,2.8 7.0,3.4 7.3,4.4 6.5,3.8 5.7,4.4 6.0,3.4 5.2,2.8 6.2,2.8" fill="#fff"/>
                  <polygon points="1.5,4.3 1.8,5.3 2.8,5.3 2.0,5.9 2.3,6.9 1.5,6.3 0.7,6.9 1.0,5.9 0.2,5.3 1.2,5.3" fill="#fff"/>
                  <polygon points="4.0,4.3 4.3,5.3 5.3,5.3 4.5,5.9 4.8,6.9 4.0,6.3 3.2,6.9 3.5,5.9 2.7,5.3 3.7,5.3" fill="#fff"/>
                  <polygon points="6.5,4.3 6.8,5.3 7.8,5.3 7.0,5.9 7.3,6.9 6.5,6.3 5.7,6.9 6.0,5.9 5.2,5.3 6.2,5.3" fill="#fff"/>
                </svg>
              </template>
            </span>
            <span class="text-xs font-semibold hidden sm:block">{{ currentLocale.toUpperCase() }}</span>
          </button>

          <!-- Language Dropdown -->
          <div
            v-if="langDropdownOpen"
            class="absolute right-0 top-full mt-1.5 z-50 w-40 rounded-xl shadow-lg overflow-hidden border"
            style="background: var(--bg-surface); border-color: var(--border); box-shadow: var(--shadow-lg);"
          >
            <button
              v-for="lang in langOptions"
              :key="lang.code"
              class="w-full flex items-center gap-2.5 px-3 py-2 text-sm text-left transition-colors hover:bg-[var(--bg-subtle)]"
              :style="{
                background: currentLocale === lang.code ? 'var(--color-primary-light)' : 'transparent',
                color: currentLocale === lang.code ? 'var(--color-primary-text)' : 'var(--text-primary)',
                fontWeight: currentLocale === lang.code ? '600' : '400'
              }"
              @click="setLocale(lang.code as any); langDropdownOpen = false"
            >
              <span class="flex items-center">
                <template v-if="lang.code === 'vi'">
                  <svg class="w-5 h-3.5 rounded-sm object-cover shadow-sm border border-black/10" viewBox="0 0 30 20" xmlns="http://www.w3.org/2000/svg">
                    <rect width="30" height="20" fill="#da251d"/>
                    <polygon points="15,4 16.18,7.62 20,7.62 16.91,9.88 18.09,13.5 15,11.25 11.91,13.5 13.09,9.88 10,7.62 13.82,7.62" fill="#ffff00"/>
                  </svg>
                </template>
                <template v-else-if="lang.code === 'en'">
                  <svg class="w-5 h-3.5 rounded-sm object-cover shadow-sm border border-black/10" viewBox="0 0 20 14" xmlns="http://www.w3.org/2000/svg">
                    <rect width="20" height="14" fill="#bb133e"/>
                    <path d="M0,1h20M0,3h20M0,5h20M0,7h20M0,9h20M0,11h20M0,13h20" stroke="#fff" stroke-width="1"/>
                    <rect width="8" height="8" fill="#002147"/>
                    <polygon points="1.5,1.8 1.8,2.8 2.8,2.8 2.0,3.4 2.3,4.4 1.5,3.8 0.7,4.4 1.0,3.4 0.2,2.8 1.2,2.8" fill="#fff"/>
                    <polygon points="4.0,1.8 4.3,2.8 5.3,2.8 4.5,3.4 4.8,4.4 4.0,3.8 3.2,4.4 3.5,3.4 2.7,2.8 3.7,2.8" fill="#fff"/>
                    <polygon points="6.5,1.8 6.8,2.8 7.8,2.8 7.0,3.4 7.3,4.4 6.5,3.8 5.7,4.4 6.0,3.4 5.2,2.8 6.2,2.8" fill="#fff"/>
                    <polygon points="1.5,4.3 1.8,5.3 2.8,5.3 2.0,5.9 2.3,6.9 1.5,6.3 0.7,6.9 1.0,5.9 0.2,5.3 1.2,5.3" fill="#fff"/>
                    <polygon points="4.0,4.3 4.3,5.3 5.3,5.3 4.5,5.9 4.8,6.9 4.0,6.3 3.2,6.9 3.5,5.9 2.7,5.3 3.7,5.3" fill="#fff"/>
                    <polygon points="6.5,4.3 6.8,5.3 7.8,5.3 7.0,5.9 7.3,6.9 6.5,6.3 5.7,6.9 6.0,5.9 5.2,5.3 6.2,5.3" fill="#fff"/>
                  </svg>
                </template>
              </span>
              <span>{{ lang.label }}</span>
              <span v-if="currentLocale === lang.code" class="ml-auto text-xs">✓</span>
            </button>
          </div>
        </div>

        <button class="theme-toggle-btn" @click="cycleTheme" :title="isDark() ? 'Chế độ sáng' : 'Chế độ tối'" aria-label="Toggle Theme">
          <Sun v-if="isDark()" class="h-4.5 w-4.5 text-secondary hover:text-primary transition-colors" />
          <Moon v-else class="h-4.5 w-4.5 text-secondary hover:text-primary transition-colors" />
        </button>
        <button class="landing-header-btn btn-login" @click="openModal">{{ t('auth.login') }}</button>
      </div>
    </header>

    <!-- MAIN PORTAL SCROLL CONTENT -->
    <div class="landing-layout-wrapper">
      
      <!-- HERO SECTION -->
      <section class="landing-hero-container">
        <div class="landing-hero-left">
          <div class="landing-badge landing-fade-in">{{ t('auth.heroBadge') }}</div>
          <h1 class="landing-main-title landing-fade-in" v-html="t('auth.heroTitle')"></h1>
          <p class="landing-description landing-fade-in">
            {{ t('auth.heroDesc') }}
          </p>

          <div class="landing-cta-row landing-fade-in">
            <button class="landing-cta-btn landing-cta-btn--primary" @click="openModal">
              {{ t('auth.heroCta') }}
              <ArrowRight class="h-4.5 w-4.5" />
            </button>
            <button class="landing-cta-btn landing-cta-btn--secondary" @click="handleContact">
              {{ t('auth.requestAccount') }}
            </button>
          </div>
        </div>

        <div class="landing-hero-right landing-fade-in">
          <!-- Glassmorphic Dashboard Mockup Wrapper -->
          <div class="landing-mockup-wrapper">
            <div class="landing-mockup-card">
              <!-- Mockup Header -->
              <div class="mockup-header">
                <div class="mockup-header-left">
                  <span class="mockup-dot mockup-dot-red"></span>
                  <span class="mockup-dot mockup-dot-yellow"></span>
                  <span class="mockup-dot mockup-dot-green"></span>
                  <span class="mockup-header-title">HRMS PANEL PRO v4.2</span>
                </div>
                <div class="mockup-header-right">
                  <span class="mockup-status-pulse"></span>
                  <span class="mockup-status-text">Active System</span>
                </div>
              </div>

              <!-- Mockup Content -->
              <div class="mockup-content">
                <div class="mockup-sidebar">
                  <div class="mockup-sidebar-item active"></div>
                  <div class="mockup-sidebar-item"></div>
                  <div class="mockup-sidebar-item"></div>
                  <div class="mockup-sidebar-item"></div>
                </div>
                <div class="mockup-main">
                  <div class="mockup-stats-row">
                    <div class="mockup-mini-card">
                      <span class="mockup-mini-label">Nhân sự hiện diện</span>
                      <span class="mockup-mini-val text-primary">32 / 48 Online</span>
                    </div>
                    <div class="mockup-mini-card">
                      <span class="mockup-mini-label">Nghỉ phép chờ duyệt</span>
                      <span class="mockup-mini-val text-warning">3 Yêu cầu</span>
                    </div>
                  </div>
                  <div class="mockup-dashboard-grid">
                    <div class="mockup-chart-card">
                      <span class="mockup-card-label">Tỷ lệ đi làm đúng giờ</span>
                      <div class="mockup-chart-bars">
                        <div class="mockup-chart-bar-col"><div class="mockup-chart-bar" style="height: 65%;"></div></div>
                        <div class="mockup-chart-bar-col"><div class="mockup-chart-bar" style="height: 85%;"></div></div>
                        <div class="mockup-chart-bar-col"><div class="mockup-chart-bar" style="height: 98%;"></div></div>
                        <div class="mockup-chart-bar-col"><div class="mockup-chart-bar" style="height: 88%;"></div></div>
                        <div class="mockup-chart-bar-col"><div class="mockup-chart-bar" style="height: 95%;"></div></div>
                      </div>
                    </div>
                    <div class="mockup-members-card">
                      <span class="mockup-card-label">Check-In Trực Tuyến</span>
                      <div class="mockup-members-avatars">
                        <div class="mockup-avatar" style="background-color: #10b981; color: white;">AN</div>
                        <div class="mockup-avatar" style="background-color: #3b82f6; color: white;">TH</div>
                        <div class="mockup-avatar" style="background-color: #8b5cf6; color: white;">MD</div>
                      </div>
                      <span class="mockup-mini-subtext">+5 checked-in mới</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Floating Badges -->
          <div class="landing-badge-wrapper-1">
            <div class="landing-float-badge landing-float-badge-1">+20% Hiệu suất</div>
          </div>
          <div class="landing-badge-wrapper-2">
            <div class="landing-float-badge landing-float-badge-2">98.2% Đúng giờ</div>
          </div>
        </div>
      </section>

      <!-- FEATURES HIGHLIGHT SECTION -->
      <section class="landing-features-section">
        <div class="features-header">
          <span class="features-subtitle">HẠ TẦNG CHỨC NĂNG</span>
          <h2 class="features-title">Cung cấp bộ giải pháp cốt lõi cho doanh nghiệp</h2>
        </div>
        <div class="features-grid">
          <!-- HR Card -->
          <div class="feature-card">
            <div class="feature-icon-wrapper feat-hr">
              <Users class="h-6 w-6" />
            </div>
            <h3>Quản lý nhân sự</h3>
            <p>Hồ sơ nhân viên số hóa, theo dõi hợp đồng lao động, lịch sử công tác và sơ đồ tổ chức phòng ban.</p>
          </div>
          <!-- Attendance Card -->
          <div class="feature-card">
            <div class="feature-icon-wrapper feat-time">
              <Calendar class="h-6 w-6" />
            </div>
            <h3>Chấm công</h3>
            <p>Check-in qua bản đồ GPS, ghi nhận ca làm việc hành chính hoặc ca gãy và đăng ký xin nghỉ phép trực tuyến.</p>
          </div>
          <!-- Payroll Card -->
          <div class="feature-card">
            <div class="feature-icon-wrapper feat-pay">
              <CreditCard class="h-6 w-6" />
            </div>
            <h3>Tính lương tự động</h3>
            <p>Chốt bảng công, áp dụng quy tắc tính lương chuẩn, khấu trừ bảo hiểm xã hội và thuế TNCN tự động.</p>
          </div>
          <!-- Departments Card -->
          <div class="feature-card">
            <div class="feature-icon-wrapper feat-dept">
              <Layers class="h-6 w-6" />
            </div>
            <h3>Quản lý phòng ban</h3>
            <p>Chia nhóm quyền lực theo ban chỉ đạo, phân loại nhân sự chính xác và hỗ trợ giao việc nội bộ.</p>
          </div>
          <!-- Recruitment Card -->
          <div class="feature-card">
            <div class="feature-icon-wrapper feat-rec">
              <Briefcase class="h-6 w-6" />
            </div>
            <h3>Tuyển dụng</h3>
            <p>Khởi tạo các chiến dịch tìm kiếm tài năng, phân nhóm ứng viên ứng tuyển và thiết lập lịch phỏng vấn.</p>
          </div>
          <!-- Analytics Card -->
          <div class="feature-card">
            <div class="feature-icon-wrapper feat-rep">
              <BarChart2 class="h-6 w-6" />
            </div>
            <h3>Báo cáo &amp; Thống kê</h3>
            <p>Bảng phân tích trực quan về năng lực lao động, biến động chi phí nhân sự và năng suất làm việc.</p>
          </div>
        </div>
      </section>

      <!-- APPLE-STYLE STICKY SHOWCASE -->
      <section class="landing-showcase-section">
        <div class="showcase-container">
          <div class="showcase-left-sticky">
            <span class="showcase-subtitle">GIAO DIỆN HỆ THỐNG</span>
            <h2 class="showcase-title">Trung tâm điều hành doanh nghiệp</h2>
            <p class="showcase-desc-side">Nhân viên và Quản trị viên tương tác trực quan trên các bảng dữ liệu đồng bộ thời gian thực.</p>
            
            <div class="showcase-menu">
              <button 
                v-for="(item, idx) in showcaseItems" 
                :key="idx"
                :class="['showcase-menu-btn', activeShowcase === idx ? 'active' : '']"
                @click="activeShowcase = idx"
              >
                <component :is="item.icon" class="h-5 w-5" />
                <div class="text-left">
                  <h4>{{ item.title }}</h4>
                  <p>{{ item.desc }}</p>
                </div>
              </button>
            </div>
          </div>

          <div class="showcase-right-view">
            <div class="showcase-display-card">
              <!-- Slide 1: Revenue Chart -->
              <div :class="['showcase-pane', activeShowcase === 0 ? 'showcase-pane--active' : '']">
                <div class="pane-header">
                  <span>Doanh thu & Chi phí Nhân sự</span>
                  <TrendingUp class="h-4.5 w-4.5 text-primary" />
                </div>
                <div class="pane-chart-container">
                  <svg viewBox="0 0 400 200" class="w-full h-full">
                    <line x1="10" y1="20" x2="390" y2="20" stroke="var(--border)" stroke-dasharray="4" />
                    <line x1="10" y1="70" x2="390" y2="70" stroke="var(--border)" stroke-dasharray="4" />
                    <line x1="10" y1="120" x2="390" y2="120" stroke="var(--border)" stroke-dasharray="4" />
                    <line x1="10" y1="170" x2="390" y2="170" stroke="var(--border)" />
                    
                    <defs>
                      <linearGradient id="chartGrad" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stop-color="var(--color-primary)" stop-opacity="0.35"/>
                        <stop offset="100%" stop-color="var(--color-primary)" stop-opacity="0"/>
                      </linearGradient>
                    </defs>
                    <path d="M10 170 C 80 125, 150 145, 220 70 C 290 90, 340 40, 390 15 L 390 170 Z" fill="url(#chartGrad)" />
                    <path d="M10 170 C 80 125, 150 145, 220 70 C 290 90, 340 40, 390 15" fill="none" stroke="var(--color-primary)" stroke-width="3" />
                    <circle cx="220" cy="70" r="5.5" fill="var(--color-primary)" stroke="white" stroke-width="2" />
                    <circle cx="390" cy="15" r="5.5" fill="var(--color-primary)" stroke="white" stroke-width="2" />
                  </svg>
                </div>
                <div class="pane-footer">
                  <div class="stat">
                    <span>Năng suất quý</span>
                    <strong>+28.5%</strong>
                  </div>
                  <div class="stat">
                    <span>Quy mô nhân sự</span>
                    <strong style="color: var(--color-success-text);">Tăng 12%</strong>
                  </div>
                </div>
              </div>

              <!-- Slide 2: Employee List -->
              <div :class="['showcase-pane', activeShowcase === 1 ? 'showcase-pane--active' : '']">
                <div class="pane-header">
                  <span>Trạng thái Nhân viên</span>
                  <Users class="h-4.5 w-4.5 text-primary" />
                </div>
                <div class="pane-list-container">
                  <div class="pane-list-item" v-for="emp in showcaseEmps" :key="emp.name">
                    <div class="avatar" :style="{ backgroundColor: emp.color }">{{ emp.initials }}</div>
                    <div class="info">
                      <strong>{{ emp.name }}</strong>
                      <span>{{ emp.role }}</span>
                    </div>
                    <span class="status-tag" :class="emp.statusClass">{{ emp.status }}</span>
                  </div>
                </div>
              </div>

              <!-- Slide 3: Department Struct -->
              <div :class="['showcase-pane', activeShowcase === 2 ? 'showcase-pane--active' : '']">
                <div class="pane-header">
                  <span>Tỷ lệ nhân lực phòng ban</span>
                  <BarChart2 class="h-4.5 w-4.5 text-primary" />
                </div>
                <div class="pane-donut-container">
                  <svg viewBox="0 0 200 200" class="w-36 h-36">
                    <circle cx="100" cy="100" r="70" fill="none" stroke="var(--border)" stroke-width="16" />
                    <circle cx="100" cy="100" r="70" fill="none" stroke="#10b981" stroke-width="16" stroke-dasharray="440" stroke-dashoffset="110" />
                    <circle cx="100" cy="100" r="70" fill="none" stroke="#3b82f6" stroke-width="16" stroke-dasharray="440" stroke-dashoffset="330" />
                    <circle cx="100" cy="100" r="70" fill="none" stroke="#8b5cf6" stroke-width="16" stroke-dasharray="440" stroke-dashoffset="400" />
                    <text x="100" y="95" text-anchor="middle" font-weight="800" font-size="22" fill="var(--text-primary)">48</text>
                    <text x="100" y="115" text-anchor="middle" font-size="10" fill="var(--text-secondary)" font-weight="700">Thành viên</text>
                  </svg>
                  <div class="donut-legends">
                    <div class="legend"><span class="dot" style="background-color: #10b981;"></span> IT (55%)</div>
                    <div class="legend"><span class="dot" style="background-color: #3b82f6;"></span> HR (25%)</div>
                    <div class="legend"><span class="dot" style="background-color: #8b5cf6;"></span> Kế toán (20%)</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- KPI STATISTICS SECTION -->
      <section class="landing-stats-section">
        <div class="features-header">
          <span class="features-subtitle">SỐ LIỆU ĐIỀU HÀNH</span>
          <h2 class="features-title">Hiệu suất vận hành doanh nghiệp đo lường được</h2>
        </div>
        <div class="stats-grid">
          <div class="stat-card">
            <TrendingUp class="stat-card-icon" />
            <div class="stat-number"><span class="stat-number-val" data-target="98.2" data-float="true">0</span>%</div>
            <span class="stat-label">Chấm công đúng giờ</span>
          </div>
          <div class="stat-card">
            <Award class="stat-card-icon" />
            <div class="stat-number"><span class="stat-number-val" data-target="1250" data-float="false">0</span>+</div>
            <span class="stat-label">Giao dịch dữ liệu/giây</span>
          </div>
          <div class="stat-card">
            <Users class="stat-card-icon" />
            <div class="stat-number">+<span class="stat-number-val" data-target="20" data-float="false">0</span>%</div>
            <span class="stat-label">Hiệu suất phòng ban</span>
          </div>
          <div class="stat-card">
            <Shield class="stat-card-icon" />
            <div class="stat-number"><span class="stat-number-val" data-target="0" data-float="false">0</span></div>
            <span class="stat-label">Sai sót chu kỳ tính lương</span>
          </div>
        </div>
      </section>

      <!-- DARK FOOTER -->
      <footer class="landing-footer-detailed">
        <div class="footer-columns">
          <div class="footer-col">
            <h5 class="footer-col-title">HRMS Portal</h5>
            <p class="footer-company-desc">Cổng thông tin quản trị và hỗ trợ nghiệp vụ nhân sự, chấm công trực tuyến dành riêng cho cán bộ nhân viên công ty.</p>
          </div>
          <div class="footer-col">
            <h5 class="footer-col-title">Công cụ cá nhân</h5>
            <a href="#" @click.prevent="openModal" class="footer-link">Báo cáo chấm công cá nhân</a>
            <a href="#" @click.prevent="openModal" class="footer-link">Đăng ký nghỉ phép trực tuyến</a>
            <a href="#" @click.prevent="openModal" class="footer-link">Tra cứu phiếu lương hàng tháng</a>
          </div>
          <div class="footer-col">
            <h5 class="footer-col-title">Quy trình nội bộ</h5>
            <a href="#" @click.prevent="openModal" class="footer-link">Thông tin sơ đồ tổ chức</a>
            <a href="#" @click.prevent="openModal" class="footer-link">Quy chế & chính sách tính lương</a>
            <a href="#" @click.prevent="openModal" class="footer-link">Hỗ trợ CNTT & Kỹ thuật</a>
          </div>
          <div class="footer-col">
            <h5 class="footer-col-title">Thông tin liên hệ</h5>
            <span class="footer-contact-item">📧 Email: support@company.com</span>
            <span class="footer-contact-item">📞 Tổng đài nội bộ: ext 102</span>
            <span class="footer-contact-item">📍 Kênh hỗ trợ: Slack #hrms-helpdesk</span>
          </div>
        </div>
        <div class="footer-bottom-row">
          <p class="footer-copyright-text">&copy; 2026 HRMS Workspace. Thiết kế nội bộ công ty.</p>
          <div class="footer-socials">
            <a href="#" @click.prevent class="footer-social-link">Intranet Portal</a>
            <a href="#" @click.prevent class="footer-social-link">Quy chuẩn bảo mật</a>
          </div>
        </div>
      </footer>

    </div>

    <!-- MODAL PORTAL (LOGIN FORM) -->
    <div v-if="showLoginModal" class="login-modal-overlay" @click.self="closeModal">
      <!-- Card Modal -->
      <div class="login-card login-modal-card">
        <!-- Close Button -->
        <button class="login-modal-close-btn" @click="closeModal">
          <X class="h-5 w-5" />
        </button>

        <!-- Logo + Title -->
        <div class="login-logo-row login-animate-item">
          <div class="login-logo-icon">
            <svg class="h-6 w-6" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <circle cx="11" cy="11" r="7" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
              <path d="M11 7V11H14" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
              <circle cx="18" cy="18" r="4.5" fill="currentColor" stroke="#ffffff" stroke-width="1.5"/>
              <path d="M16.5 18L17.5 19L19.5 17" stroke="#ffffff" stroke-width="1.2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
          </div>
          <div>
            <h1 class="login-title">{{ t('auth.titlePortal') }}</h1>
            <p class="login-subtitle">{{ t('auth.subtitlePortal') }}</p>
          </div>
        </div>

        <!-- General error -->
        <div v-if="errors.general" class="login-error login-animate-item">
          <AlertCircle class="h-4.5 w-4.5 flex-shrink-0 mt-0.5" />
          <span>{{ errors.general }}</span>
        </div>

        <!-- Form -->
        <form class="login-form" @submit.prevent="handleLogin">
          <!-- Email -->
          <div class="login-field login-animate-item">
            <label class="login-label" for="login-email">
              {{ t('auth.email') }} <span class="login-required">*</span>
            </label>
            <div class="login-input-wrapper">
              <span class="login-input-icon">
                <Mail class="h-4.5 w-4.5" />
              </span>
              <input
                id="login-email"
                v-model="email"
                type="email"
                :placeholder="t('auth.emailPlaceholder')"
                autocomplete="email"
                :class="['login-input', errors.email ? 'login-input--error' : '']"
              />
            </div>
            <p v-if="errors.email" class="login-field-error">{{ errors.email }}</p>
          </div>

          <!-- Password -->
          <div class="login-field login-animate-item">
            <label class="login-label" for="login-password">
              {{ t('auth.password') }} <span class="login-required">*</span>
            </label>
            <div class="login-input-wrapper">
              <span class="login-input-icon">
                <Lock class="h-4.5 w-4.5" />
              </span>
              <input
                id="login-password"
                v-model="password"
                type="password"
                :placeholder="t('auth.passwordPlaceholder')"
                autocomplete="current-password"
                :class="['login-input', errors.password ? 'login-input--error' : '']"
              />
            </div>
            <p v-if="errors.password" class="login-field-error">{{ errors.password }}</p>
          </div>

          <!-- Submit -->
          <button type="submit" :disabled="auth.loading" class="login-submit login-animate-item">
            <svg v-if="auth.loading" class="h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
            <LogIn v-else class="h-4 w-4" />
            {{ auth.loading ? t('auth.loginLoading') : t('auth.loginButton') }}
          </button>
        </form>
      </div>
    </div>
  </main>
</template>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&family=Inter:wght@400;500;600;700&display=swap');

.login-page {
  position: relative;
  min-height: 100vh;
  width: 100%;
  display: flex;
  flex-direction: column;
  background-color: var(--bg-page);
  overflow-x: hidden;
  font-family: 'Inter', sans-serif;
  transition: background-color var(--transition-base);
}

/* Optimized Background Layer Container */
.landing-bg-container {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 100vh;
  overflow: hidden;
  pointer-events: none;
  z-index: 1;
}

/* Ambient background blobs */
.login-blob {
  position: absolute;
  border-radius: 50%;
  filter: blur(120px);
  opacity: 0.16;
  z-index: 1;
  pointer-events: none;
  will-change: transform;
}
.login-blob-1 {
  width: 600px;
  height: 600px;
  background: radial-gradient(circle, var(--color-primary) 0%, rgba(59, 130, 246, 0) 70%);
  top: -10%;
  left: -5%;
  animation: float-blob-1 25s infinite ease-in-out;
}
.login-blob-2 {
  width: 650px;
  height: 650px;
  background: radial-gradient(circle, #10b981 0%, rgba(16, 185, 129, 0) 70%);
  bottom: 0%;
  right: -5%;
  animation: float-blob-2 28s infinite ease-in-out;
}
.login-blob-3 {
  width: 450px;
  height: 450px;
  background: radial-gradient(circle, #8b5cf6 0%, rgba(139, 92, 246, 0) 70%);
  top: 40%;
  left: 10%;
  animation: float-blob-3 22s infinite ease-in-out;
}

@keyframes float-blob-1 {
  0% { transform: translate3d(0, 0, 0) scale(1); }
  50% { transform: translate3d(40px, -60px, 0) scale(1.05); }
  100% { transform: translate3d(0, 0, 0) scale(1); }
}
@keyframes float-blob-2 {
  0% { transform: translate3d(0, 0, 0) scale(1); }
  50% { transform: translate3d(-50px, 50px, 0) scale(0.95); }
  100% { transform: translate3d(0, 0, 0) scale(1); }
}
@keyframes float-blob-3 {
  0% { transform: translate3d(0, 0, 0) scale(1); }
  50% { transform: translate3d(30px, 30px, 0) scale(1.1); }
  100% { transform: translate3d(0, 0, 0) scale(1); }
}

/* Frosted glass background overlay */
.login-bg-blur-overlay {
  position: absolute;
  inset: 0;
  background-color: color-mix(in srgb, var(--bg-page) 45%, transparent);
  pointer-events: none;
  z-index: 2;
}

/* Tech grid overlay */
.login-bg-grid {
  position: absolute;
  inset: 0;
  background-image: 
    linear-gradient(color-mix(in srgb, var(--border) 16%, transparent) 1px, transparent 1px),
    linear-gradient(90deg, color-mix(in srgb, var(--border) 16%, transparent) 1px, transparent 1px);
  background-size: 60px 60px;
  background-position: center center;
  mask-image: radial-gradient(circle, black 30%, transparent 85%);
  -webkit-mask-image: radial-gradient(circle, black 30%, transparent 85%);
  pointer-events: none;
  z-index: 3;
}

/* STICKY HEADER STYLE */
.landing-header {
  position: sticky;
  top: 0;
  z-index: 100;
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  background-color: color-mix(in srgb, var(--bg-page) 65%, transparent);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1.25rem 2rem;
  border-bottom: 1px solid color-mix(in srgb, var(--border) 35%, transparent);
  width: 100%;
}

.landing-header-left {
  display: flex;
  align-items: center;
  gap: 0.875rem;
}

.landing-logo {
  display: grid;
  place-items: center;
  width: 2.25rem;
  height: 2.25rem;
  border-radius: 10px;
  background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-primary-hover) 100%);
  color: white;
  box-shadow: 0 4px 12px color-mix(in srgb, var(--color-primary) 25%, transparent);
}

.landing-logo-text {
  display: flex;
  flex-direction: column;
}

.landing-brand-name {
  font-family: 'Plus Jakarta Sans', sans-serif;
  font-size: 1.0625rem;
  font-weight: 800;
  letter-spacing: -0.02em;
  color: var(--text-primary);
  line-height: 1.2;
}

.landing-brand-subname {
  font-size: 0.625rem;
  font-weight: 700;
  color: var(--text-secondary);
  letter-spacing: 0.05em;
  text-transform: uppercase;
}

.landing-nav-menu {
  display: none;
  align-items: center;
  gap: 1.75rem;
}
@media (min-width: 1024px) {
  .landing-nav-menu {
    display: flex;
  }
}

.landing-nav-link {
  font-family: 'Plus Jakarta Sans', sans-serif;
  font-size: 0.8125rem;
  font-weight: 700;
  color: var(--text-secondary);
  text-decoration: none;
  transition: color var(--transition-fast);
}
.landing-nav-link:hover {
  color: var(--color-primary);
}

.landing-header-right {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.landing-header-btn {
  font-family: 'Plus Jakarta Sans', sans-serif;
  padding: 0.5rem 1.125rem;
  font-size: 0.8125rem;
  font-weight: 700;
  border-radius: 10px;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.btn-login {
  border: 1px solid var(--border-strong);
  background-color: var(--bg-surface);
  color: var(--text-secondary);
}
.btn-login:hover {
  background-color: var(--bg-subtle);
  color: var(--text-primary);
}

.theme-toggle-btn {
  display: grid;
  place-items: center;
  width: 2.25rem;
  height: 2.25rem;
  border-radius: 10px;
  border: 1px solid var(--border-strong);
  background-color: var(--bg-surface);
  color: var(--text-secondary);
  cursor: pointer;
  transition: all var(--transition-fast);
}
.theme-toggle-btn:hover {
  background-color: var(--bg-subtle);
  color: var(--color-primary);
  transform: scale(1.05) rotate(12deg);
}

/* CONTENT LAYOUT WRAPPER */
.landing-layout-wrapper {
  position: relative;
  z-index: 10;
  width: 100%;
  max-width: 80rem;
  margin: 0 auto;
  padding: 0 2rem;
  display: flex;
  flex-direction: column;
}

/* HERO SECTION */
.landing-hero-container {
  position: relative;
  display: grid;
  grid-template-columns: 1fr;
  gap: 4rem;
  align-items: center;
  padding: 3rem 0;
  min-height: calc(100vh - 80px);
  border-bottom: 1px solid color-mix(in srgb, var(--border) 25%, transparent);
}
.landing-hero-container::before {
  content: '';
  position: absolute;
  top: -10%;
  left: -20%;
  width: 140%;
  height: 120%;
  background: 
    radial-gradient(at 10% 20%, rgba(16, 185, 129, 0.08) 0px, transparent 55%),
    radial-gradient(at 90% 10%, rgba(59, 130, 246, 0.06) 0px, transparent 50%),
    radial-gradient(at 50% 80%, rgba(139, 92, 246, 0.04) 0px, transparent 50%);
  filter: blur(80px);
  pointer-events: none;
  z-index: -1;
}
@media (min-width: 1024px) {
  .landing-hero-container {
    grid-template-columns: 1.1fr 0.9fr;
  }
}

.landing-badge {
  display: inline-flex;
  padding: 0.375rem 0.875rem;
  font-size: 0.6875rem;
  font-weight: 700;
  font-family: 'Plus Jakarta Sans', sans-serif;
  letter-spacing: 0.05em;
  border-radius: 9999px;
  color: var(--color-primary-text);
  background-color: var(--color-primary-light);
  border: 1px solid color-mix(in srgb, var(--color-primary) 15%, transparent);
  margin-bottom: 1.5rem;
}

.landing-main-title {
  font-family: 'Plus Jakarta Sans', sans-serif;
  font-size: 2.25rem;
  font-weight: 800;
  line-height: 1.15;
  letter-spacing: -0.035em;
  color: var(--text-primary);
  margin: 0 0 1.25rem;
}
@media (min-width: 640px) {
  .landing-main-title { font-size: 3.25rem; }
}

.gradient-text {
  background: linear-gradient(135deg, var(--color-primary) 0%, #10b981 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.landing-description {
  font-size: 1.0625rem;
  line-height: 1.65;
  color: var(--text-secondary);
  margin: 0 0 2.5rem;
  max-width: 38rem;
}

.landing-cta-row {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
}

.landing-cta-btn {
  font-family: 'Plus Jakarta Sans', sans-serif;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.875rem 1.75rem;
  font-size: 0.9375rem;
  font-weight: 700;
  border-radius: 12px;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.landing-cta-btn--primary {
  border: none;
  background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-primary-hover) 100%);
  color: white;
  box-shadow: 0 4px 15px color-mix(in srgb, var(--color-primary) 25%, transparent);
}
.landing-cta-btn--primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px color-mix(in srgb, var(--color-primary) 35%, transparent);
}

.landing-cta-btn--secondary {
  border: 1px solid var(--border-strong);
  background-color: var(--bg-surface);
  color: var(--text-primary);
}
.landing-cta-btn--secondary:hover {
  background-color: var(--bg-subtle);
  transform: translateY(-2px);
}

.landing-hero-right {
  display: flex;
  justify-content: center;
  align-items: center;
  position: relative;
}

/* Floating badges inside Hero Wrapper */
.landing-badge-wrapper-1 {
  position: absolute;
  top: 10%;
  left: -10%;
  z-index: 15;
  will-change: transform;
}
.landing-badge-wrapper-2 {
  position: absolute;
  bottom: 15%;
  right: -8%;
  z-index: 15;
  will-change: transform;
}

/* Floating badges inside Hero */
.landing-float-badge {
  position: relative;
  padding: 0.625rem 1rem;
  font-size: 0.6875rem;
  font-weight: 700;
  font-family: 'Plus Jakarta Sans', sans-serif;
  border-radius: 12px;
  border: 1px solid color-mix(in srgb, var(--border) 40%, transparent);
  background: color-mix(in srgb, var(--bg-surface) 80%, transparent);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  box-shadow: var(--shadow-md);
  transition: transform var(--transition-fast);
  will-change: transform;
}
.landing-float-badge:hover {
  transform: scale(1.05) translateY(-2px);
}
.landing-float-badge-1 { color: var(--color-success-text); }
.landing-float-badge-2 { color: var(--color-primary-text); }

/* Dashboard Mockup Wrapper */
.landing-mockup-wrapper {
  width: 100%;
  max-width: 32rem;
  will-change: transform;
}

/* Dashboard Mockup styling */
.landing-mockup-card {
  width: 100%;
  max-width: 32rem;
  border-radius: 20px;
  border: 1px solid color-mix(in srgb, var(--border) 45%, transparent);
  background: color-mix(in srgb, var(--bg-surface) 65%, transparent);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  box-shadow: 0 30px 60px rgba(0,0,0,0.12);
  overflow: hidden;
  transition: transform 0.5s var(--ease-out);
  will-change: transform;
}
.landing-mockup-card:hover {
  transform: perspective(1000px) rotateY(-2deg) rotateX(1deg) translateY(-4px);
}

.mockup-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.875rem 1.25rem;
  border-bottom: 1px solid color-mix(in srgb, var(--border) 35%, transparent);
  background-color: rgba(0, 0, 0, 0.02);
}

.mockup-header-left { display: flex; align-items: center; gap: 0.375rem; }
.mockup-dot { width: 0.5rem; height: 0.5rem; border-radius: 50%; }
.mockup-dot-red { background-color: #ef4444; }
.mockup-dot-yellow { background-color: #f59e0b; }
.mockup-dot-green { background-color: #10b981; }
.mockup-header-title { font-size: 0.6875rem; font-weight: 700; color: var(--text-secondary); margin-left: 0.5rem; font-family: monospace; letter-spacing: 0.05em; }
.mockup-header-right { display: flex; align-items: center; gap: 0.5rem; }
.mockup-status-pulse { width: 0.375rem; height: 0.375rem; border-radius: 50%; background-color: var(--color-success); position: relative; }
.mockup-status-pulse::after { content: ''; position: absolute; inset: -2px; border-radius: 50%; border: 1px solid var(--color-success); animation: ping 1.5s cubic-bezier(0, 0, 0.2, 1) infinite; }
.mockup-status-text { font-size: 0.625rem; font-weight: 600; color: var(--text-tertiary); }

.mockup-content {
  display: flex;
  height: 14rem;
}
.mockup-sidebar {
  width: 3rem;
  border-right: 1px solid color-mix(in srgb, var(--border) 35%, transparent);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  padding-top: 1.25rem;
  background-color: rgba(0, 0, 0, 0.01);
}
.mockup-sidebar-item {
  width: 1.5rem;
  height: 1.5rem;
  border-radius: 6px;
  background-color: color-mix(in srgb, var(--border) 60%, transparent);
  transition: all var(--transition-fast);
}
.mockup-sidebar-item.active {
  background-color: var(--color-primary);
  box-shadow: 0 2px 8px color-mix(in srgb, var(--color-primary) 40%, transparent);
}
.mockup-main {
  flex: 1;
  padding: 1.25rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.mockup-stats-row {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
}
.mockup-mini-card {
  display: flex;
  flex-direction: column;
  background-color: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 0.625rem 0.875rem;
  box-shadow: var(--shadow-sm);
}
.mockup-mini-label {
  font-size: 0.625rem;
  font-weight: 600;
  color: var(--text-tertiary);
  text-transform: uppercase;
  letter-spacing: 0.02em;
}
.mockup-mini-val {
  font-size: 0.875rem;
  font-weight: 800;
  color: var(--text-primary);
  margin-top: 2px;
}
.mockup-mini-val.text-warning {
  color: var(--color-warning-text);
}
.mockup-dashboard-grid {
  display: grid;
  grid-template-columns: 1.2fr 0.8fr;
  gap: 1rem;
  flex: 1;
}
.mockup-chart-card, .mockup-members-card {
  background-color: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 14px;
  padding: 0.75rem;
  display: flex;
  flex-direction: column;
  justify-content: flex-start;
  box-shadow: var(--shadow-sm);
}
.mockup-card-label {
  font-size: 0.625rem;
  font-weight: 700;
  color: var(--text-secondary);
  margin-bottom: 0.5rem;
  align-self: flex-start;
}
.mockup-chart-bars {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  width: 100%;
  height: 4rem;
  padding-bottom: 2px;
}
.mockup-chart-bar-col {
  flex: 1;
  display: flex;
  justify-content: center;
}
.mockup-chart-bar {
  width: 0.5rem;
  border-radius: 4px;
  background: linear-gradient(to top, var(--color-primary) 30%, #10b981 100%);
  box-shadow: 0 2px 5px color-mix(in srgb, var(--color-primary) 15%, transparent);
}
.mockup-members-card {
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}
.mockup-members-avatars {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-top: 0.25rem;
}
.mockup-avatar {
  width: 1.75rem;
  height: 1.75rem;
  border-radius: 50%;
  border: 2px solid var(--bg-surface);
  display: grid;
  place-items: center;
  font-size: 0.625rem;
  font-weight: 700;
  margin-left: -0.375rem;
  box-shadow: var(--shadow-sm);
}
.mockup-avatar:first-child {
  margin-left: 0;
}
.mockup-mini-subtext {
  font-size: 0.5625rem;
  font-weight: 600;
  color: var(--text-tertiary);
}

/* BRAND MARQUEE STYLE */
.logo-marquee-container {
  overflow: hidden;
  white-space: nowrap;
  width: 100%;
  padding: 2.25rem 0;
  border-top: 1px solid color-mix(in srgb, var(--border) 45%, transparent);
  border-bottom: 1px solid color-mix(in srgb, var(--border) 45%, transparent);
  background-color: color-mix(in srgb, var(--bg-surface) 35%, transparent);
  margin-bottom: 2rem;
}
.logo-marquee-track {
  display: inline-flex;
  gap: 4rem;
  animation: marquee 30s linear infinite;
}
.logo-marquee-item {
  display: flex;
  gap: 4rem;
  align-items: center;
}
.logo-marquee-item span {
  font-family: 'Plus Jakarta Sans', sans-serif;
  font-size: 1.25rem;
  font-weight: 800;
  color: var(--text-tertiary);
  opacity: 0.45;
  transition: all var(--transition-fast);
  cursor: default;
}
.logo-marquee-item span:hover {
  opacity: 0.95;
  color: var(--color-primary);
  transform: scale(1.05);
}
@keyframes marquee {
  0% { transform: translateX(0); }
  100% { transform: translateX(-50%); }
}

/* FEATURES SECTION */
.landing-features-section {
  padding: 6rem 0;
  border-bottom: 1px solid color-mix(in srgb, var(--border) 25%, transparent);
}
.features-header {
  text-align: center;
  margin-bottom: 4rem;
}
.features-subtitle {
  font-family: 'Plus Jakarta Sans', sans-serif;
  display: inline-block;
  font-size: 0.6875rem;
  font-weight: 800;
  letter-spacing: 0.1em;
  color: var(--color-primary-text);
  background-color: var(--color-primary-light);
  padding: 0.25rem 0.875rem;
  border-radius: 9999px;
  margin-bottom: 1rem;
  text-transform: uppercase;
}
.features-title {
  font-family: 'Plus Jakarta Sans', sans-serif;
  font-size: 2rem;
  font-weight: 800;
  color: var(--text-primary);
  letter-spacing: -0.03em;
  margin: 0;
}
.features-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 2rem;
}
@media (min-width: 640px) {
  .features-grid { grid-template-columns: repeat(2, 1fr); }
}
@media (min-width: 1024px) {
  .features-grid { grid-template-columns: repeat(3, 1fr); }
}
.feature-card {
  background-color: var(--bg-surface);
  border: 1px solid color-mix(in srgb, var(--border) 50%, transparent);
  border-radius: 16px;
  padding: 2.25rem 2rem;
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
  transition: border-color var(--transition-base), background-color var(--transition-base), box-shadow var(--transition-base);
  will-change: transform, opacity;
}
.feature-card:hover {
  transform: translateY(-6px);
  border-color: color-mix(in srgb, var(--color-primary) 50%, transparent);
  box-shadow: var(--shadow-lg);
  background-color: color-mix(in srgb, var(--bg-surface) 80%, transparent);
}
.feature-card:hover .feature-icon-wrapper {
  transform: rotate(6deg) scale(1.05);
}
.feature-icon-wrapper {
  display: grid;
  place-items: center;
  width: 3.25rem;
  height: 3.25rem;
  border-radius: 12px;
  color: white;
  transition: transform var(--transition-fast);
}
.feat-hr {
  background: linear-gradient(135deg, #10b981 0%, #047857 100%);
  box-shadow: 0 6px 15px rgba(16, 185, 129, 0.2);
}
.feat-time {
  background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
  box-shadow: 0 6px 15px rgba(59, 130, 246, 0.2);
}
.feat-pay {
  background: linear-gradient(135deg, #8b5cf6 0%, #6d28d9 100%);
  box-shadow: 0 6px 15px rgba(139, 92, 246, 0.2);
}
.feat-dept {
  background: linear-gradient(135deg, #ef4444 0%, #b91c1c 100%);
  box-shadow: 0 6px 15px rgba(239, 68, 68, 0.2);
}
.feat-rec {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  box-shadow: 0 6px 15px rgba(245, 158, 11, 0.2);
}
.feat-rep {
  background: linear-gradient(135deg, #06b6d4 0%, #0891b2 100%);
  box-shadow: 0 6px 15px rgba(6, 182, 212, 0.2);
}
.feature-card h3 {
  font-family: 'Plus Jakarta Sans', sans-serif;
  font-size: 1.25rem;
  font-weight: 800;
  color: var(--text-primary);
  margin: 0;
}
.feature-card p {
  font-size: 0.875rem;
  color: var(--text-secondary);
  line-height: 1.6;
  margin: 0;
}



/* APPLE STYLE STICKY SHOWCASE */
.landing-showcase-section {
  padding: 6.5rem 0;
  border-bottom: 1px solid color-mix(in srgb, var(--border) 25%, transparent);
}
.showcase-container {
  display: grid;
  grid-template-columns: 1fr;
  gap: 4rem;
}
@media (min-width: 1024px) {
  .showcase-container {
    grid-template-columns: 0.85fr 1.15fr;
  }
}
.showcase-left-sticky {
  position: relative;
}
.showcase-subtitle {
  font-family: 'Plus Jakarta Sans', sans-serif;
  font-size: 0.6875rem;
  font-weight: 800;
  letter-spacing: 0.1em;
  color: var(--color-primary-text);
  background-color: var(--color-primary-light);
  padding: 0.25rem 0.875rem;
  border-radius: 9999px;
  text-transform: uppercase;
}
.showcase-title {
  font-family: 'Plus Jakarta Sans', sans-serif;
  font-size: 2rem;
  font-weight: 800;
  color: var(--text-primary);
  margin: 1.25rem 0 0.875rem;
  letter-spacing: -0.03em;
  line-height: 1.25;
}
.showcase-desc-side {
  font-size: 0.9375rem;
  color: var(--text-secondary);
  line-height: 1.6;
  margin-bottom: 2.25rem;
}
.showcase-menu {
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
}
.showcase-menu-btn {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding: 1.125rem;
  border: 1px solid color-mix(in srgb, var(--border) 50%, transparent);
  border-radius: 12px;
  background-color: transparent;
  cursor: pointer;
  transition: all var(--transition-base);
}
.showcase-menu-btn:hover {
  background-color: color-mix(in srgb, var(--bg-surface) 60%, transparent);
  border-color: var(--border-strong);
}
.showcase-menu-btn.active {
  background-color: var(--bg-surface);
  border-color: var(--color-primary);
  box-shadow: var(--shadow-md);
}
.showcase-menu-btn.active svg {
  color: var(--color-primary);
}
.showcase-menu-btn svg {
  color: var(--text-tertiary);
  margin-top: 2px;
  transition: color var(--transition-fast);
}
.showcase-menu-btn h4 {
  font-family: 'Plus Jakarta Sans', sans-serif;
  font-size: 0.9375rem;
  font-weight: 700;
  color: var(--text-primary);
  margin: 0;
}
.showcase-menu-btn p {
  font-size: 0.75rem;
  color: var(--text-secondary);
  margin: 0.25rem 0 0;
}

.showcase-right-view {
  display: flex;
  align-items: center;
  justify-content: center;
}
.showcase-display-card {
  width: 100%;
  max-width: 32rem;
  background-color: var(--bg-surface);
  border: 1px solid var(--border);
  box-shadow: var(--shadow-xl);
  border-radius: 20px;
  min-height: 23rem;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  position: relative;
}
.showcase-pane {
  position: absolute;
  inset: 0;
  padding: 1.75rem;
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
  opacity: 0;
  visibility: hidden;
  pointer-events: none;
  transition: opacity 0.4s ease, transform 0.4s ease, visibility 0.4s ease;
  transform: translateY(15px) scale(0.98);
  will-change: transform, opacity;
}
.showcase-pane--active {
  opacity: 1;
  visibility: visible;
  pointer-events: auto;
  transform: translateY(0) scale(1);
}

.pane-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 0.8125rem;
  font-weight: 700;
  color: var(--text-secondary);
  border-bottom: 1px solid var(--border);
  padding-bottom: 0.75rem;
}
.pane-chart-container {
  height: 10rem;
  display: flex;
  align-items: center;
}
.pane-footer {
  display: flex;
  justify-content: space-between;
  border-top: 1px solid var(--border);
  padding-top: 0.875rem;
}
.pane-footer .stat {
  display: flex;
  flex-direction: column;
}
.pane-footer .stat span {
  font-size: 0.625rem;
  font-weight: 600;
  color: var(--text-tertiary);
  text-transform: uppercase;
}
.pane-footer .stat strong {
  font-size: 1rem;
  font-weight: 800;
  color: var(--text-primary);
}

/* Slide 2 Specifics */
.pane-list-container {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}
.pane-list-item {
  display: flex;
  align-items: center;
  gap: 0.875rem;
  padding: 0.625rem;
  border: 1px solid var(--border);
  border-radius: 10px;
}
.pane-list-item .avatar {
  width: 2rem;
  height: 2rem;
  border-radius: 50%;
  display: grid;
  place-items: center;
  color: white;
  font-size: 0.75rem;
  font-weight: 700;
}
.pane-list-item .info {
  flex: 1;
  display: flex;
  flex-direction: column;
}
.pane-list-item .info strong {
  font-size: 0.8125rem;
  font-weight: 700;
  color: var(--text-primary);
}
.pane-list-item .info span {
  font-size: 0.6875rem;
  color: var(--text-tertiary);
}
.status-tag {
  font-size: 0.625rem;
  font-weight: 700;
  padding: 0.125rem 0.5rem;
  border-radius: 9999px;
}
.status-tag.online {
  background-color: var(--color-success-light);
  color: var(--color-success-text);
}
.status-tag.offline {
  background-color: var(--bg-muted);
  color: var(--text-secondary);
}

/* Slide 3 Specifics */
.pane-donut-container {
  display: flex;
  align-items: center;
  justify-content: space-around;
  flex: 1;
}
.donut-legends {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.donut-legends .legend {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-secondary);
}
.donut-legends .dot {
  width: 0.5rem;
  height: 0.5rem;
  border-radius: 50%;
}

/* KPI STATISTICS SECTION */
.landing-stats-section {
  padding: 6rem 0;
  border-bottom: 1px solid color-mix(in srgb, var(--border) 25%, transparent);
}
.stats-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 2rem;
}
@media (min-width: 768px) {
  .stats-grid {
    grid-template-columns: repeat(4, 1fr);
  }
}
.stat-card {
  background-color: color-mix(in srgb, var(--bg-surface) 60%, transparent);
  border: 1px solid color-mix(in srgb, var(--border) 40%, transparent);
  border-radius: 16px;
  padding: 2.25rem 1.75rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 0.75rem;
  transition: transform var(--transition-base);
}
.stat-card:hover {
  transform: translateY(-4px);
}
.stat-card-icon {
  width: 2rem;
  height: 2rem;
  color: var(--color-primary);
  opacity: 0.85;
}
.stat-number {
  font-family: 'Plus Jakarta Sans', sans-serif;
  font-size: 2.25rem;
  font-weight: 800;
  color: var(--text-primary);
  letter-spacing: -0.03em;
}
.stat-label {
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--text-secondary);
}



/* FOOTER */
.landing-footer-detailed {
  padding: 5rem 0 2.5rem;
  border-top: 1px solid color-mix(in srgb, var(--border) 25%, transparent);
  display: flex;
  flex-direction: column;
  gap: 3.5rem;
}
.footer-columns {
  display: grid;
  grid-template-columns: 1fr;
  gap: 2.5rem;
}
@media (min-width: 640px) {
  .footer-columns { grid-template-columns: repeat(2, 1fr); }
}
@media (min-width: 1024px) {
  .footer-columns { grid-template-columns: repeat(4, 1fr); }
}
.footer-col {
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
}
.footer-col-title {
  font-family: 'Plus Jakarta Sans', sans-serif;
  font-size: 0.875rem;
  font-weight: 800;
  color: var(--text-primary);
  letter-spacing: 0.05em;
  text-transform: uppercase;
  margin: 0 0 0.25rem;
}
.footer-company-desc {
  font-size: 0.8125rem;
  color: var(--text-secondary);
  line-height: 1.6;
  margin: 0;
}

.footer-link {
  font-size: 0.8125rem;
  color: var(--text-secondary);
  text-decoration: none;
  transition: color var(--transition-fast);
}
.footer-link:hover { color: var(--color-primary); }
.footer-contact-item {
  font-size: 0.8125rem;
  color: var(--text-secondary);
}
.footer-bottom-row {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  align-items: center;
  justify-content: space-between;
  border-top: 1px solid color-mix(in srgb, var(--border) 20%, transparent);
  padding-top: 2rem;
}
@media (min-width: 640px) {
  .footer-bottom-row { flex-direction: row; }
}
.footer-copyright-text {
  font-size: 0.75rem;
  font-weight: 500;
  color: var(--text-tertiary);
  margin: 0;
}
.footer-socials {
  display: flex;
  align-items: center;
  gap: 1.25rem;
}
.footer-social-link {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-secondary);
  text-decoration: none;
  transition: color var(--transition-fast);
}
.footer-social-link:hover { color: var(--color-primary); }

/* MODAL PORTAL (LOGIN FORM) */
.login-modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 9999;
  background-color: rgba(0, 0, 0, 0.45);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1.5rem;
}

/* Glassmorphism modal card */
.login-modal-card {
  position: relative;
  width: 100%;
  max-width: 27rem;
  border-radius: 24px;
  border: 1px solid color-mix(in srgb, var(--border) 45%, transparent);
  background-color: color-mix(in srgb, var(--bg-surface) 85%, transparent);
  box-shadow: 0 35px 60px -15px rgba(0, 0, 0, 0.35);
  padding: 2.5rem;
}

.login-modal-close-btn {
  position: absolute;
  top: 1.25rem;
  right: 1.25rem;
  background: color-mix(in srgb, var(--border) 35%, transparent);
  border: none;
  width: 2rem;
  height: 2rem;
  border-radius: 50%;
  display: grid;
  place-items: center;
  color: var(--text-secondary);
  cursor: pointer;
  transition: all var(--transition-fast);
}
.login-modal-close-btn:hover {
  background: var(--border-strong);
  color: var(--text-primary);
  transform: rotate(90deg);
}

.login-logo-row {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 2rem;
}

.login-logo-icon {
  display: grid;
  place-items: center;
  width: 3.25rem;
  height: 3.25rem;
  border-radius: 14px;
  background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-primary-hover) 100%);
  color: white;
  flex-shrink: 0;
  box-shadow: 0 8px 20px color-mix(in srgb, var(--color-primary) 35%, transparent);
}

.login-title {
  font-family: 'Plus Jakarta Sans', sans-serif;
  font-size: 1.25rem;
  font-weight: 800;
  letter-spacing: -0.025em;
  color: var(--text-primary);
  margin: 0;
}

.login-subtitle {
  font-size: 0.75rem;
  font-weight: 500;
  color: var(--text-secondary);
  margin: 0.25rem 0 0;
}

/* Error alert styling */
.login-error {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  border-radius: 12px;
  border: 1px solid color-mix(in srgb, var(--color-danger) 20%, transparent);
  background-color: color-mix(in srgb, var(--color-danger) 8%, transparent);
  color: var(--color-danger);
  padding: 0.75rem 1rem;
  font-size: 0.8125rem;
  font-weight: 500;
  margin-bottom: 1.5rem;
  line-height: 1.4;
}

/* Form inputs styling */
.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.login-field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.login-label {
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--text-secondary);
  letter-spacing: 0.025em;
}

.login-required {
  color: var(--color-danger);
  margin-left: 2px;
}

.login-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  width: 100%;
}

.login-input-icon {
  position: absolute;
  left: 1.125rem;
  color: var(--text-tertiary);
  pointer-events: none;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: color var(--transition-fast);
}

.login-input:focus + .login-input-icon,
.login-input-wrapper:focus-within .login-input-icon {
  color: var(--color-primary);
}

.login-input {
  height: 3.125rem;
  width: 100%;
  border-radius: 12px;
  border: 1px solid var(--border-strong);
  background-color: var(--bg-surface);
  color: var(--text-primary);
  padding-left: 2.875rem !important;
  padding-right: 1.25rem !important;
  font-size: 0.9375rem;
  font-weight: 500;
  outline: none;
  transition: all var(--transition-fast);
}
.login-input::placeholder { color: var(--text-tertiary); }
.login-input:focus {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 4px color-mix(in srgb, var(--color-primary) 12%, transparent);
  background-color: var(--bg-surface);
}
.login-input--error {
  border-color: var(--color-danger);
  background-color: color-mix(in srgb, var(--color-danger) 4%, transparent);
}
.login-input--error:focus {
  box-shadow: 0 0 0 4px color-mix(in srgb, var(--color-danger) 12%, transparent);
}

.login-field-error {
  font-size: 0.75rem;
  font-weight: 500;
  color: var(--color-danger);
  margin: 0.25rem 0 0;
}

/* Submit button */
.login-submit {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.625rem;
  height: 3.125rem;
  width: 100%;
  border-radius: 12px;
  background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-primary-hover) 100%);
  color: white;
  font-size: 0.9375rem;
  font-weight: 700;
  border: none;
  cursor: pointer;
  margin-top: 0.5rem;
  box-shadow: 0 4px 15px color-mix(in srgb, var(--color-primary) 30%, transparent);
  transition: all var(--transition-fast);
}
.login-submit:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 6px 20px color-mix(in srgb, var(--color-primary) 40%, transparent);
}
.login-submit:active:not(:disabled) {
  transform: translateY(1px);
}
.login-submit:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}
</style>
