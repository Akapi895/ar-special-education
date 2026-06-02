from uuid import UUID

from sqlalchemy.ext.asyncio import AsyncSession

from app.models.notification import Notification
from app.repositories import NotificationRepository, ParentRepository
from app.core.exceptions import NotFound


class NotificationService:
    def __init__(self, db: AsyncSession):
        self.notification_repo = NotificationRepository(db)
        self.parent_repo = ParentRepository(db)

    async def get_notifications(self, parent_id: UUID, skip: int = 0, limit: int = 50) -> list[Notification]:
        return await self.notification_repo.get_by_parent(parent_id, skip, limit)

    async def mark_as_read(self, notification_id: UUID):
        await self.notification_repo.mark_as_read(notification_id)

    async def mark_all_as_read(self, parent_id: UUID):
        await self.notification_repo.mark_all_as_read(parent_id)

    async def count_unread(self, parent_id: UUID) -> int:
        return await self.notification_repo.count_unread(parent_id)

    async def create_notification(self, parent_id: UUID, category: str,
                                  title_vi: str, body_vi: str,
                                  child_id: UUID | None = None,
                                  title_en: str | None = None,
                                  body_en: str | None = None,
                                  data: dict | None = None) -> Notification:
        notification = Notification(
            parent_id=parent_id,
            child_id=child_id,
            category=category,
            title_vi=title_vi,
            title_en=title_en,
            body_vi=body_vi,
            body_en=body_en,
            data=data,
        )
        return await self.notification_repo.create(notification)
