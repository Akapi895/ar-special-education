# AR Unity/iOS Development Rules & Best Practices

> **Dự án**: AR Special Education  
> **Mục đích**: Kiểm soát code, tối ưu hiệu suất, giữ kiến trúc rõ ràng, dễ test, dễ sửa đổi, hạn chế lỗi phát sinh  
> **Áp dụng cho**: Tất cả script mới trong `apps/unity-client/Assets/`

---

## 1. Kiến Trúc & Layer Rules

### 1.1 Layer Separation (STRICT)

```
Layer 6: Scenes (BootLoader, scene-level orchestration)
    ↓
Layer 5: Navigation & Flow (ActivityFlowNavigator, UIScreenManager, Router)
    ↓
Layer 4: Activity (Presenter, View, Bootstrap) — MVP pattern
    ↓
Layer 3: AR Core Services (Session, Placement, Interaction)
    ↓
Layer 2: Support Services (Data, Audio, Feedback, UI)
    ↓
Layer 1: Unity Foundation (ARFoundation, XR Toolkit, uGUI)
```

**RULE 1.1** — Mỗi layer chỉ được phép phụ thuộc vào layer bên dưới. Layer 4 (Activity) không được import Layer 5 (Navigation). Activity dùng `ActivityFlowNavigator` (Layer 5) thông qua **static method calls**, không import type.

**RULE 1.2** — Mọi AR service phải implement interface trong `Core/Learning/ActivityRunner/`:
- `IARSessionService` cho session management
- `IARPlacementService` cho object placement
- `IARInteractionService` cho user interaction

**RULE 1.3** — Activity layer dùng **MVP pattern**:
- **Model**: `ActivityConfig`, `ActivityResult`, `ActivityState`
- **View**: Chỉ xử lý UI rendering và user input forwarding. KHÔNG có logic nghiệp vụ
- **Presenter**: Tất cả business logic. KHÔNG touch UI elements trực tiếp

**RULE 1.4** — KHÔNG viết script mới trong `Core/AR/` nếu không cần AR Foundation. UI-only logic đi vào `Core/UI/` hoặc `Features/`.

### 1.2 Dependency Injection

**RULE 1.5** — AR Services được resolve qua `ARServiceBootstrap` (Singleton pattern). KHÔNG dùng `new` để tạo AR service.

**RULE 1.6** — Trong Activity Presenter, nhận dependencies qua constructor parameters hoặc `Initialize()` method. KHÔNG dùng `FindAnyObjectByType` trong Presenter.

```csharp
// ✅ ĐÚNG
public void Initialize(ActivityConfig config, IQuantityMatchView view, 
    IARPlacementService placement, IARInteractionService interaction)
{
    this.placement = placement;
    this.interaction = interaction;
}

// ❌ SAI
void Start() {
    var placement = FindAnyObjectByType<ARPlacementService>();
}
```

### 1.3 Service Lifetime

**RULE 1.7** — `ARServiceBootstrap` phải có `DefaultExecutionOrder(-200)` để chạy TRƯỚC mọi service khác trong AR scene.

**RULE 1.8** — `LearningSceneServices` phải có `DefaultExecutionOrder(-150)` để đảm bảo services tồn tại TRƯỚC khi activity bootstrap chạy.

**RULE 1.9** — `GameplayActivityRouter` phải có `DefaultExecutionOrder(-140)` để route activity SAU khi AR services đã sẵn sàng.

**RULE 1.10** — Tất cả Singleton pattern phải kiểm tra `instance != null && instance != this` trong `Awake()` và destroy duplicate.

---

## 2. AR Session & Camera Rules

### 2.1 AR Session Management

**RULE 2.1** — AR Session phải được initialize TRƯỚC khi bất kỳ activity nào start. Dùng event-driven thay vì polling:

```csharp
// ✅ ĐÚNG - Event-driven
sessionService.OnSessionReady += OnARReady;
void OnARReady() { presenter.StartActivity(); }

// ❌ SAI - Polling
void Update() {
    if (sessionService.IsSessionReady && !started) {
        presenter.StartActivity();
    }
}
```

