from uuid import UUID

from sqlalchemy import select, and_

from app.models.skill import SkillMastery
from app.repositories.base import BaseRepository


class SkillRepository(BaseRepository):
    async def get_by_child(self, child_id: UUID) -> list[SkillMastery]:
        stmt = select(SkillMastery).where(SkillMastery.child_id == child_id).order_by(SkillMastery.skill_tag)
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_by_child_and_skill(self, child_id: UUID, skill_tag: str) -> SkillMastery | None:
        stmt = select(SkillMastery).where(
            and_(SkillMastery.child_id == child_id, SkillMastery.skill_tag == skill_tag)
        )
        result = await self.db.execute(stmt)
        return result.scalar_one_or_none()

    async def upsert_mastery(self, child_id: UUID, skill_tag: str, is_correct: bool):
        existing = await self.get_by_child_and_skill(child_id, skill_tag)
        from datetime import datetime, timezone
        if existing:
            existing.total_attempts += 1
            if is_correct:
                existing.correct_attempts += 1
            existing.mastery_score = round(
                (existing.correct_attempts / existing.total_attempts) * 100, 2
            ) if existing.total_attempts > 0 else 0
            existing.last_practiced = datetime.now(timezone.utc)
            await self.update(existing)
        else:
            new_mastery = SkillMastery(
                child_id=child_id,
                skill_tag=skill_tag,
                total_attempts=1,
                correct_attempts=1 if is_correct else 0,
                mastery_score=100.0 if is_correct else 0.0,
                last_practiced=datetime.now(timezone.utc),
            )
            await self.create(new_mastery)
