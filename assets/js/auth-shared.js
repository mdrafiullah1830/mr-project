// ==================== MR SHOP - SHARED AUTH MODULE ====================
// Now connects to C# API with localStorage fallback

const MR_Auth = {
  API_BASE: window.location.protocol === 'https:' ? 'https://localhost:5000/api' : 'http://localhost:5000/api',

  getUser() {
    try {
      return JSON.parse(localStorage.getItem('mr_shop_user'));
    } catch {
      return null;
    }
  },

  isLoggedIn() {
    const user = this.getUser();
    return user && user.loggedIn;
  },

  getToken() {
    return localStorage.getItem('mr_shop_token');
  },

  async login(email, password) {
    if (!email || !password) {
      MR_Cart.showToast('Please fill in all fields', 'error');
      return false;
    }

    try {
      const response = await fetch(`${this.API_BASE}/customerauth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username: email.split('@')[0], password })
      });

      if (response.ok) {
        const data = await response.json();
        localStorage.setItem('mr_shop_token', data.token);
        const userData = {
          id: data.user.id,
          username: data.user.username,
          email: data.user.email,
          role: data.user.role,
          isAdmin: data.user.role === 'admin',
          loggedIn: true,
          loginTime: new Date().toISOString(),
          profile: data.user.profile || null
        };
        localStorage.setItem('mr_shop_user', JSON.stringify(userData));
        MR_Cart.showToast('Login successful!', 'success');
        // Sync cart and wishlist from server
        await MR_Cart.syncFromServer();
        await MR_Wishlist.syncFromServer();
        return true;
      } else {
        const err = await response.json();
        MR_Cart.showToast(err.message || 'Login failed', 'error');
        return false;
      }
    } catch (err) {
      console.log('API offline, using localStorage auth');
    }

    // Offline fallback - cannot authenticate without server
    MR_Cart.showToast('Admin login not available offline. Please try again when the server is reachable.', 'error');
    return false;
  },

  async register(username, email, password, fullName, phone) {
    if (!username || !email || !password) {
      MR_Cart.showToast('Please fill in all fields', 'error');
      return false;
    }

    if (password.length < 6) {
      MR_Cart.showToast('Password must be at least 6 characters', 'error');
      return false;
    }

    try {
      const response = await fetch(`${this.API_BASE}/customerauth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, email, password, fullName: fullName || username, phone: phone || '' })
      });

      if (response.ok) {
        const data = await response.json();
        localStorage.setItem('mr_shop_token', data.token);
        const userData = {
          id: data.user.id,
          username: data.user.username,
          email: data.user.email,
          loggedIn: true,
          loginTime: new Date().toISOString(),
          profile: data.user.profile || null
        };
        localStorage.setItem('mr_shop_user', JSON.stringify(userData));
        MR_Cart.showToast('Account created successfully!', 'success');
        return true;
      } else {
        const err = await response.json();
        MR_Cart.showToast(err.message || 'Registration failed', 'error');
        return false;
      }
    } catch (err) {
      console.log('API offline, using localStorage auth');
    }

    // Offline fallback - registration requires the server
    MR_Cart.showToast('Registration requires the server to be online. Please try again later.', 'error');
    return false;
  },

  async getProfile() {
    const result = await MR_API.get('/customerauth/me');
    if (result && result.ok) {
      return result.data;
    }
    return null;
  },

  async updateProfile(profileData) {
    const result = await MR_API.put('/customerauth/profile', profileData);
    if (result && result.ok) {
      MR_Cart.showToast('Profile updated!', 'success');
      return true;
    }
    MR_Cart.showToast('Failed to update profile', 'error');
    return false;
  },

  async changePassword(currentPassword, newPassword) {
    const result = await MR_API.put('/customerauth/password', { currentPassword, newPassword });
    if (result && result.ok) {
      MR_Cart.showToast('Password changed!', 'success');
      return true;
    }
    MR_Cart.showToast(result?.data?.message || 'Failed to change password', 'error');
    return false;
  },

  logout() {
    localStorage.removeItem('mr_shop_user');
    localStorage.removeItem('mr_shop_token');
    localStorage.removeItem('mr_shop_cart');
    localStorage.removeItem('mr_shop_wishlist');
    MR_Cart.showToast('Logged out successfully', 'success');
    setTimeout(() => {
      window.location.href = 'index.html';
    }, 1000);
  },

  getUsers() {
    try {
      return JSON.parse(localStorage.getItem('mr_shop_users')) || [];
    } catch {
      return [];
    }
  },

  updateAuthUI() {
    const user = this.getUser();
    if (!user || !user.loggedIn) return;

    // Update ALL account links on the page
    const allLinks = document.querySelectorAll('a');
    for (const link of allLinks) {
      const line1 = link.querySelector('.line1');
      if (line1 && (line1.textContent.includes('Hello, Sign in') || line1.textContent.includes('Hello,') || line1.textContent.includes('Hello, sign in'))) {
        // Update text
        line1.textContent = `Hello, ${user.username}`;
        const line2 = link.querySelector('.line2');
        if (line2) line2.innerHTML = 'Account & Lists <i class="fas fa-caret-down"></i>';
        // Update href to profile page
        link.setAttribute('href', 'userprofile.html');
      }
    }
  }
};

