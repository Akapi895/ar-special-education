# Lesson 3: Composition (Number Bonds) — Implementation Plan

> **Dự án**: AR Special Education  
> **Loại Lesson**: Composition — Tách-gộp số (Number Bonds)  
> **Trạng thái trong codebase**: ❌ KHÔNG CÓ — cần tạo mới hoàn toàn  
> **Mức độ hoàn thiện hiện tại**: 0% — greenfield implementation  
> **Ưu tiên**: THẤP cho MVP — đây là bài thứ 3, cần Quantity Match và Compare hoàn chỉnh trước

---

## Phần A: Game Logic & Learning Experience Design

### A.1. Mục tiêu học tập (Learning Objectives)

| # | Objective | Mô tả | Measurable Outcome |
|---|-----------|--------|-------------------|
| LO-1 | Hiểu cấu trúc tách số | Trẻ hiểu một số có thể được chia thành 2 phần | Kéo object từ Whole sang Parts đúng cách |
| LO-2 | Hiểu cấu trúc gộp số | Trẻ hiểu 2 phần có thể gộp thành 1 tổng | Gom Parts về Whole đúng cách |
| LO-3 | Liên kết tách-gộp | Trẻ thấy tách và gộp là 2 mặt của cùng 1 quan hệ | Biểu thức `5 = 2 + 3` đúng cả 2 chiều |
| LO-4 | Hiểu biểu thức Number Bond | Trẻ nhận ra dạng `A = B + C` | Điền đúng B và/hoặc C |

### A.2. User Flow (Demo Path — Free Split Mode)

```
[1] SC_Boot → SC_MainMenu → SC_ActivitySelect
[2] User tap "Tách số" (NumberBonds)
[3] Load SC_ARGameplay
[4] AR session starts → plane detection active
[5] User tap đặt Learning Area
[6] System hiển thị Number Bond diagram:
    
           ┌─────┐
           │  5  │  ← Whole (chứa 5 objects)
           └─────┘
          ↗       ↖
        ┌───┐   ┌───┐
        │ A │   │ B │  ← Part A, Part B (trống ban đầu)
        └───┘   └───┘

[7] UI: "Con hãy chia 5 thành 2 nhóm"
[8] Expression: "5 = __ + __"
[9] User kéo 2 objects từ Whole xuống Part A
[10] Expression cập nhật realtime: "5 = 2 + __"
[11] User kéo 3 objects còn lại xuống Part B  
[12] Expression: "5 = 2 + 3"
[13] User tap "Xác nhận" (Confirm)
[14] Correct: green glow + celebration
[15] "Câu tiếp theo"
[16] After 10 rounds: Summary
```

### A.3. Activity Modes

#### Mode 1: Free Split (MVP — Ưu tiên)
- Whole ban đầu chứa đầy đủ objects
- User tự do kéo objects xuống Part A hoặc Part B
- Đúng khi: Whole = 0 AND PartA + PartB = Whole ban đầu
- Ví dụ: `5 = 2 + 3`, `7 = 3 + 4`, `6 = 6 + 0`

#### Mode 2: Target Split
- Whole đầy, một Part đã có sẵn objects (bị che)
- User cần điền phần còn thiếu
- Ví dụ: `5 = 2 + __` → cần điền 3 vào Part B

#### Mode 3: Compose (Ngược lại)
- Parts đã có sẵn, Whole trống
- User gom Parts về Whole
- Ví dụ: Part A = 2, Part B = 3 → kéo vào Whole → Whole = 5

#### Mode 4: Missing Part
- Biểu thức có một phần bị ẩn
- Ví dụ: `7 = __ + 4` → cần điền 3 vào Part A

### A.4. Number Bond Diagram (AR Visualization)

```
         ┌─────────────────┐
         │     WHOLE       │
         │   (5 objects)   │
         │  ┌───────────┐  │
         │  │ Circle 1  │  │  ← Visual zone indicator
         │  └───────────┘  │
         └─────────────────┘
              ↙       ↘
    ┌─────────────────┐ ┌─────────────────┐
    │     PART A      │ │     PART B      │
    │   (0 objects)   │ │   (0 objects)   │
    │  ┌───────────┐  │ │  ┌───────────┐  │
    │  │ Circle 2  │  │ │  │ Circle 3  │  │
    │  └───────────┘  │ │  └───────────┘  │
    └─────────────────┘ └─────────────────┘
    
    EXPRESSION: "5 = __ + __"
    
    Cập nhật khi user kéo objects:
    → Kéo 2 vào Part A: "5 = 2 + __"
    → Kéo 3 vào Part B: "5 = 2 + 3"
```

