# Lesson 3: Composition (Number Bonds) — Complete Implementation Plan

> **Dự án**: AR Special Education  
> **Lesson**: Lesson 3 — Composition / Number Bonds / Tách-gộp số  
> **ActivityId đề xuất**: `NumberBonds`  
> **LessonId đề xuất**: `LESSON_03_COMPOSITION`  
> **Trạng thái hiện tại trong codebase**: ❌ Chưa có activity `NumberBonds` hoàn chỉnh  
> **Mức độ hiện tại**: 0% implementation riêng cho lesson này, nhưng có thể tái sử dụng nhiều hạ tầng có sẵn  
> **Ưu tiên cho demo 4 bài**: **CAO** — cần có MVP để chuỗi demo đủ `Quantity Match → Compare Quantity → Number Bonds → Number Line Jump`  
> **Ưu tiên production learning path**: Sau khi `QuantityMatch` và `CompareQuantity` ổn định  
> **MVP demo scope**: `FreeSplit` + `TargetSplit`, touch/mouse drag-drop, realtime expression, confirm, feedback đúng/sai, router integration  

---

## 0. Căn cứ thiết kế

File này được viết lại từ các nguồn đã cung cấp:

1. `AR Proposal.pdf`  
   - Proposal mô tả `Number Bonds` là bài thứ ba trong lộ trình, sau `Compare Quantity`.
   - Mục tiêu là giúp trẻ hiểu bản chất tách-gộp của số, thấy quan hệ giữa số tổng và các số thành phần.
   - Trải nghiệm mong muốn là sơ đồ ba vòng tròn trong AR: `Whole`, `Part A`, `Part B`.
   - Trẻ kéo/thả vật thể để tách số hoặc gộp số.
   - Biểu thức toán học cập nhật realtime, ví dụ `5 = __ + __`, `__ = 2 + 3`.
   - Có nút xác nhận, phản hồi xanh/đỏ và âm thanh đúng/sai.
   - Proposal có nhắc `surface tracking` và `hand-tracking`.

2. `AR_UNITY_ARCHITECTURE_PIPELINE.md`  
   - Hệ thống hiện có activity layer theo pattern `Presenter / View / Bootstrap`.
   - Activity hiện có gồm `QuantityMatch`, `NumberLineJump`, `CompareQuantity`; chưa thấy `NumberBonds`.
   - `ActivityPresenter` base class đã có state machine, rounds, hint system, progress persistence, feedback trigger.
   - `ARServiceBootstrap` expose các service chính: `Session`, `Placement`, `Interaction`.
   - `GameplayActivityRouter` route activity dựa trên `SelectedActivityData.ActivityId`.
   - Flow gameplay hiện tại đi qua `SC_Boot → SC_MainMenu → SC_ActivitySelect → SC_ARGameplay`.

3. `AR_UNITY_IOS_WORKFLOW_RULES.md`  
   - Activity layer phải theo MVP.
   - Presenter chứa business logic, không thao tác trực tiếp `GameObject/Transform`.
   - View render UI, forward input, animation/feedback playback.
   - AR service phải đi qua interface và được resolve qua `ARServiceBootstrap`.
   - Không tạo service AR mới trong `Core/AR` nếu không thật sự cần ARFoundation.
   - Activity startup nên event-driven, tránh polling.
   - iOS phải ưu tiên performance, object count clamp, collider đơn giản, test trên device thật.

4. `LESSON_03_COMPOSITION.md` bản hiện tại  
   - Đã có ý tưởng tốt về user flow, activity modes, drag-drop, answer validation, data model, testing checklist.
   - Bản viết lại này giữ lại phần hợp lý, nhưng chỉnh scope, bổ sung proposal alignment, làm rõ reuse từ hệ thống hiện tại, ràng buộc theo architecture rules, và cắt MVP để dễ implement/demo hơn.

---

## 1. Status Summary

### 1.1. Hiện trạng codebase đối với lesson này

| Hạng mục | Trạng thái | Nhận xét |
|---|---:|---|
| `NumberBondsPresenter` | ❌ Chưa có | Cần tạo mới, kế thừa `ActivityPresenter` |
| `NumberBondsView` | ❌ Chưa có | Cần tạo mới, không để quá lớn như `QuantityMatchView` |
| `NumberBondsActivityBootstrap` | ❌ Chưa có | Cần tạo mới để wire config/view/services |
| `NumberBondsConfig` | ❌ Chưa có | Cần tạo ScriptableObject config |
| `NumberBondZone` | ❌ Chưa có | Cần tạo để biểu diễn `Whole`, `PartA`, `PartB` |
| `NumberBondExpressionBinder` | ❌ Chưa có | Cần tạo để cập nhật biểu thức realtime |
| Router branch `"NumberBonds"` | ❌ Chưa có | Cần thêm vào `GameplayActivityRouter` |
| AR session / placement / interaction service | ✅ Đã có hạ tầng | Tái sử dụng qua `ARServiceBootstrap` |
| Progress / Feedback / Audio / Hint | ✅ Đã có hạ tầng | Tái sử dụng từ support services |
| Scene flow `SC_ARGameplay` | ✅ Đã có | Activity mới chạy trong scene gameplay hiện tại |

### 1.2. Kết luận hiện trạng

`Number Bonds` là **greenfield activity**, nhưng không phải greenfield toàn hệ thống. Không cần tạo lại pipeline AR, scene flow, feedback, storage, hint, audio. Cần tạo mới activity implementation trong `Features/Activities/NumberBonds/`, sau đó integrate vào router và activity select.

---

## 2. Proposal Alignment

### 2.1. Proposal muốn bài học này đạt trải nghiệm gì?

Theo hướng proposal, `Number Bonds` không phải bài chọn đáp án đơn giản. Đây là bài giúp trẻ **tự tay thao tác với số lượng** để hiểu một số tổng có thể được tách thành hai phần, hoặc hai phần có thể được gộp thành một tổng.

Trải nghiệm proposal nhắm tới:

```text
Một sơ đồ Number Bond gồm 3 vòng tròn xuất hiện trong AR:

           WHOLE
          (Tổng)
          /     \
     PART A     PART B
   (Thành phần) (Thành phần)

Trẻ kéo vật thể từ Whole xuống Part A / Part B để tách số.
Hoặc kéo vật thể từ Part A / Part B về Whole để gộp số.
Biểu thức toán học cập nhật realtime theo số vật thể trong từng vùng.
```

### 2.2. Những điểm bắt buộc phải giữ từ proposal

