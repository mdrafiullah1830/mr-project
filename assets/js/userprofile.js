// Theme Toggle
const themeToggle = document.getElementById('themeToggle');
const themeIcon = document.querySelector('.theme-icon');
const html = document.documentElement;

// Initialize theme from localStorage
const savedTheme = localStorage.getItem('mr_shop_theme') || 'light';
html.setAttribute('data-theme', savedTheme);
updateThemeIcon(savedTheme);

themeToggle.addEventListener('click', () => {
  const currentTheme = html.getAttribute('data-theme');
  const newTheme = currentTheme === 'light' ? 'dark' : 'light';
  
  html.setAttribute('data-theme', newTheme);
  localStorage.setItem('mr_shop_theme', newTheme);
  updateThemeIcon(newTheme);
  
  // Add bounce animation
  themeToggle.style.animation = 'none';
  setTimeout(() => {
    themeToggle.style.animation = 'bounce 0.5s ease';
  }, 10);
});

function updateThemeIcon(theme) {
  themeIcon.textContent = theme === 'light' ? '🌙' : '☀️';
}

// Section Navigation
function switchSection(sectionId) {
  // Update active menu item
  document.querySelectorAll('.menu-item').forEach(item => {
    item.classList.remove('active');
  });
  document.querySelector(`[data-section="${sectionId}"]`).classList.add('active');
  
  // Update active content section
  document.querySelectorAll('.content-section').forEach(section => {
    section.classList.remove('active');
  });
  document.getElementById(sectionId).classList.add('active');
  
  // Load section-specific data
  if (sectionId === 'order-history') {
    loadOrders();
  } else if (sectionId === 'wishlist') {
    loadWishlist();
  } else if (sectionId === 'recently-viewed') {
    loadRecentlyViewed();
  }
}

// Edit Profile
const editBtn = document.getElementById('editBtn');
const saveBtn = document.getElementById('saveBtn');
const infoForm = document.getElementById('infoForm');
let isEditing = false;

function toggleEdit() {
  isEditing = !isEditing;
  
  const inputs = infoForm.querySelectorAll('input, textarea, select');
  inputs.forEach(input => {
    input.disabled = !isEditing;
  });
  
  if (isEditing) {
    editBtn.innerHTML = '<span class="edit-icon">❌</span><span class="edit-text">Cancel</span>';
    editBtn.style.background = 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)';
    saveBtn.classList.remove('hidden');
  } else {
    editBtn.innerHTML = '<span class="edit-icon">✏️</span><span class="edit-text">Edit</span>';
    editBtn.style.background = 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)';
    saveBtn.classList.add('hidden');
  }
}

infoForm.addEventListener('submit', (e) => {
  e.preventDefault();
  
  // Get form data
  const formData = {
    fullName: document.getElementById('fullName').value,
    phoneNumber: document.getElementById('phoneNumber').value,
    emailAddress: document.getElementById('emailAddress').value,
    address: document.getElementById('address').value,
    dob: document.getElementById('dob').value,
    gender: document.getElementById('gender').value
  };
  
  // Save to localStorage
  localStorage.setItem('mr_shop_user_profile', JSON.stringify(formData));
  
  // Update profile display
  document.getElementById('profileName').textContent = formData.fullName.split(' ')[0] + ' ' + (formData.fullName.split(' ')[1] || '');
  document.getElementById('profileEmail').textContent = formData.emailAddress;
  
  // Show success message
  showNotification('Profile updated successfully! ✅', 'success');
  
  // Exit edit mode
  toggleEdit();
});

// Load saved profile data
function loadProfileData() {
  const savedData = localStorage.getItem('mr_shop_user_profile');
  if (savedData) {
    const data = JSON.parse(savedData);
    document.getElementById('fullName').value = data.fullName;
    document.getElementById('phoneNumber').value = data.phoneNumber;
    document.getElementById('emailAddress').value = data.emailAddress;
    document.getElementById('address').value = data.address;
    document.getElementById('dob').value = data.dob;
    document.getElementById('gender').value = data.gender;
    
    document.getElementById('profileName').textContent = data.fullName.split(' ')[0] + ' ' + (data.fullName.split(' ')[1] || '');
    document.getElementById('profileEmail').textContent = data.emailAddress;
  }
}

// Change Profile Photo
function changeProfilePhoto() {
  const input = document.createElement('input');
  input.type = 'file';
  input.accept = 'image/*';
  
  input.onchange = (e) => {
    const file = e.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (event) => {
        document.getElementById('profilePhoto').src = event.target.result;
        localStorage.setItem('mr_shop_profile_photo', event.target.result);
        showNotification('Profile photo updated! 📸', 'success');
      };
      reader.readAsDataURL(file);
    }
  };
  
  input.click();
}

// Load saved profile photo
function loadProfilePhoto() {
  const savedPhoto = localStorage.getItem('mr_shop_profile_photo');
  if (savedPhoto) {
    document.getElementById('profilePhoto').src = savedPhoto;
  }
}

