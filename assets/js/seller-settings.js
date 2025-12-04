// Seller settings demo: persist to localStorage and wire simple interactions
document.addEventListener('DOMContentLoaded', ()=>{
  const form = document.getElementById('settingsForm');
  const saveMsg = document.getElementById('saveMsg');

  // inputs map
  const inputs = {
    shopName: document.getElementById('shopName'),
    shopCategory: document.getElementById('shopCategory'),
    shopDesc: document.getElementById('shopDesc'),
    shopHours: document.getElementById('shopHours'),
    sellerName: document.getElementById('sellerName'),
    sellerEmail: document.getElementById('sellerEmail'),
    sellerPhone: document.getElementById('sellerPhone'),
    sellerAddress: document.getElementById('sellerAddress'),
    bankName: document.getElementById('bankName'),
    bankAccount: document.getElementById('bankAccount'),
    mobileBank: document.getElementById('mobileBank'),
    payoutSchedule: document.getElementById('payoutSchedule'),
    txnPin: document.getElementById('txnPin'),
    notifyOrders: document.getElementById('notifyOrders'),
    notifyPayments: document.getElementById('notifyPayments'),
    notifyStock: document.getElementById('notifyStock'),
    notifyEmail: document.getElementById('notifyEmail'),
    notifySms: document.getElementById('notifySms'),
    defaultPickup: document.getElementById('defaultPickup'),
    pkgSize: document.getElementById('pkgSize'),
    deliveryPartner: document.getElementById('deliveryPartner'),
    dashboardTheme: document.getElementById('dashboardTheme'),
    language: document.getElementById('language'),
    enable2fa: document.getElementById('enable2fa')
  };

  // load settings from localStorage if available
  const KEY = 'mr_seller_settings_v1';
  function load(){
    const raw = localStorage.getItem(KEY); if(!raw) return;
    try{ const obj = JSON.parse(raw); Object.keys(inputs).forEach(k=>{ const el = inputs[k]; if(!el || obj[k] === undefined) return; if(el.type === 'checkbox') el.checked = !!obj[k]; else el.value = obj[k]; });
    }catch(e){ console.warn('Failed to parse settings', e); }
  }

  function save(obj){ localStorage.setItem(KEY, JSON.stringify(obj)); }

  form.addEventListener('submit', (e)=>{
    e.preventDefault();
    // basic validation for email
    if(inputs.sellerEmail.value && !inputs.sellerEmail.value.includes('@')){ alert('Please enter a valid email'); return; }
    if(inputs.newPassword && inputs.newPassword.value){ /* password change would go to backend in real app */ }

    const out = {};
    Object.keys(inputs).forEach(k=>{ const el = inputs[k]; if(!el) return; out[k] = el.type === 'checkbox' ? el.checked : el.value; });
    save(out);
    saveMsg.textContent = 'Saved locally (demo)';
    setTimeout(()=> saveMsg.textContent = '', 2500);
  });

  // file previews (logo & profile)
  const shopLogo = document.getElementById('shopLogo');
  const shopLogoPreview = document.getElementById('shopLogoPreview');
  if(shopLogo) shopLogo.addEventListener('change', (e)=>{
    const f = e.target.files && e.target.files[0]; if(!f) return; if(f.type.startsWith('image/')){ shopLogoPreview.src = URL.createObjectURL(f); shopLogoPreview.onload = ()=> URL.revokeObjectURL(shopLogoPreview.src); }
  });
  const profilePhoto = document.getElementById('profilePhoto');
  const profilePreview = document.getElementById('profilePreview');
  if(profilePhoto) profilePhoto.addEventListener('change', (e)=>{ const f = e.target.files && e.target.files[0]; if(!f) return; if(f.type.startsWith('image/')){ profilePreview.src = URL.createObjectURL(f); profilePreview.onload = ()=> URL.revokeObjectURL(profilePreview.src); } });

  // Deactivate / Delete handlers
  const deactivateBtn = document.getElementById('deactivateBtn');
  const deleteBtn = document.getElementById('deleteBtn');
  if(deactivateBtn) deactivateBtn.addEventListener('click', ()=>{
    if(confirm('Temporarily deactivate your account? You can reactivate later.')){
      alert('Account deactivated (demo).');
    }
  });
  if(deleteBtn) deleteBtn.addEventListener('click', ()=>{
    if(confirm('Delete your account and all shop data? This action is irreversible.')){
      // demo: clear localStorage and redirect to home
      localStorage.removeItem(KEY);
      alert('Account deleted (demo). Redirecting...');
      window.location.href = 'index.html';
    }
  });

  document.getElementById('cancelBtn').addEventListener('click', ()=>{ load(); saveMsg.textContent='Changes reverted'; setTimeout(()=>saveMsg.textContent='',1500); });

  // initial load
  load();
});
