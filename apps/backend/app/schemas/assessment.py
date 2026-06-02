from datetime import datetime
from uuid import UUID

from pydantic import BaseModel, Field


class AssessmentCreateRequest(BaseModel):
    type: str = "initial"
    answers: dict
    recommendations: list[str] | None = None


class SimpleAssessmentRequest(BaseModel):
    answers: dict
    recommendations: list[str] | None = None


class AssessmentUpdateRequest(BaseModel):
    answers: dict | None = None
    summary: dict | None = None
    recommendations: list[str] | None = None
    completed_at: datetime | None = None


class AssessmentResponse(BaseModel):
    id: UUID
    child_id: UUID
    type: str
    answers: dict
    summary: dict | None
    recommendations: list[str] | None
    completed_at: datetime | None
    created_at: datetime

    class Config:
        from_attributes = True
