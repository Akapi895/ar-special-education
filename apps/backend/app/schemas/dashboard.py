from uuid import UUID

from pydantic import BaseModel


class CategoryProgress(BaseModel):
    activity_id: UUID
    activity_code: str
    activity_title_vi: str
    total_sessions: int
    total_attempts: int
    correct_attempts: int
    success_rate: float
    avg_time_per_question: float


class EmotionAnalytics(BaseModel):
    emotion: str
    count: int
    avg_intensity: float
    percentage: float


class ChildPointsSummary(BaseModel):
    total_earned: int
    total_spent: int
    current_balance: int


class DashboardResponse(BaseModel):
    child_id: UUID
    total_sessions: int
    total_activities_completed: int
    overall_success_rate: float
    total_time_spent_minutes: float
    current_streak: int
    strongest_skill: str | None
    weakest_skill: str | None
    points: ChildPointsSummary
    category_progress: list[CategoryProgress] = []
    recent_emotions: list[EmotionAnalytics] = []


class AnalyzeEmotionReportRequest(BaseModel):
    child_id: UUID


class UpdateSkillsRequest(BaseModel):
    skill_tag: str | None = None
