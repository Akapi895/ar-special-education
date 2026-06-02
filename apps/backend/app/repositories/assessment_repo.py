from uuid import UUID

from sqlalchemy import select

from app.models.assessment import Assessment
from app.repositories.base import BaseRepository


class AssessmentRepository(BaseRepository):
    async def get_by_child(self, child_id: UUID) -> list[Assessment]:
        stmt = select(Assessment).where(
            Assessment.child_id == child_id
        ).order_by(Assessment.created_at.desc())
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_by_id(self, assessment_id: UUID) -> Assessment | None:
        return await self.get(Assessment, assessment_id)

    async def get_latest_by_child(self, child_id: UUID) -> Assessment | None:
        stmt = select(Assessment).where(
            Assessment.child_id == child_id
        ).order_by(Assessment.created_at.desc()).limit(1)
        result = await self.db.execute(stmt)
        return result.scalar_one_or_none()
