import uuid
from datetime import datetime, timezone

from sqlalchemy import Boolean, DateTime, ForeignKey, Numeric, SmallInteger, String
from sqlalchemy.dialects.postgresql import JSONB, UUID, ARRAY
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.core.database import Base


class ActivityResult(Base):
    __tablename__ = "activity_results"

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    child_id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), ForeignKey("children.id", ondelete="CASCADE"),
                                                 nullable=False)
    session_id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), ForeignKey("sessions.id", ondelete="CASCADE"),
                                                   nullable=False)
    activity_id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True),
                                                   ForeignKey("activities.id"), nullable=False)
    lesson_id: Mapped[uuid.UUID | None] = mapped_column(UUID(as_uuid=True), ForeignKey("lessons.id"))

    round_id: Mapped[str] = mapped_column(String(100), nullable=False)
    level_number: Mapped[int] = mapped_column(SmallInteger, default=1)
    difficulty: Mapped[str] = mapped_column(String(20), default="easy")

    is_correct: Mapped[bool] = mapped_column(Boolean, nullable=False)
    total_attempts: Mapped[int] = mapped_column(SmallInteger, default=1)
    hints_used_count: Mapped[int] = mapped_column(SmallInteger, default=0)
    time_spent_seconds: Mapped[float] = mapped_column(Numeric(8, 2), default=0)

    start_time: Mapped[datetime] = mapped_column(DateTime(timezone=True), nullable=False)
    end_time: Mapped[datetime] = mapped_column(DateTime(timezone=True), nullable=False)
    counts_toward_mastery: Mapped[bool] = mapped_column(Boolean, default=True)

    error_type: Mapped[str | None] = mapped_column(String(50))
    technical_issue: Mapped[dict | None] = mapped_column(JSONB)

    skill_tags: Mapped[list[str]] = mapped_column(ARRAY(String), default=list)
    additional_data: Mapped[dict | None] = mapped_column(JSONB)

    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), default=lambda: datetime.now(timezone.utc))

    child = relationship("Child", back_populates="activity_results")
    session = relationship("Session", back_populates="activity_results")
    activity = relationship("Activity", back_populates="activity_results")
    lesson = relationship("Lesson", back_populates="activity_results")
