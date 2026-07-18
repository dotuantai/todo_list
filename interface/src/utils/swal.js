import Swal from 'sweetalert2'

// ── Inject CSS into <head> once when the module is loaded ─────────────────
;(function injectSwalStyles() {
  const id = '__swal-custom-styles__'
  if (document.getElementById(id)) return
  const style = document.createElement('style')
  style.id = id
  style.textContent = `
    .swal-popup {
      background-color: var(--bs-card-bg, #fff) !important;
      color: var(--bs-body-color, #1e293b) !important;
      border-radius: 18px !important;
      padding: 28px 32px 24px !important;
      font-family: 'Inter', 'Segoe UI', system-ui, sans-serif !important;
      box-shadow: 0 25px 70px rgba(0,0,0,.18) !important;
    }
    .swal-title {
      font-size: 1.15rem !important;
      font-weight: 700 !important;
      color: var(--bs-heading-color, #0f172a) !important;
      margin-bottom: 4px !important;
    }
    .swal-html {
      font-size: 0.95rem !important;
      color: var(--bs-secondary-color, #475569) !important;
    }
    .swal-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 6px;
      padding: 9px 22px;
      border-radius: 9px;
      font-size: 0.9rem;
      font-weight: 600;
      cursor: pointer;
      border: none;
      transition: background 0.15s, transform 0.1s, box-shadow 0.15s;
    }
    .swal-btn:active { transform: scale(0.97); }
    .swal-btn--confirm {
      background: #6366f1;
      color: #fff;
      box-shadow: 0 2px 8px rgba(99,102,241,.35);
    }
    .swal-btn--confirm:hover { background: #4f46e5; }
    .swal-btn--cancel {
      background: var(--bs-secondary-bg, #f1f5f9);
      color: var(--bs-secondary-color, #475569);
      margin-right: 8px;
    }
    .swal-btn--cancel:hover { background: var(--bs-tertiary-bg, #e2e8f0); }
    .swal2-toast.swal-popup {
      padding: 14px 20px !important;
      border-radius: 12px !important;
      box-shadow: 0 8px 28px rgba(0,0,0,.13) !important;
    }
    @keyframes swal-in  { from { opacity:0; transform:scale(.92) translateY(12px); } to { opacity:1; transform:scale(1) translateY(0); } }
    @keyframes swal-out { from { opacity:1; transform:scale(1); } to { opacity:0; transform:scale(.94); } }
    .swal-anim-in  { animation: swal-in  .22s cubic-bezier(.25,.8,.25,1) both; }
    .swal-anim-out { animation: swal-out .15s ease both; }
  `
  document.head.appendChild(style)
})()

// ── Base instance with shared styles ──────────────────────────────────────────
const Base = Swal.mixin({
  customClass: {
    popup:         'swal-popup',
    confirmButton: 'swal-btn swal-btn--confirm',
    cancelButton:  'swal-btn swal-btn--cancel',
    title:         'swal-title',
    htmlContainer: 'swal-html',
  },
  buttonsStyling: false,
  showClass: { popup: 'swal-anim-in'  },
  hideClass: { popup: 'swal-anim-out' },
})

// ── Toast (top-right, auto-dismiss) ────────────────────────────────────────
const Toast = Base.mixin({
  toast:             true,
  position:          'top-end',
  showConfirmButton: false,
  timer:             3500,
  timerProgressBar:  true,
})

// ── Helpers ────────────────────────────────────────────────────────────────

/** Show success toast */
export const toastSuccess = (message) =>
  Toast.fire({ icon: 'success', title: message })

/** Show error toast */
export const toastError = (message) =>
  Toast.fire({ icon: 'error', title: message })

/** Show warning toast */
export const toastWarning = (message) =>
  Toast.fire({ icon: 'warning', title: message })

/** Show success dialog (with OK button) */
export const alertSuccess = (title, message = '') =>
  Base.fire({ icon: 'success', title, html: message || undefined })

/** Show error dialog (with OK button) */
export const alertError = (title, message = '') =>
  Base.fire({ icon: 'error', title, html: message || undefined })

/**
 * Confirmation dialog (Confirm / Cancel)
 * @returns {Promise<boolean>} true if user clicks Confirm
 */
export const confirm = async (title, message = '', confirmText = 'Confirm') => {
  const result = await Base.fire({
    icon:               'warning',
    title,
    html:               message || undefined,
    showCancelButton:   true,
    confirmButtonText:  confirmText,
    cancelButtonText:   'Cancel',
    reverseButtons:     true,
  })
  return result.isConfirmed
}

/**
 * Extract message from axios/API error
 * Backend always returns { Success, Message, Data }
 */
export const extractMessage = (error, fallback = 'An error occurred.') =>
  error?.response?.data?.Message || error?.message || fallback
