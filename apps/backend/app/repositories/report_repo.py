from uuid import UUID

from sqlalchemy import select, and_

from app.models.report import Report
from app.repositories.base import BaseRepository


class ReportRepository(BaseRepository):
    async def get_by_child(self, child_id: UUID) -> list[Report]:
        stmt = select(Report).where(
            Report.child_id == child_id
        ).order_by(Report.generated_at.desc())
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_by_id(self, report_id: UUID) -> Report | None:
        return await self.get(Report, report_id)
