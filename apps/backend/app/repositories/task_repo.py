from uuid import UUID

from sqlalchemy import select, and_, or_

from app.models.task import Task, ActivityRequirement, ChildTask
from app.repositories.base import BaseRepository


class TaskRepository(BaseRepository):
    async def get_by_id(self, task_id: UUID) -> Task | None:
        return await self.get(Task, task_id)

    async def get_by_parent(self, parent_id: UUID) -> list[Task]:
        stmt = select(Task).where(Task.parent_id == parent_id).order_by(Task.created_at.desc())
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_requirements(self, task_id: UUID) -> list[ActivityRequirement]:
        stmt = select(ActivityRequirement).where(ActivityRequirement.task_id == task_id)
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_child_task_by_id(self, child_task_id: UUID) -> ChildTask | None:
        return await self.get(ChildTask, child_task_id)

    async def get_child_tasks(self, child_id: UUID, status: str | None = None) -> list[ChildTask]:
        conditions = [ChildTask.child_id == child_id]
        if status:
            conditions.append(ChildTask.status == status)
        stmt = select(ChildTask).where(and_(*conditions)).order_by(ChildTask.created_at.desc())
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_active_child_task(self, child_id: UUID, task_id: UUID) -> ChildTask | None:
        stmt = select(ChildTask).where(
            and_(
                ChildTask.child_id == child_id,
                ChildTask.task_id == task_id,
                ChildTask.status.in_(["assigned", "in_progress"]),
            )
        )
        result = await self.db.execute(stmt)
        return result.scalar_one_or_none()

    async def get_suggested_tasks(self, child_id: UUID) -> list[Task]:
        from app.models.skill import SkillMastery
        subq = select(SkillMastery.skill_tag).where(
            and_(SkillMastery.child_id == child_id, SkillMastery.mastery_score < 80)
        )
        result = await self.db.execute(subq)
        weak_skills = [row[0] for row in result.all()]

        stmt = select(Task).where(Task.is_public == True)
        if weak_skills:
            stmt = stmt.where(Task.category.in_(weak_skills))
        stmt = stmt.order_by(Task.created_at.desc()).limit(20)
        result = await self.db.execute(stmt)
        return result.scalars().all()
