from uuid import UUID

from fastapi import Depends, Header
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.database import get_db
from app.core.exceptions import Unauthorized
from app.core.security import decode_token
from app.repositories import ParentRepository, ChildRepository


async def get_current_parent(
    authorization: str = Header(...),
    db: AsyncSession = Depends(get_db),
):
    if not authorization.startswith("Bearer "):
        raise Unauthorized("Invalid authorization header")

    token = authorization.replace("Bearer ", "")
    payload = decode_token(token)
    if not payload or payload.get("type") != "access":
        raise Unauthorized("Invalid or expired access token")

    parent_id = payload.get("sub")
    if not parent_id:
        raise Unauthorized("Invalid token payload")

    repo = ParentRepository(db)
    parent = await repo.get_by_id(UUID(parent_id))
    if not parent or not parent.is_active:
        raise Unauthorized("Parent not found or inactive")
    return parent


async def get_current_child(
    authorization: str = Header(...),
    db: AsyncSession = Depends(get_db),
):
    if not authorization.startswith("Bearer "):
        raise Unauthorized("Invalid authorization header")

    token = authorization.replace("Bearer ", "")
    payload = decode_token(token)
    if not payload or payload.get("type") != "access":
        raise Unauthorized("Invalid or expired access token")

    child_id = payload.get("sub")
    if not child_id:
        raise Unauthorized("Invalid token payload")

    repo = ChildRepository(db)
    child = await repo.get_by_id(UUID(child_id))
    if not child or not child.is_active:
        raise Unauthorized("Child not found or inactive")
    return child
