from uuid import UUID

from fastapi import APIRouter, Depends, Query
from sqlalchemy.ext.asyncio import AsyncSession

from app.api.deps import get_current_parent
from app.core.database import get_db
from app.schemas.task import (
    TaskCreateRequest,
    TaskUpdateRequest,
    TaskResponse,
    ChildTaskResponse,
    AssignTaskRequest,
    CreateAndAssignRequest,
    CompleteTaskRequest,
    VerifyTaskRequest,
    RejectTaskRequest,
    GiveUpTaskRequest,
    TaskStatusResponse,
)
from app.services.task_service import TaskService

router = APIRouter()


@router.get("/parent/tasks", response_model=list[TaskResponse])
async def list_tasks(parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    return await svc.get_tasks(parent.id)


@router.post("/parent/tasks", response_model=TaskResponse)
async def create_task(request: TaskCreateRequest, parent=Depends(get_current_parent),
                       db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    return await svc.create_task(parent.id, request)


@router.put("/parent/tasks/{task_id}", response_model=TaskResponse)
async def update_task(task_id: UUID, request: TaskUpdateRequest,
                       parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    return await svc.update_task(task_id, parent.id, request)


@router.delete("/parent/tasks/{task_id}")
async def delete_task(task_id: UUID, parent=Depends(get_current_parent),
                       db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    await svc.delete_task(task_id, parent.id)
    return {"detail": "Task deleted"}


@router.get("/parent/children/{child_id}/tasks/suggested", response_model=list[TaskResponse])
async def suggested_tasks(child_id: UUID, parent=Depends(get_current_parent),
                           db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    return await svc.get_suggested_tasks(child_id)


@router.get("/parent/children/{child_id}/tasks", response_model=list[ChildTaskResponse])
async def list_child_tasks(child_id: UUID, status: str | None = Query(None),
                            parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    return await svc.get_child_tasks(child_id, status)


@router.post("/parent/children/{child_id}/tasks/{task_id}/start", response_model=ChildTaskResponse)
async def start_task(child_id: UUID, task_id: UUID,
                      parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    ct = await svc.get_child_task(task_id)
    return await svc.start_task(ct.id)


@router.post("/parent/children/{child_id}/tasks/{task_id}/assign", response_model=ChildTaskResponse)
async def assign_task(child_id: UUID, task_id: UUID, request: AssignTaskRequest,
                       parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    return await svc.assign_task(child_id, task_id, parent.id)


@router.post("/parent/children/{child_id}/tasks/create-and-assign", response_model=ChildTaskResponse)
async def create_and_assign(child_id: UUID, request: CreateAndAssignRequest,
                             parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    return await svc.create_and_assign(parent.id, child_id, request)


@router.put("/parent/children/{child_id}/tasks/{child_task_id}", response_model=ChildTaskResponse)
async def update_child_task(child_id: UUID, child_task_id: UUID, request: dict,
                             parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    return await svc.update_child_task(child_task_id, request)


@router.delete("/parent/children/{child_id}/tasks/{child_task_id}")
async def delete_child_task(child_id: UUID, child_task_id: UUID,
                             parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    await svc.delete_child_task(child_task_id)
    return {"detail": "Child task deleted"}


@router.post("/parent/children/{child_id}/tasks/{child_task_id}/complete", response_model=ChildTaskResponse)
async def complete_task(child_id: UUID, child_task_id: UUID, request: CompleteTaskRequest,
                         parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    return await svc.complete_task(child_task_id, request.score)


@router.post("/parent/children/{child_id}/tasks/{child_task_id}/verify", response_model=ChildTaskResponse)
async def verify_task(child_id: UUID, child_task_id: UUID, request: VerifyTaskRequest,
                       parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    return await svc.verify_task(child_task_id, parent.id, request.feedback)


@router.post("/parent/children/{child_id}/tasks/{child_task_id}/reject", response_model=ChildTaskResponse)
async def reject_task(child_id: UUID, child_task_id: UUID, request: RejectTaskRequest,
                       parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    return await svc.reject_task(child_task_id, parent.id, request.feedback)


@router.get("/parent/children/{child_id}/tasks/unassigned", response_model=list[TaskResponse])
async def unassigned_tasks(child_id: UUID, parent=Depends(get_current_parent),
                            db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    all_tasks = await svc.get_tasks(parent.id)
    assigned = await svc.get_child_tasks(child_id)
    assigned_ids = {ct.task_id for ct in assigned}
    return [t for t in all_tasks if t.id not in assigned_ids]


@router.get("/parent/children/{child_id}/tasks/giveup", response_model=list[ChildTaskResponse])
async def given_up_tasks(child_id: UUID, parent=Depends(get_current_parent),
                          db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    return await svc.get_child_tasks(child_id, "given_up")


@router.get("/parent/children/{child_id}/tasks/completed", response_model=list[ChildTaskResponse])
async def completed_tasks(child_id: UUID, parent=Depends(get_current_parent),
                           db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    return await svc.get_child_tasks(child_id, "completed")


@router.post("/parent/children/{child_id}/tasks/{task_id}/giveup", response_model=ChildTaskResponse)
async def give_up_task(child_id: UUID, task_id: UUID, request: GiveUpTaskRequest,
                        parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    ct = await svc.get_child_task(task_id)
    return await svc.give_up_task(ct.id, request.reason)


@router.get("/parent/children/{child_id}/tasks/{task_id}/status", response_model=TaskStatusResponse)
async def get_task_status(child_id: UUID, task_id: UUID,
                           parent=Depends(get_current_parent), db: AsyncSession = Depends(get_db)):
    svc = TaskService(db)
    ct = await svc.get_child_task(task_id)
    return TaskStatusResponse(
        task_id=ct.task_id,
        child_task_id=ct.id,
        status=ct.status,
        score=ct.score,
        started_at=ct.started_at,
        completed_at=ct.completed_at,
    )