| Proposal requirement | Diễn giải implementation |
|---|---|
| Sơ đồ ba vòng tròn | Phải có 3 zone rõ ràng: `Whole`, `PartA`, `PartB` |
| Tách số | MVP phải cho kéo object từ `Whole` xuống `PartA`/`PartB` |
| Gộp số | Có thể là Post-MVP, nhưng kiến trúc cần chuẩn bị mode `Compose` |
| Realtime data binding | Expression phải đổi ngay khi count trong zone đổi |
| Confirm button | Không auto-complete ngay; trẻ bấm `Xác nhận` để kiểm tra |
| Feedback đúng/sai | Đúng: xanh/âm thanh tích cực; sai: đỏ nhẹ/hint, không phạt mạnh |
| AR surface tracking | Object/zone đặt trên learning area trong `SC_ARGameplay` |
| Hand-tracking | Không bắt buộc cho MVP nếu hệ thống hiện tại chưa có; dùng touch drag/drop trước |

### 2.3. Điều chỉnh hợp lý cho MVP

Proposal nhắc đến `hand-tracking`, nhưng kiến trúc hiện tại chỉ mô tả `ARInteractionService` với tap/select/highlight/drag optional, input qua EnhancedTouch và mouse fallback. Vì vậy MVP nên dùng:

```text
Primary interaction: touch drag/drop trên iOS
Editor simulation: mouse drag/drop
Post-MVP enhancement: hand-tracking nếu có SDK và thời gian
```

Không nên chặn demo vì chưa có hand-tracking thật.

---

## 3. MVP Demo Scope

### 3.1. Cần có trong bản demo

Bản demo cần đủ để chứng minh bài `Composition / Number Bonds` hoạt động trong chuỗi 4 lesson:

| Hạng mục | Bắt buộc? | Mô tả |
|---|---:|---|
| Activity xuất hiện trong Activity Select | ✅ | User chọn được `Tách-gộp số` |
| Router nhận `ActivityId = "NumberBonds"` | ✅ | Load đúng activity trong `SC_ARGameplay` |
| 3 zone AR | ✅ | `Whole`, `PartA`, `PartB` spawn trên learning area |
| Object spawn trong zone | ✅ | Round 1 spawn 5 object trong `Whole` |
| Drag/drop object | ✅ | Kéo object từ `Whole` sang `PartA`/`PartB` |
| Count realtime | ✅ | Zone count cập nhật ngay sau drop |
| Expression realtime | ✅ | Ví dụ `5 = 2 + 3` |
| Confirm button | ✅ | Kiểm tra đúng/sai |
| Feedback đúng/sai | ✅ | Glow/text/audio cơ bản |
| Round reset | ✅ | Sau đúng thì chuyển round tiếp theo |
| Summary đơn giản | ✅ | Hoàn thành 2-3 round demo |

### 3.2. Chưa cần trong bản demo

| Hạng mục | Lý do chưa cần |
|---|---|
| Full Compose Mode | Có thể chuẩn bị enum/data model, nhưng chưa cần UI hoàn chỉnh |
| Full MissingPart Mode | Post-MVP |
| Hand-tracking thật | Rủi ro cao, chưa thấy hạ tầng hiện có |
| Confetti phức tạp | Không ảnh hưởng logic học tập |
| Haptic feedback | Nice-to-have |
| Object pooling nâng cao | Chưa cần nếu demo chỉ 2-3 rounds, nhưng code nên không cản trở pooling sau này |
| Adaptive difficulty đầy đủ | Chỉ cần tracking data cơ bản trước |

### 3.3. Demo rounds đề xuất

| Round | Mode | Target | Start State | Expected Demo |
|---:|---|---:|---|---|
| 1 | `FreeSplit` | 5 | `Whole=5`, `PartA=0`, `PartB=0` | Trẻ chia thành `2 + 3` hoặc cách bất kỳ |
| 2 | `TargetSplit` | 6 | `Whole=4`, `PartA=2 locked`, `PartB=0` | Trẻ điền phần còn thiếu: `6 = 2 + 4` |
| 3 | `FreeSplit` | 7 | `Whole=7`, `PartA=0`, `PartB=0` | Trẻ tự chia thành hai nhóm |

---

## 4. Game Logic & Learning Experience Design

## 4.1. Learning Objectives

| # | Objective | Mô tả | Measurable Outcome |
|---|---|---|---|
| LO-1 | Hiểu cấu trúc tách số | Trẻ hiểu một số tổng có thể được chia thành hai phần | Kéo hết object từ `Whole` xuống `PartA`/`PartB` |
| LO-2 | Hiểu cấu trúc gộp số | Trẻ hiểu hai phần có thể tạo thành một tổng | Post-MVP: gom object từ Parts về Whole |
| LO-3 | Liên kết số lượng với biểu thức | Trẻ thấy số object trong zones tương ứng biểu thức `A = B + C` | Expression cập nhật đúng theo zone count |
| LO-4 | Hiểu phần còn thiếu | Trẻ suy ra phần còn thiếu khi biết tổng và một phần | `6 = 2 + __` → kéo 4 object vào PartB |
| LO-5 | Giảm học vẹt phép cộng/trừ | Trẻ thao tác vật lý trước khi học biểu thức trừu tượng | Trả lời dựa trên object movement, không chỉ chọn số |

---

## 4.2. User Flow — Free Split Mode

```text
[1] App launch
    SC_Boot → SC_MainMenu → SC_ActivitySelect

[2] User chọn activity
    Tap "Tách-gộp số" / "Number Bonds"

[3] Scene gameplay
    ActivityFlowNavigator.LoadActivity("NumberBonds")
    SelectedActivityData.ActivityId = "NumberBonds"
    Load SC_ARGameplay

[4] AR services ready
    ARServiceBootstrap initialize Session / Placement / Interaction
    LearningSceneServices đảm bảo ProgressStorage / Feedback / Audio

[5] Placement
    User scan mặt bàn/sàn
    User tap đặt Learning Area

[6] Activity start
    NumberBondsActivityBootstrap nhận service từ ARServiceBootstrap
    NumberBondsPresenter.Initialize(config, view, placement, interaction)
    Presenter LoadRound(1)

[7] AR visualization
    View spawn 3 zones:
        Whole ở phía trên
        PartA ở dưới trái
        PartB ở dưới phải
    View spawn 5 objects trong Whole

[8] Instruction
    UI text: "Con hãy chia 5 thành 2 nhóm"
    Expression: "5 = __ + __"

[9] Interaction
    User kéo 2 objects từ Whole sang PartA
    Expression: "5 = 2 + __"

[10] Interaction tiếp
    User kéo 3 objects còn lại sang PartB
    Expression: "5 = 2 + 3"
    Confirm button enabled

[11] Confirm
    User tap "Xác nhận"

[12] Validation
    Presenter kiểm tra:
        Whole == 0
        PartA + PartB == 5

[13] Feedback
    Correct:
        zones glow green
        audio positive
        text: "Giỏi lắm! 5 có thể tách thành 2 và 3"

[14] Next
    User tap "Câu tiếp theo" hoặc auto-continue sau delay ngắn
```

