(function (window, document) {
  const API_BASE = 'http://localhost:5010/api.php';

  const CART_STORAGE_KEY = 'mr_shop_cart';

  function escapeHtml(value) {
    return String(value ?? '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function formatPrice(value) {
    const price = Number(value || 0);
    return `৳${price.toLocaleString('en-BD', { maximumFractionDigits: 0 })}`;
  }

  function getImageUrl(product) {
    const value = product?.image_path || product?.image || product?.imageBase64 || '';

    if (!value) {
      return '../images/mrlogo.png';
    }

    if (value.startsWith('http://') || value.startsWith('https://') || value.startsWith('data:') || value.startsWith('/')) {
      return value;
    }

    const cleanPath = value
      .replace(/^\.\.\/images\//, '')
      .replace(/^\.\//, '')
      .replace(/^assets\/images\//, '')
      .replace(/^images\//, '');

    return `../images/${cleanPath}`;
  }

  async function fetchJson(url) {
    const response = await fetch(url);
    if (!response.ok) {
      throw new Error(`Request failed with ${response.status}`);
    }
    return response.json();
  }

  function readStoredJson(key, fallbackValue) {
    try {
      const rawValue = localStorage.getItem(key);
      if (!rawValue) {
        return fallbackValue;
      }

      const parsedValue = JSON.parse(rawValue);
      return parsedValue ?? fallbackValue;
    } catch (error) {
      return fallbackValue;
    }
  }

  function isFoodCategory(category) {
    const normalized = String(category ?? '').trim().toLowerCase();
    const compact = normalized.replace(/[^a-z0-9]+/g, '');

    return normalized === 'food'
      || normalized === 'food & natural'
      || normalized === 'food & natural products'
      || normalized === '🍯 food & natural'
      || normalized === '🍯 food & natural products'
      || compact === 'foodnatural';
  }

  function getFallbackFoodProducts() {
    const storedAdminProducts = readStoredJson('mrshop_admin_products', []);
    if (Array.isArray(storedAdminProducts) && storedAdminProducts.length > 0) {
      return storedAdminProducts.filter(product => isFoodCategory(product?.category));
    }

    const storedGroupedProducts = readStoredJson('mrshop_products', {});
    if (!storedGroupedProducts || Array.isArray(storedGroupedProducts)) {
      return [];
    }

    return Object.values(storedGroupedProducts).reduce((items, categoryProducts) => {
      if (Array.isArray(categoryProducts)) {
        items.push(...categoryProducts.filter(product => isFoodCategory(product?.category)));
      }
      return items;
    }, []);
  }

  function getDefaultFoodNaturalSection() {
    return {
      title: 'Food & Natural',
      subtitle: 'Fresh, organic and authentic products updated from the live catalog.',
      badge: 'Live updates',
      ctaLabel: 'Open full collection',
      featuredLimit: 4,
      category: 'food'
    };
  }

  async function fetchFoodNaturalSection() {
    const url = new URL(API_BASE, window.location.href);
    url.searchParams.set('action', 'getFoodNaturalSection');
    return fetchJson(url.toString());
  }

  async function fetchFoodProducts() {
    const url = new URL(API_BASE, window.location.href);
    url.searchParams.set('action', 'getProducts');
    url.searchParams.set('category', 'food');
    return fetchJson(url.toString());
  }

  function renderCompactCard(product) {
    const image = getImageUrl(product);
    const title = escapeHtml(product.name);
    const description = escapeHtml(product.description || 'Fresh product added from the live catalog.');
    const price = formatPrice(product.final_price ?? product.price);
    const discountLabel = Number(product.discount || 0) > 0
      ? `${Number(product.discount)}% off`
      : 'Fresh & live';

    return `
      <article class="food-natural-card">
        <div class="food-natural-card-media">
          <img src="${escapeHtml(image)}" alt="${title}" loading="lazy">
          <span class="food-natural-card-tag">${escapeHtml(discountLabel)}</span>
        </div>
        <div class="food-natural-card-body">
          <div>
            <h3>${title}</h3>
            <p>${description}</p>
          </div>
          <div class="food-natural-card-footer">
            <strong>${price}</strong>
            <a href="food&natural.html">View collection</a>
          </div>
        </div>
      </article>
    `;
  }

  function renderPageCard(product) {
    const image = getImageUrl(product);
    const title = escapeHtml(product.name);
    const description = escapeHtml(product.description || 'Fresh and natural product from the live catalog.');
    const price = formatPrice(product.final_price ?? product.price);
    const category = escapeHtml(product.category || 'food');
    const productId = escapeHtml(product.id);

    return `
      <div class="product-card" data-product-id="${productId}" role="link" tabindex="0" aria-label="View details for ${title}">
        <img src="${escapeHtml(image)}" class="product-image" alt="${title}" loading="lazy" />
        <div class="product-info">
          <h3 class="product-name">${title}</h3>
          <p class="product-desc">${description}</p>
          <div class="product-price">${price}</div>
          <div class="product-meta" style="font-size:12px;color:#64748b;font-weight:600;">Category: ${category}</div>
          <div class="product-actions">
            <div class="qty-selector">
              <button type="button" data-qty-action="decrease" data-product-id="${productId}">−</button>
              <input id="qty-${productId}" value="1" readonly />
              <button type="button" data-qty-action="increase" data-product-id="${productId}">+</button>
            </div>
            <button class="add-to-cart-btn" type="button" data-add-to-cart="${productId}">Add</button>
          </div>
        </div>
      </div>
    `;
  }

  function renderFoodNaturalPageGrid(grid, products) {
    grid.innerHTML = products.map(renderPageCard).join('');

    grid.querySelectorAll('[data-qty-action]').forEach(button => {
      button.addEventListener('click', () => {
        const productId = button.getAttribute('data-product-id');
        const action = button.getAttribute('data-qty-action');
        changeQty(productId, action === 'increase' ? 1 : -1);
      });
    });

    grid.querySelectorAll('[data-add-to-cart]').forEach(button => {
      button.addEventListener('click', () => {
        const productId = button.getAttribute('data-add-to-cart');
        const product = products.find(item => String(item.id) === String(productId));
        const input = document.getElementById(`qty-${productId}`);
        const quantity = Math.max(1, Number(input?.value || 1));
        if (product) {
          addToCart(product, quantity);
        }
      });
    });

    if (window.MRShopCategoryLiveSync && typeof window.MRShopCategoryLiveSync.bindProductCardNavigation === 'function') {
      window.MRShopCategoryLiveSync.bindProductCardNavigation(grid, products);
    }
  }

  function loadCart() {
    try {
      return JSON.parse(localStorage.getItem(CART_STORAGE_KEY)) || [];
    } catch (error) {
      return [];
    }
  }

  function saveCart(cart) {
    localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(cart));
  }

  function changeQty(productId, delta) {
    const input = document.getElementById(`qty-${productId}`);
    if (!input) return;
    input.value = Math.max(1, Number(input.value || 1) + delta);
  }

  function addToCart(product, quantity) {
    const cart = loadCart();
    const existing = cart.find(item => String(item.id) === String(product.id));

    if (existing) {
      existing.quantity += quantity;
    } else {
      cart.push({
        id: product.id,
        name: product.name,
        price: Number(product.final_price ?? product.price ?? 0),
        image: getImageUrl(product),
        quantity
      });
    }

    saveCart(cart);
    window.alert(`${product.name} added to cart!`);
  }

  async function renderFoodNaturalSpotlight() {
    const section = document.getElementById('foodNaturalSpotlight');
    const grid = document.getElementById('foodNaturalGrid');
    if (!section || !grid) return;

    const fallbackProducts = getFallbackFoodProducts();

    try {
      const response = await fetchFoodNaturalSection();
      const payload = response.data || {};
      const sectionData = payload.section || getDefaultFoodNaturalSection();
      const featuredLimit = Math.max(1, Number(sectionData.featuredLimit || 4));
      const liveProducts = Array.isArray(payload.products) ? payload.products : [];
      const products = liveProducts.length > 0
        ? liveProducts
        : fallbackProducts.slice(0, featuredLimit);

      const titleEl = document.getElementById('foodNaturalTitle');
      const subtitleEl = document.getElementById('foodNaturalSubtitle');
      const badgeEl = document.getElementById('foodNaturalBadge');
      const linkEl = document.getElementById('foodNaturalLink');

      if (titleEl && sectionData.title) titleEl.textContent = sectionData.title;
      if (subtitleEl && sectionData.subtitle) subtitleEl.textContent = sectionData.subtitle;
      if (badgeEl && sectionData.badge) badgeEl.textContent = sectionData.badge;
      if (linkEl && sectionData.ctaLabel) linkEl.textContent = sectionData.ctaLabel;

      if (!products.length) {
        grid.innerHTML = '<div class="food-natural-empty">No products found in Food & Natural yet.</div>';
        return;
      }

      grid.innerHTML = products.map(renderCompactCard).join('');
    } catch (error) {
      if (fallbackProducts.length) {
        const fallbackSection = getDefaultFoodNaturalSection();
        const titleEl = document.getElementById('foodNaturalTitle');
        const subtitleEl = document.getElementById('foodNaturalSubtitle');
        const badgeEl = document.getElementById('foodNaturalBadge');
        const linkEl = document.getElementById('foodNaturalLink');

        if (titleEl) titleEl.textContent = fallbackSection.title;
        if (subtitleEl) subtitleEl.textContent = fallbackSection.subtitle;
        if (badgeEl) badgeEl.textContent = fallbackSection.badge;
        if (linkEl) linkEl.textContent = fallbackSection.ctaLabel;

        grid.innerHTML = fallbackProducts.slice(0, fallbackSection.featuredLimit).map(renderCompactCard).join('');
        return;
      }

      grid.innerHTML = '<div class="food-natural-empty">Live catalog unavailable right now. Refresh after starting the API.</div>';
    }
  }

  async function renderFoodNaturalPage() {
    const grid = document.getElementById('productsGrid');
    if (!grid) return;

    const fallbackProducts = getFallbackFoodProducts();

    try {
      const response = await fetchFoodProducts();
      const liveProducts = Array.isArray(response.data) ? response.data : [];
      const products = liveProducts.length > 0 ? liveProducts : fallbackProducts;

      if (!products.length) {
        grid.innerHTML = '<div class="food-natural-empty" style="grid-column:1/-1;">No products found in Food & Natural yet.</div>';
        return;
      }

      renderFoodNaturalPageGrid(grid, products);
    } catch (error) {
      if (fallbackProducts.length) {
        renderFoodNaturalPageGrid(grid, fallbackProducts);
        return;
      }

      grid.innerHTML = '<div class="food-natural-empty" style="grid-column:1/-1;">Unable to load live products right now.</div>';
    }
  }

  function init() {
    renderFoodNaturalSpotlight();
    renderFoodNaturalPage();
  }

  window.MRShopFoodNatural = {
    init,
    renderFoodNaturalSpotlight,
    renderFoodNaturalPage,
    fetchFoodNaturalSection,
    fetchFoodProducts
  };

  document.addEventListener('DOMContentLoaded', init);
})(window, document);
