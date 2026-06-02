from uuid import UUID

from sqlalchemy import select, and_, delete

from app.models.auth import RefreshToken
from app.repositories.base import BaseRepository


class AuthRepository(BaseRepository):
    async def save_refresh_token(self, token: RefreshToken) -> RefreshToken:
        return await self.create(token)

    async def get_by_token_hash(self, token_hash: str) -> RefreshToken | None:
        stmt = select(RefreshToken).where(RefreshToken.token_hash == token_hash)
        result = await self.db.execute(stmt)
        return result.scalar_one_or_none()

    async def revoke_token(self, token_id: UUID):
        token = await self.get(RefreshToken, token_id)
        if token:
            token.revoked = True
            await self.update(token)

    async def revoke_all_parent_tokens(self, parent_id: UUID):
        stmt = select(RefreshToken).where(
            and_(RefreshToken.parent_id == parent_id, RefreshToken.revoked == False)
        )
        result = await self.db.execute(stmt)
        tokens = result.scalars().all()
        for token in tokens:
            token.revoked = True
            self.db.add(token)
        await self.db.flush()

    async def revoke_all_child_tokens(self, child_id: UUID):
        stmt = select(RefreshToken).where(
            and_(RefreshToken.child_id == child_id, RefreshToken.revoked == False)
        )
        result = await self.db.execute(stmt)
        tokens = result.scalars().all()
        for token in tokens:
            token.revoked = True
            self.db.add(token)
        await self.db.flush()

    async def cleanup_expired_tokens(self):
        from datetime import datetime, timezone
        stmt = delete(RefreshToken).where(RefreshToken.expires_at < datetime.now(timezone.utc))
        await self.db.execute(stmt)
        await self.db.flush()
