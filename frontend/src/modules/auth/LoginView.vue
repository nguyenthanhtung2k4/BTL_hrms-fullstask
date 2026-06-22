<script setup lang="ts">
import { ref, onMounted, watch, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '../../stores/auth'
import { useToastStore } from '../../stores/toast'
import { Mail, Lock, LogIn, AlertCircle, X, ArrowRight } from '@lucide/vue'
import gsap from 'gsap'

const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()
const { t } = useI18n()

const email = ref('')
const password = ref('')
const errors = ref<{ email?: string; password?: string; general?: string }>({})
const showLoginModal = ref(false)

async function handleLogin() {
  errors.value = {}
  if (!email.value)    { errors.value.email    = t('validation.required'); return }
  if (!password.value) { errors.value.password = t('validation.required'); return }

  try {
    await auth.login(email.value, password.value)
    toast.success(`${t('dashboard.greeting_morning')}, ${auth.displayName}!`)
    if (auth.isHR || auth.isAdmin) router.push('/')
    else if (auth.isPayrollStaff)  router.push('/payroll/periods')
    else                           router.push('/attendance/checkin')
  } catch (err: any) {
    errors.value.general = err?.response?.data?.message ?? t('auth.loginError')
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
      { scale: 1, opacity: 1, y: 0, duration: 0.5, ease: 'back.out(1.5)' }
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
  toast.info('Hỗ trợ kỹ thuật nội bộ: support@company.com | Hotline: 1900-8888')
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

  // 2. Slow hover drift for interactive mockup dashboard
  gsap.fromTo('.landing-mockup-card',
    { y: 0 },
    { y: -12, duration: 5, repeat: -1, yoyo: true, ease: 'sine.inOut' }
  )

  // 3. Floating animation for stats/mockup sub-elements
  gsap.fromTo('.landing-float-badge-1',
    { y: 0 }, { y: -8, duration: 3.5, repeat: -1, yoyo: true, ease: 'sine.inOut', delay: 0.5 }
  )
  gsap.fromTo('.landing-float-badge-2',
    { y: 0 }, { y: 8, duration: 4, repeat: -1, yoyo: true, ease: 'sine.inOut', delay: 1 }
  )

  // 4. Ambient floating animation for background blobs
  gsap.to('.login-blob-1', { x: 'random(-50, 50)', y: 'random(-50, 50)', duration: 15, repeat: -1, yoyo: true, ease: 'sine.inOut' })
  gsap.to('.login-blob-2', { x: 'random(-70, 70)', y: 'random(-70, 70)', duration: 18, repeat: -1, yoyo: true, ease: 'sine.inOut' })
  gsap.to('.login-blob-3', { x: 'random(-45, 45)', y: 'random(-45, 45)', duration: 13, repeat: -1, yoyo: true, ease: 'sine.inOut' })
})
</script>

<template>
  <main class="login-page">
    <!-- Ambient background blobs -->
    <div class="login-blob login-blob-1"></div>
    <div class="login-blob login-blob-2"></div>
    <div class="login-blob login-blob-3"></div>

    <!-- Frosted glass background blur filter layer -->
    <div class="login-bg-blur-overlay"></div>

    <!-- Premium tech grid background pattern -->
    <div class="login-bg-grid"></div>

    <!-- STICKY HEADER BAR -->
    <header class="landing-header">
      <div class="landing-header-left" @click="scrollToSection('.login-page')" style="cursor: pointer;">
        <div class="landing-logo">
          <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round"
              d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
        </div>
        <div class="landing-logo-text">
          <span class="landing-brand-name">HRMS Workspace</span>
          <span class="landing-brand-subname">Hệ Thống Nội Bộ</span>
        </div>
      </div>
      

      <div class="landing-header-right">
        <button class="landing-header-btn btn-login" @click="openModal">Đăng nhập</button>
      </div>
    </header>

    <!-- MAIN PORTAL SCROLL CONTENT -->
    <div class="landing-layout-wrapper">
      
      <!-- 2. HERO SECTION -->
      <section class="landing-hero-container">
        <div class="landing-hero-left">
          <div class="landing-badge landing-fade-in">HỆ THỐNG QUẢN TRỊ NHÂN SỰ NỘI BỘ</div>
          <h1 class="landing-main-title landing-fade-in">
            Nền tảng quản lý nhân sự &amp; tính lương nội bộ
          </h1>
          <p class="landing-description landing-fade-in">
            Hợp nhất hồ sơ nhân sự, tự động hóa dữ liệu chấm công và quản lý chu kỳ bảng lương của công ty trên một cổng thông tin duy nhất.
          </p>

          <div class="landing-cta-row landing-fade-in">
            <button class="landing-cta-btn landing-cta-btn--primary" @click="openModal">
              Đăng nhập hệ thống
              <ArrowRight class="h-4 w-4" />
            </button>
            <button class="landing-cta-btn landing-cta-btn--secondary" @click="handleContact">
              Liên hệ hỗ trợ
            </button>
          </div>
        </div>

        <div class="landing-hero-right landing-fade-in">
          <div class="landing-mockup-card">
            <!-- Mockup Header -->
            <div class="mockup-header">
              <div class="mockup-header-left">
                <span class="mockup-dot mockup-dot-red"></span>
                <span class="mockup-dot mockup-dot-yellow"></span>
                <span class="mockup-dot mockup-dot-green"></span>
                <span class="mockup-header-title">HRMS Control Panel</span>
              </div>
              <div class="mockup-header-right">
                <span class="mockup-status-pulse"></span>
                <span class="mockup-status-text">Bảng điều hành live</span>
              </div>
            </div>

            <!-- Mockup Content -->
            <div class="mockup-content">
              <div class="mockup-sidebar">
                <div class="mockup-sidebar-item active"></div>
                <div class="mockup-sidebar-item"></div>
                <div class="mockup-sidebar-item"></div>
              </div>
              <div class="mockup-main">
                <div class="mockup-stats-row">
                  <div class="mockup-mini-card">
                    <span class="mockup-mini-label">Nhân sự</span>
                    <span class="mockup-mini-val">48 NV</span>
                  </div>
                  <div class="mockup-mini-card">
                    <span class="mockup-mini-label">Chờ duyệt</span>
                    <span class="mockup-mini-val" style="color: var(--color-warning);">3 đơn</span>
                  </div>
                </div>
                <div class="mockup-dashboard-grid">
                  <div class="mockup-chart-card">
                    <div class="mockup-chart-bars">
                      <div class="mockup-chart-bar-col"><div class="mockup-chart-bar" style="height: 70%;"></div></div>
                      <div class="mockup-chart-bar-col"><div class="mockup-chart-bar" style="height: 90%;"></div></div>
                      <div class="mockup-chart-bar-col"><div class="mockup-chart-bar" style="height: 80%;"></div></div>
                    </div>
                  </div>
                  <div class="mockup-members-card">
                    <div class="mockup-member-avatar" style="background-color: var(--color-primary-light); color: var(--color-primary-text); font-size: 8px; font-weight: 700; width: 1.5rem; height: 1.5rem; border-radius: 50%; display: grid; place-items: center;">LA</div>
                    <span style="font-size: 8px; font-weight: 700;">Lan Anh</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Floating Badges -->
          <div class="landing-float-badge landing-float-badge-1">+20% hiệu suất</div>
          <div class="landing-float-badge landing-float-badge-2">120 nhân viên online</div>
          <div class="landing-float-badge landing-float-badge-3">98% chấm công đúng giờ</div>
        </div>
      </section>

      <!-- 14. FOOTER -->
      <footer class="landing-footer-detailed">
        <div class="footer-columns">
          <div class="footer-col">
            <h5 class="footer-col-title">HRMS Workspace</h5>
            <p class="footer-company-desc">Giải pháp vận hành, quản trị nguồn nhân lực và tự động hóa tính lương tối ưu cho công ty.</p>
          </div>
          <div class="footer-col">
            <h5 class="footer-col-title">Liên kết chính</h5>
            <a href="#" @click.prevent="scrollToSection('.login-page')" class="footer-link">Trang chủ</a>
            <a href="#" @click.prevent="scrollToSection('#solutions')" class="footer-link">Giới thiệu giải pháp</a>
            <a href="#" @click.prevent="scrollToSection('#why-choose-us')" class="footer-link">Hạ tầng kỹ thuật</a>
          </div>
          <div class="footer-col">
            <h5 class="footer-col-title">Không gian làm việc</h5>
            <a href="#" @click.prevent="openModal" class="footer-link">Cổng nhân viên</a>
            <a href="#" @click.prevent="openModal" class="footer-link">Trang quản trị viên</a>
            <a href="#" @click.prevent="openModal" class="footer-link">Đăng nhập chấm công</a>
          </div>
          <div class="footer-col">
            <h5 class="footer-col-title">Bộ phận hỗ trợ</h5>
            <span class="footer-contact-item">📧 support@company.com</span>
            <span class="footer-contact-item">📞 Phòng nhân sự: ext 102</span>
            <span class="footer-contact-item">📍 Trụ sở chính công ty</span>
          </div>
        </div>
        <div class="footer-bottom-row">
          <p class="footer-copyright-text">&copy; 2026 HRMS Workspace. Thiết kế nội bộ công ty.</p>
          <div class="footer-socials">
            <a href="#" @click.prevent class="footer-social-link">Intranet</a>
            <a href="#" @click.prevent class="footer-social-link">Hỗ trợ CNTT</a>
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
            <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round"
                d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
          </div>
          <div>
            <h1 class="login-title">Đăng nhập HRMS</h1>
            <p class="login-subtitle">Vui lòng nhập tài khoản được cấp</p>
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
.login-page {
  position: relative;
  min-height: 100vh;
  width: 100%;
  display: flex;
  flex-direction: column;
  background-color: var(--bg-page);
  overflow-x: hidden;
  transition: background-color var(--transition-base);
}

/* Ambient gradient background blobs underneath the blur layer */
.login-blob {
  position: absolute;
  border-radius: 50%;
  filter: blur(140px);
  opacity: 0.22;
  z-index: 1;
  pointer-events: none;
}
.login-blob-1 {
  width: 600px;
  height: 600px;
  background: radial-gradient(circle, var(--color-primary) 0%, rgba(59, 130, 246, 0) 70%);
  top: -10%;
  left: -5%;
}
.login-blob-2 {
  width: 650px;
  height: 650px;
  background: radial-gradient(circle, #06b6d4 0%, rgba(6, 182, 212, 0) 70%);
  bottom: 0%;
  right: -5%;
}
.login-blob-3 {
  width: 450px;
  height: 450px;
  background: radial-gradient(circle, #8b5cf6 0%, rgba(139, 92, 246, 0) 70%);
  top: 50%;
  left: 10%;
}

/* Frosted glass background overlay */
.login-bg-blur-overlay {
  position: absolute;
  inset: 0;
  backdrop-filter: blur(100px);
  -webkit-backdrop-filter: blur(100px);
  background-color: color-mix(in srgb, var(--bg-page) 40%, transparent);
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
  z-index: 1000;
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  background-color: color-mix(in srgb, var(--bg-page) 65%, transparent);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1.25rem 2.5rem;
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
  border-radius: 8px;
  background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-primary-hover) 100%);
  color: white;
  box-shadow: 0 4px 10px color-mix(in srgb, var(--color-primary) 30%, transparent);
}

.landing-logo-text {
  display: flex;
  flex-direction: column;
}

.landing-brand-name {
  font-size: 1rem;
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
@media (min-width: 768px) {
  .landing-nav-menu {
    display: flex;
  }
}

.landing-nav-link {
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

/* SECTION GLOBAL STYLES */
.landing-section {
  padding: 5rem 0;
  border-bottom: 1px solid color-mix(in srgb, var(--border) 25%, transparent);
}

.section-header {
  text-align: center;
  margin-bottom: 3.5rem;
}

.section-subtitle {
  display: inline-block;
  font-size: 0.6875rem;
  font-weight: 800;
  letter-spacing: 0.1em;
  color: var(--color-primary-text);
  background-color: var(--color-primary-light);
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  margin-bottom: 0.875rem;
}

.section-title {
  font-size: 1.875rem;
  font-weight: 800;
  color: var(--text-primary);
  letter-spacing: -0.03em;
  margin: 0;
}
@media (min-width: 640px) {
  .section-title {
    font-size: 2.25rem;
  }
}

.landing-grid-4 {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1.5rem;
}
@media (min-width: 640px) {
  .landing-grid-4 { grid-template-columns: repeat(2, 1fr); }
}
@media (min-width: 1024px) {
  .landing-grid-4 { grid-template-columns: repeat(4, 1fr); }
}

/* 2. HERO SECTION */
.landing-hero-container {
  display: grid;
  grid-template-columns: 1fr;
  gap: 4rem;
  align-items: center;
  padding: 6rem 0;
  border-bottom: 1px solid color-mix(in srgb, var(--border) 25%, transparent);
}
@media (min-width: 1024px) {
  .landing-hero-container {
    grid-template-columns: 1.15fr 0.85fr;
  }
}

.landing-badge {
  display: inline-flex;
  padding: 0.375rem 0.875rem;
  font-size: 0.6875rem;
  font-weight: 700;
  letter-spacing: 0.05em;
  border-radius: 9999px;
  color: var(--color-primary-text);
  background-color: var(--color-primary-light);
  border: 1px solid color-mix(in srgb, var(--color-primary) 15%, transparent);
  margin-bottom: 1.5rem;
}

.landing-main-title {
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
  transform: translateY(-1px);
  box-shadow: 0 6px 20px color-mix(in srgb, var(--color-primary) 35%, transparent);
}

.landing-cta-btn--secondary {
  border: 1px solid var(--border-strong);
  background-color: var(--bg-surface);
  color: var(--text-primary);
}
.landing-cta-btn--secondary:hover {
  background-color: var(--bg-subtle);
  transform: translateY(-1px);
}

.landing-hero-right {
  display: flex;
  justify-content: center;
  align-items: center;
  position: relative;
}

/* Floating badges inside Hero */
.landing-float-badge {
  position: absolute;
  padding: 0.625rem 1rem;
  font-size: 0.6875rem;
  font-weight: 700;
  border-radius: 12px;
  border: 1px solid color-mix(in srgb, var(--border) 40%, transparent);
  background: color-mix(in srgb, var(--bg-surface) 80%, transparent);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  box-shadow: var(--shadow-md);
  z-index: 15;
}
.landing-float-badge-1 { top: 10%; left: -10%; color: var(--color-success-text); }
.landing-float-badge-2 { bottom: 15%; right: -8%; color: var(--color-primary-text); }
.landing-float-badge-3 { top: 55%; left: -12%; color: #0891b2; }

/* Dashboard Mockup styling */
.landing-mockup-card {
  width: 100%;
  max-width: 30rem;
  border-radius: 20px;
  border: 1px solid color-mix(in srgb, var(--border) 40%, transparent);
  background: color-mix(in srgb, var(--bg-surface) 65%, transparent);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  box-shadow: 0 30px 60px rgba(0,0,0,0.12);
  overflow: hidden;
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
.mockup-header-title { font-size: 0.6875rem; font-weight: 700; color: var(--text-secondary); margin-left: 0.5rem; font-family: monospace; }
.mockup-header-right { display: flex; align-items: center; gap: 0.5rem; }
.mockup-status-pulse { width: 0.375rem; height: 0.375rem; border-radius: 50%; background-color: var(--color-success); position: relative; }
.mockup-status-pulse::after { content: ''; position: absolute; inset: -2px; border-radius: 50%; border: 1px solid var(--color-success); animation: ping 1.5s cubic-bezier(0, 0, 0.2, 1) infinite; }
.mockup-status-text { font-size: 0.625rem; font-weight: 600; color: var(--text-tertiary); }

.mockup-content { display: flex; height: 12rem; }
.mockup-sidebar { width: 2.75rem; border-right: 1px solid color-mix(in srgb, var(--border) 35%, transparent); display: flex; flex-direction: column; align-items: center; gap: 0.875rem; padding-top: 1rem; }
.mockup-sidebar-item { width: 1.25rem; height: 1.25rem; border-radius: 4px; background-color: color-mix(in srgb, var(--border) 50%, transparent); }
.mockup-sidebar-item.active { background-color: var(--color-primary); }
.mockup-main { flex: 1; padding: 1rem; display: flex; flex-direction: column; gap: 0.875rem; }
.mockup-stats-row { display: grid; grid-template-columns: repeat(2, 1fr); gap: 0.75rem; }
.mockup-mini-card { display: flex; flex-direction: column; background-color: var(--bg-surface); border: 1px solid var(--border); border-radius: 8px; padding: 0.375rem 0.625rem; }
.mockup-mini-label { font-size: 0.5625rem; font-weight: 600; color: var(--text-tertiary); }
.mockup-mini-val { font-size: 0.75rem; font-weight: 700; color: var(--text-primary); }
.mockup-dashboard-grid { display: grid; grid-template-columns: 1.3fr 0.7fr; gap: 0.75rem; flex: 1; }
.mockup-chart-card, .mockup-members-card { background-color: var(--bg-surface); border: 1px solid var(--border); border-radius: 10px; padding: 0.5rem; display: flex; flex-direction: column; justify-content: center; align-items: center; }
.mockup-chart-bars { display: flex; align-items: flex-end; justify-content: space-around; width: 100%; height: 2.5rem; }
.mockup-chart-bar-col { flex: 1; display: flex; justify-content: center; }
.mockup-chart-bar { width: 0.5rem; border-radius: 2px; background: var(--color-primary); }

/* 3. BUSINESS PAIN POINTS */
.pain-card {
  background-color: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 16px;
  padding: 2rem;
  transition: all var(--transition-base);
}
.pain-card:hover {
  transform: translateY(-4px);
  border-color: var(--color-danger);
  box-shadow: 0 10px 20px color-mix(in srgb, var(--color-danger) 8%, transparent);
}
.pain-icon { font-size: 2.25rem; display: inline-block; margin-bottom: 1rem; }
.pain-card h3 { font-size: 1.125rem; font-weight: 800; color: var(--text-primary); margin: 0 0 0.5rem; }
.pain-card p { font-size: 0.875rem; color: var(--text-secondary); line-height: 1.5; margin: 0; }

/* 4. SOLUTIONS */
.solution-card {
  background-color: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 16px;
  padding: 2rem;
  transition: all var(--transition-base);
}
.solution-card:hover {
  transform: translateY(-4px);
  border-color: var(--color-success);
  box-shadow: 0 10px 20px color-mix(in srgb, var(--color-success) 8%, transparent);
}
.solution-icon { font-size: 2.25rem; display: inline-block; margin-bottom: 1rem; }
.solution-card h3 { font-size: 1.125rem; font-weight: 800; color: var(--text-primary); margin: 0 0 1rem; }
.solution-card ul { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 0.5rem; }
.solution-card li { font-size: 0.875rem; color: var(--text-secondary); display: flex; align-items: center; gap: 0.5rem; }
.solution-card li::before { content: '✓'; color: var(--color-success-text); font-weight: 700; }

/* 5. SYSTEM MODULES */
.modules-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1.25rem;
}
@media (min-width: 640px) {
  .modules-grid { grid-template-columns: repeat(3, 1fr); }
}

.module-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  background-color: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 1.25rem;
  transition: all var(--transition-fast);
}
.module-item:hover {
  border-color: var(--color-primary);
  background-color: var(--bg-subtle);
  transform: translateY(-2px);
}
.module-icon { font-size: 1.75rem; }
.module-name { font-size: 0.875rem; font-weight: 700; color: var(--text-primary); }

/* 6. TIMELINE PROCESS */
.timeline-container {
  display: grid;
  grid-template-columns: 1fr;
  gap: 2rem;
}
@media (min-width: 768px) {
  .timeline-container { grid-template-columns: repeat(5, 1fr); }
}

.timeline-step {
  position: relative;
  background-color: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 16px;
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}
.step-num {
  font-size: 1.5rem;
  font-weight: 800;
  color: var(--color-primary-text);
  opacity: 0.8;
}
.timeline-step h4 { font-size: 1rem; font-weight: 800; color: var(--text-primary); margin: 0; }
.timeline-step p { font-size: 0.8125rem; color: var(--text-secondary); line-height: 1.5; margin: 0; }

/* 7. STATISTICS BANNER */
.stats-banner {
  background: linear-gradient(135deg, color-mix(in srgb, var(--color-primary) 12%, transparent) 0%, color-mix(in srgb, #06b6d4 12%, transparent) 100%);
  border-radius: 24px;
  border: 1px solid color-mix(in srgb, var(--border) 45%, transparent);
}

.stats-container {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 2.5rem;
}
@media (min-width: 768px) {
  .stats-container { grid-template-columns: repeat(4, 1fr); }
}

.stat-counter {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 0.5rem;
}

.stat-number {
  font-size: 2.25rem;
  font-weight: 800;
  color: var(--text-primary);
  letter-spacing: -0.03em;
}

.stat-desc {
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--text-secondary);
}

/* 8. WHY CHOOSE US */
.why-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 2rem;
}
@media (min-width: 768px) {
  .why-grid { grid-template-columns: repeat(3, 1fr); }
}

.why-card {
  background-color: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: 16px;
  padding: 2rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.why-icon { font-size: 2.25rem; }
.why-card h3 { font-size: 1.125rem; font-weight: 800; color: var(--text-primary); margin: 0; }
.why-card p { font-size: 0.875rem; color: var(--text-secondary); line-height: 1.5; margin: 0; }

/* 13. BANNER CUỐI TRANG */
.final-cta-section {
  padding: 5rem 0;
  text-align: center;
}
.cta-banner-content {
  max-width: 44rem;
  margin: 0 auto;
  background: linear-gradient(135deg, color-mix(in srgb, var(--color-primary) 12%, transparent) 0%, color-mix(in srgb, #06b6d4 12%, transparent) 100%);
  border-radius: 24px;
  border: 1px solid color-mix(in srgb, var(--border) 45%, transparent);
  padding: 3.5rem 2rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1.25rem;
}
.cta-banner-content h2 { font-size: 1.75rem; font-weight: 800; color: var(--text-primary); letter-spacing: -0.03em; margin: 0; }
.cta-banner-content p { font-size: 1rem; color: var(--text-secondary); line-height: 1.5; margin: 0 0 0.5rem; }
.cta-banner-btns { display: flex; flex-wrap: wrap; justify-content: center; gap: 1rem; }
.final-cta-btn { padding: 0.75rem 1.5rem; font-size: 0.875rem; font-weight: 700; border-radius: 10px; cursor: pointer; transition: all var(--transition-fast); }
.final-cta-btn--primary { border: none; background-color: var(--color-primary); color: white; }
.final-cta-btn--primary:hover { background-color: var(--color-primary-hover); }
.final-cta-btn--secondary { border: 1px solid var(--border-strong); background-color: var(--bg-surface); color: var(--text-primary); }
.final-cta-btn--secondary:hover { background-color: var(--bg-subtle); }

/* 14. FOOTER DETAILED */
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
  padding: 0 1rem 0 2.875rem;
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
