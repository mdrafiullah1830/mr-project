// products.js
// Helper to load products from the live backend and provide lookup by id
(function(window){
  const cache = { products: null };
  const API_CANDIDATES = [
    'http://localhost:5010/api.php?action=getProducts',
    '/api.php?action=getProducts',
    '/api/products'
  ];
  const JSON_FALLBACKS = [
    new URL('../../data/products.json', window.location.href).toString(),
    new URL('/data/products.json', window.location.href).toString()
  ].filter(Boolean);

  function normalizeProductList(payload){
    if (Array.isArray(payload)) {
      return payload;
    }

    if (payload && Array.isArray(payload.data)) {
      return payload.data;
    }

    if (payload && Array.isArray(payload.products)) {
      return payload.products;
    }

    return [];
  }

  function readStoredProducts(){
    const storedKeys = ['mrshop_admin_products', 'mrshop_products'];
    const collected = [];

    for (const key of storedKeys) {
      try {
        const rawValue = localStorage.getItem(key);
        if (!rawValue) {
          continue;
        }

        const parsedValue = JSON.parse(rawValue);
        if (Array.isArray(parsedValue)) {
          collected.push(...parsedValue);
        } else if (parsedValue && Array.isArray(parsedValue.products)) {
          collected.push(...parsedValue.products);
        } else if (parsedValue && typeof parsedValue === 'object') {
          Object.values(parsedValue).forEach(value => {
            if (Array.isArray(value)) {
              collected.push(...value);
            }
          });
        }
      } catch (error) {
        console.warn('products.js: failed to read stored products from', key, error);
      }
    }

    return collected;
  }

  function mergeProducts(primary, secondary){
    const merged = [];
    const seen = new Set();

    [...primary, ...secondary].forEach(product => {
      if (!product) {
        return;
      }

      const productId = product.id !== undefined && product.id !== null ? String(product.id) : '';
      const productName = String(product.name || '').trim().toLowerCase();
      const key = productId ? `id:${productId}` : `name:${productName}`;

      if (seen.has(key)) {
        return;
      }

      seen.add(key);
      merged.push(product);
    });

    return merged;
  }

  async function fetchJsonFromCandidates(urls){
    for (const url of urls){
      try{
        const response = await fetch(url);
        if (!response.ok) {
          continue;
        }

        const payload = await response.json();
        const products = normalizeProductList(payload);
        if (products.length > 0) {
          return products;
        }
      }catch(error){
        console.warn('products.js: fetch failed for', url, error);
      }
    }

    return [];
  }

  async function fetchProducts(){
    if (cache.products) return cache.products;

    const remoteProducts = await fetchJsonFromCandidates(API_CANDIDATES);
    const fallbackProducts = remoteProducts.length > 0 ? [] : await fetchJsonFromCandidates(JSON_FALLBACKS);
    const storedProducts = readStoredProducts();

    cache.products = mergeProducts(
      remoteProducts.length > 0 ? remoteProducts : fallbackProducts,
      storedProducts
    );

    return cache.products;
  }

  async function getProductById(id){
    const list = await fetchProducts();
    return list.find(p => String(p.id) === String(id)) || null;
  }

  async function getProductByName(name){
    if(!name) return null;
    const list = await fetchProducts();
    const lower = name.trim().toLowerCase();
    // Exact match first
    let found = list.find(p => (p.name || '').trim().toLowerCase() === lower);
    if(found) return found;
    // Partial match
    found = list.find(p => (p.name || '').trim().toLowerCase().includes(lower));
    return found || null;
  }

  window.MRShop = window.MRShop || {};
  window.MRShop.Products = {
    fetchProducts,
    getProductById,
    getProductByName
  };
})(window);
