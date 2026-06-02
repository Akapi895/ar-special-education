from uuid import UUID

from sqlalchemy.ext.asyncio import AsyncSession

from app.models.emotion import EmotionRecord
from app.repositories import EmotionRepository, ChildRepository
from app.core.exceptions import NotFound


class EmotionService:
    def __init__(self, db: AsyncSession):
        self.emotion_repo = EmotionRepository(db)
        self.child_repo = ChildRepository(db)

    async def create_record(self, child_id: UUID, emotion: str, intensity: int | None = None,
                            note: str | None = None, source: str = "parent",
                            session_id: UUID | None = None) -> EmotionRecord:
        child = await self.child_repo.get_by_id(child_id)
        if not child:
            raise NotFound("Child not found")

        record = EmotionRecord(
            child_id=child_id,
            session_id=session_id,
            emotion=emotion,
            intensity=intensity,
            note=note,
            source=source,
        )
        return await self.emotion_repo.create(record)

    async def get_records(self, child_id: UUID, skip: int = 0, limit: int = 50) -> list[EmotionRecord]:
        return await self.emotion_repo.get_by_child(child_id, skip, limit)
