// === Support Children - Interactive Features ===

// Theme Toggle
const themeToggle = document.querySelector('.wfc-theme-btn');
const body = document.body;

// Load saved theme
const savedTheme = localStorage.getItem('mrshop-theme');
if (savedTheme === 'dark') {
  body.classList.add('theme-dark');
  if (themeToggle) themeToggle.textContent = '☀️';
}

// Theme toggle handler
if (themeToggle) {
  themeToggle.addEventListener('click', () => {
    body.classList.toggle('theme-dark');
    const isDark = body.classList.contains('theme-dark');
    themeToggle.textContent = isDark ? '☀️' : '🌙';
    localStorage.setItem('mrshop-theme', isDark ? 'dark' : 'light');
  });
}

// === Animated Counters ===
function animateCounter(element, target, duration = 2000) {
  const start = 0;
  const increment = target / (duration / 16);
  let current = start;

  const timer = setInterval(() => {
    current += increment;
    if (current >= target) {
      element.textContent = target.toLocaleString('en-BD');
      clearInterval(timer);
    } else {
      element.textContent = Math.floor(current).toLocaleString('en-BD');
    }
  }, 16);
}

// === Intersection Observer for Stats ===
const statObserver = new IntersectionObserver((entries) => {
  entries.forEach(entry => {
    if (entry.isIntersecting) {
      const number = entry.target.querySelector('.wfc-stat-number');
      const target = parseInt(number.dataset.target);
      animateCounter(number, target);
      statObserver.unobserve(entry.target);
    }
  });
}, { threshold: 0.3 });

document.querySelectorAll('.wfc-stat-card').forEach(card => {
  statObserver.observe(card);
});

// === Feature Cards Scroll Animation ===
const featureObserver = new IntersectionObserver((entries) => {
  entries.forEach((entry, index) => {
    if (entry.isIntersecting) {
      setTimeout(() => {
        entry.target.classList.add('visible');
      }, index * 100);
      featureObserver.unobserve(entry.target);
    }
  });
}, { threshold: 0.2 });

document.querySelectorAll('.wfc-feature-card').forEach(card => {
  featureObserver.observe(card);
});

// === Progress Bars Animation ===
const progressObserver = new IntersectionObserver((entries) => {
  entries.forEach(entry => {
    if (entry.isIntersecting) {
      const fill = entry.target.querySelector('.wfc-progress-fill');
      const percent = parseInt(fill.dataset.percent);
      
      setTimeout(() => {
        fill.style.width = percent + '%';
      }, 300);
      
      progressObserver.unobserve(entry.target);
    }
  });
}, { threshold: 0.5 });

document.querySelectorAll('.wfc-impact-item').forEach(item => {
  progressObserver.observe(item);
});

// === Story Slider ===
class StorySlider {
  constructor() {
    this.track = document.querySelector('.wfc-story-track');
    this.prevBtn = document.querySelector('.wfc-slider-prev');
    this.nextBtn = document.querySelector('.wfc-slider-next');
    this.cards = document.querySelectorAll('.wfc-story-card');
    this.currentIndex = 0;
    this.autoSlideInterval = null;

    if (this.track && this.cards.length > 0) {
      this.init();
    }
  }

  init() {
    this.prevBtn?.addEventListener('click', () => this.prev());
    this.nextBtn?.addEventListener('click', () => this.next());
    this.startAutoSlide();

    // Pause on hover
    this.track.addEventListener('mouseenter', () => this.stopAutoSlide());
    this.track.addEventListener('mouseleave', () => this.startAutoSlide());
  }

  prev() {
    this.currentIndex = (this.currentIndex - 1 + this.cards.length) % this.cards.length;
    this.updateSlider();
  }

  next() {
    this.currentIndex = (this.currentIndex + 1) % this.cards.length;
    this.updateSlider();
  }

  updateSlider() {
    const offset = -this.currentIndex * 100;
    this.track.style.transform = `translateX(${offset}%)`;
  }

  startAutoSlide() {
    this.stopAutoSlide();
    this.autoSlideInterval = setInterval(() => this.next(), 8000);
  }

  stopAutoSlide() {
    if (this.autoSlideInterval) {
      clearInterval(this.autoSlideInterval);
      this.autoSlideInterval = null;
    }
  }
}

const storySlider = new StorySlider();

// === Donation Modal ===
class DonationModal {
  constructor() {
    this.modal = document.getElementById('donationModal');
    this.openBtns = document.querySelectorAll('.open-donation-modal');
    this.closeBtn = document.querySelector('.wfc-modal-close');
    this.overlay = document.querySelector('.wfc-modal-overlay');
    this.amountBtns = document.querySelectorAll('.wfc-amount-btn');
    this.customAmountInput = document.getElementById('customAmount');
    this.paymentBtns = document.querySelectorAll('.wfc-payment-btn');
    this.submitBtn = document.getElementById('submitDonation');
    
    this.selectedAmount = null;
    this.selectedPayment = null;

    if (this.modal) {
      this.init();
    }
  }

