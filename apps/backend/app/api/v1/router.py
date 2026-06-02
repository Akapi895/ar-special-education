from fastapi import APIRouter

from app.api.v1.auth import router as auth_router
from app.api.v1.children import router as children_router
from app.api.v1.assessments import router as assessments_router
from app.api.v1.tasks import router as tasks_router
from app.api.v1.shop import router as shop_router
from app.api.v1.dashboard import router as dashboard_router
from app.api.v1.progress import router as progress_router
from app.api.v1.emotions import router as emotions_router
from app.api.v1.interactions import router as interactions_router
from app.api.v1.reports import router as reports_router
from app.api.v1.notifications import router as notifications_router
from app.api.v1.sync import router as sync_router

router = APIRouter(prefix="/api/v1")

router.include_router(auth_router)
router.include_router(children_router)
router.include_router(assessments_router)
router.include_router(tasks_router)
router.include_router(shop_router)
router.include_router(dashboard_router)
router.include_router(progress_router)
router.include_router(emotions_router)
router.include_router(interactions_router)
router.include_router(reports_router)
router.include_router(notifications_router)
router.include_router(sync_router)
