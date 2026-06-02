from datetime import datetime
from uuid import UUID

from pydantic import BaseModel, Field


class ActivityResultResponse(BaseModel):
    id: UUID
    child_id: UUID
    session_id: UUID
    activity_id: UUID
    lesson_id: UUID | None
    round_id: str
    level_number: int
    difficulty: str
    is_correct: bool
    total_attempts: int
    hints_used_count: int
    time_spent_seconds: float
    start_time: datetime
    end_time: datetime
    counts_toward_mastery: bool
    error_type: str | None
    technical_issue: dict | None
    skill_tags: list[str]
    additional_data: dict | None
    created_at: datetime

    class Config:
        from_attributes = True


class SessionResponse(BaseModel):
    id: UUID
    child_id: UUID
    activity_id: UUID
    device_id: str | None
    start_time: datetime
    end_time: datetime | None
    duration_seconds: int | None
    status: str
    created_at: datetime

    class Config:
        from_attributes = True


class SkillMasteryResponse(BaseModel):
    id: UUID
    child_id: UUID
    skill_tag: str
    total_attempts: int
    correct_attempts: int
    mastery_score: float
    last_practiced: datetime | None
    updated_at: datetime

    class Config:
        from_attributes = True


class ActivityResponse(BaseModel):
    id: UUID
    code: str
    title_vi: str
    title_en: str
    description_vi: str | None
    description_en: str | None
    icon_url: str | None
    sort_order: int
    is_active: bool

    class Config:
        from_attributes = True


class LessonResponse(BaseModel):
    id: UUID
    activity_id: UUID
    code: str
    title_vi: str
    title_en: str
    difficulty: str
    skill_tags: list[str]
    prerequisites: list[UUID]
    sort_order: int
    is_active: bool

    class Config:
        from_attributes = True
