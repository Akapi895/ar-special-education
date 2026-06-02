from uuid import UUID

from sqlalchemy import select, and_

from app.models.child import Child
from app.repositories.base import BaseRepository


class ChildRepository(BaseRepository):
    async def get_by_id(self, child_id: UUID) -> Child | None:
        return await self.get(Child, child_id)

    async def get_by_parent(self, parent_id: UUID) -> list[Child]:
        stmt = select(Child).where(Child.parent_id == parent_id).order_by(Child.created_at)
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_by_parent_and_username(self, parent_id: UUID, username: str) -> Child | None:
        stmt = select(Child).where(
            and_(Child.parent_id == parent_id, Child.username == username)
        )
        result = await self.db.execute(stmt)
        return result.scalar_one_or_none()

    async def get_by_username(self, username: str) -> Child | None:
        stmt = select(Child).where(Child.username == username)
        result = await self.db.execute(stmt)
        return result.scalar_one_or_none()
