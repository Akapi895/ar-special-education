from datetime import datetime
from uuid import UUID

from pydantic import BaseModel, Field


class SyncSessionStart(BaseModel):
    child_id: UUID
    activity_id: UUID
    device_id: str | None = None
    start_time: datetime | None = None


class SyncSessionEnd(BaseModel):
    session_id: UUID
    end_time: datetime | None = None
    status: str = "completed"


class SyncResultItem(BaseModel):
    session_id: UUID
    round_id: str
    activity_id: UUID
    lesson_id: UUID | None = None
    level_number: int = 1
    difficulty: str = "easy"
    is_correct: bool
    total_attempts: int = 1
    hints_used_count: int = 0
    time_spent_seconds: float = 0
    start_time: datetime
    end_time: datetime
    counts_toward_mastery: bool = True
    error_type: str | None = None
    technical_issue: dict | None = None
    skill_tags: list[str] = []
    additional_data: dict | None = None


class SyncResultsBatch(BaseModel):
    child_id: UUID
    results: list[SyncResultItem]
