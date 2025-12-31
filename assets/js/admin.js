// Admin Panel JavaScript
const API_BASE_URL = 'http://localhost:5010/api/admin';

// Notification system
function showNotification(message, type = 'success') {
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.textContent = message;
    document.body.appendChild(notification);

    setTimeout(() => {
        notification.remove();
    }, 3000);
}

// Navigation
document.querySelectorAll('.nav-item').forEach(item => {
    item.addEventListener('click', () => {
        const sectionId = item.getAttribute('data-section');
        navigateToSection(sectionId);
    });
});

function navigateToSection(sectionId) {
    // Hide all sections
    document.querySelectorAll('.content-section').forEach(section => {
        section.classList.remove('active');
    });

    // Remove active from nav items
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('active');
    });

    // Show selected section
    const selectedSection = document.getElementById(sectionId);
    if (selectedSection) {
        selectedSection.classList.add('active');
    }

    // Mark nav item as active
    const navItem = document.querySelector(`[data-section="${sectionId}"]`);
    if (navItem) {
        navItem.classList.add('active');
    }

    // Load data for section
    if (sectionId === 'dashboard') {
        loadDashboard();
    } else if (sectionId === 'categories') {
        loadCategories();
    } else if (sectionId === 'products') {
        loadProducts();
    } else if (sectionId === 'orders') {
        loadOrders();
    } else if (sectionId === 'users') {
        loadUsers();
    }
}

// Logout
document.querySelector('.logout-btn')?.addEventListener('click', () => {
    localStorage.removeItem('authToken');
    localStorage.removeItem('adminInfo');
    window.location.href = 'auth.html';
});

// Display admin info
function displayAdminInfo() {
    const adminInfo = JSON.parse(localStorage.getItem('adminInfo') || '{}');
    const usernameElement = document.querySelector('.admin-username');
    if (usernameElement) {
        usernameElement.textContent = adminInfo.username || 'Admin';
    }
}

// Category Form Submission
document.getElementById('categoryForm')?.addEventListener('submit', async (e) => {
    e.preventDefault();

    const formData = new FormData();
    formData.append('name', document.getElementById('categoryName').value);
    formData.append('description', document.getElementById('categoryDescription').value);
    formData.append('display_order', document.getElementById('categoryOrder').value || 0);
    formData.append('status', document.getElementById('categoryStatus').value);

    const imageFile = document.getElementById('categoryImage').files[0];
    if (imageFile) {
        formData.append('image', imageFile);
    }

    try {
        const response = await fetch(`${API_BASE_URL}/categories`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            },
            body: formData
        });

        if (response.ok) {
            showNotification('Category added successfully!', 'success');
            document.getElementById('categoryForm').reset();
            loadCategories();
        } else {
            showNotification('Failed to add category', 'error');
        }
    } catch (error) {
        showNotification('Error adding category: ' + error.message, 'error');
    }
});

// Load Categories
async function loadCategories() {
    try {
        const response = await fetch(`${API_BASE_URL}/categories`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            }
        });

        if (response.ok) {
            const categories = await response.json();
            const tbody = document.getElementById('categoriesTableBody');
            
            if (categories.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" class="no-data">No categories yet</td></tr>';
                return;
            }

            tbody.innerHTML = categories.map(cat => `
                <tr>
                    <td>${cat.name}</td>
                    <td>${cat.description}</td>
                    <td><span class="status-badge ${cat.status}">${cat.status}</span></td>
                    <td>${cat.display_order}</td>
                    <td>
                        <button class="btn btn-text btn-small" onclick="editCategory(${cat.id})">Edit</button>
                        <button class="btn btn-text btn-small" onclick="deleteCategory(${cat.id})">Delete</button>
                    </td>
                </tr>
            `).join('');
        }
    } catch (error) {
        showNotification('Error loading categories: ' + error.message, 'error');
    }
}

