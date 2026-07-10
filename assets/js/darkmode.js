// Dark Mode for MR Shop
let darkMode = localStorage.getItem('mrshop_darkmode') === 'true';

function initDarkMode() {
  if (darkMode) {
    document.documentElement.setAttribute('data-theme', 'dark');
  }
  createToggle();
}

function createToggle() {
  if (document.querySelector('.dark-mode-toggle')) return;
  const btn = document.createElement('button');
  btn.className = 'dark-mode-toggle';
  btn.innerHTML = darkMode ? '<i class="fas fa-sun"></i>' : '<i class="fas fa-moon"></i>';
  btn.title = darkMode ? 'Switch to Light Mode' : 'Switch to Dark Mode';
  btn.onclick = toggleDarkMode;
  document.body.appendChild(btn);
}

function toggleDarkMode() {
  darkMode = !darkMode;
  localStorage.setItem('mrshop_darkmode', darkMode);
  document.documentElement.setAttribute('data-theme', darkMode ? 'dark' : 'light');
  const btn = document.querySelector('.dark-mode-toggle');
  if (btn) {
    btn.innerHTML = darkMode ? '<i class="fas fa-sun"></i>' : '<i class="fas fa-moon"></i>';
    btn.title = darkMode ? 'Switch to Light Mode' : 'Switch to Dark Mode';
  }
}

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', initDarkMode);
} else {
  initDarkMode();
}
