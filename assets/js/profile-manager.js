/**
 * User Profile Manager
 * Handles user profile data with persistent JSON storage
 * 
 * API Endpoints:
 * GET /api/profile/<user_id> - Get profile by ID
 * GET /api/profile/email/<email> - Get profile by email
 * GET /api/profile/username/<username> - Get profile by username
 * POST /api/profile - Create/Update profile
 * PUT /api/profile/<user_id> - Update profile fields
 * POST /api/profile/<user_id>/photo - Update profile photo
 * POST /api/profile/<user_id>/wishlist - Add to wishlist
 * POST /api/profile/<user_id>/recently-viewed - Add to recently viewed
 * POST /api/profile/<user_id>/orders - Add order
 * DELETE /api/profile/<user_id> - Delete profile
 */

const PROFILE_API_BASE = 'http://localhost:5002/api';

// ===================== PROFILE OPERATIONS =====================

/**
 * Get user profile from API
 */
async function getProfile(userId) {
  try {
    const response = await fetch(`${PROFILE_API_BASE}/profile/${userId}`);
    const data = await response.json();
    if (data.success) {
      console.log('✅ Profile loaded:', data.profile);
      return data.profile;
    } else {
      console.warn('⚠️ Profile not found:', data.message);
      return null;
    }
  } catch (error) {
    console.error('❌ Error fetching profile:', error);
    return null;
  }
}

/**
 * Get user profile by email
 */
async function getProfileByEmail(email) {
  try {
    const response = await fetch(`${PROFILE_API_BASE}/profile/email/${encodeURIComponent(email)}`);
    const data = await response.json();
    return data.success ? data.profile : null;
  } catch (error) {
    console.error('Error fetching profile by email:', error);
    return null;
  }
}

/**
 * Get user profile by username
 */
async function getProfileByUsername(username) {
  try {
    const response = await fetch(`${PROFILE_API_BASE}/profile/username/${encodeURIComponent(username)}`);
    const data = await response.json();
    return data.success ? data.profile : null;
  } catch (error) {
    console.error('Error fetching profile by username:', error);
    return null;
  }
}

/**
 * Create or update user profile
 */
async function saveProfile(profileData) {
  try {
    const response = await fetch(`${PROFILE_API_BASE}/profile`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(profileData)
    });
    const data = await response.json();
    if (data.success) {
      console.log('✅ Profile saved successfully');
      return data.profile;
    } else {
      console.error('❌ Error saving profile:', data.message);
      return null;
    }
  } catch (error) {
    console.error('Error saving profile:', error);
    return null;
  }
}

/**
 * Update specific profile fields
 */
