from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.api.deps import get_current_parent, get_current_child
from app.core.database import get_db
from app.schemas.auth import (
    ParentRegisterRequest,
    ParentLoginRequest,
    TokenResponse,
    TokenRefreshRequest,
    ParentProfileResponse,
    ParentUpdateRequest,
    PasswordChangeRequest,
    NotificationSettingsUpdate,
    ChildLoginRequest,
    ChildAuthResponse,
)
from app.services.auth_service import AuthService

router = APIRouter()


@router.post("/parent/auth/register", response_model=TokenResponse)
async def register_parent(request: ParentRegisterRequest, db: AsyncSession = Depends(get_db)):
    svc = AuthService(db)
    return await svc.register_parent(request.email, request.password, request.full_name, request.phone)


@router.post("/parent/auth/login", response_model=TokenResponse)
async def login_parent(request: ParentLoginRequest, db: AsyncSession = Depends(get_db)):
    svc = AuthService(db)
    return await svc.login_parent(request.email, request.password)


@router.post("/parent/auth/token", response_model=TokenResponse)
async def refresh_parent_token(request: TokenRefreshRequest, db: AsyncSession = Depends(get_db)):
    svc = AuthService(db)
    return await svc.refresh_token(request.refresh_token)


@router.get("/parent/auth/me", response_model=ParentProfileResponse)
async def get_parent_profile(parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = AuthService(db)
    return await svc.get_parent_profile(parent.id)


@router.put("/parent/auth/me", response_model=ParentProfileResponse)
async def update_parent_profile(request: ParentUpdateRequest, parent=Depends(get_current_parent),
                                 db: AsyncSession = Depends(get_db)):
    svc = AuthService(db)
    return await svc.update_parent_profile(parent.id, request.model_dump(exclude_unset=True, exclude_none=True))


@router.put("/parent/auth/me/password")
async def change_password(request: PasswordChangeRequest, parent=Depends(get_current_parent),
                           db: AsyncSession = Depends(get_db)):
    svc = AuthService(db)
    await svc.change_password(parent.id, request.current_password, request.new_password)
    return {"detail": "Password changed successfully"}


@router.delete("/parent/auth/me")
async def delete_parent_account(parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = AuthService(db)
    await svc.delete_parent(parent.id)
    return {"detail": "Account deleted"}


@router.get("/parent/auth/me/notification-settings")
async def get_notification_settings(parent=Depends(get_current_parent)):
    return parent.notification_settings


@router.put("/parent/auth/me/notification-settings")
async def update_notification_settings(request: NotificationSettingsUpdate,
                                        parent=Depends(get_current_parent),
                                        db: AsyncSession = Depends(get_db)):
    svc = AuthService(db)
    updated = await svc.update_notification_settings(
        parent.id, request.model_dump(exclude_unset=True, exclude_none=True)
    )
    return updated.notification_settings


@router.post("/parent/auth/logout")
async def logout_parent(parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = AuthService(db)
    await svc.logout(parent.id, is_parent=True)
    return {"detail": "Logged out"}


@router.post("/child/auth/login", response_model=ChildAuthResponse)
async def login_child(request: ChildLoginRequest, db: AsyncSession = Depends(get_db)):
    svc = AuthService(db)
    return await svc.login_child(request.username, request.password)


@router.post("/child/auth/token", response_model=TokenResponse)
async def refresh_child_token(request: TokenRefreshRequest, db: AsyncSession = Depends(get_db)):
    svc = AuthService(db)
    return await svc.refresh_token(request.refresh_token)


@router.get("/child/auth/me")
async def get_child_profile(child=Depends(get_current_child)):
    from app.schemas.child import ChildResponse
    return ChildResponse.model_validate(child)


@router.post("/child/auth/logout")
async def logout_child(child=Depends(get_current_child), db: AsyncSession = Depends(get_db)):
    svc = AuthService(db)
    await svc.logout(child.id, is_parent=False)
    return {"detail": "Logged out"}
