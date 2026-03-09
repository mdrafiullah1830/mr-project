#!/usr/bin/env python3
"""
MR Shop AI Chatbot - Dataset Preparation & Model Training
========================================================

This script:
1. Loads Q&A dataset
2. Prepares data for training
3. Trains a semantic search model using HuggingFace
4. Saves the model for later use

Dataset Format:
- Multilingual (Bangla + English)
- Questions paired with answers
- Category and keyword tags for better retrieval
"""

import json
import numpy as np
from pathlib import Path
from sklearn.preprocessing import normalize
import pickle

# ============================================================
# STEP 1: LOAD AND PARSE DATASET
# ============================================================

def load_dataset(json_path):
    """Load training dataset from JSON file"""
    print(f"Loading dataset from {json_path}...")
    
    with open(json_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    qa_pairs = data['shop_qa_dataset']
    print(f"✅ Loaded {len(qa_pairs)} Q&A pairs\n")
    
    return qa_pairs

# ============================================================
# STEP 2: INSTALL REQUIRED PACKAGES
# ============================================================

def install_dependencies():
    """Install required packages"""
    packages = [
        'sentence-transformers',  # For semantic embeddings
        'torch',                   # PyTorch (required by transformers)
        'transformers',            # HuggingFace models
        'numpy',
        'scikit-learn'
    ]
    
    print("Checking and installing dependencies...")
    import subprocess
    import sys
    
    for package in packages:
        try:
            __import__(package.replace('-', '_'))
            print(f"  ✅ {package}")
        except ImportError:
            print(f"  📥 Installing {package}...")
            subprocess.check_call([sys.executable, "-m", "pip", "install", package, "-q"])
            print(f"  ✅ {package}")
    print()

# ============================================================
# STEP 3: CREATE EMBEDDINGS
# ============================================================

def create_embeddings(qa_pairs):
    """
    Create semantic embeddings for all questions using sentence-transformers
    
    Why sentence-transformers?
    - Lightweight and fast
    - Works for both English and Bangla
    - Pre-trained on millions of sentences
    - CPU-friendly (no GPU required)
    """
    from sentence_transformers import SentenceTransformer
    
    print("Loading embedding model...")
    # Using multilingual model that works for 50+ languages including Bangla
    model = SentenceTransformer('sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2')
    print("✅ Embedding model loaded\n")
    
    print("Creating embeddings for all questions...")
    embeddings = []
    questions = []
    answers = []
    categories = []
    
    for item in qa_pairs:
        question = item['question']
        answer = item['answer']
        category = item['category']
        
        # Create embedding for the question
        embedding = model.encode(question, convert_to_numpy=True)
        
        embeddings.append(embedding)
        questions.append(question)
        answers.append(answer)
        categories.append(category)
    
    embeddings = np.array(embeddings)
    print(f"✅ Created {len(embeddings)} embeddings\n")
    
    return embeddings, questions, answers, categories, model

# ============================================================
# STEP 4: SAVE MODEL & DATA
# ============================================================

def save_model_and_data(embeddings, questions, answers, categories, model, output_dir):
    """Save embeddings, data, and model for inference"""
    
    output_path = Path(output_dir)
    output_path.mkdir(parents=True, exist_ok=True)
    
    print("Saving model and data...")
    
    # Save embeddings
    np.save(output_path / 'question_embeddings.npy', embeddings)
    print(f"  ✅ Embeddings saved")
    
    # Save metadata as JSON
    metadata = {
        'questions': questions,
        'answers': answers,
        'categories': categories,
        'total_pairs': len(questions)
    }
    
    with open(output_path / 'metadata.json', 'w', encoding='utf-8') as f:
        json.dump(metadata, f, ensure_ascii=False, indent=2)
    print(f"  ✅ Metadata saved ({len(questions)} pairs)")
    
    # Save the embedding model (lightweight)
    # Note: We're using a pre-trained model, so we just save reference
    model_info = {
        'model_name': 'sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2',
        'embedding_dim': embeddings.shape[1],
        'timestamp': str(Path(output_path).absolute())
    }
    
    with open(output_path / 'model_info.json', 'w') as f:
        json.dump(model_info, f, indent=2)
    print(f"  ✅ Model info saved")
    
    print(f"\n✅ Model saved to: {output_path}\n")
    
    return output_path

# ============================================================
# STEP 5: SEMANTIC SEARCH FUNCTION (FOR INFERENCE)
# ============================================================

def search_similar_question(query, embeddings, questions, model, top_k=3):
    """
    Find the most similar question in our database
    
    How it works:
    1. Convert query to embedding
    2. Calculate similarity with all stored questions
    3. Return top-k most similar matches
    """
    
    # Get embedding for user query
    query_embedding = model.encode(query, convert_to_numpy=True)
    
    # Calculate cosine similarity with all stored questions
    from sklearn.metrics.pairwise import cosine_similarity
    similarities = cosine_similarity([query_embedding], embeddings)[0]
    
    # Get indices of top matches
    top_indices = np.argsort(similarities)[::-1][:top_k]
    
    # Return top matches with scores
    results = []
    for idx in top_indices:
        results.append({
            'question': questions[idx],
            'similarity': float(similarities[idx]),
            'index': int(idx)
        })
    
    return results

# ============================================================
# MAIN EXECUTION
# ============================================================

def main():
    """Main training pipeline"""
    
    print("=" * 60)
    print("🤖 MR Shop AI Chatbot - Training Pipeline")
    print("=" * 60)
    print()
    
    # Paths
    dataset_path = Path(__file__).parent / 'training_dataset.json'
    model_output_dir = Path(__file__).parent / 'models'
    
    # Step 1: Install dependencies
    install_dependencies()
    
    # Step 2: Load dataset
    qa_pairs = load_dataset(dataset_path)
    
    # Step 3: Create embeddings
    embeddings, questions, answers, categories, model = create_embeddings(qa_pairs)
    
    # Step 4: Save model and data
    save_model_and_data(embeddings, questions, answers, categories, model, model_output_dir)
    
    # Step 5: Test the model with a sample query
    print("=" * 60)
    print("Testing semantic search with sample queries...")
    print("=" * 60)
    print()
    
    test_queries = [
        "মধু কত দামে পাওয়া যায়?",
        "What is the price of honey?",
        "কিভাবে অর্ডার করব?",
        "How long for delivery?",
        "ফেরত দেওয়া যায় কিনা?"
    ]
    
    for query in test_queries:
        print(f"Query: {query}")
        results = search_similar_question(query, embeddings, questions, model, top_k=2)
        
        for i, result in enumerate(results, 1):
            print(f"  {i}. {result['question']}")
            print(f"     Similarity: {result['similarity']:.2%}")
        print()
    
    print("=" * 60)
    print("✅ Training complete! Model ready for inference.")
    print("=" * 60)

if __name__ == '__main__':
    main()
