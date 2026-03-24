# InsightIQ — Backend Setup (Windows)

## Folder Structure
```
insightiq/
├── backend/
│   ├── main.py
│   ├── database.py
│   ├── requirements.txt
│   └── README.md
└── frontend/          ← Angular project goes here
```

## Step 1 — Install Python
1. Download Python 3.11 from https://python.org/downloads
2. Run installer → CHECK "Add Python to PATH" → Install
3. Open NEW Command Prompt, verify:
   python --version
   pip --version

## Step 2 — Set Up the Backend

Open Command Prompt in the backend/ folder:

# Create a virtual environment
python -m venv venv

# Activate it
venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt

## Step 3 — Configure Database Password

Open database.py and change:
    "password": "your_password_here"
to your actual PostgreSQL password.

## Step 4 — Run the Server

uvicorn main:app --reload --port 8000

## Step 5 — Test It

Open browser and visit:
  http://localhost:8000              → Health check
  http://localhost:8000/docs         → Swagger UI (test all endpoints)
  http://localhost:8000/api/datasets → Dataset metadata
  http://localhost:8000/api/kpis     → Live KPI values
  http://localhost:8000/api/top10    → Top 10 products

## API Endpoints Summary

GET  /api/datasets          → All dataset + field definitions
GET  /api/kpis              → Dashboard KPI cards
GET  /api/dashboard/charts  → Pre-built chart data
GET  /api/top10             → Top 10 products
GET  /api/presets           → Report preset configs
POST /api/query             → Dynamic report query engine

## POST /api/query — Example Body

{
  "dataset": "products",
  "dimensions": ["category"],
  "measures": ["revenue_ytd", "units_sold_ytd"],
  "filters": [],
  "limit": 20,
  "order_by": "revenue_ytd",
  "order_dir": "DESC"
}

Response:
{
  "dataset": "products",
  "columns": ["category", "revenue_ytd", "units_sold_ytd"],
  "rows": [
    {"category": "Dairy & Eggs", "revenue_ytd": 48200000, "units_sold_ytd": 312000},
    ...
  ],
  "count": 12
}
