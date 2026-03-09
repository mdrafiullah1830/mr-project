/**
 * Admin Panel API Integration
 * Connects adminrafi.html to C# backend API
 * Handles all product operations
 */

class AdminAPIClient {
    constructor(baseUrl = 'http://localhost:5001') {
        this.baseUrl = baseUrl;
        this.apiEndpoint = `${baseUrl}/api/admin`;
    }

    /**
     * Make API request with error handling
     */
    async request(method, endpoint, data = null) {
        try {
            const url = `${this.apiEndpoint}${endpoint}`;
            const options = {
                method: method,
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                }
            };

            if (data && (method === 'POST' || method === 'PUT')) {
                options.body = JSON.stringify(data);
            }

            const response = await fetch(url, options);
            const json = await response.json();

            if (!response.ok) {
                throw new Error(json.message || `API Error: ${response.status}`);
            }

            return json;
        } catch (error) {
            console.error(`API Error (${method} ${endpoint}):`, error);
            throw error;
        }
    }

    /**
     * Get all categories
     */
    async getCategories() {
        return this.request('GET', '/categories');
    }

    /**
     * Get all products
     */
    async getAllProducts() {
        return this.request('GET', '/products');
    }

    /**
     * Get products by category
     */
    async getProductsByCategory(category) {
        return this.request('GET', `/products?category=${encodeURIComponent(category)}`);
    }

    /**
     * Get specific product
     */
    async getProduct(productId) {
        return this.request('GET', `/products/${productId}`);
    }

    /**
     * Add new product
     */
    async addProduct(product) {
        return this.request('POST', '/products', product);
    }

    /**
     * Update product
     */
    async updateProduct(productId, product) {
        return this.request('PUT', `/products/${productId}`, product);
    }

    /**
     * Delete product
     */
    async deleteProduct(productId) {
        return this.request('DELETE', `/products/${productId}`);
    }

    /**
     * Search products
     */
    async searchProducts(query) {
        return this.request('GET', `/search?query=${encodeURIComponent(query)}`);
    }

    /**
     * Get dashboard statistics
     */
    async getStatistics() {
        return this.request('GET', '/statistics');
    }

    /**
     * Get low stock products
     */
    async getLowStockProducts(threshold = 10) {
        return this.request('GET', `/low-stock?threshold=${threshold}`);
    }

    /**
     * Sync products
     */
    async syncProducts() {
        return this.request('POST', '/sync');
    }

    /**
     * Health check
     */
    async healthCheck() {
        return this.request('GET', '/health');
    }
}

/**
 * Admin Panel Manager
 * Manages UI and integrates with API client
 */
class AdminPanelManager {
    constructor(apiBaseUrl = 'http://localhost:5001') {
        this.apiClient = new AdminAPIClient(apiBaseUrl);
        this.selectedCategory = null;
        this.allProducts = {};
        this.init();
    }

    async init() {
        console.log('🔧 Initializing Admin Panel...');
        
        // Check API connection
        await this.checkAPIConnection();
        
        // Load categories from API
        await this.loadCategoriesFromAPI();
        
        // Load products from API
        await this.loadProductsFromAPI();
        
        console.log('✅ Admin Panel initialized successfully');
    }

    /**
     * Check if API is available
     */
    async checkAPIConnection() {
        try {
            const health = await this.apiClient.healthCheck();
            console.log('✅ API is online:', health);
            return true;
        } catch (error) {
            console.warn('⚠️ API not available, using LocalStorage fallback');
            return false;
        }
    }

    /**
     * Load categories from API
     */
    async loadCategoriesFromAPI() {
        try {
            const response = await this.apiClient.getCategories();
            console.log('Categories loaded from API:', response.data);
            return response.data;
        } catch (error) {
            console.log('Using local category list');
            return null;
        }
    }

    /**
     * Load products from API
     */
    async loadProductsFromAPI() {
        try {
            const response = await this.apiClient.getAllProducts();
            this.allProducts = response.data;
            console.log('Products loaded from API:', response.count, 'products');
            return response.data;
        } catch (error) {
            console.log('Loading products from LocalStorage');
            return this.loadProductsFromLocalStorage();
        }
    }

    /**
     * Fallback: Load from LocalStorage
     */
    loadProductsFromLocalStorage() {
        const stored = localStorage.getItem('mrshop_products');
        if (stored) {
            this.allProducts = JSON.parse(stored);
            console.log('Loaded', Object.values(this.allProducts).flat().length, 'products from LocalStorage');
        }
        return this.allProducts;
    }

    /**
     * Add product to API
     */
    async addProductToAPI(product) {
        try {
            const response = await this.apiClient.addProduct(product);
            console.log('✅ Product added via API:', response.data);
            
            // Also save to LocalStorage as backup
            this.addProductToLocalStorage(product);
            
            return response.data;
        } catch (error) {
            console.error('API add failed, saving to LocalStorage');
            return this.addProductToLocalStorage(product);
        }
    }

    /**
     * Fallback: Add to LocalStorage
     */
    addProductToLocalStorage(product) {
        const category = product.category;
        if (!this.allProducts[category]) {
            this.allProducts[category] = [];
        }
        this.allProducts[category].push(product);
        localStorage.setItem('mrshop_products', JSON.stringify(this.allProducts));
        return product;
    }

    /**
     * Update product in API
     */
    async updateProductInAPI(productId, product) {
        try {
            const response = await this.apiClient.updateProduct(productId, product);
            console.log('✅ Product updated via API:', response.data);
            return response.data;
        } catch (error) {
            console.error('API update failed:', error);
            throw error;
        }
    }

    /**
     * Delete product from API
     */
    async deleteProductFromAPI(productId) {
        try {
            const response = await this.apiClient.deleteProduct(productId);
            console.log('✅ Product deleted via API:', response.data);
            return response.data;
        } catch (error) {
            console.error('API delete failed:', error);
            throw error;
        }
    }

    /**
     * Get dashboard statistics
     */
    async getDashboardStats() {
        try {
            const response = await this.apiClient.getStatistics();
            console.log('📊 Dashboard Statistics:', response.data);
            return response.data;
        } catch (error) {
            console.error('Failed to get statistics:', error);
            return null;
        }
    }

    /**
     * Sync all changes
     */
    async syncAll() {
        try {
            const response = await this.apiClient.syncProducts();
            console.log('✅ All products synced:', response);
            return response;
        } catch (error) {
            console.error('Sync failed:', error);
            return null;
        }
    }
}

/**
 * Export for use in adminrafi.html
 */
if (typeof window !== 'undefined') {
    window.AdminAPIClient = AdminAPIClient;
    window.AdminPanelManager = AdminPanelManager;
    
    // Auto-initialize if needed
    // const adminPanel = new AdminPanelManager();
}

// For Node.js/CommonJS
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { AdminAPIClient, AdminPanelManager };
}
