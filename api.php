<?php
// Enable error reporting
error_reporting(E_ALL);
ini_set('display_errors', 1);

// Set JSON response header
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type');

// Database credentials
$DB_HOST = 'localhost';
$DB_USER = 'root';
$DB_PASS = '';
$DB_NAME = 'mr_shop';

// Create connection
$conn = new mysqli($DB_HOST, $DB_USER, $DB_PASS, $DB_NAME);

// Check connection
if ($conn->connect_error) {
    http_response_code(500);
    echo json_encode([
        'success' => false,
        'error' => 'Database connection failed: ' . $conn->connect_error
    ]);
    exit();
}

// Set charset
$conn->set_charset('utf8mb4');

// Get request type
$request = isset($_GET['action']) ? $_GET['action'] : '';

if ($request === 'getProducts') {
    // Get products from database
    $category = isset($_GET['category']) ? $conn->real_escape_string($_GET['category']) : '';
    
    // Build query
    if ($category && $category !== 'all') {
        $query = "SELECT * FROM products WHERE category = '$category' AND status = 'active' ORDER BY id DESC";
    } else {
        $query = "SELECT * FROM products WHERE status = 'active' ORDER BY id DESC";
    }
    
    $result = $conn->query($query);
    
    if (!$result) {
        http_response_code(500);
        echo json_encode([
            'success' => false,
            'error' => 'Query failed: ' . $conn->error
        ]);
        exit();
    }
    
    $products = [];
    while ($row = $result->fetch_assoc()) {
        $products[] = [
            'id' => intval($row['id']),
            'name' => $row['name'],
            'price' => floatval($row['price']),
            'category' => $row['category'],
            'image' => $row['image_path'],
            'description' => $row['description'],
            'discount' => intval($row['discount']),
            'final_price' => floatval($row['final_price']),
            'rating' => floatval($row['rating']),
            'stock' => intval($row['stock']),
            'status' => $row['status']
        ];
    }
    
    echo json_encode([
        'success' => true,
        'data' => $products,
        'count' => count($products)
    ]);
    
} elseif ($request === 'getCategories') {
    // Get all categories
    $query = "SELECT id, name, slug, visibility FROM categories WHERE visibility = 'public' ORDER BY id ASC";
    $result = $conn->query($query);
    
    if (!$result) {
        http_response_code(500);
        echo json_encode([
            'success' => false,
            'error' => 'Query failed: ' . $conn->error
        ]);
        exit();
    }
    
    $categories = [];
    while ($row = $result->fetch_assoc()) {
        $categories[] = [
            'id' => intval($row['id']),
            'name' => $row['name'],
            'slug' => $row['slug']
        ];
    }
    
    echo json_encode([
        'success' => true,
        'data' => $categories,
        'count' => count($categories)
    ]);
    
} elseif ($request === 'searchProducts') {
    // Search products
    $search = isset($_GET['search']) ? $conn->real_escape_string($_GET['search']) : '';
    
    if (strlen($search) < 2) {
        echo json_encode([
            'success' => false,
            'error' => 'Search term must be at least 2 characters'
        ]);
        exit();
    }
    
    $query = "SELECT * FROM products WHERE (name LIKE '%$search%' OR description LIKE '%$search%') AND status = 'active' LIMIT 20";
    $result = $conn->query($query);
    
    if (!$result) {
        http_response_code(500);
        echo json_encode([
            'success' => false,
            'error' => 'Query failed: ' . $conn->error
        ]);
        exit();
    }
    
    $products = [];
    while ($row = $result->fetch_assoc()) {
        $products[] = [
            'id' => intval($row['id']),
            'name' => $row['name'],
            'price' => floatval($row['price']),
            'category' => $row['category'],
            'image' => $row['image_path']
        ];
    }
    
    echo json_encode([
        'success' => true,
        'data' => $products,
        'count' => count($products)
    ]);
    
} else {
    http_response_code(400);
    echo json_encode([
        'success' => false,
        'error' => 'Invalid request. Use action=getProducts or getCategories'
    ]);
}

$conn->close();
?>
