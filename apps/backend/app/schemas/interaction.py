from datetime import datetime
from uuid import UUID

from pydantic import BaseModel


class InteractionCreate(BaseModel):
    content: str
    metadata: dict | None = None


class InteractionResponse(BaseModel):
    id: UUID
    child_id: UUID
    parent_id: UUID
    type: str
    content: str
    metadata: dict | None
    created_at: datetime

    class Config:
        from_attributes = True
