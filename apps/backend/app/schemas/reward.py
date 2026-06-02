from datetime import datetime
from uuid import UUID

from pydantic import BaseModel, Field


class RewardCreateRequest(BaseModel):
    name_vi: str = Field(min_length=1, max_length=255)
    name_en: str | None = None
    description_vi: str | None = None
    description_en: str | None = None
    image_url: str | None = None
    category: str = "badge"
    cost_points: int = Field(gt=0)
    quantity: int = -1


class RewardUpdateRequest(BaseModel):
    name_vi: str | None = None
    name_en: str | None = None
    description_vi: str | None = None
    description_en: str | None = None
    image_url: str | None = None
    category: str | None = None
    cost_points: int | None = None
    quantity: int | None = None


class RewardResponse(BaseModel):
    id: UUID
    parent_id: UUID
    name_vi: str
    name_en: str | None
    description_vi: str | None
    description_en: str | None
    image_url: str | None
    category: str
    cost_points: int
    quantity: int
    is_active: bool
    created_at: datetime

    class Config:
        from_attributes = True


class QuantityUpdateRequest(BaseModel):
    quantity: int = Field(ge=-1)


class RedeemRequest(BaseModel):
    reward_id: UUID


class RedemptionRequestResponse(BaseModel):
    id: UUID
    child_id: UUID
    reward_id: UUID
    status: str
    points_spent: int
    reviewed_by: UUID | None
    reviewed_at: datetime | None
    reject_reason: str | None
    created_at: datetime

    class Config:
        from_attributes = True


class InventoryItemResponse(BaseModel):
    id: UUID
    reward_id: UUID
    reward: RewardResponse | None = None
    acquired_at: datetime
    is_equipped: bool

    class Config:
        from_attributes = True


class EquipAvatarRequest(BaseModel):
    inventory_item_id: UUID
