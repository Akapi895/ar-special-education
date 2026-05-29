# Lesson 2: Comparison — Implementation Plan

> **Dự án**: AR Special Education  
> **Loại Lesson**: Comparison — So sánh số lượng  
> **Trạng thái trong codebase**: ✅ ĐÃ CÓ Presenter/View/Bootstrap  
> **Mức độ hoàn thiện hiện tại**: ~60% — cần kiểm chứng UI và logic cụ thể  
> **Ưu tiên**: TRUNG BÌNH — đây là bài thứ 2 trong lộ trình, nên demo sau Quantity Match

---

## Phần A: Game Logic & Learning Experience Design

### A.1. Mục tiêu học tập (Learning Objectives)

| # | Objective | Mô tả | Measurable Outcome |
|---|-----------|--------|-------------------|
| LO-1 | So sánh đại lượng | Trẻ xác định nhóm nào có số lượng lớn hơn, nhỏ hơn, hoặc bằng nhau | Chọn đúng ký hiệu `>`, `<`, `=` |
| LO-2 | Đếm song song hai nhóm | Trẻ đếm nhóm bên trái, sau đó bên phải | Thao tác tap-count trên cả 2 nhóm |
| LO-3 | Hiểu ký hiệu `>`, `<`, `=` | Trẻ nhận diện mối quan hệ thông qua ký hiệu | Đặt đúng symbol giữa hai nhóm |
| LO-4 | Xác định bằng nhau | Trẻ phân biệt 2 nhóm cùng số lượng | Chọn `=` khi 2 nhóm bằng nhau |

### A.2. User Flow (Demo Path)

```
[1] SC_Boot → SC_MainMenu → SC_ActivitySelect
[2] User tap "So sánh số lượng" (CompareQuantity)
[3] Load SC_ARGameplay
[4] AR session starts → plane detection active
[5] User tap mặt bàn để đặt Learning Area
[6] System spawns 2 nhóm animals:
    Left group (blue): 3 cats
    Right group (orange): 5 cats
[7] UI header shows: "Nhóm nào nhiều hơn?" hoặc "3  ?  5"
[8] 3 large symbol buttons: ">"  "<"  "="
[9] User tap ">" (đúng vì 3 < 5)
[10] Correct: green glow + symbol enlarge + celebration
    OR Incorrect: red flash + shake symbol
[11] "Câu tiếp theo"
[12] After 10 rounds: Summary screen
```

### A.3. Interaction Modes

#### Mode 1: Symbol Selection (Default)
- User tap symbol button `>`, `<`, hoặc `=`
- Không cần tap vào groups
- Phù hợp cho trẻ đã quen với ký hiệu toán

#### Mode 2: Interactive Counting (Nâng cao)
- User tap lần lượt từng nhóm để đếm
- Mỗi lần tap → highlight từng object
- Sau khi đếm cả 2 nhóm → symbol buttons được enable
- Dùng khi trẻ chưa quen với ký hiệu `>`, `<`, `=`

### A.4. States

| State | Entry Condition | Exit Condition | Visual |
|-------|-----------------|---------------|--------|
| `Initializing` | Scene loaded | Bootstrap complete | Loading spinner |
| `Ready` | AR + placement ready | User starts | Groups spawning |
| `InProgress` | Round started | Answer submitted | Groups + symbol buttons visible |
| `Paused` | User paused | User resumed | Groups dimmed |
| `Completed` | All rounds done | — | Summary screen |

### A.5. Question Types

| Type | Question Format | Example | Correct Answer |
|------|-----------------|---------|----------------|
| `MoreThan` | "Nhóm nào nhiều hơn?" | Left=3, Right=5 | `>` (left < right → left < right) |
| `FewerThan` | "Nhóm nào ít hơn?" | Left=5, Right=3 | `<` (left > right → left > right) |
| `Equal` | "Hai nhóm có bằng nhau không?" | Left=4, Right=4 | `=` |
| `SymbolCompare` | "3  ?  5" | — | `<` (3 < 5) |

> **Lưu ý logic**: Khi hiển thị "3  ?  5", nghĩa là "3 so với 5, cái nào lớn hơn?".  
> Đáp án: `3 < 5` → chọn `<`.  
> Nếu hiển thị "5  ?  3": đáp án là `>`.  
> Cần xác định rõ convention trong UI.

### A.6. Answer Validation

| Condition | Result | Feedback |
|-----------|--------|----------|
| `selectedSymbol == correctRelation` | ✅ Correct | Green glow, "Giỏi lắm!" |
| `selectedSymbol != correctRelation` | ❌ Incorrect | Red flash, "Thử lại nhé" |
| Max attempts reached | ❌ Failed | Reveal answer |
| Tracking lost | ⚠️ Technical issue | Pause + retry |

### A.7. Feedback System