**RULE 2.2** — KHÔNG gọi `ARSession.Reset()` trong khi activity đang chạy. Nếu cần reset, cancel activity trước.

**RULE 2.3** — Camera background shader phải được bundle trong Resources hoặc build vào player. Luôn có fallback shader:

```csharp
Shader shader = Shader.Find("Hidden/ARSpecialEducation/NativeCameraBackground")
    ?? Shader.Find("Unlit/Texture")  // Fallback 1
    ?? Shader.Find("Universal Render Pipeline/Unlit");  // Fallback 2
```

### 2.2 Plane Detection

**RULE 2.4** — Sau khi learning area được placed, TẮT plane detection để tiết kiệm CPU:

```csharp
if (planeManager != null) {
    planeManager.enabled = false;
}
```

**RULE 2.5** — Plane visualization phải có toggle và mặc định là `true` trong editor, `false` trên production iOS (configurable).

**RULE 2.6** — Chỉ detect horizontal planes (phù hợp với use case học tập trẻ em):

```csharp
planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
```

### 2.3 Learning Area

**RULE 2.7** — `LearningAreaAnchor` phải track với AR reference point để không bị drift khi device di chuyển.

**RULE 2.8** — Khi clear learning area, phải destroy tất cả spawned objects TRƯỚC:

```csharp
public void ClearLearningArea(bool clearSpawnedObjects = true) {
    if (clearSpawnedObjects) ClearSpawnedObjects();
    // then destroy anchor
}
```

---

## 3. Activity Rules

### 3.1 Presenter Rules

**RULE 3.1** — Presenter phải implement state machine rõ ràng. Valid transitions:

```
Initializing → Ready → InProgress → Paused
                    ↘            ↘
                     Completed ← Failed
                    ↗
Cancelled (from any state)
```

**RULE 3.2** — Presenter KHÔNG được truy cập trực tiếp vào GameObject/Transform. Tất cả object manipulation qua `IARPlacementService`.

**RULE 3.3** — `ActivityPresenter` là base class — KHÔNG thêm activity-specific logic vào đây. Thêm vào derived class.

**RULE 3.4** — Result persistence phải xảy ra NGAY SAU khi round complete, trong `HandleCorrectAnswer()` và `HandleIncorrectAnswer()`:

```csharp
protected virtual void HandleCorrectAnswer(ActivityAnswer answer) {
    currentResult.Complete(true, null);
    PersistRoundResult();  // ✅ Must persist immediately
    ChangeState(ActivityState.Completed);
}
```

### 3.2 View Rules

**RULE 3.5** — View chỉ xử lý 3 thứ:
1. Render UI elements theo state
2. Forward user input events lên Presenter
3. Animation/feedback playback

**RULE 3.6** — View KHÔNG bao giờ call `SceneManager.LoadScene` trực tiếp. Luôn qua `ActivityFlowNavigator`.

**RULE 3.7** — Nếu View cần tạo UI runtime, extract factory logic ra class riêng. KHÔNG viết > 500 lines trong một View file.

**RULE 3.8** — Tất cả button listeners phải được remove trong `OnDestroy()` hoặc `OnDisable()`:

```csharp
void OnDestroy() {
    if (myButton != null) myButton.onClick.RemoveAllListeners();
    CancelInvoke();  // Cancel any pending invokes
}
```

### 3.3 Config & Questions

**RULE 3.9** — Activity config phải là `ScriptableObject`. KHÔNG hardcode questions trong code (ngoại trừ runtime fallback).

**RULE 3.10** — Config validation phải có trong `Initialize()`:

```csharp
public virtual void Initialize(ActivityConfig activityConfig) {
    if (activityConfig == null || !activityConfig.IsValid()) {
        Debug.LogError($"[Presenter] Invalid config.");
        return;  // Do NOT continue with null config
    }
    config = activityConfig;
}
```

### 3.4 Hint System

