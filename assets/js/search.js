// ==================== MR SHOP - SEARCH MODULE ====================
// Real-time search with suggestions

const MR_Search = {
  debounceTimer: null,

  init(inputSelector, resultsSelector) {
    const input = document.querySelector(inputSelector);
    const results = document.querySelector(resultsSelector);
    if (!input || !results) return;

    input.addEventListener('input', (e) => {
      clearTimeout(this.debounceTimer);
      this.debounceTimer = setTimeout(() => {
        this.showSuggestions(e.target.value, results);
      }, 200);
    });

    input.addEventListener('focus', (e) => {
      if (e.target.value.length >= 2) {
        this.showSuggestions(e.target.value, results);
      }
    });

    input.addEventListener('keydown', (e) => {
      if (e.key === 'Enter') {
        e.preventDefault();
        this.doSearch(input.value);
        results.style.display = 'none';
      }
    });

    document.addEventListener('click', (e) => {
      if (!input.contains(e.target) && !results.contains(e.target)) {
        results.style.display = 'none';
      }
    });
  },

  showSuggestions(query, resultsEl) {
    if (query.length < 2) {
      resultsEl.style.display = 'none';
      return;
    }

    const results = MR_searchProducts(query).slice(0, 8);
    
    if (results.length === 0) {
      resultsEl.innerHTML = '<div class="search-suggestion-item">No products found</div>';
      resultsEl.style.display = 'block';
      return;
    }

    // Group by category
    const categories = {};
    results.forEach(p => {
      if (!categories[p.category]) categories[p.category] = [];
      categories[p.category].push(p);
    });

    let html = '';
    for (const [cat, products] of Object.entries(categories)) {
      html += `<div class="search-category-label">${MR_CATEGORIES[cat] || cat}</div>`;
      products.forEach(p => {
        const discount = MR_getDiscount(p.originalPrice, p.price);
        html += `
          <div class="search-suggestion-item" onclick="MR_Search.goToProduct(${p.id})">
            <img src="${p.image}" alt="${p.name}" onerror="this.src='https://via.placeholder.com/40x40?text=No+Image'">
            <div class="search-suggestion-info">
              <div class="search-suggestion-name">${p.name}</div>
              <div class="search-suggestion-price">
                <span class="search-price-current">৳${p.price.toLocaleString()}</span>
                ${discount > 0 ? `<span class="search-price-original">৳${p.originalPrice.toLocaleString()}</span><span class="search-price-discount">-${discount}%</span>` : ''}
              </div>
            </div>
            <div class="search-suggestion-arrow"><i class="fas fa-arrow-right"></i></div>
          </div>
        `;
      });
    }

    resultsEl.innerHTML = html;
    resultsEl.style.display = 'block';
  },

  doSearch(query) {
    if (!query || query.trim() === '') return;
    sessionStorage.setItem('mr_search_query', query.trim());
    window.location.href = 'search-results.html';
  },

  goToProduct(id) {
    const product = MR_getProductById(id);
    if (product) {
      window.location.href = product.link || `product-details.html?id=${id}`;
    }
  },

  getResults() {
    const query = sessionStorage.getItem('mr_search_query');
    if (!query) return [];
    return MR_searchProducts(query);
  },

  getSuggestions(query) {
    if (!query || query.length < 2) return [];
    return MR_searchProducts(query).slice(0, 6);
  }
};

// Category filter
function MR_filterByCategory(category) {
  sessionStorage.setItem('mr_filter_category', category);
  window.location.href = 'search-results.html';
}

function MR_getFilterCategory() {
  return sessionStorage.getItem('mr_filter_category') || 'all';
}
