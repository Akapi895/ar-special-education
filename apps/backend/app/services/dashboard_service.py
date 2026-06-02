from uuid import UUID

from sqlalchemy.ext.asyncio import AsyncSession

from app.core.exceptions import NotFound
from app.repositories import (
    ChildRepository, SessionRepository, ResultRepository,
    SkillRepository, EmotionRepository, PointRepository, ActivityRepository,
)


class DashboardService:
    def __init__(self, db: AsyncSession):
        self.child_repo = ChildRepository(db)
        self.session_repo = SessionRepository(db)
        self.result_repo = ResultRepository(db)
        self.skill_repo = SkillRepository(db)
        self.emotion_repo = EmotionRepository(db)
        self.point_repo = PointRepository(db)
        self.activity_repo = ActivityRepository(db)

    async def get_dashboard(self, child_id: UUID) -> dict:
        child = await self.child_repo.get_by_id(child_id)
        if not child:
            raise NotFound("Child not found")

        stats = await self.result_repo.get_stats_by_child(child_id)
        total_sessions = await self.session_repo.count_by_child(child_id)
        skills = await self.skill_repo.get_by_child(child_id)
        emotions = await self.emotion_repo.get_analytics(child_id)
        points_earned = await self.point_repo.get_earned_total(child_id)
        points_spent = await self.point_repo.get_spent_total(child_id)
        points_balance = await self.point_repo.get_balance(child_id)

        strongest = max(skills, key=lambda s: s.mastery_score) if skills else None
        weakest = min(skills, key=lambda s: s.mastery_score) if skills else None

        activities = await self.activity_repo.get_all_active()
        category_progress = []
        for act in activities:
            act_results = await self.result_repo.get_by_child_and_activity(child_id, act.id)
            if act_results:
                total = len(act_results)
                correct = sum(1 for r in act_results if r.is_correct)
                avg_time = float(sum(r.time_spent_seconds for r in act_results)) / total if total > 0 else 0
                category_progress.append({
                    "activity_id": str(act.id),
                    "activity_code": act.code,
                    "activity_title_vi": act.title_vi,
                    "total_sessions": 0,
                    "total_attempts": total,
                    "correct_attempts": correct,
                    "success_rate": round(correct / total * 100, 2) if total > 0 else 0,
                    "avg_time_per_question": round(avg_time, 2),
                })

        return {
            "child_id": child_id,
            "total_sessions": total_sessions,
            "total_activities_completed": stats["total_attempts"],
            "overall_success_rate": round(stats["success_rate"], 2),
            "total_time_spent_minutes": round(stats["total_time_seconds"] / 60, 2),
            "current_streak": 0,
            "strongest_skill": strongest.skill_tag if strongest else None,
            "weakest_skill": weakest.skill_tag if weakest else None,
            "points": {
                "total_earned": points_earned,
                "total_spent": points_spent,
                "current_balance": points_balance,
            },
            "category_progress": category_progress,
            "recent_emotions": emotions[:5] if emotions else [],
        }

    async def get_category_progress(self, child_id: UUID) -> list[dict]:
        activities = await self.activity_repo.get_all_active()
        result = []
        for act in activities:
            act_results = await self.result_repo.get_by_child_and_activity(child_id, act.id)
            total = len(act_results)
            correct = sum(1 for r in act_results if r.is_correct) if act_results else 0
            avg_time = float(sum(r.time_spent_seconds for r in act_results)) / total if total > 0 else 0
            result.append({
                "activity_id": str(act.id),
                "activity_code": act.code,
                "activity_title_vi": act.title_vi,
                "total_sessions": 0,
                "total_attempts": total,
                "correct_attempts": correct,
                "success_rate": round(correct / total * 100, 2) if total > 0 else 0,
                "avg_time_per_question": round(avg_time, 2),
            })
        return result

    async def get_emotion_analytics(self, child_id: UUID) -> list[dict]:
        return await self.emotion_repo.get_analytics(child_id)
