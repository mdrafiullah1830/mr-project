// ==================== MR SHOP - UNIFIED PRODUCT DATA ====================
// Now fetches from C# API with localStorage fallback

const API_BASE_URL = window.location.hostname === 'localhost' ? 'http://localhost:5000/api' : 'https://mrshop-bd.azurewebsites.net/api';

// Fallback products (used when API is offline)
const MR_PRODUCTS_FALLBACK = [
  { id: 1, name: "Sundarbans Wild Honey - Pure & Organic 500g", price: 850, originalPrice: 1200, category: "food", image: "./assets/images/honey.jpg", rating: 4.8, reviews: 234, prime: true, description: "Raw, natural honey from the deep Sundarbans.", stock: 15, link: "food&natural.html" },
  { id: 2, name: "Pure Mustard Oil - Cold Pressed 1L", price: 320, originalPrice: 400, category: "food", image: "./assets/images/oil.jpg", rating: 4.6, reviews: 189, prime: true, description: "Traditional cold-pressed mustard oil.", stock: 25, link: "food&natural.html" },
  { id: 3, name: "Organic Basmati Rice - Premium 5kg", price: 180, originalPrice: 250, category: "food", image: "./assets/images/rice.jpg", rating: 4.5, reviews: 312, prime: true, description: "Premium long-grain basmati rice.", stock: 40, link: "food&natural.html" },
  { id: 101, name: "Authentic Bogura Doi - Traditional 500g", price: 180, originalPrice: 220, category: "sweets", image: "./assets/images/doi.jpg", rating: 4.9, reviews: 456, prime: true, description: "Authentic Bogura yogurt.", stock: 20, link: "sweets&dairy.html" },
  { id: 201, name: "Nakshi Kantha - Hand Embroidered Quilt", price: 1200, originalPrice: 1800, category: "handicrafts", image: "./assets/images/nakshi.jpg", rating: 4.9, reviews: 89, prime: false, description: "Hand-embroidered quilt.", stock: 5, link: "handricrafts.html" },
  { id: 301, name: "Jamdani Saree - Pure Cotton Handloom", price: 4500, originalPrice: 6000, category: "clothing", image: "./assets/images/saree.jpg", rating: 4.9, reviews: 78, prime: false, description: "Pure cotton Jamdani saree.", stock: 6, link: "clothing.html" },
  { id: 401, name: "Bengali Literature Collection", price: 350, originalPrice: 480, category: "books", image: "./assets/images/book1.jpg", rating: 4.7, reviews: 145, prime: true, description: "Classic Bengali literature.", stock: 25, link: "book.html" },
  { id: 501, name: "Antique Coin - Singapore 50 Cents (1978)", price: 150, originalPrice: 200, category: "antique", image: "./assets/images/coin1.jpeg", rating: 4.5, reviews: 56, prime: false, description: "Rare Singapore coin.", stock: 3, link: "antique.html" }
];

// Global products array - will be populated from API or fallback
let MR_PRODUCTS = [...MR_PRODUCTS_FALLBACK];

// Category display names
const MR_CATEGORIES = {
  food: "Food & Natural",
  sweets: "Sweets & Dairy",
  handicrafts: "Handicrafts",
  clothing: "Clothing",
  books: "Books",
  antique: "Antique & Collectibles"
};

// Fetch products from API
async function MR_fetchProducts() {
  try {
    const response = await fetch(`${API_BASE_URL}/products?limit=100`);
    if (response.ok) {
      const data = await response.json();
      const apiProducts = data.products || data;
      if (apiProducts && apiProducts.length > 0) {
        MR_PRODUCTS = apiProducts.map(p => ({
          id: p.id,
          name: p.name,
          slug: p.slug,
          price: p.price,
          originalPrice: p.discountPrice || p.price,
          category: p.categoryId || 'general',
          image: (p.thumbnailImage || '').startsWith('./') ? p.thumbnailImage : (p.thumbnailImage ? `./${p.thumbnailImage}` : './assets/images/placeholder.jpg'),
          rating: p.averageRating || 0,
          reviews: p.reviewCount || 0,
          prime: false,
          description: p.shortDescription || p.description || '',
          stock: p.stockQuantity || 0,
          soldCount: p.soldCount || 0,
          viewCount: p.viewCount || 0,
          tags: p.tags || [],
          link: `product-details.html?id=${p.id}`
        }));
        console.log('Products loaded from API:', MR_PRODUCTS.length);
        return true;
      }
    }
  } catch (err) {
    console.log('API offline, using fallback products');
  }
  return false;
}

function getCategoryLink(category) {
  const links = {
    food: 'food&natural.html',
    sweets: 'sweets&dairy.html',
    handicrafts: 'handricrafts.html',
    clothing: 'clothing.html',
    books: 'book.html',
    antique: 'antique.html'
  };
  return links[category] || 'index.html';
}

// Helper functions
function MR_getProductById(id) {
  return MR_PRODUCTS.find(p => p.id === id);
}

function MR_getProductsByCategory(category) {
  if (!category || category === 'all') return MR_PRODUCTS;
  return MR_PRODUCTS.filter(p => p.category === category);
}

