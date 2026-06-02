from uuid import UUID

from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.api.deps import get_current_parent
from app.core.database import get_db
from app.schemas.child import (
    ChildCreateRequest,
    ChildUpdateRequest,
    ChildResponse,
    OnboardingCompleteRequest,
    OnboardingResponse,
)
from app.services.child_service import ChildService

router = APIRouter()


@router.get("/parent/children", response_model=list[ChildResponse])
async def list_children(parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = ChildService(db)
    return await svc.get_children(parent.id)


@router.post("/parent/children", response_model=ChildResponse)
async def create_child(request: ChildCreateRequest, parent=Depends(get_current_parent),
                        db: AsyncSession = Depends(get_db)):
    svc = ChildService(db)
    return await svc.create_child(parent.id, request)


@router.get("/parent/children/{child_id}", response_model=ChildResponse)
async def get_child(child_id: UUID, parent=Depends(get_current_parent),
                     db: AsyncSession = Depends(get_db)):
    svc = ChildService(db)
    return await svc.get_child(child_id, parent.id)


@router.put("/parent/children/{child_id}", response_model=ChildResponse)
async def update_child(child_id: UUID, request: ChildUpdateRequest,
                        parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = ChildService(db)
    return await svc.update_child(child_id, parent.id, request)


@router.delete("/parent/children/{child_id}")
async def delete_child(child_id: UUID, parent=Depends(get_current_parent),
                        db: AsyncSession = Depends(get_db)):
    svc = ChildService(db)
    await svc.delete_child(child_id, parent.id)
    return {"detail": "Child deleted"}


@router.post("/parent/children/{child_id}/select", response_model=ChildResponse)
async def select_child(child_id: UUID, parent=Depends(get_current_parent),
                        db: AsyncSession = Depends(get_db)):
    svc = ChildService(db)
    child = await svc.get_child(child_id, parent.id)
    return child


@router.post("/parent/onboarding/complete", response_model=OnboardingResponse)
async def complete_onboarding(request: OnboardingCompleteRequest,
                               parent=Depends(get_current_parent),
                               db: AsyncSession = Depends(get_db)):
    svc = ChildService(db)
    children = await svc.complete_onboarding(
        parent.id, request.parent, request.children, request.assessments
    )
    from app.schemas.child import ParentProfileResponse
    return OnboardingResponse(
        parent=ParentProfileResponse.model_validate(parent),
        children=[ChildResponse.model_validate(c) for c in children],
    )
