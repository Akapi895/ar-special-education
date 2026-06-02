from uuid import UUID

from sqlalchemy import select, and_

from app.models.reward import Reward, RedemptionRequest, Inventory, AvatarItem
from app.repositories.base import BaseRepository


class RewardRepository(BaseRepository):
    async def get_by_id(self, reward_id: UUID) -> Reward | None:
        return await self.get(Reward, reward_id)

    async def get_by_parent(self, parent_id: UUID) -> list[Reward]:
        stmt = select(Reward).where(Reward.parent_id == parent_id).order_by(Reward.created_at.desc())
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_redemption_requests(self, parent_id: UUID, status: str | None = None) -> list[RedemptionRequest]:
        from app.models.child import Child
        stmt = select(RedemptionRequest).join(Child).where(Child.parent_id == parent_id)
        if status:
            stmt = stmt.where(RedemptionRequest.status == status)
        stmt = stmt.order_by(RedemptionRequest.created_at.desc())
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_redemption_by_id(self, request_id: UUID) -> RedemptionRequest | None:
        return await self.get(RedemptionRequest, request_id)

    async def get_child_inventory(self, child_id: UUID) -> list[Inventory]:
        stmt = select(Inventory).where(Inventory.child_id == child_id).order_by(Inventory.acquired_at.desc())
        result = await self.db.execute(stmt)
        return result.scalars().all()

    async def get_inventory_item(self, child_id: UUID, reward_id: UUID) -> Inventory | None:
        stmt = select(Inventory).where(
            and_(Inventory.child_id == child_id, Inventory.reward_id == reward_id)
        )
        result = await self.db.execute(stmt)
        return result.scalar_one_or_none()

    async def get_avatar_item_by_reward(self, reward_id: UUID) -> AvatarItem | None:
        stmt = select(AvatarItem).where(AvatarItem.reward_id == reward_id)
        result = await self.db.execute(stmt)
        return result.scalar_one_or_none()

    async def unequip_all_by_slot(self, child_id: UUID, slot: str):
        from app.models.reward import Inventory, AvatarItem
        stmt = select(Inventory).join(AvatarItem, Inventory.reward_id == AvatarItem.reward_id).where(
            and_(Inventory.child_id == child_id, AvatarItem.slot == slot, Inventory.is_equipped == True)
        )
        result = await self.db.execute(stmt)
        items = result.scalars().all()
        for item in items:
            item.is_equipped = False
            self.db.add(item)
        await self.db.flush()