#### Visual Feedback
- **Correct**: Symbol button chuyển xanh, scale up 1.0→1.2→1.0, confetti particles
- **Incorrect**: Button shake, red border flash
- **Hint Level 1**: Text "Con hãy đếm từng nhóm rồi so sánh"
- **Hint Level 2**: Highlight larger group với color overlay
- **Hint Level 3**: Hiển thị count numbers bên trên mỗi nhóm (3 vs 5)

#### Audio Feedback
- **Correct**: Celebration chime + "Giỏi lắm con!"
- **Incorrect**: Soft buzz + "Thử lại nhé"
- **Instruction**: "Con hãy chọn dấu lớn hơn, nhỏ hơn, hay bằng"

### A.8. Data Objects

#### Question Data (CompareQuantityQuestion)
```csharp
int leftCount;                          // Số lượng nhóm trái, e.g. 3
int rightCount;                         // Số lượng nhóm phải, e.g. 5
ComparisonAnswer correctAnswer;          // Dáp án đúng: More, Fewer, Equal
GroupArrangementPattern arrangement;     // Layout: Horizontal, Triangle, Diagonal
string objectPrefabName;                 // Tên prefab để spawn
List<ActivityHint> customHints;         // Custom hints
bool showEqualOption;                   // Có hiển thị nút "=" không
```

#### Answer Data (CompareQuantityAnswer)
```csharp
ComparisonAnswer selectedAnswer;         // User chọn: More, Fewer, Equal
ComparisonAnswer correctAnswer;         // Đáp án đúng
int leftCount;
int rightCount;
float responseTimeSeconds;
int attemptNumber;
bool hintsUsed;
GroupArrangementPattern arrangement;
```

#### Enums
```csharp
public enum ComparisonAnswer {
    More,    // Left > Right
    Fewer,   // Left < Right
    Equal    // Left == Right
}

public enum GroupArrangementPattern {
    Horizontal,  // Left -- Right
    Triangle,    //   Left
               // Right
    Diagonal    // Left
               //    Right
}
```

### A.9. Demo Scenario

**Round 1 (More than - dễ nhất)**:
```
Left: 3 cats
Right: 5 cats
Question: "Nhóm nào nhiều hơn?"
Correct: Fewer (Left < Right → Left is fewer → chọn "<")
Symbol display: "3  ?  5"
```

**Round 2 (Equal)**:
```
Left: 4 cats
Right: 4 cats
Question: "Hai nhóm có bằng nhau không?"
Correct: Equal
```

**Round 3 (Fewer than)**:
```
Left: 6 cats
Right: 2 cats
Question: "Nhóm nào ít hơn?"
Correct: Fewer (Left > Right → Left is more → chọn "<")
```

---

## Phần B: Implementation Plan

### B.1. Hiện Trạng Code

#### Đã có ✅
| Script | Lines | Status |
|--------|-------|--------|
| `CompareQuantityPresenter.cs` | ~1025 | Core logic, cần kiểm chứng UI |
| `CompareQuantityView.cs` | ~1025 | UI hoàn chỉnh, runtime generation |
| `CompareQuantityConfig.cs` | (tồn tại) | ScriptableObject config |
| `CompareQuantityQuestion.cs` | (tồn tại) | Data model |
| `CompareQuantityAnswer.cs` | (tồn tại) | Answer model |
| `CompareQuantityActivityBootstrap.cs` | (tồn tại) | Bootstrap |

#### Chưa có ⚠️
| Component | Status | Ghi chú |
|-----------|--------|---------|
| `ComparisonAnswer` enum | Cần kiểm tra | Có thể đã có trong Question/Answer model |
| `GroupArrangementPattern` enum | Cần kiểm tra | Có thể dùng chung với QuantityMatch |
| `ICompareQuantityView` interface | Có trong proposal | Kiểm tra có trong codebase không |
| Symbol buttons UI | Trong View | Cần verify 3 buttons `>`, `<`, `=` |
| Two-group layout builder | Trong Presenter | Spawn 2 groups thay vì 3+ |

#### Kiểm tra cần làm (trước khi implement)
1. Đọc `CompareQuantityPresenter.cs` để xem logic `CheckAnswer` và `LoadRound`
2. Đọc `CompareQuantityView.cs` để xem UI elements và event handlers
3. Kiểm tra `CompareQuantityQuestion` có đủ fields cho các question types
4. Kiểm tra `CompareQuantityConfig` có `allowEqualCase` và `hintMode` không

### B.2. Module/Script có thể tái sử dụng

