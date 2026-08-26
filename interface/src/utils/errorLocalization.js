import vi from '../locales/vi.json'
import en from '../locales/en.json'

const locales = { vi, en }

export const localizeErrorCode = (errorCode, fallback = '') => {
  if (!errorCode) return fallback
  const locale = localStorage.getItem('locale') || 'vi'
  return locales[locale]?.errorCodes?.[errorCode] || fallback || errorCode
}