---

## 4.3. User Flow — Target Split Mode

```text
Round target: 6 = 2 + __

[1] View hiển thị:
        Whole chứa 4 object còn cần phân bổ
        PartA đã có 2 object, locked
        PartB trống

[2] UI:
        "Con hãy tìm phần còn thiếu"
        Expression: "6 = 2 + __"

[3] User kéo 4 object từ Whole xuống PartB

[4] Expression cập nhật:
        "6 = 2 + 4"

[5] Confirm:
        Đúng nếu Whole == 0, PartA == 2, PartB == 4
```

Mode này rất phù hợp cho demo vì thể hiện rõ hơn ý nghĩa "phần còn thiếu", là cầu nối trực tiếp sang phép trừ.

---

## 4.4. Activity Modes

### Mode 1 — `FreeSplit` MVP

```text
Input:
    wholeTarget = 5
    knownPartA = -1
    knownPartB = -1

Start:
    Whole = 5
    PartA = 0
    PartB = 0

Task:
    "Con hãy chia 5 thành 2 nhóm"

Correct:
    Whole == 0
    PartA + PartB == 5
```

Lưu ý: mọi cách tách đều hợp lệ, gồm `0 + 5`, `1 + 4`, `2 + 3`, `3 + 2`, `4 + 1`, `5 + 0`.

### Mode 2 — `TargetSplit` MVP

```text
Input:
    wholeTarget = 6
    knownPartA = 2
    knownPartB = -1

Start:
    Whole = 4
    PartA = 2 locked
    PartB = 0

Task:
    "6 = 2 + __"

Correct:
    Whole == 0
    PartA == 2
    PartB == 4
```

### Mode 3 — `Compose` Post-MVP

```text
Input:
    knownPartA = 2
    knownPartB = 3

Start:
    PartA = 2
    PartB = 3
    Whole = 0

Task:
    "Hãy gộp 2 và 3 thành một nhóm"

Correct:
    Whole == 5
    PartA == 0
    PartB == 0
```

### Mode 4 — `MissingPart` Post-MVP

```text
Input:
    wholeTarget = 7
    knownPartA = -1
    knownPartB = 4

Task:
    "7 = __ + 4"

Correct:
    PartA == 3
    PartB == 4
```

---

## 4.5. AR Visualization

### 4.5.1. Layout trong không gian AR

```text
                 [WHOLE ZONE]
              ┌────────────────┐
              │  Whole: 5      │
              │  🍎 🍎 🍎 🍎 🍎 │
              └────────────────┘
                    ↙      ↘

        [PART A ZONE]      [PART B ZONE]
      ┌──────────────┐   ┌──────────────┐
      │ Part A: 0    │   │ Part B: 0    │
      │              │   │              │
      └──────────────┘   └──────────────┘

              Expression Overlay:
                  5 = __ + __
```

### 4.5.2. Yêu cầu visual

| Thành phần | Yêu cầu |
|---|---|
| Whole zone | Lớn hơn Parts, label rõ `Tổng` hoặc `Whole` |
| Part zones | Hai vùng bằng nhau, label `Phần A`, `Phần B` |
| Connection lines | Có thể là line renderer hoặc visual đơn giản nối Whole với Parts |
| Count label | Hiện số object trong từng zone |
| Expression | Overlay UI hoặc world-space text, dễ nhìn |
| Draggable objects | Prefab có collider đơn giản, renderer hỗ trợ highlight |
| Drop highlight | Zone glow khi object đang được kéo ở gần |

### 4.5.3. iOS constraints

- Zone hitbox phải lớn hơn visual circle để trẻ dễ drop.
- Không dùng mesh collider phức tạp.
- Max object cho demo nên là 7 hoặc 8.
- Object nên snap về slot trong zone, không để chồng lộn xộn.
- Plane detection nên tắt hoặc giảm sau khi learning area đã placed.

---

## 4.6. State Model

Activity vẫn dùng state machine của `ActivityPresenter`, nhưng Number Bonds cần thêm local interaction state trong View.

### 4.6.1. Activity-level states

| State | Owner | Entry Condition | Exit Condition |
|---|---|---|---|
| `Initializing` | Presenter/Base | Activity bootstrap gọi Initialize | Config + services valid |
| `Ready` | Presenter/Base | AR + placement + view ready | StartActivity |
| `InProgress` | Presenter/Base | Round loaded | SubmitAnswer |
| `Completed` | Presenter/Base | Round/activity hoàn thành | Next round / summary |
| `Failed` | Presenter/Base | Max attempts hoặc fatal learning failure | Retry / reveal |
| `Cancelled` | Presenter/Base | User back/cancel | Scene cleanup |

### 4.6.2. Interaction-level states

| State | Owner | Mô tả |
|---|---|---|
| `Idle` | View/Drag adapter | Không kéo gì |
| `DraggingObject` | View/Drag adapter | User đang kéo object |
| `HoveringValidZone` | View/Zone | Object đang ở trên zone có thể drop |
| `DropAnimating` | View | Object snap vào zone hoặc return |
| `ConfirmPending` | Presenter/View | Điều kiện cơ bản đủ, bật confirm |
| `ShowingFeedback` | View | Đang phát feedback đúng/sai |

Presenter không nên xử lý `Transform` trực tiếp. Presenter chỉ nhận sự kiện trừu tượng như `OnMoveCommitted(objectId, fromZone, toZone)`.

---

## 4.7. Interaction Flow

### 4.7.1. Drag-drop flow chuẩn

```text
[1] User touch-down / mouse-down trên learning object
[2] View/DragAdapter detect object bằng ARInteractionService hoặc raycast fallback
[3] View phát visual pickup:
        scale 1.0 → 1.1
        shadow/outline
[4] User kéo object trên AR plane
[5] View kiểm tra nearest zone:
        nếu gần PartA → PartA glow
        nếu gần PartB → PartB glow
        nếu ngoài zones → không glow
[6] User thả tay
[7] View xác định target zone
[8] Nếu target zone hợp lệ:
        View snap object vào slot trong zone
        View phát event OnObjectDropped(objectId, fromZone, toZone)
[9] Presenter cập nhật model counts
[10] Presenter gọi View.UpdateExpression(...)
[11] Presenter cập nhật Confirm button state
[12] Nếu target zone không hợp lệ:
        View animate object về zone cũ
        Không tính là answer sai
```

