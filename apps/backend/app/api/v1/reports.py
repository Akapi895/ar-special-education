from uuid import UUID

from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.api.deps import get_current_parent
from app.core.database import get_db
from app.schemas.report import ReportResponse, GenerateReportRequest
from app.services.report_service import ReportService

router = APIRouter()


@router.get("/parent/reports/{child_id}", response_model=list[ReportResponse])
async def list_reports(child_id: UUID, parent=Depends(get_current_parent),
                        db: AsyncSession = Depends(get_db)):
    svc = ReportService(db)
    return await svc.get_reports(child_id)


@router.get("/parent/reports/{child_id}/{report_id}", response_model=ReportResponse)
async def get_report(child_id: UUID, report_id: UUID,
                      parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = ReportService(db)
    return await svc.get_report(report_id)


@router.post("/parent/reports/{child_id}/generate", response_model=ReportResponse)
async def generate_report(child_id: UUID, request: GenerateReportRequest,
                           parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = ReportService(db)
    return await svc.generate_report(
        child_id, request.type, request.period_start, request.period_end
    )
