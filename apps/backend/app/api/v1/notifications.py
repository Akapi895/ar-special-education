from uuid import UUID

from fastapi import APIRouter, Depends, Query
from sqlalchemy.ext.asyncio import AsyncSession

from app.api.deps import get_current_parent
from app.core.database import get_db
from app.schemas.notification import NotificationResponse, UnreadCountResponse
from app.services.notification_service import NotificationService

router = APIRouter()


@router.get("/parent/notifications", response_model=list[NotificationResponse])
async def list_notifications(skip: int = Query(0, ge=0), limit: int = Query(50, ge=1, le=200),
                              parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = NotificationService(db)
    return await svc.get_notifications(parent.id, skip, limit)


@router.put("/parent/notifications/{notification_id}/read")
async def mark_notification_read(notification_id: UUID, parent=Depends(get_current_parent),
                                  db: AsyncSession = Depends(get_db)):
    svc = NotificationService(db)
    await svc.mark_as_read(notification_id)
    return {"detail": "Marked as read"}


@router.put("/parent/notifications/read-all")
async def mark_all_read(parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = NotificationService(db)
    await svc.mark_all_as_read(parent.id)
    return {"detail": "All notifications marked as read"}


@router.get("/parent/notifications/unread-count", response_model=UnreadCountResponse)
async def unread_count(parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = NotificationService(db)
    count = await svc.count_unread(parent.id)
    return UnreadCountResponse(unread_count=count)
