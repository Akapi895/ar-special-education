from uuid import UUID
from datetime import date, datetime, timezone

from sqlalchemy.ext.asyncio import AsyncSession

from app.models.report import Report
from app.repositories import ReportRepository, ChildRepository, ResultRepository, SkillRepository, EmotionRepository
from app.core.exceptions import NotFound


class ReportService:
    def __init__(self, db: AsyncSession):
        self.report_repo = ReportRepository(db)
        self.child_repo = ChildRepository(db)
        self.result_repo = ResultRepository(db)
        self.skill_repo = SkillRepository(db)
        self.emotion_repo = EmotionRepository(db)

    async def get_reports(self, child_id: UUID) -> list[Report]:
        return await self.report_repo.get_by_child(child_id)

    async def get_report(self, report_id: UUID) -> Report:
        report = await self.report_repo.get_by_id(report_id)
        if not report:
            raise NotFound("Report not found")
        return report

    async def generate_report(self, child_id: UUID, report_type: str = "monthly",
                              period_start: date | None = None,
                              period_end: date | None = None) -> Report:
        child = await self.child_repo.get_by_id(child_id)
        if not child:
            raise NotFound("Child not found")

        if not period_end:
            period_end = date.today()
        if not period_start:
            if report_type == "weekly":
                from datetime import timedelta
                period_start = period_end - timedelta(days=7)
            else:
                from dateutil.relativedelta import relativedelta
                period_start = period_end.replace(day=1)

        stats = await self.result_repo.get_stats_by_child(child_id)
        skills = await self.skill_repo.get_by_child(child_id)
        emotions = await self.emotion_repo.get_analytics(child_id)

        report_data = {
            "period": {
                "start": str(period_start),
                "end": str(period_end),
            },
            "statistics": stats,
            "skills": [
                {
                    "skill_tag": s.skill_tag,
                    "mastery_score": float(s.mastery_score),
                    "total_attempts": s.total_attempts,
                    "last_practiced": str(s.last_practiced) if s.last_practiced else None,
                }
                for s in skills
            ],
            "emotions": emotions,
        }

        titles = {
            "weekly": f"Báo cáo tuần {period_start} - {period_end}",
            "monthly": f"Báo cáo tháng {period_start.month}/{period_start.year}",
            "emotion": f"Báo cáo cảm xúc {period_start} - {period_end}",
            "custom": f"Báo cáo {period_start} - {period_end}",
        }

        report = Report(
            child_id=child_id,
            type=report_type,
            title_vi=titles.get(report_type, f"Báo cáo {period_start} - {period_end}"),
            title_en=f"Report {period_start} - {period_end}",
            summary=f"Success rate: {stats['success_rate']:.1f}%, Skills mastered: {sum(1 for s in skills if s.mastery_score >= 80)}/{len(skills)}" if skills else "No data",
            data=report_data,
            period_start=period_start,
            period_end=period_end,
        )
        return await self.report_repo.create(report)
