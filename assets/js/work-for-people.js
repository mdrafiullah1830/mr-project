/* === Work for People JavaScript === */

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
const customAmountInput = document.getElementById('customAmount');
const donorNameInput = document.getElementById('donorName');
const donorEmailInput = document.getElementById('donorEmail');
const senderPhoneInput = document.getElementById('senderPhone');
const senderLastFourInput = document.getElementById('senderLastFour');
const transactionIdInput = document.getElementById('transactionId');
const donationMessageInput = document.getElementById('donationMessage');
const donationFeedback = document.getElementById('donationFeedback');

const DONATION_API_URL = window.MR_DONATION_API || 'http://localhost:5001/api/donations';
const PHONE_REGEX = /^01[3-9]\d{8}$/;

function setDonationFeedback(status, message) {
  if (!donationFeedback) return;
  donationFeedback.textContent = message || '';
  donationFeedback.className = status ? `wfp-feedback ${status}` : 'wfp-feedback';
}

function toggleSubmitLoading(isLoading) {
  if (!submitDonation) return;
  submitDonation.disabled = isLoading;
  submitDonation.textContent = isLoading ? 'Processing...' : 'Donate Now';
}

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
  if (customAmountInput) customAmountInput.value = '';
  [donorNameInput, donorEmailInput, senderPhoneInput, senderLastFourInput, transactionIdInput, donationMessageInput]
    .filter(Boolean)
    .forEach((input) => {
      input.value = '';
    });
  setDonationFeedback('', '');
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
    selectedAmount = Number(btn.dataset.amount);
    if (customAmountInput) customAmountInput.value = '';
  });
});

// Custom amount
if (customAmountInput) {
  customAmountInput.addEventListener('input', (e) => {
    const value = Number(e.target.value);
    selectedAmount = Number.isFinite(value) && value > 0 ? value : null;
    document.querySelectorAll('.wfp-amount-btn').forEach((btn) => {
      btn.classList.remove('selected');
    });
  });
}

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
submitDonation.addEventListener('click', async () => {
  setDonationFeedback('', '');

  if (!Number.isFinite(selectedAmount) || selectedAmount < 10) {
    setDonationFeedback('error', 'Please select or enter a donation amount (minimum ৳10).');
    return;
  }

  if (!selectedPaymentMethod) {
    setDonationFeedback('error', 'Please choose a payment method.');
    return;
  }

  const donorName = donorNameInput?.value.trim() || '';
  if (donorName.length < 2) {
    setDonationFeedback('error', 'Please enter your full name.');
    donorNameInput?.focus();
    return;
  }

  const senderNumber = senderPhoneInput?.value.trim() || '';
  if (!PHONE_REGEX.test(senderNumber)) {
    setDonationFeedback('error', 'Enter a valid Bangladeshi wallet number (01XXXXXXXXX).');
    senderPhoneInput?.focus();
    return;
  }

  let senderLastFour = senderLastFourInput?.value.trim() || '';
  if (!senderLastFour) {
    senderLastFour = senderNumber.slice(-4);
    if (senderLastFourInput) senderLastFourInput.value = senderLastFour;
  }

  if (!/^\d{4}$/.test(senderLastFour)) {
    setDonationFeedback('error', 'Last four digits must be exactly 4 numbers.');
    senderLastFourInput?.focus();
    return;
  }

  const transactionId = transactionIdInput?.value.trim() || '';
  if (transactionId.length < 5) {
    setDonationFeedback('error', 'Please provide the transaction ID from your send money receipt.');
    transactionIdInput?.focus();
    return;
  }

  const payload = {
    amount: Number(selectedAmount.toFixed(2)),
    provider: selectedPaymentMethod,
    donorName,
    donorEmail: donorEmailInput?.value.trim() || '',
    senderNumber,
    senderLastFour,
    transactionId,
    message: donationMessageInput?.value.trim() || '',
    source: 'work-for-people-page',
  };

  await submitDonationRequest(payload);
});

async function submitDonationRequest(payload) {
  try {
    toggleSubmitLoading(true);
    setDonationFeedback('info', 'Submitting your donation...');
    const response = await fetch(DONATION_API_URL, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    const result = await response.json();
    if (!response.ok || !result.success) {
      throw new Error(result.error || 'Unable to submit donation right now.');
    }

    const receipt = result.receipt || {};
    const providerLabel = payload.provider?.toUpperCase() || 'bKASH';
    showSuccessNotification(`Thank you, ${payload.donorName}! ৳${payload.amount} via ${providerLabel} was recorded. TxID: ${payload.transactionId}.`);
    setDonationFeedback('success', result.message || 'Donation recorded successfully.');

    // Close modal after a short pause so the user can read the message
    setTimeout(() => {
      closeDonateModal();
    }, 800);

    console.log('Donation receipt', receipt);
  } catch (error) {
    const message = error.message || 'Unable to submit donation right now.';
    setDonationFeedback('error', message);
    showErrorNotification(message);
  } finally {
    toggleSubmitLoading(false);
  }
}

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

function showErrorNotification(message) {
  const notification = document.createElement('div');
  notification.style.cssText = `
    position: fixed;
    top: 24px;
    right: 24px;
    max-width: 400px;
    padding: 20px 24px;
    background: linear-gradient(135deg, #ef4444, #b91c1c);
    color: white;
    border-radius: 12px;
    box-shadow: 0 10px 30px rgba(239, 68, 68, 0.3);
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
