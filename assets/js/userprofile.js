// Authentication Check - Ensure user is logged in
function checkUserAuthentication() {
  const userData = localStorage.getItem('mr_shop_user');
  if (!userData) {
    // User not logged in, redirect to auth
    alert('Please log in first to access your profile');
    window.location.href = 'auth.html#login';
    return false;
  }
  return true;
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
  } else if (sectionId === 'seller-approvals') {
    loadAdminNotifications();
  }
}

// ===== ADMIN: SELLER APPROVALS FUNCTIONALITY =====

// Load admin notifications for seller requests
async function loadAdminNotifications() {
  try {
    const response = await fetch('http://localhost:5010/api/sellerrequest/admin/notifications');
    const result = await response.json();
    
    if (!result.success) {
      document.getElementById('approvalsContainer').innerHTML = `
        <div style="padding: 20px; background: #fff3cd; border-radius: 8px; color: #856404;">
          <p>⚠️ ${result.error || 'Failed to load notifications'}</p>
        </div>
      `;
      return;
    }
    
    const notifications = result.data || [];
    const container = document.getElementById('approvalsContainer');
    
    // Update count
    document.getElementById('pendingCount').textContent = notifications.length;
    if (notifications.length > 0) {
      document.getElementById('approvalBadge').textContent = notifications.length;
      document.getElementById('approvalBadge').style.display = 'inline-block';
    } else {
      document.getElementById('approvalBadge').style.display = 'none';
    }
    
    if (notifications.length === 0) {
      container.innerHTML = `
        <div style="padding: 40px 20px; text-align: center; background: #f8f9fa; border-radius: 8px;">
          <p style="font-size: 48px; margin-bottom: 10px;">✅</p>
          <p style="color: #666; font-size: 16px;">No pending seller requests</p>
          <p style="color: #999; font-size: 12px;">All seller applications have been reviewed!</p>
        </div>
      `;
      return;
    }
    
    // Render each notification as a card
    container.innerHTML = notifications.map(notif => `
      <div style="
        background: white;
        border: 2px solid ${notif.status === 'Unread' ? '#ff6b6b' : '#e2e8f0'};
        border-radius: 12px;
        padding: 20px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 20px;
      ">
        <div style="flex: 1;">
          <div style="display: flex; align-items: center; gap: 10px; margin-bottom: 10px;">
            <h4 style="margin: 0; font-size: 16px; font-weight: 600; color: #1e293b;">
              ${escapeHtml(notif.fullName)}
            </h4>
            ${notif.status === 'Unread' ? '<span style="background: #ff6b6b; color: white; padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 600;">NEW</span>' : ''}
          </div>
          
          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-bottom: 15px;">
            <div>
              <span style="color: #999; font-size: 12px;">📧 Email</span>
              <p style="margin: 5px 0 0 0; color: #1e293b; font-size: 14px;">${escapeHtml(notif.email)}</p>
            </div>
            <div>
              <span style="color: #999; font-size: 12px;">📞 Phone</span>
              <p style="margin: 5px 0 0 0; color: #1e293b; font-size: 14px;">${escapeHtml(notif.phone)}</p>
            </div>
            <div>
              <span style="color: #999; font-size: 12px;">🏪 Business Name</span>
              <p style="margin: 5px 0 0 0; color: #1e293b; font-size: 14px;">${escapeHtml(notif.shopName)}</p>
            </div>
            <div>
              <span style="color: #999; font-size: 12px;">📅 Submitted</span>
              <p style="margin: 5px 0 0 0; color: #1e293b; font-size: 14px;">${new Date(notif.submittedAt).toLocaleString()}</p>
            </div>
          </div>
        </div>
        
        <div style="display: flex; flex-direction: column; gap: 8px; min-width: 140px;">
          <button onclick="approveSellerRequest('${notif.requestId}')" style="
            background: linear-gradient(135deg, #28a745, #20c997);
            color: white;
            border: none;
            padding: 10px 16px;
            border-radius: 8px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.2s ease;
          " onmouseover="this.style.transform='translateY(-2px)'; this.style.boxShadow='0 4px 12px rgba(40,167,69,0.3)'" onmouseout="this.style.transform='translateY(0)'; this.style.boxShadow=''">
            ✅ Approve
          </button>
          <button onclick="rejectSellerRequest('${notif.requestId}')" style="
            background: linear-gradient(135deg, #dc3545, #e83e8c);
            color: white;
            border: none;
            padding: 10px 16px;
            border-radius: 8px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.2s ease;
          " onmouseover="this.style.transform='translateY(-2px)'; this.style.boxShadow='0 4px 12px rgba(220,53,69,0.3)'" onmouseout="this.style.transform='translateY(0)'; this.style.boxShadow=''">
            ❌ Reject
          </button>
          <button onclick="markAsRead('${notif.requestId}')" style="
            background: #6c757d;
            color: white;
            border: none;
            padding: 10px 16px;
            border-radius: 8px;
            font-weight: 600;
            font-size: 12px;
            cursor: pointer;
            transition: all 0.2s ease;
          " onmouseover="this.style.transform='translateY(-2px)'" onmouseout="this.style.transform='translateY(0)'">
            👁️ Mark Read
          </button>
        </div>
      </div>
    `).join('');
    
  } catch (error) {
    console.error('Error loading admin notifications:', error);
    document.getElementById('approvalsContainer').innerHTML = `
      <div style="padding: 20px; background: #f8d7da; border-radius: 8px; color: #721c24;">
        <p>❌ Error loading notifications: ${error.message}</p>
      </div>
    `;
  }
}

