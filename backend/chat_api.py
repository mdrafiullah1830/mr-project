"""
AI-Powered Chat API for MR Shop
Analyzes website data and answers user questions intelligently
"""

from flask import Flask, request, jsonify
from flask_cors import CORS
import json
import os
from pathlib import Path
from datetime import datetime
import re

from payment import (
    bootstrap_processor,
    PaymentRequest,
    ValidationError as PaymentValidationError,
    PaymentError,
)

app = Flask(__name__)
CORS(app)  # Enable CORS for frontend communication

# Base directory
BASE_DIR = Path(__file__).parent.parent


donation_processor = bootstrap_processor()

ORGANISATION_WALLETS = {
    "bkash": "01700123456",
    "nagad": "01800123456",
    "rocket": "01900123456",
}

DONATIONS_FILE = BASE_DIR / "data" / "donations.json"
BD_PHONE_PATTERN = re.compile(r"^01[3-9]\d{8}$")


def record_donation(entry: dict) -> None:
    """Append donation entry to JSON log for transparency."""

    DONATIONS_FILE.parent.mkdir(parents=True, exist_ok=True)
    donations = []
    if DONATIONS_FILE.exists():
        try:
            with open(DONATIONS_FILE, "r", encoding="utf-8") as fp:
                donations = json.load(fp)
        except json.JSONDecodeError:
            donations = []

    donations.append(entry)
    with open(DONATIONS_FILE, "w", encoding="utf-8") as fp:
        json.dump(donations, fp, ensure_ascii=False, indent=2)

class WebsiteKnowledgeBase:
    """Loads and indexes all website data for AI chat responses"""
    
    def __init__(self):
        self.products = []
        self.categories = []
        self.pages_content = {}
        self.load_all_data()
    
    def load_all_data(self):
        """Load all JSON and HTML files"""
        try:
            # Load products
            products_path = BASE_DIR / 'data' / 'products.json'
            if products_path.exists():
                with open(products_path, 'r', encoding='utf-8') as f:
                    self.products = json.load(f)
            
            # Load categories
            categories_path = BASE_DIR / 'data' / 'categories.json'
            if categories_path.exists():
                with open(categories_path, 'r', encoding='utf-8') as f:
                    self.categories = json.load(f)
            
            # Load HTML page contents for context
            html_files = [
                'index.html', 'about.html', 'contact.html', 
                'workfor.html', 'workforpeople.html', 'workforchild.html'
            ]
            
            for html_file in html_files:
                html_path = BASE_DIR / html_file
                if html_path.exists():
                    with open(html_path, 'r', encoding='utf-8') as f:
                        content = f.read()
                        # Extract text content (simplified)
                        text = self._extract_text_from_html(content)
                        self.pages_content[html_file] = text
            
            print(f"✅ Loaded {len(self.products)} products, {len(self.categories)} categories")
            
        except Exception as e:
            print(f"❌ Error loading data: {e}")
    
    def _extract_text_from_html(self, html_content):
        """Extract meaningful text from HTML"""
        # Remove script and style tags
        html_content = re.sub(r'<script[^>]*>.*?</script>', '', html_content, flags=re.DOTALL)
        html_content = re.sub(r'<style[^>]*>.*?</style>', '', html_content, flags=re.DOTALL)
        # Remove HTML tags
        text = re.sub(r'<[^>]+>', ' ', html_content)
        # Clean up whitespace
        text = re.sub(r'\s+', ' ', text).strip()
        return text[:2000]  # Limit to 2000 chars per page
    
    def get_context(self, query):
        """Build context relevant to user query"""
        query_lower = query.lower()
        context = {
            'products': [],
            'categories': [cat['name'] for cat in self.categories],
            'general_info': []
        }
        
        # Find relevant products
        for product in self.products:
            if (query_lower in product.get('name', '').lower() or
                query_lower in product.get('description', '').lower() or
                query_lower in product.get('category', '').lower()):
                context['products'].append(product)
        
        # If no specific products found, include all products for general queries
        if not context['products'] and any(word in query_lower for word in ['product', 'sell', 'buy', 'price', 'available']):
            context['products'] = self.products[:5]  # Limit to 5 products
        
        # Include relevant page content
        for page_name, content in self.pages_content.items():
            if any(keyword in query_lower for keyword in ['about', 'contact', 'work', 'help', 'mission']):
                if 'about' in query_lower and 'about' in page_name.lower():
                    context['general_info'].append(content[:500])
                elif 'work' in query_lower and 'work' in page_name.lower():
                    context['general_info'].append(content[:500])
        
        return context

