/* === Work for People JavaScript === */

// === Theme Toggle ===
const themeToggle = document.getElementById('themeToggle');
const htmlEl = document.documentElement;

const savedTheme = localStorage.getItem('mrshop-theme') || 'light';
if (savedTheme === 'dark') {
  htmlEl.classList.add('theme-dark');
  themeToggle.textContent = '☀️';
}

themeToggle.addEventListener('click', () => {
  htmlEl.classList.toggle('theme-dark');
  const isDark = htmlEl.classList.contains('theme-dark');
  themeToggle.textContent = isDark ? '☀️' : '🌙';
  localStorage.setItem('mrshop-theme', isDark ? 'dark' : 'light');
});

// === Animated Counter ===
function animateCounter(element, target, duration = 2000) {
  const start = 0;
  const increment = target / (duration / 16);
  let current = start;

  const timer = setInterval(() => {
    current += increment;
    if (current >= target) {
      element.textContent = target.toLocaleString();
      clearInterval(timer);
    } else {
      element.textContent = Math.floor(current).toLocaleString();
    }
  }, 16);
}

// Trigger counter animation on scroll
const statObserver = new IntersectionObserver(
  (entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        const target = parseInt(entry.target.dataset.target);
        animateCounter(entry.target, target);
        statObserver.unobserve(entry.target);
      }
    });
  },
  { threshold: 0.5 }
);

document.querySelectorAll('.wfp-stat-number').forEach((el) => {
  statObserver.observe(el);
});

// === Feature Cards Scroll Animation ===
const featureObserver = new IntersectionObserver(
  (entries) => {
    entries.forEach((entry, index) => {
      if (entry.isIntersecting) {
        setTimeout(() => {
          entry.target.classList.add('visible');
        }, index * 100);
        featureObserver.unobserve(entry.target);
      }
    });
  },
  { threshold: 0.2 }
);

document.querySelectorAll('[data-animate]').forEach((el) => {
  featureObserver.observe(el);
});

// === Progress Bars Animation ===
const progressObserver = new IntersectionObserver(
  (entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        const fills = entry.target.querySelectorAll('.wfp-progress-fill');
        fills.forEach((fill, index) => {
          setTimeout(() => {
            const progress = fill.dataset.progress;
            fill.style.width = progress + '%';
          }, index * 200);
        });
        progressObserver.unobserve(entry.target);
      }
    });
  },
  { threshold: 0.5 }
);

const impactBars = document.querySelector('.wfp-impact-bars');
if (impactBars) {
  progressObserver.observe(impactBars);
}

// === Testimonial Slider ===
const testimonialTrack = document.getElementById('testimonialTrack');
const prevBtn = document.getElementById('testimonialPrev');
const nextBtn = document.getElementById('testimonialNext');

let currentTestimonial = 0;
const totalTestimonials = document.querySelectorAll('.wfp-testimonial-card').length;

function updateTestimonialSlider() {
  const offset = -currentTestimonial * 100;
  testimonialTrack.style.transform = `translateX(${offset}%)`;
}

prevBtn.addEventListener('click', () => {
  currentTestimonial = (currentTestimonial - 1 + totalTestimonials) % totalTestimonials;
  updateTestimonialSlider();
});

nextBtn.addEventListener('click', () => {
  currentTestimonial = (currentTestimonial + 1) % totalTestimonials;
  updateTestimonialSlider();
});

// Auto-slide testimonials every 8 seconds
setInterval(() => {
  currentTestimonial = (currentTestimonial + 1) % totalTestimonials;
  updateTestimonialSlider();
}, 8000);

// === Donation Modal ===
const donateModal = document.getElementById('donateModal');
const modalOverlay = document.getElementById('modalOverlay');
const modalClose = document.getElementById('modalClose');
const submitDonation = document.getElementById('submitDonation');

let selectedAmount = null;
let selectedPaymentMethod = null;

window.openDonateModal = function() {
  donateModal.classList.add('open');
  document.body.style.overflow = 'hidden';
};

function closeDonateModal() {
  donateModal.classList.remove('open');
  document.body.style.overflow = '';
  resetDonateForm();
}

