from uuid import UUID

from sqlalchemy.ext.asyncio import AsyncSession

from app.core.exceptions import NotFound
from app.models.assessment import Assessment
from app.repositories import AssessmentRepository, ChildRepository


class AssessmentService:
    def __init__(self, db: AsyncSession):
        self.assessment_repo = AssessmentRepository(db)
        self.child_repo = ChildRepository(db)

    async def create_assessment(self, child_id: UUID, assessment_type: str,
                                answers: dict, recommendations: list[str] | None = None) -> Assessment:
        child = await self.child_repo.get_by_id(child_id)
        if not child:
            raise NotFound("Child not found")

        assessment = Assessment(
            child_id=child_id,
            type=assessment_type,
            answers=answers,
            recommendations=recommendations,
        )
        return await self.assessment_repo.create(assessment)

    async def get_assessments(self, child_id: UUID) -> list[Assessment]:
        return await self.assessment_repo.get_by_child(child_id)

    async def get_assessment(self, assessment_id: UUID) -> Assessment:
        assessment = await self.assessment_repo.get_by_id(assessment_id)
        if not assessment:
            raise NotFound("Assessment not found")
        return assessment

    async def update_assessment(self, assessment_id: UUID, data: dict) -> Assessment:
        assessment = await self.get_assessment(assessment_id)
        for key, value in data.items():
            if value is not None:
                setattr(assessment, key, value)
        return await self.assessment_repo.update(assessment)
