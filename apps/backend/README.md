# Backend — AR Math Learning

Python FastAPI backend phục vụ chung cho Unity Client và Web Frontend.

## Công nghệ

- **FastAPI** (Python 3.11+)
- **SQLAlchemy 2.0** (async) + **asyncpg**
- **PostgreSQL**
- **JWT** (python-jose) + **bcrypt**
- **Alembic** (migrations)

## Cấu trúc

```
apps/backend/
├── app/
│   ├── api/v1/          # API routes
│   ├── core/            # Config, database, security
│   ├── models/          # SQLAlchemy models
│   ├── schemas/         # Pydantic schemas
│   ├── services/        # Business logic
│   └── repositories/    # Data access layer
├── tests/
├── migrations/
├── requirements.txt
└── .env.example
```

## Cài đặt

```bash
cp .env.example .env
pip install -r requirements.txt
```

## Chạy

```bash
uvicorn app.main:app --reload
```

API docs tại `http://localhost:8000/docs`.