# Initialize knowledge base
kb = WebsiteKnowledgeBase()

class ChatAI:
    """AI-powered chat response generator"""
    
    def __init__(self, knowledge_base):
        self.kb = knowledge_base
    
    def generate_response(self, user_message):
        """Generate intelligent response based on user query"""
        user_message_lower = user_message.lower()
        
        # Get relevant context
        context = self.kb.get_context(user_message)
        
        # Greeting responses
        if any(word in user_message_lower for word in ['hello', 'hi', 'hey', 'greetings']):
            return self._greeting_response()
        
        # Product queries
        if any(word in user_message_lower for word in ['product', 'item', 'sell', 'available', 'buy', 'purchase']):
            return self._product_response(context, user_message)
        
        # Category queries
        if any(word in user_message_lower for word in ['category', 'categories', 'type', 'section']):
            return self._category_response(context)
        
        # Price queries
        if any(word in user_message_lower for word in ['price', 'cost', 'how much', 'expensive', 'cheap']):
            return self._price_response(context, user_message)
        
        # About/mission queries
        if any(word in user_message_lower for word in ['about', 'who', 'mission', 'purpose']):
            return self._about_response()
        
        # Work/charity queries
        if any(word in user_message_lower for word in ['work', 'charity', 'help', 'donate', 'support']):
            return self._work_response()
        
        # Shipping/delivery queries
        if any(word in user_message_lower for word in ['ship', 'delivery', 'deliver', 'receive']):
            return self._shipping_response()
        
        # Payment queries
        if any(word in user_message_lower for word in ['payment', 'pay', 'card', 'cash']):
            return self._payment_response()
        
        # Return queries
        if any(word in user_message_lower for word in ['return', 'refund', 'exchange']):
            return self._return_response()
        
        # Contact queries
        if any(word in user_message_lower for word in ['contact', 'reach', 'email', 'phone']):
            return self._contact_response()
        
        # Default response
        return self._default_response(context, user_message)
    
    def _greeting_response(self):
        return {
            'message': "Hello! 👋 Welcome to MR Shop! I'm here to help you find products, answer questions about our categories, shipping, and more. How can I assist you today?",
            'type': 'greeting'
        }
    
    def _product_response(self, context, query):
        if context['products']:
            products_info = []
            for product in context['products'][:3]:  # Limit to 3 products
                products_info.append(
                    f"• **{product['name']}** - ৳{product['price']}\n"
                    f"  {product.get('description', 'No description available')}\n"
                    f"  Category: {product.get('category', 'N/A')}"
                )
            
            message = "Here are some products from our collection:\n\n" + "\n\n".join(products_info)
            message += "\n\nWould you like to know more about any specific product?"
            return {'message': message, 'type': 'product', 'products': context['products'][:3]}
        else:
            return {
                'message': f"We currently have {len(self.kb.products)} products available across categories like {', '.join(context['categories'])}. Could you tell me more about what you're looking for?",
                'type': 'product'
            }
    
    def _category_response(self, context):
        categories_list = ', '.join(context['categories'])
        return {
            'message': f"We offer products in the following categories:\n\n{categories_list}\n\nWhich category interests you? I can show you products from any of these!",
            'type': 'category',
            'categories': context['categories']
        }
    
    def _price_response(self, context, query):
        if context['products']:
            prices = [p['price'] for p in context['products'] if 'price' in p]
            if prices:
                avg_price = sum(prices) / len(prices)
                min_price = min(prices)
                max_price = max(prices)
                return {
                    'message': f"Our products range from ৳{min_price} to ৳{max_price}. The average price is around ৳{int(avg_price)}. Would you like to see specific products in your budget?",
                    'type': 'price'
                }
        return {
            'message': "Our products are competitively priced! Could you tell me which product or category you're interested in? I can give you specific pricing details.",
            'type': 'price'
        }
    
    def _about_response(self):
        return {
            'message': "🛍️ **About MR Shop**\n\nWe're MR Shop, your trusted online marketplace for authentic Bangladeshi products! We offer:\n\n• Handcrafted items\n• Traditional food & sweets\n• Clothing & textiles\n• Books and more\n\n💙 **Our Mission**: 10% of our profits go to support education, healthcare, and shelter for underprivileged people and children.\n\nWe believe in business with purpose!",
            'type': 'about'
        }
    
    def _work_response(self):
        return {
            'message': "🤝 **Work For People & Children**\n\nWe're committed to social impact! 10% of every profit goes toward:\n\n👥 **For People:**\n• Education programs\n• Healthcare support\n• Food security\n• Shelter assistance\n\n👶 **For Children:**\n• Child education\n• Nutrition programs\n• Healthcare services\n• Safe shelter\n\nEvery purchase helps make a difference!",
            'type': 'charity'
        }
    
    def _shipping_response(self):
        return {
            'message': "📦 **Shipping & Delivery**\n\nWe offer reliable shipping across Bangladesh:\n\n• Standard delivery: 3-5 business days\n• Express delivery available\n• Free shipping on orders over ৳1000\n• Secure packaging\n• Order tracking available\n\nNeed help with a specific delivery question?",
            'type': 'shipping'
        }
    
    def _payment_response(self):
        return {
            'message': "💳 **Payment Options**\n\nWe accept multiple payment methods:\n\n• Cash on Delivery (COD)\n• Credit/Debit Cards\n• Mobile Banking (bKash, Nagad, Rocket)\n• Bank Transfer\n\nAll transactions are secure and encrypted. Which payment method would you prefer?",
            'type': 'payment'
        }
    
    def _return_response(self):
        return {
            'message': "🔄 **Returns & Exchanges**\n\n• 7-day return policy\n• Products must be unused and in original packaging\n• Free returns for defective items\n• Exchange available for size/color issues\n• Refunds processed within 5-7 business days\n\nHave a specific return request? Let me know!",
            'type': 'return'
        }
    
    def _contact_response(self):
        return {
            'message': "📞 **Contact Us**\n\nWe're here to help!\n\n• Email: support@mrshop.com\n• Phone: +880-XXX-XXXXXX\n• Live Chat: Available 9 AM - 9 PM\n• Social Media: @mrshop\n\nFeel free to reach out anytime. How else can I assist you?",
            'type': 'contact'
        }
    
    def _default_response(self, context, query):
        return {
            'message': f"I'm here to help! I can assist you with:\n\n• Product information and availability\n• Categories and pricing\n• Shipping and delivery\n• Payment options\n• Our charity work\n• Returns and exchanges\n\nCould you please clarify your question? I'm here to make your shopping experience better! 😊",
            'type': 'help'
        }