### A.5. States

| State | Entry Condition | Exit Condition | Visual |
|-------|-----------------|---------------|--------|
| `Initializing` | Scene loaded | Bootstrap complete | Loading spinner |
| `Ready` | AR + placement ready | User starts | Diagram spawning |
| `InProgress` | Round started | Answer submitted | Objects draggable |
| `Dragging` | User bắt đầu kéo | User thả object | Object đang di chuyển |
| `ConfirmPending` | All objects moved | User tap Confirm hoặc Cancel | Confirm button highlighted |
| `Completed` | All rounds done | — | Summary screen |

### A.6. Drag-Drop Interaction Flow

```
[1] User touch-down trên object trong Whole
[2] Object "pick up" — scale up 1.1x, shadow appears
[3] User drag finger across screen
[4] Object di chuyển theo finger (trên AR plane)
[5] Khi finger vào vùng Part A hoặc Part B:
    → Vùng đó highlight (border glow)
    → Release sẽ drop vào vùng đó
[6] User lift finger
[7] Object "drop" — kiểm tra vị trí
    → Nếu trong Part A/B zone → object stays, count tăng
    → Nếu không → object animate về Whole
[8] Expression binder cập nhật count
[9] Khi Whole = 0 AND PartA + PartB = target → enable Confirm button
```

### A.7. Answer Validation

| Condition | Result | Feedback |
|-----------|--------|----------|
| `PartA + PartB == wholeTarget AND Whole == 0` | ✅ Correct | Green glow, "Giỏi lắm!" |
| `Whole > 0` khi confirm | ❌ Incorrect (NotAllSplit) | "Con hãy chia hết tất cả" |
| `PartA + PartB != wholeTarget` khi confirm | ❌ Incorrect (WrongTotal) | "Tổng không đúng, thử lại" |
| Object drop outside zones | ⚠️ No action | Object animate back |
| Max attempts | ❌ Failed | Reveal answer |
| Tracking lost | ⚠️ Technical | Pause + retry |

### A.8. Feedback System

#### Visual Feedback
- **Correct**: Whole/Parts glow green, objects bounce, confetti
- **Incorrect (chưa split hết)**: Whole pulse đỏ, hint text "Chia hết tất cả xuống 2 phần"
- **Incorrect (sai tổng)**: Parts flash đỏ, hint text "Tổng không đúng"
- **Object picked up**: Scale 1.0→1.1, shadow
- **Object over valid zone**: Zone border glow
- **Object dropped in zone**: Scale bounce 1.1→1.0, count number animation

#### Audio Feedback
- **Object moved to Part A**: Soft "pop" sound
- **Object moved to Part B**: Soft "pop" sound (khác pitch)
- **Object returned to Whole**: "Whoosh" sound
- **Correct**: Celebration chime + verbal
- **Incorrect**: Soft buzz

### A.9. Data Objects

#### Question Data (NumberBondQuestion)
```csharp
int wholeTarget;              // Số tổng, e.g. 5
int knownPartA;               // Phần đã biết A, e.g. -1 (chưa biết)
int knownPartB;               // Phần đã biết B, e.g. -1 (chưa biết)
NumberBondMode mode;          // FreeSplit, TargetSplit, Compose, MissingPart
List<ActivityHint> customHints;
string objectPrefabName;
```

#### Answer Data (NumberBondAnswer)
```csharp
int finalWholeCount;          // Số objects còn lại trong Whole
int finalPartACount;          // Số objects trong Part A
int finalPartBCount;          // Số objects trong Part B
int expectedWhole;            // Target ban đầu
float responseTimeSeconds;
int attemptNumber;
bool hintsUsed;
int moveCount;                // Số lần di chuyển objects
```

#### Enums
```csharp
public enum NumberBondMode {
    FreeSplit,    // Whole → PartA + PartB, điền cả 2
    TargetSplit,  // Whole → PartA + __, điền 1 phần
    Compose,      // PartA + PartB → Whole
    MissingPart   // __ + __ = Whole, điền cả 2 (giống FreeSplit nhưng khác UI)
}

public enum BondZone {
    Whole,
    PartA,
    PartB,
    None
}
```

### A.10. Demo Scenario

