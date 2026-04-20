<?php
// Enable error reporting
error_reporting(E_ALL);
ini_set('display_errors', 1);

// Set JSON response header
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');
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

function ensureProductsSellerColumn(mysqli $conn): void {
    $result = $conn->query("SHOW COLUMNS FROM products LIKE 'seller'");

    if ($result && $result->num_rows === 0) {
        $conn->query("ALTER TABLE products ADD COLUMN seller VARCHAR(255) NULL AFTER category");
    }
}

ensureProductsSellerColumn($conn);

function ensureProductsImagePathColumn(mysqli $conn): void {
    $result = $conn->query("SHOW COLUMNS FROM products LIKE 'image_path'");

    if (!$result || $result->num_rows === 0) {
        return;
    }

    $column = $result->fetch_assoc();
    $columnType = strtolower((string)($column['Type'] ?? ''));

    if (strpos($columnType, 'longtext') === false) {
        $conn->query("ALTER TABLE products MODIFY COLUMN image_path LONGTEXT NULL AFTER seller");
    }
}

ensureProductsImagePathColumn($conn);

function sendJsonResponse(int $statusCode, array $payload): void {
    http_response_code($statusCode);
    echo json_encode($payload);
    exit();
}

function getRequestPayload(): array {
    $payload = [];

    $rawInput = file_get_contents('php://input');
    if (!empty($rawInput)) {
        $decoded = json_decode($rawInput, true);
        if (is_array($decoded)) {
            $payload = $decoded;
        }
    }

    if (!empty($_POST)) {
        $payload = array_merge($payload, $_POST);
    }

    return $payload;
}

function normalizeCategorySlug($category): string {
    $normalized = strtolower(trim((string) $category));

    $categoryMap = [
        'food & natural' => 'food',
        'food & natural products' => 'food',
        '🍯 food & natural' => 'food',
        '🍯 food & natural products' => 'food',
        'sweets & dairy' => 'sweets',
        '🍰 sweets & dairy' => 'sweets',
        'handicrafts' => 'handicrafts',
        '🎨 handicrafts' => 'handicrafts',
        'clothing' => 'clothing',
        '👗 clothing' => 'clothing',
        'books' => 'books',
        '📚 books' => 'books',
        'antique & collectibles' => 'antique',
        '🪙 antique & collectibles' => 'antique',
        'antiques' => 'antique'
    ];

    if (isset($categoryMap[$normalized])) {
        return $categoryMap[$normalized];
    }

    $slug = preg_replace('/[^a-z0-9]+/', '', $normalized);
    return $slug ?: 'food';
}

function formatProductRow(array $row): array {
    $price = isset($row['price']) ? floatval($row['price']) : 0.0;
    $discount = isset($row['discount']) ? intval($row['discount']) : 0;
    $finalPrice = isset($row['final_price']) && $row['final_price'] !== null
        ? floatval($row['final_price'])
        : max(0, $price - (($price * $discount) / 100));
    $imagePath = $row['image_path'] ?? ($row['image'] ?? '');

    return [
        'id' => intval($row['id']),
        'name' => $row['name'] ?? '',
        'price' => $price,
        'category' => $row['category'] ?? '',
        'seller' => $row['seller'] ?? 'Admin',
        'image' => $imagePath,
        'image_path' => $imagePath,
        'description' => $row['description'] ?? '',
        'discount' => $discount,
        'final_price' => $finalPrice,
        'rating' => isset($row['rating']) ? floatval($row['rating']) : 0.0,
        'stock' => isset($row['stock']) ? intval($row['stock']) : 0,
        'status' => $row['status'] ?? 'active'
    ];
}

function fetchActiveProducts(mysqli $conn, ?string $category = null, ?int $limit = null): array {
    $query = "SELECT * FROM products WHERE status = 'active'";

    if (!empty($category) && $category !== 'all') {
        $safeCategory = $conn->real_escape_string($category);
        $query .= " AND category = '" . $safeCategory . "'";
    }

    $query .= " ORDER BY updated_at DESC, id DESC";

    if ($limit !== null && $limit > 0) {
        $query .= " LIMIT " . intval($limit);
    }

    $result = $conn->query($query);

    if (!$result) {
        throw new Exception('Query failed: ' . $conn->error);
    }

    $products = [];
    while ($row = $result->fetch_assoc()) {
        $products[] = formatProductRow($row);
    }

    return $products;
}