| Module | Reuse Cho | Cách dùng |
|--------|----------|-----------|
| `ActivityPresenter` (base) | Tất cả | State machine, hint, persistence |
| `ARServiceBootstrap` | Tất cả | Resolve services |
| `ARPlacementService` | CompareQuantity | `SpawnAtLearningAreaPosition` cho 2 groups |
| `ARInteractionService` | Tất cả | Không cần cho symbol selection mode |
| `FeedbackServiceProxy` | Tất cả | Audio/visual feedback |
| `SimpleAudioManager` | Tất cả | Instructions |
| `ProgressStorageProxy` | Tất cả | Lưu results |
| `HintSystem` (static) | Tất cả | 3-level hints |
| `ActivityPrefabSetup` | Tất cả | Animal prefabs |
| `ARGroupSpawnUtility` | CompareQuantity | `SpawnGrid`/`SpawnCircle` |

### B.3. Những gì cần bổ sung/chỉnh sửa

#### B.3.1. Logic Changes (MEDIUM priority)

| # | Thay đổi | File | Mô tảchi tiết |
|---|-----------|------|---------------|
| 1 | Kiểm tra `CheckAnswer` logic | `CompareQuantityPresenter.cs` | Hiện tại logic so sánh như thế nào? CorrectAnswer = Left > Right → More? Cần xác nhận convention |
| 2 | Thêm `SpawnTwoGroups` method | `CompareQuantityPresenter.cs` | Spawn 2 groups: left và right. Có thể dùng lại `SpawnGroups` với n=2 hoặc viết riêng |
| 3 | Binding symbol buttons với `selectedAnswer` | `CompareQuantityView.cs` | OnSymbolSelected → Presenter.SubmitAnswer |
| 4 | Thêm question type `SymbolCompare` | `CompareQuantityQuestion.cs` | Hiển thị "3 ? 5" thay vì "Nhóm nào nhiều hơn?" |
| 5 | Cập nhật UI layout cho 2 groups | `CompareQuantityView.cs` | Thay vì 3 group labels, chỉ 2: "Bên trái" và "Bên phải" |

#### B.3.2. UI Changes (MEDIUM priority)

| # | Thay đổi | File | Mô tảchi tiết |
|---|-----------|------|---------------|
| 1 | Symbol buttons `>`, `<`, `=` | `CompareQuantityView.cs` | 3 buttons đủ lớn, dễ tap trên iPhone (min 120x120px) |
| 2 | Two-group layout | `CompareQuantityView.cs` | Left group bên trái, right group bên phải, không chồng lấn |
| 3 | Count overlay | Option: hiện số count bên trên mỗi nhóm khi counting mode |
| 4 | Expression display | `CompareQuantityView.cs` | Hiện "3  ?  5" hoặc "Nhóm nào nhiều hơn?" tùy question type |

#### B.3.3. Config Changes (LOW priority)

| # | Thay đổi | File | Mô tảchi tiết |
|---|-----------|------|---------------|
| 1 | Thêm `allowEqualCase` | `CompareQuantityConfig.cs` | Cho phép/cấm câu hỏi bằng nhau |
| 2 | Thêm `questionType` | `CompareQuantityQuestion.cs` | `MoreThan`, `FewerThan`, `Equal`, `SymbolCompare` |
| 3 | Tạo `SO_CompareQuantityConfig_Demo.asset` | ScriptableObject | Demo config 10 rounds |

### B.4. Prefab/Asset cần thiết

| Asset | Type | Nguồn | Ghi chú |
|--------|------|--------|---------|
| Animal prefabs | Prefab | Đã có | Cần check tên |
| Symbol button textures | Sprite/Texture | Tự tạo | Hoặc dùng TextMeshPro ">", "<", "=" |
| Audio clips | AudioClip | Placeholder | "correct", "incorrect", "instruction" |

### B.5. Dependencies cần lưu ý

| Dependency | Source | Potential Issue |
|------------|--------|------------------|
| `CompareQuantityPresenter` inherits `ActivityPresenter` | Base class | Đảm bảo base.Initialize được gọi |
| `GameplayActivityRouter` has `"CompareQuantity"` branch | Router code | Verify branch đúng tên ActivityId |
| `CompareQuantityActivityBootstrap` exists in scene | Scene | Có thể nằm trong SC_ARGameplay |
| 2 groups không chồng lấn | Layout calculation | Cần tính spacing đúng |

### B.6. Rủi ro khi implement

| # | Rủi ro | Mức | Mitigation |
|---|---------|------|-------------|
| R1 | Convention `>`/`<` ngược | CAO | Xác định rõ: "3 > 5" hay "3 < 5"? UI phải thể hiện đúng |
| R2 | Two-group layout bị chồng lấn | TRUNG BÌNH | Tính spacing dựa trên group footprint, test trên nhiều iPhone sizes |
| R3 | View quá lớn (~1025 lines) | TRUNG BÌNH | Không mở rộng, chỉ sửa phần cần thiết |
| R4 | Count overlay không hiện đúng vị trí | THẤP | Test trên device thật |

