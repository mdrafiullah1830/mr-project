# MR Shop Secure Admin Panel

The admin console is a standalone Flask app that lives at the hidden route `/mrshop-admin-a847ks09`. It is intentionally unlinked from the public UI, restricted to `localhost`, and requires session-based authentication.

## Capabilities

- **Guarded entry** – Only `127.0.0.1/::1` may reach the route. Every request is audited in `data/security_log.json`.
- **Session login** – Default credentials `mrshop18 / MRsho18admin` are stored (hashed) in `data/admin.json`. Passwords are verified with Werkzeug.
- **Realtime dashboard** – View live counts for products, categories, sellers, VIP users, and aggregated revenue.
- **JSON persistence** – CRUD operations directly edit the store JSON files (`products.json`, `categories.json`, `sellers.json`, `users.json`, `settings.json`).
- **Security log** – Every login, logout, IP block, and CRUD mutation is recorded with timestamp + IP.

## Running the admin server

```bash
cd backend
source .venv/bin/activate  # or python3 -m venv .venv && source .venv/bin/activate
pip install -r requirements.txt
"$PWD/.venv/bin/python" admin.py
```

The server binds to `127.0.0.1:5010` by default. Visit `http://localhost:5010/mrshop-admin-a847ks09` to reach the login screen.

> **Important:** Deploying beyond localhost requires placing the app behind a reverse proxy that still terminates on localhost or expanding the `ALLOWED_LOCAL_IPS` set.

## Data files

| File | Purpose |
| ---- | ------- |
| `data/categories.json` | Category metadata including slug + visibility |
| `data/products.json` | Product catalog with stock, price, status |
| `data/sellers.json` | Marketplace partner roster |
| `data/users.json` | Customer insights feeding analytics |
| `data/settings.json` | Global storefront settings (name, hero message, maintenance flag) |
| `data/security_log.json` | Rotating security/audit history |
| `data/admin.json` | Hashed admin credentials |

All helper endpoints read/write via a thread-safe JSON utility inside `backend/admin.py`.

## API overview

| Route | Method | Description |
| ----- | ------ | ----------- |
| `/mrshop-admin-a847ks09/login` | GET/POST | Session login form |
| `/mrshop-admin-a847ks09/dashboard` | GET | Main dashboard UI |
| `/mrshop-admin-a847ks09/api/data` | GET | Aggregated dashboard payload |
| `/mrshop-admin-a847ks09/api/categories` | POST | Create category |
| `/mrshop-admin-a847ks09/api/categories/<slug>` | PATCH/DELETE | Update or delete category |
| `/mrshop-admin-a847ks09/api/products` | POST | Create product |
| `/mrshop-admin-a847ks09/api/products/<id>` | PATCH/DELETE | Update/delete product |
| `/mrshop-admin-a847ks09/api/sellers` | POST | Add seller |
| `/mrshop-admin-a847ks09/api/sellers/<id>` | PATCH/DELETE | Update/delete seller |
| `/mrshop-admin-a847ks09/api/settings` | PATCH | Save global settings |
| `/mrshop-admin-a847ks09/api/health` | GET | Local-only liveness probe |

All API endpoints return JSON payloads with a `success` flag and are session protected.

## Customisation tips

- Update `DEFAULT_ADMIN_USERNAME` / `DEFAULT_ADMIN_PASSWORD` in `backend/admin.py` or replace `data/admin.json` with your own hashed credentials.
- Expand `ALLOWED_LOCAL_IPS` if the console must be accessed from additional internal addresses.
- Edit `assets/css/admin.css` or `assets/js/admin.js` to adjust the look and feel of the dashboard.
- Extend the `security_log.json` retention by changing `MAX_LOG_ENTRIES`.
