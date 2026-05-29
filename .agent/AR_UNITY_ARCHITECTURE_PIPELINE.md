# Tổng Kết Kiến Trúc & Pipeline - AR Unity Client

> **Dự án**: AR Special Education - Ứng dụng học toán cho trẻ em bằng AR  
> **Unity Version**: Unity 6 (theo csproj)  
> **AR Framework**: ARFoundation + XR Interaction Toolkit  
> **Target Platform**: iOS (AR), Editor (simulation)

---

## 1. Tổng Quan Kiến Trúc

### 1.1 Kiến Trúc Layer (Bottom-Up)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           LAYER 6: SCENES                                    │
│  SC_Boot → SC_MainMenu → SC_ActivitySelect → SC_ARGameplay → SC_Progress    │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│                         LAYER 5: NAVIGATION & FLOW                           │
│         ActivityFlowNavigator | UIScreenManager | SceneManager                │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│                          LAYER 4: ACTIVITY LAYER                             │
│  ┌─────────────────┐  ┌──────────────────┐  ┌────────────────────────────┐   │
│  │ QuantityMatch   │  │ NumberLineJump   │  │ CompareQuantity           │   │
│  │ Presenter      │  │ Presenter        │  │ Presenter                 │   │
│  │ View           │  │ View             │  │ View                      │   │
│  │ Bootstrap      │  │ Bootstrap         │  │ Bootstrap                 │   │
│  └─────────────────┘  └──────────────────┘  └────────────────────────────┘   │
│                                                                              │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │              ActivityPresenter (Base Class)                            │   │
│  │  - State machine: Initializing → Ready → InProgress → Paused/Completed │   │
│  │  - Hint system (unified HintSystem)                                   │   │
│  │  - Result tracking & persistence                                       │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│                          LAYER 3: AR SERVICES (Core)                        │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │                     ARServiceBootstrap (Singleton)                    │   │
│  │  - Locates/resolves all AR services                                   │   │
│  │  - Decides mock vs real placement (Editor vs Mobile)                  │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
│                                    │                                         │
│  ┌─────────────────┐  ┌──────────────┴───────────┐  ┌──────────────────┐   │
│  │ IARSessionService│ │  IARPlacementService   │  │IARInteractionSvc  │   │
│  │                 │  │                        │  │                   │   │
│  │ - ARSessionSvc  │  │ - ARPlacementService   │  │ - ARInteraction   │   │
│  │ - ARSessionBkup │  │ - ARPlacementMock      │  │   Service         │   │
│  │                 │  │                        │  │                   │   │
│  │ - Tracking state│  │ - Learning area anchor │  │ - Tap/Select      │   │
│  │ - Native camera │  │ - Plane detection      │  │ - Highlight       │   │
│  │ - WebCam fallback│ │ - Object spawning      │  │ - Drag (optional) │   │
│  └─────────────────┘  └────────────────────────┘  └──────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│                          LAYER 2: SUPPORT SERVICES                            │
│  ┌────────────────┐  ┌──────────────────┐  ┌───────────────────────────┐    │
│  │ Data Layer     │  │ Feedback System  │  │ Audio Manager            │    │
│  │ - Progress     │  │ - Correct/       │  │ - Instructions          │    │
│  │   Storage      │  │   Incorrect/VFX  │  │ - Number playback        │    │
│  │ - LocalPrefs   │  │ - Hint bubbles   │  │ - Replay support         │    │
│  │ - Profile mgmt │  │                  │  │                          │    │
│  └────────────────┘  └──────────────────┘  └───────────────────────────┘    │
│                                                                              │
│  ┌────────────────┐  ┌──────────────────┐  ┌───────────────────────────┐    │
│  │ Hint System    │  │ UI Navigation    │  │ Performance Settings      │    │
│  │ - Unified hint │  │ - UIScreenManager│  │ - RuntimePerfSettings     │    │
│  │   state        │  │ - Screen stack   │  │ - Object count clamping   │    │
│  │ - Per-activity │  │ - Fade transitions│  │ - FPS target              │    │
│  └────────────────┘  └──────────────────┘  └───────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│                          LAYER 1: UNITY FOUNDATION                          │
│  ARFoundation | XR Interaction Toolkit | Unity UI (uGUI) | UnityEngine      │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Dependency Diagram

