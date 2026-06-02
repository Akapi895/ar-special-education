from uuid import UUID

from sqlalchemy import select, and_

from app.models.session import Session
from app.repositories.base import BaseRepository


class SessionRepository(BaseRepository):
    async def get_by_id(self, session_id: UUID) -> Session | None:
        return await self.get(Session, session_id)

    async def get_by_child(self, child_id: UUID, skip: int = 0, limit: int = 50) -> list[Session]:
        stmt = select(Session).where(
            Session.child_id == child_id
        ).order_by(Session.start_time.desc()).offset(skip).limit(limit)
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_active_session(self, child_id: UUID) -> Session | None:
        stmt = select(Session).where(
            and_(Session.child_id == child_id, Session.status == "in_progress")
        ).order_by(Session.start_time.desc()).limit(1)
        result = await self.db.execute(stmt)
        return result.scalar_one_or_none()

    async def count_by_child(self, child_id: UUID) -> int:
        from sqlalchemy.functions import func
        stmt = select(func.count()).select_from(Session).where(Session.child_id == child_id)
        result = await self.db.execute(stmt)
        return result.scalar()
