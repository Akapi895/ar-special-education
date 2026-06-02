# AR Math Learning

An augmented reality application that helps children learn basic math through interactive AR activities, paired with a parent dashboard for progress tracking.

## Architecture

```
├── apps/
│   ├── unity-client/     # AR learning app (Unity)
│   ├── frontend/         # Parent dashboard web app (React)
│   └── backend/          # API server (Python FastAPI)
├── docs/                 # Documentation & reports
└── scripts/              # Build & dev automation
```

### Components

| Component | Tech | Description |
|-----------|------|-------------|
| **Unity Client** | Unity 6000, AR Foundation | AR-based math learning activities for children |
| **Frontend** | React 19, Vite, TailwindCSS | Parent dashboard for monitoring progress, managing tasks & rewards |
| **Backend** | Python FastAPI, PostgreSQL, SQLAlchemy | Shared API serving both Unity and web frontend |

## Activities

| Activity | Description |
|----------|-------------|
| **Quantity Match** | Match numbers with corresponding quantities |
| **Compare Quantity** | Compare two groups (more / less / equal) |
| **Number Line Jump** | Jump along a number line to reach a target |
| **Number Bonds** | Decompose numbers into parts |

## Backend

Located at `apps/backend/`. FastAPI server with JWT authentication, async SQLAlchemy, and PostgreSQL.

### API Modules

- **Auth** — Parent & child registration, login, token refresh
- **Children** — Child profile CRUD, onboarding
- **Tasks** — Task library, assignment, completion workflow
- **Progress** — Session & activity results, skill mastery
- **Rewards** — Shop, redemption, inventory, avatar customization
- **Dashboard** — Aggregated progress & analytics
- **Reports** — Weekly/monthly/emotion reports
- **Notifications** — Push-style parent notifications
- **Sync** — Unity client data synchronization

## Getting Started

```bash
# Backend
cd apps/backend
cp .env.example .env
pip install -r requirements.txt
uvicorn app.main:app --reload

# Frontend
cd apps/frontend
npm install
npm run dev

# Unity client
# Open apps/unity-client in Unity 6000.0.71f1
```

## License

MIT
