// search.js - real-time product search using C# backend API
(function(){
  // Wait for DOM to be ready
  function init() {
    const searchInput = document.getElementById('searchInput');
    const searchResults = document.getElementById('searchResults');
    
    if (!searchInput || !searchResults) {
      console.warn('Search elements not found in DOM');
      return;
    }

    let searchTimeout;
    let currentQuery = '';

    // API Configuration
    const API_BASE = 'http://localhost:5010/api/search';
    const DEBOUNCE_DELAY = 300; // Wait 300ms after user stops typing

    /**
     * Fetch search results from backend API
     */
    async function fetchSearchResults(query) {
      if (!query || query.length < 1) {
        searchResults.classList.remove('open');
        searchResults.setAttribute('aria-hidden', 'true');
        return;
      }

      try {
        searchResults.innerHTML = '<div class="search-loading">🔍 Searching...</div>';
        searchResults.classList.add('open');
        searchResults.setAttribute('aria-hidden', 'false');

        const response = await fetch(`${API_BASE}?query=${encodeURIComponent(query)}&pageSize=8&sortBy=relevance`);
        
        if (!response.ok) {
          throw new Error(`API error: ${response.status}`);
        }

        const data = await response.json();

        if (data.success && data.data && data.data.results.length > 0) {
          renderSearchResults(data.data.results);
        } else {
          searchResults.innerHTML = '<div class="search-no-results">No products found for "' + escapeHtml(query) + '"</div>';
        }
      } catch (error) {
        console.error('Search error:', error);
        searchResults.innerHTML = '<div class="search-error">⚠️ Connection error. API may be offline.</div>';
      }
    }

    /**
     * Escape HTML to prevent XSS
     */
    function escapeHtml(text) {
      const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
      };
      return text.replace(/[&<>"']/g, m => map[m]);
    }

    /**
     * Render search results as dropdown items
     */
    function renderSearchResults(results) {
      if (!results || results.length === 0) {
        searchResults.innerHTML = '<div class="search-no-results">No products found</div>';
        return;
      }

      const html = results.map((product, idx) => {
        const discountBadge = product.discount > 0 
          ? `<div class="search-result-discount">-${product.discount}%</div>` 
          : '';
        
        const ratingStars = product.rating > 0 
          ? `<div class="search-result-rating">⭐ ${product.rating}</div>`
          : '';

        return `
  <div class="search-result-item" role="option" tabindex="0" data-id="${product.id}" aria-label="${escapeHtml(product.name)}, ৳${product.final_price}">
          <div class="search-result-image-wrapper">
            <img src="${product.image_path || 'https://via.placeholder.com/80?text=No+Image'}" 
                 alt="${escapeHtml(product.name)}" 
                 class="search-result-image"
                 onerror="this.src='https://via.placeholder.com/80?text=No+Image'" />
            ${discountBadge}
          </div>
          <div class="search-result-info">
            <div class="search-result-name">${escapeHtml(product.name)}</div>
            <div class="search-result-category">📦 ${escapeHtml(product.category)}</div>
            <div class="search-result-price-row">
              <span class="search-result-price">৳ ${product.final_price}</span>
              ${product.discount > 0 ? `<span class="search-result-original">৳ ${product.price}</span>` : ''}
            </div>
            ${ratingStars}
          </div>
        </div>
      `;
      }).join('');

      searchResults.innerHTML = html;

      // Add click handlers to results
      document.querySelectorAll('.search-result-item').forEach(item => {
        item.addEventListener('click', function() {
          const id = this.getAttribute('data-id');
          if (id) {
            // Redirect to product details page
            window.location.href = `product.html?id=${encodeURIComponent(id)}`;
          }
        });

        item.addEventListener('keydown', function(e) {
          if (e.key === 'Enter') {
            this.click();
          }
        });
      });
    }

    /**
     * Handle search input with debouncing
     */
    searchInput.addEventListener('input', function() {
      currentQuery = this.value.trim().toLowerCase();
      
      // Clear previous timeout
      if (searchTimeout) {
        clearTimeout(searchTimeout);
      }

      if (currentQuery.length === 0) {
        searchResults.classList.remove('open');
        searchResults.setAttribute('aria-hidden', 'true');
        return;
      }

      // Debounce API call
      searchTimeout = setTimeout(() => {
        fetchSearchResults(currentQuery);
      }, DEBOUNCE_DELAY);
    });

    /**
     * Close dropdown when clicking outside
     */
    document.addEventListener('click', function(e) {
      if (!searchInput.contains(e.target) && !searchResults.contains(e.target)) {
        searchResults.classList.remove('open');
        searchResults.setAttribute('aria-hidden', 'true');
      }
    });

    /**
     * Close dropdown on Escape key
     */
    document.addEventListener('keydown', function(e) {
      if (e.key === 'Escape' && searchResults.classList.contains('open')) {
        searchResults.classList.remove('open');
        searchResults.setAttribute('aria-hidden', 'true');
        searchInput.value = '';
        currentQuery = '';
      }
    });

    /**
     * Keyboard navigation in dropdown
     */
    searchInput.addEventListener('keydown', function(e) {
      const items = searchResults.querySelectorAll('.search-result-item');
      if (items.length === 0) return;
      
      if (e.key === 'ArrowDown') {
        e.preventDefault();
        items[0].focus();
      } else if (e.key === 'Enter') {
        e.preventDefault();
        fetchSearchResults(this.value);
      }
    });

    /**
     * Arrow key navigation between results
     */
    document.addEventListener('keydown', function(e) {
      if (!searchResults.classList.contains('open')) return;

      const items = Array.from(searchResults.querySelectorAll('.search-result-item'));
      const focused = document.activeElement;
      const currentIndex = items.indexOf(focused);

      if (e.key === 'ArrowDown') {
        e.preventDefault();
        if (currentIndex < items.length - 1) {
          items[currentIndex + 1].focus();
        } else if (currentIndex >= 0) {
          items[0].focus();
        }
      } else if (e.key === 'ArrowUp') {
        e.preventDefault();
        if (currentIndex > 0) {
          items[currentIndex - 1].focus();
        } else {
          searchInput.focus();
        }
      }
    });
  }

  // Initialize when DOM is ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
