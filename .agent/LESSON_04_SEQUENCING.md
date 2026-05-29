# Lesson 4: Sequencing — Implementation Plan

> **Dự án**: AR Special Education  
> **Loại Lesson**: Sequencing — Đếm trên trục số (Number Line Jump)  
> **Trạng thái trong codebase**: ✅ ĐÃ CÓ đầy đủ Presenter/View/Bootstrap/Config  
> **Mức độ hoàn thiện hiện tại**: ~80% — đã có core logic, cần cải thiện interaction và feedback  
> **Ưu tiên**: TRUNG BÌNH — bài cuối cùng trong lộ trình, demo sau 3 bài kia

---

## Phần A: Game Logic & Learning Experience Design

### A.1. Mục tiêu học tập (Learning Objectives)

| # | Objective | Mô tả | Measurable Outcome |
|---|-----------|--------|-------------------|
| LO-1 | Hiểu phép cộng qua dịch chuyển | Trẻ hiểu cộng = tiến sang phải trên trục số | Kéo nhân vật đúng số bước |
| LO-2 | Hiểu phép trừ qua dịch chuyển | Trẻ hiểu trừ = lùi sang trái trên trục số | Kéo nhân vật ngược đúng số bước |
| LO-3 | Đếm bước nhảy | Trẻ đếm số bước di chuyển | Bấm/cuộn qua đúng N vạch |
| LO-4 | Liên kết phép tính với trục số | Trẻ thấy `3+4=7` là "từ 3 nhảy 4 bước đến 7" | Character dừng ở đúng vị trí kết quả |

### A.2. User Flow (Demo Path)

```
[1] SC_Boot → SC_MainMenu → SC_ActivitySelect
[2] User tap "Đếm trên trục số" (NumberLineJump)
[3] Load SC_ARGameplay
[4] AR session starts → plane detection active
[5] User tap đặt Learning Area
[6] System hiển thị trục số AR:
    
    [0] [1] [2] [3] [4] [5] [6] [7] [8] [9] [10]
     │
    🐸 ← Character bắt đầu ở vị trí 3

[7] UI header: "3 + 4 = ?"
[8] 2 buttons: "←" (lùi) và "→" (tiến)
[9] User tap "→" 4 lần
[10] Character nhảy từng bước: 3 → 4 → 5 → 6 → 7
[11] Equation cập nhật: "3 + 4 = 3" → "3 + 4 = 4" → ... → "3 + 4 = 7"
[12] User tap "Xác nhận" khi character ở 7
[13] Correct: green glow + celebration
[14] "Câu tiếp theo"
[15] After 10 rounds: Summary
```

### A.3. Interaction Modes

#### Mode 1: Button Step (Default — Ưu tiên cho demo)
- 2 buttons: "←" và "→"
- Mỗi tap = 1 bước nhảy
- Character animate từng tick một
- Phù hợp cho trẻ nhỏ, tốt trên iPhone

#### Mode 2: Tap Number Tiles (Thay thế button)
- Tap trực tiếp lên vạch số để nhảy đến đó
- Character animate từ vị trí hiện tại đến vị trí tap
- Dùng khi trẻ đã quen với trục số

#### Mode 3: Drag Character (AR-native)
- Kéo nhân vật trên mặt phẳng
- Character snap vào nearest tick khi thả
- Dùng AR touch drag interaction
- ⚠️ Có thể không ổn định trên iOS, cần test kỹ

#### Mode 4: Continuous Drag (Advanced)
- Kéo liên tục, character theo finger
- Không snap, free movement
- ⚠️ Không khuyến khích vì trẻ cần hiểu từng bước

### A.4. Number Line Visualization