function resetDonateForm() {
  selectedAmount = null;
  selectedPaymentMethod = null;
  document.querySelectorAll('.wfp-amount-btn').forEach((btn) => {
    btn.classList.remove('selected');
  });
  document.querySelectorAll('.wfp-payment-btn').forEach((btn) => {
    btn.classList.remove('selected');
  });
  document.getElementById('customAmount').value = '';
}

modalClose.addEventListener('click', closeDonateModal);
modalOverlay.addEventListener('click', closeDonateModal);

// ESC key to close
document.addEventListener('keydown', (e) => {
  if (e.key === 'Escape' && donateModal.classList.contains('open')) {
    closeDonateModal();
  }
});

// Amount selection
document.querySelectorAll('.wfp-amount-btn').forEach((btn) => {
  btn.addEventListener('click', () => {
    document.querySelectorAll('.wfp-amount-btn').forEach((b) => {
      b.classList.remove('selected');
    });
    btn.classList.add('selected');
    selectedAmount = parseInt(btn.dataset.amount);
    document.getElementById('customAmount').value = '';
  });
});

// Custom amount
document.getElementById('customAmount').addEventListener('input', (e) => {
  selectedAmount = parseInt(e.target.value);
  document.querySelectorAll('.wfp-amount-btn').forEach((btn) => {
    btn.classList.remove('selected');
  });
});

// Payment method selection
document.querySelectorAll('.wfp-payment-btn').forEach((btn) => {
  btn.addEventListener('click', () => {
    document.querySelectorAll('.wfp-payment-btn').forEach((b) => {
      b.classList.remove('selected');
    });
    btn.classList.add('selected');
    selectedPaymentMethod = btn.dataset.method;
  });
});

// Submit donation
submitDonation.addEventListener('click', () => {
  if (!selectedAmount || selectedAmount < 10) {
    alert('Please select or enter a donation amount (minimum ৳10)');
    return;
  }

  if (!selectedPaymentMethod) {
    alert('Please select a payment method');
    return;
  }

  // Show success message
  showSuccessNotification(`Thank you! Your donation of ৳${selectedAmount} via ${selectedPaymentMethod.toUpperCase()} will help transform lives.`);

  // Log donation (in real app, send to backend)
  console.log({
    amount: selectedAmount,
    method: selectedPaymentMethod,
    timestamp: new Date().toISOString(),
    source: 'work-for-people-page'
  });

  // Close modal
  closeDonateModal();
});

// Success notification
function showSuccessNotification(message) {
  const notification = document.createElement('div');
  notification.style.cssText = `
    position: fixed;
    top: 24px;
    right: 24px;
    max-width: 400px;
    padding: 20px 24px;
    background: linear-gradient(135deg, #10b981, #059669);
    color: white;
    border-radius: 12px;
    box-shadow: 0 10px 30px rgba(16, 185, 129, 0.3);
    font-weight: 600;
    z-index: 300;
    animation: slideInRight 0.4s ease;
  `;
  notification.textContent = message;

  document.body.appendChild(notification);

  setTimeout(() => {
    notification.style.animation = 'slideOutRight 0.4s ease';
    setTimeout(() => {
      notification.remove();
    }, 400);
  }, 4000);
}

// Add animation styles
const style = document.createElement('style');
style.textContent = `
  @keyframes slideInRight {
    from {
      opacity: 0;
      transform: translateX(100px);
    }
    to {
      opacity: 1;
      transform: translateX(0);
    }
  }
  @keyframes slideOutRight {
    from {
      opacity: 1;
      transform: translateX(0);
    }
    to {
      opacity: 0;
      transform: translateX(100px);
    }
  }
`;
document.head.appendChild(style);

// === Smooth Scroll for Internal Links ===
document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
  anchor.addEventListener('click', function (e) {
    e.preventDefault();
    const target = document.querySelector(this.getAttribute('href'));
    if (target) {
      target.scrollIntoView({
        behavior: 'smooth',
        block: 'start'
      });
    }
  });
});

// === Page Load Animation ===
window.addEventListener('load', () => {
  document.body.style.opacity = '0';
  document.body.style.transition = 'opacity 0.4s ease';
  
  setTimeout(() => {
    document.body.style.opacity = '1';
  }, 100);
});
