from datetime import date, datetime
from uuid import UUID

from pydantic import BaseModel


class ReportResponse(BaseModel):
    id: UUID
    child_id: UUID
    type: str
    title_vi: str
    title_en: str | None
    summary: str | None
    data: dict
    pdf_url: str | None
    period_start: date
    period_end: date
    generated_at: datetime
    is_read: bool

    class Config:
        from_attributes = True


class GenerateReportRequest(BaseModel):
    type: str = "monthly"
    period_start: date | None = None
    period_end: date | None = None
