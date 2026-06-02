from datetime import datetime, timezone
from uuid import UUID

from sqlalchemy.ext.asyncio import AsyncSession

from app.core.exceptions import Conflict, NotFound, Unauthorized, BadRequest
from app.core.security import (
    hash_password,
    verify_password,
    create_access_token,
    create_refresh_token,
    decode_token,
)
from app.models.auth import RefreshToken
from app.repositories import ParentRepository, ChildRepository, AuthRepository


class AuthService:
    def __init__(self, db: AsyncSession):
        self.parent_repo = ParentRepository(db)
        self.child_repo = ChildRepository(db)
        self.auth_repo = AuthRepository(db)

    async def register_parent(self, email: str, password: str, full_name: str, phone: str | None = None):
        existing = await self.parent_repo.get_by_email(email)
        if existing:
            raise Conflict("Email already registered")

        from app.models.parent import Parent
        parent = Parent(
            email=email,
            hashed_password=hash_password(password),
            full_name=full_name,
            phone=phone,
            notification_settings={
                "progress_report": True,
                "emotion_alert": True,
                "task_reminder": True,
            },
        )
        parent = await self.parent_repo.create(parent)

        access_token = create_access_token(parent.id)
        refresh_token = create_refresh_token(parent.id)
        await self._save_refresh_token(parent.id, refresh_token, is_parent=True)

        return {
            "access_token": access_token,
            "refresh_token": refresh_token,
            "token_type": "bearer",
        }

    async def login_parent(self, email: str, password: str):
        parent = await self.parent_repo.get_by_email(email)
        if not parent or not verify_password(password, parent.hashed_password):
            raise Unauthorized("Invalid email or password")

        access_token = create_access_token(parent.id)
        refresh_token = create_refresh_token(parent.id)
        await self._save_refresh_token(parent.id, refresh_token, is_parent=True)

        return {
            "access_token": access_token,
            "refresh_token": refresh_token,
            "token_type": "bearer",
        }

    async def refresh_token(self, token: str):
        payload = decode_token(token)
        if not payload or payload.get("type") != "refresh":
            raise Unauthorized("Invalid refresh token")

        from app.core.security import create_access_token, create_refresh_token
        subject = payload.get("sub")
        if not subject:
            raise Unauthorized("Invalid token payload")

        from app.core.security import hash_password as hash_token
        token_hash = hash_token(token)[:64]
        stored = await self.auth_repo.get_by_token_hash(token_hash)
        if not stored or stored.revoked or stored.expires_at < datetime.now(timezone.utc):
            raise Unauthorized("Refresh token expired or revoked")

        stored.revoked = True
        await self.auth_repo.update(stored)

        parent_id = stored.parent_id
        child_id = stored.child_id
        owner_id = parent_id or child_id

        new_access = create_access_token(owner_id)
        new_refresh = create_refresh_token(owner_id)
        await self._save_refresh_token(owner_id, new_refresh, is_parent=bool(parent_id))

        return {
            "access_token": new_access,
            "refresh_token": new_refresh,
            "token_type": "bearer",
        }

    async def login_child(self, username: str, password: str):
        child = await self.child_repo.get_by_username(username)
        if not child or not verify_password(password, child.hashed_password):
            raise Unauthorized("Invalid username or password")

        access_token = create_access_token(child.id)
        refresh_token = create_refresh_token(child.id)
        await self._save_refresh_token(child.id, refresh_token, is_parent=False)

        return {
            "access_token": access_token,
            "refresh_token": refresh_token,
            "token_type": "bearer",
            "child_id": child.id,
        }

    async def get_parent_profile(self, parent_id: UUID):
        parent = await self.parent_repo.get_by_id(parent_id)
        if not parent:
            raise NotFound("Parent not found")
        return parent

    async def update_parent_profile(self, parent_id: UUID, data: dict):
        parent = await self.parent_repo.get_by_id(parent_id)
        if not parent:
            raise NotFound("Parent not found")
        for key, value in data.items():
            if value is not None:
                setattr(parent, key, value)
        return await self.parent_repo.update(parent)

    async def change_password(self, parent_id: UUID, current_password: str, new_password: str):
        parent = await self.parent_repo.get_by_id(parent_id)
        if not parent:
            raise NotFound("Parent not found")
        if not verify_password(current_password, parent.hashed_password):
            raise BadRequest("Current password is incorrect")
        parent.hashed_password = hash_password(new_password)
        await self.parent_repo.update(parent)
        await self.auth_repo.revoke_all_parent_tokens(parent_id)

    async def delete_parent(self, parent_id: UUID):
        parent = await self.parent_repo.get_by_id(parent_id)
        if not parent:
            raise NotFound("Parent not found")
        await self.parent_repo.delete(parent)

    async def update_notification_settings(self, parent_id: UUID, settings: dict):
        parent = await self.parent_repo.update_notification_settings(parent_id, settings)
        if not parent:
            raise NotFound("Parent not found")
        return parent

    async def logout(self, user_id: UUID, is_parent: bool = True):
        if is_parent:
            await self.auth_repo.revoke_all_parent_tokens(user_id)
        else:
            await self.auth_repo.revoke_all_child_tokens(user_id)

    async def _save_refresh_token(self, owner_id: UUID, token: str, is_parent: bool):
        from datetime import timedelta
        from app.core.config import settings
        expires_at = datetime.now(timezone.utc) + timedelta(days=settings.REFRESH_TOKEN_EXPIRE_DAYS)

        refresh = RefreshToken(
            parent_id=owner_id if is_parent else None,
            child_id=None if is_parent else owner_id,
            token_hash=hash_password(token)[:64],
            expires_at=expires_at,
        )
        await self.auth_repo.save_refresh_token(refresh)
