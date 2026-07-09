<template>
  <div class="container-fluid min-vh-100 d-flex align-items-center justify-content-center p-3 p-md-4 auth-shell">
    <div class="card border-0 shadow-lg rounded-4 overflow-hidden w-100" style="max-width: 1000px;">
      <div class="row g-0">
        <!-- Hero Column -->
        <div class="col-lg-6 bg-primary text-white p-4 p-md-5 d-flex flex-column justify-content-center auth-hero">
          <div class="brand-badge bg-white bg-opacity-25 rounded-3 d-flex align-items-center justify-content-center fw-bold text-white mb-3" style="width: 48px; height: 48px; font-size: 1.25rem;">
            TF
          </div>
          <h1 class="fw-bold h2 mb-2 text-white">TaskFlow Pro</h1>
          <p class="text-white text-opacity-75 mb-4">
            Organize work with a calm, modern workspace that feels as polished as your tasks.
          </p>
          <ul class="list-unstyled mb-0 d-grid gap-2">
            <li class="d-flex align-items-center gap-2">
              <i class="bi bi-check-circle-fill text-white"></i> Track work in one place
            </li>
            <li class="d-flex align-items-center gap-2">
              <i class="bi bi-check-circle-fill text-white"></i> Assign and update tasks easily
            </li>
            <li class="d-flex align-items-center gap-2">
              <i class="bi bi-check-circle-fill text-white"></i> Keep deadlines visible and clear
            </li>
          </ul>
        </div>

        <!-- Panel Column -->
        <div class="col-lg-6 p-4 p-md-5 bg-white d-flex flex-column justify-content-center">
          <div class="mb-4">
            <span class="badge bg-primary bg-opacity-10 text-primary px-3 py-2 rounded-pill fw-bold mb-3" style="font-size: 0.8rem;">
              TaskFlow
            </span>
            <h2 class="fw-bold text-dark h4 mb-1">{{ title }}</h2>
            <p class="text-muted small mb-0">{{ subtitle }}</p>
          </div>

          <slot />

          <div class="mt-4 text-center text-muted small">
            <span>{{ switchLabel }} </span>
            <router-link :to="switchTo" class="text-primary fw-semibold text-decoration-none hover-underline">{{ switchText }}</router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: { type: String, default: 'Welcome back' },
  subtitle: { type: String, default: 'Sign in to continue' },
  switchLabel: { type: String, default: 'Need an account?' },
  switchText: { type: String, default: 'Create account' },
  switchTo: { type: String, default: '/register' }
})
</script>

<style scoped>
.auth-shell {
  background: radial-gradient(circle at top left, rgba(99,102,241,0.15), transparent 40%),
              linear-gradient(135deg, #f8faff 0%, #eef2ff 100%);
}
.auth-hero {
  background: linear-gradient(135deg, #4f46e5 0%, #6366f1 100%) !important;
}
.hover-underline:hover {
  text-decoration: underline !important;
}
</style>
