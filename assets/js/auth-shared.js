// ==================== MR SHOP - SHARED AUTH MODULE ====================
// Now connects to C# API with localStorage fallback

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
        body: JSON.stringify({ email, password })
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
      console.log('API offline, using localStorage fallback');
    }

    // Offline fallback - check localStorage for registered users
    const users = JSON.parse(localStorage.getItem('mr_shop_users') || '[]');
    const foundUser = users.find(u => {
      const matchEmail = u.email === email || u.email === email.split('@')[0] + '@mrshop.com';
      const matchPass = u.password === password;
      return matchEmail && matchPass;
    });

    if (foundUser) {
      const userData = {
        id: foundUser.id || Date.now(),
        username: foundUser.username || foundUser.fullName || email.split('@')[0],
        email: foundUser.email,
        role: foundUser.role || 'customer',
        isAdmin: foundUser.role === 'admin',
        loggedIn: true,
        loginTime: new Date().toISOString(),
        profile: foundUser.profile || null
      };
      localStorage.setItem('mr_shop_user', JSON.stringify(userData));
      MR_Cart.showToast('Login successful!', 'success');
      return true;
    }

    // If no registered users exist, create a basic user for demo
    const basicUser = {
      id: Date.now(),
      username: email.split('@')[0],
      email: email,
      role: 'customer',
      isAdmin: false,
      loggedIn: true,
      loginTime: new Date().toISOString(),
      profile: null
    };
    localStorage.setItem('mr_shop_user', JSON.stringify(basicUser));
    MR_Cart.showToast('Login successful!', 'success');
    return true;
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
        body: JSON.stringify({ name: fullName || username, email, password, phone: phone || '' })
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
      console.log('API offline, using localStorage fallback');
    }

    // Offline fallback - save to localStorage
    const users = JSON.parse(localStorage.getItem('mr_shop_users') || '[]');
    const existingUser = users.find(u => u.email === email);
    
    if (existingUser) {
      MR_Cart.showToast('Email already registered!', 'error');
      return false;
    }

    const newUser = {
      id: Date.now(),
      username: username,
      fullName: fullName || username,
      email: email,
      password: password,
      phone: phone || '',
      role: 'customer',
      profile: null
    };
    users.push(newUser);
    localStorage.setItem('mr_shop_users', JSON.stringify(users));

    // Auto login after registration
    const userData = {
      id: newUser.id,
      username: newUser.username,
      email: newUser.email,
      role: 'customer',
      isAdmin: false,
      loggedIn: true,
      loginTime: new Date().toISOString(),
      profile: null
    };
    localStorage.setItem('mr_shop_user', JSON.stringify(userData));
    MR_Cart.showToast('Account created successfully!', 'success');
    return true;
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
    localStorage.removeItem('mr_shop_seller');
    localStorage.removeItem('mr_shop_seller_token');
    window.location.href = 'index.html';
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

        localStorage.setItem('mr_shop_user', JSON.stringify(userData));
        MR_Auth.updateAuthUI();

        const currentPage = window.location.pathname.split('/').pop();
        if (currentPage === 'signin.html' || currentPage === 'signup.html' || currentPage === 'auth.html') {
            window.location.href = 'userprofile.html';
        } else {
            window.location.reload();
        }

    } catch (err) {
        console.error('Google login error:', err);
        alert('Google login failed. Please try again.');
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
