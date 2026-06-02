from uuid import UUID

from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.api.deps import get_current_parent
from app.core.database import get_db
from app.schemas.progress import ActivityResponse, LessonResponse, SessionResponse
from app.repositories import ActivityRepository, SessionRepository

router = APIRouter()


@router.get("/parent/children/{child_id}/sessions", response_model=list[SessionResponse])
async def list_sessions(child_id: UUID, parent=Depends(get_current_parent),
                         db: AsyncSession = Depends(get_db)):
    repo = SessionRepository(db)
    return await repo.get_by_child(child_id)
