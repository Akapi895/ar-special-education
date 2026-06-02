from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.database import get_db
from app.schemas.sync import (
    SyncSessionStart,
    SyncSessionEnd,
    SyncResultsBatch,
)
from app.schemas.progress import SessionResponse, ActivityResultResponse, ActivityResponse
from app.services.sync_service import SyncService

router = APIRouter()


@router.post("/sync/session/start", response_model=SessionResponse)
async def sync_start_session(request: SyncSessionStart, db: AsyncSession = Depends(get_db)):
    svc = SyncService(db)
    session = await svc.start_session(
        request.child_id, request.activity_id, request.device_id, request.start_time
    )
    return session


@router.post("/sync/session/end", response_model=SessionResponse)
async def sync_end_session(request: SyncSessionEnd, db: AsyncSession = Depends(get_db)):
    svc = SyncService(db)
    return await svc.end_session(request.session_id, request.end_time, request.status)


@router.post("/sync/results", response_model=list[ActivityResultResponse])
async def sync_results(request: SyncResultsBatch, db: AsyncSession = Depends(get_db)):
    svc = SyncService(db)
    results = await svc.sync_results(
        request.child_id,
        [r.model_dump() for r in request.results],
    )
    return results


@router.get("/sync/activities")
async def sync_activities(db: AsyncSession = Depends(get_db)):
    svc = SyncService(db)
    return await svc.get_activities()


@router.get("/sync/lessons")
async def sync_lessons(db: AsyncSession = Depends(get_db)):
    svc = SyncService(db)
    activities = await svc.get_activities()
    lessons = []
    for act in activities:
        lessons.extend(act.get("lessons", []))
    return lessons