```
                    ┌──────────────────┐
                    │    BootLoader    │
                    │ (SC_Boot scene)  │
                    └────────┬─────────┘
                             │ Initialize services
                             ▼
                    ┌──────────────────┐
                    │ProgressStorage   │ ←── LocalProgressStorage
                    │FeedbackService   │ ←── FeedbackServiceProxy
                    │SimpleAudioManager│
                    │RuntimePerfSettings│
                    └────────┬─────────┘
                             │ Load SC_MainMenu
                             ▼
                    ┌──────────────────┐
                    │ UIScreenManager  │
                    │ (Singleton)      │
                    └────────┬─────────┘
                             │ User selects activity
                             ▼
                    ┌──────────────────┐
                    │ActivityFlowNav   │
                    │SelectedActivity  │
                    │   .Data         │
                    └────────┬─────────┘
                             │ Load SC_ARGameplay
                             ▼
        ┌──────────────────────────────────────────────┐
        │           SC_ARGameplay Scene                  │
        │                                               │
        │  ┌─────────────────────┐  ┌────────────────┐  │
        │  │  LearningSceneSvc   │  │GameplayActivity│  │
        │  │  (execution -150)   │  │Router (-140)   │  │
        │  └──────────┬──────────┘  └───────┬────────┘  │
        │             │                     │           │
        │             ▼                     ▼           │
        │  ┌──────────────────────────────────────────┐  │
        │  │        ARServiceBootstrap (-200)         │  │
        │  │  ┌──────────────┐                       │  │
        │  │  │ARSessionSvc │──┐                    │  │
        │  │  ├──────────────┤  │                    │  │
        │  │  │ARPlacementSvc│  │   ┌──────────────┐ │  │
        │  │  ├──────────────┤  ├──▶│ARServiceReg │ │  │
        │  │  │ARInteraction │──┘   │  istry      │ │  │
        │  │  │  Service    │       └──────┬───────┘ │  │
        │  │  └──────────────┘              │         │  │
        │  └──────────────────────────────────────────┘  │
        │                      │                          │
        │                      ▼                          │
        │  ┌──────────────────────────────────────────┐   │
        │  │    [ActivityName]ActivityBootstrap       │   │
        │  │    (e.g. QuantityMatchActivityBootstrap)│   │
        │  │                                          │   │
        │  │    ┌──────────┐   ┌───────────────┐     │   │
        │  │    │Presenter │◀──│  View         │     │   │
        │  │    │(Logic)  │──▶│  (UI/Layout) │     │   │
        │  │    └──────────┘   └───────────────┘     │   │
        │  │         │                               │   │
        │  │         ▼                               │   │
        │  │    ┌─────────────────┐                 │   │
        │  │    │ARGroupSpawnUtil│                 │   │
        │  │    │(Object spawning)│                 │   │
        │  │    └─────────────────┘                 │   │
        │  └──────────────────────────────────────────┘   │
        └──────────────────────────────────────────────────┘
```

---

## 2. Chi Tiết Từng Layer

### 2.1 Layer 1: Unity Foundation

| Component | Role | Key Classes |
|-----------|------|-------------|
| **ARFoundation** | AR session, plane detection, raycasting, camera background | `ARSession`, `XROrigin`, `ARCameraBackground`, `ARPlaneManager`, `ARRaycastManager` |
| **XR Interaction Toolkit** | XR input, touch handling, samples | `XRBaseController`, sample scripts |
| **Unity UI** | 2D overlay for activity UI | `Canvas`, `CanvasGroup`, `Button`, `Text` |

### 2.2 Layer 2: Support Services

| Service | File | Responsibility |
|---------|------|----------------|
| **ProgressStorage** | `LocalProgressStorage.cs` | JSON persistence to `Application.persistentDataPath`. Learner profiles, session tracking, activity results, skill mastery calculation |
| **LearnerProfileStore** | (nested in LocalProgressStorage) | Multiple learner profiles via PlayerPrefs + JSON |
| **FeedbackServiceProxy** | (in Core/SUPPORT) | Global feedback hooks (sound + VFX) for correct/incorrect/activity complete |
| **SimpleAudioManager** | `SimpleAudioManager.cs` | Audio playback, instruction replay, number-to-speech, lazy initialization |
| **HintSystem** | `HintSystem.cs` | Static class, unified hint state per activity/round |
| **UIScreenManager** | `UIScreenManager.cs` | Singleton, stack-based screen navigation with fade transitions |
| **RuntimePerformanceSettings** | (in Core/SUPPORT) | Clamp object counts per device tier, FPS target |

