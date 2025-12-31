# MR Shop AI Chat System 🤖

An intelligent AI-powered chat system that analyzes your website data and provides contextual answers to customer questions.

## Features ✨

- **Smart Context Analysis**: Automatically reads and indexes all products, categories, and page content
- **Natural Language Understanding**: Understands customer queries about products, pricing, shipping, etc.
- **Real-time Responses**: Instant AI-generated answers based on your actual website data
- **Messenger-Style Interface**: Modern, user-friendly chat UI
- **Product Recommendations**: Suggests relevant products based on customer questions
- **Multi-topic Support**: Handles queries about:
  - Products and availability
  - Pricing and categories
  - Shipping and delivery
  - Payment methods
  - Returns and exchanges
  - Your charity work
  - Contact information

## Installation 🚀

### 1. Install Python Dependencies

```bash
cd backend
pip install -r requirements.txt
```

Or install individually:
```bash
pip install Flask Flask-CORS
```

### 2. Start the Backend Server

```bash
cd backend
python chat_api.py
```

You should see:
```
🚀 Starting MR Shop AI Chat Server...
📊 Loaded X products
📁 Loaded X categories
✅ Server ready at http://localhost:5000
```

### 3. Open the Chat Interface

Simply open `chat.html` in your browser or click the chat icon (💬) from your main website.

## How It Works 🔧

### Backend Architecture

**chat_api.py** - The main Flask server with:

1. **WebsiteKnowledgeBase**: Loads and indexes all your data
   - Reads `data/products.json`
   - Reads `data/categories.json`
   - Extracts content from HTML pages
   
2. **ChatAI**: Intelligent response generator
   - Analyzes user queries
   - Finds relevant context from your data
   - Generates natural, helpful responses

3. **API Endpoints**:
   - `POST /api/chat` - Main chat endpoint
   - `GET /api/health` - Server health check
   - `GET /api/products` - Get all products
   - `GET /api/categories` - Get all categories

### Frontend Integration

**chat.html** - Updated with:
- Real API connection to backend
- Typing indicators
- Error handling
- Welcome message
- Formatted bot responses

## API Usage 📡

### Send a Message

**Endpoint**: `POST http://localhost:5000/api/chat`

**Request**:
```json
{
  "message": "What products do you sell?"
}
```

**Response**:
```json
{
  "success": true,
  "response": "Here are some products from our collection...",
  "type": "product",
  "timestamp": "2025-12-06T10:30:00",
  "data": {
    "products": [...],
    "categories": [...]
  }
}
```

### Check Server Health

**Endpoint**: `GET http://localhost:5000/api/health`

**Response**:
```json
{
  "status": "healthy",
  "products_loaded": 10,
  "categories_loaded": 5,
  "timestamp": "2025-12-06T10:30:00"
}
```

## Example Queries 💬

Try these questions in the chat:

- "What products do you sell?"
- "Show me handicrafts"
- "What's the price range?"
- "How does shipping work?"
- "Tell me about your charity work"
- "Do you accept credit cards?"
- "What's your return policy?"
- "How can I contact you?"

## Customization 🎨

### Add More Products

Edit `data/products.json`:
```json
[
  {
    "id": 202,
    "name": "Your Product Name",
    "price": 1500,
    "description": "Product description",
    "category": "handicrafts",
    "image": "path/to/image.jpg"
  }
]
```

The AI will automatically include new products in responses!

### Customize Bot Responses

Edit the response methods in `backend/chat_api.py`:

```python
def _greeting_response(self):
    return {
        'message': "Your custom greeting here!",
        'type': 'greeting'
    }
```

### Change API Port

Edit `backend/chat_api.py` (last line):
```python
app.run(debug=True, host='0.0.0.0', port=5000)  # Change port here
```

And update `chat.html`:
```javascript
const API_URL = 'http://localhost:5000/api/chat';  // Update port here
```

## Troubleshooting 🔍

### "Connection Error" in Chat

**Problem**: Frontend can't connect to backend

**Solutions**:
1. Make sure backend is running: `python backend/chat_api.py`
2. Check the terminal for errors
3. Verify port 5000 is not in use
4. Check browser console for CORS errors

### "Module not found" Error

**Problem**: Flask not installed

**Solution**:
```bash
pip install Flask Flask-CORS
```

### Bot Gives Generic Responses

**Problem**: AI can't find your data

**Solutions**:
1. Check `data/products.json` exists and has valid JSON
2. Check `data/categories.json` exists
3. Restart the backend server to reload data

### CORS Errors

**Problem**: Browser blocks API requests

**Solution**: Already handled with Flask-CORS, but if issues persist:
```bash
pip install --upgrade Flask-CORS
```

## Advanced Features 🚀

### Add OpenAI Integration (Optional)

For even smarter responses, integrate OpenAI GPT:

1. Install OpenAI:
```bash
pip install openai
```

2. Update `chat_api.py`:
```python
import openai

openai.api_key = 'your-api-key'

def generate_response(self, user_message):
    context = self.kb.get_context(user_message)
    
    # Use GPT for more natural responses
    response = openai.ChatCompletion.create(
        model="gpt-3.5-turbo",
        messages=[
            {"role": "system", "content": f"You are MR Shop assistant. Context: {context}"},
            {"role": "user", "content": user_message}
        ]
    )
    
    return response.choices[0].message.content
```

### Add Database Storage

Store conversations in a database:

```bash
pip install Flask-SQLAlchemy
```

Create models for chat history and analytics.

## Production Deployment 🌐

### Using Gunicorn

```bash
pip install gunicorn
gunicorn -w 4 -b 0.0.0.0:5000 backend.chat_api:app
```

### Using Docker

Create `Dockerfile`:
```dockerfile
FROM python:3.9
WORKDIR /app
COPY backend/requirements.txt .
RUN pip install -r requirements.txt
COPY . .
CMD ["python", "backend/chat_api.py"]
```

## Security Notes 🔒

- Add rate limiting for production
- Implement user authentication if needed
- Use environment variables for sensitive data
- Enable HTTPS in production
- Sanitize user inputs (already handled)

## Performance Tips ⚡

- Enable caching for frequently asked questions
- Use Redis for session storage
- Implement response queuing for high traffic
- Monitor API response times

## Support 💙

Questions? Issues? Contact: support@mrshop.com

## License

MIT License - Feel free to modify and use!

---

Built with ❤️ by MR Shop Team
