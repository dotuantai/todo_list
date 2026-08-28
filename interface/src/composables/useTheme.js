import { ref } from 'vue'

const getInitialTheme = () => {
  const saved = localStorage.getItem('theme')
  if (saved === 'dark' || saved === 'light') return saved
  const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches
  return prefersDark ? 'dark' : 'light'
}

const isDarkMode = ref(getInitialTheme() === 'dark')

export function useTheme() {
  const applyTheme = (theme) => {
    const validTheme = theme === 'dark' ? 'dark' : 'light'
    isDarkMode.value = (validTheme === 'dark')
    document.documentElement.setAttribute('data-bs-theme', validTheme)
    localStorage.setItem('theme', validTheme)
    window.dispatchEvent(new CustomEvent('theme-changed', { detail: validTheme }))
  }

  const toggleTheme = () => {
    const nextTheme = isDarkMode.value ? 'light' : 'dark'
    applyTheme(nextTheme)
  }

  const initTheme = () => {
    const theme = getInitialTheme()
    applyTheme(theme)
  }

  return {
    isDarkMode,
    toggleTheme,
    applyTheme,
    initTheme
  }
}