// Approve seller request
async function approveSellerRequest(requestId) {
  if (!confirm('Are you sure you want to approve this seller?')) return;
  
  try {
    const response = await fetch(`http://localhost:5010/api/sellerrequest/admin/${requestId}/status`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        status: 'Approved',
        notes: 'Approved by admin on ' + new Date().toLocaleString()
      })
    });
    
    const result = await response.json();
    
    if (result.success) {
      showNotification('✅ Seller approved successfully!', 'success');
      loadAdminNotifications(); // Reload the list
    } else {
      showNotification('❌ Error approving seller: ' + (result.error || 'Unknown error'), 'error');
    }
  } catch (error) {
    console.error('Error approving seller:', error);
    showNotification('❌ Error: ' + error.message, 'error');
  }
}

// Reject seller request
async function rejectSellerRequest(requestId) {
  const reason = prompt('Enter rejection reason (optional):');
  if (reason === null) return; // User cancelled
  
  try {
    const response = await fetch(`http://localhost:5010/api/sellerrequest/admin/${requestId}/status`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        status: 'Rejected',
        notes: reason || 'Rejected by admin on ' + new Date().toLocaleString()
      })
    });
    
    const result = await response.json();
    
    if (result.success) {
      showNotification('❌ Seller request rejected', 'warning');
      loadAdminNotifications(); // Reload the list
    } else {
      showNotification('❌ Error rejecting seller: ' + (result.error || 'Unknown error'), 'error');
    }
  } catch (error) {
    console.error('Error rejecting seller:', error);
    showNotification('❌ Error: ' + error.message, 'error');
  }
}

// Mark notification as read
async function markAsRead(requestId) {
  try {
    const response = await fetch(`http://localhost:5010/api/sellerrequest/admin/acknowledge/${requestId}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    });
    
    const result = await response.json();
    
    if (result.success) {
      loadAdminNotifications(); // Reload the list
    } else {
      console.warn('Error marking as read:', result.error);
    }
  } catch (error) {
    console.error('Error marking as read:', error);
  }
}

// Helper function to escape HTML special characters
function escapeHtml(text) {
  if (!text) return '';
  const map = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#039;'
  };
  return String(text).replace(/[&<>"']/g, m => map[m]);
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

infoForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  
  // Get current user ID
  const userData = localStorage.getItem('mr_shop_user');
  if (!userData) {
    showNotification('Please log in first', 'error');
    return;
  }
  
  const user = JSON.parse(userData);
  const userId = user.id || user.username;
  
  // Get form data
  const formData = {
    full_name: document.getElementById('fullName').value,
    phone_number: document.getElementById('phoneNumber').value,
    email_address: document.getElementById('emailAddress').value,
    address: document.getElementById('address').value,
    date_of_birth: document.getElementById('dob').value,
    gender: document.getElementById('gender').value
  };
  
  try {
    // Save to backend API
    const response = await fetch(`http://localhost:5010/api/profile/${userId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(formData)
    });

    const result = await response.json();

    if (result.success) {
      // Also save to localStorage for offline access
      localStorage.setItem('mr_shop_user_profile', JSON.stringify(formData));
      
      // Update profile display
      document.getElementById('profileName').textContent = formData.full_name.split(' ')[0] + ' ' + (formData.full_name.split(' ')[1] || '');
      document.getElementById('profileEmail').textContent = formData.email_address;
      
      // Show success message
      showNotification('Profile updated successfully! ✅', 'success');
      
      // Exit edit mode
      toggleEdit();
    } else {
      showNotification('Failed to update profile: ' + result.message, 'error');
    }
  } catch (error) {
    console.error('Error updating profile:', error);
    // Still save to localStorage even if backend fails
    localStorage.setItem('mr_shop_user_profile', JSON.stringify(formData));
    
    // Update profile display
    document.getElementById('profileName').textContent = formData.full_name.split(' ')[0] + ' ' + (formData.full_name.split(' ')[1] || '');
    document.getElementById('profileEmail').textContent = formData.email_address;
    
    // Show message about offline mode
    showNotification('Profile saved locally. (Offline Mode)', 'info');
    
    // Exit edit mode
    toggleEdit();
  }
});

