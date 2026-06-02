from uuid import UUID

from fastapi import APIRouter, Depends, Query
from sqlalchemy.ext.asyncio import AsyncSession

from app.api.deps import get_current_parent
from app.core.database import get_db
from app.schemas.interaction import InteractionCreate, InteractionResponse
from app.services.interaction_service import InteractionService

router = APIRouter()


@router.post("/parent/children/{child_id}/interact/chat")
async def chat_interaction(child_id: UUID, request: InteractionCreate,
                            parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = InteractionService(db)
    interaction = await svc.create_chat(child_id, parent.id, request.content, request.metadata)
    return InteractionResponse.model_validate(interaction)


@router.get("/parent/children/{child_id}/interact/logs")
async def get_interaction_logs(child_id: UUID, skip: int = Query(0, ge=0), limit: int = Query(50, ge=1, le=200),
                                parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = InteractionService(db)
    logs = await svc.get_logs(child_id, skip, limit)
    return [InteractionResponse.model_validate(log) for log in logs]


@router.get("/parent/children/{child_id}/interact/history")
async def get_chat_history(child_id: UUID, skip: int = Query(0, ge=0), limit: int = Query(50, ge=1, le=200),
                            parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = InteractionService(db)
    history = await svc.get_chat_history(child_id, skip, limit)
    return [InteractionResponse.model_validate(h) for h in history]
