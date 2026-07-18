// ==================== MR SHOP - SHARED API MODULE ====================
// Centralized API client with auth token handling

const MR_API = {
  BASE_URL: window.location.hostname === 'localhost' ? 'http://localhost:5000/api' : 'https://mrshop-bd.azurewebsites.net/api',

  // Get auth token from localStorage
  getToken() {
    return localStorage.getItem('mr_shop_token');
  },

  // Check if user is logged in
  isLoggedIn() {
    return !!this.getToken();
  },

  // Make authenticated API request
  async request(endpoint, options = {}) {
    const url = `${this.BASE_URL}${endpoint}`;
    const token = this.getToken();

    const headers = {
      'Content-Type': 'application/json',
      ...options.headers
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    try {
      const response = await fetch(url, { ...options, headers });

      if (response.status === 401) {
        // Token expired or invalid
        localStorage.removeItem('mr_shop_token');
        localStorage.removeItem('mr_shop_user');
        return null;
      }

      const data = await response.json();
      return { ok: response.ok, status: response.status, data };
    } catch (err) {
      console.error(`API Error [${endpoint}]:`, err);
      return null;
    }
  },

  // GET request
  async get(endpoint) {
    return this.request(endpoint, { method: 'GET' });
  },

  // POST request
  async post(endpoint, body) {
    return this.request(endpoint, {
      method: 'POST',
      body: JSON.stringify(body)
    });
  },

  // PUT request
  async put(endpoint, body) {
    return this.request(endpoint, {
      method: 'PUT',
      body: JSON.stringify(body)
    });
  },

  // DELETE request
  async delete(endpoint) {
    return this.request(endpoint, { method: 'DELETE' });
  }
};
