# Lesson 1: Quantity Recognition (Quantity Match) — Implementation Plan

> **Dự án**: AR Special Education  
> **Loại Lesson**: Quantity Recognition — Nhận biết số lượng  
> **Trạ thái trong codebase**: ✅ ĐÃ CÓ đầy đủ Presenter/View/Bootstrap/Config  
> **Mức độ hoàn thiện hiện tại**: ~75% — cần cải thiện interaction mode và hint feedback  
> **Ưu tiên**: CAO — đây là bài nền tảng, nên demo sớm nhất

---

## Phần A: Game Logic & Learning Experience Design

### A.1. Mục tiêu học tập (Learning Objectives)

| # | Objective | Mô tả | Measurable Outcome |
|---|-----------|--------|-------------------|
| LO-1 | Liên kết số-lượng | Trẻ nhìn N vật thể → nhận ra số N tương ứng | Chọn đúng group chứa đúng số lượng |
| LO-2 | Đếm tuần tự | Trẻ đếm từng vật thể một | Tap lần lượt vào object để highlight |
| LO-3 | So sánh nhóm | Trẻ phân biệt nhóm nhiều hơn/ít hơn | Chọn nhóm đúng sau khi đếm |

### A.2. User Flow (Demo Path)

```
[1] SC_Boot → SC_MainMenu → SC_ActivitySelect
[2] User tap "Nhận biết số lượng" (QuantityMatch)
[3] Load SC_ARGameplay
[4] AR session starts → plane detection active
[5] User tap mặt bàn để đặt Learning Area
[6] System spawns 3 groups of animals:
    Group A (blue): 2 cats
    Group B (orange): 4 cats  ← correct
    Group C (green): 3 cats
[7] UI header shows: "Con hãy chọn nhóm có 4 con mèo"
[8] User tap 1 lần vào group B → system highlights group
[9] User tap 2 lần vào group B → system counts "1, 2, 3, 4" → auto-submit
[10] Correct: green glow + celebration audio
    OR Incorrect: orange flash + shake
[11] "Câu tiếp theo" → spawn new question
[12] After 10 rounds: Summary screen
```

### A.3. Interaction Modes

#### Mode 1: Selection Mode (Rounds 1–5, default)
- User tap group → hệ thống count từng object trong group đó → auto-submit khi count = object count
- Tapping 1 lần: highlight group
- Tapping 2 lần: bắt đầu đếm (highlight từng object)
- Tapping 3 lần: reset count
- Tap lần thứ N (N = object count): auto-submit answer

#### Mode 2: Number Input Mode (Rounds 6–10)
- User nhìn groups nhưng nhập số vào keypad thay vì tap group
- Keypad hiển thị số 1–10
- User chọn số → submit
- Tốt cho người dùng đã quen với bài

#### Mode 3 (Editor Only): Free Roam Simulation
- Animals di chuyển tự do trong vùng giới hạn
- Camera orbit theo chuột phải
- F key: auto-frame animals
- Dùng để test trong Editor không có AR

### A.4. States

| State | Entry Condition | Exit Condition | Visual |
|-------|-----------------|---------------|--------|
| `Initializing` | Scene loaded | Bootstrap complete | Loading spinner |
| `Ready` | AR + placement ready | User starts | Groups spawning |
| `InProgress` | Round started | Answer submitted | Groups visible, hint button active |
| `Paused` | User paused | User resumed | Groups dimmed |
| `Completed` | All rounds done | — | Summary screen |

### A.5. Answer Validation

| Condition | Result | Feedback |
|-----------|--------|----------|
| `selectedGroup.Count == targetNumber` | ✅ Correct | Green glow, confetti, "Giỏi lắm!" |
| `selectedGroup.Count < targetNumber` | ❌ Incorrect (TooFew) | Orange flash, "Thử nhóm khác nhé" |
| `selectedGroup.Count > targetNumber` | ❌ Incorrect (TooMany) | Orange flash, "Thử nhóm khác nhé" |
| Max attempts reached | ❌ Failed | Red overlay, reveal answer |
| AR tracking lost | ⚠️ Technical issue | Pause + retry prompt |

### A.6. Feedback System

#### Visual Feedback
- **Correct**: Group area indicator chuyển xanh lá, scale pulse 1.0→1.1→1.0, animals bounce
- **Incorrect**: Screen shake (UITween), group area flash đỏ
- **Hint Level 1**: Text bubble "Con hãy đếm từng nhóm nhé"
- **Hint Level 2**: Highlight từng object trong group đúng theo thứ tự 1, 2, 3...
- **Hint Level 3**: Đếm to + animation pointing arrow