function MR_searchProducts(query) {
  if (!query) return MR_PRODUCTS;
  const q = query.toLowerCase();
  return MR_PRODUCTS.filter(p => 
    p.name.toLowerCase().includes(q) || 
    p.description.toLowerCase().includes(q) || 
    p.category.toLowerCase().includes(q)
  );
}

function MR_getDiscount(original, current) {
  return Math.round(((original - current) / original) * 100);
}

function MR_generateStars(rating) {
  const full = Math.floor(rating);
  const half = rating % 1 >= 0.5;
  let html = '';
  for (let i = 0; i < full; i++) html += '<i class="fas fa-star"></i>';
  if (half) html += '<i class="fas fa-star-half-alt"></i>';
  const empty = 5 - full - (half ? 1 : 0);
  for (let i = 0; i < empty; i++) html += '<i class="far fa-star"></i>';
  return html;
}

function MR_getDeliveryDate() {
  const d = new Date();
  d.setDate(d.getDate() + 3 + Math.floor(Math.random() * 3));
  return d.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' });
}

function MR_createProductCard(product) {
  const discount = MR_getDiscount(product.originalPrice, product.price);
  const hasDiscount = discount > 0;
  const stockStatus = product.stock === 0 ? 'out' : product.stock < 5 ? 'low' : 'in';
  const stockText = product.stock === 0 ? 'Out of Stock' : product.stock < 5 ? `Only ${product.stock} left` : 'In Stock';
  const stockColor = product.stock === 0 ? '#cc0c39' : product.stock < 5 ? '#e77600' : '#007600';

  return `
    <div class="amz-product-card" onclick="MR_goToProduct(${product.id})">
      ${hasDiscount ? `<span class="deal-badge">${discount}% off</span>` : ''}
      <div class="amz-product-img-wrap">
        <img src="${product.image}" alt="${product.name}" onerror="this.src='https://via.placeholder.com/200x200?text=No+Image'">
      </div>
      <div class="amz-product-info">
        <div class="amz-product-title">${product.name}</div>
        <div class="amz-product-rating">
          <span class="amz-stars">${MR_generateStars(product.rating)}</span>
          <span class="amz-rating-count">${product.reviews.toLocaleString()}</span>
        </div>
        <div class="amz-product-price">
          <span class="amz-price-symbol">৳</span>
          <span class="amz-price-whole">${product.price.toLocaleString()}</span>
          ${hasDiscount ? `<span class="amz-price-original">৳${product.originalPrice.toLocaleString()}</span><span class="amz-price-discount">-${discount}%</span>` : ''}
        </div>
        ${product.prime ? '<div class="amz-prime-badge"><span class="prime-icon">MR</span> Prime</div>' : ''}
        <div class="amz-delivery-info" style="color:${stockColor};font-weight:600;">${stockText}</div>
        <div class="amz-delivery-info">FREE delivery <strong>${MR_getDeliveryDate()}</strong></div>
        <button class="amz-product-btn amz-product-btn-cart" onclick="event.stopPropagation();MR_addToCart(${product.id})">Add to Cart</button>
      </div>
    </div>
  `;
}

function MR_goToProduct(id) {
  const product = MR_getProductById(id);
  if (product) {
    window.location.href = product.link || `product-details.html?id=${id}`;
  }
}

// Load products from localStorage (admin added products)
function MR_loadLocalProducts() {
  try {
    const localProducts = JSON.parse(localStorage.getItem('mr_shop_products')) || [];
    if (localProducts.length > 0) {
      // Map local products to frontend format
      const mapped = localProducts.map(p => ({
        id: p.id,
        name: p.name,
        price: p.price,
        originalPrice: p.originalPrice || p.price,
        category: p.category,
        image: p.image || './assets/images/placeholder.jpg',
        images: p.images || [],
        rating: p.rating || 0,
        reviews: p.reviews || 0,
        prime: p.prime || false,
        description: p.description || '',
        stock: p.stock || 0,
        link: getCategoryLink(p.category)
      }));
      // Add to MR_PRODUCTS (avoid duplicates by id)
      for (const lp of mapped) {
        if (!MR_PRODUCTS.find(p => p.id === lp.id)) {
          MR_PRODUCTS.push(lp);
        }
      }
      console.log('Local products loaded:', mapped.length);
    }
  } catch (err) {
    console.log('Error loading local products:', err);
  }
}

// Load local products immediately when script loads
MR_loadLocalProducts();

// Initialize products on page load
document.addEventListener('DOMContentLoaded', async () => {
  const apiLoaded = await MR_fetchProducts();
  if (!apiLoaded) {
    // Load from localStorage when API is offline
    MR_loadLocalProducts();
  } else {
    // Also load local products to merge admin-added ones
    MR_loadLocalProducts();
  }
  // Re-render any product grids on the page
  document.querySelectorAll('[data-products-grid]').forEach(grid => {
    const category = grid.dataset.category;
    const products = category ? MR_getProductsByCategory(category) : MR_PRODUCTS;
    grid.innerHTML = products.map(p => MR_createProductCard(p)).join('');
  });
});