```
                    [TARGET: 7]
                         │
    ┌───────────────────────────────────────────┐
    │                                           │
    ▼                                           │
[0] [1] [2] [3] [4] [5] [6] [7] [8] [9] [10]  │
    │   │   │   │   │   │   │   │   │   │   │  │
    │   │   │   │   │   │   │   │   │   │   │  │
    │   │   │   ├───────┐   │   │   │   │   │  │
    │   │   │   │  🐸   │   │   │   │   │   │  │
    │   │   │   │START=3│   │   │   │   │   │  │
    │   │   │   └───────┘   │   │   │   │   │  │
    │   │   │               │   │   │   │   │  │
    │   │   │ ←←←←←←←←←←←←←┘   │   │   │   │  │
    │   │   │                   │   │   │   │  │
    └───────────────────────────────────────────┘
    
    EQUATION DISPLAY:
    ┌──────────────────────────┐
    │    3   +   4   =   7    │
    │              ↑           │
    │         currentPosition  │
    └──────────────────────────┘
    
    CONTROL BUTTONS:
    ┌─────────┐    ┌─────────┐
    │    ←    │    │    →    │
    │  (Lùi)  │    │ (Tiến)  │
    └─────────┘    └─────────┘
```

### A.5. States

| State | Entry Condition | Exit Condition | Visual |
|-------|-----------------|---------------|--------|
| `Initializing` | Scene loaded | Bootstrap complete | Loading spinner |
| `Ready` | AR + placement ready | User starts | Number line spawning |
| `InProgress` | Round started | Answer submitted | Character visible, buttons active |
| `Jumping` | User tap direction button | Animation complete | Character mid-jump, buttons disabled |
| `ConfirmPending` | Character stopped at position | User tap Confirm/Cancel | Confirm button highlighted |
| `Completed` | All rounds done | — | Summary screen |

### A.6. Number Line Configuration

| Parameter | Value | Ghi chú |
|-----------|-------|---------|
| Min number | 0 | Không đổi |
| Max number | 10 | Có thể mở rộng đến 15/20 |
| Start range | 0–5 | Bắt đầu thấp để có room nhảy |
| Jump steps | 1–5 | Số bước nhảy |
| Operation | Add hoặc Subtract | Cộng = tiến, Trừ = lùi |

### A.7. Answer Validation

| Condition | Result | Feedback |
|-----------|--------|----------|
| `characterPosition == targetNumber` | ✅ Correct | Green glow, "Giỏi lắm!" |
| `characterPosition != target` | ❌ Incorrect | Red flash, "Thử lại nhé" |
| Character overshoot boundary | ⚠️ Bump animation | "Không thể nhảy nữa" |
| Max jumps exceeded | ❌ Failed | "Hết số bước nhảy" |
| Tracking lost | ⚠️ Technical | Pause + retry |

### A.8. Feedback System

#### Visual Feedback
- **Correct**: Equation box chuyển xanh, character bounce, confetti
- **Incorrect**: Equation box đỏ, shake animation
- **Jump animation**: Arc trajectory, character scales up slightly at peak
- **Boundary hit**: Character bump animation, flash boundary
- **Max jumps warning**: Warning icon khi còn 1-2 jumps

#### Audio Feedback
- **Step sound**: Soft "boing" mỗi lần nhảy
- **Number spoken**: Đọc số khi nhảy đến vị trí mới
- **Correct**: Celebration chime + verbal
- **Incorrect**: Soft buzz
- **Boundary**: "Không thể đi tiếp" hoặc "Không thể lùi nữa"

### A.9. Data Objects

#### Question Data (NumberLineJumpQuestion)
```csharp
int startNumber;              // Vị trí bắt đầu, e.g. 3
int targetNumber;             // Kết quả mong muốn, e.g. 7
int numberLineMin;           // Min của trục số, e.g. 0
int numberLineMax;           // Max của trục số, e.g. 10
JumpOperation operation;      // Add hoặc Subtract
int jumpCount;               // Số bước cần nhảy (auto: target-start)
bool showEquationDuringJumps; // Cập nhật equation realtime
int maxJumpsAllowed;         // Giới hạn số bước (auto: jumpCount + 2 buffer)
List<ActivityHint> customHints;
```