### 2.3 Layer 3: AR Core Services

#### 2.3.1 ARServiceBootstrap
- **File**: `Core/AR/ARServiceBootstrap.cs`
- **Execution Order**: `-200` (runs first)
- **Singleton**: `ARServiceBootstrap.Instance`
- **Responsibilities**:
  - Resolve all AR services via `FindAnyObjectByType`
  - Decide mock vs real placement: `usePlacementMockInEditor && Editor && !Mobile`
  - Initialize all services in correct order
- **Exposes**: `Session`, `Placement`, `Interaction` (as interfaces)

#### 2.3.2 IARSessionService / ARSessionService
- **Interface**: `Core/Learning/ActivityRunner/IARSessionService.cs`
- **Implementation**: `Core/AR/ARSession/ARSessionService.cs`
- **Key Features**:
  - Wraps Unity's `ARSession` + ARFoundation
  - Tracks `ARSessionState` → maps to `TrackingQuality` enum (None/Poor/Fair/Good/Excellent)
  - Manages `ARCameraBackground` (native camera passthrough)
  - **WebCamTexture fallback**: If AR fails after 2.5s, creates a fallback quad with live camera feed
  - Events: `OnSessionReady`, `OnSessionLost`

#### 2.3.3 IARPlacementService / ARPlacementService
- **Interface**: `Core/Learning/ActivityRunner/IARPlacementService.cs`
- **Implementation**: `Core/AR/Placement/ARPlacementService.cs`
- **Key Features**:
  - Raycasts to detect planes (horizontal only by default)
  - `LearningAreaAnchor`: stable anchor that tracks with AR
  - Object spawning: `SpawnAtPosition`, `SpawnGrid`, `SpawnCircle`, `SpawnAtLearningAreaPosition`
  - Auto-creates `LearningAreaAnchor` if not exists
  - Can hide plane visualization after placement
  - References `ARSessionBootstrap` for XR Origin and managers

#### 2.3.4 IARInteractionService / ARInteractionService
- **Interface**: (defined in ARInteractionService.cs itself)
- **Implementation**: `Core/AR/Interaction/ARInteractionService.cs`
- **Key Features**:
  - **Tap/Select**: Physics raycast against registered interactables
  - **Highlight**: Scale + color tint (via MaterialPropertyBlock)
  - **Drag** (optional): Plane-projected drag on selected object
  - Input: `EnhancedTouch` (Touchscreen) + Mouse fallback
  - UI occlusion check via `EventSystem.current.RaycastAll`
  - Tracks interactable registry with `Dictionary<GameObject, InteractableEntry>`

#### 2.3.5 ARPlaneDetectionController
- **File**: `Core/AR/PlaneDetection/ARPlaneDetectionController.cs`
- Manages plane visualization toggle
- Tracks `HashSet<TrackableId> knownValidPlanes`
- Validates planes: must be `HorizontalUp`, area >= 0.15m², `TrackingState == Tracking`
- Fires `OnPlaneDetected`, `OnPlaneLost`, `OnPlaneScanUpdated`

#### 2.3.6 ARPlacementController
- **File**: `Core/AR/Placement/ARPlacementController.cs`
- Separate from `ARPlacementService` — handles **tap-to-place** workflow
- Creates `LearningAreaAnchor` on user tap
- Auto-spawns default prefab after placement
- Clear/reset learning area

### 2.4 Layer 4: Activity Layer (MVP Pattern)

