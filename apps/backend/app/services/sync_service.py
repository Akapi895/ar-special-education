from uuid import UUID

from sqlalchemy.ext.asyncio import AsyncSession

from app.core.exceptions import NotFound
from app.models.session import Session
from app.models.result import ActivityResult
from app.repositories import (
    SessionRepository, ResultRepository, SkillRepository,
    ActivityRepository, ChildRepository,
)


class SyncService:
    def __init__(self, db: AsyncSession):
        self.session_repo = SessionRepository(db)
        self.result_repo = ResultRepository(db)
        self.skill_repo = SkillRepository(db)
        self.activity_repo = ActivityRepository(db)
        self.child_repo = ChildRepository(db)

    async def start_session(self, child_id: UUID, activity_id: UUID,
                            device_id: str | None = None, start_time=None) -> Session:
        from datetime import datetime, timezone
        child = await self.child_repo.get_by_id(child_id)
        if not child:
            raise NotFound("Child not found")

        session = Session(
            child_id=child_id,
            activity_id=activity_id,
            device_id=device_id,
            start_time=start_time or datetime.now(timezone.utc),
            status="in_progress",
        )
        return await self.session_repo.create(session)

    async def end_session(self, session_id: UUID, end_time=None, status: str = "completed") -> Session:
        session = await self.session_repo.get_by_id(session_id)
        if not session:
            raise NotFound("Session not found")

        from datetime import datetime, timezone
        session.end_time = end_time or datetime.now(timezone.utc)
        session.status = status
        if session.start_time and session.end_time:
            session.duration_seconds = int((session.end_time - session.start_time).total_seconds())
        return await self.session_repo.update(session)

    async def sync_results(self, child_id: UUID, results_data: list[dict]) -> list[ActivityResult]:
        created_results = []
        for data in results_data:
            result = ActivityResult(
                child_id=child_id,
                session_id=data["session_id"],
                activity_id=data["activity_id"],
                lesson_id=data.get("lesson_id"),
                round_id=data["round_id"],
                level_number=data.get("level_number", 1),
                difficulty=data.get("difficulty", "easy"),
                is_correct=data["is_correct"],
                total_attempts=data.get("total_attempts", 1),
                hints_used_count=data.get("hints_used_count", 0),
                time_spent_seconds=data.get("time_spent_seconds", 0),
                start_time=data["start_time"],
                end_time=data["end_time"],
                counts_toward_mastery=data.get("counts_toward_mastery", True),
                error_type=data.get("error_type"),
                technical_issue=data.get("technical_issue"),
                skill_tags=data.get("skill_tags", []),
                additional_data=data.get("additional_data"),
            )
            result = await self.result_repo.create(result)
            created_results.append(result)

            if result.counts_toward_mastery and result.skill_tags:
                for skill_tag in result.skill_tags:
                    await self.skill_repo.upsert_mastery(
                        child_id, skill_tag, result.is_correct
                    )

        return created_results

    async def get_activities(self) -> list[dict]:
        activities = await self.activity_repo.get_all_active()
        result = []
        for act in activities:
            lessons = await self.activity_repo.get_lessons_by_activity(act.id)
            result.append({
                "id": str(act.id),
                "code": act.code,
                "title_vi": act.title_vi,
                "title_en": act.title_en,
                "description_vi": act.description_vi,
                "description_en": act.description_en,
                "sort_order": act.sort_order,
                "lessons": [
                    {
                        "id": str(lesson.id),
                        "code": lesson.code,
                        "title_vi": lesson.title_vi,
                        "title_en": lesson.title_en,
                        "difficulty": lesson.difficulty,
                        "skill_tags": lesson.skill_tags,
                        "sort_order": lesson.sort_order,
                    }
                    for lesson in lessons
                ],
            })
        return result