# Initialize AI chat
chat_ai = ChatAI(kb)

@app.route('/api/chat', methods=['POST'])
def chat():
    """Main chat endpoint"""
    try:
        data = request.get_json()
        
        if not data or 'message' not in data:
            return jsonify({'error': 'Message is required'}), 400
        
        user_message = data['message'].strip()
        
        if not user_message:
            return jsonify({'error': 'Message cannot be empty'}), 400
        
        # Generate AI response
        response = chat_ai.generate_response(user_message)
        
        # Log conversation (optional)
        print(f"[{datetime.now()}] User: {user_message}")
        print(f"[{datetime.now()}] Bot: {response['message'][:100]}...")
        
        return jsonify({
            'success': True,
            'response': response['message'],
            'type': response.get('type', 'general'),
            'timestamp': datetime.now().isoformat(),
            'data': {
                'products': response.get('products', []),
                'categories': response.get('categories', [])
            }
        })
    
    except Exception as e:
        print(f"❌ Error in chat endpoint: {e}")
        return jsonify({
            'success': False,
            'error': 'Something went wrong. Please try again.',
            'response': "I'm having trouble processing your request right now. Please try again in a moment."
        }), 500

@app.route('/api/health', methods=['GET'])
def health():
    """Health check endpoint"""
    return jsonify({
        'status': 'healthy',
        'products_loaded': len(kb.products),
        'categories_loaded': len(kb.categories),
        'timestamp': datetime.now().isoformat()
    })

