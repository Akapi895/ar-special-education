from datetime import datetime
from uuid import UUID

from pydantic import BaseModel


class NotificationResponse(BaseModel):
    id: UUID
    parent_id: UUID
    child_id: UUID | None
    category: str
    title_vi: str
    title_en: str | None
    body_vi: str
    body_en: str | None
    data: dict | None
    is_read: bool
    created_at: datetime

    class Config:
        from_attributes = True


class UnreadCountResponse(BaseModel):
    unread_count: int