#### Answer Data (NumberLineJumpAnswer)
```csharp
int startNumber;             // Vị trí bắt đầu
int finalPosition;           // Vị trí cuối cùng
int targetNumber;            // Kết quả đúng
List<JumpRecord> jumps;      // Lịch sử từng bước
bool hasOvershot;            // Nhảy quá target
bool hitBoundary;            // Chạm biên
bool exceededMaxJumps;       // Quá số bước cho phép
float responseTimeSeconds;
int attemptNumber;
bool hintsUsed;
```

#### Jump Record
```csharp
public class JumpRecord {
    public JumpStepDirection Direction { get; }  // Left or Right
    public int FromPosition { get; }
    public int ToPosition { get; }
    public float Timestamp { get; }
}
```

#### Enums
```csharp
public enum JumpOperation {
    Add,     // Tiến (+)
    Subtract // Lùi (-)
}

public enum JumpStepDirection {
    Left,    // -1
    Right    // +1
}
```

### A.10. Demo Scenarios

**Round 1 (Addition — dễ nhất)**:
```
Start: 3
Operation: Add
JumpCount: 4
Target: 7
Equation: "3 + 4 = ?"
Character starts at 3
User tap Right 4 times: 3→4→5→6→7
Confirm → correct
```

**Round 2 (Subtraction)**:
```
Start: 8
Operation: Subtract
JumpCount: 3
Target: 5
Equation: "8 - 3 = ?"
Character starts at 8
User tap Left 3 times: 8→7→6→5
Confirm → correct
```

**Round 3 (Easy addition)**:
```
Start: 0
Operation: Add
JumpCount: 2
Target: 2
Equation: "0 + 2 = ?"
Character starts at 0
User tap Right 2 times: 0→1→2
Confirm → correct
```

---

## Phần B: Implementation Plan

### B.1. Hiện Trạng Code

#### Đã có ✅
| Script | Lines | Status |
|--------|-------|--------|
| `NumberLineJumpPresenter.cs` | ~946 | Core logic hoàn chỉnh, state machine, jump animation |
| `NumberLineJumpView.cs` | (tồn tại) | UI: buttons, equation display, feedback |
| `NumberLineJumpConfig.cs` | (tồn tại) | ScriptableObject config |
| `NumberLineJumpQuestion.cs` | (tồn tại) | Question model |
| `NumberLineJumpAnswer.cs` | (tồn tại) | Answer model |
| `NumberLineJumpActivityBootstrap.cs` | (tồn tại) | Bootstrap |
| `NumberLineJumpConfig.cs` | (tồn tại) | Config với hints, feedback |
| `JumpRecord.cs` | (tồn tại) | Jump history model |
| `NumberLineBillboardBehavior.cs` | (trong Presenter) | Billboard cho number labels |

#### Đã implement tốt
- ✅ Number line creation với colored tiles (NormalTileColor, StartTileColor, TargetTileColor, CurrentTileColor)
- ✅ Character spawning tại start position (sphere hoặc prefab)
- ✅ Jump animation với arc trajectory
- ✅ Boundary bump animation
- ✅ Equation display cập nhật realtime
- ✅ Overshoot/ExceededMaxJumps tracking
- ✅ 3-level hints
- ✅ Correct/Incorrect feedback với outcome-specific messages
- ✅ Round progression

#### Cần cải thiện ⚠️
| Component | Status | Ghi chú |
|-----------|--------|---------|
| Interaction mode selection | Logic có sẵn nhưng UI cần verify | Buttons ←/→ đã hoạt động |
| Character prefab | Có placeholder sphere | Nên có character prefab tốt hơn (frog/rabbit) |
| Expression format | Equation Prompt Mode có | Cần verify 2 format: "3 + 4 = ?" vs "3 + 4 = 3" |
| Number labels | TextMeshPro có | Billboard hoạt động |

