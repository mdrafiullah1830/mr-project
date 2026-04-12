// Complete Google OAuth Implementation for MR Shop
// This includes demo mode + real OAuth support

class GoogleOAuthManager {
  constructor() {
    // Configuration
    this.CLIENT_ID = '1049849814562-uj8d0he569bht0m9tv0rl62erj5q519p.apps.googleusercontent.com'; // Updated with real Client ID
    this.PROJECT_ID = 'mr-shop-480319';
    this.IS_DEMO = this.CLIENT_ID.includes('YOUR_ACTUAL');
    
    // Demo users for testing
    this.DEMO_USERS = [
      {
        id: 'demo_user_1',
        name: 'Ahmed Khan',
        email: 'ahmed.khan@gmail.com',
        picture: 'https://ui-avatars.com/api/?name=Ahmed+Khan&background=667eea&color=fff',
        verified_email: true,
        locale: 'en',
        given_name: 'Ahmed',
        family_name: 'Khan'
      },
      {
        id: 'demo_user_2',
        name: 'Fatima Ali',
        email: 'fatima.ali@gmail.com',
        picture: 'https://ui-avatars.com/api/?name=Fatima+Ali&background=764ba2&color=fff',
        verified_email: true,
        locale: 'en',
        given_name: 'Fatima',
        family_name: 'Ali'
      },
      {
        id: 'demo_user_3',
        name: 'Hassan Muhammad',
        email: 'hassan.m@gmail.com',
        picture: 'https://ui-avatars.com/api/?name=Hassan+Muhammad&background=f093fb&color=fff',
        verified_email: true,
        locale: 'en',
        given_name: 'Hassan',
        family_name: 'Muhammad'
      }
    ];
  }

  /**
   * Initialize Google OAuth
   */
  init() {
    console.log('🔐 Google OAuth Manager Initialized');
    console.log('Mode:', this.IS_DEMO ? '📱 DEMO' : '✅ PRODUCTION');
    console.log('Project:', this.PROJECT_ID);
  }

  /**
   * Initiate login with Google
   */
  async login() {
    console.log('🔐 Google Login Initiated');
    
    if (this.IS_DEMO) {
      console.log('📱 Using demo mode (no real Google API)');
      this.simulateDemoLogin();
    } else {
      console.log('✅ Using real Google OAuth');
      this.initiateRealOAuth();
    }
  }

  /**
   * Simulate demo login
   */
  async simulateDemoLogin() {
    try {
      // Show loading
      const notification = document.querySelector('.auth-notification');
      if (notification) {
        notification.textContent = '⏳ Connecting to demo account...';
        notification.className = 'auth-notification info';
        notification.style.display = 'block';
      }

      // Simulate network delay
      await new Promise(resolve => setTimeout(resolve, 1500));

      // Pick random demo user
      const user = this.DEMO_USERS[Math.floor(Math.random() * this.DEMO_USERS.length)];

      // Create user data
      const userData = {
        id: user.id,
        name: user.name,
        email: user.email,
        picture: user.picture,
        given_name: user.given_name,
        family_name: user.family_name,
        verified_email: true,
        locale: user.locale,
        loginTime: new Date().toISOString(),
        provider: 'google_demo',
        isDemoUser: true
      };

      // Save to localStorage
      localStorage.setItem('mr_shop_user', JSON.stringify(userData));
      localStorage.setItem('user_login_time', new Date().toISOString());

      // Show success
      if (notification) {
        notification.textContent = `✅ Welcome ${user.given_name}! Redirecting...`;
        notification.className = 'auth-notification success';
      }

      // Redirect after 1 second
      setTimeout(() => {
        window.location.href = '/assets/html/userprofile.html';
      }, 1000);

    } catch (error) {
      console.error('Demo login error:', error);
      this.showError('Login failed. Please try again.');
    }
  }

