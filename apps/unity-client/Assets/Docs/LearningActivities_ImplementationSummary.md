# Learning Activities / AR Mini-games - Implementation Summary

**Document Version:** 1.0
**Date:** 2026-05-08
**Module:** Learning Layer (Activities)
**Status:** Implementation Complete

---

## 1. OVERVIEW

### What This Module Does

This module implements the **learning layer** of the AR math education application. It turns the AR environment into structured learning activities with:

- **Three Complete Activities:** Quantity Match, Compare Quantity, Number Line Jump
- **Activity Framework:** Base classes for Presenter/View/Config/Models
- **Support Systems:** Hint escalation, feedback (sound/VFX), local progress tracking
- **Data Models:** Answers, Results, Questions, Hints with full serialization support

### What This Module Does NOT Do

| Responsibility | NOT Our Job | Handled By |
|----------------|-------------|------------|
| AR session management | ❌ | AR Core |
| Plane detection | ❌ | AR Core |
| Object placement/spawning | ❌ | AR Core (via IARPlacementService) |
| Low-level interaction (tap/drag) | ❌ | AR Core (via IARInteractionService) |
| Audio playback | ❌ | Audio Team |
| VFX/particle effects | ❌ | VFX Team |

**Key Principle:** The learning layer assumes AR services exist and provides **interfaces** (`IARPlacementService`, `IARInteractionService`, `IARSessionService`) for the AR team to implement.

---

## 2. ARCHITECTURE DIAGRAM

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              SCENE (SC_ARGameplay)                          │
│                                                                              │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                        ACTIVITY PRESENTER                             │  │
│  │  (QuantityMatchPresenter / CompareQuantityPresenter /               │  │
│  │   NumberLineJumpPresenter)                                          │  │
│  │                                                                      │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │  │
│  │  │   Activity   │  │   Activity   │  │   Activity   │              │  │
│  │  │   Config     │  │   Result     │  │    State     │              │  │
│  │  │(Scriptable)  │  │   (Model)    │  │   (Enum)     │              │  │
│  │  └──────────────┘  └──────────────┘  └──────────────┘              │  │
│  │                                                                      │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                    │                                       │
│                                    │ calls                                 │
│                                    ▼                                       │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                         ACTIVITY VIEW                                 │  │
│  │  (QuantityMatchView / CompareQuantityView / NumberLineJumpView)       │  │
│  │                                                                      │  │
│  │  - Displays UI (buttons, text, feedback panels)                       │  │
│  │  - Receives user input                                                │  │
│  │  - Shows hints and feedback                                           │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ uses
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                            SUPPORT SERVICES                                  │
│                                                                              │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────────┐      │
│  │   HintSystem     │  │  FeedbackSystem  │  │  LocalProgressStorage │      │
│  │                  │  │                  │  │                      │      │
│  │ - Escalation     │  │ - Sound triggers │  │ - JSON persistence   │      │
│  │ - Usage tracking │  │ - VFX triggers   │  │ - Session tracking   │      │
│  │ - Cooldowns      │  │ - Color coding   │  │ - Statistics         │      │
│  └──────────────────┘  └──────────────────┘  └──────────────────────┘      │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ depends on (interfaces)
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           AR CORE INTERFACES                                 │
│                   (To be implemented by AR Team)                            │
│                                                                              │
│  ┌─────────────────────────────┐  ┌─────────────────────────────────────┐  │
│  │    IARPlacementService       │  │     IARInteractionService           │  │
│  │                              │  │                                     │  │
│  │ + SpawnAtPosition()          │  │ + RegisterInteractable()           │  │
│  │ + SpawnGrid()                │  │ + OnObjectTapped event             │  │
│  │ + SpawnCircle()              │  │ + SetHighlight()                  │  │
│  │ + ClearSpawnedObjects()      │  │ + SetInteractionEnabled()        │  │
│  └─────────────────────────────┘  └─────────────────────────────────────┘  │
│                                                                              │
│  ┌─────────────────────────────┐                                            │
│  │    IARSessionService        │                                            │
│  │                             │                                            │
│  │ + IsSessionReady            │                                            │
│  │ + IsTrackingStable          │                                            │
│  │ + TrackingQuality           │                                            │
│  └─────────────────────────────┘                                            │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. FULL FILE TREE