### B.2. Module/Script có thể tái sử dụng

| Module | Reuse Cho | Cách dùng |
|--------|----------|-----------|
| `ActivityPresenter` (base) | NumberLineJump | State machine, hint, persistence |
| `ARServiceBootstrap` | NumberLineJump | Resolve services |
| `ARPlacementService` | NumberLineJump | `SpawnAtPosition` cho tiles và character |
| `ARInteractionService` | NumberLineJump | Tap tiles để jump (Mode 2) |
| `ARGroupSpawnUtility` | NumberLineJump | `CalculateGroupPositions` với Horizontal arrangement |
| `FeedbackServiceProxy` | NumberLineJump | Audio/visual feedback |
| `SimpleAudioManager` | NumberLineJump | Number playback |
| `ProgressStorageProxy` | NumberLineJump | Lưu results |
| `HintSystem` | NumberLineJump | 3-level hints |
| `ActivityPrefabSetup` | NumberLineJump | Character prefab |

### B.3. Những gì cần bổ sung/chỉnh sửa

#### B.3.1. Logic Changes (LOW priority — đã hoàn thiện)

| # | Thay đổi | File | Mô tả |
|---|-----------|------|--------|
| 1 | Verify equation format trong Round 1-5 vs 6-10 | `NumberLineJumpPresenter.cs` | Hiện tại có 2 modes: equation prompt (rounds 6+) và position-based (rounds 1-5). Cần verify UI hiển thị đúng |
| 2 | Thêm character prefab lookup | `NumberLineJumpPresenter.cs` | `GetCharacterPrefab()` đã dùng `ActivityPrefabSetup.Instance?.GetJumpCharacterPrefab()` — verify method tồn tại |
| 3 | Snap-to-tick cho drag mode | `ARInteractionService` | Nếu dùng drag, character phải snap vào tick gần nhất |

#### B.3.2. UI Changes (MEDIUM priority)

| # | Thay đổi | File | Mô tả |
|---|-----------|------|--------|
| 1 | Tách `NumberLineBuilder` ra khỏi Presenter | `NumberLineJumpPresenter.cs` → `NumberLineBuilder.cs` | Giảm size Presenter, builder cho tiles, labels, colors |
| 2 | Thêm character controller riêng | `NumberLineCharacterController.cs` | Handle position, snap, animation |
| 3 | Expression binder tách riêng | `NumberLineExpressionBinder.cs` | Reactive binding cho equation UI |

#### B.3.3. Config Changes (LOW priority)

| # | Thay đổi | File | Mô tả |
|---|-----------|------|--------|
| 1 | Verify `jumpConfig.MaxJumpsWarningThreshold` | `NumberLineJumpConfig.cs` | Threshold để show warning |
| 2 | Thêm `interactionMode` field | `NumberLineJumpConfig.cs` | ButtonStep, TapTile, DragCharacter |
| 3 | Tạo `SO_NumberLineJumpConfig_Demo.asset` | ScriptableObject | Demo config 10 rounds |

### B.4. Prefab/Asset cần thiết

| Asset | Type | Nguồn | Ghi chú |
|--------|------|--------|---------|
| Jump character prefab (frog/rabbit) | Prefab | Cần tạo hoặc tìm | Hiện tại dùng sphere placeholder |
| Number tile textures | Sprite/Texture | Tự tạo hoặc procedural | Hoặc dùng Mesh + TextMeshPro |
| Audio: step sound | AudioClip | Tự record | "Boing" cho mỗi jump |
| Audio: number playback | AudioClip | Tự record hoặc TTS | Đọc số khi land |

### B.5. Dependencies cần lưu ý

