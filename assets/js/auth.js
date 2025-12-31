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
        // Allow anyone to login - accept any username/password combination
        // Store user session with provided credentials
        
        // Check if username is "mrshop" - make them admin
        const isAdmin = username.toLowerCase() === 'mrshop';
        
        const userData = {
          id: 'user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9),
          username: username,
          email: username.includes('@') ? username : username + '@mrshop.com',
          role: isAdmin ? 'admin' : 'user',
          loggedIn: true,
          loginTime: new Date().toISOString()
        };
        localStorage.setItem('mr_shop_user', JSON.stringify(userData));
        
        // Also save profile data for offline access
        const profileData = {
          full_name: username,
          email_address: userData.email,
          phone_number: '',
          address: '',
          date_of_birth: '',
          gender: 'male'
        };
        localStorage.setItem('mr_shop_user_profile', JSON.stringify(profileData));
        
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
        showAuthNotification(isAdmin ? '👑 Welcome Admin!' : 'Login successful! Redirecting...', 'success');
        
        // Redirect to user profile after short delay
        setTimeout(()=>{
          window.location.href = 'userprofile.html';
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
        // Call C# API for sign up
        const response = await fetch('http://localhost:5010/api/auth/signup', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            username: username,
            email: email,
            password: password
          })
        });

        const result = await response.json();

        if(result.success){
          // Store user data
          const userData = {
            id: result.data.id,
            username: result.data.username,
            email: result.data.email,
            loggedIn: true,
            loginTime: result.data.created_at
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
          showAuthNotification(result.message || 'Registration successful! Redirecting...', 'success');
          
          // Redirect to user profile after short delay
          setTimeout(()=>{
            window.location.href = 'userprofile.html';
          }, 1000);
        } else {
          showAuthNotification(result.message || 'Registration failed', 'error');
        }
      } catch(error) {
        console.error('Registration error:', error);
        showAuthNotification('Connection error. Please try again.', 'error');
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
        // Call C# API for forgot password
        const response = await fetch('http://localhost:5010/api/auth/forgot-password', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ email: email })
        });

        const result = await response.json();

        if(result.success){
          showAuthNotification(result.message || 'Password reset instructions sent!', 'success');
          
          // Redirect to reset password form after short delay
          setTimeout(()=>{
            // Store email for reset form
            sessionStorage.setItem('reset_email', email);
            wrapper.classList.remove('forgot');
            wrapper.classList.add('reset');
            history.replaceState(null,'', '#reset-password');
            // Pre-fill email in reset form
            document.getElementById('reset-email').value = email;
          }, 2000);
        } else {
          showAuthNotification(result.message || 'An error occurred', 'error');
        }
      } catch(error) {
        console.error('Forgot password error:', error);
        showAuthNotification('Connection error. Please try again.', 'error');
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
        // Call C# API for reset password
        const response = await fetch('http://localhost:5010/api/auth/reset-password', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            email: email,
            new_password: newPassword,
            confirm_password: confirmPassword
          })
        });

        const result = await response.json();

        if(result.success){
          showAuthNotification(result.message || 'Password reset successful!', 'success');
          
          // Clear session storage
          sessionStorage.removeItem('reset_email');
          
          // Redirect to login after short delay
          setTimeout(()=>{
            wrapper.classList.remove('reset');
            history.replaceState(null,'', '#login');
          }, 2000);
        } else {
          showAuthNotification(result.message || 'Password reset failed', 'error');
        }
      } catch(error) {
        console.error('Reset password error:', error);
        showAuthNotification('Connection error. Please try again.', 'error');
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