**Round 1 (Free Split — dễ nhất)**:
```
Target: 5
Mode: FreeSplit
KnownPartA: -1 (chưa biết)
KnownPartB: -1 (chưa biết)
Start: Whole=5, PartA=0, PartB=0
UI: "Con hãy chia 5 thành 2 nhóm"
Expression: "5 = __ + __"
Success: Whole=0, PartA=2, PartB=3
```

**Round 2 (Free Split — khó hơn)**:
```
Target: 7
Mode: FreeSplit
Start: Whole=7, PartA=0, PartB=0
UI: "Con hãy chia 7 thành 2 nhóm"
Success: Whole=0, PartA=4, PartB=3
```

**Round 3 (Target Split)**:
```
Target: 6
Mode: TargetSplit
KnownPartA: 2
KnownPartB: -1
Start: Whole=4 (6-2), PartA=2, PartB=0
UI: "6 = 2 + __"
Success: Whole=0, PartA=2, PartB=4
```

---

## Phần B: Implementation Plan

### B.1. Hiện Trạng Code

#### Không có gì ❌
Đây là **greenfield implementation** — toàn bộ activity cần được tạo mới.

### B.2. Cấu trúc thư mục cần tạo

```
Assets/Features/Activities/NumberBonds/
├── Scripts/
│   ├── NumberBondsPresenter.cs       # Game logic (kéo thừa ActivityPresenter)
│   ├── NumberBondsView.cs            # UI: buttons, layout
│   ├── NumberBondsConfig.cs          # ScriptableObject config
│   ├── NumberBondsQuestion.cs         # Question data model
│   ├── NumberBondsAnswer.cs          # Answer data model
│   ├── NumberBondsActivityBootstrap.cs # Bootstrap
│   ├── NumberBondZone.cs             # Zone behavior (Whole/PartA/PartB)
│   ├── NumberBondExpressionBinder.cs  # Realtime expression binding
│   └── DragDropHandler.cs            # AR drag-drop logic
├── Interfaces/
│   └── INumberBondsView.cs           # View interface
├── Prefabs/
│   └── NumberBondZone.prefab         # Zone visual (circle + label)
└── Art/
    └── (zone textures, audio, etc.)
```

### B.3. Module/Script có thể tái sử dụng

| Module | Reuse Cho | Cách dùng |
|--------|----------|-----------|
| `ActivityPresenter` (base) | NumberBonds | State machine, hint, persistence |
| `ARServiceBootstrap` | NumberBonds | Resolve placement/interaction services |
| `ARPlacementService` | NumberBonds | `SpawnAtPosition` cho zone circles và objects |
| `ARInteractionService` | NumberBonds | Tap detection + drag (cần mở rộng cho AR drag) |
| `FeedbackServiceProxy` | NumberBonds | Audio/visual feedback |
| `SimpleAudioManager` | NumberBonds | Number playback, instructions |
| `ProgressStorageProxy` | NumberBonds | Lưu results |
| `HintSystem` | NumberBonds | 3-level hints |
| `ActivityPrefabSetup` | NumberBonds | Object prefabs |

### B.4. Những gì cần tạo mới

#### B.4.1. Core Scripts (P0 — Bắt buộc cho MVP)

| # | Script | Lines ước tính | Nội dung chính |
|---|--------|----------------|----------------|
| 1 | `NumberBondsConfig.cs` | ~120 | ScriptableObject, `wholeTarget`, `mode`, hints, feedback strings |
| 2 | `NumberBondsQuestion.cs` | ~50 | Question model |
| 3 | `NumberBondsAnswer.cs` | ~40 | Answer model |
| 4 | `NumberBondsPresenter.cs` | ~400 | Logic: LoadRound, CheckAnswer, OnZoneCountChanged, SubmitAnswer |
| 5 | `NumberBondsView.cs` | ~300 | UI: zone circles, expression display, confirm button, drag-drop area |
| 6 | `INumberBondsView.cs` | ~30 | Interface |
| 7 | `NumberBondsActivityBootstrap.cs` | ~80 | Bootstrap (dựa trên CompareQuantity bootstrap) |
| 8 | `NumberBondZone.cs` | ~150 | Zone behavior: accept object, remove object, count tracking, visual feedback |
| 9 | `NumberBondExpressionBinder.cs` | ~80 | Reactive binding: count → expression text |

#### B.4.2. AR Drag-Drop System (P0 — Bắt buộc)