  init() {
    // Open modal
    this.openBtns.forEach(btn => {
      btn.addEventListener('click', () => this.open());
    });

    // Close modal
    this.closeBtn?.addEventListener('click', () => this.close());
    this.overlay?.addEventListener('click', () => this.close());

    // Amount selection
    this.amountBtns.forEach(btn => {
      btn.addEventListener('click', () => {
        this.amountBtns.forEach(b => b.classList.remove('selected'));
        btn.classList.add('selected');
        this.selectedAmount = parseInt(btn.dataset.amount);
        this.customAmountInput.value = '';
      });
    });

    // Custom amount
    this.customAmountInput?.addEventListener('input', (e) => {
      this.amountBtns.forEach(b => b.classList.remove('selected'));
      this.selectedAmount = parseInt(e.target.value) || null;
    });

    // Payment method
    this.paymentBtns.forEach(btn => {
      btn.addEventListener('click', () => {
        this.paymentBtns.forEach(b => b.classList.remove('selected'));
        btn.classList.add('selected');
        this.selectedPayment = btn.dataset.method;
      });
    });

    // Submit
    this.submitBtn?.addEventListener('click', () => this.submit());
  }

  open() {
    this.modal.classList.add('open');
    document.body.style.overflow = 'hidden';
  }

  close() {
    this.modal.classList.remove('open');
    document.body.style.overflow = '';
  }

  submit() {
    if (!this.selectedAmount) {
      this.showNotification('❌ Please select or enter a donation amount', 'error');
      return;
    }

    if (!this.selectedPayment) {
      this.showNotification('❌ Please select a payment method', 'error');
      return;
    }

    // Simulate donation processing
    this.close();
    this.showNotification(`✅ Thank you for your generous donation of ৳${this.selectedAmount.toLocaleString('en-BD')}! 💚`, 'success');
    
    // Reset form
    this.resetForm();
  }

  resetForm() {
    this.amountBtns.forEach(b => b.classList.remove('selected'));
    this.paymentBtns.forEach(b => b.classList.remove('selected'));
    this.customAmountInput.value = '';
    this.selectedAmount = null;
    this.selectedPayment = null;
  }

  showNotification(message, type = 'success') {
    // Remove existing notifications
    document.querySelectorAll('.wfc-notification').forEach(n => n.remove());

    const notification = document.createElement('div');
    notification.className = 'wfc-notification';
    notification.style.cssText = `
      position: fixed;
      top: 100px;
      right: 24px;
      background: ${type === 'success' ? 'linear-gradient(135deg, #4ade80, #22c55e)' : 'linear-gradient(135deg, #ef4444, #dc2626)'};
      color: white;
      padding: 20px 28px;
      border-radius: 16px;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
      z-index: 300;
      font-size: 1.05rem;
      font-weight: 600;
      animation: slideIn 0.4s ease;
      max-width: 400px;
      font-family: 'Nunito', sans-serif;
    `;
    notification.textContent = message;

    document.body.appendChild(notification);

    setTimeout(() => {
      notification.style.animation = 'slideOut 0.4s ease';
      setTimeout(() => notification.remove(), 400);
    }, 5000);
  }
}

// Add notification animations
const style = document.createElement('style');
style.textContent = `
  @keyframes slideIn {
    from {
      transform: translateX(400px);
      opacity: 0;
    }
    to {
      transform: translateX(0);
      opacity: 1;
    }
  }
  
  @keyframes slideOut {
    from {
      transform: translateX(0);
      opacity: 1;
    }
    to {
      transform: translateX(400px);
      opacity: 0;
    }
  }
`;
document.head.appendChild(style);

const donationModal = new DonationModal();

// === Background Pattern Random Positioning ===
function randomizePatternPositions() {
  const stars = document.querySelectorAll('.wfc-star');
  const clouds = document.querySelectorAll('.wfc-cloud');
  const hearts = document.querySelectorAll('.wfc-heart');

  const randomPosition = () => ({
    top: Math.random() * 90 + '%',
    left: Math.random() * 90 + '%',
  });

  stars.forEach(star => {
    const pos = randomPosition();
    star.style.top = pos.top;
    star.style.left = pos.left;
  });

  clouds.forEach(cloud => {
    const pos = randomPosition();
    cloud.style.top = pos.top;
    cloud.style.left = pos.left;
  });

  hearts.forEach(heart => {
    const pos = randomPosition();
    heart.style.top = pos.top;
    heart.style.left = pos.left;
  });
}

// Randomize on load
randomizePatternPositions();

// === Smooth Scroll for Navigation Links ===
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
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

// === Console Message ===
console.log('%c💙 Support Children Initiative 💚', 'font-size: 24px; font-weight: bold; color: #60a5fa; font-family: Fredoka, Nunito, sans-serif;');
console.log('%c10% of our profits go to help underprivileged children 🌈', 'font-size: 14px; color: #64748b; font-family: Nunito, sans-serif;');
