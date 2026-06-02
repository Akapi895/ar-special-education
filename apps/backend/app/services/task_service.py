from uuid import UUID

from sqlalchemy.ext.asyncio import AsyncSession

from app.core.exceptions import NotFound, BadRequest, Forbidden
from app.models.task import Task, ChildTask, ActivityRequirement
from app.repositories import TaskRepository, ParentRepository, ChildRepository


class TaskService:
    def __init__(self, db: AsyncSession):
        self.task_repo = TaskRepository(db)
        self.parent_repo = ParentRepository(db)
        self.child_repo = ChildRepository(db)

    async def create_task(self, parent_id: UUID, data) -> Task:
        task = Task(
            parent_id=parent_id,
            title=data.title,
            description=data.description,
            difficulty=data.difficulty,
            category=data.category,
            estimated_minutes=data.estimated_minutes,
        )
        task = await self.task_repo.create(task)

        if data.activity_requirements:
            for req in data.activity_requirements:
                ar = ActivityRequirement(
                    task_id=task.id,
                    activity_id=req.activity_id,
                    min_rounds=req.min_rounds,
                    target_score=req.target_score,
                )
                await self.task_repo.create(ar)

        return task

    async def get_tasks(self, parent_id: UUID) -> list[Task]:
        return await self.task_repo.get_by_parent(parent_id)

    async def get_task(self, task_id: UUID) -> Task:
        task = await self.task_repo.get_by_id(task_id)
        if not task:
            raise NotFound("Task not found")
        return task

    async def update_task(self, task_id: UUID, parent_id: UUID, data) -> Task:
        task = await self.get_task(task_id)
        if task.parent_id != parent_id:
            raise Forbidden("Not allowed to update this task")
        update_data = data.model_dump(exclude_unset=True, exclude_none=True)
        for key, value in update_data.items():
            setattr(task, key, value)
        return await self.task_repo.update(task)

    async def delete_task(self, task_id: UUID, parent_id: UUID):
        task = await self.get_task(task_id)
        if task.parent_id != parent_id:
            raise Forbidden("Not allowed to delete this task")
        await self.task_repo.delete(task)

    async def assign_task(self, child_id: UUID, task_id: UUID, assigned_by: UUID) -> ChildTask:
        existing = await self.task_repo.get_active_child_task(child_id, task_id)
        if existing:
            raise BadRequest("Task already assigned to this child")

        child_task = ChildTask(
            child_id=child_id,
            task_id=task_id,
            assigned_by=assigned_by,
            status="assigned",
        )
        return await self.task_repo.create(child_task)

    async def create_and_assign(self, parent_id: UUID, child_id: UUID, data) -> ChildTask:
        task = await self.create_task(parent_id, data)
        return await self.assign_task(child_id, task.id, parent_id)

    async def get_child_tasks(self, child_id: UUID, status: str | None = None) -> list[ChildTask]:
        return await self.task_repo.get_child_tasks(child_id, status)

    async def get_child_task(self, child_task_id: UUID) -> ChildTask:
        ct = await self.task_repo.get_child_task_by_id(child_task_id)
        if not ct:
            raise NotFound("Child task not found")
        return ct

    async def update_child_task(self, child_task_id: UUID, data: dict) -> ChildTask:
        ct = await self.get_child_task(child_task_id)
        for key, value in data.items():
            if value is not None:
                setattr(ct, key, value)
        return await self.task_repo.update(ct)

    async def delete_child_task(self, child_task_id: UUID):
        ct = await self.get_child_task(child_task_id)
        await self.task_repo.delete(ct)

    async def start_task(self, child_task_id: UUID):
        ct = await self.get_child_task(child_task_id)
        from datetime import datetime, timezone
        ct.status = "in_progress"
        ct.started_at = datetime.now(timezone.utc)
        return await self.task_repo.update(ct)

    async def complete_task(self, child_task_id: UUID, score: float | None = None):
        ct = await self.get_child_task(child_task_id)
        from datetime import datetime, timezone
        ct.status = "completed"
        ct.score = score
        ct.completed_at = datetime.now(timezone.utc)
        return await self.task_repo.update(ct)

    async def verify_task(self, child_task_id: UUID, parent_id: UUID, feedback: str | None = None):
        ct = await self.get_child_task(child_task_id)
        from datetime import datetime, timezone
        ct.status = "verified"
        ct.verified_by = parent_id
        ct.verified_at = datetime.now(timezone.utc)
        if feedback:
            ct.feedback = feedback
        return await self.task_repo.update(ct)

    async def reject_task(self, child_task_id: UUID, parent_id: UUID, feedback: str):
        ct = await self.get_child_task(child_task_id)
        from datetime import datetime, timezone
        ct.status = "rejected"
        ct.verified_by = parent_id
        ct.verified_at = datetime.now(timezone.utc)
        ct.feedback = feedback
        return await self.task_repo.update(ct)

    async def give_up_task(self, child_task_id: UUID, reason: str | None = None):
        ct = await self.get_child_task(child_task_id)
        ct.status = "given_up"
        ct.given_up_reason = reason
        return await self.task_repo.update(ct)

    async def get_suggested_tasks(self, child_id: UUID) -> list[Task]:
        return await self.task_repo.get_suggested_tasks(child_id)
