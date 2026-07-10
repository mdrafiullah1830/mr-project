/* === Book Page JavaScript === */

// Product data
const book = {
  id: 401,
  title: 'The Art of Code',
  author: 'Jane Doe',
  formats: {
    hardcover: { price: 1450, label: 'Hardcover' },
    paperback: { price: 890, label: 'Paperback' },
    pdf: { price: 450, label: 'PDF' },
    epub: { price: 450, label: 'ePub' }
  },
  image: 'https://images.unsplash.com/photo-1543002588-bfa74002ed7e?q=80&w=800&auto=format&fit=crop',
  isbn: '978-1234567890',
  description: 'A masterful exploration of software craftsmanship, blending technical depth with artistic insight.'
};

let selectedFormat = 'hardcover';

// === Theme Toggle ===
const themeToggle = document.getElementById('themeToggle');
const htmlEl = document.documentElement;

// Load saved theme
const savedTheme = localStorage.getItem('mrshop-theme') || 'light';
if (savedTheme === 'dark') {
  htmlEl.classList.add('theme-dark');
  themeToggle.textContent = '☀️';
}

// Toggle theme
themeToggle.addEventListener('click', () => {
  htmlEl.classList.toggle('theme-dark');
  const isDark = htmlEl.classList.contains('theme-dark');
  themeToggle.textContent = isDark ? '☀️' : '🌙';
  localStorage.setItem('mrshop-theme', isDark ? 'dark' : 'light');
});

// === Format Selection ===
const formatGrid = document.getElementById('formatGrid');
const priceDisplay = document.getElementById('priceDisplay');

formatGrid.addEventListener('click', (e) => {
  const btn = e.target.closest('.format-btn');
  if (!btn) return;

  // Update selection
  document.querySelectorAll('.format-btn').forEach(b => b.classList.remove('selected'));
  btn.classList.add('selected');

  // Update format and price
  selectedFormat = btn.dataset.format;
  priceDisplay.textContent = btn.dataset.price;
});

// Set default selection
document.querySelector('[data-format="hardcover"]').classList.add('selected');

// === Look Inside Modal ===
const lookInsideBtn = document.getElementById('lookInsideBtn');
const previewModal = document.getElementById('previewModal');
const modalOverlay = document.getElementById('modalOverlay');
const modalClose = document.getElementById('modalClose');

function openModal() {
  previewModal.classList.add('open');
  document.body.style.overflow = 'hidden';
}

function closeModal() {
  previewModal.classList.remove('open');
  document.body.style.overflow = '';
}

lookInsideBtn.addEventListener('click', openModal);
modalClose.addEventListener('click', closeModal);
modalOverlay.addEventListener('click', closeModal);

// ESC key to close
document.addEventListener('keydown', (e) => {
  if (e.key === 'Escape' && previewModal.classList.contains('open')) {
    closeModal();
  }
});

// === Chapter Preview ===
const chapterItems = document.querySelectorAll('.chapter-item');

chapterItems.forEach((item) => {
  item.addEventListener('click', () => {
    const chapterNum = item.dataset.chapter;
    const chapterTitle = item.querySelector('.ch-title').textContent;
    
    // Update modal content
    document.getElementById('modalBody').innerHTML = `
      <p><strong>Chapter ${chapterNum}: ${chapterTitle}</strong></p>
      <p>
        This is a preview of Chapter ${chapterNum}. In this chapter, you'll explore essential concepts and patterns that will transform how you approach software development.
      </p>
      <p>
        With real-world examples and practical exercises, you'll learn to write code that's not just functional, but elegant and maintainable. The lessons here apply across languages and frameworks.
      </p>
      <p><em>Continue reading to dive deeper into the principles and practices that define master craftsmanship...</em></p>
    `;
    
    openModal();
  });
});

// === Social Share ===
const shareBtns = document.querySelectorAll('.share-btn');
const pageUrl = encodeURIComponent(window.location.href);
const pageTitle = encodeURIComponent(book.title + ' by ' + book.author);

shareBtns.forEach((btn) => {
  btn.addEventListener('click', () => {
    const network = btn.dataset.network;
    let url = '';

    switch (network) {
      case 'facebook':
        url = `https://www.facebook.com/sharer/sharer.php?u=${pageUrl}`;
        break;
      case 'twitter':
        url = `https://twitter.com/intent/tweet?url=${pageUrl}&text=${pageTitle}`;
        break;
      case 'whatsapp':
        url = `https://wa.me/?text=${pageTitle}%20${pageUrl}`;
        break;
      case 'email':
        url = `mailto:?subject=${pageTitle}&body=Check out this book: ${pageUrl}`;
        break;
    }

    if (url) {
      window.open(url, '_blank', 'width=600,height=500');
    }
  });
});