#### Audio Feedback
- **Correct**: Celebration chime + verbal "Giỏi lắm con!"
- **Incorrect**: Soft buzz + verbal "Thử lại nhé"
- **Instruction**: "Con hãy chọn nhóm có X con vật"

### A.7. Data Objects

#### Question Data (QuantityMatchQuestion)
```csharp
int targetNumber;           // Số cần tìm, e.g. 4
int numberOfGroups;          // Số nhóm hiển thị, e.g. 3
int[] objectCountsPerGroup;  // Số object mỗi nhóm, e.g. [2, 4, 3]
int correctGroupIndex;       // Index nhóm đúng, e.g. 1
string objectPrefabName;     // Tên prefab để spawn
List<ActivityHint> customHints; // Custom hints cho question này
```

#### Round Data (QuantityMatchAnswer)
```csharp
int selectedGroupIndex;     // Nhóm user chọn
int selectedGroupCount;      // Số count của nhóm đó
int targetNumber;            // Target ban đầu
float responseTimeSeconds;   // Thời gian phản hồi
int attemptNumber;          // Lần thử thứ mấy
bool hintsUsed;              // Có dùng hint không
GroupArrangementPattern arrangement; // Layout pattern
ErrorType? errorType;       // Loại lỗi nếu sai
```

### A.8. Demo Scenario

**Round 1 (Demo cơ bản)**:
```
Target: 4
Groups: [3, 4, 2]
UI: "Con hãy chọn nhóm có 4 con mèo"
Arrangement: Horizontal (xếp hàng để dễ đếm)
```

**Round 2 (Demo có group lớn hơn target)**:
```
Target: 3
Groups: [5, 3, 4]
UI: "Con hãy chọn nhóm có 3 con mèo"
Arrangement: Triangle (3 điểm)
```

**Round 6 (Chuyển sang Number Input)**:
```
Target: 5
Groups: [3, 4, 6, 2, 5]
UI: "Con hãy nhập số con mèo con thấy"
Interaction: Keypad thay vì tap group
```

---

## Phần B: Implementation Plan

### B.1. Hiện Trạng Code

#### Đã có ✅
| Script | Lines | Status |
|--------|-------|--------|
| `QuantityMatchPresenter.cs` | ~1075 | Core logic hoàn chỉnh |
| `QuantityMatchView.cs` | ~1892 | UI hoàn chỉnh, runtime UI generation |
| `QuantityMatchConfig.cs` | ~183 | ScriptableObject config |
| `QuantityMatchQuestion.cs` | ~100 | Data model |
| `QuantityMatchAnswer.cs` | (trong file khác) | Answer model |
| `QuantityMatchActivityBootstrap.cs` | ~124 | Bootstrap hoạt động |
| `QuantityMatchRuntimeUI.cs` | (tồn tại) | Runtime UI components |
| `GroupAreaIndicator.cs` | (tồn tại) | Group highlight/feedback |

#### Chưa có ⚠️
| Component | Status | Ghi chú |
|-----------|--------|----------|
| `NumberCardView` cho drag-drop | Không cần cho MVP | Proposal muốn drag-drop nhưng tap selection đủ cho demo |
| `CountingAnimationSequence` | Chưa tách riêng | Animation count nằm trong View, cần refactor nếu mở rộng |
| `QuantityMatchLayoutBuilder` | Chưa tách riêng | View quá lớn (~1892 lines) |
| Two-finger pinch zoom | Không thấy trong code | Không cần cho MVP |
| Voice counting (đếm to) | Không thấy trong code | Không cần cho MVP |

### B.2. Module/Script có thể tái sử dụng

| Module | Reuse Cho | Cách dùng |
|--------|----------|-----------|
| `ActivityPresenter` (base) | Tất cả activities | State machine, hint, persistence |
| `ARServiceBootstrap` | Tất cả AR activities | Resolve services |
| `ARPlacementService` | Tất cả activities | `SpawnGrid`, `SpawnCircle` |
| `ARInteractionService` | Tất cả activities | `RegisterInteractable`, tap detection |
| `FeedbackServiceProxy` | Tất cả activities | Audio/visual feedback |
| `SimpleAudioManager` | Tất cả activities | Instructions, number playback |
| `ProgressStorageProxy` | Tất cả activities | Lưu results |
| `HintSystem` (static) | Tất cả activities | 3-level hint progression |
| `ActivityPrefabSetup` | Tất cả activities | Lấy animal prefabs |
| `ARGroupSpawnUtility` | NumberLineJump | `CalculateGroupPositions` |

