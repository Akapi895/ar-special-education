from uuid import UUID

from sqlalchemy import select

from app.models.interaction import Interaction
from app.repositories.base import BaseRepository


class InteractionRepository(BaseRepository):
    async def get_by_child(self, child_id: UUID, skip: int = 0, limit: int = 50) -> list[Interaction]:
        stmt = select(Interaction).where(
            Interaction.child_id == child_id
        ).order_by(Interaction.created_at.desc()).offset(skip).limit(limit)
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_chat_history(self, child_id: UUID, skip: int = 0, limit: int = 50) -> list[Interaction]:
        stmt = select(Interaction).where(
            Interaction.child_id == child_id
        ).order_by(Interaction.created_at.asc()).offset(skip).limit(limit)
        result = await self.db.execute(stmt)
        return result.scalars().all()
