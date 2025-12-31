// products.js
// Helper to load products from the API and provide lookup by id
(function(window){
  const cache = { products: null };

  async function fetchProducts(){
    if (cache.products) return cache.products;
    try{
      const res = await fetch('/api/products');
      if (!res.ok) throw new Error('Failed to fetch products');
      const data = await res.json();
      cache.products = Array.isArray(data) ? data : [];
      return cache.products;
    }catch(err){
      console.warn('products.js: /api/products fetch failed, will try local fallbacks', err);

      // Try relative data paths (works when opening files directly without a server)
      const candidates = [
        'data/products.json',
        './data/products.json',
        window.location.origin ? (window.location.origin + '/data/products.json') : null
      ].filter(Boolean);

      for (const path of candidates){
        try{
          const r = await fetch(path);
          if (r.ok){
            const d = await r.json();
            cache.products = Array.isArray(d) ? d : [];
            console.info('products.js: loaded products from', path);
            return cache.products;
          }
        }catch(e){
          console.warn('products.js: fetch failed for', path, e);
        }
      }

      // As a last resort, return empty array
      cache.products = [];
      return cache.products;
    }
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
    getProductById
    ,getProductByName
  };
})(window);