### 4.7.2. Nguyên tắc phân quyền

| Logic | Đặt ở đâu | Lý do |
|---|---|---|
| Detect touch/raycast | View hoặc DragAdapter | Liên quan input/Unity object |
| Di chuyển object transform | View hoặc DragAdapter | Presenter không touch Transform |
| Kiểm tra zone vật lý | View/Zone | Liên quan position/hitbox |
| Chuyển object từ zone này sang zone khác ở model | Presenter | Business state |
| Validate answer | Presenter | Business logic |
| Update expression text | View/Binder theo lệnh Presenter | Rendering |
| Feedback correct/incorrect | Presenter trigger, View phát | Tách logic và visual |

---

## 4.8. Answer Validation

### 4.8.1. Validation cho MVP

| Mode | Điều kiện đúng | Điều kiện sai chính |
|---|---|---|
| `FreeSplit` | `Whole == 0 && PartA + PartB == wholeTarget` | Còn object trong Whole |
| `TargetSplit` | `Whole == 0 && PartA == knownPartA && PartB == wholeTarget - knownPartA` | PartB chưa đủ hoặc Whole còn object |
| `Compose` Post-MVP | `Whole == target && PartA == 0 && PartB == 0` | Chưa gom hết |
| `MissingPart` Post-MVP | `missingPart == wholeTarget - knownPart` | Điền sai phần còn thiếu |

### 4.8.2. Validation result types

```csharp
public enum NumberBondValidationResult
{
    Correct,
    NotAllObjectsMoved,
    WrongPartCount,
    WrongTotal,
    LockedZoneModified,
    InvalidMove,
    TechnicalIssue
}
```

### 4.8.3. Feedback theo validation result

| Result | Feedback | Có tính là learning error? |
|---|---|---:|
| `Correct` | Glow xanh, audio khen, hiển thị biểu thức đúng | Không |
| `NotAllObjectsMoved` | Whole pulse nhẹ, text “Con hãy chia hết các vật thể xuống hai phần” | Có |
| `WrongPartCount` | Highlight phần còn thiếu, text “Con thử đếm lại phần còn thiếu nhé” | Có |
| `WrongTotal` | Parts flash nhẹ, expression vàng/đỏ | Có |
| `LockedZoneModified` | Snap object về vị trí cũ, text “Phần này đã cho sẵn rồi” | Không hoặc UX error |
| `InvalidMove` | Object return, không báo sai | Không |
| `TechnicalIssue` | Pause, hiện “Mình thử đặt lại vùng học nhé” | Không |

---

## 4.9. Hint System

### 4.9.1. Hint levels cho FreeSplit

| Level | Trigger | Hint |
|---:|---|---|
| 1 | Sai lần 1 hoặc confirm khi Whole còn object | “Con hãy kéo hết các vật thể từ Tổng xuống hai phần nhé.” |
| 2 | Sai lần 2 | Highlight từng object còn trong Whole |
| 3 | Sai lần 3 | Animation gợi ý kéo 1 object từ Whole xuống PartA |
| 4 | Quá khó | Giảm target xuống số nhỏ hơn ở round tiếp theo |

### 4.9.2. Hint levels cho TargetSplit

| Level | Trigger | Hint |
|---:|---|---|
| 1 | PartB chưa đủ | “Con đã có 2 rồi, cần thêm bao nhiêu để thành 6?” |
| 2 | Sai lần 2 | Hiển thị counter: `2 + ? = 6` |
| 3 | Sai lần 3 | Highlight số object còn lại trong Whole |
| 4 | Quá khó | Tạm chuyển về FreeSplit target nhỏ hơn |

### 4.9.3. Không dùng feedback tiêu cực mạnh

Vì proposal nhấn mạnh giảm quá tải nhận thức và tránh học vẹt/lo âu, feedback sai nên hướng dẫn lại thay vì phạt mạnh:

```text
Không nên:
    "Sai rồi!"
    âm thanh buzz lớn
    màn hình đỏ mạnh

Nên:
    "Mình thử đếm lại nhé"
    glow đỏ nhẹ
    highlight object còn thiếu
```

---

## 4.10. Progression / Difficulty

### 4.10.1. Level progression đề xuất

| Level | Mode | Target Range | Layout | Notes |
|---:|---|---:|---|---|
| 1 | FreeSplit | 3–5 | Object xếp gọn trong Whole | Demo-friendly |
| 2 | FreeSplit | 5–7 | Object hơi phân tán | Tăng quan sát |
| 3 | TargetSplit | 5–7 | One known part locked | Tìm phần còn thiếu |
| 4 | TargetSplit | 6–10 | Known part thay đổi | Chuẩn bị phép trừ |
| 5 | Compose | 3–8 | Parts có sẵn | Gộp thành tổng |
| 6 | MissingPart | 5–10 | Một phần ẩn | Post-MVP |

### 4.10.2. Rule tăng/giảm độ khó

```text
Nếu correct >= 2 rounds liên tiếp, attempts <= 1, no hint:
    tăng level hoặc tăng target

Nếu sai cùng error type >= 2 lần:
    giữ level, tăng hint support

Nếu lỗi là InvalidMove / TechnicalIssue:
    không tính là learning failure

Nếu drag/drop quá khó:
    tăng zone hitbox hoặc chuyển sang tap-to-move fallback
```

---

## 4.11. Tracking Metrics

### 4.11.1. Metrics cần lưu

| Metric | Ý nghĩa |
|---|---|
| `wholeTarget` | Số tổng của round |
| `mode` | FreeSplit / TargetSplit / Compose / MissingPart |
| `finalWholeCount` | Object còn lại trong Whole khi confirm |
| `finalPartACount` | Object trong PartA |
| `finalPartBCount` | Object trong PartB |
| `attemptCount` | Số lần confirm |
| `moveCount` | Số lần move object thành công |
| `invalidDropCount` | Số lần thả ngoài zone |
| `hintCount` | Số hint đã dùng |
| `responseTimeSeconds` | Thời gian từ round start đến correct |
| `validationResult` | Correct / NotAllObjectsMoved / WrongPartCount... |
| `technicalIssueType` | Tracking lost, placement unavailable... nếu có |

### 4.11.2. Phân biệt lỗi học tập và lỗi thao tác