// Delete Category
async function deleteCategory(id) {
    if (!confirm('Are you sure you want to delete this category?')) return;

    try {
        const response = await fetch(`${API_BASE_URL}/categories/${id}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            }
        });

        if (response.ok) {
            showNotification('Category deleted successfully!', 'success');
            loadCategories();
        } else {
            showNotification('Failed to delete category', 'error');
        }
    } catch (error) {
        showNotification('Error deleting category: ' + error.message, 'error');
    }
}

// ==================== PRODUCTS ====================

// Product Form Submission
document.getElementById('productForm')?.addEventListener('submit', async (e) => {
    e.preventDefault();

    const formData = new FormData();
    formData.append('name', document.getElementById('productName').value);
    formData.append('category_id', document.getElementById('productCategory').value);
    formData.append('price', document.getElementById('productPrice').value);
    formData.append('stock', document.getElementById('productStock').value);
    formData.append('description', document.getElementById('productDescription').value);
    formData.append('discount', document.getElementById('productDiscount').value || 0);

    const imageFile = document.getElementById('productImage').files[0];
    if (imageFile) {
        formData.append('image', imageFile);
    }

    try {
        const response = await fetch(`${API_BASE_URL}/products`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            },
            body: formData
        });

        if (response.ok) {
            showNotification('Product added successfully!', 'success');
            document.getElementById('productForm').reset();
            loadProducts();
            loadCategories();
        } else {
            showNotification('Failed to add product', 'error');
        }
    } catch (error) {
        showNotification('Error adding product: ' + error.message, 'error');
    }
});

// Load Products
async function loadProducts() {
    try {
        const response = await fetch(`${API_BASE_URL}/products`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            }
        });

        if (response.ok) {
            const products = await response.json();
            const tbody = document.getElementById('productsTableBody');
            
            if (products.length === 0) {
                tbody.innerHTML = '<tr><td colspan="7" class="no-data">No products yet</td></tr>';
                return;
            }

            tbody.innerHTML = products.map(prod => `
                <tr>
                    <td>${prod.name}</td>
                    <td>${prod.category}</td>
                    <td>$${parseFloat(prod.price).toFixed(2)}</td>
                    <td>${prod.stock}</td>
                    <td>${prod.discount}%</td>
                    <td>
                        <button class="btn btn-text btn-small" onclick="deleteProduct(${prod.id})">Delete</button>
                    </td>
                </tr>
            `).join('');
        }
    } catch (error) {
        showNotification('Error loading products: ' + error.message, 'error');
    }
}

// Delete Product
async function deleteProduct(id) {
    if (!confirm('Are you sure you want to delete this product?')) return;

    try {
        const response = await fetch(`${API_BASE_URL}/products/${id}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            }
        });

        if (response.ok) {
            showNotification('Product deleted successfully!', 'success');
            loadProducts();
        } else {
            showNotification('Failed to delete product', 'error');
        }
    } catch (error) {
        showNotification('Error deleting product: ' + error.message, 'error');
    }
}

// ==================== ORDERS ====================

// Load Orders
async function loadOrders() {
    try {
        const response = await fetch(`${API_BASE_URL}/orders`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            }
        });

        if (response.ok) {
            const orders = await response.json();
            const tbody = document.getElementById('ordersTableBody');
            
            if (orders.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" class="no-data">No orders yet</td></tr>';
                return;
            }

            tbody.innerHTML = orders.map(order => `
                <tr>
                    <td>#${order.id}</td>
                    <td>${order.customer_name}</td>
                    <td>$${parseFloat(order.total).toFixed(2)}</td>
                    <td><span class="status-badge ${order.status}">${order.status}</span></td>
                    <td>${new Date(order.created_at).toLocaleDateString()}</td>
                    <td>
                        <button class="btn btn-text btn-small" onclick="viewOrder(${order.id})">View</button>
                    </td>
                </tr>
            `).join('');
        }
    } catch (error) {
        showNotification('Error loading orders: ' + error.message, 'error');
    }
}

// Filter Orders
document.getElementById('orderFilter')?.addEventListener('change', async (e) => {
    const filter = e.target.value;
    try {
        const response = await fetch(`${API_BASE_URL}/orders?status=${filter}`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            }
        });

        if (response.ok) {
            const orders = await response.json();
            const tbody = document.getElementById('ordersTableBody');
            
            if (orders.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" class="no-data">No orders found</td></tr>';
                return;
            }

            tbody.innerHTML = orders.map(order => `
                <tr>
                    <td>#${order.id}</td>
                    <td>${order.customer_name}</td>
                    <td>$${parseFloat(order.total).toFixed(2)}</td>
                    <td><span class="status-badge ${order.status}">${order.status}</span></td>
                    <td>${new Date(order.created_at).toLocaleDateString()}</td>
                    <td>
                        <button class="btn btn-text btn-small" onclick="viewOrder(${order.id})">View</button>
                    </td>
                </tr>
            `).join('');
        }
    } catch (error) {
        showNotification('Error filtering orders: ' + error.message, 'error');
    }
});

// ==================== USERS ====================

// Load Users
async function loadUsers() {
    try {
        const response = await fetch(`${API_BASE_URL}/users`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            }
        });

        if (response.ok) {
            const users = await response.json();
            const tbody = document.getElementById('usersTableBody');
            
            if (users.length === 0) {
                tbody.innerHTML = '<tr><td colspan="5" class="no-data">No users yet</td></tr>';
                return;
            }

            tbody.innerHTML = users.map(user => `
                <tr>
                    <td>${user.username}</td>
                    <td>${user.email}</td>
                    <td>${user.role || 'user'}</td>
                    <td>${new Date(user.created_at).toLocaleDateString()}</td>
                    <td>
                        <button class="btn btn-text btn-small" onclick="viewUser(${user.id})">View</button>
                    </td>
                </tr>
            `).join('');
        }
    } catch (error) {
        showNotification('Error loading users: ' + error.message, 'error');
    }
}