**RULE 3.11** — Dùng `HintSystem` (shared/static) cho tất cả activities. KHÔNG tạo hint system riêng.

**RULE 3.12** — Hint phải auto-hide sau 5-6 giây. Luôn cancel previous hint timer khi show hint mới:

```csharp
CancelInvoke(nameof(HideHint));
Invoke(nameof(HideHint), 5f);
```

---

## 4. Data & Persistence Rules

### 4.1 JSON Serialization

**RULE 4.1** — `DateTime` phải serialize thành string ISO 8601 format (`"o"`), KHÔNG dùng `JsonUtility.ToJson` trực tiếp trên DateTime.

**RULE 4.2** — KHÔNG dùng `Dictionary` trong serializable classes. Dùng `List<T>` với key field:

```csharp
// ✅ ĐÚNG
[Serializable]
public class ProgressData {
    [SerializeField] private List<ActivityStatisticsEntry> activityStatistics;
}

// ❌ SAI
[Serializable]
public class ProgressData {
    public Dictionary<string, ActivityStatistics> activityStatistics;
}
```

**RULE 4.3** — Sau khi deserialize, phải gọi `DeserializeAfterLoad()` để convert strings về DateTime:

```csharp
ProgressData data = JsonUtility.FromJson<ProgressData>(json);
data.DeserializeAfterLoad();  // ✅ Must call
```

### 4.2 File I/O

**RULE 4.4** — File write phải trong try-catch và luôn có fallback:

```csharp
try {
    File.WriteAllText(path, json);
} catch (Exception e) {
    Debug.LogError($"[Storage] Save failed: {e.Message}");
    // Fallback: keep in-memory, retry later
}
```

**RULE 4.5** — Learner ID phải được sanitize trước khi dùng làm filename:

```csharp
string safeId = new string(learnerId.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());
```

### 4.3 Progress Tracking

**RULE 4.6** — Mỗi session phải có `SessionData` với `StartTime` và `EndTime`. Gọi `EndSession()` trong `OnDestroy()` của scene:

```csharp
void OnDestroy() {
    if (ProgressStorageProxy.Instance != null) {
        ProgressStorageProxy.Instance.EndSession();
    }
}
```

**RULE 4.7** — Technical issues (AR failure, crash) phải được tracked với `SaveTechnicalIssue()` để phân biệt với learning failure.

---

## 5. iOS Performance Rules

### 5.1 Object Spawning

**RULE 5.1** — LUÔN clamp số objects trong group theo device tier:

```csharp
int clampedCount = RuntimePerformanceSettings.ClampGroupObjectCount(requestedCount);
if (clampedCount < requestedCount) {
    Debug.LogWarning($"Clamped from {requestedCount} to {clampedCount}");
}
```

**RULE 5.2** — Object prefabs phải có simplified colliders (sphere, không dùng mesh collider):

```csharp
// Trong prefab setup
SphereCollider col = gameObject.AddComponent<SphereCollider>();
col.radius = 0.08f;  // ~8cm radius
```

**RULE 5.3** — KHÔNG spawn objects nếu `RuntimePerformanceSettings.ShouldLimitObjects` trả về `true` trên low-end device.

### 5.2 Update Frequency

**RULE 5.4** — AR placement update (raycasting) nên throttle khi activity đang in-progress và đã có stable placement:

```csharp
// Throttle từ every-frame sang every 0.5s khi đã place
if (hasLearningArea && !needsRePlacement) {
    // Skip raycast update
} else {
    UpdatePlacementPosition();
}
```

**RULE 5.5** — KHÔNG dùng `Update()` cho UI logic nếu có thể dùng events. `Update()` chỉ cho input polling và real-time tracking.

### 5.3 Memory & GC

**RULE 5.6** — Reuse `MaterialPropertyBlock` thay vì tạo mới mỗi lần highlight:

```csharp
private MaterialPropertyBlock highlightBlock;  // Instance field
void ApplyHighlight() {
    if (highlightBlock == null) highlightBlock = new MaterialPropertyBlock();
    renderer.GetPropertyBlock(highlightBlock);
    // ... set color
    renderer.SetPropertyBlock(highlightBlock);
}
```

