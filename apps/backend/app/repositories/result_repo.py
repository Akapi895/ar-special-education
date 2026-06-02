from uuid import UUID

from sqlalchemy import select, func, and_

from app.models.result import ActivityResult
from app.repositories.base import BaseRepository


class ResultRepository(BaseRepository):
    async def get_by_session(self, session_id: UUID) -> list[ActivityResult]:
        stmt = select(ActivityResult).where(ActivityResult.session_id == session_id)
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_by_child(self, child_id: UUID, skip: int = 0, limit: int = 100) -> list[ActivityResult]:
        stmt = select(ActivityResult).where(
            ActivityResult.child_id == child_id
        ).order_by(ActivityResult.created_at.desc()).offset(skip).limit(limit)
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_by_child_and_activity(self, child_id: UUID, activity_id: UUID) -> list[ActivityResult]:
        stmt = select(ActivityResult).where(
            and_(ActivityResult.child_id == child_id, ActivityResult.activity_id == activity_id)
        ).order_by(ActivityResult.created_at.desc())
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_stats_by_child(self, child_id: UUID) -> dict:
        from app.models.result import ActivityResult
        total = await self.count(ActivityResult)
        correct_stmt = select(func.count()).select_from(ActivityResult).where(
            and_(ActivityResult.child_id == child_id, ActivityResult.is_correct == True)
        )
        correct = await self.db.execute(correct_stmt)
        correct_count = correct.scalar() or 0

        time_stmt = select(func.coalesce(func.sum(ActivityResult.time_spent_seconds), 0)).where(
            ActivityResult.child_id == child_id
        )
        total_time = await self.db.execute(time_stmt)
        total_time_seconds = float(total_time.scalar() or 0)

        return {
            "total_attempts": total,
            "correct_attempts": correct_count,
            "success_rate": (correct_count / total * 100) if total > 0 else 0,
            "total_time_seconds": total_time_seconds,
        }

    async def count_by_child_and_activity(self, child_id: UUID, activity_id: UUID) -> int:
        from sqlalchemy.functions import func
        stmt = select(func.count()).select_from(ActivityResult).where(
            and_(ActivityResult.child_id == child_id, ActivityResult.activity_id == activity_id)
        )
        result = await self.db.execute(stmt)
        return result.scalar() or 0
