// ==================== MR SHOP - FAST LOADER ====================
// Single script tag - loads fonts & icons without blocking page render
(function() {
  // Google Fonts - non-blocking
  const fontLink = document.createElement('link');
  fontLink.rel = 'stylesheet';
  fontLink.href = 'https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap';
  document.head.appendChild(fontLink);

  // Font Awesome - non-blocking with integrity hash
  const faLink = document.createElement('link');
  faLink.rel = 'stylesheet';
  faLink.href = 'https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css';
  faLink.integrity = 'sha512-DTOQO9RWCH3ppGqcWaEA1BIZOC6xxalwEsw9c2QQeAIftl+Vegovlnee1c9QX4TctnWMn13TZye+giMm8e2LwA==';
  faLink.crossOrigin = 'anonymous';
  document.head.appendChild(faLink);

  // Update cart count from localStorage
  try {
    const cart = JSON.parse(localStorage.getItem('mr_shop_cart')) || [];
    const count = cart.reduce((s, i) => s + i.quantity, 0);
    document.querySelectorAll('.amz-cart-count').forEach(el => el.textContent = count);
    const wishlist = JSON.parse(localStorage.getItem('mr_shop_wishlist')) || [];
    document.querySelectorAll('.amz-wishlist-count').forEach(el => el.textContent = wishlist.length);
  } catch(e) {}
})();
