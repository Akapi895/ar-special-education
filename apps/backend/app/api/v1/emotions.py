from uuid import UUID

from fastapi import APIRouter, Depends, Query
from sqlalchemy.ext.asyncio import AsyncSession

from app.api.deps import get_current_parent
from app.core.database import get_db
from app.schemas.emotion import EmotionRecordCreate, EmotionRecordResponse
from app.services.emotion_service import EmotionService

router = APIRouter()


@router.post("/parent/children/{child_id}/interact/emotion", response_model=EmotionRecordResponse)
async def create_emotion(child_id: UUID, request: EmotionRecordCreate,
                          parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = EmotionService(db)
    return await svc.create_record(
        child_id=child_id,
        emotion=request.emotion,
        intensity=request.intensity,
        note=request.note,
        source=request.source,
        session_id=request.session_id,
    )


@router.get("/parent/children/{child_id}/emotions", response_model=list[EmotionRecordResponse])
async def list_emotions(child_id: UUID, skip: int = Query(0, ge=0), limit: int = Query(50, ge=1, le=200),
                         parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = EmotionService(db)
    return await svc.get_records(child_id, skip, limit)
