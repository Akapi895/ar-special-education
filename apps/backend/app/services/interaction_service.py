from uuid import UUID

from sqlalchemy.ext.asyncio import AsyncSession

from app.models.interaction import Interaction
from app.repositories import InteractionRepository, ChildRepository, ParentRepository
from app.core.exceptions import NotFound


class InteractionService:
    def __init__(self, db: AsyncSession):
        self.interaction_repo = InteractionRepository(db)
        self.child_repo = ChildRepository(db)
        self.parent_repo = ParentRepository(db)

    async def create_chat(self, child_id: UUID, parent_id: UUID, content: str,
                          metadata: dict | None = None) -> Interaction:
        child = await self.child_repo.get_by_id(child_id)
        if not child:
            raise NotFound("Child not found")
        parent = await self.parent_repo.get_by_id(parent_id)
        if not parent:
            raise NotFound("Parent not found")

        interaction = Interaction(
            child_id=child_id,
            parent_id=parent_id,
            type="chat",
            content=content,
            metadata=metadata,
        )
        return await self.interaction_repo.create(interaction)

    async def get_logs(self, child_id: UUID, skip: int = 0, limit: int = 50) -> list[Interaction]:
        return await self.interaction_repo.get_by_child(child_id, skip, limit)

    async def get_chat_history(self, child_id: UUID, skip: int = 0, limit: int = 50) -> list[Interaction]:
        return await self.interaction_repo.get_chat_history(child_id, skip, limit)
