from uuid import UUID

from fastapi import APIRouter, Depends, Query
from sqlalchemy.ext.asyncio import AsyncSession

from app.api.deps import get_current_parent, get_current_child
from app.core.database import get_db
from app.schemas.reward import (
    RewardCreateRequest,
    RewardUpdateRequest,
    RewardResponse,
    QuantityUpdateRequest,
    RedeemRequest,
    RedemptionRequestResponse,
    InventoryItemResponse,
    EquipAvatarRequest,
)
from app.services.shop_service import ShopService

router = APIRouter()


@router.get("/parent/shop/rewards", response_model=list[RewardResponse])
async def list_rewards(parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = ShopService(db)
    return await svc.get_rewards(parent.id)


@router.post("/parent/shop/rewards", response_model=RewardResponse)
async def create_reward(request: RewardCreateRequest, parent=Depends(get_current_parent),
                         db: AsyncSession = Depends(get_db)):
    svc = ShopService(db)
    return await svc.create_reward(parent.id, request)


@router.put("/parent/shop/rewards/{reward_id}", response_model=RewardResponse)
async def update_reward(reward_id: UUID, request: RewardUpdateRequest,
                         parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = ShopService(db)
    return await svc.update_reward(reward_id, parent.id, request)


@router.delete("/parent/shop/rewards/{reward_id}")
async def delete_reward(reward_id: UUID, parent=Depends(get_current_parent),
                         db: AsyncSession = Depends(get_db)):
    svc = ShopService(db)
    await svc.delete_reward(reward_id, parent.id)
    return {"detail": "Reward deleted"}


@router.put("/parent/shop/rewards/{reward_id}/quantity", response_model=RewardResponse)
async def update_reward_quantity(reward_id: UUID, request: QuantityUpdateRequest,
                                  parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = ShopService(db)
    return await svc.update_quantity(reward_id, parent.id, request.quantity)


@router.get("/parent/shop/redemption-requests", response_model=list[RedemptionRequestResponse])
async def list_redemption_requests(status: str | None = Query(None),
                                    parent=Depends(get_current_parent),
                                    db: AsyncSession = Depends(get_db)):
    svc = ShopService(db)
    return await svc.get_redemption_requests(parent.id, status)


@router.post("/parent/shop/redemption-requests/{request_id}/approve", response_model=RedemptionRequestResponse)
async def approve_redemption(request_id: UUID, parent=Depends(get_current_parent),
                              db: AsyncSession = Depends(get_db)):
    svc = ShopService(db)
    return await svc.approve_redemption(request_id, parent.id)


@router.post("/parent/shop/redemption-requests/{request_id}/reject", response_model=RedemptionRequestResponse)
async def reject_redemption(request_id: UUID, parent=Depends(get_current_parent),
                             db: AsyncSession = Depends(get_db)):
    svc = ShopService(db)
    return await svc.reject_redemption(request_id, parent.id)


@router.post("/parent/children/{child_id}/redeem", response_model=RedemptionRequestResponse)
async def redeem_reward(child_id: UUID, request: RedeemRequest,
                         parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = ShopService(db)
    return await svc.redeem_reward(child_id, request.reward_id)


@router.get("/parent/children/{child_id}/inventory", response_model=list[InventoryItemResponse])
async def get_inventory(child_id: UUID, parent=Depends(get_current_parent),
                         db: AsyncSession = Depends(get_db)):
    svc = ShopService(db)
    return await svc.get_inventory(child_id)


@router.post("/parent/children/{child_id}/avatar/equip", response_model=InventoryItemResponse)
async def equip_avatar(child_id: UUID, request: EquipAvatarRequest,
                        parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = ShopService(db)
    return await svc.equip_avatar_item(child_id, request.inventory_item_id)
