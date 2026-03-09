#!/usr/bin/env python3
"""
MR Shop AI Chatbot - FastAPI Backend Server
============================================

This is the main chat API server that:
1. Loads pre-trained embeddings and metadata
2. Handles user queries in Bangla or English
3. Finds the most similar Q&A from dataset
4. Returns intelligent responses with confidence scores
5. Tracks conversation history

Port: 5001
Endpoints:
  POST /api/chat - Send message and get response
  GET  /api/chat/health - Health check
  GET  / - API info
"""

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional
import numpy as np
import json
from pathlib import Path
import logging
from datetime import datetime
from sentence_transformers import SentenceTransformer
from sklearn.metrics.pairwise import cosine_similarity

# ============================================================
# CONFIGURATION
# ============================================================

MODEL_DIR = Path(__file__).parent / 'models'
LOG_FILE = Path(__file__).parent / 'chat_api.log'

# Setup logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler(LOG_FILE),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

# ============================================================
# FASTAPI SETUP
# ============================================================

app = FastAPI(
    title="MR Shop AI Chatbot API",
    description="Intelligent multilingual chatbot for shop queries",
    version="2.0.0"
)

# Enable CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ============================================================
# DATA MODELS
# ============================================================

class ChatMessage(BaseModel):
    """User message request"""
    message: str
    user_id: Optional[str] = None

class ChatResponse(BaseModel):
    """Chat response with metadata"""
    response: str
    confidence: float
    category: str
    timestamp: str
    similar_questions: List[dict] = []

class HealthResponse(BaseModel):
    """Health check response"""
    status: str
    model_loaded: bool
    data_points: int

# ============================================================
# GLOBAL STATE
# ============================================================

class ChatbotModel:
    """Chatbot model wrapper"""
    
    def __init__(self):
        self.embeddings = None
        self.questions = None
        self.answers = None
        self.categories = None
        self.encoder_model = None
        self.loaded = False
        
    def load(self):
        """Load model and data from disk"""
        logger.info("Loading chatbot model...")
        
        try:
            # Load embeddings
            embeddings_path = MODEL_DIR / 'question_embeddings.npy'
            self.embeddings = np.load(embeddings_path)
            logger.info(f"✅ Loaded {len(self.embeddings)} embeddings")
            
            # Load metadata
            metadata_path = MODEL_DIR / 'metadata.json'
            with open(metadata_path, 'r', encoding='utf-8') as f:
                metadata = json.load(f)
            
            self.questions = metadata['questions']
            self.answers = metadata['answers']
            self.categories = metadata['categories']
            logger.info(f"✅ Loaded {len(self.questions)} Q&A pairs")
            
            # Load embedding model
            logger.info("Loading sentence encoder...")
            self.encoder_model = SentenceTransformer(
                'sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2'
            )
            logger.info("✅ Sentence encoder loaded")
            
            self.loaded = True
            logger.info("✅ Chatbot model fully loaded and ready!")
            
        except Exception as e:
            logger.error(f"❌ Error loading model: {str(e)}")
            self.loaded = False
            raise
    
    def get_response(self, query: str, top_k: int = 1) -> dict:
        """
        Get response for user query using hybrid semantic + keyword matching
        
        Process:
        1. Extract keywords from query
        2. Search for questions with matching keywords
        3. Use semantic similarity as tiebreaker
        4. Return best match with confidence score
        """
        
        if not self.loaded:
            raise ValueError("Model not loaded")
        
        try:
            # Extract keywords from query
            query_words = set(query.split())
            query_lower = query.lower()
            
            logger.info(f"Query: '{query}'")
            logger.info(f"Query words: {query_words}")
            
            # Encode query for semantic fallback
            query_embedding = self.encoder_model.encode(query, convert_to_numpy=True)
            similarities = cosine_similarity([query_embedding], self.embeddings)[0]
            
            # Find best match using keyword-first approach
            candidates = []
            for idx, question in enumerate(self.questions):
                question_words = set(question.split())
                keyword_overlap = len(query_words & question_words)
                semantic_score = float(similarities[idx])
                
                # Scoring: keyword match is primary, semantic score is tiebreaker
                # A keyword match is worth about 0.2 in semantic score
                score_with_keywords = semantic_score + (keyword_overlap * 0.15)
                
                candidates.append({
                    'idx': idx,
                    'keyword_overlap': keyword_overlap,
                    'semantic_score': semantic_score,
                    'combined_score': score_with_keywords,
                    'question': question
                })
                
                logger.info(f"  Q{idx}: keywords={keyword_overlap}, semantic={semantic_score:.3f}, combined={score_with_keywords:.3f}")
            
            # Sort by combined score (keywords + semantic)
            candidates.sort(key=lambda x: x['combined_score'], reverse=True)
            best = candidates[0]
            best_idx = best['idx']
            
            logger.info(f"Final choice: idx={best_idx}, overlap={best['keyword_overlap']}, score={best['semantic_score']:.3f}")
            
            confidence = best['semantic_score']
            
            # Get similar questions for context
            similar = [
                {
                    'question': candidates[i]['question'],
                    'similarity': candidates[i]['semantic_score']
                }
                for i in range(1, min(3, len(candidates)))
            ]
            
            return {
                'response': self.answers[best_idx],
                'confidence': confidence,
                'category': self.categories[best_idx],
                'best_match_question': self.questions[best_idx],
                'similar_questions': similar[:2]
            }
            
        except Exception as e:
            logger.error(f"Error processing query: {str(e)}")
            raise

