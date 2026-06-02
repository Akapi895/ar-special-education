from uuid import UUID

from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.api.deps import get_current_parent
from app.core.database import get_db
from app.schemas.dashboard import DashboardResponse, AnalyzeEmotionReportRequest, UpdateSkillsRequest
from app.services.dashboard_service import DashboardService

router = APIRouter()


@router.get("/parent/dashboard/{child_id}")
async def get_dashboard(child_id: UUID, parent=Depends(get_current_parent),
                         db: AsyncSession = Depends(get_db)):
    svc = DashboardService(db)
    return await svc.get_dashboard(child_id)


@router.get("/parent/dashboard/{child_id}/category-progress")
async def get_category_progress(child_id: UUID, parent=Depends(get_current_parent),
                                 db: AsyncSession = Depends(get_db)):
    svc = DashboardService(db)
    return await svc.get_category_progress(child_id)


@router.get("/parent/dashboard/{child_id}/emotion-analytics")
async def get_emotion_analytics(child_id: UUID, parent=Depends(get_current_parent),
                                 db: AsyncSession = Depends(get_db)):
    svc = DashboardService(db)
    return await svc.get_emotion_analytics(child_id)


@router.post("/parent/dashboard/{child_id}/analyze-emotion-report")
async def analyze_emotion_report(child_id: UUID, request: AnalyzeEmotionReportRequest,
                                  parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = DashboardService(db)
    return await svc.get_emotion_analytics(child_id)


@router.post("/parent/dashboard/{child_id}/update-skills")
async def update_skills(child_id: UUID, request: UpdateSkillsRequest,
                         parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = DashboardService(db)
    dashboard = await svc.get_dashboard(child_id)
    return dashboard