// Search Users
document.getElementById('userSearch')?.addEventListener('input', async (e) => {
    const query = e.target.value;
    if (query.length < 2) {
        loadUsers();
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/users/search?q=${query}`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            }
        });

        if (response.ok) {
            const users = await response.json();
            const tbody = document.getElementById('usersTableBody');
            
            if (users.length === 0) {
                tbody.innerHTML = '<tr><td colspan="5" class="no-data">No users found</td></tr>';
                return;
            }

            tbody.innerHTML = users.map(user => `
                <tr>
                    <td>${user.username}</td>
                    <td>${user.email}</td>
                    <td>${user.role || 'user'}</td>
                    <td>${new Date(user.created_at).toLocaleDateString()}</td>
                    <td>
                        <button class="btn btn-text btn-small" onclick="viewUser(${user.id})">View</button>
                    </td>
                </tr>
            `).join('');
        }
    } catch (error) {
        showNotification('Error searching users: ' + error.message, 'error');
    }
});

// ==================== DASHBOARD ====================

// Load Dashboard
async function loadDashboard() {
    try {
        const [categoriesRes, productsRes, ordersRes, usersRes] = await Promise.all([
            fetch(`${API_BASE_URL}/categories`, {
                headers: { 'Authorization': `Bearer ${localStorage.getItem('authToken')}` }
            }),
            fetch(`${API_BASE_URL}/products`, {
                headers: { 'Authorization': `Bearer ${localStorage.getItem('authToken')}` }
            }),
            fetch(`${API_BASE_URL}/orders`, {
                headers: { 'Authorization': `Bearer ${localStorage.getItem('authToken')}` }
            }),
            fetch(`${API_BASE_URL}/users`, {
                headers: { 'Authorization': `Bearer ${localStorage.getItem('authToken')}` }
            })
        ]);

        const categories = await categoriesRes.json();
        const products = await productsRes.json();
        const orders = await ordersRes.json();
        const users = await usersRes.json();

        // Update stat cards
        document.querySelector('[data-stat="categories"]').textContent = categories.length;
        document.querySelector('[data-stat="products"]').textContent = products.length;
        document.querySelector('[data-stat="orders"]').textContent = orders.length;
        document.querySelector('[data-stat="users"]').textContent = users.length;

        // Calculate totals
        const totalRevenue = orders.reduce((sum, order) => sum + parseFloat(order.total || 0), 0);
        document.querySelector('[data-stat="revenue"]').textContent = `$${totalRevenue.toFixed(2)}`;
    } catch (error) {
        showNotification('Error loading dashboard: ' + error.message, 'error');
    }
}

// ==================== SETTINGS ====================

// Save Site Settings
document.getElementById('siteSettingsForm')?.addEventListener('submit', async (e) => {
    e.preventDefault();

    const settings = {
        site_name: document.getElementById('siteName').value,
        site_description: document.getElementById('siteDescription').value,
        contact_email: document.getElementById('contactEmail').value
    };

    try {
        const response = await fetch(`${API_BASE_URL}/settings`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            },
            body: JSON.stringify(settings)
        });

        if (response.ok) {
            showNotification('Settings saved successfully!', 'success');
        } else {
            showNotification('Failed to save settings', 'error');
        }
    } catch (error) {
        showNotification('Error saving settings: ' + error.message, 'error');
    }
});

// Backup Data
document.getElementById('backupBtn')?.addEventListener('click', async () => {
    try {
        const response = await fetch(`${API_BASE_URL}/backup`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            }
        });

        if (response.ok) {
            const data = await response.json();
            const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `admin-backup-${new Date().toISOString()}.json`;
            a.click();
            showNotification('Backup downloaded successfully!', 'success');
        } else {
            showNotification('Failed to backup data', 'error');
        }
    } catch (error) {
        showNotification('Error backing up data: ' + error.message, 'error');
    }
});

// Clear All Data
document.getElementById('clearDataBtn')?.addEventListener('click', async () => {
    if (!confirm('⚠️ WARNING: This will delete ALL data. Are you absolutely sure?')) return;
    if (!confirm('This action cannot be undone. Type "YES" to confirm.')) return;

    try {
        const response = await fetch(`${API_BASE_URL}/clear-data`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            }
        });

        if (response.ok) {
            showNotification('All data cleared successfully!', 'success');
            setTimeout(() => {
                window.location.reload();
            }, 2000);
        } else {
            showNotification('Failed to clear data', 'error');
        }
    } catch (error) {
        showNotification('Error clearing data: ' + error.message, 'error');
    }
});

// ==================== MODAL MANAGEMENT ====================

// Close Modal
document.querySelector('.modal-close')?.addEventListener('click', () => {
    document.getElementById('editModal').classList.remove('active');
});

// Close modal when clicking outside
window.addEventListener('click', (e) => {
    const modal = document.getElementById('editModal');
    if (e.target === modal) {
        modal.classList.remove('active');
    }
});

// ==================== INITIALIZATION ====================

document.addEventListener('DOMContentLoaded', () => {
    // Check if user is logged in
    const authToken = localStorage.getItem('authToken');
    if (!authToken) {
        window.location.href = 'auth.html';
        return;
    }

    displayAdminInfo();

    // Load dashboard on page load
    navigateToSection('dashboard');

    // Load categories for product form
    loadCategories();
});