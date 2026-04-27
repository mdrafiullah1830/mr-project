(function (window, document) {
  const API_BASE = 'http://localhost:5010/api.php';
  const ADMIN_PRODUCTS_KEY = 'mrshop_admin_products';
  const GROUPED_PRODUCTS_KEY = 'mrshop_products';
  const SYNC_EVENT_NAME = 'mrshopProductsUpdated';
  const SYNC_CHANNEL_NAME = 'mrshop_products';

  function normalizeCategorySlug(value) {
    const normalized = String(value || '').toLowerCase().trim();
    const compact = normalized.replace(/[^a-z0-9]+/g, '');

    if (compact === 'foodnatural' || compact === 'foodnaturalproducts') {
      return 'food';
    }

    if (compact === 'sweetsdairy') {
      return 'sweets';
    }

    if (compact === 'handicrafts') {
      return 'handicrafts';
    }

    if (compact === 'clothing') {
      return 'clothing';
    }

    if (compact === 'books') {
      return 'books';
    }

    if (compact === 'antiquecollectibles' || compact === 'antiques') {
      return 'antique';
    }

    return compact;
  }

  function escapeHtml(value) {
    return String(value ?? '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function toNumber(value, fallback = 0) {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : fallback;
  }

  function normalizeImageUrl(imageValue) {
    const value = String(imageValue || '').trim();

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

  function normalizeProduct(product, fallbackCategory) {
    const category = normalizeCategorySlug(product?.category || fallbackCategory || '');
    const price = product?.final_price !== undefined && product?.final_price !== null
      ? toNumber(product.final_price, 0)
      : toNumber(product?.price, 0);
    const imageValue = product?.image || product?.image_path || product?.imageBase64 || product?.img || '';

    return {
      id: product?.id,
      name: product?.name || 'Untitled product',
      description: product?.description || product?.desc || '',
      desc: product?.desc || product?.description || '',
      price,
      originalPrice: toNumber(product?.price, price),
      final_price: product?.final_price !== undefined && product?.final_price !== null ? toNumber(product.final_price, price) : price,
      stock: toNumber(product?.stock, 0),
      discount: toNumber(product?.discount, 0),
      rating: toNumber(product?.rating, 0),
      seller: product?.seller || product?.shop_name || 'MR Shop',
      category,
      tag: product?.tag || product?.subcategory || '',
      link: product?.link || '',
      image: normalizeImageUrl(imageValue),
      img: normalizeImageUrl(imageValue),
      image_path: imageValue,
      imageBase64: product?.imageBase64 || product?.image_path || product?.image || imageValue,
      createdAt: product?.createdAt || product?.created_at || new Date().toISOString()
    };
  }

  function dedupeProducts(products) {
    const map = new Map();

    products.forEach(product => {
      if (!product) {
        return;
      }

      const key = product.id !== undefined && product.id !== null
        ? `id:${String(product.id)}`
        : `name:${String(product.name || '').toLowerCase()}|category:${String(product.category || '')}`;

      map.set(key, product);
    });

    return Array.from(map.values());
  }

  function collectGroupedProducts(groupedProducts) {
    if (!groupedProducts || Array.isArray(groupedProducts)) {
      return [];
    }

    return Object.entries(groupedProducts).flatMap(([groupCategory, products]) => {
      if (!Array.isArray(products)) {
        return [];
      }

      return products.map(product => normalizeProduct({ ...product, category: product?.category || groupCategory }, groupCategory));
    });
  }

  function getStoredCategoryProducts(categorySlug) {
    const normalizedCategory = normalizeCategorySlug(categorySlug);
    const flatProducts = readStoredJson(ADMIN_PRODUCTS_KEY, []);
    const groupedProducts = readStoredJson(GROUPED_PRODUCTS_KEY, {});

    const matches = [];

    if (Array.isArray(flatProducts)) {
      matches.push(...flatProducts.filter(product => normalizeCategorySlug(product?.category) === normalizedCategory).map(product => normalizeProduct(product, normalizedCategory)));
    }

    matches.push(...collectGroupedProducts(groupedProducts).filter(product => normalizeCategorySlug(product?.category) === normalizedCategory));

    return dedupeProducts(matches);
  }

  async function fetchCategoryProducts(categorySlug) {
    const url = new URL(API_BASE, window.location.href);
    url.searchParams.set('action', 'getProducts');

    const normalizedCategory = normalizeCategorySlug(categorySlug);
    if (normalizedCategory && normalizedCategory !== 'all') {
      url.searchParams.set('category', normalizedCategory);
    }

    try {
      const response = await fetch(url.toString());
      const data = await response.json();

      if (!response.ok || !data.success || !Array.isArray(data.data)) {
        return [];
      }

      return data.data.map(product => normalizeProduct(product, normalizedCategory));
    } catch (error) {
      return [];
    }
  }

  async function loadCategoryProducts(categorySlug) {
    const normalizedCategory = normalizeCategorySlug(categorySlug);
    const apiProducts = await fetchCategoryProducts(normalizedCategory);

    if (apiProducts.length > 0) {
      return apiProducts;
    }

    return getStoredCategoryProducts(normalizedCategory);
  }

  function mergeProducts(staticProducts, liveProducts, categorySlug) {
    const normalizedCategory = normalizeCategorySlug(categorySlug);
    const combined = [
      ...(Array.isArray(staticProducts) ? staticProducts : []).map(product => normalizeProduct(product, normalizedCategory)),
      ...(Array.isArray(liveProducts) ? liveProducts : []).map(product => normalizeProduct(product, normalizedCategory))
    ];

    return dedupeProducts(combined);
  }

  function openProductDetails(productOrId, productSnapshot = null) {
    const productId = typeof productOrId === 'object' && productOrId !== null
      ? productOrId.id
      : productOrId;

    if (productId === undefined || productId === null || productId === '') {
      return;
    }

    if (productSnapshot && typeof sessionStorage !== 'undefined') {
      try {
        sessionStorage.setItem('mrshop_selected_product', JSON.stringify(productSnapshot));
      } catch (error) {
        console.warn('Failed to cache selected product for detail view:', error);
      }
    }

    window.location.href = `product.html?id=${encodeURIComponent(productId)}`;
  }

  function bindProductCardNavigation(container, products = []) {
    if (!container || container.dataset.productNavigationBound === 'true') {
      return;
    }

    container.dataset.productNavigationBound = 'true';
    container.__productLookup = new Map(
      (Array.isArray(products) ? products : [])
        .filter(product => product && product.id !== undefined && product.id !== null)
        .map(product => [String(product.id), product])
    );

    const shouldIgnoreTarget = target => Boolean(target && target.closest('button, a, input, select, textarea, label, [data-skip-product-navigation="true"]'));

    const handleCardActivation = event => {
      const card = event.target.closest('[data-product-id]');
      if (!card || !container.contains(card) || shouldIgnoreTarget(event.target)) {
        return;
      }

      const productId = card.getAttribute('data-product-id');
      const productSnapshot = container.__productLookup?.get(String(productId)) || null;
      openProductDetails(productId, productSnapshot);
    };

    container.addEventListener('click', handleCardActivation);
    container.addEventListener('keydown', event => {
      if (event.key !== 'Enter' && event.key !== ' ') {
        return;
      }

      const card = event.target.closest('[data-product-id]');
      if (!card || !container.contains(card) || shouldIgnoreTarget(event.target)) {
        return;
      }

      event.preventDefault();
      const productId = card.getAttribute('data-product-id');
      const productSnapshot = container.__productLookup?.get(String(productId)) || null;
      openProductDetails(productId, productSnapshot);
    });
  }

  function watchCategoryUpdates(onUpdate) {
    if (typeof onUpdate !== 'function') {
      return () => {};
    }

    const triggerUpdate = () => {
      try {
        onUpdate();
      } catch (error) {
        console.error('Category sync update failed:', error);
      }
    };

    window.addEventListener(SYNC_EVENT_NAME, triggerUpdate);
    window.addEventListener('storage', (event) => {
      if (event.key === ADMIN_PRODUCTS_KEY || event.key === GROUPED_PRODUCTS_KEY) {
        triggerUpdate();
      }
    });

    if (window.BroadcastChannel) {
      try {
        const channel = new BroadcastChannel(SYNC_CHANNEL_NAME);
        channel.onmessage = (event) => {
          if (event.data && event.data.type === 'productsUpdated') {
            triggerUpdate();
          }
        };
      } catch (error) {
        console.warn('BroadcastChannel is unavailable for category sync:', error);
      }
    }

    return triggerUpdate;
  }

  window.MRShopCategoryLiveSync = {
    escapeHtml,
    fetchCategoryProducts,
    getStoredCategoryProducts,
    loadCategoryProducts,
    mergeProducts,
    normalizeCategorySlug,
    normalizeImageUrl,
    normalizeProduct,
    openProductDetails,
    bindProductCardNavigation,
    toNumber,
    watchCategoryUpdates
  };
})(window, document);