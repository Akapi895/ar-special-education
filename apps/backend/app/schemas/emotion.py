from datetime import datetime
from uuid import UUID

from pydantic import BaseModel, Field


class EmotionRecordCreate(BaseModel):
    emotion: str = Field(min_length=1)
    intensity: int | None = Field(default=None, ge=1, le=5)
    note: str | None = None
    source: str = "parent"
    session_id: UUID | None = None


class EmotionRecordResponse(BaseModel):
    id: UUID
    child_id: UUID
    session_id: UUID | None
    emotion: str
    intensity: int | None
    note: str | None
    source: str
    recorded_at: datetime

    class Config:
        from_attributes = True