```
Assets/Core/Learning/
├── Models/
│   ├── ActivityState.cs                    # Enum: Initializing, Ready, InProgress, etc.
│   ├── ActivityHint.cs                     # Hint data structure
│   ├── ActivityAnswer.cs                   # Base answer model
│   └── ActivityResult.cs                   # Result model with SessionId, DifficultyLevel
│
├── ActivityRunner/
│   ├── IActivityRunner.cs                  # Activity lifecycle interface
│   ├── ActivityView.cs                     # IActivityView + IActivityPresenter interfaces
│   ├── ActivityConfig.cs                   # Base ScriptableObject config
│   ├── ActivityPresenter.cs                # Base presenter with state machine
│   ├── IARPlacementService.cs              # Interface: spawn/place objects (AR Team TODO)
│   ├── IARInteractionService.cs            # Interface: tap/select input (AR Team TODO)
│   └── IARSessionService.cs                # Interface: session state (AR Team TODO)
│
└── Utils/
    └── ARGroupSpawnUtility.cs              # Shared group spawning logic

Assets/Core/Support/
├── HintSystem/
│   ├── HintSystem.cs                       # Hint escalation, tracking, cooldown
│   └── HintServiceProxy.cs                 # MonoBehaviour singleton proxy
│
└── FeedbackSystem/
    ├── FeedbackType.cs                     # Enums: FeedbackType, FeedbackIntensity
    ├── FeedbackData.cs                     # FeedbackData + FeedbackConfig
    ├── FeedbackSystem.cs                   # Sound/VFX triggers
    └── FeedbackServiceProxy.cs             # MonoBehaviour singleton proxy

Assets/Core/Data/LocalStorage/
├── LocalProgressStorage.cs                 # JSON-based progress storage
└── ProgressStorageProxy.cs                 # MonoBehaviour singleton proxy

Assets/Features/Activities/QuantityMatch/Scripts/
├── QuantityMatchQuestion.cs                # Question data (target, groups, counts)
├── QuantityMatchAnswer.cs                  # Answer data (selected group, count)
├── QuantityMatchConfig.cs                  # Config with questions, feedback, hints
├── QuantityMatchPresenter.cs               # Presenter with answer checking
├── IQuantityMatchView.cs                   # View interface
└── QuantityMatchView.cs                    # MonoBehaviour view

Assets/Features/Activities/CompareQuantity/Scripts/
├── JumpDirection.cs                        # Enums (moved to shared location)
├── CompareQuantityQuestion.cs              # Question data (left/right counts)
├── CompareQuantityAnswer.cs                # Answer data (comparison choice)
├── CompareQuantityConfig.cs                # Config with button labels, outcome feedback
├── CompareQuantityPresenter.cs             # Presenter using ARGroupSpawnUtility
├── ICompareQuantityView.cs                 # View interface
└── CompareQuantityView.cs                  # MonoBehaviour view

Assets/Features/Activities/NumberLineJump/Scripts/
├── JumpDirection.cs                        # Direction enums for jumps
├── NumberLineJumpQuestion.cs               # Question (range, start, target, direction)
├── NumberLineJumpAnswer.cs                 # Answer with JumpRecord list
├── NumberLineJumpConfig.cs                 # Config with overshoot/boundary feedback
├── NumberLineJumpPresenter.cs              # Presenter with movement, equation tracking
├── INumberLineJumpView.cs                  # View interface
└── NumberLineJumpView.cs                   # MonoBehaviour view
```

---

## 4. HOW TO ADD A NEW ACTIVITY

### Step 1: Create Folder Structure

```
Assets/Features/Activities/YourActivity/
├── Scripts/
├── Prefabs/
├── UI/
├── Art/ (optional)
├── Audio/ (optional)
└── Tests/ (optional)
```

### Step 2: Define Your Models

Create your specific question and answer models in `Scripts/`:

```csharp
// YourActivityQuestion.cs
[Serializable]
public class YourActivityQuestion
{
    // Define question-specific fields
    public int SomeValue;
    public string SomeSetting;

    public bool IsValid() { /* validation logic */ }
}

// YourActivityAnswer.cs (inherits from ActivityAnswer)
[Serializable]
public class YourActivityAnswer : ActivityAnswer
{
    public int UserChoice;
    public float SomeMetric;

    public bool IsCorrect() { /* correctness logic */ }
}
```

### Step 3: Create Config

Create a ScriptableObject config that extends `ActivityConfig`:

