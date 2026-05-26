# Tong Ket He Thong Unity Hien Tai

**Ngay ra soat:** 2026-05-26  
**Pham vi:** monorepo `BTL`, trong tam la Unity client tai `apps/unity-client`  
**Nguon doi chieu:** `README.md`, `ROLE_DOC.txt`, cac file markdown trong `.agent/`, tai lieu trong `Assets/Docs/`, va code/assets hien co trong Unity project.

## 1. Nhan dinh nhanh

He thong hien tai khong con la skeleton rong nhu audit cu trong `.agent/SYSTEM_STATUS.md` nua. Code AR Core, scene san pham, config asset, runtime UI va app shell da duoc bo sung dang ke.

Trang thai gan dung nhat:

| Khu vuc | Trang thai hien tai | Ghi chu |
|---|---|---|
| Unity client | Dang la phan chinh cua he thong | Unity `6000.0.71f1`, co AR Foundation/XRI/URP |
| AR Core | Da co implement code | Session, placement, placement mock, interaction, sandbox |
| Learning framework | Da co khung dung chung | Presenter/View/Config/Result/Hint/Feedback/Progress |
| Activities | Da co 3 activity | Quantity Match, Number Line Jump, Compare Quantity |
| Runtime UI | Da co fallback runtime UI | Cac view co the tao UI khi thieu prefab UI |
| Product scenes | Da co noi dung YAML that | 6 scene co dung luong, khong con 0-byte |
| Config assets | Da co `SO_*_Easy.asset` cho 3 activity | Moi activity hien co 10 rounds easy |
| App shell | Da co code va scene co controller | Main menu, activity select, progress dashboard |
| Backend | Gan nhu skeleton | Chua co API/thuc thi business logic |
| Automated tests | Chua thay test C# thuc te | Cac thu muc `Tests/` chua co test script |

Luu y: `unity_compile.log` hien ket thuc bang loi Unity project dang duoc mo boi instance khac, nen lan ra soat nay la static review, khong phai runtime playtest trong Unity Editor/device.

## 2. Monorepo overview

```text
BTL/
|-- README.md
|-- ROLE_DOC.txt
|-- .agent/
|   |-- SYSTEM_STATUS.md
|   |-- SYSTEM_REVIEW.md
|   |-- IMPLEMENTATION_TODO.md
|   |-- PHASE2_TEST_GUIDE.md
|   |-- LOCAL_UNITY_FULL_TEST_GUIDE.md
|   `-- PHASE_VERIFICATION.md
|-- apps/
|   |-- unity-client/
|   `-- backend/
|-- docs/
|   |-- planning/
|   `-- report-latex/
`-- scripts/
```

`apps/unity-client` la project can tap trung. `apps/backend` hien chi co folder skeleton, `.env.example`, `requirements.txt` va cac `.gitkeep`.

## 3. Unity project

**Unity version:** `6000.0.71f1`  
**Packages chinh trong `Packages/manifest.json`:**

| Package | Version | Vai tro |
|---|---:|---|
| `com.unity.xr.arfoundation` | `6.3.4` | AR session, plane, raycast |
| `com.unity.xr.arcore` | `6.3.1` | Android AR provider |
| `com.unity.xr.arkit` | `6.3.4` | iOS AR provider |
| `com.unity.xr.interaction.toolkit` | `3.3.0` | XRI / AR interaction support |
| `com.unity.xr.management` | `4.5.4` | XR loader management |
| `com.unity.render-pipelines.universal` | `17.0.4` | URP |
| `com.unity.inputsystem` | `1.19.0` | Input System |

## 4. Assets directory map

