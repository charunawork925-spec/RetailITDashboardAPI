# ============================================================
# InsightIQ - Database Connection
# database.py
# ============================================================

import psycopg2
import os

# ── Edit these to match your PostgreSQL setup ──────────────
DB_CONFIG = {
    "host":     os.getenv("DB_HOST",     "localhost"),
    "port":     int(os.getenv("DB_PORT", 5432)),
    "database": os.getenv("DB_NAME",     "insightiq"),
    "user":     os.getenv("DB_USER",     "postgres"),
    "password": os.getenv("DB_PASSWORD", "1234"),  
}

def get_connection():
    """Return a new psycopg2 connection"""
    return psycopg2.connect(**DB_CONFIG)
