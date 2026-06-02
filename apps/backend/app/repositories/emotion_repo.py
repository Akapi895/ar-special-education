from uuid import UUID

from sqlalchemy import select, func, and_

from app.models.emotion import EmotionRecord
from app.repositories.base import BaseRepository


class EmotionRepository(BaseRepository):
    async def get_by_child(self, child_id: UUID, skip: int = 0, limit: int = 50) -> list[EmotionRecord]:
        stmt = select(EmotionRecord).where(
            EmotionRecord.child_id == child_id
        ).order_by(EmotionRecord.recorded_at.desc()).offset(skip).limit(limit)
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_analytics(self, child_id: UUID) -> list[dict]:
        stmt = select(
            EmotionRecord.emotion,
            func.count().label("count"),
            func.avg(EmotionRecord.intensity).label("avg_intensity"),
        ).where(EmotionRecord.child_id == child_id
        ).group_by(EmotionRecord.emotion)
        result = await self.db.execute(stmt)
        rows = result.all()
        total = sum(row.count for row in rows) if rows else 1
        return [
            {
                "emotion": row.emotion,
                "count": row.count,
                "avg_intensity": float(row.avg_intensity) if row.avg_intensity else 0,
                "percentage": round(row.count / total * 100, 2),
            }
            for row in rows
        ]