| Tình huống | Loại lỗi |
|---|---|
| Confirm khi Whole còn object | Learning error |
| Điền thiếu phần còn lại | Learning error |
| Drop ngoài zone | Interaction error |
| Kéo nhầm vào locked zone | UX/interaction error |
| Mất tracking AR | Technical issue |
| Object không raycast được | Technical issue |

---

## 5. Technical Mapping to Current System

## 5.1. Module hiện có có thể tái sử dụng

| Module hiện có | Tái sử dụng thế nào |
|---|---|
| `ActivityPresenter` | Base state machine, rounds, hint, persistence, feedback |
| `IARPlacementService` | Spawn zones và learning objects trên learning area |
| `IARInteractionService` | Register interactables, tap/select/highlight, drag nếu khả dụng |
| `ARServiceBootstrap` | Resolve `Session`, `Placement`, `Interaction` |
| `FeedbackServiceProxy` | Trigger correct/incorrect/activity complete feedback |
| `SimpleAudioManager` | Phát instruction, number audio, positive/incorrect sound |
| `ProgressStorageProxy` | Lưu round result và session result |
| `HintSystem` | Quản lý hint state theo activity/round |
| `RuntimePerformanceSettings` | Clamp số object theo device tier |
| `GameplayActivityRouter` | Thêm branch route `"NumberBonds"` |
| `SC_ARGameplay` | Scene chạy activity AR hiện tại |

### 5.2. Không nên tạo mới

| Không nên tạo | Lý do |
|---|---|
| `NumberBondsARService` trong `Core/AR` | Không cần service AR mới; chỉ là activity logic |
| `NumberBondsSceneManager` | Scene flow đã có `ActivityFlowNavigator`/router |
| Presenter tự `FindAnyObjectByType` | Vi phạm dependency injection rule |
| Presenter tự move `Transform` | Vi phạm MVP; View/adapter xử lý object |
| Hardcode questions trong Presenter | Config nên là ScriptableObject |

---

## 6. Folder Structure

Tạo mới:

```text
Assets/Features/Activities/NumberBonds/
├── Scripts/
│   ├── NumberBondsConfig.cs
│   ├── NumberBondsQuestion.cs
│   ├── NumberBondsAnswer.cs
│   ├── NumberBondsPresenter.cs
│   ├── NumberBondsView.cs
│   ├── NumberBondsActivityBootstrap.cs
│   ├── NumberBondZone.cs
│   ├── NumberBondZoneView.cs
│   ├── NumberBondExpressionBinder.cs
│   ├── NumberBondDragAdapter.cs
│   ├── NumberBondObjectView.cs
│   └── NumberBondTypes.cs
├── Interfaces/
│   └── INumberBondsView.cs
├── Prefabs/
│   ├── NumberBondZone.prefab
│   ├── NumberBondApple.prefab
│   └── NumberBondConnectionLine.prefab
└── Configs/
    └── SO_NumberBondsConfig_Demo.asset
```

Sửa file hiện có:

```text
Assets/_Project/Scripts/GameplayActivityRouter.cs
Assets/_Project/Scripts/ActivitySelectController.cs hoặc data source activity list nếu có
Build Settings / Scene config nếu activity select phụ thuộc scene/prefab references
```

---

## 7. Script Responsibilities

## 7.1. `NumberBondsConfig.cs`

**Vai trò**: ScriptableObject chứa config rounds, visual theme, constraints.

```csharp
[CreateAssetMenu(
    fileName = "SO_NumberBondsConfig",
    menuName = "AR Special Education/Activities/Number Bonds Config")]
public class NumberBondsConfig : ActivityConfig
{
    public List<NumberBondsQuestion> questions;
    public int maxObjectsPerRound = 8;
    public int maxAttemptsPerRound = 3;
    public bool enableTargetSplit = true;
    public bool enableCompose = false;
    public bool enableMissingPart = false;
    public GameObject objectPrefab;
    public GameObject zonePrefab;
    public AudioClip correctClip;
    public AudioClip incorrectClip;

    public override bool IsValid()
    {
        return questions != null
            && questions.Count > 0
            && objectPrefab != null
            && zonePrefab != null;
    }
}
```

## 7.2. `NumberBondsQuestion.cs`

```csharp
[Serializable]
public class NumberBondsQuestion
{
    public int wholeTarget;
    public int knownPartA = -1;
    public int knownPartB = -1;
    public NumberBondMode mode = NumberBondMode.FreeSplit;
    public string instructionText;
    public string objectPrefabName;
    public bool allowZeroPart = true;
}
```

## 7.3. `NumberBondsAnswer.cs`

```csharp
[Serializable]
public class NumberBondsAnswer
{
    public int finalWholeCount;
    public int finalPartACount;
    public int finalPartBCount;
    public int expectedWhole;
    public NumberBondMode mode;
    public float responseTimeSeconds;
    public int attemptNumber;
    public int moveCount;
    public int invalidDropCount;
    public int hintCount;
    public NumberBondValidationResult validationResult;
}
```

## 7.4. `NumberBondTypes.cs`

```csharp
public enum NumberBondMode
{
    FreeSplit,
    TargetSplit,
    Compose,
    MissingPart
}

public enum BondZone
{
    Whole,
    PartA,
    PartB,
    None
}

public enum NumberBondValidationResult
{
    Correct,
    NotAllObjectsMoved,
    WrongPartCount,
    WrongTotal,
    LockedZoneModified,
    InvalidMove,
    TechnicalIssue
}
```

## 7.5. `INumberBondsView.cs`

```csharp
public interface INumberBondsView : IActivityView
{
    event Action<string, BondZone, BondZone> OnObjectMoveCommitted;
    event Action OnConfirmRequested;
    event Action OnHintRequested;

    void SetupRound(NumberBondsQuestion question, NumberBondRoundState state);
    void UpdateZoneCounts(int whole, int partA, int partB);
    void UpdateExpression(string expression);
    void SetConfirmEnabled(bool enabled);
    void ShowValidationFeedback(NumberBondValidationResult result);
    void ResetRoundVisuals();
    void ClearSpawnedObjects();
}
```

## 7.6. `NumberBondsPresenter.cs`

**Vai trò chính**:

- Nhận config/view/services trong `Initialize`.
- Không gọi `FindAnyObjectByType`.
- Không thao tác `GameObject/Transform` trực tiếp.
- Load question.
- Quản lý model count.
- Validate answer.
- Trigger hint/feedback/persistence.

Pseudo-flow:

```csharp
public class NumberBondsPresenter : ActivityPresenter
{
    private NumberBondsConfig config;
    private INumberBondsView view;
    private NumberBondsQuestion currentQuestion;
    private NumberBondRoundState currentState;

    public void Initialize(
        NumberBondsConfig config,
        INumberBondsView view,
        IARPlacementService placement,
        IARInteractionService interaction)
    {
        if (config == null || !config.IsValid())
        {
            Debug.LogError("[NumberBondsPresenter] Invalid config.");
            return;
        }

        this.config = config;
        this.view = view;

        view.OnObjectMoveCommitted += HandleObjectMoveCommitted;
        view.OnConfirmRequested += SubmitCurrentAnswer;
        view.OnHintRequested += RequestHint;

        base.Initialize(config);
    }

    protected override void LoadRound(int roundNumber)
    {
        currentQuestion = config.questions[roundNumber];
        currentState = NumberBondRoundState.FromQuestion(currentQuestion);
        view.SetupRound(currentQuestion, currentState);
        view.UpdateExpression(BuildExpression());
        view.SetConfirmEnabled(false);
    }

    private void HandleObjectMoveCommitted(string objectId, BondZone from, BondZone to)
    {
        if (!CanMove(from, to))
        {
            view.ShowValidationFeedback(NumberBondValidationResult.InvalidMove);
            return;
        }

        currentState.MoveObject(from, to);
        view.UpdateZoneCounts(
            currentState.WholeCount,
            currentState.PartACount,
            currentState.PartBCount);

        view.UpdateExpression(BuildExpression());
        view.SetConfirmEnabled(CanConfirm());
    }

    private void SubmitCurrentAnswer()
    {
        var result = ValidateCurrentState();
        if (result == NumberBondValidationResult.Correct)
        {
            HandleCorrectAnswer(BuildAnswer(result));
        }
        else
        {
            HandleIncorrectAnswer(BuildAnswer(result));
            view.ShowValidationFeedback(result);
        }
    }
}
```

## 7.7. `NumberBondsView.cs`

**Vai trò chính**:

- Render instruction, expression, confirm button, hint button.
- Tạo/spawn hoặc yêu cầu spawn zones và objects.
- Forward drag/drop event lên Presenter.
- Không quyết định đúng/sai.
- Không persist result.

Responsibilities:

| Responsibility | Có trong View? |
|---|---:|
| Create zone visuals | ✅ |
| Create object visuals | ✅ |
| Detect drag/drop | ✅ qua adapter |
| Update expression UI | ✅ |
| Validate answer correctness | ❌ |
| Save progress | ❌ |
| Decide next round | ❌ |
| Play visual feedback | ✅ theo lệnh Presenter |

## 7.8. `NumberBondZone.cs` / `NumberBondZoneView.cs`

Nên tách logic zone thành component riêng để View không phình quá lớn.

```csharp
public class NumberBondZone : MonoBehaviour
{
    public BondZone ZoneType { get; private set; }
    public int Count => objectIds.Count;
    public bool IsLocked { get; private set; }

    private readonly List<string> objectIds = new();

    public void Initialize(BondZone zoneType, bool isLocked)
    {
        ZoneType = zoneType;
        IsLocked = isLocked;
    }

    public bool CanAcceptObject()
    {
        return !IsLocked;
    }

    public void AddObjectId(string objectId)
    {
        if (!objectIds.Contains(objectId))
            objectIds.Add(objectId);
    }

    public void RemoveObjectId(string objectId)
    {
        objectIds.Remove(objectId);
    }
}
```

Visual/hitbox/slot placement nên nằm trong `NumberBondZoneView`.

## 7.9. `NumberBondExpressionBinder.cs`

**Vai trò**: Format và render expression từ state.

Examples:

| Mode | State | Expression |
|---|---|---|
| FreeSplit | Whole=5 target, A=0, B=0 | `5 = __ + __` |
| FreeSplit | A=2, B=0, Whole=3 | `5 = 2 + __` |
| FreeSplit | A=2, B=3, Whole=0 | `5 = 2 + 3` |
| TargetSplit | target=6, known A=2, B=0 | `6 = 2 + __` |
| TargetSplit | B=4 | `6 = 2 + 4` |
| Compose | A=2, B=3, Whole=0 | `2 + 3 = __` |
| Compose | Whole=5 | `2 + 3 = 5` |

Binder không tự đọc GameObject count; Presenter truyền state hoặc View nhận model update rồi gọi binder.

## 7.10. `NumberBondDragAdapter.cs`

**Vai trò**: Adapter input/drag-drop cho Number Bonds.

Ưu tiên dùng `IARInteractionService` nếu drag support đủ ổn. Nếu không đủ, dùng custom raycast fallback trong phạm vi `Features/Activities/NumberBonds`, không đưa vào `Core/AR` ngay.

Rules:

```text
- Không validate answer.
- Không update progress.
- Không gọi Presenter.SubmitAnswer.
- Chỉ phát event drag/drop.
- Không log trong mỗi frame.
- Có guard multi-touch.
- Drop ngoài zone thì yêu cầu View animate return.
```

---

## 8. Technical Flow

## 8.1. Startup flow

```text
SC_ARGameplay loaded
    ↓
ARServiceBootstrap.Awake (-200)
    Resolve Session / Placement / Interaction
    ↓
LearningSceneServices.Awake (-150)
    Ensure ProgressStorage / Feedback / Audio / Router
    ↓
GameplayActivityRouter.Start (-140)
    RouteSelectedActivity()
    if ActivityId == "NumberBonds":
        CreateNumberBondsActivity()
    ↓
NumberBondsActivityBootstrap
    Wait for Placement available + LearningArea
    Prefer event-driven OnLearningAreaPlaced
    ↓
NumberBondsPresenter.Initialize(...)
    ↓
Presenter.StartActivity()
    ↓
LoadRound(0)
    ↓
View.SetupRound(...)
```

## 8.2. Round flow

```text
LoadRound
    ↓
Build initial NumberBondRoundState
    ↓
View spawn zones + objects
    ↓
ExpressionBinder render expression
    ↓
User drag/drop objects
    ↓
View emits OnObjectMoveCommitted
    ↓
Presenter updates model
    ↓
Presenter tells View update count/expression/confirm state
    ↓
User presses Confirm
    ↓
Presenter ValidateCurrentState
    ↓
Correct:
    Persist result
    Feedback correct
    Next round
Incorrect:
    Persist attempt or round result depending base behavior
    Feedback + hint
```

---

## 9. Router Integration

Trong `GameplayActivityRouter.cs`, thêm branch:

```csharp
switch (SelectedActivityData.ActivityId)
{
    case "QuantityMatch":
        StartExistingQuantityMatch();
        break;

    case "NumberLineJump":
        CreateNumberLineJumpActivity();
        break;

    case "CompareQuantity":
        CreateCompareQuantityActivity();
        break;

    case "NumberBonds":
        CreateNumberBondsActivity();
        break;

    default:
        Debug.LogWarning($"[GameplayActivityRouter] Unknown activity id: {SelectedActivityData.ActivityId}");
        break;
}
```

Thêm method:

```csharp
private void CreateNumberBondsActivity()
{
    // Prefer finding existing bootstrap in scene if prefab/object is already placed.
    // Otherwise instantiate runtime object using same style as NumberLineJump/CompareQuantity.
}
```

Lưu ý:

- Nếu current router đã có pattern `StartExisting...` cho activity có sẵn trong scene, bám theo pattern đó.
- Nếu `NumberLineJump`/`CompareQuantity` được tạo runtime, bám theo cách đó để nhất quán.
- Không tạo scene mới cho Number Bonds nếu `SC_ARGameplay` đã là gameplay scene chung.

---

## 10. Config Demo Asset

Tạo `SO_NumberBondsConfig_Demo.asset` với nội dung tương đương:

```text
ActivityId: NumberBonds
LessonId: LESSON_03_COMPOSITION
NumberOfRounds: 3
MaxAttemptsPerRound: 3
MaxObjectsPerRound: 7
ObjectTheme: Apple hoặc Cube
EnableTargetSplit: true
EnableCompose: false
EnableMissingPart: false

Questions:
  1. mode=FreeSplit, wholeTarget=5, knownPartA=-1, knownPartB=-1
  2. mode=TargetSplit, wholeTarget=6, knownPartA=2, knownPartB=-1
  3. mode=FreeSplit, wholeTarget=7, knownPartA=-1, knownPartB=-1
```

---

## 11. Prefab Requirements

### 11.1. Learning object prefab

| Requirement | Mô tả |
|---|---|
| Collider | SphereCollider hoặc BoxCollider, không dùng MeshCollider |
| Renderer | Material hỗ trợ `_BaseColor` hoặc `_Color` để highlight |
| Scale | Vừa tay, không quá nhỏ trên iPhone |
| Layer | Có layer hoặc tag để drag adapter nhận diện |
| ID component | Có `NumberBondObjectView` lưu `ObjectId` và current zone |

### 11.2. Zone prefab

| Requirement | Mô tả |
|---|---|
| Visual circle | Dễ nhìn trên mặt bàn/sàn |
| Hitbox | Lớn hơn visual, min diameter khoảng 0.3m cho demo |
| Label | `Whole`, `Part A`, `Part B` hoặc tiếng Việt `Tổng`, `Phần A`, `Phần B` |
| Count label | Hiện count realtime |
| Highlight state | Normal / Hover / Correct / Incorrect / Locked |
| Slots | Có slot positions để object snap vào zone gọn gàng |

---

## 12. Performance Notes for iOS

| Vấn đề | Rule implementation |
|---|---|
| Quá nhiều object | Clamp target <= 8 cho demo |
| Drag lag | Không log trong drag update |
| GC pressure | Reuse lists, không LINQ trong frame loop |
| Collider phức tạp | Dùng SphereCollider/BoxCollider |
| Plane detection tốn CPU | Sau placement, giảm/tắt plane visualization nếu service hỗ trợ |
| Runtime instantiate nhiều | Với demo ít round có thể instantiate/destroy; post-MVP nên pooling |
| FPS target | Theo rules AR iOS, target 30 FPS thay vì cố 60 FPS |

---

## 13. Error Handling

### 13.1. AR / Technical errors

| Tình huống | Behavior |
|---|---|
| Placement unavailable | Không start round, show “Hãy quét mặt phẳng trước” |
| Learning area chưa đặt | Subscribe event placement hoặc hiển thị hướng dẫn tap-to-place |
| Tracking lost | Pause interaction, show overlay tracking lost |
| Object spawn failed | Log error, show technical message, không tính sai |
| Config invalid | Log error, không start activity |

### 13.2. Interaction errors

| Tình huống | Behavior |
|---|---|
| Drop ngoài zone | Animate return, không tính sai |
| Kéo vào locked zone | Snap return, text “Phần này đã cho sẵn rồi” |
| Multi-touch gây conflict | Chỉ nhận active pointer đầu tiên |
| User kéo object khi feedback đang chạy | Disable drag tạm thời |

---

## 14. Testing Checklist

## 14.1. Editor simulation

| # | Test Case | Expected | Priority |
|---|---|---|---|
| E1 | Boot → ActivitySelect → NumberBonds | Load `SC_ARGameplay` đúng | P0 |
| E2 | Router nhận `"NumberBonds"` | Gọi đúng create/start NumberBonds | P0 |
| E3 | Config invalid | Log error, không crash | P0 |
| E4 | Learning area mock ready | Activity start được trong Editor | P0 |
| E5 | Spawn 3 zones | Whole trên, Parts dưới | P0 |
| E6 | Spawn 5 objects trong Whole | Count Whole = 5 | P0 |
| E7 | Drag object bằng mouse | Object follow mouse/plane | P0 |
| E8 | Drop vào PartA | Count Whole giảm, PartA tăng | P0 |
| E9 | Expression update | `5 = 1 + __`, rồi `5 = 2 + 3` | P0 |
| E10 | Confirm khi đúng | Correct feedback, next round | P0 |
| E11 | Confirm khi Whole còn object | Hint NotAllObjectsMoved | P0 |
| E12 | Drop ngoài zone | Object return, không tính sai | P1 |
| E13 | TargetSplit locked PartA | Không cho sửa PartA nếu locked | P1 |
| E14 | Complete all rounds | Summary/activity complete flow | P1 |

## 14.2. iOS device

| # | Test Case | Expected | Priority |
|---|---|---|---|
| I1 | Cold start | App launch ổn | P0 |
| I2 | Camera permission | Có message nếu denied | P0 |
| I3 | Plane detection | Detect mặt bàn/sàn | P0 |
| I4 | Place learning area | Anchor stable | P0 |
| I5 | Zones visible | Không quá nhỏ trên màn hình iPhone | P0 |
| I6 | Touch drag | Mượt, không nhảy object | P0 |
| I7 | Drop vào zone | Snap đúng, count update | P0 |
| I8 | Tracking lost khi đang drag | Pause/recover không crash | P1 |
| I9 | 3 rounds liên tiếp | Không leak object rõ ràng | P1 |
| I10 | Progress after restart | Nếu storage available, result vẫn lưu | P1 |