@app.route('/api/products', methods=['GET'])
def get_products():
    """Get all products"""
    category = request.args.get('category', None)
    
    if category:
        filtered_products = [p for p in kb.products if p.get('category') == category]
        return jsonify({'products': filtered_products, 'count': len(filtered_products)})
    
    return jsonify({'products': kb.products, 'count': len(kb.products)})

@app.route('/api/categories', methods=['GET'])
def get_categories():
    """Get all categories"""
    return jsonify({'categories': kb.categories, 'count': len(kb.categories)})


@app.route('/api/donations', methods=['POST'])
def create_donation():
    """Accept donation submissions from the Work for People page."""

    payload = request.get_json(silent=True) or {}
    provider = (payload.get('provider') or '').strip().lower()

    if provider not in ORGANISATION_WALLETS:
        return jsonify({'success': False, 'error': 'Select a supported payment method (bKash, Nagad, Rocket).'}), 400

    try:
        amount = float(payload.get('amount', 0))
    except (TypeError, ValueError):
        amount = 0.0

    if amount < 10:
        return jsonify({'success': False, 'error': 'Minimum donation amount is ৳10.'}), 400

    donor_name = (payload.get('donorName') or 'Anonymous Donor').strip()
    donor_email = (payload.get('donorEmail') or '').strip()
    sender_number = (payload.get('senderNumber') or '').strip()
    sender_last_four = (payload.get('senderLastFour') or '').strip()
    transaction_id = (payload.get('transactionId') or '').strip()
    message = (payload.get('message') or '').strip()
    source = (payload.get('source') or 'work-for-people').strip()

    if not donor_name:
        return jsonify({'success': False, 'error': 'Please share your name so we can confirm your donation.'}), 400

    if not BD_PHONE_PATTERN.match(sender_number):
        return jsonify({'success': False, 'error': 'Enter a valid Bangladeshi mobile wallet number (e.g., 01XXXXXXXXX).'}), 400

    if not sender_last_four:
        sender_last_four = sender_number[-4:]

    if not sender_last_four.isdigit() or len(sender_last_four) != 4:
        return jsonify({'success': False, 'error': 'Last four digits must be exactly 4 numbers.'}), 400

    if len(transaction_id) < 5:
        return jsonify({'success': False, 'error': 'Please provide the transaction ID from your send money receipt.'}), 400

    receiver_number = ORGANISATION_WALLETS[provider]

    try:
        payment_request = PaymentRequest(
            provider=provider,
            sender_number=sender_number,
            receiver_number=receiver_number,
            amount=amount,
            reference=f"Donation from {donor_name}",
            sender_last_four=sender_last_four,
            user_transaction_id=transaction_id,
            metadata={
                'donor_name': donor_name,
                'donor_email': donor_email,
                'message': message,
                'source': source,
            },
        )
        receipt = donation_processor.send(payment_request)

        record_donation({
            'donor_name': donor_name,
            'donor_email': donor_email,
            'message': message,
            'provider': provider,
            'amount': amount,
            'user_transaction_id': transaction_id,
            'receipt': receipt.as_dict(),
            'created_at': receipt.created_at.isoformat(),
        })

        return jsonify({
            'success': True,
            'message': f"Thank you, {donor_name}! Your donation has been recorded.",
            'receipt': receipt.as_dict(),
        }), 201

    except PaymentValidationError as exc:
        return jsonify({'success': False, 'error': str(exc)}), 400
    except PaymentError as exc:
        return jsonify({'success': False, 'error': str(exc)}), 422
    except Exception as exc:  # pragma: no cover
        print(f"❌ Error while creating donation: {exc}")
        return jsonify({'success': False, 'error': 'Unable to submit donation right now. Please try again later.'}), 500

if __name__ == '__main__':
    print("🚀 Starting MR Shop AI Chat Server...")
    print(f"📊 Loaded {len(kb.products)} products")
    print(f"📁 Loaded {len(kb.categories)} categories")
    print("✅ Server ready at http://localhost:5001")
    app.run(debug=True, host='0.0.0.0', port=5001)