function loadFoodNaturalSectionConfig(): array {
    $configPath = __DIR__ . '/data/private/catalog/food-natural.json';

    if (!file_exists($configPath)) {
        return [
            'section' => [
                'title' => 'Food & Natural',
                'subtitle' => 'Fresh, organic and authentic products updated from the live catalog.',
                'badge' => 'Live updates',
                'ctaLabel' => 'Open full collection',
                'featuredLimit' => 4,
                'category' => 'food'
            ]
        ];
    }

    $raw = file_get_contents($configPath);
    $decoded = json_decode($raw, true);

    if (!is_array($decoded)) {
        return [
            'section' => [
                'title' => 'Food & Natural',
                'subtitle' => 'Fresh, organic and authentic products updated from the live catalog.',
                'badge' => 'Live updates',
                'ctaLabel' => 'Open full collection',
                'featuredLimit' => 4,
                'category' => 'food'
            ]
        ];
    }

    return $decoded;
}

function getNextProductId(mysqli $conn): int {
    $result = $conn->query("SELECT COALESCE(MAX(id), 200) + 1 AS next_id FROM products");
    if (!$result) {
        throw new Exception('Unable to calculate next product ID: ' . $conn->error);
    }

    $row = $result->fetch_assoc();
    return intval($row['next_id'] ?? 201);
}

function fetchProductById(mysqli $conn, int $id): ?array {
    $statement = $conn->prepare('SELECT * FROM products WHERE id = ? LIMIT 1');
    if (!$statement) {
        throw new Exception('Prepare failed: ' . $conn->error);
    }

    $statement->bind_param('i', $id);
    $statement->execute();
    $result = $statement->get_result();
    $row = $result ? $result->fetch_assoc() : null;
    $statement->close();

    return $row ? formatProductRow($row) : null;
}

function buildProductFromPayload(array $payload, ?array $existingProduct = null): array {
    $name = trim((string)($payload['name'] ?? ($existingProduct['name'] ?? '')));
    $description = trim((string)($payload['description'] ?? ($existingProduct['description'] ?? '')));
    $price = isset($payload['price']) ? floatval($payload['price']) : floatval($existingProduct['price'] ?? 0);
    $discount = isset($payload['discount']) ? intval($payload['discount']) : intval($existingProduct['discount'] ?? 0);
    $rating = isset($payload['rating']) ? floatval($payload['rating']) : floatval($existingProduct['rating'] ?? 0);
    $stock = isset($payload['stock']) ? intval($payload['stock']) : intval($existingProduct['stock'] ?? 0);
    $status = trim((string)($payload['status'] ?? ($existingProduct['status'] ?? 'active')));
    $seller = trim((string)($payload['seller'] ?? ($existingProduct['seller'] ?? 'Admin')));
    $category = normalizeCategorySlug($payload['category'] ?? ($existingProduct['category'] ?? 'food'));
    $imagePath = trim((string)($payload['image_path'] ?? ($payload['imageBase64'] ?? ($existingProduct['image_path'] ?? ''))));
    $finalPrice = isset($payload['final_price']) ? floatval($payload['final_price']) : max(0, $price - (($price * $discount) / 100));

    return [
        'name' => $name,
        'description' => $description,
        'price' => $price,
        'discount' => $discount,
        'final_price' => $finalPrice,
        'rating' => $rating,
        'stock' => $stock,
        'status' => in_array($status, ['active', 'inactive', 'discontinued'], true) ? $status : 'active',
        'seller' => $seller,
        'category' => $category,
        'image_path' => $imagePath
    ];
}

// Get request type
$request = $_GET['action'] ?? $_POST['action'] ?? '';

