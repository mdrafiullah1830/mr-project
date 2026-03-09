# MR Shop Search System Documentation

## Overview
Complete product search system with:
- **C# Backend Search Service** - Intelligent product search with relevance scoring
- **JSON Data Storage** - All products stored in `/data/products.json`
- **Web API** - RESTful API endpoints for search operations
- **JavaScript Frontend** - Real-time search with online/offline fallback
- **Automatic Updates** - Data automatically syncs when products change

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   Frontend (HTML/JS)                     │
│              index.html + search-api.js                  │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────┐
│              Web API (SearchController)                  │
│  GET /api/search/search?query=...&category=...         │
│  GET /api/search/category/{category}                    │
│  GET /api/search/categories                             │
│  GET /api/search/product/{id}                           │
│  GET /api/search/all                                    │
│  POST /api/search/reload                                │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────┐
│            C# Search Service (SearchService)             │
│  - Product Loading                                      │
│  - Relevance Scoring                                    │
│  - Category Filtering                                   │
│  - Full-Text Search                                     │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────┐
│              JSON Data Files                             │
│  /data/products.json       (Product catalog)            │
│  /data/search-config.json  (Search configuration)       │
└─────────────────────────────────────────────────────────┘
```

---

## Files Created

### 1. **C# Backend Files**

#### `/SearchService.cs`
Main search engine that:
- Loads products from JSON
- Performs intelligent relevance scoring
- Filters by category
- Handles word tokenization
- Calculates match scores

**Key Classes:**
- `Product` - Product data model
- `SearchResult` - Individual search result with score
- `SearchResponse` - API response wrapper
- `SearchService` - Main search logic

#### `/SearchController.cs`
Web API controller exposing search endpoints:
- `GET /api/search/search` - Search with query and category
- `GET /api/search/category/{category}` - Get products by category
- `GET /api/search/categories` - List all categories
- `GET /api/search/product/{id}` - Get single product
- `GET /api/search/all` - Get all products
- `POST /api/search/reload` - Reload data from JSON

### 2. **Frontend Files**

#### `/assets/js/search-api.js`
JavaScript module providing:
- `SearchAPI` object - Online API calls
- `OfflineSearch` object - Fallback search using localStorage
- `searchProducts(query, category)` - Main search function
- `displaySearchResults(results)` - Results rendering
- Auto-initialization on page load

#### `/assets/css/search-results.css`
Styling for:
- Search results grid layout
- Product cards with images
- Discount badges
- Pricing display
- Responsive design for mobile/tablet/desktop

### 3. **Data Files**

#### `/data/products.json`
Main product catalog in JSON format:
```json
[
  {
    "id": 201,
    "name": "Product Name",
    "price": 1999,
    "final_price": 1799,
    "description": "Product description",
    "category": "electronics",
    "image_path": "./assets/images/product.jpg",
    "discount": 10,
    "rating": 4.5,
    "stock": 25,
    "status": "active"
  }
]
```

#### `/data/search-config.json`
Configuration metadata:
- Data source path
- Search field definitions
- Available categories
- Version and timestamp

---

## API Endpoints

### 1. Search Products
```
GET /api/search/search?query=smartphone&category=electronics
```

**Request:**
```json
{
  "query": "smartphone",
  "category": "electronics" // optional
}
```

**Response:**
```json
{
  "success": true,
  "query": "smartphone",
  "category": "electronics",
  "total_results": 5,
  "results": [
    {
      "product": {
        "id": 201,
        "name": "Smartphone X10 Pro",
        "price": 15999,
        "final_price": 14399,
        "description": "Latest smartphone with AI camera",
        "category": "electronics",
        "rating": 4.5,
        "stock": 25
      },
      "relevance_score": 25.5,
      "matched_fields": ["name", "category"]
    }
  ]
}
```

### 2. Get By Category
```
GET /api/search/category/electronics
```

### 3. Get All Categories
```
GET /api/search/categories
```

### 4. Get Product by ID
```
GET /api/search/product/201
```

### 5. Get All Products
```
GET /api/search/all?active=true
```

### 6. Reload Data
```
POST /api/search/reload
```

---

## Relevance Scoring Algorithm

Products are ranked by relevance based on:

| Match Type | Score |
|-----------|-------|
| Exact match in name | +10 |
| Match in category | +7 |
| Match in description | +3 |
| Partial match | +2 |

**Example:**
- Search: "running shoes"
- Product: "Sports Running Shoes" (category: clothing)
- Matches: "running" in name (+10), "shoes" in name (+10), "clothing" similar to query
- **Total Score: 20+**

---

## Integration with index.html

### 1. Add Search Form
```html
<div class="search-container">
  <input 
    type="text" 
    id="search-input" 
    placeholder="Search products..." 
    aria-label="Search products"
  />
  <button id="search-btn">Search</button>
</div>

