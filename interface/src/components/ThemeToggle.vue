<template>
  <button 
    class="btn btn-light p-0 border rounded-3 d-flex align-items-center justify-content-center theme-toggle-btn" 
    style="width: 36px; height: 36px; outline: none; box-shadow: none;" 
    @click="toggleTheme" 
    :title="$t('common.SCR0021')"
    type="button"
  >
    <i class="bi fs-6" :class="isDarkMode ? 'bi-sun-fill text-warning' : 'bi-moon-stars-fill text-secondary'"></i>
  </button>
</template>

<script setup>
import { useTheme } from '../composables/useTheme.js'

const { isDarkMode, toggleTheme } = useTheme()
</script>

<style scoped>
.theme-toggle-btn {
  transition: transform 0.2s cubic-bezier(0.16, 1, 0.3, 1), background-color 0.2s ease, border-color 0.2s ease;
}
.theme-toggle-btn:hover {
  transform: scale(1.06);
}
.theme-toggle-btn:active {
  transform: scale(0.94);
}
</style>