### B.7. Implementation Priority Order

```
THỨ TỰ THỰC HIỆN (sau Quantity Match):

[P1] Đọc code hiện tại của CompareQuantityPresenter/View/Config
     → Hiểu logic so sánh hiện tại
     → Xác định convention >, <, =
     
[P2] Kiểm tra/verify 3 symbol buttons trong View
     → Buttons ">", "<", "=" đã có chưa?
     → Event handlers đã wire chưa?
     
[P3] Tạo SO_CompareQuantityConfig_Demo.asset
     → 5 câu MoreThan, 3 câu Equal, 2 câu FewerThan
     → Đảm bảo correctAnswer logic đúng
     
[P4] Test flow: Spawn 2 groups → tap symbol → feedback
     → Verify groups spawn đúng vị trí
     → Verify symbol selection works
     
[P5] Test feedback: correct/incorrect
     → Audio + visual feedback
     → Hint levels
     
[P6] Test navigation: next round, activity complete
```

---

## Phần C: Testing Checklist

### C.1. Unity Editor Testing

| # | Test Case | Expected Result | Priority |
|---|-----------|----------------|----------|
| T1 | Boot → chọn CompareQuantity → SC_ARGameplay | Scene load smooth | P0 |
| T2 | Spawn 2 groups: left=3, right=5 | Groups spawn ở 2 vị trí riêng biệt | P0 |
| T3 | Groups không chồng lấn | Visual separation rõ | P0 |
| T4 | Symbol buttons hiển thị: >, <, = | 3 buttons đủ lớn, dễ thấy | P0 |
| T5 | Tap ">" khi left < right → đúng | Green glow, celebration | P0 |
| T6 | Tap "=" khi left ≠ right → sai | Red flash, retry | P0 |
| T7 | Round với Equal case (4=4) | Hiển thị "=" là đúng | P1 |
| T8 | Hint Level 1: text hint | Text hiển thị | P1 |
| T9 | Hint Level 2: highlight larger group | Color overlay trên nhóm lớn hơn | P2 |
| T10 | Cancel → back to menu | Scene transition | P0 |
| T11 | Complete 10 rounds → summary | Summary screen | P1 |

### C.2. iOS Device Testing

| # | Test Case | Expected Result | Priority |
|---|-----------|----------------|----------|
| i1 | Tap symbol buttons trên iPhone | Buttons đủ lớn, dễ tap, không bị ẩn | P0 |
| i2 | Two groups không chồng lấn | Layout đúng trên iPhone SE, 13, 14 Pro Max | P0 |
| i3 | Group bên trái = 5, bên phải = 3 → đáp án ">" | Left > Right → chọn ">" | P0 |
| i4 | Equal case: 4 = 4 | "=" là đúng | P0 |
| i5 | Count overlay (nếu có) | Số hiện đúng bên trên mỗi nhóm | P2 |
| i6 | Memory after 10 rounds | Stable | P1 |

---

## Phần D: Architecture Notes

### D.1. Ký hiệu So Sánh — Xác Định Convention

```
Question: "3  ?  5"
→ Hỏi: 3 với 5, cái nào lớn hơn?
→ Đáp: 5 lớn hơn 3
→ Đáp án: 3 < 5  (chọn nút "<")
```

```
Question: "5  ?  3"  
→ Hỏi: 5 với 3, cái nào lớn hơn?
→ Đáp: 5 lớn hơn 3
→ Đáp án: 5 > 3  (chọn nút ">")
```

```
Convention trong code (cần verify):
→ correctAnswer được lưu là ComparisonAnswer.More khi nào?
→ left > right → More?  HOẶC  left < right → Fewer?
→ CẦN XÁC ĐỊNH TRƯỚC KHI IMPLEMENT
```

### D.2. Two-Group Layout

```csharp
// Trong CompareQuantityPresenter
private void SpawnTwoGroups(int leftCount, int rightCount) {
    Vector3 center = GetLearningAreaCenter();
    
    // Left group position
    Vector3 leftPos = center - Vector3.right * groupSpacing * 0.5f;
    SpawnGroup(leftCount, leftPos, "LeftGroup");
    
    // Right group position  
    Vector3 rightPos = center + Vector3.right * groupSpacing * 0.5f;
    SpawnGroup(rightCount, rightPos, "RightGroup");
}
```

### D.3. Bridge Sang Number Bonds (Optional Enhancement)

Nếu muốn nối demo CompareQuantity → NumberBonds, thêm step:

```
Round 10: "Nhóm bên trái nhiều hơn bên phải bao nhiêu?"
→ 5 vs 3 → đáp án: 2
→ Đây là bridge tự nhiên từ so sánh sang tách-gộp
→ Không bắt buộc cho MVP nhưng là enhancement tốt
```

---

*Cập nhật: 29/05/2026*