// === Cart Functions ===
// Use shared MR_Cart module instead of local cart functions

function showToast(message) {
  if (typeof MR_Cart !== 'undefined') {
    MR_Cart.showToast(message);
  } else {
    const toast = document.getElementById('bkToast');
    toast.textContent = message;
    toast.classList.add('show');
    setTimeout(() => {
      toast.classList.remove('show');
    }, 2500);
  }
}

// === Add to Cart ===
const addToCartBtn = document.getElementById('addToCartBtn');

addToCartBtn.addEventListener('click', async () => {
  const formatData = book.formats[selectedFormat];
  
  if (typeof MR_Cart !== 'undefined') {
    // Use shared cart module
    await MR_Cart.addItem(book.id, 1);
    showToast(`Added ${formatData.label} to cart`);
  } else {
    // Fallback to local cart
    const stored = localStorage.getItem('mr_shop_cart');
    const cart = stored ? JSON.parse(stored) : [];
    
    const existingIndex = cart.findIndex(
      (item) => item.id === book.id && item.format === selectedFormat
    );

    if (existingIndex !== -1) {
      cart[existingIndex].quantity += 1;
    } else {
      cart.push({
        id: book.id,
        name: book.title,
        author: book.author,
        format: formatData.label,
        price: formatData.price,
        image: book.image,
        description: book.description,
        quantity: 1
      });
    }

    localStorage.setItem('mr_shop_cart', JSON.stringify(cart));
    showToast(`Added ${formatData.label} to cart`);
  }
});

// === Buy Now ===
const buyNowBtn = document.getElementById('buyNowBtn');

buyNowBtn.addEventListener('click', () => {
  addToCartBtn.click();
  setTimeout(() => {
    window.location.href = 'cart.html';
  }, 300);
});

// === Verified Reviews ===
const REV_KEY = 'mr_reviews_401';

const sampleReviews = [
  {
    author: 'Sarah Johnson',
    date: 'December 10, 2025',
    stars: 5,
    text: 'An absolute masterpiece. This book changed the way I think about software design. Every chapter is packed with insights that apply to real-world projects.',
    verified: true
  },
  {
    author: 'Michael Chen',
    date: 'December 5, 2025',
    stars: 5,
    text: 'Clear, concise, and beautifully written. Jane Doe has a gift for explaining complex patterns in a way that clicks immediately. Highly recommend for any developer.',
    verified: true
  },
  {
    author: 'Emily Rodriguez',
    date: 'November 28, 2025',
    stars: 4,
    text: 'Fantastic resource with great examples. Lost one star only because I wish there were more exercises at the end of each chapter. Still a must-read!',
    verified: true
  },
  {
    author: 'David Kim',
    date: 'November 20, 2025',
    stars: 5,
    text: 'This is the book I recommend to every junior developer on my team. It teaches principles that transcend any single language or framework.',
    verified: true
  }
];

function loadReviews() {
  const stored = localStorage.getItem(REV_KEY);
  let reviews = stored ? JSON.parse(stored) : sampleReviews;
  if (!stored) localStorage.setItem(REV_KEY, JSON.stringify(sampleReviews));
  renderReviews(reviews);
}

function renderReviews(reviews) {
  const container = document.getElementById('reviewsList');
  
  if (!reviews.length) {
    container.innerHTML = '<p style="color: var(--bk-muted);">No reviews yet. Be the first to review this book!</p>';
    return;
  }

  container.innerHTML = reviews.map((r) => `
    <div class="review-card">
      <div class="review-header">
        <div class="review-author">${escapeHtml(r.author)}</div>
        <div class="review-date">${escapeHtml(r.date)}</div>
      </div>
      <div class="review-stars">${'★'.repeat(r.stars)}${'☆'.repeat(5 - r.stars)}</div>
      <div class="review-text">${escapeHtml(r.text)}</div>
      ${r.verified ? '<div class="review-verified">Verified Purchase</div>' : ''}
    </div>
  `).join('');
}

function escapeHtml(text) {
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}

// === Scroll Reveal Animation ===
const observerOptions = {
  threshold: 0.1,
  rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver((entries) => {
  entries.forEach((entry) => {
    if (entry.isIntersecting) {
      entry.target.classList.add('visible');
      observer.unobserve(entry.target);
    }
  });
}, observerOptions);

document.querySelectorAll('.reveal').forEach((el) => {
  observer.observe(el);
});

// === Init ===
loadReviews();
