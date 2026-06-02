from datetime import datetime
from uuid import UUID

from pydantic import BaseModel, EmailStr, Field


class ParentRegisterRequest(BaseModel):
    email: EmailStr
    password: str = Field(min_length=6, max_length=128)
    full_name: str = Field(min_length=1, max_length=255)
    phone: str | None = None


class ParentLoginRequest(BaseModel):
    email: EmailStr
    password: str


class TokenResponse(BaseModel):
    access_token: str
    refresh_token: str
    token_type: str = "bearer"


class TokenRefreshRequest(BaseModel):
    refresh_token: str


class ParentProfileResponse(BaseModel):
    id: UUID
    email: str
    full_name: str
    phone: str | None
    avatar_url: str | None
    role: str
    notification_settings: dict
    is_verified: bool
    created_at: datetime

    class Config:
        from_attributes = True


class ParentUpdateRequest(BaseModel):
    full_name: str | None = None
    phone: str | None = None
    avatar_url: str | None = None


class PasswordChangeRequest(BaseModel):
    current_password: str
    new_password: str = Field(min_length=6, max_length=128)


class NotificationSettingsUpdate(BaseModel):
    progress_report: bool | None = None
    emotion_alert: bool | None = None
    task_reminder: bool | None = None


class ChildLoginRequest(BaseModel):
    username: str
    password: str


class ChildAuthResponse(BaseModel):
    access_token: str
    refresh_token: str
    token_type: str = "bearer"
    child_id: UUID


class ErrorResponse(BaseModel):
    detail: str
