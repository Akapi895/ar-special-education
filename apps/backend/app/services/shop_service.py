from uuid import UUID

from sqlalchemy.ext.asyncio import AsyncSession

from app.core.exceptions import NotFound, BadRequest, Forbidden
from app.models.reward import Reward, RedemptionRequest, Inventory, AvatarItem
from app.repositories import RewardRepository, ChildRepository, PointRepository


class ShopService:
    def __init__(self, db: AsyncSession):
        self.reward_repo = RewardRepository(db)
        self.child_repo = ChildRepository(db)
        self.point_repo = PointRepository(db)

    async def create_reward(self, parent_id: UUID, data) -> Reward:
        reward = Reward(
            parent_id=parent_id,
            name_vi=data.name_vi,
            name_en=data.name_en,
            description_vi=data.description_vi,
            description_en=data.description_en,
            image_url=data.image_url,
            category=data.category,
            cost_points=data.cost_points,
            quantity=data.quantity,
        )
        return await self.reward_repo.create(reward)

    async def get_rewards(self, parent_id: UUID) -> list[Reward]:
        return await self.reward_repo.get_by_parent(parent_id)

    async def get_reward(self, reward_id: UUID) -> Reward:
        reward = await self.reward_repo.get_by_id(reward_id)
        if not reward:
            raise NotFound("Reward not found")
        return reward

    async def update_reward(self, reward_id: UUID, parent_id: UUID, data) -> Reward:
        reward = await self.get_reward(reward_id)
        if reward.parent_id != parent_id:
            raise Forbidden("Not allowed to update this reward")
        update_data = data.model_dump(exclude_unset=True, exclude_none=True)
        for key, value in update_data.items():
            setattr(reward, key, value)
        return await self.reward_repo.update(reward)

    async def delete_reward(self, reward_id: UUID, parent_id: UUID):
        reward = await self.get_reward(reward_id)
        if reward.parent_id != parent_id:
            raise Forbidden("Not allowed to delete this reward")
        await self.reward_repo.delete(reward)

    async def update_quantity(self, reward_id: UUID, parent_id: UUID, quantity: int) -> Reward:
        reward = await self.get_reward(reward_id)
        if reward.parent_id != parent_id:
            raise Forbidden("Not allowed to update this reward")
        reward.quantity = quantity
        return await self.reward_repo.update(reward)

    async def get_redemption_requests(self, parent_id: UUID, status: str | None = None) -> list[RedemptionRequest]:
        return await self.reward_repo.get_redemption_requests(parent_id, status)

    async def approve_redemption(self, request_id: UUID, parent_id: UUID):
        req = await self.reward_repo.get_redemption_by_id(request_id)
        if not req:
            raise NotFound("Redemption request not found")
        child = await self.child_repo.get_by_id(req.child_id)
        if not child or child.parent_id != parent_id:
            raise Forbidden("Not allowed to approve this request")

        from datetime import datetime, timezone
        req.status = "approved"
        req.reviewed_by = parent_id
        req.reviewed_at = datetime.now(timezone.utc)
        await self.reward_repo.update(req)

        existing_inv = await self.reward_repo.get_inventory_item(req.child_id, req.reward_id)
        if not existing_inv:
            inv = Inventory(child_id=req.child_id, reward_id=req.reward_id)
            await self.reward_repo.create(inv)

        return req

    async def reject_redemption(self, request_id: UUID, parent_id: UUID):
        req = await self.reward_repo.get_redemption_by_id(request_id)
        if not req:
            raise NotFound("Redemption request not found")
        child = await self.child_repo.get_by_id(req.child_id)
        if not child or child.parent_id != parent_id:
            raise Forbidden("Not allowed to reject this request")

        from datetime import datetime, timezone
        req.status = "rejected"
        req.reviewed_by = parent_id
        req.reviewed_at = datetime.now(timezone.utc)
        await self.reward_repo.update(req)

        await self.point_repo.add_transaction(
            req.child_id, "spend_redeem",
            abs(req.points_spent),
            reference_type="redemption",
            reference_id=request_id,
            description=f"Refund for rejected redemption #{request_id}",
        )
        return req

    async def redeem_reward(self, child_id: UUID, reward_id: UUID):
        child = await self.child_repo.get_by_id(child_id)
        if not child:
            raise NotFound("Child not found")

        reward = await self.get_reward(reward_id)
        if not reward.is_active:
            raise BadRequest("Reward is not available")

        if reward.quantity != -1 and reward.quantity <= 0:
            raise BadRequest("Reward out of stock")

        balance = await self.point_repo.get_balance(child_id)
        if balance < reward.cost_points:
            raise BadRequest("Not enough points")

        req = RedemptionRequest(
            child_id=child_id,
            reward_id=reward_id,
            status="pending",
            points_spent=reward.cost_points,
        )
        req = await self.reward_repo.create(req)

        await self.point_repo.add_transaction(
            child_id, "spend_redeem",
            -reward.cost_points,
            reference_type="redemption",
            reference_id=req.id,
            description=f"Redeemed {reward.name_vi}",
        )

        if reward.quantity > 0:
            reward.quantity -= 1
            await self.reward_repo.update(reward)

        return req

    async def get_inventory(self, child_id: UUID) -> list[Inventory]:
        return await self.reward_repo.get_child_inventory(child_id)

    async def equip_avatar_item(self, child_id: UUID, inventory_item_id: UUID):
        inv = await self.reward_repo.get(Inventory, inventory_item_id)
        if not inv or inv.child_id != child_id:
            raise NotFound("Inventory item not found")

        avatar = await self.reward_repo.get_avatar_item_by_reward(inv.reward_id)
        if not avatar:
            raise BadRequest("Item is not an avatar item")

        await self.reward_repo.unequip_all_by_slot(child_id, avatar.slot)

        inv.is_equipped = True
        await self.reward_repo.update(inv)

        child = await self.child_repo.get_by_id(child_id)
        if child:
            child.avatar_equipped = inv.reward_id
            await self.child_repo.update(child)

        return inv
