import uuid
from datetime import datetime, timezone

from sqlalchemy import Boolean, DateTime, ForeignKey, SmallInteger, String, Text
from sqlalchemy.dialects.postgresql import JSONB, UUID
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.core.database import Base


class Child(Base):
    __tablename__ = "children"

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    parent_id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), ForeignKey("parents.id", ondelete="CASCADE"),
                                                  nullable=False)
    username: Mapped[str] = mapped_column(String(50), nullable=False)
    hashed_password: Mapped[str] = mapped_column(Text, nullable=False)
    display_name: Mapped[str] = mapped_column(String(100), nullable=False)
    age_years: Mapped[int] = mapped_column(SmallInteger, nullable=False)
    grade: Mapped[str | None] = mapped_column(String(50))
    avatar_equipped: Mapped[uuid.UUID | None] = mapped_column(UUID(as_uuid=True))
    preferences: Mapped[dict] = mapped_column(JSONB, default=dict)
    is_active: Mapped[bool] = mapped_column(Boolean, default=True)
    last_active_at: Mapped[datetime | None] = mapped_column(DateTime(timezone=True))
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), default=lambda: datetime.now(timezone.utc))
    updated_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), default=lambda: datetime.now(timezone.utc), onupdate=lambda: datetime.now(timezone.utc)
    )

    parent = relationship("Parent", back_populates="children")
    sessions = relationship("Session", back_populates="child", cascade="all, delete-orphan")
    activity_results = relationship("ActivityResult", back_populates="child", cascade="all, delete-orphan")
    skill_masteries = relationship("SkillMastery", back_populates="child", cascade="all, delete-orphan")
    assessments = relationship("Assessment", back_populates="child", cascade="all, delete-orphan")
    child_tasks = relationship("ChildTask", back_populates="child", cascade="all, delete-orphan")
    emotion_records = relationship("EmotionRecord", back_populates="child", cascade="all, delete-orphan")
    interactions = relationship("Interaction", back_populates="child", cascade="all, delete-orphan")
    reports = relationship("Report", back_populates="child", cascade="all, delete-orphan")
    redemption_requests = relationship("RedemptionRequest", back_populates="child", cascade="all, delete-orphan")
    inventory = relationship("Inventory", back_populates="child", cascade="all, delete-orphan")
    points_ledger = relationship("PointsLedger", back_populates="child", cascade="all, delete-orphan")
    refresh_tokens = relationship("RefreshToken", back_populates="child", cascade="all, delete-orphan",
                                  foreign_keys="RefreshToken.child_id")
