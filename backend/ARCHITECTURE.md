# MR Shop AI Chat System - Architecture

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        USER INTERFACE                           │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                     chat.html                            │  │
│  │                                                          │  │
│  │  • Messenger-style UI                                   │  │
│  │  • Message input/output                                 │  │
│  │  • Typing indicators                                    │  │
│  │  • Real-time updates                                    │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ HTTP POST
                              │ { message: "user question" }
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      BACKEND API                                │
│                  (Flask Server - Port 5001)                     │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              chat_api.py - Main Router                   │  │
│  │                                                          │  │
│  │  Routes:                                                │  │
│  │  • POST /api/chat        → Chat endpoint               │  │
│  │  • GET  /api/health      → Health check                │  │
│  │  • GET  /api/products    → Get products                │  │
│  │  • GET  /api/categories  → Get categories              │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                  │
│                              ▼                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │         WebsiteKnowledgeBase (Data Loader)              │  │
│  │                                                          │  │
│  │  • Loads products.json                                  │  │
│  │  • Loads categories.json                                │  │
│  │  • Extracts HTML content                                │  │
│  │  • Indexes all data                                     │  │
│  │  • Provides search/context functions                    │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                  │
│                              ▼                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │            ChatAI (Response Generator)                   │  │
│  │                                                          │  │
│  │  • Analyzes user queries                                │  │
│  │  • Finds relevant context                               │  │
│  │  • Generates natural responses                          │  │
│  │  • Handles multiple topic types:                        │  │
│  │    - Products                                           │  │
│  │    - Categories                                         │  │
│  │    - Pricing                                            │  │
│  │    - Shipping                                           │  │
│  │    - Payment                                            │  │
│  │    - Returns                                            │  │
│  │    - Charity/Mission                                    │  │
│  │    - Contact                                            │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ JSON Response
                              │ { response: "AI answer", ... }
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                          DATA LAYER                             │
│                                                                 │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐   │
│  │ products.json  │  │categories.json │  │  HTML Pages    │   │
│  │                │  │                │  │                │   │
│  │ • Product list │  │ • Category     │  │ • index.html   │   │
│  │ • Prices       │  │   names        │  │ • about.html   │   │
│  │ • Descriptions │  │ • Metadata     │  │ • workfor.html │   │
│  │ • Categories   │  │                │  │ • etc.         │   │
│  └────────────────┘  └────────────────┘  └────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

## Data Flow Example

### User asks: "What handicrafts do you sell?"

```
1. Frontend (chat.html)
   └─> POST http://localhost:5001/api/chat
       Body: { "message": "What handicrafts do you sell?" }

2. Backend receives request
   └─> Routes to chat_ai.generate_response()

3. ChatAI analyzes query
   └─> Detects keywords: "handicrafts", "sell"
   └─> Identifies intent: Product query

4. WebsiteKnowledgeBase searches
   └─> Filters products where category == "handicrafts"
   └─> Returns: [
         {
           "id": 201,
           "name": "Sample Nakshi Kantha",
           "price": 1200,
           "description": "Hand-stitched...",
           "category": "handicrafts"
         }
       ]

5. ChatAI formats response
   └─> Generates natural language answer:
       "Here are some products from our collection:
        • Sample Nakshi Kantha - ৳1200
          Hand-stitched Nakshi Kantha sample product.
          Category: handicrafts"

6. Backend sends JSON response
   └─> {
         "success": true,
         "response": "Here are some products...",
         "type": "product",
         "data": { "products": [...] }
       }

7. Frontend displays response
   └─> Shows bot message bubble
   └─> Formats with bold/bullets
   └─> Auto-scrolls to bottom
```

## Component Interaction

```
chat.html                    chat_api.py
    │                            │
    │  1. User types message     │
    │──────────────────────────> │
    │                            │
    │                       2. Receive & parse
    │                            │
    │                       3. Load context
    │                       from knowledge base
    │                            │
    │                       4. Analyze query
    │                       with ChatAI
    │                            │
    │                       5. Generate response
    │                            │
    │  6. Send JSON response     │
    │ <────────────────────────  │
    │                            │
    7. Display bot message       │
    with formatting              │
    │                            │
```

## File Structure

```
mr project/
│
├── chat.html                     # Frontend chat interface
├── index.html                    # Main site (with chat icon link)
│
├── data/
│   ├── products.json            # Product database
│   └── categories.json          # Category database
│
└── backend/
    ├── chat_api.py              # Main Flask server + AI logic
    ├── requirements.txt         # Python dependencies
    ├── start_chat_server.sh     # Server start script
    ├── test_chat.py             # Test suite
    ├── README_CHAT.md           # Full documentation
    └── venv/                    # Virtual environment (auto-created)
```