  /**
   * Initiate real Google OAuth flow
   */
  async initiateRealOAuth() {
    try {
      const REDIRECT_URI = window.location.origin + '/assets/html/auth.html';
      const SCOPE = 'openid email profile';
      const RESPONSE_TYPE = 'token';
      const STATE = this.generateState();

      // Save state for CSRF protection
      sessionStorage.setItem('oauth_state', STATE);

      // Build OAuth URL
      const authURL = new URL('https://accounts.google.com/o/oauth2/v2/auth');
      authURL.searchParams.append('client_id', this.CLIENT_ID);
      authURL.searchParams.append('redirect_uri', REDIRECT_URI);
      authURL.searchParams.append('response_type', RESPONSE_TYPE);
      authURL.searchParams.append('scope', SCOPE);
      authURL.searchParams.append('state', STATE);
      authURL.searchParams.append('access_type', 'online');
      authURL.searchParams.append('prompt', 'consent');

      // Redirect to Google
      window.location.href = authURL.toString();

    } catch (error) {
      console.error('OAuth error:', error);
      this.showError('Failed to initiate Google OAuth. Please try again.');
    }
  }

  /**
   * Handle OAuth callback
   */
  async handleCallback() {
    try {
      const hash = window.location.hash.substring(1);
      if (!hash) return;

      const params = new URLSearchParams(hash);
      const accessToken = params.get('access_token');
      const state = params.get('state');

      // Verify state
      const savedState = sessionStorage.getItem('oauth_state');
      if (state !== savedState) {
        throw new Error('CSRF validation failed');
      }

      if (!accessToken) {
        throw new Error('No access token received');
      }

      // Fetch user info
      await this.fetchUserInfo(accessToken);

    } catch (error) {
      console.error('Callback error:', error);
      this.showError('Login failed: ' + error.message);
    }
  }

  /**
   * Fetch user info from Google
   */
  async fetchUserInfo(accessToken) {
    try {
      const response = await fetch(
        'https://www.googleapis.com/oauth2/v2/userinfo?access_token=' + accessToken
      );

      if (!response.ok) {
        throw new Error('Failed to get user info');
      }

      const userData = await response.json();

      // Save user data
      localStorage.setItem('mr_shop_user', JSON.stringify({
        id: userData.id,
        name: userData.name,
        email: userData.email,
        picture: userData.picture,
        verified_email: userData.verified_email,
        locale: userData.locale,
        loginTime: new Date().toISOString(),
        provider: 'google',
        accessToken: accessToken
      }));

      // Redirect to profile
      this.showSuccess('Login successful! Redirecting...');
      setTimeout(() => {
        window.location.href = '/assets/html/userprofile.html';
      }, 1000);

    } catch (error) {
      console.error('User info error:', error);
      this.showError('Failed to get user info: ' + error.message);
    }
  }

  /**
   * Generate random state for CSRF protection
   */
  generateState() {
    return Math.random().toString(36).substring(2, 15) + 
           Math.random().toString(36).substring(2, 15);
  }

  /**
   * Show error notification
   */
  showError(message) {
    const notification = document.querySelector('.auth-notification');
    if (notification) {
      notification.textContent = '❌ ' + message;
      notification.className = 'auth-notification error';
      notification.style.display = 'block';
    } else {
      alert('Error: ' + message);
    }
  }

  /**
   * Show success notification
   */
  showSuccess(message) {
    const notification = document.querySelector('.auth-notification');
    if (notification) {
      notification.textContent = '✅ ' + message;
      notification.className = 'auth-notification success';
      notification.style.display = 'block';
    }
  }

  /**
   * Logout user
   */
  logout() {
    localStorage.removeItem('mr_shop_user');
    sessionStorage.removeItem('oauth_state');
    window.location.href = '/assets/html/auth.html#login';
  }

  /**
   * Get current user
   */
  getCurrentUser() {
    const user = localStorage.getItem('mr_shop_user');
    return user ? JSON.parse(user) : null;
  }

  /**
   * Check if user is logged in
   */
  isLoggedIn() {
    return this.getCurrentUser() !== null;
  }
}

// Create global instance
window.GoogleOAuth = new GoogleOAuthManager();

// Initialize on page load
window.addEventListener('load', () => {
  GoogleOAuth.init();
});