// Orders Data
const ordersData = [
  {
    id: '#MR5001',
    status: 'delivered',
    date: '2025-11-28',
    total: '৳1,250',
    items: 3,
    images: ['https://picsum.photos/seed/order1a/200', 'https://picsum.photos/seed/order1b/200', 'https://picsum.photos/seed/order1c/200']
  },
  {
    id: '#MR5000',
    status: 'pending',
    date: '2025-11-27',
    total: '৳850',
    items: 2,
    images: ['https://picsum.photos/seed/order2a/200', 'https://picsum.photos/seed/order2b/200']
  },
  {
    id: '#MR4999',
    status: 'delivered',
    date: '2025-11-25',
    total: '৳2,100',
    items: 5,
    images: ['https://picsum.photos/seed/order3a/200', 'https://picsum.photos/seed/order3b/200', 'https://picsum.photos/seed/order3c/200']
  },
  {
    id: '#MR4998',
    status: 'delivered',
    date: '2025-11-22',
    total: '৳650',
    items: 1,
    images: ['https://picsum.photos/seed/order4a/200']
  },
  {
    id: '#MR4997',
    status: 'cancelled',
    date: '2025-11-20',
    total: '৳320',
    items: 1,
    images: ['https://picsum.photos/seed/order5a/200']
  },
  {
    id: '#MR4996',
    status: 'delivered',
    date: '2025-11-18',
    total: '৳1,450',
    items: 4,
    images: ['https://picsum.photos/seed/order6a/200', 'https://picsum.photos/seed/order6b/200']
  }
];

let currentFilter = 'all';

function loadOrders() {
  const container = document.getElementById('ordersContainer');
  const filteredOrders = currentFilter === 'all' 
    ? ordersData 
    : ordersData.filter(order => order.status === currentFilter);
  
  if (filteredOrders.length === 0) {
    container.innerHTML = `
      <div class="empty-state">
        <div class="empty-state-icon">📦</div>
        <p class="empty-state-text">No orders found</p>
      </div>
    `;
    return;
  }
  
  container.innerHTML = filteredOrders.map(order => `
    <div class="order-card">
      <div class="order-header">
        <span class="order-id">${order.id}</span>
        <span class="order-status ${order.status}">${order.status}</span>
      </div>
      <div class="order-details">
        <div class="order-detail-item">
          <span>📅</span>
          <span>${new Date(order.date).toLocaleDateString('en-GB')}</span>
        </div>
        <div class="order-detail-item">
          <span>💰</span>
          <span>${order.total}</span>
        </div>
        <div class="order-detail-item">
          <span>📦</span>
          <span>${order.items} item${order.items > 1 ? 's' : ''}</span>
        </div>
      </div>
      <div class="order-items">
        ${order.images.map(img => `
          <img src="${img}" alt="Product" class="order-item-thumb">
        `).join('')}
      </div>
    </div>
  `).join('');
}

function filterOrders(filter, element) {
  currentFilter = filter;
  
  // Update active filter button
  document.querySelectorAll('.filter-btn').forEach(btn => {
    btn.classList.remove('active');
  });
  if (element) {
    element.classList.add('active');
  }
  
  loadOrders();
}

// Wishlist Data
const wishlistData = [
  {
    id: 1,
    name: 'Antique Coin Collection',
    price: '৳450',
    image: 'https://picsum.photos/seed/wishlist1/300'
  },
  {
    id: 2,
    name: 'Bengali Sweet Box',
    price: '৳280',
    image: 'https://picsum.photos/seed/wishlist2/300'
  },
  {
    id: 3,
    name: 'Handcrafted Saree',
    price: '৳2,500',
    image: 'https://picsum.photos/seed/wishlist3/300'
  },
  {
    id: 4,
    name: 'Pure Honey Jar',
    price: '৳320',
    image: 'https://picsum.photos/seed/wishlist4/300'
  },
  {
    id: 5,
    name: 'Traditional Pottery',
    price: '৳180',
    image: 'https://picsum.photos/seed/wishlist5/300'
  },
  {
    id: 6,
    name: 'Handmade Basket',
    price: '৳220',
    image: 'https://picsum.photos/seed/wishlist6/300'
  }
];

function loadWishlist() {
  const container = document.getElementById('wishlistContainer');
  
  if (wishlistData.length === 0) {
    container.innerHTML = `
      <div class="empty-state">
        <div class="empty-state-icon">❤️</div>
        <p class="empty-state-text">Your wishlist is empty</p>
      </div>
    `;
    return;
  }
  
  container.innerHTML = wishlistData.map(item => `
    <div class="product-card">
      <button class="product-remove-btn" onclick="removeFromWishlist(${item.id})">❌</button>
      <img src="${item.image}" alt="${item.name}" class="product-image">
      <div class="product-info">
        <h3 class="product-name">${item.name}</h3>
        <p class="product-price">${item.price}</p>
      </div>
    </div>
  `).join('');
}