**RULE 5.7** — KHÔNG dùng LINQ trong Update/FixedUpdate loops. Cache results.

**RULE 5.8** — Pool objects thay vì Destroy/Instantiate liên tục nếu activity có nhiều round.

### 5.4 Shader & Rendering

**RULE 5.9** — Fallback camera shader phải là unlit/simple shader. KHÔNG dùng PBR hoặc shader có nhiều instructions.

**RULE 5.10** — Target 30 FPS trên iOS (không phải 60). Điều chỉnh `Application.targetFrameRate = 30` cho AR scenes.

### 5.5 Build Settings

**RULE 5.11** — iOS Player Settings checklist trước build:
- [ ] `Architecture`: ARM64
- [ ] `Scripting Backend`: IL2CPP
- [ ] `Managed Stripping Level`: Low (để tránh stripping code AR Foundation)
- [ ] `Target SDK`: Latest stable
- [ ] `Camera Usage Description` phải có trong Player Settings
- [ ] `ARKit` capability enabled trong Xcode project

---

## 6. Testing Rules

### 6.1 Editor Testing (Simulation)

**RULE 6.1** — Editor mode dùng `ARPlacementServiceMock` (tự động qua `ARServiceBootstrap`). KHÔNG test AR Foundation trong Editor (không có ARKit support).

**RULE 6.2** — Test với `SC_TestSandbox` scene trước khi test trong activity scene.

**RULE 6.3** — Simulation mode phải có key input thay thế touch:
- Left click = tap
- Hold left click = drag (nếu enableDrag = true)

### 6.2 Unit Testing

**RULE 6.4** — Presenter logic phải có unit tests:
- State transitions
- Answer validation
- Hint request/limit
- Result tracking

**RULE 6.5** — KHÔNG unit test AR services trực tiếp (cần mock AR Foundation). Test qua interfaces.

**RULE 6.6** — Test data persistence với mock file system hoặc isolated storage.

### 6.3 Integration Testing

**RULE 6.7** — Test scene transitions:
- Boot → MainMenu → ActivitySelect → ARGameplay
- Activity complete → next activity or dashboard
- Cancel → back navigation

**RULE 6.8** — Test AR failure scenarios:
- AR not supported → graceful fallback
- Permission denied → user-friendly message
- Tracking lost → recovery flow

### 6.4 Device Testing

**RULE 6.9** — Test trên device THỰC cho tất cả AR functionality. KHÔNG rely on Editor simulation cho AR behavior.

**RULE 6.10** — Test checklist trên iOS device:
- [ ] App launch cold start
- [ ] AR session initialization
- [ ] Plane detection & visualization
- [ ] Learning area placement
- [ ] Object spawning & interaction
- [ ] Activity complete flow
- [ ] Progress persistence across app restart
- [ ] Memory usage stable sau 10 rounds
- [ ] Battery drain acceptable (target: < 15%/30min)

---

## 7. Debugging & Logging Rules

### 7.1 Logging

**RULE 7.1** — Phân biệt log levels:
- `Debug.Log()` — Debug info, state changes (strip in release)
- `Debug.LogWarning()` — Recoverable issues
- `Debug.LogError()` — Fatal errors, broken invariants

**RULE 7.2** — Log format chuẩn:

```csharp
Debug.Log($"[ClassName] Action: {details}. Context={additionalInfo}");
Debug.LogError($"[ClassName] Failed: {reason}. Expected={expected} Actual={actual}");
```

**RULE 7.3** — KHÔNG log trong Update/Frame-critical loops. Log tại state transitions và async events.

### 7.2 iOS Debugging

**RULE 7.4** — Enable `Development Build` trong Build Settings khi build cho iOS để có console logs.

**RULE 7.5** — Dùng Xcode Instruments để check:
- Memory allocations (Leaks, Allocations)
- GPU frame time
- Energy impact