// Load saved profile data from backend
async function loadProfileData() {
  // Get current user ID
  const userData = localStorage.getItem('mr_shop_user');
  if (!userData) return;
  
  const user = JSON.parse(userData);
  const userId = user.id || user.username;
  
  // Update status indicator
  updateBackendStatus('connecting', '🔄 Connecting...');
  
  try {
    // Check if profile exists in backend
    const checkResponse = await fetch(`http://localhost:5010/api/profile/${userId}/exists`);
    const checkResult = await checkResponse.json();
    
    if (!checkResult.data.exists) {
      // Create new profile with default data
      updateBackendStatus('creating', '✨ Creating profile...');
      
      const newProfile = {
        user_id: userId,
        full_name: user.username || 'User',
        email_address: user.email || '',
        phone_number: '',
        address: '',
        date_of_birth: '',
        gender: 'male'
      };
      
      const createResponse = await fetch(`http://localhost:5010/api/profile`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newProfile)
      });
      
      const createResult = await createResponse.json();
      if (createResult.success) {
        console.log('✅ Profile created successfully');
        showNotification('Welcome! Your profile has been created. 🎉', 'success');
      }
    }
    
    // Fetch profile from backend
    const response = await fetch(`http://localhost:5010/api/profile/${userId}`);
    const result = await response.json();
    
    if (result.success && result.data) {
      const data = result.data;
      
      // Auto-fill inputs with backend data
      if (data.full_name) document.getElementById('fullName').value = data.full_name;
      if (data.phone_number) document.getElementById('phoneNumber').value = data.phone_number;
      if (data.email_address) document.getElementById('emailAddress').value = data.email_address;
      if (data.address) document.getElementById('address').value = data.address;
      if (data.date_of_birth) document.getElementById('dob').value = data.date_of_birth;
      if (data.gender) document.getElementById('gender').value = data.gender;
      
      // Update sidebar display
      if (data.full_name) {
        document.getElementById('profileName').textContent = data.full_name;
      }
      if (data.email_address) {
        document.getElementById('profileEmail').textContent = data.email_address;
      }
      
      // Load profile photo
      if (data.profile_photo_path) {
        const photoUrl = `http://localhost:5010${data.profile_photo_path}`;
        console.log('🖼️ Loading profile photo from:', photoUrl);
        document.getElementById('profilePhoto').src = photoUrl;
      }
      
      // Also save to localStorage for backup
      localStorage.setItem('mr_shop_user_profile', JSON.stringify(data));
      
      // Update status to connected
      updateBackendStatus('connected', '✅ Connected to Backend');
    } else {
      // If profile doesn't exist in backend, try loading from localStorage
      updateBackendStatus('offline', '⚠️ Offline Mode');
      
      const savedData = localStorage.getItem('mr_shop_user_profile');
      if (savedData) {
        const data = JSON.parse(savedData);
        document.getElementById('fullName').value = data.fullName || data.full_name || '';
        document.getElementById('phoneNumber').value = data.phoneNumber || data.phone_number || '';
        document.getElementById('emailAddress').value = data.emailAddress || data.email_address || '';
        document.getElementById('address').value = data.address || '';
        document.getElementById('dob').value = data.dob || data.date_of_birth || '';
        document.getElementById('gender').value = data.gender || 'male';
      }
    }
  } catch (error) {
    console.error('Error loading profile:', error);
    updateBackendStatus('offline', '⚠️ Working Offline');
    
    // Fallback to localStorage - use data from login
    const savedData = localStorage.getItem('mr_shop_user_profile');
    if (savedData) {
      const data = JSON.parse(savedData);
      document.getElementById('fullName').value = data.fullName || data.full_name || '';
      document.getElementById('phoneNumber').value = data.phoneNumber || data.phone_number || '';
      document.getElementById('emailAddress').value = data.emailAddress || data.email_address || '';
      document.getElementById('address').value = data.address || '';
      document.getElementById('dob').value = data.dob || data.date_of_birth || '';
      document.getElementById('gender').value = data.gender || 'male';
    }
  }
}

