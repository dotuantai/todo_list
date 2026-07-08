<template>
  <div class="auth-shell">
    <div class="auth-card">
      <div class="auth-hero">
        <div class="brand-badge">TF</div>
        <h1>TaskFlow Pro</h1>
        <p>Organize work with a calm, modern workspace that feels as polished as your tasks.</p>

        <ul class="feature-list">
          <li><i class="bi bi-check-circle-fill"></i> Track work in one place</li>
          <li><i class="bi bi-check-circle-fill"></i> Assign and update tasks easily</li>
          <li><i class="bi bi-check-circle-fill"></i> Keep deadlines visible and clear</li>
        </ul>
      </div>

      <div class="auth-panel">
        <div class="panel-top">
          <div class="brand-pill">TaskFlow</div>
          <div class="panel-copy">
            <h2>{{ title }}</h2>
            <p>{{ subtitle }}</p>
          </div>
        </div>

        <slot />

        <div class="switch-link">
          <span>{{ switchLabel }}</span>
          <router-link :to="switchTo">{{ switchText }}</router-link>
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
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
  background:
    radial-gradient(circle at top left, rgba(99,102,241,0.22), transparent 34%),
    linear-gradient(135deg, #f8faff 0%, #eef2ff 55%, #f8fafc 100%);
}

.auth-card {
  width: min(1100px, 100%);
  display: grid;
  grid-template-columns: 1.05fr 0.95fr;
  background: rgba(255,255,255,0.95);
  border: 1px solid rgba(226, 232, 240, 0.9);
  border-radius: 28px;
  overflow: hidden;
  box-shadow: 0 24px 80px rgba(15, 23, 42, 0.12);
}

.auth-hero {
  padding: 40px;
  background: linear-gradient(135deg, #4f46e5 0%, #6366f1 50%, #818cf8 100%);
  color: white;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.brand-badge {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  background: rgba(255,255,255,0.2);
  backdrop-filter: blur(10px);
  margin-bottom: 18px;
}

.auth-hero h1 {
  font-size: 1.9rem;
  font-weight: 700;
  margin-bottom: 10px;
}

.auth-hero p {
  margin-bottom: 20px;
  color: rgba(255,255,255,0.9);
  line-height: 1.65;
}

.feature-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: grid;
  gap: 10px;
}

.feature-list li {
  display: flex;
  align-items: center;
  gap: 10px;
  color: rgba(255,255,255,0.95);
}

.auth-panel {
  padding: 38px;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.panel-top {
  margin-bottom: 24px;
}

.brand-pill {
  display: inline-flex;
  align-items: center;
  padding: 6px 12px;
  border-radius: 999px;
  background: #eef2ff;
  color: #4f46e5;
  font-size: 0.82rem;
  font-weight: 700;
  margin-bottom: 12px;
}

.panel-copy h2 {
  font-size: 1.55rem;
  font-weight: 700;
  color: #111827;
  margin-bottom: 6px;
}

.panel-copy p {
  color: #64748b;
  margin-bottom: 0;
  line-height: 1.6;
}

.switch-link {
  margin-top: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  color: #64748b;
  font-size: 0.95rem;
}

.switch-link a {
  color: #4f46e5;
  font-weight: 600;
  text-decoration: none;
}

.switch-link a:hover {
  text-decoration: underline;
}

@media (max-width: 900px) {
  .auth-card {
    grid-template-columns: 1fr;
  }

  .auth-hero {
    padding: 32px 24px;
  }

  .auth-panel {
    padding: 28px 24px 32px;
  }
}
</style>