**RULE 7.6** — Common iOS crash scenarios cần log rõ:
- `NSInternalInconsistencyException` (AR session conflict)
- Memory pressure → OOM kill
- Camera permission denied

### 7.3 Remote Logging (Production)

**RULE 7.7** — Track anonymous crash reports qua Unity Cloud Diagnostics hoặc custom solution.

**RULE 7.8** — Log AR session events với device metadata:
```csharp
AnalyticsService.TrackEvent("ar_session", new {
    state = currentState.ToString(),
    device = SystemInfo.deviceModel,
    os = SystemInfo.operatingSystem,
    trackingQuality = trackingQuality.ToString()
});
```

---

## 8. Code Style & Organization

### 8.1 File Organization

**RULE 8.1** — Mỗi class trong file riêng. File name = class name + `.cs`.

**RULE 8.2** — Namespace convention:
- `Core.AR.*` — AR Foundation wrappers
- `Core.Learning.*` — Activity framework
- `Core.UI.*` — UI components
- `Core.Data.*` — Data persistence
- `Features.Activities.*` — Activity implementations
- `Project.App` — Scene-level orchestration

**RULE 8.3** — KHÔNG viết file mới trong `Assets/_Project/Scripts/` nếu không phải scene-specific. Thêm vào layer tương ứng trong `Core/` hoặc `Features/`.

### 8.2 Script Size

**RULE 8.4** — Target < 500 lines per file. Nếu file > 500 lines, tách thành multiple files:
- `*View.cs` — View logic
- `*View.RuntimeUI.cs` — Runtime UI factory
- `*View.Layout.cs` — Layout constants
- `*Presenter.cs` — Presenter logic

**RULE 8.5** — Refactor `QuantityMatchView.cs` (1892 lines) thành:
- `QuantityMatchView.cs` (< 300 lines)
- `QuantityMatchRuntimeUI.cs` (~600 lines)
- `QuantityMatchLayoutConstants.cs` (~100 lines)
- `QuantityMatchViewState.cs` (~200 lines)

### 8.3 Comments

**RULE 8.6** — KHÔNG comment những thứ hiển nhiên:
```csharp
// ❌ SAI
// Check if null
if (obj == null) return;

// ✅ ĐÚNG
if (obj == null) return;
```

**RULE 8.7** — CHỈ comment khi:
- Non-obvious business rules
- Performance trade-offs
- Platform-specific workarounds
- Complex algorithm rationale

### 8.4 Null Safety