if ($request === 'addProduct') {
    $payload = getRequestPayload();

    if (empty($payload['name'])) {
        sendJsonResponse(400, [
            'success' => false,
            'error' => 'Product name is required'
        ]);
    }

    if (!isset($payload['price']) || floatval($payload['price']) < 0) {
        sendJsonResponse(400, [
            'success' => false,
            'error' => 'Price is required and must be non-negative'
        ]);
    }

    $product = buildProductFromPayload($payload);
    $productId = isset($payload['id']) && intval($payload['id']) > 0 ? intval($payload['id']) : getNextProductId($conn);

    $statement = $conn->prepare('INSERT INTO products (id, name, price, description, category, seller, image_path, discount, final_price, rating, stock, status) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)');
    if (!$statement) {
        sendJsonResponse(500, [
            'success' => false,
            'error' => 'Prepare failed: ' . $conn->error
        ]);
    }

    $statement->bind_param(
        'isdssssiddis',
        $productId,
        $product['name'],
        $product['price'],
        $product['description'],
        $product['category'],
        $product['seller'],
        $product['image_path'],
        $product['discount'],
        $product['final_price'],
        $product['rating'],
        $product['stock'],
        $product['status']
    );

    if (!$statement->execute()) {
        sendJsonResponse(500, [
            'success' => false,
            'error' => 'Insert failed: ' . $statement->error
        ]);
    }

    $statement->close();

    $createdProduct = fetchProductById($conn, $productId);

    sendJsonResponse(201, [
        'success' => true,
        'message' => 'Product added successfully',
        'data' => $createdProduct
    ]);

} elseif ($request === 'updateProduct') {
    $payload = getRequestPayload();
    $productId = isset($payload['id']) ? intval($payload['id']) : 0;

    if ($productId <= 0) {
        sendJsonResponse(400, [
            'success' => false,
            'error' => 'Product ID is required'
        ]);
    }

    $existingProduct = fetchProductById($conn, $productId);
    if (!$existingProduct) {
        sendJsonResponse(404, [
            'success' => false,
            'error' => 'Product not found'
        ]);
    }

    $product = buildProductFromPayload($payload, $existingProduct);

    $statement = $conn->prepare('UPDATE products SET name = ?, price = ?, description = ?, category = ?, seller = ?, image_path = ?, discount = ?, final_price = ?, rating = ?, stock = ?, status = ?, updated_at = CURRENT_TIMESTAMP WHERE id = ?');
    if (!$statement) {
        sendJsonResponse(500, [
            'success' => false,
            'error' => 'Prepare failed: ' . $conn->error
        ]);
    }

    $statement->bind_param(
        'sdssssiddisi',
        $product['name'],
        $product['price'],
        $product['description'],
        $product['category'],
        $product['seller'],
        $product['image_path'],
        $product['discount'],
        $product['final_price'],
        $product['rating'],
        $product['stock'],
        $product['status'],
        $productId
    );

    if (!$statement->execute()) {
        sendJsonResponse(500, [
            'success' => false,
            'error' => 'Update failed: ' . $statement->error
        ]);
    }

    $statement->close();

    $updatedProduct = fetchProductById($conn, $productId);

    sendJsonResponse(200, [
        'success' => true,
        'message' => 'Product updated successfully',
        'data' => $updatedProduct
    ]);

} elseif ($request === 'deleteProduct') {
    $payload = getRequestPayload();
    $productId = isset($payload['id']) ? intval($payload['id']) : 0;

    if ($productId <= 0) {
        sendJsonResponse(400, [
            'success' => false,
            'error' => 'Product ID is required'
        ]);
    }

    $existingProduct = fetchProductById($conn, $productId);
    if (!$existingProduct) {
        sendJsonResponse(404, [
            'success' => false,
            'error' => 'Product not found'
        ]);
    }

    $statement = $conn->prepare('DELETE FROM products WHERE id = ?');
    if (!$statement) {
        sendJsonResponse(500, [
            'success' => false,
            'error' => 'Prepare failed: ' . $conn->error
        ]);
    }

    $statement->bind_param('i', $productId);

    if (!$statement->execute()) {
        sendJsonResponse(500, [
            'success' => false,
            'error' => 'Delete failed: ' . $statement->error
        ]);
    }

    $statement->close();

    sendJsonResponse(200, [
        'success' => true,
        'message' => 'Product deleted successfully',
        'data' => [
            'deletedId' => $productId,
            'deletedProduct' => $existingProduct
        ]
    ]);

} elseif ($request === 'getFoodNaturalSection') {
    $config = loadFoodNaturalSectionConfig();
    $section = $config['section'] ?? [];
    $featuredLimit = isset($_GET['limit']) ? max(1, intval($_GET['limit'])) : intval($section['featuredLimit'] ?? 4);
    $category = $section['category'] ?? 'food';

    try {
        $products = fetchActiveProducts($conn, $category, $featuredLimit);
    } catch (Exception $exception) {
        sendJsonResponse(500, [
            'success' => false,
            'error' => $exception->getMessage()
        ]);
    }

    sendJsonResponse(200, [
        'success' => true,
        'message' => 'Food & Natural section retrieved successfully',
        'data' => [
            'section' => $section,
            'products' => $products
        ],
        'count' => count($products)
    ]);

} elseif ($request === 'getProducts') {
    // Get products from database
    $category = isset($_GET['category']) ? $conn->real_escape_string($_GET['category']) : '';

    try {
        $products = fetchActiveProducts($conn, $category && $category !== 'all' ? $category : null, null);
    } catch (Exception $exception) {
        sendJsonResponse(500, [
            'success' => false,
            'error' => $exception->getMessage()
        ]);
    }

    sendJsonResponse(200, [
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
        $name = $row['name'];
        if (isset($row['slug']) && strtolower((string) $row['slug']) === 'food') {
            $name = 'Food & Natural';
        }

        $categories[] = [
            'id' => intval($row['id']),
            'name' => $name,
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
        $products[] = formatProductRow($row);
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
        'error' => 'Invalid request. Use a supported action such as getProducts, getCategories, getFoodNaturalSection, addProduct, updateProduct, deleteProduct, or searchProducts'
    ]);
}

$conn->close();
?>
