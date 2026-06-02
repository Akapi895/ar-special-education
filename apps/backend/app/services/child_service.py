from uuid import UUID

from sqlalchemy.ext.asyncio import AsyncSession

from app.core.exceptions import Conflict, NotFound, BadRequest
from app.core.security import hash_password
from app.models.child import Child
from app.repositories import ChildRepository, ParentRepository
from app.schemas.child import ChildCreateRequest, ChildUpdateRequest


class ChildService:
    def __init__(self, db: AsyncSession):
        self.child_repo = ChildRepository(db)
        self.parent_repo = ParentRepository(db)

    async def create_child(self, parent_id: UUID, data: ChildCreateRequest) -> Child:
        existing = await self.child_repo.get_by_parent_and_username(parent_id, data.username)
        if existing:
            raise Conflict("Child with this username already exists")

        parent = await self.parent_repo.get_by_id(parent_id)
        if not parent:
            raise NotFound("Parent not found")

        child = Child(
            parent_id=parent_id,
            username=data.username,
            hashed_password=hash_password(data.password),
            display_name=data.display_name,
            age_years=data.age_years,
            grade=data.grade,
        )
        return await self.child_repo.create(child)

    async def get_children(self, parent_id: UUID) -> list[Child]:
        return await self.child_repo.get_by_parent(parent_id)

    async def get_child(self, child_id: UUID, parent_id: UUID | None = None) -> Child:
        child = await self.child_repo.get_by_id(child_id)
        if not child:
            raise NotFound("Child not found")
        if parent_id and child.parent_id != parent_id:
            raise NotFound("Child not found")
        return child

    async def update_child(self, child_id: UUID, parent_id: UUID, data: ChildUpdateRequest) -> Child:
        child = await self.get_child(child_id, parent_id)
        update_data = data.model_dump(exclude_unset=True, exclude_none=True)
        if "password" in update_data:
            update_data["hashed_password"] = hash_password(update_data.pop("password"))
        for key, value in update_data.items():
            setattr(child, key, value)
        return await self.child_repo.update(child)

    async def delete_child(self, child_id: UUID, parent_id: UUID):
        child = await self.get_child(child_id, parent_id)
        await self.child_repo.delete(child)

    async def complete_onboarding(self, parent_id: UUID, parent_data, children_data, assessments_data):
        parent = await self.parent_repo.get_by_id(parent_id)
        if not parent:
            raise NotFound("Parent not found")

        if parent_data.full_name:
            parent.full_name = parent_data.full_name
        if parent_data.phone:
            parent.phone = parent_data.phone
        await self.parent_repo.update(parent)

        created_children = []
        for child_data in children_data:
            child = await self.create_child(parent_id, child_data)
            created_children.append(child)

        if assessments_data:
            from app.services.assessment_service import AssessmentService
            assessment_svc = AssessmentService(self.child_repo.db)
            for ass_data in assessments_data:
                child = await self.child_repo.get_by_parent_and_username(parent_id, ass_data.child_username)
                if child:
                    await assessment_svc.create_assessment(
                        child.id, "initial", ass_data.answers, ass_data.recommendations
                    )

        return created_children
