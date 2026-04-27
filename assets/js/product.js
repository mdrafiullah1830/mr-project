// product.js - Page logic for product.html
(function(){
  function $(sel){return document.querySelector(sel)}

  function normalizeImageUrl(imageValue) {
    const value = String(imageValue || '').trim();

    if (!value) {
      return '/assets/images/mrlogo.png';
    }

    if (value.startsWith('http://') || value.startsWith('https://') || value.startsWith('data:')) {
      return value;
    }

    const cleanPath = value
      .replace(/^\.\.\/images\//, '')
      .replace(/^\.\.\/assets\/images\//, '')
      .replace(/^\.\/assets\/images\//, '')
      .replace(/^\.\//, '')
      .replace(/^\/assets\/images\//, '')
      .replace(/^assets\/images\//, '')
      .replace(/^images\//, '')
      .replace(/^\//, '');

    return `../images/${cleanPath}`;
  }

  function showMessage(text, timeout=3000){
    const el = $('#message');
    if(!el) return;
    el.textContent = text;
    if(timeout>0) setTimeout(()=>{ el.textContent = '' }, timeout);
  }

  function getIdFromQuery(){
    const params = new URLSearchParams(window.location.search);
    return params.get('id');
  }
  function getNameFromQuery(){
    const params = new URLSearchParams(window.location.search);
    return params.get('name');
  }

  function getSelectedProductSnapshot(){
    try{
      const raw = sessionStorage.getItem('mrshop_selected_product');
      if(!raw) return null;
      const parsed = JSON.parse(raw);
      return parsed && typeof parsed === 'object' ? parsed : null;
    }catch(err){
      return null;
    }
  }

  function renderProduct(product){
    if(!product){
      $('#productName').textContent = 'Product not found';
      $('#productPrice').textContent = '';
      $('#productDescription').textContent = '';
      $('#productImage').innerHTML = '<div style="padding:40px;color:#888">No product image</div>';
      return;
    }

    $('#productName').textContent = product.name || '';
  const price = (product.final_price !== undefined) ? product.final_price : (product.price || 0);
  $('#productPrice').textContent = '৳' + price;
  $('#productCategory').textContent = product.category || '';
  $('#productDescription').textContent = product.description || '';

  const img = document.createElement('img');
  img.src = normalizeImageUrl(product.image || product.image_path || product.imageBase64 || '/assets/images/mrlogo.png');
    img.alt = product.name || 'Product image';
    $('#productImage').innerHTML = '';
    $('#productImage').appendChild(img);
  }

  function addToCart(product, qty=1){
    try{
      if (window.MRShopCartFlow && typeof window.MRShopCartFlow.addToCart === 'function') {
        const added = window.MRShopCartFlow.addToCart(product, {
          quantity: qty,
          returnUrl: window.location.href
        });

        if (added) {
          showMessage('Added to cart');
        }

        return;
      }

      const key = 'mr_shop_cart';
      const legacyKey = 'mrshop_cart';
      const raw = localStorage.getItem(key);
      const legacyRaw = localStorage.getItem(legacyKey);
      const cart = raw ? JSON.parse(raw) : [];

      if (cart.length === 0 && legacyRaw) {
        const legacyCart = JSON.parse(legacyRaw);
        if (Array.isArray(legacyCart) && legacyCart.length > 0) {
          cart.push(...legacyCart);
        }
      }

      const price = (product.final_price !== undefined) ? product.final_price : (product.price || 0);
      const existing = cart.find(i=>String(i.id)===String(product.id));
      if(existing){ existing.quantity = (existing.quantity||0) + qty; }
      else { cart.push({ id: product.id, name: product.name, price: price, quantity: qty }); }
      localStorage.setItem(key, JSON.stringify(cart));
      localStorage.setItem(legacyKey, JSON.stringify(cart));
      showMessage('Added to cart');
    }catch(err){
      console.error('Add to cart failed', err);
      showMessage('Failed to add to cart');
    }
  }

  async function buyNow(product){
    if(!product) return showMessage('Product not available');
    try{
      const res = await fetch('/api/payment', { method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify({ productId: String(product.id), quantity: 1 }) });
      const data = await res.json();
      if(res.ok && data && data.paymentUrl){
        window.location.href = data.paymentUrl;
      } else {
        showMessage('Payment initiation failed');
      }
    }catch(err){
      console.error('Buy now failed', err);
      showMessage('Network error initiating payment');
    }
  }

  // Init
  document.addEventListener('DOMContentLoaded', async function(){
    try{
      const id = getIdFromQuery();
      if(!id){ showMessage('No product id provided in URL'); return; }

      if (!window.MRShop || !window.MRShop.Products || typeof window.MRShop.Products.getProductById !== 'function'){
        showMessage('Product loader not available. Make sure assets/js/products.js is loaded.');
        return;
      }

      const snapshotProduct = getSelectedProductSnapshot();
      let product = snapshotProduct && String(snapshotProduct.id) === String(id) ? snapshotProduct : await window.MRShop.Products.getProductById(id);

      // If product not found by id, try name fallback
      if(!product){
        const name = getNameFromQuery();
        if(name && window.MRShop.Products.getProductByName){
          product = await window.MRShop.Products.getProductByName(name);
        }
      }

      renderProduct(product);

      if(!product){
        $('#addToCartBtn').disabled = true;
        $('#buyNowBtn').disabled = true;
        showMessage('Product not found.');
        return;
      }

      $('#addToCartBtn').addEventListener('click', function(){ addToCart(product, 1) });
      $('#buyNowBtn').addEventListener('click', function(){ buyNow(product) });
    }catch(err){
      console.error('product.js init error', err);
      showMessage('An error occurred while loading the product. Check console for details.');
    }
  });
})();
