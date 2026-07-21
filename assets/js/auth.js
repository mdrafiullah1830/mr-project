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
    loginForm.addEventListener('submit', async (e)=>{
      e.preventDefault();
      
      const username = document.getElementById('login-username').value;
      const password = document.getElementById('login-password').value;
      
      if(username && password){
        // Check seller credentials first
        const sellers = JSON.parse(localStorage.getItem('mr_shop_sellers')) || [];
        const seller = sellers.find(s => {
          const isApproved = s.status === 'approved' || s.isApproved === true;
          const hasCreds = s.credentials && s.credentials.username && s.credentials.password;
          const matchUser = (s.credentials && s.credentials.username === username) || s.email === username;
          const matchPass = s.credentials && s.credentials.password === password;
          return isApproved && hasCreds && matchUser && matchPass;
        });
        if(seller){
          localStorage.setItem('mr_shop_seller', JSON.stringify(seller));
          localStorage.setItem('mr_shop_seller_token', 'seller_session_' + Date.now());
          alert('Seller login successful!');
          window.location.href = 'seller.html';
          return;
        }

        // Use shared auth module
        const email = username.includes('@') ? username : username + '@mrshop.com';
        const success = await MR_Auth.login(email, password);
        if(success){
          const user = MR_Auth.getUser();
          const ADMIN_EMAIL = 'mrshop.bd.18@gmail.com';
          if(user && user.email === ADMIN_EMAIL && (user.isAdmin || user.role === 'admin')){
            setTimeout(()=>{ window.location.href = 'admin.html'; }, 500);
          } else {
            setTimeout(()=>{ window.location.href = 'userprofile.html'; }, 500);
          }
        }
      } else {
        showAuthNotification('Please fill in all fields', 'error');
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
      
      if(username && email && password){
        // Validate email format
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if(!emailRegex.test(email)){
          showAuthNotification('Please enter a valid email address', 'error');
          return;
        }
        
        // Use shared auth module
        const success = await MR_Auth.register(username, email, password);
        if(success){
          const user = MR_Auth.getUser();
          const ADMIN_EMAIL = 'mrshop.bd.18@gmail.com';
          if(user && user.email === ADMIN_EMAIL && (user.isAdmin || user.role === 'admin')){
            setTimeout(()=>{ window.location.href = 'admin.html'; }, 500);
          } else {
            setTimeout(()=>{ window.location.href = 'userprofile.html'; }, 500);
          }
        }
      } else {
        showAuthNotification('Please fill in all fields', 'error');
      }
    });
  }

  // Progressive enhancement: keep forms working without JS
  // (no extra interception here; just leave normal POST behavior)
})();

// Social Login Functions - require OAuth integration
function loginWithGoogle(){
  showAuthNotification('Google login coming soon! Use email/password to sign in.', 'info');
}

function loginWithFacebook(){
  showAuthNotification('Facebook login coming soon! Use email/password to sign in.', 'info');
}

function loginWithApple(){
  showAuthNotification('Apple login coming soon! Use email/password to sign in.', 'info');
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