// Update backend status indicator
function updateBackendStatus(status, text) {
  const statusDot = document.getElementById('statusDot');
  const statusText = document.getElementById('statusText');
  
  if (!statusDot || !statusText) return;
  
  const colors = {
    connecting: '#ffc107',
    creating: '#17a2b8',
    connected: '#28a745',
    offline: '#ff9800',
    error: '#dc3545'
  };
  
  statusDot.style.background = colors[status] || '#6c757d';
  statusText.textContent = text;
}

// Change Profile Photo
function changeProfilePhoto() {
  const input = document.createElement('input');
  input.type = 'file';
  input.accept = 'image/*';
  input.style.display = 'none';
  
  input.onchange = async (e) => {
    const file = e.target.files[0];
    if (!file) return;
    
    console.log('📸 Photo selected:', file.name, 'Size:', file.size, 'Type:', file.type);
    
    // Get current user ID
    const userData = localStorage.getItem('mr_shop_user');
    if (!userData) {
      showNotification('Please log in first', 'error');
      console.error('❌ No user data found in localStorage');
      return;
    }
    
    const user = JSON.parse(userData);
    const userId = user.id || user.username;
    console.log('👤 User ID:', userId);
    
    // Validate file size (5MB max)
    if (file.size > 5 * 1024 * 1024) {
      showNotification('File size exceeds 5MB limit', 'error');
      console.error('❌ File too large:', file.size);
      return;
    }
    
    // Show preview immediately
    const reader = new FileReader();
    reader.onload = (event) => {
      document.getElementById('profilePhoto').src = event.target.result;
      console.log('✅ Preview loaded');
    };
    reader.readAsDataURL(file);
    
    try {
      console.log('🔍 Checking if profile exists...');
      
      // FIRST: Ensure profile exists before uploading photo
      const checkResponse = await fetch(`http://localhost:5010/api/profile/${userId}/exists`);
      const checkResult = await checkResponse.json();
      
      console.log('Profile exists:', checkResult.data.exists);
      
      if (!checkResult.data.exists) {
        console.log('✨ Creating profile first...');
        
        // Create profile first
        const newProfile = {
          user_id: userId,
          full_name: user.username || 'User',
          email_address: user.email || '',
          phone_number: '',
          address: '',
          date_of_birth: '',
          gender: 'male'
        };
        
        const createResponse = await fetch(`http://localhost:5010/api/profile`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(newProfile)
        });
        
        const createResult = await createResponse.json();
        console.log('Profile creation result:', createResult);
        
        if (!createResult.success) {
          showNotification('Failed to create profile: ' + createResult.message, 'error');
          console.error('❌ Profile creation failed:', createResult.message);
          return;
        }
        
        console.log('✅ Profile created successfully');
      }
      
      console.log('📤 Uploading photo to backend...');
      
      // NOW: Upload photo
      const formData = new FormData();
      formData.append('file', file);
      
      const response = await fetch(`http://localhost:5010/api/profile/${userId}/photo`, {
        method: 'POST',
        body: formData
      });
      
      console.log('Upload response status:', response.status);
      
      const result = await response.json();
      console.log('Upload result:', result);
      
      if (result.success) {
        // Save the server path
        localStorage.setItem('mr_shop_profile_photo_path', result.data.photo_path);
        showNotification('Profile photo updated! 📸', 'success');
        console.log('✅ Photo uploaded successfully:', result.data.photo_path);
        
        // Reload the photo from server to verify
        const photoUrl = `http://localhost:5010${result.data.photo_path}`;
        console.log('🖼️ Loading photo from:', photoUrl);
        document.getElementById('profilePhoto').src = photoUrl;
      } else {
        showNotification('Failed to upload photo: ' + result.message, 'error');
        console.error('❌ Upload failed:', result.message);
      }
    } catch (error) {
      console.error('❌ Error uploading photo:', error);
      showNotification('Error uploading photo. Please try again.', 'error');
    }
    
    // Clean up - remove the input from DOM
    document.body.removeChild(input);
  };
  
  // IMPORTANT: Add to DOM before clicking - fixes browser compatibility issues
  document.body.appendChild(input);
  input.click();
}