```csharp
[CreateAssetMenu(fileName = "SO_YourActivityConfig", menuName = "AR Learning/Your Activity")]
public class YourActivityConfig : ActivityConfig
{
    [Header("Your Activity Settings")]
    [SerializeField] private List<YourActivityQuestion> questions;

    [Header("Feedback Strings")]
    [SerializeField] private string correctFeedback = "Great!";
    [SerializeField] private string incorrectFeedback = "Try again!";

    [Header("Hints")]
    [SerializeField] private List<ActivityHint> defaultHints;

    // Properties
    public List<YourActivityQuestion> Questions => questions;
    public string CorrectFeedback => correctFeedback;

    public override bool IsValid()
    {
        return base.IsValid() && questions != null && questions.Count > 0;
    }

    public override List<ActivityHint> GetHintsForLevel(int levelNumber)
    {
        var question = GetQuestion(levelNumber - 1);
        return question?.CustomHints ?? defaultHints;
    }

    public YourActivityQuestion GetQuestion(int index)
    {
        return index >= 0 && index < questions.Count ? questions[index] : null;
    }
}
```

### Step 4: Create Presenter

Extend `ActivityPresenter` and implement abstract methods:

```csharp
public class YourActivityPresenter : ActivityPresenter
{
    private YourActivityConfig activityConfig;
    private IYourActivityView view;

    // Implement abstract methods
    protected override void LoadRound(int roundNumber)
    {
        // Load question data
        // Spawn AR objects via IARPlacementService
        // Update view
    }

    protected override bool CheckAnswer(ActivityAnswer answer)
    {
        // Cast to YourActivityAnswer
        // Return true/false
    }

    protected override ErrorType? GetErrorType(ActivityAnswer answer)
    {
        // Return specific error type for analytics
    }

    // Your activity-specific methods
    public void Initialize(YourActivityConfig config, IYourActivityView activityView,
        IARPlacementService placement, IARInteractionService interaction)
    {
        activityConfig = config;
        view = activityView;

        base.Initialize(config);
        view.Initialize(this);

        // Subscribe to view events
        view.OnAnswerSelected += HandleAnswerSelected;
        // ... other events
    }

    private void HandleAnswerSelected(YourActivityAnswer answer)
    {
        SubmitAnswer(answer);
    }

    protected override void HandleCorrectAnswer(ActivityAnswer answer)
    {
        view?.ShowCorrectFeedback(activityConfig.CorrectFeedback);
        base.HandleCorrectAnswer(answer);
    }
}
```

### Step 5: Create View Interface

```csharp
public interface IYourActivityView : IActivityView
{
    event Action<YourActivityAnswer> OnAnswerSelected;

    void ShowQuestion(int someValue);
    void ShowCorrectFeedback(string message);
    void ShowIncorrectFeedback(string message);
    void UpdateProgress(int current, int total);
}
```

### Step 6: Create View Implementation

```csharp
public class YourActivityView : MonoBehaviour, IYourActivityView
{
    // UI References
    [SerializeField] private Button answerButton;
    [SerializeField] private Text feedbackText;

    // Events
    public event Action<ActivityAnswer> OnAnswerSelected;
    public event Action OnHintRequested;
    public event Action OnCancelRequested;
    public event Action<YourActivityAnswer> OnYourAnswerSelected;

    // Implement IActivityView
    public void Initialize(IActivityPresenter presenter) { }
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
    public void ShowCorrectFeedback() => ShowCorrectFeedback("Great!");
    public void ShowIncorrectFeedback() => ShowIncorrectFeedback("Try again!");
    public void ShowHint(ActivityHint hint) { /* display hint */ }
    public void UpdateProgress(int current, int total) { /* update UI */ }
    public void SetInputEnabled(bool enabled) { /* enable/disable */ }

    // Your specific methods
    public void ShowQuestion(int someValue) { /* display question */ }

    private void OnButtonClick()
    {
        OnYourAnswerSelected?.Invoke(new YourActivityAnswer { UserChoice = someValue });
    }
}
```

### Step 7: Register with Scene

Add your activity to `SC_ARGameplay.unity`:
1. Create GameObject with your View component
2. Create GameObject with your Presenter component
3. Create/assign your Config ScriptableObject
4. Wire up references in Inspector

### Required Config Fields

Every activity config **must** include:
- List of questions/rounds
- Difficulty level (inherited from ActivityConfig)
- Max hints allowed per question
- Feedback strings (correct, incorrect, failed)
- Default hints (3-level progression)

---

## 5. HANDOFF TODOS

### AR Team — Interfaces to Implement

#### IARPlacementService

