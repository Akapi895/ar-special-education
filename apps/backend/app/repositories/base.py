from uuid import UUID

from sqlalchemy import select, delete
from sqlalchemy.ext.asyncio import AsyncSession


class BaseRepository:
    def __init__(self, db: AsyncSession):
        self.db = db

    async def get(self, model, id: UUID):
        return await self.db.get(model, id)

    async def get_all(self, model, skip: int = 0, limit: int = 100, order_by=None):
        stmt = select(model).offset(skip).limit(limit)
        if order_by is not None:
            stmt = stmt.order_by(order_by)
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def create(self, model_instance):
        self.db.add(model_instance)
        await self.db.flush()
        await self.db.refresh(model_instance)
        return model_instance

    async def update(self, model_instance):
        self.db.add(model_instance)
        await self.db.flush()
        await self.db.refresh(model_instance)
        return model_instance

    async def delete(self, model_instance):
        await self.db.delete(model_instance)
        await self.db.flush()

    async def delete_by_id(self, model, id: UUID):
        stmt = delete(model).where(model.id == id)
        await self.db.execute(stmt)
        await self.db.flush()

    async def count(self, model):
        from sqlalchemy.functions import func
        stmt = select(func.count()).select_from(model)
        result = await self.db.execute(stmt)
        return result.scalar()