```text
apps/unity-client/Assets/
|-- _Project/                  # Product scenes, app shell scripts, editor setup menus
|-- Core/                      # AR Core, Learning framework, Data, Support services
|-- Features/                  # Product features/activities
|-- Resources/                 # Runtime-loadable animal prefabs
|-- Shared/                    # Shared prefab/art/audio/ui folders and AR shared prefabs
|-- MobileARTemplateAssets/    # Unity Mobile AR template reference/demo assets
|-- Samples/                   # XR Interaction Toolkit samples
|-- Scenes/                    # Template SampleScene
|-- Settings/
|-- TextMesh Pro/
|-- ThirdParty/
|-- XR/
`-- XRI/
```

`Assets/DataDefinitions/` chua ton tai, du `README.md` co mo ta day la noi luu ScriptableObject dinh nghia noi dung. Hien tai activity config dang nam trong tung feature: `Features/Activities/*/ScriptableObjects/`.

## 5. Core modules

### 5.1 `Assets/Core/AR`

AR Core da duoc implement thanh cac module:

| Module | File tieu bieu | Vai tro |
|---|---|---|
| Session | `ARSessionService.cs`, `ARSessionFallback.cs`, `ARSessionBootstrap.cs` | Quan ly session/tracking, fallback khi khong co ARSession |
| Placement | `ARPlacementService.cs`, `ARPlacementServiceMock.cs`, `ARPlacementController.cs`, `LearningAreaAnchor.cs` | Raycast plane, spawn object, grid/circle spawn, mock placement trong Editor |
| Interaction | `ARInteractionService.cs`, `ARTapInteractor.cs`, `ARDragInteractor.cs`, `ARSelectableObject.cs` | Dang ky object co the tap, physics raycast, highlight, select/drag |
| PlaneDetection | `ARPlaneDetectionController.cs` | Dieu khien plane detection |
| Sandbox | `ARSandboxController.cs` | Test spawn grid/circle/clear/tap bang phim |
| Bootstrap | `ARServiceBootstrap.cs` | Resolve va khoi tao session/placement/interaction |

Trong Editor desktop, `ARServiceBootstrap` uu tien `ARPlacementServiceMock`, giup test luong hoc ma khong can plane that.

### 5.2 `Assets/Core/Learning`

Learning framework gom:

| Module | File | Vai tro |
|---|---|---|
| Activity runner | `ActivityPresenter.cs`, `ActivityConfig.cs`, `ActivityView.cs`, `IActivityRunner.cs` | State machine, round lifecycle, submit answer, hint, save result, feedback |
| AR contracts | `IARPlacementService.cs`, `IARInteractionService.cs`, `IARSessionService.cs` | Interface de learning layer khong phu thuoc truc tiep AR Foundation |
| Models | `ActivityResult.cs`, `ActivityAnswer.cs`, `ActivityHint.cs`, `ActivityState.cs` | Data dung chung cho result/answer/hint/state |
| Utils | `ARGroupSpawnUtility.cs` | Ho tro spawn group trong AR cho activity |

`ActivityPresenter` hien da goi `ProgressStorageProxy.Instance.SaveResult(currentResult)` khi round ket thuc, va goi `FeedbackServiceProxy` cho correct/incorrect/success. Hint da di qua `HintSystem` dung chung.

### 5.3 `Assets/Core/Data`

`LocalProgressStorage` va `ProgressStorageProxy` ghi tien do vao local JSON:

| File | Vai tro |
|---|---|
| `LocalProgressStorage.cs` | Luu/load `learning_progress.json`, session data, thong ke activity |
| `ProgressStorageProxy.cs` | MonoBehaviour singleton, auto session, API tien loi cho scene |

Da co fix cho `DateTime` bang `StartTimeString`/`EndTimeString` va thong ke bang `List<ActivityStatisticsEntry>`. Mot diem can tiep tuc xem lai: `ActivityResult.ErrorType` dang la nullable enum (`ErrorType?`), co nguy co khong serialize on dinh bang Unity `JsonUtility`.

### 5.4 `Assets/Core/Support`

| Module | File | Trang thai |
|---|---|---|
| Hint system | `HintSystem.cs`, `HintServiceProxy.cs` | Co escalation, tracking, cooldown; con TODO contextual hint theo error type |
| Feedback system | `FeedbackSystem.cs`, `FeedbackServiceProxy.cs`, `FeedbackData.cs`, `FeedbackType.cs` | Co event/hook cho sound/VFX, nhung playback that van la TODO/log |

## 6. Feature modules

```text
Assets/Features/
|-- Activities/
|   |-- QuantityMatch/
|   |-- NumberLineJump/
|   |-- CompareQuantity/
|   |-- Shared/
|   |-- ActivityPrefabSetup.cs
|   `-- ARAnimalPresentation.cs
|-- Home/
|-- Progress/
`-- ParentMode/
```

### 6.1 Quantity Match

**Path:** `Assets/Features/Activities/QuantityMatch/`

| File | Vai tro |
|---|---|
| `QuantityMatchPresenter.cs` | Load round, spawn group, xu ly chon group/nhap so, check answer |
| `QuantityMatchView.cs` | UI hien target/progress/buttons/hint/feedback/summary |
| `QuantityMatchRuntimeUI.cs` | Tao Canvas fallback neu thieu UI prefab |
| `QuantityMatchConfig.cs` | ScriptableObject config, hints, feedback, spawn settings |
| `QuantityMatchQuestion.cs` | Target number, group counts, correct group |
| `QuantityMatchAnswer.cs` | Answer model |
| `QuantityMatchActivityBootstrap.cs` | Khoi dong activity trong scene |
| `SO_QuantityMatchConfig_Easy.asset` | 10 round easy |

### 6.2 Number Line Jump

**Path:** `Assets/Features/Activities/NumberLineJump/`

| File | Vai tro |
|---|---|
| `NumberLineJumpPresenter.cs` | Spawn number line, character, jump logic, equation, answer check |
| `NumberLineJumpView.cs` | UI dieu huong jump, equation, feedback, summary |
| `NumberLineJumpRuntimeUI.cs` | Runtime UI fallback |
| `NumberLineJumpConfig.cs` | Config/hints/feedback/tile spacing |
| `NumberLineJumpQuestion.cs` | Range, start, target, direction, max jumps |
| `NumberLineJumpAnswer.cs` | Jump history, final position, equation |
| `JumpDirection.cs` | Direction enum |
| `NumberLineJumpActivityBootstrap.cs` | Khoi dong activity |
| `SO_NumberLineJumpConfig_Easy.asset` | 10 round easy |

Gioi han dang thay trong code: `GetTilePrefab()` van log not implemented va dung fallback primitive/tile runtime; jump animation/boundary bump con TODO.

### 6.3 Compare Quantity

**Path:** `Assets/Features/Activities/CompareQuantity/`

| File | Vai tro |
|---|---|
| `CompareQuantityPresenter.cs` | Spawn hai group, nhan More/Fewer/Equal, check answer |
| `CompareQuantityView.cs` | UI question, comparison buttons, feedback |
| `CompareQuantityRuntimeUI.cs` | Runtime UI fallback |
| `CompareQuantityConfig.cs` | Config labels, hints, outcome feedback |
| `CompareQuantityQuestion.cs` | Left/right counts, correct comparison |
| `CompareQuantityAnswer.cs` | Selected comparison |
| `CompareQuantityActivityBootstrap.cs` | Khoi dong activity |
| `SO_CompareQuantityConfig_Easy.asset` | 10 round easy |

### 6.4 Shared activity helpers

| File | Vai tro |
|---|---|
| `ActivityPrefabSetup.cs` | Tao placeholder prefabs, load animal prefab tu `Resources/ARAnimals`, normalize material/size |
| `ARAnimalPresentation.cs` | Idle animation nhe cho learning object |
| `Shared/README_SCENE_SETUP.md` | Huong dan setup scene/activity |

Feature `Prefabs/`, `UI/`, `Art/`, `Audio/`, `Tests/` hien da co cau truc, nhung chua thay prefab/audio/test rieng cho tung activity.

## 7. Project shell va editor tooling

### 7.1 `_Project/Scripts`

| File | Vai tro |
|---|---|
| `BootLoader.cs` | Khoi tao services va load Main Menu |
| `MainMenuController.cs` | Nut Start Learning / View Progress |
| `ActivitySelectController.cs` | Tao/wire activity buttons, luu `SelectedActivityData`, load gameplay |
| `ActivityFlowNavigator.cs` | Dieu huong next activity va progress dashboard |
| `GameplayActivityRouter.cs` | Tao NumberLineJump/CompareQuantity runtime khi activity duoc chon |
| `ActivityLoader.cs` | Loader theo selected activity, hien co ve sau/khong thay gan trong `SC_ARGameplay` |
| `LearningSceneServices.cs` | Dam bao progress/feedback/router ton tai trong gameplay scene |
| `ProgressDashboardView.cs` | Doc local progress va hien overall/activity stats |

### 7.2 `_Project/Editor`

Editor menu da co cho:

| File | Menu/chuc nang |
|---|---|
| `QuantityMatchConfigFactory.cs` | Tao `SO_QuantityMatchConfig_Easy.asset` |
| `NumberLineJumpConfigFactory.cs` | Tao `SO_NumberLineJumpConfig_Easy.asset` |
| `CompareQuantityConfigFactory.cs` | Tao `SO_CompareQuantityConfig_Easy.asset` |
| `ARTestSandboxMenu.cs` | Setup `SC_TestSandbox` |
| `ARGameplaySceneMenu.cs` | Setup `SC_ARGameplay` cho Quantity Match |
| `SceneSetupMenu.cs` | Setup Boot/MainMenu/ActivitySelect/Progress scenes |
| `ProductSceneRepairMenu.cs` | Repair product scenes |
| `LocalUnityFullTestRunner.cs` | Ho tro test local full flow trong Editor |

## 8. Scenes hien co

Tat ca scene san pham nam trong `Assets/_Project/Scenes/` va co dung luong that:

| Scene | Vai tro | Trang thai nhan thay |
|---|---|---|
| `SC_Boot.unity` | Init services, load Main Menu | Co `BootLoader`, nhung chua nam trong Build Settings hien tai |
| `SC_MainMenu.unity` | Menu chinh | Co `MainMenuController`, canvas, buttons |
| `SC_ActivitySelect.unity` | Chon activity | Co buttons cho 3 activity va `ActivitySelectController` |
| `SC_ARGameplay.unity` | Gameplay AR dung chung | Co `ARServiceBootstrap`, `QuantityMatchActivity`, `LearningSceneServices`, `QuantityMatchRuntimeUI` |
| `SC_ProgressDashboard.unity` | Dashboard tien do | Co `ProgressDashboardView` va activity stats text |
| `SC_TestSandbox.unity` | Sandbox AR | Co `AR Session`, `ARSandboxController`, `ARServiceBootstrap` |

`ProjectSettings/EditorBuildSettings.asset` hien dang bat dau tu `SC_MainMenu`, sau do `SC_ActivitySelect`, `SC_ARGameplay`, `SC_ProgressDashboard`, `SC_TestSandbox`. `SC_Boot` ton tai nhung khong co trong Build Settings.

## 9. Data/content assets

Da co 3 config asset:

| Asset | Noi dung |
|---|---|
| `SO_QuantityMatchConfig_Easy.asset` | 10 round: dem/khop so voi group, target 2-10 |
| `SO_NumberLineJumpConfig_Easy.asset` | 10 round: range 0-10, start/target, left/right jumps |
| `SO_CompareQuantityConfig_Easy.asset` | 10 round: compare left/right, More/Fewer/Equal |

`Assets/Resources/ARAnimals/Prefabs/` co nhieu animal prefab imported, duoc `ActivityPrefabSetup` load de thay the placeholder hinh khoi.

`Assets/Shared/Prefabs/` co cac prefab AR chung:

| Prefab | Vai tro |
|---|---|
| `PFB_ARAnchorMarker.prefab` | Marker anchor |
| `PFB_ARInteractiveObject.prefab` | Object AR co tuong tac |
| `PFB_ARMockObject.prefab` | Object mock |
| `PFB_LearningAreaMarker.prefab` | Marker vung hoc |

Chua thay prefab UI/lesson rieng nhu `PFB_QuantityMatchPanel`, `PFB_NumberTile`, `PFB_JumpCharacter` trong feature folders; he thong dang dua nhieu vao runtime UI va runtime/generated placeholders.

## 10. Runtime flow hien tai

### Direct gameplay flow

```text
SC_ARGameplay
-> ARServiceBootstrap resolve session/placement/interaction
-> LearningSceneServices tao ProgressStorageProxy, FeedbackServiceProxy, GameplayActivityRouter neu thieu
-> QuantityMatchActivityBootstrap khoi dong Quantity Match mac dinh
-> QuantityMatchRuntimeUI tao UI neu thieu reference
-> ActivityPresenter quan ly round, hint, feedback, save result
```

### Shell flow

```text
SC_MainMenu
-> Start Learning
-> SC_ActivitySelect
-> chon activity, set SelectedActivityData
-> SC_ARGameplay
-> GameplayActivityRouter tao activity tuong ung neu khac QuantityMatch
-> hoan thanh, ActivityFlowNavigator co the sang activity tiep theo hoac dashboard
```

Mot diem can chu y: `GameplayActivityRouter` co the load config NumberLine/Compare tu `Resources`, neu khong co thi trong Editor dung `AssetDatabase`, va neu van khong co thi tao runtime config. Hien config assets dang nam trong `Features/.../ScriptableObjects`, khong phai `Resources/ActivityConfigs`, nen can xac nhan packaging khi build device.

## 11. Build/run state

- `unity_compile.log` hien khong phai log compile pass; no ket thuc bang thong bao Unity project dang duoc mo boi instance khac.
- Chua co bang chung trong repo ve device AR pass.
- Chua co EditMode/PlayMode tests.
- Cac file `.agent/PHASE2_TEST_GUIDE.md` va `.agent/LOCAL_UNITY_FULL_TEST_GUIDE.md` mo ta cach test Phase 1/2/4 trong Unity Editor.

## 12. Rut gon trang thai module

| Module | Muc do san sang |
|---|---|
| AR sandbox/editor mock | Gan san sang de test local |
| Quantity Match vertical slice | Gan playable trong Editor/mock, can playtest xac nhan |
| Number Line Jump | Code/config/runtime UI co, can verify scene route va UX |
| Compare Quantity | Code/config/runtime UI co, can verify scene route va UX |
| Shell | Co scene/controller, nhung Build Settings chua dung Boot |
| Progress dashboard | Co UI/doc du lieu local, can test voi JSON that |
| Audio/VFX | Hook co, playback that chua co |
| Content/prefab polish | Thieu prefab UI/lesson chuan; dang dung runtime/fallback |
| Backend sync | Chua implement |
| Test automation | Chua co test C# thuc te |
