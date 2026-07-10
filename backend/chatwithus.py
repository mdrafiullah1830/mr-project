#!/usr/bin/env python3
"""
Simple local website QA assistant.

Usage:
  python chatwithus.py            # starts interactive REPL
  python chatwithus.py --index    # re-index files and exit

What it does:
- Scans the project directory for .html/.htm/.md/.txt files
- Extracts visible text (strips HTML tags and scripts/styles)
- Builds a lightweight TF-IDF index (pure Python, no external deps)
- In REPL, accepts user questions and returns the most relevant snippets and source files

Notes:
- This is an offline, local tool. It doesn't call any external API.
- It's intended to help answer site-related questions by searching the site's content.
"""

import os
import re
import math
import argparse
from collections import Counter, defaultdict

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
FILE_EXTENSIONS = ('.html', '.htm', '.md', '.txt')

# small English stopword set for better matching
STOPWORDS = set("""
 a about above after again against all am an and any are aren't as at be because been before being below between both but by can could couldn't did didn't do does doesn't doing don't down during each few for from further had hadn't has hasn't have haven't having he he'd he'll he's her here here's hers herself him himself his how how's i i'd i'll i'm i've if in into is isn't it it's its itself let's me more most mustn't my myself no nor not of off on once only or other ought our ours ourselves out over own same shan't she she'd she'll she's should shouldn't so some such than that that's the their theirs them themselves then there there's these they they'd they'll they're they've this those through to too under until up very was wasn't we we'd we'll we're we've were weren't what what's when when's where where's which while who who's whom why why's with won't would wouldn't you you'd you'll you're you've your yours yourself yourselves
""".split())

RE_TAGS = re.compile(r'<(script|style).*?>.*?</\1>', re.S | re.I)
RE_HTML = re.compile(r'<[^>]+>')
RE_WHITESPACE = re.compile(r'\s+')
RE_TOKEN = re.compile(r"\w+", re.UNICODE)


def extract_text_from_html(text):
    """Strip scripts/styles and HTML tags, return plain text."""
    no_script = RE_TAGS.sub(' ', text)
    no_tags = RE_HTML.sub(' ', no_script)
    collapsed = RE_WHITESPACE.sub(' ', no_tags)
    return collapsed.strip()


def load_documents(root):
    docs = []
    for dirpath, dirnames, filenames in os.walk(root):
        for fn in filenames:
            if fn.startswith('.'):
                continue
            if fn.lower().endswith(FILE_EXTENSIONS):
                path = os.path.join(dirpath, fn)
                try:
                    with open(path, 'r', encoding='utf-8', errors='replace') as f:
                        raw = f.read()
                except Exception:
                    continue
                if fn.lower().endswith(('.html', '.htm')):
                    text = extract_text_from_html(raw)
                else:
                    text = raw
                if len(text) < 10:
                    continue
                docs.append({'path': os.path.relpath(path, ROOT), 'text': text})
    return docs


def tokenize(s):
    s = s.lower()
    tokens = RE_TOKEN.findall(s)
    return [t for t in tokens if t not in STOPWORDS]


def build_index(docs):
    """Build TF-IDF index on document chunks. Returns structures for querying."""
    # Break each document into chunks (paragraphs) to return focused snippets
    chunks = []  # each chunk: {id, doc_path, text}
    cid = 0
    for d in docs:
        # split on two or more newlines or by sentence-like chunks
        paras = re.split(r'\n{2,}|\r{2,}', d['text'])
        for p in paras:
            p = p.strip()
            if len(p) < 30:
                continue
            chunks.append({'id': cid, 'path': d['path'], 'text': p})
            cid += 1
    if not chunks:
        # fallback: use full docs
        for d in docs:
            chunks.append({'id': cid, 'path': d['path'], 'text': d['text']})
            cid += 1

    # compute term frequencies and document frequencies
    term_freqs = {}
    df = Counter()
    for c in chunks:
        toks = tokenize(c['text'])
        tf = Counter(toks)
        term_freqs[c['id']] = tf
        for t in set(tf.keys()):
            df[t] += 1

    N = len(chunks)
    idf = {}
    for term, freq in df.items():
        idf[term] = math.log((N + 1) / (freq + 1)) + 1.0

    # compute TF-IDF vectors (sparse dicts)
    tfidf = {}
    norms = {}
    for cid, tf in term_freqs.items():
        vec = {}
        norm = 0.0
        for term, cnt in tf.items():
            val = (1 + math.log(cnt)) * idf.get(term, 0.0)
            vec[term] = val
            norm += val * val
        norm = math.sqrt(norm) if norm > 0 else 1.0
        # normalize
        for term in vec:
            vec[term] /= norm
        tfidf[cid] = vec
        norms[cid] = norm

    index = {
        'chunks': chunks,
        'tfidf': tfidf,
        'idf': idf,
    }
    return index


def query_index(index, question, top_k=3):
    q_tokens = tokenize(question)
    if not q_tokens:
        return []
    # build query vector
    q_tf = Counter(q_tokens)
    q_vec = {}
    qnorm = 0.0
    for term, cnt in q_tf.items():
        val = (1 + math.log(cnt)) * index['idf'].get(term, math.log(len(index['chunks'])+1))
        q_vec[term] = val
        qnorm += val * val
    qnorm = math.sqrt(qnorm) if qnorm>0 else 1.0
    for term in q_vec:
        q_vec[term] /= qnorm

    # compute cosine similarity with chunks
    scores = []
    for c in index['chunks']:
        cid = c['id']
        vec = index['tfidf'].get(cid, {})
        # dot product
        dot = 0.0
        for t, v in q_vec.items():
            dot += v * vec.get(t, 0.0)
        if dot > 0:
            scores.append((dot, c))
    scores.sort(key=lambda x: x[0], reverse=True)
    return scores[:top_k]


def pretty_print_results(results):
    if not results:
        print("Sorry — I couldn't find any relevant content in the site files.")
        return
    for score, chunk in results:
        print('\n---')
        print(f"File: {chunk['path']}  (score: {score:.3f})")
        snippet = chunk['text']
        # trim snippet to reasonable length
        if len(snippet) > 800:
            snippet = snippet[:800].rsplit(' ',1)[0] + '...'
        print('\n' + snippet + '\n')


def interactive_loop(index):
    print('Local site assistant — ask questions about the website content. Type "quit" to exit.')
    try:
        while True:
            q = input('\nYou: ').strip()
            if not q:
                continue
            if q.lower() in ('quit', 'exit'):
                print('Goodbye!')
                break
            results = query_index(index, q, top_k=4)
            pretty_print_results(results)
    except (KeyboardInterrupt, EOFError):
        print('\nBye')


def main():
    parser = argparse.ArgumentParser(description='Local website Q&A assistant')
    parser.add_argument('--index-only', action='store_true', help='Index files and exit')
    args = parser.parse_args()

    print('Scanning project files under', ROOT)
    docs = load_documents(ROOT)
    print(f'Found {len(docs)} document files.')
    index = build_index(docs)
    print(f'Indexed {len(index["chunks"])} content chunks.')

    if args.index_only:
        print('Indexing completed. Exiting.')
        return

    interactive_loop(index)

if __name__ == '__main__':
    main()
