from uuid import UUID

from sqlalchemy import select

from app.models.parent import Parent
from app.repositories.base import BaseRepository


class ParentRepository(BaseRepository):
    async def get_by_email(self, email: str) -> Parent | None:
        stmt = select(Parent).where(Parent.email == email)
        result = await self.db.execute(stmt)
        return result.scalar_one_or_none()

    async def get_by_id(self, parent_id: UUID) -> Parent | None:
        return await self.get(Parent, parent_id)

    async def update_notification_settings(self, parent_id: UUID, settings: dict) -> Parent | None:
        parent = await self.get_by_id(parent_id)
        if parent:
            parent.notification_settings = {**parent.notification_settings, **settings}
            await self.update(parent)
        return parent