// Load saved profile photo from backend
async function loadProfilePhoto() {
  // Get current user ID
  const userData = localStorage.getItem('mr_shop_user');
  if (!userData) return;
  
  const user = JSON.parse(userData);
  const userId = user.id || user.username;
  
  try {
    // Fetch profile from backend to get photo path
    const response = await fetch(`http://localhost:5010/api/profile/${userId}`);
    const result = await response.json();
    
    if (result.success && result.data && result.data.profile_photo_path) {
      // Load image from backend server
      const photoUrl = `http://localhost:5010${result.data.profile_photo_path}`;
      document.getElementById('profilePhoto').src = photoUrl;
      localStorage.setItem('mr_shop_profile_photo_path', result.data.profile_photo_path);
    } else {
      // Fallback to localStorage base64 if exists
      const savedPhoto = localStorage.getItem('mr_shop_profile_photo');
      if (savedPhoto) {
        document.getElementById('profilePhoto').src = savedPhoto;
      }
    }
  } catch (error) {
    console.error('Error loading profile photo:', error);
    // Fallback to localStorage
    const savedPhoto = localStorage.getItem('mr_shop_profile_photo');
    if (savedPhoto) {
      document.getElementById('profilePhoto').src = savedPhoto;
    }
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

function filterOrders(filter) {
  currentFilter = filter;
  
  // Update active filter button
  document.querySelectorAll('.filter-btn').forEach(btn => {
    btn.classList.remove('active');
  });
  event.target.classList.add('active');
  
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
    // Clear all session data
    localStorage.removeItem('mr_shop_user');
    localStorage.removeItem('mr_shop_user_profile');
    sessionStorage.removeItem('reset_email');
    
    showNotification('Logging out... 👋', 'info');
    setTimeout(() => {
      // Redirect to home page instead of auth page
      window.location.href = 'index.html';
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
  initializeAdminFeatures();
  animateHeroStats();
});

// Animate hero stat counters
function animateHeroStats(){
  const counters = document.querySelectorAll('.stat-value[data-count]');
  counters.forEach(el => {
    const target = Number(el.getAttribute('data-count')) || 0;
    let start = 0;
    const duration = 900;
    const stepTime = Math.max(15, Math.floor(duration / Math.max(1, target)));
    const timer = setInterval(() => {
      start += 1;
      if (el.id === 'statSpent') {
        el.textContent = '৳' + start;
      } else {
        el.textContent = start;
      }
      if (start >= target) clearInterval(timer);
    }, stepTime);
  });
  // Sync hero name and mini info from sidebar
  const name = document.getElementById('profileName')?.textContent || 'User';
  const email = document.getElementById('profileEmail')?.textContent || '';
  document.getElementById('heroName').textContent = name.split(' ')[0] || name;
  document.getElementById('miniName').textContent = name;
  document.getElementById('miniEmail').textContent = email;
  // If profile photo already loaded, copy to hero
  const photo = document.getElementById('profilePhoto')?.src;
  if (photo) document.getElementById('heroPhoto').src = photo;
}

// Initialize admin features (seller approvals)
function initializeAdminFeatures() {
  const userData = localStorage.getItem('mr_shop_user');
  if (!userData) return;
  
  const user = JSON.parse(userData);
  const isAdmin = user.username === 'mrshop'; // Admin check
  
  // Show seller approvals menu only for admin
  const sellerApprovalsMenu = document.getElementById('sellerApprovalsMenu');
  if (isAdmin && sellerApprovalsMenu) {
    sellerApprovalsMenu.style.display = 'block';
    
    // Auto-load notifications every 30 seconds if admin
    setInterval(() => {
      const currentSection = document.querySelector('.menu-item.active');
      if (currentSection && currentSection.dataset.section === 'seller-approvals') {
        loadAdminNotifications();
      }
    }, 30000);
  }
}

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