| Method | Expected Behavior |
|--------|-------------------|
| `SpawnAtPosition(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent)` | Spawn prefab at exact world position. Return spawned GameObject. |
| `SpawnGrid(GameObject prefab, Vector3 center, int count, float spacing)` | Spawn `count` objects in a grid pattern centered at `center`. Return array of spawned objects. |
| `SpawnCircle(GameObject prefab, Vector3 center, int count, float radius)` | Spawn `count` objects in a circle around `center`. Return array of spawned objects. |
| `ClearSpawnedObjects()` | Destroy all objects spawned through this service. |
| `IsPlacementAvailable` (property) | Return true if a valid AR plane is detected. |
| `CurrentPlacementPosition` (property) | Return the center position of the detected plane. |
| `OnPlacementPositionAvailable` (event) | Fire when a plane becomes available. |
| `OnPlacementPositionLost` (event) | Fire when tracking is lost. |

#### IARInteractionService

| Method | Expected Behavior |
|--------|-------------------|
| `RegisterInteractable(GameObject obj, object data)` | Mark object as tap-able. Store `data` for later retrieval. |
| `UnregisterInteractable(GameObject obj)` | Remove object from interaction system. |
| `GetInteractableData(GameObject obj)` | Return the data stored when registering. |
| `OnObjectTapped` (event) | Fire with tapped GameObject when user taps it. |
| `SetHighlight(GameObject obj, bool highlight)` | Show/hide visual highlight on object. |
| `SetInteractionEnabled(bool enabled)` | Enable/disable all registered interactables. |
| `ClearInteractables()` | Unregister all interactables. |

#### IARSessionService

| Member | Expected Behavior |
|--------|-------------------|
| `IsSessionReady` (property) | Return true if AR session is running and tracking. |
| `IsTrackingStable` (property) | Return true if tracking quality is good enough for activities. |
| `TrackingQuality` (property) | Return current tracking quality (None/Poor/Fair/Good/Excellent). |
| `OnSessionReady` (event) | Fire when AR session becomes ready. |
| `OnSessionLost` (event) | Fire when AR session is lost. |

### Audio Team — Sound Triggers

**Trigger Location:** `FeedbackServiceProxy.HandleSoundRequested(string soundName)`

**Sound Names Used:**
| Sound Name | When Played | Intensity |
|------------|-------------|-----------|
| `SFX_Correct` | Correct answer | Medium |
| `SFX_Incorrect` | Incorrect answer | Medium |
| `SFX_Hint` | Hint shown | Low |
| `SFX_Success` | Activity completed | High |
| `SFX_Failed` | Activity failed (max attempts) | High |
| `SFX_Click` | (optional) Button tap | Low |

**Implementation:** In `FeedbackServiceProxy.HandleSoundRequested()`, play the audio clip matching the sound name.

### VFX Team — Effect Triggers

**Trigger Location:** `FeedbackServiceProxy.HandleVisualEffectRequested(string effectName)`

**Effect Names Used:**
| Effect Name | When Played | Position |
|-------------|-------------|----------|
| `VFX_CorrectConfetti` | Correct answer | Screen center or target position |
| `VFX_IncorrectShake` | Incorrect answer | Screen center |
| `VFX_SuccessFireworks` | Activity completed | Screen center |
| `VFX_BoundaryBump` | Number line boundary hit | Character position |

**Implementation:** In `FeedbackServiceProxy.HandleVisualEffectRequested()`, instantiate/play the particle system matching the effect name.

### Prefabs Needed

| Prefab Name | Used By | Description | TODO |
|-------------|---------|-------------|------|
| **Quantity Objects** | | | |
| `PFB_Apple` | QuantityMatch, CompareQuantity | Red apple 3D model | Create simple sphere/capsule with red material |
| `PFB_Carrot` | QuantityMatch, CompareQuantity | Orange carrot 3D model | Create cylinder with orange material |
| `PFB_Star` | QuantityMatch, CompareQuantity | Star shape for correct feedback | Create star mesh or particle |
| **Number Line** | | | |
| `PFB_NumberTile` | NumberLineJump | Tile with number display | Create cube with text mesh child showing number |
| `PFB_JumpCharacter` | NumberLineJump | Character that jumps on number line | Create simple sphere or capsule with face |
| **UI Prefabs** | | | |
| `PFB_QuantityMatchPanel` | QuantityMatch | Main UI panel for quantity matching | Create with buttons, text, feedback panels |
| `PFB_CompareQuantityPanel` | CompareQuantity | Main UI panel for comparison | Create with More/Fewer/Equal buttons |
| `PFB_NumberLineJumpPanel` | NumberLineJump | Main UI panel for number line | Create with arrow buttons, equation display |

---

## 6. KNOWN LIMITATIONS & ASSUMPTIONS

### Assumptions About AR Core

