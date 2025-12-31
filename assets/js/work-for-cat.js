// === Support Cats — Interactive Behavior ===

// === Animated Counters ===
function animateCounter(element, target, duration = 2000) {
  const start = 0;
  const fpsInterval = 16;
  const increment = target / (duration / fpsInterval);
  let current = start;

  const timer = setInterval(() => {
    current += increment;
    if (current >= target) {
      element.textContent = Number(target).toLocaleString('en-BD');
      clearInterval(timer);
    } else {
      element.textContent = Math.floor(current).toLocaleString('en-BD');
    }
  }, fpsInterval);
}

const counterObserver = new IntersectionObserver(
  (entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        const target = Number(entry.target.dataset.target || 0);
        animateCounter(entry.target, target);
        counterObserver.unobserve(entry.target);
      }
    });
  },
  { threshold: 0.5 }
);

document.querySelectorAll('.wfc-stat-number').forEach((el) => counterObserver.observe(el));

// === Feature card reveal ===
const featureObserver = new IntersectionObserver(
  (entries) => {
    entries.forEach((entry, index) => {
      if (entry.isIntersecting) {
        setTimeout(() => entry.target.classList.add('visible'), index * 80);
        featureObserver.unobserve(entry.target);
      }
    });
  },
  { threshold: 0.2 }
);

document.querySelectorAll('.wfc-feature-card').forEach((card) => featureObserver.observe(card));

// === Progress bars ===
const impactBars = document.querySelector('.wfc-impact-bars');
if (impactBars) {
  const barObserver = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.querySelectorAll('.wfc-progress-fill').forEach((fill) => {
            const progress = fill.dataset.progress || 0;
            const color = fill.dataset.color;
            if (color) fill.style.background = color;
            requestAnimationFrame(() => {
              fill.style.width = `${progress}%`;
            });
          });
          barObserver.unobserve(entry.target);
        }
      });
    },
    { threshold: 0.4 }
  );

  barObserver.observe(impactBars);
}

// === Story Slider ===
const storyTrack = document.getElementById('storyTrack');
const storyPrev = document.getElementById('storyPrev');
const storyNext = document.getElementById('storyNext');
const storyCards = document.querySelectorAll('.wfc-story-card');
let storyIndex = 0;

function updateStorySlider() {
  const offset = -(storyIndex * 100);
  if (storyTrack) storyTrack.style.transform = `translateX(${offset}%)`;
}

if (storyPrev && storyNext && storyCards.length) {
  storyPrev.addEventListener('click', () => {
    storyIndex = (storyIndex - 1 + storyCards.length) % storyCards.length;
    updateStorySlider();
  });

  storyNext.addEventListener('click', () => {
    storyIndex = (storyIndex + 1) % storyCards.length;
    updateStorySlider();
  });

  setInterval(() => {
    storyIndex = (storyIndex + 1) % storyCards.length;
    updateStorySlider();
  }, 7000);
}

// === Donation Modal ===
const donateModal = document.getElementById('donateModal');
const modalOverlay = document.getElementById('modalOverlay');
const modalClose = document.getElementById('modalClose');
const submitDonation = document.getElementById('submitDonation');
const customAmount = document.getElementById('customAmount');
const amountButtons = document.querySelectorAll('.wfc-amount-btn');
const paymentButtons = document.querySelectorAll('.wfc-payment-btn');

let selectedAmount = null;
let selectedMethod = null;

window.openDonateModal = function () {
  if (!donateModal) return;
  donateModal.classList.add('open');
  document.body.style.overflow = 'hidden';
};

function closeDonateModal() {
  donateModal?.classList.remove('open');
  document.body.style.overflow = '';
}

modalOverlay?.addEventListener('click', closeDonateModal);
modalClose?.addEventListener('click', closeDonateModal);

document.addEventListener('keydown', (event) => {
  if (event.key === 'Escape' && donateModal?.classList.contains('open')) {
    closeDonateModal();
  }
});

amountButtons.forEach((btn) => {
  btn.addEventListener('click', () => {
    amountButtons.forEach((b) => b.classList.remove('selected'));
    btn.classList.add('selected');
    selectedAmount = Number(btn.dataset.amount);
    if (customAmount) customAmount.value = '';
  });
});

customAmount?.addEventListener('input', (e) => {
  amountButtons.forEach((b) => b.classList.remove('selected'));
  selectedAmount = Number(e.target.value) || null;
});

paymentButtons.forEach((btn) => {
  btn.addEventListener('click', () => {
    paymentButtons.forEach((b) => b.classList.remove('selected'));
    btn.classList.add('selected');
    selectedMethod = btn.dataset.method;
  });
});

function showNotification(message, type = 'success') {
  document.querySelectorAll('.wfc-notification').forEach((n) => n.remove());
  const toast = document.createElement('div');
  toast.className = 'wfc-notification';
  toast.textContent = message;
  toast.style.background = type === 'success'
    ? 'linear-gradient(135deg, #22c55e, #16a34a)'
    : 'linear-gradient(135deg, #f87171, #dc2626)';
  toast.style.animation = 'slideIn 0.3s ease forwards';
  document.body.appendChild(toast);
  setTimeout(() => {
    toast.style.animation = 'slideOut 0.3s ease forwards';
    setTimeout(() => toast.remove(), 300);
  }, 4000);
}

const notificationStyles = document.createElement('style');
notificationStyles.textContent = `
  @keyframes slideIn { from { opacity: 0; transform: translateX(120px); } to { opacity: 1; transform: translateX(0); } }
  @keyframes slideOut { from { opacity: 1; transform: translateX(0); } to { opacity: 0; transform: translateX(120px); } }
`;
document.head.appendChild(notificationStyles);

submitDonation?.addEventListener('click', () => {
  if (!selectedAmount || selectedAmount < 10) {
    showNotification('Please choose at least ৳10 to donate.', 'error');
    return;
  }

  if (!selectedMethod) {
    showNotification('Select your preferred payment method.', 'error');
    return;
  }

  closeDonateModal();
  showNotification(`Thank you for donating ৳${selectedAmount.toLocaleString('en-BD')} via ${selectedMethod}!`);

  amountButtons.forEach((b) => b.classList.remove('selected'));
  paymentButtons.forEach((b) => b.classList.remove('selected'));
  if (customAmount) customAmount.value = '';
  selectedAmount = null;
  selectedMethod = null;
});

// === Background position tweaks for fun ===
function randomizePattern() {
  const items = document.querySelectorAll('.wfc-star, .wfc-cloud, .wfc-heart');
  items.forEach((item, index) => {
    const top = Math.random() * 80 + 5;
    const left = Math.random() * 80 + 5;
    item.style.top = `${top}%`;
    item.style.left = `${left}%`;
    item.style.animationDelay = `${index * 0.6}s`;
  });
}

window.addEventListener('load', randomizePattern);

console.log('%c🐾 Support Cats Initiative — MR Shop', 'font-size: 20px; color: #f97316; font-weight: 700;');