## Technology Stack

```
┌─────────────────────────────────────────────┐
│            Frontend Layer                   │
│                                             │
│  • HTML5 (Semantic markup)                 │
│  • CSS3 (Modern styling, animations)       │
│  • Vanilla JavaScript (No frameworks)      │
│  • Fetch API (HTTP requests)               │
└─────────────────────────────────────────────┘
                    │
                    │ REST API
                    ▼
┌─────────────────────────────────────────────┐
│            Backend Layer                    │
│                                             │
│  • Python 3.x                              │
│  • Flask (Web framework)                   │
│  • Flask-CORS (Cross-origin support)       │
│  • Custom NLP logic                        │
│  • JSON file I/O                           │
└─────────────────────────────────────────────┘
                    │
                    │ File I/O
                    ▼
┌─────────────────────────────────────────────┐
│             Data Layer                      │
│                                             │
│  • JSON files (products, categories)       │
│  • HTML files (page content)               │
│  • (Optional) SQLite for chat history      │
└─────────────────────────────────────────────┘
```

## AI Logic Flow

```
User Query: "What products do you sell?"
    │
    ▼
┌─────────────────────────────────────┐
│   Query Analysis                    │
│   • Tokenize words                  │
│   • Identify keywords               │
│   • Detect intent                   │
└─────────────────────────────────────┘
    │
    ├── Contains: "product", "sell"
    ├── Intent: PRODUCT_QUERY
    │
    ▼
┌─────────────────────────────────────┐
│   Context Retrieval                 │
│   • Search products database        │
│   • Filter by category (if any)    │
│   • Get relevant data               │
└─────────────────────────────────────┘
    │
    ├── Found: 1 product
    ├── Category: handicrafts
    │
    ▼
┌─────────────────────────────────────┐
│   Response Generation               │
│   • Format product info             │
│   • Add helpful context             │
│   • Create natural language         │
└─────────────────────────────────────┘
    │
    ├── Output: Formatted answer
    │
    ▼
┌─────────────────────────────────────┐
│   Response Delivery                 │
│   • Return JSON                     │
│   • Include metadata                │
│   • Log conversation                │
└─────────────────────────────────────┘
```

## Scalability

```
Current Setup (Development):
┌──────────────────┐
│  Single Flask    │
│  Process         │
│  Port: 5001      │
└──────────────────┘

Production Setup (Recommended):
┌─────────────────────────────────┐
│      Load Balancer (Nginx)      │
└─────────────────────────────────┘
    │           │            │
    ▼           ▼            ▼
┌────────┐  ┌────────┐  ┌────────┐
│ Worker │  │ Worker │  │ Worker │
│   1    │  │   2    │  │   3    │
└────────┘  └────────┘  └────────┘
    │           │            │
    └───────────┴────────────┘
                │
                ▼
    ┌──────────────────────┐
    │   Database (Redis)   │
    │   • Session storage  │
    │   • Response cache   │
    └──────────────────────┘
```

## Security Considerations

```
┌────────────────────────────────────────┐
│         Security Layers                │
│                                        │
│  1. Input Sanitization                │
│     • HTML escaping                   │
│     • XSS prevention                  │
│                                        │
│  2. CORS Protection                   │
│     • Whitelist domains               │
│     • Secure headers                  │
│                                        │
│  3. Rate Limiting (Production)        │
│     • Max requests per minute         │
│     • IP-based throttling             │
│                                        │
│  4. HTTPS (Production)                │
│     • SSL/TLS encryption              │
│     • Secure cookies                  │
│                                        │
│  5. Authentication (Optional)         │
│     • User sessions                   │
│     • API keys                        │
└────────────────────────────────────────┘
```

## Future Enhancements

```
Phase 1 (Current): ✅ Done
• Basic AI chat
• Product queries
• Static responses

Phase 2 (Optional):
• OpenAI integration
• More natural responses
• Context memory

Phase 3 (Advanced):
• User authentication
• Chat history storage
• Analytics dashboard
• Sentiment analysis

Phase 4 (Enterprise):
• Multi-language support
• Voice input/output
• Image recognition
• Recommendation engine
```

---

This architecture provides:
- ✅ Modular design
- ✅ Easy to scale
- ✅ Simple to maintain
- ✅ Clear separation of concerns
- ✅ Production-ready foundation