### B.3. Những gì cần bổ sung/chỉnh sửa

#### B.3.1. Logic Changes (MEDIUM priority)

| # | Thay đổi | File | Mô tả chi tiết |
|---|-----------|------|----------------|
| 1 | Thêm `interactionMode` vào `QuantityMatchConfig` | `QuantityMatchConfig.cs` | Thêm enum `InteractionMode { TapToSelect, NumberInput }` và field `interactionMode` để switch giữa 2 modes rõ ràng thay vì hardcode `roundNumber > 5` |
| 2 | Tách `CountingSequence` thành coroutine riêng | `QuantityMatchPresenter.cs` | Hiện tại counting sequence là inline, nên tách để có thể interrupt/retry |
| 3 | Thêm `CountingHint` visual | `GroupAreaIndicator.cs` hoặc script mới | Highlight từng object theo thứ tự 1-2-3... khi hint level 2 được gọi |
| 4 | Binding expression text với current count | `QuantityMatchView.cs` | Khi counting, UI text hiển thị "Đang đếm: 1, 2, 3..." |

#### B.3.2. UI Changes (LOW priority — View đã lớn)

| # | Thay đổi | File | Mô tả chi tiết |
|---|-----------|------|----------------|
| 1 | Tách runtime UI factory | `QuantityMatchView.cs` → `QuantityMatchRuntimeUI.cs` | Giữ nguyên View chỉ xử lý events, tách UI building ra factory class |
| 2 | Tách layout constants | `QuantityMatchLayoutConstants.cs` | Các hằng số như `RuntimeButtonSize`, `RuntimeDigitButtonSize` nên tách riêng |
| 3 | Thêm icon cho hint button | Button textures | Thay text "Gợi ý" bằng icon 💡 |

#### B.3.3. AR Interaction Changes (LOW priority)

| # | Thay đổi | File | Mô tả chi tiết |
|---|-----------|------|----------------|
| 1 | Smooth drag-selection thay vì tap-N | `QuantityMatchPresenter.cs` | Thay vì tap 3 lần để count, cho phép kéo qua objects để highlight theo đường đi |
| 2 | Object scale animation khi count | `GroupAreaIndicator.cs` | Mỗi object scale up/down khi được count |

#### B.3.4. Data/Config Changes (LOW priority)

| # | Thay đổi | File | Mô tả chi tiết |
|---|-----------|------|----------------|
| 1 | Thêm `interactionMode` field | `QuantityMatchConfig.cs` | Switch mode rõ ràng |
| 2 | Thêm `arrangementMode` field | `QuantityMatchConfig.cs` | `Horizontal`, `Triangle`, `Circle`, `Random` |
| 3 | Tạo `SO_QuantityMatchConfig_Demo.asset` | ScriptableObject | Demo config với 10 rounds cố định |

### B.4. Prefab/Asset cần thiết

| Asset | Type | Nguồn | Ghi chú |
|--------|------|--------|---------|
| Animal prefabs (cat, dog, etc.) | Prefab (.prefab) | Đã có trong `Features/Activities/QuantityMatch/Art/` hoặc `Shared/Prefabs/` | Cần check tên chính xác |
| UI Button textures | Sprite/Texture | Tự tạo hoặc dùng Unity built-in | Hoặc dùng SpriteRenderer với màu đơn sắc |
| Audio clips | AudioClip | Tự record hoặc placeholder | Cần: correct, incorrect, instruction, number 1-10 |
| Demo config | ScriptableObject | Tạo mới | `Assets/Resources/ActivityConfigs/SO_QuantityMatchConfig_Demo.asset` |

### B.5. Dependencies cần lưu ý

| Dependency | Source | Potential Issue |
|------------|--------|----------------|
| `ActivityPrefabSetup.Instance` | Scene hoặc prefab | Phải tồn tại trong `SC_ARGameplay` scene. Nếu null → fallback vào `defaultObjectPrefab` trong Presenter |
| `ARServiceBootstrap` | Scene | Phải có execution order `-200`. Nếu không có → `TryStartActivity` sẽ fail |
| `ARServiceBootstrap.Placement.IsPlacementAvailable` | Runtime | Cần user đặt learning area TRƯỚC khi activity start. Nếu không → bootstrap poll forever |
| `SimpleAudioManager` | Core/Support | Must `EnsureExists()` trước khi gọi `PlayNumber()` |
| `ProgressStorageProxy.Instance` | Core/Data | Must initialized trong `BootLoader` hoặc `LearningSceneServices` |