## 14.3. Presenter unit tests

| # | Test | Expected |
|---|---|---|
| U1 | FreeSplit `Whole=0, A=2, B=3, target=5` | Correct |
| U2 | FreeSplit `Whole=1, A=2, B=2, target=5` | NotAllObjectsMoved |
| U3 | TargetSplit `target=6, knownA=2, B=4` | Correct |
| U4 | TargetSplit `target=6, knownA=2, B=3` | WrongPartCount |
| U5 | Invalid move to locked zone | LockedZoneModified |
| U6 | Move object updates counts | Model counts correct |
| U7 | Hint request increments hint count | hintCount correct |

---

## 15. Implementation Priority Order

```text
[P0-1] Tạo folder Features/Activities/NumberBonds
[P0-2] Tạo NumberBondTypes, Question, Answer, Config
[P0-3] Tạo INumberBondsView
[P0-4] Tạo NumberBondRoundState model thuần C# để dễ unit test
[P0-5] Implement NumberBondsPresenter validation logic
[P0-6] Implement NumberBondsView basic UI + expression
[P0-7] Implement NumberBondZone / ZoneView
[P0-8] Implement object spawn vào Whole zone
[P0-9] Implement drag/drop adapter basic
[P0-10] Integrate zone count → presenter → expression
[P0-11] Add confirm validation + correct/incorrect feedback
[P0-12] Add GameplayActivityRouter branch
[P0-13] Create SO_NumberBondsConfig_Demo.asset
[P0-14] Editor test full demo path

[P1-1] Improve TargetSplit locked zone behavior
[P1-2] Add hint levels
[P1-3] Add progress metrics
[P1-4] iOS device test
[P1-5] Fix drag/drop UX issues

[P2-1] Compose Mode
[P2-2] MissingPart Mode
[P2-3] Better animations
[P2-4] Object pooling
[P2-5] Hand-tracking exploration
```

---

## 16. Demo Acceptance Criteria

Bản demo được xem là đạt khi:

```text
[ ] User chọn được NumberBonds từ ActivitySelect hoặc bằng activity id.
[ ] SC_ARGameplay load không crash.
[ ] AR/mock placement ready thì activity start.
[ ] 3 zones xuất hiện đúng layout.
[ ] Round 1 có 5 objects trong Whole.
[ ] User kéo được object từ Whole sang PartA/PartB.
[ ] Count của zones cập nhật đúng.
[ ] Expression cập nhật realtime.
[ ] Confirm khi chia đủ thì correct.
[ ] Confirm khi chưa chia hết thì có hint/feedback.
[ ] Round 2 TargetSplit chạy được.
[ ] Complete demo rounds hiển thị complete/summary.
[ ] Không có lỗi nghiêm trọng trong console.
```

---

## 17. Risks & Mitigations

| Risk | Mức | Nguyên nhân | Mitigation |
|---|---:|---|---|
| Drag-drop AR không ổn định trên iOS | Cao | Touch raycast/plane projection khó | Có mouse/touch fallback; test device sớm |
| Presenter/View boundary bị lẫn | Cao | Drag-drop liên quan GameObject | Presenter chỉ xử lý model, View xử lý Transform |
| View phình to | Trung bình | Runtime UI + drag + zones + feedback | Tách ZoneView, ExpressionBinder, DragAdapter |
| Router integration lệch pattern hiện có | Trung bình | Chưa biết router hiện instantiate thế nào | Bám theo pattern của NumberLineJump/CompareQuantity |
| Object quá nhiều gây lag | Trung bình | AR mobile performance | Clamp max 7/8 object cho demo |
| Hand-tracking scope creep | Cao | Proposal có nhắc hand-tracking | Ghi rõ Post-MVP |
| Feedback sai gây áp lực | Trung bình | Trẻ Dyscalculia dễ lo âu | Feedback hướng dẫn, không phạt mạnh |
| Technical issue bị tính là học sai | Trung bình | AR tracking/drop issue | Tách `TechnicalIssue` và `InteractionError` |

---

## 18. Post-MVP Enhancements

### 18.1. Compose Mode

```text
Task:
    "Hãy gộp 2 và 3 thành một nhóm"

Start:
    PartA = 2
    PartB = 3
    Whole = 0

User:
    Kéo objects từ PartA và PartB về Whole

Expression:
    2 + 3 = __
    2 + 3 = 5

Correct:
    Whole = 5
    PartA = 0
    PartB = 0
```

### 18.2. Missing Part Mode

```text
Task:
    "7 = __ + 4"

Start:
    PartB = 4 locked
    Whole = 3
    PartA = 0

User:
    Kéo 3 object vào PartA

Correct:
    PartA = 3
```

### 18.3. All Combinations Mode

```text
Task:
    "Con hãy tìm nhiều cách tách số 5"

Valid combinations:
    0 + 5
    1 + 4
    2 + 3
    3 + 2
    4 + 1
    5 + 0

Learning value:
    Trẻ hiểu một số có nhiều cấu trúc tách khác nhau.
```

### 18.4. Story Context

```text
Ví dụ:
    "Có 6 quả táo. Con cho bạn 2 quả. Còn lại mấy quả?"

Mapping:
    Whole = 6
    PartA = 2 locked
    PartB = missing
```

---

## 19. Prompt ngắn để yêu cầu agent implement MVP

```text
Bám sát @.agent/LESSON_03_COMPOSITION.md, implement MVP cho NumberBonds activity.

Scope bắt buộc:
- Tạo Features/Activities/NumberBonds theo MVP pattern Presenter/View/Bootstrap/Config.
- Thêm ActivityId "NumberBonds" vào GameplayActivityRouter.
- Implement FreeSplit và TargetSplit mode.
- Spawn 3 zones Whole/PartA/PartB trong SC_ARGameplay qua placement service hiện có.
- Spawn learning objects trong Whole.
- Cho phép drag/drop object bằng touch/mouse fallback; ưu tiên reuse ARInteractionService nếu đủ.
- Expression cập nhật realtime theo count: ví dụ "5 = 2 + 3".
- Confirm validation: FreeSplit đúng khi Whole=0 và PartA+PartB=target; TargetSplit đúng khi missing part đúng.
- Feedback đúng/sai cơ bản, không làm hand-tracking thật, không làm Compose/MissingPart full.
- Presenter không được FindAnyObjectByType và không manipulate Transform trực tiếp.
- View/adapter xử lý GameObject/Transform, Presenter xử lý model và validation.
- Không tạo service mới trong Core/AR nếu không cần.
```

---

*Cập nhật: 29/05/2026*
