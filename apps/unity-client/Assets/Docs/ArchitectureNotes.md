# Unity Architecture Notes

## Runtime Boundaries

- Learning activities depend on `Core.Learning.ActivityRunner` interfaces for AR placement, interaction, and session state.
- Runtime AR services currently live in two namespace families: `Core.AR.*` for newer adapters and `ARSpecialEducation.Core.AR` for older AR components.
- New production code should prefer `Core.AR.*`. Existing `ARSpecialEducation.Core.AR` classes are kept for compatibility until a dedicated namespace migration is scheduled.
- Editor setup scripts may use `AssetDatabase` and reflection. Runtime production flow should not.

## Activity Loading

- `SC_Boot` initializes progress, feedback, and audio services.
- `SC_MainMenu` routes to activity select or progress dashboard.
- `SC_ActivitySelect` stores `SelectedActivityData.ActivityId` and `SelectedActivityData.LessonId`.
- `SC_ARGameplay` uses `GameplayActivityRouter` and public bootstrap `Configure(...)` APIs to start the selected activity.

## Data Flow

- Every persisted round uses `ActivityResult`.
- `ActivityResult` stores serializable learning and technical issue structures, lesson id, round id, and skill tags.
- Technical issues are saved separately from learning mistakes and do not count toward mastery.

## Assembly Definition Plan

Do not add asmdefs until Unity Editor validation can run without the project-open lock. Suggested split:

- `Core.Runtime`
- `Core.AR`
- `Core.Learning`
- `Core.UI`
- `Features.Activities`
- `Project.Runtime`
- `Project.Editor`

Add these only after device/editor validation proves package references are stable.