### B.6. Rủi ro khi Implement

| # | Rủi ro | Mức | Mitigation |
|---|---------|------|-------------|
| R1 | `QuantityMatchView` quá lớn (1892 lines) gây khó debug | CAO | Chỉ sửa những phần cần thiết, không mở rộng thêm. Tạo file mới nếu cần thêm logic |
| R2 | Demo path không ổn định nếu AR placement chưa ready | CAO | Dùng `SC_TestSandbox` trước để verify AR pipeline hoạt động |
| R3 | Animal prefabs không đúng tên → `SpawnAtPosition` return null | TRUNG BÌNH | Check `ActivityPrefabSetup.Instance.GetAnimalPrefab()` return đúng prefab |
| R4 | Number Input keypad không hiển thị đúng layout trên iPhone | TRUNG BÌNH | Test trên device thật với các kích thước màn hình khác nhau |
| R5 | Counting sequence bị interrupt bởi scene transition | THẤP | `OnDestroy` phải cleanup coroutines |

### B.7. Implementation Priority Order

```
THỨ TỰ THỰC HIỆN (để demo sớm nhất):

[P1] Tạo SO_QuantityMatchConfig_Demo.asset với 10 rounds đơn giản
     → Không cần code mới, chỉ tạo ScriptableObject data
     
[P2] Verify SC_ARGameplay scene có đủ: ARServiceBootstrap, ActivityPrefabSetup, ARServiceRegistry
     → Kiểm tra scene setup để tránh null reference

[P3] Tạo/verify demo config references đúng trong GameplayActivityRouter
     → Check router path "QuantityMatch" → StartExistingQuantityMatchActivity()

[P4] Thêm interactionMode field vào QuantityMatchConfig (1 dòng code)
     → Cần để switch giữa tap-selection và number-input rõ ràng

[P5] Test toàn bộ demo flow: Boot → ActivitySelect → ARGameplay → Spawn → Answer → Feedback
     → Unity Editor với AR Simulation

[P6] Nếu counting animation cần cải thiện → thêm CountingHint visual
     → Nhẹ, không cần refactor lớn
```

---

## Phần C: Testing Checklist

### C.1. Unity Editor Testing

| # | Test Case | Expected Result | Priority |
|---|-----------|----------------|----------|
| T1 | Boot → MainMenu → ActivitySelect → chọn QuantityMatch | Scene transition smooth, không crash | P0 |
| T2 | AR Simulation mode: plane detection hoạt động | Plane marker xuất hiện khi di chuột | P0 |
| T3 | Tap để đặt learning area | Anchor được tạo, `HasLearningArea = true` | P0 |
| T4 | Groups spawn với đúng số lượng | Group A=2, B=4, C=3 như config | P0 |
| T5 | Tap 1 lần vào group → highlight | Group color thay đổi | P0 |
| T6 | Tap 2 lần vào group → bắt đầu counting | Objects highlight theo thứ tự 1, 2, 3... | P1 |
| T7 | Counting đủ số → auto-submit | Answer submitted, feedback hiển thị | P0 |
| T8 | Chọn đúng → green glow + sound | Visual + audio feedback | P0 |
| T9 | Chọn sai → orange flash + shake | Visual feedback | P0 |
| T10 | Hint button → hint text hiển thị | Hint bubble với text | P1 |
| T11 | Round complete → "Câu tiếp theo" | New groups spawn | P0 |
| T12 | Hoàn thành 10 rounds → summary | Progress dashboard hoặc completion screen | P1 |
| T13 | Cancel button → back to MainMenu | Scene transition | P0 |
| T14 | Editor: F key → camera auto-frame animals | Camera orbit quanh animals | P2 |
| T15 | Editor: mouse right-drag → orbit camera | Camera control | P2 |

### C.2. iOS Device Testing

| # | Test Case | Expected Result | Priority |
|---|-----------|----------------|----------|
| i1 | Cold start app | Boot → MainMenu < 3 giây | P0 |
| i2 | Tap "Bắt đầu" | AR permission dialog xuất hiện | P0 |
| i3 | AR session initialization | "Scanning..." indicator, plane detection | P0 |
| i4 | AR Tracking lost (cheo camera đi) | Tracking lost message, auto-recover | P1 |
| i5 | Tap đặt learning area | Anchor placed, area visualization | P0 |
| i6 | Groups spawn đúng vị trí | Objects nằm trên mặt bàn thật | P0 |
| i7 | Tap interaction trên iPhone | Highlight + count hoạt động mượt | P0 |
| i8 | Keypad number input (rounds 6+) | Keypad hiển thị đúng kích thước | P1 |
| i9 | Correct answer feedback | Haptic + sound + visual | P1 |
| i10 | Memory after 10 rounds | Stable, không tăng đáng kể | P1 |
| i11 | Battery drain 30 phút | < 15% | P2 |
| i12 | Orientation change (portrait↔landscape) | UI layout không break | P2 |

