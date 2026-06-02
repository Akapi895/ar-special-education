from datetime import datetime
from uuid import UUID

from pydantic import BaseModel, Field


class ChildCreateRequest(BaseModel):
    username: str = Field(min_length=1, max_length=50)
    password: str = Field(min_length=4, max_length=128)
    display_name: str = Field(min_length=1, max_length=100)
    age_years: int = Field(ge=2, le=18)
    grade: str | None = None


class ChildUpdateRequest(BaseModel):
    username: str | None = None
    password: str | None = None
    display_name: str | None = None
    age_years: int | None = None
    grade: str | None = None
    preferences: dict | None = None


class ChildResponse(BaseModel):
    id: UUID
    parent_id: UUID
    username: str
    display_name: str
    age_years: int
    grade: str | None
    avatar_equipped: UUID | None
    preferences: dict
    is_active: bool
    last_active_at: datetime | None
    created_at: datetime

    class Config:
        from_attributes = True


class OnboardingCompleteRequest(BaseModel):
    parent: ParentOnboardingData
    children: list[ChildOnboardingData]
    assessments: list[AssessmentOnboardingData] | None = None


class ParentOnboardingData(BaseModel):
    full_name: str
    phone: str | None = None


class ChildOnboardingData(BaseModel):
    username: str
    password: str
    display_name: str
    age_years: int
    grade: str | None = None


class AssessmentOnboardingData(BaseModel):
    child_username: str
    answers: dict
    recommendations: list[str] | None = None


class OnboardingResponse(BaseModel):
    parent: ParentProfileResponse
    children: list[ChildResponse]

    from app.schemas.auth import ParentProfileResponse
