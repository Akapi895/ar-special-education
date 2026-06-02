from uuid import UUID

from sqlalchemy import select

from app.models.activity import Activity, Lesson
from app.repositories.base import BaseRepository


class ActivityRepository(BaseRepository):
    async def get_all_active(self) -> list[Activity]:
        stmt = select(Activity).where(Activity.is_active == True).order_by(Activity.sort_order)
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_by_code(self, code: str) -> Activity | None:
        stmt = select(Activity).where(Activity.code == code)
        result = await self.db.execute(stmt)
        return result.scalar_one_or_none()

    async def get_lessons_by_activity(self, activity_id: UUID) -> list[Lesson]:
        stmt = select(Lesson).where(
            Lesson.activity_id == activity_id, Lesson.is_active == True
        ).order_by(Lesson.sort_order)
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_lesson_by_id(self, lesson_id: UUID) -> Lesson | None:
        return await self.get(Lesson, lesson_id)

    async def get_lesson_by_code(self, code: str) -> Lesson | None:
        stmt = select(Lesson).where(Lesson.code == code)
        result = await self.db.execute(stmt)
        return result.scalar_one_or_none()

    async def get_all_lessons(self) -> list[Lesson]:
        stmt = select(Lesson).order_by(Lesson.sort_order)
        result = await self.db.execute(stmt)
        return result.scalars().all()
