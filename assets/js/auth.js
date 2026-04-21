// auth.js - toggles the animated auth page and reads hash to pick state
(function(){
  const wrapper = document.getElementById('authWrapper');
  if(!wrapper) return;

  // links inside the auth page
  const loginLinks = document.querySelectorAll('.login-link');
  const registerLink = document.querySelector('.register-link');
  const forgotLinks = document.querySelectorAll('.forgot-link');

  if(registerLink){ 
    registerLink.addEventListener('click', (e)=>{ 
      e.preventDefault(); 
      wrapper.classList.remove('forgot', 'reset');
      wrapper.classList.add('active'); 
      history.replaceState(null,'', '#register'); 
    }); 
  }
  
  loginLinks.forEach(link => {
    if(link){ 
      link.addEventListener('click', (e)=>{ 
        e.preventDefault(); 
        wrapper.classList.remove('active', 'forgot', 'reset'); 
        history.replaceState(null,'', '#login'); 
      }); 
    }
  });

  forgotLinks.forEach(link => {
    if(link){ 
      link.addEventListener('click', (e)=>{ 
        e.preventDefault(); 
        wrapper.classList.remove('active', 'reset');
        wrapper.classList.add('forgot'); 
        history.replaceState(null,'', '#forgot-password'); 
      }); 
    }
  });

  // when page loads, open appropriate panel based on hash
  function applyInitial(){
    const h = location.hash.replace('#','');
    wrapper.classList.remove('active', 'forgot', 'reset');
    if(h==='register') wrapper.classList.add('active');
    else if(h==='forgot-password') wrapper.classList.add('forgot');
    else if(h==='reset-password') wrapper.classList.add('reset');
  }

  applyInitial();
  // also react to hash changes
  window.addEventListener('hashchange', applyInitial);

  // Handle Login Form Submission
  const loginForm = document.getElementById('loginForm');
  if(loginForm){
    loginForm.addEventListener('submit', async (e)=>{
      e.preventDefault();
      
      const username = document.getElementById('login-username').value;
      const password = document.getElementById('login-password').value;
      
      // Simple validation
      if(!username || !password){
        showAuthNotification('Please fill in all fields', 'error');
        return;
      }

      try {
        const normalizedUsername = username.trim();
        const normalizedLookup = normalizedUsername.toLowerCase();
        const isAdmin = normalizedLookup === 'mrshop';
        const storedUsers = JSON.parse(localStorage.getItem('mr_shop_users')) || [];
        const sellerAccounts = JSON.parse(localStorage.getItem('mrshop_seller_accounts')) || [];

        const matchedSeller = sellerAccounts.find(account => {
          const accountUsername = String(account.username || '').toLowerCase();
          const accountEmail = String(account.email || '').toLowerCase();
          return (accountUsername === normalizedLookup || accountEmail === normalizedLookup) && String(account.password || '') === password;
        });

        const matchedUser = storedUsers.find(account => {
          const accountUsername = String(account.username || '').toLowerCase();
          const accountEmail = String(account.email || '').toLowerCase();
          return (accountUsername === normalizedLookup || accountEmail === normalizedLookup) && String(account.password || '') === password;
        });

        let userData;
        if (matchedSeller) {
          userData = {
            id: matchedSeller.requestId ? `seller_${matchedSeller.requestId}` : 'seller_' + Date.now(),
            username: matchedSeller.username,
            email: matchedSeller.email || (matchedSeller.username + '@mrshop.com'),
            role: 'seller',
            loggedIn: true,
            loginTime: new Date().toISOString(),
            sellerRequestId: matchedSeller.requestId || null,
            approvedAt: matchedSeller.generatedAt || new Date().toISOString(),
            source: 'admin-generated'
          };
        } else if (matchedUser) {
          userData = {
            id: matchedUser.id || ('user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9)),
            username: matchedUser.username || normalizedUsername,
            email: matchedUser.email || (normalizedUsername.includes('@') ? normalizedUsername : normalizedUsername + '@mrshop.com'),
            role: matchedUser.role || (isAdmin ? 'admin' : 'user'),
            loggedIn: true,
            loginTime: new Date().toISOString()
          };
        } else if (isAdmin) {
          userData = {
            id: 'user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9),
            username: normalizedUsername,
            email: normalizedUsername.includes('@') ? normalizedUsername : normalizedUsername + '@mrshop.com',
            role: 'admin',
            loggedIn: true,
            loginTime: new Date().toISOString()
          };
        } else {
          // Offline mode fallback for legacy demo flows.
          userData = {
            id: 'user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9),
            username: normalizedUsername,
            email: normalizedUsername.includes('@') ? normalizedUsername : normalizedUsername + '@mrshop.com',
            role: 'user',
            loggedIn: true,
            loginTime: new Date().toISOString()
          };
        }

        localStorage.setItem('mr_shop_user', JSON.stringify(userData));

        // Keep a local list of known accounts so approved seller credentials can be reused.
        let users = JSON.parse(localStorage.getItem('mr_shop_users')) || [];
        const existingUserIndex = users.findIndex(account => {
          const accountUsername = String(account.username || '').toLowerCase();
          const accountEmail = String(account.email || '').toLowerCase();
          return accountUsername === userData.username.toLowerCase() || accountEmail === userData.email.toLowerCase();
        });

        const accountRecord = {
          id: userData.id,
          username: userData.username,
          email: userData.email,
          password: password,
          role: userData.role || 'user',
          lastLogin: new Date().toISOString(),
          loginTime: new Date().toISOString()
        };

        if (userData.sellerRequestId) {
          accountRecord.sellerRequestId = userData.sellerRequestId;
          accountRecord.approvedAt = userData.approvedAt || new Date().toISOString();
        }

        if (existingUserIndex >= 0) {
          users[existingUserIndex] = {
            ...users[existingUserIndex],
            ...accountRecord
          };
        } else {
          users.push(accountRecord);
        }

        localStorage.setItem('mr_shop_users', JSON.stringify(users));
        
        // Also save profile data for offline access
        const profileData = {
          user_id: userData.id,
          username: userData.username,
          role: userData.role || 'user',
          email_address: userData.email,
          full_name: userData.username,
          phone_number: '',
          address: '',
          date_of_birth: '',
          gender: 'male',
          profile_photo_path: null,
          bio: '',
          orders: [],
          wishlist: [],
          recently_viewed: [],
          reviews: []
        };
        localStorage.setItem('mr_shop_user_profile', JSON.stringify(profileData));
        
        // Save profile to API (if available)
        try {
          await saveProfile(profileData);
        } catch (error) {
          console.warn('Profile API unavailable, using localStorage', error);
        }
        
        // If admin, also set adminInfo in localStorage
        if (isAdmin) {
          const adminInfo = {
            username: username,
            email: userData.email,
            isAdmin: true,
            loginTime: new Date().toISOString()
          };
          localStorage.setItem('adminInfo', JSON.stringify(adminInfo));
        }
        
        // Show success message
        const redirectTarget = userData.role === 'seller' ? 'seller.html' : 'userprofile.html';
        const successMessage = userData.role === 'seller'
          ? '✅ Seller credentials verified! Redirecting to seller dashboard...'
          : isAdmin
            ? '👑 Welcome Admin!'
            : 'Login successful! Redirecting...';

        showAuthNotification(successMessage, 'success');
        
        // Redirect to user profile after short delay
        setTimeout(()=>{
          window.location.href = redirectTarget;
        }, 1000);
      } catch(error) {
        console.error('Login error:', error);
        showAuthNotification('An error occurred. Please try again.', 'error');
      }
    });
  }
  
  // Handle Register Form Submission
  const registerForm = document.getElementById('registerForm');
  if(registerForm){
    registerForm.addEventListener('submit', async (e)=>{
      e.preventDefault();
      
      const username = document.getElementById('reg-username').value;
      const email = document.getElementById('reg-email').value;
      const password = document.getElementById('reg-password').value;
      
      // Simple validation
      if(!username || !email || !password){
        showAuthNotification('Please fill in all fields', 'error');
        return;
      }
        
      // Validate email format
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if(!emailRegex.test(email)){
        showAuthNotification('Please enter a valid email address', 'error');
        return;
      }
      
      // Validate password length
      if(password.length < 6){
        showAuthNotification('Password must be at least 6 characters', 'error');
        return;
      }

      try {
        // Store user data in localStorage (offline mode - no backend required)
        const userData = {
          id: 'user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9),
          username: username,
          email: email,
          role: 'user',
          loggedIn: true,
          loginTime: new Date().toISOString()
        };
        localStorage.setItem('mr_shop_user', JSON.stringify(userData));
        
        // Store in profile data as well
        const profileData = {
          user_id: userData.id,
          username: username,
          role: 'user',
          email_address: email,
          full_name: username,
          phone_number: '',
          address: '',
          date_of_birth: '',
          gender: 'male',
          profile_photo_path: null,
          bio: '',
          orders: [],
          wishlist: [],
          recently_viewed: [],
          reviews: []
        };
        localStorage.setItem('mr_shop_user_profile', JSON.stringify(profileData));
        
        // Save profile to API (if available)
        try {
          await saveProfile(profileData);
        } catch (error) {
          console.warn('Profile API unavailable, using localStorage', error);
        }
        
        // Show success message
        showAuthNotification('Registration successful! Redirecting...', 'success');
        
        // Redirect to user profile after short delay
        setTimeout(()=>{
          window.location.href = 'userprofile.html';
        }, 1000);
      } catch(error) {
        console.error('Registration error:', error);
        showAuthNotification('An error occurred. Please try again.', 'error');
      }
    });
  }

  // Handle Forgot Password Form Submission
  const forgotPasswordForm = document.getElementById('forgotPasswordForm');
  if(forgotPasswordForm){
    forgotPasswordForm.addEventListener('submit', async (e)=>{
      e.preventDefault();
      
      const email = document.getElementById('forgot-email').value;
      
      // Simple validation
      if(!email){
        showAuthNotification('Please enter your email address', 'error');
        return;
      }

      // Validate email format
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if(!emailRegex.test(email)){
        showAuthNotification('Please enter a valid email address', 'error');
        return;
      }

      try {
        // Offline mode - simulate password reset by storing in localStorage
        showAuthNotification('Password reset instructions would be sent! Moving to reset form...', 'success');
        
        // Redirect to reset password form after short delay
        setTimeout(()=>{
          // Store email for reset form
          sessionStorage.setItem('reset_email', email);
          wrapper.classList.remove('forgot');
          wrapper.classList.add('reset');
          history.replaceState(null,'', '#reset-password');
          // Pre-fill email in reset form
          document.getElementById('reset-email').value = email;
        }, 1500);
      } catch(error) {
        console.error('Forgot password error:', error);
        showAuthNotification('An error occurred. Please try again.', 'error');
      }
    });
  }

  // Handle Reset Password Form Submission
  const resetPasswordForm = document.getElementById('resetPasswordForm');
  if(resetPasswordForm){
    resetPasswordForm.addEventListener('submit', async (e)=>{
      e.preventDefault();
      
      const email = document.getElementById('reset-email').value;
      const newPassword = document.getElementById('reset-new-password').value;
      const confirmPassword = document.getElementById('reset-confirm-password').value;
      
      // Simple validation
      if(!email || !newPassword || !confirmPassword){
        showAuthNotification('Please fill in all fields', 'error');
        return;
      }

      // Validate email format
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if(!emailRegex.test(email)){
        showAuthNotification('Please enter a valid email address', 'error');
        return;
      }
      
      // Validate password length
      if(newPassword.length < 6){
        showAuthNotification('Password must be at least 6 characters', 'error');
        return;
      }

      // Check if passwords match
      if(newPassword !== confirmPassword){
        showAuthNotification('Passwords do not match', 'error');
        return;
      }

      try {
        // Offline mode - update password in localStorage for the user
        // Get all stored users from localStorage
        let users = JSON.parse(localStorage.getItem('mr_shop_users')) || [];
        
        // Find and update the user with the matching email
        const userIndex = users.findIndex(u => u.email === email);
        
        if (userIndex >= 0) {
          users[userIndex].password = newPassword;
          localStorage.setItem('mr_shop_users', JSON.stringify(users));
        } else {
          // If no user found, create a new one
          users.push({
            id: 'user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9),
            email: email,
            password: newPassword,
            username: email.split('@')[0],
            loginTime: new Date().toISOString()
          });
          localStorage.setItem('mr_shop_users', JSON.stringify(users));
        }
        
        showAuthNotification('Password reset successful!', 'success');
        
        // Clear session storage
        sessionStorage.removeItem('reset_email');
        
        // Redirect to login after short delay
        setTimeout(()=>{
          wrapper.classList.remove('reset');
          history.replaceState(null,'', '#login');
        }, 1500);
      } catch(error) {
        console.error('Reset password error:', error);
        showAuthNotification('An error occurred. Please try again.', 'error');
      }
    });
  }

  // Progressive enhancement: keep forms working without JS
  // (no extra interception here; just leave normal POST behavior)
})();

// Social Login Functions
function loginWithGoogle(){
  if (typeof GoogleOAuth !== 'undefined') {
    GoogleOAuth.login();
  } else {
    console.error('GoogleOAuth manager not loaded');
    simulateGoogleLogin();
  }
}

// Initiate Google OAuth Flow
function initiateGoogleOAuth() {
  // Configuration
  // Replace with your actual Google Client ID from Google Cloud Console
  // Format: XXXXX-YYYYY.apps.googleusercontent.com
  const CLIENT_ID = 'YOUR_CLIENT_ID_HERE'; // TODO: Add your actual Client ID
  const PROJECT_ID = 'mr-shop-480319';
  const REDIRECT_URI = window.location.origin + '/assets/html/auth.html';
  const SCOPE = 'email profile';
  const RESPONSE_TYPE = 'token';
  const STATE = generateRandomState();

  // Save state for CSRF protection
  sessionStorage.setItem('oauth_state', STATE);

  // Check if CLIENT_ID is configured
  if (CLIENT_ID.includes('YOUR_CLIENT_ID') || CLIENT_ID.includes('YOUR_GOOGLE')) {
    console.warn('⚠️ Google Client ID not configured. Using demo mode.');
    console.log('📝 To setup real OAuth:');
    console.log('1. Go to: https://console.cloud.google.com');
    console.log('2. Select project: ' + PROJECT_ID);
    console.log('3. Create OAuth 2.0 Client ID');
    console.log('4. Update CLIENT_ID in /assets/js/auth.js (line ~337)');
    simulateGoogleLogin();
    return;
  }

  // Build OAuth URL for real Google OAuth
  const authURL = new URL('https://accounts.google.com/o/oauth2/v2/auth');
  authURL.searchParams.append('client_id', CLIENT_ID);
  authURL.searchParams.append('redirect_uri', REDIRECT_URI);
  authURL.searchParams.append('response_type', RESPONSE_TYPE);
  authURL.searchParams.append('scope', SCOPE);
  authURL.searchParams.append('state', STATE);
  authURL.searchParams.append('access_type', 'online');
  authURL.searchParams.append('prompt', 'consent');

  // Redirect to Google OAuth
  window.location.href = authURL.toString();
}

// Generate random state for CSRF protection
function generateRandomState() {
  return Math.random().toString(36).substring(2, 15) + 
         Math.random().toString(36).substring(2, 15);
}

// Handle Google OAuth Callback
function handleGoogleCallback() {
  // This would be called after OAuth redirect
  const hash = window.location.hash.substring(1);
  if (!hash) return;

  const params = new URLSearchParams(hash);
  const accessToken = params.get('access_token');
  const state = params.get('state');
  const savedState = sessionStorage.getItem('oauth_state');

  // Verify state for CSRF protection
  if (state !== savedState) {
    showAuthNotification('Security validation failed. Please try again.', 'error');
    return;
  }

  if (!accessToken) {
    showAuthNotification('Authorization failed. Please try again.', 'error');
    return;
  }

  // Get user info with access token
  fetchGoogleUserInfo(accessToken);
}

// Fetch user info from Google
async function fetchGoogleUserInfo(accessToken) {
  try {
    const response = await fetch('https://www.googleapis.com/oauth2/v2/userinfo?access_token=' + accessToken);
    const googleUser = await response.json();

    if (googleUser.error) {
      throw new Error(googleUser.error_description);
    }

    // Create user data from Google response
    const userData = {
      id: 'user_' + googleUser.id,
      username: googleUser.name || googleUser.email.split('@')[0],
      email: googleUser.email,
      firstName: googleUser.given_name,
      lastName: googleUser.family_name,
      avatar: googleUser.picture,
      loggedIn: true,
      loginTime: new Date().toISOString(),
      provider: 'google',
      googleId: googleUser.id,
      accessToken: accessToken
    };

    localStorage.setItem('mr_shop_user', JSON.stringify(userData));
    localStorage.setItem('google_access_token', accessToken);

    // Clear sensitive data from session storage
    sessionStorage.removeItem('oauth_state');

    showAuthNotification('Google login successful! Redirecting...', 'success');
    setTimeout(() => { 
      window.location.href = 'userprofile.html'; 
    }, 1000);
  } catch (error) {
    console.error('Google OAuth error:', error);
    showAuthNotification('Failed to get user info: ' + error.message, 'error');
  }
}

// Simulate Google login for demo purposes (when Google API is not available)
function simulateGoogleLogin() {
  setTimeout(() => {
    // Create realistic demo data
    const demoGoogleUsers = [
      { name: 'Ahmed Khan', email: 'ahmed.khan@gmail.com', picture: 'https://via.placeholder.com/200?text=Ahmed' },
      { name: 'Fatima Ali', email: 'fatima.ali@gmail.com', picture: 'https://via.placeholder.com/200?text=Fatima' },
      { name: 'Muhammad Hassan', email: 'hassan.m@gmail.com', picture: 'https://via.placeholder.com/200?text=Hassan' }
    ];
    
    const randomUser = demoGoogleUsers[Math.floor(Math.random() * demoGoogleUsers.length)];
    
    const userData = {
      username: randomUser.name,
      email: randomUser.email,
      avatar: randomUser.picture,
      loggedIn: true,
      loginTime: new Date().toISOString(),
      provider: 'google',
      isDemo: true,
      demoNote: 'This is a demo login. Configure Google OAuth for real authentication.'
    };
    localStorage.setItem('mr_shop_user', JSON.stringify(userData));
    showAuthNotification('✓ Demo Google login successful! Redirecting...', 'success');
    setTimeout(() => { window.location.href = 'userprofile.html'; }, 1500);
  }, 1500);
}

function loginWithFacebook(){
  showAuthNotification('Connecting to Facebook...', 'info');
  // In production, integrate with Facebook Login API
  setTimeout(()=>{
    const userData = {
      username: 'Facebook User',
      email: 'user@facebook.com',
      loggedIn: true,
      loginTime: new Date().toISOString(),
      provider: 'facebook'
    };
    localStorage.setItem('mr_shop_user', JSON.stringify(userData));
    showAuthNotification('Facebook login successful! Redirecting...', 'success');
    setTimeout(()=>{ window.location.href = 'userprofile.html'; }, 1000);
  }, 1500);
}

function loginWithApple(){
  showAuthNotification('Connecting to Apple...', 'info');
  // In production, integrate with Sign in with Apple
  setTimeout(()=>{
    const userData = {
      username: 'Apple User',
      email: 'user@icloud.com',
      loggedIn: true,
      loginTime: new Date().toISOString(),
      provider: 'apple'
    };
    localStorage.setItem('mr_shop_user', JSON.stringify(userData));
    showAuthNotification('Apple login successful! Redirecting...', 'success');
    setTimeout(()=>{ window.location.href = 'userprofile.html'; }, 1000);
  }, 1500);
}

// Signup social functions (same as login for Google)
function signupWithGoogle(){ loginWithGoogle(); }
function signupWithFacebook(){ loginWithFacebook(); }
function signupWithApple(){ loginWithApple(); }

// Notification System
function showAuthNotification(message, type = 'success'){
  // Remove any existing notifications
  const existing = document.querySelector('.auth-notification');
  if(existing) existing.remove();
  
  const notification = document.createElement('div');
  notification.className = 'auth-notification';
  notification.style.cssText = `
    position: fixed;
    top: 20px;
    right: 20px;
    padding: 16px 24px;
    background: ${type === 'success' ? 'linear-gradient(135deg, #10b981, #059669)' : 
                 type === 'info' ? 'linear-gradient(135deg, #3b82f6, #2563eb)' : 
                 'linear-gradient(135deg, #ef4444, #dc2626)'};
    color: white;
    border-radius: 12px;
    box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
    font-weight: 600;
    z-index: 10000;
    animation: slideInRight 0.4s ease;
    max-width: 90vw;
    font-size: 14px;
  `;
  notification.textContent = message;
  document.body.appendChild(notification);
  
  setTimeout(()=>{
    notification.style.animation = 'fadeOut 0.4s ease';
    setTimeout(()=>{ notification.remove(); }, 400);
  }, 3000);
}

// Add animation keyframes
if(!document.getElementById('auth-animations')){
  const style = document.createElement('style');
  style.id = 'auth-animations';
  style.textContent = `
    @keyframes slideInRight {
      from { transform: translateX(400px); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
    @keyframes fadeOut {
      from { opacity: 1; }
      to { opacity: 0; transform: translateX(400px); }
    }
  `;
  document.head.appendChild(style);
}