| Dependency | Source | Potential Issue |
|------------|--------|-----------------|
| `NumberLineJumpView` implements `INumberLineJumpView` | View interface | Phải verify interface methods đầy đủ |
| `JumpStepDirection` enum | Trong Presenter hoặc shared | Cần tìm định nghĩa |
| `ARGroupSpawnUtility.CalculateGroupPositions` | Core/AR/Placement | Dùng cho Horizontal arrangement |
| `ActivityPrefabSetup.Instance` | Scene | Character prefab lookup |

### B.6. Rủi ro khi implement

| # | Rủi ro | Mức | Mitigation |
|---|---------|------|-------------|
| R1 | Number labels không billboard đúng | THẤP | `NumberLineBillboardBehavior` đã có trong Presenter |
| R2 | Jump animation lag trên iOS | TRUNG BÌNH | Animation duration có thể adjust (hiện tại configurable) |
| R3 | Equation format không rõ ràng | TRUNG BÌNH | Verify 2 formats: "3 + 4 = ?" (prompt) vs "3 + 4 = 3" (live) |
| R4 | Character prefab không tìm thấy | THẤP | Fallback về sphere placeholder |

### B.7. Implementation Priority Order

```
THỨ TỰ THỰC HIỆN (sau 3 bài kia):

[P1] Verify code hiện tại của NumberLineJumpPresenter
     → Đọc code để hiểu jump animation, equation format
     → Verify interaction flow
     
[P2] Tạo SO_NumberLineJumpConfig_Demo.asset
     → 5 câu Addition (3+2, 4+1, 2+3, 5+2, 1+4)
     → 3 câu Subtraction (7-2, 8-3, 6-1)
     → 2 câu khó hơn (4+5, 9-4)
     
[P3] Verify number line visualization trong Editor
     → Tiles spawn đúng vị trí
     → Labels hiển thị đúng số
     → Billboard hoạt động
     
[P4] Verify character placement
     → Character spawn tại start position
     → Fallback sphere hoạt động
     
[P5] Test button interaction: ←/→ buttons
     → Character nhảy từng bước
     → Equation update realtime
     
[P6] Test round flow: start → jump → confirm → feedback → next
     
[P7] Test complete activity flow: 10 rounds → summary
```

---

## Phần C: Testing Checklist

### C.1. Unity Editor Testing

| # | Test Case | Expected Result | Priority |
|---|-----------|----------------|----------|
| T1 | Boot → chọn NumberLineJump → SC_ARGameplay | Scene load, number line spawn | P0 |
| T2 | Number line 0-10 spawn đúng vị trí | 11 tiles ngang, cách đều | P0 |
| T3 | Number labels hiển thị đúng | 0, 1, 2, ... 10 | P0 |
| T4 | Character spawn tại start position | Sphere/prefab ở vị trí 3 | P0 |
| T5 | Equation display: "3 + 4 = ?" | Header hiển thị đúng | P0 |
| T6 | Tap "→" button → character nhảy | 3 → 4, animation arc | P0 |
| T7 | Equation update sau jump | "3 + 4 = 4" khi character ở 4 | P0 |
| T8 | Jump 4 lần → 7 → confirm → correct | Green glow, celebration | P0 |
| T9 | Jump quá target (overshoot) → wrong | Feedback "Đi quá rồi" | P1 |
| T10 | Boundary hit → bump animation | Character bump, flash | P1 |
| T11 | Max jumps warning khi còn 1 | Warning icon | P2 |
| T12 | Cancel → reset position | Character về start | P0 |
| T13 | Round complete → new round | Number line reset, character spawn mới | P0 |
| T14 | Complete 10 rounds → summary | Summary screen | P1 |

### C.2. iOS Device Testing