| # | Component | Mô tả |
|---|-----------|--------|
| 1 | AR drag detection | Dùng `ARInteractionService` drag optional HOẶC custom raycast |
| 2 | Zone proximity detection | Kiểm tra object position vs zone boundaries |
| 3 | Object transfer logic | Move object từ zone này sang zone khác |
| 4 | Animation | Pick up, drop, snap animations |

#### B.4.3. Prefabs & Assets (P0)

| # | Asset | Type | Mô tả |
|---|-------|------|--------|
| 1 | NumberBondZoneCircle.prefab | Prefab | Circle visual với TextMeshPro cho count |
| 2 | WholeCircle.prefab | Prefab | Kế thừa ZoneCircle, custom visual |
| 3 | PartCircle.prefab | Prefab | Kế thừa ZoneCircle, custom visual |
| 4 | DragIndicator.prefab | Prefab | Visual khi đang kéo object |

#### B.4.4. Router Integration (P0)

| # | Thay đổi | File | Mô tả |
|---|-----------|------|--------|
| 1 | Thêm branch `"NumberBonds"` | `GameplayActivityRouter.cs` | `case "NumberBonds": CreateNumberBondsActivity(); break;` |
| 2 | Thêm `CreateNumberBondsActivity()` | `GameplayActivityRouter.cs` | Instantiate presenter/view/bootstrap |

### B.5. Implementation Detail: AR Drag-Drop

#### B.5.1. Approach 1: Dùng ARInteractionService drag (Khuyến nghị cho MVP)

```csharp
// NumberBondsPresenter.cs
public void Initialize(...) {
    // ARInteractionService đã có drag optional
    interactionService.OnObjectDragged += HandleObjectDragged;
    interactionService.OnObjectDropped += HandleObjectDropped;
}

private void HandleObjectDropped(GameObject obj, Vector3 position) {
    // Kiểm tra object đang ở zone nào
    BondZone targetZone = DetectZone(position);
    
    if (targetZone == BondZone.None) {
        // Animate về vị trí cũ
        AnimateReturnToOriginalPosition(obj);
        return;
    }
    
    // Lấy zone hiện tại của object
    BondZone currentZone = GetObjectCurrentZone(obj);
    
    if (currentZone != targetZone) {
        // Transfer object
        TransferObject(obj, currentZone, targetZone);
        UpdateZoneCounts();
        UpdateExpression();
    }
}
```

#### B.5.2. Approach 2: Touch/Mouse raycast (Fallback nếu ARInteractionService drag không đủ)

```csharp
// DragDropHandler.cs
public class DragDropHandler : MonoBehaviour {
    private GameObject draggedObject;
    private Vector3 dragOffset;
    private Camera arCamera;
    
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            TryStartDrag();
        }
        if (draggedObject != null && Input.GetMouseButton(0)) {
            UpdateDrag();
        }
        if (draggedObject != null && Input.GetMouseButtonUp(0)) {
            EndDrag();
        }
    }
    
    private void TryStartDrag() {
        Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            if (IsLearningObject(hit.collider.gameObject)) {
                draggedObject = hit.collider.gameObject;
                dragOffset = draggedObject.transform.position - hit.point;
            }
        }
    }
    
    private void UpdateDrag() {
        Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("ARPlane"))) {
            draggedObject.transform.position = hit.point;
        }
    }
    
    private void EndDrag() {
        BondZone targetZone = DetectZone(draggedObject.transform.position);
        // ... transfer logic
        draggedObject = null;
    }
}
```

### B.6. Implementation Detail: NumberBondZone

```csharp
// NumberBondZone.cs
public class NumberBondZone : MonoBehaviour {
    public BondZone ZoneType { get; private set; }
    
    [Header("Visuals")]
    public GameObject circleVisual;
    public TextMeshProUGUI countLabel;
    
    private List<GameObject> objectsInZone = new List<GameObject>();
    public int Count => objectsInZone.Count;
    
    public event Action<BondZone, int> OnCountChanged; // zone, newCount
    
    public void Initialize(BondZone type, Vector3 position) {
        ZoneType = type;
        transform.position = position;
        UpdateVisuals();
    }
    
    public bool AcceptObject(GameObject obj) {
        if (!CanAccept()) return false;
        
        objectsInZone.Add(obj);
        obj.transform.SetParent(transform, true);
        UpdateVisuals();
        OnCountChanged?.Invoke(ZoneType, Count);
        return true;
    }
    
    public bool RemoveObject(GameObject obj) {
        if (!objectsInZone.Contains(obj)) return false;
        
        objectsInZone.Remove(obj);
        UpdateVisuals();
        OnCountChanged?.Invoke(ZoneType, Count);
        return true;
    }
    
    public void ClearAllObjects() {
        foreach (var obj in objectsInZone.ToList()) {
            RemoveObject(obj);
        }
    }
    
    private void UpdateVisuals() {
        countLabel.text = Count.ToString();
        // Scale pulse animation khi count thay đổi
    }
    
    public bool CanAccept() {
        // TargetSplit mode: Part có known value không cho thêm
        return true; // Tùy mode
    }
}
```

