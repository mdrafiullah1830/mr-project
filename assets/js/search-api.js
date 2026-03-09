/**
 * Search Integration Module
 * Handles product search functionality connected to C# backend
 * Usage: Call searchProducts(query) to search and display results
 */

const SearchAPI = {
  // Base API URL - adjust based on your server
  baseURL: 'http://localhost:5010/api/search',
  
  /**
   * Search products by query and optional category
   * @param {string} query - Search query
   * @param {string} category - Optional category filter
   * @returns {Promise} Search results
   */
  search: async function(query, category = null) {
    try {
      const params = new URLSearchParams({ query });
      if (category) params.append('category', category);
      
      const response = await fetch(`${this.baseURL}/search?${params}`);
      if (!response.ok) throw new Error(`API error: ${response.status}`);
      
      return await response.json();
    } catch (error) {
      console.error('Search error:', error);
      return {
        success: false,
        message: 'Search failed. Using offline search instead.',
        results: []
      };
    }
  },

  /**
   * Get products by category
   * @param {string} category - Category name
   * @returns {Promise} Products in category
   */
  getByCategory: async function(category) {
    try {
      const response = await fetch(`${this.baseURL}/category/${encodeURIComponent(category)}`);
      if (!response.ok) throw new Error(`API error: ${response.status}`);
      
      return await response.json();
    } catch (error) {
      console.error('Category fetch error:', error);
      return { success: false, results: [] };
    }
  },

  /**
   * Get all categories
   * @returns {Promise} List of categories
   */
  getCategories: async function() {
    try {
      const response = await fetch(`${this.baseURL}/categories`);
      if (!response.ok) throw new Error(`API error: ${response.status}`);
      
      return await response.json();
    } catch (error) {
      console.error('Categories fetch error:', error);
      return { success: false, categories: [] };
    }
  },

  /**
   * Get product by ID
   * @param {number} id - Product ID
   * @returns {Promise} Product details
   */
  getProductById: async function(id) {
    try {
      const response = await fetch(`${this.baseURL}/product/${id}`);
      if (!response.ok) throw new Error(`API error: ${response.status}`);
      
      return await response.json();
    } catch (error) {
      console.error('Product fetch error:', error);
      return { success: false };
    }
  },

  /**
   * Get all products
   * @param {boolean} active - Only active products
   * @returns {Promise} All products
   */
  getAllProducts: async function(active = true) {
    try {
      const response = await fetch(`${this.baseURL}/all?active=${active}`);
      if (!response.ok) throw new Error(`API error: ${response.status}`);
      
      return await response.json();
    } catch (error) {
      console.error('Products fetch error:', error);
      return { success: false, products: [] };
    }
  }
};

/**
 * Offline Search Fallback (localStorage-based)
 * Used when API is unavailable
 */
const OfflineSearch = {
  /**
   * Perform offline search using localStorage data
   */
  search: function(query, category = null) {
    try {
      const products = JSON.parse(localStorage.getItem('mr_shop_products')) || [];
      const searchTerms = query.toLowerCase().split(' ').filter(t => t);
      
      const results = products
        .filter(p => p.status === 'active')
        .filter(p => !category || p.category.toLowerCase() === category.toLowerCase())
        .map(p => ({
          product: p,
          relevance_score: this.calculateScore(p, searchTerms),
          matched_fields: this.getMatchedFields(p, searchTerms)
        }))
        .filter(r => r.relevance_score > 0)
        .sort((a, b) => b.relevance_score - a.relevance_score);

      return {
        success: true,
        query: query,
        category: category,
        total_results: results.length,
        results: results,
        message: `Found ${results.length} product(s) (offline)`
      };
    } catch (error) {
      console.error('Offline search error:', error);
      return { success: false, results: [], message: 'Search unavailable' };
    }
  },

  calculateScore: function(product, terms) {
    let score = 0;
    const name = product.name.toLowerCase();
    const desc = product.description.toLowerCase();
    const cat = product.category.toLowerCase();

    terms.forEach(term => {
      if (name.includes(term)) score += 10;
      if (cat.includes(term)) score += 7;
      if (desc.includes(term)) score += 3;
    });

    return score;
  },

  getMatchedFields: function(product, terms) {
    const matched = [];
    const name = product.name.toLowerCase();
    const desc = product.description.toLowerCase();
    const cat = product.category.toLowerCase();

    terms.forEach(term => {
      if (name.includes(term) && !matched.includes('name')) matched.push('name');
      if (cat.includes(term) && !matched.includes('category')) matched.push('category');
      if (desc.includes(term) && !matched.includes('description')) matched.push('description');
    });

    return matched;
  }
};

