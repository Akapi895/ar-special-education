from datetime import datetime
from uuid import UUID

from pydantic import BaseModel, Field


class ActivityRequirementCreate(BaseModel):
    activity_id: UUID
    min_rounds: int = 1
    target_score: float | None = None


class TaskCreateRequest(BaseModel):
    title: str = Field(min_length=1, max_length=255)
    description: str | None = None
    difficulty: str = "easy"
    category: str | None = None
    estimated_minutes: int | None = None
    activity_requirements: list[ActivityRequirementCreate] | None = None


class TaskUpdateRequest(BaseModel):
    title: str | None = None
    description: str | None = None
    difficulty: str | None = None
    category: str | None = None
    estimated_minutes: int | None = None


class ActivityRequirementResponse(BaseModel):
    id: UUID
    activity_id: UUID
    min_rounds: int
    target_score: float | None

    class Config:
        from_attributes = True


class TaskResponse(BaseModel):
    id: UUID
    parent_id: UUID
    title: str
    description: str | None
    difficulty: str
    category: str | None
    estimated_minutes: int | None
    is_public: bool
    activity_requirements: list[ActivityRequirementResponse] = []
    created_at: datetime
    updated_at: datetime

    class Config:
        from_attributes = True


class ChildTaskResponse(BaseModel):
    id: UUID
    child_id: UUID
    task_id: UUID
    task: TaskResponse | None = None
    assigned_by: UUID
    status: str
    score: float | None
    started_at: datetime | None
    completed_at: datetime | None
    verified_at: datetime | None
    feedback: str | None
    given_up_reason: str | None
    created_at: datetime
    updated_at: datetime

    class Config:
        from_attributes = True


class AssignTaskRequest(BaseModel):
    pass


class CreateAndAssignRequest(BaseModel):
    title: str = Field(min_length=1, max_length=255)
    description: str | None = None
    difficulty: str = "easy"
    category: str | None = None
    estimated_minutes: int | None = None
    activity_requirements: list[ActivityRequirementCreate] | None = None


class CompleteTaskRequest(BaseModel):
    score: float | None = None


class VerifyTaskRequest(BaseModel):
    feedback: str | None = None


class RejectTaskRequest(BaseModel):
    feedback: str


class GiveUpTaskRequest(BaseModel):
    reason: str | None = None


class TaskStatusResponse(BaseModel):
    task_id: UUID
    child_task_id: UUID
    status: str
    score: float | None
    started_at: datetime | None
    completed_at: datetime | None
