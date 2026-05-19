# Implementation TODO — Aligned with SYSTEM_REVIEW.md

**Last updated:** 2026-05-18

## Phase 0 — Scope freeze ✅

- [x] Document MVP boundary (SYSTEM_REVIEW.md § Phase 0)
- [x] Defer backend, new activities, parent mode complexity

---

## Phase 1 — AR Core + test sandbox 🔄

| ID | Task | Status | Paths |
|----|------|--------|-------|
| P1.1 | `ARSessionService` | ✅ | `Core/AR/ARSession/ARSessionService.cs` |
| P1.2 | `ARSessionFallback` (editor/mock) | ✅ | `Core/AR/ARSession/ARSessionFallback.cs` |
| P1.3 | `ARPlacementService` | ✅ | `Core/AR/Placement/ARPlacementService.cs` |
| P1.4 | `ARPlacementServiceMock` | ✅ | `Core/AR/Placement/ARPlacementServiceMock.cs` |
| P1.5 | `ARInteractionService` | ✅ | `Core/AR/Interaction/ARInteractionService.cs` |
| P1.6 | `ARServiceBootstrap` | ✅ | `Core/AR/ARServiceBootstrap.cs` |
| P1.7 | `ARSandboxController` | ✅ | `Core/AR/Sandbox/ARSandboxController.cs` |
| P1.8 | Editor: Setup Test Sandbox Scene | ✅ | `_Project/Editor/ARTestSandboxMenu.cs` |
| P1.9 | Run menu in Unity → save `SC_TestSandbox.unity` | ⏳ Manual | `_Project/Scenes/SC_TestSandbox.unity` |
| P1.10 | Device/simulator acceptance (planes, spawn, tap) | ⏳ Manual | — |

**Exit gate:** Sandbox passes on device OR editor mock (G/C/X keys + tap logs).

---

## Phase 2 — Quantity Match vertical slice 🔄

| ID | Task | Status | Paths |
|----|------|--------|-------|
| P2.1 | Wire `PersistRoundResult` + feedback in base presenter | ✅ | `Core/Learning/ActivityRunner/ActivityPresenter.cs` |
| P2.2 | `LearningSceneServices` (progress + feedback roots) | ✅ | `_Project/Scripts/LearningSceneServices.cs` |
| P2.3 | `QuantityMatchActivityBootstrap` | ✅ | `Features/.../QuantityMatchActivityBootstrap.cs` |
| P2.4 | Runtime UI builder | ✅ | `QuantityMatchRuntimeUI.cs`, `QuantityMatchView.cs` |
| P2.5 | Prefab resolution (`GetObjectPrefab`) | ✅ | `QuantityMatchPresenter.cs`, `ActivityPrefabSetup.cs` |
| P2.6 | Editor: Easy SO config | ✅ | `_Project/Editor/QuantityMatchConfigFactory.cs` |
| P2.7 | Editor: Setup AR Gameplay Scene | ✅ | `_Project/Editor/ARGameplaySceneMenu.cs` |
| P2.8 | Run menus in Unity → save scenes/assets | ⏳ Manual | `SC_ARGameplay.unity`, `SO_*.asset` |
| P2.9 | E2E: play QM → answer → hint → save JSON | ⏳ Manual | `persistentDataPath/learning_progress.json` |

**Exit gate:** One full round on device with saved `ActivityResult`.

---

## Phase 3 — Number Line Jump ✅

- [x] `NumberLineJumpActivityBootstrap.cs`
- [x] `NumberLineJumpRuntimeUI.cs`
- [x] `NumberLineJumpConfigFactory.cs` (Editor)
- [x] `SO_NumberLineJumpConfig_Easy.asset` (use Editor menu)
- [ ] Tile/character prefabs + labels (AR team to provide)
- [ ] Bootstrap + active root in `SC_ARGameplay` (manual scene setup)

## Phase 4 — App shell ✅

- [x] `BootLoader.cs` - Initializes services and loads main menu
- [x] `MainMenuController.cs` - Navigation to activity select and progress
- [x] `ActivitySelectController.cs` - Activity selection with activity data passing
- [x] `ActivityLoader.cs` - Dynamic activity loading in gameplay scene
- [x] `ProgressDashboardView.cs` - Reads and displays LocalProgressStorage
- [x] `SceneSetupMenu.cs` - Editor menu for scene setup
- [x] `SC_Boot`, `SC_MainMenu`, `SC_ActivitySelect` navigation scripts
- [x] `SC_ProgressDashboard` reads `LocalProgressStorage`

## Phase 5 — Progress hardening ✅

- [x] Fix `DateTime` / `Dictionary` JSON (TD-04)
  - Added `StartTimeString`/`EndTimeString` to `ActivityResult`
  - Replaced `Dictionary` with serializable `List<ActivityStatisticsEntry>`
  - Added `PrepareForSerialization()` and `DeserializeAfterLoad()` methods
- [x] Unify hint path (TD-03)
  - `ActivityPresenter.RequestHint()` now uses shared `HintSystem` service
  - Added `ResetHints()` method for activity restart
  - All hint requests now go through unified service with tracking

## Phase 6 — Compare Quantity + polish 🔄

- [x] `CompareQuantityActivityBootstrap.cs`
- [x] `CompareQuantityRuntimeUI.cs`
- [x] `CompareQuantityConfigFactory.cs` (Editor)
- [x] `SO_CompareQuantityConfig_Easy.asset` (use Editor menu)
- [ ] Audio/VFX clips (AR/Audio team to provide)
- [ ] EditMode tests

---

## Unity editor steps (required once)

1. **AR Learning → Create Quantity Match Easy Config**
2. **AR Learning → Setup Test Sandbox Scene**
3. **AR Learning → Setup AR Gameplay Scene (Quantity Match)**
4. Play `SC_TestSandbox` (editor: G/C/X/L keys; mock placement auto)
5. Play `SC_ARGameplay` (wait for placement → activity starts)