1. **Plane Detection:** We assume `IARPlacementService.CurrentPlacementPosition` returns a stable center point on a detected plane.
2. **Object Persistence:** We assume spawned objects persist until explicitly cleared or scene changes.
3. **Tap Detection:** We assume `IARInteractionService.OnObjectTapped` fires reliably for AR objects.
4. **Session State:** We assume `IARSessionService.IsSessionReady` accurately reflects AR tracking state.

### What Breaks If AR Interfaces Are Not Implemented

| Component | Dependency | Breaks If Missing |
|-----------|------------|-------------------|
| Quantity Match | `SpawnCircle()` | No object groups appear |
| Compare Quantity | `SpawnCircle()` | No groups appear |
| Number Line Jump | `SpawnAtPosition()` | No tiles or character appear |
| All Activities | `RegisterInteractable()` | Tap/touch input doesn't work |
| All Activities | AR session | Can't start activities (no placement position) |

**Fallback Behavior:** The spawn utilities create placeholder primitives (cubes/spheres) if prefabs are null, but full functionality requires AR service implementation.

### Shortcuts Taken

1. **Prefab Loading:** `GetObjectPrefab()` methods return null (TODO). Activities currently create placeholder primitives.
2. **Sound/VFX:** Events are fired but no actual audio/visual playback occurs (TODO for Audio/VFX teams).
3. **Tile Numbers:** NumberLineJump tiles don't show numbers (TODO: TextMesh on tile prefabs).
4. **Animation:** Character movement is direct position assignment, not animated jump.
5. **Persistence:** Progress saves to local JSON only (no backend sync yet).

### Thread Safety

- All systems run on Unity main thread.
- No multi-threading assumptions.
- File I/O operations are synchronous (acceptable for local JSON size).

---

## 7. DEFINITION OF DONE — CHECKLIST

Per-Activity Requirements (from ROLE_DOC):

| Requirement | Status | Notes |
|-------------|--------|-------|
| Runs on the shared scene | ✅ PASS | All activities use SC_ARGameplay via config-based loading |
| Has a clear flow | ✅ PASS | Initialize → Ready → InProgress → Completed/Failed |
| Can receive user input | ✅ PASS | View interfaces + IARInteractionService integration |
| Has answer checking | ✅ PASS | Each activity implements CheckAnswer() override |
| Has correct/incorrect feedback | ✅ PASS | Config-driven feedback strings + FeedbackSystem |
| Saves result locally | ✅ PASS | LocalProgressStorage via ProgressStorageProxy |
| Hints exist for wrong answers | ✅ PASS | 3-level hint progression per activity |
| Immediate positive feedback | ✅ PASS | FeedbackService with sound/VFX triggers |
| Basic results displayable | ✅ PASS | ActivityResult model with all required fields |

Module-Level Requirements:

| Requirement | Status | Notes |
|-------------|--------|-------|
| New activities can be added | ✅ PASS | Framework designed for extensibility |
| No hard dependency on AR core internals | ✅ PASS | Uses interfaces only (IARPlacementService, etc.) |
| Does not break the shared scene | ✅ PASS | Each activity cleans up spawned objects |
| Learning logic not mixed with display logic | ✅ PASS | Presenter (logic) / View (UI) separation |
| Code is easy to read and review | ✅ PASS | Clear naming, one responsibility per file |
| Hint system works | ✅ PASS | HintSystem with escalation, tracking, cooldown |
| Feedback system works | ✅ PASS | FeedbackSystem with sound/VFX hooks |
| Local progress is saved correctly | ✅ PASS | JSON persistence with session tracking |
| Content not hardcoded | ✅ PASS | All content in ScriptableObject configs |
| Folder structure follows conventions | ✅ PASS | PascalCase folders, SC_/PFB_/SO_ prefixes |

Items Marked as TODO:

| TODO | Owner | Description |
|------|-------|-------------|
| IARPlacementService implementation | AR Team | Spawn/place objects in AR |
| IARInteractionService implementation | AR Team | Handle tap/select/drag input |
| IARSessionService implementation | AR Team | Session state tracking |
| Audio playback | Audio Team | Implement HandleSoundRequested() |
| VFX playback | VFX Team | Implement HandleVisualEffectRequested() |
| Prefab creation | Art Team | Create activity-specific 3D prefabs |
| Character jump animation | AR Team | Animate NumberLineJump character movement |
| Number tile display | Art Team | Add text to NumberTile prefabs |

---

## END OF DOCUMENT

For questions or clarifications, refer to:
- ROLE_DOC.txt for original requirements
- Individual activity files for implementation details
- Base classes (ActivityPresenter, ActivityConfig) for extension points