### B.7. Implementation Detail: Expression Binder

```csharp
// NumberBondExpressionBinder.cs
public class NumberBondExpressionBinder : MonoBehaviour {
    [Header("UI Elements")]
    public TextMeshProUGUI wholeLabel;
    public TextMeshProUGUI partALabel;
    public TextMeshProUGUI partBLabel;
    public TextMeshProUGUI operatorText; // "="
    
    private int targetWhole;
    private int currentPartA;
    private int currentPartB;
    private bool isCompleteMode; // FreeSplit: cả 2 phần; TargetSplit: 1 phần
    
    public void Initialize(int targetWhole, int knownPartA, int knownPartB, bool isCompose) {
        this.targetWhole = targetWhole;
        currentPartA = knownPartA >= 0 ? knownPartA : 0;
        currentPartB = knownPartB >= 0 ? knownPartB : 0;
        
        wholeLabel.text = targetWhole.ToString();
        UpdateExpression();
    }
    
    public void OnZoneCountChanged(BondZone zone, int count) {
        switch (zone) {
            case BondZone.Whole:
                // Không cập nhật whole label
                break;
            case BondZone.PartA:
                currentPartA = count;
                break;
            case BondZone.PartB:
                currentPartB = count;
                break;
        }
        UpdateExpression();
    }
    
    private void UpdateExpression() {
        // FreeSplit: "5 = 2 + 3" HOẶC "5 = 2 + __" nếu PartB chưa đủ
        partALabel.text = currentPartA.ToString();
        partBLabel.text = currentPartB.ToString();
        
        // Highlight parts chưa hoàn thành
        bool partAComplete = currentPartA > 0 || targetWhole == 0;
        bool partBComplete = currentPartB > 0 || targetWhole == 0;
        
        // Color coding: xanh = đã có, vàng = đang điền
    }
    
    public bool IsFullySplit() {
        return currentPartA + currentPartB == targetWhole;
    }
}
```

### B.8. Dependencies cần lưu ý

| Dependency | Source | Potential Issue |
|------------|--------|-----------------|
| `ARInteractionService` drag support | Core/AR/Interaction | Cần verify drag optional hoạt động tốt trên iOS |
| AR plane detection cho drag-to-position | ARPlacementService | Object phải snap vào plane khi drop |
| TextMeshPro package | Unity TMPro | Phải import package nếu chưa có |
| Layer "ARPlane" | ARFoundation | Zones phải detect plane đúng layer |

### B.9. Rủi ro khi implement

| # | Rủi ro | Mức | Mitigation |
|---|---------|------|-------------|
| R1 | AR drag-drop không ổn định trên iOS | CAO | Implement Approach 2 (custom raycast) làm fallback |
| R2 | Object bị drop ngoài zones | CAO | Animate return + haptic feedback |
| R3 | Quá nhiều object (10+) trong Whole | TRUNG BÌNH | Clamp max 8 objects cho iOS performance |
| R4 | Expression binder không cập nhật realtime | TRUNG BÌNH | Event-driven, không poll |
| R5 | Zones quá nhỏ → khó drop vào | TRUNG BÌNH | Zones phải có hitbox đủ lớn (min 0.3m diameter) |
| R6 | Activity mới, không có test baseline | CAO | Viết unit tests cho presenter logic |

### B.10. Implementation Priority Order

```
THỨ TỰ THỰC HIỆN:

[P1] Tạo cấu trúc thư mục Features/Activities/NumberBonds/
     → Tạo file stubs cho tất cả scripts
     
[P2] Implement NumberBondsConfig + Question + Answer
     → ScriptableObject template
     
[P3] Implement NumberBondZone.cs
     → Accept/Remove object
     → Count tracking
     → Visual feedback
     
[P4] Implement NumberBondExpressionBinder.cs
     → Realtime binding
     → Expression format
     
[P5] Implement NumberBondsPresenter.cs
     → LoadRound
     → CheckAnswer logic
     → Event handlers
     
[P6] Implement NumberBondsView.cs
     → Create zone circles
     → UI buttons
     
[P7] Implement AR drag-drop
     → Integrate với ARInteractionService
     → Zone transfer logic
     
[P8] Thêm router branch
     → GameplayActivityRouter
     
[P9] Tạo SO_NumberBondsConfig_Demo.asset
     → 10 rounds FreeSplit
     
[P10] Test: Boot → NumberBonds → Spawn → Drag → Drop → Feedback
```