# Initialize chatbot model
chatbot = ChatbotModel()

# ============================================================
# STARTUP/SHUTDOWN EVENTS
# ============================================================

@app.on_event("startup")
async def startup_event():
    """Load model on server startup"""
    logger.info("=" * 60)
    logger.info("🚀 MR Shop AI Chatbot API Starting...")
    logger.info("=" * 60)
    
    try:
        chatbot.load()
    except Exception as e:
        logger.error(f"Failed to start server: {str(e)}")
        raise

@app.on_event("shutdown")
async def shutdown_event():
    """Cleanup on shutdown"""
    logger.info("🛑 Server shutting down...")

# ============================================================
# API ENDPOINTS
# ============================================================

@app.get("/", response_model=dict)
async def root():
    """Root endpoint - API information"""
    return {
        "service": "MR Shop AI Chatbot API",
        "version": "2.0.0",
        "description": "Multilingual chatbot for shop queries (Bangla + English)",
        "endpoints": {
            "chat": "POST /api/chat - Send message and get response",
            "health": "GET /api/chat/health - Health check",
            "docs": "GET /docs - API documentation (Swagger UI)"
        },
        "features": [
            "Semantic search using neural networks",
            "Multilingual support (Bangla + English)",
            "Confidence scoring",
            "Category classification",
            "Real-time response"
        ],
        "example": {
            "request": {"message": "আপনাদের মধু কত দামে পাওয়া যায়?"},
            "response": {
                "response": "আমাদের তিন ধরনের মধু আছে...",
                "confidence": 0.95,
                "category": "pricing"
            }
        }
    }

@app.get("/api/chat/health", response_model=HealthResponse)
async def health_check():
    """Health check endpoint"""
    return HealthResponse(
        status="healthy",
        model_loaded=chatbot.loaded,
        data_points=len(chatbot.questions) if chatbot.questions else 0
    )

@app.post("/api/chat", response_model=ChatResponse)
async def chat_endpoint(request: ChatMessage):
    """
    Main chat endpoint
    
    Input: { "message": "Your question here" }
    Output: { "response": "Answer", "confidence": 0.95, ... }
    """
    
    if not chatbot.loaded:
        raise HTTPException(status_code=503, detail="Chatbot model not loaded")
    
    if not request.message or not request.message.strip():
        raise HTTPException(status_code=400, detail="Message cannot be empty")
    
    query = request.message.strip()
    
    try:
        # Get response
        result = chatbot.get_response(query)
        
        # Log interaction
        logger.info(f"Query: {query[:50]}... | Confidence: {result['confidence']:.2%} | Category: {result['category']}")
        
        return ChatResponse(
            response=result['response'],
            confidence=result['confidence'],
            category=result['category'],
            timestamp=datetime.now().isoformat(),
            similar_questions=result['similar_questions']
        )
        
    except Exception as e:
        logger.error(f"Error processing chat request: {str(e)}")
        raise HTTPException(status_code=500, detail="Error processing your message")

# ============================================================
# MAIN
# ============================================================

if __name__ == "__main__":
    import uvicorn
    
    print("=" * 60)
    print("🚀 MR Shop AI Chatbot - FastAPI Server")
    print("=" * 60)
    print("Starting server on http://0.0.0.0:5001")
    print("Documentation: http://localhost:5001/docs")
    print("=" * 60)
    
    uvicorn.run(
        "chatbot_api:app",
        host="0.0.0.0",
        port=5001,
        reload=False,
        log_level="info"
    )
