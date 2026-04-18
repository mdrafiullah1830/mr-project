-- =====================================================
-- MR SHOP DATABASE SETUP SCRIPT
-- Database Name: mr_shop
-- Created for XAMPP MySQL
-- =====================================================

-- CREATE DATABASE
CREATE DATABASE IF NOT EXISTS mr_shop;
USE mr_shop;

-- DISABLE FOREIGN KEY CHECKS TEMPORARILY
SET FOREIGN_KEY_CHECKS=0;

-- =====================================================
-- TABLE 1: users
-- =====================================================
CREATE TABLE IF NOT EXISTS users (
  id INT PRIMARY KEY AUTO_INCREMENT,
  username VARCHAR(100) NOT NULL UNIQUE,
  email VARCHAR(100) NOT NULL UNIQUE,
  password_hash VARCHAR(255) NOT NULL,
  role ENUM('user', 'admin', 'seller') DEFAULT 'user',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  INDEX idx_username (username),
  INDEX idx_email (email),
  INDEX idx_role (role)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- TABLE 2: categories
-- =====================================================
CREATE TABLE IF NOT EXISTS categories (
  id INT PRIMARY KEY AUTO_INCREMENT,
  name VARCHAR(100) NOT NULL UNIQUE,
  slug VARCHAR(100),
  description TEXT,
  icon VARCHAR(255),
  visibility ENUM('public', 'private') DEFAULT 'public',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_slug (slug)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- TABLE 3: products
-- =====================================================
CREATE TABLE IF NOT EXISTS products (
  id INT PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  price DECIMAL(10, 2) NOT NULL,
  description TEXT,
  category VARCHAR(100),
  image_path VARCHAR(500),
  discount INT DEFAULT 0,
  final_price DECIMAL(10, 2),
  rating DECIMAL(3, 2),
  stock INT DEFAULT 0,
  status ENUM('active', 'inactive', 'discontinued') DEFAULT 'active',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  INDEX idx_category (category),
  INDEX idx_status (status),
  INDEX idx_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- TABLE 4: orders
-- =====================================================
CREATE TABLE IF NOT EXISTS orders (
  id VARCHAR(50) PRIMARY KEY,
  user_id INT,
  user_name VARCHAR(255),
  total DECIMAL(12, 2),
  payment_method VARCHAR(50),
  shipping_address JSON,
  status ENUM('pending', 'confirmed', 'packed', 'shipped', 'delivered', 'cancelled') DEFAULT 'pending',
  tracking_id VARCHAR(100),
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  INDEX idx_user_id (user_id),
  INDEX idx_status (status),
  INDEX idx_tracking_id (tracking_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- TABLE 5: order_items
-- =====================================================
CREATE TABLE IF NOT EXISTS order_items (
  id INT PRIMARY KEY AUTO_INCREMENT,
  order_id VARCHAR(50) NOT NULL,
  product_id INT,
  product_name VARCHAR(255),
  category VARCHAR(100),
  quantity INT NOT NULL,
  price DECIMAL(10, 2) NOT NULL,
  image VARCHAR(500),
  INDEX idx_order_id (order_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- TABLE 6: user_profiles
-- =====================================================
CREATE TABLE IF NOT EXISTS user_profiles (
  id INT PRIMARY KEY AUTO_INCREMENT,
  user_id INT UNIQUE NOT NULL,
  first_name VARCHAR(100),
  last_name VARCHAR(100),
  phone VARCHAR(20),
  address TEXT,
  city VARCHAR(50),
  country VARCHAR(50),
  bio TEXT,
  profile_image VARCHAR(500),
  date_of_birth DATE,
  gender VARCHAR(20),
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- TABLE 7: admin (Admin Credentials & Settings)
-- =====================================================
CREATE TABLE IF NOT EXISTS admin (
  id INT PRIMARY KEY AUTO_INCREMENT,
  username VARCHAR(100) NOT NULL UNIQUE,
  password_hash TEXT NOT NULL,
  recovery_question TEXT,
  recovery_hint TEXT,
  recovery_answer_hash TEXT,
  last_password_reset TIMESTAMP,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- TABLE 8: sellers
-- =====================================================
CREATE TABLE IF NOT EXISTS sellers (
  id INT PRIMARY KEY AUTO_INCREMENT,
  user_id INT UNIQUE,
  shop_name VARCHAR(255) NOT NULL,
  shop_description TEXT,
  phone VARCHAR(20),
  email VARCHAR(100),
  address TEXT,
  city VARCHAR(50),
  bank_account VARCHAR(100),
  status ENUM('pending', 'approved', 'rejected', 'active') DEFAULT 'pending',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  INDEX idx_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- TABLE 9: seller_requests
-- =====================================================
CREATE TABLE IF NOT EXISTS seller_requests (
  id INT PRIMARY KEY AUTO_INCREMENT,
  user_id INT,
  shop_name VARCHAR(255) NOT NULL,
  shop_description TEXT,
  nid_number VARCHAR(50),
  business_license VARCHAR(100),
  bank_account VARCHAR(100),
  phone VARCHAR(20),
  email VARCHAR(100),
  address TEXT,
  status ENUM('pending', 'approved', 'rejected') DEFAULT 'pending',
  rejection_reason TEXT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  INDEX idx_status (status),
  INDEX idx_user_id (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- TABLE 10: donations
-- =====================================================
CREATE TABLE IF NOT EXISTS donations (
  id INT PRIMARY KEY AUTO_INCREMENT,
  donor_name VARCHAR(255),
  donor_email VARCHAR(100),
  amount DECIMAL(10, 2),
  message TEXT,
  status ENUM('pending', 'completed', 'failed') DEFAULT 'pending',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- TABLE 11: security_log
-- =====================================================
CREATE TABLE IF NOT EXISTS security_log (
  id INT PRIMARY KEY AUTO_INCREMENT,
  user_id INT,
  action VARCHAR(100),
  ip_address VARCHAR(45),
  user_agent VARCHAR(500),
  timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  details JSON,
  INDEX idx_user_id (user_id),
  INDEX idx_timestamp (timestamp)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- TABLE 12: settings
-- =====================================================
CREATE TABLE IF NOT EXISTS settings (
  id INT PRIMARY KEY AUTO_INCREMENT,
  setting_key VARCHAR(100) NOT NULL UNIQUE,
  setting_value TEXT,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- INSERT SAMPLE DATA
-- =====================================================

-- Insert Users
INSERT INTO users (id, username, email, password_hash, role, created_at, updated_at) VALUES
(1, 'testuser', 'test@test.com', 'F/j5AWCvsxjQPYA4wI0OaeSrBy96uApcQH0m4ic/uMzyt4R2', 'user', '2025-12-08T00:15:48.391043Z', '2025-12-08T00:16:12.691807Z'),
(2, 'rafi1830', 'ullahmdrafi295@gmail.com', 'MWrtLIPVn8hCnLEUfGX4EiHxjGF46ewj7G0jklslIDMpNpIv', 'user', '2025-12-08T00:19:07.005222Z', '2025-12-08T00:19:07.005223Z'),
(3, 'resettest', 'resettest@test.com', 'laWWmaem5xqhWp7kaJjqhJdztXBlzA31PUa/Ty4GdsBwEOgn', 'user', '2025-12-08T00:29:56.591706Z', '2025-12-08T00:30:08.78187Z'),
(4, 'tanisha1213', 'tanishahossen@gmail.com', 'NfonTN5NqqHK/3kZykAHv9eieziMH3zhZTDhTtliTu5/XCJJ', 'user', '2025-12-08T02:24:04.005647Z', '2025-12-08T02:24:04.005647Z'),
(5, 'mrshop', 'admin@mrshop.com', 'F/OrAL/yO1ipsAF41G1EJSK9Vjt9PfOx3M40A55z3mq8HcFM', 'admin', '2025-12-08T09:50:39.254718Z', '2025-12-08T09:50:39.254718Z');

-- Insert Categories
INSERT INTO categories (name, slug, visibility, created_at) VALUES
('handicrafts', 'handicrafts', 'public', '2025-12-06T15:34:07.308142Z'),
('food', 'food', 'public', '2025-12-06T15:34:07.308152Z'),
('sweets', 'sweets', 'public', '2025-12-06T15:34:07.308157Z'),
('clothing', 'clothing', 'public', '2025-12-06T15:34:07.308161Z'),
('books', 'books', 'public', '2025-12-06T15:34:07.308164Z');

-- Insert Products
INSERT INTO products (id, name, price, description, category, image_path, discount, final_price, rating, stock, status) VALUES
(201, 'Smartphone X10 Pro', 15999, 'Latest smartphone with AI camera and 5G connectivity', 'electronics', 'https://i.ibb.co/S6qMxwr/phone.jpg', 10, 14399, 4.5, 25, 'active'),
(202, 'Sports Running Shoes', 2499, 'Comfortable running shoes with premium sole', 'clothing', 'https://i.ibb.co/fqzGyvY/shoes.jpg', 15, 2124, 4.8, 50, 'active'),
(203, 'Digital Watch Pro', 1299, 'Smartwatch with fitness tracking and heart rate monitor', 'electronics', 'https://i.ibb.co/wwzCzKR/watch.jpg', 20, 1039, 4.3, 15, 'active'),
(204, 'Wireless Bluetooth Headphones', 3999, 'Premium noise-cancelling headphones with 30-hour battery', 'electronics', 'https://i.ibb.co/dM2Hv5S/headphones.jpg', 25, 2999, 4.7, 30, 'active');

-- Insert Admin
INSERT INTO admin (username, password_hash, recovery_question, recovery_hint, recovery_answer_hash, last_password_reset) VALUES
('mrshop18', 'scrypt:32768:8:1$b2RkxOKoQTHj5S3Y$2373eb5cf27347be343c68019d711d7744a20126ddc490cb3595f26c3ea286e51a0e9b1b36c95e4548f2ad42f343f94d2a0ff28e97b558a917355dfc9783ca48', 'What is the emergency access code you saved during setup?', 'It''s printed on your secure setup card.', 'scrypt:32768:8:1$LcSHBUJlz5oPCcbP$1169a47998b404376c8fcf5e5fc3df82192e5317e9a56bac55b7ee3a48c62b4776e585e76be253c78c3eb5c7f18a619098aa997fddb23ad4fe41c44a76e10638', '2025-12-06T16:36:13.839157Z');

-- RE-ENABLE FOREIGN KEY CHECKS
SET FOREIGN_KEY_CHECKS=1;

-- =====================================================
-- ✅ DATABASE SETUP COMPLETE
-- =====================================================
