// search.js - real-time product search on home page
(function(){
  // Products data (extracted from the page or hardcoded)
  const PRODUCTS = [
    { name: 'Smartphone X10', price: '৳ 15,999', image: 'https://i.ibb.co/S6qMxwr/phone.jpg' },
    { name: 'Sports Shoes', price: '৳ 2,499', image: 'https://i.ibb.co/fqzGyvY/shoes.jpg' },
    { name: 'Digital Watch', price: '৳ 1,299', image: 'https://i.ibb.co/wwzCzKR/watch.jpg' },
    { name: 'Wireless Headphones', price: '৳ 3,999', image: 'https://i.ibb.co/dM2Hv5S/headphones.jpg' },
    // Antique coins
    { name: 'Singapore 50 cents (1978)', price: '৳ 150', image: './assets/images/coin1.jpeg' },
    { name: 'Bahrain 100 Fils (2010)', price: '৳ 150', image: './assets/images/coin2.jpeg' },
    { name: 'Saudi Arabia 50 Halala', price: '৳ 150', image: './assets/images/coin4.jpeg' },
  ];

  const searchInput = document.getElementById('searchInput');
  const searchResults = document.getElementById('searchResults');

  if (!searchInput || !searchResults) return;

  // Filter and display results on input
  searchInput.addEventListener('input', function() {
    const query = this.value.trim().toLowerCase();
    
    if (query.length === 0) {
      searchResults.classList.remove('open');
      searchResults.setAttribute('aria-hidden', 'true');
      return;
    }

    // Filter products by name (case-insensitive substring match)
    const matches = PRODUCTS.filter(p => p.name.toLowerCase().includes(query));

    // Render results
    if (matches.length === 0) {
      searchResults.innerHTML = '<div class="search-no-results">No products found</div>';
    } else {
      searchResults.innerHTML = matches.map((p, idx) => `
        <div class="search-result-item" role="option" aria-label="${p.name}, ${p.price}">
          <img src="${p.image}" alt="${p.name}" />
          <div class="search-result-info">
            <div class="search-result-name">${p.name}</div>
            <div class="search-result-price">${p.price}</div>
          </div>
        </div>
      `).join('');
    }

    searchResults.classList.add('open');
    searchResults.setAttribute('aria-hidden', 'false');
  });

  // Close results when clicking outside
  document.addEventListener('click', function(e) {
    if (!searchInput.contains(e.target) && !searchResults.contains(e.target)) {
      searchResults.classList.remove('open');
      searchResults.setAttribute('aria-hidden', 'true');
    }
  });

  // Close results on Escape key
  document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape' && searchResults.classList.contains('open')) {
      searchResults.classList.remove('open');
      searchResults.setAttribute('aria-hidden', 'true');
      searchInput.value = '';
    }
  });

  // Allow keyboard navigation (optional: arrow keys to select, Enter to submit)
  searchInput.addEventListener('keydown', function(e) {
    const items = searchResults.querySelectorAll('.search-result-item');
    if (items.length === 0) return;
    
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      items[0].focus();
    }
  });
})();
