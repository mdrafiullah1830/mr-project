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
      console.warn('products.js: /api/products fetch failed, trying /data/products.json', err);
      try{
        const res2 = await fetch('/data/products.json');
        if (res2.ok){
          const data2 = await res2.json();
          cache.products = Array.isArray(data2) ? data2 : [];
          return cache.products;
        }
      }catch(err2){
        console.warn('products.js: /data/products.json fetch failed', err2);
      }
      cache.products = [];
      return cache.products;
    }
  }

  async function getProductById(id){
    const list = await fetchProducts();
    return list.find(p => String(p.id) === String(id)) || null;
  }

  window.MRShop = window.MRShop || {};
  window.MRShop.Products = {
    fetchProducts,
    getProductById
  };
})(window);