async function updateProfileFields(userId, fields) {
  try {
    const response = await fetch(`${PROFILE_API_BASE}/profile/${userId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(fields)
    });
    const data = await response.json();
    if (data.success) {
      console.log('✅ Profile updated');
      return data.profile;
    } else {
      console.error('Error updating profile:', data.message);
      return null;
    }
  } catch (error) {
    console.error('Error updating profile:', error);
    return null;
  }
}

/**
 * Update user profile photo
 */
async function updateProfilePhoto(userId, photoUrl, photoPath = null) {
  try {
    const photoData = { profile_photo_url: photoUrl };
    if (photoPath) photoData.profile_photo_path = photoPath;
    
    const response = await fetch(`${PROFILE_API_BASE}/profile/${userId}/photo`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(photoData)
    });
    const data = await response.json();
    if (data.success) {
      console.log('✅ Profile photo updated');
      return data.profile;
    } else {
      console.error('Error updating photo:', data.message);
      return null;
    }
  } catch (error) {
    console.error('Error updating profile photo:', error);
    return null;
  }
}

/**
 * Add item to user's wishlist
 */
async function addToWishlist(userId, item) {
  try {
    const response = await fetch(`${PROFILE_API_BASE}/profile/${userId}/wishlist`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ item })
    });
    const data = await response.json();
    if (data.success) {
      console.log('✅ Item added to wishlist');
      return data.wishlist;
    }
    return null;
  } catch (error) {
    console.error('Error adding to wishlist:', error);
    return null;
  }
}

/**
 * Add item to recently viewed
 */
async function addToRecentlyViewed(userId, item) {
  try {
    const response = await fetch(`${PROFILE_API_BASE}/profile/${userId}/recently-viewed`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ item })
    });
    const data = await response.json();
    if (data.success) {
      console.log('✅ Added to recently viewed');
      return data.recently_viewed;
    }
    return null;
  } catch (error) {
    console.error('Error adding to recently viewed:', error);
    return null;
  }
}

/**
 * Add order to user profile
 */
async function addOrder(userId, order) {
  try {
    const response = await fetch(`${PROFILE_API_BASE}/profile/${userId}/orders`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ order })
    });
    const data = await response.json();
    if (data.success) {
      console.log('✅ Order added');
      return data.orders;
    }
    return null;
  } catch (error) {
    console.error('Error adding order:', error);
    return null;
  }
}

/**
 * Delete user profile
 */
async function deleteProfile(userId) {
  try {
    const response = await fetch(`${PROFILE_API_BASE}/profile/${userId}`, {
      method: 'DELETE'
    });
    const data = await response.json();
    if (data.success) {
      console.log('✅ Profile deleted');
      return true;
    }
    return false;
  } catch (error) {
    console.error('Error deleting profile:', error);
    return false;
  }
}

// ===================== LOCAL STORAGE HELPERS =====================

/**
 * Get current user from localStorage
 */
function getCurrentUser() {
  try {
    const userStr = localStorage.getItem('mr_shop_user');
    return userStr ? JSON.parse(userStr) : null;
  } catch (error) {
    console.error('Error getting current user:', error);
    return null;
  }
}

/**
 * Save current user to localStorage
 */
function setCurrentUser(user) {
  try {
    localStorage.setItem('mr_shop_user', JSON.stringify(user));
    console.log('✅ User saved to localStorage');
    return true;
  } catch (error) {
    console.error('Error saving user:', error);
    return false;
  }
}

/**
 * Load user profile after sign in
 */
async function loadUserProfileAfterSignIn(userId, email) {
  try {
    // First try to get profile by userId
    let profile = await getProfile(userId);
    
    // If not found by ID, try by email
    if (!profile) {
      profile = await getProfileByEmail(email);
    }
    
    // If still not found, create new profile with basic data
    if (!profile) {
      const newProfile = {
        user_id: userId,
        email_address: email,
        full_name: '',
        phone_number: '',
        address: '',
        profile_photo_url: `https://i.pravatar.cc/200?img=${Math.floor(Math.random() * 70)}`,
        bio: '',
        gender: '',
        date_of_birth: null,
        orders: [],
        wishlist: [],
        recently_viewed: [],
        reviews: []
      };
      profile = await saveProfile(newProfile);
    }
    
    return profile;
  } catch (error) {
    console.error('Error loading profile after sign in:', error);
    return null;
  }
}

/**
 * Store user profile in sessionStorage for current session
 */
function storeProfileSession(profile) {
  try {
    sessionStorage.setItem('mr_shop_profile', JSON.stringify(profile));
    console.log('✅ Profile stored in session');
    return true;
  } catch (error) {
    console.error('Error storing profile session:', error);
    return false;
  }
}

/**
 * Get user profile from sessionStorage
 */
function getProfileSession() {
  try {
    const profileStr = sessionStorage.getItem('mr_shop_profile');
    return profileStr ? JSON.parse(profileStr) : null;
  } catch (error) {
    console.error('Error getting profile session:', error);
    return null;
  }
}

/**
 * Clear user profile from sessionStorage
 */
function clearProfileSession() {
  try {
    sessionStorage.removeItem('mr_shop_profile');
    console.log('✅ Profile session cleared');
    return true;
  } catch (error) {
    console.error('Error clearing profile session:', error);
    return false;
  }
}

// ===================== INITIALIZATION =====================

/**
 * Check if user is signed in and load profile
 */
async function initializeUserProfile() {
  const user = getCurrentUser();
  
  if (user) {
    console.log('👤 User found in localStorage:', user);
    
    // Load full profile from API
    const profile = await loadUserProfileAfterSignIn(user.id || user.user_id, user.email);
    
    if (profile) {
      storeProfileSession(profile);
      return profile;
    }
  }
  
  return null;
}

// Auto-initialize on page load
document.addEventListener('DOMContentLoaded', () => {
  initializeUserProfile().then(profile => {
    if (profile) {
      console.log('✅ User profile initialized:', profile);
    }
  });
});

// Export functions for use in other files
if (typeof module !== 'undefined' && module.exports) {
  module.exports = {
    getProfile,
    getProfileByEmail,
    getProfileByUsername,
    saveProfile,
    updateProfileFields,
    updateProfilePhoto,
    addToWishlist,
    addToRecentlyViewed,
    addOrder,
    deleteProfile,
    getCurrentUser,
    setCurrentUser,
    loadUserProfileAfterSignIn,
    storeProfileSession,
    getProfileSession,
    clearProfileSession,
    initializeUserProfile
  };
}