```
┌─────────────────────────────────────────────────────┐
│                    ActivityPresenter                 │
│  (Base class: Core/Learning/ActivityRunner/)        │
│                                                     │
│  State Machine:                                      │
│  ┌──────────────┐                                   │
│  │ Initializing │ ──▶ Initialize() ──▶ Ready        │
│  └──────────────┘                                   │
│         │                                           │
│         ▼                                           │
│  ┌──────────────┐                                   │
│  │    Ready     │ ──▶ StartActivity() ──▶ InProgress│
│  └──────────────┘                                   │
│         │                                           │
│         ▼                                           │
│  ┌──────────────────────────────────────────┐       │
│  │              InProgress                   │       │
│  │  SubmitAnswer() ──▶ CheckAnswer()       │       │
│  │       │                    │              │       │
│  │       ├─ Correct ──▶ Completed ──┬─ Next──│       │
│  │       └─ Incorrect ──▶ Retry/Failed    │       │
│  └──────────────────────────────────────────┘       │
│         │                                           │
│         ▼                                           │
│  ┌──────────────┐  ┌──────────────┐                 │
│  │  Completed   │  │   Failed     │                 │
│  └──────────────┘  └──────────────┘                 │
│         │                                           │
│         ▼                                           │
│  ┌──────────────┐                                   │
│  │  Cancelled  │ (Cancel() from any state)          │
│  └──────────────┘                                   │
│                                                     │
│  Key Responsibilities:                               │
│  - Rounds management (currentRound, NumberOfRounds)  │
│  - Hint request via unified HintSystem               │
│  - Result tracking & persistence via ProgressStorage  │
│  - Feedback triggers via FeedbackServiceProxy        │
│  - Delegates LoadRound/CheckAnswer to derived class  │
└─────────────────────────────────────────────────────┘
```

#### Activity Implementations

| Activity | Presenter | View | Bootstrap |
|----------|-----------|------|-----------|
| **QuantityMatch** | `QuantityMatchPresenter` | `QuantityMatchView` (1892 lines!) | `QuantityMatchActivityBootstrap` |
| **NumberLineJump** | `NumberLineJumpPresenter` | `NumberLineJumpView` | `NumberLineJumpActivityBootstrap` |
| **CompareQuantity** | `CompareQuantityPresenter` | `CompareQuantityView` (1025 lines) | `CompareQuantityActivityBootstrap` |

#### Activity Flow for QuantityMatch (Example)

```
1. GameplayActivityRouter.RouteSelectedActivity()
   └─ Detects activityId == "QuantityMatch"
      └─ Starts existing QuantityMatchActivityBootstrap

2. QuantityMatchActivityBootstrap.TryStartActivity()
   ├─ Waits for: bootstrap.Placement.IsPlacementAvailable
   ├─ Waits for: bootstrap.Placement.HasLearningArea
   └─ Calls: presenter.Initialize(config, view, placement, interaction)

3. QuantityMatchPresenter (extends ActivityPresenter)
   ├─ Initialize → sets config, sessionId, state = Ready
   ├─ StartActivity → StartNextRound
   │   └─ LoadRound(roundNumber) → spawns AR groups via ARGroupSpawnUtility
   │       └─ view.ShowQuestion(target, groupCount)
   │
   └─ User taps AR group (ARInteractionService.OnObjectTapped)
      └─ presenter.SubmitAnswer(answer)
          ├─ CheckAnswer (derived impl) → bool isCorrect
          ├─ if correct: HandleCorrectAnswer → view.ShowCorrectFeedback → PersistResult
          └─ if incorrect: HandleIncorrectAnswer → view.ShowIncorrectFeedback → retry or fail

4. On round complete:
   └─ ContinueToNextRound → LoadNextRound → repeat
      └─ On final round: CompleteActivity → view.ShowActivityComplete → ActivityFlowNavigator
```

### 2.5 Layer 5: Navigation & Flow

| Class | File | Role |
|-------|------|------|
| **ActivityFlowNavigator** | `_Project/Scripts/ActivityFlowNavigator.cs` | Static navigation between activities. `LoadActivity(id)`, `LoadNextActivity()`, `LoadProgressDashboard()` |
| **UIScreenManager** | `Core/UI/Navigation/UIScreenManager.cs` | Stack-based screen transitions with fade animation |
| **UIScreen** | `Core/UI/Navigation/UIScreen.cs` | Base class for screens. `CanvasGroup` fade transitions, `OnEnter`/`OnExit` events |
| **GameplayActivityRouter** | `_Project/Scripts/GameplayActivityRouter.cs` | Routes to correct activity based on `SelectedActivityData`. Creates runtime presenters if not in scene |
| **SelectedActivityData** | `_Project/Scripts/ActivityLoader.cs` | Static class holding `ActivityId`, `LessonId`, `ConfigPath` across scene loads |
| **LearningSceneServices** | `_Project/Scripts/LearningSceneServices.cs` | Ensures all services exist in gameplay scene, starts session |