| # | Test Case | Expected Result | Priority |
|---|-----------|----------------|----------|
| i1 | AR plane detection | Trục số nằm trên mặt bàn thật | P0 |
| i2 | Tap đặt Learning Area | Anchor placed, line spawn đúng | P0 |
| i3 | Button tap trên iPhone | Buttons đủ lớn, responsive | P0 |
| i4 | Jump animation performance | 60 FPS, smooth arc | P0 |
| i5 | Equation update không lag | Realtime update mượt | P0 |
| i6 | Audio: step sound playback | Mỗi jump có sound | P1 |
| i7 | Audio: number playback | Đọc số khi land | P2 |
| i8 | Memory after 10 rounds | Stable | P1 |
| i9 | Orientation change | Number line layout không break | P2 |

---

## Phần D: Architecture Notes

### D.1. Number Line Layout Calculation

```csharp
// Trong NumberLineJumpPresenter
private void CreateNumberLine() {
    int min = currentQuestion.NumberLineMin; // 0
    int max = currentQuestion.NumberLineMax;  // 10
    int count = max - min + 1;               // 11
    
    Vector3 center = GetLearningAreaCenter();
    Vector3[] positions = ARGroupSpawnUtility.CalculateGroupPositions(
        numberOfGroups: count,
        centerPosition: center,
        spacing: jumpConfig.TileSpacing,
        arrangementPattern: GroupArrangementPattern.Horizontal
    );
    
    for (int i = 0; i < count; i++) {
        int number = min + i;
        Vector3 pos = positions[i] + Vector3.up * jumpConfig.NumberLineHeight;
        CreateNumberTile(number, pos);
    }
}
```

### D.2. Jump Animation

```csharp
// Arc trajectory cho jump
private IEnumerator AnimateCharacterJump(Vector3 start, Vector3 end, float duration) {
    float elapsed = 0f;
    float arcHeight = 0.22f; // Configurable
    
    while (elapsed < duration) {
        float t = elapsed / duration;
        float eased = t * t * (3f - 2f * t); // Smoothstep
        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
        
        characterObject.transform.position = Vector3.Lerp(start, end, eased) 
            + Vector3.up * arc;
        
        elapsed += Time.deltaTime;
        yield return null;
    }
    
    characterObject.transform.position = end;
}
```

### D.3. Two Equation Formats

```csharp
// Format 1: Rounds 1-5 (Position-based)
// Character position = right side of equation
// "3 + 4 = 3" (after 0 jumps)
// "3 + 4 = 4" (after 1 jump)
// "3 + 4 = 5" (after 2 jumps)
// ...
// "3 + 4 = 7" (after 4 jumps = CORRECT)

// Format 2: Rounds 6-10 (Prompt-based)
// Equation hiển thị "3 + 4 = ?"
// Không cập nhật right side
// User phải nhảy đến target rồi confirm
```

### D.4. Jump Bounds Checking

```csharp
private bool IsWithinBounds(int position) {
    return position >= currentQuestion.NumberLineMin 
        && position <= currentQuestion.NumberLineMax;
}

private bool HasOvershotTarget(int position, JumpStepDirection direction) {
    if (direction == JumpStepDirection.Right) {
        return position > currentQuestion.TargetNumber;
    } else {
        return position < currentQuestion.TargetNumber;
    }
}
```

---

## Phần E: Nối kết với các bài khác

### E.1. Từ Number Bonds sang Number Line Jump

```
Number Bonds: "5 = 2 + 3"
→ Giải thích: 2 + 3 = 5
→ Chuyển: "Hãy nhảy từ 0 đến 5 bằng cách nhảy 2 rồi 3"

Number Line Jump: "0 + 5 = ?"
→ Nhảy 0→2→5
→ Kết quả: 5
```

### E.2. Từ Compare Quantity sang Number Line Jump

```
Compare Quantity: "5 > 3"
→ Giải thích: 5 lớn hơn 3 là 2 đơn vị
→ Chuyển: "Hãy nhảy từ 3 đến 5"

Number Line Jump: "3 + 2 = ?"
→ Nhảy 3→4→5
→ Kết quả: 5
```

---

*Cập nhật: 29/05/2026*