---

## Phần C: Testing Checklist

### C.1. Unity Editor Testing

| # | Test Case | Expected Result | Priority |
|---|-----------|----------------|----------|
| T1 | Boot → chọn NumberBonds → SC_ARGameplay | Scene load smooth, zones spawn | P0 |
| T2 | 3 zones spawn đúng vị trí | Whole trên, PartA/PartB dưới | P0 |
| T3 | Objects spawn trong Whole | 5 objects trong Whole zone | P0 |
| T4 | Drag object ra khỏi Whole | Object di chuyển theo mouse | P0 |
| T5 | Drop object vào Part A | Object stays, PartA count = 1 | P0 |
| T6 | Drop object vào Part B | Object stays, PartB count = 1 | P0 |
| T7 | Expression cập nhật realtime | "5 = 1 + 1" khi 2 objects đã move | P0 |
| T8 | Split đủ: Whole=0, PartA+PartB=5 → Confirm enabled | Button highlight | P0 |
| T9 | Tap Confirm khi đúng | Green glow, celebration | P0 |
| T10 | Tap Confirm khi chưa split hết | Error hint, "Chia hết tất cả" | P0 |
| T11 | Drop object ngoài zones | Object animate về Whole | P1 |
| T12 | Round complete → new round | Reset, new objects spawn | P0 |
| T13 | Complete 10 rounds → summary | Summary screen | P1 |

### C.2. iOS Device Testing

| # | Test Case | Expected Result | Priority |
|---|-----------|----------------|----------|
| i1 | AR plane detection | Plane xuất hiện khi scan | P0 |
| i2 | Tap đặt Learning Area | Anchor placed | P0 |
| i3 | Drag object trên iPhone | Touch drag mượt, không lag | P0 |
| i4 | Drop vào zone (touch release) | Object snap vào zone, count update | P0 |
| i5 | Multi-touch (2 fingers) | Không confuse drag | P1 |
| i6 | Expression update realtime | Không delay khi drop | P0 |
| i7 | Haptic feedback khi drop | Vibration nhẹ | P2 |
| i8 | Memory after 10 rounds | Stable | P1 |
| i9 | Performance: 60 FPS khi drag | Smooth animation | P1 |

---

## Phần D: Optional Enhancements (Post-MVP)

### D.1. Compose Mode (Ngược lại Free Split)

```
UI: "Hãy gom 2 + 3 thành 1 nhóm"

     ┌─────────┐           ┌─────────┐
     │ PART A  │           │ PART B  │
     │  (2)    │           │  (3)    │
     └─────────┘           └─────────┘
            ↘             ↙
              ┌─────────┐
              │  WHOLE  │
              │   (?)   │  ← Trống ban đầu
              └─────────┘

User kéo objects từ Parts về Whole
Expression: "2 + 3 = 5"
```

### D.2. Animation: Object Transfer Visual

```csharp
// Khi object transfer giữa zones
IEnumerator AnimateTransfer(GameObject obj, Vector3 from, Vector3 to) {
    float duration = 0.3f;
    float elapsed = 0f;
    
    while (elapsed < duration) {
        float t = elapsed / duration;
        obj.transform.position = Vector3.Lerp(from, to, t);
        obj.transform.localScale = Vector3.one * (1f + 0.1f * Mathf.Sin(t * Mathf.PI));
        elapsed += Time.deltaTime;
        yield return null;
    }
    
    obj.transform.position = to;
    obj.transform.localScale = Vector3.one;
}
```

### D.3. Hint Levels

| Level | Hint | Trigger | Content |
|-------|------|---------|---------|
| 1 | Text | Sai 1 lần | "Con hãy chia hết tất cả xuống 2 phần" |
| 2 | Animation | Sai 2 lần | Animate 2 objects xuống Part A |
| 3 | Visual | Sai 3 lần | Highlight Part B với số đang thiếu |

---

*Cập nhật: 29/05/2026*
