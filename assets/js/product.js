// product.js - Page logic for product.html
(function(){
  function $(sel){return document.querySelector(sel)}

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

  function renderProduct(product){
    if(!product){
      $('#productName').textContent = 'Product not found';
      $('#productPrice').textContent = '';
      $('#productDescription').textContent = '';
      $('#productImage').innerHTML = '<div style="padding:40px;color:#888">No product image</div>';
      return;
    }

    $('#productName').textContent = product.name || '';
    $('#productPrice').textContent = '৳' + (product.price || 0);
    $('#productCategory').textContent = product.category || '';
    $('#productDescription').textContent = product.description || '';

    const img = document.createElement('img');
    img.src = product.image || '/assets/images/placeholder.jpg';
    img.alt = product.name || 'Product image';
    $('#productImage').innerHTML = '';
    $('#productImage').appendChild(img);
  }

  function addToCart(product, qty=1){
    try{
      const key = 'mrshop_cart';
      const raw = localStorage.getItem(key);
      const cart = raw ? JSON.parse(raw) : [];
      const existing = cart.find(i=>String(i.id)===String(product.id));
      if(existing){ existing.quantity = (existing.quantity||0) + qty; }
      else { cart.push({ id: product.id, name: product.name, price: product.price, quantity: qty }); }
      localStorage.setItem(key, JSON.stringify(cart));
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
    const id = getIdFromQuery();
    if(!id){ showMessage('No product id provided in URL'); return; }
    const product = await window.MRShop.Products.getProductById(id);
    renderProduct(product);

    $('#addToCartBtn').addEventListener('click', function(){ addToCart(product, 1) });
    $('#buyNowBtn').addEventListener('click', function(){ buyNow(product) });
  });
})();