// Social login functions - require OAuth integration
var googleInitialized = false;

function initGoogleSignIn() {
    if (typeof google === 'undefined' || !google.accounts) {
        setTimeout(initGoogleSignIn, 200);
        return;
    }
    google.accounts.id.initialize({
        client_id: '407138009600-5qc9upb4bec6iss4n1ujhef5g92mbvso.apps.googleusercontent.com',
        callback: handleGoogleResponse,
        auto_select: false,
        cancel_on_tap_outside: true
    });
    googleInitialized = true;
    renderGoogleButtons();
}

function renderGoogleButtons() {
    if (!googleInitialized) return;
    var containers = document.querySelectorAll('.google-signin-btn');
    containers.forEach(function(container) {
        if (container.children.length === 0) {
            google.accounts.id.renderButton(container, {
                type: 'standard',
                theme: 'outline',
                size: 'large',
                width: container.offsetWidth || 300,
                text: 'continue_with',
                shape: 'rectangular'
            });
        }
    });
}

function loginWithGoogle() {
    if (googleInitialized) {
        google.accounts.id.prompt();
    } else {
        MR_Cart.showToast('Google Sign-In is loading. Please try again in a moment.', 'info');
    }
}

async function handleGoogleResponse(response) {
    // Decode Google JWT to get user info (works without backend)
    try {
        const payload = JSON.parse(atob(response.credential.split('.')[1]));
        const userData = {
            id: payload.sub,
            username: payload.name || payload.email.split('@')[0],
            email: payload.email,
            role: 'customer',
            isAdmin: false,
            loggedIn: true,
            loginTime: new Date().toISOString(),
            profile: payload.picture || null
        };

        // Try backend first
        try {
            const res = await fetch(`${MR_Auth.API_BASE}/customerauth/google`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ credential: response.credential }),
                signal: AbortSignal.timeout(5000)
            });

            if (res.ok) {
                const data = await res.json();
                localStorage.setItem('mr_shop_token', data.token);
                userData.id = data.user.id;
                userData.role = data.user.role || 'customer';
                userData.isAdmin = data.user.role === 'admin';
            }
        } catch (apiErr) {
            console.log('Backend offline, using localStorage for Google auth');
        }

        // Save to localStorage (works with or without backend)
        localStorage.setItem('mr_shop_user', JSON.stringify(userData));
        
        // Immediately update UI
        MR_Auth.updateAuthUI();
        
        MR_Cart.showToast('Google login successful!', 'success');
        
        // Redirect based on current page
        setTimeout(() => {
            const currentPage = window.location.pathname.split('/').pop();
            if (currentPage === 'signin.html' || currentPage === 'signup.html' || currentPage === 'auth.html') {
                window.location.href = 'userprofile.html';
            } else {
                // On other pages, reload to update header
                window.location.reload();
            }
        }, 800);

    } catch (err) {
        console.error('Google login error:', err);
        MR_Cart.showToast('Google login failed. Please try again.', 'error');
    }
}

function loginWithFacebook() {
    MR_Cart.showToast('Facebook login coming soon! Use email/password to sign in.', 'info');
}

function loginWithApple() {
    MR_Cart.showToast('Apple login coming soon! Use email/password to sign in.', 'info');
}

function signupWithGoogle() { loginWithGoogle(); }
function signupWithFacebook() { loginWithFacebook(); }
function signupWithApple() { loginWithApple(); }

document.addEventListener('DOMContentLoaded', () => {
  // Check if user is already logged in - redirect from auth pages
  const user = MR_Auth.getUser();
  const currentPage = window.location.pathname.split('/').pop();
  
  if (user && user.loggedIn) {
    // If on auth pages, redirect to profile
    if (currentPage === 'signin.html' || currentPage === 'signup.html' || currentPage === 'auth.html') {
      window.location.href = 'userprofile.html';
      return;
    }
  }
  
  MR_Auth.updateAuthUI();
  initGoogleSignIn();
});
