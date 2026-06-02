from uuid import UUID

from sqlalchemy import select, func

from app.models.point import PointsLedger
from app.repositories.base import BaseRepository


class PointRepository(BaseRepository):
    async def get_balance(self, child_id: UUID) -> int:
        stmt = select(
            func.coalesce(func.sum(PointsLedger.amount), 0)
        ).where(PointsLedger.child_id == child_id)
        result = await self.db.execute(stmt)
        return int(result.scalar() or 0)

    async def get_earned_total(self, child_id: UUID) -> int:
        stmt = select(
            func.coalesce(func.sum(PointsLedger.amount), 0)
        ).where(
            PointsLedger.child_id == child_id,
            PointsLedger.amount > 0,
        )
        result = await self.db.execute(stmt)
        return int(result.scalar() or 0)

    async def get_spent_total(self, child_id: UUID) -> int:
        stmt = select(
            func.coalesce(func.sum(PointsLedger.amount), 0)
        ).where(
            PointsLedger.child_id == child_id,
            PointsLedger.amount < 0,
        )
        result = await self.db.execute(stmt)
        return abs(int(result.scalar() or 0))

    async def add_transaction(self, child_id: UUID, transaction_type: str, amount: int,
                              reference_type: str | None = None, reference_id: UUID | None = None,
                              description: str | None = None) -> PointsLedger:
        current_balance = await self.get_balance(child_id)
        new_balance = current_balance + amount

        entry = PointsLedger(
            child_id=child_id,
            transaction_type=transaction_type,
            amount=amount,
            balance_after=new_balance,
            reference_type=reference_type,
            reference_id=reference_id,
            description=description,
        )
        return await self.create(entry)