function removeFromWishlist(id) {
  const index = wishlistData.findIndex(item => item.id === id);
  if (index > -1) {
    wishlistData.splice(index, 1);
    loadWishlist();
    showNotification('Removed from wishlist', 'info');
  }
}

function clearWishlist() {
  if (confirm('Are you sure you want to clear your entire wishlist?')) {
    wishlistData.length = 0;
    loadWishlist();
    showNotification('Wishlist cleared', 'info');
  }
}

// Recently Viewed Data
const recentlyViewedData = [
  {
    id: 1,
    name: 'Fresh Gur',
    price: '৳150',
    image: 'https://picsum.photos/seed/recent1/300'
  },
  {
    id: 2,
    name: 'Tangail er Chomchom',
    price: '৳200',
    image: 'https://picsum.photos/seed/recent2/300'
  },
  {
    id: 3,
    name: 'Mishti Doi',
    price: '৳120',
    image: 'https://picsum.photos/seed/recent3/300'
  },
  {
    id: 4,
    name: 'Rasgulla',
    price: '৳180',
    image: 'https://picsum.photos/seed/recent4/300'
  },
  {
    id: 5,
    name: 'Sandesh',
    price: '৳220',
    image: 'https://picsum.photos/seed/recent5/300'
  },
  {
    id: 6,
    name: 'Gulab Jamun',
    price: '৳200',
    image: 'https://picsum.photos/seed/recent6/300'
  }
];

function loadRecentlyViewed() {
  const container = document.getElementById('recentlyViewedContainer');
  
  if (recentlyViewedData.length === 0) {
    container.innerHTML = `
      <div class="empty-state">
        <div class="empty-state-icon">👁️</div>
        <p class="empty-state-text">No recently viewed products</p>
      </div>
    `;
    return;
  }
  
  container.innerHTML = recentlyViewedData.map(item => `
    <div class="product-card">
      <button class="product-remove-btn" onclick="removeFromHistory(${item.id})">❌</button>
      <img src="${item.image}" alt="${item.name}" class="product-image">
      <div class="product-info">
        <h3 class="product-name">${item.name}</h3>
        <p class="product-price">${item.price}</p>
      </div>
    </div>
  `).join('');
}

function removeFromHistory(id) {
  const index = recentlyViewedData.findIndex(item => item.id === id);
  if (index > -1) {
    recentlyViewedData.splice(index, 1);
    loadRecentlyViewed();
    showNotification('Removed from history', 'info');
  }
}

function clearHistory() {
  if (confirm('Are you sure you want to clear your viewing history?')) {
    recentlyViewedData.length = 0;
    loadRecentlyViewed();
    showNotification('History cleared', 'info');
  }
}

// Logout
function logout() {
  if (confirm('Are you sure you want to logout?')) {
    // Clear user session
    localStorage.removeItem('mr_shop_user');
    
    showNotification('Logging out... 👋', 'info');
    setTimeout(() => {
      window.location.href = 'auth.html';
    }, 1000);
  }
}

// Notification System
function showNotification(message, type = 'success') {
  const notification = document.createElement('div');
  notification.style.cssText = `
    position: fixed;
    top: 20px;
    right: 20px;
    padding: 1rem 1.5rem;
    background: ${type === 'success' ? 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)' : 
                 type === 'info' ? 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)' : 
                 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)'};
    color: white;
    border-radius: 12px;
    box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
    font-weight: 600;
    z-index: 10000;
    animation: slideInRight 0.4s ease, fadeOut 0.4s ease 2.6s;
  `;
  notification.textContent = message;
  document.body.appendChild(notification);
  
  setTimeout(() => {
    notification.remove();
  }, 3000);
}

// Add notification animations
const style = document.createElement('style');
style.textContent = `
  @keyframes slideInRight {
    from {
      transform: translateX(400px);
      opacity: 0;
    }
    to {
      transform: translateX(0);
      opacity: 1;
    }
  }
  
  @keyframes fadeOut {
    from {
      opacity: 1;
    }
    to {
      opacity: 0;
    }
  }
  
  @keyframes bounce {
    0%, 100% { transform: scale(1); }
    50% { transform: scale(1.2); }
  }
`;
document.head.appendChild(style);

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
  checkAuthentication();
  loadProfileData();
  loadProfilePhoto();
  loadOrders();
});

// Check if user is logged in
function checkAuthentication() {
  const userData = localStorage.getItem('mr_shop_user');
  if (!userData) {
    // Not logged in, redirect to auth page
    window.location.href = 'auth.html';
    return;
  }
  
  // Load user data
  const user = JSON.parse(userData);
  if (user.username) {
    document.getElementById('profileName').textContent = user.username;
  }
  if (user.email) {
    document.getElementById('profileEmail').textContent = user.email;
  }
}