### 2.6 Layer 6: Scenes

| Scene | Purpose | Key Objects |
|-------|---------|-------------|
| **SC_Boot** | App entry point | `BootLoader` |
| **SC_MainMenu** | Entry menu | `MainMenuView` |
| **SC_ActivitySelect** | Choose activity | `ActivitySelectController` |
| **SC_ARGameplay** | Main gameplay with AR | `ARServiceBootstrap`, `XR Origin`, activity objects |
| **SC_ProgressDashboard** | Learning progress | `ProgressDashboardView` |
| **SC_TestSandbox** | AR testing without activity | `ARServiceBootstrap`, `ARPlacementController` |

---

## 3. Startup Pipeline (Full Flow)

```
┌──────────────────────────────────────────────────────────────────┐
│                    1. APPLICATION LAUNCH                         │
└────────────────────────────┬───────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│                 2. SC_Boot LOADS                                │
│  BootLoader.Start()                                              │
│  ├─ RuntimePerformanceSettings.Apply()                           │
│  ├─ ProgressStorageProxy.Initialize()                             │
│  ├─ FeedbackServiceProxy.Initialize()                            │
│  ├─ SimpleAudioManager.EnsureExists()                            │
│  └─ Invoke(LoadMainMenu, 0.5f)                                   │
└────────────────────────────┬───────────────────────────────────┘
                             │
                             ▼ SceneManager.LoadScene("SC_MainMenu")
┌──────────────────────────────────────────────────────────────────┐
│                 3. SC_MainMenu LOADS                            │
│  UIScreenManager.Start()                                          │
│  ├─ Register all UIScreens                                       │
│  ├─ Hide non-initial screens                                     │
│  └─ Push initialScreen (MainMenu)                                │
│                                                                  │
│  User taps "Bắt đầu"                                             │
│  └─ UIScreenManager.PushScreen(ActivitySelectScreen)             │
└────────────────────────────┬───────────────────────────────────┘
                             │
                             ▼ SceneManager.LoadScene("SC_ActivitySelect")
┌──────────────────────────────────────────────────────────────────┐
│                 4. SC_ActivitySelect LOADS                       │
│  ActivitySelectController.Start()                                 │
│                                                                  │
│  User selects activity → ActivityFlowNavigator.LoadActivity(id)   │
│  └─ SelectedActivityData.ActivityId = id                         │
│  └─ SceneManager.LoadScene("SC_ARGameplay")                     │
└────────────────────────────┬───────────────────────────────────┘
                             │
                             ▼ SceneManager.LoadScene("SC_ARGameplay")
┌──────────────────────────────────────────────────────────────────┐
│                 5. SC_ARGameplay LOADS                          │
│                                                                  │
│  Execution Order (-200): ARServiceBootstrap.Awake()              │
│  ├─ ResolveServices()                                            │
│  │   ├─ Find ARSessionService via FindAnyObjectByType           │
│  │   ├─ Find ARInteractionService via FindAnyObjectByType       │
│  │   ├─ Decide mock vs real placement                           │
│  │   └─ Resolve to ARPlacementService or ARPlacementMock        │
│  └─ InitializeServices()                                        │
│      ├─ Session.Initialize()                                     │
│      │   ├─ EnsureSessionReference (create if missing)          │
│      │   ├─ EnsureCameraBackground (ARCameraBackground)          │
│      │   ├─ StartFallbackEvaluationIfNeeded()                    │
│      │   │   └─ After 2.5s: if no native camera → WebCam fallback│
│      │   └─ ARSession.stateChanged += OnARSessionStateChanged  │
│      ├─ Placement.Initialize()                                  │
│      │   ├─ ResolveReferences (ARRaycastManager, ARPlaneManager)│
│      │   ├─ planeManager.requestedDetectionMode = Horizontal    │
│      │   └─ Start UpdatePlacementPosition coroutine             │
│      └─ Interaction.Initialize()                                │
│          └─ EnhancedTouchSupport.Enable()                        │
│                                                                  │
│  Execution Order (-150): LearningSceneServices.Awake()           │
│  ├─ RuntimePerformanceSettings.Apply() (redundant)              │
│  ├─ Ensure ProgressStorageProxy exists                          │
│  ├─ Ensure FeedbackServiceProxy exists                           │
│  ├─ SimpleAudioManager.EnsureExists() (redundant)                │
│  ├─ Ensure GameplayActivityRouter exists                         │
│  └─ ProgressStorageProxy.Instance.StartSession()                 │
│                                                                  │
│  Execution Order (-140): GameplayActivityRouter.Start()          │
│  ├─ RouteSelectedActivity()                                      │
│  │   └─ Switch on SelectedActivityData.ActivityId               │
│  │       ├─ "QuantityMatch" → StartExistingQuantityMatch()      │
│  │       ├─ "NumberLineJump" → CreateNumberLineJumpActivity()  │
│  │       └─ "CompareQuantity" → CreateCompareQuantityActivity() │
│  └─ Clear SelectedActivityData                                   │
│                                                                  │
│  [For QuantityMatch] ActivityBootstrap.TryStartActivity()        │
│  ├─ Check bootstrap.Placement.IsPlacementAvailable              │
│  │   └─ If false: Invoke(TryStartActivity, 0.5f) → retry        │
│  ├─ Check bootstrap.Placement.HasLearningArea                   │
│  │   └─ If false: ARPlacementController handles tap-to-place    │
│  │       └─ OnLearningAreaPlaced event → fires                  │
│  └─ presenter.Initialize(config, view, placement, interaction)  │
│      └─ presenter.StartActivity()                              │
│          └─ view.ShowQuestion(target, groups)                   │
│              └─ ARGroupSpawnUtility.SpawnGroup()                │
│                  └─ ARPlacementService.SpawnCircle/SpawnGrid()  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 4. Điểm Dễ Gây Bug & Rủi Ro

### 4.1 AR-Related Risks

| # | Điểm Rủi Ro | File | Nguyên Nhân | Hậu Quả |
|---|-------------|------|-------------|----------|
| 1 | **Race condition giữa ARSession và Activity** | `QuantityMatchActivityBootstrap` | `TryStartActivity` poll 0.5s chờ `IsPlacementAvailable` | Activity không start nếu placement sẵn sàng trước khi bootstrap chạy |
| 2 | **Double Initialization** | `ARSessionService`, `ARServiceBootstrap` | Cả 2 đều call `Initialize()` trên service | Có thể reset state không mong muốn |
| 3 | **WebCamTexture Fallback Leak** | `ARSessionService` | Fallback quad/material/texture không được destroy đúng cách khi session recovered | Memory leak trên iOS |
| 4 | **Plane Detection khi đã place** | `ARPlacementService` | `UpdatePlacementPosition` chạy liên tục dù đã có learning area | Waste CPU |
| 5 | **LearningAreaAnchor null reference** | `ARPlacementService.EnsureLearningArea` | Tạo anchor nhưng plane có thể null | Anchor không track đúng |
| 6 | **FindAnyObjectByType hoisting** | Nhiều service | `FindAnyObjectByType` tìm object đã bị inactive | Mock service thay vì real service |
| 7 | **Camera background shader missing** | `ARSessionService` | Shader `Hidden/ARSpecialEducation/NativeCameraBackground` có thể không tồn tại | Fallback sang Unlit → camera trắng |
| 8 | **Scene dependencies với AR objects** | Build Settings | Nếu AR object không có trong scene → crash | N/A |

### 4.2 Activity-Related Risks

| # | Điểm Rủi Ro | File | Nguyên Nhân | Hậu Quả |
|---|-------------|------|-------------|----------|
| 1 | **QuantityMatchView quá lớn (1892 dòng)** | `QuantityMatchView.cs` | Runtime UI generation + button creation logic dồn vào 1 file | Khó debug, dễ break khi sửa |
| 2 | **Static HintSystem** | `HintSystem.cs` | Singleton pattern nhưng không có cleanup | Hint state leak giữa sessions |
| 3 | **Presenter khởi tạo đồng thời** | `GameplayActivityRouter` | Nhiều bootstrap cùng khởi tạo cùng lúc | Race condition với AR services |
| 4 | **Scene load giữa activity** | `ActivityFlowNavigator` | `SceneManager.LoadScene` destroy hết objects | AR session reset, state mất |
| 5 | **Config null không throw** | `ActivityPresenter.Initialize` | Check `activityConfig.IsValid()` nhưng vẫn tiếp tục nếu invalid | NullReferenceException khi truy cập config |
| 6 | **Auto-continue không cancel khi destroy** | `QuantityMatchView.AutoContinueToNextRound` | `Invoke` không được `CancelInvoke` khi view bị destroy | Scene chuyển không mong muốn |

### 4.3 Data/Storage Risks

| # | Điểm Rủi Ro | File | Nguyên Nhân | Hậu Quả |
|---|-------------|------|-------------|----------|
| 1 | **JSON DateTime serialization** | `LocalProgressStorage` | DateTime không serialize được với `JsonUtility` → dùng string "o" format | Parse error nếu locale khác |
| 2 | **Dictionary không serialize được** | `ProgressData` | Ban đầu dùng `Dictionary` → fix = serializable list | Performance degrade khi có nhiều results |
| 3 | **File write race condition** | `LocalProgressStorage` | Nhiều async writes cùng lúc | Data corruption |
| 4 | **LearnerId sanitization** | `LearnerProfileStore` | `SanitizeLearnerId` xử lý `Path.GetInvalidFileNameChars` | Special char trong id gây lỗi |

### 4.4 Performance Risks (iOS Specific)

| # | Điểm Rủi Ro | File/Layer | Nguyên Nhân |
|---|-------------|-----------|-------------|
| 1 | **LateUpdate cho fallback quad** | `ARSessionService.LateUpdate` | Fallback camera update ở LateUpdate → thêm frame budget |
| 2 | **MaterialPropertyBlock copy** | `ARInteractionService.ApplyHighlightColor` | Tạo block mới mỗi highlight → GC pressure |
| 3 | **Spawn nhiều objects** | `ARGroupSpawnUtility` | Với số lớn (8-10), spawn grid/circle tạo nhiều draw calls |
| 4 | **Runtime UI creation** | `QuantityMatchView.BuildRuntimeUi` | Tạo UI lúc runtime → GC spike khi vào activity |
| 5 | **Plane visualization iteration** | `ARPlacementService.HidePlaneVisualizationAfterPlacement` | Duyệt `planeManager.trackables` mỗi lần place |

---

## 5. Optimization Points

### 5.1 iOS Performance

| Optimization | Current | Suggested |
|-------------|---------|-----------|
| **Object count per group** | Không giới hạn | Clamp qua `RuntimePerformanceSettings` (đã có) |
| **Plane count** | Unlimited | Set `planeManager.requestedDetectionMode = None` sau placement |
| **Update frequency** | Every frame | Throttle AR updates khi activity đang chạy |
| **Shader complexity** | Fallback shader chain | Dùng shader đơn giản hơn cho iOS |
| **Mesh colliders** | Default sphere | Dùng simplified colliders |
| **UI Canvas mode** | ScreenSpaceOverlay | Giữ nguyên nhưng disable raycast trên background |

### 5.2 Architecture

| Issue | Current | Suggested |
|-------|---------|-----------|
| **QuantityMatchView size** | 1892 lines | Tách ra: `RuntimeButtonFactory`, `RuntimeLayoutBuilder` |
| **Static HintSystem** | Static class | Chuyển thành injected service với lifetime scope |
| **Scene coupling** | SceneManager.LoadScene for all navigation | Consider Addressables for faster load |
| **No DI container** | Manual `FindAnyObjectByType` | Dùng Zenject hoặc VContainer để quản lý dependencies |
| **Activity bootstrap polling** | `Invoke` retry loop | Dùng event-driven: subscribe to `OnLearningAreaPlaced` |

---

## 6. File Structure Summary

```
apps/unity-client/Assets/
├── Core/
│   ├── AR/
│   │   ├── ARServiceBootstrap.cs          [Layer 3] - Singleton, resolves AR services
│   │   ├── ARSession/
│   │   │   ├── ARSessionBootstrap.cs       [Layer 3] - ARSession lifecycle
│   │   │   └── ARSessionService.cs         [Layer 3] - AR Foundation wrapper + fallback
│   │   ├── Placement/
│   │   │   ├── ARPlacementService.cs        [Layer 3] - Object spawning, learning area
│   │   │   ├── ARPlacementController.cs     [Layer 3] - Tap-to-place workflow
│   │   │   └── ARPlacementServiceMock.cs   [Layer 3] - Editor simulation
│   │   ├── Interaction/
│   │   │   └── ARInteractionService.cs     [Layer 3] - Tap, select, highlight, drag
│   │   └── PlaneDetection/
│   │       └── ARPlaneDetectionController.cs [Layer 3] - Plane tracking & visualization
│   ├── Learning/
│   │   ├── ActivityRunner/
│   │   │   ├── ActivityPresenter.cs         [Layer 4] - Base MVP presenter
│   │   │   ├── ActivityView.cs              [Layer 4] - View/Presenter interfaces
│   │   │   ├── IARSessionService.cs         [Layer 3] - AR session interface
│   │   │   ├── IARPlacementService.cs       [Layer 3] - Placement interface
│   │   │   └── Models/ (ActivityState, ActivityResult, etc.)
│   │   └── Utils/
│   │       └── ARGroupSpawnUtility.cs      [Layer 4] - Shared object spawning
│   ├── Data/
│   │   └── LocalStorage/
│   │       └── LocalProgressStorage.cs     [Layer 2] - JSON persistence
│   ├── UI/
│   │   ├── Navigation/
│   │   │   ├── UIScreenManager.cs          [Layer 2] - Screen stack & transitions
│   │   │   └── UIScreen.cs                [Layer 2] - Base screen with fade
│   │   └── Components/ (UIFeedbackOverlay, UIHintBubble, etc.)
│   └── Support/
│       ├── AudioManager/ (SimpleAudioManager)
│       ├── FeedbackSystem/ (FeedbackServiceProxy, FeedbackType)
│       ├── HintSystem/ (HintSystem)
│       └── Performance/ (RuntimePerformanceSettings)
│
├── Features/Activities/
│   ├── QuantityMatch/
│   │   ├── Scripts/QuantityMatchPresenter.cs
│   │   ├── Scripts/QuantityMatchView.cs         ⚠️ 1892 lines
│   │   ├── Scripts/QuantityMatchConfig.cs
│   │   ├── Scripts/QuantityMatchActivityBootstrap.cs
│   │   └── Scripts/QuantityMatchRuntimeUI.cs
│   ├── NumberLineJump/
│   │   └── (similar structure)
│   ├── CompareQuantity/
│   │   └── (similar structure)
│   └── ActivityPrefabSetup.cs
│
└── _Project/
    ├── Scripts/
    │   ├── BootLoader.cs                   [Layer 6] - App entry
    │   ├── LearningSceneServices.cs        [Layer 5] - Service injection
    │   ├── GameplayActivityRouter.cs       [Layer 5] - Activity routing
    │   ├── ActivityFlowNavigator.cs        [Layer 5] - Scene navigation
    │   ├── ActivityLoader.cs               [Layer 5] - SelectedActivityData
    │   ├── ActivitySelectController.cs
    │   └── ProgressDashboardView.cs
    └── Scenes/
        ├── SC_Boot.unity
        ├── SC_MainMenu.unity
        ├── SC_ActivitySelect.unity
        ├── SC_ARGameplay.unity             [Main AR scene]
        ├── SC_ProgressDashboard.unity
        └── SC_TestSandbox.unity            [AR testing]
```

---

## 7. Key Interfaces Summary

| Interface | Purpose | Key Methods |
|-----------|---------|-------------|
| `IARSessionService` | AR state management | `Initialize()`, `StartSession()`, `IsSessionReady`, `TrackingQuality` |
| `IARPlacementService` | AR object placement | `SpawnAtPosition()`, `SpawnGrid()`, `SpawnCircle()`, `IsPlacementAvailable` |
| `IARInteractionService` | User interaction | `RegisterInteractable()`, `SetHighlight()`, `OnObjectTapped` |
| `IActivityPresenter` | Activity business logic | `SubmitAnswer()`, `RequestHint()`, `ContinueToNextRound()` |
| `IActivityView` | Activity UI | `ShowQuestion()`, `ShowCorrectFeedback()`, `ShowHint()` |

---

*Cập nhật: 29/05/2026*
