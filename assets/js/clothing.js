// R Clothing - Product Page Interactions
(function(){
  const product = {
    id: 301,
    title: 'Premium Linen-Cotton Kurta',
    brand: 'R Clothing',
    price: 2250,
    original: 2999,
    discount: 25,
    images: [
      'https://images.unsplash.com/photo-1542060748-10c28b62716d?q=80&w=1600&auto=format&fit=crop',
      'https://images.unsplash.com/photo-1620799139504-5c5e27c0a2f1?q=80&w=1600&auto=format&fit=crop',
      'https://images.unsplash.com/photo-1520975922215-c0f734f87c37?q=80&w=1600&auto=format&fit=crop',
      'https://images.unsplash.com/photo-1591047139829-d91aecb6caea?q=80&w=1600&auto=format&fit=crop'
    ],
    colors: ['Navy','Ivory','Maroon','Olive'],
    sizes: ['S','M','L','XL','XXL']
  };

  const mainImage = document.getElementById('mainImage');
  const mainImageWrapper = document.getElementById('mainImageWrapper');
  const thumbs = document.getElementById('thumbs');
  const sizeGrid = document.getElementById('sizeGrid');
  const colorRow = document.getElementById('colorRow');
  const addToCartBtn = document.getElementById('addToCartBtn');
  const buyNowBtn = document.getElementById('buyNowBtn');
  const toast = document.getElementById('rcToast');

  let selectedSize = null;
  let selectedColor = null;
  let zoomActive = false;

  // Image zoom (mousemove adjusts transform-origin)
  function handleZoomMove(e){
    const rect = mainImageWrapper.getBoundingClientRect();
    const x = ((e.clientX - rect.left) / rect.width) * 100;
    const y = ((e.clientY - rect.top) / rect.height) * 100;
    mainImage.style.transformOrigin = `${x}% ${y}%`;
  }

  function enableZoom(){ zoomActive = true; mainImageWrapper.classList.add('zooming'); }
  function disableZoom(){ zoomActive = false; mainImageWrapper.classList.remove('zooming'); mainImage.style.transformOrigin = '50% 50%'; }

  mainImageWrapper.addEventListener('mousemove', handleZoomMove);
  mainImageWrapper.addEventListener('mouseenter', enableZoom);
  mainImageWrapper.addEventListener('mouseleave', disableZoom);
  // Touch: toggle zoom
  mainImageWrapper.addEventListener('touchstart', (e)=>{
    e.preventDefault(); zoomActive ? disableZoom() : enableZoom();
  }, {passive:false});

  // Thumbnails
  if(thumbs){
    thumbs.addEventListener('click', (e)=>{
      const btn = e.target.closest('.rc-thumb');
      if(!btn) return;
      const src = btn.dataset.src;
      if(src){
        mainImage.src = src;
        document.querySelectorAll('.rc-thumb').forEach(b=>b.classList.remove('rc-thumb--active'));
        btn.classList.add('rc-thumb--active');
      }
    });
  }

  // Size selection
  sizeGrid.addEventListener('click', (e)=>{
    const btn = e.target.closest('.size-btn');
    if(!btn) return;
    document.querySelectorAll('.size-btn').forEach(b=>b.classList.remove('selected'));
    btn.classList.add('selected');
    selectedSize = btn.dataset.size;
  });

  // Color selection
  colorRow.addEventListener('click', (e)=>{
    const btn = e.target.closest('.color-swatch');
    if(!btn) return;
    document.querySelectorAll('.color-swatch').forEach(b=>b.classList.remove('selected'));
    btn.classList.add('selected');
    selectedColor = btn.dataset.color;
  });

  // Cart helpers
  function getCart(){ try{ return JSON.parse(localStorage.getItem('mr_shop_cart')) || []; }catch{ return []; } }
  function setCart(c){ localStorage.setItem('mr_shop_cart', JSON.stringify(c)); }

  function addToCart(){
    if(!selectedSize){ showToast('Please select a size'); return; }
    if(!selectedColor){ showToast('Please select a color'); return; }
    const cart = getCart();
    const key = `${product.id}-${selectedSize}-${selectedColor}`;
    const idx = cart.findIndex(i => (i.id === product.id && i.size === selectedSize && i.color === selectedColor));
    if(idx>-1){ cart[idx].quantity += 1; }
    else{
      cart.push({
        id: product.id,
        name: product.title,
        price: product.price,
        image: product.images[0],
        description: `${product.brand} • ${selectedColor} • ${selectedSize}`,
        quantity: 1,
        size: selectedSize,
        color: selectedColor
      });
    }
    setCart(cart);
    showToast(`Added 1 × ${product.title} (${selectedColor}, ${selectedSize})`);
  }

  addToCartBtn.addEventListener('click', addToCart);
  buyNowBtn.addEventListener('click', ()=>{ addToCart(); setTimeout(()=>{ window.location.href = 'cart.html'; }, 400); });

  function showToast(msg){
    toast.textContent = msg; toast.classList.add('show'); setTimeout(()=>toast.classList.remove('show'), 1800);
  }

  // Reviews
  const REV_KEY = 'mr_reviews_' + product.id;
  const reviewsList = document.getElementById('reviewsList');
  const reviewForm = document.getElementById('reviewForm');
  const reviewText = document.getElementById('reviewText');
  const reviewStars = document.getElementById('reviewStars');
  let selectedStars = 0;

  function loadReviews(){
    let list = [];
    try{ list = JSON.parse(localStorage.getItem(REV_KEY)) || []; }catch{}
    renderReviews(list);
  }

  function renderReviews(list){
    if(list.length === 0){
      reviewsList.innerHTML = `<div class="review-card"><div class="review-meta">No reviews yet — be the first to review.</div></div>`;
      updateSummary(0, 0); return;
    }
    reviewsList.innerHTML = list.map(r => `
      <div class="review-card">
        <div class="review-header">
          <div class="review-stars">${'★'.repeat(r.stars)}${'☆'.repeat(5-r.stars)}</div>
          <div class="review-meta">${new Date(r.time).toLocaleDateString()}</div>
        </div>
        <div class="review-text">${escapeHtml(r.text)}</div>
      </div>`).join('');
    const avg = list.reduce((a,b)=>a+b.stars,0)/list.length;
    updateSummary(avg, list.length);
  }

  function updateSummary(avg, count){
    const scoreEls = document.querySelectorAll('#ratingSummary .score, #ratingLarge .score');
    scoreEls.forEach(el => el.textContent = count ? `${avg.toFixed(1)}${el.id==='ratingLarge'?' out of 5':''}` : (el.id==='ratingLarge'?'No reviews':'—'));
  }

  function escapeHtml(s){
    return s.replace(/[&<>"]/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c]));
  }

  reviewStars.addEventListener('click', (e)=>{
    const btn = e.target.closest('button');
    if(!btn) return;
    selectedStars = parseInt(btn.dataset.star,10);
    reviewStars.querySelectorAll('button').forEach((b,i)=>{
      b.classList.toggle('active', i < selectedStars);
    });
  });

  reviewForm.addEventListener('submit', (e)=>{
    e.preventDefault();
    if(selectedStars < 1){ showToast('Please choose a star rating'); return; }
    const text = (reviewText.value||'').trim();
    if(!text){ showToast('Please write a short review'); return; }
    let list = [];
    try{ list = JSON.parse(localStorage.getItem(REV_KEY)) || []; }catch{}
    list.unshift({ stars: selectedStars, text, time: Date.now() });
    localStorage.setItem(REV_KEY, JSON.stringify(list));
    reviewText.value=''; selectedStars=0; reviewStars.querySelectorAll('button').forEach(b=>b.classList.remove('active'));
    loadReviews();
    showToast('Thanks for your review!');
  });

  // Related slider
  const track = document.getElementById('relatedTrack');
  const prevBtn = document.getElementById('relPrev');
  const nextBtn = document.getElementById('relNext');
  function scrollTrack(dir){
    const card = track.querySelector('.rel-card');
    const step = card ? (card.getBoundingClientRect().width + 12) * 2 : 400;
    track.scrollBy({left: dir * step, behavior:'smooth'});
  }
  prevBtn.addEventListener('click', ()=>scrollTrack(-1));
  nextBtn.addEventListener('click', ()=>scrollTrack(1));

  // Init
  loadReviews();
})();
