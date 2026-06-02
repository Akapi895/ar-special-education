from uuid import UUID

from sqlalchemy import select, func, and_

from app.models.notification import Notification
from app.repositories.base import BaseRepository


class NotificationRepository(BaseRepository):
    async def get_by_parent(self, parent_id: UUID, skip: int = 0, limit: int = 50) -> list[Notification]:
        stmt = select(Notification).where(
            Notification.parent_id == parent_id
        ).order_by(Notification.created_at.desc()).offset(skip).limit(limit)
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_by_id(self, notification_id: UUID) -> Notification | None:
        return await self.get(Notification, notification_id)

    async def mark_as_read(self, notification_id: UUID):
        notification = await self.get_by_id(notification_id)
        if notification:
            notification.is_read = True
            await self.update(notification)

    async def mark_all_as_read(self, parent_id: UUID):
        stmt = select(Notification).where(
            and_(Notification.parent_id == parent_id, Notification.is_read == False)
        )
        result = await self.db.execute(stmt)
        notifications = result.scalars().all()
        for n in notifications:
            n.is_read = True
            self.db.add(n)
        await self.db.flush()

    async def count_unread(self, parent_id: UUID) -> int:
        stmt = select(func.count()).select_from(Notification).where(
            and_(Notification.parent_id == parent_id, Notification.is_read == False)
        )
        result = await self.db.execute(stmt)
        return result.scalar() or 0
