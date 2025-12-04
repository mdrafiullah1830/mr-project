// auth.js - toggles the animated auth page and reads hash to pick state
(function(){
  const wrapper = document.getElementById('authWrapper');
  if(!wrapper) return;

  // links inside the auth page
  const loginLink = document.querySelector('.login-link');
  const registerLink = document.querySelector('.register-link');

  if(registerLink){ registerLink.addEventListener('click', (e)=>{ e.preventDefault(); wrapper.classList.add('active'); history.replaceState(null,'', '#register'); }); }
  if(loginLink){ loginLink.addEventListener('click', (e)=>{ e.preventDefault(); wrapper.classList.remove('active'); history.replaceState(null,'', '#login'); }); }

  // when page loads, open appropriate panel based on hash
  function applyInitial(){
    const h = location.hash.replace('#','');
    if(h==='register') wrapper.classList.add('active');
    else wrapper.classList.remove('active');
  }

  applyInitial();
  // also react to hash changes
  window.addEventListener('hashchange', applyInitial);

  // Handle Login Form Submission
  const loginForm = document.getElementById('loginForm');
  if(loginForm){
    loginForm.addEventListener('submit', (e)=>{
      e.preventDefault();
      
      const username = document.getElementById('login-username').value;
      const password = document.getElementById('login-password').value;
      
      // Simple validation
      if(username && password){
        // Store user session (in real app, this would be done by backend)
        const userData = {
          username: username,
          email: username.includes('@') ? username : username + '@mrshop.com',
          loggedIn: true,
          loginTime: new Date().toISOString()
        };
        localStorage.setItem('mr_shop_user', JSON.stringify(userData));
        
        // Show success message
        showAuthNotification('Login successful! Redirecting...', 'success');
        
        // Redirect to user profile after short delay
        setTimeout(()=>{
          window.location.href = 'userprofile.html';
        }, 1000);
      } else {
        showAuthNotification('Please fill in all fields', 'error');
      }
    });
  }
  
  // Handle Register Form Submission
  const registerForm = document.getElementById('registerForm');
  if(registerForm){
    registerForm.addEventListener('submit', (e)=>{
      e.preventDefault();
      
      const username = document.getElementById('reg-username').value;
      const email = document.getElementById('reg-email').value;
      const password = document.getElementById('reg-password').value;
      
      // Simple validation
      if(username && email && password){
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
        
        // Store user data (in real app, this would be done by backend)
        const userData = {
          username: username,
          email: email,
          loggedIn: true,
          loginTime: new Date().toISOString()
        };
        localStorage.setItem('mr_shop_user', JSON.stringify(userData));
        
        // Store in profile data as well
        const profileData = {
          fullName: username,
          phoneNumber: '',
          emailAddress: email,
          address: '',
          dob: '',
          gender: 'male'
        };
        localStorage.setItem('mr_shop_user_profile', JSON.stringify(profileData));
        
        // Show success message
        showAuthNotification('Registration successful! Redirecting...', 'success');
        
        // Redirect to user profile after short delay
        setTimeout(()=>{
          window.location.href = 'userprofile.html';
        }, 1000);
      } else {
        showAuthNotification('Please fill in all fields', 'error');
      }
    });
  }

  // Progressive enhancement: keep forms working without JS
  // (no extra interception here; just leave normal POST behavior)
})();

// Social Login Functions
function loginWithGoogle(){
  showAuthNotification('Connecting to Google...', 'info');
  // In production, integrate with Google OAuth API
  // For demo purposes, simulate successful login
  setTimeout(()=>{
    const userData = {
      username: 'Google User',
      email: 'user@gmail.com',
      loggedIn: true,
      loginTime: new Date().toISOString(),
      provider: 'google'
    };
    localStorage.setItem('mr_shop_user', JSON.stringify(userData));
    showAuthNotification('Google login successful! Redirecting...', 'success');
    setTimeout(()=>{ window.location.href = 'userprofile.html'; }, 1000);
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

// Signup social functions (same as login for demo)
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