**RULE 8.8** — KHÔNG dùng `!obj` để check null. Luôn dùng `obj == null` (C# convention).

**RULE 8.9** — SerializeField references phải được validated trong Awake():

```csharp
void Awake() {
    if (arSession == null) {
        arSession = FindAnyObjectByType<ARSession>();
    }
    if (arSession == null) {
        Debug.LogError("[Bootstrap] ARSession missing!");
    }
}
```

**RULE 8.10** — Dùng null-conditional operator `?.` để tránh NRE khi chain calls:
```csharp
// ✅ Safe
presenter?.Initialize(config, view, placement);

// ❌ Risky
presenter.Initialize(config, view, placement);  // NRE if presenter is null
```

---

## 9. Scene & Navigation Rules

### 9.1 Scene Management

**RULE 9.1** — KHÔNG dùng `SceneManager.LoadScene()` với additive mode trong AR scenes. Chỉ dùng single mode để tránh AR state conflicts.

**RULE 9.2** — Trước khi load scene mới, ensure session cleanup:

```csharp
void PrepareToExit() {
    presenter?.Cancel();
    ProgressStorageProxy.Instance?.EndSession();
}
```

**RULE 9.3** — All scenes phải được add vào `Build Settings`. Check bằng:

```csharp
if (!Application.CanStreamedLevelBeLoaded(sceneName)) {
    Debug.LogError($"[Nav] Scene {sceneName} not in Build Settings!");
    return;
}
```

### 9.2 Prefab Rules

**RULE 9.4** — Activity object prefabs phải có:
- Collider (sphere hoặc box, không mesh collider)
- Renderer với material support `_BaseColor` hoặc `_Color`
- Không có nested inactive children

**RULE 9.5** — Prefabs phải được tested trong sandbox scene trước khi dùng trong activity.

### 9.3 Input Handling

**RULE 9.6** — KHÔNG handle input trong Update nếu không cần. Dùng event-based input system:

```csharp
// ✅ Event-based
ARInteractionService.OnObjectTapped += OnObjectTapped;

// ❌ Update polling (wasteful)
void Update() {
    if (Input.GetMouseButtonDown(0)) ProcessTap();
}
```

**RULE 9.7** — UI input phải check `EventSystem.current.IsPointerOverGameObject()` trước khi process AR raycasts.

---

## 10. Error Handling Rules

### 10.1 AR Errors

**RULE 10.1** — Handle AR unsupported case:

```csharp
void HandleARUnavailable(string reason) {
    // Show user-friendly message
    uiView.ShowARUnsupportedMessage(reason);
    // Offer alternative (non-AR mode) if possible
    // Log for analytics
    AnalyticsService.TrackEvent("ar_unsupported", new { reason });
}
```

**RULE 10.2** — AR session loss phải show feedback overlay, KHÔNG crash hoặc silent fail:

```csharp
sessionService.OnSessionLost += OnSessionLost;
void OnSessionLost() {
    view.ShowTrackingLostMessage();
    // Auto-retry tracking after delay
    StartCoroutine(RetryTrackingAfter(3f));
}
```

### 10.2 Activity Errors

**RULE 10.3** — Presenter error phải propagate lên View thông qua result hoặc event:

```csharp
void SubmitAnswer(ActivityAnswer answer) {
    try {
        bool correct = CheckAnswer(answer);
        // ...
    } catch (Exception e) {
        currentResult.SetTechnicalIssue(TechnicalIssueType.RuntimeError, e.Message);
        currentResult.Complete(false, ErrorType.Other);
        // Log
        Debug.LogException(e);
    }
}
```

**RULE 10.4** — KHÔNG swallow exceptions silently. Luôn log hoặc propagate.

---

## 11. Version Control & Workflow

### 11.1 Git Commits

**RULE 11.1** — Commit message format:
```
[type]: [short description]

[detailed changes if needed]

- Changed: ...
- Fixed: ...
- Added: ...
```

Types: `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `chore`

### 11.2 Branch Naming

**RULE 11.2** — Branch naming:
- `feature/[activity-name]` — New activity or feature
- `fix/[issue-name]` — Bug fix
- `perf/[component]` — Performance improvement
- `refactor/[component]` — Code refactor

### 11.3 Pre-commit Checklist

**RULE 11.3** — Trước khi commit AR/Activity changes:
- [ ] Build thành công trong Editor
- [ ] Không có `Debug.LogError` trong code mới
- [ ] Unit tests pass (nếu có)
- [ ] Scene references intact (check SceneManager.GetActiveScene())

---

## 12. Quick Reference Checklist

### Trước khi viết Script mới

- [ ] Script này thuộc layer nào? (AR/Activity/UI/Data/Support)
- [ ] Có interface tương ứng chưa?
- [ ] Có cần mock cho Editor simulation không?
- [ ] File sẽ có bao nhiêu dòng? > 500 thì tách ra

### Trước khi Build iOS

- [ ] Development Build enabled?
- [ ] Architecture: ARM64?
- [ ] Camera Usage Description đã thêm?
- [ ] ARKit capability enabled?
- [ ] Managed Stripping Level: Low?
- [ ] Scripting Backend: IL2CPP?
- [ ] Test trên device thực (không chỉ Editor)?

### Trước khi Merge PR

- [ ] Architecture rules followed?
- [ ] iOS performance rules followed?
- [ ] Null safety checked?
- [ ] No Debug.LogError in production path?
- [ ] Scene transitions tested?
- [ ] AR failure scenarios tested?

---

*Cập nhật: 29/05/2026*
