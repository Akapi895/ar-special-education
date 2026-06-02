from uuid import UUID

from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.api.deps import get_current_parent
from app.core.database import get_db
from app.schemas.assessment import (
    AssessmentCreateRequest,
    AssessmentUpdateRequest,
    AssessmentResponse,
    SimpleAssessmentRequest,
)
from app.services.assessment_service import AssessmentService

router = APIRouter()


@router.get("/parent/children/{child_id}/assessments", response_model=list[AssessmentResponse])
async def list_assessments(child_id: UUID, parent=Depends(get_current_parent),
                            db: AsyncSession = Depends(get_db)):
    svc = AssessmentService(db)
    return await svc.get_assessments(child_id)


@router.post("/parent/children/{child_id}/assessments", response_model=AssessmentResponse)
async def create_assessment(child_id: UUID, request: AssessmentCreateRequest,
                             parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = AssessmentService(db)
    return await svc.create_assessment(child_id, request.type, request.answers, request.recommendations)


@router.get("/parent/children/{child_id}/assessments/{assessment_id}", response_model=AssessmentResponse)
async def get_assessment(child_id: UUID, assessment_id: UUID,
                          parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = AssessmentService(db)
    return await svc.get_assessment(assessment_id)


@router.put("/parent/children/{child_id}/assessments/{assessment_id}", response_model=AssessmentResponse)
async def update_assessment(child_id: UUID, assessment_id: UUID,
                             request: AssessmentUpdateRequest,
                             parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = AssessmentService(db)
    return await svc.update_assessment(assessment_id, request.model_dump(exclude_unset=True, exclude_none=True))


@router.post("/parent/children/{child_id}/assessments/simple", response_model=AssessmentResponse)
async def simple_assessment(child_id: UUID, request: SimpleAssessmentRequest,
                             parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = AssessmentService(db)
    return await svc.create_assessment(child_id, "initial", request.answers, request.recommendations)