---

## Phần D: Refactoring Notes (Optional — Không cần cho demo)

### D.1. Nếu muốn giảm size QuantityMatchView

Tách `QuantityMatchView.cs` (1892 lines) thành:

| File mới | Lines ước tính | Nội dung |
|-----------|---------------|----------|
| `QuantityMatchView.cs` | ~250 | Event handlers, view state, wire to presenter |
| `QuantityMatchRuntimeUI.cs` | ~400 | `BuildRuntimeUi()`, `Create*Button()`, `Create*Text()` |
| `QuantityMatchLayoutConstants.cs` | ~100 | Tất cả `static readonly` constants |
| `QuantityMatchViewState.cs` | ~150 | Internal state class cho view |

### D.2. Counting Sequence Extraction

```csharp
// Trong QuantityMatchPresenter.cs
private IEnumerator PlayCountingSequence(int groupIndex) {
    var group = spawnedGroups[groupIndex];
    var objects = group.GetComponentsInChildren<Renderer>();
    
    for (int i = 0; i < objects.Length; i++) {
        // Scale pulse
        objects[i].transform.localScale = Vector3.one * 1.3f;
        yield return new WaitForSeconds(0.15f);
        objects[i].transform.localScale = Vector3.one;
        
        // Audio: speak number
        SimpleAudioManager.Instance.PlayNumber(i + 1);
        
        // View update
        view?.ShowCountingFeedback(groupIndex, i + 1, objects.Length);
    }

    // Auto-submit after counting
    HandleGroupSelected(groupIndex, objects.Length);
}
```

---

## Phần E: Implementation Notes & Bug Tracking

### E.1. Các thay đổi đã thực hiện (29/05/2026)

| # | File | Thay đổi | Mô tả |
|---|------|-----------|--------|
| 1 | `QuantityMatchPresenter.cs` | Xóa duplicate field | `currentUsesNumberInputMode` bị khai báo 2 lần → xóa bản trùng lặp |
| 2 | `QuantityMatchView.cs` | Fix Clear/Submit buttons | `SetActive(false)` → `SetActive(hasValue)` để hiện nút khi có input |
| 3 | `QuantityMatchView.cs` | Add keyboard input dev | Hỗ trợ Input System + Legacy Input Manager cho dev test |
| 4 | `QuantityMatchView.cs` | Fix NullReferenceException | Thêm null check cho `Image` và `Text` trong `CreateNumberInputText` |
| 5 | `QuantityMatchConfig.cs` | Thêm `switchToNumberInputAtRound` | Config field để set round chuyển sang number input |
| 6 | `QuantityMatchPresenter.cs` | Dùng config value | Thay `SelectionQuestionCount = 5` bằng `config.switchToNumberInputAtRound` |

### E.2. Lỗi còn tồn tại

| # | Lỗi | Nguyên nhân | Ảnh hưởng | Trạng thái |
|---|------|-------------|-------------|-------------|
| 1 | `Particle Velocity curves must all be in the same mode` | Confetti/particle prefab có inconsistency về curve mode | Không ảnh hưởng gameplay, chỉ warning | **CẦN FIX** - cần kiểm tra particle prefab |
| 2 | Round 6+ keyboard input không hoạt động | Có thể Input System event bị consume bởi UI | Không thể test number input mode | **CẦN INVESTIGATE** |

### E.3. Cách test Number Input Mode (Round 6+)

Do lỗi particle và Input System, test trên Editor gặp khó khăn. Các phương án:

1. **Ưu tiên test trên device thật (iOS/Android)** - Input System hoạt động tốt hơn trên mobile
2. **Tạm thời bỏ qua** - Tính năng number input mode vẫn compile đúng, chỉ cần test khi build lên device
3. **Debug thêm** - Nếu cần test sớm, thêm log để trace keyboard events

### E.4. Known Limitations

- **Unity 6 + Input System**: Legacy `UnityEngine.Input` không hoạt động khi Input System active
- **Runtime UI generation**: Sử dụng `Text` (Legacy) thay vì TextMeshPro - cần migrate về sau
- **Particle effects**: Cần kiểm tra confetti prefab cho consistency

---

*Cập nhật: 29/05/2026*
