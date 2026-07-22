// ==================== MR SHOP - SHARED AUTH MODULE ====================
// API-only auth - no localStorage fallback for data

const MR_Auth = {
  API_BASE: window.location.hostname === 'localhost' ? 'http://localhost:5000/api' : 'https://mrshop-bd.azurewebsites.net/api',

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
        body: JSON.stringify({ email: email.trim(), password })
      });

      if (response.ok) {
        const data = await response.json();
        localStorage.setItem('mr_shop_token', data.token);
        const userData = {
          id: data.user.id,
          username: data.user.name,
          email: data.user.email,
          role: data.user.role,
          isAdmin: data.user.role === 'admin',
          loggedIn: true,
          loginTime: new Date().toISOString()
        };
        localStorage.setItem('mr_shop_user', JSON.stringify(userData));
        MR_Cart.showToast('Login successful!', 'success');
        await MR_Cart.syncFromServer();
        await MR_Wishlist.syncFromServer();
        return true;
      } else {
        const err = await response.json();
        MR_Cart.showToast(err.message || 'Login failed', 'error');
        return false;
      }
    } catch (err) {
      MR_Cart.showToast('Cannot connect to server. Please try again.', 'error');
      return false;
    }
  },

  async register(username, email, password, fullName, phone) {
    if (!username || !email || !password) {
      MR_Cart.showToast('Please fill in all fields', 'error');
      return false;
    }

    if (password.length < 8) {
      MR_Cart.showToast('Password must be at least 8 characters', 'error');
      return false;
    }

    try {
      const response = await fetch(`${this.API_BASE}/customerauth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: fullName || username,
          email: email.trim(),
          password,
          phone: phone || ''
        })
      });

      if (response.ok) {
        const data = await response.json();
        localStorage.setItem('mr_shop_token', data.token);
        const userData = {
          id: data.user.id,
          username: data.user.name,
          email: data.user.email,
          role: data.user.role || 'customer',
          isAdmin: data.user.role === 'admin',
          loggedIn: true,
          loginTime: new Date().toISOString()
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
      MR_Cart.showToast('Cannot connect to server. Please try again.', 'error');
      return false;
    }
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

  updateAuthUI() {
    const user = this.getUser();
    if (!user || !user.loggedIn) return;

    const selectors = ['#accountLink', '.amz-header-link'];
    for (const sel of selectors) {
      const links = document.querySelectorAll(sel);
      for (const link of links) {
        const line1 = link.querySelector('.line1');
        if (line1 && (line1.textContent.includes('Hello, Sign in') || line1.textContent.includes('Hello,'))) {
          line1.textContent = `Hello, ${user.username}`;
          const line2 = link.querySelector('.line2');
          if (line2) line2.innerHTML = 'Account & Lists <i class="fas fa-caret-down"></i>';
          return;
        }
      }
    }
  }
};

// Google OAuth
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
        MR_Cart.showToast('Google Sign-In is loading. Please try again.', 'info');
    }
}

async function handleGoogleResponse(response) {
    console.log('[GoogleAuth] Credential received from Google:', !!response.credential);
    try {
        console.log('[GoogleAuth] Calling backend:', MR_Auth.API_BASE + '/customerauth/google');
        const res = await fetch(`${MR_Auth.API_BASE}/customerauth/google`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ credential: response.credential })
        });

        console.log('[GoogleAuth] Backend response status:', res.status);

        if (res.ok) {
            const data = await res.json();
            console.log('[GoogleAuth] Backend response:', { hasToken: !!data.token, hasUser: !!data.user, role: data.user?.role });
            localStorage.setItem('mr_shop_token', data.token);
            const userData = {
                id: data.user.id,
                username: data.user.name,
                email: data.user.email,
                role: data.user.role || 'customer',
                isAdmin: data.user.role === 'admin',
                loggedIn: true,
                loginTime: new Date().toISOString()
            };
            localStorage.setItem('mr_shop_user', JSON.stringify(userData));
            MR_Cart.showToast('Google login successful!', 'success');

            // Redirect based on server-provided role
            const role = data.user.role || 'customer';
            let redirectUrl = 'userprofile.html';
            if (role === 'admin') {
                redirectUrl = 'admin.html';
            } else if (role === 'seller') {
                redirectUrl = 'seller.html';
            }
            console.log('[GoogleAuth] Redirecting to:', redirectUrl);
            setTimeout(() => window.location.href = redirectUrl, 800);
        } else {
            const err = await res.json().catch(() => ({}));
            console.error('[GoogleAuth] Backend error:', res.status, err);
            MR_Cart.showToast(err.message || 'Google login failed', 'error');
        }
    } catch (err) {
        console.error('[GoogleAuth] Login error:', err);
        MR_Cart.showToast('Cannot connect to server. Google login failed.', 'error');
    }
}

function loginWithFacebook() {
    if (typeof FB === 'undefined') {
        MR_Cart.showToast('Facebook SDK is loading. Please try again.', 'info');
        return;
    }

    FB.login(function(response) {
        if (response.authResponse) {
            handleFacebookResponse(response.authResponse);
        } else {
            MR_Cart.showToast('Facebook login cancelled.', 'info');
        }
    }, { scope: 'email,public_profile' });
}

async function handleFacebookResponse(authResponse) {
    try {
        const res = await fetch(`${MR_Auth.API_BASE}/customerauth/facebook`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                accessToken: authResponse.accessToken,
                userID: authResponse.userID
            })
        });

        if (res.ok) {
            const data = await res.json();
            localStorage.setItem('mr_shop_token', data.token);
            const userData = {
                id: data.user.id,
                username: data.user.name,
                email: data.user.email,
                role: data.user.role || 'customer',
                isAdmin: data.user.role === 'admin',
                loggedIn: true,
                loginTime: new Date().toISOString()
            };
            localStorage.setItem('mr_shop_user', JSON.stringify(userData));
            MR_Cart.showToast('Facebook login successful!', 'success');

            // Redirect based on server-provided role
            const role = data.user.role || 'customer';
            let redirectUrl = 'userprofile.html';
            if (role === 'admin') {
                redirectUrl = 'admin.html';
            } else if (role === 'seller') {
                redirectUrl = 'seller.html';
            }
            setTimeout(() => window.location.href = redirectUrl, 800);
        } else {
            const err = await res.json();
            MR_Cart.showToast(err.message || 'Facebook login failed', 'error');
        }
    } catch (err) {
        MR_Cart.showToast('Cannot connect to server. Facebook login failed.', 'error');
    }
}

function loginWithApple() {
    MR_Cart.showToast('Apple login coming soon!', 'info');
}

function signupWithGoogle() { loginWithGoogle(); }
function signupWithFacebook() { loginWithFacebook(); }
function signupWithApple() { loginWithApple(); }

document.addEventListener('DOMContentLoaded', () => {
  MR_Auth.updateAuthUI();
  initGoogleSignIn();
});
