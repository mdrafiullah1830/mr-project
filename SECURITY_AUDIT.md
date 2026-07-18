# MR Shop - Security Audit Report

**Date:** July 12, 2026
**Severity Scale:** CRITICAL > HIGH > MEDIUM > LOW > INFO

---

## CRITICAL Issues

### 1. Hardcoded Admin Credentials in Client-Side Code
- **File:** `assets/js/auth-shared.js:67-68`
- **Credentials:** `admin@mrmart18.com` / `mrmart18.bd`
- **Impact:** Anyone viewing page source can login as admin
- **Fix:** Remove from client code, use API-only authentication

### 2. Production MongoDB Credentials in Git
- **File:** `MRShop.API/appsettings.json`
- **Impact:** Database accessible to anyone with repo access
- **Fix:** Rotate credentials immediately, use environment variables only

### 3. JWT Secret Key in Git
- **File:** `MRShop.API/appsettings.json`
- **Impact:** Can forge JWT tokens for any user
- **Fix:** Rotate key immediately, use environment variables only

---

## HIGH Issues

### 4. Weak Password Hashing
- **File:** `MRShop.API/Controllers/AuthController.cs:189-193`
- **Algorithm:** SHA-256 with static salt `"MRShop_Salt_2024"`
- **Impact:** Vulnerable to brute-force and rainbow table attacks
- **Fix:** Use BCrypt or Argon2 with per-user random salts

### 5. SSL Validation Disabled for MongoDB
- **File:** `MRShop.API/Services/MongoDbService.cs:28`
- **Code:** `ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true`
- **Impact:** Man-in-the-middle attacks on database connection
- **Fix:** Remove callback, enable proper TLS validation

### 6. Swagger Exposed in Production
- **File:** `MRShop.API/Program.cs:131`
- **Impact:** Full API documentation publicly accessible
- **Fix:** Gate Swagger behind `IsDevelopment()` check

### 7. No Rate Limiting on Auth Endpoints
- **File:** `MRShop.API/Controllers/AuthController.cs`
- **Impact:** Brute-force attacks on login/register
- **Fix:** Add rate limiting middleware

### 8. Cart Prices from Client
- **File:** `MRShop.API/Controllers/CartController.cs`
- **Impact:** Price manipulation possible
- **Fix:** Validate prices server-side against product collection

### 9. IDOR on Product Deletion
- **File:** `MRShop.API/Controllers/ProductsController.cs`
- **Impact:** Any seller can delete any other seller's products
- **Fix:** Verify product ownership before deletion

---

## MEDIUM Issues

### 10. No MongoDB Indexes
- **Impact:** Full collection scans as data grows
- **Fix:** Add indexes on Email, UserId, ProductId

### 11. Unescaped Regex in Product Search
- **File:** `MRShop.API/Controllers/ProductsController.cs:41`
- **Impact:** ReDoS vulnerability
- **Fix:** Escape regex special characters

### 12. No Global Exception Handling
- **Impact:** Raw 500 errors with stack traces
- **Fix:** Add global exception middleware

### 13. CORS Allows Localhost in Production
- **File:** `MRShop.API/Program.cs:59-66`
- **Impact:** Reduced security posture
- **Fix:** Remove localhost from production CORS policy

### 14. Static Files from Parent Directory
- **File:** `MRShop.API/Program.cs:123-128`
- **Impact:** May expose unintended files
- **Fix:** Use specific path instead of parent directory

### 15. Rate Limiting Memory Leak
- **File:** `backend/chat_server.py`
- **Impact:** Unbounded memory growth with many IPs
- **Fix:** Add periodic cleanup of rate limit data

---

## LOW Issues

### 16. No Product Soft-Delete
- **Impact:** Order history integrity compromised
- **Fix:** Set IsActive = false instead of hard delete

### 17. No Pagination Limit Cap
- **Impact:** Memory exhaustion with limit=1000000
- **Fix:** Cap limit parameter

### 18. Health Endpoint Doesn't Check MongoDB
- **Impact:** False healthy status when DB is down
- **Fix:** Add MongoDB ping to health check

### 19. No Password Complexity Requirements
- **Impact:** Users can set empty/single-character passwords
- **Fix:** Add minimum password policy

### 20. Duplicate Social Login Definitions
- **Files:** `auth-shared.js` and `auth.js`
- **Impact:** Function overwrites
- **Fix:** Remove duplicate definitions

---

## Recommendations Summary

| Priority | Action | Effort |
|----------|--------|--------|
| 1 | Rotate all exposed secrets | 1 hour |
| 2 | Implement BCrypt hashing | 2 hours |
| 3 | Remove hardcoded admin credentials | 1 hour |
| 4 | Add MongoDB indexes | 1 hour |
| 5 | Enable SSL validation | 30 min |
| 6 | Add rate limiting on auth | 2 hours |
| 7 | Server-side price validation | 3 hours |
| 8 | Add global error handling | 2 hours |
| 9 | Fix CORS policy | 30 min |
| 10 | Add input validation | 4 hours |