<!-- Results will appear here -->
<div id="search-results"></div>
```

### 2. Include JavaScript and CSS
```html
<!-- In <head> -->
<link rel="stylesheet" href="assets/css/search-results.css">

<!-- Before </body> -->
<script src="assets/js/search-api.js"></script>
```

### 3. Use Search Function
```javascript
// Simple search
searchProducts('smartphone');

// Search with category filter
searchProducts('shoes', 'clothing');
```

---

## How to Update Data

### Method 1: Update JSON File
1. Edit `/data/products.json` directly
2. Add/modify/remove products
3. Call API reload endpoint:
   ```bash
   curl -X POST http://localhost:5010/api/search/reload
   ```

### Method 2: Automatic Reload
Frontend automatically reloads data on page load:
```javascript
loadOfflineData(); // Stores products in localStorage for offline search
```

### Method 3: Real-time Sync
Update database → JSON export → Call `/api/search/reload` → Done!

---

## Offline Fallback

When API is unavailable:
1. JavaScript falls back to localStorage data
2. Uses same relevance algorithm
3. Seamless user experience
4. Data syncs when API becomes available again

---

## Search Features

### 1. Multi-Word Queries
```
Search: "wireless bluetooth speaker"
Result: Matches products containing any/all words
```

### 2. Category Filtering
```
Search: "phone" in "electronics" category
Result: Only electronics with "phone" match
```

### 3. Fuzzy Matching
```
Search: "bluetooth"
Result: "Bluetooth", "bluetooth", "BLUETOOTH" all match
```

### 4. Relevance Ranking
```
Result 1: Bluetooth Headphones (name match) - Score: 20
Result 2: Wireless Speaker (description match) - Score: 15
Result 3: Phone with Bluetooth (description) - Score: 8
```

---

## Performance Optimization

### 1. Caching
- Products cached in memory (SearchService)
- localStorage caching for offline
- Browser cache for static assets

### 2. Lazy Loading
- Images lazy-loaded in search results
- Only load visible images

### 3. Efficient Filtering
- Substring matching (not regex)
- Case-insensitive comparison
- Single pass scoring

---

## Configuration

### Search Fields (in search-config.json)
```json
{
  "searchFields": ["name", "description", "category"],
  "categories": [
    "electronics",
    "clothing",
    "handicrafts",
    "antiques",
    "books",
    "food",
    "sweets",
    "dairy",
    "natural"
  ]
}
```

---

## Example Usage

### C# Backend
```csharp
// Initialize
var searchService = new SearchService("./data/products.json");

// Search
var results = searchService.Search("smartphone", "electronics");

// Get category
var phones = searchService.GetByCategory("electronics");

// Get all categories
var categories = searchService.GetCategories();

// Reload data
searchService.ReloadProducts();
```

### JavaScript Frontend
```javascript
// Search products
await searchProducts('smartphone');

// Search with category
await searchProducts('shoes', 'clothing');

// Get categories
const categories = await SearchAPI.getCategories();

// Get specific product
const product = await SearchAPI.getProductById(201);

// Get all products
const all = await SearchAPI.getAllProducts();
```

---

## Error Handling

### API Errors
- Network issues → Fallback to localStorage
- Invalid query → User message
- Server errors → Offline mode

### Data Errors
- Missing file → Shows empty results
- Invalid JSON → Logs error, continues
- Missing fields → Uses defaults

---

## Testing

### Test Search
```bash
curl "http://localhost:5010/api/search/search?query=shoe"
```

### Test Category
```bash
curl "http://localhost:5010/api/search/category/clothing"
```

### Test Categories List
```bash
curl "http://localhost:5010/api/search/categories"
```

### Test Reload
```bash
curl -X POST http://localhost:5010/api/search/reload
```

---

## Troubleshooting

### Search returns no results
- Check query spelling
- Verify products exist in `/data/products.json`
- Check product status is "active"
- Reload data: POST `/api/search/reload`

### API not responding
- Verify server is running: `dotnet run`
- Check data file exists: `/data/products.json`
- Check file path in SearchService

### Images not showing
- Verify image paths in JSON
- Check relative vs absolute paths
- Use `../images/filename` for relative paths

### Offline search not working
- Check localStorage quota
- Verify `loadOfflineData()` is called
- Check browser console for errors

---

## Future Enhancements

1. **Advanced Filters**
   - Price range
   - Rating filter
   - Stock availability

2. **Search Analytics**
   - Popular searches
   - No-result queries
   - User behavior tracking

3. **AI-Powered Search**
   - Synonym matching
   - Typo correction
   - Auto-suggestions

4. **Faceted Search**
   - Brand filter
   - Size filter
   - Color filter

---

## Support

For issues or questions:
1. Check this documentation
2. Review console errors (F12)
3. Check `/data/search-config.json` for configuration
4. Verify `/data/products.json` is valid JSON

---

**Version:** 1.0  
**Last Updated:** 2026-01-27  
**Maintained By:** MR Shop Team