/**
 * Main Search Function
 * Call this from index.html when user searches
 */
async function searchProducts(query, category = null) {
  if (!query || query.trim().length === 0) {
    showNotification('Please enter a search query', 'error');
    return;
  }

  showNotification('Searching...', 'info');

  // Try API first, fallback to offline
  let results = await SearchAPI.search(query, category);
  
  if (!results.success) {
    console.log('API unavailable, using offline search');
    results = OfflineSearch.search(query, category);
  }

  displaySearchResults(results);
}

/**
 * Display search results
 */
function displaySearchResults(results) {
  const resultsContainer = document.getElementById('search-results');
  
  if (!resultsContainer) {
    console.warn('Search results container not found');
    return;
  }

  if (!results.success || results.total_results === 0) {
    resultsContainer.innerHTML = `
      <div class="search-no-results">
        <h3>No Products Found</h3>
        <p>${results.message || 'Try a different search query'}</p>
      </div>
    `;
    return;
  }

  let html = `
    <div class="search-header">
      <h2>Search Results</h2>
      <p>Found ${results.total_results} product(s) for "${results.query}"${results.category ? ` in ${results.category}` : ''}</p>
    </div>
    <div class="search-grid">
  `;

  results.results.forEach(result => {
    const p = result.product;
    html += `
      <div class="product-card search-result" data-product-id="${p.id}">
        <div class="product-image">
          <img src="${p.image_path}" alt="${p.name}" loading="lazy" />
          ${p.discount > 0 ? `<span class="discount-badge">${p.discount}% OFF</span>` : ''}
        </div>
        <div class="product-info">
          <h3>${p.name}</h3>
          <p class="product-category">${p.category}</p>
          <p class="product-description">${p.description}</p>
          <div class="product-meta">
            <span class="rating">⭐ ${p.rating}</span>
            <span class="stock">${p.stock > 0 ? '✓ In Stock' : '✗ Out of Stock'}</span>
          </div>
          <div class="product-pricing">
            ${p.discount > 0 ? `<span class="original-price">₹${p.price.toFixed(2)}</span>` : ''}
            <span class="final-price">₹${p.final_price.toFixed(2)}</span>
          </div>
          <div class="matched-fields">
            Matched: ${result.matched_fields.join(', ')} (Score: ${result.relevance_score.toFixed(1)})
          </div>
          <button class="btn-add-cart" onclick="addToCart(${p.id})">Add to Cart</button>
        </div>
      </div>
    `;
  });

  html += `</div>`;
  resultsContainer.innerHTML = html;
  resultsContainer.style.display = 'block';

  // Scroll to results
  resultsContainer.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

/**
 * Initialize search on page load
 * Attach event listeners to search inputs
 */
function initializeSearch() {
  const searchInput = document.getElementById('search-input');
  const searchBtn = document.getElementById('search-btn');

  if (searchInput && searchBtn) {
    searchBtn.addEventListener('click', () => {
      const query = searchInput.value;
      searchProducts(query);
    });

    searchInput.addEventListener('keypress', (e) => {
      if (e.key === 'Enter') {
        const query = searchInput.value;
        searchProducts(query);
      }
    });
  }

  // Load offline data if available
  loadOfflineData();
}

/**
 * Load products to localStorage for offline search
 */
async function loadOfflineData() {
  try {
    const response = await fetch('http://localhost:5010/api/search/all?active=true');
    if (response.ok) {
      const data = await response.json();
      localStorage.setItem('mr_shop_products', JSON.stringify(data.products));
      console.log('✓ Offline data loaded:', data.products.length, 'products');
    }
  } catch (error) {
    console.log('Could not load offline data:', error);
  }
}

/**
 * Notification helper
 */
function showNotification(message, type = 'info') {
  const notification = document.createElement('div');
  notification.className = `notification notification-${type}`;
  notification.textContent = message;
  notification.style.cssText = `
    position: fixed;
    top: 20px;
    right: 20px;
    padding: 12px 20px;
    background: ${type === 'success' ? '#10b981' : type === 'error' ? '#ef4444' : '#3b82f6'};
    color: white;
    border-radius: 8px;
    z-index: 1000;
    animation: slideIn 0.3s ease;
  `;
  document.body.appendChild(notification);
  
  setTimeout(() => {
    notification.style.animation = 'slideOut 0.3s ease';
    setTimeout(() => notification.remove(), 300);
  }, 3000);
}

// Initialize search when DOM is ready
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', initializeSearch);
} else {
  initializeSearch();
}
