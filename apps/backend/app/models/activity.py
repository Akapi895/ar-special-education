import uuid
from datetime import datetime, timezone

from sqlalchemy import Boolean, SmallInteger, String, Text
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.core.database import Base


class Activity(Base):
    __tablename__ = "activities"

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    code: Mapped[str] = mapped_column(String(50), unique=True, nullable=False)
    title_vi: Mapped[str] = mapped_column(String(255), nullable=False)
    title_en: Mapped[str] = mapped_column(String(255), nullable=False)
    description_vi: Mapped[str | None] = mapped_column(Text)
    description_en: Mapped[str | None] = mapped_column(Text)
    icon_url: Mapped[str | None] = mapped_column(Text)
    sort_order: Mapped[int] = mapped_column(SmallInteger, default=0)
    is_active: Mapped[bool] = mapped_column(Boolean, default=True)
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), default=lambda: datetime.now(timezone.utc))

    lessons = relationship("Lesson", back_populates="activity", cascade="all, delete-orphan")
    sessions = relationship("Session", back_populates="activity")
    activity_results = relationship("ActivityResult", back_populates="activity")


class Lesson(Base):
    __tablename__ = "lessons"

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    activity_id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True),
                                                   ForeignKey("activities.id", ondelete="CASCADE"))
    code: Mapped[str] = mapped_column(String(50), unique=True, nullable=False)
    title_vi: Mapped[str] = mapped_column(String(255), nullable=False)
    title_en: Mapped[str] = mapped_column(String(255), nullable=False)
    difficulty: Mapped[str] = mapped_column(String(20), default="easy")
    skill_tags: Mapped[list[str]] = mapped_column(postgresql.ARRAY(String), default=list)
    prerequisites: Mapped[list[uuid.UUID]] = mapped_column(postgresql.ARRAY(UUID(as_uuid=True)), default=list)
    sort_order: Mapped[int] = mapped_column(SmallInteger, default=0)
    is_active: Mapped[bool] = mapped_column(Boolean, default=True)
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), default=lambda: datetime.now(timezone.utc))

    activity = relationship("Activity", back_populates="lessons")
    activity_results = relationship("ActivityResult", back_populates="lesson")

    from sqlalchemy import ForeignKey
    from sqlalchemy.dialects import postgresql
